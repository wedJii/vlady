using System;
using System.Collections.Generic;

[Serializable]
public class SlotSaveData
{
    public string itemID;
    public int amount;

    public SlotSaveData(string id, int amount)
    {
        this.itemID = id;
        this.amount = amount;
    }
}

[Serializable]
public class InventorySaveData
{
    public List<SlotSaveData> slots = new List<SlotSaveData>();
}
