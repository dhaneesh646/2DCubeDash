using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public float breakVelocityThreshold = 14f;
    public ParticleSystem breakFX;
    public AudioSource breakSFX;

    void OnCollisionEnter2D(Collision2D c)
    {
        if (!c.collider.CompareTag("Player")) return;
        if (Mathf.Abs(c.relativeVelocity.x) >= breakVelocityThreshold)
        {
            if (breakFX) Instantiate(breakFX, c.contacts[0].point, Quaternion.identity);
            if (breakSFX) AudioSource.PlayClipAtPoint(breakSFX.clip, transform.position);
            Destroy(gameObject);
        }
    }
}
