using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject damageIndicatorPrefab;
    [SerializeField] private float invincibilityDuration = 0.2f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private int currentHealth;
    private bool isInvincible;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer ??= GetComponent<SpriteRenderer>();
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        
        currentHealth -= damage;
        
        // Show damage indicator
        ShowDamageIndicator(damage, Color.yellow);
        
        // Brief invincibility
        StartCoroutine(InvincibilityFrames());
        
        if (currentHealth <= 0)
            Die();
    }
    
    private void ShowDamageIndicator(int damage, Color color)
    {
        if (damageIndicatorPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject indicator = Instantiate(damageIndicatorPrefab, spawnPos, Quaternion.identity);
            
            if (indicator.TryGetComponent<DamageIndicator>(out var damageComp))
                damageComp.Initialize(damage, color);
        }
    }
    
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
            
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvincible = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }
    
    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, transform.rotation);
            
        // Spawn rewards before destroying
        if (RewardSystem.Instance != null)
        {
            RewardSystem.Instance.SpawnRewards(transform.position);
        }
        
        Destroy(gameObject);
    }
}