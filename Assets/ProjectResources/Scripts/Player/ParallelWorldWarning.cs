using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParallelWorldWarning : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Image warningFillImage;
    [SerializeField] CanvasGroup warningCanvasGroup;
    [SerializeField] RectTransform warningTransform; // For scaling animation
    
    [Header("Warning Settings")]
    [SerializeField] float maxDetectionRange = 10f;
    [SerializeField] float minWarningDistance = 3f;
    [SerializeField] float maxPulseIntensity = 0.8f;
    [SerializeField] float basePulseSpeed = 1f;
    
    [Header("Heart Animation Settings")]
    [SerializeField] float minHeartScale = 0.8f;
    [SerializeField] float maxHeartScale = 1.2f;
    [SerializeField] float scaleAnimationSpeed = 3f;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color dangerColor = Color.red;
    [SerializeField] float colorTransitionSpeed = 2f;
    
    [Header("Audio Settings")]
    [SerializeField] float audioMaxIntensity = 1f;
    [SerializeField] float audioActivationThreshold = 0.2f; // Minimum danger level to activate audio
    
    [Header("Hazard Settings")]
    [SerializeField] string hazardTag = "ParallelHazard";
    
    private Transform player;
    private bool isWarningActive = false;
    private Coroutine warningCoroutine;
    private Vector3 originalHeartScale;
    private Color originalHeartColor;
    private float currentDangerLevel = 0f;
    private bool audioWarningActive = false;
    
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        GameManager.Instance.OnPlayerRespawn += () =>
        {
            isWarningActive = false;
            currentDangerLevel = 0f;
            
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            StartCoroutine(FadeOutWarning());
        };
        
        if (warningFillImage != null)
        {
            warningFillImage.fillAmount = 0f;
            originalHeartColor = warningFillImage.color;
        }
        
        if (warningCanvasGroup != null)
        {
            warningCanvasGroup.alpha = 0f;
        }
        
        if (warningTransform != null)
        {
            originalHeartScale = warningTransform.localScale;
        }
    }
    
    void Update()
    {
        CheckForHazards();
        UpdateHeartbeatAudio();
    }
    
    void CheckForHazards()
    {
        GameObject[] hazards = GameObject.FindGameObjectsWithTag(hazardTag);
        float closestDistance = Mathf.Infinity;
        bool hazardNearby = false;
        
        foreach (GameObject hazard in hazards)
        {
            float distance = Vector2.Distance(player.position, hazard.transform.position);
            
            if (distance < maxDetectionRange)
            {
                hazardNearby = true;
                closestDistance = Mathf.Min(closestDistance, distance);
            }
        }
        
        currentDangerLevel = hazardNearby ? CalculateDangerLevel(closestDistance) : 0f;
        
        if (hazardNearby)
        {
            if (!isWarningActive)
            {
                isWarningActive = true;
                if (warningCoroutine != null) StopCoroutine(warningCoroutine);
                warningCoroutine = StartCoroutine(PulseWarning(currentDangerLevel));
            }
        }
        else if (isWarningActive)
        {
            isWarningActive = false;
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            StartCoroutine(FadeOutWarning());
        }
    }
    
    void UpdateHeartbeatAudio()
    {
        bool shouldAudioBeActive = currentDangerLevel >= audioActivationThreshold;
        
        if (shouldAudioBeActive && !audioWarningActive)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetHeartbeatIntensity(currentDangerLevel * audioMaxIntensity);
            }
            audioWarningActive = true;
        }
        else if (!shouldAudioBeActive && audioWarningActive)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopHeartbeat();
            }
            audioWarningActive = false;
        }
        else if (audioWarningActive)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetHeartbeatIntensity(currentDangerLevel * audioMaxIntensity);
            }
        }
    }
    
    float CalculateDangerLevel(float distance)
    {
        float normalizedDistance = Mathf.Clamp01((maxDetectionRange - distance) / (maxDetectionRange - minWarningDistance));
        
        float exponentialDanger = Mathf.Pow(normalizedDistance, 0.5f); 
        
        return Mathf.Clamp01(exponentialDanger);
    }
    
    IEnumerator PulseWarning(float baseIntensity)
    {
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            warningCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        warningCanvasGroup.alpha = 1f;
        
        while (isWarningActive)
        {
            GameObject[] hazards = GameObject.FindGameObjectsWithTag(hazardTag);
            float closestDistance = Mathf.Infinity;
            
            foreach (GameObject hazard in hazards)
            {
                float distance = Vector2.Distance(player.position, hazard.transform.position);
                if (distance < closestDistance) closestDistance = distance;
            }
            
            currentDangerLevel = CalculateDangerLevel(closestDistance);
            
            float pulseRate = Mathf.Lerp(basePulseSpeed, basePulseSpeed * 3f, currentDangerLevel); // Faster pulse for more danger
            float intensity = Mathf.Lerp(0.3f, maxPulseIntensity, currentDangerLevel);
            
            yield return StartCoroutine(AnimateWarningBeat(pulseRate, intensity, currentDangerLevel));
        }
    }
    
    IEnumerator AnimateWarningBeat(float pulseRate, float intensity, float dangerLevel)
    {
        float beatDuration = 1f / pulseRate;
        float elapsed = 0f;
        
        Color targetColor = Color.Lerp(normalColor, dangerColor, dangerLevel);
        
        while (elapsed < beatDuration && isWarningActive)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / beatDuration;
            
            float beatPattern = Mathf.Sin(t * Mathf.PI * 2f);
            
            warningFillImage.fillAmount = (beatPattern * 0.5f + 0.5f) * intensity;
            
            if (warningTransform != null)
            {
                float scaleFactor = Mathf.Lerp(minHeartScale, maxHeartScale, (beatPattern + 1f) * 0.5f);
                warningTransform.localScale = originalHeartScale * scaleFactor;
            }
            
            if (warningFillImage != null)
            {
                warningFillImage.color = Color.Lerp(originalHeartColor, targetColor, dangerLevel);
            }
            
            yield return null;
        }
        
        if (warningTransform != null)
        {
            warningTransform.localScale = originalHeartScale;
        }
    }
    
    IEnumerator FadeOutWarning()
    {
        float fadeDuration = 0.5f;
        float elapsed = 0f;
        float startAlpha = warningCanvasGroup.alpha;
        Vector3 startScale = warningTransform != null ? warningTransform.localScale : Vector3.one;
        Color startColor = warningFillImage != null ? warningFillImage.color : Color.white;
        
        if (audioWarningActive && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopHeartbeat();
            audioWarningActive = false;
        }
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            warningCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            
            if (warningTransform != null)
            {
                warningTransform.localScale = Vector3.Lerp(startScale, originalHeartScale, t);
            }
            
            if (warningFillImage != null)
            {
                warningFillImage.color = Color.Lerp(startColor, originalHeartColor, t);
            }
            
            yield return null;
        }
        
        warningCanvasGroup.alpha = 0f;
        warningFillImage.fillAmount = 0f;
        
        if (warningTransform != null)
        {
            warningTransform.localScale = originalHeartScale;
        }
        
        if (warningFillImage != null)
        {
            warningFillImage.color = originalHeartColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, maxDetectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minWarningDistance);
            
            if (Application.isPlaying)
            {
                Gizmos.color = Color.Lerp(Color.green, Color.red, currentDangerLevel);
                Gizmos.DrawWireSphere(player.position, 0.5f + currentDangerLevel * 0.5f);
            }
        }
    }

    void OnDestroy()
    {
        if (audioWarningActive && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopHeartbeat();
        }
        GameManager.Instance.OnPlayerRespawn -= () =>
        {
            isWarningActive = false;
            currentDangerLevel = 0f;
            
            if (warningCoroutine != null) StopCoroutine(warningCoroutine);
            StartCoroutine(FadeOutWarning());
        };
    }
}