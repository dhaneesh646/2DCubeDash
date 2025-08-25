using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class OnboardingController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject onboardingPanel;
    [SerializeField] Image displayImage;
    [SerializeField] List<Sprite> onboardingSprites; 

    private int currentIndex = 0;
    [SerializeField] Button nextButton;

    void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public void StartOnboarding()
    {
        if (onboardingSprites.Count > 0)
        {
            onboardingPanel.SetActive(true);
            displayImage.sprite = onboardingSprites[0];
            currentIndex = 1; 
        }
    }



    public void OnNextButtonClicked()
    {
        if (currentIndex < onboardingSprites.Count)
        {
            displayImage.sprite = onboardingSprites[currentIndex];
            currentIndex++;
        }
        else
        {
            LoadGameScene();
        }
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("Level1");
    }
}