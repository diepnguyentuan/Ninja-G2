using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("Prompt UI")]
    public GameObject questPromptPanel;
    public TMP_Text questText;
    public Button acceptButton;
    public Button declineButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        questPromptPanel.SetActive(false);

        acceptButton.onClick.AddListener(OnAccept);
        declineButton.onClick.AddListener(OnDecline);
    }

    // 🟢 Hiển thị lời mời nhận nhiệm vụ
    public void ShowQuestPrompt(string message)
    {
        questText.text = message;
        questPromptPanel.SetActive(true);
    }

    private void OnAccept()
    {
        questPromptPanel.SetActive(false);
        QuestManager.Instance.StartQuest();   // 🟢 Bắt đầu nhiệm vụ thực tế
    }

    private void OnDecline()
    {
        questPromptPanel.SetActive(false);
        Debug.Log("Người chơi từ chối nhiệm vụ.");
    }
}
