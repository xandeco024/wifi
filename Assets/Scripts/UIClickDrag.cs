using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private bool enableLogging = false;

    private bool isClicked = false;
    private bool isDragging = false;

    public bool IsClicked => isClicked;
    public bool IsDragging => isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        isClicked = true;
        if (enableLogging) Debug.Log($"Click iniciado em: {gameObject.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (enableLogging) Debug.Log($"Arrastando: {gameObject.name}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isClicked = false;
        isDragging = false;
        if (enableLogging) Debug.Log($"Click finalizado em: {gameObject.name}");
    }
} 