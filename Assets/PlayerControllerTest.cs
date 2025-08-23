using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(StaminaController))]
public class PlayerControllerTest : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float accel = 60f;
    [SerializeField] float decel = 70f;
    [Range(0f, 1f)][SerializeField] float airControl = 0.7f;

    [Header("Jump")]
    [SerializeField] float jumpForce = 14f;
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBuffer = 0.12f;
    [Range(0f, 1f)][SerializeField] float cutJumpGravityMultiplier = 0.5f;

    [Header("Charged Jump")]
    [SerializeField] float maxChargeTime = 1f;
    [SerializeField] float maxChargedJumpForce = 20f;
    [SerializeField] float minChargeTime = 0.3f; // Minimum hold time for charged jump
    [SerializeField] Slider chargeSlider;
    [SerializeField] float chargedJumpStaminaCost = 30f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashTime = 0.12f;
    [SerializeField] float dashStaminaCost = 20f; // NEW - stamina cost per dash
    [SerializeField] float dashMinDelay = 0.05f; // tiny delay between dashes

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.15f;
    [SerializeField] LayerMask groundLayer;

    [Header("FX Hooks")]
    [SerializeField] TrailRenderer dashTrail;
    [SerializeField] ParticleSystem landDust;

    // Component references
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private StaminaController staminaController;

    // State variables
    private float inputX;
    private bool grounded;
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private float defaultGravity;

    // Charged jump variables
    private float chargeStartTime;
    private bool isChargingJump;
    private float currentCharge;
    private bool hasEnoughStaminaForCharge;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        staminaController = GetComponent<StaminaController>();
        defaultGravity = rb.gravityScale;

        // Hide charge slider initially
        if (chargeSlider != null)
            chargeSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        // input
        inputX = Input.GetAxisRaw("Horizontal");

        // Handle jump input (normal and charged)
        HandleJumpInput();

        // ground check
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        if (grounded) lastGroundedTime = Time.time;

        // jump execution (normal jump only)
        bool canCoyote = Time.time - lastGroundedTime <= coyoteTime;
        bool bufferedJump = Time.time - lastJumpPressedTime <= jumpBuffer;

        if (bufferedJump && canCoyote && !isDashing && !isChargingJump)
        {
            PerformJump(jumpForce);
            lastJumpPressedTime = -999f;
        }

        // variable jump (cut jump)
        if (!grounded && rb.linearVelocity.y > 0f && !Input.GetButton("Jump"))
            rb.gravityScale = defaultGravity / Mathf.Max(0.01f, cutJumpGravityMultiplier);
        else
            rb.gravityScale = defaultGravity;

        // dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && Time.time >= nextDashTime)
        {
            if (staminaController.ConsumeStamina(dashStaminaCost))
            {
                StartDash();
            }
        }

        if (isDashing && Time.time >= dashEndTime)
            EndDash();

        // Update charge slider
        UpdateChargeSlider();
    }

    void HandleJumpInput()
    {
        // Check if player has enough stamina for charged jump
        hasEnoughStaminaForCharge = staminaController.HasStaminaForAction(0.1f);

        // Start charging when jump button is pressed, grounded, and has stamina
        if (Input.GetButtonDown("Jump") && (grounded || Time.time - lastGroundedTime <= coyoteTime) && hasEnoughStaminaForCharge)
        {
            chargeStartTime = Time.time;
        }

        // Continue charging while button is held and has stamina
        if (Input.GetButton("Jump") && hasEnoughStaminaForCharge &&
            (grounded || Time.time - lastGroundedTime <= coyoteTime))
        {
            float holdTime = Time.time - chargeStartTime;

            // Only start visual charging after minimum hold time
            if (holdTime >= minChargeTime && !isChargingJump)
            {
                StartCharging();
            }

            if (isChargingJump)
            {
                ContinueCharging();
            }
        }

        // Release jump when button is released
        if (Input.GetButtonUp("Jump"))
        {
            if (isChargingJump)
            {
                ReleaseChargedJump();
            }
            else
            {
                // Normal jump buffer
                lastJumpPressedTime = Time.time;
            }
        }

        // Stop charging if stamina runs out
        if (isChargingJump && !hasEnoughStaminaForCharge)
        {
            ReleaseChargedJumpEarly();
        }
    }

    void StartCharging()
    {
        isChargingJump = true;
        currentCharge = 0f;

        // Show charge slider
        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(true);
            chargeSlider.value = 0f;
        }

        // Only slightly reduce movement while charging
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
    }

    void ContinueCharging()
    {
        // Calculate charge percentage (0 to 1)
        float holdTime = Time.time - chargeStartTime;
        currentCharge = Mathf.Clamp01((holdTime - minChargeTime) / (maxChargeTime - minChargeTime));

        // Consume stamina while charging
        if (chargeSlider.value != 1)
        {
            staminaController.ConsumeStaminaOverTime();
        }

        // Apply movement even while charging
        HandleMovement(true);
    }

    void ReleaseChargedJump()
    {
        float holdTime = Time.time - chargeStartTime;

        if (holdTime >= minChargeTime)
        {
            // Calculate jump force based on charge
            float chargeFactor = Mathf.Clamp01((holdTime - minChargeTime) / (maxChargeTime - minChargeTime));
            float chargedJumpForce = Mathf.Lerp(jumpForce, maxChargedJumpForce, chargeFactor);


            PerformJump(chargedJumpForce);
        }
        else
        {
            // Normal jump if released too early
            PerformJump(jumpForce);
        }

        EndCharging();
    }

    void ReleaseChargedJumpEarly()
    {
        // Jump with whatever charge we had
        float holdTime = Time.time - chargeStartTime;
        float chargeFactor = Mathf.Clamp01((holdTime - minChargeTime) / (maxChargeTime - minChargeTime));
        float chargedJumpForce = Mathf.Lerp(jumpForce, maxChargedJumpForce, chargeFactor);

        PerformJump(chargedJumpForce);
        EndCharging();
    }

    void EndCharging()
    {
        isChargingJump = false;
        staminaController.StopConsumption();

        // Hide charge slider
        if (chargeSlider != null)
            chargeSlider.gameObject.SetActive(false);
    }

    void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset y velocity
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        // Continue horizontal movement after jump
        HandleMovement(false);
    }

    void UpdateChargeSlider()
    {
        if (isChargingJump && chargeSlider != null)
        {
            chargeSlider.value = currentCharge;
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Handle movement (unless we're charging, which handles its own movement)
        if (!isChargingJump)
        {
            HandleMovement(false);
        }
    }

    void HandleMovement(bool isCharging)
    {
        float target = inputX * moveSpeed;
        float currentAccel = (Mathf.Abs(target) > 0.01f) ? accel : decel;

        // Reduce acceleration slightly while charging, but don't stop movement
        if (isCharging)
        {
            currentAccel *= 0.7f;
        }

        // Apply different acceleration in air vs ground
        if (!grounded)
        {
            currentAccel *= airControl;
        }

        float speed = Mathf.MoveTowards(rb.linearVelocity.x, target, currentAccel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    void StartDash()
    {
        isDashing = true;
        dashEndTime = Time.time + dashTime;
        nextDashTime = Time.time + dashMinDelay; // no cooldown, just a small delay

        float dir = Mathf.Sign(Mathf.Abs(inputX) > 0.01f ? inputX : transform.localScale.x);
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
        rb.gravityScale = 0f;

        // no stamina regen while dashing
        staminaController.StopConsumption();
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = defaultGravity;
        if (dashTrail) dashTrail.emitting = false;
        LeanScale(Vector3.one, 0.08f);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (landDust != null && c.collider.IsTouchingLayers(groundLayer))
            landDust.Play();
    }

    // lightweight tween for scale (no packages)
    void LeanScale(Vector3 target, float time)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleCo(target, time));
    }

    System.Collections.IEnumerator ScaleCo(Vector3 target, float time)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / time);
            yield return null;
        }
        transform.localScale = target;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}