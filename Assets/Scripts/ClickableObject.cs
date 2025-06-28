using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private string objectName = "";
    
    [Header("Interaction Settings")]
    [SerializeField] private bool canBeClicked = true;
    [SerializeField] private bool canBeDragged = true;
    [SerializeField] private bool canBeHovered = true;
    
    [Header("Unity Events (Optional)")]
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;
    public UnityEvent OnClickDown;
    public UnityEvent OnClickUp;
    public UnityEvent OnClick;
    public UnityEvent OnDragStart;
    public UnityEvent OnDragEnd;
    
    // Estados públicos para outros scripts acessarem
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool IsDragging { get; private set; }
    public bool IsClicked { get; private set; }
    
    // Events em C# para scripts que preferem usar Actions
    public static event Action<ClickableObject> OnAnyObjectHoverEnter;
    public static event Action<ClickableObject> OnAnyObjectHoverExit;
    public static event Action<ClickableObject> OnAnyObjectClick;
    public static event Action<ClickableObject> OnAnyObjectDragStart;
    public static event Action<ClickableObject> OnAnyObjectDragEnd;
    
    // Events específicos deste objeto
    public event Action<ClickableObject> OnThisObjectHoverEnter;
    public event Action<ClickableObject> OnThisObjectHoverExit;
    public event Action<ClickableObject> OnThisObjectClick;
    public event Action<ClickableObject> OnThisObjectDragStart;
    public event Action<ClickableObject> OnThisObjectDragEnd;
    public event Action<ClickableObject, Vector2> OnThisObjectDrag;
    
    // Propriedades úteis para UI
    public Vector2 MousePosition => Input.mousePosition;
    public Vector2 LocalMousePosition { get; private set; }
    public RectTransform RectTransform { get; private set; }
    
    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        
        // Se não foi definido um nome personalizado, usa o nome do GameObject
        if (string.IsNullOrEmpty(objectName))
            objectName = gameObject.name;
    }
    
    void Start()
    {
        // Reset todos os estados no início
        ResetStates();
    }
    
    void Update()
    {
        // Atualiza posição local do mouse em coordenadas UI
        UpdateLocalMousePosition();
    }
    
    private void UpdateLocalMousePosition()
    {
        if (RectTransform != null)
        {
            // Tenta encontrar o canvas automaticamente
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = canvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas?.worldCamera;
            
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, uiCamera, out localPos))
            {
                LocalMousePosition = localPos;
            }
        }
    }
    
    #region Pointer Events
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!canBeHovered) return;
        
        IsHovered = true;
        DebugLog("hovered (enter)");
        
        // Disparar eventos
        OnHoverEnter?.Invoke();
        OnAnyObjectHoverEnter?.Invoke(this);
        OnThisObjectHoverEnter?.Invoke(this);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!canBeHovered) return;
        
        IsHovered = false;
        DebugLog("hovered (exit)");
        
        // Disparar eventos
        OnHoverExit?.Invoke();
        OnAnyObjectHoverExit?.Invoke(this);
        OnThisObjectHoverExit?.Invoke(this);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canBeClicked) return;
        
        IsPressed = true;
        DebugLog("pressed down");
        
        // Disparar eventos
        OnClickDown?.Invoke();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!canBeClicked) return;
        
        IsPressed = false;
        DebugLog("released");
        
        // Disparar eventos
        OnClickUp?.Invoke();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canBeClicked) return;
        
        IsClicked = true;
        DebugLog("clicked");
        
        // Disparar eventos
        OnClick?.Invoke();
        OnAnyObjectClick?.Invoke(this);
        OnThisObjectClick?.Invoke(this);
        
        // Reset click state após um frame
        StartCoroutine(ResetClickedState());
    }
    
    #endregion
    
    #region Drag Events
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canBeDragged) return;
        
        IsDragging = true;
        DebugLog("drag started");
        
        // Disparar eventos
        OnDragStart?.Invoke();
        OnAnyObjectDragStart?.Invoke(this);
        OnThisObjectDragStart?.Invoke(this);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!canBeDragged || !IsDragging) return;
        
        DebugLog($"dragging to {eventData.position}");
        
        // Disparar evento com posição
        OnThisObjectDrag?.Invoke(this, eventData.position);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canBeDragged) return;
        
        IsDragging = false;
        DebugLog("drag ended");
        
        // Disparar eventos
        OnDragEnd?.Invoke();
        OnAnyObjectDragEnd?.Invoke(this);
        OnThisObjectDragEnd?.Invoke(this);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Reseta todos os estados de interação
    /// </summary>
    public void ResetStates()
    {
        IsHovered = false;
        IsPressed = false;
        IsDragging = false;
        IsClicked = false;
    }
    
    /// <summary>
    /// Ativa/desativa tipos específicos de interação
    /// </summary>
    public void SetInteractionEnabled(bool click, bool drag, bool hover)
    {
        canBeClicked = click;
        canBeDragged = drag;
        canBeHovered = hover;
        
        DebugLog($"interaction settings changed - Click: {click}, Drag: {drag}, Hover: {hover}");
    }
    
    /// <summary>
    /// Força um estado específico (útil para testes ou situações especiais)
    /// </summary>
    public void ForceState(bool hovered, bool pressed, bool dragging)
    {
        IsHovered = hovered;
        IsPressed = pressed;
        IsDragging = dragging;
        
        DebugLog($"state forced - Hovered: {hovered}, Pressed: {pressed}, Dragging: {dragging}");
    }
    
    #endregion
    
    #region Utility Methods
    
    private void DebugLog(string action)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ClickableObject] {objectName} {action}");
        }
    }
    
    private System.Collections.IEnumerator ResetClickedState()
    {
        yield return null; // Espera um frame
        IsClicked = false;
    }
    
    /// <summary>
    /// Verifica se este objeto é o que está sendo draggado atualmente
    /// </summary>
    public bool IsCurrentlyDragging()
    {
        return IsDragging;
    }
    
    /// <summary>
    /// Retorna informações de debug sobre o estado atual
    /// </summary>
    public string GetStateInfo()
    {
        return $"{objectName} - Hovered: {IsHovered}, Pressed: {IsPressed}, Dragging: {IsDragging}, Clicked: {IsClicked}";
    }
    
    #endregion
    
    // Método chamado quando o objeto é destruído para limpar eventos
    void OnDestroy()
    {
        OnAnyObjectHoverEnter = null;
        OnAnyObjectHoverExit = null;
        OnAnyObjectClick = null;
        OnAnyObjectDragStart = null;
        OnAnyObjectDragEnd = null;
    }
} 