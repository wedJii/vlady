using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    public Inventory inventory;         
    public GameObject slotPrefab;       
    public Transform slotsParent;       

    private List<UI_InventorySlot> uiSlots = new List<UI_InventorySlot>();

    private void Start()
    {
        InitUI();
        UpdateUI();
    }

    public void InitUI()
    {
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        uiSlots.Clear();

        for (int i = 0; i < inventory.capacity; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, slotsParent);
            UI_InventorySlot uiSlot = newSlotObj.GetComponent<UI_InventorySlot>();
            
            // Передаем ссылку и индекс слота
            uiSlot.Init(this, i); 
            
            uiSlots.Add(uiSlot);
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            uiSlots[i].Render(inventory.slots[i]);
        }
    }

    // ВОТ ЭТОГО МЕТОДА НЕ ХВАТАЕТ ИЛИ ФАЙЛ НЕ СОХРАНЕН:
    public void SwapSlots(int fromIndex, int toIndex)
    {
        var tempSlot = inventory.slots[fromIndex];
        inventory.slots[fromIndex] = inventory.slots[toIndex];
        inventory.slots[toIndex] = tempSlot;

        UpdateUI();
    }
}