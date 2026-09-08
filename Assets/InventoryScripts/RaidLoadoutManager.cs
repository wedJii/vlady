using System;
using System.Collections.Generic;
using UnityEngine;

public static class RaidLoadoutManager
{
    private const string PREFS_STASH_KEY = "Save_StashSlots";
    private const string PREFS_RAID_KEY = "Save_RaidSlots";
    private const string PREFS_PLATES_KEY = "Save_PlateSlots";
    private const string PREFS_COLLECTED_KEY = "Save_CollectedWorldItems";
    private const string PREFS_INITIALIZED_KEY = "Save_ProfileInitialized";
    private const string PREFS_IN_RAID_KEY = "Save_PlayerIsInRaid";
    private const string PREFS_GRANTED_STARTER_KEY = "Save_GrantedStarterItems";

    private static readonly List<InventorySlot> _stashSlots = new();
    private static readonly List<InventorySlot> _raidSlots = new();
    private static readonly List<InventorySlot> _plateSlots = new();
    private static readonly HashSet<string> _collectedWorldItemIDs = new();
    private static readonly Dictionary<string, Item> _itemDatabase = new();

    public static bool IsInitialized { get; private set; }

    public static bool HasStashItems => _stashSlots.Exists(s => s != null && !s.IsEmpty);
    public static bool HasRaidItems => _raidSlots.Exists(s => s != null && !s.IsEmpty);
    public static bool HasPlateItems => _plateSlots.Exists(s => s != null && !s.IsEmpty);
    public static bool HasAnyItems => HasStashItems || HasRaidItems || HasPlateItems;

