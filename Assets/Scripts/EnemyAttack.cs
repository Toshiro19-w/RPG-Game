using System.Collections;
using UnityEngine;

public class SkeletonAttack : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint; // Vị trí bắn đạn
    [SerializeField] float fireRate = 1f; // Tốc độ bắn (giây)
    [SerializeField] float fireSpeed = 5f; // Tốc độ đạn
    [SerializeField] float bulletLifetime = 5f; // Thời gian sống của đạn
    [SerializeField] float shootingRange = 3f; // Khoảng cách tối đa để bắn
    EnemyMovement enemyMovement; // Tham chiếu đến EnemyMovement
    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        
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
            if (enemyMovement != null && enemyMovement.HasSpottedPlayer() && IsPlayerAlive())
            {
                bool inShootingRange = CheckShootingRange();
                
                if (inShootingRange)
                {
                    // Dừng di chuyển khi trong tầm bắn và bắn
                    enemyMovement.SetStopForShooting(true);
                    Shoot();
                }
                else
                {
                    // Cho phép di chuyển khi không trong tầm bắn
                    enemyMovement.SetStopForShooting(false);
                }
            }
            else
            {
                // Cho phép di chuyển khi không trong điều kiện bắn
                if (enemyMovement != null)
                {
                    enemyMovement.SetStopForShooting(false);
                }
            }
            yield return new WaitForSeconds(fireRate); // Lặp lại sau mỗi khoảng thời gian fireRate
        }
    }

    bool CheckShootingRange()
    {
        Transform player = enemyMovement.GetPlayerTransform();
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            return distanceToPlayer <= shootingRange;
        }
        return false;
    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null || enemyMovement == null)
        {
            return;
        }
        
        // Tạo đạn tại vị trí firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Xác định hướng bắn
        Transform player = enemyMovement.GetPlayerTransform();
        if (player != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * fireSpeed; // Tốc độ đạn (có thể chỉnh sửa)
            }
        }
        Destroy(bullet, bulletLifetime); // Hủy đạn sau một thời gian nhất định
    }

    void OnDrawGizmosSelected()
    {
        // Hiển thị khoảng cách bắn trong Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }

    // Public methods để các script khác có thể truy cập
    public bool IsInShootingRange()
    {
        Transform player = enemyMovement?.GetPlayerTransform();
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            return distanceToPlayer <= shootingRange;
        }
        return false;
    }

    public float GetShootingRange()
    {
        return shootingRange;
    }

    public float GetFireRate()
    {
        return fireRate;
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
    
    // Đảm bảo skeleton có thể di chuyển lại khi script bị tắt
    void OnDisable()
    {
        if (enemyMovement != null)
        {
            enemyMovement.SetStopForShooting(false);
        }
    }
    
    void OnDestroy()
    {
        if (enemyMovement != null)
        {
            enemyMovement.SetStopForShooting(false);
        }
    }
}
