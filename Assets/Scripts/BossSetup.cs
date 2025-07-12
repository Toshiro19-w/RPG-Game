using UnityEngine;

[System.Serializable]
public class BossSetup : MonoBehaviour
{
    [Header("Boss Setup Info")]
    [SerializeField] private bool setupComplete = false;
    
    void Start()
    {
        if (!setupComplete)
        {
            SetupBossComponents();
            setupComplete = true;
        }
    }
    
    private void SetupBossComponents()
    {
        // Đảm bảo Boss có tất cả components cần thiết
        
        // Kiểm tra và thêm BossStompingAttack nếu chưa có
        if (GetComponent<BossStompingAttack>() == null)
        {
            gameObject.AddComponent<BossStompingAttack>();
            Debug.Log("Đã thêm BossStompingAttack component vào Boss");
        }
        
        // Kiểm tra Animator
        if (GetComponent<Animator>() == null)
        {
            Debug.LogWarning("Boss thiếu Animator component!");
        }
        
        // Kiểm tra BossAttack
        if (GetComponent<BossAttack>() == null)
        {
            Debug.LogWarning("Boss thiếu BossAttack component!");
        }
        
        // Kiểm tra EnemyMovement
        if (GetComponent<EnemyMovement>() == null)
        {
            Debug.LogWarning("Boss thiếu EnemyMovement component!");
        }
        
        // Kiểm tra BossAnimationController
        if (GetComponent<BossAnimationController>() == null)
        {
            Debug.LogWarning("Boss thiếu BossAnimationController component!");
        }
        
        Debug.Log("Boss setup hoàn tất!");
    }
    
    [ContextMenu("Force Setup Boss")]
    public void ForceSetup()
    {
        SetupBossComponents();
        setupComplete = true;
    }
}
