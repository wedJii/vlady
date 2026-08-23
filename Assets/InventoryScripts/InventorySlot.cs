using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public Item item;
    public int amount;

    public bool IsEmpty => item == null || amount <= 0;

    public InventorySlot() { }

    public InventorySlot(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public void Add(int value) => amount += value;

    public void Set(Item newItem, int newAmount)
    {
        item = newItem;
        amount = newAmount;
        if (amount <= 0) Clear();
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }
}