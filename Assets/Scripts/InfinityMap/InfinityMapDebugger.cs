using UnityEngine;

namespace InfinityMap
{
    /// <summary>
    /// Utility script để debug và fix các vấn đề Unity Inspector
    /// </summary>
    public class InfinityMapDebugger : MonoBehaviour
    {
        [Header("Debug Tools")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool autoFixNullReferences = true;
        
        void Start()
        {
            if (enableDebugLogs)
            {
                DebugSystemComponents();
            }
            
            if (autoFixNullReferences)
            {
                FixNullReferences();
            }
        }
        
        private void DebugSystemComponents()
        {
            Debug.Log("=== Infinity Map System Debug ===");
            
            // Check PlayerLevel
            var playerLevel = FindFirstObjectByType<PlayerLevel>();
            Debug.Log($"PlayerLevel found: {playerLevel != null}");
            
            // Check Player
            var player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"Player GameObject found: {player != null}");
            
            if (player != null)
            {
                var playerHealth = player.GetComponent<PlayerHealth>();
                Debug.Log($"PlayerHealth component found: {playerHealth != null}");
                
                if (playerHealth != null)
                {
                    Debug.Log($"Player Health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
                }
            }
            
            // Check Enemy Spawner
            var spawner = FindFirstObjectByType<InfinityEnemySpawner>();
            Debug.Log($"InfinityEnemySpawner found: {spawner != null}");
            
            // Check Manager
            var manager = FindFirstObjectByType<InfinityMapManager>();
            Debug.Log($"InfinityMapManager found: {manager != null}");
            
            Debug.Log("=== Debug Complete ===");
        }
        
        private void FixNullReferences()
        {
            // Find and fix common null reference issues
            
            // Fix PlayerHealth HPBar references
            var players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (var playerHealth in players)
            {
                if (playerHealth.hpBar == null)
                {
                    var hpBar = FindFirstObjectByType<HPBar>();
                    if (hpBar != null)
                    {
                        // We can't assign directly due to private field, but we log it
                        Debug.LogWarning($"PlayerHealth on {playerHealth.name} has null HPBar reference. Please assign manually.");
                    }
                }
            }
            
            // Fix Enemy spawner references
            var spawners = FindObjectsByType<InfinityEnemySpawner>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                Debug.Log($"Spawner found with {spawner.GetCurrentEnemyCount()} enemies");
            }
        }
        
        [ContextMenu("Force Debug Check")]
        public void ForceDebugCheck()
        {
            DebugSystemComponents();
            FixNullReferences();
        }
        
        [ContextMenu("Clear All Spawned Enemies")]
        public void ClearAllEnemies()
        {
            var spawner = FindFirstObjectByType<InfinityEnemySpawner>();
            if (spawner != null)
            {
                spawner.ClearAllEnemies();
                Debug.Log("All spawned enemies cleared!");
            }
        }
        
        [ContextMenu("Add Test EXP")]
        public void AddTestExp()
        {
            if (PlayerLevel.Instance != null)
            {
                PlayerLevel.Instance.AddExp(50);
                Debug.Log("Added 50 EXP for testing!");
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw debug info
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, player.transform.position);
            }
        }
    }
}
