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

    [Header("UI Panels & Buttons")]
    [SerializeField] private GameObject lootMenu;
    [SerializeField] private GameObject startButton;

    [Header("Starter Configuration")]
    [SerializeField] private List<StarterItemEntry> defaultStarterItems = new();

    [Header("Inspector Cheats (Выдача в сундук)")]
    [Tooltip("Нажмите галочку в Инспекторе, чтобы выдать предметы из списка defaultStarterItems в сундук")]
    [SerializeField] private bool grantItemsToStashNow;

    [Header("Scene Transition")]
    [SerializeField] private string dungeonSceneName = "Dungeon 1";

    private bool _isLoadingLobby;

    public GameObject LootMenu => lootMenu;
    public GameObject StartButton => startButton;

    private void OnEnable()
    {
        if (stashInventory != null)
            stashInventory.OnInventoryChanged += OnLobbyInventoryChanged;
        if (raidInventory != null)
            raidInventory.OnInventoryChanged += OnLobbyInventoryChanged;
    }

    private void OnDisable()
    {
        if (stashInventory != null)
            stashInventory.OnInventoryChanged -= OnLobbyInventoryChanged;
        if (raidInventory != null)
            raidInventory.OnInventoryChanged -= OnLobbyInventoryChanged;
    }

    private void OnLobbyInventoryChanged()
    {
        if (_isLoadingLobby) return;
        RaidLoadoutManager.SaveLoadout(stashInventory, raidInventory);
    }

    private void OnValidate()
    {
        if (grantItemsToStashNow)
        {
            grantItemsToStashNow = false;
            GiveStarterItems();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (stashInventory != null)
            stashInventory.plateCapacity = 0;

        if (raidInventory != null)
            raidInventory.plateCapacity = 0;

        _isLoadingLobby = true;
        try
        {
            int stashCap = stashInventory != null ? stashInventory.Capacity : 20;
            int raidCap = raidInventory != null ? raidInventory.Capacity : 8;
            int plateCap = 3;

            if (!RaidLoadoutManager.IsInitialized)
            {
                var starterSlots = new List<InventorySlot>();
                if (defaultStarterItems != null)
                {
                    foreach (var entry in defaultStarterItems)
                    {
                        if (entry.item != null && entry.amount > 0)
                        {
                            RaidLoadoutManager.RegisterItem(entry.item);
                            starterSlots.Add(new InventorySlot(entry.item, entry.amount));
                        }
                    }
                }
                RaidLoadoutManager.Initialize(starterSlots, stashCap, raidCap, plateCap, force: false);
            }

            RaidLoadoutManager.LoadToLobby(stashInventory, raidInventory);
        }
        finally
        {
            _isLoadingLobby = false;
        }

        RefreshAllUI();

        if (lootMenu != null)
            lootMenu.SetActive(false);

        if (startButton != null)
            startButton.SetActive(false);
    }

    private void Update()
    {
        bool escPressed = false;
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            escPressed = true;

        if (!escPressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    escPressed = true;
            }
            catch { }
        }

        if (escPressed)
        {
            if (lootMenu != null && lootMenu.activeSelf)
                CloseLootMenu();
            else
                BackToMainMenu();
        }
    }

    public void RefreshAllUI()
    {
        if (lootMenu != null)
        {
            foreach (var uiInv in lootMenu.GetComponentsInChildren<UI_Inventory>(true))
                uiInv.UpdateUI();
        }
    }

    public void SelectDungeon(string sceneName)
    {
        dungeonSceneName = sceneName;
        if (lootMenu != null)
        {
            lootMenu.SetActive(true);
            RefreshAllUI();
        }
        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }

    public void SelectDungeon1() => SelectDungeon("Dungeon 1");

    public void CloseLootMenu()
    {
        if (lootMenu != null)
            lootMenu.SetActive(false);
        if (startButton != null)
            startButton.SetActive(false);
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

    [ContextMenu("Выдать все пластины в сундук")]
    public void GiveAllPlates()
    {
        var allPlates = Resources.FindObjectsOfTypeAll<UbgradePlate>();
#if UNITY_EDITOR
        if (allPlates == null || allPlates.Length == 0)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:UbgradePlate");
            var list = new List<UbgradePlate>();
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var p = UnityEditor.AssetDatabase.LoadAssetAtPath<UbgradePlate>(path);
                if (p != null) list.Add(p);
            }
            allPlates = list.ToArray();
        }
#endif
        if (allPlates == null || allPlates.Length == 0)
        {
            Debug.LogWarning("[LobbyMenuManager] Пластины не найдены в проекте!");
            return;
        }

        int count = 0;
        foreach (var plate in allPlates)
        {
            if (plate == null) continue;
            RaidLoadoutManager.RegisterItem(plate);
            if (stashInventory != null)
            {
                stashInventory.AddItem(plate, 1);
                count++;
            }
        }

        if (count > 0)
        {
            RaidLoadoutManager.SaveLoadout(stashInventory, raidInventory);
            RefreshAllUI();
            Debug.Log($"<color=green>[LobbyMenuManager]</color> Успешно выдано {count} пластин в сундук!");
        }
    }

    [ContextMenu("Выдать стартовые предметы в сундук")]
    public void GiveStarterItems()
    {
        if (defaultStarterItems == null || defaultStarterItems.Count == 0 || stashInventory == null)
        {
            Debug.LogWarning("[LobbyMenuManager] Список defaultStarterItems пуст!");
            return;
        }

        int added = 0;
        foreach (var entry in defaultStarterItems)
        {
            if (entry.item != null && entry.amount > 0)
            {
                RaidLoadoutManager.RegisterItem(entry.item);
                stashInventory.AddItem(entry.item, entry.amount);
                added += entry.amount;
            }
        }

        if (added > 0)
        {
            RaidLoadoutManager.SaveLoadout(stashInventory, raidInventory);
            RefreshAllUI();
            Debug.Log($"<color=green>[LobbyMenuManager]</color> Выдано {added} стартовых предметов в сундук!");
        }
    }

    [ContextMenu("Сбросить сохранение инвентаря")]
    public void ResetInventorySave()
    {
        RaidLoadoutManager.ClearSaveData();
        Start();
        RefreshAllUI();
        Debug.Log("<color=yellow>[LobbyMenuManager]</color> Сохранения сброшены к дефолту.");
    }
}
