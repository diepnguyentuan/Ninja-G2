using System.Collections;
using UnityEngine;

public class DragonAI : MonoBehaviour
{
    // --- Trạng thái của Rồng ---
    private enum AIState
    {
        Idle_OnPerch,   // 0: Chờ cutscene
        Intro_Jumping,  // 1: Intro
        Phase1_Fighting // 2: Chiến đấu
    }
    private AIState currentState = AIState.Idle_OnPerch;

    [Header("--- THAM CHIẾU BẮT BUỘC ---")]
    public Transform dragonModel;
    public Animator animator;
    [Tooltip("Kéo Rigidbody 2D của DragonRoot vào đây")]
    public Rigidbody2D rb;
    [Tooltip("Kéo đối tượng ClawHitbox vào đây (Quan trọng)")]
    public BossMeleeHitbox meleeHitbox;

    [Header("--- CÀI ĐẶT CHIẾN ĐẤU ---")]
    public float moveSpeed = 3.5f; // Tăng tốc độ di chuyển lên xíu cho hung hãn
    public float meleeRange = 4f;
    public float stopDistance = 2.0f; // Dừng lại gần hơn để dễ đánh trúng

    [Header("--- THỜI GIAN COOLDOWN ---")]
    public float attackCooldown = 2.0f; // Đánh nhanh hơn (giảm từ 3s xuống 2s)
    public float jumpCooldown = 8.0f;   // Nhảy thường xuyên hơn
    public float jumpForceX = 15f;
    public float jumpForceY = 10f;

    [Header("--- CÀI ĐẶT INTRO ---")]
    public float introJumpDuration = 1.5f;
    public float introJumpHeight = 4f;

    // Biến nội bộ
    private Transform playerTransform;
    private Vector3 initialPerchPosition;
    private float nextAttackTime = 0f;
    private float nextJumpTime = 0f;

    // Cờ trạng thái (State Flags)
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isMoving = false;
    private bool isFacingRight_Sprite = false;

    void Start()
    {
        initialPerchPosition = transform.position;
        this.enabled = false; // Tắt script chờ Cutscene gọi

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true; // Treo trên tường thì tắt vật lý

        // Xác định hướng nhìn ban đầu
        if (dragonModel != null)
            isFacingRight_Sprite = (dragonModel.localScale.x < 0);

        // Đảm bảo Hitbox tắt ngay từ đầu để an toàn
        if (meleeHitbox != null) meleeHitbox.gameObject.SetActive(false);
    }

    // --- HÀM NÀY ĐƯỢC GỌI BỞI BOSS INTRO TRIGGER ---
    public void StartCombat()
    {
        this.enabled = true;

        // Kiểm tra PlayerStats Singleton
        if (PlayerStats.instance != null)
        {
            playerTransform = PlayerStats.instance.transform;
        }
        else
        {
            Debug.LogError("DragonAI: LỖI! Không tìm thấy PlayerStats.instance. Rồng sẽ đứng im.");
            return;
        }

        if (currentState == AIState.Idle_OnPerch)
        {
            StartCoroutine(IntroJumpRoutine());
        }
    }

    #region LOGIC DI CHUYỂN VÀ QUYẾT ĐỊNH (BRAIN)

    void Update()
    {
        // Nếu không phải pha chiến đấu, hoặc thiếu Player, hoặc đang bận đánh/nhảy -> Return
        if (currentState != AIState.Phase1_Fighting || playerTransform == null || isAttacking || isJumping)
        {
            // Nếu đang bận mà vẫn trôi (velocity) thì hãm lại cho an toàn
            if ((isAttacking || isJumping) && rb != null && !isJumping)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            return;
        }

        FacePlayer(); // Luôn quay mặt về phía đối thủ

        // Nếu đã sẵn sàng hành động tiếp theo
        if (Time.time >= nextAttackTime)
        {
            DecideAction();
        }
        else
        {
            // Trong thời gian chờ hồi chiêu đánh, Rồng sẽ tìm cách tiếp cận
            MoveTowardsPlayerIfNeeded();
        }
    }

    void DecideAction()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        // Debug.Log($"Khoảng cách tới Player: {distance}");

