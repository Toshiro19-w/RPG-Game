using UnityEngine;
using System.Collections;

public class DungeonSceneManager : MonoBehaviour
{
    public static DungeonSceneManager Instance;
    
    [Header("Scene Management")]
    [SerializeField] private int currentMapLevel = 1;
    [SerializeField] private bool autoLoadOnStart = true;
    
    [Header("Memory Management")]
    [SerializeField] private bool clearOnSceneChange = true;
    [SerializeField] private float clearDelay = 0.1f;
    
    private DungeonManager dungeonManager;
    private CompactDungeonManager compactDungeonManager;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (autoLoadOnStart)
        {
            StartCoroutine(LoadDungeonForCurrentMap());
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && clearOnSceneChange)
        {
            ClearCurrentDungeon();
        }
    }
    
    public IEnumerator LoadDungeonForCurrentMap()
    {
        // Lấy map level từ PlayerPrefs nếu có
        currentMapLevel = PlayerPrefs.GetInt("CurrentMapLevel", 1);
        
        yield return new WaitForSeconds(clearDelay);
        
        // Clear dungeon cũ trước
        ClearCurrentDungeon();
        
        yield return new WaitForSeconds(0.1f);
        
        // Tìm managers
        dungeonManager = FindFirstObjectByType<DungeonManager>();
        compactDungeonManager = FindFirstObjectByType<CompactDungeonManager>();
        
        // Tạo seed dựa trên map level
        int mapSeed = GenerateMapSeed(currentMapLevel);
        
        // Load dungeon mới
        if (compactDungeonManager != null)
        {
            compactDungeonManager.dungeonLevel = currentMapLevel;
            compactDungeonManager.GenerateDungeonWithSeed(mapSeed);
        }
        else if (dungeonManager != null)
        {
            dungeonManager.GenerateDungeon();
        }
        
        Debug.Log($"Loaded dungeon for Map Level {currentMapLevel}");
    }
    
    public void ClearCurrentDungeon()
    {
        // Clear tất cả enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        
        foreach (GameObject enemy in enemies)
            if (enemy != null) Destroy(enemy);
            
        foreach (GameObject boss in bosses)
            if (boss != null) Destroy(boss);
        
        // Clear dungeon structure
        if (dungeonManager != null)
            dungeonManager.ClearDungeon();
            
        // Force garbage collection
        System.GC.Collect();
        
        Debug.Log("Dungeon cleared to save memory");
    }
    
    public void GoToNextMap()
    {
        currentMapLevel++;
        PlayerPrefs.SetInt("CurrentMapLevel", currentMapLevel);
        
        // Clear current dungeon
        if (clearOnSceneChange)
            ClearCurrentDungeon();
        
        // Load new dungeon
        StartCoroutine(LoadDungeonForCurrentMap());
    }
    
    private int GenerateMapSeed(int mapLevel)
    {
        // Tạo seed dựa trên map level để có thể reproduce
        return (mapLevel * 12345) + Random.Range(1, 1000);
    }
    
    public int GetCurrentMapLevel() => currentMapLevel;
    
    [ContextMenu("Clear Dungeon")]
    public void ForceClearDungeon()
    {
        ClearCurrentDungeon();
    }
    
    [ContextMenu("Reload Current Map")]
    public void ReloadCurrentMap()
    {
        StartCoroutine(LoadDungeonForCurrentMap());
    }
}