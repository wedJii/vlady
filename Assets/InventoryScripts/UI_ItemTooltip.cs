using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ItemTooltip : MonoBehaviour
{
    public static UI_ItemTooltip Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Vector2 _offset = new(18f, -18f);

    private CanvasGroup _canvasGroup;
    private CanvasGroup CanvasGroup => _canvasGroup ??= (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>());

    private void Awake()
    {
        Instance = this;
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;
        if (_titleText == null) _titleText = GetComponentInChildren<TextMeshProUGUI>();
        Hide();
    }

    private void Update()
    {
        if (CanvasGroup != null && CanvasGroup.alpha > 0f)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 mousePos = Input.mousePosition;
        float screenW = Screen.width;
        float screenH = Screen.height;

        var rt = transform as RectTransform;
        float width = rt != null ? rt.sizeDelta.x : 210f;
        float height = rt != null ? rt.sizeDelta.y : 65f;

        float posX = mousePos.x + _offset.x;
        float posY = mousePos.y + _offset.y;

        // Защита от вылета за границы экрана
        if (posX + width > screenW) posX = mousePos.x - width - 10f;
        if (posY - height < 0) posY = mousePos.y + height + 10f;

        transform.position = new Vector3(posX, posY, 0f);
    }

    public static void Show(Item item)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        EnsureInstance();
        if (Instance == null) return;

        // Проверяем, что тултип висит на правильном ScreenSpaceOverlay канвасе
        Canvas targetCanvas = GetTargetCanvas();
        if (targetCanvas != null && Instance.transform.parent != targetCanvas.transform)
        {
            Instance.transform.SetParent(targetCanvas.transform, false);
        }

        if (Instance._titleText != null)
        {
            string desc = string.IsNullOrEmpty(item.description) ? "" : $"\n<size=85%><color=#E0E0E0>{item.description}</color></size>";
            Instance._titleText.text = $"<b><color=#FFE600>{item.displayName}</color></b>{desc}";
        }

        Instance.transform.SetAsLastSibling();
        Instance.CanvasGroup.alpha = 1f;
        Instance.UpdatePosition();
    }

    public static void Hide()
    {
        if (Instance != null && Instance.CanvasGroup != null)
            Instance.CanvasGroup.alpha = 0f;
    }

    private static Canvas GetTargetCanvas()
    {
        if (UI_Inventory.Instance != null && UI_Inventory.Instance.Canvas != null)
        {
            return UI_Inventory.Instance.Canvas.rootCanvas != null 
                   ? UI_Inventory.Instance.Canvas.rootCanvas 
                   : UI_Inventory.Instance.Canvas;
        }

        var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            if (c != null && c.isActiveAndEnabled && c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c.rootCanvas != null ? c.rootCanvas : c;
        }

        return null;
    }

    private static void EnsureInstance()
    {
        if (Instance != null) return;

        var targetCanvas = GetTargetCanvas();
        if (targetCanvas == null) return;

        var root = new GameObject("UI_ItemTooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(UI_ItemTooltip));
        root.transform.SetParent(targetCanvas.transform, false);

        var bg = root.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.10f, 0.14f, 0.96f);
        bg.raycastTarget = false;

        var existingImg = targetCanvas.GetComponentInChildren<Image>();
        if (existingImg != null && existingImg.sprite != null)
        {
            bg.sprite = existingImg.sprite;
            bg.type = Image.Type.Sliced;
        }

        var textObj = new GameObject("TooltipText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(root.transform, false);

        var rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10, 6);
        rt.offsetMax = new Vector2(-10, -6);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        
        var anyTmp = targetCanvas.GetComponentInChildren<TextMeshProUGUI>();
        if (anyTmp != null && anyTmp.font != null)
            tmp.font = anyTmp.font;

        tmp.fontSize = 13;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 9;
        tmp.fontSizeMax = 13;
        tmp.enableWordWrapping = true;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(210, 65);
        rootRt.pivot = new Vector2(0, 1);

        Instance = root.GetComponent<UI_ItemTooltip>();
        Instance._titleText = tmp;
        Hide();
    }
}
