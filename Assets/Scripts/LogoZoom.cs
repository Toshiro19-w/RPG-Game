using UnityEngine;

public class LogoZoom : MonoBehaviour
{
    public float zoomSpeed = 1.5f;
    public float zoomAmount = 0.1f;

    private Vector3 originalScale;
    private float timer;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime * zoomSpeed;
        float scale = 1 + Mathf.Sin(timer) * zoomAmount;
        transform.localScale = originalScale * scale;
    }
}
