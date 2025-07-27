using UnityEngine;

public class LootTest : MonoBehaviour
{
    public LootTable lootTable;

    void Start()
    {
        var loot = lootTable.GetLoot();
        foreach (var item in loot)
        {
            Debug.Log($"You found {item.minQuantity}x {item.itemData.itemName}");
        }
    }
}
