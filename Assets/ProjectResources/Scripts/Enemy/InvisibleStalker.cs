using UnityEngine;

public class InvisibleStalker : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] float patrolSpeed = 2f;

    [Header("Enemy Settings")]
    [SerializeField] float killRange = 1f; // Range to kill player
    [SerializeField] ParticleSystem deathEffect; // Particle effect when enemy dies

    private Transform player;
    private Vector3 targetPoint;
    private bool isAlive = true;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPoint = pointB.position;
    }

    void Update()
    {
        if (!isAlive) return;

        Patrol();
        CheckForPlayer();
        UpdateFacingDirection();
    }

    void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            // Flip target point
            targetPoint = targetPoint == pointA.position ? pointB.position : pointA.position;
        }
    }

    void UpdateFacingDirection()
    {
        // Face the direction of movement
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (targetPoint == pointA.position);
        }
    }

    void CheckForPlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // Kill player if they get too close (without dashing)
        if (dist < killRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsDashing())
            {
                KillPlayer();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        // Check if player dashed into us
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsDashing())
            {
                Die();
            }
        }
    }

    void KillPlayer()
    {
        Debug.Log("Player killed by enemy!");
        GameManager.Instance.PlayerDied(player.gameObject);
    }

    void Die()
    {
        isAlive = false;

        // Play death effect
        if (deathEffect != null)
        {
            deathEffect.Play();
        }
        //  if (CameraShake.Instance != null)
        //     CameraShake.Instance.Shake(0.2f, 0.15f);

        GetComponent<Collider2D>().enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        Destroy(gameObject, 0.5f);

        Debug.Log("Enemy defeated by dash!");
    }
}