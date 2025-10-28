using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class MonsterPatrol : MonoBehaviour, IDamageable
{
    #region Public Variables (Inspector Settings)

    [Header("Patrol Settings")]
    public float moveSpeed = 2f;
    public float patrolDistance = 4f;
    public float idleTime = 2f;

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float maxChaseDistance = 12f;
    public Transform playerTransform;
    public GameObject attackHitbox;
    public float searchDuration = 3f;

    [Header("Health Settings")]
    public int maxHealth = 3;

    [Header("Health Bar")]
    public GameObject healthBarCanvasPrefab;
    public Transform healthBarAttachPoint;

    [Header("Physics")]
    public float knockbackPower = 2.5f;
    public float knockbackDuration = 0.2f;
    public float stunDurationAfterKnockback = 0.3f;

    [Header("Loot Drop")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;
    [Range(0f, 1f)]
    public float coinDropChance = 0.75f;
    public GameObject healthPotionPrefab;
    [Range(0f, 1f)]
    public float potionDropChance = 0.1f;
    public float lootDropForce = 2.5f;

    #endregion

    #region Private Variables

    private int currentHealth;
    private bool isDead = false;
    private HealthBar healthBarScript;

    private Vector3 startPosition;
    private Vector3 initialScale;
    private bool isFacingRight = true; // Sẽ được cập nhật chính xác trong Start()
    private Coroutine currentAICoroutine;

    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isReturning = false;
    private bool isSearching = false;
    private bool isTakingDamage = false;

    private Animator animator;
    private Collider2D mainCollider;

    private readonly int hashIsWalking = Animator.StringToHash("isWalking");
    private readonly int hashIsChasing = Animator.StringToHash("isChasing");
    private readonly int hashIsAttacking = Animator.StringToHash("isAttacking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashTakeDamage = Animator.StringToHash("TakeDamage");
    private readonly int hashDeath = Animator.StringToHash("Death");

    #endregion

    #region Initialization

    void Start()
    {
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
        initialScale = transform.localScale;
        // Xác định chính xác hướng ban đầu dựa trên scale X
        isFacingRight = (initialScale.x < 0);

        currentHealth = maxHealth;

        InitializeHealthBar();
        FindPlayer();

        SwitchState(AIState.Patrolling);
    }

    void InitializeHealthBar()
    {
        if (healthBarCanvasPrefab != null && healthBarAttachPoint != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarCanvasPrefab, healthBarAttachPoint);
            RectTransform healthBarRect = healthBarInstance.GetComponent<RectTransform>();
            if (healthBarRect != null)
            {
                healthBarRect.localPosition = Vector3.zero;
                healthBarRect.localRotation = Quaternion.identity;
                healthBarRect.localScale = Vector3.one;
            }
            healthBarScript = healthBarInstance.GetComponentInChildren<HealthBar>();
            UpdateHealthBarVisuals();
        }
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning($"Quái vật {gameObject.name} không tìm thấy Player!", this);
            }
        }
    }

    #endregion

    #region Update Loop (Decision Making)

    void Update()
    {
        if (isDead || isTakingDamage) return;
        if (playerTransform == null) return;

        float distanceToPlayerX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float distanceToPlayerY = Mathf.Abs(transform.position.y - playerTransform.position.y);
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);

        if (isChasing)
        {
            // *** SỬA LỖI 1: Luôn cập nhật hướng quay mặt KHI ĐANG CHASE ***
            FacePlayer(); // Gọi ở đây đảm bảo nó quay đúng hướng trước khi di chuyển/tấn công
            // *** ---------------------------------------------------- ***

            if (ShouldStopChasing(distanceToPlayerX, distanceFromStart, distanceToPlayerY))
            {
                SwitchState(AIState.Searching);
            }
            else
            {
                HandleChasingAndAttacking(distanceToPlayerX, distanceToPlayerY);
            }
        }
        else // Not chasing
        {
            if (ShouldStartChasing(distanceToPlayerX, distanceToPlayerY))
            {
                SwitchState(AIState.Chasing);
            }
        }
    }

    bool ShouldStopChasing(float distPX, float distStart, float distPY)
    {
        return distPX > detectionRange || distStart > maxChaseDistance || distPY > 3f;
    }

    bool ShouldStartChasing(float distPX, float distPY)
    {
        return distPX <= detectionRange && distPY < 3f && !isSearching;
    }

    #endregion

    #region State Switching Logic

    private enum AIState { Patrolling, Chasing, Searching, Returning }

    void SwitchState(AIState newState)
    {
        StopCurrentAICoroutine();

        isChasing = (newState == AIState.Chasing);
        isSearching = (newState == AIState.Searching);
        isReturning = (newState == AIState.Returning);

        if (!isChasing) isAttacking = false;

        animator.SetBool(hashIsChasing, isChasing);
        animator.SetBool(hashIsWalking, newState == AIState.Returning || newState == AIState.Patrolling);
        animator.SetBool(hashIsAttacking, isAttacking);

        switch (newState)
        {
            case AIState.Patrolling:
                currentAICoroutine = StartCoroutine(PatrolRoutine());
                break;
            case AIState.Chasing:
                // *** SỬA LỖI 1: Quay mặt ngay khi bắt đầu Chase ***
                FacePlayer(); // Đảm bảo quay đúng hướng ngay khi chuyển sang Chase
                              // *** ------------------------------------------ ***
                              // Logic chính nằm trong Update
                break;
            case AIState.Searching:
                currentAICoroutine = StartCoroutine(SearchRoutine());
                break;
            case AIState.Returning:
                currentAICoroutine = StartCoroutine(ReturnRoutine());
                break;
        }
    }

    void StopCurrentAICoroutine()
    {
        if (currentAICoroutine != null)
        {
            StopCoroutine(currentAICoroutine);
            currentAICoroutine = null;
        }
        isSearching = false;
    }

    #endregion

    #region AI Behaviours (Coroutines & Handlers)

    // PatrolRoutine giữ nguyên logic Flip cũ
    private IEnumerator PatrolRoutine()
    {
        Vector3 targetPosition;
        while (!isDead)
        {
            animator.SetBool(hashIsWalking, false);
            if (ShouldInterruptAI()) yield break;
            yield return new WaitForSeconds(idleTime);
            if (ShouldInterruptAI()) yield break;

            targetPosition = startPosition - new Vector3(patrolDistance, 0, 0);
            animator.SetBool(hashIsWalking, true);
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                if (ShouldInterruptAI()) yield break;
                // *** QUAN TRỌNG: Đảm bảo quay đúng hướng KHI tuần tra trái ***
                EnsureFacingDirection(false); // Luôn quay trái khi đi về targetPosition (bên trái)
                // *** ---------------------------------------------------- ***
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            if (ShouldInterruptAI()) yield break;
            transform.position = targetPosition;

            animator.SetBool(hashIsWalking, false);
            if (ShouldInterruptAI()) yield break;
            yield return new WaitForSeconds(idleTime);
            if (ShouldInterruptAI()) yield break;

            targetPosition = startPosition;
            animator.SetBool(hashIsWalking, true);
            while (Vector3.Distance(transform.position, startPosition) > 0.01f)
            {
                if (ShouldInterruptAI()) yield break;
                // *** QUAN TRỌNG: Đảm bảo quay đúng hướng KHI tuần tra phải ***
                EnsureFacingDirection(true); // Luôn quay phải khi đi về startPosition (bên phải)
                                             // *** ---------------------------------------------------- ***
                transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            if (ShouldInterruptAI()) yield break;
            transform.position = startPosition;

        }
    }


    private IEnumerator SearchRoutine()
    {
        animator.SetBool(hashIsWalking, false);
        animator.SetBool(hashIsChasing, false);

        float searchTimer = 0f;
        while (searchTimer < searchDuration)
        {
            if (ShouldInterruptAI()) yield break;
            searchTimer += Time.deltaTime;
            yield return null;
        }

        if (!isDead && !isTakingDamage && !isChasing)
        {
            SwitchState(AIState.Returning);
        }
    }

    private IEnumerator ReturnRoutine()
    {
        isReturning = true;
        animator.SetBool(hashIsWalking, true);

        // *** SỬA LỖI 2: Quay mặt đúng hướng NGAY KHI BẮT ĐẦU quay về ***
        EnsureFacingDirection(startPosition.x > transform.position.x); // Quay về hướng của startPosition
        // *** ---------------------------------------------------- ***

        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            if (ShouldInterruptAI()) yield break;
            // *** QUAN TRỌNG: Đảm bảo quay đúng hướng TRONG KHI quay về ***
            EnsureFacingDirection(startPosition.x > transform.position.x);
            // *** ---------------------------------------------------- ***
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Đã về đến nơi và không bị ngắt
        if (!isDead && !isTakingDamage && !isChasing)
        {
            transform.position = startPosition;
            animator.SetBool(hashIsWalking, false);
            // Reset chính xác về trạng thái ban đầu
            transform.localScale = initialScale;
            isFacingRight = (initialScale.x < 0);
            isReturning = false;
            SwitchState(AIState.Patrolling);
        }
    }

    // Xử lý logic khi đang Chase
    void HandleChasingAndAttacking(float distanceX, float distanceY)
    {
        // FacePlayer() đã được gọi trong Update()

        if (isAttacking) return; // Đang animation attack thì dừng

        // Ưu tiên tấn công
        if (distanceX <= attackRange && distanceY < 1.0f)
        {
            StartAttack();
        }
        else // Nếu không thì di chuyển
        {
            MoveTowardsPlayer();
        }
    }

    // Hàm gọi để đảm bảo quái vật quay mặt về Player
    void FacePlayer()
    {
        if (playerTransform == null || isAttacking || isTakingDamage || isDead) return;
        EnsureFacingDirection(playerTransform.position.x > transform.position.x);
    }

    // *** HÀM MỚI: Đảm bảo quái vật quay đúng hướng chỉ định ***
    // faceRight = true: quay phải
    // faceRight = false: quay trái
    void EnsureFacingDirection(bool shouldFaceRight)
    {
        if (shouldFaceRight && !isFacingRight) // Cần quay phải, đang quay trái -> Lật
        {
            Flip();
        }
        else if (!shouldFaceRight && isFacingRight) // Cần quay trái, đang quay phải -> Lật
        {
            Flip();
        }
        // Nếu đã đúng hướng thì không làm gì
    }
    // *** ------------------------------------------------ ***

    bool ShouldInterruptAI()
    {
        return isDead || isTakingDamage || isChasing;
    }

    #endregion

    #region Combat Logic (Giữ Nguyên)

    void StartAttack()
    {
        isAttacking = true;
        animator.SetBool(hashIsAttacking, true);
        animator.SetTrigger(hashAttack);
    }

    void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;
        animator.SetBool(hashIsAttacking, false);
        float chaseSpeed = moveSpeed * 1.5f;
        transform.position = Vector2.MoveTowards(
            transform.position,
            new Vector2(playerTransform.position.x, transform.position.y),
            chaseSpeed * Time.deltaTime
        );
    }

    public void EnableAttackHitbox() { if (attackHitbox != null) attackHitbox.SetActive(true); }
    public void DisableAttackHitbox() { if (attackHitbox != null) attackHitbox.SetActive(false); }
    public void AttackComplete() { isAttacking = false; animator.SetBool(hashIsAttacking, false); }

    public void TakeDamage(int amount, Vector2 playerPosition)
    {
        if (isDead || isTakingDamage) return;

        currentHealth -= amount;
        UpdateHealthBarVisuals();

        if (currentHealth <= 0)
        {
            SwitchStateToDeath();
        }
        else
        {
            StopCurrentAICoroutine();
            StartCoroutine(KnockbackRoutine(playerPosition));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 playerPosition)
    {
        isTakingDamage = true;
        animator.SetTrigger(hashTakeDamage);

        Vector2 knockbackDirection = ((Vector2)transform.position - playerPosition).normalized;
        float timer = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)knockbackDirection * knockbackPower;
        while (timer < knockbackDuration)
        {
            if (isDead) yield break;
            transform.position = Vector3.Lerp(startPos, endPos, timer / knockbackDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(stunDurationAfterKnockback);

        if (isDead) yield break;

        isTakingDamage = false;
        DecideNextStateAfterDamage();
    }

    void DecideNextStateAfterDamage()
    {
        if (playerTransform == null) { SwitchState(AIState.Returning); return; }

        float distanceToPlayerX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float distanceToPlayerY = Mathf.Abs(transform.position.y - playerTransform.position.y);
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);

        if (distanceToPlayerX <= detectionRange && distanceToPlayerY < 3f && distanceFromStart <= maxChaseDistance)
        {
            SwitchState(AIState.Chasing);
        }
        else
        {
            SwitchState(AIState.Returning);
        }
    }


    void SwitchStateToDeath()
    {
        if (isDead) return;
        isDead = true;
        isTakingDamage = false; isChasing = false; isReturning = false; isSearching = false; isAttacking = false;

        animator.SetTrigger(hashDeath);
        StopAllCoroutines();
        currentAICoroutine = null;

        if (mainCollider != null) mainCollider.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        if (healthBarAttachPoint != null && healthBarAttachPoint.childCount > 0)
            Destroy(healthBarAttachPoint.GetChild(0).gameObject);

        DropLoot();
        Destroy(gameObject, 2f);
    }

    void DropLoot()
    {
        if (coinPrefab != null && Random.value <= coinDropChance)
        {
            for (int i = 0; i < coinDropAmount; i++)
            {
                Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
                GameObject coin = Instantiate(coinPrefab, dropPosition, Quaternion.identity);
                Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
                if (coinRb != null)
                {
                    Vector2 dropForce = new Vector2(Random.Range(-1f, 1f), Random.Range(1f, 2f)).normalized * lootDropForce;
                    coinRb.AddForce(dropForce, ForceMode2D.Impulse);
                }
            }
        }

        if (healthPotionPrefab != null && Random.value <= potionDropChance)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            GameObject potion = Instantiate(healthPotionPrefab, dropPosition, Quaternion.identity);
            Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
            if (potionRb != null)
            {
                Vector2 dropForce = new Vector2(Random.Range(-1f, 1f), Random.Range(1f, 2f)).normalized * lootDropForce;
                potionRb.AddForce(dropForce, ForceMode2D.Impulse);
            }
        }
    }

    #endregion

    #region Utility Functions

    // Hàm Flip chỉ lật hình và cập nhật isFacingRight
    void Flip()
    {
        isFacingRight = !isFacingRight; // Cập nhật trạng thái bool
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale; // Lật hình ảnh thực tế

        // Lật ngược thanh máu để nó không bị lật theo quái vật
        if (healthBarAttachPoint != null)
        {
            Vector3 healthBarScale = healthBarAttachPoint.localScale;
            healthBarScale.x *= -1; // Chỉ lật trục X
            healthBarAttachPoint.localScale = healthBarScale;
        }
    }

    void UpdateHealthBarVisuals()
    {
        if (healthBarScript != null)
        {
            healthBarScript.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    #endregion

    #region Gizmos (Debugging)

    void OnDrawGizmosSelected()
    {
        Vector3 currentStartPosition = Application.isPlaying && startPosition != Vector3.zero ? startPosition : transform.position;

        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Application.isPlaying && !isDead)
        { Gizmos.color = Color.blue; Gizmos.DrawWireSphere(currentStartPosition, maxChaseDistance); }

        Gizmos.color = Color.cyan;
        Vector3 patrolEnd = currentStartPosition - new Vector3(patrolDistance, 0, 0);
        Gizmos.DrawLine(currentStartPosition + Vector3.up * 0.1f, patrolEnd + Vector3.up * 0.1f);
        Gizmos.DrawWireSphere(currentStartPosition, 0.2f); Gizmos.DrawWireSphere(patrolEnd, 0.2f);
    }

    #endregion
}