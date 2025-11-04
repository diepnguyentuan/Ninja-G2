using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    // Dùng Singleton pattern y hệt script PlayerStats của bạn
    public static PersistentUI instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Dòng quan trọng nhất
        }
        else
        {
            // Nếu đã có 1 cái UI tồn tại (quay lại Village), hủy cái mới này
            Destroy(gameObject);
        }
    }
}