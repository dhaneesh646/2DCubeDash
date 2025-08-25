using System.Collections;
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
    [SerializeField] float minChargeTime = 0.3f;
    [SerializeField]  Image chargeFillImage;
    [SerializeField] GameObject chargeCanvas;
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
    [SerializeField] float maxLandingSquash = 0.7f; 
    [SerializeField] float landingSquashDuration = 0.15f; 
    [SerializeField] float minLandingVelocity = 5f;

    [Header("FX Hooks")]
    [SerializeField] TrailRenderer dashTrail;

    [Header("Wheels")]
    [SerializeField] Transform[] wheels;
    [SerializeField] float wheelRadius = 0.5f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private StaminaController staminaController;
    private PlayerParticleController particleController;


    private float inputX;
    private bool grounded;
    private bool wasGrounded;
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private float defaultGravity;
    private Vector3 originalScale;
    private bool isLanding;

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
        originalScale = transform.localScale;

        if (chargeFillImage != null)
            chargeFillImage.gameObject.SetActive(false);

        GameManager.Instance.OnPlayerDeath += () =>
        {
            chargeCanvas.SetActive(false);
        };
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        HandleJumpInput();



        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        if (grounded && !wasGrounded)
        {
            OnLanding();
            particleController.Landed();
        }
        wasGrounded = grounded;
        if (grounded) lastGroundedTime = Time.time;



        bool canCoyote = Time.time - lastGroundedTime <= coyoteTime;
        bool bufferedJump = Time.time - lastJumpPressedTime <= jumpBuffer;
        if (bufferedJump && canCoyote && !isDashing && !isChargingJump)
        {
            PerformJump(jumpForce);
            lastJumpPressedTime = -999f;
        }
        if (!grounded && rb.linearVelocity.y > 0f && !Input.GetButton("Jump"))
            rb.gravityScale = defaultGravity / Mathf.Max(0.01f, cutJumpGravityMultiplier);
        else
            rb.gravityScale = defaultGravity;



        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && Time.time >= nextDashTime)
        {
            if (staminaController.ConsumeStamina(dashStaminaCost))
            {
                StartDash();
            }
        }
        if (isDashing && Time.time >= dashEndTime)
            EndDash();



        UpdatechargeFillImage();


    }

    void OnLanding()
    {
        float fallSpeed = Mathf.Abs(Mathf.Min(0, rb.linearVelocity.y));

        if (fallSpeed >= minLandingVelocity)
        {
            isLanding = true;

            float squashAmount = Mathf.Clamp(fallSpeed / 15f, 0.1f, maxLandingSquash);

            Vector3 squashScale = new Vector3(
                originalScale.x * (1f + squashAmount * 0.3f), 
                originalScale.y * (1f - squashAmount),
                originalScale.z 
            );

            StartCoroutine(LandingSquash(squashScale));
        }
    }

    IEnumerator LandingSquash(Vector3 targetScale)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < landingSquashDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (landingSquashDuration / 2f);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        startScale = transform.localScale;

        while (elapsed < landingSquashDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (landingSquashDuration / 2f);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        isLanding = false;
    }

    void HandleJumpInput()
    {
        hasEnoughStaminaForCharge = staminaController.HasStaminaForAction(0.1f);

        if (Input.GetButtonDown("Jump") && (grounded || Time.time - lastGroundedTime <= coyoteTime) && hasEnoughStaminaForCharge)
        {
            chargeStartTime = Time.time;
        }

        if (Input.GetButton("Jump") && hasEnoughStaminaForCharge &&
            (grounded || Time.time - lastGroundedTime <= coyoteTime))
        {
            float holdTime = Time.time - chargeStartTime;

            if (holdTime >= minChargeTime && !isChargingJump)
            {
                StartCharging();
            }

            if (isChargingJump)
            {
                ContinueCharging();
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            if (isChargingJump)
            {
                ReleaseChargedJump();
            }
            else
            {
                lastJumpPressedTime = Time.time;
            }
        }

        if (isChargingJump && !hasEnoughStaminaForCharge)
        {
            ReleaseChargedJumpEarly();
        }
    }

    void StartCharging()
    {
        isChargingJump = true;
        currentCharge = 0f;

        if (chargeFillImage != null)
        {
            chargeCanvas.SetActive(true);
            chargeFillImage.gameObject.SetActive(true);
            chargeFillImage.fillAmount = 0f;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
    }

    void ContinueCharging()
    {
        float holdTime = Time.time - chargeStartTime;
        currentCharge = Mathf.Clamp01((holdTime - minChargeTime) / (maxChargeTime - minChargeTime));

        if (chargeFillImage.fillAmount != 1)
        {
            staminaController.ConsumeStaminaOverTime();
        }

        HandleMovement(true);
    }

    void ReleaseChargedJump()
    {
        float holdTime = Time.time - chargeStartTime;

        if (holdTime >= minChargeTime)
        {
            float chargeFactor = Mathf.Clamp01((holdTime - minChargeTime) / (maxChargeTime - minChargeTime));
            float chargedJumpForce = Mathf.Lerp(jumpForce, maxChargedJumpForce, chargeFactor);

            PerformJump(chargedJumpForce);
        }
        else
        {
            PerformJump(jumpForce);
        }

        EndCharging();
    }

    void ReleaseChargedJumpEarly()
    {
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

        if (chargeFillImage != null)
            chargeFillImage.gameObject.SetActive(false);
    }

    void PerformJump(float force)
    {
        AudioManager.Instance.PlayEffect(SoundEffect.PlayerJump);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        HandleMovement(false);

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

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? accel : decel;
        if (!grounded) accelRate *= airControl;

        if (Mathf.Abs(targetSpeed) < 0.01f)
            accelRate *= 1.5f;

        if (isCharging)
            accelRate *= 0.7f;

        float movement = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(movement, rb.linearVelocity.y);
    }

    void RotateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;

        if (!grounded && !isDashing) return;

        float angularSpeed = rb.linearVelocity.x / wheelRadius;
        float rotationAmount = angularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime;

        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.forward, -rotationAmount);
        }
    }

    void StartDash()
    {
        AudioManager.Instance.PlayEffect(SoundEffect.PlayerDash);
        isDashing = true;
        dashTrail.emitting = isDashing;
        dashEndTime = Time.time + dashTime;
        nextDashTime = Time.time + dashMinDelay;

        float dir = Mathf.Sign(Mathf.Abs(inputX) > 0.01f ? inputX : transform.localScale.x);
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
        rb.gravityScale = 0f;

        staminaController.StopConsumption();

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