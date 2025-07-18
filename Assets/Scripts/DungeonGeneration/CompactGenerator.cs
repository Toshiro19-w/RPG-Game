using UnityEngine;
using System.Collections.Generic;

public class CompactGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    public int gridSize = 5;
    public int roomCount = 12;
    public float treasureRoomChance = 0.15f;
    public float shopRoomChance = 0.1f;
    public float secretRoomChance = 0.05f;
    public int dungeonSeed = 0;
    public bool useRandomSeed = true;
    
    [Header("References")]
    public DungeonManager dungeonManager;
    public RoomTemplateManager roomTemplates;
    
    private Room[,] roomGrid;
    private List<Vector2Int> roomPositions = new List<Vector2Int>();
    private Vector2Int startRoomPos;
    private Vector2Int bossRoomPos;
    
    void Awake()
    {
        if (dungeonManager == null)
            dungeonManager = GetComponent<DungeonManager>();
            
        if (useRandomSeed)
            dungeonSeed = Random.Range(1, 100000);
            
        Random.InitState(dungeonSeed);
    }
    
    public void GenerateDungeon()
    {
        // Initialize room grid
        roomGrid = new Room[gridSize, gridSize];
        roomPositions.Clear();
        
        // Generate Soul Knight style layout
        GenerateSoulKnightLayout();
        
        // Connect rooms
        ConnectRooms();
        
        Debug.Log($"Soul Knight style dungeon generated with {roomPositions.Count} rooms");
    }
    
    private void GenerateSoulKnightLayout()
    {
        // 1. Place start room in the center
        startRoomPos = new Vector2Int(gridSize / 2, gridSize / 2);
        CreateRoom(startRoomPos, Room.RoomType.Start);
        roomPositions.Add(startRoomPos);
        
        // 2. Create random rooms around
        int roomsToCreate = Mathf.Min(roomCount, gridSize * gridSize - 1);
        int roomsCreated = 1; // Already created start room
        
        // List of positions that can be expanded
        List<Vector2Int> expandablePositions = new List<Vector2Int> { startRoomPos };
        
        while (roomsCreated < roomsToCreate && expandablePositions.Count > 0)
        {
            // Choose a random position to expand from
            int randomIndex = Random.Range(0, expandablePositions.Count);
            Vector2Int currentPos = expandablePositions[randomIndex];
            
            // Find valid directions to expand
            List<Vector2Int> validDirections = GetValidDirections(currentPos);
            
            if (validDirections.Count == 0)
            {
                // No more directions to expand, remove this position
                expandablePositions.RemoveAt(randomIndex);
                continue;
            }
            
            // Choose a random direction
            Vector2Int direction = validDirections[Random.Range(0, validDirections.Count)];
            Vector2Int newPos = currentPos + direction;
            
            // Determine room type
            Room.RoomType roomType = DetermineRoomType(roomsCreated, roomsToCreate);
            
            // Create new room
            CreateRoom(newPos, roomType);
            roomPositions.Add(newPos);
            expandablePositions.Add(newPos);
            roomsCreated++;
            
            // If it's a boss room, save its position
            if (roomType == Room.RoomType.Boss)
            {
                bossRoomPos = newPos;
            }
        }
        
        // 3. Ensure there's a boss room
        EnsureBossRoom();
        
        // 4. Create additional special rooms (shop, treasure)
        CreateSpecialRooms();
    }
    
    private Room.RoomType DetermineRoomType(int currentRoomCount, int totalRooms)
    {
        // Boss room will be created when reaching 70-80% of rooms
        if (currentRoomCount >= totalRooms * 0.7f && currentRoomCount <= totalRooms * 0.8f && !HasRoomOfType(Room.RoomType.Boss))
        {
            return Room.RoomType.Boss;
        }
        
        // Chance to create special rooms
        float roll = Random.value;
        
        if (roll < treasureRoomChance && !HasRoomOfType(Room.RoomType.Treasure))
            return Room.RoomType.Treasure;
        else if (roll < treasureRoomChance + shopRoomChance && !HasRoomOfType(Room.RoomType.Shop))
            return Room.RoomType.Shop;
            
        return Room.RoomType.Normal;
    }
    
    private void EnsureBossRoom()
    {
        if (!HasRoomOfType(Room.RoomType.Boss))
        {
            // Find the farthest room from start room to place boss
            Vector2Int farthestPos = FindFarthestRoomFrom(startRoomPos);
            
            if (farthestPos != startRoomPos)
            {
                Room room = roomGrid[farthestPos.x, farthestPos.y];
                room.roomType = Room.RoomType.Boss;
                bossRoomPos = farthestPos;
                Debug.Log($"Set boss room at position {farthestPos}");
            }
        }
    }
    
    private void CreateSpecialRooms()
    {
        // Ensure there's at least one shop and one treasure room
        if (!HasRoomOfType(Room.RoomType.Shop))
        {
            CreateSpecialRoom(Room.RoomType.Shop);
        }
        
        if (!HasRoomOfType(Room.RoomType.Treasure))
        {
            CreateSpecialRoom(Room.RoomType.Treasure);
        }
    }
    
    private void CreateSpecialRoom(Room.RoomType roomType)
    {
        // Find a normal room to convert to special room
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            if (room.roomType == Room.RoomType.Normal)
            {
                room.roomType = roomType;
                Debug.Log($"Created {roomType} room at position {pos}");
                return;
            }
        }
    }
    
    private Vector2Int FindFarthestRoomFrom(Vector2Int sourcePos)
    {
        Vector2Int farthestPos = sourcePos;
        float maxDistance = 0;
        
        foreach (Vector2Int pos in roomPositions)
        {
            float distance = Vector2Int.Distance(sourcePos, pos);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestPos = pos;
            }
        }
        
        return farthestPos;
    }
    
    private bool HasRoomOfType(Room.RoomType type)
    {
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            if (room != null && room.roomType == type)
                return true;
        }
        return false;
    }
    
    private List<Vector2Int> GetValidDirections(Vector2Int pos)
    {
        List<Vector2Int> validDirections = new List<Vector2Int>();
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = pos + dir;
            
            if (IsValidPosition(newPos) && roomGrid[newPos.x, newPos.y] == null)
            {
                validDirections.Add(dir);
            }
        }
        
        return validDirections;
    }
    
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }
    
    private void CreateRoom(Vector2Int pos, Room.RoomType roomType)
    {
        // Use DungeonManager to create room
        dungeonManager.CreateRoom(pos, roomType);
        roomGrid[pos.x, pos.y] = dungeonManager.GetRoomAt(pos);
        Debug.Log($"Created {roomType} room at position {pos}");
    }
    
    private void ConnectRooms()
    {
        // Connect adjacent rooms
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            
            // Check 4 directions
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = pos + dir;
                
                if (IsValidPosition(neighborPos) && roomGrid[neighborPos.x, neighborPos.y] != null)
                {
                    Room neighbor = roomGrid[neighborPos.x, neighborPos.y];
                    dungeonManager.CreateDoorConnection(room, neighbor, dir);
                }
            }
        }
    }
    
    // Method to generate minimap like Soul Knight
    public Texture2D GenerateMinimap(int pixelsPerRoom = 10)
    {
        int mapSize = gridSize * pixelsPerRoom;
        Texture2D minimap = new Texture2D(mapSize, mapSize);
        
        // Set all pixels to transparent
        Color[] pixels = new Color[mapSize * mapSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        minimap.SetPixels(pixels);
        
        // Draw rooms
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            Color roomColor = GetRoomColor(room.roomType);
            
            // Draw room
            int startX = pos.x * pixelsPerRoom;
            int startY = pos.y * pixelsPerRoom;
            
            for (int x = 0; x < pixelsPerRoom; x++)
            {
                for (int y = 0; y < pixelsPerRoom; y++)
                {
                    // Leave border
                    if (x == 0 || y == 0 || x == pixelsPerRoom - 1 || y == pixelsPerRoom - 1)
                    {
                        minimap.SetPixel(startX + x, startY + y, Color.black);
                    }
                    else
                    {
                        minimap.SetPixel(startX + x, startY + y, roomColor);
                    }
                }
            }
            
            // Draw doors
            foreach (Door door in room.doors)
            {
                if (door.gameObject.activeSelf)
                {
                    // Determine door direction
                    Vector3 doorDir = door.transform.position - room.transform.position;
                    Vector2Int direction = new Vector2Int(
                        Mathf.RoundToInt(doorDir.x),
                        Mathf.RoundToInt(doorDir.y)
                    );
                    
                    // Draw door
                    int doorX = startX + pixelsPerRoom / 2;
                    int doorY = startY + pixelsPerRoom / 2;
                    
                    if (direction.x > 0) // Right door
                    {
                        doorX = startX + pixelsPerRoom - 1;
                    }
                    else if (direction.x < 0) // Left door
                    {
                        doorX = startX;
                    }
                    else if (direction.y > 0) // Top door
                    {
                        doorY = startY + pixelsPerRoom - 1;
                    }
                    else if (direction.y < 0) // Bottom door
                    {
                        doorY = startY;
                    }
                    
                    minimap.SetPixel(doorX, doorY, Color.white);
                }
            }
        }
        
        minimap.Apply();
        return minimap;
    }
    
    private Color GetRoomColor(Room.RoomType roomType)
    {
        switch (roomType)
        {
            case Room.RoomType.Start:
                return Color.green;
            case Room.RoomType.Boss:
                return Color.red;
            case Room.RoomType.Treasure:
                return Color.yellow;
            case Room.RoomType.Shop:
                return new Color(0.5f, 0.5f, 1f); // Light blue
            default:
                return Color.gray;
        }
    }
}