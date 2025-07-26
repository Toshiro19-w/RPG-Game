using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class nextMap : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private bool requireBossDefeat = true;
    [SerializeField] private KeyCode interactKey = KeyCode.B;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject portalVisual;

    [Header("Random Map Pool")]
    [Tooltip("Điền tên các Scene map bạn muốn dịch chuyển ngẫu nhiên tới vào đây.")]
    [SerializeField] private List<string> randomMapPool = new List<string>();
    
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

        // Kiểm tra xem danh sách map có rỗng không
        if (randomMapPool.Count == 0)
        {
            Debug.LogError("Random Map Pool đang bị trống! Hãy gán tên các Scene map vào trong Inspector.");
            return;
        }
        
        // Đặt flag để tạo dungeon mới (nếu cần)
        PlayerPrefs.SetInt("RegenerateDungeon", 1);
        
        // --- LOGIC CHỌN MAP NGẪU NHIÊN ---
        // 1. Lấy một chỉ số ngẫu nhiên từ 0 đến số lượng map trong danh sách - 1
        int randomIndex = Random.Range(0, randomMapPool.Count);
        
        // 2. Lấy tên của map tại vị trí ngẫu nhiên đó
        string nextSceneName = randomMapPool[randomIndex];
        
        // 3. Lưu tên map đã chọn và chuyển đến màn hình Loading
        Debug.Log($"Đã chọn ngẫu nhiên map: {nextSceneName}. Đang chuyển cảnh...");
        PlayerPrefs.SetString("NextSceneToLoad", nextSceneName);
        SceneManager.LoadScene("Loading");
    }
}
