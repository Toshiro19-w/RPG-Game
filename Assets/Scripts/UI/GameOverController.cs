using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button lobbyButton;
    
    [Header("Settings")]
    [SerializeField] private float delayBeforeShow = 1f;
    
    private static GameOverController instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Ẩn panel khi bắt đầu
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Gán sự kiện cho các nút
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(TryAgain);
            
        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(GoToLobby);
    }
    
    public static void ShowGameOver()
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ShowPanelWithDelay());
        }
    }
    
    private IEnumerator ShowPanelWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    
    private void TryAgain()
    {
        // Load lại scene hiện tại
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    private void GoToLobby()
    {
        // Chuyển từ scene hiện tại -> Loading -> Lobby
        PlayerPrefs.SetString("NextSceneToLoad", "Lobby");
        
        // Kiểm tra scene Loading có tồn tại không
        try
        {
            SceneManager.LoadScene("Loading");
        }
        catch
        {
            Debug.LogError("Loading scene not found! Loading Lobby directly.");
            SceneManager.LoadScene("Lobby");
        }
    }
}