using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int capacity = 20;
    [SerializeField] private List<InventorySlot> slots = new();

    public int Capacity => capacity;
    public IReadOnlyList<InventorySlot> Slots => slots;
    public event Action OnInventoryChanged;

    private void Awake() => EnsureCapacity();

    public void EnsureCapacity()
    {
        if (slots == null) slots = new List<InventorySlot>();
        while (slots.Count < capacity) slots.Add(new InventorySlot());
        while (slots.Count > capacity) slots.RemoveAt(slots.Count - 1);
    }

    public void LoadFromSlots(IReadOnlyList<InventorySlot> sourceSlots)
    {
        EnsureCapacity();
        for (int i = 0; i < capacity; i++)
        {
            if (i < sourceSlots.Count && sourceSlots[i] != null && !sourceSlots[i].IsEmpty)
                slots[i].Set(sourceSlots[i].item, sourceSlots[i].amount);
            else
                slots[i].Clear();
        }
        OnInventoryChanged?.Invoke();
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
        if (fromIndex == toIndex || !IsValidIndex(fromIndex) || !IsValidIndex(toIndex)) return;

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

    public void TransferTo(int fromIndex, Inventory targetInventory, int toIndex)
    {
        if (targetInventory == null || targetInventory == this)
        {
            MoveOrMerge(fromIndex, toIndex);
            return;
        }

        if (!IsValidIndex(fromIndex) || !targetInventory.IsValidIndex(toIndex)) return;

        var from = slots[fromIndex];
        var to = targetInventory.slots[toIndex];
        if (from.IsEmpty) return;

        // 1. Если целевой слот пустой -> просто перемещаем
        if (to.IsEmpty)
        {
            to.Set(from.item, from.amount);
            from.Clear();
        }
        // 2. Если одинаковый стакаемый предмет -> слияние стаков
        else if (from.item == to.item && to.item.isStackable)
        {
            int space = to.item.maxStack - to.amount;
            int moveAmount = Mathf.Min(from.amount, space);
            to.Add(moveAmount);
            from.amount -= moveAmount;
            if (from.amount <= 0) from.Clear();
        }
        // 3. Разные предметы -> обмен между инвентарями
        else
        {
            var tempItem = from.item;
            var tempAmount = from.amount;
            from.Set(to.item, to.amount);
            to.Set(tempItem, tempAmount);
        }

        OnInventoryChanged?.Invoke();
        targetInventory.OnInventoryChanged?.Invoke();
    }

    public bool QuickTransfer(int fromIndex, Inventory targetInventory)
    {
        if (targetInventory == null || !IsValidIndex(fromIndex)) return false;

        var from = slots[fromIndex];
        if (from.IsEmpty) return false;

        var item = from.item;
        int amount = from.amount;

        if (targetInventory.AddItem(item, amount))
        {
            from.Clear();
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public void RemoveAt(int slotIndex, int amount = 1)
    {
        if (!IsValidIndex(slotIndex)) return;
        var slot = slots[slotIndex];
        if (slot.IsEmpty) return;

        slot.amount -= amount;
        if (slot.amount <= 0) slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    public void ClearAll()
    {
        foreach (var slot in slots) slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    private bool IsValidIndex(int index) => index >= 0 && index < slots.Count;
}