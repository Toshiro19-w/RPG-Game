using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    [Header("Minimap Settings")]
    public RectTransform minimapParent;
    public GameObject roomIconPrefab;
    public float iconSize = 20f;
    public float iconSpacing = 25f;
    
    [Header("Room Colors")]
    public Color visitedColor = Color.white;
    public Color currentColor = Color.yellow;
    public Color clearedColor = Color.green;
    public Color bossColor = Color.red;
    public Color treasureColor = Color.gold;
    public Color shopColor = Color.blue;
    public Color unvisitedColor = Color.gray;
    
    private Dictionary<Room, GameObject> roomIcons = new Dictionary<Room, GameObject>();
    private Room currentRoom;
    
    void Start()
    {
        if (DungeonManager.Instance != null)
        {
            CreateMinimap();
            InvokeRepeating(nameof(UpdateMinimap), 0f, 0.5f);
        }
    }
    
    private void CreateMinimap()
    {
        List<Room> allRooms = DungeonManager.Instance.GetAllRooms();
        
        foreach (Room room in allRooms)
        {
            CreateRoomIcon(room);
        }
    }
    
    private void CreateRoomIcon(Room room)
    {
        GameObject icon = Instantiate(roomIconPrefab, minimapParent);
        
        // Đặt vị trí icon dựa trên grid position
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        Vector2 iconPos = new Vector2(
            room.gridPosition.x * iconSpacing,
            room.gridPosition.y * iconSpacing
        );
        iconRect.anchoredPosition = iconPos;
        iconRect.sizeDelta = Vector2.one * iconSize;
        
        roomIcons[room] = icon;
        UpdateRoomIcon(room);
    }
    
    private void UpdateMinimap()
    {
        if (DungeonManager.Instance == null) return;
        
        Room newCurrentRoom = DungeonManager.Instance.GetCurrentRoom();
        if (newCurrentRoom != currentRoom)
        {
            // Cập nhật room cũ
            if (currentRoom != null)
                UpdateRoomIcon(currentRoom);
            
            currentRoom = newCurrentRoom;
        }
        
        // Cập nhật tất cả room icons
        foreach (var kvp in roomIcons)
        {
            UpdateRoomIcon(kvp.Key);
        }
    }
    
    private void UpdateRoomIcon(Room room)
    {
        if (!roomIcons.ContainsKey(room)) return;
        
        GameObject icon = roomIcons[room];
        Image iconImage = icon.GetComponent<Image>();
        
        if (iconImage == null) return;
        
        // Xác định màu dựa trên trạng thái room
        Color iconColor = GetRoomColor(room);
        iconImage.color = iconColor;
        
        // Hiển thị icon chỉ khi đã thăm hoặc là current room
        bool shouldShow = room.isVisited || room == currentRoom;
        icon.SetActive(shouldShow);
    }
    
    private Color GetRoomColor(Room room)
    {
        // Current room luôn có màu vàng
        if (room == currentRoom)
            return currentColor;
        
        // Nếu chưa thăm
        if (!room.isVisited)
            return unvisitedColor;
        
        // Nếu đã clear
        if (room.isCleared)
        {
            return room.roomType switch
            {
                Room.RoomType.Boss => bossColor,
                Room.RoomType.Treasure => treasureColor,
                Room.RoomType.Shop => shopColor,
                _ => clearedColor
            };
        }
        
        // Đã thăm nhưng chưa clear
        return visitedColor;
    }
}