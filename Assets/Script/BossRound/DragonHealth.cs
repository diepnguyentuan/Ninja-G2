using UnityEngine;
using UnityEngine.UI; // Cần thư viện UI

// Quan trọng: Phải "kế thừa" IDamageable
public class DragonHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 500;
    private int currentHealth;

    [Header("Tham chiếu (Kéo thả)")]
    [Tooltip("Kéo Image 'BossHealthBar_Fill' từ Canvas vào đây")]
    public Image bossHealthFillImage; // Thanh máu của Boss (trên Canvas)

    [Tooltip("Kéo Animator của Rồng (DragonRed) vào đây")]
    public Animator animator;

    [Tooltip("Kéo script AI của Rồng (Dragon_Root) vào đây")]
    public DragonAI dragonAI;

    void Start()
    {
        currentHealth = maxHealth;
        // Cập nhật thanh máu lúc đầu (đầy)
        UpdateHealthBar();
    }

    // --- HÀM TỪ INTERFACE IDamageable ---
    // Hàm này sẽ được PlayerMove.PerformHitCheck() TỰ ĐỘNG GỌI
    public void TakeDamage(int damage, Vector2 playerPosition)
    {
        if (currentHealth <= 0) return; // Đã chết

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log("Rồng nhận " + damage + " sát thương!");
        UpdateHealthBar();

        // Kích hoạt animation trúng đòn
        animator.SetTrigger("StunedTrigger"); // Dùng Trigger bạn đã có

        // Kiểm tra Giai đoạn 2 (ví dụ 70% máu)
        if (currentHealth <= (maxHealth * 0.7f))
        {
            // (Thêm logic Giai đoạn 2 ở đây - ví dụ: dragonAI.StartPhase2())
        }

        // Kiểm tra chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (bossHealthFillImage != null)
        {
            // Ép kiểu (float) để phép chia ra số thập phân
            bossHealthFillImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("Rồng đã chết!");
        animator.SetTrigger("DeathTrigger");

        // Tắt AI
        if (dragonAI != null) dragonAI.enabled = false;

        // Tắt Collider để Player không đánh trúng nữa
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Tắt script này
        this.enabled = false;
    }
}