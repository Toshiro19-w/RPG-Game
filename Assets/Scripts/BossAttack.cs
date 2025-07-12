using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Boss Attack Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint; // Vị trí bắn đạn
    [SerializeField] float fireRate = 2f; // Tốc độ bắn cho Boss (chậm hơn skeleton)
    [SerializeField] float fireSpeed = 7f; // Tốc độ đạn của Boss (nhanh hơn)
    [SerializeField] float bulletLifetime = 8f; // Thời gian sống của đạn Boss
    [SerializeField] float shootingRange = 6f; // Khoảng cách bắn của Boss (xa hơn)
    
    [Header("Boss Special Attacks")]
    [SerializeField] bool enableBurstFire = true; // Bắn liên tiếp
    [SerializeField] int burstCount = 3; // Số đạn trong một lần bắn liên tiếp
    [SerializeField] float burstDelay = 0.3f; // Khoảng cách giữa các đạn trong burst
    
    private EnemyMovement enemyMovement;
    private BossAnimationController bossAnimController;
    private BossStompingAttack bossStompingAttack; // Thêm reference đến stomping attack
    private bool isShooting = false;
    
    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        bossAnimController = GetComponent<BossAnimationController>();
        bossStompingAttack = GetComponent<BossStompingAttack>(); // Lấy stomping attack component
        
        // Kiểm tra các components quan trọng
        if (enemyMovement == null)
        {
            Debug.LogError($"EnemyMovement component không tìm thấy trên {gameObject.name}!");
        }
        
        if (bulletPrefab == null)
        {
            Debug.LogError($"Bullet Prefab chưa được gán cho {gameObject.name}!");
        }
        
        if (firePoint == null)
        {
            Debug.LogError($"Fire Point chưa được gán cho {gameObject.name}!");
        }
        
        // Bắt đầu kiểm tra và bắn đạn
        StartCoroutine(CheckAndShoot());
    }

    IEnumerator CheckAndShoot()
    {
        while (true)
        {
            // Kiểm tra nếu đang stomping, không bắn
            if (bossStompingAttack != null && bossStompingAttack.IsStomping())
            {
                yield return new WaitForSeconds(fireRate);
                continue;
            }
            
            if (enemyMovement != null && enemyMovement.HasSpottedPlayer() && 
                CheckShootingRange() && IsPlayerAlive())
            {
                if (!isShooting)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            yield return new WaitForSeconds(fireRate);
        }
    }

    private IEnumerator PerformAttack()
    {
        isShooting = true;

        if (enableBurstFire)
        {
            // Bắn liên tiếp
            for (int i = 0; i < burstCount; i++)
            {
                // Kiểm tra người chơi còn sống trước khi bắn
                if (!IsPlayerAlive())
                {
                    break; // Dừng bắn nếu người chơi chết
                }
                
                Shoot();
                if (i < burstCount - 1) // Không delay sau viên cuối
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }
        }
        else
        {
            // Bắn đơn
            if (IsPlayerAlive())
            {
                Shoot();
            }
        }
        
        isShooting = false;
    }

    bool CheckShootingRange()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            return distanceToPlayer <= shootingRange;
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

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null || enemyMovement == null)
        {
            return;
        }
        
        // Tạo đạn tại vị trí firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Xác định hướng bắn với độ chính xác cao hơn cho Boss
        Transform player = enemyMovement.GetPlayerTransform();
        if (player != null)
        {
            // Predict player position (advanced AI for Boss)
            Vector2 targetPosition = PredictPlayerPosition(player);
            Vector2 direction = (targetPosition - (Vector2)firePoint.position).normalized;
            
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * fireSpeed;
            }
            
            // Xoay đạn theo hướng bắn
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        Destroy(bullet, bulletLifetime);
    }
    
    // AI nâng cao: dự đoán vị trí người chơi
    private Vector2 PredictPlayerPosition(Transform player)
    {
        if (player == null || firePoint == null) return Vector2.zero;
        
        Vector2 currentPos = player.position;
        
        // Thử lấy velocity của player để dự đoán
        if (player.TryGetComponent<Rigidbody2D>(out var playerRb))
        {
            // Tính thời gian đạn bay đến player
            float distanceToPlayer = Vector2.Distance(firePoint.position, currentPos);
            float timeToReach = distanceToPlayer / fireSpeed;
            
            // Dự đoán vị trí player sau thời gian timeToReach
            Vector2 predictedPos = currentPos + (playerRb.linearVelocity * timeToReach);
            return predictedPos;
        }
        
        return currentPos;
    }

    void OnDrawGizmosSelected()
    {
        // Hiển thị khoảng cách bắn trong Scene view - Màu cam cho shooting range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
        
        // Hiển thị firePoint - Màu cyan cho fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }

    // Public methods để BossAnimationController có thể truy cập
    public bool IsInShootingRange()
    {
        return CheckShootingRange();
    }

    public float GetShootingRange()
    {
        return shootingRange;
    }

    public float GetFireRate()
    {
        return fireRate;
    }
    
    public bool IsShooting()
    {
        return isShooting;
    }
}
