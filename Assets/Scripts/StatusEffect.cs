using UnityEngine;
using System.Collections;

public class StatusEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float originalSpeed;
    private bool isSlowed;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }
    
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
    }
    
    public void ApplySlow(float slowPercent, float duration)
    {
        if (!isSlowed)
            StartCoroutine(SlowEffect(slowPercent, duration));
    }
    
    private IEnumerator SlowEffect(float slowPercent, float duration)
    {
        isSlowed = true;
        
        // Change color to light blue
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.7f, 0.9f, 1f, 1f);
        
        // Slow movement if has movement component
        if (TryGetComponent<EnemyMovement>(out var enemyMovement))
        {
            originalSpeed = enemyMovement.GetSpeed();
            enemyMovement.SetSpeed(originalSpeed * (1f - slowPercent));
        }
        
        yield return new WaitForSeconds(duration);
        
        // Restore original state
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
            
        if (TryGetComponent<EnemyMovement>(out var movement))
            movement.SetSpeed(originalSpeed);
        
        isSlowed = false;
    }
}