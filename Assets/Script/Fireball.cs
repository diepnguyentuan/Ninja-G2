using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 12f;       // vận tốc bay
    public float lifeTime = 2f;     // tự hủy dự phòng (giây)
    public float maxDistance = 6f;  // phạm vi tối đa (đơn vị world)

    public int damage = 10;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private int dir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Gọi ngay sau khi Instantiate
    public void Launch(int direction)
    {
        dir = (Mathf.Sign(direction) >= 0) ? 1 : -1;
        startPos = transform.position;

        if (rb) rb.velocity = new Vector2(dir * speed, 0f);

        // Flip root để cả Visual + Collider cùng quay đúng chiều
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;

        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Tự hủy khi vượt quá phạm vi
        if (maxDistance > 0f && Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    // Tuỳ chọn: hủy khi chạm ground/tường (nếu bạn gắn tag)
    // --- THAY THẾ TOÀN BỘ HÀM OnTriggerEnter2D CŨ ---
    void OnCollisionEnter2D(Collision2D collision) // <--- ĐÃ ĐỔI TÊN HÀM
    {
        // Lấy Collider va chạm
        Collider2D other = collision.collider;

        // 1. ƯU TIÊN SÁT THƯƠNG RỒNG
        if (other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            IDamageable damageableObject = other.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                // Gửi sát thương (dùng transform.position của Fireball)
                damageableObject.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject); // Hủy quả cầu lửa
            return;
        }

        // 2. Kiểm tra Ground/Tường
        if (other.CompareTag("ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
