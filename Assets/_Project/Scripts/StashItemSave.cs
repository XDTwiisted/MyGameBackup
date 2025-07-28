using System;

[System.Serializable]
public class StashItemSave
{
    public string itemID;
    public int count;

    public StashItemSave(string id, int count)
    {
        this.itemID = id;
        this.count = count;
    }

    // Optional parameterless constructor for JsonUtility
    public StashItemSave() { }
}
