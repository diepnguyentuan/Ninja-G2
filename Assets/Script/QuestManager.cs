using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    // Singleton Access Point (Điểm truy cập duy nhất)
    [Header("Quest UI")]
    public GameObject questPanel;
    public TMP_Text questTitle;
    public TMP_Text questDescription;
    public TMP_Text questProgressText;

    [Header("Quest Data")]
    public string questTitleText = "Tiêu diệt đàn sói";
    public string questDescriptionText = "Giết 5 con sói đang quấy phá làng.";
    public int targetCount = 5;
    public int currentCount = 0;
    public bool isQuestActive = false;
    public bool questCompleted = false;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartQuest()
    {
        isQuestActive = true;
        questCompleted = false;
        currentCount = 0;

        questPanel.SetActive(true);
        questTitle.text = questTitleText;
        questDescription.text = questDescriptionText;
        UpdateProgressUI();
        Debug.Log("[Quest] Bắt đầu nhiệm vụ tiêu diệt đàn sói!");
    }
    public void RegisterKill()
    {
        if (!isQuestActive || questCompleted) return;
        currentCount++;
        UpdateProgressUI();
        if (currentCount >= targetCount)
        {
            questCompleted = true;
            questProgressText.text = $"Hoàn thành nhiệm vụ! ({currentCount}/{targetCount})";
            Debug.Log("[Quest] Nhiệm vụ hoàn thành");
        }
    }

    private void UpdateProgressUI()
    {
        questProgressText.text = $"Tiến trình: {currentCount}/{targetCount}";
    }

    //public bool IsQuestActive()
    //{
    //    return questActive && !questCompleted;
    //}
}