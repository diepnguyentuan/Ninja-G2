using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private SpawnData spawnData; // Kéo file "PlayerSpawnData" vào đây

    [Header("Default Spawn")]
    [SerializeField] private string defaultSpawnName = "Default"; // Tên điểm spawn mặc định

    void Start()
    {
        // Tìm Player trong scene bằng Tag (Script PlayerMove của bạn đã dùng Tag "Player" rồi)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("PlayerSpawner: Không tìm thấy Player trong scene!");
            return;
        }

        // 1. Đọc "bức thư"
        string targetSpawnName = spawnData != null ? spawnData.nextSpawnPointName : defaultSpawnName;

        // Nếu "bức thư" trống (ví dụ: bạn chạy thẳng từ scene Village), dùng tên mặc định
        if (string.IsNullOrEmpty(targetSpawnName))
        {
            targetSpawnName = defaultSpawnName;
        }

        // 2. Tìm tất cả "hòm thư" (PlayerSpawnPoint) trong scene
        PlayerSpawnPoint[] allSpawnPoints = FindObjectsOfType<PlayerSpawnPoint>();
        Transform targetPoint = null;

        // 3. Tìm "hòm thư" có tên khớp
        foreach (PlayerSpawnPoint point in allSpawnPoints)
        {
            if (point.spawnName == targetSpawnName)
            {
                targetPoint = point.transform;
                break; // Tìm thấy rồi!
            }
        }

        // 4. Nếu không tìm thấy (gõ sai tên, v.v.), hãy cảnh báo
        if (targetPoint == null)
        {
            Debug.LogWarning($"Không tìm thấy spawn point tên: '{targetSpawnName}'.");
            // Thử tìm điểm mặc định
            foreach (PlayerSpawnPoint point in allSpawnPoints)
            {
                if (point.spawnName == defaultSpawnName)
                {
                    targetPoint = point.transform;
                    break;
                }
            }
        }

        // 5. Dịch chuyển Player
        if (targetPoint != null)
        {
            player.transform.position = targetPoint.position;
            Debug.Log($"Đã dịch chuyển Player đến: {targetSpawnName}");
        }
        else
        {
            Debug.LogError("Không tìm thấy BẤT KỲ spawn point nào! Player sẽ ở vị trí mặc định.");
        }
    }
}