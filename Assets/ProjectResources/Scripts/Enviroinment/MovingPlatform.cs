using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    public float speed = 3f;
    public float wait = 0.5f;

    private bool toB = true;
    private float waitUntil;
    private bool isLevelOver = false;

    void Start()
    {
        GameManager.Instance.OnLevelStatusUpdated += UpdateLevelStatus;
        
    }

    void UpdateLevelStatus(bool isOver)
    {
        isLevelOver = isOver;
    }

    void FixedUpdate()
    {
        if( isLevelOver ) return;
        if (Time.time < waitUntil) return;

        Vector3 target = toB ? pointB.position : pointA.position;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            toB = !toB;
            waitUntil = Time.time + wait;
        }
    }
}