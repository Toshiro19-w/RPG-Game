using UnityEngine;
using System.Collections.Generic; // Cần thiết để dùng List

// Đặt class này ở trên cùng của file, hoặc bên trong UpgradePanelManager
// Nó định nghĩa cấu trúc dữ liệu cho mỗi kỹ năng trong UI.
[System.Serializable] // Dòng này giúp nó hiện ra trong Inspector
public class SkillUIData
{
    public KeyCode skillKey;
    public string skillName;
    public Sprite skillIcon;
}


public class UpgradePanelManager : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("Khung chứa 3 nút đầu tiên (hàng trên)")]
    [SerializeField] private Transform topRowContainer; 
    [Tooltip("Khung chứa các nút còn lại (hàng dưới)")]
    [SerializeField] private Transform bottomRowContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject upgradeButtonPrefab;

    [Header("Skill UI Database")]
    [Tooltip("Điền thông tin UI cho mỗi kỹ năng ở đây. Kéo thả Sprite và đặt tên.")]
    [SerializeField] private List<SkillUIData> skillUIDatabase = new List<SkillUIData>();

    private PlayerCombat playerCombat;

    // OnEnable được gọi mỗi khi GameObject này được kích hoạt (bật lên)
    void OnEnable() 
    {
        // Nếu chưa có tham chiếu tới PlayerCombat, tìm nó trong Scene
        playerCombat ??= FindObjectOfType<PlayerCombat>();
        
        // Luôn làm mới bảng nâng cấp mỗi khi nó được mở
        RefreshPanel();
    }

    // Hàm chính để cập nhật toàn bộ bảng nâng cấp
    public void RefreshPanel()
    {
        if (playerCombat == null) return; // An toàn nếu không tìm thấy Player

        // 1. Xóa tất cả các nút cũ ở cả hai hàng để tránh trùng lặp
        foreach (Transform child in topRowContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in bottomRowContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Lấy danh sách các kỹ năng CHƯA được nâng cấp từ PlayerCombat
        List<KeyCode> upgradableSkills = playerCombat.GetUpgradableSkills();

        // 3. Tạo nút mới cho từng kỹ năng trong danh sách
        for (int i = 0; i < upgradableSkills.Count; i++)
        {
            KeyCode currentKey = upgradableSkills[i];
            
            // 4. Tìm thông tin UI (tên, icon) tương ứng với KeyCode hiện tại
            SkillUIData uiData = GetSkillUIData(currentKey);
            if (uiData == null)
            {
                Debug.LogWarning($"Không tìm thấy thông tin UI cho kỹ năng: {currentKey}. Hãy kiểm tra Skill UI Database.");
                continue; // Bỏ qua và không tạo nút cho kỹ năng này
            }

            // 5. Chọn đúng hàng để đặt nút vào (hàng trên hoặc hàng dưới)
            Transform parentContainer = (i < 3) ? topRowContainer : bottomRowContainer;

            // 6. Tạo một bản sao của prefab nút và đặt nó vào đúng hàng
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, parentContainer);
            UpgradeButtonUI buttonUI = buttonGO.GetComponent<UpgradeButtonUI>();

            // 7. Lấy giá tiền và gọi hàm Setup để điền tất cả thông tin vào nút
            int cost = playerCombat.GetUpgradeCost(currentKey);
            buttonUI.Setup(currentKey, uiData.skillIcon, uiData.skillName, cost, this);
        }
    }

    // Hàm này được gọi bởi nút bấm khi người chơi muốn nâng cấp
    public void AttemptUpgrade(KeyCode key)
    {
        if (playerCombat.AttemptToUpgradeSkill(key))
        {
            // Nếu nâng cấp thành công (đủ tiền), làm mới lại bảng
            // Nút vừa bấm sẽ biến mất vì kỹ năng đó không còn trong danh sách upgradable
            RefreshPanel();
        }
        else
        {
            // Nâng cấp thất bại (không đủ tiền), có thể thêm hiệu ứng âm thanh hoặc rung lắc nút
            Debug.Log("Không đủ tiền!");
        }
    }

    // Hàm trợ giúp để tìm thông tin UI từ "cơ sở dữ liệu"
    private SkillUIData GetSkillUIData(KeyCode key)
    {
        foreach (var data in skillUIDatabase)
        {
            if (data.skillKey == key)
            {
                return data;
            }
        }
        return null; // Trả về null nếu không tìm thấy
    }
}