using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapRoomGenerator : MonoBehaviour
{
    [Header("Room Templates")]
    public RoomTemplateManager templateManager;
    public DungeonTheme currentTheme;
    
    [Header("Room Settings")]
    public int roomWidth = 16;
    public int roomHeight = 10;
    public float decorationDensity = 0.1f;
    
    [Header("Door Settings")]
    public GameObject doorPrefab;
    
    public GameObject GenerateRoom(Room.RoomType roomType, Vector2Int gridPosition, Dictionary<int, bool> doorDirections)
    {
        // Get room template based on room type
        GameObject roomTemplate = null;
        
        if (templateManager != null)
        {
            roomTemplate = templateManager.GetRandomTemplate(roomType);
        }
        
        if (roomTemplate == null)
        {
            // Create a basic room if no template is available
            roomTemplate = CreateBasicRoom(roomType);
        }
        
        // Instantiate room
        GameObject roomObj = Instantiate(roomTemplate);
        roomObj.name = $"Room_{roomType}_{gridPosition.x}_{gridPosition.y}";
        
        // Setup room component
        Room room = roomObj.GetComponent<Room>();
        if (room == null)
        {
            room = roomObj.AddComponent<Room>();
        }
        
        room.roomType = roomType;
        room.gridPosition = gridPosition;
        
        // Create doors based on directions
        CreateDoors(roomObj, doorDirections);
        
        // Add enemies and props based on room type
        PopulateRoom(roomObj, roomType);
        
        return roomObj;
    }
    
    private GameObject CreateBasicRoom(Room.RoomType roomType)
    {
        // Create a new game object for the room
        GameObject roomObj = new GameObject($"BasicRoom_{roomType}");
        
        // Add tilemaps for floor, walls, and decorations
        Transform tilemapsParent = new GameObject("Tilemaps").transform;
        tilemapsParent.SetParent(roomObj.transform);
        
        // Create floor tilemap
        GameObject floorObj = new GameObject("Floor");
        floorObj.transform.SetParent(tilemapsParent);
        Tilemap floorTilemap = floorObj.AddComponent<Tilemap>();
        TilemapRenderer floorRenderer = floorObj.AddComponent<TilemapRenderer>();
        floorRenderer.sortingOrder = 0;
        
        // Create wall tilemap
        GameObject wallObj = new GameObject("Walls");
        wallObj.transform.SetParent(tilemapsParent);
        Tilemap wallTilemap = wallObj.AddComponent<Tilemap>();
        TilemapRenderer wallRenderer = wallObj.AddComponent<TilemapRenderer>();
        wallRenderer.sortingOrder = 5;
        wallObj.AddComponent<TilemapCollider2D>();
        
        // Create decoration tilemap
        GameObject decorObj = new GameObject("Decorations");
        decorObj.transform.SetParent(tilemapsParent);
        Tilemap decorTilemap = decorObj.AddComponent<Tilemap>();
        TilemapRenderer decorRenderer = decorObj.AddComponent<TilemapRenderer>();
        decorRenderer.sortingOrder = 2;
        
        // Add Room component
        Room room = roomObj.AddComponent<Room>();
        room.roomType = roomType;
        
        // Create spawn points for enemies
        GameObject spawnPointsObj = new GameObject("EnemySpawnPoints");
        spawnPointsObj.transform.SetParent(roomObj.transform);
        
        int spawnPointCount = roomType == Room.RoomType.Boss ? 1 : 4;
        Transform[] spawnPoints = new Transform[spawnPointCount];
        
        for (int i = 0; i < spawnPointCount; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.SetParent(spawnPointsObj.transform);
            
            // Position spawn points in different areas of the room
            float x = (i % 2 == 0) ? -roomWidth/4f : roomWidth/4f;
            float y = (i < 2) ? -roomHeight/4f : roomHeight/4f;
            
            // Boss in center
            if (roomType == Room.RoomType.Boss)
            {
                x = 0;
                y = 0;
            }
            
            spawnPoint.transform.localPosition = new Vector3(x, y, 0);
            spawnPoints[i] = spawnPoint.transform;
        }
        
        room.enemySpawnPoints = spawnPoints;
        
        // Create reward spawn point
        GameObject rewardSpawnObj = new GameObject("RewardSpawnPoint");
        rewardSpawnObj.transform.SetParent(roomObj.transform);
        rewardSpawnObj.transform.localPosition = Vector3.zero;
        room.rewardSpawnPoint = rewardSpawnObj.transform;
        
        // Fill tilemaps with basic tiles
        if (currentTheme != null)
        {
            FillBasicTilemaps(floorTilemap, wallTilemap, decorTilemap);
        }
        
        return roomObj;
    }
    
    private void FillBasicTilemaps(Tilemap floorTilemap, Tilemap wallTilemap, Tilemap decorTilemap)
    {
        // Get tiles from theme
        TileBase floorTile = currentTheme.GetRandomFloorTile();
        TileBase wallTile = currentTheme.GetRandomWallTile();
        TileBase decorTile = currentTheme.GetRandomDecorationTile();
        
        if (floorTile == null || wallTile == null)
        {
            Debug.LogError("Missing tiles in theme!");
            return;
        }
        
        // Fill floor
        for (int x = -roomWidth/2; x <= roomWidth/2; x++)
        {
            for (int y = -roomHeight/2; y <= roomHeight/2; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
        
        // Create walls around the perimeter
        for (int x = -roomWidth/2 - 1; x <= roomWidth/2 + 1; x++)
        {
            // Top and bottom walls
            wallTilemap.SetTile(new Vector3Int(x, -roomHeight/2 - 1, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(x, roomHeight/2 + 1, 0), wallTile);
        }
        
        for (int y = -roomHeight/2 - 1; y <= roomHeight/2 + 1; y++)
        {
            // Left and right walls
            wallTilemap.SetTile(new Vector3Int(-roomWidth/2 - 1, y, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(roomWidth/2 + 1, y, 0), wallTile);
        }
        
        // Add random decorations
        if (decorTile != null)
        {
            int decorCount = Mathf.FloorToInt(roomWidth * roomHeight * decorationDensity);
            
            for (int i = 0; i < decorCount; i++)
            {
                int x = Random.Range(-roomWidth/2 + 1, roomWidth/2);
                int y = Random.Range(-roomHeight/2 + 1, roomHeight/2);
                
                decorTilemap.SetTile(new Vector3Int(x, y, 0), decorTile);
            }
        }
    }
    
    private void CreateDoors(GameObject roomObj, Dictionary<int, bool> doorDirections)
    {
        Room room = roomObj.GetComponent<Room>();
        
        // Door positions relative to room - đặt cửa sát tường hơn
        Vector3[] doorPositions = {
            new Vector3(0, roomHeight/2, 0),         // Top (0)
            new Vector3(roomWidth/2, 0, 0),          // Right (1)
            new Vector3(0, -roomHeight/2, 0),        // Bottom (2)
            new Vector3(-roomWidth/2, 0, 0)          // Left (3)
        };
        
        // Tạo cửa ở tất cả các hướng, bất kể doorDirections
        for (int i = 0; i < 4; i++)
        {
            // Create door
            GameObject doorObj = Instantiate(doorPrefab, roomObj.transform);
            doorObj.name = $"Door_{i}";
            doorObj.transform.localPosition = doorPositions[i];
            
            // Rotate door based on direction
            float rotation = i * 90f;
            doorObj.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            
            // Setup Door component
            Door door = doorObj.GetComponent<Door>();
            if (door == null)
            {
                door = doorObj.AddComponent<Door>();
            }
            
            // Chỉ kích hoạt cửa nếu được chỉ định trong doorDirections
            bool isActive = doorDirections.ContainsKey(i) && doorDirections[i];
            door.SetActive(isActive);
            
            // Add to room's door list
            room.doors.Add(door);
            
            // Chỉ tạo lỗ trên tường nếu cửa được kích hoạt
            if (isActive)
            {
                ClearWallForDoor(roomObj, i);
            }
            
            Debug.Log($"Created door {i} in room {roomObj.name}, active: {isActive}");
        }
    }
    
    private void ClearWallForDoor(GameObject roomObj, int doorIndex)
    {
        // Find wall tilemap
        Transform tilemapsParent = roomObj.transform.Find("Tilemaps");
        if (tilemapsParent == null) return;
        
        Transform wallsTransform = tilemapsParent.Find("Walls");
        if (wallsTransform == null) return;
        
        Tilemap wallTilemap = wallsTransform.GetComponent<Tilemap>();
        if (wallTilemap == null) return;
        
        // Clear wall tiles at door position
        Vector3Int doorTilePos;
        
        switch (doorIndex)
        {
            case 0: // Top
                doorTilePos = new Vector3Int(0, roomHeight/2 + 1, 0);
                wallTilemap.SetTile(doorTilePos, null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(-1, 0, 0), null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(1, 0, 0), null);
                break;
                
            case 1: // Right
                doorTilePos = new Vector3Int(roomWidth/2 + 1, 0, 0);
                wallTilemap.SetTile(doorTilePos, null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(0, -1, 0), null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(0, 1, 0), null);
                break;
                
            case 2: // Bottom
                doorTilePos = new Vector3Int(0, -roomHeight/2 - 1, 0);
                wallTilemap.SetTile(doorTilePos, null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(-1, 0, 0), null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(1, 0, 0), null);
                break;
                
            case 3: // Left
                doorTilePos = new Vector3Int(-roomWidth/2 - 1, 0, 0);
                wallTilemap.SetTile(doorTilePos, null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(0, -1, 0), null);
                wallTilemap.SetTile(doorTilePos + new Vector3Int(0, 1, 0), null);
                break;
        }
    }
    
    private void PopulateRoom(GameObject roomObj, Room.RoomType roomType)
    {
        Room room = roomObj.GetComponent<Room>();
        
        // Add enemies based on room type
        if (currentTheme != null)
        {
            switch (roomType)
            {
                case Room.RoomType.Normal:
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject enemy = currentTheme.GetRandomCommonEnemy();
                        if (enemy != null)
                        {
                            room.enemies.Add(enemy);
                        }
                    }
                    break;
                    
                case Room.RoomType.Boss:
                    GameObject boss = currentTheme.GetRandomBossEnemy();
                    if (boss != null)
                    {
                        room.enemies.Add(boss);
                    }
                    break;
                    
                case Room.RoomType.Treasure:
                    GameObject rareEnemy = currentTheme.GetRandomRareEnemy();
                    if (rareEnemy != null)
                    {
                        room.enemies.Add(rareEnemy);
                    }
                    break;
            }
        }
        
        // Add props and obstacles
        AddPropsToRoom(roomObj, roomType);
    }
    
    private void AddPropsToRoom(GameObject roomObj, Room.RoomType roomType)
    {
        if (currentTheme == null) return;
        
        // Create props container
        GameObject propsContainer = new GameObject("Props");
        propsContainer.transform.SetParent(roomObj.transform);
        
        int propCount = roomType switch
        {
            Room.RoomType.Normal => Random.Range(2, 5),
            Room.RoomType.Boss => Random.Range(1, 3),
            Room.RoomType.Treasure => Random.Range(3, 6),
            Room.RoomType.Shop => Random.Range(4, 7),
            _ => Random.Range(1, 4)
        };
        
        // Add props
        for (int i = 0; i < propCount; i++)
        {
            GameObject propPrefab = currentTheme.GetRandomProp();
            if (propPrefab != null)
            {
                // Find valid position
                float x = Random.Range(-roomWidth/2 + 1.5f, roomWidth/2 - 1.5f);
                float y = Random.Range(-roomHeight/2 + 1.5f, roomHeight/2 - 1.5f);
                
                GameObject prop = Instantiate(propPrefab, propsContainer.transform);
                prop.transform.localPosition = new Vector3(x, y, 0);
                prop.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
            }
        }
        
        // Add obstacles
        int obstacleCount = roomType switch
        {
            Room.RoomType.Normal => Random.Range(2, 4),
            Room.RoomType.Boss => Random.Range(3, 6),
            Room.RoomType.Treasure => Random.Range(1, 3),
            _ => Random.Range(0, 2)
        };
        
        for (int i = 0; i < obstacleCount; i++)
        {
            GameObject obstaclePrefab = currentTheme.GetRandomObstacle();
            if (obstaclePrefab != null)
            {
                // Find valid position
                float x = Random.Range(-roomWidth/2 + 2f, roomWidth/2 - 2f);
                float y = Random.Range(-roomHeight/2 + 2f, roomHeight/2 - 2f);
                
                GameObject obstacle = Instantiate(obstaclePrefab, propsContainer.transform);
                obstacle.transform.localPosition = new Vector3(x, y, 0);
            }
        }
    }
}