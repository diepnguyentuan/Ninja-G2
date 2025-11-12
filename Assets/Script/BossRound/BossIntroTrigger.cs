using System.Collections;
using UnityEngine;
using TMPro;

public class BossIntroTrigger : MonoBehaviour
{
    private enum IntroState
    {
        Waiting,
        Running,
        Finished
    }

    [Header("Tham chiếu Chính")]
    [Tooltip("Kéo GameObject GỐC 'Dragon_Root' vào đây")]
    public GameObject dragonBoss;
    [Tooltip("Kéo GameObject 'DragonRed' (chứa Animator) vào đây")]
    public Animator dragonAnimator;
    [Tooltip("Kéo Empty Object 'DragonLookAtPoint' (con của Rồng) vào đây")]
    public Transform dragonLookAtPoint;
    [Tooltip("Kéo Canvas/Image thanh máu CỦA BOSS vào đây")]
    public GameObject bossHealthBarUI;

    [Header("Hộp thoại (World Space)")]
    [Tooltip("Kéo GameObject 'DialogueBackground' (trên đầu Rồng) vào đây")]
    public GameObject dialogueBoxUI;
    [Tooltip("Kéo Text object 'DialogueText' (con của Background) vào đây")]
    public TextMeshProUGUI dialogueText;

    [Header("Cài đặt Cutscene")]
    public float cameraHoldTime = 5.0f;
    public float cameraMoveSpeed = 5.0f;
    [TextArea(2, 5)]
    public string bossLine = "Kẻ nào dám... quấy rầy giấc ngủ của ta?!";

    [Header("Player Intro Movement")]
    [Tooltip("Kéo Empty Object định nghĩa vị trí X mà Player nên đứng sau Intro.")]
    public Transform playerIntroTargetPosition;
    public float playerIntroMoveSpeed = 3.0f; // Tốc độ Player tự động di chuyển

    // --- Biến riêng tư (Private) để script tự quản lý ---
    private Animator dragonAnim;
    private CameraFollow cameraFollowScript;
    private Transform cameraTransform;
    private Transform playerTransform;
    private PlayerMove playerMoveScript;

    private Transform originalCameraParent;

    private float cameraTargetZ = -10f;
    private IntroState state = IntroState.Waiting;

    void Start()
    {
        // 1. Lấy (Cache) Animator của Rồng
        if (dragonAnimator != null)
        {
            dragonAnim = dragonAnimator;
        }
        else
        {
            Debug.LogError("BossIntroTrigger: CHƯA GÁN DRAGON ANIMATOR!");
        }

        // 2. Đảm bảo các UI được ẩn khi bắt đầu
        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(false);
        if (dialogueBoxUI != null)
            dialogueBoxUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ kích hoạt nếu là Player VÀ cutscene chưa chạy bao giờ
        if (state == IntroState.Waiting && other.CompareTag("Player"))
        {
            // 1. TÌM CAMERA NGAY KHI PLAYER VA CHẠM
            if (Camera.main != null)
            {
                cameraFollowScript = Camera.main.GetComponent<CameraFollow>();
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogError("BossIntroTrigger: KHÔNG TÌM THẤY MAIN CAMERA!");
                return;
            }

            // 2. Lấy giá trị Z an toàn từ script CameraFollow
            if (cameraFollowScript != null)
            {
                // Giả sử CameraFollow có một offset Z hoặc dùng giá trị mặc định
                // (Nếu bạn không có biến Offset Z trong CameraFollow, hãy giữ nguyên cameraTargetZ = -10f)
                // cameraTargetZ = cameraFollowScript.Offset.z; 
            }

            // 3. Đánh dấu là "đang chạy"
            state = IntroState.Running;

            // 4. Lấy các component của Player
            playerMoveScript = other.GetComponent<PlayerMove>();
            playerTransform = other.transform;

            if (playerMoveScript != null)
            {
                // Bắt đầu Coroutine "đạo diễn" cutscene
                StartCoroutine(StartBossIntroSequence());
            }

            // 5. Tắt trigger đi
            GetComponent<Collider2D>().enabled = false;
        }
    }

