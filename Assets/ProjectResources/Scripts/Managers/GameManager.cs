using System;
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
    public Action<bool> OnLevelStatusUpdated;
    public Action OnPlayerRespawn;
    public Action OnPlayerDeath;
    public Action<GameObject> PlayerKilled;
    public Action OnLevelCompleted;
    public Action<string> MoveNextLevel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        respawnPoint = defaultSpawnPoint ? defaultSpawnPoint.position : Vector2.zero;
        PlayerKilled = PlayerDied;
        MoveNextLevel += (nextSceneName) => StartCoroutine(LoadNextScene(nextSceneName));
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
        dieCoroutine = StartCoroutine(RespawnPlayer(player));
    }

    public IEnumerator RespawnPlayer(GameObject player)
    {
        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }

        if (isPlayerAlive)
        {
            OnPlayerDeath?.Invoke();
            playerParticles.Die(player.transform);
            isPlayerAlive = false;
        }

        yield return new WaitForSeconds(1.5f);
        player.transform.position = respawnPoint;
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;


        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = true;
        }
        isPlayerAlive = true;
        OnPlayerRespawn?.Invoke();

    }

    public void LevelComplete(string nextSceneName = null)
    {
        Debug.Log("Level Complete! Loading next level...");
        AudioManager.Instance.PlayEffect(SoundEffect.LevelComplete);
        OnLevelStatusUpdated?.Invoke(true);
        OnLevelCompleted?.Invoke();
    }

    IEnumerator LoadNextScene(string nextSceneName)
    {
        yield return new WaitForSeconds(2f);
        if (!string.IsNullOrEmpty(nextSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        MoveNextLevel = null;
        PlayerKilled = null;
    }
}
