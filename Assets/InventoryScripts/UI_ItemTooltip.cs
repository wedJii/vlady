using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ItemTooltip : MonoBehaviour
{
    public static UI_ItemTooltip Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private Vector2 _offset = new(15f, -15f);

    private CanvasGroup _canvasGroup;

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
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;

        if (_titleText == null)
            _titleText = GetComponentInChildren<TextMeshProUGUI>();

        Hide();
    }

    private void Update()
    {
        if (CanvasGroup.alpha > 0f)
        {
            transform.position = (Vector2)Input.mousePosition + _offset;
        }
    }

    public static void Show(Item item)
    {
        if (Instance == null || item == null) return;

        if (Instance._titleText != null)
        {
            string desc = string.IsNullOrEmpty(item.description) ? "" : $"\n<size=75%><color=#BBBBBB>{item.description}</color></size>";
            Instance._titleText.text = $"<b>{item.displayName}</b>{desc}";
        }

        Instance.CanvasGroup.alpha = 1f;
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.CanvasGroup.alpha = 0f;
    }
}
