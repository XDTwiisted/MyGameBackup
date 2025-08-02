using UnityEngine;

public static class RarityColors
{
    public static Color GetColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.4f, 0.4f, 0.4f);// Medium-light gray
            case ItemRarity.Uncommon: return new Color(0.0f, 0.6f, 0.0f);
            case ItemRarity.Rare: return new Color(0.2f, 0.4f, 1f);
            case ItemRarity.Epic: return new Color(0.5f, 0f, 1f);
            case ItemRarity.Legendary: return new Color(1f, 0.5f, 0f);
            default: return Color.gray;
        }
    }
}
