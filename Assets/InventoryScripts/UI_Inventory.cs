using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Inventory : MonoBehaviour
{
    public static UI_Inventory Instance { get; private set; }

    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool startOpen = false;
    [SerializeField] private UI_Inventory linkedTransferUI;

    private readonly List<UI_InventorySlot> _uiSlots = new();
    private static Image _dragGhostImage;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Vector3 _baseScale;
    private bool _isOpen;
    private Tween _scaleTween;
    private Tween _fadeTween;

    public Inventory Inventory => inventory;
    public Canvas Canvas => _canvas;
    public bool IsOpen => _isOpen;
    public UI_Inventory LinkedTransferUI => linkedTransferUI;

    private CanvasGroup CanvasGroup => _canvasGroup ??= (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>());

    private void Awake()
    {
        Instance = this;
        _canvas = GetComponentInParent<Canvas>();
        EnsureDragGhost();

        _baseScale = transform.localScale;
        if (_baseScale == Vector3.zero) _baseScale = Vector3.one;

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
    }

    private void Update()
    {
        if (startOpen) return;

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

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        _scaleTween?.Kill();
        _fadeTween?.Kill();

        if (_baseScale == Vector3.zero)
            _baseScale = Vector3.one;

        if (_isOpen)
        {
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
            _scaleTween = transform.DOScale(_baseScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            _fadeTween = CanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
            UpdateUI();
        }
        else
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
            UI_ItemTooltip.Hide();
            _scaleTween = transform.DOScale(_baseScale * 0.7f, 0.15f).SetEase(Ease.InBack).SetUpdate(true);
            _fadeTween = CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);
        }
    }

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
        if (inventory == null || slotsParent == null) return;

        _uiSlots.Clear();

        var existingSlots = slotsParent.GetComponentsInChildren<UI_InventorySlot>(true);
        if (existingSlots.Length > 0)
        {
            for (int i = 0; i < existingSlots.Length; i++)
            {
                existingSlots[i].Init(this, i);
                _uiSlots.Add(existingSlots[i]);
            }
        }
        else if (slotPrefab != null)
        {
            for (int i = 0; i < inventory.Capacity; i++)
            {
                var slotObj = Instantiate(slotPrefab, slotsParent);
                var uiSlot = slotObj.GetComponent<UI_InventorySlot>() ?? slotObj.AddComponent<UI_InventorySlot>();
                uiSlot.Init(this, i);
                _uiSlots.Add(uiSlot);
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
    }

    public void TransferOrSwap(UI_Inventory fromUI, int fromIndex, int toIndex)
    {
        if (fromUI == null || fromUI.Inventory == null || this.Inventory == null) return;

        if (fromUI == this)
        {
            inventory.MoveOrMerge(fromIndex, toIndex);
            UpdateUI();
        }
        else
        {
            fromUI.Inventory.TransferTo(fromIndex, this.Inventory, toIndex);
            fromUI.UpdateUI();
            this.UpdateUI();
        }
    }

    public void QuickTransfer(int slotIndex)
    {
        if (linkedTransferUI != null && linkedTransferUI.Inventory != null && inventory != null)
        {
            inventory.QuickTransfer(slotIndex, linkedTransferUI.Inventory);
            UpdateUI();
            linkedTransferUI.UpdateUI();
        }
    }
}