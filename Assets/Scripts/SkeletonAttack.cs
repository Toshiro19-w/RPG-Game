using System.Collections;
using UnityEngine;

public class SkeletonAttack : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint; // Vị trí bắn đạn
    [SerializeField] float fireRate = 1f; // Tốc độ bắn (giây)
    [SerializeField] float fireSpeed = 5f; // Tốc độ đạn
    [SerializeField] float bulletLifetime = 5f; // Thời gian sống của đạn
    EnemyMovement enemyMovement; // Tham chiếu đến EnemyMovement
    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        // Bắt đầu kiểm tra và bắn đạn
        StartCoroutine(CheckAndShoot());
    }

    IEnumerator CheckAndShoot()
    {
        while (true)
        {
            if (enemyMovement != null && enemyMovement.HasSpottedPlayer())
            {
                Shoot();
            }
            yield return new WaitForSeconds(fireRate); // Lặp lại sau mỗi khoảng thời gian fireRate
        }
    }

    void Shoot()
    {
        if (firePoint != null && bulletPrefab != null)
        {
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
    }
}
