using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{

    [SerializeField] ParticleSystem[] movementParticleSystem;
    [SerializeField] ParticleSystem fallParticleSystem;
    [Range(0, 10)]
    [SerializeField] int occurAfterVelocity;

    [Range(0, 0.2f)]
    [SerializeField] float dustParticleInterval;
    Rigidbody2D rb;
    PlayerControllerTest playerController;


    float counter;

    void Start()
    {
        playerController = GetComponent<PlayerControllerTest>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        counter += Time.deltaTime;
        if (rb != null)
        {
            if (playerController.Grounded() && Mathf.Abs(rb.linearVelocity.x) > occurAfterVelocity && counter >= dustParticleInterval)
            {
                foreach (var movementParticleSystem in movementParticleSystem)
                    if (movementParticleSystem != null)
                    {
                        movementParticleSystem.Play();
                        counter = 0;
                    }
            }
        }
        else
        {
            Debug.Log("rigid body is null");
        }
    }

    public void Landed()
    {
        if (fallParticleSystem != null)
        {
            fallParticleSystem.Play();
        }
    }





    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Ground"))
    //     {
    //         Debug.Log("Grounded");
    //         isGrounded = true;
    //         if (fallParticleSystem != null)
    //         {
    //             fallParticleSystem.Play();
    //         }
    //     }
    // }

    // void OnTriggerExit2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Ground"))
    //     {
    //         Debug.Log("Not Grounded");
    //         isGrounded = false;
    //     }
    // }
}
