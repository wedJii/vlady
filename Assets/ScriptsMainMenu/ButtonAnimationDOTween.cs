using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalSize;

    private void Awake()
    {
        originalSize = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalSize * 1.1f, 0.25f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalSize, 0.25f).SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalSize;
    }
}