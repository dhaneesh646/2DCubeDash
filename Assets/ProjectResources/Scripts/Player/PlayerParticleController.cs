using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem movementParticleSystem;
    [SerializeField] ParticleSystem fallParticleSystem;
    [Range(0, 10)]
    [SerializeField] int occurAfterVelocity;

    [Range(0, 0.2f)]
    [SerializeField] float dustParticleInterval;
    [SerializeField] Rigidbody2D rb;
    private bool isGrounded;

    float counter;

    


    void Update()
    {
        counter += Time.deltaTime;
        if (rb != null)
        {
            if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > occurAfterVelocity && counter >= dustParticleInterval)
            {
                movementParticleSystem.Play();
                counter = 0;
            }
        }
        else
        {
            Debug.Log("rigid body is null");
        }
    }

    

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            Debug.Log("Grounded");
            isGrounded = true;
            if (fallParticleSystem != null)
            {
                fallParticleSystem.Play();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            Debug.Log("Not Grounded");
            isGrounded = false;
        }
    }
}
