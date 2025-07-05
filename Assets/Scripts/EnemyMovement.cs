using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float detectionRange = 5f; // Added for player detection
    [SerializeField] float stoppingDistance = 1f; // Added for attack range
    Rigidbody2D myRigidbody;
    Animator animator;
    bool hasSpottedPlayer = false; // Kiểm tra xem đã phát hiện người chơi hay chưa

    Transform player;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stoppingDistance)
        {
            // Di chuyển về phía người chơi
            MoveTowardsPlayer();
            hasSpottedPlayer = true; // Đánh dấu là đã phát hiện người chơi
        }
        else
        {
            animator.SetBool("isMoving", false);
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
        // Bật animation di chuyển
        animator.SetBool("isMoving", true);

        // Xác định hướng di chuyển
        Vector3 direction = (player.position - transform.position).normalized;

        // Xoay Enemy để hướng về phía người chơi
        if (direction.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            Flip();
        }

        // Di chuyển về phía người chơi
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

    public bool HasSpottedPlayer()
    {
        return hasSpottedPlayer; // Trả về trạng thái phát hiện người chơi
    }
    
    public Transform GetPlayerTransform()
    {
        return player; // Trả về vị trí của người chơi
    }

}