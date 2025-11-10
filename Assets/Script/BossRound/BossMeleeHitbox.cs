// TẠO FILE BossMeleeHitbox.cs và GẮN VÀO ClawHitbox
using UnityEngine;

public class BossMeleeHitbox : MonoBehaviour
{
    [Header("Cài đặt")]
    public int damageAmount = 30; // Sát thương đòn cào
    public LayerMask targetLayer; // Layer của Player (thường là "Player")

    // Biến cờ để đảm bảo chỉ gây sát thương 1 lần mỗi lần tấn công
    private bool hasHit = false;

    // Hàm public được gọi bởi DragonAI để kích hoạt/tắt Hitbox
    public void ResetAndActivate()
    {
        hasHit = false;
        gameObject.SetActive(true); // Bật GameObject (và Collider)
    }

    public void Deactivate()
    {
        gameObject.SetActive(false); // Tắt GameObject (và Collider)
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra Layer Mask VÀ chưa gây sát thương trong đòn này
        if (((1 << other.gameObject.layer) & targetLayer) != 0 && !hasHit)
        {
            // Kiểm tra Player có IDamageable không
            IDamageable damageableObject = other.GetComponent<IDamageable>();

            if (damageableObject != null)
            {
                // Gây sát thương và đặt cờ đã đánh trúng
                damageableObject.TakeDamage(damageAmount, transform.position);
                hasHit = true;
            }
        }
    }
}