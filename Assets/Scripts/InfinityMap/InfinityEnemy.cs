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
        [SerializeField] private int scoreReward = 5;

        [Header("Visual Effects")]
        [SerializeField] private GameObject deathEffect;

        private InfinityEnemyMovement movement;
        private EnemyHealth enemyHealth;
        private SkeletonAttack skeletonAttack;
        private SlimeAttack slimeAttack;

        private bool isDead = false;
        private int currentExpReward;
        private int currentScoreReward;

        void Start()
        {
            InitializeEnemy();
        }

        private void InitializeEnemy()
        {
            currentExpReward = expReward;
            currentScoreReward = scoreReward;

            movement = GetComponent<InfinityEnemyMovement>();
            enemyHealth = GetComponent<EnemyHealth>();
            skeletonAttack = GetComponent<SkeletonAttack>();
            slimeAttack = GetComponent<SlimeAttack>();

            if (enemyHealth == null)
            {
                enemyHealth = gameObject.AddComponent<EnemyHealth>();
            }

            if (movement == null)
            {
                movement = gameObject.AddComponent<InfinityEnemyMovement>();
            }

            if (enemyHealth != null)
            {
                InvokeRepeating(nameof(CheckIfDead), 0.1f, 0.1f);
            }
        }

        private void CheckIfDead()
        {
            if (isDead || enemyHealth == null) return;

            if (GetCurrentHealth() <= 0)
            {
                OnEnemyDeath();
            }
        }

        private void OnEnemyDeath()
        {
            if (isDead) return;
            isDead = true;
            
            CancelInvoke(nameof(CheckIfDead));
            
            if (PlayerScore.Instance != null)
            {
                PlayerScore.Instance.AddScoreFromEnemy(this);
            }
            
            if (InfinityMapManager.Instance != null)
            {
                InfinityMapManager.Instance.OnEnemyKilled();
            }
            
            if (RewardSystem.Instance != null && gameObject.scene.isLoaded)
            {
                RewardSystem.Instance.SpawnRewards(transform.position);
            }
            
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            CancelInvoke(nameof(CheckIfDead));
        }

        public EnemyType GetEnemyType() => enemyType;
        public bool IsDead() => isDead;
        public int GetExpReward() => currentExpReward;
        public int GetScoreReward() => currentScoreReward;
        public EnemyHealth GetEnemyHealth() => enemyHealth;

        public void SetEnemyType(EnemyType type) => enemyType = type;
        public void SetExpReward(int exp) => currentExpReward = exp;
        public void SetScoreReward(int score) => currentScoreReward = score;

        public void SetMaxHealth(int health)
        {
            if (enemyHealth != null)
            {
                try
                {
                    var healthField = typeof(EnemyHealth).GetField("maxHealth",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    healthField?.SetValue(enemyHealth, health);

                    var currentHealthField = typeof(EnemyHealth).GetField("currentHealth",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    currentHealthField?.SetValue(enemyHealth, health);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not set health: {e.Message}");
                }
            }
        }

        public void SetDamage(int damage)
        {
            try
            {
                switch (enemyType)
                {
                    case EnemyType.Slime:
                        if (slimeAttack != null)
                        {
                            var damageField = typeof(SlimeAttack).GetField("attackDamage",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            damageField?.SetValue(slimeAttack, damage);
                        }
                        break;
                        
                    case EnemyType.Skeleton:
                        if (skeletonAttack != null)
                        {
                            var bulletDamageField = typeof(SkeletonAttack).GetField("bulletDamage",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            bulletDamageField?.SetValue(skeletonAttack, damage);
                        }
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set damage: {e.Message}");
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
                    Debug.LogWarning($"Could not get current health: {e.Message}");
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
                    Debug.LogWarning($"Could not get max health: {e.Message}");
                }
            }
            return 0;
        }
    }
}