using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "Villager";
    [TextArea(3, 5)] public string greetLine = "Xin chào! Bạn cần giúp gì không?";
    [Header("References")]
    public DialogueManager dialogueManager;
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    private bool isPlayerInRange = false;
    public ShopManager shopManager;

    public QuestUIManager questUIManager;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (!dialogueManager.IsDialogueActive())
                StartDialogue();
        }
    }
    private void StartDialogue()
    {
        List<string> options = new List<string>
        {
            "Mua vật phẩm",
            "Nói chuyện",
            "Tạm biệt"
        };

        dialogueManager.ShowDialogue($"{npcName}: {greetLine}", options, OnChoiceSelected);
    }
    private void OnChoiceSelected(int index)
    {
        switch (index)
        {
            case 0:
                if (shopManager != null) shopManager.OpenShop();
                else Debug.LogWarning("Chưa gán ShopManager cho NPC!");
                break;
            case 1:
                if (questUIManager != null) questUIManager.ShowQuestPrompt("Bạn có muốn nhận nhiệm vụ tiêu diệt đàn sói không?");
                else Debug.LogWarning("Chưa gán QuestUIManager cho NPC!");
                break;
            case 2:
                Debug.Log("Người chơi chọn: Tạm biệt");
                break;
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
            dialogueManager.HideDialogue();
        }
    }
}
