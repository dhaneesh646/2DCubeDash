using UnityEngine;
using UnityEngine.UI;

public class StaminaController : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaConsumptionRate = 25f; // per second
    [SerializeField] float staminaRegenerationRate = 15f; // per second
    [SerializeField] float regenerationDelay = 1f; // seconds before regeneration starts
    
    [Header("UI References")]
    [SerializeField] Slider staminaSlider;
    
    private float currentStamina;
    private float lastConsumptionTime;
    private bool isConsuming;
    
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => currentStamina / maxStamina;
    
    void Start()
    {
        currentStamina = maxStamina;
        UpdateStaminaUI();
    }
    
    void Update()
    {
        if (isConsuming)
        {
            lastConsumptionTime = Time.time;
        }
        else if (Time.time - lastConsumptionTime >= regenerationDelay && currentStamina < maxStamina)
        {
            RegenerateStamina();
        }
    }
    
    public bool CanConsumeStamina(float amount)
    {
        return currentStamina >= amount;
    }
    
    public bool ConsumeStamina(float amount)
    {
        if (CanConsumeStamina(amount))
        {
            currentStamina = Mathf.Max(0, currentStamina - amount);
            isConsuming = true;
            UpdateStaminaUI();
            return true;
        }
        return false;
    }
    
    public void ConsumeStaminaOverTime()
    {
        float consumption = staminaConsumptionRate * Time.deltaTime;
        if (ConsumeStamina(consumption))
        {
            isConsuming = true;
        }
        else
        {
            isConsuming = false;
        }
    }
    
    public void StopConsumption()
    {
        isConsuming = false;
    }
    
    private void RegenerateStamina()
    {
        float regeneration = staminaRegenerationRate * Time.deltaTime;
        currentStamina = Mathf.Min(maxStamina, currentStamina + regeneration);
        UpdateStaminaUI();
    }
    
    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = StaminaPercentage;
        }
    }
    
    // For external queries
    public bool HasStaminaForAction(float requiredPercentage = 0.1f)
    {
        return StaminaPercentage >= requiredPercentage;
    }
}