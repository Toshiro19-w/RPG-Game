using UnityEngine;
using TMPro;
using System.Collections;

namespace InfinityMap
{
    public class WaveCountdownUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI countdownText;
        
        [Header("Settings")]
        [SerializeField] private float initialCountdownDuration = 3f;
        [SerializeField] private float betweenWavesCountdownDuration = 10f;
        [SerializeField] private string initialMessage = "Ải này sẽ bắt đầu trong {0}...";
        [SerializeField] private string nextWaveMessage = "Đợt tiếp theo sẽ bắt đầu trong {0}...";
        
        private InfinityEnemySpawner enemySpawner;
        private Coroutine countdownCoroutine;
        
        private void Awake()
        {
            // Tìm enemy spawner
            enemySpawner = FindObjectOfType<InfinityEnemySpawner>();
            
            if (enemySpawner == null)
            {
                Debug.LogError("Không tìm thấy InfinityEnemySpawner!");
            }
            
            // Ẩn text ban đầu
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Countdown Text chưa được gán!");
            }
        }
        
        // Không tự động bắt đầu đếm ngược trong Start() vì InfinityEnemySpawner sẽ gọi StartInitialCountdown()
        // private void Start()
        // {
        //     // Bắt đầu đếm ngược khi vào màn
        //     StartInitialCountdown();
        // }
        
        public void StartInitialCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            
            countdownCoroutine = StartCoroutine(CountdownRoutine(initialCountdownDuration, initialMessage, true));
        }
        
        public void StartNextWaveCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            
            countdownCoroutine = StartCoroutine(CountdownRoutine(betweenWavesCountdownDuration, nextWaveMessage, false));
        }
        
        private IEnumerator CountdownRoutine(float duration, string messageFormat, bool isInitial)
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                
                // Tạm dừng spawner nếu đây là đếm ngược ban đầu
                if (isInitial && enemySpawner != null)
                {
                    enemySpawner.StopSpawning();
                }
                
                float timeRemaining = duration;
                
                while (timeRemaining > 0)
                {
                    // Hiển thị thời gian còn lại (làm tròn lên)
                    int secondsRemaining = Mathf.CeilToInt(timeRemaining);
                    countdownText.text = string.Format(messageFormat, secondsRemaining);
                    
                    yield return new WaitForSeconds(0.1f); // Cập nhật mỗi 0.1 giây để mượt hơn
                    timeRemaining -= 0.1f;
                }
                
                // Ẩn text khi đếm ngược kết thúc
                countdownText.gameObject.SetActive(false);
                
                // Bắt đầu spawner nếu đây là đếm ngược ban đầu
                if (isInitial && enemySpawner != null)
                {
                    enemySpawner.ResumeSpawning();
                }
            }
        }
    }
}