        // 1. NẾU TRONG TẦM ĐÁNH -> VẢ LUÔN
        if (distance <= meleeRange)
        {
            Debug.LogWarning(">> Đã vào tầm! Tấn công Melee!");
            StartCoroutine(MeleeAttackRoutine());
        }
        // 2. NẾU Ở XA VÀ HỒI CHIÊU NHẢY -> NHẢY BỔ VÀO
        else if (Time.time >= nextJumpTime && distance > meleeRange)
        {
            Debug.Log(">> Xa quá! Nhảy tiếp cận!");
            StartCoroutine(JumpRoutine());
        }
        // 3. CÒN LẠI -> CHẠY BỘ TIẾP CẬN
        else
        {
            MoveTowardsPlayerIfNeeded();
        }
    }

    // Logic di chuyển bám đuổi
    void MoveTowardsPlayerIfNeeded()
    {
        if (isAttacking || isJumping) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Chỉ di chuyển nếu khoảng cách > khoảng cách dừng
        if (distance > stopDistance)
        {
            if (!isMoving)
            {
                isMoving = true;
                animator.SetTrigger("MoveTrigger");
            }

            // Tính hướng
            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        }
        else
        {
            // Đã đến đủ gần -> Dừng lại
            StopMoving();
        }
    }

    void StopMoving()
    {
        if (isMoving)
        {
            isMoving = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
            // Có thể set trigger Idle nếu Animator cần
        }
    }

    // Logic quay mặt (Flip)
    void FacePlayer()
    {
        if (playerTransform == null || isAttacking) return; // Đang đánh thì không được xoay lung tung

        bool playerIsOnRight = (playerTransform.position.x > transform.position.x);
        if (playerIsOnRight != isFacingRight_Sprite)
        {
            isFacingRight_Sprite = playerIsOnRight;
            float scaleX = Mathf.Abs(dragonModel.localScale.x) * (playerIsOnRight ? -1 : 1);
            dragonModel.localScale = new Vector3(scaleX, dragonModel.localScale.y, dragonModel.localScale.z);
        }
    }

    #endregion

    #region CÁC HÀNH ĐỘNG CỤ THỂ (ACTIONS)

    // --- TẤN CÔNG CẬN CHIẾN (ĐÃ SỬA LOGIC HITBOX) ---
    private IEnumerator MeleeAttackRoutine()
    {
        StopMoving(); // Đứng lại để đánh
        isAttacking = true;
        animator.SetTrigger("AttackTrigger");

        // 1. Chờ tay giơ lên (Animation Wind-up)
        // Bạn có thể chỉnh số 0.3f này cho khớp với Animation của bạn
        yield return new WaitForSeconds(0.3f);

        // 2. BẬT HITBOX (QUAN TRỌNG NHẤT)
        if (meleeHitbox != null)
        {
            // Bật GameObject lên để Physics bắt đầu tính toán va chạm
            meleeHitbox.gameObject.SetActive(true);
            meleeHitbox.ResetAndActivate();
            // Debug.Log(">>> HITBOX ON");
        }

        // 3. Giữ Hitbox trong thời gian ngắn (Thời gian gây đam)
        yield return new WaitForSeconds(0.2f);

        // 4. TẮT HITBOX
        if (meleeHitbox != null)
        {
            meleeHitbox.Deactivate();
            meleeHitbox.gameObject.SetActive(false); // Tắt đi để không gây đam ảo
            // Debug.Log(">>> HITBOX OFF");
        }

        // 5. Chờ nốt Animation kết thúc (Back-swing)
        yield return new WaitForSeconds(1.0f);

        // Reset
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    // --- NHẢY TIẾP CẬN (GAP CLOSER) ---
    private IEnumerator JumpRoutine()
    {
        StopMoving();
        isJumping = true;
        animator.SetTrigger("JumpTrigger");

        // Tính hướng nhảy về phía Player
        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);

        // Reset vận tốc cũ trước khi nhảy
        rb.velocity = Vector2.zero;

        // Bùm! Nhảy
        rb.AddForce(new Vector2(direction * jumpForceX, jumpForceY), ForceMode2D.Impulse);

        // Thời gian trên không (ước lượng)
        yield return new WaitForSeconds(1.2f);

        isJumping = false;
        nextJumpTime = Time.time + jumpCooldown;
    }

    // --- INTRO CUTSCENE (GIỮ NGUYÊN) ---
    private IEnumerator IntroJumpRoutine()
    {
        currentState = AIState.Intro_Jumping;
        isJumping = true;
        animator.SetTrigger("JumpTrigger");

        float elapsedTime = 0f;
        Vector3 startPos = initialPerchPosition;
        if (playerTransform == null) yield break;

        Vector3 endPos = new Vector3(playerTransform.position.x, playerTransform.position.y, 0);

        while (elapsedTime < introJumpDuration)
        {
            float height = Mathf.Sin(Mathf.PI * elapsedTime / introJumpDuration) * introJumpHeight;
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / introJumpDuration) + Vector3.up * height;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        isJumping = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        currentState = AIState.Phase1_Fighting;
        nextAttackTime = Time.time + 1.0f;
        nextJumpTime = Time.time + 5.0f; // Cho 5s rồi mới bắt đầu nhảy nhót
    }

    #endregion
}