using UnityEngine;
using System.Collections;

public class SlimeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f; // Khoảng cách để bắt đầu tấn công
    [SerializeField] private float lungeSpeed = 8f; // Tốc độ lướt vào
    [SerializeField] private float lungeDistance = 3f; // Khoảng cách lướt
    [SerializeField] private int attackDamage = 15; // Sát thương gây ra
    [SerializeField] private float attackCooldown = 2f; // Thời gian nghỉ giữa các lần tấn công
    [SerializeField] private float knockbackForce = 5f; // Lực đẩy ngược lại
    [SerializeField] private float knockbackDuration = 0.5f; // Thời gian bật ngược
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugGizmos = true;
    
    // Components
    private EnemyMovement enemyMovement;
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D slimeCollider;
    
    // Attack state
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isKnockedBack = false;
    private Vector3 attackDirection;
    private Vector3 originalPosition;
    
    void Start()
    {
        // Lấy các components cần thiết
        enemyMovement = GetComponent<EnemyMovement>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        slimeCollider = GetComponent<Collider2D>();
        
        // Kiểm tra components quan trọng
        if (enemyMovement == null)
        {
            Debug.LogError($"EnemyMovement component không tìm thấy trên {gameObject.name}!");
        }
        
        if (rb == null)
        {
            Debug.LogError($"Rigidbody2D component không tìm thấy trên {gameObject.name}!");
        }
        
        // Bắt đầu kiểm tra tấn công
        StartCoroutine(CheckAndAttack());
    }

    void Update()
    {
        // Update attack logic sẽ được xử lý trong Coroutines
    }
    
    IEnumerator CheckAndAttack()
    {
        while (true)
        {
            if (enemyMovement != null && enemyMovement.HasSpottedPlayer() && 
                canAttack && !isAttacking && !isKnockedBack && IsPlayerAlive())
            {
                if (CheckAttackRange())
                {
                    StartCoroutine(PerformLungeAttack());
                }
            }
            yield return new WaitForSeconds(0.1f); // Kiểm tra mỗi 0.1 giây
        }
    }
    
    bool CheckAttackRange()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            return distanceToPlayer <= attackRange;
        }
        return false;
    }
    
    private bool IsPlayerAlive()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
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
    
    IEnumerator PerformLungeAttack()
    {
        isAttacking = true;
        canAttack = false;
        
        // Dừng di chuyển thông thường
        if (enemyMovement != null)
        {
            enemyMovement.SetStopForShooting(true);
        }
        
        // Lưu vị trí ban đầu
        originalPosition = transform.position;
        
        // Tính toán hướng tấn công
        Transform player = enemyMovement.GetPlayerTransform();
        if (player != null)
        {
            attackDirection = (player.position - transform.position).normalized;
            
            // Trigger animation nếu có
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            Debug.Log($"Slime bắt đầu lướt về phía Player!");
            
            // Thực hiện lướt vào
            yield return StartCoroutine(LungeTowardsPlayer());
            
            // Kiểm tra và gây sát thương
            CheckAndDealDamage();
            
            // Bật ngược trở lại
            yield return StartCoroutine(KnockbackAfterAttack());
        }
        
        // Khôi phục trạng thái
        isAttacking = false;
        
        // Cho phép di chuyển lại
        if (enemyMovement != null)
        {
            enemyMovement.SetStopForShooting(false);
        }
        
        // Cooldown trước lần tấn công tiếp theo
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        
        Debug.Log($"Slime sẵn sàng cho lần tấn công tiếp theo!");
    }
    
    IEnumerator LungeTowardsPlayer()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (attackDirection * lungeDistance);
        
        float elapsed = 0f;
        float lungeDuration = lungeDistance / lungeSpeed;
        
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lungeDuration;
            
            // Di chuyển slime theo hướng tấn công
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            transform.position = newPosition;
            
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    
    void CheckAndDealDamage()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player == null) return;
        
        // Kiểm tra khoảng cách với player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= 1f) // Khoảng cách tấn công
        {
            // Gây sát thương cho player
            if (player.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Slime gây {attackDamage} sát thương cho Player!");
            }
            else if (player.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Slime gây {attackDamage} sát thương cho Player!");
            }
        }
        else
        {
            Debug.Log($"Slime không trúng Player (khoảng cách: {distanceToPlayer:F2})");
        }
    }
    
    IEnumerator KnockbackAfterAttack()
    {
        isKnockedBack = true;
        
        // Tính toán hướng bật ngược (ngược lại với hướng tấn công)
        Vector3 knockbackDirection = -attackDirection;
        Vector3 startPosition = transform.position;
        Vector3 knockbackTarget = startPosition + (knockbackDirection * knockbackForce);
        
        float elapsed = 0f;
        
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / knockbackDuration;
            
            // Sử dụng easing để tạo hiệu ứng bật ngược tự nhiên
            float easedProgress = 1f - (1f - progress) * (1f - progress);
            
            Vector3 newPosition = Vector3.Lerp(startPosition, knockbackTarget, easedProgress);
            transform.position = newPosition;
            
            yield return null;
        }
        
        isKnockedBack = false;
        Debug.Log($"Slime hoàn thành bật ngược!");
    }
    
    // Xử lý va chạm với player trong lúc tấn công
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isAttacking && other.CompareTag("Player"))
        {
            // Gây sát thương ngay khi chạm vào
            if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Slime va chạm và gây {attackDamage} sát thương cho Player!");
            }
            else if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"Slime va chạm và gây {attackDamage} sát thương cho Player!");
            }
        }
    }
    
    // Public methods cho các script khác
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public bool CanAttack()
    {
        return canAttack;
    }
    
    public float GetAttackRange()
    {
        return attackRange;
    }
    
    public float GetAttackCooldown()
    {
        return attackCooldown;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!enableDebugGizmos) return;
        
        // Hiển thị phạm vi tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Hiển thị khoảng cách lướt
        if (isAttacking && attackDirection != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Vector3 lungeTarget = transform.position + (attackDirection * lungeDistance);
            Gizmos.DrawLine(transform.position, lungeTarget);
            Gizmos.DrawWireSphere(lungeTarget, 0.3f);
        }
        
        // Hiển thị vị trí ban đầu nếu đang tấn công
        if (isAttacking && originalPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originalPosition, 0.2f);
        }
    }
}
