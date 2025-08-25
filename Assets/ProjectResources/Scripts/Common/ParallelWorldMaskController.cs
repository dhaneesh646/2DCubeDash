using UnityEngine;
using UnityEngine.UI;

public class ParallelWorldMaskController : MonoBehaviour
{
    [Header("Mask Settings")]
    [SerializeField] Transform maskTransform;
    [SerializeField] GameObject maskObject;

    [Header("Energy Settings")]
    [SerializeField] float maxEnergy = 1f;
    [SerializeField] float drainRate = 0.1f;
    [SerializeField] float regenRate = 0.05f;
    private float currentEnergy;

    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.E; // 👈 Toggle key for ability

    [Header("UI")]
    public Image energyBar;
    [SerializeField] Color full = Color.green;
    [SerializeField] Color mid = Color.yellow;
    [SerializeField] Color low = Color.red;

    private Camera mainCamera;
    private bool isRevealing;
    public static bool IsMaskActive { get; private set; }

    void Awake()
    {
        mainCamera = Camera.main;
        currentEnergy = maxEnergy;
        if (energyBar) energyBar.fillAmount = maxEnergy;
        UpdateUI();
        maskObject.SetActive(false);

        GameManager.Instance.OnPlayerRespawn += () =>
        {
            currentEnergy = maxEnergy;
            UpdateUI();
            StopReveal();
        };
    }

    void Update()
    {
        HandleToggleInput();
        HandleEnergy();
        UpdateUI();
        if (isRevealing) FollowMouse();
    }

    void HandleToggleInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (!isRevealing && currentEnergy > 0f)
            {
                StartReveal();
            }
            else
            {
                StopReveal();
            }
        }
    }

    void HandleEnergy()
    {
        if (isRevealing)
        {
            currentEnergy -= drainRate * Time.deltaTime;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                StopReveal(); // 👈 Auto-disable if energy runs out
            }
        }
        else
        {
            if (currentEnergy < maxEnergy)
                currentEnergy += regenRate * Time.deltaTime;
        }
    }

    void StartReveal()
    {
        isRevealing = true;
        maskObject.SetActive(true);
        IsMaskActive = true;
    }

    void StopReveal()
    {
        isRevealing = false;
        maskObject.SetActive(false);
        IsMaskActive = false;
    }

    void FollowMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        maskTransform.position = new Vector3(worldPos.x, worldPos.y, maskTransform.position.z);
    }

    void UpdateUI()
    {
        if (energyBar) energyBar.fillAmount = currentEnergy;

        float t = currentEnergy / maxEnergy;
        if (t > 0.5f)
        {
            float lerpT = (t - 0.5f) / 0.5f;
            energyBar.color = Color.Lerp(mid, full, lerpT);
        }
        else
        {
            float lerpT = t / 0.5f;
            energyBar.color = Color.Lerp(low, mid, lerpT);
        }
    }

    void OnDestroy()
    {
        GameManager.Instance.OnPlayerRespawn -= () =>
        {
            currentEnergy = maxEnergy;
            UpdateUI();
            StopReveal();
        };
    }
}