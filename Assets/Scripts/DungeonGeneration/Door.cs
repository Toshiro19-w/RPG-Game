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
    
    void Start()
    {
        // Kiểm tra xem cửa có thể sử dụng được không
        CheckDoorUsability();
    }
    
    public bool CheckDoorUsability()
    {
        bool isUsable = teleportPoint != null && targetRoom != null;
        
        if (!isUsable && isOpen)
        {
            // Nếu cửa đang mở nhưng không thể sử dụng, đóng nó lại
            isOpen = false;
            
            if (doorCollider != null)
                doorCollider.enabled = false;
                
            if (doorRenderer != null)
                doorRenderer.color = new Color(0.7f, 0.3f, 0.3f, 0.5f); // Màu đỏ nhạt để chỉ ra vấn đề
                
            Debug.LogWarning($"Door {name} in {transform.parent?.name} is not usable (no teleport point or target room)");
        }
        
        return isUsable;
    }
    
    public void SetActive(bool active)
    {
        // Không tắt GameObject, chỉ điều chỉnh trạng thái cửa
        isOpen = active;
        
        // Kiểm tra xem cửa có teleportPoint và targetRoom không
        bool canBeActive = active && teleportPoint != null && targetRoom != null;
        
        if (doorCollider != null)
            doorCollider.enabled = canBeActive;
            
        if (doorRenderer != null)
        {
            // Chỉ hiển thị cửa nếu có thể kích hoạt hoặc đang đóng
            doorRenderer.enabled = canBeActive || !active;
            doorRenderer.color = canBeActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f); // Màu xám nếu đóng
        }
            
        if (animator != null && canBeActive)
            animator.SetTrigger(openAnimationTrigger);
            
        Debug.Log($"Door {name} in {transform.parent?.name} set {(canBeActive ? "active" : "inactive")} (requested: {active})");
    }
    
    public void SetTargetRoom(Room room)
    {
        targetRoom = room;
        
        // Kiểm tra lại xem cửa có thể sử dụng được không sau khi đặt targetRoom
        if (room != null)
            CheckDoorUsability();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Kiểm tra nhanh xem cửa có thể sử dụng được không
        if (!isOpen || isLocked || teleportPoint == null || targetRoom == null)
        {
            // Vô hiệu hóa collider để tránh lỗi trong tương lai
            if (doorCollider != null)
                doorCollider.enabled = false;
                
            // Đổi màu cửa thành đỏ để chỉ ra vấn đề
            if (doorRenderer != null)
                doorRenderer.color = new Color(0.7f, 0.3f, 0.3f, 0.5f);
                
            Debug.LogWarning($"Door {name} in {transform.parent?.name} cannot be used. isOpen: {isOpen}, isLocked: {isLocked}, teleportPoint: {teleportPoint}, targetRoom: {targetRoom}");
            return;
        }
        
        Debug.Log($"Player entered door trigger: {name}, isOpen: {isOpen}, isLocked: {isLocked}, teleportPoint: valid, targetRoom: {targetRoom.name}");
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