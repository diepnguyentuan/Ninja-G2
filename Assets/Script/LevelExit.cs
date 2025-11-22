using UnityEngine;
using UnityEngine.SceneManagement; // RẤT QUAN TRỌNG: Phải có dòng này để dùng SceneManager

public class LevelExit : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField]
    private string nextSceneName = "Boss"; // Tên scene để tải

    [Header("Spawn Settings")]
    [SerializeField] private SpawnData spawnData; // Kéo file "PlayerSpawnData" vào đây
    [SerializeField] private string spawnNameForNextLevel; // Gõ tên "hòm thư" muốn đến

    // Biến này để đảm bảo không gọi LoadScene nhiều lần
    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng va chạm có tag là "Player" VÀ chưa đang tải
        if (other.CompareTag("Player") && !isLoading)
        {
            // Đánh dấu là đang tải để tránh gọi lại
            isLoading = true;
            // "Viết" tên điểm đến vào "bức thư" (SpawnData)
            if (spawnData != null && !string.IsNullOrEmpty(spawnNameForNextLevel))
            {
                spawnData.nextSpawnPointName = spawnNameForNextLevel;
            }
            else
            {
                Debug.LogWarning("Chưa cài đặt SpawnData hoặc SpawnName cho LevelExit!");
            }
            // ---------------------------------------------

            Debug.Log($"Player hit the exit. Loading scene: {nextSceneName}");
            LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        // Tải scene dựa theo tên đã cung cấp
        // (Bạn cũng có thể thêm hiệu ứng mờ dần ở đây)
        SceneManager.LoadScene(nextSceneName);
    }
}