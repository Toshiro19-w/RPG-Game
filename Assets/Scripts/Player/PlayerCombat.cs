// PlayerCombat.cs (Phiên bản hoàn chỉnh với hệ thống nâng cấp)

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
    [SerializeField]
    private ProjectileData[] projectiles = new ProjectileData[4]
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

    // --- CÁC BIẾN NÂNG CẤP ĐÃ ĐƯỢC THÊM ---
    private readonly Dictionary<KeyCode, float> cooldownTimers = new();
    private readonly Dictionary<KeyCode, bool> skillUpgraded = new();
    private readonly Dictionary<KeyCode, int> skillUpgradeCosts = new();
    private PlayerWallet playerWallet;

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
        playerWallet = GetComponent<PlayerWallet>(); // Lấy ví tiền

        if (projectiles.Length >= 4)
        {
            projectiles[0].prefab = iceArrowPrefab;
            projectiles[1].prefab = fireballPrefab;
            projectiles[2].prefab = iceArrowPrefab;
            projectiles[3].prefab = fireWallPrefab;
        }
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        // Khởi tạo trạng thái chưa nâng cấp
        foreach (var projectile in projectiles)
        {
            skillUpgraded[projectile.key] = false;
        }
        skillUpgraded[meleeKey] = false;
        skillUpgraded[flashKey] = false;

        // ĐẶT GIÁ NÂNG CẤP Ở ĐÂY
        skillUpgradeCosts[KeyCode.Q] = 10; // Cầu lửa
        skillUpgradeCosts[KeyCode.E] = 10; // Băng tiễn
        skillUpgradeCosts[KeyCode.R] = 20; // Bão băng
        skillUpgradeCosts[KeyCode.T] = 15; // Tường lửa
        skillUpgradeCosts[KeyCode.Space] = 5;  // Đánh thường
        skillUpgradeCosts[KeyCode.F] = 50;  // Lướt
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
        // ... (phần code di chuyển giữ nguyên)
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
        AudioManager.Instance.Play("Teleport");
        rb.position = targetPosition;
        
        // --- THAY ĐỔI --- Logic giảm hồi chiêu khi nâng cấp
        float finalFlashCooldown = IsSkillUpgraded(flashKey) ? 30f : flashCooldown;
        cooldownTimers[flashKey] = finalFlashCooldown;
        OnSkillUsed?.Invoke(flashKey, finalFlashCooldown); // Báo cho UI cooldown đúng
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

    // Đặt vào trong file PlayerCombat.cs, thay thế hàm PerformAttack cũ

