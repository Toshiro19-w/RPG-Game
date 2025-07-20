using UnityEngine;
using System.Collections;

public class FadeInManager : MonoBehaviour
{
    [Tooltip("CanvasGroup được sử dụng cho hiệu ứng fade. Nó nên bắt đầu với Alpha = 1.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("Thời gian (giây) cho hiệu ứng fade-in.")]
    [SerializeField] private float fadeDuration = 1.0f;

    void Start()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("FadeCanvasGroup chưa được gán trong FadeInManager!", this);
            // Tự hủy để tránh lỗi
            Destroy(gameObject);
            return;
        }

        // Đảm bảo màn hình bắt đầu là màu đen và bắt đầu coroutine fade-in
        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = 1.0f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        // Đảm bảo màn hình hoàn toàn trong suốt và không chặn tương tác
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}