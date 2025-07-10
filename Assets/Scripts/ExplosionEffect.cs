using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 15;
    [SerializeField] private LayerMask enemyLayer = -1;
    [SerializeField] private float lifetime = 1f;
    
    [Header("Visual Effects")]
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private ParticleSystem explosionParticles;
    private Vector3 targetScale;
    
    void Start()
    {
        targetScale = Vector3.one * maxScale;
        
        // Play particle effect
        if (explosionParticles != null)
            explosionParticles.Play();
        
        // Deal damage to enemies in radius
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(explosionDamage);
        }
        
        // Auto destroy
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Scale up animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}