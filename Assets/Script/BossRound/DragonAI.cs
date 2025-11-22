using System.Collections;
using UnityEngine;

public class DragonAI : MonoBehaviour
{
    // --- Trạng thái của Rồng ---
    private enum AIState
    {
        Idle_OnPerch,   // 0: Đứng im trên mỏm đá (chờ cutscene)
        Intro_Jumping,  // 1: Đang bay/nhảy xuống mặt đất
        Phase1_Fighting // 2: Đang chiến đấu (di chuyển, tấn công)
    }
    private AIState currentState = AIState.Idle_OnPerch;

    [Header("Tham chiếu (Kéo thả)")]
    private Transform playerTransform;
    public Transform dragonModel;
    public Animator animator;

    [Tooltip("Kéo Rigidbody 2D của DragonRoot vào đây")]
    public Rigidbody2D rb; // <== MỚI

    [Header("Cài đặt Chiến đấu (GĐ 1)")]
    public float moveSpeed = 2.5f;
    // ĐÃ LOẠI BỎ groundLevelY
    public float meleeRange = 4f;
    public float rangedRange = 15f;
    public float stopDistance = 3.5f;

    [Header("Thời gian Cooldown")]
    public float attackCooldown = 3.0f;
    public float jumpCooldown = 10.0f;
    public float jumpForceX = 15f; // Lực nhảy ngang
    public float jumpForceY = 10f; // Lực nhảy dọc

    [Header("Cài đặt Intro Jump")]
    public float introJumpDuration = 1.5f;
    public float introJumpHeight = 4f;
    private Vector3 initialPerchPosition;

    // Biến trạng thái nội bộ
    private float nextAttackTime = 0f;
    private float nextJumpTime = 0f;
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isMoving = false;
    private bool isFacingRight_Sprite = false;

    [Header("Tham chiếu Tấn công Gần")]
    [Tooltip("Kéo đối tượng ClawHitbox vào đây")]
    public BossMeleeHitbox meleeHitbox;

    void Start()
    {
        initialPerchPosition = transform.position;
        this.enabled = false;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        // Nếu Rồng có Rigidbody, đảm bảo nó là Kinematic khi chờ trên vách đá 
        if (rb != null) rb.isKinematic = true;

        if (dragonModel != null)
        {
            isFacingRight_Sprite = (dragonModel.localScale.x < 0);
        }
    }

    public void StartCombat()
    {
        this.enabled = true;
        if (PlayerStats.instance != null)
        {
            playerTransform = PlayerStats.instance.transform;
        }
        else
        {
            Debug.LogError("DragonAI: KHÔNG TÌM THẤY PlayerStats.instance!");
            currentState = AIState.Idle_OnPerch;
            return;
        }

        if (currentState == AIState.Idle_OnPerch)
        {
            StartCoroutine(IntroJumpRoutine());
        }
    }

    // --- COROUTINE NHẢY XUỐNG ĐẤT (Cinematic) ---
    private IEnumerator IntroJumpRoutine()
    {
        currentState = AIState.Intro_Jumping;
        isJumping = true;
        animator.SetTrigger("JumpTrigger");

        float elapsedTime = 0f;
        Vector3 startPos = initialPerchPosition;
        if (playerTransform == null) yield break;

        // Vị trí đáp X: Vị trí Player, Vị trí đáp Y: Vị trí Rồng đang đứng (Y sẽ được vật lý xử lý sau)
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

        // BẬT LẠI VẬT LÝ sau khi Intro Jump xong (Gravity sẽ kéo Rồng xuống Ground)
        if (rb != null)
        {
            // 1. CHUYỂN SANG DYNAMIC (ĐỂ TRỌNG LỰC KÉO XUỐNG)
            rb.isKinematic = false;

            // 2. MỞ KHÓA TRỤC X VÀ Y, CHỈ KHÓA XOAY Z
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // GIAI ĐOẠN 1 BẮT ĐẦU
        currentState = AIState.Phase1_Fighting;
        nextAttackTime = Time.time + 1.0f;
        nextJumpTime = Time.time + jumpCooldown;
    }


    // --- VÒNG LẶP AI CHÍNH (GIAI ĐOẠN 1) ---
    void Update()
    {
        if (currentState != AIState.Phase1_Fighting || playerTransform == null || isAttacking || isJumping)
        {
            if (isMoving)
            {
                isMoving = false;
                if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);
            }
            return;
        }

        FacePlayer();

        if (Time.time >= nextJumpTime)
        {
            StartCoroutine(JumpRoutine(true)); // Nhảy né/lùi sau Cooldown (10s)
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            DecideAction();
        }
        else
        {
            MoveTowardsPlayerIfNeeded(false);
        }
    }

    // --- SỬA LỖI QUAY MẶT (Giữ nguyên) ---
    void FacePlayer()
    {
        if (playerTransform == null) return;
        bool playerIsOnRight = (playerTransform.position.x > transform.position.x);
        if (playerIsOnRight != isFacingRight_Sprite)
        {
            isFacingRight_Sprite = playerIsOnRight;
            float scaleX = Mathf.Abs(dragonModel.localScale.x) * (playerIsOnRight ? -1 : 1);
            dragonModel.localScale = new Vector3(scaleX, dragonModel.localScale.y, dragonModel.localScale.z);
        }
    }

    #region AI Giai đoạn 1 (Các hàm con)

