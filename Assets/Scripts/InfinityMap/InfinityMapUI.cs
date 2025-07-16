using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InfinityMap
{
    public class InfinityMapUI : MonoBehaviour
    {
        [Header("Player Info UI")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI expText;
        
        [Header("Game Info UI")]
        [SerializeField] private TextMeshProUGUI gameTimeText;
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private TextMeshProUGUI enemiesKilledText;
        
        [Header("Level Up Effect")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private float levelUpDisplayTime = 2f;
        
        void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void InitializeUI()
        {
            // Hide level up panel initially
            if (levelUpPanel != null)
                levelUpPanel.SetActive(false);
            
            // Initialize UI with current values
            UpdateHealthUI();
            UpdateExpUI();
            UpdateLevelUI();
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to player events - PlayerHealth từ folder Player không có events
            // PlayerHealth.OnHealthChanged += OnHealthChanged; // Comment vì event không tồn tại
            PlayerLevel.OnExpChanged += OnExpChanged;
            PlayerLevel.OnLevelUp += OnLevelUp;
        }
        
        void Update()
        {
            UpdateGameInfoUI();
            UpdateHealthUI(); // Cập nhật health UI trong Update để realtime
        }
        
        private void OnExpChanged(int currentExp, int expToNext)
        {
            UpdateExpUI(currentExp, expToNext);
        }
        
        private void OnLevelUp(int newLevel)
        {
            UpdateLevelUI(newLevel);
            ShowLevelUpEffect(newLevel);
        }
        
        private void UpdateHealthUI(int currentHealth = -1, int maxHealth = -1)
        {
            // Get values from PlayerHealth if not provided
            if (currentHealth == -1 || maxHealth == -1)
            {
                // Tìm PlayerHealth từ Player GameObject
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    // Sử dụng global PlayerHealth class (từ folder Player)
                    global::PlayerHealth playerHealth = playerObj.GetComponent<global::PlayerHealth>();
                    if (playerHealth != null)
                    {
                        currentHealth = playerHealth.CurrentHealth;
                        maxHealth = playerHealth.MaxHealth;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            
            // Update health bar
            if (healthBar != null && maxHealth > 0)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }
            
            // Update health text
            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }
        
        private void UpdateExpUI(int currentExp = -1, int expToNext = -1)
        {
            // Get values from PlayerLevel if not provided
            if (currentExp == -1 || expToNext == -1)
            {
                if (PlayerLevel.Instance != null)
                {
                    currentExp = PlayerLevel.Instance.GetCurrentExp();
                    expToNext = PlayerLevel.Instance.GetExpToNextLevel();
                }
                else
                {
                    return;
                }
            }
            
            // Update exp bar
            if (expBar != null && expToNext > 0)
            {
                expBar.maxValue = expToNext;
                expBar.value = currentExp;
            }
            
            // Update exp text
            if (expText != null)
            {
                expText.text = $"{currentExp}/{expToNext}";
            }
        }
        
        private void UpdateLevelUI(int level = -1)
        {
            if (level == -1)
            {
                if (PlayerLevel.Instance != null)
                {
                    level = PlayerLevel.Instance.GetCurrentLevel();
                }
                else
                {
                    return;
                }
            }
            
            if (levelText != null)
            {
                levelText.text = $"Level {level}";
            }
        }
        
        private void UpdateGameInfoUI()
        {
            if (InfinityMapManager.Instance == null) return;
            
            // Update game time
            if (gameTimeText != null)
            {
                gameTimeText.text = $"Time: {InfinityMapManager.Instance.GetFormattedGameTime()}";
            }
            
            // Update enemy count
            if (enemyCountText != null && InfinityMapManager.Instance.GetEnemySpawner() != null)
            {
                int current = InfinityMapManager.Instance.GetEnemySpawner().GetCurrentEnemyCount();
                int max = InfinityMapManager.Instance.GetEnemySpawner().GetMaxEnemyCount();
                enemyCountText.text = $"Enemies: {current}/{max}";
            }
            
            // Update enemies killed
            if (enemiesKilledText != null)
            {
                enemiesKilledText.text = $"Killed: {InfinityMapManager.Instance.GetEnemiesKilled()}";
            }
        }
        
        private void ShowLevelUpEffect(int newLevel)
        {
            if (levelUpPanel == null) return;
            
            // Update level up text
            if (levelUpText != null)
            {
                levelUpText.text = $"LEVEL {newLevel}!";
            }
            
            // Show panel
            levelUpPanel.SetActive(true);
            
            // Hide after delay
            Invoke(nameof(HideLevelUpEffect), levelUpDisplayTime);
        }
        
        private void HideLevelUpEffect()
        {
            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(false);
            }
        }
        
        // Public methods for buttons
        public void OnPauseButtonClicked()
        {
            if (InfinityMapManager.Instance != null)
            {
                InfinityMapManager.Instance.TogglePause();
            }
        }
        
        public void OnResumeButtonClicked()
        {
            if (InfinityMapManager.Instance != null)
            {
                InfinityMapManager.Instance.TogglePause();
            }
        }
        
        public void OnRestartButtonClicked()
        {
            if (InfinityMapManager.Instance != null)
            {
                InfinityMapManager.Instance.RestartGame();
            }
        }
        
        public void OnQuitButtonClicked()
        {
            if (InfinityMapManager.Instance != null)
            {
                InfinityMapManager.Instance.QuitToMenu();
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            // PlayerHealth.OnHealthChanged -= OnHealthChanged; // Comment vì event không tồn tại
            PlayerLevel.OnExpChanged -= OnExpChanged;
            PlayerLevel.OnLevelUp -= OnLevelUp;
        }
    }
}
