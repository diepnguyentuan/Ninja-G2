using System;
using System.Collections;
using UnityEngine;

public class MonsterPatrol : MonoBehaviour, IDamageable
{
    [Header("Patrol Settings")]
    public float moveSpeed = 2f; // Tốc độ di chuyển của quái vật
    public float patrolDistance = 4f; // Khoảng cách đi tuần tra từ điểm bắt đầu
    public float idleTime = 5f; // Thời gian đứng yên

    [Header("AI Settings")]
    public float detectionRange = 10f; // Khoảng cách phát hiện người chơi
    public float attackRange = 1.5f; // Khoảng cách để tấn công
    public float maxChaseDistance = 10f; // Quái vật sẽ ngừng đuổi nếu đi xa hơn khoảng cách này từ điểm bắt đầu
    public Transform playerTransform; // Gán transform của người chơi vào đây
    public GameObject attackHitbox;
    public float searchDuration = 3f; // Thời gian "Tìm kiếm" trước khi từ bỏ

    [Header("Health Settings")]
    public int maxHealth = 3; // Quái sẽ chết sau 3 hit
    private int currentHealth;
    private bool isDead = false;

    [Header("Health Bar")]
    public GameObject healthBarCanvasPrefab; // Prefab thanh máu ta sẽ tạo
    public Transform healthBarAttachPoint; // Vị trí để gắn thanh máu (trên đầu quái)
    private HealthBar healthBarScript; // Script điều khiển thanh máu

    [Header("Physics")]
    public float knockbackPower = 0.5f; // Quái sẽ bị đẩy lùi bao xa
    public float knockbackDuration = 0.2f; // Thời gian văng

    [Header("Loot Drop")]
    public GameObject coinPrefab; // Gán Prefab Coin vào đây
    public int coinDropAmount = 1; // Số lượng xu rơi ra
    [Range(0f, 1f)] // Thanh trượt từ 0 đến 1
    public float coinDropChance = 0.75f; // Tỉ lệ rơi xu (75%)

    // Thêm các biến tương tự nếu có Health Potion
    public GameObject healthPotionPrefab;
    [Range(0f, 1f)]
    public float potionDropChance = 0.1f; // 10%

    // Private variables
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 initialScale; // Thêm biến để lưu scale ban đầu
    private bool isFacingRight = true;
    private Coroutine currentRoutine; // Dùng một biến Coroutine duy nhất để quản lý
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isReturning = false;
    private bool isSearching = false; // Trạng thái mới: đang tìm kiếm
    private bool isTakingDamage = false;

    // Components
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        initialScale = transform.localScale; // Lưu lại scale ban đầu

        // Xác định hướng ban đầu của sprite
        isFacingRight = (initialScale.x > 0);

        currentHealth = maxHealth;

        if (healthBarCanvasPrefab != null && healthBarAttachPoint != null)
        {
            // Tạo thanh máu từ Prefab
            GameObject healthBarInstance = Instantiate(healthBarCanvasPrefab, healthBarAttachPoint.position, Quaternion.identity);

            // Gắn thanh máu làm con của "Attach Point"
            healthBarInstance.transform.SetParent(healthBarAttachPoint, false);

            RectTransform healthBarRect = healthBarInstance.GetComponent<RectTransform>();
            // Ép nó về đúng vị trí (0,0,0) so với cha (AttachPoint)
            healthBarRect.localPosition = Vector3.zero;
            healthBarRect.localRotation = Quaternion.identity;
            healthBarRect.localScale = Vector3.one;

            // Lấy script điều khiển từ thanh máu
            healthBarScript = healthBarInstance.GetComponentInChildren<HealthBar>();

            if (healthBarScript != null)
            {
                healthBarScript.UpdateHealthBar(currentHealth, maxHealth); // Cập nhật lần đầu
            }
        }

        // Tự động tìm người chơi nếu chưa được gán
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        // Bắt đầu tuần tra
        currentRoutine = StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        if (isDead) return;
        // Nếu đang bị thương, KHÔNG làm bất cứ việc gì khác
        if (isTakingDamage) return;
        // Nếu không tìm thấy người chơi, không làm gì cả
        if (playerTransform == null) return;

        float distanceToPlayerX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float distanceToPlayerY = Mathf.Abs(transform.position.y - playerTransform.position.y);

        float distanceFromStart = Vector3.Distance(transform.position, startPosition);