    void DecideAction()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        Debug.Log($"[DragonAI] DecideAction: Player distance = {distance}. Melee range is {meleeRange}.");

        // QUY TẮC 1: Tấn công Gần (Melee Attack)
        if (distance <= meleeRange)
        {
            Debug.LogWarning("[DragonAI] Quyết định: Player ở trong tầm Melee. Bắt đầu MeleeAttackRoutine()");
            StartCoroutine(MeleeAttackRoutine());
        }

        // QUY TẮC 2: Tấn công Tầm trung (50/50 Jump In / Ranged Attack)
        else if (distance <= rangedRange)
        {
            if (Random.value < 0.5f)
            {
                Debug.Log("[DragonAI] Quyết định: Tấn công tầm xa (RangedAttackRoutine)");
                StartCoroutine(RangedAttackRoutine());
            }
            else
            {
                Debug.Log("[DragonAI] Quyết định: Nhảy tới (JumpRoutine - False)");
                StartCoroutine(JumpRoutine(false)); // Nhảy tới Player
            }
        }

        // QUY TẮC 3: Quá xa (Di chuyển bộ)
        else // distance > rangedRange (15m)
        {
            Debug.Log("[DragonAI] Quyết định: Player quá xa. Di chuyển (MoveTowardsPlayerIfNeeded)");
            MoveTowardsPlayerIfNeeded(true);
        }
    }

    // >> HÀM DI CHUYỂN DÙNG RIGIDBODY 2D <<
    void MoveTowardsPlayerIfNeeded(bool forceMove)
    {
        if (playerTransform == null || rb == null) return;

        float targetX = playerTransform.position.x;
        float distanceToTarget = Mathf.Abs(transform.position.x - targetX);
        float direction = Mathf.Sign(targetX - transform.position.x);

        if (distanceToTarget > stopDistance || forceMove)
        {
            if (!isMoving)
            {
                isMoving = true;
                animator.SetTrigger("MoveTrigger");
            }
            // Di chuyển bằng Rigidbody (chỉ tác động trục X, Y để gravity xử lý)
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        Debug.Log("[DragonAI] MeleeAttackRoutine: Bắt đầu chạy Coroutine.");
        if (isMoving) isMoving = false;
        isAttacking = true;
        animator.SetTrigger("AttackTrigger");

        // SỬA: Chỉ dừng vận tốc X, GIỮ vận tốc Y để Rồng rơi xuống (nếu lơ lửng)
        if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);

        // 1. Chờ đợi Animation (Ví dụ: 0.3 giây trước khi móng vuốt đến vị trí tấn công)
        yield return new WaitForSeconds(0.3f);

        // 2. KÍCH HOẠT HITBOX (PHẦN GÂY SÁT THƯƠNG THỰC TẾ)
        if (meleeHitbox != null)
        {
            Debug.LogWarning("[DragonAI] MeleeAttackRoutine: >>> KÍCH HOẠT CLAW HITBOX <<<");
            meleeHitbox.ResetAndActivate(); // Bật Hitbox
        }
        else
        {
            Debug.LogError("[DragonAI] MeleeAttackRoutine: LỖI!! meleeHitbox bị NULL. Không thể kích hoạt.");
        }

        // 3. Giữ Hitbox trong thời gian ngắn (Ví dụ: 0.2 giây)
        yield return new WaitForSeconds(0.2f);

        // 4. TẮT HITBOX
        if (meleeHitbox != null)
        {
            Debug.Log("[DragonAI] MeleeAttackRoutine: Tắt Claw Hitbox.");
            meleeHitbox.Deactivate(); // Tắt Hitbox
        }

        // 5. Chờ hết thời gian còn lại của animation
        yield return new WaitForSeconds(1.0f);

        Debug.Log("[DragonAI] MeleeAttackRoutine: Kết thúc tấn công. Reset Cooldown.");
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private IEnumerator RangedAttackRoutine()
    {
        if (isMoving) isMoving = false;
        isAttacking = true;
        animator.SetTrigger("SpecialATrigger");

        // SỬA: Chỉ dừng vận tốc X, GIỮ vận tốc Y để Rồng rơi xuống (nếu lơ lửng)
        if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);

        // LOGIC TẠO FIREBALL/LỬA CỦA BẠN SẼ ĐƯỢC THỰC HIỆN TẠI ĐÂY
        // Ví dụ:
        /*
        if (fireballPrefab != null && firePoint != null)
        {
            Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        }
        */

        // Chờ Animation/Hiệu ứng phun lửa hoàn tất
        yield return new WaitForSeconds(2.5f);

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    // >> HÀM JUMP ROUTINE DÙNG ADD FORCE <<
    private IEnumerator JumpRoutine(bool jumpToEscape) // true = nhảy lùi, false = nhảy tới
    {
        if (isMoving) isMoving = false;
        isJumping = true;
        animator.SetTrigger("JumpTrigger");

        if (rb == null) yield break;

        float targetX = playerTransform.position.x;
        float direction = Mathf.Sign(targetX - transform.position.x);
        float jumpDirection = jumpToEscape ? -direction : direction; // Hướng lực ngang

        rb.velocity = Vector2.zero;

        // Áp dụng lực nhảy
        rb.AddForce(new Vector2(jumpDirection * jumpForceX, jumpForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(jumpToEscape ? 1.0f : 1.5f);

        nextJumpTime = Time.time + jumpCooldown;
        isJumping = false;
    }

    #endregion
}