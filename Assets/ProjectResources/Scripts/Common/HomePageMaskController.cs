using UnityEngine;

public class HomeageMaskController : MonoBehaviour
{
    [SerializeField] Transform maskTransform;
    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        maskTransform.position = new Vector3(worldPos.x, worldPos.y, maskTransform.position.z);
    }
}
