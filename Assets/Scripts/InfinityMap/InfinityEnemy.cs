using UnityEngine;

namespace InfinityMap
{
    public enum EnemyType
    {
        Skeleton,
        Slime
    }
    
    public class InfinityEnemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private int expReward = 10;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject deathEffect;
        
        // Components - sử dụng lại từ hệ thống cũ
        private InfinityEnemyMovement movement;
        private EnemyHealth enemyHealth; // Sử dụng EnemyHealth cũ
        private SkeletonAttack skeletonAttack; // Cho Skeleton
        private SlimeAttack slimeAttack; // Cho Slime
        
        // State
        private bool isDead = false;
        private int currentExpReward;
        
        void Start()
        {
            InitializeEnemy();
        }
        
        private void InitializeEnemy()
        {
            currentExpReward = expReward;
            
            // Safely get components
            movement = GetComponent<InfinityEnemyMovement>();
            enemyHealth = GetComponent<EnemyHealth>();
            
            // Get attack components dựa vào enemy type
            skeletonAttack = GetComponent<SkeletonAttack>();
            slimeAttack = GetComponent<SlimeAttack>();
            
            // Validate required components
            if (enemyHealth == null)
            {
                Debug.LogError($"EnemyHealth component missing on {gameObject.name}! Adding default component.");
                enemyHealth = gameObject.AddComponent<EnemyHealth>();
            }
            
            if (movement == null)
            {
                Debug.LogWarning($"InfinityEnemyMovement component missing on {gameObject.name}! Adding default component.");
                movement = gameObject.AddComponent<InfinityEnemyMovement>();
            }
            
            // Subscribe to death event của EnemyHealth
            if (enemyHealth != null)
            {
                // Vì EnemyHealth cũ không có event, ta sẽ check trong Update
                InvokeRepeating(nameof(CheckIfDead), 0.1f, 0.1f);
            }
        }
        
        private void CheckIfDead()
        {
            if (isDead) return;
            
            // Kiểm tra nếu GameObject bị destroy (EnemyHealth tự destroy khi chết)
            if (this == null || gameObject == null)
            {
                OnEnemyDeath();
                return;
            }
        }
        
        private void OnEnemyDeath()
        {
            if (isDead) return;
            isDead = true;
            
            // Award experience to player
            if (PlayerLevel.Instance != null)
            {
                PlayerLevel.Instance.AddExp(currentExpReward);
            }
            
            // Notify manager
            if (InfinityMapManager.Instance != null)
            {
                InfinityMapManager.Instance.OnEnemyKilled();
            }
            
            // Show death effect
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            
            Debug.Log($"{enemyType} died and awarded {currentExpReward} exp!");
            
            CancelInvoke(nameof(CheckIfDead));
        }
        
        void OnDestroy()
        {
            if (!isDead)
            {
                OnEnemyDeath();
            }
        }
        
        // Getters
        public EnemyType GetEnemyType() => enemyType;
        public bool IsDead() => isDead;
        public int GetExpReward() => currentExpReward;
        public EnemyHealth GetEnemyHealth() => enemyHealth;
        
        // Setters for spawner configuration
        public void SetEnemyType(EnemyType type) => enemyType = type;
        public void SetExpReward(int exp) => currentExpReward = exp;
        
        // Scale health và damage thông qua EnemyHealth và Attack components
        public void SetMaxHealth(int health)
        {
            if (enemyHealth != null)
            {
                try
                {
                    // EnemyHealth cũ không có public setter, ta sẽ dùng reflection
                    var healthField = typeof(EnemyHealth).GetField("maxHealth", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (healthField != null)
                    {
                        healthField.SetValue(enemyHealth, health);
                    }
                    
                    var currentHealthField = typeof(EnemyHealth).GetField("currentHealth", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (currentHealthField != null)
                    {
                        currentHealthField.SetValue(enemyHealth, health);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not set health via reflection: {e.Message}");
                }
            }
        }
        
        public void SetDamage(int damage)
        {
            try
            {
                // Set damage cho attack components tương ứng
                if (enemyType == EnemyType.Slime && slimeAttack != null)
                {
                    var damageField = typeof(SlimeAttack).GetField("attackDamage", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (damageField != null)
                    {
                        damageField.SetValue(slimeAttack, damage);
                    }
                }
                // Skeleton damage được set thông qua bullet damage, không cần modify ở đây
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set damage via reflection: {e.Message}");
            }
        }
        
        public int GetCurrentHealth()
        {
            if (enemyHealth != null)
            {
                try
                {
                    var currentHealthField = typeof(EnemyHealth).GetField("currentHealth", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (currentHealthField != null)
                    {
                        return (int)currentHealthField.GetValue(enemyHealth);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not get current health via reflection: {e.Message}");
                }
            }
            return 0;
        }
        
        public int GetMaxHealth()
        {
            if (enemyHealth != null)
            {
                try
                {
                    var maxHealthField = typeof(EnemyHealth).GetField("maxHealth", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (maxHealthField != null)
                    {
                        return (int)maxHealthField.GetValue(enemyHealth);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not get max health via reflection: {e.Message}");
                }
            }
            return 0;
        }
    }
}
