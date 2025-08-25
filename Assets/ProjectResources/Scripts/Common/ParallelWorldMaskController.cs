using UnityEngine;
using UnityEngine.UI;

public class ParallelWorldMaskController : MonoBehaviour
{
    [Header("Mask Settings")]
    [SerializeField] Transform maskTransform; // assign the SpriteMask
    [SerializeField] GameObject maskObject;   // parent or mask GameObject (so we can enable/disable)

    [Header("Energy Settings")]
    [SerializeField] float maxEnergy = 1f;       // total seconds of reveal
    [SerializeField] float drainRate = 0.1f;       // energy drained per second
    [SerializeField] float regenRate = 0.05f;     // energy regen per second
    private float currentEnergy;

    [Header("Input Settings")]
    public KeyCode revealKey = KeyCode.Mouse1; // right mouse to reveal

    [Header("UI")]
    public Image energyBar; // drag UI Slider here

    private Camera mainCamera;
    private bool isRevealing;
    [SerializeField] Color full = Color.green;
    [SerializeField] Color mid = Color.yellow;
    [SerializeField] Color low = Color.red;

    void Awake()
    {
        mainCamera = Camera.main;
        currentEnergy = maxEnergy;
        if (energyBar) energyBar.fillAmount = maxEnergy;
        UpdateUI();
        maskObject.SetActive(false); // mask hidden at start
    }



    void Update()
    {
        HandleReveal();
        UpdateUI();
        if (isRevealing) FollowMouse();
    }

    void HandleReveal()
    {
        if (Input.GetKey(revealKey) && currentEnergy > 0f)
        {
            isRevealing = true;
            maskObject.SetActive(true);

            currentEnergy -= drainRate * Time.deltaTime;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                StopReveal();
            }
        }
        else
        {
            StopReveal();
            if (currentEnergy < maxEnergy)
                currentEnergy += regenRate * Time.deltaTime;
        }
    }

    void StopReveal()
    {
        if (isRevealing)
        {
            isRevealing = false;
            maskObject.SetActive(false);
        }
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
}

