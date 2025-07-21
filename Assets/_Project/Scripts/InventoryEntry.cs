[System.Serializable]
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
