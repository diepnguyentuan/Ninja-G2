using UnityEngine;

public class FireShooter : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;          // điểm bắn (Empty ở tay)
    public GameObject fireballPrefab;    // prefab Fireball (root)

    [Header("Shoot Settings")]
    public float projectileSpeed = 12f;  // sẽ gán sang Fireball.speed
    public float projectileRange = 6f;   // phạm vi tối đa (world units)
    public float fireCooldown = 0.4f;    // chống spam F

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        if (!firePoint || !fireballPrefab) return;

        // Hướng theo mặt nhân vật (flip bằng localScale.x)
        int dir = transform.localScale.x >= 0 ? 1 : -1;

        var go = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        // Truyền thông số & phóng
        var fb = go.GetComponent<Fireball>();
        if (fb)
        {
            fb.speed = projectileSpeed;
            fb.maxDistance = projectileRange;
            fb.Launch(dir);
        }
        else
        {
            // Fallback nếu quên gắn Fireball.cs
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb) rb.velocity = new Vector2(dir * projectileSpeed, 0f);
            Destroy(go, projectileRange / Mathf.Max(0.01f, projectileSpeed)); // ước lượng thời gian bay
        }
    }

    // Vẽ tầm bắn để canh nhanh trong Editor (tuỳ chọn)
    void OnDrawGizmosSelected()
    {
        if (!firePoint) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + Vector3.right * projectileRange);
        Gizmos.DrawLine(firePoint.position, firePoint.position + Vector3.left * projectileRange);
    }
}
