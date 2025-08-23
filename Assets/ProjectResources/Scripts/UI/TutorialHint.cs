using UnityEngine;
using TMPro;

public class TutorialHint : MonoBehaviour
{
    public string hintText;
    private TextMeshPro textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        textMesh.text = "";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            textMesh.text = hintText;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            textMesh.text = "";
    }
}
