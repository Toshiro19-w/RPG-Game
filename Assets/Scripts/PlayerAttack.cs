using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public Camera mainCamera;
    public float attackInterval = 0.5f;
    public float delayAttackTime = 0.2f; // Thời gian delay trước khi tấn công
    public float attackSpeed = 5f; // Tốc độ tấn công, có thể dùng để điều chỉnh tốc độ của animation

    private float attackTimer = 0f;
    private bool isDelaying = false;

    public PlayerMovement playerMovement;
    private float originalSpeed;
    private bool isAttacking = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        if (playerMovement != null)
        {
            originalSpeed = playerMovement.getSpeed();
        }
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        bool isAttackPressed = Input.GetMouseButton(0);

        // Ưu tiên trạng thái attack ngay khi nhấn tấn công
        if ((isAttackPressed && !isAttacking) || isAttacking)
        {
            
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);
            if (playerMovement != null)
                playerMovement.setSpeed(0f);
        }
        else
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isMoving", true);
            if (playerMovement != null)
                playerMovement.setSpeed(originalSpeed);
        }

        if (isAttackPressed && attackTimer >= attackInterval && !isDelaying && !isAttacking)
        {
            StartCoroutine(AttackWithDelay());
            attackTimer = 0f;
        }
    }


    private IEnumerator AttackWithDelay()
    {
        isDelaying = true;
        isAttacking = true;
        yield return new WaitForSeconds(delayAttackTime);
        Attack();
        isDelaying = false;
    }

    void Attack()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        Vector3 direction = mousePosition - transform.position;
        direction.z = 0;
        direction.Normalize();

        // Set blend tree parameters
        animator.SetFloat("AttackX", direction.x);
        animator.SetFloat("AttackY", direction.y);
        animator.SetBool("isAttacking", true);
    }

    // Hàm này sẽ được gọi từ Animation Event ở cuối animation attack
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        if (playerMovement != null)
            playerMovement.setSpeed(originalSpeed);
        animator.SetBool("isAttacking", false);
    }
}
