using UnityEngine;
using System.Collections.Generic;

public class RewardSystem : MonoBehaviour
{
    public static RewardSystem Instance { get; private set; }

    [SerializeField] private RewardDropData rewardDropData;
    private List<GameObject> activeRewards = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SpawnRewards(Vector3 position)
    {
        if (rewardDropData == null) return;
        
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
    }

    public void CleanupAllRewards()
    {
        foreach (var reward in activeRewards)
        {
            if (reward != null)
            {
                reward.SetActive(false);
                Destroy(reward);
            }
        }
        activeRewards.Clear();
    }

    void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        CleanupAllRewards();
    }

    void OnDisable()
    {
        CleanupAllRewards();
    }

    void OnDestroy() 
    {
        CleanupAllRewards();
    }

    void OnApplicationQuit()
    {
        CleanupAllRewards();
    }
}