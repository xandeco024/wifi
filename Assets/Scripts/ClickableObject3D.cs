using UnityEngine;

/// <summary>
/// Versão 3D do ClickableObject - usa Physics raycast ao invés de UI EventSystem
/// Mantém a mesma interface de eventos para compatibilidade
/// </summary>
[RequireComponent(typeof(Collider))]
public class ClickableObject3D : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private bool clickEnabled = true;
    [SerializeField] private bool dragEnabled = false;
    [SerializeField] private bool hoverEnabled = true;
    
    [Header("3D Settings")]
    [SerializeField] private LayerMask raycastLayers = -1;
    [SerializeField] private float maxRaycastDistance = 100f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool forceDebugNextFrame = false;
    
    // Estados de interação
    private bool isHovered = false;
    private bool isPressed = false;
    private bool isDragging = false;
    private bool wasClicked = false;
    
    // Posições para drag
    private Vector3 dragStartWorldPosition;
    private Vector3 lastMouseWorldPosition;
    
    // Componentes
    private Camera mainCamera;
    private Collider objectCollider;
    
    // Eventos locais (este objeto específico)
    public System.Action<ClickableObject3D> OnThisObjectClick;
    public System.Action<ClickableObject3D> OnThisObjectHoverEnter;
    public System.Action<ClickableObject3D> OnThisObjectHoverExit;
    public System.Action<ClickableObject3D> OnThisObjectDragStart;
    public System.Action<ClickableObject3D> OnThisObjectDragEnd;
    
    // Eventos globais (qualquer objeto)
    public static System.Action<ClickableObject3D> OnAnyObjectClick;
    public static System.Action<ClickableObject3D> OnAnyObjectHoverEnter;
    public static System.Action<ClickableObject3D> OnAnyObjectHoverExit;
    public static System.Action<ClickableObject3D> OnAnyObjectDragStart;
    public static System.Action<ClickableObject3D> OnAnyObjectDragEnd;
    
    // Propriedades públicas
    public bool IsHovered => isHovered;
    public bool IsPressed => isPressed;
    public bool IsDragging => isDragging;
    public bool IsClicked => wasClicked;
    
    void Awake()
    {
        // Configuração inicial
        mainCamera = Camera.main;
        objectCollider = GetComponent<Collider>();
        
        // Validação completa
        ValidateSetup();
        
        DebugLog($"Inicializado - Camera: {(mainCamera != null ? mainCamera.name : "NULL")}, Collider: {(objectCollider != null ? objectCollider.GetType().Name : "NULL")}");
    }
    
    private void ValidateSetup()
    {
        bool hasErrors = false;
        
        if (objectCollider == null)
        {
            Debug.LogError($"[ClickableObject3D] {gameObject.name} precisa de um Collider!");
            hasErrors = true;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError($"[ClickableObject3D] Nenhuma câmera encontrada! Certifique-se que há uma Camera com tag 'MainCamera'");
            hasErrors = true;
        }
        
        if (objectCollider != null && !objectCollider.enabled)
        {
            Debug.LogWarning($"[ClickableObject3D] Collider de {gameObject.name} está desabilitado!");
        }
        
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[ClickableObject3D] {gameObject.name} não está ativo na hierarquia!");
        }
        
        // Verifica layer
        int objectLayer = gameObject.layer;
        bool layerInMask = (raycastLayers.value & (1 << objectLayer)) != 0;
        if (!layerInMask)
        {
            Debug.LogWarning($"[ClickableObject3D] {gameObject.name} está no layer {objectLayer} que não está incluído no raycastLayers!");
        }
        
        if (!hasErrors)
        {
            DebugLog("Setup válido - objeto pronto para interação");
        }
    }
    
    void Update()
    {
        // Debug forçado por um frame
        if (forceDebugNextFrame)
        {
            forceDebugNextFrame = false;
            bool tempDebug = enableDebugLogs;
            enableDebugLogs = true;
            
            DebugLog("=== DEBUG FORÇADO ===");
            DebugLog($"Mouse Screen Pos: {Input.mousePosition}");
            DebugLog($"Camera: {(mainCamera != null ? mainCamera.name : "NULL")}");
            DebugLog($"Collider: {(objectCollider != null ? objectCollider.enabled : false)}");
            DebugLog($"GameObject ativo: {gameObject.activeInHierarchy}");
            
            UpdateInteractions();
            
            enableDebugLogs = tempDebug;
            DebugLog("=== FIM DEBUG ===");
        }
        else
        {
            UpdateInteractions();
        }
    }
    
    #region Interaction System
    
    private void UpdateInteractions()
    {
        // Verifica se mouse está sobre este objeto
        bool currentlyHovered = IsMouseOverObject();
        
        // Hover Enter/Exit
        if (hoverEnabled && currentlyHovered != isHovered)
        {
            isHovered = currentlyHovered;
            
            if (isHovered)
            {
                OnHoverEnter();
            }
            else
            {
                OnHoverExit();
            }
        }
        
        // Só processa cliques se mouse estiver sobre o objeto
        if (!currentlyHovered) return;
        
        // Clique/Drag
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
        
        // Drag update
        if (isDragging && dragEnabled)
        {
            UpdateDrag();
        }
    }
    
    private bool IsMouseOverObject()
    {
        if (mainCamera == null) 
        {
            DebugLog("Camera é null - não pode fazer raycast");
            return false;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        DebugLog($"Raycast - Origin: {ray.origin}, Direction: {ray.direction}");
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastLayers))
        {
            DebugLog($"Raycast HIT - Object: {hit.collider.name}, Distance: {hit.distance}");
            bool isThisObject = hit.collider == objectCollider;
            DebugLog($"É este objeto? {isThisObject}");
            return isThisObject;
        }
        else
        {
            DebugLog("Raycast não atingiu nenhum objeto");
        }
        
        return false;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;
        
        // Projeta mouse na mesma altura Z do objeto
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnMouseDown()
    {
        isPressed = true;
        wasClicked = true;
        
        if (dragEnabled)
        {
            dragStartWorldPosition = GetMouseWorldPosition();
            lastMouseWorldPosition = dragStartWorldPosition;
        }
        
        DebugLog($"Mouse down on {gameObject.name}");
    }
    
    private void OnMouseUp()
    {
        if (isPressed)
        {
            // Clique (se não estava dragando ou drag não habilitado)
            if (clickEnabled && (!isDragging || !dragEnabled))
            {
                OnClick();
            }
            
            // Fim do drag
            if (isDragging && dragEnabled)
            {
                OnDragEnd();
            }
        }
        
        isPressed = false;
        isDragging = false;
        wasClicked = false;
        
        DebugLog($"Mouse up on {gameObject.name}");
    }
    
    private void UpdateDrag()
    {
        Vector3 currentMouseWorld = GetMouseWorldPosition();
        
        // Inicia drag se moveu mouse suficiente
        if (!isDragging && Vector3.Distance(currentMouseWorld, dragStartWorldPosition) > 0.1f)
        {
            isDragging = true;
            OnDragStart();
        }
        
        lastMouseWorldPosition = currentMouseWorld;
    }
    
    private void OnClick()
    {
        DebugLog($"Click on {gameObject.name}");
        
        // Dispara eventos
        OnThisObjectClick?.Invoke(this);
        OnAnyObjectClick?.Invoke(this);
    }
    
    private void OnHoverEnter()
    {
        DebugLog($"Hover enter on {gameObject.name}");
        
        // Dispara eventos
        OnThisObjectHoverEnter?.Invoke(this);
        OnAnyObjectHoverEnter?.Invoke(this);
    }
    
    private void OnHoverExit()
    {
        DebugLog($"Hover exit on {gameObject.name}");
        
        // Dispara eventos
        OnThisObjectHoverExit?.Invoke(this);
        OnAnyObjectHoverExit?.Invoke(this);
    }
    
    private void OnDragStart()
    {
        DebugLog($"Drag start on {gameObject.name}");
        
        // Dispara eventos
        OnThisObjectDragStart?.Invoke(this);
        OnAnyObjectDragStart?.Invoke(this);
    }
    
    private void OnDragEnd()
    {
        DebugLog($"Drag end on {gameObject.name}");
        
        // Dispara eventos
        OnThisObjectDragEnd?.Invoke(this);
        OnAnyObjectDragEnd?.Invoke(this);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Configura quais tipos de interação estão habilitados
    /// </summary>
    public void SetInteractionEnabled(bool click = true, bool drag = false, bool hover = true)
    {
        clickEnabled = click;
        dragEnabled = drag;
        hoverEnabled = hover;
        
        DebugLog($"Interactions set - Click: {click}, Drag: {drag}, Hover: {hover}");
    }
    
    /// <summary>
    /// Força reset de todos os estados
    /// </summary>
    public void ResetStates()
    {
        isHovered = false;
        isPressed = false;
        isDragging = false;
        wasClicked = false;
        
        DebugLog("States reset");
    }
    
    /// <summary>
    /// Simula um clique programaticamente
    /// </summary>
    public void SimulateClick()
    {
        OnClick();
    }
    
    /// <summary>
    /// Retorna informações de debug do objeto
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Object: {gameObject.name}\n" +
               $"Hovered: {isHovered}, Pressed: {isPressed}, Dragging: {isDragging}\n" +
               $"Click: {clickEnabled}, Drag: {dragEnabled}, Hover: {hoverEnabled}\n" +
               $"Position: {transform.position}";
    }
    
    #endregion
    
    #region Debug
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ClickableObject3D - {gameObject.name}] {message}");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos || objectCollider == null) return;
        
        // Mostra bounds do collider
        Gizmos.color = isHovered ? Color.yellow : Color.white;
        Gizmos.DrawWireCube(objectCollider.bounds.center, objectCollider.bounds.size);
        
        // Mostra raycast do mouse se estiver hovering
        if (isHovered && mainCamera != null)
        {
            Gizmos.color = Color.red;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Gizmos.DrawRay(ray.origin, ray.direction * maxRaycastDistance);
        }
        
        // Estados visuais
        if (isPressed)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
        
        if (isDragging)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(dragStartWorldPosition, lastMouseWorldPosition);
        }
    }
    
    [ContextMenu("Test Click")]
    private void TestClick()
    {
        SimulateClick();
    }
    
    [ContextMenu("Reset States")]
    private void TestResetStates()
    {
        ResetStates();
    }
    
    [ContextMenu("Print Debug Info")]
    private void TestDebugInfo()
    {
        Debug.Log(GetDebugInfo());
    }
    
    [ContextMenu("Force Debug Next Frame")]
    private void TestForceDebug()
    {
        forceDebugNextFrame = true;
        DebugLog("Debug forçado para próximo frame");
    }
    
    [ContextMenu("Test Raycast Now")]
    private void TestRaycastNow()
    {
        bool wasDebugEnabled = enableDebugLogs;
        enableDebugLogs = true;
        
        DebugLog("=== TESTE DE RAYCAST ===");
        bool mouseOver = IsMouseOverObject();
        DebugLog($"Mouse sobre objeto: {mouseOver}");
        
        enableDebugLogs = wasDebugEnabled;
    }
    
    [ContextMenu("Validate Setup")]
    private void TestValidateSetup()
    {
        ValidateSetup();
    }
    
    #endregion
} 