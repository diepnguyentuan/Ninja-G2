using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRootManager : MonoBehaviour
{
    [Header("References")]
    public GameObject canvasInventory;
    public GameObject canvasQuest;
    public GameObject canvasShop;
    public GameObject canvasHUD;
    public GameObject questManager;
    public GameObject playerStats;

    private void Awake()
    {
        // Giữ lại toàn bộ GameRoot khi đổi scene
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Bật tắt theo nhu cầu test
        ToggleAll(false);  // Ẩn hết ban đầu
        ShowHUD(true);     // Giữ HUD hiện mặc định
    }

    // ==== Toggle từng phần ====
    public void ShowInventory(bool show)
    {
        if (canvasInventory != null) canvasInventory.SetActive(show);
    }

    public void ShowQuest(bool show)
    {
        if (canvasQuest != null) canvasQuest.SetActive(show);
    }

    public void ShowShop(bool show)
    {
        if (canvasShop != null) canvasShop.SetActive(show);
    }

    public void ShowHUD(bool show)
    {
        if (canvasHUD != null) canvasHUD.SetActive(show);
    }

    public void ToggleAll(bool show)
    {
        ShowInventory(show);
        ShowQuest(show);
        ShowShop(show);
        ShowHUD(show);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) ShowInventory(!canvasInventory.activeSelf);
        if (Input.GetKeyDown(KeyCode.F2)) ShowQuest(!canvasQuest.activeSelf);
        if (Input.GetKeyDown(KeyCode.F3)) ShowShop(!canvasShop.activeSelf);
        if (Input.GetKeyDown(KeyCode.F4)) ShowHUD(!canvasHUD.activeSelf);
    }
}
