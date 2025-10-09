using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 6f;
    public float jumpHeight = 12f; // đang dùng như vận tốc Y ban đầu

    // NEW — cooldown cho đòn chém
    [Header("Attack")]
    public float attackCooldown = 0.35f; // thời gian chờ giữa 2 lần bấm Space
    float nextAttackTime = 0f;           // thời điểm được phép chém tiếp

    bool grounded;
    bool facingRight = true;

    Rigidbody2D myBody;
    Animator myAnim;

    bool jumpQueued;

    void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();

        myBody.freezeRotation = true;
        if (myBody.gravityScale <= 0) myBody.gravityScale = 2f;
    }

    void Update()
    {
        // NEW — Attack có cooldown + không cho khi đang trong state Attack
        if (Input.GetKeyDown(KeyCode.Space) && myAnim)
        {
            if (Time.time >= nextAttackTime && !IsInAttack()) // tránh spam
            {
                myAnim.SetTrigger("Attack");                  // Idle/Run -> Attack_Slash
                nextAttackTime = Time.time + attackCooldown;  // đặt lại thời điểm cho lần kế
            }
        }

        // Nhấn W 1 lần để nhảy
        if (Input.GetKeyDown(KeyCode.W) && grounded)
        {
            jumpQueued = true;
            if (myAnim) myAnim.SetTrigger("Jump");
        }

        if (myAnim) myAnim.SetBool("IsGrounded", grounded);
    }

    void FixedUpdate()
    {
        float move = Input.GetAxisRaw("Horizontal");

        myBody.velocity = new Vector2(move * maxSpeed, myBody.velocity.y);

        if (move > 0 && !facingRight) flip();
        else if (move < 0 && facingRight) flip();

        if (myAnim) myAnim.SetFloat("speed", Mathf.Abs(move));

        if (jumpQueued && grounded)
        {
            grounded = false;
            myBody.velocity = new Vector2(myBody.velocity.x, jumpHeight);
        }
        jumpQueued = false;
    }

    // ===== Ground detection theo Tag "ground" =====
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("ground"))
            grounded = true;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("ground"))
            grounded = true;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("ground"))
            grounded = false;
    }

    void flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // NEW — kiểm tra có đang ở state Attack không (dựa vào Tag "Attack")
    bool IsInAttack()
    {
        if (!myAnim) return false;
        var info = myAnim.GetCurrentAnimatorStateInfo(0);
        if (info.IsTag("Attack")) return true;

        // Nếu bạn chưa set Tag trong Animator, tạm fallback theo tên:
        // return info.IsName("Attack_Slash");
        return false;
    }
}
