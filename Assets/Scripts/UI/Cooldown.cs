using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm dòng này nếu bạn dùng TextMeshPro

public class SkillCooldownUI : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private KeyCode skillKey; // KeyCode của kỹ năng mà UI này đại diện

    [Header("UI Components")]
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText; // Dùng TextMeshProUGUI
    // Hoặc dùng: [SerializeField] private Text cooldownText; nếu bạn dùng Text thường

    private PlayerCombat playerCombat;
    private float currentCooldown;
    private bool isCoolingDown = false;

    void Start()
    {
        // Tìm PlayerCombat script trong scene
        playerCombat = FindAnyObjectByType<PlayerCombat>();
        if (playerCombat == null)
        {
            Debug.LogError("PlayerCombat script not found in the scene!");
            return;
        }

        // Đăng ký lắng nghe sự kiện từ PlayerCombat
        playerCombat.OnSkillUsed += HandleSkillUsed;
        
        // Ẩn UI cooldown lúc bắt đầu
        cooldownOverlay.fillAmount = 0;
        cooldownText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh lỗi
        if (playerCombat != null)
        {
            playerCombat.OnSkillUsed -= HandleSkillUsed;
        }
    }
    
    // Phương thức này sẽ được gọi khi một kỹ năng được sử dụng trong PlayerCombat
    private void HandleSkillUsed(KeyCode key, float cooldownDuration)
    {
        // Chỉ kích hoạt nếu key được nhấn khớp với key của UI này
        if (key == skillKey)
        {
            currentCooldown = cooldownDuration;
            isCoolingDown = true;
            cooldownText.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1; // Bắt đầu đầy
        }
    }

    void Update()
    {
        if (isCoolingDown)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown <= 0)
            {
                // Cooldown kết thúc
                isCoolingDown = false;
                cooldownOverlay.fillAmount = 0;
                cooldownText.gameObject.SetActive(false);
            }
            else
            {
                // Cập nhật UI
                cooldownText.text = Mathf.Ceil(currentCooldown).ToString();
                // Lấy tổng cooldown từ PlayerCombat để tính tỷ lệ fillAmount chính xác
                float totalCooldown = playerCombat.GetCooldownForSkill(skillKey);
                cooldownOverlay.fillAmount = currentCooldown / totalCooldown;
            }
        }
    }
}