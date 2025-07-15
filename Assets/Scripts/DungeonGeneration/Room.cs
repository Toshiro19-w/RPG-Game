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

        Room currentRoom = FindFirstObjectByType<Room>();
        currentRoom.EnterRoom();
        
        // Nếu player đã có trong room từ đầu
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            Invoke(nameof(CheckPlayerInRoom), 0.1f);
        }
    }
    
    private void CheckPlayerInRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < 10f) // Player trong room
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
        Debug.Log($"Entering room: {gameObject.name}, isVisited: {isVisited}");
        
        if (!isVisited)
        {
            isVisited = true;
            SpawnEnemies();
        }
        
        UpdateDoors();
    }
    
    private void SpawnEnemies()
    {
        if (enemiesSpawned || roomType == RoomType.Start || roomType == RoomType.Shop) return;
        
        Debug.Log($"Spawning enemies in {gameObject.name}. Enemy count: {enemies.Count}, Spawn points: {enemySpawnPoints?.Length}");
        Debug.Log($"Room cleared, spawning {GetRewardCount()} rewards at {rewardSpawnPoint?.position}");
        
        if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
        {
            foreach (var spawnPoint in enemySpawnPoints)
            {
                if (enemies.Count > 0)
                {
                    int randomIndex = Random.Range(0, enemies.Count);
                    GameObject enemy = Instantiate(enemies[randomIndex], spawnPoint.position, spawnPoint.rotation);
                    enemy.transform.SetParent(transform);
                    Debug.Log($"Spawned {enemy.name} at {spawnPoint.position}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"No enemy spawn points found in {gameObject.name}!");
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
        
        for (int i = 0; i < doors.Count && i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] != null && doors[i] != null)
            {
                Debug.Log($"Activating door {i} to room {connectedRooms[i].name}");
                doors[i].SetActive(true);
                doors[i].SetTargetRoom(connectedRooms[i]);
            }
            else
            {
                Debug.LogWarning($"Door {i} or connected room is null!");
            }
        }
    }
    
    public void ConnectToRoom(Room otherRoom, Door door)
    {
        if (!connectedRooms.Contains(otherRoom))
        {
            connectedRooms.Add(otherRoom);
            doors.Add(door);
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