public class LootItem
{
    public InventoryItemData itemData;
    public int minQuantity;
    public int maxQuantity;
    public float chance;

    public LootItem(InventoryItemData data, int minQty, int maxQty, float chance)
    {
        itemData = data;
        minQuantity = minQty;
        maxQuantity = maxQty;
        this.chance = chance;
    }
}
