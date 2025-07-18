using UnityEngine;
using System.Collections.Generic;

public class CompactDungeonGenerator : MonoBehaviour
{
    [Header("Generator References")]
    public DungeonManager dungeonManager;
    public RoomTemplateManager roomTemplates;
    
    [Header("Generation Settings")]
    public int dungeonSeed = 0;
    public bool useRandomSeed = true;
    public int dungeonLevel = 1;
    
    [Header("Soul Knight Style Settings")]
    [Range(3, 8)]
    public int gridSize = 5; // Kích thước lưới (5x5, 6x6, etc.)
    [Range(5, 20)]
    public int roomCount = 12; // Số lượng phòng
    [Range(0f, 1f)]
    public float treasureRoomChance = 0.15f;
    [Range(0f, 1f)]
    public float shopRoomChance = 0.1f;
    [Range(0f, 1f)]
    public float secretRoomChance = 0.05f;
    
    // Mảng 2D để theo dõi các phòng
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
        // Khởi tạo lưới phòng
        roomGrid = new Room[gridSize, gridSize];
        roomPositions.Clear();
        startRoomPos = default;
        bossRoomPos = default;
        
        // Tạo cấu trúc dungeon kiểu Soul Knight
        GenerateSoulKnightLayout();
        
        // Kết nối các phòng
        ConnectRooms();
        
