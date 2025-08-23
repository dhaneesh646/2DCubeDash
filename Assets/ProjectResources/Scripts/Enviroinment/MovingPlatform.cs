using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 3f;
    public float wait = 0.5f;
    bool toB = true;
    float waitUntil;

    void FixedUpdate()
    {
        if (Time.time < waitUntil) return;
        Vector3 target = (toB ? (Vector3)pointB : (Vector3)pointA);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            toB = !toB;
            waitUntil = Time.time + wait;
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player"))
            c.collider.transform.SetParent(transform);
    }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag("Player"))
            c.collider.transform.SetParent(null);
    }
}