        if (isChasing)
        {
            // Điều kiện để NGỪNG đuổi theo
            if (distanceToPlayerX > detectionRange || distanceFromStart > maxChaseDistance || distanceToPlayerY > 3f)
            {
                isChasing = false;
                animator.SetBool("isChasing", false);

                isAttacking = false;
                animator.SetBool("isAttacking", false);
                if (currentRoutine != null)
                {
                    StopCoroutine(currentRoutine);
                }
                currentRoutine = StartCoroutine(SearchRoutine());
                return;
            }
            else
            {
                // Tiếp tục đuổi theo
                HandleChasingAndAttacking(distanceToPlayerX, distanceToPlayerY);
            }
        }
        else // Không đang đuổi theo (đang tuần tra hoặc đang quay về)
        {
            // Điều kiện để BẮT ĐẦU đuổi theo
            if (distanceToPlayerX <= detectionRange && distanceToPlayerY < 3f && !isReturning) // Thêm 3f hoặc giá trị phù hợp
            {
                if (distanceToPlayerX <= detectionRange && distanceToPlayerY < 3f && !isReturning)
                {
                    // Player đã ở trong tầm

                    if (isSearching)
                    {
                        // Nếu đang tìm kiếm -> thấy player -> Hủy tìm kiếm
                        Debug.Log("Player re-acquired during search!");
                        isSearching = false;
                        if (currentRoutine != null) StopCoroutine(currentRoutine); // Dừng SearchRoutine
                    }
                    else
                    {
                        // Nếu đang tuần tra -> thấy player -> Dừng tuần tra
                        if (currentRoutine != null) StopCoroutine(currentRoutine); // Dừng PatrolRoutine
                    }

                    // Bắt đầu đuổi theo (hoặc tiếp tục đuổi)
                    isChasing = true;
                    animator.SetBool("isChasing", true);
                    animator.SetBool("isWalking", false);
                    currentRoutine = StartCoroutine(ChaseRoutine());
                }
            }
        }
    }

    // Coroutine riêng cho việc đuổi theo để quản lý trạng thái trong Update
    private IEnumerator ChaseRoutine()
    {
        while (isChasing)
        {
            yield return null; // Coroutine này chỉ chạy để Update có thể dừng nó
        }
    }

    void HandleChasingAndAttacking(float distanceX, float distanceY)
    {
        if (isAttacking) return;
        FacePlayer();
        // HÀNH ĐỘNG 1: Tấn công nếu đủ điều kiện
        if (distanceX <= attackRange && distanceY < 1.0f)
        {
            isAttacking = true;
            animator.SetBool("isAttacking", true);
            // DỪNG di chuyển bằng cách không gọi lệnh di chuyển
            // (Bạn có thể thêm lệnh `rb.velocity = Vector2.zero;` nếu dùng Rigidbody để di chuyển)
            animator.SetTrigger("Attack");
            Debug.Log("In Attack Range. Stopping and Attacking.");
        }
        // HÀNH ĐỘNG 2: Nếu không tấn công được, thì mới di chuyển để đuổi theo
        else if (isChasing) // Thêm điều kiện isChasing để chắc chắn
        {
            float chaseSpeed = moveSpeed * 1.5f;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(playerTransform.position.x, transform.position.y), chaseSpeed * Time.deltaTime);
        }
    }

    // Hàm này sẽ bật hitbox
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }
    }

    // Hàm này sẽ tắt hitbox
    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    void FacePlayer()
    {
        // Nếu người chơi ở bên phải và quái vật đang quay trái -> Lật
        if (playerTransform.position.x < transform.position.x && !isFacingRight)
        {
            Flip();
        }
        // Nếu người chơi ở bên trái và quái vật đang quay phải -> Lật
        else if (playerTransform.position.x > transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    private IEnumerator ReturnRoutine()
    {
        animator.SetBool("isWalking", true);

        // Quay mặt về phía điểm bắt đầu
        if (startPosition.x < transform.position.x && !isFacingRight) Flip();
        else if (startPosition.x > transform.position.x && isFacingRight) Flip();

        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            if (isTakingDamage) yield break;
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPosition;
        animator.SetBool("isWalking", false);

        // THAY ĐỔI LỚN: Reset lại hướng và scale về đúng trạng thái ban đầu một cách triệt để
        transform.localScale = initialScale;
        isFacingRight = (initialScale.x > 0); // Cập nhật lại trạng thái isFacingRight cho đúng

        isReturning = false; // BÁO HIỆU: ĐÃ VỀ ĐẾN NHÀ, SẴN SÀNG CHIẾN ĐẤU LẠI

        // Bắt đầu lại tuần tra
        currentRoutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        // Logic tuần tra như cũ
        while (true)
        {
            if (isTakingDamage) yield break;
            animator.SetBool("isWalking", false);
            yield return new WaitForSeconds(idleTime);

            if (isTakingDamage) yield break;

            targetPosition = startPosition - new Vector3(patrolDistance, 0, 0);

            animator.SetBool("isWalking", true);
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                if (isTakingDamage) yield break;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPosition;

            animator.SetBool("isWalking", false);
            yield return new WaitForSeconds(idleTime);

            if (isTakingDamage) yield break;

            Flip();

            animator.SetBool("isWalking", true);
            while (Vector3.Distance(transform.position, startPosition) > 0.01f)
            {
                if (isTakingDamage) yield break;
                transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = startPosition;

            Flip();
        }
    }

    private IEnumerator SearchRoutine()
    {
        Debug.Log("Player lost! Searching...");
        isSearching = true;

        // Đứng yên (Animator sẽ tự động chuyển từ Run -> Idle
        // vì isChasing = false và isWalking = false)

        yield return new WaitForSeconds(searchDuration);

        if (!isSearching || isTakingDamage) yield break;
        // HẾT GIỜ: Nếu chúng ta vẫn ở đây (chưa bị ngắt bởi Update)
        // có nghĩa là player không xuất hiện trở lại.
        Debug.Log("Search finished. Player not found. Returning home.");
        isSearching = false;
        isReturning = true; // Bây giờ mới bắt đầu quay về
        currentRoutine = StartCoroutine(ReturnRoutine());
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn để dễ hình dung tầm phát hiện và tấn công
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        // Vẽ đường giới hạn đuổi theo
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, maxChaseDistance);
        }
    }
    public void AttackComplete()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    // HÀM MỚI: Đây là hàm từ interface IDamageable
    public void TakeDamage(int amount, Vector2 hitPoint)
    {
        // Nếu đã chết hoặc đang bị thương, không nhận thêm sát thương
        if (isDead || isTakingDamage) return;

        // 1. Trừ máu
        currentHealth -= amount;
        Debug.Log("Monster health: " + currentHealth);

        // 2. Cập nhật thanh máu
        if (healthBarScript != null)
        {
            healthBarScript.UpdateHealthBar(currentHealth, maxHealth);
        }

        // 3. Kiểm tra xem đã chết chưa
        if (currentHealth <= 0)
        {
            Die(); // Gọi hàm chết
            return; // Dừng lại, không chạy "hit stun" nữa
        }
        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(hitPoint));
    }

    private IEnumerator KnockbackRoutine(Vector2 playerPosition)
    {
        // 1. "Đóng băng" AI
        isTakingDamage = true;

        // 2. Kích hoạt animation bị đánh (Chỉ để hiển thị)
        animator.SetTrigger("TakeDamage");

        // 3. Tính toán hướng văng
        Vector2 knockbackDirection = ((Vector2)transform.position - playerPosition).normalized;

        // 4. Thực hiện văng
        float timer = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)knockbackDirection * knockbackPower;

        while (timer < knockbackDuration) // knockbackDuration = 0.2f (ví dụ)
        {
            // Kiểm tra nếu chết giữa chừng (hiếm nhưng có thể)
            if (isDead) yield break;

            transform.position = Vector3.Lerp(startPos, endPos, timer / knockbackDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // --- 5. LOGIC MỚI: TỰ HỒI PHỤC ---
        // Đợi thêm một khoảng thời gian "choáng" sau khi văng xong.
        // Thời gian này nên bằng hoặc hơi dài hơn độ dài animation "TakeDamage".
        // Ví dụ: Nếu animation dài 0.5 giây, bạn có thể đợi 0.3 giây nữa (0.2 văng + 0.3 choáng).
        float stunDurationAfterKnockback = 0.3f;
        yield return new WaitForSeconds(stunDurationAfterKnockback);

        // Kiểm tra lại nếu đã chết trong lúc đợi
        if (isDead) yield break;

        // 6. Tự gỡ khóa và bật lại AI (Không cần DamageComplete nữa)
        Debug.Log("Knockback/Stun finished. Resuming AI.");
        isTakingDamage = false;

        // Quyết định trạng thái tiếp theo
        if (playerTransform == null)
        {
            currentRoutine = StartCoroutine(ReturnRoutine());
            yield break;
        }

        float distanceToPlayerX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float distanceToPlayerY = Mathf.Abs(transform.position.y - playerTransform.position.y);
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);

        if (distanceToPlayerX <= detectionRange && distanceToPlayerY < 3f && distanceFromStart <= maxChaseDistance)
        {
            isChasing = true;
            animator.SetBool("isChasing", true);
            currentRoutine = StartCoroutine(ChaseRoutine());
        }
        else
        {
            isReturning = true;
            currentRoutine = StartCoroutine(ReturnRoutine());
        }
    }

    private void Die()
    {
        Debug.Log("Monster has died.");

        // 1. Đánh dấu là đã chết
        isDead = true;
        isTakingDamage = false; // Tắt trạng thái bị thương

        // 2. Kích hoạt animation chết (bạn cần tạo trigger "Death" trong Animator)
        animator.SetTrigger("Death");

        // 3. Dừng mọi AI
        StopAllCoroutines();

        // 4. Tắt Collider để Player có thể đi xuyên qua
        GetComponent<Collider2D>().enabled = false;

        // Tùy chọn: Tắt Rigidbody (nếu có)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false; // Tắt vật lý
        }

        // 5. Hủy GameObject quái vật (và thanh máu) sau 2 giây
        // (Cho animation chết có thời gian chạy)
        Destroy(gameObject, 2f);
    }

}

