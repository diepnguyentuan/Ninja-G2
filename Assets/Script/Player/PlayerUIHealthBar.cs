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
        PlayerStats.OnHealthChanged += UpdateHealthBar;
    }

    // Hủy đăng ký "lắng nghe" khi script bị tắt (rất quan trọng)
    void OnDisable()
    {
        // Chỉ hủy đăng ký
        PlayerStats.OnHealthChanged -= UpdateHealthBar;
    }

    // Hàm Start() chạy sau TẤT CẢ các hàm Awake()
    // Giúp lấy giá trị máu ban đầu một cách an toàn
    void Start()
    {
        if (PlayerStats.instance != null)
        {
            // Lấy giá trị máu ban đầu ngay khi bắt đầu
            UpdateHealthBar(PlayerStats.instance.currentHealth, PlayerStats.instance.maxHealth);
        }
    }

    // Hàm này sẽ được PlayerStats tự động gọi nhờ "event"
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (fillImage == null) return;

        if (maxHealth <= 0)
        {
            fillImage.fillAmount = 0;
        }
        else
        {
            // Tính toán tỉ lệ và cập nhật thanh "Fill"
            // Phải ép kiểu (float) để phép chia ra số thập phân
            fillImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}