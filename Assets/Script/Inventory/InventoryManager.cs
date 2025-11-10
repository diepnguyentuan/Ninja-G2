using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<InventoryItem> items = new List<InventoryItem>();
    public int maxSlots = 20;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ nguyên toàn bộ GameObject này khi đổi Scene
        }
        else
        {
            Destroy(gameObject); // Hủy bỏ các bản sao khác
        }
    }
    public void AddItem(ItemData data, int quantity = 1)
    {
        var existing = items.Find(i => i.data == data);
        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            if (items.Count < maxSlots)
                items.Add(new InventoryItem(data, quantity));
            else
                Debug.Log("Túi đồ đã đầy!");
        }

        Debug.Log($"[Inventory] Nhặt: {data.itemName} x{quantity}");
    }
    public void RemoveItem(ItemData data, int quantity = 1)
    {
        var existing = items.Find(i => i.data == data);
        if (existing != null)
        {
            existing.quantity -= quantity;
            if (existing.quantity <= 0)
                items.Remove(existing);
        }
    }
}
