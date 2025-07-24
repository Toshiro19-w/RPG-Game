using UnityEngine;
using System.Collections;

public class DungeonRegenerator : MonoBehaviour
{
    [Header("Auto Regeneration")]
    [SerializeField] private bool checkOnStart = true;
    [SerializeField] private float delayBeforeRegeneration = 0.5f;
    
    private DungeonManager dungeonManager;
    private CompactDungeonManager compactDungeonManager;
    
    void Start()
    {
        if (checkOnStart)
        {
            StartCoroutine(CheckAndRegenerateDungeon());
        }
    }
    
    private IEnumerator CheckAndRegenerateDungeon()
    {
        yield return new WaitForSeconds(delayBeforeRegeneration);
        
        // Kiểm tra flag regenerate
        if (PlayerPrefs.GetInt("RegenerateDungeon", 0) == 1)
        {
            Debug.Log("Tạo dungeon mới sau khi chuyển map...");
            
            // Xóa flag
            PlayerPrefs.DeleteKey("RegenerateDungeon");
            
            // Tìm dungeon managers
            dungeonManager = FindFirstObjectByType<DungeonManager>();
            compactDungeonManager = FindFirstObjectByType<CompactDungeonManager>();
            
            // Tạo seed mới
            int newSeed = Random.Range(1, 100000);
            
            // Xóa enemies cũ
            ClearExistingEnemies();
            
            if (compactDungeonManager != null)
            {
                compactDungeonManager.GenerateDungeonWithSeed(newSeed);
            }
            else if (dungeonManager != null)
            {
                dungeonManager.ClearDungeon();
                yield return new WaitForSeconds(0.1f);
                dungeonManager.GenerateDungeon();
            }
        }
    }
    
    private void ClearExistingEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        
        foreach (GameObject enemy in enemies)
            Destroy(enemy);
            
        foreach (GameObject boss in bosses)
            Destroy(boss);
    }
    
    [ContextMenu("Force Regenerate Dungeon")]
    public void ForceRegenerateDungeon()
    {
        StartCoroutine(CheckAndRegenerateDungeon());
    }
}