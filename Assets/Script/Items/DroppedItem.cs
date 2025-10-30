using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[DroppedItem] InventoryManager.Instance == null → bạn chưa có InventoryManager trong scene!");
                return;
            }

            if (itemData == null)
            {
                Debug.LogError("[DroppedItem] itemData == null → bạn chưa gán ItemData cho DroppedItem prefab!");
                return;
            }

            InventoryManager.Instance.AddItem(itemData, quantity);
            Destroy(gameObject);
        }
    }
}
