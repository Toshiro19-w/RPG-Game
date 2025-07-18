using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompactMinimap : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RawImage minimapImage;
    public RectTransform playerMarker;
    public int pixelsPerRoom = 10;
    public float updateInterval = 0.5f;
    
    [Header("Room Colors")]
    public Color startRoomColor = Color.green;
    public Color bossRoomColor = Color.red;
    public Color treasureRoomColor = Color.yellow;
    public Color shopRoomColor = new Color(0.5f, 0.5f, 1f);
    public Color normalRoomColor = Color.gray;
    public Color currentRoomColor = Color.white;
    public Color unexploredRoomColor = new Color(0.2f, 0.2f, 0.2f);
    
    private CompactDungeonGenerator dungeonGenerator;
    private DungeonManager dungeonManager;
    private Texture2D minimapTexture;
    private float updateTimer;
    private Room currentRoom;
    private Dictionary<Room, Vector2Int> roomPositions = new Dictionary<Room, Vector2Int>();
    
    void Start()
    {
        dungeonGenerator = FindFirstObjectByType<CompactDungeonGenerator>();
        dungeonManager = FindFirstObjectByType<DungeonManager>();
        
        if (dungeonGenerator == null || dungeonManager == null)
        {
            Debug.LogError("CompactMinimap: Missing required components!");
            enabled = false;
            return;
        }
        
        // Khởi tạo minimap sau khi dungeon được tạo
        Invoke("InitializeMinimap", 1.0f);
    }
    
    public void InitializeMinimap()
    {
        if (minimapImage == null)
        {
            Debug.LogError("SoulKnightMinimap: Missing minimap image!");
            return;
        }
        
        // Tạo texture cho minimap
        int gridSize = dungeonGenerator.gridSize;
        int mapSize = gridSize * pixelsPerRoom;
        minimapTexture = new Texture2D(mapSize, mapSize);
        minimapTexture.filterMode = FilterMode.Point; // Pixel perfect
        
        // Đặt tất cả các pixel thành trong suốt
        Color[] pixels = new Color[mapSize * mapSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        minimapTexture.SetPixels(pixels);
        minimapTexture.Apply();
        
        // Gán texture cho UI
        minimapImage.texture = minimapTexture;
        
        // Lưu vị trí các phòng
        StoreRoomPositions();
        
        // Cập nhật minimap lần đầu
        UpdateMinimap();
    }
    
    void StoreRoomPositions()
    {
        roomPositions.Clear();
        
        List<Room> allRooms = dungeonManager.GetAllRooms();
        foreach (Room room in allRooms)
        {
            roomPositions[room] = room.gridPosition;
        }
    }
    
    void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0;
            UpdateMinimap();
        }
        
        // Cập nhật vị trí marker người chơi
        UpdatePlayerMarker();
    }
    
    void UpdateMinimap()
    {
        if (minimapTexture == null || dungeonManager == null)
            return;
        
        // Xóa minimap
        int mapSize = dungeonGenerator.gridSize * pixelsPerRoom;
        Color[] pixels = new Color[mapSize * mapSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        minimapTexture.SetPixels(pixels);
        
        // Lấy phòng hiện tại
        currentRoom = dungeonManager.GetCurrentRoom();
        
        // Vẽ các phòng đã khám phá
        List<Room> allRooms = dungeonManager.GetAllRooms();
        foreach (Room room in allRooms)
        {
            if (room.isVisited || ShouldShowUnexploredRoom(room))
            {
                DrawRoomOnMinimap(room);
            }
        }
        
        minimapTexture.Apply();
    }
    
    bool ShouldShowUnexploredRoom(Room room)
    {
        // Hiển thị các phòng chưa khám phá nhưng kết nối với phòng đã khám phá
        if (currentRoom != null)
        {
            foreach (Room connectedRoom in currentRoom.connectedRooms)
            {
                if (room == connectedRoom)
                    return true;
            }
        }
        return false;
    }
    
    void DrawRoomOnMinimap(Room room)
    {
        if (!roomPositions.TryGetValue(room, out Vector2Int pos))
            return;
        
        // Xác định màu phòng
        Color roomColor;
        
        if (room == currentRoom)
        {
            roomColor = currentRoomColor;
        }
        else if (!room.isVisited)
        {
            roomColor = unexploredRoomColor;
        }
        else
        {
            roomColor = GetRoomColor(room.roomType);
        }
        
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
                    minimapTexture.SetPixel(startX + x, startY + y, Color.black);
                }
                else
                {
                    minimapTexture.SetPixel(startX + x, startY + y, roomColor);
                }
            }
        }
        
        // Vẽ các cửa cho phòng đã khám phá
        if (room.isVisited)
        {
            foreach (Door door in room.doors)
            {
                if (door.gameObject.activeSelf)
                {
                    // Xác định hướng cửa
                    Vector3 doorDir = door.transform.position - room.transform.position;
                    Vector2Int direction = new Vector2Int(
                        Mathf.RoundToInt(doorDir.normalized.x),
                        Mathf.RoundToInt(doorDir.normalized.y)
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
                    
                    minimapTexture.SetPixel(doorX, doorY, Color.white);
                }
            }
        }
    }
    
    void UpdatePlayerMarker()
    {
        if (playerMarker == null || currentRoom == null)
            return;
        
        // Tính toán vị trí marker dựa trên phòng hiện tại
        if (roomPositions.TryGetValue(currentRoom, out Vector2Int pos))
        {
            float normalizedX = (float)(pos.x + 0.5f) / dungeonGenerator.gridSize;
            float normalizedY = (float)(pos.y + 0.5f) / dungeonGenerator.gridSize;
            
            // Chuyển đổi sang tọa độ UI (0-1)
            playerMarker.anchorMin = new Vector2(normalizedX, normalizedY);
            playerMarker.anchorMax = new Vector2(normalizedX, normalizedY);
            playerMarker.anchoredPosition = Vector2.zero;
        }
    }
    
    Color GetRoomColor(Room.RoomType roomType)
    {
        switch (roomType)
        {
            case Room.RoomType.Start:
                return startRoomColor;
            case Room.RoomType.Boss:
                return bossRoomColor;
            case Room.RoomType.Treasure:
                return treasureRoomColor;
            case Room.RoomType.Shop:
                return shopRoomColor;
            default:
                return normalRoomColor;
        }
    }
    
    // Phương thức để hiển thị/ẩn minimap
    public void ToggleMinimap(bool show)
    {
        if (minimapImage != null)
            minimapImage.gameObject.SetActive(show);
        
        if (playerMarker != null)
            playerMarker.gameObject.SetActive(show);
    }
}