using System;
using System.Collections.Generic;
using UnityEngine;

public static class RaidLoadoutManager
{
    private const string PREFS_STASH_KEY = "Save_StashSlots";
    private const string PREFS_RAID_KEY = "Save_RaidSlots";
    private const string PREFS_COLLECTED_KEY = "Save_CollectedWorldItems";
    private const string PREFS_INITIALIZED_KEY = "Save_ProfileInitialized";
    private const string PREFS_IN_RAID_KEY = "Save_PlayerIsInRaid";

    private static readonly List<InventorySlot> _stashSlots = new();
    private static readonly List<InventorySlot> _raidSlots = new();
    private static readonly HashSet<string> _collectedWorldItemIDs = new();
    private static readonly Dictionary<string, Item> _itemDatabase = new();

    public static bool IsInitialized { get; private set; }

    public static bool HasStashItems => _stashSlots.Exists(s => s != null && !s.IsEmpty);
    public static bool HasRaidItems => _raidSlots.Exists(s => s != null && !s.IsEmpty);
    public static bool HasAnyItems => HasStashItems || HasRaidItems;

    public static IReadOnlyList<InventorySlot> StashSlots => _stashSlots;
    public static IReadOnlyList<InventorySlot> RaidSlots => _raidSlots;

    [Serializable]
    private struct SerializedSlot
    {
        public string itemId;
        public int amount;
    }

    [Serializable]
    private class SlotListWrapper
    {
        public List<SerializedSlot> slots = new();
        public List<string> collectedItems = new();
    }

    static RaidLoadoutManager()
    {
        LoadFromDisk();
        CheckCrashOrAbandonment();
    }

    public static void CheckCrashOrAbandonment()
    {
        // Если при запуске флаг InRaid == 1, значит игрок вылетел, закрыл игру (Alt+F4) или вышел без эвакуации
        if (PlayerPrefs.GetInt(PREFS_IN_RAID_KEY, 0) == 1)
        {
            Debug.LogWarning("<color=orange>[RaidLoadoutManager]</color> Обнаружен незавершенный рейд (вылет/выход из игры). Снаряжение рейда утеряно!");
            foreach (var slot in _raidSlots)
                slot.Clear();

            PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
            SaveToDisk();
        }
    }

    public static void RegisterItem(Item item)
    {
        if (item == null) return;
        if (!string.IsNullOrEmpty(item.id)) _itemDatabase[item.id] = item;
        if (!string.IsNullOrEmpty(item.name)) _itemDatabase[item.name] = item;
    }

    public static Item FindItem(string idOrName)
    {
        if (string.IsNullOrEmpty(idOrName)) return null;
        if (_itemDatabase.TryGetValue(idOrName, out var found) && found != null) return found;

        var allItems = Resources.FindObjectsOfTypeAll<Item>();
        foreach (var it in allItems)
        {
            if (it != null)
            {
                if (!string.IsNullOrEmpty(it.id)) _itemDatabase[it.id] = it;
                if (!string.IsNullOrEmpty(it.name)) _itemDatabase[it.name] = it;
            }
        }

        return _itemDatabase.TryGetValue(idOrName, out var item) ? item : null;
    }

    public static bool IsWorldItemCollected(string uniqueID)
    {
        if (string.IsNullOrEmpty(uniqueID)) return false;
        return _collectedWorldItemIDs.Contains(uniqueID);
    }

    public static void MarkWorldItemCollected(string uniqueID)
    {
        if (string.IsNullOrEmpty(uniqueID)) return;
        _collectedWorldItemIDs.Add(uniqueID);
        // Не сохраняем сразу на диск — сохранение экстракта произойдет ТОЛЬКО при успешной эвакуации
    }

