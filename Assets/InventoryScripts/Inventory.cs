using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int capacity = 20;
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        for (int i = 0; i < capacity; i++)
        {
            slots.Add(new InventorySlot(null, 0));
        }
    }
    
    public bool AddItem(Item item, int amount)
    {
        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.amount < item.maxStack)
                {
                    int spaceInSlot = item.maxStack - slot.amount;
                    int amountToAdd = Mathf.Min(amount, spaceInSlot);
                    
                    slot.AddAmount(amountToAdd);
                    amount -= amountToAdd;

                    if (amount <= 0) return true;
                }
            }
        }
        
        while (amount > 0)
        {
            InventorySlot emptySlot = GetEmptySlot();
            if (emptySlot == null) return false;

            int amountToAdd = item.isStackable ? Mathf.Min(amount, item.maxStack) : 1;
            emptySlot.item = item;
            emptySlot.amount = amountToAdd;
            amount -= amountToAdd;
        }

        return true;
    }
    private InventorySlot GetEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.item == null) return slot;
        }
        return null;
    }
}