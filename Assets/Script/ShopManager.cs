using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


[System.Serializable]
public class ShopItem
{
    public string itemName;
    public int price;
    public Sprite icon;
}
public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public TMP_Text playerGoldText;
    public Transform itemListContainer;
    public GameObject shopItemPrefab;

    [Header("Player Info (demo)")]
    public int playerGold = 100;
    [Header("Item For Sale")]
    public List<ShopItem> itemsForSale = new List<ShopItem>();
    // Start is called before the first frame update
    void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    public void OpenShop()
    {
        if (shopPanel == null) return;
        shopPanel.SetActive(true);
        RefreshUI();
    }
    public void CloseShop()
    {
        if (shopPanel == null) return;
        shopPanel.SetActive(false);
    }
    private void RefreshUI()
    {
        if (playerGoldText != null)
            playerGoldText.text = $"Tiền của bạn: {playerGold}";
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var item in itemsForSale)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, itemListContainer);
            TMP_Text[] texts = itemObj.GetComponentsInChildren<TMP_Text>();

            Button buyBtn = itemObj.GetComponentInChildren<Button>();
            Image iconImage = null;
            Image[] images = itemObj.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name == "ItemIcon")
                {
                    iconImage = img;
                    break;
                }
            }
            TMP_Text nameText = null, priceText = null;

            foreach (var t in texts)
            {
                if (t.name == "ItemNameText")
                    nameText = t;
                else if (t.name == "ItemPriceText")
                    priceText = t;
            }
            if (nameText != null) nameText.text = item.itemName;
            if (priceText != null) priceText.text = $"{item.price} vàng";
            if (iconImage != null && item.icon != null) iconImage.sprite = item.icon;

            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => TryBuyItem(item));
        }
    }
    private void TryBuyItem(ShopItem item)
    {
        if (playerGold >= item.price)
        {
            playerGold -= item.price;
            Debug.Log($"[SHOP] Đã mua: {item.itemName} (-{item.price})");

            // TODO: thêm vào Inventory tại đây nếu bạn có hệ thống túi đồ
            RefreshUI();
        }
        else
        {
            Debug.Log("[SHOP] Không đủ tiền!");
        }
    }
    // Update is called once per frame
}
