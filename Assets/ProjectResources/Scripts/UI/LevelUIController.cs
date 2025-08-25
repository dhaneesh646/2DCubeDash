using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] GameObject levelCompleteUI;
    [SerializeField] GameObject menuPanel;
    [SerializeField] Button resumeButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuPanelMenuButton;
    [SerializeField] string nextSceneName;

    [SerializeField] Button goToNextLevelButton;
    [SerializeField] Button levelPanelMenuButtomn;
    void Start()
    {
        GameManager.Instance.OnLevelCompleted += () =>
        {
            levelCompleteUI.SetActive(true);
            Time.timeScale = 0f;
        };

        resumeButton.onClick.AddListener(() =>
        {
            menuPanel.SetActive(false);
            Time.timeScale = 1f;
        });

        restartButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        goToNextLevelButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(nextSceneName))
                GameManager.Instance.MoveNextLevel?.Invoke(nextSceneName);
        });

        levelPanelMenuButtomn.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        });

        menuPanelMenuButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel.activeSelf)
            {
                menuPanel.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                menuPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
    
    
}
