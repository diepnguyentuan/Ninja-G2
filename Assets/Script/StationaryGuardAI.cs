using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script AI cho quái vật đứng im, chỉ tấn công khi Player vào tầm
// Dùng chung cho Bat, Rat, và các quái vật "Lính gác" khác
public class StationaryGuardAI : MonoBehaviour, IDamageable
{
    [Header("AI Settings")]
    [Tooltip("Tầm tấn công của quái vật")]
    public float attackRange = 3f;
    [Tooltip("Thời gian chờ giữa 2 đòn đánh")]
    public float attackCooldown = 2f;
    [Tooltip("Layer của Player (Thường là 'Player')")]
    public LayerMask playerLayer;

    [Header("Health Settings")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Tham chiếu (Kéo thả)")]
    [Tooltip("Kéo Animator của quái vật vào đây")]
    public Animator animator;
    [Tooltip("Kéo Model (object con chứa hình ảnh) vào đây để lật mặt")]
    public Transform monsterModel;
    [Tooltip("Kéo Hitbox tấn công (có Tag 'EnemyAttack') vào đây")]
    public GameObject attackHitbox; // Object con có Trigger Collider và Tag "EnemyAttack"

    // Biến nội bộ
    private Transform playerTransform;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isFacingRight = true;
    private Vector3 initialModelScale;

    // Animator Hashes (Tên Trigger trong Animator Controller)
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashStuned = Animator.StringToHash("Stuned");
    private readonly int hashDeath = Animator.StringToHash("Death");

    void Start()
    {
        currentHealth = maxHealth;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (monsterModel != null) initialModelScale = monsterModel.localScale;

        // Đảm bảo hitbox luôn tắt khi bắt đầu
        if (attackHitbox != null) attackHitbox.SetActive(false);

        FindPlayer();
    }

    void Update()
    {
        // Nếu đang chết hoặc đang tấn công, không làm gì cả
        if (isDead || isAttacking) return;

        // Nếu mất dấu Player, tìm lại
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return; // Vẫn không tìm thấy, chờ frame sau
        }

        // Luôn quay mặt về phía Player
        FacePlayer();

        // Kiểm tra Player trong tầm và tấn công
        CheckForPlayerAndAttack();
    }

    void FindPlayer()
    {
        // Tối ưu: Dùng PlayerStats Singleton nếu có
        if (PlayerStats.instance != null)
        {
            playerTransform = PlayerStats.instance.transform;
        }
        else
        {
            // Cách dự phòng: Tìm bằng Tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    void FacePlayer()
    {
        if (playerTransform == null) return;

        bool shouldFaceRight = (playerTransform.position.x > transform.position.x);

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            if (monsterModel != null)
            {
                // Lật model con (giống Rồng)
                Vector3 modelScale = monsterModel.localScale;
                modelScale.x *= -1;
                monsterModel.localScale = modelScale;
            }
            else
            {
                // Cách lật dự phòng (giống Sói)
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
    }

    void CheckForPlayerAndAttack()
    {
        // Kiểm tra xem có Player trong vòng tròn tầm đánh không
        bool playerInAttackRange = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);

        // Nếu Player trong tầm VÀ đã hết cooldown
        if (playerInAttackRange && Time.time >= nextAttackTime)
        {
            StartAttack();
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        animator.SetTrigger(hashAttack); // Kích hoạt Animation "Attack"
    }

    // --- CÁC HÀM NÀY ĐƯỢC GỌI BẰNG ANIMATION EVENT ---
    // (Bạn phải thêm Event vào Animation Clip 'AttackRat'/'AttackBat')
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(true);
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    public void AttackComplete()
    {
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown; // Đặt Cooldown
    }

    // --- HÀM NHẬN SÁT THƯƠNG (TỪ INTERFACE) ---
    public void TakeDamage(int damage, Vector2 attackPosition)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Kích hoạt animation trúng đòn
            animator.SetTrigger(hashStuned);
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger(hashDeath);

        // Tắt vật lý và script
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().simulated = false;
        }
        this.enabled = false;

        // (Thêm logic rơi item/kinh nghiệm ở đây nếu muốn)

        Destroy(gameObject, 3f); // Hủy object sau 3 giây
    }

    // Vẽ Gizmos để bạn thấy tầm đánh trong Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}