    /// <summary>
    /// Coroutine "Đạo diễn" toàn bộ màn Cutscene
    /// </summary>
    private IEnumerator StartBossIntroSequence()
    {
        // --- PHẦN 1: CAMERA DI CHUYỂN TỚI RỒNG ---

        // 1. Khóa Player và Script CameraFollow
        playerMoveScript.enabled = false;
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

        originalCameraParent = cameraTransform.parent;
        cameraTransform.SetParent(null);

        Rigidbody2D dragonRB = dragonBoss.GetComponent<Rigidbody2D>();


        // 2. Tính toán vị trí camera sẽ di chuyển đến
        Vector3 dragonCamPos = new Vector3(
            dragonLookAtPoint.position.x,
            dragonLookAtPoint.position.y,
            cameraTargetZ
        );

        // 3. Di chuyển camera
        while (Vector3.Distance(cameraTransform.position, dragonCamPos) > 0.1f)
        {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                dragonCamPos,
                Time.deltaTime * cameraMoveSpeed
            );
            yield return null;
        }
        cameraTransform.position = dragonCamPos;

        // --- PHẦN 2: RỒNG NÓI & PLAYER DI CHUYỂN ---

        // 4. Kích hoạt animation "Talk" và hiện hộp thoại
        if (dragonAnim != null)
            dragonAnim.Play("TalkDragon");

        if (dialogueBoxUI != null)
        {
            dialogueText.text = bossLine;
            dialogueBoxUI.SetActive(true);
        }

        // 5. Giữ camera ở chỗ Rồng VÀ di chuyển Player
        float timer = 0f;
        Vector3 playerTargetPos = Vector3.zero;
        bool playerArrived = false;

        // Thiết lập vị trí đích của Player (Chỉ thay đổi X, giữ nguyên Y/Z)
        if (playerIntroTargetPosition != null)
        {
            playerTargetPos = new Vector3(
                playerIntroTargetPosition.position.x,
                playerTransform.position.y, // Giữ Y của Player
                playerTransform.position.z
            );
        }

        // Vòng lặp chạy trong suốt thời gian Rồng nói (cameraHoldTime)
        while (timer < cameraHoldTime)
        {
            // 5A. Di chuyển Player tự động (Chỉ khi có Target Position)
            if (playerIntroTargetPosition != null && !playerArrived)
            {
                playerTransform.position = Vector3.MoveTowards(
                    playerTransform.position,
                    playerTargetPos,
                    playerIntroMoveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(playerTransform.position, playerTargetPos) < 0.1f)
                {
                    playerArrived = true;
                    playerTransform.position = playerTargetPos;
                }
            }

            // 5B. Đếm thời gian camera giữ
            timer += Time.deltaTime;
            yield return null;
        }

        // --- PHẦN 3: CHUYỂN TIẾP VỀ PLAYER ---

        // 6. Ẩn Hộp thoại và Hiện Thanh máu Boss
        if (dialogueBoxUI != null)
            dialogueBoxUI.SetActive(false);

        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(true);

        // 7. Rồng bắt đầu hạ cánh xuống đất (IntroJumpRoutine)
        DragonAI bossAI = dragonBoss.GetComponent<DragonAI>();
        if (bossAI != null)
        {
            bossAI.StartCombat(); // Hàm này sẽ kích hoạt IntroJumpRoutine
        }

        // *Đợi một khoảng thời gian để Rồng thực hiện Intro Jump trước khi Camera lia về Player*
        // Giả sử IntroJumpDuration = 1.5s, chúng ta chờ khoảng 1.0s trước khi camera quay về.
        yield return new WaitForSeconds(1.0f);

        // 8. Camera bắt đầu quay về Player
        Vector3 playerCamPos = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y,
            cameraTargetZ
        );

        while (Vector3.Distance(cameraTransform.position, playerCamPos) > 0.1f)
        {
            // Cập nhật vị trí Player vì Rồng đang Intro Jump, Player đã di chuyển xong
            playerCamPos.x = playerTransform.position.x;
            playerCamPos.y = playerTransform.position.y;

            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                playerCamPos,
                Time.deltaTime * cameraMoveSpeed
            );
            yield return null;
        }
        cameraTransform.position = playerCamPos;

        // --- PHẦN 4: TRẬN ĐẤU BẮT ĐẦU ---

        // 9. Kích hoạt lại Camera và Player
        if (cameraFollowScript != null)
        {
            cameraTransform.SetParent(originalCameraParent);

            cameraFollowScript.enabled = true;
        }

        playerMoveScript.enabled = true;

        // 10. Đánh dấu cutscene đã xong
        state = IntroState.Finished;
    }
}