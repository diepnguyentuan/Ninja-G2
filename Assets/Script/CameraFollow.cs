using UnityEngine;
using System.Collections; // Cần thiết cho Coroutines

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Sẽ tự động tìm nếu rỗng

    [Header("Settings")]
    public float smoothTime = 0.3f;

    // *** THAY ĐỔI LỚN 1: Xóa 'offset' tính toán ***
    // Thay bằng một offset CỐ ĐỊNH. Bạn có thể chỉnh (0, 0, -10) trong Inspector
    // để camera lùi ra xa hoặc lại gần.
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    public Vector3 Offset => offset;

    // --- BIẾN CHO HIỆU ỨNG RUNG (Giữ nguyên) ---
    [Header("Camera Shake")]
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;
    private Coroutine currentShakeCoroutine;
    private Vector3 originalPosition;

    // *** THAY ĐỔI LỚN 2: Xóa 'lowY' và 'playerMoveScript' ***
    // Logic 'lowY' cũ bị lỗi khi đổi scene. Chúng ta sẽ bỏ nó đi
    // để ưu tiên sửa lỗi "không thấy Player".
    // private PlayerMove playerMoveScript;
    // private float lowY;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // *** THAY ĐỔI LỚN 3: TỰ ĐỘNG TÌM PLAYER ***
        // Nếu 'target' (Player) chưa được gán trong Inspector
        if (target == null)
        {
            // Tự tìm Player bằng Tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                // Nếu không tìm thấy, báo lỗi và dừng lại
                Debug.LogError("CameraFollow: KHÔNG THỂ TÌM THẤY đối tượng có Tag 'Player'!");
                return; // Dừng hàm Start
            }
        }

        // *** THAY ĐỔI LỚN 4: Xóa logic Start() cũ ***
        // Xóa hết các dòng tính 'playerMoveScript', 'offset' và 'lowY' cũ
        // playerMoveScript = target.GetComponent<PlayerMove>(); ...
        // offset = transform.position - target.position; // <-- NGUYÊN NHÂN GÂY LỖI
        // lowY = transform.position.y;

        // *** THAY ĐỔI LỚN 5: "Snap" camera ***
        // Di chuyển camera NGAY LẬP TỨC đến vị trí Player ở frame đầu tiên
        // Điều này đảm bảo 'originalPosition' cho việc rung (shake) được đặt đúng
        transform.position = target.position + offset;
        originalPosition = transform.position;
    }

    void LateUpdate()
    {
        // Chỉ di chuyển camera theo Player nếu KHÔNG đang rung
        if (currentShakeCoroutine == null)
        {
            if (target == null) return; // Kiểm tra lại (đề phòng Player chết)

            // Vị trí mục tiêu mới dựa trên offset cố định
            Vector3 targetCamPosition = target.position + offset;

            // *** THAY ĐỔI LỚN 6: Xóa logic 'lowY' cũ ***
            // if (playerMoveScript != null) ... (toàn bộ khối if đó đã bị xóa)

            // Di chuyển camera mượt mà
            transform.position = Vector3.SmoothDamp(transform.position, targetCamPosition, ref velocity, smoothTime);

            // Liên tục cập nhật vị trí gốc khi camera di chuyển bình thường
            originalPosition = transform.position;
        }
    }

    // --- HÀM RUNG (Giữ nguyên, không thay đổi) ---
    public void TriggerShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            transform.localPosition = originalPosition;
        }
        currentShakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;

            transform.localPosition = new Vector3(originalPosition.x + xOffset, originalPosition.y + yOffset, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        currentShakeCoroutine = null;
    }
}