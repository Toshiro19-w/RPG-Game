using UnityEngine;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;
    
    [Header("Dungeon Settings")]
    public int dungeonWidth = 5;
    public int dungeonHeight = 5;
    public float roomSpacing = 20f;
    
    [Header("Room Generation")]
    public TilemapRoomGenerator roomGenerator;
    public bool useTilemapRooms = true;
    
    [Header("Generation Settings")]
    public bool useProceduralGeneration = true;
    public bool generateOnStart = true;
    
    [Header("Debug")]
    public bool showGridGizmos = true;
    public bool regenerateDungeon = false;
    
    private Room[,] roomGrid;
    private List<Room> allRooms = new List<Room>();
    private Room currentRoom;
    private Room startRoom;
    private Room bossRoom;
    
    private DungeonGenerator dungeonGenerator;
    private CompactDungeonGenerator compactDungeonGenerator;
    
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
            return;
        }
        
        // Initialize generators
        if (useProceduralGeneration)
        {
            dungeonGenerator = GetComponent<DungeonGenerator>();
            if (dungeonGenerator == null)
            {
                dungeonGenerator = gameObject.AddComponent<DungeonGenerator>();
            }
        }
        else
        {
            compactDungeonGenerator = GetComponent<CompactDungeonGenerator>();
            if (compactDungeonGenerator == null)
            {
                compactDungeonGenerator = gameObject.AddComponent<CompactDungeonGenerator>();
            }
            
            // Đảm bảo CompactDungeonGenerator có tham chiếu đến DungeonManager
            if (compactDungeonGenerator != null)
            {
                compactDungeonGenerator.dungeonManager = this;
            }
        }
        
        // Khởi tạo roomGrid
        roomGrid = new Room[dungeonWidth, dungeonHeight];
    }
    
    void Start()
    {
        // Chỉ tạo dungeon nếu không có CompactDungeonManager hoặc nếu được yêu cầu trực tiếp
        CompactDungeonManager compactManager = GetComponent<CompactDungeonManager>();
        if ((compactManager == null && generateOnStart) || (compactManager != null && !compactManager.generateOnStart && generateOnStart))
        {
            GenerateDungeon();
        }
    }
    
    void Update()
    {
        // Debug: Regenerate dungeon with R key
        if (regenerateDungeon && Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Regenerating dungeon with Alt + R");
            regenerateDungeon = true; // Set flag to regenerate
        }
        
        if (regenerateDungeon)
        {
            Debug.Log("Regenerating dungeon...");
            regenerateDungeon = false; // Reset flag to prevent continuous regeneration
            ClearDungeon();
            GenerateDungeon();
        }
    }
    
    public void GenerateDungeon()
    {
        Debug.Log("Generating new dungeon...");
        roomGrid = new Room[dungeonWidth, dungeonHeight];
        allRooms.Clear();
        startRoom = null;
        bossRoom = null;
        currentRoom = null;
        
        if (useProceduralGeneration && dungeonGenerator != null)
        {
            // Use procedural generator
            dungeonGenerator.GenerateDungeon();
        }
        else if (compactDungeonGenerator != null)
        {
            // Use compact grid-based generator
            compactDungeonGenerator.GenerateDungeon();
        }
        else
        {
            Debug.LogError("No dungeon generator available!");
            return;
        }
        
        // Place player in start room - chỉ thực hiện nếu không có CompactDungeonManager
        CompactDungeonManager compactManager = GetComponent<CompactDungeonManager>();
        if (compactManager == null)
        {
            SetPlayerToStartRoom();
        }
        
        Debug.Log($"Dungeon generated with {allRooms.Count} rooms");
    }
    
    public void ClearDungeon()
    {
        Debug.Log("Clearing existing dungeon...");
        
        // Destroy all existing rooms
        foreach (Room room in allRooms)
        {
            if (room != null && room.gameObject != null)
            {
                Destroy(room.gameObject);
            }
        }
        
        allRooms.Clear();
        startRoom = null;
        bossRoom = null;
        currentRoom = null;
    }
    
    public void CreateRoom(Vector2Int gridPos, Room.RoomType roomType)
    {
        Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, gridPos.y * roomSpacing, 0);
        GameObject roomObj;
        
        if (useTilemapRooms && roomGenerator != null)
        {
            // Determine possible door directions
            Dictionary<int, bool> doorDirections = GetPossibleDoorDirections(gridPos, roomType);
            
            // Generate room from tilemap
            roomObj = roomGenerator.GenerateRoom(roomType, gridPos, doorDirections);
            roomObj.transform.position = worldPos;
            roomObj.transform.SetParent(transform);
        }
        else
        {
            // Fallback to prefab-based room generation
            Debug.LogWarning("Falling back to prefab-based room generation. Please assign room prefabs.");
            return;
        }
        
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
            Debug.Log($"Start room created at {gridPos}");
        }
        else if (roomType == Room.RoomType.Boss)
        {
            bossRoom = room;
            Debug.Log($"Boss room created at {gridPos}");
        }
    }
    
    private Dictionary<int, bool> GetPossibleDoorDirections(Vector2Int gridPos, Room.RoomType roomType)
    {
        Dictionary<int, bool> doorDirections = new Dictionary<int, bool>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        
        // Tạo cửa ở tất cả các hướng cho mọi loại phòng
        for (int i = 0; i < directions.Length; i++)
        {
            // Mặc định tạo cửa ở tất cả các hướng
            doorDirections[i] = true;
        }
        
        Debug.Log($"Room at {gridPos} of type {roomType} will have doors in all directions");
        
        return doorDirections;
    }
    
    public void CreateDoorConnection(Room from, Room to, Vector2Int direction)
    {
        // Check if rooms are already connected
        if (from.connectedRooms.Contains(to))
        {
            return; // Already connected, don't create another connection
        }
        
        // Find corresponding doors
        Door fromDoor = GetDoorInDirection(from, direction);
        Door toDoor = GetDoorInDirection(to, -direction);
        
        if (fromDoor == null || toDoor == null)
        {
            Debug.LogWarning($"Cannot connect rooms: missing doors in direction {direction}");
            return;
        }
        
        // Create teleport point
        GameObject teleportPoint = new GameObject("TeleportPoint");
        teleportPoint.transform.SetParent(to.transform);
        teleportPoint.transform.localPosition = GetDoorPosition(-direction); // Opposite position
        Debug.Log($"Created teleport point in {to.name} at local position {teleportPoint.transform.localPosition}, world position {teleportPoint.transform.position}");
        
        // Connect doors
        fromDoor.teleportPoint = teleportPoint.transform;
        fromDoor.SetTargetRoom(to);
        fromDoor.SetActive(true);
        
        // Create teleport point for the other door
        GameObject teleportPointReverse = new GameObject("TeleportPoint");
        teleportPointReverse.transform.SetParent(from.transform);
        teleportPointReverse.transform.localPosition = GetDoorPosition(direction); // Opposite position
        Debug.Log($"Created reverse teleport point in {from.name} at local position {teleportPointReverse.transform.localPosition}, world position {teleportPointReverse.transform.position}");
        
        // Connect the other door
        toDoor.teleportPoint = teleportPointReverse.transform;
        toDoor.SetTargetRoom(from);
        toDoor.SetActive(true);
        
        // Add rooms to each other's connected rooms list
        from.connectedRooms.Add(to);
        to.connectedRooms.Add(from);
        
        // Add to connection list using ConnectToRoom method
        from.ConnectToRoom(to, fromDoor);
        to.ConnectToRoom(from, toDoor);
    }
    
    private Door GetDoorInDirection(Room room, Vector2Int direction)
    {
        // Determine door index based on direction
        int doorIndex;
        if (direction == Vector2Int.up) doorIndex = 0;
        else if (direction == Vector2Int.right) doorIndex = 1;
        else if (direction == Vector2Int.down) doorIndex = 2;
        else if (direction == Vector2Int.left) doorIndex = 3;
        else return null;
        
        // Find door by name
        Transform doorTransform = room.transform.Find($"Door_{doorIndex}");
        if (doorTransform != null)
        {
            Door door = doorTransform.GetComponent<Door>();
            if (door != null)
            {
                Debug.Log($"Found door {doorIndex} in room {room.name} by transform");
                return door;
            }
        }
        
        // If not found, try to find in doors list
        if (room.doors != null && room.doors.Count > doorIndex && room.doors[doorIndex] != null)
        {
            Debug.Log($"Found door {doorIndex} in room {room.name} by index in doors list");
            return room.doors[doorIndex];
        }
        
        // Last resort: search by name in doors list
        foreach (Door door in room.doors)
        {
            if (door != null && door.gameObject.name == $"Door_{doorIndex}")
            {
                Debug.Log($"Found door {doorIndex} in room {room.name} by name in doors list");
                return door;
            }
        }
        
        Debug.LogError($"Could not find door {doorIndex} in room {room.name}!");
        return null;
    }
    
    private Vector3 GetDoorPosition(Vector2Int direction)
    {
        // Tăng offset để đặt teleportPoint gần tường hơn
        float offset = roomSpacing * 0.45f;
        
        if (direction == Vector2Int.up) return new Vector3(0, offset, 0);
        if (direction == Vector2Int.down) return new Vector3(0, -offset, 0);
        if (direction == Vector2Int.left) return new Vector3(-offset, 0, 0);
        if (direction == Vector2Int.right) return new Vector3(offset, 0, 0);
        
        return Vector3.zero;
    }
    
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < dungeonWidth && pos.y >= 0 && pos.y < dungeonHeight;
    }
    
    public void SetPlayerToStartRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && startRoom != null)
        {
            player.transform.position = startRoom.transform.position;
            Debug.Log($"Player positioned at start room: {startRoom.transform.position}");
        }
        else
        {
            Debug.LogWarning("Could not position player: Player or start room is null");
        }
    }
    
    public void SetCurrentRoom(Room room)
    {
        currentRoom = room;
    }
    
    public void OpenNewRooms(Room clearedRoom)
    {
        // Logic to open new rooms after clearing a room
        foreach (Room connectedRoom in clearedRoom.connectedRooms)
        {
            if (!connectedRoom.isVisited)
            {
                Debug.Log($"Room {clearedRoom.name} cleared, connected room {connectedRoom.name} is now accessible");
            }
        }
    }
    
    public Room GetRoomAt(Vector2Int gridPos)
    {
        if (IsValidPosition(gridPos))
        {
            return roomGrid[gridPos.x, gridPos.y];
        }
        return null;
    }
    
    public bool HasRoomOfType(Room.RoomType type)
    {
        foreach (Room room in allRooms)
        {
            if (room.roomType == type)
                return true;
        }
        return false;
    }
    
    public Room GetCurrentRoom() => currentRoom;
    public Room GetStartRoom() => startRoom;
    public Room GetBossRoom() => bossRoom;
    public List<Room> GetAllRooms() => allRooms;
    
    void OnDrawGizmos()
    {
        if (!showGridGizmos || roomGrid == null) return;
        
        // Draw grid
        Gizmos.color = Color.gray;
        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 pos = new Vector3(x * roomSpacing, y * roomSpacing, 0);
                Gizmos.DrawWireCube(pos, new Vector3(roomSpacing * 0.9f, roomSpacing * 0.9f, 0.1f));
            }
        }
        
        // Draw rooms
        if (allRooms != null)
        {
            foreach (Room room in allRooms)
            {
                if (room == null) continue;
                
                // Different colors for different room types
                switch (room.roomType)
                {
                    case Room.RoomType.Start:
                        Gizmos.color = Color.green;
                        break;
                    case Room.RoomType.Boss:
                        Gizmos.color = Color.red;
                        break;
                    case Room.RoomType.Treasure:
                        Gizmos.color = Color.yellow;
                        break;
                    case Room.RoomType.Shop:
                        Gizmos.color = Color.blue;
                        break;
                    default:
                        Gizmos.color = Color.white;
                        break;
                }
                
                Vector3 pos = room.transform.position;
                Gizmos.DrawCube(pos, new Vector3(roomSpacing * 0.8f, roomSpacing * 0.8f, 0.1f));
                
                // Draw connections
                Gizmos.color = Color.cyan;
                foreach (Room connectedRoom in room.connectedRooms)
                {
                    if (connectedRoom != null)
                    {
                        Gizmos.DrawLine(room.transform.position, connectedRoom.transform.position);
                    }
                }
            }
        }
    }
}