using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InfinityMap
{
    public class ExpBarUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image expFillImage;
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private TextMeshProUGUI levelText;
        
        void Start()
        {
            // Subscribe to level events
            PlayerLevel.OnExpChanged += UpdateExpBar;
            PlayerLevel.OnLevelUp += UpdateLevel;
            
            // Initialize with current values
            if (PlayerLevel.Instance != null)
            {
                UpdateExpBar(PlayerLevel.Instance.GetCurrentExp(), PlayerLevel.Instance.GetExpToNextLevel());
                UpdateLevel(PlayerLevel.Instance.GetCurrentLevel());
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            PlayerLevel.OnExpChanged -= UpdateExpBar;
            PlayerLevel.OnLevelUp -= UpdateLevel;
        }
        
        private void UpdateExpBar(int currentExp, int expToNext)
        {
            if (expFillImage != null)
            {
                float fillAmount = (float)currentExp / expToNext;
                expFillImage.fillAmount = fillAmount;
            }
            
            if (expText != null)
            {
                expText.text = $"{currentExp}/{expToNext}";
            }
        }
        
        private void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }
        }
    }
}