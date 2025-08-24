using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Color inactiveColor = Color.white;
    [SerializeField] Color activeColor = Color.green;
    [SerializeField] float colorTransitionDuration = 0.5f;
    [SerializeField] float scaleBounceAmount = 1.2f;
    [SerializeField] float scaleBounceDuration = 0.3f;
    [SerializeField] float rotationAmount = 15f;
    [SerializeField] float rotationDuration = 0.3f;
    [SerializeField] Transform spawnpoint;

    private SpriteRenderer sr;
    private bool isActive;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = inactiveColor;
        spawnpoint = transform;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            GameManager.Instance.SetRespawnPoint(spawnpoint.position);

            isActive = true;
            if (sr) StartCoroutine(AnimateColor(sr.color, activeColor));
            StartCoroutine(AnimateScaleBounce());
            StartCoroutine(AnimateRotationWiggle());
        }
    }

    System.Collections.IEnumerator AnimateColor(Color fromColor, Color toColor)
    {
        float elapsed = 0f;
        while (elapsed < colorTransitionDuration)
        {
            sr.color = Color.Lerp(fromColor, toColor, elapsed / colorTransitionDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = toColor;
    }

    System.Collections.IEnumerator AnimateScaleBounce()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleBounceAmount;
        float elapsed = 0f;

        while (elapsed < scaleBounceDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / scaleBounceDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < scaleBounceDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / scaleBounceDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    System.Collections.IEnumerator AnimateRotationWiggle()
    {
        Quaternion originalRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotationAmount);
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }
}