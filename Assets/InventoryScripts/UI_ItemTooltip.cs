using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ItemTooltip : MonoBehaviour
{
    public static UI_ItemTooltip Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Vector2 _offset = new(15f, -15f);

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
        if (CanvasGroup.alpha > 0f)
            transform.position = (Vector2)Input.mousePosition + _offset;
    }

    public static void Show(Item item)
    {
        if (item == null) return;
        EnsureInstance();
        if (Instance == null) return;

        if (Instance._titleText != null)
        {
            string desc = string.IsNullOrEmpty(item.description) ? "" : $"\n<size=80%><color=#CCCCCC>{item.description}</color></size>";
            Instance._titleText.text = $"<b>{item.displayName}</b>{desc}";
        }

        Instance.transform.SetAsLastSibling();
        Instance.CanvasGroup.alpha = 1f;
    }

    public static void Hide()
    {
        if (Instance != null)
            Instance.CanvasGroup.alpha = 0f;
    }

    private static void EnsureInstance()
    {
        if (Instance != null) return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var root = new GameObject("UI_ItemTooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(UI_ItemTooltip));
        root.transform.SetParent(canvas.transform, false);

        var bg = root.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
        bg.raycastTarget = false;

        var textObj = new GameObject("TooltipText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(root.transform, false);

        var rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10, 10);
        rt.offsetMax = new Vector2(-10, -10);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rootRt = root.GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(200, 70);
        rootRt.pivot = new Vector2(0, 1);

        Instance = root.GetComponent<UI_ItemTooltip>();
        Instance._titleText = tmp;
        Hide();
    }
}
