using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RoomTemplate", menuName = "Dungeon/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [Header("Room Info")]
    public Room.RoomType roomType;
    public string templateName;
    
    [Header("Tilemaps")]
    public TileBase[] floorTiles;
    public TileBase[] wallTiles;
    public TileBase[] decorationTiles;
    
    [Header("Wall Configuration")]
    public int topWallHeight = 3;
    public int sideWallHeight = 1;
    public bool stackableWalls = false;
    
    [Header("Layout")]
    public int roomWidth = 12;
    public int roomHeight = 8;
    public float decorationDensity = 0.1f;
    
    [Header("Content")]
    public GameObject[] propPrefabs;
    public GameObject[] enemyPrefabs;
    public int minProps = 1;
    public int maxProps = 5;
    public int minEnemies = 0;
    public int maxEnemies = 3;
    
    [Header("Door Positions")]
    public Vector2Int[] doorPositions = new Vector2Int[4]; // Up, Right, Down, Left
    
    private void OnValidate()
    {
        // Khởi tạo vị trí cửa mặc định nếu chưa được thiết lập
        if (doorPositions == null || doorPositions.Length < 4)
            doorPositions = new Vector2Int[4];
            
        if (doorPositions[0] == Vector2Int.zero) doorPositions[0] = new Vector2Int(0, roomHeight/2);     // Up
        if (doorPositions[1] == Vector2Int.zero) doorPositions[1] = new Vector2Int(roomWidth/2, 0);      // Right
        if (doorPositions[2] == Vector2Int.zero) doorPositions[2] = new Vector2Int(0, -roomHeight/2);    // Down
        if (doorPositions[3] == Vector2Int.zero) doorPositions[3] = new Vector2Int(-roomWidth/2, 0);     // Left
    }
}