private IEnumerator PerformAttack(string animationTrigger, GameObject projectilePrefab, float projectileSpeed)
{
    // 1. Đặt trạng thái "đang tấn công" và dừng di chuyển của người chơi
    isAttacking = true;
    playerMovement?.StopMovement(); // Dừng di chuyển ngay lập tức

    // 2. Cài đặt các thông số cho Animator
    Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    Vector2 direction = (mousePosition - rb.position).normalized;

    animator?.SetFloat("AttackX", direction.x);
    animator?.SetFloat("AttackY", direction.y);
    animator?.SetBool("isAttacking", true);
    animator?.SetTrigger(animationTrigger);

    // 3. Phát âm thanh và tạo viên đạn (nếu có)
    if (projectilePrefab != null)
    {
        // Kiểm tra loại chiêu để phát âm thanh tương ứng
        if (animationTrigger.Contains("Fire")) 
        {
            AudioManager.Instance.Play("fire_ball");
        }
        else if (animationTrigger.Contains("Ice")) 
        {
            AudioManager.Instance.Play("arrow");
        }
        else if (animationTrigger.Contains("Melee")) // Thêm âm thanh cho đòn đánh thường nếu có
        {
            // AudioManager.Instance.Play("MeleeSound"); // Ví dụ
        }
        
        // Tạo viên đạn
        GameObject projectile = Instantiate(projectilePrefab, rb.position, Quaternion.identity);
        if (projectile.TryGetComponent<Rigidbody2D>(out var projectileRb))
            projectileRb.linearVelocity = direction * projectileSpeed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // 4. Kiểm tra nâng cấp để bắn lần thứ 2
    KeyCode currentKey = GetKeyFromAnimationTrigger(animationTrigger);
    if (currentKey != KeyCode.None && IsSkillUpgraded(currentKey))
    {
        StartCoroutine(DelayedAttack(0.2f, projectilePrefab, projectileSpeed));
    }

    // 5. Chờ cho animation tấn công kết thúc
    yield return new WaitForSeconds(attackAnimationDuration);

    // 6. Reset lại trạng thái và cho phép người chơi di chuyển trở lại
    isAttacking = false;
    animator?.SetBool("isAttacking", false);
    animator?.SetFloat("AttackX", 0);
    animator?.SetFloat("AttackY", 0);
    playerMovement?.ResumeMovement(); // Cho phép di chuyển trở lại
}

    private IEnumerator PerformIceBarrage(string animationTrigger, GameObject projectilePrefab, float projectileSpeed)
    {
        isAttacking = true;
        playerMovement?.StopMovement();
        // ... (phần code đầu hàm giữ nguyên) ...
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;
        animator?.SetFloat("AttackX", direction.x);
        animator?.SetFloat("AttackY", direction.y);
        animator?.SetBool("isAttacking", true);
        animator?.SetTrigger(animationTrigger);
        AudioManager.Instance.Play("arrows");

        if (projectilePrefab != null)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - iceBarrageSpread / 2f;
            float angleStep = iceBarrageSpread / (iceBarrageCount - 1);
            for (int i = 0; i < iceBarrageCount; i++)
            {
                // ... (code tạo đạn giữ nguyên) ...
                float currentAngle = startAngle + (angleStep * i);
                Vector2 shootDirection = new Vector2(
                    Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                    Mathf.Sin(currentAngle * Mathf.Deg2Rad)
                );
                GameObject projectile = Instantiate(projectilePrefab, rb.position, Quaternion.identity);
                if (projectile.TryGetComponent<Rigidbody2D>(out var projectileRb))
                    projectileRb.linearVelocity = shootDirection * projectileSpeed;
                projectile.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            }
        }
        
        // --- THÊM MỚI --- Kiểm tra nâng cấp để bắn lần thứ 2
        if (IsSkillUpgraded(KeyCode.R))
        {
            StartCoroutine(DelayedIceBarrage(0.3f, projectilePrefab, projectileSpeed));
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
        // ... (phần code đầu hàm giữ nguyên) ...
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        animator?.SetFloat("AttackX", direction.x);
        animator?.SetFloat("AttackY", direction.y);
        animator?.SetBool("isAttacking", true);
        animator?.SetTrigger(animationTrigger);
        AudioManager.Instance.Play("fire");

        if (fireWallPrefab != null)
        {
            // --- THAY ĐỔI --- Tăng số lượng tường lửa khi nâng cấp
            int finalWallCount = IsSkillUpgraded(KeyCode.T) ? fireWallCount * 2 : fireWallCount;
            Vector2 startPos = mousePosition - (perpendicular * fireWallSpacing * (finalWallCount - 1) / 2f);

            for (int i = 0; i < finalWallCount; i++)
            {
                // ... (code tạo tường lửa giữ nguyên) ...
                Vector2 spawnPos = startPos + (perpendicular * fireWallSpacing * i);
                GameObject fireWall = Instantiate(fireWallPrefab, spawnPos, Quaternion.identity);
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
    
    // --- HÀM CŨ ĐÃ SỬA ---
    public float GetCooldownForSkill(KeyCode key)
    {
        // --- THAY ĐỔI --- Trả về cooldown đúng sau khi nâng cấp
        if (key == flashKey)
        {
            return IsSkillUpgraded(key) ? 30f : flashCooldown;
        }

        if (key == meleeKey) return meleeCooldown;
        
        foreach (var projectile in projectiles)
        {
            if (projectile.key == key) return projectile.cooldown;
        }
        
        return 0f;
    }

    public bool IsAttacking() => isAttacking;
    
    // --- HÀM TRỢ GIÚP MỚI --- (Cho việc đánh lần 2)
    private IEnumerator DelayedAttack(float delay, GameObject prefab, float speed)
    {
        yield return new WaitForSeconds(delay);
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;
        GameObject projectile = Instantiate(prefab, rb.position, Quaternion.identity);
        if (projectile.TryGetComponent<Rigidbody2D>(out var projectileRb))
            projectileRb.linearVelocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private IEnumerator DelayedIceBarrage(float delay, GameObject projectilePrefab, float projectileSpeed)
    {
        yield return new WaitForSeconds(delay);
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - rb.position).normalized;
        if (projectilePrefab != null)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - iceBarrageSpread / 2f;
            float angleStep = iceBarrageSpread / (iceBarrageCount - 1);
            for (int i = 0; i < iceBarrageCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 shootDirection = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                GameObject projectile = Instantiate(projectilePrefab, rb.position, Quaternion.identity);
                if (projectile.TryGetComponent<Rigidbody2D>(out var projectileRb))
                    projectileRb.linearVelocity = shootDirection * projectileSpeed;
                projectile.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            }
        }
    }
    
    private KeyCode GetKeyFromAnimationTrigger(string trigger)
    {
        if (trigger.Contains("Fire")) return KeyCode.Q;
        if (trigger.Contains("Ice") && !trigger.Contains("Barrage")) return KeyCode.E;
        if (trigger.Contains("Melee")) return KeyCode.Space;
        return KeyCode.None;
    }

    // --- CÁC HÀM PUBLIC MỚI ĐỂ UI VÀ TRẠM NÂNG CẤP GỌI ---
    
    public int GetUpgradeCost(KeyCode key)
    {
        return skillUpgradeCosts.TryGetValue(key, out int cost) ? cost : -1;
    }

    public bool IsSkillUpgraded(KeyCode key)
    {
        return skillUpgraded.TryGetValue(key, out bool upgraded) && upgraded;
    }

    public bool AttemptToUpgradeSkill(KeyCode key)
    {
        if (IsSkillUpgraded(key))
        {
            Debug.Log("Skill already upgraded!");
            return false;
        }

        int cost = GetUpgradeCost(key);
        if (playerWallet != null && playerWallet.SpendCoins(cost))
        {
            skillUpgraded[key] = true;
            AudioManager.Instance.Play("choose");
            Debug.Log($"Successfully upgraded {key}!");
            // Nếu là kỹ năng Lướt, cập nhật lại cooldown ngay lập tức cho UI
            if (key == flashKey)
            {
                 OnSkillUsed?.Invoke(key, GetCooldownForSkill(key));
            }
            return true;
        }

        Debug.Log("Upgrade failed. Not enough coins.");
        return false;
    }

    public List<KeyCode> GetUpgradableSkills()
    {
        List<KeyCode> upgradable = new();
        foreach (var skill in skillUpgraded)
        {
            if (!skill.Value)
            {
                upgradable.Add(skill.Key);
            }
        }
        return upgradable;
    }
}