using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    [Header("Quest Prompt")]
    public GameObject questPromptPanel;
    public TMP_Text questText;
    public Button acceptButton;
    public Button declineButton;

    [Header("Quest Panel")]
    public GameObject questPanel;
    public TMP_Text questTitle;
    public TMP_Text questDescription;

    private bool hasAcceptedQuest = false;
    // Start is called before the first frame update
    void Start()
    {
        questPromptPanel.SetActive(false);
        questPanel.SetActive(false);

        acceptButton.onClick.AddListener(OnAcceptQuest);
        declineButton.onClick.AddListener(OnDeclineQuest);

    }

    public void ShowQuestPrompt(string message)
    {
        questText.text = message;
        questPromptPanel.SetActive(true);
    }

    private void OnAcceptQuest()
    {
        hasAcceptedQuest = true;
        questPromptPanel.SetActive(false);
        ShowQuestPanel("Tiêu diệt đàn sói", "Đánh bại 10 con sói quanh làng.");
    }

    private void OnDeclineQuest()
    {
        questPromptPanel.SetActive(false);
    }

    private void ShowQuestPanel(string title, string description)
    {
        questTitle.text = "Nhiệm vụ: " + title;
        questDescription.text = description;
        questPanel.SetActive(true);
    }
}
