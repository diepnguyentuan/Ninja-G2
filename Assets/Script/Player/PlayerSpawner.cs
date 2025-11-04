using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Kéo file PlayerSpawnData (ScriptableObject) vào đây.")]
    [SerializeField] private SpawnData spawnData;

    [Header("Default Spawn")]
    [Tooltip("Tên điểm spawn sẽ được dùng nếu không có dữ liệu chuyển scene (ví dụ: chạy thẳng scene này).")]
    [SerializeField] private string defaultSpawnName = "Default";

    // Sử dụng Awake() thay vì Start() để đảm bảo script này chạy SỚM nhất có thể 
    // sau khi scene được tải, trước khi các script khác cố gắng sử dụng Player.
    void Awake()
    {
        // Khởi tạo biến Transform của Player
        Transform playerTransform = null;

        // CÁCH 1: Ưu tiên tìm Player thông qua Singleton Pattern (Đáng tin cậy nhất cho DontDestroyOnLoad)
        if (PlayerStats.instance != null)
        {
            playerTransform = PlayerStats.instance.transform;
        }

        // CÁCH 2: Nếu Singleton chưa được thiết lập, tìm bằng Tag (ít đáng tin cậy hơn)
        if (playerTransform == null)
        {
            GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null)
            {
                playerTransform = playerByTag.transform;
            }
        }

        // Kiểm tra cuối cùng: Nếu vẫn không tìm thấy Player, thông báo lỗi và dừng
        if (playerTransform == null)
        {
            Debug.LogError("PlayerSpawner: KHÔNG THỂ TÌM THẤY Player (kiểm tra Player có Tag 'Player' và có script PlayerStats chưa).");
            return;
        }

        // --- BẮT ĐẦU LOGIC CHUYỂN CẢNH ---

        // 1. Đọc "bức thư" (Spawn Name đã được lưu từ scene trước)
        string targetSpawnName = spawnData != null && !string.IsNullOrEmpty(spawnData.nextSpawnPointName)
                                 ? spawnData.nextSpawnPointName
                                 : defaultSpawnName;

        // 2. Tìm tất cả "hòm thư" (PlayerSpawnPoint) trong scene
        PlayerSpawnPoint[] allSpawnPoints = FindObjectsOfType<PlayerSpawnPoint>();
        Transform targetPoint = null;

        // 3. Tìm "hòm thư" có tên khớp với tên gửi đến
        foreach (PlayerSpawnPoint point in allSpawnPoints)
        {
            if (point.spawnName == targetSpawnName)
            {
                targetPoint = point.transform;
                Debug.Log($"[SUCCESS] PlayerSpawner: Đã tìm thấy điểm spawn: '{targetSpawnName}'");
                break; // Tìm thấy rồi!
            }
        }

        // 4. Nếu không tìm thấy điểm đích (có thể do lỗi gõ tên):
        if (targetPoint == null)
        {
            Debug.LogWarning($"[WARNING] PlayerSpawner: Không tìm thấy điểm spawn đích tên: '{targetSpawnName}'. Đang thử tìm điểm '{defaultSpawnName}'.");

            // Thử tìm điểm mặc định (để Player không bị treo)
            foreach (PlayerSpawnPoint point in allSpawnPoints)
            {
                if (point.spawnName == defaultSpawnName)
                {
                    targetPoint = point.transform;
                    Debug.Log($"[SUCCESS] PlayerSpawner: Đã chuyển Player đến điểm mặc định: '{defaultSpawnName}'");
                    break;
                }
            }
        }

        // 5. Dịch chuyển Player đến vị trí tìm được
        if (targetPoint != null)
        {
            // Thiết lập vị trí mới cho Player
            playerTransform.position = targetPoint.position;

            // Đảm bảo Player được kích hoạt (nếu bạn có tắt nó trong quá trình load)
            playerTransform.gameObject.SetActive(true);

            // 6. Xóa "bức thư" (Xóa tên đã lưu trong SpawnData)
            // Việc này rất quan trọng để khi bạn quay lại Scene này lần nữa, nó sẽ dùng Default.
            if (spawnData != null)
            {
                spawnData.nextSpawnPointName = null;
            }
        }
        else
        {
            Debug.LogError($"[FATAL] PlayerSpawner: Không tìm thấy BẤT KỲ spawn point nào. Player sẽ ở vị trí mặc định cũ.");
        }
    }
}