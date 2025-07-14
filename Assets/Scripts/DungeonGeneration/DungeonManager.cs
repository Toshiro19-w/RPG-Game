using UnityEngine;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;
    
    [Header("Dungeon Settings")]
    public int dungeonWidth = 5;
    public int dungeonHeight = 5;
    public float roomSpacing = 20f;
    
    [Header("Room Prefabs")]
    public GameObject normalRoomPrefab;
    public GameObject bossRoomPrefab;
    public GameObject treasureRoomPrefab;
    public GameObject shopRoomPrefab;
    public GameObject startRoomPrefab;
    
    [Header("Generation Settings")]
    public int minRooms = 8;
    public int maxRooms = 12;
    public float branchChance = 0.3f;
    
    private Room[,] roomGrid;
    private List<Room> allRooms = new List<Room>();
    private Room currentRoom;
    private Room startRoom;
    
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
        GenerateDungeon();
    }
    
    public void GenerateDungeon()
    {
        roomGrid = new Room[dungeonWidth, dungeonHeight];
        allRooms.Clear();
        
        // Tạo phòng start ở giữa
        Vector2Int startPos = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        CreateRoom(startPos, Room.RoomType.Start);
        
        // Tạo đường đi chính
        GenerateMainPath();
        
        // Tạo các nhánh phụ
        GenerateBranches();
        
        // Kết nối các phòng
        ConnectRooms();
        
        // Đặt player vào phòng start
        SetPlayerToStartRoom();
    }
    
    private void GenerateMainPath()
    {
        Vector2Int currentPos = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        int roomsToGenerate = Random.Range(minRooms, maxRooms);
        
        for (int i = 1; i < roomsToGenerate; i++)
        {
            Vector2Int nextPos = GetRandomAdjacentPosition(currentPos);
            
            if (IsValidPosition(nextPos) && roomGrid[nextPos.x, nextPos.y] == null)
            {
                Room.RoomType roomType = Room.RoomType.Normal;
                
                // Boss room ở cuối
                if (i == roomsToGenerate - 1)
                    roomType = Room.RoomType.Boss;
                // Treasure room ngẫu nhiên
                else if (Random.value < 0.2f)
                    roomType = Room.RoomType.Treasure;
                // Shop room ngẫu nhiên
                else if (Random.value < 0.15f)
                    roomType = Room.RoomType.Shop;
                
                CreateRoom(nextPos, roomType);
                currentPos = nextPos;
            }
        }
    }
    
    private void GenerateBranches()
    {
        List<Room> roomsToProcess = new List<Room>(allRooms);
        
        foreach (Room room in roomsToProcess)
        {
            if (Random.value < branchChance)
            {
                Vector2Int branchPos = GetRandomAdjacentPosition(room.gridPosition);
                
                if (IsValidPosition(branchPos) && roomGrid[branchPos.x, branchPos.y] == null)
                {
                    Room.RoomType branchType = Random.value < 0.4f ? Room.RoomType.Treasure : Room.RoomType.Normal;
                    CreateRoom(branchPos, branchType);
                }
            }
        }
    }
    
    private void CreateRoom(Vector2Int gridPos, Room.RoomType roomType)
    {
        GameObject roomPrefab = GetRoomPrefab(roomType);
        if (roomPrefab == null) return;
        
        Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, gridPos.y * roomSpacing, 0);
        GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
        
        Room room = roomObj.GetComponent<Room>();
        if (room == null)
            room = roomObj.AddComponent<Room>();
        
        room.gridPosition = gridPos;
        room.roomType = roomType;
        
        roomGrid[gridPos.x, gridPos.y] = room;
        allRooms.Add(room);
        
        if (roomType == Room.RoomType.Start)
        {
            startRoom = room;
            currentRoom = room;
        }
    }
    
    private GameObject GetRoomPrefab(Room.RoomType roomType)
    {
        return roomType switch
        {
            Room.RoomType.Start => startRoomPrefab,
            Room.RoomType.Boss => bossRoomPrefab,
            Room.RoomType.Treasure => treasureRoomPrefab,
            Room.RoomType.Shop => shopRoomPrefab,
            _ => normalRoomPrefab
        };
    }
    
    private void ConnectRooms()
    {
        foreach (Room room in allRooms)
        {
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = room.gridPosition + dir;
                
                if (IsValidPosition(neighborPos))
                {
                    Room neighbor = roomGrid[neighborPos.x, neighborPos.y];
                    if (neighbor != null)
                    {
                        CreateDoorConnection(room, neighbor, dir);
                    }
                }
            }
        }
    }
    
    private void CreateDoorConnection(Room from, Room to, Vector2Int direction)
    {
        // Tạo door từ from đến to
        GameObject doorObj = new GameObject($"Door_to_{to.roomType}");
        doorObj.transform.SetParent(from.transform);
        
        // Đặt vị trí door dựa trên hướng
        Vector3 doorPos = GetDoorPosition(direction);
        doorObj.transform.localPosition = doorPos;
        
        // Thêm components
        Door door = doorObj.AddComponent<Door>();
        BoxCollider2D trigger = doorObj.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = Vector2.one;
        
        door.SetTargetRoom(to);
        from.ConnectToRoom(to, door);
    }
    
    private Vector3 GetDoorPosition(Vector2Int direction)
    {
        float offset = roomSpacing * 0.4f;
        
        if (direction == Vector2Int.up) return new Vector3(0, offset, 0);
        if (direction == Vector2Int.down) return new Vector3(0, -offset, 0);
        if (direction == Vector2Int.left) return new Vector3(-offset, 0, 0);
        if (direction == Vector2Int.right) return new Vector3(offset, 0, 0);
        
        return Vector3.zero;
    }
    
    private Vector2Int GetRandomAdjacentPosition(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int randomDir = directions[Random.Range(0, directions.Length)];
        return pos + randomDir;
    }
    
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < dungeonWidth && pos.y >= 0 && pos.y < dungeonHeight;
    }
    
    private void SetPlayerToStartRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && startRoom != null)
        {
            player.transform.position = startRoom.transform.position;
        }
    }
    
    public void SetCurrentRoom(Room room)
    {
        currentRoom = room;
    }
    
    public void OpenNewRooms(Room clearedRoom)
    {
        // Logic để mở các phòng mới sau khi clear room
        foreach (Room connectedRoom in clearedRoom.connectedRooms)
        {
            if (!connectedRoom.isVisited)
            {
                // Có thể thêm hiệu ứng mở door ở đây
            }
        }
    }
    
    public Room GetCurrentRoom() => currentRoom;
    public List<Room> GetAllRooms() => allRooms;
}