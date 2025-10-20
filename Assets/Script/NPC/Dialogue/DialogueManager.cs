using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("UI Reference")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private bool isDialogueActive = false;
    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // Update is called once per frame
    public void ShowDialogue(string text)
    {
        if (dialoguePanel == null || dialogueText == null) return;

        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
    }
    public void HideDialogue()
    {
        if (dialoguePanel == null) return;

        isDialogueActive = false;
        dialoguePanel.SetActive(false);
    }
    public bool IsShowing() => isDialogueActive;
    void Update()
    {

    }
}
