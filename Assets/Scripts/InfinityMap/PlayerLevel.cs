using UnityEngine;
using System;

namespace InfinityMap
{
    public class PlayerLevel : MonoBehaviour
    {
        public static PlayerLevel Instance;
        
        [Header("Level Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExp = 0;
        [SerializeField] private int expToNextLevel = 100;
        [SerializeField] private float expMultiplier = 1.5f; // Multiplier cho exp cần thiết mỗi level
        
        [Header("Level Effects")]
        [SerializeField] private float enemySpawnRateMultiplier = 0.9f; // Giảm thời gian spawn 10% mỗi level
        [SerializeField] private float minSpawnRate = 0.3f; // Thời gian spawn tối thiểu
        
        // Events
        public static event Action<int> OnLevelUp;
        public static event Action<int, int> OnExpChanged; // current exp, exp to next level
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Broadcast initial values
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }
        
        public void AddExp(int exp)
        {
            currentExp += exp;
            
            // Check for level up
            while (currentExp >= expToNextLevel)
            {
                LevelUp();
            }
            
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }
        
        private void LevelUp()
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            
            // Calculate new exp requirement
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * expMultiplier);
            
            Debug.Log($"Level Up! New Level: {currentLevel}");
            OnLevelUp?.Invoke(currentLevel);
        }
        
        public int GetCurrentLevel() => currentLevel;
        public int GetCurrentExp() => currentExp;
        public int GetExpToNextLevel() => expToNextLevel;
        
        // Tính toán spawn rate dựa trên level
        public float GetEnemySpawnRate(float baseSpawnRate)
        {
            float adjustedRate = baseSpawnRate * Mathf.Pow(enemySpawnRateMultiplier, currentLevel - 1);
            return Mathf.Max(adjustedRate, minSpawnRate);
        }
        
        // Tính toán số lượng enemy tối đa dựa trên level
        public int GetMaxEnemyCount(int baseMaxCount)
        {
            return baseMaxCount + (currentLevel - 1) * 2; // Tăng 2 enemy mỗi level
        }
        
        // Set level manually (for testing)
        public void SetLevel(int level)
        {
            currentLevel = Mathf.Max(1, level);
            currentExp = 0;
            expToNextLevel = Mathf.RoundToInt(100 * Mathf.Pow(expMultiplier, currentLevel - 1));
            
            OnLevelUp?.Invoke(currentLevel);
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }
    }
}
