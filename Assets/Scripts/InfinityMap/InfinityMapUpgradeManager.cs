using UnityEngine;

namespace InfinityMap
{
    public class InfinityMapUpgradeManager : MonoBehaviour
    {
        [SerializeField] private SkillUpgradeStation[] upgradeStations;
        private bool isInWave = false;

        private InfinityEnemySpawner enemySpawner;

        private void Start()
        {
            // Tự động tìm tất cả SkillUpgradeStation trong scene nếu chưa được gán
            if (upgradeStations == null || upgradeStations.Length == 0)
            {
                upgradeStations = FindObjectsOfType<SkillUpgradeStation>();
            }

            // Tìm Enemy Spawner
            enemySpawner = FindAnyObjectByType<InfinityEnemySpawner>();
            if (enemySpawner != null)
            {
                // Vô hiệu hóa các trạm nâng cấp khi đang trong wave
                SetUpgradeStationsActive(!enemySpawner.IsWaveActive());
            }
            else
            {
                Debug.LogError("Không tìm thấy InfinityEnemySpawner!");
            }
        }

        // Gọi khi bắt đầu wave mới
        private void Update()
        {
            if (enemySpawner != null)
            {
                bool shouldBeActive = !enemySpawner.IsWaveActive();

                // Chỉ cập nhật khi trạng thái thay đổi để tránh gọi không cần thiết
                if (isInWave != enemySpawner.IsWaveActive())
                {
                    isInWave = enemySpawner.IsWaveActive();
                    SetUpgradeStationsActive(shouldBeActive);
                }
            }
        }

        // Gọi khi bắt đầu wave mới (có thể gọi từ bên ngoài nếu cần)
        public void OnWaveStart()
        {
            isInWave = true;
            SetUpgradeStationsActive(false);
        }

        // Gọi khi kết thúc wave (có thể gọi từ bên ngoài nếu cần)
        public void OnWaveEnd()
        {
            isInWave = false;
            SetUpgradeStationsActive(true);
        }

        private void SetUpgradeStationsActive(bool active)
        {
            foreach (var station in upgradeStations)
            {
                if (station != null)
                {
                    station.SetAvailable(active);
                }
            }
        }
    }
}