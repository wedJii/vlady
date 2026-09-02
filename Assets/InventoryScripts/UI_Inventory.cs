using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Inventory : MonoBehaviour
{
    public static UI_Inventory Instance { get; private set; }

    [Header("Inventory Model")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject slotPrefab;

    [Header("Slot Containers")]
    [SerializeField] private Transform slotsParent;

    [Header("Behavior Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool startOpen = false;
    [SerializeField] private UI_Inventory linkedTransferUI;

    private readonly List<UI_InventorySlot> _uiSlots = new();
    private readonly List<UI_InventorySlot> _uiPlateSlots = new();

    private static Image _dragGhostImage;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Vector3 _baseScale;
    private bool _isOpen;
    private Tween _scaleTween;
    private Tween _fadeTween;

    private RectTransform _rectTransform;
    private RectTransform RectTr => _rectTransform ??= GetComponent<RectTransform>();
    private Vector2 _baseAnchoredPosition;
    private Tween _posTween;

    public Inventory Inventory => inventory;
    public Canvas Canvas => _canvas;
    public bool IsOpen => _isOpen;
    public UI_Inventory LinkedTransferUI => linkedTransferUI;

    private CanvasGroup CanvasGroup => _canvasGroup ??= (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>());

    private void Awake()
    {
        if (Instance == null && !startOpen) Instance = this;
        _canvas = GetComponentInParent<Canvas>();
        EnsureDragGhost();

        _baseScale = transform.localScale;
        if (_baseScale == Vector3.zero) _baseScale = Vector3.one;
        _baseAnchoredPosition = RectTr.anchoredPosition;

        _isOpen = startOpen;
        if (_isOpen)
        {
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
            transform.localScale = _baseScale;
        }
        else
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
            transform.localScale = _baseScale * 0.7f;
        }
    }

    private void Start()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>() ?? FindFirstObjectByType<Inventory>();

        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
            inventory.OnInventoryChanged += UpdateUI;
        }

        InitUI();
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
            inventory.OnInventoryChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateUI;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _scaleTween?.Kill();
        _fadeTween?.Kill();
        _posTween?.Kill();
    }

    private void Update()
    {
        if (startOpen) return;

        // Если открыт сундук — управление закрытием сундука берет на себя UI_Chest
        if (UI_Chest.Instance != null && UI_Chest.Instance.IsOpen)
            return;

        bool togglePressed = false;

        // New Input System
        if (Keyboard.current != null && (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame))
        {
            togglePressed = true;
        }

        // Legacy Input fallback
        if (!togglePressed)
        {
            try
            {
                if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
                    togglePressed = true;
            }
            catch { }
        }

        if (togglePressed)
        {
            ToggleInventory();
        }
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        _scaleTween?.Kill();
        _fadeTween?.Kill();

        if (_baseScale == Vector3.zero)
            _baseScale = Vector3.one;

        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.interactable = true;
        _scaleTween = transform.DOScale(_baseScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        UpdateUI();
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _scaleTween?.Kill();
        _fadeTween?.Kill();

        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        UI_ItemTooltip.Hide();
        _scaleTween = transform.DOScale(_baseScale * 0.7f, 0.15f).SetEase(Ease.InBack).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);
    }

    public void ToggleInventory()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void ShiftPosition(Vector2 targetPos, float duration = 0.2f)
    {
        _posTween?.Kill();
        _posTween = RectTr.DOAnchorPos(targetPos, duration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    public void ResetPosition(float duration = 0.2f)
    {
        _posTween?.Kill();
        _posTween = RectTr.DOAnchorPos(_baseAnchoredPosition, duration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    public void SetInventory(Inventory newInv)
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateUI;

        inventory = newInv;

        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
            inventory.OnInventoryChanged += UpdateUI;
        }

        InitUI();
    }

    public void SetLinkedTransferUI(UI_Inventory linkedUI) => linkedTransferUI = linkedUI;

    private void EnsureDragGhost()
    {
        if (_dragGhostImage != null) return;

        var ghostObj = new GameObject("GlobalDragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        var rootCanvas = _canvas != null ? _canvas.rootCanvas : FindFirstObjectByType<Canvas>();
        ghostObj.transform.SetParent(rootCanvas != null ? rootCanvas.transform : transform.root, false);

        _dragGhostImage = ghostObj.GetComponent<Image>();
        _dragGhostImage.raycastTarget = false;

        var cg = ghostObj.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        ghostObj.SetActive(false);
    }

    public void ShowDragGhost(Sprite sprite, Vector2 size)
    {
        EnsureDragGhost();
        if (_dragGhostImage == null) return;

        _dragGhostImage.sprite = sprite;
        _dragGhostImage.color = Color.white;
        _dragGhostImage.rectTransform.sizeDelta = size;
        _dragGhostImage.gameObject.SetActive(true);
        _dragGhostImage.transform.SetAsLastSibling();
        UpdateDragGhostPosition(Input.mousePosition);
    }

    public void UpdateDragGhostPosition(Vector2 position)
    {
        if (_dragGhostImage != null && _dragGhostImage.gameObject.activeSelf)
            _dragGhostImage.transform.position = position;
    }

    public void HideDragGhost()
    {
        if (_dragGhostImage != null)
            _dragGhostImage.gameObject.SetActive(false);
    }

    public void InitUI()
    {
        if (inventory == null) return;

        // Удаляем любую старую внешнюю панель, если она осталась
        var oldPanel = transform.Find("AutoPlatePanel");
        if (oldPanel != null)
        {
            if (Application.isPlaying) Destroy(oldPanel.gameObject);
            else DestroyImmediate(oldPanel.gameObject);
        }

        _uiSlots.Clear();
        _uiPlateSlots.Clear();

        var container = slotsParent != null ? slotsParent : transform;
        var allSlots = new List<UI_InventorySlot>();
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            if (child.name.Contains("AutoPlatePanel")) continue;
            var s = child.GetComponent<UI_InventorySlot>() ?? child.gameObject.AddComponent<UI_InventorySlot>();
            allSlots.Add(s);
        }

        int total = inventory.Capacity + inventory.PlateCapacity;
        while (allSlots.Count < total && slotPrefab != null)
        {
            var slotObj = Instantiate(slotPrefab, container);
            var uiSlot = slotObj.GetComponent<UI_InventorySlot>() ?? slotObj.AddComponent<UI_InventorySlot>();
            allSlots.Add(uiSlot);
        }

        // Если инвентарь со слотами под пластины (в Dungeon 1)
        if (inventory.PlateCapacity > 0 && allSlots.Count >= 18)
        {
            // Первые 3 ряда (15 ячеек) — обычный рюкзак
            inventory.capacity = 15;
            inventory.plateCapacity = 3;

            for (int i = 0; i < 15; i++)
            {
                allSlots[i].gameObject.SetActive(true);
                allSlots[i].Init(this, i, false);
                _uiSlots.Add(allSlots[i]);
            }

            // 4-й ряд (ячейки 15, 16, 17) — 3 слота под пластины
            for (int i = 0; i < 3; i++)
            {
                int slotIdx = 15 + i;
                allSlots[slotIdx].gameObject.SetActive(true);
                allSlots[slotIdx].Init(this, i, true);
                _uiPlateSlots.Add(allSlots[slotIdx]);
            }

            // Скрываем оставшиеся 2 ячейки 4-го ряда (18, 19)
            for (int i = 18; i < allSlots.Count; i++)
            {
                allSlots[i].gameObject.SetActive(false);
            }

            // Маленькая аккуратная надпись «ПЛАСТИНЫ» над 4-м рядом
            var headerObj = transform.Find("PlateHeaderLabel");
            if (headerObj == null)
            {
                var go = new GameObject("PlateHeaderLabel", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
                go.transform.SetParent(transform, false);

                var le = go.GetComponent<LayoutElement>();
                le.ignoreLayout = true;

                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (_uiSlots.Count > 0 && _uiSlots[0] != null && _uiSlots[0].AmountText != null && _uiSlots[0].AmountText.font != null)
                    tmp.font = _uiSlots[0].AmountText.font;

                tmp.text = "<b><color=#4ECDC4>ПЛАСТИНЫ</color></b>";
                tmp.fontSize = 11;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;

                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 85f);
                rt.sizeDelta = new Vector2(200f, 18f);
            }
        }
        else
        {
            var headerObj = transform.Find("PlateHeaderLabel");
            if (headerObj != null)
            {
                if (Application.isPlaying) Destroy(headerObj.gameObject);
                else DestroyImmediate(headerObj.gameObject);
            }

            // Для обычных инвентарей без пластин (в сундуках и viborDungeon)
            for (int i = 0; i < allSlots.Count; i++)
            {
                if (i < inventory.Capacity)
                {
                    allSlots[i].gameObject.SetActive(true);
                    allSlots[i].Init(this, i, false);
                    _uiSlots.Add(allSlots[i]);
                }
                else
                {
                    allSlots[i].gameObject.SetActive(false);
                }
            }
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (inventory == null) return;

        for (int i = 0; i < _uiSlots.Count; i++)
        {
            if (i < inventory.Slots.Count)
                _uiSlots[i].Render(inventory.Slots[i]);
        }

        for (int i = 0; i < _uiPlateSlots.Count; i++)
        {
            if (i < inventory.PlateSlots.Count)
                _uiPlateSlots[i].Render(inventory.PlateSlots[i]);
        }
    }

    public void TransferOrSwap(UI_Inventory fromUI, int fromIndex, bool fromIsPlate, int toIndex, bool toIsPlate)
    {
        if (fromUI == null || fromUI.Inventory == null || this.Inventory == null) return;

        var player = FindFirstObjectByType<PlayerControll>();

        if (fromUI == this)
        {
            inventory.MoveOrMerge(fromIndex, fromIsPlate, toIndex, toIsPlate, player);
            UpdateUI();
        }
        else
        {
            fromUI.Inventory.TransferTo(fromIndex, fromIsPlate, this.Inventory, toIndex, toIsPlate, player);
            fromUI.UpdateUI();
            this.UpdateUI();
        }
    }

    public void QuickTransfer(int slotIndex, bool isPlate)
    {
        var player = FindFirstObjectByType<PlayerControll>();

        // 1. Клик ПКМ по пластине в верхних 3 рядах -> быстро надеваем в 4-й ряд (слот пластин)
        if (!isPlate)
        {
            var slotList = inventory.Slots;
            if (slotIndex >= 0 && slotIndex < slotList.Count)
            {
                var slot = slotList[slotIndex];
                if (!slot.IsEmpty && slot.item is UbgradePlate plateItem)
                {
                    for (int i = 0; i < inventory.PlateSlots.Count; i++)
                    {
                        if (inventory.PlateSlots[i].IsEmpty)
                        {
                            inventory.PlateSlots[i].Set(plateItem, 1);
                            if (player != null) plateItem.OnApply(player);
                            slot.Clear();
                            UpdateUI();
                            return;
                        }
                    }
                }
            }
        }
        // 2. Клик ПКМ по пластине в 4-м ряду (слоте пластин) -> быстро снимаем в верхние 3 ряда
        else
        {
            var plateList = inventory.PlateSlots;
            if (slotIndex >= 0 && slotIndex < plateList.Count)
            {
                var slot = plateList[slotIndex];
                if (!slot.IsEmpty)
                {
                    var item = slot.item;
                    if (inventory.AddItem(item, 1))
                    {
                        if (item is UbgradePlate p && player != null) p.OnRemove(player);
                        slot.Clear();
                        UpdateUI();
                        return;
                    }
                }
            }
        }

        if (linkedTransferUI != null && linkedTransferUI.Inventory != null && inventory != null)
        {
            inventory.QuickTransfer(slotIndex, isPlate, linkedTransferUI.Inventory, player);
            UpdateUI();
            linkedTransferUI.UpdateUI();
        }
    }

    public void TransferOrSwap(UI_Inventory fromUI, int fromIndex, int toIndex) =>
        TransferOrSwap(fromUI, fromIndex, false, toIndex, false);

    public void QuickTransfer(int slotIndex) =>
        QuickTransfer(slotIndex, false);
}