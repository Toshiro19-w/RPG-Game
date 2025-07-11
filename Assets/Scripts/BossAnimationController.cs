using System.Collections;
using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootingAnimationTrigger = "Shooting";
    [SerializeField] private string idleAnimationTrigger = "Idle";
    
    [Header("Shooting Detection")]
    [SerializeField] private float shootingAnimationDuration = 1f; // Thời gian animation bắn
    
    private EnemyMovement enemyMovement;
    private SkeletonAttack skeletonAttack;
    private BossAttack bossAttack; // Thêm hỗ trợ cho BossAttack
    private bool isShooting = false;
    private bool isInShootingRange = false;
    
    void Start()
    {
        // Tự động lấy components với null checks
        if (animator == null)
            animator = GetComponent<Animator>();
            
        enemyMovement = GetComponent<EnemyMovement>();
        skeletonAttack = GetComponent<SkeletonAttack>();
        bossAttack = GetComponent<BossAttack>();
        
        // Kiểm tra các components quan trọng
        if (animator == null)
        {
            Debug.LogError($"Animator component không tìm thấy trên {gameObject.name}!");
        }
        
        if (enemyMovement == null)
        {
            Debug.LogError($"EnemyMovement component không tìm thấy trên {gameObject.name}!");
        }
    }
    
    void Update()
    {
        CheckShootingState();
    }
    
    private void CheckShootingState()
    {
        if (enemyMovement == null || animator == null) return;
        
        // Kiểm tra xem người chơi còn sống không
        if (!IsPlayerAlive())
        {
            // Nếu người chơi chết, dừng bắn ngay lập tức
            if (isInShootingRange || isShooting)
            {
                isInShootingRange = false;
                StopShootingAnimation();
            }
            return;
        }
        
        // Kiểm tra xem Boss có phát hiện người chơi và trong tầm bắn không
        bool hasSpottedPlayer = enemyMovement.HasSpottedPlayer();
        bool inRange = IsPlayerInShootingRange();
        bool attackComponentIsShooting = IsAttackComponentShooting();
        
        // Nếu phát hiện người chơi và trong tầm bắn
        if (hasSpottedPlayer && inRange)
        {
            if (!isInShootingRange)
            {
                // Chuyển sang trạng thái bắn
                isInShootingRange = true;
                StartShootingAnimation();
            }
            // Cập nhật animation dựa trên trạng thái bắn của attack component
            else if (attackComponentIsShooting && !isShooting)
            {
                StartShootingAnimation();
            }
            else if (!attackComponentIsShooting && isShooting)
            {
                // Tạm dừng animation nhưng vẫn giữ trạng thái trong tầm bắn
                StopShootingAnimationTemporary();
            }
        }
        else
        {
            if (isInShootingRange)
            {
                // Quay lại trạng thái idle
                isInShootingRange = false;
                StopShootingAnimation();
            }
        }
    }
    
    private bool IsAttackComponentShooting()
    {
        if (bossAttack != null)
        {
            return bossAttack.IsShooting();
        }
        // SkeletonAttack không có IsShooting method, nên ta giả định luôn bắn khi trong tầm
        else if (skeletonAttack != null)
        {
            return true; // SkeletonAttack bắn liên tục khi trong tầm
        }
        
        return false;
    }
    
    private bool IsPlayerAlive()
    {
        if (enemyMovement == null) return false;
        
        Transform player = enemyMovement.GetPlayerTransform();
        if (player == null) return false;
        
        // Kiểm tra xem player GameObject có active không
        if (!player.gameObject.activeInHierarchy) return false;
        
        // Kiểm tra PlayerHealth component nếu có
        if (player.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            return playerHealth.CurrentHealth > 0;
        }
        
        // Nếu không có PlayerHealth component, chỉ kiểm tra GameObject active
        return true;
    }
    
    private bool IsPlayerInShootingRange()
    {
        if (bossAttack != null)
        {
            return bossAttack.IsInShootingRange();
        }
        else if (skeletonAttack != null)
        {
            return skeletonAttack.IsInShootingRange();
        }
        
        return false;
    }
    
    private float GetShootingRange()
    {
        if (bossAttack != null)
        {
            return bossAttack.GetShootingRange();
        }
        else if (skeletonAttack != null)
        {
            return skeletonAttack.GetShootingRange();
        }
        
        // Mặc định return 5f nếu không tìm thấy
        return 5f;
    }
    
    private float GetFireRate()
    {
        if (bossAttack != null)
        {
            return bossAttack.GetFireRate();
        }
        else if (skeletonAttack != null)
        {
            return skeletonAttack.GetFireRate();
        }
        
        return shootingAnimationDuration;
    }
    
    private void StartShootingAnimation()
    {
        if (isShooting || animator == null) return;
        
        isShooting = true;
        
        // Dừng animation di chuyển
        animator.SetBool("isMoving", false);
        
        // Bắt đầu animation bắn
        animator.SetTrigger(shootingAnimationTrigger);
        animator.SetBool("isShooting", true);
        
        // Bắt đầu coroutine để quản lý thời gian animation
        StartCoroutine(ShootingAnimationCoroutine());
    }
    
    private void StopShootingAnimation()
    {
        isShooting = false;
        
        if (animator == null) return;
        
        // Dừng animation bắn
        animator.SetBool("isShooting", false);
        
        // Chuyển về idle nếu không di chuyển
        if (enemyMovement != null)
        {
            Transform player = enemyMovement.GetPlayerTransform();
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                // Chỉ set idle nếu không cần di chuyển
                if (distanceToPlayer <= 1f) // stoppingDistance
                {
                    animator.SetTrigger(idleAnimationTrigger);
                }
            }
        }
    }
    
    private void StopShootingAnimationTemporary()
    {
        isShooting = false;
        if (animator != null)
        {
            animator.SetBool("isShooting", false);
        }
        // Không trigger idle animation, chỉ dừng shooting
    }
    
    private IEnumerator ShootingAnimationCoroutine()
    {
        while (isShooting && isInShootingRange && IsPlayerAlive())
        {
            // Sử dụng fire rate từ attack component hoặc giá trị mặc định
            float animationDuration = GetFireRate();
            
            // Chờ thời gian animation bắn
            yield return new WaitForSeconds(animationDuration);
            
            // Kiểm tra lại trạng thái người chơi trước khi tiếp tục
            if (!IsPlayerAlive())
            {
                // Người chơi chết, dừng animation
                StopShootingAnimation();
                break;
            }
            
            // Nếu vẫn trong tầm bắn và người chơi còn sống, tiếp tục animation
            if (isInShootingRange && IsPlayerAlive())
            {
                animator.SetTrigger(shootingAnimationTrigger);
            }
        }
        
        // Đảm bảo dừng animation nếu người chơi chết
        if (!IsPlayerAlive())
        {
            StopShootingAnimation();
        }
    }
    
    // Public methods để các script khác có thể gọi
    public bool IsShooting()
    {
        return isShooting;
    }
    
    public void ForceStartShooting()
    {
        StartShootingAnimation();
    }
    
    public void ForceStopShooting()
    {
        StopShootingAnimation();
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (enemyMovement != null)
        {
            float shootingRange = GetShootingRange();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, shootingRange);
            
            // Hiển thị trạng thái hiện tại
            if (isShooting)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
        }
    }
}
