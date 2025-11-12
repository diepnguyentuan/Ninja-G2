using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class RatAI : MonoBehaviour, IDamageable
{
    #region --- SETTINGS ---
    [Header("AI Movement")]
    public float attackRange = 2.5f; // Tầm đánh của chuột thường ngắn hơn dơi chút
    public float attackCooldown = 1.5f; // Chuột có thể đánh nhanh hơn
    private float nextAttackTime = 0f;

    [Header("Attack Timing")]
    [Tooltip("Thời gian chờ để vung tay (Giây)")]
    public float damageWindUpTime = 0.2f; // Chỉnh cho khớp animation cào của chuột
    [Tooltip("Thời gian Hitbox tồn tại (Giây)")]
    public float damageActiveTime = 0.2f;

    [Header("Combat Stats")]
    public int maxHealth = 50; // Máu chuột (tùy chỉnh)
    public int attackDamage = 15;

    // 🎯 KINH NGHIỆM CỦA CHUỘT: 100 XP
    [Tooltip("Số kinh nghiệm nhận được khi giết quái")]
    public int xpValue = 100;

    public GameObject attackHitbox; // Kéo AttackHitbox ở tay chuột vào đây
    private int currentHealth;

    [Header("References")]
    public Transform playerTransform;

    [Header("Health Bar UI")]
    [Tooltip("Kéo Prefab 'HealthBarCanvas' vào đây")]
    public GameObject healthBarPrefab;
    [Tooltip("Kéo GameObject con 'HealthBarAttachPoint' vào đây")]
    public Transform healthBarAttachPoint;

    private HealthBar healthBarScript;

    // Private Variables
    private Animator animator;
    private bool isFacingRight = false;
    private bool isDead = false;
    private bool isAttacking = false;

    private Collider2D weaponCollider;

    // Animator Hashes
    private readonly int hashAttackTrigger = Animator.StringToHash("AttackTrigger");
    private readonly int hashStunedTrigger = Animator.StringToHash("StunedTrigger");
    private readonly int hashDeathTrigger = Animator.StringToHash("DeathTrigger");
    private readonly int hashIdleBool = Animator.StringToHash("Idle");
    #endregion

    #region --- INITIALIZATION ---
    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // --- SETUP HITBOX ---
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            weaponCollider = attackHitbox.GetComponent<Collider2D>();

            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
                Debug.Log("✅ RatAI: Đã tắt Collider móng vuốt.");
            }
            else
            {
                Debug.LogError("❌ LỖI: GameObject 'AttackHitbox' của Chuột thiếu Collider!");
            }
        }
        else
        {
            Debug.LogError("❌ LỖI: Chưa kéo AttackHitbox vào script RatAI!");
        }

        // Xác định hướng dựa trên Scale (giống dơi)
        isFacingRight = transform.localScale.x < 0;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        animator.SetBool(hashIdleBool, true);

        if (healthBarPrefab != null && healthBarAttachPoint != null)
        {
            // 1. Tạo ra thanh máu
            GameObject hbObject = Instantiate(healthBarPrefab, healthBarAttachPoint);
            hbObject.transform.localPosition = Vector3.zero;

            // 2. Cố gắng lấy script HealthBar
            // Lưu ý: Dùng GetComponentInChildren nếu script nằm ở con, 
            // hoặc GetComponent nếu nằm ngay ở root của prefab.
            healthBarScript = hbObject.GetComponent<HealthBar>();

            if (healthBarScript == null)
            {
                // Thử tìm ở con nếu không thấy ở cha
                healthBarScript = hbObject.GetComponentInChildren<HealthBar>();
            }

            // 3. Kiểm tra kết quả và Cập nhật
            if (healthBarScript != null)
            {
                Debug.Log($"✅ Đã tìm thấy script! Cập nhật máu: {currentHealth} / {maxHealth}");
                healthBarScript.UpdateHealthBar(currentHealth, maxHealth);
            }
            else
            {
                Debug.LogError("❌ LỖI TO: Đã tạo ra Canvas nhưng KHÔNG tìm thấy script 'HealthBar' bên trong nó!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Cảnh báo: Chưa kéo HealthBarPrefab hoặc AttachPoint vào Inspector!");
        }
    }
    #endregion

    #region --- UPDATE LOOP ---
    void Update()
    {
        if (isDead || playerTransform == null) return;

        FlipTowardsPlayer();

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            if (!isAttacking)
            {
                animator.SetBool(hashIdleBool, true);
            }
        }
    }
    #endregion

    #region --- COMBAT LOGIC (COROUTINE) ---

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        animator.SetBool(hashIdleBool, false);
        animator.SetTrigger(hashAttackTrigger);

        // Chờ vung tay
        yield return new WaitForSeconds(damageWindUpTime);

        if (weaponCollider != null)
        {
            weaponCollider.enabled = true; // Bật sát thương
        }

        // Chờ gây sát thương
        yield return new WaitForSeconds(damageActiveTime);

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false; // Tắt sát thương
        }

        isAttacking = false;
        animator.SetBool(hashIdleBool, true);
    }

    public void TakeDamage(int amount, Vector2 playerPosition)
    {
        if (isDead) return;

        currentHealth -= amount;

        UpdateHealthUI();

        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            if (weaponCollider != null) weaponCollider.enabled = false;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger(hashStunedTrigger);
        }
    }

    void UpdateHealthUI()
    {
        if (healthBarScript != null)
        {
            healthBarScript.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        if (weaponCollider != null) weaponCollider.enabled = false;

        animator.SetTrigger(hashDeathTrigger);

        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;

        // 🎯 CỘNG 100 XP CHO PLAYER
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.GainExp(xpValue);
            Debug.Log($"Đã tiêu diệt Chuột! +{xpValue} XP");
        }

        Destroy(gameObject, 2f);
    }
    #endregion

    #region --- UTILITY ---
    void FlipTowardsPlayer()
    {
        if (isAttacking) return;

        float dir = playerTransform.position.x - transform.position.x;
        if (dir > 0 && !isFacingRight) Flip();
        else if (dir < 0 && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        if (healthBarAttachPoint != null)
        {
            Vector3 hbScale = healthBarAttachPoint.localScale;
            hbScale.x *= -1;
            healthBarAttachPoint.localScale = hbScale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}