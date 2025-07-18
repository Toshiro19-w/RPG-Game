using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int dungeonWidth = 5;
    public int dungeonHeight = 5;
    public float roomSpacing = 20f;
    public int minRooms = 8;
    public int maxRooms = 12;
    
    [Header("Room Chances")]
    public float treasureRoomChance = 0.3f;
    public float shopRoomChance = 0.2f;
    public float branchChance = 0.3f;
    
    [Header("References")]
    public TilemapRoomGenerator roomGenerator;
    
    [Header("Debug")]
    public bool showGenerationGizmos = true;
    
    private DungeonManager dungeonManager;
    
    private void Awake()
    {
        dungeonManager = GetComponent<DungeonManager>();
    }
    
    public void GenerateDungeon()
    {
        Debug.Log("Starting dungeon generation...");
        
        // Create start room in the center
        Vector2Int startPos = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        dungeonManager.CreateRoom(startPos, Room.RoomType.Start);
        
        // Generate main path
        List<Vector2Int> mainPath = GenerateMainPath(startPos);
        
        // Generate branches from main path
        GenerateBranches(mainPath);
        
        // Connect all rooms
        ConnectAllRooms();
        
        Debug.Log("Dungeon generation completed!");
    }
    
    private List<Vector2Int> GenerateMainPath(Vector2Int startPos)
    {
        List<Vector2Int> path = new List<Vector2Int> { startPos };
        Vector2Int currentPos = startPos;
        int targetRooms = Random.Range(minRooms, maxRooms + 1);
        
        for (int i = 1; i < targetRooms; i++)
        {
            Vector2Int nextPos = GetNextValidPosition(currentPos, path);
            if (nextPos == currentPos) break; // No valid position found
            
            Room.RoomType roomType = DetermineRoomType(i, targetRooms, path);
            dungeonManager.CreateRoom(nextPos, roomType);
            path.Add(nextPos);
            currentPos = nextPos;
        }
        
        return path;
    }
    
    private Vector2Int GetNextValidPosition(Vector2Int currentPos, List<Vector2Int> existingPath)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = currentPos + dir;
            if (IsValidRoomPosition(newPos) && !existingPath.Contains(newPos))
            {
                validPositions.Add(newPos);
            }
        }
        
        if (validPositions.Count == 0) return currentPos;
        return validPositions[Random.Range(0, validPositions.Count)];
    }
    
    private bool IsValidRoomPosition(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= dungeonWidth || 
            pos.y < 0 || pos.y >= dungeonHeight)
            return false;
            
        // Check if position is already occupied
        if (dungeonManager.GetRoomAt(pos) != null)
            return false;
            
        return true;
    }
    
    private Room.RoomType DetermineRoomType(int currentRoom, int totalRooms, List<Vector2Int> path)
    {
        // Boss room at the end
        if (currentRoom == totalRooms - 1)
            return Room.RoomType.Boss;
            
        // Don't generate special rooms too early in the path
        if (currentRoom < 2)
            return Room.RoomType.Normal;
            
        float roll = Random.value;
        
        // Higher chance for treasure rooms later in the path
        if (roll < treasureRoomChance * (currentRoom / (float)totalRooms))
            return Room.RoomType.Treasure;
            
        // Shop rooms in the middle section of the dungeon
        if (roll < shopRoomChance && currentRoom > totalRooms / 3 && currentRoom < totalRooms * 2/3)
            return Room.RoomType.Shop;
            
        return Room.RoomType.Normal;
    }
    
    private void GenerateBranches(List<Vector2Int> mainPath)
    {
        List<Vector2Int> processedPositions = new List<Vector2Int>(mainPath);
        int branchesCreated = 0;
        
        // Try to create branches from each position in main path except start and boss rooms
        for (int i = 1; i < mainPath.Count - 1; i++)
        {
            if (Random.value > branchChance) continue;
            
            Vector2Int branchStart = mainPath[i];
            Vector2Int branchPos = GetNextValidPosition(branchStart, processedPositions);
            
            if (branchPos != branchStart)
            {
                Room.RoomType branchType = DetermineBranchRoomType();
                dungeonManager.CreateRoom(branchPos, branchType);
                processedPositions.Add(branchPos);
                branchesCreated++;
            }
        }
        
        Debug.Log($"Created {branchesCreated} branch rooms");
    }
    
    private Room.RoomType DetermineBranchRoomType()
    {
        float roll = Random.value;
        
        if (roll < 0.3f && !dungeonManager.HasRoomOfType(Room.RoomType.Treasure))
            return Room.RoomType.Treasure;
        else if (roll < 0.5f && !dungeonManager.HasRoomOfType(Room.RoomType.Shop))
            return Room.RoomType.Shop;
            
        return Room.RoomType.Normal;
    }
    
    private void ConnectAllRooms()
    {
        Debug.Log("Connecting all rooms...");
        
        // Kết nối các phòng Start và Boss trước
        foreach (Room room in dungeonManager.GetAllRooms())
        {
            if (room.roomType == Room.RoomType.Start || room.roomType == Room.RoomType.Boss)
            {
                ConnectSpecialRoom(room);
            }
        }
        
        // Kết nối các phòng bình thường
        foreach (Room room in dungeonManager.GetAllRooms())
        {
            if (room.roomType != Room.RoomType.Start && room.roomType != Room.RoomType.Boss)
            {
                ConnectNormalRoom(room);
            }
        }
    }
    
    private void ConnectSpecialRoom(Room room)
    {
        // Chỉ kết nối phòng Start và Boss với 1 phòng khác
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        ShuffleDirections(directions);
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = room.gridPosition + dir;
            Room neighbor = dungeonManager.GetRoomAt(neighborPos);
            
            if (neighbor != null && !room.connectedRooms.Contains(neighbor))
            {
                Debug.Log($"Connecting {room.roomType} room at {room.gridPosition} to {neighbor.roomType} room at {neighbor.gridPosition}");
                dungeonManager.CreateDoorConnection(room, neighbor, dir);
                return; // Chỉ tạo 1 kết nối rồi thoát
            }
        }
    }
    
    private void ConnectNormalRoom(Room room)
    {
        // Kết nối phòng bình thường với tất cả các phòng kề nhau
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = room.gridPosition + dir;
            Room neighbor = dungeonManager.GetRoomAt(neighborPos);
            
            if (neighbor != null && !room.connectedRooms.Contains(neighbor))
            {
                Debug.Log($"Connecting normal room at {room.gridPosition} to room at {neighbor.gridPosition} in direction {dir}");
                dungeonManager.CreateDoorConnection(room, neighbor, dir);
            }
        }
        
        Debug.Log($"Connected room at {room.gridPosition} to {room.connectedRooms.Count} other rooms");
    }
    
    // Phương thức để đảo ngẫu nhiên thứ tự các hướng
    private void ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
    }
}