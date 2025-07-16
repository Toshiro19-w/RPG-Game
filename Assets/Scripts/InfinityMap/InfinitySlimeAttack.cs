using UnityEngine;
using System.Collections;
using InfinityMap;

public class InfinitySlimeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f; // Khoảng cách để bắt đầu tấn công
    [SerializeField] private float lungeSpeed = 8f; // Tốc độ lướt vào
    [SerializeField] private float lungeDistance = 3f; // Khoảng cách lướt
    [SerializeField] private int attackDamage = 15; // Sát thương gây ra
    [SerializeField] private float attackCooldown = 2f; // Thời gian nghỉ giữa các lần tấn công
    [SerializeField] private float knockbackForce = 5f; // Lực đẩy ngược lại
    [SerializeField] private float knockbackDuration = 0.5f; // Thời gian bật ngược
    
    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayerMask = -1; // Layer của tường
    [SerializeField] private float wallCheckDistance = 0.8f; // Khoảng cách check tường
    [SerializeField] private float safetyBuffer = 0.3f; // Khoảng cách an toàn từ tường
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugGizmos = true;
    
    // Components
    private InfinityEnemyMovement enemyMovement;
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
        enemyMovement = GetComponent<InfinityEnemyMovement>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        slimeCollider = GetComponent<Collider2D>();
        
        // Kiểm tra components quan trọng
        if (enemyMovement == null)
        {
            Debug.LogError($"InfinityEnemyMovement component không tìm thấy trên {gameObject.name}!");
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
            if (enemyMovement != null && 
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
        
        // Dừng di chuyển thông thường - sử dụng method của InfinityEnemyMovement
        if (enemyMovement != null)
        {
            enemyMovement.SetStopForAttack(true);
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
            
            Debug.Log($"InfinitySlime bắt đầu lướt về phía Player!");
                        
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
            enemyMovement.SetStopForAttack(false);
        }
        
        // Cooldown trước lần tấn công tiếp theo
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        
        Debug.Log($"InfinitySlime sẵn sàng cho lần tấn công tiếp theo!");
        }
    
    IEnumerator LungeTowardsPlayer()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = CalculateSafeLungeTarget();
        
        float elapsed = 0f;
        float actualDistance = Vector3.Distance(startPosition, targetPosition);
        float lungeDuration = actualDistance / lungeSpeed;
        
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lungeDuration;
            
            // Di chuyển slime theo hướng tấn công với kiểm tra tường
            Vector3 desiredPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            Vector3 safePosition = GetSafePosition(desiredPosition);
            transform.position = safePosition;
            
            yield return null;
        }
        
        transform.position = GetSafePosition(targetPosition);
    }
    
    void CheckAndDealDamage()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player == null) return;
        
        // Kiểm tra khoảng cách với player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= 1f) // Khoảng cách tấn công
        {
            // Kiểm tra an toàn cho player trước khi gây sát thương
            CheckPlayerSafety();
            
            // Gây sát thương cho player
            if (player.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"InfinitySlime gây {attackDamage} sát thương cho Player!");
            }
            else if (player.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"InfinitySlime gây {attackDamage} sát thương cho Player!");
            }
        }
        else
        {
            Debug.Log($"InfinitySlime không trúng Player (khoảng cách: {distanceToPlayer:F2})");
        }
    }
    
    IEnumerator KnockbackAfterAttack()
    {
        isKnockedBack = true;
        
        // Tính toán hướng bật ngược (ngược lại với hướng tấn công)
        Vector3 knockbackDirection = -attackDirection;
        Vector3 startPosition = transform.position;
        Vector3 knockbackTarget = CalculateSafeKnockbackTarget(startPosition, knockbackDirection);
        
        float elapsed = 0f;
        
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / knockbackDuration;
            
            // Sử dụng easing để tạo hiệu ứng bật ngược tự nhiên
            float easedProgress = 1f - (1f - progress) * (1f - progress);
            
            Vector3 desiredPosition = Vector3.Lerp(startPosition, knockbackTarget, easedProgress);
            Vector3 safePosition = GetSafePosition(desiredPosition);
            transform.position = safePosition;
            
            yield return null;
        }
        
        // Đảm bảo slime ở vị trí an toàn cuối cùng
        transform.position = GetSafePosition(knockbackTarget);
        isKnockedBack = false;
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
                Debug.Log($"InfinitySlime va chạm và gây {attackDamage} sát thương cho Player!");
            }
            else if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"InfinitySlime va chạm và gây {attackDamage} sát thương cho Player!");
            }
        }
    }
    
    // Kiểm tra va chạm với tường
    private bool IsWallInDirection(Vector3 position, Vector3 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, wallLayerMask);
        return hit.collider != null;
    }
    
    // Tính toán mục tiêu lướt an toàn
    private Vector3 CalculateSafeLungeTarget()
    {
        Vector3 startPosition = transform.position;
        Vector3 idealTarget = startPosition + (attackDirection * lungeDistance);
        
        // Kiểm tra xem có tường nào chặn đường không
        float checkStep = 0.2f;
        float maxSafeDistance = 0f;
        
        for (float distance = checkStep; distance <= lungeDistance; distance += checkStep)
        {
            Vector3 checkPoint = startPosition + (attackDirection * distance);
            
            if (IsWallInDirection(checkPoint, Vector3.zero, 0.1f))
            {
                // Tìm thấy tường, dừng ở khoảng cách an toàn trước đó
                break;
            }
            maxSafeDistance = distance;
        }
        
        // Trừ thêm safety buffer để tránh kẹt trong tường
        maxSafeDistance = Mathf.Max(0.5f, maxSafeDistance - safetyBuffer);
        return startPosition + (attackDirection * maxSafeDistance);
    }
    
    // Tính toán vị trí knockback an toàn
    private Vector3 CalculateSafeKnockbackTarget(Vector3 startPos, Vector3 knockbackDir)
    {
        Vector3 idealTarget = startPos + (knockbackDir * knockbackForce);
        
        // Kiểm tra xem có tường nào chặn đường knockback không
        float checkStep = 0.2f;
        float maxSafeDistance = 0f;
        
        for (float distance = checkStep; distance <= knockbackForce; distance += checkStep)
        {
            Vector3 checkPoint = startPos + (knockbackDir * distance);
            
            if (IsWallInDirection(checkPoint, Vector3.zero, 0.1f))
            {
                // Tìm thấy tường, dừng ở khoảng cách an toàn trước đó
                break;
            }
            maxSafeDistance = distance;
        }
        
        // Trừ thêm safety buffer
        maxSafeDistance = Mathf.Max(0.2f, maxSafeDistance - safetyBuffer);
        return startPos + (knockbackDir * maxSafeDistance);
    }
    
    // Lấy vị trí an toàn gần nhất
    private Vector3 GetSafePosition(Vector3 desiredPosition)
    {
        // Kiểm tra xem vị trí mong muốn có an toàn không
        if (!IsWallInDirection(desiredPosition, Vector3.zero, 0.1f))
        {
            return desiredPosition;
        }
        
        // Nếu không an toàn, tìm vị trí an toàn gần nhất
        Vector3[] directions = {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right,
            new Vector3(1, 1, 0).normalized, new Vector3(-1, 1, 0).normalized,
            new Vector3(1, -1, 0).normalized, new Vector3(-1, -1, 0).normalized
        };
        
        foreach (Vector3 dir in directions)
        {
            for (float distance = safetyBuffer; distance <= 1f; distance += 0.1f)
            {
                Vector3 testPosition = desiredPosition + (dir * distance);
                if (!IsWallInDirection(testPosition, Vector3.zero, 0.1f))
                {
                    return testPosition;
                }
            }
        }
        
        // Nếu không tìm được vị trí an toàn, quay về vị trí hiện tại
        return transform.position;
    }
    
    // Kiểm tra xem player có bị đẩy vào tường không và điều chỉnh
    private void CheckPlayerSafety()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player == null) return;
        
        // Kiểm tra xem player có đang ở gần tường không
        Vector3 playerPos = player.position;
        Vector3 pushDirection = (playerPos - transform.position).normalized;
        
        // Kiểm tra xem có tường ở phía sau player không
        if (IsWallInDirection(playerPos, pushDirection, 0.5f))
        {
            // Tìm vị trí an toàn cho player
            Vector3 safePlayerPos = GetSafePosition(playerPos);
            
            // Nếu player có Rigidbody2D, đẩy nhẹ về vị trí an toàn
            if (player.TryGetComponent<Rigidbody2D>(out var playerRb))
            {
                Vector3 correctionForce = (safePlayerPos - playerPos) * 2f;
                playerRb.AddForce(correctionForce, ForceMode2D.Impulse);
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
    
    // Debug and utility methods
    public bool IsCurrentlyAttacking()
    {
        return isAttacking;
    }
    
    public bool CanCurrentlyAttack()
    {
        return canAttack && !isKnockedBack && IsPlayerAlive();
    }
    
    public float GetDistanceToPlayer()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, player.position);
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!enableDebugGizmos) return;
        
        // Hiển thị phạm vi tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Hiển thị khoảng cách lướt an toàn
        if (isAttacking && attackDirection != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Vector3 safeLungeTarget = CalculateSafeLungeTarget();
            Gizmos.DrawLine(transform.position, safeLungeTarget);
            Gizmos.DrawWireSphere(safeLungeTarget, 0.3f);
            
            // Hiển thị wall check rays
            Gizmos.color = Color.magenta;
            for (float distance = 0.2f; distance <= lungeDistance; distance += 0.4f)
            {
                Vector3 checkPoint = transform.position + (attackDirection * distance);
                Gizmos.DrawWireCube(checkPoint, Vector3.one * 0.1f);
            }
        }
        
        // Hiển thị vị trí ban đầu nếu đang tấn công
        if (isAttacking && originalPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originalPosition, 0.2f);
        }
        
        // Hiển thị wall detection range
        Gizmos.color = Color.cyan;
        Vector3[] checkDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        foreach (Vector3 dir in checkDirections)
        {
            Vector3 rayEnd = transform.position + (dir * wallCheckDistance);
            if (IsWallInDirection(transform.position, dir, wallCheckDistance))
            {
                Gizmos.color = Color.red; // Có tường
            }
            else
            {
                Gizmos.color = Color.green; // Không có tường
            }
            Gizmos.DrawRay(transform.position, dir * wallCheckDistance);
        }
    }
}