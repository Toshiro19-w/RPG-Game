using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "DungeonTheme", menuName = "Dungeon/Dungeon Theme")]
public class DungeonTheme : ScriptableObject
{
    [Header("Theme Info")]
    public string themeName;
    public Sprite themePreview;
    
    [Header("Tiles")]
    public TileBase[] floorTiles;
    public TileBase[] wallTiles;
    public TileBase[] decorationTiles;
    
    [Header("Colors")]
    public Color ambientColor = Color.white;
    public Color roomColor = Color.white;
    
    [Header("Props")]
    public GameObject[] environmentProps;
    public GameObject[] obstacles;
    
    [Header("Enemies")]
    public GameObject[] commonEnemies;
    public GameObject[] rareEnemies;
    public GameObject[] bossEnemies;
    
    // Lấy tile ngẫu nhiên từ mảng
    public TileBase GetRandomFloorTile()
    {
        if (floorTiles == null || floorTiles.Length == 0) return null;
        return floorTiles[Random.Range(0, floorTiles.Length)];
    }
    
    public TileBase GetRandomWallTile()
    {
        if (wallTiles == null || wallTiles.Length == 0) return null;
        return wallTiles[Random.Range(0, wallTiles.Length)];
    }
    
    public TileBase GetRandomDecorationTile()
    {
        if (decorationTiles == null || decorationTiles.Length == 0) return null;
        return decorationTiles[Random.Range(0, decorationTiles.Length)];
    }
    
    // Lấy prop ngẫu nhiên
    public GameObject GetRandomProp()
    {
        if (environmentProps == null || environmentProps.Length == 0) return null;
        return environmentProps[Random.Range(0, environmentProps.Length)];
    }
    
    public GameObject GetRandomObstacle()
    {
        if (obstacles == null || obstacles.Length == 0) return null;
        return obstacles[Random.Range(0, obstacles.Length)];
    }
    
    // Lấy enemy ngẫu nhiên
    public GameObject GetRandomCommonEnemy()
    {
        if (commonEnemies == null || commonEnemies.Length == 0) return null;
        return commonEnemies[Random.Range(0, commonEnemies.Length)];
    }
    
    public GameObject GetRandomRareEnemy()
    {
        if (rareEnemies == null || rareEnemies.Length == 0) return null;
        return rareEnemies[Random.Range(0, rareEnemies.Length)];
    }
    
    public GameObject GetRandomBossEnemy()
    {
        if (bossEnemies == null || bossEnemies.Length == 0) return null;
        return bossEnemies[Random.Range(0, bossEnemies.Length)];
    }
}