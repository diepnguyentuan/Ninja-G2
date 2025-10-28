using UnityEngine;
using System.Collections; // Cần thiết cho Coroutines

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Gán Player vào đây

    [Header("Settings")]
    public float smoothTime = 0.3f; // Thời gian camera bắt kịp

    // --- BIẾN CHO HIỆU ỨNG RUNG ---
    [Header("Camera Shake")]
    public float shakeDuration = 0.1f; // Thời gian rung
    public float shakeMagnitude = 0.1f; // Cường độ rung
    private Coroutine currentShakeCoroutine; // Quản lý coroutine rung
    private Vector3 originalPosition; // Lưu vị trí gốc
    // --- ----------------------- ---

    private PlayerMove playerMoveScript; // Script của Player
    private Vector3 offset; // Khoảng cách ban đầu camera-player
    private float lowY; // Giới hạn dưới của camera
    private Vector3 velocity = Vector3.zero; // Biến cần cho SmoothDamp

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Chưa gán Target (nhân vật)!");
            return;
        }

        playerMoveScript = target.GetComponent<PlayerMove>();
        if (playerMoveScript == null)
        {
            Debug.LogError("CameraFollow: Không tìm thấy script 'PlayerMove' trên Target!");
            // Vẫn tiếp tục chạy nhưng chức năng giới hạn Y sẽ không hoạt động
        }

        offset = transform.position - target.position;
        lowY = transform.position.y;

        // Lưu vị trí ban đầu (quan trọng cho ShakeRoutine)
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        // Chỉ di chuyển camera theo Player nếu KHÔNG đang rung
        if (currentShakeCoroutine == null)
        {
            if (target == null) return; // Kiểm tra target lần nữa

            Vector3 targetCamPosition = target.position + offset;

            // Xử lý giới hạn dưới (lowY) chỉ khi có playerMoveScript
            if (playerMoveScript != null)
            {
                if (playerMoveScript.IsGrounded())
                {
                    // Cập nhật lowY khi player chạm đất ở vị trí thấp hơn
                    if (targetCamPosition.y < lowY) // Chỉ cập nhật nếu thực sự thấp hơn
                    {
                        lowY = targetCamPosition.y;
                    }
                }
                // Nếu đang rơi và vị trí mục tiêu thấp hơn lowY, giữ camera lại
                else if (targetCamPosition.y < lowY)
                {
                    targetCamPosition.y = lowY;
                }
            }

            // Di chuyển camera mượt mà
            transform.position = Vector3.SmoothDamp(transform.position, targetCamPosition, ref velocity, smoothTime);

            // Liên tục cập nhật vị trí gốc khi camera di chuyển bình thường
            originalPosition = transform.localPosition;
        }
    }

    // --- HÀM KÍCH HOẠT RUNG ---
    public void TriggerShake()
    {
        // Dừng coroutine cũ nếu đang chạy và trả về vị trí gốc
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            transform.localPosition = originalPosition; // Reset ngay lập tức
        }
        // Bắt đầu coroutine mới
        currentShakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    // --- COROUTINE XỬ LÝ RUNG ---
    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // Tính toán độ lệch ngẫu nhiên
            float xOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;

            // Áp dụng độ lệch vào vị trí gốc ĐÃ LƯU
            transform.localPosition = new Vector3(originalPosition.x + xOffset, originalPosition.y + yOffset, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null; // Chờ frame tiếp theo
        }

        // Đảm bảo trả về vị trí gốc sau khi rung xong
        transform.localPosition = originalPosition;
        currentShakeCoroutine = null; // Đánh dấu đã rung xong
    }
}