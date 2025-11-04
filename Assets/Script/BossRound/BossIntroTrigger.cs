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

    // --- Biến riêng tư (Private) để script tự quản lý ---
    private Animator dragonAnim;
    private CameraFollow cameraFollowScript;
    private Transform cameraTransform;
    private Transform playerTransform;
    private PlayerMove playerMoveScript;

    private float cameraTargetZ = -10f; // Giá trị Z an toàn, mặc định
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

        // --- LOGIC TÌM CAMERA ĐÃ ĐƯỢC DI CHUYỂN KHỎI START() ---

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
            // --- BẮT ĐẦU SỬA LỖI ---
            // 1. TÌM CAMERA NGAY KHI PLAYER VA CHẠM
            // (Vì Player mang theo Camera "bất tử")
            if (Camera.main != null)
            {
                cameraFollowScript = Camera.main.GetComponent<CameraFollow>();
                cameraTransform = Camera.main.transform;
            }
            else
            {
                // Nếu VẪN không tìm thấy, báo lỗi và dừng lại
                Debug.LogError("BossIntroTrigger: KHÔNG TÌM THẤY MAIN CAMERA! (Camera \"bất tử\" có bị thiếu Tag 'MainCamera' không?)");
                return; // Dừng cutscene
            }

            // 2. Lấy giá trị Z an toàn từ script CameraFollow
            if (cameraFollowScript != null)
            {
                cameraTargetZ = cameraFollowScript.Offset.z;
            }
            // --- KẾT THÚC SỬA LỖI ---


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
    /// Đây là Coroutine "Đạo diễn" toàn bộ màn Cutscene Giai đoạn 0
    /// </summary>
    private IEnumerator StartBossIntroSequence()
    {
        // --- PHẦN 1: CAMERA DI CHUYỂN TỚI RỒNG ---

        // 1. Khóa Player và Script CameraFollow
        playerMoveScript.enabled = false;
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = false;

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

        // --- PHẦN 2: RỒNG NÓI & GIỮ CAMERA ---

        // 4. Kích hoạt animation "Talk" và hiện hộp thoại
        if (dragonAnim != null)
            dragonAnim.Play("TalkDragon");

        if (dialogueBoxUI != null)
        {
            dialogueText.text = bossLine;
            dialogueBoxUI.SetActive(true);
        }

        // 5. Giữ camera ở chỗ Rồng
        yield return new WaitForSeconds(cameraHoldTime);

        // --- PHẦN 3: CHUYỂN TIẾP VỀ PLAYER ---

        // 6. Ẩn Hộp thoại và Hiện Thanh máu Boss
        if (dialogueBoxUI != null)
            dialogueBoxUI.SetActive(false);

        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(true);

        // 7. Camera bắt đầu quay về Player
        Vector3 playerCamPos = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y,
            cameraTargetZ
        );

        while (Vector3.Distance(cameraTransform.position, playerCamPos) > 0.1f)
        {
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

        // 8. Rồng vào thế chiến đấu
        if (dragonAnim != null)
            dragonAnim.SetTrigger("StartFight");

        // 9. Kích hoạt "bộ não" AI của Rồng
        DragonAI bossAI = dragonBoss.GetComponent<DragonAI>();
        if (bossAI != null)
        {
            bossAI.StartCombat();
        }

        // 10. Trả lại điều khiển cho Camera và Player
        if (cameraFollowScript != null)
            cameraFollowScript.enabled = true;

        playerMoveScript.enabled = true;

        // 11. Đánh dấu cutscene đã xong
        state = IntroState.Finished;
    }
}