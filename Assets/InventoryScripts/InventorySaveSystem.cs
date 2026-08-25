using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class InventorySaveSystem : MonoBehaviour
{
    public Inventory inventory;
    public UI_Inventory uiInventory;
    
    // Массив всех предметов, которые существуют в игре (для поиска по ID)
    public List<Item> allGameItems;

    private string saveFilePath;

    private void Awake()
    {
        // Путь к файлу сохранения на компьютере/телефоне
        saveFilePath = Path.Combine(Application.persistentDataPath, "inventory_save.json");
    }

    // --- СОХРАНЕНИЕ ---
    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var slot in inventory.slots)
        {
            if (slot.item != null)
            {
                saveData.slots.Add(new SlotSaveData(slot.item.id, slot.amount));
            }
            else
            {
                saveData.slots.Add(new SlotSaveData("", 0)); // Пустой слот
            }
        }

        // Переводим класс в строку JSON
        string json = JsonUtility.ToJson(saveData, true);
        // Записываем в файл
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"Инвентарь сохранен в: {saveFilePath}");
    }

    // --- ЗАГРУЗКА ---
    public void LoadInventory()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Файл сохранения не найден!");
            return;
        }

        // Читаем JSON-текст из файла
        string json = File.ReadAllText(saveFilePath);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        // Очищаем и восстанавливаем логику инвентаря
        for (int i = 0; i < inventory.capacity; i++)
        {
            if (i < saveData.slots.Count && !string.IsNullOrEmpty(saveData.slots[i].itemID))
            {
                Item foundItem = GetItemByID(saveData.slots[i].itemID);
                inventory.slots[i].item = foundItem;
                inventory.slots[i].amount = saveData.slots[i].amount;
            }
            else
            {
                inventory.slots[i].ClearSlot();
            }
        }

        // Обновляем визуал UI
        uiInventory.UpdateUI();
        Debug.Log("Инвентарь успешно загружен!");
    }

    // Поиск предмета из базы по его уникальному ID
    private Item GetItemByID(string id)
    {
        foreach (var item in allGameItems)
        {
            if (item.id == id) return item;
        }
        Debug.LogError($"Предмет с ID {id} не найден в базе allGameItems!");
        return null;
    }

    // Горячие клавиши для проверки в редакторе
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) SaveInventory(); // Нажми S для сохранения
        if (Input.GetKeyDown(KeyCode.L)) LoadInventory(); // Нажми L для загрузки
    }
}