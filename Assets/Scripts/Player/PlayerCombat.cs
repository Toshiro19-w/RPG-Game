using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerCombat : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDistance = 5f;
    [SerializeField] private float flashCooldown = 60f;
    [SerializeField] private KeyCode flashKey = KeyCode.F;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Melee Attack Settings")]
    [SerializeField] private GameObject meleeProjectilePrefab;
    [SerializeField] private float meleeCooldown = 0.5f;
    [SerializeField] private KeyCode meleeKey = KeyCode.Space;
    [SerializeField] private float meleeProjectileSpeed = 6f;
    
    [Header("Skill Settings")]
    [SerializeField] private GameObject iceArrowPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject fireWallPrefab;
    [SerializeField] private ProjectileData[] projectiles = new ProjectileData[4]
    {
        new() { speed = 8f, cooldown = 2f, key = KeyCode.E }, // Ice Arrow
        new() { speed = 10f, cooldown = 1f, key = KeyCode.Q }, // Fire Ball
        new() { speed = 9f, cooldown = 4f, key = KeyCode.R },  // Ice Barrage
        new() { speed = 0f, cooldown = 6f, key = KeyCode.T }   // Fire Wall
    };
    
    [Header("Ice Barrage Settings")]
    [SerializeField] private int iceBarrageCount = 7;
    [SerializeField] private float iceBarrageSpread = 45f;
    
    [Header("Fire Wall Settings")]
    [SerializeField] private int fireWallCount = 5;
    [SerializeField] private float fireWallSpacing = 1f;
    [SerializeField] private float fireWallDuration = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackAnimationDuration = 0.3f;

    private readonly Dictionary<KeyCode, float> cooldownTimers = new();
    private Camera mainCamera;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private bool isAttacking;

    public event Action<KeyCode, float> OnSkillUsed;

    [System.Serializable]
    public class ProjectileData
    {
        [HideInInspector] public GameObject prefab;
        public float speed;
        public float cooldown;
        public KeyCode key;
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (projectiles.Length >= 4)
        {
            projectiles[0].prefab = iceArrowPrefab;
            projectiles[1].prefab = fireballPrefab;
            projectiles[2].prefab = iceArrowPrefab;
            projectiles[3].prefab = fireWallPrefab;
        }
    }

    void Update()
    {
        UpdateCooldowns();
        HandleInputs();
    }
    
    private void UpdateCooldowns()
    {
        var keys = new List<KeyCode>(cooldownTimers.Keys);
        foreach (var key in keys)
        {
            cooldownTimers[key] -= Time.deltaTime;
            if (cooldownTimers[key] <= 0)
                cooldownTimers.Remove(key);
        }
    }
    
    private void HandleInputs()
    {
        if (isAttacking) return;

        if (Input.GetKeyDown(flashKey) && !cooldownTimers.ContainsKey(flashKey))
            Flash();
            

            
        if (Input.GetKeyDown(meleeKey) && !cooldownTimers.ContainsKey(meleeKey))
            MeleeAttack();
            
        foreach (var projectile in projectiles)
        {
            if (Input.GetKeyDown(projectile.key) && !cooldownTimers.ContainsKey(projectile.key))
                CastSkill(projectile);
        }
    }

    private void Flash()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 currentPosition = rb.position;
        Vector2 direction = (mousePosition - currentPosition).normalized;
        Vector2 targetPosition = currentPosition + (direction * flashDistance);

        RaycastHit2D hit = Physics2D.Raycast(currentPosition, direction, flashDistance, obstacleLayer);
        if (hit.collider != null)
            targetPosition = hit.point - (direction * 0.1f);

        if (Physics2D.OverlapCircle(targetPosition, 0.1f, obstacleLayer) != null)
            return;

        animator?.SetTrigger("Flash");
        rb.position = targetPosition;
        cooldownTimers[flashKey] = flashCooldown;
        OnSkillUsed?.Invoke(flashKey, flashCooldown);
    }

    private void MeleeAttack()
    {
        StartCoroutine(PerformAttack("MeleeAttack", meleeProjectilePrefab, meleeProjectileSpeed));
        cooldownTimers[meleeKey] = meleeCooldown;
        OnSkillUsed?.Invoke(meleeKey, meleeCooldown);
    }

    private void CastSkill(ProjectileData data)
    {
        string triggerName = data.key == KeyCode.E ? "CastIce" :
                           data.key == KeyCode.Q ? "CastFire" :
                           data.key == KeyCode.R ? "CastIceBarrage" : "CastFireWall";


        if (data.key == KeyCode.R)
            StartCoroutine(PerformIceBarrage(triggerName, data.prefab, data.speed));
        else if (data.key == KeyCode.T)
            StartCoroutine(PerformFireWall(triggerName, data.prefab));
        else
            StartCoroutine(PerformAttack(triggerName, data.prefab, data.speed));

        cooldownTimers[data.key] = data.cooldown;
        OnSkillUsed?.Invoke(data.key, data.cooldown);
    }

    private IEnumerator PerformAttack(string animationTrigger, GameObject projectilePrefab, float projectileSpeed)
    {
        isAttacking = true;
        playerMovement?.StopMovement();

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;

        animator?.SetFloat("AttackX", direction.x);
        animator?.SetFloat("AttackY", direction.y);
        animator?.SetBool("isAttacking", true);
        animator?.SetTrigger(animationTrigger);

        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, rb.position, Quaternion.identity);
            
            if (projectile.TryGetComponent<Rigidbody2D>(out var projectileRb))
                projectileRb.linearVelocity = direction * projectileSpeed;
                
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Set projectile type for special effects
            if (projectile.TryGetComponent<ProjectileScript>(out var projScript))
            {
                if (animationTrigger.Contains("Fire"))
                    projScript.SetProjectileType(ProjectileScript.ProjectileType.Fireball);
                else if (animationTrigger.Contains("Ice"))
                    projScript.SetProjectileType(ProjectileScript.ProjectileType.IceArrow);
            }
        }

        yield return new WaitForSeconds(attackAnimationDuration);
        
        isAttacking = false;
        animator?.SetBool("isAttacking", false);
        animator?.SetFloat("AttackX", 0);
        animator?.SetFloat("AttackY", 0);
        playerMovement?.ResumeMovement();
    }

    private IEnumerator PerformIceBarrage(string animationTrigger, GameObject projectilePrefab, float projectileSpeed)
    {
        isAttacking = true;
        playerMovement?.StopMovement();

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;

        animator?.SetFloat("AttackX", direction.x);
        animator?.SetFloat("AttackY", direction.y);
        animator?.SetBool("isAttacking", true);
        animator?.SetTrigger(animationTrigger);

        if (projectilePrefab != null)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - iceBarrageSpread / 2f;
            float angleStep = iceBarrageSpread / (iceBarrageCount - 1);

            for (int i = 0; i < iceBarrageCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 shootDirection = new Vector2(
                    Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                    Mathf.Sin(currentAngle * Mathf.Deg2Rad)
                );

                GameObject projectile = Instantiate(projectilePrefab, rb.position, Quaternion.identity);
                
                if (projectile.TryGetComponent<Rigidbody2D>(out var projectileRb))
                    projectileRb.linearVelocity = shootDirection * projectileSpeed;
                    
                projectile.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
                
                // Set ice arrow type for slow effect
                if (projectile.TryGetComponent<ProjectileScript>(out var projScript))
                    projScript.SetProjectileType(ProjectileScript.ProjectileType.IceArrow);
            }
        }

        yield return new WaitForSeconds(attackAnimationDuration);
        
        isAttacking = false;
        animator?.SetBool("isAttacking", false);
        animator?.SetFloat("AttackX", 0);
        animator?.SetFloat("AttackY", 0);
        playerMovement?.ResumeMovement();
    }

    private IEnumerator PerformFireWall(string animationTrigger, GameObject fireWallPrefab)
    {
        isAttacking = true;
        playerMovement?.StopMovement();

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        animator?.SetFloat("AttackX", direction.x);
        animator?.SetFloat("AttackY", direction.y);
        animator?.SetBool("isAttacking", true);
        animator?.SetTrigger(animationTrigger);

        if (fireWallPrefab != null)
        {
            Vector2 startPos = mousePosition - (perpendicular * fireWallSpacing * (fireWallCount - 1) / 2f);
            
            for (int i = 0; i < fireWallCount; i++)
            {
                Vector2 spawnPos = startPos + (perpendicular * fireWallSpacing * i);
                GameObject fireWall = Instantiate(fireWallPrefab, spawnPos, Quaternion.identity);
                
                // Set fire wall type for slow effect
                if (fireWall.TryGetComponent<ProjectileScript>(out var projScript))
                    projScript.SetProjectileType(ProjectileScript.ProjectileType.FireWall);
                    
                Destroy(fireWall, fireWallDuration);
            }
        }

        yield return new WaitForSeconds(attackAnimationDuration);
        
        isAttacking = false;
        animator?.SetBool("isAttacking", false);
        animator?.SetFloat("AttackX", 0);
        animator?.SetFloat("AttackY", 0);
        playerMovement?.ResumeMovement();
    }

    public float GetCooldownForSkill(KeyCode key)
    {
        // Kiểm tra các kỹ năng trong mảng projectiles trước
        foreach (var projectile in projectiles)
        {
            if (projectile.key == key)
            {
                return projectile.cooldown;
            }
        }

        // Kiểm tra các kỹ năng đặc biệt khác
        if (key == flashKey)
        {
            return flashCooldown;
        }

        if (key == meleeKey)
        {
            return meleeCooldown;
        }

        // Nếu không tìm thấy, trả về 0
        return 0f;
    }

    public bool IsAttacking() => isAttacking;
}