using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("UI Reference")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;


    private bool isDialogueActive = false;
    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // Update is called once per frame
    public void ShowDialogue(string text, List<string> choices, System.Action<int> onChoiceSelected)
    {
        if (dialoguePanel == null || dialogueText == null || choiceContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError("DialogueManager: UI references are not set properly.");
            return;
        }
        dialoguePanel.SetActive(true);
        isDialogueActive = true;
        dialogueText.text = text;
        for (int i = 0; i < choices.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.text = choices[i];
            }
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                onChoiceSelected?.Invoke(index);
                HideDialogue();
            });
        }
    }
    public void HideDialogue()
    {
        if (!isDialogueActive) return;
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}
