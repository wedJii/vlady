using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_InventoryAnimator : MonoBehaviour
{
    [SerializeField] private float _animationDuration = 0.25f;
    [SerializeField] private Ease _openEase = Ease.OutBack;
    [SerializeField] private Ease _closeEase = Ease.InBack;

    private CanvasGroup _canvasGroup;
    private Tween _scaleTween;
    private Tween _fadeTween;
    private bool _isOpen = true;

    public bool IsOpen => _isOpen;

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
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Open()
    {
        _isOpen = true;
        _scaleTween?.Kill();
        _fadeTween?.Kill();

        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.interactable = true;

        transform.localScale = Vector3.one * 0.7f;
        _scaleTween = transform.DOScale(1f, _animationDuration).SetEase(_openEase).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(1f, _animationDuration).SetUpdate(true);
    }

    public void Close()
    {
        _isOpen = false;
        _scaleTween?.Kill();
        _fadeTween?.Kill();

        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;

        _scaleTween = transform.DOScale(0.7f, _animationDuration).SetEase(_closeEase).SetUpdate(true);
        _fadeTween = CanvasGroup.DOFade(0f, _animationDuration).SetUpdate(true);
    }

    public void Toggle()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }
}
