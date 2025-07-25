using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float minimumLoadTime = 3.0f;
    [SerializeField] private float fadeDuration = 1.0f;

    private string sceneToLoad;

    void Start()
    {
        if (progressBar == null)
        {
            Debug.LogError("ProgressBar is not assigned in LoadingManager!");
            return;
        }
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("FadeCanvasGroup is not assigned in LoadingManager!");
            return;
        }
        if (loadingText == null)
        {
            Debug.LogWarning("LoadingText is not assigned in LoadingManager, percentage will not be displayed.");
        }

        // Kiểm tra xem có scene index không
        if (PlayerPrefs.HasKey("NextSceneIndex"))
        {
            int sceneIndex = PlayerPrefs.GetInt("NextSceneIndex");
            sceneToLoad = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            sceneToLoad = System.IO.Path.GetFileNameWithoutExtension(sceneToLoad);
            PlayerPrefs.DeleteKey("NextSceneIndex");
        }
        else
        {
            // Lấy tên scene cần tải từ PlayerPrefs
            sceneToLoad = PlayerPrefs.GetString("NextSceneToLoad", "Lobby");
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("No scene found to load! Defaulting to Lobby.");
            sceneToLoad = "Lobby";
        }

        // Bắt đầu với màn hình trong suốt (không có fade)
        fadeCanvasGroup.alpha = 0f;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Kiểm tra scene có tồn tại trong Build Settings không
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == sceneToLoad)
            {
                sceneExists = true;
                break;
            }
        }
        
        if (!sceneExists)
        {
            Debug.LogError($"Scene '{sceneToLoad}' not found in Build Settings! Loading Lobby instead.");
            sceneToLoad = "Lobby";
        }
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        if (operation == null)
        {
            Debug.LogError($"Failed to load scene: {sceneToLoad}");
            yield break;
        }
        
        Debug.Log($"Loading scene: {sceneToLoad}");
        
        operation.allowSceneActivation = false;

        float startTime = Time.time;
        bool loadingFinished = false;

        // Vòng lặp chờ loading
        while (!loadingFinished)
        {
            float actualProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float elapsedTime = Time.time - startTime;
            float displayedProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);

            // Cập nhật progress bar dựa trên tiến trình thực tế và thời gian chờ tối thiểu
            if (progressBar != null)
            {
                progressBar.value = Mathf.Max(actualProgress, displayedProgress);
                
                if (loadingText != null)
                {
                    loadingText.text = $"Loading... {(int)(progressBar.value * 100)}%";
                }
            }

            // Khi scene đã tải xong và đã đủ thời gian chờ, kết thúc vòng lặp
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
                loadingFinished = true;

            yield return null;
        }

        // Bắt đầu hiệu ứng mờ dần (fade-out)
        if (fadeCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
        }

        // Cho phép kích hoạt scene mới sau khi đã fade-out xong
        operation.allowSceneActivation = true;
    }
}