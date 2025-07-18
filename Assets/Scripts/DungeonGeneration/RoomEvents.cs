using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class RoomEvents : MonoBehaviour
{
    // Room state events
    public UnityEvent onRoomEnter;
    public UnityEvent onRoomClear;
    public UnityEvent onRoomExit;
    
    // Combat events
    public UnityEvent onCombatStart;
    public UnityEvent onCombatEnd;
    public UnityEvent onEnemyDefeated;
    public UnityEvent onAllEnemiesDefeated;
    
    // Door events
    public UnityEvent onDoorsLocked;
    public UnityEvent onDoorsUnlocked;
    
    // Reward events
    public UnityEvent onRewardSpawned;
    public UnityEvent onRewardCollected;
    
    // Special room events
    public UnityEvent onShopEnter;
    public UnityEvent onBossRoomEnter;
    public UnityEvent onBossDefeated;
    public UnityEvent onTreasureRoomOpen;
    
    private Room room;
    
    void Awake()
    {
        room = GetComponent<Room>();
        if (room == null)
        {
            Debug.LogError("RoomEvents must be attached to a GameObject with a Room component!");
            enabled = false;
            return;
        }
        
        // Set up default listeners
        onRoomEnter.AddListener(() => Debug.Log($"Entered room: {room.gameObject.name}"));
        onRoomClear.AddListener(() => Debug.Log($"Room cleared: {room.gameObject.name}"));
        onCombatStart.AddListener(() => Debug.Log($"Combat started in room: {room.gameObject.name}"));
        onCombatEnd.AddListener(() => Debug.Log($"Combat ended in room: {room.gameObject.name}"));
        
        // Special room type listeners
        if (room.roomType == Room.RoomType.Boss)
        {
            onBossRoomEnter.AddListener(() => Debug.Log("Entered boss room!"));
            onBossDefeated.AddListener(() => Debug.Log("Boss defeated!"));
        }
        else if (room.roomType == Room.RoomType.Shop)
        {
            onShopEnter.AddListener(() => Debug.Log("Entered shop room!"));
        }
        else if (room.roomType == Room.RoomType.Treasure)
        {
            onTreasureRoomOpen.AddListener(() => Debug.Log("Treasure room opened!"));
        }
    }
    
    public void TriggerRoomEnter()
    {
        onRoomEnter?.Invoke();
        
        switch (room.roomType)
        {
            case Room.RoomType.Boss:
                onBossRoomEnter?.Invoke();
                break;
            case Room.RoomType.Shop:
                onShopEnter?.Invoke();
                break;
            case Room.RoomType.Treasure:
                onTreasureRoomOpen?.Invoke();
                break;
        }
    }
    
    public void TriggerCombatStart()
    {
        onCombatStart?.Invoke();
    }
    
    public void TriggerCombatEnd()
    {
        onCombatEnd?.Invoke();
    }
    
    public void TriggerEnemyDefeated()
    {
        onEnemyDefeated?.Invoke();
        
        // Check if this was the last enemy
        if (!room.HasEnemies())
        {
            onAllEnemiesDefeated?.Invoke();
            TriggerRoomClear();
        }
    }
    
    public void TriggerRoomClear()
    {
        onRoomClear?.Invoke();
        
        if (room.roomType == Room.RoomType.Boss)
        {
            onBossDefeated?.Invoke();
        }
    }
    
    public void TriggerDoorsLocked()
    {
        onDoorsLocked?.Invoke();
    }
    
    public void TriggerDoorsUnlocked()
    {
        onDoorsUnlocked?.Invoke();
    }
    
    public void TriggerRewardSpawned()
    {
        onRewardSpawned?.Invoke();
    }
    
    public void TriggerRewardCollected()
    {
        onRewardCollected?.Invoke();
    }
    
    public void TriggerRoomExit()
    {
        onRoomExit?.Invoke();
    }
}
