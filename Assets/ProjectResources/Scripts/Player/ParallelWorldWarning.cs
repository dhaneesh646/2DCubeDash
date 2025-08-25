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
        
        // Initialize UI
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
        // Find all hazards in the scene
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
        
        // Calculate current danger level
        currentDangerLevel = hazardNearby ? CalculateDangerLevel(closestDistance) : 0f;
        
        // Update warning system based on hazard proximity
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
        // Only activate audio if danger level exceeds threshold
        bool shouldAudioBeActive = currentDangerLevel >= audioActivationThreshold;
        
        if (shouldAudioBeActive && !audioWarningActive)
        {
            // Start heartbeat audio
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetHeartbeatIntensity(currentDangerLevel * audioMaxIntensity);
            }
            audioWarningActive = true;
        }
        else if (!shouldAudioBeActive && audioWarningActive)
        {
            // Stop heartbeat audio
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopHeartbeat();
            }
            audioWarningActive = false;
        }
        else if (audioWarningActive)
        {
            // Update heartbeat intensity based on current danger
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetHeartbeatIntensity(currentDangerLevel * audioMaxIntensity);
            }
        }
    }
    
    float CalculateDangerLevel(float distance)
    {
        // Calculate danger level from 0 to 1 based on distance
        // Inverse relationship: closer distance = higher danger
        float normalizedDistance = Mathf.Clamp01((maxDetectionRange - distance) / (maxDetectionRange - minWarningDistance));
        
        // Add exponential curve for more intense warning at close range
        float exponentialDanger = Mathf.Pow(normalizedDistance, 0.5f); // Square root for faster ramp-up
        
        return Mathf.Clamp01(exponentialDanger);
    }
    
    IEnumerator PulseWarning(float baseIntensity)
    {
        // Fade in quickly
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            warningCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        warningCanvasGroup.alpha = 1f;
        
        // Continuous pulsing with heart animation
        while (isWarningActive)
        {
            // Recalculate danger level each pulse in case hazard moved
            GameObject[] hazards = GameObject.FindGameObjectsWithTag(hazardTag);
            float closestDistance = Mathf.Infinity;
            
            foreach (GameObject hazard in hazards)
            {
                float distance = Vector2.Distance(player.position, hazard.transform.position);
                if (distance < closestDistance) closestDistance = distance;
            }
            
            currentDangerLevel = CalculateDangerLevel(closestDistance);
            
            // Calculate pulse parameters based on danger level
            float pulseRate = Mathf.Lerp(basePulseSpeed, basePulseSpeed * 3f, currentDangerLevel); // Faster pulse for more danger
            float intensity = Mathf.Lerp(0.3f, maxPulseIntensity, currentDangerLevel);
            
            // Animate one complete heart beat
            yield return StartCoroutine(AnimateHeartBeat(pulseRate, intensity, currentDangerLevel));
        }
    }
    
    IEnumerator AnimateHeartBeat(float pulseRate, float intensity, float dangerLevel)
    {
        float beatDuration = 1f / pulseRate;
        float elapsed = 0f;
        
        // Calculate target color based on danger level
        Color targetColor = Color.Lerp(normalColor, dangerColor, dangerLevel);
        
        while (elapsed < beatDuration && isWarningActive)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / beatDuration;
            
            // Create a heart beat pattern using a sine wave
            float beatPattern = Mathf.Sin(t * Mathf.PI * 2f);
            
            // Fill amount animation (pulsing)
            warningFillImage.fillAmount = (beatPattern * 0.5f + 0.5f) * intensity;
            
            // Heart scale animation (beating)
            if (warningTransform != null)
            {
                float scaleFactor = Mathf.Lerp(minHeartScale, maxHeartScale, (beatPattern + 1f) * 0.5f);
                warningTransform.localScale = originalHeartScale * scaleFactor;
            }
            
            // Color transition based on danger
            if (warningFillImage != null)
            {
                warningFillImage.color = Color.Lerp(originalHeartColor, targetColor, dangerLevel);
            }
            
            yield return null;
        }
        
        // Ensure we return to original scale at the end of beat
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
        
        // Stop heartbeat audio when warning fades out
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
            
            // Also scale down and return to normal color during fade out
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
        
        // Reset to original values
        if (warningTransform != null)
        {
            warningTransform.localScale = originalHeartScale;
        }
        
        if (warningFillImage != null)
        {
            warningFillImage.color = originalHeartColor;
        }
    }
    
    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        if (player != null)
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, maxDetectionRange);
            
            // Draw warning range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minWarningDistance);
            
            // Draw current danger level if in play mode
            if (Application.isPlaying)
            {
                Gizmos.color = Color.Lerp(Color.green, Color.red, currentDangerLevel);
                Gizmos.DrawWireSphere(player.position, 0.5f + currentDangerLevel * 0.5f);
            }
        }
    }
    
    // Public method to manually trigger warning (optional)
    public void TriggerWarning(float intensity, float duration)
    {
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(ManualWarning(intensity, duration));
    }
    
    IEnumerator ManualWarning(float intensity, float duration)
    {
        isWarningActive = true;
        
        // Start audio warning
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetHeartbeatIntensity(intensity * audioMaxIntensity);
            audioWarningActive = true;
        }
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            warningCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
            yield return null;
        }
        
        warningCanvasGroup.alpha = 1f;
        
        // Pulse for duration
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float currentDanger = Mathf.PingPong((Time.time - startTime) * 2f, 1f);
            yield return StartCoroutine(AnimateHeartBeat(basePulseSpeed * 2f, intensity, currentDanger));
        }
        
        // Fade out
        StartCoroutine(FadeOutWarning());
        isWarningActive = false;
    }
    
    // Public access to current danger level for other systems
    public float GetCurrentDangerLevel()
    {
        return currentDangerLevel;
    }
    
    public bool IsWarningActive()
    {
        return isWarningActive;
    }
    
    // Clean up audio when disabled or destroyed
    void OnDisable()
    {
        if (audioWarningActive && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopHeartbeat();
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