using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenuManager : MonoBehaviour
{
    [Serializable]
    public struct StarterItemEntry
    {
        public Item item;
        public int amount;
    }

    [Header("Inventories")]
    [SerializeField] private Inventory stashInventory;
    [SerializeField] private Inventory raidInventory;

    [Header("Starter Configuration")]
    [SerializeField] private List<StarterItemEntry> defaultStarterItems = new();

    [Header("Scene Transition")]
    [SerializeField] private string dungeonSceneName = "Dungeon 1";

    private void Start()
    {
        Time.timeScale = 1f;

        if (stashInventory != null)
            stashInventory.plateCapacity = 0;

        if (raidInventory != null)
            raidInventory.plateCapacity = 0;

        // 1. Считываем все предметы, настроенные в Инспекторе
        var inspectorItems = new List<InventorySlot>();

        if (defaultStarterItems != null && defaultStarterItems.Count > 0)
        {
            foreach (var entry in defaultStarterItems)
            {
                if (entry.item != null && entry.amount > 0)
                    inspectorItems.Add(new InventorySlot(entry.item, entry.amount));
            }
        }

        if (stashInventory != null && stashInventory.Slots != null)
        {
            foreach (var s in stashInventory.Slots)
            {
                if (s != null && !s.IsEmpty && s.item != null)
                    inspectorItems.Add(new InventorySlot(s.item, s.amount));
            }
        }

        // 2. Первичная инициализация при чистом старте
        if (!RaidLoadoutManager.IsInitialized || !RaidLoadoutManager.HasAnyItems)
        {
            int stashCap = stashInventory != null ? stashInventory.Capacity : 20;
            int raidCap = raidInventory != null ? raidInventory.Capacity : 8;
            int plateCap = 3;
            RaidLoadoutManager.Initialize(inspectorItems, stashCap, raidCap, plateCap, force: true);
        }

        // 3. Загружаем инвентарь
        RaidLoadoutManager.LoadToLobby(stashInventory, raidInventory);

        // 4. Подтягиваем настроенные в Инспекторе предметы, если они отсутствуют у игрока (например, после смерти)
        if (inspectorItems.Count > 0 && stashInventory != null)
        {
            bool addedAny = false;
            foreach (var extra in inspectorItems)
            {
                bool hasInStash = stashInventory.ContainsItem(extra.item, 1);
                bool hasInRaid = raidInventory != null && raidInventory.ContainsItem(extra.item, 1);

                if (!hasInStash && !hasInRaid)
                {
                    stashInventory.AddItem(extra.item, extra.amount);
                    addedAny = true;
                }
            }

            if (addedAny)
            {
                RaidLoadoutManager.SaveLoadout(stashInventory, raidInventory);
            }
        }
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            BackToMainMenu();
    }

    public void StartRaid()
    {
        RaidLoadoutManager.StartRaidSession(stashInventory, raidInventory);
        Time.timeScale = 1f;
        SceneManager.LoadScene(dungeonSceneName);
    }

    public void BackToMainMenu()
    {
        RaidLoadoutManager.SaveLoadout(stashInventory, raidInventory);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
