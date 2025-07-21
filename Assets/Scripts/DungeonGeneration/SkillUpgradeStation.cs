using UnityEngine;
public class SkillUpgradeStation : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    private bool playerIsNear = false;

    void Start() => upgradePanel?.SetActive(false);

    void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.B))
        {
            bool isActive = !upgradePanel.activeSelf;
            upgradePanel.SetActive(isActive);
            Time.timeScale = isActive ? 0f : 1f; // Dừng/Chạy game
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIsNear = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}