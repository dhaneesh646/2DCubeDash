using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Vector2 respawnPoint;
    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] PlayerParticleController playerParticles;
    private bool isPlayerAlive = true;

    private Coroutine dieCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        respawnPoint = defaultSpawnPoint ? defaultSpawnPoint.position : Vector2.zero;
    }

    public void SetRespawnPoint(Vector2 pos)
    {
        Debug.Log("Set Respawn Point: " + pos);
        respawnPoint = pos;
    }

    public void PlayerDied(GameObject player)
    {
        if (dieCoroutine != null)
        {
            StopCoroutine(dieCoroutine);
        }
        StartCoroutine(RespawnPlayer(player));
    }

    public IEnumerator RespawnPlayer(GameObject player)
    {
        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }
        if (isPlayerAlive)
        {
            playerParticles.Die(player.transform);
            isPlayerAlive = false;
        }

        yield return new WaitForSeconds(1.5f);
        player.transform.position = respawnPoint;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        var stamina = player.GetComponent<StaminaController>();
        if (stamina) stamina.StopConsumption();
        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = true;
        }
        isPlayerAlive = true;

    }

    public void LevelComplete(string nextSceneName = null)
    {
        Debug.Log("Level Complete! Loading next level...");
        AudioManager.Instance.PlayEffect(SoundEffect.LevelComplete);
        StartCoroutine(LoadNextScene("nextSceneName"));
    }

    IEnumerator LoadNextScene(string nextSceneName)
    {
        yield return new WaitForSeconds(2f);
        if (!string.IsNullOrEmpty(nextSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
