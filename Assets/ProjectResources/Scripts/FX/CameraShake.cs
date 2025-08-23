// Scripts/CameraShake.cs
using UnityEngine;
public class CameraShake : MonoBehaviour
{
    Vector3 origin;
    void Awake() { origin = transform.position; }
    public void Shake(float amp = 0.15f, float dur = 0.08f)
    { StopAllCoroutines(); StartCoroutine(Co(amp, dur)); }
    System.Collections.IEnumerator Co(float a, float d)
    {
        float t = 0; while (t < d)
        {
            t += Time.deltaTime;
            transform.position = origin + (Vector3)Random.insideUnitCircle * a;
            yield return null;
        }
        transform.position = origin;
    }
}
