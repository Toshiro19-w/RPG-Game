using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityMap
{
    public class InfinityMapManager : MonoBehaviour
    {
        public static InfinityMapManager Instance;
        
        [Header("Game State")]
        [SerializeField] private bool gameIsActive = true;
        
        [Header("References")]
        [SerializeField] private InfinityEnemySpawner enemySpawner;
        [SerializeField] private RewardSystem rewardSystem;
        // PlayerHealth sẽ được tìm từ Player GameObject
        
        [Header("UI References")]
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private GameObject pauseUI;
        
        [Header("Game Settings")]
        [SerializeField] private bool pauseOnPlayerDeath = true;
        [SerializeField] private float gameOverDelay = 2f;
        // Game state
        private bool isPaused = false;
        private float gameTime = 0f;
        private int enemiesKilled = 0;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            SetupReferences();
        }
        
        void Start()
        {
            InitializeGame();
            SubscribeToEvents();
        }

        private void SetupReferences()
        {
            // Auto-find references if not assigned
            if (enemySpawner == null)
                enemySpawner = FindFirstObjectByType<InfinityEnemySpawner>();

            if (rewardSystem == null)
                rewardSystem = FindFirstObjectByType<RewardSystem>();

            // Tạo RewardSystem nếu không tìm thấy
            if (rewardSystem == null)
            {
                GameObject rewardSystemObj = new GameObject("RewardSystem");
                rewardSystem = rewardSystemObj.AddComponent<RewardSystem>();
                Debug.Log("Created RewardSystem automatically");
            }
        }
        
        private void InitializeGame()
        {
            gameIsActive = true;
            isPaused = false;
            gameTime = 0f;
            enemiesKilled = 0;
            
            Time.timeScale = 1f;
            
            // Hide UI panels
            if (gameOverUI != null) gameOverUI.SetActive(false);
            if (pauseUI != null) pauseUI.SetActive(false);
            
            Debug.Log("Infinity Map initialized!");
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to player death - sử dụng PlayerHealth từ folder Player
            // PlayerHealth.OnPlayerDeath += OnPlayerDeath; // Cần kiểm tra event này có tồn tại không
        }
        
        void Update()
        {
            if (!gameIsActive) return;
            
            // Update game time
            if (!isPaused)
            {
                gameTime += Time.deltaTime;
            }
            
            // Handle pause input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        
        private void OnPlayerDeath()
        {
            Debug.Log("Player died! Game Over.");
            
            if (pauseOnPlayerDeath)
            {
                Invoke(nameof(ShowGameOver), gameOverDelay);
            }
        }
        

        
        private void ShowGameOver()
        {
            gameIsActive = false;
            
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
            
            // Stop enemy spawning
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
            }

            // Cleanup all rewards
            if (rewardSystem != null)
            {
                rewardSystem.CleanupAllRewards();
            }
            
            Time.timeScale = 0f;
        }
        
        public void TogglePause()
        {
            if (!gameIsActive) return;
            
            isPaused = !isPaused;
            
            if (isPaused)
            {
                Time.timeScale = 0f;
                if (pauseUI != null) pauseUI.SetActive(true);
                if (enemySpawner != null) enemySpawner.StopSpawning();
            }
            else
            {
                Time.timeScale = 1f;
                if (pauseUI != null) pauseUI.SetActive(false);
                if (enemySpawner != null) enemySpawner.ResumeSpawning();
            }
        }
        
        public void RestartGame()
        {
            if (rewardSystem != null)
            {
                rewardSystem.CleanupAllRewards();
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void QuitToMenu()
        {
            if (rewardSystem != null)
            {
                rewardSystem.CleanupAllRewards();
            }
            Time.timeScale = 1f;
            // Load main menu scene - adjust scene name as needed
            SceneManager.LoadScene("MainMenu");
        }
        
        public void OnEnemyKilled()
        {
            enemiesKilled++;
            Debug.Log($"Enemy killed! Total: {enemiesKilled}");
        }
        
        // Getters for UI and other systems
        public bool IsGameActive() => gameIsActive;
        public bool IsPaused() => isPaused;
        public float GetGameTime() => gameTime;
        public int GetEnemiesKilled() => enemiesKilled;
        
        public string GetFormattedGameTime()
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        public InfinityEnemySpawner GetEnemySpawner() => enemySpawner;
        
        public global::PlayerHealth GetPlayerHealth()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            return playerObj?.GetComponent<global::PlayerHealth>();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            // PlayerHealth.OnPlayerDeath -= OnPlayerDeath; // Comment vì event có thể không tồn tại

            // Cleanup all rewards before destroying
            if (rewardSystem != null)
            {
                rewardSystem.CleanupAllRewards();
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && gameIsActive && !isPaused)
            {
                TogglePause();
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && gameIsActive && !isPaused)
            {
                TogglePause();
            }
        }
    }
}