        Debug.Log($"Soul Knight style dungeon generated with {roomPositions.Count} rooms");
    }
    
    private void GenerateSoulKnightLayout()
    {
        // 1. Đặt phòng start ở giữa lưới nếu chưa có
        if (startRoomPos == default)
        {
            startRoomPos = new Vector2Int(gridSize / 2, gridSize / 2);
            CreateRoom(startRoomPos, Room.RoomType.Start);
            roomPositions.Add(startRoomPos);
        }
        
        // 2. Tạo các phòng ngẫu nhiên xung quanh
        int roomsToCreate = Mathf.Min(roomCount, gridSize * gridSize - 1);
        int roomsCreated = 1; // Đã tạo phòng start
        
        // Danh sách các vị trí có thể mở rộng
        List<Vector2Int> expandablePositions = new List<Vector2Int> { startRoomPos };
        
        while (roomsCreated < roomsToCreate && expandablePositions.Count > 0)
        {
            // Chọn một vị trí ngẫu nhiên để mở rộng
            int randomIndex = Random.Range(0, expandablePositions.Count);
            Vector2Int currentPos = expandablePositions[randomIndex];
            
            // Tìm các hướng có thể mở rộng
            List<Vector2Int> validDirections = GetValidDirections(currentPos);
            
            if (validDirections.Count == 0)
            {
                // Không còn hướng để mở rộng, loại bỏ vị trí này
                expandablePositions.RemoveAt(randomIndex);
                continue;
            }
            
            // Chọn một hướng ngẫu nhiên
            Vector2Int direction = validDirections[Random.Range(0, validDirections.Count)];
            Vector2Int newPos = currentPos + direction;
            
            // Xác định loại phòng
            Room.RoomType roomType = DetermineRoomType(roomsCreated, roomsToCreate);
            
            // Tạo phòng mới
            CreateRoom(newPos, roomType);
            roomPositions.Add(newPos);
            expandablePositions.Add(newPos);
            roomsCreated++;
            
            // Nếu là phòng boss, lưu lại vị trí
            if (roomType == Room.RoomType.Boss)
            {
                bossRoomPos = newPos;
            }
        }
        
        // 3. Đảm bảo có phòng boss
        EnsureBossRoom();
        
        // 4. Tạo thêm các phòng đặc biệt (shop, treasure)
        CreateSpecialRooms();
    }
    
    private Room.RoomType DetermineRoomType(int currentRoomCount, int totalRooms)
    {
        // Phòng boss sẽ được tạo khi đạt đến 70-80% số phòng và chưa có phòng boss
        if (currentRoomCount >= totalRooms * 0.7f && currentRoomCount <= totalRooms * 0.8f && !HasRoomOfType(Room.RoomType.Boss) && bossRoomPos == default)
        {
            return Room.RoomType.Boss;
        }
        
        // Cơ hội tạo phòng đặc biệt
        float roll = Random.value;
        
        if (roll < treasureRoomChance && !HasRoomOfType(Room.RoomType.Treasure))
            return Room.RoomType.Treasure;
        else if (roll < treasureRoomChance + shopRoomChance && !HasRoomOfType(Room.RoomType.Shop))
            return Room.RoomType.Shop;
        
        return Room.RoomType.Normal;
    }
    
    private void EnsureBossRoom()
    {
        if (!HasRoomOfType(Room.RoomType.Boss) && bossRoomPos == default)
        {
            // Tìm phòng xa nhất từ phòng start để đặt boss
            Vector2Int farthestPos = FindFarthestRoomFrom(startRoomPos);
            
            if (farthestPos != startRoomPos)
            {
                Room room = roomGrid[farthestPos.x, farthestPos.y];
                if (room != null && room.roomType != Room.RoomType.Start)
                {
                    room.roomType = Room.RoomType.Boss;
                    bossRoomPos = farthestPos;
                    Debug.Log($"Set boss room at position {farthestPos}");
                }
            }
        }
    }
    
    private void CreateSpecialRooms()
    {
        // Đảm bảo có ít nhất một phòng shop và một phòng treasure
        // Chỉ tạo nếu có đủ phòng normal để chuyển đổi
        int normalRoomCount = CountRoomsOfType(Room.RoomType.Normal);
        int specialRoomsNeeded = 0;
        
        if (!HasRoomOfType(Room.RoomType.Shop))
            specialRoomsNeeded++;
            
        if (!HasRoomOfType(Room.RoomType.Treasure))
            specialRoomsNeeded++;
            
        // Chỉ tạo nếu có đủ phòng normal
        if (normalRoomCount >= specialRoomsNeeded)
        {
            if (!HasRoomOfType(Room.RoomType.Shop))
            {
                CreateSpecialRoom(Room.RoomType.Shop);
            }
            
            if (!HasRoomOfType(Room.RoomType.Treasure))
            {
                CreateSpecialRoom(Room.RoomType.Treasure);
            }
        }
        else
        {
            Debug.LogWarning($"Not enough normal rooms ({normalRoomCount}) to create special rooms ({specialRoomsNeeded})");
        }
    }
    
    private int CountRoomsOfType(Room.RoomType type)
    {
        int count = 0;
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            if (room != null && room.roomType == type)
                count++;
        }
        return count;
    }
    
    private void CreateSpecialRoom(Room.RoomType roomType)
    {
        // Tìm một phòng normal để chuyển thành phòng đặc biệt
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
        // Sử dụng DungeonManager để tạo phòng
        dungeonManager.CreateRoom(pos, roomType);
        roomGrid[pos.x, pos.y] = dungeonManager.GetRoomAt(pos);
        Debug.Log($"Created {roomType} room at position {pos}");
    }
    
    private void ConnectRooms()
    {
        // Kết nối các phòng liền kề
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            
            // Kiểm tra 4 hướng
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
    
    // Phương thức để tạo minimap giống Soul Knight
    public Texture2D GenerateMinimap(int pixelsPerRoom = 10)
    {
        int mapSize = gridSize * pixelsPerRoom;
        Texture2D minimap = new Texture2D(mapSize, mapSize);
        
        // Đặt tất cả các pixel thành trong suốt
        Color[] pixels = new Color[mapSize * mapSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        minimap.SetPixels(pixels);
        
        // Vẽ các phòng
        foreach (Vector2Int pos in roomPositions)
        {
            Room room = roomGrid[pos.x, pos.y];
            Color roomColor = GetRoomColor(room.roomType);
            
            // Vẽ phòng
            int startX = pos.x * pixelsPerRoom;
            int startY = pos.y * pixelsPerRoom;
            
            for (int x = 0; x < pixelsPerRoom; x++)
            {
                for (int y = 0; y < pixelsPerRoom; y++)
                {
                    // Để lại viền
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
            
            // Vẽ các cửa
            foreach (Door door in room.doors)
            {
                if (door.gameObject.activeSelf)
                {
                    // Xác định hướng cửa
                    Vector3 doorDir = door.transform.position - room.transform.position;
                    Vector2Int direction = new Vector2Int(
                        Mathf.RoundToInt(doorDir.x),
                        Mathf.RoundToInt(doorDir.y)
                    );
                    
                    // Vẽ cửa
                    int doorX = startX + pixelsPerRoom / 2;
                    int doorY = startY + pixelsPerRoom / 2;
                    
                    if (direction.x > 0) // Cửa bên phải
                    {
                        doorX = startX + pixelsPerRoom - 1;
                    }
                    else if (direction.x < 0) // Cửa bên trái
                    {
                        doorX = startX;
                    }
                    else if (direction.y > 0) // Cửa phía trên
                    {
                        doorY = startY + pixelsPerRoom - 1;
                    }
                    else if (direction.y < 0) // Cửa phía dưới
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