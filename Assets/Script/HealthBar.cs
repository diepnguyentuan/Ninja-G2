using UnityEngine;
using UnityEngine.UI; // Cần thư viện UI

public class HealthBar : MonoBehaviour
{
    // Gán Image "Fill" vào đây
    public Image fillImage;
    private Camera mainCamera;

    void Start()
    {
        // Tìm camera chính
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // --- BILLBOARDING ---
        // Đoạn code này làm cho thanh máu luôn quay mặt về phía camera
        if (mainCamera == null) return;
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }

    // Hàm public để cập nhật thanh máu
    public void UpdateHealthBar(float current, float max)
    {
        if (fillImage == null) return;

        if (max <= 0)
        {
            fillImage.fillAmount = 0;
        }
        else
        {
            // Tính toán tỉ lệ máu
            fillImage.fillAmount = current / max;
        }
    }
}