using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage;

    [Header("Slot Mode")]
    [SerializeField] private bool isPlateSlot = false;

    private int _slotIndex;
    private UI_Inventory _uiInventory;
    private CanvasGroup _canvasGroup;
    private Tween _hoverTween;
    [Header("Colors (Inspector)")]
    [SerializeField] private Color normalBgColor = new(0.16f, 0.18f, 0.24f, 0.95f);
    [SerializeField] private Color plateBgColor = new(0.12f, 0.28f, 0.38f, 0.98f);
    [SerializeField] private Color placeholderNormalColor = new(0.25f, 0.55f, 0.85f, 0.85f);
    [SerializeField] private Color placeholderPlateColor = new(0.25f, 0.75f, 0.85f, 0.85f);

    public UI_Inventory UIInventory => _uiInventory;
    public int SlotIndex => _slotIndex;
    public bool IsPlateSlot => isPlateSlot;
    public TextMeshProUGUI AmountText => amountText;

    private CanvasGroup CanvasGroup => _canvasGroup ??= (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>());

    private void Awake() => EnsureComponents();

    private void EnsureComponents()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (icon == null)
        {
            var iconTr = transform.Find("Icon");
            if (iconTr != null) icon = iconTr.GetComponent<Image>();
            if (icon == null) icon = GetComponentInChildren<Image>();
        }

        if (amountText == null)
            amountText = transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();

        if (amountText != null)
        {
            amountText.color = Color.white;
            amountText.faceColor = new Color32(255, 255, 255, 255);
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = 8;
            amountText.fontSizeMax = 12;
            amountText.alignment = TextAlignmentOptions.BottomRight;
            amountText.raycastTarget = false;
        }
    }

    public void Init(UI_Inventory uiInv, int index, bool isPlate = false)
    {
        EnsureComponents();
        _uiInventory = uiInv;
        _slotIndex = index;
        isPlateSlot = isPlate;

        if (backgroundImage != null)
        {
            backgroundImage.color = isPlateSlot ? plateBgColor : normalBgColor;
        }
    }

    public void Render(InventorySlot slot)
    {
        EnsureComponents();

        if (backgroundImage != null)
        {
            backgroundImage.color = isPlateSlot ? plateBgColor : normalBgColor;
        }

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
                    icon.color = isPlateSlot ? placeholderPlateColor : placeholderNormalColor;
                    icon.enabled = true;
                }
            }

            if (amountText != null)
            {
                string txt = slot.amount > 1 ? slot.amount.ToString() : "";
                amountText.text = txt;
                amountText.color = Color.white;
                amountText.faceColor = new Color32(255, 255, 255, 255);
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

        if (_uiInventory != null && _uiInventory.Inventory != null)
        {
            var list = _uiInventory.Inventory.GetSlotList(isPlateSlot);
            if (_slotIndex >= 0 && _slotIndex < list.Count)
            {
                var slot = list[_slotIndex];
                if (slot != null && !slot.IsEmpty && slot.item != null)
                {
                    UI_ItemTooltip.Show(slot.item);
                }
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
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _uiInventory?.QuickTransfer(_slotIndex, isPlateSlot);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        UI_ItemTooltip.Hide();

        if (_uiInventory == null || _uiInventory.Inventory == null) return;
        var list = _uiInventory.Inventory.GetSlotList(isPlateSlot);
        if (_slotIndex < 0 || _slotIndex >= list.Count) return;

        var slot = list[_slotIndex];
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

        if (draggedSlot != null && draggedSlot.UIInventory != null && draggedSlot.UIInventory.Inventory != null)
        {
            var sourceList = draggedSlot.UIInventory.Inventory.GetSlotList(draggedSlot.IsPlateSlot);
            if (draggedSlot.SlotIndex < 0 || draggedSlot.SlotIndex >= sourceList.Count) return;

            var draggedItem = sourceList[draggedSlot.SlotIndex]?.item;

            
            if (this.IsPlateSlot && draggedItem != null && !(draggedItem is UbgradePlate))
            {
                RejectShake();
                return;
            }

            _uiInventory.TransferOrSwap(
                draggedSlot.UIInventory, 
                draggedSlot.SlotIndex, 
                draggedSlot.IsPlateSlot, 
                this._slotIndex, 
                this.isPlateSlot
            );
        }
    }

    public void RejectShake()
    {
        transform.DOKill();
        transform.DOShakePosition(0.25f, new Vector3(8f, 0f, 0f), 15).SetUpdate(true);
    }
}