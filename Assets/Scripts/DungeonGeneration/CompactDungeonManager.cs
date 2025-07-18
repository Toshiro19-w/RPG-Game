using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CompactDungeonManager : MonoBehaviour
{
    [Header("Generator References")]
    public CompactDungeonGenerator dungeonGenerator;
    public CompactRoomGenerator roomGenerator;
    public CompactMinimap minimap;
    
    [Header("Dungeon Settings")]
    public int dungeonLevel = 1;
    public bool generateOnStart = true;
    public int dungeonSeed = 0;
    public bool useRandomSeed = true;
    
    [Header("UI")]
    public Slider generationProgressBar;
    public Text generationStatusText;
    public GameObject minimapPanel;
    public KeyCode minimapToggleKey = KeyCode.M;
    
    [Header("Player")]
    public GameObject playerPrefab;
    public bool spawnPlayerAtStart = true;
    
    private DungeonManager dungeonManager;
    private GameObject player;
    private bool isGenerating = false;
    private bool minimapVisible = false;
    
    void Awake()
    {
        // Tìm các thành phần nếu chưa được gán
        if (dungeonGenerator == null)
            dungeonGenerator = GetComponent<CompactDungeonGenerator>();
            
        if (roomGenerator == null)
            roomGenerator = GetComponent<CompactRoomGenerator>();
            
        if (dungeonManager == null)
            dungeonManager = GetComponent<DungeonManager>();

        if (minimap == null)
            minimap = FindFirstObjectByType<CompactMinimap>();

        // Khởi tạo seed
        if (useRandomSeed)
            dungeonSeed = Random.Range(1, 100000);
            
        Random.InitState(dungeonSeed);
    }
    
    void Start()
    {
        if (generateOnStart)
        {
            StartCoroutine(GenerateDungeonWithProgress());
        }
        
        // Ẩn minimap ban đầu
        if (minimapPanel != null)
            minimapPanel.SetActive(false);
    }
    
    void Update()
    {
        // Bật/tắt minimap khi nhấn phím
        if (Input.GetKeyDown(minimapToggleKey))
        {
            ToggleMinimap();
        }
    }
    
    public void ToggleMinimap()
    {
        minimapVisible = !minimapVisible;
        
        if (minimapPanel != null)
            minimapPanel.SetActive(minimapVisible);
            
        if (minimap != null)
            minimap.ToggleMinimap(minimapVisible);
    }
    
    public void GenerateDungeonWithSeed(int seed)
    {
        dungeonSeed = seed;
        Random.InitState(dungeonSeed);
        StartCoroutine(GenerateDungeonWithProgress());
    }
    
    public IEnumerator GenerateDungeonWithProgress()
    {
        if (isGenerating)
            yield break;
            
        isGenerating = true;
        
        // Hiển thị UI tiến trình nếu có
        if (generationProgressBar != null)
        {
            generationProgressBar.gameObject.SetActive(true);
            generationProgressBar.value = 0;
        }
        
        if (generationStatusText != null)
        {
            generationStatusText.gameObject.SetActive(true);
            generationStatusText.text = "Khởi tạo dungeon...";
        }
        
        // Xóa dungeon cũ nếu có
        if (dungeonManager != null)
        {
            dungeonManager.ClearDungeon();
        }
        
        yield return null;
        UpdateProgress(0.1f, "Tạo cấu trúc dungeon...");
        
        // Tạo cấu trúc dungeon - Sử dụng DungeonManager để tạo dungeon
        if (dungeonManager != null)
        {
            // Đặt seed cho dungeon
            if (dungeonGenerator != null)
            {
                dungeonGenerator.dungeonSeed = dungeonSeed;
                dungeonGenerator.dungeonLevel = dungeonLevel;
            }
            
            // Tạo dungeon thông qua DungeonManager
            dungeonManager.GenerateDungeon();
        }
        
        yield return null;
        UpdateProgress(0.4f, "Tạo các phòng...");
        
        yield return null;
        UpdateProgress(0.7f, "Tạo các vật phẩm và kẻ địch...");
        
        // Tạo các vật phẩm và kẻ địch
        SpawnEnemiesAndItems();
        
        yield return null;
        UpdateProgress(0.9f, "Hoàn thiện dungeon...");
        
        // Đặt player vào phòng start
        if (spawnPlayerAtStart && dungeonManager != null)
        {
            SpawnPlayer();
        }
        
        // Khởi tạo minimap
        if (minimap != null)
        {
            minimap.InitializeMinimap();
        }
        
        yield return new WaitForSeconds(0.5f);
        UpdateProgress(1.0f, "Hoàn thành!");
        
        // Ẩn UI tiến trình sau 1 giây
        yield return new WaitForSeconds(1.0f);
        
        if (generationProgressBar != null)
            generationProgressBar.gameObject.SetActive(false);
            
        if (generationStatusText != null)
            generationStatusText.gameObject.SetActive(false);
            
        isGenerating = false;
    }
    
    private void UpdateProgress(float progress, string status)
    {
        if (generationProgressBar != null)
            generationProgressBar.value = progress;
            
        if (generationStatusText != null)
            generationStatusText.text = status;
    }
    
    private void SpawnEnemiesAndItems()
    {
        if (dungeonManager == null) return;
        
        List<Room> allRooms = dungeonManager.GetAllRooms();
        
        foreach (Room room in allRooms)
        {
            // Không spawn enemies trong phòng start và shop
            if (room.roomType == Room.RoomType.Start || room.roomType == Room.RoomType.Shop)
                continue;
                
            // Thêm enemies vào danh sách của phòng
            if (room.enemies.Count == 0)
            {
                int enemyCount = room.roomType switch
                {
                    Room.RoomType.Normal => Random.Range(2, 4),
                    Room.RoomType.Boss => 1, // Boss room chỉ có 1 boss
                    Room.RoomType.Treasure => Random.Range(1, 3),
                    _ => 0
                };
                
                // Thêm enemies vào danh sách (cần thêm prefabs của enemies vào đây)
                // Đây chỉ là ví dụ, bạn cần thay thế bằng prefabs thực tế của mình
                for (int i = 0; i < enemyCount; i++)
                {
                    // Tìm prefabs của enemies trong project
                    GameObject enemyPrefab = null;
                    
                    if (room.roomType == Room.RoomType.Boss)
                    {
                        enemyPrefab = Resources.Load<GameObject>("Prefabs/Boss");
                    }
                    else
                    {
                        string[] enemyTypes = { "Skeleton", "Slime" };
                        string randomType = enemyTypes[Random.Range(0, enemyTypes.Length)];
                        enemyPrefab = Resources.Load<GameObject>($"Prefabs/{randomType}");
                    }
                    
                    if (enemyPrefab != null)
                    {
                        room.enemies.Add(enemyPrefab);
                    }
                }
            }
        }
    }
    
    private void SpawnPlayer()
    {
        Room startRoom = dungeonManager.GetStartRoom();
        
        if (startRoom != null)
        {
            // Tìm player hiện có
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            
            if (existingPlayer != null)
            {
                // Di chuyển player đến phòng start
                existingPlayer.transform.position = startRoom.transform.position;
                player = existingPlayer;
            }
            else if (playerPrefab != null)
            {
                // Tạo player mới
                player = Instantiate(playerPrefab, startRoom.transform.position, Quaternion.identity);
            }
            
            // Đánh dấu phòng start đã được khám phá
            startRoom.isVisited = true;
            startRoom.isCleared = true;
            startRoom.EnterRoom();
            
            // Cập nhật phòng hiện tại
            dungeonManager.SetCurrentRoom(startRoom);
        }
    }
    
    // Phương thức để chuyển đến level tiếp theo
    public void GoToNextLevel()
    {
        dungeonLevel++;
        
        // Tạo seed mới nếu sử dụng seed ngẫu nhiên
        if (useRandomSeed)
            dungeonSeed = Random.Range(1, 100000);
            
        StartCoroutine(GenerateDungeonWithProgress());
    }

}