using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CompactRoomGenerator : MonoBehaviour
{
    [Header("Room Templates")]
    public GameObject normalRoomPrefab;
    public GameObject bossRoomPrefab;
    public GameObject treasureRoomPrefab;
    public GameObject shopRoomPrefab;
    public GameObject startRoomPrefab;
    
    [Header("Room Variants")]
    public List<GameObject> normalRoomVariants = new List<GameObject>();
    public List<GameObject> bossRoomVariants = new List<GameObject>();
    public List<GameObject> treasureRoomVariants = new List<GameObject>();
    public List<GameObject> shopRoomVariants = new List<GameObject>();
    public List<GameObject> startRoomVariants = new List<GameObject>();
    
    [Header("Room Settings")]
    public int roomWidth = 16;
    public int roomHeight = 10;
    public float roomSpacing = 20f;
    
    [Header("Obstacle Settings")]
    public GameObject[] obstaclesPrefabs;
    public GameObject[] destructiblePrefabs;
    [Range(0, 10)]
    public int minObstacles = 2;
    [Range(0, 20)]
    public int maxObstacles = 8;
    
    [Header("Door Settings")]
    public GameObject doorPrefab;
    
    private Dictionary<Room.RoomType, List<GameObject>> roomTemplates = new Dictionary<Room.RoomType, List<GameObject>>();
    
    void Awake()
    {
        // Khởi tạo từ điển mẫu phòng
        InitializeRoomTemplates();
    }
    
    private void InitializeRoomTemplates()
    {
        // Thêm các mẫu phòng vào từ điển
        roomTemplates[Room.RoomType.Normal] = new List<GameObject>(normalRoomVariants);
        if (normalRoomPrefab != null && !roomTemplates[Room.RoomType.Normal].Contains(normalRoomPrefab))
            roomTemplates[Room.RoomType.Normal].Add(normalRoomPrefab);
            
        roomTemplates[Room.RoomType.Boss] = new List<GameObject>(bossRoomVariants);
        if (bossRoomPrefab != null && !roomTemplates[Room.RoomType.Boss].Contains(bossRoomPrefab))
            roomTemplates[Room.RoomType.Boss].Add(bossRoomPrefab);
            
        roomTemplates[Room.RoomType.Treasure] = new List<GameObject>(treasureRoomVariants);
        if (treasureRoomPrefab != null && !roomTemplates[Room.RoomType.Treasure].Contains(treasureRoomPrefab))
            roomTemplates[Room.RoomType.Treasure].Add(treasureRoomPrefab);
            
        roomTemplates[Room.RoomType.Shop] = new List<GameObject>(shopRoomVariants);
        if (shopRoomPrefab != null && !roomTemplates[Room.RoomType.Shop].Contains(shopRoomPrefab))
            roomTemplates[Room.RoomType.Shop].Add(shopRoomPrefab);
            
        roomTemplates[Room.RoomType.Start] = new List<GameObject>(startRoomVariants);
        if (startRoomPrefab != null && !roomTemplates[Room.RoomType.Start].Contains(startRoomPrefab))
            roomTemplates[Room.RoomType.Start].Add(startRoomPrefab);
            
        // Đảm bảo mỗi loại phòng có ít nhất một mẫu
        foreach (var type in System.Enum.GetValues(typeof(Room.RoomType)))
        {
            Room.RoomType roomType = (Room.RoomType)type;
            if (!roomTemplates.ContainsKey(roomType) || roomTemplates[roomType].Count == 0)
            {
                roomTemplates[roomType] = new List<GameObject> { normalRoomPrefab };
                Debug.LogWarning($"No templates for {roomType} rooms, using normal room template instead.");
            }
        }
    }
    
    public GameObject GenerateRoom(Room.RoomType roomType, Vector2Int gridPosition, Dictionary<int, bool> doorDirections)
    {
        // Chọn mẫu phòng ngẫu nhiên dựa trên loại phòng
        GameObject roomTemplate = GetRandomRoomTemplate(roomType);
        
        if (roomTemplate == null)
        {
            Debug.LogError($"No template available for {roomType} room!");
            return null;
        }
        
        // Tính toán vị trí thế giới
        Vector3 worldPos = new Vector3(gridPosition.x * roomSpacing, gridPosition.y * roomSpacing, 0);
        
        // Tạo phòng từ mẫu
        GameObject roomObj = Instantiate(roomTemplate, worldPos, Quaternion.identity);
        roomObj.name = $"Room_{roomType}_{gridPosition.x}_{gridPosition.y}";
        
        // Thiết lập thành phần Room
        Room room = roomObj.GetComponent<Room>();
        if (room == null)
            room = roomObj.AddComponent<Room>();
            
        room.roomType = roomType;
        room.gridPosition = gridPosition;
        
        // Tạo cửa dựa trên hướng được chỉ định
        CreateDoors(roomObj, doorDirections);
        
        // Thêm chướng ngại vật ngẫu nhiên (trừ phòng start)
        if (roomType != Room.RoomType.Start)
        {
            AddRandomObstacles(roomObj, roomType);
        }
        
        return roomObj;
    }
    
    private GameObject GetRandomRoomTemplate(Room.RoomType roomType)
    {
        if (roomTemplates.ContainsKey(roomType) && roomTemplates[roomType].Count > 0)
        {
            int randomIndex = Random.Range(0, roomTemplates[roomType].Count);
            return roomTemplates[roomType][randomIndex];
        }
        
        // Fallback to normal room if no template is available
        if (normalRoomPrefab != null)
            return normalRoomPrefab;
            
        return null;
    }
    
    private void CreateDoors(GameObject roomObj, Dictionary<int, bool> doorDirections)
    {
        Room room = roomObj.GetComponent<Room>();
        
        // Vị trí cửa tương đối với phòng
        Vector3[] doorPositions = {
            new Vector3(0, roomHeight/2, 0),  // Top (0)
            new Vector3(roomWidth/2, 0, 0),   // Right (1)
            new Vector3(0, -roomHeight/2, 0), // Bottom (2)
            new Vector3(-roomWidth/2, 0, 0)   // Left (3)
        };
        
        // Tạo cửa ở các hướng được chỉ định
        for (int i = 0; i < 4; i++)
        {
            if (doorDirections.ContainsKey(i) && doorDirections[i])
            {
                // Tạo cửa
                GameObject doorObj = Instantiate(doorPrefab, roomObj.transform);
                doorObj.name = $"Door_{i}";
                doorObj.transform.localPosition = doorPositions[i];
                
                // Xoay cửa theo hướng
                float rotation = i * 90f;
                doorObj.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                
                // Thiết lập thành phần Door
                Door door = doorObj.GetComponent<Door>();
                if (door == null)
                    door = doorObj.AddComponent<Door>();
                    
                door.SetActive(false); // Cửa ban đầu đóng
                
                // Thêm vào danh sách cửa của phòng
                room.doors.Add(door);
            }
        }
    }
    
    private void AddRandomObstacles(GameObject roomObj, Room.RoomType roomType)
    {
        if (obstaclesPrefabs == null || obstaclesPrefabs.Length == 0)
            return;
            
        // Xác định số lượng chướng ngại vật dựa trên loại phòng
        int obstacleCount = roomType switch
        {
            Room.RoomType.Boss => Random.Range(minObstacles, minObstacles + 2),
            Room.RoomType.Treasure => Random.Range(minObstacles, maxObstacles / 2),
            Room.RoomType.Shop => Random.Range(minObstacles, maxObstacles / 3),
            _ => Random.Range(minObstacles, maxObstacles)
        };
        
        // Tạo một container cho các chướng ngại vật
        GameObject obstaclesContainer = new GameObject("Obstacles");
        obstaclesContainer.transform.SetParent(roomObj.transform);
        obstaclesContainer.transform.localPosition = Vector3.zero;
        
        // Danh sách vị trí đã sử dụng để tránh chồng chéo
        List<Vector2> usedPositions = new List<Vector2>();
        
        // Thêm vị trí trung tâm vào danh sách đã sử dụng
        usedPositions.Add(Vector2.zero);
        
        // Thêm vị trí cửa vào danh sách đã sử dụng
        Room room = roomObj.GetComponent<Room>();
        foreach (Door door in room.doors)
        {
            usedPositions.Add(new Vector2(door.transform.localPosition.x, door.transform.localPosition.y));
            
            // Thêm khu vực xung quanh cửa
            for (float x = -1.5f; x <= 1.5f; x += 0.5f)
            {
                for (float y = -1.5f; y <= 1.5f; y += 0.5f)
                {
                    usedPositions.Add(new Vector2(
                        door.transform.localPosition.x + x,
                        door.transform.localPosition.y + y
                    ));
                }
            }
        }
        
        // Tạo chướng ngại vật
        for (int i = 0; i < obstacleCount; i++)
        {
            // Chọn chướng ngại vật ngẫu nhiên
            GameObject obstaclePrefab = obstaclesPrefabs[Random.Range(0, obstaclesPrefabs.Length)];
            
            // Tìm vị trí hợp lệ
            Vector3 position = FindValidObstaclePosition(roomWidth, roomHeight, usedPositions);
            
            // Tạo chướng ngại vật
            GameObject obstacle = Instantiate(obstaclePrefab, obstaclesContainer.transform);
            obstacle.transform.localPosition = position;
            
            // Xoay ngẫu nhiên
            obstacle.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
            
            // Thêm vị trí vào danh sách đã sử dụng
            usedPositions.Add(new Vector2(position.x, position.y));
            
            // Thêm khu vực xung quanh chướng ngại vật
            float obstacleSize = 1.0f;
            for (float x = -obstacleSize; x <= obstacleSize; x += 0.5f)
            {
                for (float y = -obstacleSize; y <= obstacleSize; y += 0.5f)
                {
                    usedPositions.Add(new Vector2(position.x + x, position.y + y));
                }
            }
        }
        
        // Thêm một số chướng ngại vật có thể phá hủy (nếu có)
        if (destructiblePrefabs != null && destructiblePrefabs.Length > 0)
        {
            int destructibleCount = Random.Range(0, obstacleCount / 2);
            
            for (int i = 0; i < destructibleCount; i++)
            {
                GameObject destructiblePrefab = destructiblePrefabs[Random.Range(0, destructiblePrefabs.Length)];
                Vector3 position = FindValidObstaclePosition(roomWidth, roomHeight, usedPositions);
                
                GameObject destructible = Instantiate(destructiblePrefab, obstaclesContainer.transform);
                destructible.transform.localPosition = position;
                
                usedPositions.Add(new Vector2(position.x, position.y));
            }
        }
    }
    
    private Vector3 FindValidObstaclePosition(float width, float height, List<Vector2> usedPositions)
    {
        // Giới hạn khu vực đặt chướng ngại vật để tránh chặn lối đi
        float maxX = width / 2 - 1;
        float maxY = height / 2 - 1;
        
        // Số lần thử tối đa
        int maxAttempts = 50;
        int attempts = 0;
        
        while (attempts < maxAttempts)
        {
            // Tạo vị trí ngẫu nhiên
            float x = Random.Range(-maxX, maxX);
            float y = Random.Range(-maxY, maxY);
            Vector2 position = new Vector2(x, y);
            
            // Kiểm tra xem vị trí có hợp lệ không
            bool isValid = true;
            foreach (Vector2 usedPos in usedPositions)
            {
                if (Vector2.Distance(position, usedPos) < 1.5f)
                {
                    isValid = false;
                    break;
                }
            }
            
            if (isValid)
            {
                return new Vector3(x, y, 0);
            }
            
            attempts++;
        }
        
        // Nếu không tìm được vị trí hợp lệ, trả về vị trí ngẫu nhiên
        return new Vector3(
            Random.Range(-maxX, maxX),
            Random.Range(-maxY, maxY),
            0
        );
    }
}