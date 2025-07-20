using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

namespace InfinityMap
{
    public class PlayerScore : MonoBehaviour
    {
        public static PlayerScore Instance;
        
        [Header("Score Settings")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private float waveMultiplier = 0.1f; // 10% increase per wave
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        
        // Score data
        private int highScore;
        
        // Events
        public Action<int> OnScoreChanged;
        public Action<int> OnNewHighScore;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadHighScore();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            UpdateUI();
        }
        
        public void AddScore(int baseScore, int waveNumber = 1)
        {
            // Apply wave multiplier
            float waveBonus = 1f + (waveNumber - 1) * waveMultiplier;
            int finalScore = Mathf.RoundToInt(baseScore * waveBonus);
            
            // Add to total score
            currentScore += finalScore;
            
            // Check for high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
                OnNewHighScore?.Invoke(highScore);
            }
            
            // Trigger events
            OnScoreChanged?.Invoke(currentScore);
            
            UpdateUI();
            
            Debug.Log($"Score: +{finalScore} (Base: {baseScore}, Wave: {waveNumber}) Total: {currentScore}");
        }
        
        public void AddScoreFromEnemy(InfinityEnemy enemy)
        {
            if (enemy == null) return;
            
            int baseScore = enemy.GetScoreReward();
            // Không dùng wave multiplier, chỉ thêm điểm cơ bản
            AddScore(baseScore, 1);
        }
        
        public void ResetScore()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score \n{currentScore:N0}";
            
            if (highScoreText != null)
                highScoreText.text = $"High Score {highScore:N0}";
        }
        
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("InfinityHighScore", 0);
        }
        
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("InfinityHighScore", highScore);
            PlayerPrefs.Save();
        }
        
        // Getters
        public int GetCurrentScore() => currentScore;
        public int GetHighScore() => highScore;
        
        // Setters
        public void SetWaveMultiplier(float multiplier) => waveMultiplier = multiplier;
        public void SetScore(int score) 
        { 
            currentScore = score;
            OnScoreChanged?.Invoke(currentScore);
            UpdateUI();
        }
    }
}