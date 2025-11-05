using UnityEngine;
using System;

public class QuestManager : MonoBehaviour
{
    // Singleton Access Point (Điểm truy cập duy nhất)
    public static QuestManager Instance;

    [Header("Quest Data")]
    public const int TARGET_COUNT = 3; // Mục tiêu: Tiêu diệt 3 con sói
    public int currentKills = 0;
    public bool isQuestActive = false;

    // --- EVENTS (Các tín hiệu để giao tiếp với UI) ---
    // Event này thông báo cho UI biết số lượng đã thay đổi
    public static event Action<int, int> OnQuestProgressed;
    // Event này thông báo cho UI/GameManager biết nhiệm vụ đã hoàn thành
    public static event Action OnQuestCompleted;

    void Awake()
    {
        // Thiết lập Singleton và DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            // GIỮ QUEST MANAGER SỐNG SÓT QUA CÁC SCENE
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Hủy bản sao (duplicate)
            Destroy(gameObject);
        }
    }

    // --- CÁC HÀM XỬ LÝ LOGIC CHÍNH ---

    // 1. GỌI KHI NGƯỜI CHƠI CHẤP NHẬN NHIỆM VỤ (từ QuestUIManager)
    public void StartWolfQuest()
    {
        if (isQuestActive) return;

        isQuestActive = true;
        currentKills = 0;

        Debug.Log("Nhiệm vụ SÓI đã được kích hoạt.");

        // Thông báo cho UI hiển thị tiến trình lần đầu
        OnQuestProgressed?.Invoke(currentKills, TARGET_COUNT);
    }

    // 2. GỌI TỪ SCRIPT WOLF HEALTH (khi một con sói bị tiêu diệt)
    public void RegisterKill()
    {
        if (!isQuestActive) return;

        currentKills++;

        Debug.Log($"Đã tiêu diệt: {currentKills}/{TARGET_COUNT}");

        // Thông báo cho UI cập nhật số lượng
        OnQuestProgressed?.Invoke(currentKills, TARGET_COUNT);

        if (currentKills >= TARGET_COUNT)
        {
            CompleteQuest();
        }
    }

    private void CompleteQuest()
    {
        isQuestActive = false;
        Debug.Log("NHIỆM VỤ THÀNH CÔNG! Sói đã bị tiêu diệt đủ.");

        // Thông báo cho UI hiển thị thông báo hoàn thành
        OnQuestCompleted?.Invoke();
    }
}