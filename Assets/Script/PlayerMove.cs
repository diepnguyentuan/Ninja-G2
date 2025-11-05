using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 6f;
    public float jumpHeight = 12f;

    [Header("Attack")]
    public float attackCooldown = 0.35f;
    float nextAttackTime = 0f;
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public int attackDamage = 1; // Nên là 1 để khớp với máu quái vật là 3
    public LayerMask monsterLayer;

    // --- THÊM CÁC BIẾN NÀY TỪ FIRESHOOTER ---
    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Transform firePoint;          // Điểm bắn (Empty ở tay)
    public int fireballDamage = 100;
    public float projectileSpeed = 12f;
    public float projectileRange = 6f;
    public float fireCooldown = 0.4f;
    private float nextFireTime = 0f;

    [Header("Physics")]
    public float knockbackForce = 15f; // Lực bị văng đi

    bool grounded;
    bool facingRight = true;
    bool jumpQueued;
    private bool isKnockedBack = false; // Trạng thái bị văng/choáng

    Rigidbody2D myBody;
    Animator myAnim;

    void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        myBody.freezeRotation = true;
        if (myBody.gravityScale <= 0) myBody.gravityScale = 2f;
        if (attackPoint == null)
        {
            Debug.LogError("Chưa gán AttackPoint cho PlayerMove script!");
        }
    }

    void Update()
    {
        // Không nhận input nếu đang bị văng/choáng
        if (isKnockedBack) return;

        // Xử lý tấn công
        if (Input.GetKeyDown(KeyCode.Space) && myAnim)
        {
            if (Time.time >= nextAttackTime && !IsInAttack())
            {
                myAnim.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;
                PerformHitCheck();
            }
        }

        // --- THÊM LOGIC BẮN TỪ FIRESHOOTER ---
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
        {
            ShootFireball();
            nextFireTime = Time.time + fireCooldown;
        }

        // Xử lý nhảy
        if (Input.GetKeyDown(KeyCode.W) && grounded)
        {
            jumpQueued = true;
            if (myAnim) myAnim.SetTrigger("Jump");
        }

        // Cập nhật Animator
        if (myAnim) myAnim.SetBool("IsGrounded", grounded);
    }

    void ShootFireball()
    {
        if (!firePoint || !fireballPrefab) return;

        // Lấy hướng từ biến "facingRight" 
        int dir = facingRight ? 1 : -1;
        // --------------------------------

        var go = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        var fb = go.GetComponent<Fireball>();
        if (fb)
        {
            fb.speed = projectileSpeed;
            fb.maxDistance = projectileRange;
            fb.damage = fireballDamage;
            fb.Launch(dir);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb) rb.velocity = new Vector2(dir * projectileSpeed, 0f);
            Destroy(go, projectileRange / Mathf.Max(0.01f, projectileSpeed));
        }
    }

    // Thực hiện kiểm tra va chạm đòn đánh
    void PerformHitCheck()
    {
        if (attackPoint == null) return;
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, monsterLayer);
        foreach (Collider2D collider in hitObjects)
        {
            IDamageable damageableObject = collider.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                // Gửi vị trí của Player để quái biết hướng văng
                damageableObject.TakeDamage(attackDamage, transform.position);
            }
        }
    }

    void FixedUpdate()
    {
        // Nếu đang bị văng/choáng, không xử lý di chuyển input
        if (isKnockedBack) return;

        // Xử lý di chuyển ngang
        float move = Input.GetAxisRaw("Horizontal");
        myBody.velocity = new Vector2(move * maxSpeed, myBody.velocity.y);

        // Xử lý lật mặt
        if (move > 0 && !facingRight) flip();
        else if (move < 0 && facingRight) flip();

        // Cập nhật Animator
        if (myAnim) myAnim.SetFloat("speed", Mathf.Abs(move));

        // Xử lý nhảy (trong FixedUpdate để tương tác tốt với vật lý)
        if (jumpQueued && grounded)
        {
            grounded = false; // Đặt grounded thành false NGAY LẬP TỨC khi nhảy
            myBody.velocity = new Vector2(myBody.velocity.x, jumpHeight);
            // Không cần myAnim.SetTrigger("Jump") ở đây nữa nếu đã có trong Update
        }
        jumpQueued = false; // Reset cờ chờ nhảy
    }

    // Phát hiện chạm đất
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("ground"))
            grounded = true;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        // Dùng Stay để đảm bảo grounded=true nếu đứng yên trên mép
        if (collision.collider.CompareTag("ground"))
            grounded = true;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("ground"))
            grounded = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ xử lý nếu va chạm với hitbox địch và chưa bị văng
        if (other.CompareTag("EnemyAttack") && !isKnockedBack)
        {
            Debug.Log("Player đã bị trúng đòn!");

            // Trừ máu thông qua Singleton
            if (PlayerStats.instance != null)
            {
                PlayerStats.instance.TakeDamage(10); // Ví dụ sát thương quái = 10
            }

            // Tính toán và áp dụng lực văng
            Vector2 knockbackDirection = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;
            myBody.velocity = Vector2.zero;
            myBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // Kích hoạt trạng thái bị văng/choáng
            isKnockedBack = true;

            CameraFollow camFollow = Camera.main?.GetComponent<CameraFollow>();

            // 1. TẮT FOLLOW VÀ KÍCH HOẠT RUNG
            if (camFollow != null)
            {
                camFollow.enabled = false;
                camFollow.TriggerShake();
            }

            // Dừng Coroutine cũ nếu có và bắt đầu Coroutine mới bằng tên chuỗi
            StopCoroutine("KnockbackCooldown");
            StartCoroutine("KnockbackCooldown");
        }
    }

    // Coroutine để kết thúc trạng thái bị văng/choáng sau một khoảng thời gian
    private IEnumerator KnockbackCooldown()
    {
        yield return new WaitForSeconds(0.2f);

        CameraFollow camFollow = Camera.main?.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.enabled = true;
        }
        // -----------------------------

        isKnockedBack = false; // Cho phép điều khiển trở lại
    }

    // Hàm lật mặt nhân vật
    void flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // Kiểm tra xem có đang trong animation tấn công không (dùng Tag)
    bool IsInAttack()
    {
        if (!myAnim) return false;
        // Layer 0 là layer animation cơ bản
        AnimatorStateInfo info = myAnim.GetCurrentAnimatorStateInfo(0);
        return info.IsTag("Attack"); // Bạn cần đặt Tag "Attack" cho state tấn công trong Animator
    }

    // Cung cấp trạng thái chạm đất cho script khác
    public bool IsGrounded()
    {
        return grounded;
    }

    // Vẽ Gizmos cho tầm đánh
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}