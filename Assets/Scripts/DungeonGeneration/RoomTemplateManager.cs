using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoomTemplateManager", menuName = "Dungeon/Room Template Manager")]
public class RoomTemplateManager : ScriptableObject
{
    [System.Serializable]
    public class RoomTemplateCollection
    {
        public string collectionName;
        public GameObject[] templates;
    }
    
    [Header("Room Templates")]
    public RoomTemplateCollection normalRooms;
    public RoomTemplateCollection bossRooms;
    public RoomTemplateCollection treasureRooms;
    public RoomTemplateCollection shopRooms;
    public RoomTemplateCollection startRooms;
    
    [Header("Special Rooms")]
    public GameObject defaultNormalRoom;
    public GameObject defaultBossRoom;
    public GameObject defaultTreasureRoom;
    public GameObject defaultShopRoom;
    public GameObject defaultStartRoom;
    
    // Lấy mẫu phòng ngẫu nhiên theo loại
    public GameObject GetRandomTemplate(Room.RoomType roomType)
    {
        RoomTemplateCollection collection = GetCollectionByType(roomType);
        
        if (collection == null || collection.templates == null || collection.templates.Length == 0)
        {
            return GetDefaultTemplate(roomType);
        }
        
        return collection.templates[Random.Range(0, collection.templates.Length)];
    }
    
    // Lấy mẫu phòng mặc định theo loại
    public GameObject GetDefaultTemplate(Room.RoomType roomType)
    {
        return roomType switch
        {
            Room.RoomType.Normal => defaultNormalRoom,
            Room.RoomType.Boss => defaultBossRoom,
            Room.RoomType.Treasure => defaultTreasureRoom,
            Room.RoomType.Shop => defaultShopRoom,
            Room.RoomType.Start => defaultStartRoom,
            _ => defaultNormalRoom
        };
    }
    
    // Lấy collection theo loại phòng
    private RoomTemplateCollection GetCollectionByType(Room.RoomType roomType)
    {
        return roomType switch
        {
            Room.RoomType.Normal => normalRooms,
            Room.RoomType.Boss => bossRooms,
            Room.RoomType.Treasure => treasureRooms,
            Room.RoomType.Shop => shopRooms,
            Room.RoomType.Start => startRooms,
            _ => normalRooms
        };
    }
}