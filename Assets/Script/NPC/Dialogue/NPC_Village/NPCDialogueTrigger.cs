using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public string npcName = "Villager";
    [TextArea(3, 5)] public string dialogueText = "Xin chào! Bạn cần giúp gì không?";
    public DialogueManager dialogueManager;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (dialogueManager == null) return;

            if (dialogueManager.IsShowing())
                dialogueManager.HideDialogue();
            else
                dialogueManager.ShowDialogue($"{dialogueText}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            isPlayerInRange = false;
            if (dialogueManager != null && dialogueManager.IsShowing())
                dialogueManager.HideDialogue();
        }
    }
}
