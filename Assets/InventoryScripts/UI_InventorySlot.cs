using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    [HideInInspector] public int slotIndex; // Индекс этого слота в массиве
    private UI_Inventory uiInventory;
    private Transform originalParent;

    public void Init(UI_Inventory uiInv, int index)
    {
        uiInventory = uiInv;
        slotIndex = index;
    }

    public void Render(InventorySlot slot)
    {
        if (slot != null && slot.item != null)
        {
            icon.sprite = slot.item.icon;
            icon.enabled = true;

            if (slot.item.isStackable && slot.amount > 1)
            {
                amountText.text = slot.amount.ToString();
                amountText.enabled = true;
            }
            else
            {
                amountText.enabled = false;
            }
        }
        else
        {
            icon.enabled = false;
            amountText.enabled = false;
        }
    }

    // --- Начало перетаскивания ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Проверяем, есть ли предмет в слоте
        if (uiInventory.inventory.slots[slotIndex].item == null) return;

        originalParent = icon.transform.parent;
        // Переносим иконку на самую верхнюю панель Canvas, чтобы она отображалась ПОВЕРХ всех слотов
        icon.transform.SetParent(uiInventory.transform.root);
        icon.raycastTarget = false; // Отключаем Raycast, чтобы мышка "видела" слот под иконкой
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (uiInventory.inventory.slots[slotIndex].item == null) return;
        
        icon.transform.position = Input.mousePosition;
    }

    // --- Конец перетаскивания ---
    public void OnEndDrag(PointerEventData eventData)
    {
        if (uiInventory.inventory.slots[slotIndex].item == null) return;
        
        icon.transform.SetParent(originalParent);
        icon.transform.localPosition = Vector3.zero;
        icon.raycastTarget = true;
        
        uiInventory.UpdateUI();
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj != null)
        {
            UI_InventorySlot fromSlot = droppedObj.GetComponent<UI_InventorySlot>();
            if (fromSlot != null)
            {
                uiInventory.SwapSlots(fromSlot.slotIndex, this.slotIndex);
            }
        }
    }
}