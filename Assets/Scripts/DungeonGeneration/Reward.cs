using UnityEngine;


public class Reward : MonoBehaviour
{
    [Header("Reward Settings")]
    public RewardData rewardData;
    
    [Header("Visual")]
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;
    
    [Header("Auto Collect")]
    public float attractDistance = 3f;
    public float moveSpeed = 5f;
    
    private Vector3 startPos;
    private CircleCollider2D rewardTrigger;
    private Transform player;
    private bool isMovingToPlayer = false;
    
    void Start()
    {
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Setup sprite từ RewardData hoặc tạo sprite mặc định
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        
        if (rewardData != null && rewardData.rewardSprite != null)
        {
            sr.sprite = rewardData.rewardSprite;
        }
        else
        {
            // Tạo sprite mặc định nếu không có RewardData
            sr.color = Color.yellow;
            sr.sprite = CreateDefaultSprite();
        }
        
        // Đảm bảo sprite hiển thị
        sr.sortingOrder = 10;
        
        // Tạo trigger tự động
        rewardTrigger = gameObject.AddComponent<CircleCollider2D>();
        rewardTrigger.isTrigger = true;
        rewardTrigger.radius = 0.5f;
    }
    
    private Sprite CreateDefaultSprite()
    {
        // Tạo texture 32x32 màu vàng
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.yellow;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attractDistance && !isMovingToPlayer)
        {
            isMovingToPlayer = true;
        }
        
        if (isMovingToPlayer)
        {
            // Bay về phía player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Hiệu ứng float bình thường
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectReward(other.gameObject);
        }
    }
    
    private void CollectReward(GameObject player)
    {
        if (rewardData == null) return;
        
        int rewardValue = rewardData.GetRandomValue();
        
        switch (rewardData.rewardType)
        {
            case RewardData.RewardType.Coin:
                if (player.TryGetComponent<PlayerWallet>(out var wallet))
                {
                    wallet.AddCoins(rewardValue);
                }
                break;
                
            case RewardData.RewardType.Health:
                if (player.TryGetComponent<PlayerHealth>(out var health))
                {
                    health.Heal(rewardValue);
                    Debug.Log($"Healed {rewardValue} HP!");
                }
                break;
            
        }
        
        Destroy(gameObject);
    }
}