    public static IReadOnlyList<InventorySlot> StashSlots => _stashSlots;
    public static IReadOnlyList<InventorySlot> RaidSlots => _raidSlots;
    public static IReadOnlyList<InventorySlot> PlateSlots => _plateSlots;

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
        if (PlayerPrefs.GetInt(PREFS_IN_RAID_KEY, 0) == 1)
        {
            Debug.LogWarning("<color=orange>[RaidLoadoutManager]</color> Незавершенный рейд. Снаряжение рейда утеряно.");
            foreach (var slot in _raidSlots) slot.Clear();
            foreach (var slot in _plateSlots) slot.Clear();
            _collectedWorldItemIDs.Clear();

            PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
            SaveToDisk();
        }
    }

    public static void RegisterItem(Item item)
    {
        if (item == null) return;
        if (!string.IsNullOrEmpty(item.id)) _itemDatabase[item.id] = item;
        if (!string.IsNullOrEmpty(item.name)) _itemDatabase[item.name] = item;
        if (!string.IsNullOrEmpty(item.displayName)) _itemDatabase[item.displayName] = item;
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
                if (!string.IsNullOrEmpty(it.displayName)) _itemDatabase[it.displayName] = it;
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
    }

    public static void Initialize(IEnumerable<InventorySlot> starterItems, int stashCapacity = 20, int raidCapacity = 8, int plateCapacity = 3, bool force = false)
    {
        CheckCrashOrAbandonment();

        if (IsInitialized && !force) return;

        if (PlayerPrefs.GetInt(PREFS_INITIALIZED_KEY, 0) == 1 && !force)
        {
            LoadFromDisk();
            if (IsInitialized) return;
        }

        _stashSlots.Clear();
        _raidSlots.Clear();
        _plateSlots.Clear();

        while (_stashSlots.Count < stashCapacity) _stashSlots.Add(new InventorySlot());
        while (_raidSlots.Count < raidCapacity) _raidSlots.Add(new InventorySlot());
        while (_plateSlots.Count < plateCapacity) _plateSlots.Add(new InventorySlot());

        var grantedSet = new HashSet<string>();

        if (starterItems != null)
        {
            int index = 0;
            foreach (var slot in starterItems)
            {
                if (slot != null && !slot.IsEmpty && index < _stashSlots.Count)
                {
                    _stashSlots[index].Set(slot.item, slot.amount);
                    RegisterItem(slot.item);
                    string key = !string.IsNullOrEmpty(slot.item.id) ? slot.item.id : slot.item.name;
                    if (!string.IsNullOrEmpty(key)) grantedSet.Add(key);
                    index++;
                }
            }
        }

        PlayerPrefs.SetString(PREFS_GRANTED_STARTER_KEY, string.Join(";", grantedSet));
        IsInitialized = true;
        PlayerPrefs.SetInt(PREFS_INITIALIZED_KEY, 1);
        SaveToDisk();
    }

    public static void SyncNewStarterItems(IEnumerable<LobbyMenuManager.StarterItemEntry> starterEntries, Inventory stashInventory, Inventory raidInventory = null)
    {
        if (starterEntries == null || stashInventory == null) return;

        bool addedAny = false;
        foreach (var entry in starterEntries)
        {
            if (entry.item == null || entry.amount <= 0) continue;
            RegisterItem(entry.item);

            int currentTotal = stashInventory.CountItem(entry.item) + (raidInventory != null ? raidInventory.CountItem(entry.item) : 0);
            if (currentTotal < entry.amount)
            {
                stashInventory.AddItem(entry.item, entry.amount - currentTotal);
                addedAny = true;
            }
        }

        if (addedAny)
        {
            SaveLoadout(stashInventory, raidInventory);
        }
    }

    public static void StartRaidSession(Inventory stashInventory, Inventory raidInventory)
    {
        if (stashInventory != null)
            CopySlots(stashInventory.Slots, _stashSlots, stashInventory.Capacity);

        if (raidInventory != null)
        {
            CopySlots(raidInventory.Slots, _raidSlots, raidInventory.Capacity);
            CopySlots(raidInventory.PlateSlots, _plateSlots, raidInventory.PlateCapacity > 0 ? raidInventory.PlateCapacity : 3);
        }

        _collectedWorldItemIDs.Clear();
        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 1);
        SaveToDisk();
        PlayerPrefs.Save();
    }

    public static void SaveLoadout(Inventory stashInventory, Inventory raidInventory)
    {
        if (stashInventory != null)
            CopySlots(stashInventory.Slots, _stashSlots, stashInventory.Capacity);

        if (raidInventory != null)
        {
            CopySlots(raidInventory.Slots, _raidSlots, raidInventory.Capacity);
            CopySlots(raidInventory.PlateSlots, _plateSlots, raidInventory.PlateCapacity > 0 ? raidInventory.PlateCapacity : 3);
        }

        IsInitialized = true;
        SaveToDisk();
    }

    public static void LoadToLobby(Inventory stashInventory, Inventory raidInventory)
    {
        CheckCrashOrAbandonment();

        if (stashInventory != null)
        {
            while (_stashSlots.Count < stashInventory.Capacity)
                _stashSlots.Add(new InventorySlot());
            stashInventory.LoadFromSlots(_stashSlots);
        }

        if (raidInventory != null)
        {
            while (_raidSlots.Count < raidInventory.Capacity)
                _raidSlots.Add(new InventorySlot());
            raidInventory.LoadFromSlots(_raidSlots);
        }

        if (_plateSlots.Count > 0)
        {
            for (int i = 0; i < _plateSlots.Count; i++)
            {
                var pSlot = _plateSlots[i];
                if (pSlot != null && !pSlot.IsEmpty && pSlot.item != null)
                {
                    bool added = false;
                    if (raidInventory != null)
                        added = raidInventory.AddItem(pSlot.item, pSlot.amount);

                    if (!added && stashInventory != null)
                        stashInventory.AddItem(pSlot.item, pSlot.amount);

                    pSlot.Clear();
                }
            }
            SaveToDisk();
        }
    }

    public static void ApplyToDungeon(Inventory dungeonInventory, PlayerControll player = null)
    {
        if (dungeonInventory == null) return;
        dungeonInventory.LoadFromSlots(_raidSlots);
        dungeonInventory.LoadPlatesFromSlots(_plateSlots, player);

        if (player != null)
            dungeonInventory.ApplyAllEquippedPlates(player);
    }

    public static void OnSuccessfulExtraction(Inventory dungeonInventory)
    {
        if (dungeonInventory != null)
        {
            CopySlots(dungeonInventory.Slots, _raidSlots, dungeonInventory.Capacity);
            CopySlots(dungeonInventory.PlateSlots, _plateSlots, dungeonInventory.PlateCapacity > 0 ? dungeonInventory.PlateCapacity : 3);
        }

        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
        SaveToDisk();
        Debug.Log("<color=green>[RaidLoadoutManager]</color> Успешная эвакуация! Снаряжение и пластины сохранены.");
    }

    public static void SaveDungeonLoot(Inventory dungeonInventory) => OnSuccessfulExtraction(dungeonInventory);

    public static void OnPlayerDied()
    {
        foreach (var slot in _raidSlots) slot.Clear();
        foreach (var slot in _plateSlots) slot.Clear();
        _collectedWorldItemIDs.Clear();

        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
        SaveToDisk();
        Debug.LogWarning("<color=red>[RaidLoadoutManager]</color> Игрок погиб в рейде! Снаряжение и пластины утеряны.");
    }

    public static void OnRaidAbandoned()
    {
        foreach (var slot in _raidSlots) slot.Clear();
        foreach (var slot in _plateSlots) slot.Clear();
        _collectedWorldItemIDs.Clear();

        PlayerPrefs.SetInt(PREFS_IN_RAID_KEY, 0);
        SaveToDisk();
        Debug.LogWarning("<color=red>[RaidLoadoutManager]</color> Рейд покинут! Снаряжение рейда и пластины утеряны.");
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
            var plateWrapper = new SlotListWrapper();

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

                foreach (var s in _plateSlots)
                {
                    if (s != null && !s.IsEmpty && s.item != null)
                    {
                        RegisterItem(s.item);
                        plateWrapper.slots.Add(new SerializedSlot { itemId = !string.IsNullOrEmpty(s.item.id) ? s.item.id : s.item.name, amount = s.amount });
                    }
                    else
                    {
                        plateWrapper.slots.Add(new SerializedSlot { itemId = "", amount = 0 });
                    }
                }
            }

            stashWrapper.collectedItems = new List<string>(_collectedWorldItemIDs);

            PlayerPrefs.SetString(PREFS_STASH_KEY, JsonUtility.ToJson(stashWrapper));
            PlayerPrefs.SetString(PREFS_RAID_KEY, JsonUtility.ToJson(raidWrapper));
            PlayerPrefs.SetString(PREFS_PLATES_KEY, JsonUtility.ToJson(plateWrapper));
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
            string plateJson = PlayerPrefs.GetString(PREFS_PLATES_KEY, "");

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

            if (!string.IsNullOrEmpty(plateJson))
            {
                var wrapper = JsonUtility.FromJson<SlotListWrapper>(plateJson);
                if (wrapper != null)
                {
                    _plateSlots.Clear();
                    foreach (var s in wrapper.slots)
                    {
                        var item = FindItem(s.itemId);
                        _plateSlots.Add(item != null && s.amount > 0 ? new InventorySlot(item, s.amount) : new InventorySlot());
                    }
                }
            }

            while (_stashSlots.Count < 20) _stashSlots.Add(new InventorySlot());
            while (_raidSlots.Count < 8) _raidSlots.Add(new InventorySlot());
            while (_plateSlots.Count < 3) _plateSlots.Add(new InventorySlot());

            IsInitialized = PlayerPrefs.GetInt(PREFS_INITIALIZED_KEY, 0) == 1 || _stashSlots.Count > 0 || _raidSlots.Count > 0;
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
        PlayerPrefs.DeleteKey(PREFS_PLATES_KEY);
        PlayerPrefs.DeleteKey(PREFS_COLLECTED_KEY);
        PlayerPrefs.DeleteKey(PREFS_INITIALIZED_KEY);
        PlayerPrefs.DeleteKey(PREFS_IN_RAID_KEY);
        PlayerPrefs.DeleteKey(PREFS_GRANTED_STARTER_KEY);
        PlayerPrefs.Save();
        _stashSlots.Clear();
        _raidSlots.Clear();
        _plateSlots.Clear();
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
