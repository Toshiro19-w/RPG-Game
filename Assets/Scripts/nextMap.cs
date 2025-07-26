using UnityEngine;
using UnityEngine.SceneManagement;

public class nextMap : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private bool requireBossDefeat = true;
    [SerializeField] private KeyCode interactKey = KeyCode.B;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject portalVisual;
    
    private bool playerInRange = false;
    
    void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
            
        UpdatePortalVisibility();
    }
    
    void Update()
    {
        UpdatePortalVisibility();
        
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryLoadNextScene();
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Hiển thị prompt nếu có thể tương tác
            if (CanInteract())
            {
                ShowInteractPrompt(true);
            }
            else if (requireBossDefeat)
            {
                Debug.Log("Hãy hạ gục boss trước khi sử dụng portal!");
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            ShowInteractPrompt(false);
        }
    }
    
    private bool CanInteract()
    {
        return !requireBossDefeat || BossManager.IsBossDefeated;
    }
    
    private void ShowInteractPrompt(bool show)
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(show);
    }
    
    private void UpdatePortalVisibility()
    {
        bool canShow = CanInteract();
        
        if (portalVisual != null)
            portalVisual.SetActive(canShow);
            
        // Ẩn/hiện collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = canShow;
    }
    
    private void TryLoadNextScene()
    {
        if (!CanInteract())
        {
            Debug.Log("Không thể sử dụng portal! Hãy hạ gục boss trước.");
            return;
        }
        
        // Đặt flag để tạo dungeon mới
        PlayerPrefs.SetInt("RegenerateDungeon", 1);
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string nextSceneName = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
            nextSceneName = System.IO.Path.GetFileNameWithoutExtension(nextSceneName);

            PlayerPrefs.SetString("NextSceneToLoad", nextSceneName);
            SceneManager.LoadScene("Loading");
        }
        else
        {
            Debug.Log("Reached the last map. Transitioning to Lobby (or Game End Scene).");
            PlayerPrefs.SetString("NextSceneToLoad", "Lobby");
            SceneManager.LoadScene("Loading");
        }
    }
}
