using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public InventoryItemData itemData;
    public int quantity;
    public int currentDurability;

    //  Constructor for durable items
    public ItemInstance(InventoryItemData data, int qty, int durability)
    {
        itemData = data;
        quantity = qty;
        currentDurability = durability;
    }

    // Optional: default constructor for serialization
    public ItemInstance() { }
}
