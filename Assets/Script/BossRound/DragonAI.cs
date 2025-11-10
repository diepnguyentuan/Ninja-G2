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
    // --- SỬA LỖI: Biến này sẽ được gán tự động ---
    private Transform playerTransform; // Không cần kéo vào Inspector nữa

    public Transform dragonModel;       // Kéo GameObject "DragonRed" (con) vào đây
    public Animator animator;           // Kéo "DragonRed" (con) vào đây

    [Header("Cài đặt Chiến đấu (GĐ 1)")]
    public float moveSpeed = 2.5f;
    public float groundLevelY = -8f; // (Giữ nguyên giá trị -8 bạn đã đặt)
    public float meleeRange = 4f;
    public float rangedRange = 15f;
    public float stopDistance = 3.5f;

    [Header("Thời gian Cooldown")]
    public float attackCooldown = 3.0f;
    public float jumpCooldown = 10.0f;

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
        this.enabled = false; // Tắt AI, chờ BossIntroTrigger

        // --- SỬA LỖI: Không tìm Player trong Start() ---

        // Xác định hướng quay mặt ban đầu của sprite
        if (dragonModel != null)
        {
            isFacingRight_Sprite = (dragonModel.localScale.x < 0);
        }
    }

    // Hàm này được gọi bởi BossIntroTrigger để bắt đầu
    public void StartCombat()
    {
        this.enabled = true; // Bật AI lên

        // --- SỬA LỖI: TÌM PLAYER TẠI ĐÂY ---
        // Dùng Singleton để tìm Player "Xịn" (bất tử)
        if (PlayerStats.instance != null)
        {
            playerTransform = PlayerStats.instance.transform;
        }
        else
        {
            // Nếu không tìm thấy Player (ví dụ: test scene Boss), script sẽ dừng
            Debug.LogError("DragonAI: KHÔNG TÌM THẤY PlayerStats.instance!");
            currentState = AIState.Idle_OnPerch; // Quay về trạng thái chờ
            return;
        }
        // ------------------------------------

        // Bắt đầu kịch bản nhảy xuống
        if (currentState == AIState.Idle_OnPerch)
        {
            StartCoroutine(IntroJumpRoutine());
        }
    }

    // --- COROUTINE NHẢY XUỐNG ĐẤT ---
    private IEnumerator IntroJumpRoutine()
    {
        currentState = AIState.Intro_Jumping;
        isJumping = true;
        animator.SetTrigger("JumpTrigger");

        float elapsedTime = 0f;
        Vector3 startPos = initialPerchPosition;

        // --- SỬA LỖI: Kiểm tra Player một lần nữa cho chắc ---
        if (playerTransform == null)
        {
            Debug.LogError("DragonAI: playerTransform bị null ngay trước khi nhảy!");
            currentState = AIState.Idle_OnPerch; // Quay về chờ
            yield break; // Dừng coroutine
        }
        // ----------------------------------------------------

        // Vị trí đáp: (Dòng 86-90 cũ của bạn)
        Vector3 endPos = new Vector3(playerTransform.position.x + (stopDistance * 1.5f), groundLevelY, 0);

        while (elapsedTime < introJumpDuration)
        {
            float height = Mathf.Sin(Mathf.PI * elapsedTime / introJumpDuration) * introJumpHeight;
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / introJumpDuration) + Vector3.up * height;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        isJumping = false;

        // --- GIAI ĐOẠN 1 BẮT ĐẦU ---
        currentState = AIState.Phase1_Fighting;
        nextAttackTime = Time.time + 1.0f;
        nextJumpTime = Time.time + jumpCooldown;
    }


    // --- VÒNG LẶP AI CHÍNH (GIAI ĐOẠN 1) ---
    void Update()
    {
        // (Kiểm tra null ở đây nữa)
        if (currentState != AIState.Phase1_Fighting || playerTransform == null || isAttacking || isJumping)
        {
            if (isMoving) isMoving = false;
            return;
        }

        FacePlayer();

        if (Time.time >= nextJumpTime)
        {
            StartCoroutine(JumpRoutine());
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            DecideAction();
        }
        else
        {
            MoveTowardsPlayerIfNeeded();
        }
    }

    // --- SỬA LỖI QUAY MẶT (ĐI LÙI) ---
    void FacePlayer()
    {
        // (Kiểm tra null)
        if (playerTransform == null) return;

        bool playerIsOnRight = (playerTransform.position.x > transform.position.x);

        if (playerIsOnRight && !isFacingRight_Sprite)
        {
            isFacingRight_Sprite = true;
            dragonModel.localScale = new Vector3(-Mathf.Abs(dragonModel.localScale.x), dragonModel.localScale.y, dragonModel.localScale.z);
        }
        else if (!playerIsOnRight && isFacingRight_Sprite)
        {
            isFacingRight_Sprite = false;
            dragonModel.localScale = new Vector3(Mathf.Abs(dragonModel.localScale.x), dragonModel.localScale.y, dragonModel.localScale.z);
        }
    }

    #region AI Giai đoạn 1 (Các hàm con)

    void DecideAction()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= meleeRange)
        {
            StartCoroutine(MeleeAttackRoutine());
        }
        else if (distance <= rangedRange)
        {
            StartCoroutine(RangedAttackRoutine());
        }
        else
        {
            MoveTowardsPlayerIfNeeded();
        }
    }

    void MoveTowardsPlayerIfNeeded()
    {
        if (playerTransform == null) return;
        Vector2 targetPosition = new Vector2(playerTransform.position.x, groundLevelY);
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (distanceToTarget > stopDistance)
        {
            if (!isMoving)
            {
                isMoving = true;
                animator.SetTrigger("MoveTrigger");
            }
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
            }
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        if (isMoving) isMoving = false;
        isAttacking = true;
        animator.SetTrigger("AttackTrigger");

        // 1. Chờ đợi Animation (Ví dụ: 0.3 giây trước khi móng vuốt đến vị trí tấn công)
        yield return new WaitForSeconds(0.3f);

        // 2. KÍCH HOẠT HITBOX (PHẦN GÂY SÁT THƯƠNG THỰC TẾ)
        if (meleeHitbox != null)
        {
            meleeHitbox.ResetAndActivate(); // Bật Hitbox
        }

        // 3. Giữ Hitbox trong thời gian ngắn (Ví dụ: 0.2 giây)
        yield return new WaitForSeconds(0.2f);

        // 4. TẮT HITBOX
        if (meleeHitbox != null)
        {
            meleeHitbox.Deactivate(); // Tắt Hitbox
        }

        // 5. Chờ hết thời gian còn lại của animation (tổng cộng 1.5s - 0.3s - 0.2s = 1.0s)
        yield return new WaitForSeconds(1.0f);

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private IEnumerator RangedAttackRoutine()
    {
        if (isMoving) isMoving = false;
        isAttacking = true;
        animator.SetTrigger("SpecialATrigger");

        // (Thêm logic tạo tia lửa ở đây)

        yield return new WaitForSeconds(2.5f);

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private IEnumerator JumpRoutine()
    {
        if (isMoving) isMoving = false;
        isJumping = true;
        animator.SetTrigger("JumpTrigger");

        float jumpDistance = 5f;
        float jumpDuration = 1.0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - (isFacingRight_Sprite ? Vector3.left : Vector3.right) * jumpDistance;
        endPos.y = groundLevelY;
        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            float height = Mathf.Sin(Mathf.PI * elapsedTime / jumpDuration) * 2f;
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / jumpDuration) + Vector3.up * height;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;

        nextJumpTime = Time.time + jumpCooldown;
        isJumping = false;
    }

    #endregion
}