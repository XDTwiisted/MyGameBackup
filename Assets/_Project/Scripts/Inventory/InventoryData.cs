using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryEntry
{
    public InventoryItemData itemData;
    public int quantity;

    public InventoryEntry(InventoryItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}

[Serializable]
public class InventoryItemSave
{
    public string itemID;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<InventoryItemSave> items = new List<InventoryItemSave>();
}
