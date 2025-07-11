using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float detectionRange = 5f; // Added for player detection
    [SerializeField] float stoppingDistance = 1f; // Added for attack range
    
    [Header("Wall Detection")]
    [SerializeField] LayerMask wallLayerMask = -1; // Layer của tường
    [SerializeField] float wallCheckDistance = 0.5f; // Khoảng cách check tường
    [SerializeField] Transform wallCheckPoint; // Điểm kiểm tra tường (có thể null, sẽ dùng transform.position)
    [SerializeField] bool enableWallFollowing = true; // Bật tính năng đi theo tường
    [SerializeField] float wallFollowDistance = 0.3f; // Khoảng cách đi theo tường
    
    private bool isFollowingWall = false; // Đang đi theo tường
    private Vector3 lastWallDirection = Vector3.zero; // Hướng tường cuối cùng
    private float wallFollowTimer = 0f; // Timer để thoát khỏi chế độ wall follow
    private const float MAX_WALL_FOLLOW_TIME = 3f; // Thời gian tối đa đi theo tường
    
    Rigidbody2D myRigidbody;
    Animator animator;
    bool hasSpottedPlayer = false; // Kiểm tra xem đã phát hiện người chơi hay chưa

    Transform player;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Tìm player an toàn
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy GameObject với tag 'Player'!");
        }
    }
    void Update()
    {
        // Kiểm tra null trước khi sử dụng
        if (player == null || animator == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Cập nhật wall follow timer
        if (isFollowingWall)
        {
            wallFollowTimer += Time.deltaTime;
            if (wallFollowTimer > MAX_WALL_FOLLOW_TIME)
            {
                // Thoát khỏi chế độ wall follow sau một thời gian
                isFollowingWall = false;
                wallFollowTimer = 0f;
            }
        }
        
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stoppingDistance)
        {
            // Di chuyển về phía người chơi
            MoveTowardsPlayer();
            hasSpottedPlayer = true; // Đánh dấu là đã phát hiện người chơi
        }
        else
        {
            animator.SetBool("isMoving", false);
            isFollowingWall = false; // Dừng wall following khi không chase player
            wallFollowTimer = 0f;
        }

        if (hasSpottedPlayer)
        {
            // Chỉ di chuyển nếu chưa đến quá gần người chơi
            if (distanceToPlayer > stoppingDistance)
            {
                MoveTowardsPlayer();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // Kiểm tra null trước khi sử dụng
        if (player == null || animator == null) return;
        
        // Bật animation di chuyển
        animator.SetBool("isMoving", true);

        // Xác định hướng di chuyển cơ bản
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Xoay Enemy để hướng về phía người chơi
        if (directionToPlayer.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (directionToPlayer.x < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
        
        // Tính toán hướng di chuyển dựa trên pathfinding
        Vector3 moveDirection = GetOptimalMoveDirection(directionToPlayer);
        
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
            transform.position = newPosition;
        }
    }

    // Tính toán hướng di chuyển tối ưu với pathfinding thông minh
    private Vector3 GetOptimalMoveDirection(Vector3 directionToPlayer)
    {
        // Nếu đang trong chế độ wall following
        if (isFollowingWall && enableWallFollowing)
        {
            Vector3 wallFollowDirection = GetWallFollowDirection();
            if (wallFollowDirection != Vector3.zero)
            {
                // Kiểm tra xem có thể quay lại hướng player không
                if (!CheckWallCollision(directionToPlayer))
                {
                    // Có thể đi thẳng về player, thoát khỏi wall following
                    isFollowingWall = false;
                    wallFollowTimer = 0f;
                    return directionToPlayer;
                }
                return wallFollowDirection;
            }
        }
        
        // Kiểm tra có thể đi thẳng về player không
        if (!CheckWallCollision(directionToPlayer))
        {
            isFollowingWall = false;
            return directionToPlayer;
        }
        
        // Gặp tường, tìm đường thay thế
        Vector3 alternativeDirection = FindBestAlternativeDirection(directionToPlayer);
        
        if (alternativeDirection != Vector3.zero)
        {
            // Bắt đầu wall following nếu cần
            if (enableWallFollowing && !isFollowingWall)
            {
                isFollowingWall = true;
                wallFollowTimer = 0f;
                lastWallDirection = alternativeDirection;
            }
            return alternativeDirection;
        }
        
        return Vector3.zero;
    }
    
    // Tìm hướng đi theo tường
    private Vector3 GetWallFollowDirection()
    {
        if (lastWallDirection == Vector3.zero) return Vector3.zero;
        
        // Thử tiếp tục theo hướng wall hiện tại
        if (!CheckWallCollision(lastWallDirection))
        {
            return lastWallDirection;
        }
        
        // Nếu không thể tiếp tục, tìm hướng mới
        Vector3 newDirection = FindBestAlternativeDirection(lastWallDirection);
        if (newDirection != Vector3.zero)
        {
            lastWallDirection = newDirection;
            return newDirection;
        }
        
        // Không tìm thấy hướng, thoát wall following
        isFollowingWall = false;
        return Vector3.zero;
    }
    
    // Tìm hướng thay thế tốt nhất
    private Vector3 FindBestAlternativeDirection(Vector3 originalDirection)
    {
        // Danh sách các hướng ưu tiên (theo độ gần với hướng gốc)
        Vector3[] directions = {
            // Các hướng lệch 45 độ (ưu tiên cao)
            RotateVector2D(originalDirection, 45f),
            RotateVector2D(originalDirection, -45f),
            
            // Các hướng vuông góc (ưu tiên trung bình)
            RotateVector2D(originalDirection, 90f),
            RotateVector2D(originalDirection, -90f),
            
            // Chỉ di chuyển theo một trục (backup)
            new Vector3(originalDirection.x, 0, 0).normalized,
            new Vector3(0, originalDirection.y, 0).normalized,
            
            // Hướng ngược lại (ưu tiên thấp)
            RotateVector2D(originalDirection, 135f),
            RotateVector2D(originalDirection, -135f)
        };
        
        foreach (Vector3 direction in directions)
        {
            if (direction != Vector3.zero && !CheckWallCollision(direction))
            {
                return direction;
            }
        }
        
        return Vector3.zero;
    }
    
    // Xoay vector 2D theo góc
    private Vector3 RotateVector2D(Vector3 vector, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);
        
        return new Vector3(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos,
            0
        );
    }

    // Kiểm tra có va chạm với tường không (đơn giản)
    private bool CheckWallCollision(Vector3 direction)
    {
        Vector3 checkPosition = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, direction, wallCheckDistance, wallLayerMask);
        return hit.collider != null;
    }
    
    // Tìm hướng di chuyển thay thế đơn giản
    private Vector3 GetAlternativeDirection(Vector3 originalDirection)
    {
        // Thử các hướng xung quanh (đơn giản hóa)
        Vector3[] alternatives = {
            new Vector3(originalDirection.x, 0, 0).normalized,  // Chỉ di chuyển theo X
            new Vector3(0, originalDirection.y, 0).normalized,  // Chỉ di chuyển theo Y
            new Vector3(-originalDirection.y, originalDirection.x, 0).normalized,  // Vuông góc trái
            new Vector3(originalDirection.y, -originalDirection.x, 0).normalized   // Vuông góc phải
        };
        
        foreach (Vector3 direction in alternatives)
        {
            if (!CheckWallCollision(direction))
            {
                return direction;
            }
        }
        
        return Vector3.zero; // Không tìm thấy hướng nào
    }

    void Flip()
    {
        // Đảo ngược hướng của Enemy
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
    void OnDrawGizmosSelected()
    {
        // Hiển thị detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Hiển thị stopping distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        
        // Hiển thị wall check distance nếu có player
        if (player != null && hasSpottedPlayer)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Hiển thị hướng đến player
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, directionToPlayer * wallCheckDistance);
            
            // Hiển thị trạng thái wall following
            if (isFollowingWall)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 0.8f, Vector3.one * 0.3f);
                
                // Hiển thị hướng wall follow hiện tại
                if (lastWallDirection != Vector3.zero)
                {
                    Gizmos.color = Color.orange;
                    Gizmos.DrawRay(transform.position, lastWallDirection * wallCheckDistance);
                }
            }
            
            // Hiển thị các hướng thay thế có thể
            Gizmos.color = Color.cyan;
            Vector3[] testDirections = {
                RotateVector2D(directionToPlayer, 45f),
                RotateVector2D(directionToPlayer, -45f),
                RotateVector2D(directionToPlayer, 90f),
                RotateVector2D(directionToPlayer, -90f)
            };
            
            foreach (Vector3 dir in testDirections)
            {
                if (!CheckWallCollision(dir))
                {
                    Gizmos.color = Color.green; // Hướng có thể đi
                }
                else
                {
                    Gizmos.color = Color.red; // Hướng bị chặn
                }
                Gizmos.DrawRay(transform.position, dir * wallCheckDistance * 0.5f);
            }
        }
        
        // Hiển thị wall check point nếu có
        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wallCheckPoint.position, 0.1f);
        }
    }

    public bool HasSpottedPlayer()
    {
        return hasSpottedPlayer; // Trả về trạng thái phát hiện người chơi
    }

    public Transform GetPlayerTransform()
    {
        return player; // Trả về vị trí của người chơi
    }

    public float GetSpeed()
    {
        return moveSpeed; // Trả về tốc độ di chuyển của Enemy
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed; // Cập nhật tốc độ di chuyển của Enemy
    }

}