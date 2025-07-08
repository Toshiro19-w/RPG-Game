using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask enemyLayer = -1;
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private bool piercing = false;
    
    [Header("Visual Effects")]
    [SerializeField] private bool rotateWhileFlying = true;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool pulseEffect = false;
    [SerializeField] private float pulseSpeed = 2f;
    
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        if (rotateWhileFlying)
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
        if (pulseEffect)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
            transform.localScale = originalScale * pulse;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit enemy
        if (IsInLayerMask(other.gameObject, enemyLayer))
        {
            DealDamage(other);
            if (!piercing) DestroyProjectile();
        }
        // Check if hit obstacle
        else if (IsInLayerMask(other.gameObject, obstacleLayer))
        {
            DestroyProjectile();
        }
    }
    
    private void DealDamage(Collider2D target)
    {
        // Try different damage interfaces
        if (target.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage);
        else if (target.TryGetComponent<EnemyHealth>(out var enemyHealth))
            enemyHealth.TakeDamage(damage);
        else
            Destroy(target.gameObject); // Fallback
    }
    
    private void DestroyProjectile()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, transform.rotation);
            
        Destroy(gameObject);
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }
}

// Interface for damage system
public interface IDamageable
{
    void TakeDamage(int damage);
}