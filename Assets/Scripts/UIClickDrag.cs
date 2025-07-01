using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Referência opcional ao WiringManager responsável (definida automaticamente)
    private WiringManager wiringManager;

    [SerializeField] private bool enableLogging = false;

    private bool isClicked = false;
    private bool isDragging = false;

    public bool IsClicked => isClicked;
    public bool IsDragging => isDragging;

    [Header("Configuração de Pino")]
    [Tooltip("Número do pino (1 a 8) para validação do cabo")]
    [Range(1,8)]
    public int pinNumber = 1;

    private void Awake()
    {
        // Tenta encontrar um WiringManager no pai e registrar este ponto.
        wiringManager = GetComponentInParent<WiringManager>();
        if (wiringManager != null)
        {
            wiringManager.RegisterDraggablePoint(this);
        }
    }

    private void OnDestroy()
    {
        if (wiringManager != null)
        {
            wiringManager.UnregisterDraggablePoint(this);
        }
    }

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