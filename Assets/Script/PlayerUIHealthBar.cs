using UnityEngine;
using UnityEngine.UI; // Cần thư viện UI

[RequireComponent(typeof(Image))]
public class PlayerUIHealthBar : MonoBehaviour
{
    private Image fillImage;

    void Awake()
    {
        fillImage = GetComponent<Image>();
    }

    // Đăng ký "lắng nghe" sự kiện khi script này được bật
    void OnEnable()
    {
        // Chỉ đăng ký
        if (PlayerStats.instance != null)
        {
            PlayerStats.OnHealthChanged += UpdateHealthBar;
        }
    }

    // Hủy đăng ký "lắng nghe" khi script bị tắt (rất quan trọng)
    void OnDisable()
    {
        // Chỉ hủy đăng ký
        if (PlayerStats.instance != null)
        {
            PlayerStats.OnHealthChanged -= UpdateHealthBar;
        }

        // Hàm Start() chạy sau TẤT CẢ các hàm Awake()
        // Giúp lấy giá trị máu ban đầu một cách an toàn
    }
    void Start()
    {
        if (PlayerStats.instance != null)
        {
            // Lấy giá trị máu ban đầu ngay khi bắt đầu
            if (PlayerStats.instance != null)
            {
                UpdateHealthBar(PlayerStats.instance.currentHealth, PlayerStats.instance.maxHealth);
            }
            else
            {
                Debug.LogWarning("⚠ PlayerStats.instance chưa tồn tại trong scene!");
            }
        }
    }

    // Hàm này sẽ được PlayerStats tự động gọi nhờ "event"
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (fillImage == null)
        {
            Debug.LogError("⚠ Chưa gán Image cho PlayerUIHealthBar!");
            return;
        }

        // Nếu maxHealth <= 0, tránh chia cho 0
        if (maxHealth <= 0)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        // Tính tỉ lệ và gán vào thanh Fill
        fillImage.fillAmount = Mathf.Clamp01((float)currentHealth / maxHealth);
    }
}