using UnityEngine;

namespace InfinityMap
{
    public class InfinityEnemyMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float stoppingDistance = 1f; // Khoảng cách dừng lại để tấn công
        
        [Header("Wall Detection")]
        [SerializeField] private LayerMask wallLayerMask = -1;
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private bool enableWallFollowing = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Wall following variables
        private bool isFollowingWall = false;
        private Vector3 lastWallDirection = Vector3.zero;
        private float wallFollowTimer = 0f;
        private const float MAX_WALL_FOLLOW_TIME = 3f;
        
        // Attack control - tương thích với SkeletonAttack và SlimeAttack
        private bool shouldStopForShooting = false; // Cho SkeletonAttack
        
        // Components
        private Rigidbody2D myRigidbody;
        private Animator animator;
        private Transform player;
        
        // State
        private bool isMoving = false;
        
        void Start()
        {
            InitializeComponents();
            FindPlayer();
            
            // Nếu không tìm thấy player ngay lập tức, thử lại sau 0.5 giây
            if (player == null)
            {
                Invoke(nameof(DelayedPlayerSearch), 0.5f);
            }
        }
        
        private void DelayedPlayerSearch()
        {
            if (player == null)
            {
                FindPlayer();
                if (player == null)
                {
                    // Thử lại sau 1 giây nữa
                    Invoke(nameof(DelayedPlayerSearch), 1f);
                }
            }
        }
        
        private void InitializeComponents()
        {
            myRigidbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            
            // Đảm bảo Rigidbody2D được cấu hình đúng để movement hoạt động
            if (myRigidbody != null)
            {
                myRigidbody.gravityScale = 0f; // Tắt gravity cho 2D top-down
                myRigidbody.freezeRotation = true; // Không cho phép xoay
            }
        }
        
