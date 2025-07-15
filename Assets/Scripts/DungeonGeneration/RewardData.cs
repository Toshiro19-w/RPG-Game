using UnityEngine;

[CreateAssetMenu(fileName = "RewardData", menuName = "Dungeon/Reward Data")]
public class RewardData : ScriptableObject
{
    [Header("Basic Info")]
    public string rewardName;
    public Sprite rewardSprite;
    public RewardType rewardType;
    
    [Header("Values")]
    public int value = 10;
    public int minValue = 5;
    public int maxValue = 20;
    
    [Header("Effects")]
    public bool hasSpecialEffect = false;
    public string effectDescription;
    
    public enum RewardType
    {
        Coin,
        Health
    }
    
    public int GetRandomValue()
    {
        return Random.Range(minValue, maxValue + 1);
    }
}