using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerCombat playerCombat;
    private bool canMove = true;

    public float getSpeed()
    {
        return speed;
    }

    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void StopMovement()
    {
        canMove = false;
        movement = Vector2.zero;
        animator.SetBool("isMoving", false);
    }

    public void ResumeMovement()
    {
        canMove = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerCombat = GetComponent<PlayerCombat>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (!canMove || (playerCombat != null && playerCombat.IsAttacking()))
        {
            movement = Vector2.zero;
        }
        else
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            if (movement.x != 0 && movement.y != 0)
                movement *= 0.7071f;
        }

        Animate();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + speed * Time.fixedDeltaTime * movement);
    }

    void Animate()
    {
        bool isMoving = movement.sqrMagnitude > 0;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
        }
    }
}
