using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public bool isLocked = false;
    public Transform teleportPoint;
    public float teleportOffset = 1.5f; // Khoảng cách dịch chuyển thêm theo hướng cửa
    
    [Header("Animation")]
    public Animator animator;
    public string openAnimationTrigger = "Open";
    public string closeAnimationTrigger = "Close";
    
    [Header("References")]
    public BoxCollider2D doorCollider;
    public SpriteRenderer doorRenderer;
    
    private Room targetRoom;
    
    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (doorCollider == null)
            doorCollider = GetComponent<BoxCollider2D>();
            
        if (doorRenderer == null)
            doorRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void SetActive(bool active)
    {
        // Không tắt GameObject, chỉ điều chỉnh trạng thái cửa
        isOpen = active;
        
        if (doorCollider != null)
            doorCollider.enabled = active;
            
        if (doorRenderer != null)
        {
            doorRenderer.enabled = true; // Luôn hiển thị cửa
            doorRenderer.color = active ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f); // Màu xám nếu đóng
        }
            
        if (animator != null && active)
            animator.SetTrigger(openAnimationTrigger);
            
        Debug.Log($"Door {name} in {transform.parent?.name} set {(active ? "active" : "inactive")}");
    }
    
    public void SetTargetRoom(Room room)
    {
        targetRoom = room;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        Debug.Log($"Player entered door trigger: {name}, isOpen: {isOpen}, isLocked: {isLocked}, teleportPoint: {(teleportPoint != null ? "valid" : "null")}, targetRoom: {(targetRoom != null ? targetRoom.name : "null")}");
        
        if (!isOpen || isLocked)
        {
            Debug.LogError($"Door {name} in {transform.parent?.name} is not open or is locked. isOpen: {isOpen}, isLocked: {isLocked}");
            return;
        }
        
        if (teleportPoint == null || targetRoom == null)
        {
            Debug.LogError($"Door {name} in {transform.parent?.name} has invalid teleport point or target room. teleportPoint: {teleportPoint}, targetRoom: {targetRoom}");
            return;
        }
        
        Debug.Log($"Door {name} teleport point position: {teleportPoint.position}, parent: {teleportPoint.parent?.name}");
        
        Debug.Log($"Player entering door {name} to room {targetRoom.name}");

        // Cache player's rigidbody and disable it temporarily
        var playerRb = other.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.simulated = false;
        }

        // Store player's relative velocity for momentum preservation
        Vector2 relativeVelocity = playerRb != null ? playerRb.linearVelocity : Vector2.zero;

        // Teleport player
        // Tính toán vị trí teleport với offset
        Vector3 direction = (teleportPoint.position - transform.position).normalized;
        Vector3 teleportPosition = teleportPoint.position + direction * teleportOffset;
        other.transform.position = teleportPosition;

        // Restore player's momentum in the new room's orientation
        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.linearVelocity = relativeVelocity;
        }

        // Trigger room transition effects
        StartCoroutine(RoomTransitionRoutine(targetRoom));
    }

    private System.Collections.IEnumerator RoomTransitionRoutine(Room newRoom)
    {
        // Let any transition effects play
        yield return new WaitForSeconds(0.1f);

        // Notify the target room that player has entered
        newRoom.EnterRoom();

        // Update current room in DungeonManager
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.SetCurrentRoom(newRoom);
        }
    }
    
    public void Lock()
    {
        isLocked = true;
        
        // Change appearance to locked door
        if (doorRenderer != null)
        {
            // Change color or sprite to indicate locked state
            doorRenderer.color = Color.red;
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
        
        // Change appearance back to normal
        if (doorRenderer != null)
        {
            doorRenderer.color = Color.white;
        }
    }
}