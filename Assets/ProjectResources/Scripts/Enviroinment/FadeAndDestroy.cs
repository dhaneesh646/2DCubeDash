using UnityEngine;

public class FadeAndDestroy : MonoBehaviour
{
    [SerializeField] float life = 1.8f;
    [SerializeField] float fade = 0.7f;

    SpriteRenderer sr;
    float t;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        t += Time.deltaTime;
        if (t > life)
        {
            float a = Mathf.Clamp01(1f - (t - life) / fade);
            var c = sr.color; c.a = a; sr.color = c;
            if (a <= 0f) Destroy(gameObject);
        }
    }
}
