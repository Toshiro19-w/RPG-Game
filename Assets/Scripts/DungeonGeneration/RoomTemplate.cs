using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTemplate : MonoBehaviour
{
    [Header("Tilemap Components")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase[] floorTiles;
    public TileBase[] wallTiles;
    
    [Header("Room Settings")]
    public int roomWidth = 12;
    public int roomHeight = 8;
    
    void Start()
    {
        if (floorTilemap == null) floorTilemap = GetComponentInChildren<Tilemap>();
        GenerateRoom();
    }
    
    private void GenerateRoom()
    {
        // Tạo sàn
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x - roomWidth/2, y - roomHeight/2, 0);
                TileBase randomFloor = floorTiles[Random.Range(0, floorTiles.Length)];
                floorTilemap.SetTile(pos, randomFloor);
            }
        }
        
        // Tạo tường
        CreateWalls();
    }
    
    private void CreateWalls()
    {
        int halfWidth = roomWidth / 2;
        int halfHeight = roomHeight / 2;
        
        // Tường trên/dưới
        for (int x = -halfWidth - 1; x <= halfWidth + 1; x++)
        {
            if (x != 0) // Để trống chỗ door giữa
            {
                Vector3Int topPos = new Vector3Int(x, halfHeight + 1, 0);
                Vector3Int bottomPos = new Vector3Int(x, -halfHeight - 1, 0);
                
                TileBase randomWall1 = wallTiles[Random.Range(0, wallTiles.Length)];
                TileBase randomWall2 = wallTiles[Random.Range(0, wallTiles.Length)];
                wallTilemap.SetTile(topPos, randomWall1);
                wallTilemap.SetTile(bottomPos, randomWall2);
            }
        }
        
        // Tường trái/phải
        for (int y = -halfHeight; y <= halfHeight; y++)
        {
            if (y != 0) // Để trống chỗ door giữa
            {
                Vector3Int leftPos = new Vector3Int(-halfWidth - 1, y, 0);
                Vector3Int rightPos = new Vector3Int(halfWidth + 1, y, 0);
                
                TileBase randomWall3 = wallTiles[Random.Range(0, wallTiles.Length)];
                TileBase randomWall4 = wallTiles[Random.Range(0, wallTiles.Length)];
                wallTilemap.SetTile(leftPos, randomWall3);
                wallTilemap.SetTile(rightPos, randomWall4);
            }
        }
    }
}