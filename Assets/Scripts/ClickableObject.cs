using UnityEngine;

/// <summary>
/// Sistema de interação para objetos 3D usando raycast
/// Detecta cliques, hover e drag em objetos 3D
/// </summary>
public class ClickableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private bool clickEnabled = true;
    [SerializeField] private bool dragEnabled = false;
    [SerializeField] private bool hoverEnabled = true;
    [SerializeField] private bool doubleClickEnabled = true;
    [SerializeField] private float doubleClickTime = 0.3f;
    
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask raycastLayerMask = -1; // Todos os layers
    [SerializeField] private float raycastDistance = 100f;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool enableVisualFeedback = true;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;
    [SerializeField] private float feedbackDuration = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showRaycastGizmos = false;
    
    private Camera mainCamera;
    private bool isDragging = false;
    private bool isHovering = false;
    
    // Double click detection
    private float lastClickTime = 0f;
    private int clickCount = 0;
    
    // Componentes
    private Renderer objectRenderer;
    private Color originalColor;
    private Material originalMaterial;
    
    // Eventos
    public System.Action<ClickableObject> OnThisObjectClick;
    public System.Action<ClickableObject> OnThisObjectDoubleClick;
    public System.Action<ClickableObject> OnThisObjectHoverEnter;
    public System.Action<ClickableObject> OnThisObjectHoverExit;
    public System.Action<ClickableObject> OnThisObjectDragStart;
    public System.Action<ClickableObject> OnThisObjectDragEnd;
    
    // Propriedades
    public bool IsClickEnabled => clickEnabled;
    public bool IsDragEnabled => dragEnabled;
    public bool IsHoverEnabled => hoverEnabled;
    public bool IsDragging => isDragging;
    public bool IsHovering => isHovering;
    
    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Auto-configuração de componentes
        SetupComponents();
    }
    
    void Start()
    {
        // Salva cor/material original
        SaveOriginalAppearance();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    #region Setup
    
    private void SetupComponents()
    {
        // Auto-configura Renderer
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            objectRenderer = GetComponentInChildren<Renderer>();
        }
        
        // Garante que tem Collider para raycast
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            // Adiciona BoxCollider se não existir
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            
            // Ajusta tamanho baseado no Renderer
            if (objectRenderer != null)
            {
                Bounds bounds = objectRenderer.bounds;
                boxCollider.size = bounds.size;
                boxCollider.center = bounds.center - transform.position;
            }
            
            DebugLog("BoxCollider adicionado automaticamente");
        }
    }
    
    private void SaveOriginalAppearance()
    {
        if (objectRenderer != null)
        {
            if (objectRenderer.material != null)
            {
                originalMaterial = objectRenderer.material;
                originalColor = objectRenderer.material.color;
            }
        }
    }
    
    #endregion
    
    #region Input Handling
    
    private void HandleInput()
    {
        if (mainCamera == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        bool isMouseOverObject = Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject;
        
        // Hover
        if (hoverEnabled)
        {
            if (isMouseOverObject && !isHovering)
            {
                isHovering = true;
                OnThisObjectHoverEnter?.Invoke(this);
                Debug.Log($"Hover enter: {gameObject.name}");
            }
            else if (!isMouseOverObject && isHovering)
            {
                isHovering = false;
                OnThisObjectHoverExit?.Invoke(this);
                Debug.Log($"Hover exit: {gameObject.name}");
            }
        }
        
        // Click and Double Click
        if (clickEnabled && isMouseOverObject && Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
        
        // Drag
        if (dragEnabled && isMouseOverObject)
        {
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                isDragging = true;
                OnThisObjectDragStart?.Invoke(this);
                Debug.Log($"Drag start: {gameObject.name}");
            }
        }
        
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            OnThisObjectDragEnd?.Invoke(this);
            Debug.Log($"Drag end: {gameObject.name}");
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void HandleClick()
    {
        float currentTime = Time.time;
        
        if (doubleClickEnabled && currentTime - lastClickTime < doubleClickTime)
        {
            // É um duplo clique
            clickCount = 0; // Reset
            OnThisObjectDoubleClick?.Invoke(this);
            DebugLog($"Duplo clique em: {gameObject.name}");
        }
        else
        {
            // Primeiro clique ou clique simples
            clickCount = 1;
            lastClickTime = currentTime;
            
            // Agenda verificação de clique simples
            StartCoroutine(CheckSingleClick());
        }
    }
    
    private System.Collections.IEnumerator CheckSingleClick()
    {
        yield return new WaitForSeconds(doubleClickTime);
        
        if (clickCount == 1)
        {
            // Foi apenas um clique simples
            OnThisObjectClick?.Invoke(this);
            DebugLog($"Clique simples em: {gameObject.name}");
        }
        
        clickCount = 0;
    }
    
    #endregion
    
    #region Visual Feedback
    
    private void SetColor(Color color)
    {
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = color;
        }
    }
    
    private void RestoreOriginalColor()
    {
        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
    
    private System.Collections.IEnumerator FlashColor(Color flashColor)
    {
        Color originalColor = objectRenderer != null ? objectRenderer.material.color : Color.white;
        
        SetColor(flashColor);
        yield return new WaitForSeconds(feedbackDuration);
        
        if (isHovering)
        {
            SetColor(hoverColor);
        }
        else
        {
            RestoreOriginalColor();
        }
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
    }
    
    /// <summary>
    /// Simula um clique no objeto
    /// </summary>
    public void SimulateClick()
    {
        OnThisObjectClick?.Invoke(this);
    }
    
    /// <summary>
    /// Simula hover enter no objeto
    /// </summary>
    public void SimulateHoverEnter()
    {
        if (!isHovering)
        {
            OnThisObjectHoverEnter?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Simula hover exit no objeto
    /// </summary>
    public void SimulateHoverExit()
    {
        if (isHovering)
        {
            OnThisObjectHoverExit?.Invoke(this);
        }
    }
    
    /// <summary>
    /// Retorna informações de debug
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Object: {gameObject.name}\n" +
               $"Hovered: {isHovering}\n" +
               $"Dragging: {isDragging}\n" +
               $"Click Enabled: {clickEnabled}\n" +
               $"Drag Enabled: {dragEnabled}\n" +
               $"Hover Enabled: {hoverEnabled}";
    }
    
    public void SetClickEnabled(bool enabled)
    {
        clickEnabled = enabled;
    }
    
    public void SetDragEnabled(bool enabled)
    {
        dragEnabled = enabled;
    }
    
    public void SetHoverEnabled(bool enabled)
    {
        hoverEnabled = enabled;
    }
    
    #endregion
    
    #region Utility Methods
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ClickableObject] {gameObject.name}: {message}");
        }
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmos()
    {
        if (!showRaycastGizmos) return;
        
        // Desenha raio do mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        Gizmos.color = isHovering ? Color.green : Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * raycastDistance);
        
        // Desenha esfera no ponto de hit
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastDistance, raycastLayerMask))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hit.point, 0.1f);
        }
    }
    
    #endregion
    
    #region Context Menu
    
    [ContextMenu("Test Click")]
    private void TestClick()
    {
        if (Application.isPlaying)
        {
            SimulateClick();
        }
        else
        {
            Debug.LogWarning("[ClickableObject] Teste só funciona no Play Mode!");
        }
    }
    
    [ContextMenu("Test Hover")]
    private void TestHover()
    {
        if (Application.isPlaying)
        {
            SimulateHoverEnter();
        }
        else
        {
            Debug.LogWarning("[ClickableObject] Teste só funciona no Play Mode!");
        }   
    }
    
    [ContextMenu("Print Debug Info")]
    private void ContextPrintDebugInfo()
    {
        Debug.Log(GetDebugInfo());
    }
    
         #endregion
} 