        private void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"Enemy {gameObject.name} không tìm thấy Player với tag 'Player'! Sẽ thử lại...");
            }
        }
        
        void Update()
        {
            if (player == null) 
            {
                FindPlayer(); // Thử tìm lại player
                return;
            }
            
            // Debug để kiểm tra xem có đang chạy logic movement không
            if (enableDebugLogs && Time.frameCount % 60 == 0) // Log mỗi giây (60 fps)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            }
            
            HandleMovement();
            UpdateWallFollowTimer();
        }
        
        private void HandleMovement()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Debug: In ra thông tin về khoảng cách và điều kiện
            bool shouldMove = distanceToPlayer > stoppingDistance && !shouldStopForShooting;
            
            // Luôn luôn di chuyển về phía player nếu không bị dừng để bắn
            if (shouldMove)
            {
                MoveTowardsPlayer();
                isMoving = true;
            }
            else
            {
                StopMoving();
                isMoving = false;
            }
            
            // Update animator
            if (animator != null)
            {
                animator.SetBool("isMoving", isMoving);
            }
        }
        
        private void UpdateWallFollowTimer()
        {
            if (isFollowingWall)
            {
                wallFollowTimer += Time.deltaTime;
                if (wallFollowTimer > MAX_WALL_FOLLOW_TIME)
                {
                    isFollowingWall = false;
                    wallFollowTimer = 0f;
                }
            }
        }
        
        private void MoveTowardsPlayer()
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Flip enemy to face player
            FlipTowardsDirection(directionToPlayer);
            
            // Calculate optimal move direction with pathfinding
            Vector3 moveDirection = GetOptimalMoveDirection(directionToPlayer);
            
            if (moveDirection != Vector3.zero)
            {
                Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
                transform.position = newPosition;
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogWarning($"Enemy {gameObject.name} không thể tìm được hướng di chuyển hợp lệ!");
            }
        }
        
        private void StopMoving()
        {
            // Enemy stops moving but can still face the player
            if (player != null)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                FlipTowardsDirection(directionToPlayer);
            }
        }
        
        private void FlipTowardsDirection(Vector3 direction)
        {
            if (direction.x > 0 && transform.localScale.x < 0)
            {
                Flip();
            }
            else if (direction.x < 0 && transform.localScale.x > 0)
            {
                Flip();
            }
        }
        
        private Vector3 GetOptimalMoveDirection(Vector3 directionToPlayer)
        {
            // Wall following logic
            if (isFollowingWall && enableWallFollowing)
            {
                Vector3 wallFollowDirection = GetWallFollowDirection();
                if (wallFollowDirection != Vector3.zero)
                {
                    // Check if can go directly to player
                    if (!CheckWallCollision(directionToPlayer))
                    {
                        isFollowingWall = false;
                        wallFollowTimer = 0f;
                        return directionToPlayer;
                    }
                    return wallFollowDirection;
                }
            }
            
            // Try direct path to player first
            if (!CheckWallCollision(directionToPlayer))
            {
                isFollowingWall = false;
                return directionToPlayer;
            }
            
            // Find alternative direction
            Vector3 alternativeDirection = FindBestAlternativeDirection(directionToPlayer);
            
            if (alternativeDirection != Vector3.zero)
            {
                if (enableWallFollowing && !isFollowingWall)
                {
                    isFollowingWall = true;
                    wallFollowTimer = 0f;
                    lastWallDirection = alternativeDirection;
                }
                return alternativeDirection;
            }
            
            // Fallback: Nếu không thể tìm được hướng nào, vẫn di chuyển thẳng về player
            // Điều này đảm bảo enemy không bao giờ "stuck" hoàn toàn
            if (enableDebugLogs)
                Debug.LogWarning($"Enemy {gameObject.name} không thể tìm hướng di chuyển, sử dụng fallback - di chuyển thẳng về player");
            return directionToPlayer;
        }
        
        private Vector3 GetWallFollowDirection()
        {
            if (lastWallDirection == Vector3.zero) return Vector3.zero;
            
            // Continue current wall direction
            if (!CheckWallCollision(lastWallDirection))
            {
                return lastWallDirection;
            }
            
            // Find new direction
            Vector3 newDirection = FindBestAlternativeDirection(lastWallDirection);
            if (newDirection != Vector3.zero)
            {
                lastWallDirection = newDirection;
                return newDirection;
            }
            
            // Exit wall following
            isFollowingWall = false;
            return Vector3.zero;
        }
        
        private Vector3 FindBestAlternativeDirection(Vector3 originalDirection)
        {
            Vector3[] directions = {
                RotateVector2D(originalDirection, 45f),
                RotateVector2D(originalDirection, -45f),
                RotateVector2D(originalDirection, 90f),
                RotateVector2D(originalDirection, -90f),
                new Vector3(originalDirection.x, 0, 0).normalized,
                new Vector3(0, originalDirection.y, 0).normalized,
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
        
        private bool CheckWallCollision(Vector3 direction)
        {
            Vector3 checkPosition = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(checkPosition, direction, wallCheckDistance, wallLayerMask);
            return hit.collider != null;
        }
        
        private void Flip()
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        
        // Public methods for external control - tương thích với scripts cũ
        public void SetStopForShooting(bool shouldStop)
        {
            shouldStopForShooting = shouldStop;
        }
        
        public bool IsStoppedForShooting()
        {
            return shouldStopForShooting;
        }
        
        // Alias methods để tương thích với scripts mới nếu cần
        public void SetStopForAttack(bool shouldStop)
        {
            shouldStopForShooting = shouldStop;
        }
        
        public bool IsStoppedForAttack()
        {
            return shouldStopForShooting;
        }
        
        public float GetSpeed()
        {
            return moveSpeed;
        }
        
        public void SetSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
        }
        
        public Transform GetPlayerTransform()
        {
            return player;
        }
        
        public bool IsMovingTowardsPlayer()
        {
            return isMoving;
        }
        
        void OnDrawGizmosSelected()
        {
            if (player == null) return;
            
            // Draw stopping distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, stoppingDistance);
            
            // Draw line to player
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);
            
            // Draw wall check distance
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, directionToPlayer * wallCheckDistance);
            
            // Draw wall following state
            if (isFollowingWall)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 0.8f, Vector3.one * 0.3f);
                
                if (lastWallDirection != Vector3.zero)
                {
                    Gizmos.color = Color.orange;
                    Gizmos.DrawRay(transform.position, lastWallDirection * wallCheckDistance);
                }
            }
        }
    }
}
