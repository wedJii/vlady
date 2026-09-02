using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Chest : MonoBehaviour
{
    public static UI_Chest Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private UI_Inventory chestUIInventory;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    private Chest _currentChest;
    private CanvasGroup _canvasGroup;
    private Tween _scaleTween;
    private Tween _fadeTween;
    private Vector3 _baseScale;
    private bool _isOpen;

    private float _openTime;

    public bool IsOpen => _isOpen;
    public Chest CurrentChest => _currentChest;
    public UI_Inventory ChestUIInventory => chestUIInventory;

    private CanvasGroup CanvasGroup => _canvasGroup ??= (chestPanel != null 
        ? (chestPanel.GetComponent<CanvasGroup>() ?? chestPanel.AddComponent<CanvasGroup>()) 
        : (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>()));

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (chestPanel == null) chestPanel = gameObject;

        _baseScale = chestPanel.transform.localScale == Vector3.zero ? Vector3.one : chestPanel.transform.localScale;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        SetClosedVisualsImmediate();
    }

    private void Update()
    {
        if (!_isOpen) return;

        // Даем небольшую задержку 0.12 сек, чтобы нажатие E при открытии не закрыло сундук мгновенно
        if (Time.unscaledTime - _openTime < 0.12f) return;

        bool closePressed = false;

        // New Input System
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || 
                                        Keyboard.current.escapeKey.wasPressedThisFrame || 
                                        Keyboard.current.tabKey.wasPressedThisFrame))
        {
            closePressed = true;
        }

        // Legacy Input
        if (!closePressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
                    closePressed = true;
            }
            catch { }
        }

        if (closePressed)
        {
            Close();
        }
    }

    private void SetClosedVisualsImmediate()
    {
        _isOpen = false;
        CanvasGroup.alpha = 0f;
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        chestPanel.transform.localScale = _baseScale * 0.7f;
    }

    public static UI_Chest GetOrCreate()
    {
        if (Instance != null) return Instance;

        var found = FindFirstObjectByType<UI_Chest>();
        if (found != null) return found;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[UI_Chest] Не найден Canvas в сцене!");
            return null;
        }

        var rootTr = canvas.transform;
        var chestGo = new GameObject("ChestPanel_Auto", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(UI_Chest));
        chestGo.transform.SetParent(rootTr, false);

        // Располагаем сундук слева (x = -195, y = 0)
        var rt = chestGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-195f, 0f);
        rt.sizeDelta = new Vector2(380f, 320f);

        var bg = chestGo.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        bg.type = Image.Type.Sliced;

        // Title
        var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(chestGo.transform, false);
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(15f, -14f);
        titleRt.sizeDelta = new Vector2(250f, 28f);

        var tmp = titleGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "<b><color=#E5C07B>СУНДУК</color></b>";
        tmp.fontSize = 15;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;

        // Close Button
        var btnGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(chestGo.transform, false);
        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 1f);
        btnRt.anchorMax = new Vector2(1f, 1f);
        btnRt.pivot = new Vector2(1f, 1f);
        btnRt.anchoredPosition = new Vector2(-12f, -12f);
        btnRt.sizeDelta = new Vector2(26f, 26f);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.75f, 0.25f, 0.25f, 0.9f);

        var btnTxtGo = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtGo.transform.SetParent(btnGo.transform, false);
        var btnTxtRt = btnTxtGo.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero;
        btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.sizeDelta = Vector2.zero;
        var btnTmp = btnTxtGo.GetComponent<TextMeshProUGUI>();
        btnTmp.text = "✕";
        btnTmp.fontSize = 14;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.white;
        btnTmp.raycastTarget = false;

        // Grid container for slots
        var gridGo = new GameObject("ChestSlotsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(chestGo.transform, false);
        var gridRt = gridGo.GetComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0.5f, 0.5f);
        gridRt.anchorMax = new Vector2(0.5f, 0.5f);
        gridRt.pivot = new Vector2(0.5f, 0.5f);
        gridRt.anchoredPosition = new Vector2(0f, -15f);
        gridRt.sizeDelta = new Vector2(360f, 240f);

        var grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.spacing = new Vector2(10f, 10f);
        grid.padding = new RectOffset(0, 0, 0, 0);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        // UI_Inventory on chest
        var uiInv = chestGo.AddComponent<UI_Inventory>();

        var playerUI = UI_Inventory.Instance;
        var slotPrefabField = typeof(UI_Inventory).GetField("slotPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerSlotPrefab = slotPrefabField?.GetValue(playerUI) as GameObject;
        if (playerSlotPrefab != null)
            slotPrefabField?.SetValue(uiInv, playerSlotPrefab);

        var slotsParentField = typeof(UI_Inventory).GetField("slotsParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        slotsParentField?.SetValue(uiInv, gridGo.transform);

        var startOpenField = typeof(UI_Inventory).GetField("startOpen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        startOpenField?.SetValue(uiInv, true);

        var uiChest = chestGo.GetComponent<UI_Chest>();
        uiChest.chestPanel = chestGo;
        uiChest.chestUIInventory = uiInv;
        uiChest.titleText = tmp;
        uiChest.closeButton = btnGo.GetComponent<Button>();
        uiChest.closeButton.onClick.AddListener(uiChest.Close);

        return uiChest;
    }

    public void Open(Chest chest)
    {
        if (chest == null) return;
        _currentChest = chest;
        _isOpen = true;
        _openTime = Time.unscaledTime;

        EnsureSetup();

        // 1. Связываем с инвентарем игрока и сдвигаем инвентарь игрока вправо (x = 195, y = 0)
        var playerUI = UI_Inventory.Instance;
        if (playerUI != null)
        {
            playerUI.SetLinkedTransferUI(chestUIInventory);
            if (chestUIInventory != null)
                chestUIInventory.SetLinkedTransferUI(playerUI);

            playerUI.ShiftPosition(new Vector2(195f, 0f), 0.2f);
            playerUI.Open();
        }

        // 2. Привязываем инвентарь сундука
        if (chestUIInventory != null)
        {
            chestUIInventory.SetInventory(chest.ChestInventory);
            chestUIInventory.Open();
        }

        // 3. Анимируем появление панели сундука слева
        _scaleTween?.Kill();
        _fadeTween?.Kill();
        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.interactable = true;
        _scaleTween = chestPanel.transform.DOScale(_baseScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);

        chest.OnOpened();
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;

        // 1. Отвязываем и возвращаем инвентарь игрока в центр (0, 0)
        var playerUI = UI_Inventory.Instance;
        if (playerUI != null)
        {
            playerUI.SetLinkedTransferUI(null);
            playerUI.ResetPosition(0.2f);
            playerUI.Close();
        }

        if (chestUIInventory != null)
        {
            chestUIInventory.SetLinkedTransferUI(null);
        }

        // 2. Анимируем исчезновение панели сундука
        _scaleTween?.Kill();
        _fadeTween?.Kill();
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        UI_ItemTooltip.Hide();
        _scaleTween = chestPanel.transform.DOScale(_baseScale * 0.7f, 0.15f).SetEase(Ease.InBack).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);

        if (_currentChest != null)
        {
            _currentChest.OnClosed();
            _currentChest = null;
        }
    }

    private void EnsureSetup()
    {
        if (chestPanel == null) chestPanel = gameObject;
        if (chestUIInventory == null) chestUIInventory = GetComponentInChildren<UI_Inventory>();
        if (_baseScale == Vector3.zero) _baseScale = Vector3.one;

        // Гарантируем корректное расположение слева (x = -195, y = 0)
        if (chestPanel != null && chestPanel.TryGetComponent(out RectTransform rt))
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-195f, 0f);
            rt.sizeDelta = new Vector2(380f, 320f);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _scaleTween?.Kill();
        _fadeTween?.Kill();
    }
}
