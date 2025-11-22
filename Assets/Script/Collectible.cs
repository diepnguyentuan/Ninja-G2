using UnityEngine;

// Đảm bảo vật phẩm có Collider2D
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    // Loại vật phẩm (để phân biệt Coin và Potion)
    public enum ItemType { Coin, HealthPotion }
    public ItemType itemType = ItemType.Coin;

    public int healAmount = 20; // Lượng máu hồi (chỉ dùng nếu là HealthPotion)

    private bool collected = false; // Tránh nhặt nhiều lần

    void Start()
    {
        // Đảm bảo Collider là Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"Collider trên {gameObject.name} chưa được đặt là Trigger!", this);
            col.isTrigger = true;
        }
    }

    // Hàm này được gọi khi có GameObject khác đi vào Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải Player không và chưa được nhặt
        if (!collected && other.CompareTag("Player"))
        {
            Collect(other.gameObject); // Gọi hàm xử lý nhặt
        }
    }

    // Hàm xử lý khi được nhặt
    private void Collect(GameObject playerObject)
    {
        collected = true; // Đánh dấu đã nhặt

        Debug.Log($"Nhặt được: {itemType}");

        // Xử lý tùy theo loại vật phẩm
        switch (itemType)
        {
            case ItemType.Coin:
                // TODO: Tăng điểm số hoặc tiền của người chơi
                // Ví dụ: ScoreManager.instance.AddScore(1);
                break;

            case ItemType.HealthPotion:
                // Gọi hàm Heal trong PlayerStats
                if (PlayerStats.instance != null)
                {
                    PlayerStats.instance.Heal(healAmount);
                }
                break;
        }

        // TODO: Thêm hiệu ứng âm thanh hoặc particle khi nhặt

        // Biến mất vật phẩm
        Destroy(gameObject);
    }
}