    public static void Initialize(IEnumerable<InventorySlot> starterItems, int stashCapacity = 20, int raidCapacity = 8, bool force = false)
    {
        CheckCrashOrAbandonment();

        if (IsInitialized && !force && HasAnyItems) return;

        // Если в сохранении уже есть данные профиля, загружаем их
        if (PlayerPrefs.GetInt(PREFS_INITIALIZED_KEY, 0) == 1 && !force)
        {
            LoadFromDisk();
            if (IsInitialized && HasAnyItems) return;
        }

        _stashSlots.Clear();
        _raidSlots.Clear();

        while (_stashSlots.Count < stashCapacity) _stashSlots.Add(new InventorySlot());
        while (_raidSlots.Count < raidCapacity) _raidSlots.Add(new InventorySlot());

        if (starterItems != null)
        {
            int index = 0;
            foreach (var slot in starterItems)
            {
                if (slot != null && !slot.IsEmpty && index < _stashSlots.Count)
                {
                    _stashSlots[index].Set(slot.item, slot.amount);
                    RegisterItem(slot.item);
                    index++;
                }
            }
        }

        IsInitialized = true;
        PlayerPrefs.SetInt(PREFS_INITIALIZED_KEY, 1);
        SaveToDisk();
    }

    public static void StartRaidSession(Inventory stashInventory, Inventory raidInventory)
    {
        if (stashInventory != null)
            CopySlots(stashInventory.Slots, _stashSlots, stashInventory.Capacity);

        if (raidInventory != null)
            CopySlots(raidInventory.Slots, _raidSlots, raidInventory.Capacity);

        // Помечаем, что игрок сейчас в рейде.
        // На диске снаряжение рейда обнуляется, чтобы при Alt+F4 / вылете оно не оставалось у игрока
        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 1);
        PlayerPrefs.Save();
    }

    public static void SaveLoadout(Inventory stashInventory, Inventory raidInventory)
    {
        if (stashInventory != null)
            CopySlots(stashInventory.Slots, _stashSlots, stashInventory.Capacity);

        if (raidInventory != null)
            CopySlots(raidInventory.Slots, _raidSlots, raidInventory.Capacity);

        IsInitialized = true;
        SaveToDisk();
    }

    public static void LoadToLobby(Inventory stashInventory, Inventory raidInventory)
    {
        CheckCrashOrAbandonment();

        if (stashInventory != null && _stashSlots.Count > 0)
            stashInventory.LoadFromSlots(_stashSlots);

        if (raidInventory != null && _raidSlots.Count > 0)
            raidInventory.LoadFromSlots(_raidSlots);
    }

    public static void ApplyToDungeon(Inventory dungeonInventory)
    {
        if (dungeonInventory == null) return;
        dungeonInventory.LoadFromSlots(_raidSlots);
    }

    // Единственный легальный способ спасти и сохранить лут — успешная эвакуация
    public static void OnSuccessfulExtraction(Inventory dungeonInventory)
    {
        if (dungeonInventory != null)
            CopySlots(dungeonInventory.Slots, _raidSlots, dungeonInventory.Capacity);

        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
        SaveToDisk();
        Debug.Log("<color=green>[RaidLoadoutManager]</color> Успешная эвакуация! Лут сохранен на склад.");
    }

    public static void SaveDungeonLoot(Inventory dungeonInventory) => OnSuccessfulExtraction(dungeonInventory);

    public static void OnPlayerDied()
    {
        // При гибели в рейде всё снаряжение рейда теряется
        foreach (var slot in _raidSlots)
            slot.Clear();

        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
        SaveToDisk();
        Debug.LogWarning("<color=red>[RaidLoadoutManager]</color> Игрок погиб в рейде! Все вещи в рейде утеряны.");
    }

    public static void OnRaidAbandoned()
    {
        // Выход через меню паузы / лив из рейда = потеря всех рейдовых вещей
        foreach (var slot in _raidSlots)
            slot.Clear();

        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
        SaveToDisk();
        Debug.LogWarning("<color=red>[RaidLoadoutManager]</color> Рейд покинут до эвакуации! Все вещи в рейде утеряны.");
    }

    public static void SaveToDisk()
    {
        try
        {
            var stashWrapper = new SlotListWrapper();
            foreach (var s in _stashSlots)
            {
                if (s != null && !s.IsEmpty && s.item != null)
                {
                    RegisterItem(s.item);
                    stashWrapper.slots.Add(new SerializedSlot { itemId = !string.IsNullOrEmpty(s.item.id) ? s.item.id : s.item.name, amount = s.amount });
                }
                else
                {
                    stashWrapper.slots.Add(new SerializedSlot { itemId = "", amount = 0 });
                }
            }

            var raidWrapper = new SlotListWrapper();
            // Если игрок сейчас в рейде — на диск пишем пустые слоты (защита от краша / Alt+F4)
            bool isInRaid = PlayerPrefs.GetInt(PREFS_IN_RAID_KEY, 0) == 1;

            if (!isInRaid)
            {
                foreach (var s in _raidSlots)
                {
                    if (s != null && !s.IsEmpty && s.item != null)
                    {
                        RegisterItem(s.item);
                        raidWrapper.slots.Add(new SerializedSlot { itemId = !string.IsNullOrEmpty(s.item.id) ? s.item.id : s.item.name, amount = s.amount });
                    }
                    else
                    {
                        raidWrapper.slots.Add(new SerializedSlot { itemId = "", amount = 0 });
                    }
                }
            }

            stashWrapper.collectedItems = new List<string>(_collectedWorldItemIDs);

            PlayerPrefs.SetString(PREFS_STASH_KEY, JsonUtility.ToJson(stashWrapper));
            PlayerPrefs.SetString(PREFS_RAID_KEY, JsonUtility.ToJson(raidWrapper));
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RaidLoadoutManager] Ошибка сохранения: {ex.Message}");
        }
    }

    public static void LoadFromDisk()
    {
        try
        {
            if (!PlayerPrefs.HasKey(PREFS_STASH_KEY)) return;

            string stashJson = PlayerPrefs.GetString(PREFS_STASH_KEY, "");
            string raidJson = PlayerPrefs.GetString(PREFS_RAID_KEY, "");

            if (!string.IsNullOrEmpty(stashJson))
            {
                var wrapper = JsonUtility.FromJson<SlotListWrapper>(stashJson);
                if (wrapper != null)
                {
                    _stashSlots.Clear();
                    foreach (var s in wrapper.slots)
                    {
                        var item = FindItem(s.itemId);
                        _stashSlots.Add(item != null && s.amount > 0 ? new InventorySlot(item, s.amount) : new InventorySlot());
                    }

                    if (wrapper.collectedItems != null)
                    {
                        _collectedWorldItemIDs.Clear();
                        foreach (var id in wrapper.collectedItems)
                            _collectedWorldItemIDs.Add(id);
                    }
                }
            }

            if (!string.IsNullOrEmpty(raidJson))
            {
                var wrapper = JsonUtility.FromJson<SlotListWrapper>(raidJson);
                if (wrapper != null)
                {
                    _raidSlots.Clear();
                    foreach (var s in wrapper.slots)
                    {
                        var item = FindItem(s.itemId);
                        _raidSlots.Add(item != null && s.amount > 0 ? new InventorySlot(item, s.amount) : new InventorySlot());
                    }
                }
            }

            IsInitialized = _stashSlots.Count > 0 || _raidSlots.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RaidLoadoutManager] Ошибка загрузки: {ex.Message}");
        }
    }

    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(PREFS_STASH_KEY);
        PlayerPrefs.DeleteKey(PREFS_RAID_KEY);
        PlayerPrefs.DeleteKey(PREFS_COLLECTED_KEY);
        PlayerPrefs.DeleteKey(PREFS_INITIALIZED_KEY);
        PlayerPrefs.DeleteKey(PREFS_IN_RAID_KEY);
        PlayerPrefs.Save();
        _stashSlots.Clear();
        _raidSlots.Clear();
        _collectedWorldItemIDs.Clear();
        IsInitialized = false;
    }

    private static void CopySlots(IReadOnlyList<InventorySlot> source, List<InventorySlot> destination, int capacity)
    {
        destination.Clear();
        for (int i = 0; i < capacity; i++)
        {
            if (i < source.Count && source[i] != null && !source[i].IsEmpty)
                destination.Add(new InventorySlot(source[i].item, source[i].amount));
            else
                destination.Add(new InventorySlot());
        }
    }
}
