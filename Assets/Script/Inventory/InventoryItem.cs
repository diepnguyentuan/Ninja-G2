using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int quantity;

    public InventoryItem(ItemData data, int quantity)
    {
        this.data = data;
        this.quantity = quantity;
    }
}
