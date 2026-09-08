using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Chest : MonoBehaviour
{
    public static UI_Chest Instance { get; private set; }
    public static float LastCloseTime { get; private set; }

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
        if (chestUIInventory == null) chestUIInventory = GetComponentInChildren<UI_Inventory>(true);

        _baseScale = chestPanel.transform.localScale == Vector3.zero ? Vector3.one : chestPanel.transform.localScale;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        SetClosedVisualsImmediate();
    }

    private void Update()
    {
        if (!_isOpen) return;
        if (Time.unscaledTime - _openTime < 0.15f) return;

        bool closePressed = false;
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || 
                                        Keyboard.current.escapeKey.wasPressedThisFrame || 
                                        Keyboard.current.tabKey.wasPressedThisFrame))
        {
            closePressed = true;
        }

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

        var found = FindFirstObjectByType<UI_Chest>(FindObjectsInactive.Include);
        if (found != null)
        {
            Instance = found;
            return found;
        }

        var chestPrefab = Resources.Load<GameObject>("PrefabsInventory/ChestPanel") ??
                          Resources.Load<GameObject>("ChestPanel");
        if (chestPrefab != null)
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                var inst = Instantiate(chestPrefab, canvas.transform);
                var ui = inst.GetComponent<UI_Chest>();
                if (ui != null)
                {
                    Instance = ui;
                    return ui;
                }
            }
        }

        return null;
    }

    public void Open(Chest chest)
    {
        if (chest == null) return;
        _currentChest = chest;
        _isOpen = true;
        _openTime = Time.unscaledTime;

        if (chestPanel == null) chestPanel = gameObject;
        if (chestUIInventory == null) chestUIInventory = GetComponentInChildren<UI_Inventory>(true);

        var playerUI = UI_Inventory.Instance;
        if (playerUI != null)
        {
            playerUI.SetLinkedTransferUI(chestUIInventory);
            if (chestUIInventory != null)
                chestUIInventory.SetLinkedTransferUI(playerUI);

            playerUI.ShiftPosition(new Vector2(165f, 0f), 0.2f);
            playerUI.Open();
        }

        if (chestUIInventory != null)
        {
            chestUIInventory.SetInventory(chest.ChestInventory);
            chestUIInventory.Open();
        }

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
        LastCloseTime = Time.unscaledTime;

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

        _scaleTween?.Kill();
        _fadeTween?.Kill();
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        UI_ItemTooltip.Hide();
        _scaleTween = chestPanel.transform.DOScale(_baseScale * 0.7f, 0.15f).SetEase(Ease.InBack).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);

        if (_currentChest != null)
        {
            var c = _currentChest;
            _currentChest = null;
            c.OnClosed();
        }

        
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _scaleTween?.Kill();
        _fadeTween?.Kill();
    }
}