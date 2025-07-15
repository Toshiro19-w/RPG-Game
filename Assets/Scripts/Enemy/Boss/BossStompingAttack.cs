using System.Collections;
using UnityEngine;

public class BossStompingAttack : MonoBehaviour
{
    [Header("Stomping Attack Settings")]
    [SerializeField] private int shootsBeforeStomping = 4; // Số lần bắn trước khi stomping (3-5)
    [SerializeField] private float stompingDamage = 30f; // Sát thương stomping
    [SerializeField] private float stompingRange = 2f; // Phạm vi gây sát thương
    [SerializeField] private float flyHeight = 3f; // Độ cao bay lên
    [SerializeField] private float flySpeed = 8f; // Tốc độ bay
    [SerializeField] private float stompingDuration = 1.5f; // Thời gian thực hiện stomping
    [SerializeField] private LayerMask playerLayerMask = -1; // Layer của player
    
    [Header("Physics Settings")]
    [SerializeField] private bool disableCollisionDuringStomping = true; // Tắt collision khi stomping
    [SerializeField] private bool makePlayerPassThrough = true; // Player có thể đi qua Boss khi stomping
    
    [Header("Animation Triggers")]
    [SerializeField] private string stompingAnimationTrigger = "Stomping";
    [SerializeField] private string shootingAnimationTrigger = "Shooting";
    [SerializeField] private string idleAnimationTrigger = "Idle";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugGizmos = true;
    
    private Animator animator;
    private BossAttack bossAttack;
    private EnemyMovement enemyMovement;
    private BossAnimationController bossAnimationController;
    
    // Physics components
    private Collider2D bossCollider;
    private Rigidbody2D bossRigidbody;
    
    // Stomping state variables
    private int currentShootCount = 0;
    private bool isStomping = false;
    private bool isFlying = false;
    private Vector3 originalPosition;
    private Vector3 targetStompPosition;
    private Vector3 targetPlayerPosition;
    
    // Tracking variables
    private bool wasShootingLastFrame = false;
    
    private void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        bossAttack = GetComponent<BossAttack>();
        enemyMovement = GetComponent<EnemyMovement>();
        bossAnimationController = GetComponent<BossAnimationController>();
        bossCollider = GetComponent<Collider2D>();
        bossRigidbody = GetComponent<Rigidbody2D>();
        
        // Validate components
        if (animator == null)
        {
            Debug.LogError($"Animator component không tìm thấy trên {gameObject.name}!");
        }
        else
        {
            // Kiểm tra xem Animator có các parameters cần thiết không
            ValidateAnimatorParameters();
        }
        
        if (bossAttack == null)
        {
            Debug.LogError($"BossAttack component không tìm thấy trên {gameObject.name}!");
        }
        
        if (enemyMovement == null)
        {
            Debug.LogError($"EnemyMovement component không tìm thấy trên {gameObject.name}!");
        }
        
        // Get physics components for collision control
        bossCollider = GetComponent<Collider2D>();
        bossRigidbody = GetComponent<Rigidbody2D>();
        
        if (bossCollider == null)
        {
            Debug.LogWarning($"Collider2D component không tìm thấy trên {gameObject.name}! Physics collision control sẽ không hoạt động.");
        }
        
