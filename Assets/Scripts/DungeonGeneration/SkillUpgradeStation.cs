// SkillUpgradeStation.cs (Phiên bản đã sửa)

using UnityEngine;

public class SkillUpgradeStation : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    private bool playerIsNear = false;
    private bool isAvailable = true;

    public void SetAvailable(bool available)
    {
        isAvailable = available;
        if (!available && upgradePanel != null && upgradePanel.activeSelf)
        {
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void Start()
    {
        if (upgradePanel == null)
        {
            UpgradePanelManager panelManager = FindObjectOfType<UpgradePanelManager>(true);

            if (panelManager != null)
            {
                upgradePanel = panelManager.gameObject;
                Debug.Log("SkillUpgradeStation đã tự động tìm thấy UpgradeUIPanel!");
            }
            else
            {
                Debug.LogError("KHÔNG TÌM THẤY UpgradeUIPanel trong Scene! Hãy chắc chắn nó tồn tại và có script UpgradePanelManager.");
            }
        }
        upgradePanel?.SetActive(false); 
    }

    void Update()
    {
        if (playerIsNear && isAvailable && Input.GetKeyDown(KeyCode.B) && upgradePanel != null)
        {
            bool isActive = !upgradePanel.activeSelf;
            upgradePanel.SetActive(isActive);
            Time.timeScale = isActive ? 0f : 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIsNear = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && upgradePanel != null)
        {
            playerIsNear = false;
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}