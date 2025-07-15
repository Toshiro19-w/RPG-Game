using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public Room targetRoom;
    public Transform teleportPoint;
    
    [Header("Visual")]
    public SpriteRenderer doorSprite;
    public Sprite openSprite;
    public Sprite closedSprite;
    
    private bool isActive = false;
    private BoxCollider2D doorTrigger;
    
    void Start()
    {
        doorTrigger = GetComponent<BoxCollider2D>();
        if (doorSprite == null) doorSprite = GetComponent<SpriteRenderer>();
        SetActive(false);
    }
    
    public void SetActive(bool active)
    {
        Debug.Log($"Door {gameObject.name} SetActive: {active}");
        isActive = active;
        
        if (doorSprite != null)
        {
            doorSprite.sprite = active ? openSprite : closedSprite;
            doorSprite.enabled = true;
            doorSprite.color = active ? Color.white : Color.gray;
            Debug.Log($"Door sprite updated: {doorSprite.sprite?.name}, enabled: {doorSprite.enabled}");
        }
        else
        {
            Debug.LogWarning($"Door {gameObject.name} has no SpriteRenderer!");
            // Tạo SpriteRenderer nếu chưa có
            doorSprite = gameObject.AddComponent<SpriteRenderer>();
            doorSprite.color = active ? Color.green : Color.red;
            doorSprite.sortingOrder = 5;
        }
        
        if (doorTrigger != null)
            doorTrigger.enabled = active;
        else
            Debug.LogWarning($"Door {gameObject.name} has no BoxCollider2D!");
    }
    
    public void SetTargetRoom(Room room)
    {
        targetRoom = room;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || targetRoom == null) return;
        
        if (other.CompareTag("Player"))
        {
            TeleportPlayer(other.transform);
        }
    }
    
    private void TeleportPlayer(Transform player)
    {
        if (teleportPoint != null)
        {
            player.position = teleportPoint.position;
        }
        else
        {
            player.position = targetRoom.transform.position;
        }
        
        targetRoom.EnterRoom();
        DungeonManager.Instance?.SetCurrentRoom(targetRoom);
    }
}