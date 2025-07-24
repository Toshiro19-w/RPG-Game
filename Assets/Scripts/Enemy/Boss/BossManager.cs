using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    public static bool IsBossDefeated { get; set; } = false;

    [SerializeField] private EnemyHealth bossHealth;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            IsBossDefeated = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<EnemyHealth>();
    }
    
    void Update()
    {
        // Kiểm tra nếu boss đã bị tiêu diệt
        if (!IsBossDefeated && bossHealth != null && bossHealth.gameObject == null)
        {
            OnBossDefeated();
        }
    }
    
    private void OnBossDefeated()
    {
        IsBossDefeated = true;
        Debug.Log("Boss đã bị hạ gục! Portal có thể được kích hoạt.");
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            OnBossDefeated();
        }
    }
    
    public static void ResetBossStatus()
    {
        IsBossDefeated = false;
        Debug.Log("Boss status đã được reset");
    }
}