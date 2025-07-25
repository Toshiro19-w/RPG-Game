using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

namespace InfinityMap
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyType enemyType;
        public GameObject enemyPrefab;
        public int baseHealth = 100;
        public int baseDamage = 20;
        public int baseExpReward = 10;
        [Range(0f, 1f)]
        public float spawnWeight = 1f; // Tỷ lệ spawn
    }
    
    public class InfinityEnemySpawner : MonoBehaviour
    {
        [Header("Wave Spawn Settings")]
        [SerializeField] private int initialWaveSize = 20; // Số lượng enemy trong đợt đầu tiên
        [SerializeField] private float waveMultiplier = 1.5f; // Hệ số nhân số lượng enemy mỗi đợt
        [SerializeField] private float timeBetweenWaves = 12f; // Thời gian nghỉ giữa các đợt (giây)
        [SerializeField] private float enemySpawnInterval = 1f; // Thời gian giữa mỗi lần spawn enemy trong đợt
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 15f; // Bán kính spawn xung quanh player
        [SerializeField] private float minDistanceFromPlayer = 8f; // Khoảng cách tối thiểu từ player
        [SerializeField] private float maxDistanceFromPlayer = 20f; // Khoảng cách tối đa từ player
        
        [Header("Tilemap Reference")]
        [SerializeField] private Tilemap floorTilemap; // Floor Tilemap để spawn enemy
        
        [Header("Enemy Data")]
        [SerializeField] private List<EnemySpawnData> enemySpawnData = new List<EnemySpawnData>();
        
        [Header("Level Scaling")]
        [SerializeField] private float healthScaling = 1.2f; // Health tăng 20% mỗi level
        [SerializeField] private float damageScaling = 1.1f; // Damage tăng 10% mỗi level
        [SerializeField] private float expScaling = 1.1f; // Exp tăng 10% mỗi level
        
        // Runtime variables
        private Transform player;
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private Coroutine spawnCoroutine;
        private int currentWaveNumber = 1;
        private int currentWaveSize;
        private int enemiesRemainingInWave;
        private bool isWaveActive = false;
        private bool isWaitingForNextWave = false;
        
        // Tilemap bounds
        private BoundsInt tilemapBounds;
        private List<Vector3Int> validSpawnPositions = new List<Vector3Int>();
        
        void Start()
        {
            InitializeSpawner();
            
            // Tìm WaveCountdownUI nếu có
            FindWaveCountdownUI();
            
            // Nếu có WaveCountdownUI, để nó bắt đầu đếm ngược
            // WaveCountdownUI sẽ gọi ResumeSpawning khi đếm ngược kết thúc
            if (waveCountdownUI != null)
            {
                // Gọi StartInitialCountdown để bắt đầu đếm ngược ban đầu
                waveCountdownUI.StartInitialCountdown();
                // Không cần gọi StartSpawning() vì WaveCountdownUI sẽ gọi
            }
            else
            {
                // Nếu không có WaveCountdownUI, bắt đầu spawn ngay
                StartSpawning();
            }
        }
        
        private void InitializeSpawner()
        {
            FindPlayer();
            CalculateTilemapBounds();
            UpdateSpawnSettings();
        }
        
        private void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Không tìm thấy Player với tag 'Player'!");
            }
        }
        
        private void CalculateTilemapBounds()
        {
            if (floorTilemap == null)
            {
                Debug.LogError("Floor Tilemap chưa được gán!");
                return;
            }
            
            tilemapBounds = floorTilemap.cellBounds;
            CacheValidSpawnPositions();
        }
        
        private void CacheValidSpawnPositions()
        {
            validSpawnPositions.Clear();
            
            for (int x = tilemapBounds.xMin; x < tilemapBounds.xMax; x++)
            {
                for (int y = tilemapBounds.yMin; y < tilemapBounds.yMax; y++)
                {
                    Vector3Int cellPosition = new Vector3Int(x, y, 0);
                    
                    // Check if there's a tile at this position
                    if (floorTilemap.HasTile(cellPosition))
                    {
                        validSpawnPositions.Add(cellPosition);
                    }
                }
            }
            
            Debug.Log($"Cached {validSpawnPositions.Count} valid spawn positions");
        }
        
        private void UpdateSpawnSettings()
        {
            // Tính toán kích thước của đợt hiện tại dựa trên số đợt và hệ số nhân
            currentWaveSize = Mathf.RoundToInt(initialWaveSize * Mathf.Pow(waveMultiplier, currentWaveNumber - 1));
            Debug.Log($"Wave {currentWaveNumber}: Size = {currentWaveSize} enemies");
        }
        

        
        private void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            
            // Khởi tạo đợt đầu tiên
            currentWaveNumber = 1;
            UpdateSpawnSettings();
            spawnCoroutine = StartCoroutine(WaveSpawnRoutine());
        }
        
        private WaveCountdownUI waveCountdownUI;
        
        private void FindWaveCountdownUI()
        {
            if (waveCountdownUI == null)
            {
                waveCountdownUI = FindAnyObjectByType<WaveCountdownUI>();
            }
        }
        
        private IEnumerator WaveSpawnRoutine()
        {
            // Tìm WaveCountdownUI nếu có
            FindWaveCountdownUI();
            
            while (true)
            {
                // Bắt đầu đợt mới
                isWaveActive = true;
                isWaitingForNextWave = false;
                enemiesRemainingInWave = currentWaveSize;
                
                Debug.Log($"Starting Wave {currentWaveNumber} with {enemiesRemainingInWave} enemies");
                
                // Spawn từng enemy trong đợt
                for (int i = 0; i < currentWaveSize; i++)
                {
                    if (player != null)
                    {
                        SpawnRandomEnemy();
                        yield return new WaitForSeconds(enemySpawnInterval);
                    }
                    else
                    {
                        // Tìm lại player nếu không tìm thấy
                        FindPlayer();
                        yield return new WaitForSeconds(1f);
                        i--; // Thử lại
                    }
                }
                
                // Chờ cho đến khi tất cả enemy trong đợt bị tiêu diệt
                yield return StartCoroutine(WaitForWaveCompletion());
                
                // Nghỉ giữa các đợt
                isWaitingForNextWave = true;
                Debug.Log($"Wave {currentWaveNumber} completed! Waiting {timeBetweenWaves} seconds until next wave...");
                
                // Hiển thị đếm ngược cho đợt tiếp theo
                if (waveCountdownUI != null)
                {
                    waveCountdownUI.StartNextWaveCountdown();
                }
                
                yield return new WaitForSeconds(timeBetweenWaves);
                
                // Chuẩn bị cho đợt tiếp theo
                currentWaveNumber++;
                UpdateSpawnSettings();
            }
        }
        
        private IEnumerator WaitForWaveCompletion()
        {
            while (true)
            {
                CleanupDestroyedEnemies();
                
                if (spawnedEnemies.Count == 0)
                {
                    // Tất cả enemy đã bị tiêu diệt
                    break;
                }
                
                yield return new WaitForSeconds(1f);
            }
            
            isWaveActive = false;
        }
        
        private void CleanupDestroyedEnemies()
        {
            spawnedEnemies.RemoveAll(enemy => enemy == null);
        }
        
        private void SpawnRandomEnemy()
        {
            if (enemySpawnData.Count == 0)
            {
                Debug.LogWarning("Không có enemy data để spawn!");
                return;
            }
            
            // Select random enemy type based on weight
            EnemySpawnData selectedData = SelectRandomEnemyData();
            if (selectedData == null || selectedData.enemyPrefab == null) return;
            
            // Find valid spawn position
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition == Vector3.zero) return;
            
            // Spawn enemy
            GameObject spawnedEnemy = Instantiate(selectedData.enemyPrefab, spawnPosition, Quaternion.identity, transform);
            
            // Configure enemy based on current level
            ConfigureEnemyForLevel(spawnedEnemy, selectedData);
            
            // Add to spawned enemies list
            spawnedEnemies.Add(spawnedEnemy);
        }
        
        private EnemySpawnData SelectRandomEnemyData()
        {
            float totalWeight = 0f;
            foreach (var data in enemySpawnData)
            {
                totalWeight += data.spawnWeight;
            }
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var data in enemySpawnData)
            {
                currentWeight += data.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return data;
                }
            }
            
            return enemySpawnData[0]; // Fallback
        }
        
        private Vector3 GetValidSpawnPosition()
        {
            if (validSpawnPositions.Count == 0 || player == null) 
                return Vector3.zero;
            
            int attempts = 0;
            int maxAttempts = 50;
            
            while (attempts < maxAttempts)
            {
                // Get random position from valid positions
                Vector3Int randomCellPos = validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
                Vector3 worldPos = floorTilemap.CellToWorld(randomCellPos) + floorTilemap.cellSize * 0.5f;
                
                float distanceToPlayer = Vector3.Distance(worldPos, player.position);
                
                // Check if position is within desired range
                if (distanceToPlayer >= minDistanceFromPlayer && distanceToPlayer <= maxDistanceFromPlayer)
                {
                    // Check if position is not occupied by other enemies
                    bool isOccupied = false;
                    foreach (GameObject enemy in spawnedEnemies)
                    {
                        if (enemy != null && Vector3.Distance(worldPos, enemy.transform.position) < 2f)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                    
                    if (!isOccupied)
                    {
                        return worldPos;
                    }
                }
                
                attempts++;
            }
            
            Debug.LogWarning("Không thể tìm được vị trí spawn hợp lệ sau " + maxAttempts + " lần thử!");
            return Vector3.zero;
        }
        
        private void ConfigureEnemyForLevel(GameObject enemyObject, EnemySpawnData data)
        {
            InfinityEnemy enemy = enemyObject.GetComponent<InfinityEnemy>();
            if (enemy == null) return;
            
            // Apply settings
            enemy.SetEnemyType(data.enemyType);
            enemy.SetExpReward(data.baseExpReward);
            
            // Health và damage scaling sẽ được handle bởi EnemyHealth và Attack components
        }
        
        void OnDrawGizmosSelected()
        {
            if (player == null) return;
            
            // Draw spawn radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, spawnRadius);
            
            // Draw min distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
            
            // Draw max distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
            
            // Draw tilemap bounds
            if (floorTilemap != null)
            {
                Gizmos.color = Color.green;
                Vector3Int centerInt = new Vector3Int(
                    Mathf.RoundToInt(tilemapBounds.center.x), 
                    Mathf.RoundToInt(tilemapBounds.center.y), 
                    Mathf.RoundToInt(tilemapBounds.center.z)
                );
                Vector3 center = floorTilemap.CellToWorld(centerInt);
                Vector3 size = new Vector3(tilemapBounds.size.x, tilemapBounds.size.y, 1) * floorTilemap.cellSize.x;
                Gizmos.DrawWireCube(center, size);
            }
        }
        
        void OnDestroy()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
        }
        
        // Public methods for external control
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
        
        public void ResumeSpawning()
        {
            StartSpawning();
        }
        
        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedEnemies.Clear();
        }
        
        public int GetCurrentEnemyCount()
        {
            CleanupDestroyedEnemies();
            return spawnedEnemies.Count;
        }
        
        public int GetCurrentWaveNumber() => currentWaveNumber;
        public int GetCurrentWaveSize() => currentWaveSize;
        public bool IsWaveActive() => isWaveActive;
        public bool IsWaitingForNextWave() => isWaitingForNextWave;
        public int GetEnemiesRemainingInWave() => enemiesRemainingInWave;
    }
}
