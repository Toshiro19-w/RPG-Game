using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject damageIndicatorPrefab;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private int currentHealth;
    private bool isInvincible;
    
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer ??= GetComponent<SpriteRenderer>();
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Show damage indicator
        ShowDamageIndicator(damage, Color.red);
        
        // Start invincibility
        StartCoroutine(InvincibilityFrames());
        
        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Show heal indicator
        ShowDamageIndicator(healAmount, Color.green);
    }
    
    private void ShowDamageIndicator(int amount, Color color)
    {
        if (damageIndicatorPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject indicator = Instantiate(damageIndicatorPrefab, spawnPos, Quaternion.identity);
            
            if (indicator.TryGetComponent<DamageIndicator>(out var damageComp))
                damageComp.Initialize(amount, color);
        }
    }
    
    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;
        
        // Flashing effect
        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
            
            elapsed += 0.2f;
        }
        
        isInvincible = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }
    
    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, transform.rotation);
            
        Debug.Log("Player Died!");
        gameObject.SetActive(false);
    }
}