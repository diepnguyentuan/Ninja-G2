using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManger : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("UI References")]
    public GameObject inventoryPanel;   // Panel chính
    public Transform itemGrid;          // Grid chứa các slot
    public GameObject slotPrefab;       // Prefab InventorySlot

    private bool isOpen = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }
    private void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
        if (isOpen) RefreshUI();
    }

    public void RefreshUI()
    {
        if (itemGrid == null)
        {
            Debug.LogError("[InventoryUI] itemGrid chưa được gán!");
            return;
        }
        if (slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] slotPrefab chưa được gán!");
            return;
        }
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[InventoryUI] InventoryManager.Instance == null!");
            return;
        }
        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        foreach (var invItem in InventoryManager.Instance.items)
        {
            var slot = Instantiate(slotPrefab, itemGrid);
            var icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            var qty = slot.transform.Find("QuantityText").GetComponent<TMP_Text>();

            icon.sprite = invItem.data.icon;
            qty.text = invItem.quantity > 1 ? invItem.quantity.ToString() : "";
        }
    }
}
