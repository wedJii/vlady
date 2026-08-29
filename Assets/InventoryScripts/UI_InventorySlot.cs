using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    private int _slotIndex;
    private UI_Inventory _uiInventory;
    private CanvasGroup _canvasGroup;
    private Tween _hoverTween;

    public UI_Inventory UIInventory => _uiInventory;
    public int SlotIndex => _slotIndex;

    private CanvasGroup CanvasGroup => _canvasGroup ??= (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>());

    private void Awake() => EnsureComponents();

    private void EnsureComponents()
    {
        if (icon == null)
        {
            var iconTr = transform.Find("Icon");
            if (iconTr != null) icon = iconTr.GetComponent<Image>();
            if (icon == null) icon = GetComponentInChildren<Image>();
        }

        if (amountText == null)
            amountText = transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();

        if (amountText != null)
            amountText.color = Color.white;
    }

    public void Init(UI_Inventory uiInv, int index)
    {
        EnsureComponents();
        _uiInventory = uiInv;
        _slotIndex = index;
    }

    public void Render(InventorySlot slot)
    {
        EnsureComponents();

        if (slot != null && !slot.IsEmpty && slot.item != null)
        {
            if (icon != null)
            {
                if (slot.item.icon != null)
                {
                    icon.sprite = slot.item.icon;
                    icon.color = Color.white;
                    icon.enabled = true;
                }
                else
                {
                    icon.sprite = null;
                    icon.color = new Color(0.2f, 0.55f, 0.85f, 0.9f);
                    icon.enabled = true;
                }
            }

            if (amountText != null)
            {
                string txt = slot.amount > 1 ? slot.amount.ToString() : (slot.item.icon == null ? slot.item.displayName : "");
                amountText.text = txt;
                amountText.color = Color.white;
                amountText.enabled = !string.IsNullOrEmpty(txt);
            }
        }
        else
        {
            if (icon != null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }
            if (amountText != null)
            {
                amountText.text = "";
                amountText.enabled = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(1.08f, 0.15f).SetUpdate(true);

        if (_uiInventory != null && _uiInventory.Inventory != null && _slotIndex < _uiInventory.Inventory.Slots.Count)
        {
            var slot = _uiInventory.Inventory.Slots[_slotIndex];
            if (slot != null && !slot.IsEmpty && slot.item != null)
            {
                UI_ItemTooltip.Show(slot.item);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(1f, 0.15f).SetUpdate(true);
        UI_ItemTooltip.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Клик ПКМ выполняет быстрый перенос в связанный инвентарь (Склад <-> Рюкзак)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _uiInventory?.QuickTransfer(_slotIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        UI_ItemTooltip.Hide();

        if (_uiInventory == null || _uiInventory.Inventory == null || _slotIndex >= _uiInventory.Inventory.Slots.Count) return;

        var slot = _uiInventory.Inventory.Slots[_slotIndex];
        if (slot == null || slot.IsEmpty || slot.item == null) return;

        if (icon != null)
            icon.color = new Color(1f, 1f, 1f, 0.3f);

        CanvasGroup.blocksRaycasts = false;
        _uiInventory.ShowDragGhost(slot.item.icon, icon != null ? icon.rectTransform.sizeDelta : new Vector2(50, 50));
    }

    public void OnDrag(PointerEventData eventData)
    {
        _uiInventory?.UpdateDragGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (icon != null)
            icon.color = Color.white;

        CanvasGroup.blocksRaycasts = true;
        _uiInventory?.HideDragGhost();
        _uiInventory?.UpdateUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || _uiInventory == null) return;

        var draggedSlot = eventData.pointerDrag.GetComponent<UI_InventorySlot>() ?? 
                          eventData.pointerDrag.GetComponentInParent<UI_InventorySlot>();

        if (draggedSlot != null && draggedSlot.UIInventory != null)
        {
            _uiInventory.TransferOrSwap(draggedSlot.UIInventory, draggedSlot.SlotIndex, this._slotIndex);
        }
    }
}