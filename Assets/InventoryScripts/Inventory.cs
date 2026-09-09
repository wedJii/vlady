using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Вместимость")]
    public int capacity = 20;
    public List<InventorySlot> slots = new();

    [Header("Слоты пластин")]
    public int plateCapacity = 3;
    public List<InventorySlot> plateSlots = new();

    public int Capacity => capacity;
    public int PlateCapacity => plateCapacity;
    public IReadOnlyList<InventorySlot> Slots => slots;
    public IReadOnlyList<InventorySlot> PlateSlots => plateSlots;

    public event Action OnInventoryChanged;

    public void NotifyInventoryChanged() => OnInventoryChanged?.Invoke();

    private void Awake()
    {
        EnsureCapacity();
        EnsurePlateCapacity();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            EnsureCapacity();
            EnsurePlateCapacity();
            OnInventoryChanged?.Invoke();
        }
    }

    public void EnsureCapacity()
    {
        if (slots == null) slots = new List<InventorySlot>();
        while (slots.Count < capacity) slots.Add(new InventorySlot());
        while (slots.Count > capacity) slots.RemoveAt(slots.Count - 1);
    }

    public void EnsurePlateCapacity()
    {
        if (plateSlots == null) plateSlots = new List<InventorySlot>();
        while (plateSlots.Count < plateCapacity) plateSlots.Add(new InventorySlot());
        while (plateSlots.Count > plateCapacity) plateSlots.RemoveAt(plateSlots.Count - 1);
    }

    public List<InventorySlot> GetSlotList(bool isPlate) => isPlate ? plateSlots : slots;

    public bool IsValidIndex(int index, bool isPlate)
    {
        var list = GetSlotList(isPlate);
        return list != null && index >= 0 && index < list.Count;
    }

    public bool CanAcceptItem(Item item, bool isPlateSlot)
    {
        if (item == null) return true;
        if (isPlateSlot) return item is UbgradePlate;
        return true; // В обычный инвентарь можно класть любые предметы, включая пластины
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

    public void LoadPlatesFromSlots(IReadOnlyList<InventorySlot> sourcePlates, PlayerControll player = null)
    {
        EnsurePlateCapacity();

        // Снимаем старые эффекты, если они были надеты
        if (player != null)
        {
            foreach (var slot in plateSlots)
            {
                if (slot != null && !slot.IsEmpty && slot.item is UbgradePlate oldPlate)
                    oldPlate.OnRemove(player);
            }
        }

        for (int i = 0; i < plateCapacity; i++)
        {
            if (i < sourcePlates.Count && sourcePlates[i] != null && !sourcePlates[i].IsEmpty && sourcePlates[i].item is UbgradePlate)
            {
                plateSlots[i].Set(sourcePlates[i].item, 1);
                if (player != null && sourcePlates[i].item is UbgradePlate newPlate)
                    newPlate.OnApply(player);
            }
            else
            {
                plateSlots[i].Clear();
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public void ApplyAllEquippedPlates(PlayerControll player)
    {
        if (player == null) return;
        foreach (var slot in plateSlots)
        {
            if (slot != null && !slot.IsEmpty && slot.item is UbgradePlate plate)
            {
                plate.OnApply(player);
            }
        }
    }

    public void RemoveAllEquippedPlates(PlayerControll player)
    {
        if (player == null) return;
        foreach (var slot in plateSlots)
        {
            if (slot != null && !slot.IsEmpty && slot.item is UbgradePlate plate)
            {
                plate.OnRemove(player);
            }
        }
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

        // 2. Раскладываем остаток в пустые обычные слоты
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

    public int CountItem(Item item)
    {
        if (item == null) return 0;
        int count = 0;
        if (slots != null)
        {
            foreach (var s in slots)
            {
                if (s != null && s.item != null && IsSameItem(s.item, item))
                    count += s.amount;
            }
        }
        if (plateSlots != null)
        {
            foreach (var s in plateSlots)
            {
                if (s != null && s.item != null && IsSameItem(s.item, item))
                    count += s.amount;
            }
        }
        return count;
    }

    public static bool IsSameItem(Item a, Item b)
    {
        if (a == b) return true;
        if (a == null || b == null) return false;
        if (!string.IsNullOrEmpty(a.id) && !string.IsNullOrEmpty(b.id))
            return a.id == b.id;
        return a.name == b.name;
    }

    public bool ContainsItem(Item item, int amount = 1) => CountItem(item) >= amount;

    public void MoveOrMerge(int fromIndex, bool fromIsPlate, int toIndex, bool toIsPlate, PlayerControll player = null)
    {
        if (fromIndex == toIndex && fromIsPlate == toIsPlate) return;
        if (!IsValidIndex(fromIndex, fromIsPlate) || !IsValidIndex(toIndex, toIsPlate)) return;

        var fromList = GetSlotList(fromIsPlate);
        var toList = GetSlotList(toIsPlate);

        var from = fromList[fromIndex];
        var to = toList[toIndex];
        if (from.IsEmpty) return;

        // Проверка допустимости типов предметов для слотов пластин
        if (!CanAcceptItem(from.item, toIsPlate)) return;
        if (!to.IsEmpty && !CanAcceptItem(to.item, fromIsPlate)) return;

        if (player == null) player = FindFirstObjectByType<PlayerControll>();

        if (from.item is CoinItem || (from.item != null && from.item.id == "coin"))
        {
            if (player != null)
            {
                player.CurrentCoins += from.amount;
                from.Clear();
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // Если оба обычных слота содержат одинаковый стакаемый предмет -> объединяем
        if (!fromIsPlate && !toIsPlate && !to.IsEmpty && from.item == to.item && to.item.isStackable)
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

        // Снимаем/надеваем пластины при перемещении
        if (fromIsPlate && from.item is UbgradePlate fromPlate && player != null)
            fromPlate.OnRemove(player);

        if (toIsPlate && !to.IsEmpty && to.item is UbgradePlate toPlate && player != null)
            toPlate.OnRemove(player);

        var tempItem = from.item;
        var tempAmount = from.amount;
        from.Set(to.item, to.amount);
        to.Set(tempItem, tempAmount);

        if (toIsPlate && to.item is UbgradePlate newlyEquipped && player != null)
            newlyEquipped.OnApply(player);

        if (fromIsPlate && from.item is UbgradePlate newlyEquippedInFrom && player != null)
            newlyEquippedInFrom.OnApply(player);

        OnInventoryChanged?.Invoke();
    }

    public void TransferTo(int fromIndex, bool fromIsPlate, Inventory targetInventory, int toIndex, bool toIsPlate, PlayerControll player = null)
    {
        if (targetInventory == null || targetInventory == this)
        {
            MoveOrMerge(fromIndex, fromIsPlate, toIndex, toIsPlate, player);
            return;
        }

        if (!IsValidIndex(fromIndex, fromIsPlate) || !targetInventory.IsValidIndex(toIndex, toIsPlate)) return;

        var fromList = GetSlotList(fromIsPlate);
        var toList = targetInventory.GetSlotList(toIsPlate);

        var from = fromList[fromIndex];
        var to = toList[toIndex];
        if (from.IsEmpty) return;

        if (!targetInventory.CanAcceptItem(from.item, toIsPlate)) return;
        if (!to.IsEmpty && !CanAcceptItem(to.item, fromIsPlate)) return;

        if (player == null) player = FindFirstObjectByType<PlayerControll>();

        // Снимаем эффекты перед переносом
        if (fromIsPlate && from.item is UbgradePlate fromPlate && player != null)
            fromPlate.OnRemove(player);

        if (toIsPlate && !to.IsEmpty && to.item is UbgradePlate toPlate && player != null)
            toPlate.OnRemove(player);

        // 1. Если целевой слот пустой -> просто переносим
        if (to.IsEmpty)
        {
            to.Set(from.item, from.amount);
            from.Clear();
        }
        // 2. Одинаковые стакаемые в обычных инвентарях
        else if (!fromIsPlate && !toIsPlate && from.item == to.item && to.item.isStackable)
        {
            int space = to.item.maxStack - to.amount;
            int moveAmount = Mathf.Min(from.amount, space);
            to.Add(moveAmount);
            from.amount -= moveAmount;
            if (from.amount <= 0) from.Clear();
        }
        // 3. Обмен между слотами
        else
        {
            var tempItem = from.item;
            var tempAmount = from.amount;
            from.Set(to.item, to.amount);
            to.Set(tempItem, tempAmount);
        }

        // Применяем эффекты для вновь надетых пластин
        if (toIsPlate && to.item is UbgradePlate newEquipped && player != null)
            newEquipped.OnApply(player);

        if (fromIsPlate && from.item is UbgradePlate newFromEquipped && player != null)
            newFromEquipped.OnApply(player);

        OnInventoryChanged?.Invoke();
        targetInventory.OnInventoryChanged?.Invoke();
    }

    public bool QuickTransfer(int fromIndex, bool fromIsPlate, Inventory targetInventory, PlayerControll player = null)
    {
        if (targetInventory == null || !IsValidIndex(fromIndex, fromIsPlate)) return false;

        var fromList = GetSlotList(fromIsPlate);
        var from = fromList[fromIndex];
        if (from.IsEmpty) return false;

        if (player == null) player = FindFirstObjectByType<PlayerControll>();

        var item = from.item;
        int amount = from.amount;

        if (item is CoinItem || (item != null && item.id == "coin"))
        {
            if (player != null)
            {
                player.CurrentCoins += amount;
                from.Clear();
                OnInventoryChanged?.Invoke();
                targetInventory.OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Если переносим из слота пластины в другой инвентарь или наоборот
        if (fromIsPlate && item is UbgradePlate plate && player != null)
            plate.OnRemove(player);

        if (targetInventory.AddItem(item, amount))
        {
            from.Clear();
            OnInventoryChanged?.Invoke();
            targetInventory.OnInventoryChanged?.Invoke();
            return true;
        }

        // Если не удалось добавить в целевой инвентарь и это была пластина в слоте пластины — возвращаем эффект
        if (fromIsPlate && item is UbgradePlate plate2 && player != null)
            plate2.OnApply(player);

        return false;
    }

    public void RemoveAt(int slotIndex, bool isPlate = false, int amount = 1, PlayerControll player = null)
    {
        if (!IsValidIndex(slotIndex, isPlate)) return;
        var list = GetSlotList(isPlate);
        var slot = list[slotIndex];
        if (slot.IsEmpty) return;

        if (isPlate && slot.item is UbgradePlate plate)
        {
            if (player == null) player = FindFirstObjectByType<PlayerControll>();
            if (player != null) plate.OnRemove(player);
        }

        slot.amount -= amount;
        if (slot.amount <= 0) slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    public void ClearAll(PlayerControll player = null)
    {
        foreach (var slot in slots) slot.Clear();

        if (player == null) player = FindFirstObjectByType<PlayerControll>();
        if (player != null) RemoveAllEquippedPlates(player);

        foreach (var slot in plateSlots) slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    // Совместимость со старыми вызовами (по умолчанию для обычных слотов)
    public void MoveOrMerge(int fromIndex, int toIndex) => MoveOrMerge(fromIndex, false, toIndex, false);
    public void TransferTo(int fromIndex, Inventory targetInventory, int toIndex) => TransferTo(fromIndex, false, targetInventory, toIndex, false);
    public bool QuickTransfer(int fromIndex, Inventory targetInventory) => QuickTransfer(fromIndex, false, targetInventory);
}