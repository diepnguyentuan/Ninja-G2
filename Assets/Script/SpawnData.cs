using UnityEngine;

// Dòng này cho phép bạn tạo file asset từ script này trong menu Create
[CreateAssetMenu(fileName = "PlayerSpawnData", menuName = "Game/Player Spawn Data")]
public class SpawnData : ScriptableObject
{
    // Tên của điểm spawn mà Player sẽ được dịch chuyển đến
    public string nextSpawnPointName;
}