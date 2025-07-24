using UnityEngine;

[CreateAssetMenu(fileName = "RewardDropData", menuName = "RPG Game/Reward Drop Data")]
public class RewardDropData : ScriptableObject
{
    [System.Serializable]
    public class RewardDrop
    {
        public GameObject rewardPrefab;
        [Range(0f, 100f)] public float dropChance;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    public RewardDrop[] possibleRewards;
    public RewardDrop coinReward; // Thêm coin reward
}