        originalPosition = transform.position;
    }
    
    private void ValidateAnimatorParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Animator hoặc Animator Controller chưa được gán!");
            return;
        }
        
        // Kiểm tra trigger parameters
        bool hasStompingTrigger = false;
        bool hasStompingBool = false;
        
        foreach (var param in animator.parameters)
        {
            if (param.name == stompingAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasStompingTrigger = true;
                Debug.Log($"✓ Tìm thấy Stomping trigger: {stompingAnimationTrigger}");
            }
            if (param.name == "isStomping" && param.type == AnimatorControllerParameterType.Bool)
            {
                hasStompingBool = true;
                Debug.Log($"✓ Tìm thấy isStomping bool parameter");
            }
        }
        
        if (!hasStompingTrigger)
        {
            Debug.LogError($"❌ Không tìm thấy trigger '{stompingAnimationTrigger}' trong Animator Controller!");
        }
        
        if (!hasStompingBool)
        {
            Debug.LogError($"❌ Không tìm thấy bool parameter 'isStomping' trong Animator Controller!");
        }
        
        if (hasStompingTrigger && hasStompingBool)
        {
            Debug.Log("✓ Tất cả animation parameters đã được setup đúng!");
        }
    }
    
    private void Update()
    {
        if (isStomping) return; // Không làm gì khi đang stomping
        
        TrackShootingBehavior();
    }
    
    private void TrackShootingBehavior()
    {
        if (bossAttack == null) return;
        
        bool isShootingNow = bossAttack.IsShooting();
        
        // Detect when shooting ends (transition from shooting to not shooting)
        if (wasShootingLastFrame && !isShootingNow)
        {
            currentShootCount++;
            
            // Check if it's time to stomp
            if (currentShootCount >= shootsBeforeStomping)
            {
                TriggerStompingAttack();
                currentShootCount = 0; // Reset counter
            }
        }
        
        wasShootingLastFrame = isShootingNow;
    }
    
    private void TriggerStompingAttack()
    {
        if (isStomping || enemyMovement == null) return;
        
        Transform player = enemyMovement.GetPlayerTransform();
        if (player == null) return;
        
        // Xác định vị trí player hiện tại
        targetPlayerPosition = player.position;
        targetStompPosition = targetPlayerPosition;
        
        StartCoroutine(PerformStompingSequence());
    }
    
    private IEnumerator PerformStompingSequence()
    {
        isStomping = true;
        isFlying = true;
        
        // Disable other boss behaviors during stomping
        DisableBossComponents();
        
        // Phase 1: Fly up to target position
        yield return StartCoroutine(FlyToTarget());
        
        // Phase 2: Perform stomping animation and damage
        yield return StartCoroutine(PerformStomping());
        
        // Phase 3: Return to ground
        yield return StartCoroutine(ReturnToGround());
        
        // Re-enable boss behaviors
        EnableBossComponents();
        
        isStomping = false;
        isFlying = false;
        
        Debug.Log("Boss Stomping attack hoàn thành!");
    }
    
    private IEnumerator FlyToTarget()
    {
        originalPosition = transform.position;
        Vector3 flyPosition = new Vector3(targetStompPosition.x, targetStompPosition.y + flyHeight, 0);
        
        // Tạm thời không trigger animation ở đây, chỉ set state
        if (animator != null)
        {
            Debug.Log("Boss đang bay lên, chuẩn bị Stomping...");
            animator.SetBool("isStomping", true);
        }
        
        // Calculate flight time based on distance
        float distance = Vector3.Distance(transform.position, flyPosition);
        float flightTime = distance / flySpeed;
        
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        
        while (elapsedTime < flightTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / flightTime;
            
            // Smooth curve for flying motion
            progress = Mathf.SmoothStep(0f, 1f, progress);
            
            transform.position = Vector3.Lerp(startPosition, flyPosition, progress);
            yield return null;
        }
        
        transform.position = flyPosition;
        Debug.Log("Boss đã bay đến vị trí target, chuẩn bị đáp xuống!");
    }
    
    private IEnumerator PerformStomping()
    {
        // Trigger stomping animation ngay trước khi đáp xuống
        if (animator != null)
        {
            Debug.Log($"🎯 TRIGGERING Stomping animation: {stompingAnimationTrigger} - Boss đang đáp xuống!");
            animator.SetTrigger(stompingAnimationTrigger);
        }
        else
        {
            Debug.LogError("❌ Animator is null! Cannot trigger Stomping animation.");
        }
        
        // Wait for a brief moment at the peak để animation có thời gian start
        yield return new WaitForSeconds(0.1f);
        
        // Quick descent to stomp - Boss lao xuống nhanh
        Vector3 stompPosition = new Vector3(targetStompPosition.x, originalPosition.y, 0);
        float descendTime = 0.3f; // Tăng thời gian để animation có thể chạy
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        
        Debug.Log($"🔽 Boss đang lao xuống từ {startPosition} đến {stompPosition}");
        
        while (elapsedTime < descendTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / descendTime;
            progress = progress * progress; // Accelerate descent
            
            transform.position = Vector3.Lerp(startPosition, stompPosition, progress);
            yield return null;
        }
        
        transform.position = stompPosition;
        Debug.Log("💥 Boss đã đáp xuống đất! Checking for damage...");
        
        // Trigger stomp damage
        PerformStompDamage();
        
        // Wait for stomping animation to complete
        yield return new WaitForSeconds(stompingDuration);
        Debug.Log("Stomping animation hoàn thành!");
    }
    
    private IEnumerator ReturnToGround()
    {
        // Already at ground level, just need to clean up animation
        if (animator != null)
        {
            animator.SetBool("isStomping", false);
            // Reset to idle or shooting state
            animator.SetTrigger(idleAnimationTrigger);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private void PerformStompDamage()
    {
        if (enemyMovement == null) return;
        
        Transform player = enemyMovement.GetPlayerTransform();
        if (player == null) return;
        
        // Check if player is within stomping range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= stompingRange)
        {
            // Deal damage to player
            if (player.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage((int)stompingDamage);
                Debug.Log($"Boss Stomping gây {stompingDamage} sát thương cho Player!");
            }
            else if (player.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage((int)stompingDamage);
                Debug.Log($"Boss Stomping gây {stompingDamage} sát thương cho Player!");
            }
        }
        else
        {
            Debug.Log($"Player ngoài tầm Stomping (khoảng cách: {distanceToPlayer:F2})");
        }
        
        // Visual/Audio effects could be added here
        CreateStompEffect();
    }
    
    private void CreateStompEffect()
    {
        // Create visual effect for stomping (can be expanded)
        Debug.Log("Stomp effect tại vị trí: " + transform.position);
        
        // Here you could instantiate particle effects, screen shake, etc.
        // Example:
        // if (stompEffectPrefab != null)
        // {
        //     Instantiate(stompEffectPrefab, transform.position, Quaternion.identity);
        // }
    }
    
    private void DisableBossComponents()
    {
        // Disable movement and normal attack behavior
        if (bossAttack != null)
        {
            bossAttack.enabled = false;
        }
        
        if (bossAnimationController != null)
        {
            bossAnimationController.enabled = false;
        }
        
        // Optionally disable movement
        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }
        
        // Disable collision if enabled
        if (disableCollisionDuringStomping)
        {
            DisableBossPhysics();
        }
    }
    
    private void EnableBossComponents()
    {
        // Re-enable movement and normal attack behavior
        if (bossAttack != null)
        {
            bossAttack.enabled = true;
        }
        
        if (bossAnimationController != null)
        {
            bossAnimationController.enabled = true;
        }
        
        if (enemyMovement != null)
        {
            enemyMovement.enabled = true;
        }
        
        // Re-enable collision
        if (disableCollisionDuringStomping)
        {
            EnableBossPhysics();
        }
    }
    
    private void DisableBossPhysics()
    {
        if (bossCollider != null)
        {
            bossCollider.isTrigger = true; // Make Boss pass-through
            Debug.Log("🔄 Boss collision disabled - Player có thể đi qua Boss");
        }
        
        if (makePlayerPassThrough && enemyMovement != null)
        {
            Transform player = enemyMovement.GetPlayerTransform();
            if (player != null)
            {
                // Disable collision between Boss and Player
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                if (playerCollider != null && bossCollider != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, playerCollider, true);
                    Debug.Log("🔄 Boss-Player collision ignored during stomping");
                }
            }
        }
    }
    
    private void EnableBossPhysics()
    {
        if (bossCollider != null)
        {
            bossCollider.isTrigger = false; // Restore normal collision
            Debug.Log("🔄 Boss collision enabled - Trở lại collision bình thường");
        }
        
        if (makePlayerPassThrough && enemyMovement != null)
        {
            Transform player = enemyMovement.GetPlayerTransform();
            if (player != null)
            {
                // Re-enable collision between Boss and Player
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                if (playerCollider != null && bossCollider != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, playerCollider, false);
                    Debug.Log("🔄 Boss-Player collision restored");
                }
            }
        }
    }
    
    // Public methods for external access
    public bool IsStomping()
    {
        return isStomping;
    }
    
    public bool IsFlying()
    {
        return isFlying;
    }
    
    public void ResetShootCount()
    {
        currentShootCount = 0;
    }
    
    public void ForceStompingAttack()
    {
        if (!isStomping)
        {
            TriggerStompingAttack();
        }
    }
    
    // Debug methods for testing
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestAnimationOnly()
    {
        if (animator != null)
        {
            Debug.Log("🎭 Testing Stomping animation only (no movement)");
            Debug.Log($"Animation trigger: {stompingAnimationTrigger}");
            
            // Test trigger
            animator.SetTrigger(stompingAnimationTrigger);
            animator.SetBool("isStomping", true);
            
            // Reset after 2 seconds
            StartCoroutine(ResetAnimationAfterDelay(2f));
        }
        else
        {
            Debug.LogError("❌ Animator is null! Cannot test animation.");
        }
    }
    
    private IEnumerator ResetAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetBool("isStomping", false);
            animator.SetTrigger(idleAnimationTrigger);
            Debug.Log("Animation test completed, reset to idle");
        }
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (!enableDebugGizmos) return;
        
        // Draw stomping range - Màu đỏ cho phạm vi gây sát thương
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stompingRange);
        
        // Draw fly height - Màu xanh lam cho độ cao bay
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + Vector3.up * flyHeight, Vector3.one * 0.3f);
        
        // Draw target position if stomping - Màu vàng cho vị trí target
        if (isStomping)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetStompPosition, 0.5f);
            
            // Draw flight path - Màu xanh lá cho đường bay
            Gizmos.color = Color.green;
            Vector3 flyPos = new Vector3(targetStompPosition.x, targetStompPosition.y + flyHeight, 0);
            Gizmos.DrawLine(transform.position, flyPos);
            Gizmos.DrawLine(flyPos, targetStompPosition);
        }
        
        // Draw shoot count indicator - Màu trắng cho chỉ số bắn
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Vector3 textPos = transform.position + Vector3.up * 2f;
            // In a real scenario, you might use UnityEditor.Handles for text
            for (int i = 0; i < currentShootCount; i++)
            {
                Gizmos.DrawWireCube(textPos + Vector3.right * (i * 0.3f), Vector3.one * 0.1f);
            }
        }
    }
}
