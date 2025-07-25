using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RewardSystem : MonoBehaviour
{
    public static RewardSystem Instance { get; private set; }

    [SerializeField] private RewardDropData rewardDropData;
    private List<GameObject> activeRewards = new List<GameObject>();
    private bool isCleaningUp = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    public void SpawnRewards(Vector3 position)
    {
        if (rewardDropData == null || isCleaningUp) return;
        
        // Spawn regular rewards
        foreach (var rewardDrop in rewardDropData.possibleRewards)
        {
            if (rewardDrop.rewardPrefab == null) continue;
            
            float randomChance = Random.Range(0f, 100f);
            if (randomChance <= rewardDrop.dropChance)
            {
                int amount = Random.Range(rewardDrop.minAmount, rewardDrop.maxAmount + 1);
                
                for (int i = 0; i < amount; i++)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
                    Vector3 spawnPosition = position + new Vector3(randomOffset.x, randomOffset.y, 0);
                    
                    GameObject reward = Instantiate(rewardDrop.rewardPrefab, spawnPosition, Quaternion.identity);
                    activeRewards.Add(reward);
                }
            }
        }

        // Spawn coins
        if (rewardDropData.coinReward != null && rewardDropData.coinReward.rewardPrefab != null)
        {
            float randomChance = Random.Range(0f, 100f);
            if (randomChance <= rewardDropData.coinReward.dropChance)
            {
                int amount = Random.Range(rewardDropData.coinReward.minAmount, 
                                    rewardDropData.coinReward.maxAmount + 1);
                
                for (int i = 0; i < amount; i++)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
                    Vector3 spawnPosition = position + new Vector3(randomOffset.x, randomOffset.y, 0);
                    
                    GameObject coin = Instantiate(rewardDropData.coinReward.rewardPrefab, 
                                            spawnPosition, 
                                            Quaternion.identity);
                    activeRewards.Add(coin);
                }
            }
        }
        
        // Clean up null references
        activeRewards.RemoveAll(reward => reward == null);
    }

    public void CleanupAllRewards()
    {
        if (isCleaningUp) return;
        isCleaningUp = true;
        
        for (int i = activeRewards.Count - 1; i >= 0; i--)
        {
            if (activeRewards[i] != null)
            {
                DestroyImmediate(activeRewards[i]);
            }
        }
        activeRewards.Clear();
        
        isCleaningUp = false;
    }

    void OnSceneUnloaded(Scene scene)
    {
        CleanupAllRewards();
    }

    void OnDisable()
    {
        CleanupAllRewards();
    }

    void OnDestroy() 
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        if (!isCleaningUp)
        {
            CleanupAllRewards();
        }
    }

    void OnApplicationQuit()
    {
        CleanupAllRewards();
    }
    
    public void RemoveReward(GameObject reward)
    {
        activeRewards.Remove(reward);
    }
}