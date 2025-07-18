using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Room : MonoBehaviour
{
    [Header("Room Settings")]
    public RoomType roomType = RoomType.Normal;
    public Vector2Int gridPosition;
    public bool isCleared = false;
    public bool isVisited = false;
    
    [Header("Connections")]
    public List<Door> doors = new List<Door>();
    public List<Room> connectedRooms = new List<Room>();
    
    [Header("Enemies")]
    public List<GameObject> enemies = new List<GameObject>();
    public Transform[] enemySpawnPoints;
    
    [Header("Rewards")]
    public GameObject[] rewardPrefabs;
    public Transform rewardSpawnPoint;
    
    private bool enemiesSpawned = false;
    
    public enum RoomType
    {
        Normal,
        Boss,
        Treasure,
        Shop,
        Start
    }
    
    void Start()
    {
        SetupRoom();

        if (roomType == RoomType.Start)
        {
            EnterRoom();
            
            // Nếu player đã có trong room từ đầu
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                Invoke(nameof(CheckPlayerInRoom), 0.1f);
            }
        }
    }
    
    private void CheckPlayerInRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < 30f) // Player trong room
            {
                Debug.Log($"Player detected in {gameObject.name}, entering room...");
                EnterRoom();
            }
        }
    }
    
    private void SetupRoom()
    {
        foreach (var door in doors)
        {
            door.SetActive(false);
        }
        
        if (roomType == RoomType.Start)
        {
            isVisited = true;
            isCleared = true;
        }
    }
    
    public void EnterRoom()
    {
        Debug.Log($"Entering room: {gameObject.name}, isVisited: {isVisited}, isCleared: {isCleared}");
        
        if (!isVisited)
        {
            isVisited = true;
            SpawnEnemies();
        }
        
        // Chỉ mở doors nếu room đã clear hoặc là Start room
        if (isCleared || roomType == RoomType.Start)
        {
            UpdateDoors();
        }
        else
        {
            Debug.Log($"Room {gameObject.name} not cleared yet, doors remain closed");
            // Đảm bảo doors đóng
            foreach (var door in doors)
            {
                door.SetActive(false);
            }
        }
    }
    
    private void SpawnEnemies()
    {
        if (enemiesSpawned || roomType == RoomType.Start || roomType == RoomType.Shop) return;
        
        Debug.Log($"Spawning enemies in {gameObject.name}. Enemy count: {enemies.Count}, Spawn points: {enemySpawnPoints?.Length}");
        
        if (enemySpawnPoints != null && enemySpawnPoints.Length > 0 && enemies.Count > 0)
        {
            int enemyCount = roomType == RoomType.Boss ? enemySpawnPoints.Length : 
                           Random.Range(enemySpawnPoints.Length / 2, enemySpawnPoints.Length + 1);
            
            List<Transform> availableSpawnPoints = new List<Transform>(enemySpawnPoints);
            
            for (int i = 0; i < enemyCount; i++)
            {
                if (availableSpawnPoints.Count == 0) break;
                
                int spawnPointIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                availableSpawnPoints.RemoveAt(spawnPointIndex);
                
                int enemyIndex = Random.Range(0, enemies.Count);
                GameObject enemy = Instantiate(enemies[enemyIndex], spawnPoint.position, spawnPoint.rotation);
                enemy.transform.SetParent(transform);
                
                // Scale boss enemies
                if (roomType == RoomType.Boss)
                {
                    enemy.transform.localScale *= 1.5f;
                    // Boss enemies will be scaled through their own component settings
                }
                
                Debug.Log($"Spawned {enemy.name} at {spawnPoint.position}");
            }
        }
        else
        {
            Debug.LogWarning($"No enemy spawn points or enemies found in {gameObject.name}!");
        }
        
        enemiesSpawned = true;
    }
    
    public void ClearRoom()
    {
        if (isCleared) return;
        
        isCleared = true;
        
        if (rewardPrefabs.Length > 0 && rewardSpawnPoint != null)
        {
            // Spawn nhiều rewards dựa trên room type
            int rewardCount = GetRewardCount();
            
            for (int i = 0; i < rewardCount; i++)
            {
                int randomReward = Random.Range(0, rewardPrefabs.Length);
                Vector3 spawnPos = rewardSpawnPoint.position + new Vector3(
                    Random.Range(-1f, 1f), 
                    Random.Range(-1f, 1f), 
                    0
                );
                GameObject reward = Instantiate(rewardPrefabs[randomReward], spawnPos, rewardSpawnPoint.rotation);
                Debug.Log($"Spawned reward at {spawnPos}, visible: {reward.GetComponent<SpriteRenderer>()?.enabled}");
            }
        }
        
        Debug.Log($"Room {gameObject.name} cleared! Updating doors...");
        DungeonManager.Instance?.OpenNewRooms(this);
        UpdateDoors();
        
        // Force cập nhật doors sau 0.1s
        Invoke(nameof(UpdateDoors), 0.1f);
    }
    
    private void UpdateDoors()
    {
        Debug.Log($"UpdateDoors: {doors.Count} doors, {connectedRooms.Count} connected rooms, isCleared: {isCleared}");
        
        // Chỉ mở doors nếu room đã clear hoặc là Start room
        if (!isCleared && roomType != RoomType.Start)
        {
            Debug.Log($"Room {gameObject.name} not cleared, doors remain closed");
            return;
        }
        
        // Kích hoạt tất cả các cửa có teleportPoint
        foreach (Door door in doors)
        {
            if (door != null && door.teleportPoint != null)
            {
                Debug.Log($"Activating door {door.name} with teleport point");
                door.SetActive(true);
            }
        }
        
        // Kích hoạt các cửa dựa trên connectedRooms
        for (int i = 0; i < doors.Count && i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] != null && doors[i] != null)
            {
                Debug.Log($"Activating door {i} to room {connectedRooms[i].name}");
                doors[i].SetActive(true);
                
                // Chỉ đặt targetRoom nếu chưa được đặt
                if (doors[i].teleportPoint == null)
                {
                    doors[i].SetTargetRoom(connectedRooms[i]);
                    Debug.LogWarning($"Door {i} in {gameObject.name} had no teleport point, setting target room only");
                }
            }
            else
            {
                Debug.LogWarning($"Door {i} or connected room is null!");
            }
        }
    }
    
    public void ConnectToRoom(Room otherRoom, Door door)
    {
        // Kiểm tra xem phòng đã được kết nối chưa
        int existingIndex = connectedRooms.IndexOf(otherRoom);
        
        if (existingIndex >= 0)
        {
            // Nếu đã kết nối, cập nhật cửa
            if (existingIndex < doors.Count)
            {
                doors[existingIndex] = door;
                Debug.Log($"Updated door connection from {name} to {otherRoom.name}");
            }
            else
            {
                // Trường hợp bất thường: có phòng nhưng không có cửa
                doors.Add(door);
                Debug.LogWarning($"Room {name} had connection to {otherRoom.name} but no door, adding door");
            }
        }
        else
        {
            // Thêm kết nối mới
            connectedRooms.Add(otherRoom);
            doors.Add(door);
            Debug.Log($"Connected room {name} to {otherRoom.name} with door {door.name}");
        }
        
        // Đảm bảo cửa có tham chiếu đến phòng đích
        door.SetTargetRoom(otherRoom);
        
        // Kích hoạt cửa nếu phòng đã được clear hoặc là phòng bắt đầu
        if (isCleared || roomType == RoomType.Start)
        {
            door.SetActive(true);
            Debug.Log($"Activated door {door.name} in {name} to {otherRoom.name} because room is cleared or start room");
        }
    }
    
    public bool HasEnemies()
    {
        return transform.GetComponentsInChildren<EnemyHealth>().Length > 0;
    }
    
    private int GetRewardCount()
    {
        return roomType switch
        {
            RoomType.Boss => Random.Range(2, 4),
            RoomType.Treasure => Random.Range(3, 5),
            RoomType.Normal => Random.Range(1, 3),
            _ => 1
        };
    }
    
    void Update()
    {
        if (isVisited && !isCleared && !HasEnemies())
        {
            ClearRoom();
        }
    }
}