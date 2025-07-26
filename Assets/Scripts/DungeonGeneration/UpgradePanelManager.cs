using UnityEngine;
using UnityEngine.UI; // Phải có dòng này để dùng Button và Image
using System.Collections.Generic; // Phải có dòng này để dùng List

// Bạn nên định nghĩa class nhỏ này ở trên cùng của file script.
// Nó sẽ giúp tạo ra một danh sách tùy chỉnh đẹp mắt trong Inspector.
[System.Serializable]
public class SkillUIData
{
    public KeyCode skillKey;
    public string skillName;
    public Sprite skillIcon;
}


public class UpgradePanelManager : MonoBehaviour
{
    // === CÁC THAM CHIẾU TỪ EDITOR ===

    [Header("UI Containers")]
    [Tooltip("Khung chứa 3 nút đầu tiên (hàng trên)")]
    [SerializeField] private Transform topRowContainer; 
    [Tooltip("Khung chứa các nút còn lại (hàng dưới)")]
    [SerializeField] private Transform bottomRowContainer;
    
    [Header("Special Buttons")]
    [Tooltip("Kéo nút bấm 'Hồi Máu' từ Hierarchy vào đây")]
    [SerializeField] private Button healButton; 

    [Header("Prefabs")]
    [Tooltip("Kéo prefab của nút nâng cấp từ cửa sổ Project vào đây")]
    [SerializeField] private GameObject upgradeButtonPrefab;

    [Header("Skill UI Database")]
    [Tooltip("Điền thông tin UI cho mỗi kỹ năng ở đây. Kéo thả Sprite và đặt tên cho từng cái.")]
    [SerializeField] private List<SkillUIData> skillUIDatabase = new List<SkillUIData>();
    
    // === CÁC HẰNG SỐ ===
    private const int HEAL_COST = 5;
    private const int HEAL_AMOUNT = 20; // Số máu được hồi, bạn có thể thay đổi

    // === CÁC THAM CHIẾU NỘI BỘ (TỰ ĐỘNG TÌM) ===
    private PlayerCombat playerCombat;
    private PlayerWallet playerWallet;
    private PlayerHealth playerHealth;

    // OnEnable được gọi mỗi khi Panel này được kích hoạt (hiện ra)
    void OnEnable() 
    {
        // Tìm các component của Player một lần để tối ưu hóa
        playerCombat ??= FindObjectOfType<PlayerCombat>();
        playerWallet ??= FindObjectOfType<PlayerWallet>();
        playerHealth ??= FindObjectOfType<PlayerHealth>();
        
        // Luôn làm mới lại toàn bộ bảng nâng cấp mỗi khi nó được mở
        RefreshPanel();
    }

    // Hàm chính để cập nhật toàn bộ bảng nâng cấp
    public void RefreshPanel()
    {
        // An toàn nếu không tìm thấy Player
        if (playerCombat == null) 
        {
            Debug.LogError("UpgradePanelManager không tìm thấy PlayerCombat!");
            return;
        }

        // 1. Xóa tất cả các nút nâng cấp kỹ năng cũ để tránh trùng lặp
        foreach (Transform child in topRowContainer) Destroy(child.gameObject);
        foreach (Transform child in bottomRowContainer) Destroy(child.gameObject);

        // 2. Lấy danh sách các kỹ năng CHƯA được nâng cấp từ Player
        List<KeyCode> upgradableSkills = playerCombat.GetUpgradableSkills();

        // 3. Tạo nút mới cho từng kỹ năng trong danh sách
        for (int i = 0; i < upgradableSkills.Count; i++)
        {
            KeyCode currentKey = upgradableSkills[i];
            
            // Tìm thông tin UI (tên, icon) tương ứng
            SkillUIData uiData = GetSkillUIData(currentKey);
            if (uiData == null)
            {
                Debug.LogWarning($"Không tìm thấy thông tin UI cho kỹ năng: {currentKey}. Hãy kiểm tra Skill UI Database.");
                continue; // Bỏ qua, không tạo nút cho kỹ năng này
            }

            // Chọn đúng hàng để đặt nút vào (hàng trên hoặc hàng dưới)
            Transform parentContainer = (i < 3) ? topRowContainer : bottomRowContainer;

            // Tạo một bản sao của prefab nút và đặt nó vào đúng hàng
            GameObject buttonGO = Instantiate(upgradeButtonPrefab, parentContainer);
            UpgradeButtonUI buttonUI = buttonGO.GetComponent<UpgradeButtonUI>();

            // Lấy giá tiền và gọi hàm Setup để điền tất cả thông tin vào nút
            int cost = playerCombat.GetUpgradeCost(currentKey);
            buttonUI.Setup(currentKey, uiData.skillIcon, uiData.skillName, cost, this);
        }
        
        // 4. Cập nhật trạng thái của nút hồi máu (có thể nhấn hay không)
        UpdateHealButtonState();
    }
    
    // Hàm này được gọi bởi các nút kỹ năng khi được nhấn
    public void AttemptUpgrade(KeyCode key)
    {
        if (playerCombat.AttemptToUpgradeSkill(key))
        {
            // Nếu nâng cấp thành công, làm mới lại bảng
            // (nút vừa bấm sẽ biến mất vì kỹ năng đó không còn trong danh sách có thể nâng cấp)
            RefreshPanel();
        }
        else
        {
            Debug.Log("Không đủ tiền để nâng cấp kỹ năng!");
            // Có thể thêm hiệu ứng âm thanh hoặc rung lắc nút ở đây
        }
    }
    
    // Hàm này được gọi bởi nút hồi máu khi được nhấn
    public void AttemptToHeal()
    {
        AudioManager.Instance.Play("click");
        if (playerHealth == null || playerWallet == null) return;

        if (playerHealth.IsHealthFull())
        {
            Debug.Log("Máu đã đầy!");
            return;
        }

        if (playerWallet.SpendCoins(HEAL_COST))
        {
            playerHealth.Heal(HEAL_AMOUNT);
            Debug.Log($"Hồi {HEAL_AMOUNT} máu thành công!");
        }
        else
        {
            Debug.Log("Không đủ tiền để hồi máu!");
        }

        // Sau khi mua máu (thành công hay thất bại), cập nhật lại trạng thái nút
        UpdateHealButtonState();
    }
    
    // Hàm kiểm tra và cập nhật trạng thái của nút hồi máu
    private void UpdateHealButtonState()
    {
        if (healButton == null || playerWallet == null || playerHealth == null) return;

        // Nút chỉ có thể tương tác khi MÁU CHƯA ĐẦY và CÓ ĐỦ TIỀN
        bool canHeal = !playerHealth.IsHealthFull() && playerWallet.CanAfford(HEAL_COST);
        healButton.interactable = canHeal;
    }

    // Hàm trợ giúp để tìm thông tin UI từ "cơ sở dữ liệu" trong Inspector
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