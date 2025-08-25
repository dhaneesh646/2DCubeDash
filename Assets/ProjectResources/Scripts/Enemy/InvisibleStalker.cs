using UnityEngine;

public class InvisibleStalker : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] float patrolSpeed = 2f;

    [Header("Enemy Settings")]
    [SerializeField] float killRange = 1f;
    [SerializeField] ParticleSystem deathEffect;

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
            targetPoint = targetPoint == pointA.position ? pointB.position : pointA.position;
        }
    }

    void UpdateFacingDirection()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (targetPoint == pointA.position);
        }
    }

    void CheckForPlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);

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
        GameManager.Instance.PlayerKilled?.Invoke(player.gameObject);
    }

    void Die()
    {
        isAlive = false;

        if (deathEffect != null)
        {
            deathEffect.Play();
        }

        GetComponent<Collider2D>().enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        Destroy(gameObject, 0.5f);

        Debug.Log("Enemy defeated by dash!");
    }
}