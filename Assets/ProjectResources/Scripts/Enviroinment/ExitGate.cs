using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGate : MonoBehaviour
{
    public string nextSceneName;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!string.IsNullOrEmpty(nextSceneName))
            GameManager.Instance.LevelComplete(nextSceneName);
    }
}
