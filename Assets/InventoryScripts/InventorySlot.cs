using System;

[Serializable]
public class InventorySlot
{
    public Item item;
    public int amount;
    
    public InventorySlot(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public void AddAmount(int value) 
    { 
        amount += value; 
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;
    }
}