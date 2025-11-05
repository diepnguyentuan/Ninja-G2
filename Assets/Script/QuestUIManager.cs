using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestUIManager : MonoBehaviour
{
    [Header("Quest Prompt")]
    public GameObject QuestPromptPanel; // Panel chứa QusetText, Accept, Decline
    public TMP_Text QusetText;          // Text nội dung lời đề nghị nhiệm vụ
    public Button AcceptButton;
    public Button DeclineButton;

    [Header("Quest Panel")]
    public GameObject QuestPanel;       // Panel hiển thị nhiệm vụ đang theo dõi
    public TMP_Text QuestTitle;
    public TMP_Text QuestDescription;
    public TMP_Text QuestProgressText; // Tiến trình (ví dụ: 0/3)

    void Awake()
    {
        // Đảm bảo các panel bị ẩn khi khởi tạo
        if (QuestPromptPanel != null) QuestPromptPanel.SetActive(false);
        if (QuestPanel != null) QuestPanel.SetActive(false);

        // Gắn sự kiện vào các nút (Accept/Decline)
        AcceptButton.onClick.AddListener(OnAcceptClicked);
        DeclineButton.onClick.AddListener(OnDeclineClicked);
    }

    void OnEnable()
    {
        // Đăng ký lắng nghe từ Singleton QuestManager (Quan trọng!)
        QuestManager.OnQuestProgressed += UpdateProgressUI;
        QuestManager.OnQuestCompleted += ShowCompletionMessage;
    }

    void OnDisable()
    {
        // Hủy đăng ký khi tắt
        QuestManager.OnQuestProgressed -= UpdateProgressUI;
        QuestManager.OnQuestCompleted -= ShowCompletionMessage;
    }

    // --- LOGIC HIỂN THỊ HỘP THOẠI GIAO NHIỆM VỤ ---

    // Hàm này được gọi từ NPCDialogueTrigger (sau khi Player chọn "Nói chuyện")
    public void ShowQuestPrompt(string promptText)
    {
        if (QuestPromptPanel != null)
        {
            QusetText.text = promptText;
            QuestPromptPanel.SetActive(true);
        }
    }

    public void HideQuestPrompt()
    {
        if (QuestPromptPanel != null)
        {
            QuestPromptPanel.SetActive(false);
        }
    }

    // --- LOGIC NÚT BẤM ---

    private void OnAcceptClicked()
    {
        // Yêu cầu QuestManager bắt đầu nhiệm vụ
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartWolfQuest();
        }

        // Sau khi chấp nhận, ẩn prompt và hiện panel tiến trình
        HideQuestPrompt();
        ShowQuestPanel(true); // Hiển thị panel tiến trình

        // (Tùy chọn: Thêm hiệu ứng âm thanh chấp nhận)
    }

    private void OnDeclineClicked()
    {
        HideQuestPrompt();
        Debug.Log("Người chơi từ chối nhiệm vụ.");
    }

    // --- LOGIC CẬP NHẬT TIẾN TRÌNH (LẮNG NGHE EVENT) ---

    // Hàm này được QuestManager gọi mỗi khi có sói bị giết
    private void UpdateProgressUI(int current, int target)
    {
        // Đảm bảo panel hiển thị khi tiến trình cập nhật lần đầu (sau khi Accept)
        ShowQuestPanel(true);

        if (QuestProgressText != null)
        {
            QuestProgressText.text = $"Tiến trình: {current}/{target}";
        }
    }

    private void ShowCompletionMessage()
    {
        // Hiển thị thông báo lớn (ví dụ: dùng một panel khác hoặc chính QuestPanel)
        if (QuestProgressText != null)
        {
            // Thay đổi text để báo thành công
            QuestProgressText.text = "NHIỆM VỤ THÀNH CÔNG! (Quay về làng)";
            QuestProgressText.color = Color.yellow; // Đổi màu chữ

            // Sau 5 giây, tắt thông báo
            Invoke(nameof(HideProgressUI), 5f);
        }
    }

    public void ShowQuestPanel(bool show)
    {
        if (QuestPanel != null)
        {
            QuestPanel.SetActive(show);
        }
        if (!show)
        {
            // Reset màu chữ sau khi hoàn thành
            QuestProgressText.color = Color.white;
        }
    }

    private void HideProgressUI()
    {
        ShowQuestPanel(false);
    }
}