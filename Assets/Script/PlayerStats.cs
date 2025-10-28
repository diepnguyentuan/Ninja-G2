using UnityEngine;
using System; // Cần cho "Action"

public class PlayerStats : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static PlayerStats instance; // Biến static để các script khác truy cập

    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    // Event này sẽ "phát sóng" thông báo mỗi khi máu thay đổi
    public static event Action<int, int> OnHealthChanged;

    void Awake()
    {
        // --- Logic Singleton ---
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Khởi tạo máu
        currentHealth = maxHealth;
    }

    // Hàm Start() chạy sau Awake()
    // Phát sóng trạng thái máu ban đầu cho bất kỳ UI nào đã lắng nghe
    void Start()
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }
    }

    // Hàm public để nhận sát thương
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Đã chết

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // Phát sóng sự kiện
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }

        // Kiểm tra nếu chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Hàm public để hồi máu
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Phát sóng sự kiện
        if (OnHealthChanged != null)
        {
            OnHealthChanged(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Thêm logic chết ở đây (ví dụ: tải lại scene)
    }
}