using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    private int _slotIndex;
    private UI_Inventory _uiInventory;
    private CanvasGroup _canvasGroup;
    private Tween _hoverTween;

    private CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }

    private void Awake()
    {
        EnsureComponents();
    }

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

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
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
                icon.sprite = slot.item.icon;
                icon.enabled = true; // Показываем иконку (если спрайта нет, Unity отобразит белый тестовый квадрат)
                icon.color = Color.white;
            }

            if (amountText != null)
            {
                bool showCount = slot.amount > 1;
                amountText.text = showCount ? slot.amount.ToString() : "";
                amountText.color = Color.white;
                amountText.enabled = showCount;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        UI_ItemTooltip.Hide();

        if (_uiInventory == null || _uiInventory.Inventory == null) return;
        if (_slotIndex >= _uiInventory.Inventory.Slots.Count) return;

        var slot = _uiInventory.Inventory.Slots[_slotIndex];
        if (slot == null || slot.IsEmpty || slot.item == null) return;

        if (icon != null)
            icon.color = new Color(1f, 1f, 1f, 0.4f);

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
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedSlot = eventData.pointerDrag?.GetComponent<UI_InventorySlot>();
        if (draggedSlot != null && _uiInventory != null)
        {
            _uiInventory.SwapOrMerge(draggedSlot._slotIndex, this._slotIndex);
        }
    }
}