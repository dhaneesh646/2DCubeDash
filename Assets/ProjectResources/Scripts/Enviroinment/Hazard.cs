using UnityEngine;
using UnityEngine.SceneManagement;

public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Dead");
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.PlayerDied(other.gameObject);
    }
}
