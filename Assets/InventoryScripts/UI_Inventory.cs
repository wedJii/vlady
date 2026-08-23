using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Inventory : MonoBehaviour
{
    public static UI_Inventory Instance { get; private set; }

    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool startOpen = true;

    private readonly List<UI_InventorySlot> _uiSlots = new();
    private Image _dragGhostImage;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private bool _isOpen = true;
    private Tween _scaleTween;
    private Tween _fadeTween;

    public Inventory Inventory => inventory;
    public Canvas Canvas => _canvas;

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
        Instance = this;
        _canvas = GetComponentInParent<Canvas>();
        CreateDragGhost();
    }

    private void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        InitUI();

        _isOpen = startOpen;
        if (!startOpen)
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
            transform.localScale = Vector3.one * 0.7f;
        }
        else
        {
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
            transform.localScale = Vector3.one;
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateUI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        _scaleTween?.Kill();
        _fadeTween?.Kill();

        if (_isOpen)
        {
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
            _scaleTween = transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            _fadeTween = CanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        }
        else
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
            UI_ItemTooltip.Hide();
            _scaleTween = transform.DOScale(0.7f, 0.15f).SetEase(Ease.InBack).SetUpdate(true);
            _fadeTween = CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);
        }
    }

    private void CreateDragGhost()
    {
        var ghostObj = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        ghostObj.transform.SetParent(_canvas != null ? _canvas.transform : transform.root, false);

        _dragGhostImage = ghostObj.GetComponent<Image>();
        _dragGhostImage.raycastTarget = false;

        var cg = ghostObj.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        ghostObj.SetActive(false);
    }

    public void ShowDragGhost(Sprite sprite, Vector2 size)
    {
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
        {
            _dragGhostImage.transform.position = position;
        }
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

    public void SwapOrMerge(int fromIndex, int toIndex)
    {
        if (inventory != null)
            inventory.MoveOrMerge(fromIndex, toIndex);
    }
}