using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// Этот скрипт вешается ПРЯМО НА КНОПКУ и сам отслеживает мышку
public class ButtonAnimationDOTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalSize;
    private float speed = 0.25f;
    private float scaleMultiplier = 1.1f;

    private void Awake()
    {
        // Запоминаем родной размер кнопки один раз при старте
        originalSize = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalSize * scaleMultiplier, speed);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalSize, speed);
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalSize;
    }
}    
    