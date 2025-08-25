using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BreakableWall : MonoBehaviour
{
    [Header("Break Rules")]
    [SerializeField] float breakVelocityThreshold = 14f;
    [SerializeField] bool dashAlwaysBreaks = true;

    [Header("Prefabs / FX")]
    [SerializeField] GameObject brokenWallPrefab; 

    [Header("Shards Impulse")]
    [SerializeField] float shardForceMin = 4f;
    [SerializeField] float shardForceMax = 7f;
    [SerializeField] float shardTorque = 200f;

    bool broken = false;

    void OnCollisionEnter2D(Collision2D c)
    {
        if (broken) return;
        if (!c.collider.CompareTag("Player")) return;

        var player = c.collider.GetComponent<PlayerController>();
        if (player == null) return;

        var contact = c.contacts.Length > 0 ? c.contacts[0] : default;
        var point = contact.point;
        var normal = contact.normal; 

        if (dashAlwaysBreaks && player.IsDashing())
        {
            Break(point, normal);
            return;
        }

        if (Mathf.Abs(c.relativeVelocity.x) >= breakVelocityThreshold ||
            Mathf.Abs(c.relativeVelocity.y) >= breakVelocityThreshold)
        {
            Break(point, normal);
        }
    }

    void Break(Vector2 contactPoint, Vector2 normal)
    {
        broken = true;

        if (brokenWallPrefab)
        {
            var shardsRoot = Instantiate(brokenWallPrefab, transform.position, transform.rotation);
            var rbs = shardsRoot.GetComponentsInChildren<Rigidbody2D>();

            foreach (var rb in rbs)
            {
                Vector2 dir = ((Vector2)rb.worldCenterOfMass - contactPoint).normalized;
                dir = (dir + Random.insideUnitCircle * 0.25f).normalized;

                float force = Random.Range(shardForceMin, shardForceMax);
                rb.AddForce(dir * force, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-shardTorque, shardTorque), ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
