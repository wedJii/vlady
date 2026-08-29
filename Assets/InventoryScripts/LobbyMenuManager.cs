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

        // Если в хранилище еще нет предметов
        if (!RaidLoadoutManager.IsInitialized || !RaidLoadoutManager.HasAnyItems)
        {
            var initialSlots = new List<InventorySlot>();

            // 1. Проверяем defaultStarterItems
            if (defaultStarterItems != null && defaultStarterItems.Count > 0)
            {
                foreach (var entry in defaultStarterItems)
                {
                    if (entry.item != null && entry.amount > 0)
                        initialSlots.Add(new InventorySlot(entry.item, entry.amount));
                }
            }

            // 2. Если defaultStarterItems пуст, проверяем не настроены ли слоты напрямую в компоненте Inventory
            if (initialSlots.Count == 0 && stashInventory != null && stashInventory.Slots != null)
            {
                foreach (var s in stashInventory.Slots)
                {
                    if (s != null && !s.IsEmpty && s.item != null)
                        initialSlots.Add(new InventorySlot(s.item, s.amount));
                }
            }

            int stashCap = stashInventory != null ? stashInventory.Capacity : 20;
            int raidCap = raidInventory != null ? raidInventory.Capacity : 8;
            RaidLoadoutManager.Initialize(initialSlots, stashCap, raidCap, force: true);
        }

        RaidLoadoutManager.LoadToLobby(stashInventory, raidInventory);
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
