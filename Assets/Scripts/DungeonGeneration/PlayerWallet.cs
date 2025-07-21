// PlayerWallet.cs
using UnityEngine;
using TMPro; // Thêm dòng này để dùng TextMeshPro
using System; // Thêm dòng này để dùng Action

public class PlayerWallet : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText; // Kéo UI Text hiển thị coin vào đây

    public int CurrentCoins { get; private set; }

    // Event này sẽ thông báo cho các script khác khi số coin thay đổi
    public event Action<int> OnCoinsChanged;

    void Start()
    {
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        CurrentCoins += amount;
        Debug.Log($"Added {amount} coins. Total: {CurrentCoins}");
        UpdateCoinUI();
        OnCoinsChanged?.Invoke(CurrentCoins); // Phát sự kiện
    }

    public bool CanAfford(int amount)
    {
        return CurrentCoins >= amount;
    }

    public bool SpendCoins(int amount)
    {
        if (CanAfford(amount))
        {
            CurrentCoins -= amount;
            Debug.Log($"Spent {amount} coins. Remaining: {CurrentCoins}");
            UpdateCoinUI();
            OnCoinsChanged?.Invoke(CurrentCoins); // Phát sự kiện
            return true;
        }
        Debug.Log("Not enough coins!");
        return false;
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = CurrentCoins.ToString();
        }
    }
}