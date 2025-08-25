using UnityEngine;
using UnityEngine.UI;

public class HomeController : MonoBehaviour
{
    [SerializeField] Button playButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playButton.onClick.AddListener(() =>
        {
            GetComponent<OnboardingController>().StartOnboarding();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
