using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(StaminaController))]
public class PlayerController : MonoBehaviour
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
    [SerializeField]  Image chargeFillImage;
    [SerializeField] public GameObject chargeCanvas;
    [SerializeField] float chargedJumpStaminaCost = 30f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashTime = 0.12f;
    [SerializeField] float dashStaminaCost = 20f;
    [SerializeField] float dashMinDelay = 0.05f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.15f;
    [SerializeField] LayerMask groundLayer;

    [Header("Landing Effects")]
    [SerializeField] float maxLandingSquash = 0.7f; // How much the cube squashes on landing
    [SerializeField] float landingSquashDuration = 0.15f; // How long the squash effect lasts
    [SerializeField] float minLandingVelocity = 5f; // Minimum fall speed to trigger squash effect
    [SerializeField] ParticleSystem landingImpactParticles; // Particles to play on hard landing

    [Header("FX Hooks")]
    [SerializeField] TrailRenderer dashTrail;
    [SerializeField] ParticleSystem landDust;

    [Header("Wheels")]
    [SerializeField] Transform[] wheels;   // Assign wheel sprites in inspector
    [SerializeField] float wheelRadius = 0.5f; // radius in Unity units

    // Component references
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private StaminaController staminaController;
    private PlayerParticleController particleController;


    // State variables
    private float inputX;
    private bool grounded;
    private bool wasGrounded; // Track previous grounded state
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private float defaultGravity;
    private Vector3 originalScale; // Store the cube's original scale

    // Landing impact variables
    private float landingImpactVelocity; // How fast we were falling when we landed
    private bool isLanding; // Are we currently in a landing animation

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
        particleController = GetComponent<PlayerParticleController>();
        defaultGravity = rb.gravityScale;
        dashTrail.emitting = false;
        originalScale = transform.localScale; // Store the original scale

        // Hide charge slider initially
        if (chargeFillImage != null)
            chargeFillImage.gameObject.SetActive(false);
    }

    void Update()
    {
        // Store previous grounded state before checking

        // input
        inputX = Input.GetAxisRaw("Horizontal");

        // Handle jump input (normal and charged)
        HandleJumpInput();

        // ground check
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Check for landing impact
        if (grounded && !wasGrounded)
        {
            OnLanding();
            particleController.Landed();
        }
        wasGrounded = grounded;


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
        UpdatechargeFillImage();
    }

    void OnLanding()
    {
        // Calculate how fast we were falling (negative Y velocity)
        float fallSpeed = Mathf.Abs(Mathf.Min(0, rb.linearVelocity.y));
        landingImpactVelocity = fallSpeed;

        // Only trigger landing effects if we fell fast enough
        if (fallSpeed >= minLandingVelocity)
        {
            isLanding = true;

            // Calculate squash amount based on fall speed (more speed = more squash)
            float squashAmount = Mathf.Clamp(fallSpeed / 15f, 0.1f, maxLandingSquash);

            // Apply squash effect
            Vector3 squashScale = new Vector3(
                originalScale.x * (1f + squashAmount * 0.3f), // Wider
                originalScale.y * (1f - squashAmount),        // Shorter
                originalScale.z                               // Depth unchanged
            );

            // Start the landing animation
            StartCoroutine(LandingSquash(squashScale));

            // Play landing particles based on impact strength
            if (landDust != null)
            {
                var emission = landDust.emission;
                emission.rateOverTime = Mathf.Lerp(5f, 30f, squashAmount / maxLandingSquash);
                landDust.Play();
            }

            // Play impact particles for hard landings
            if (landingImpactParticles != null && fallSpeed > minLandingVelocity * 1.5f)
            {
                landingImpactParticles.Play();
            }
        }
    }

    System.Collections.IEnumerator LandingSquash(Vector3 targetScale)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        // Squash down
        while (elapsed < landingSquashDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (landingSquashDuration / 2f);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Return to normal scale
        elapsed = 0f;
        startScale = transform.localScale;

        while (elapsed < landingSquashDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (landingSquashDuration / 2f);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        // Ensure we're back to exact original scale
        transform.localScale = originalScale;
        isLanding = false;
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
        if (chargeFillImage != null)
        {
            chargeCanvas.SetActive(true);
            chargeFillImage.gameObject.SetActive(true);
            chargeFillImage.fillAmount = 0f;
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
        if (chargeFillImage.fillAmount != 1)
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
        if (chargeFillImage != null)
            chargeFillImage.gameObject.SetActive(false);
    }

    void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset y velocity
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        // Continue horizontal movement after jump
        HandleMovement(false);

        // Small scale effect when jumping
        LeanScale(new Vector3(0.9f, 1.1f, 1f), 0.1f);
    }

    void UpdatechargeFillImage()
    {
        if (isChargingJump && chargeFillImage != null)
        {
            chargeFillImage.fillAmount = currentCharge;
        }
    }

    void FixedUpdate()
    {
        if (!isChargingJump && !isLanding)
        {
            HandleMovement(false);
        }
        RotateWheels();
    }

    void HandleMovement(bool isCharging)
    {
        float targetSpeed = inputX * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        // Different accel values for ground vs air
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? accel : decel;
        if (!grounded) accelRate *= airControl;

        // Extra snappiness → higher decel than accel
        if (Mathf.Abs(targetSpeed) < 0.01f)
            accelRate *= 1.5f; // stop quicker than you start

        // Reduce accel slightly while charging
        if (isCharging)
            accelRate *= 0.7f;

        // Calculate new velocity with smoothing
        float movement = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(movement, rb.linearVelocity.y);
    }

    void RotateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;

        // Only rotate wheels when on the ground OR dashing
        if (!grounded && !isDashing) return;

        // Angular speed = v / r
        float angularSpeed = rb.linearVelocity.x / wheelRadius;
        float rotationAmount = angularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime;

        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.forward, -rotationAmount);
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTrail.emitting = isDashing;
        dashEndTime = Time.time + dashTime;
        nextDashTime = Time.time + dashMinDelay;

        float dir = Mathf.Sign(Mathf.Abs(inputX) > 0.01f ? inputX : transform.localScale.x);
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
        rb.gravityScale = 0f;

        // no stamina regen while dashing
        staminaController.StopConsumption();

        // Dash stretch effect
        LeanScale(new Vector3(1.2f, 0.8f, 1f), 0.1f);
    }

    void EndDash()
    {
        isDashing = false;
        dashTrail.emitting = isDashing;
        rb.gravityScale = defaultGravity;
        LeanScale(Vector3.one, 0.08f);
    }



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

    public bool Grounded() => grounded;
    public bool IsDashing() => isDashing;
}