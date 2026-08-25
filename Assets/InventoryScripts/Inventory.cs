using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int capacity = 20;
    public List<InventorySlot> slots = new();

    public int Capacity => capacity;
    public IReadOnlyList<InventorySlot> Slots => slots;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        EnsureCapacity();
    }

    private void EnsureCapacity()
    {
        if (slots == null) slots = new List<InventorySlot>();
        while (slots.Count < capacity) slots.Add(new InventorySlot());
        while (slots.Count > capacity) slots.RemoveAt(slots.Count - 1);
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        EnsureCapacity();

        // 1. Складываем в существующие неполные стаки
        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.amount < item.maxStack)
                {
                    int space = item.maxStack - slot.amount;
                    int add = Mathf.Min(amount, space);
                    slot.Add(add);
                    amount -= add;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 2. Раскладываем остаток в пустые слоты
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                int add = item.isStackable ? Mathf.Min(amount, item.maxStack) : 1;
                slot.Set(item, add);
                amount -= add;

                if (amount <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return amount <= 0;
    }

    public void MoveOrMerge(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || fromIndex < 0 || fromIndex >= slots.Count || toIndex < 0 || toIndex >= slots.Count)
            return;

        var from = slots[fromIndex];
        var to = slots[toIndex];

        if (from.IsEmpty) return;

        // Если оба слота содержат одинаковый стакаемый предмет -> объединяем стаки
        if (!to.IsEmpty && from.item == to.item && to.item.isStackable)
        {
            int space = to.item.maxStack - to.amount;
            if (space > 0)
            {
                int moveAmount = Mathf.Min(from.amount, space);
                to.Add(moveAmount);
                from.amount -= moveAmount;
                if (from.amount <= 0) from.Clear();
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // Иначе меняем местами содержимое слотов
        var tempItem = from.item;
        var tempAmount = from.amount;
        from.Set(to.item, to.amount);
        to.Set(tempItem, tempAmount);

        OnInventoryChanged?.Invoke();
    }

    public void RemoveAt(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        var slot = slots[slotIndex];
        if (slot.IsEmpty) return;

        slot.amount -= amount;
        if (slot.amount <= 0) slot.Clear();
        OnInventoryChanged?.Invoke();
    }
}