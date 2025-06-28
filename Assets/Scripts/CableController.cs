using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CableController : MonoBehaviour
{
    [Header("Cable Settings")]
    [SerializeField] private GameObject cablePrefab; // Prefab da Image do cabo
    [SerializeField] private float cableWidth = 5f; // Largura do cabo
    [SerializeField] private Color cableColor = Color.yellow;
    [SerializeField] private Color cableConnectedColor = Color.green;
    
    [Header("References")]
    [SerializeField] private RectTransform cableContainer; // Container para organizar cabos
    [SerializeField] private Canvas gameCanvas; // Canvas principal
    [SerializeField] private RectTransform modemRectTransform; // Posição do modem
    
    [Header("Cable Behavior")]
    [SerializeField] private LayerMask pcLayerMask = -1; // Layer dos PCs (se necessário)
    [SerializeField] private float connectionSnapDistance = 50f; // Distância para snap
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showCableGizmos = true;
    
    // Estado do sistema
    private bool isDragging = false;
    private GameObject activeCable = null;
    private RectTransform activeCableRect = null;
    private Image activeCableImage = null;
    private Vector2 dragStartPosition;
    private Vector2 currentMousePosition;
    
    // Componentes
    private Camera uiCamera;
    
    // Singleton para fácil acesso
    public static CableController Instance { get; private set; }
    
    // Contador para nomes únicos de cabos
    private static int cableCounter = 0;
    
    // Eventos
    public System.Action<Vector2> OnCableDragStart;
    public System.Action<Vector2> OnCableDragUpdate;
    public System.Action<ComputerBehavior> OnCableConnected; // PC conectado com sucesso
    public System.Action OnCableDragCanceled;
    
    // Propriedades públicas
    public bool IsDragging => isDragging;
    public bool HasActiveCable => activeCable != null;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-configuração
        if (gameCanvas == null)
            gameCanvas = FindObjectOfType<Canvas>();
            
        // Auto-configuração do modem se não foi definido
        if (modemRectTransform == null)
        {
            Modem modemComponent = FindObjectOfType<Modem>();
            if (modemComponent != null)
            {
                modemRectTransform = modemComponent.GetComponent<RectTransform>();
                DebugLog("Modem RectTransform auto-detectado");
            }
        }
        
        // Auto-configuração do Cable Container se não foi definido
        if (cableContainer == null && gameCanvas != null)
        {
            Transform container = gameCanvas.transform.Find("CableContainer");
            if (container != null)
            {
                cableContainer = container.GetComponent<RectTransform>();
                DebugLog("Cable Container auto-detectado");
            }
        }
            
        // Detecta câmera da UI
        uiCamera = gameCanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : gameCanvas?.worldCamera;
    }
    
    void Start()
    {
        ValidateReferences();
    }
    
    void Update()
    {
        if (isDragging)
        {
            UpdateCableVisual();
        }
        
        // Input de cancelamento (ESC)
        if (isDragging && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCableDrag();
        }
    }
    
    #region Cable Drag System
    
    /// <summary>
    /// Inicia o drag do cabo a partir do modem
    /// </summary>
    public void StartCableDrag()
    {
        if (isDragging || modemRectTransform == null)
        {
            DebugLog("Não é possível iniciar drag - já está dragando ou modem inválido");
            return;
        }
        
        // Posição inicial do cabo (centro do modem)
        dragStartPosition = modemRectTransform.anchoredPosition;
        
        // Cria o cabo visual
        CreateCableVisual();
        
        // Define estado
        isDragging = true;
        
        DebugLog($"Cabo iniciado na posição {dragStartPosition}");
        
        // Dispara evento
        OnCableDragStart?.Invoke(dragStartPosition);
    }
    
    /// <summary>
    /// Atualiza o visual do cabo durante o drag
    /// </summary>
    private void UpdateCableVisual()
    {
        if (activeCable == null || activeCableRect == null) return;
        
        // Converte posição do mouse para coordenadas UI
        UpdateMousePosition();
        
        // Calcula direção e distância
        Vector2 direction = currentMousePosition - dragStartPosition;
        float distance = direction.magnitude;
        
        if (distance < 1f) return; // Evita divisão por zero
        
        // Atualiza posição do cabo (centro entre início e mouse)
        Vector2 cableCenter = dragStartPosition + direction * 0.5f;
        activeCableRect.anchoredPosition = cableCenter;
        
        // Atualiza tamanho do cabo (comprimento)
        activeCableRect.sizeDelta = new Vector2(distance, cableWidth);
        
        // Atualiza rotação do cabo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        activeCableRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Dispara evento de atualização
        OnCableDragUpdate?.Invoke(currentMousePosition);
    }
    
    /// <summary>
    /// Finaliza o drag do cabo
    /// </summary>
    public void EndCableDrag()
    {
        if (!isDragging)
        {
            DebugLog("Tentativa de finalizar drag sem estar dragando");
            return;
        }
        
        // Usa diretamente a posição do mouse em coordenadas de tela para o raycast UI
        Vector2 screenPosition = Input.mousePosition;
        
        DebugLog($"Verificando PC na posição de tela: {screenPosition}");
        
        // Tenta conectar a um PC
        ComputerBehavior targetPC = FindPCAtScreenPosition(screenPosition);
        
        if (targetPC != null && CanConnectToPC(targetPC))
        {
            // Conexão bem-sucedida
            ConnectCableToPC(targetPC);
        }
        else
        {
            // Conexão falhou - executa diagnóstico e cancela cabo
            if (targetPC == null)
            {
                DebugLog("Executando diagnóstico de PCs...");
                DiagnosePCRaycastSetup();
            }
            CancelCableDrag();
        }
    }
    
    /// <summary>
    /// Cancela o drag do cabo
    /// </summary>
    public void CancelCableDrag()
    {
        if (!isDragging && activeCable == null) return;
        
        DebugLog("Cabo cancelado");
        
        // Remove cabo visual
        DestroyCableVisual();
        
        // Reset estado
        isDragging = false;
        
        // Dispara evento
        OnCableDragCanceled?.Invoke();
    }
    
    #endregion
    
    #region Cable Visual System
    
    /// <summary>
    /// Cria o cabo visual
    /// </summary>
    private void CreateCableVisual()
    {
        if (cablePrefab != null)
        {
            // Usa prefab se disponível
            activeCable = Instantiate(cablePrefab);
        }
        else
        {
            // Cria cabo procedural
            CreateProceduralCable();
        }
        
        // Configura parent
        if (cableContainer != null)
            activeCable.transform.SetParent(cableContainer, false);
        else if (gameCanvas != null)
            activeCable.transform.SetParent(gameCanvas.transform, false);
        
        // Obtém componentes
        activeCableRect = activeCable.GetComponent<RectTransform>();
        activeCableImage = activeCable.GetComponent<Image>();
        
        // Configuração inicial
        if (activeCableImage != null)
        {
            activeCableImage.color = cableColor;
        }
        
        // Configura pivot para rotação no centro
        if (activeCableRect != null)
        {
            activeCableRect.pivot = new Vector2(0.5f, 0.5f);
            activeCableRect.anchorMin = new Vector2(0.5f, 0.5f);
            activeCableRect.anchorMax = new Vector2(0.5f, 0.5f);
        }
        
        DebugLog("Cabo visual criado");
    }
    
    /// <summary>
    /// Cria um cabo procedural caso não tenha prefab
    /// </summary>
    private void CreateProceduralCable()
    {
        // Cria GameObject do cabo
        cableCounter++;
        activeCable = new GameObject($"Cable_{cableCounter:D3}");
        DebugLog($"Cabo {activeCable.name} criado");
        
        // Adiciona RectTransform
        activeCableRect = activeCable.AddComponent<RectTransform>();
        
        // Adiciona Image
        activeCableImage = activeCable.AddComponent<Image>();
        activeCableImage.color = cableColor;
        
        // Configuração do RectTransform
        activeCableRect.pivot = new Vector2(0.5f, 0.5f);
        activeCableRect.anchorMin = new Vector2(0.5f, 0.5f);
        activeCableRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        DebugLog("Cabo procedural criado");
    }
    
    /// <summary>
    /// Remove o cabo visual
    /// </summary>
    private void DestroyCableVisual()
    {
        if (activeCable != null)
        {
            Destroy(activeCable);
            activeCable = null;
            activeCableRect = null;
            activeCableImage = null;
            
            DebugLog("Cabo visual destruído");
        }
    }
    
    #endregion
    
    #region PC Connection System
    
    /// <summary>
    /// Encontra PC na posição da tela especificada
    /// </summary>
    private ComputerBehavior FindPCAtScreenPosition(Vector2 screenPosition)
    {
        // Raycast UI para encontrar PCs
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        
        // Realiza raycast
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        DebugLog($"Raycast encontrou {results.Count} objetos na posição {screenPosition}");
        
        // Debug: lista todos os objetos encontrados
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            DebugLog($"Objeto {i}: {result.gameObject.name} (Layer: {result.gameObject.layer})");
            
            // Verifica se tem ComputerBehavior
            ComputerBehavior pc = result.gameObject.GetComponent<ComputerBehavior>();
            if (pc != null)
            {
                DebugLog($"PC encontrado: {pc.name} (Estado: {pc.CurrentState})");
                return pc;
            }
            
            // Verifica se tem ComputerBehavior nos pais
            ComputerBehavior parentPC = result.gameObject.GetComponentInParent<ComputerBehavior>();
            if (parentPC != null)
            {
                DebugLog($"PC encontrado no pai: {parentPC.name} (Estado: {parentPC.CurrentState})");
                return parentPC;
            }
        }
        
        DebugLog("Nenhum PC encontrado na posição do mouse");
        return null;
    }
    
    /// <summary>
    /// Verifica se pode conectar ao PC
    /// </summary>
    private bool CanConnectToPC(ComputerBehavior pc)
    {
        if (pc == null) return false;
        
        // Só pode conectar se estiver desconectado
        bool canConnect = pc.CurrentState == ComputerBehavior.PCState.Disconnected;
        
        DebugLog($"Pode conectar ao PC {pc.name}: {canConnect} (Estado: {pc.CurrentState})");
        
        return canConnect;
    }
    
    /// <summary>
    /// Conecta o cabo ao PC
    /// </summary>
    private void ConnectCableToPC(ComputerBehavior pc)
    {
        DebugLog($"Conectando cabo ao PC {pc.name}");
        
        // Conecta o PC
        pc.ConnectPC();
        
        // Muda cor do cabo para conectado
        if (activeCableImage != null)
        {
            activeCableImage.color = cableConnectedColor;
        }
        
        // Fixa o cabo na posição final
        Vector2 pcPosition = pc.GetComponent<RectTransform>().anchoredPosition;
        FixCableToPosition(pcPosition);
        
        // CORREÇÃO: Guarda referência específica deste cabo antes de resetar activeCable
        GameObject connectedCable = activeCable;
        string cableName = connectedCable != null ? connectedCable.name : "null";
        DebugLog($"Cabo {cableName} conectado ao PC {pc.name}, agendando destruição em 2s");
        
        // Para o sistema de drag e reseta variáveis para permitir novo cabo
        isDragging = false;
        activeCable = null;
        activeCableRect = null;
        activeCableImage = null;
        
        // Dispara evento
        OnCableConnected?.Invoke(pc);
        
        // Agenda destruição DESTE cabo específico após cooldown do PC
        StartCoroutine(DestroyCableAfterDelay(connectedCable, 2f));
    }
    
    /// <summary>
    /// Fixa o cabo numa posição específica
    /// </summary>
    private void FixCableToPosition(Vector2 endPosition)
    {
        if (activeCableRect == null) return;
        
        // Calcula posição e rotação finais
        Vector2 direction = endPosition - dragStartPosition;
        float distance = direction.magnitude;
        Vector2 cableCenter = dragStartPosition + direction * 0.5f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Aplica transformações finais
        activeCableRect.anchoredPosition = cableCenter;
        activeCableRect.sizeDelta = new Vector2(distance, cableWidth);
        activeCableRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        DebugLog($"Cabo fixado entre {dragStartPosition} e {endPosition}");
    }
    
    /// <summary>
    /// Destrói o cabo após delay
    /// </summary>
    private IEnumerator DestroyCableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Remove o cabo
        DestroyCableVisual();
        
        DebugLog("Cabo removido após delay");
    }
    
    /// <summary>
    /// Destrói um cabo específico após delay (para múltiplos cabos simultâneos)
    /// </summary>
    private IEnumerator DestroyCableAfterDelay(GameObject specificCable, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Remove o cabo específico
        if (specificCable != null)
        {
            DebugLog($"Destruindo cabo específico: {specificCable.name}");
            Destroy(specificCable);
        }
        else
        {
            DebugLog("Cabo específico já foi destruído ou é null");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Atualiza posição do mouse em coordenadas UI
    /// </summary>
    private void UpdateMousePosition()
    {
        Vector2 localMousePos;
        RectTransform canvasRect = gameCanvas.GetComponent<RectTransform>();
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, Input.mousePosition, uiCamera, out localMousePos))
        {
            currentMousePosition = localMousePos;
        }
    }
    
    /// <summary>
    /// Valida referências necessárias
    /// </summary>
    private bool ValidateReferences()
    {
        bool isValid = true;
        
        if (gameCanvas == null)
        {
            Debug.LogError("[CableController] Game Canvas não encontrado!");
            isValid = false;
        }
        
        if (modemRectTransform == null)
        {
            Debug.LogError("[CableController] Modem RectTransform não definido!");
            isValid = false;
        }
        
        if (cableContainer == null)
        {
            DebugLog("Cable Container não definido - cabos serão criados no Canvas");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Log de debug
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[CableController] {message}");
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Força cancelamento de qualquer cabo ativo
    /// </summary>
    public void ForceCancelActiveCable()
    {
        if (isDragging || HasActiveCable)
        {
            CancelCableDrag();
        }
    }
    
    /// <summary>
    /// Retorna informações de debug do sistema
    /// </summary>
    public string GetCableSystemInfo()
    {
        return $"Dragging: {isDragging}, Active Cable: {HasActiveCable}, " +
               $"Start Pos: {dragStartPosition}, Mouse Pos: {currentMousePosition}";
    }
    
    /// <summary>
    /// Diagnóstico dos PCs na cena para verificar configuração de raycast
    /// </summary>
    public void DiagnosePCRaycastSetup()
    {
        ComputerBehavior[] allPCs = FindObjectsOfType<ComputerBehavior>();
        DebugLog($"=== DIAGNÓSTICO DE PCs ({allPCs.Length} encontrados) ===");
        
        for (int i = 0; i < allPCs.Length; i++)
        {
            ComputerBehavior pc = allPCs[i];
            Image pcImage = pc.GetComponent<Image>();
            
            DebugLog($"PC {i}: {pc.name}");
            DebugLog($"  - Posição: {pc.transform.position}");
            DebugLog($"  - RectTransform: {pc.GetComponent<RectTransform>()?.anchoredPosition}");
            DebugLog($"  - Tem Image: {pcImage != null}");
            DebugLog($"  - Raycast Target: {(pcImage != null ? pcImage.raycastTarget : false)}");
            DebugLog($"  - Canvas pai: {pc.GetComponentInParent<Canvas>()?.name}");
            DebugLog($"  - Estado: {pc.CurrentState}");
            DebugLog($"  - GameObject ativo: {pc.gameObject.activeInHierarchy}");
        }
        
        DebugLog("=== FIM DIAGNÓSTICO ===");
    }
    
    /// <summary>
    /// Configura cor do cabo dinamicamente
    /// </summary>
    public void SetCableColor(Color newColor)
    {
        cableColor = newColor;
        if (activeCableImage != null)
        {
            activeCableImage.color = newColor;
        }
    }
    
    #endregion
    
    #region Input Integration
    
    /// <summary>
    /// Método para ser chamado pelo Modem quando inicia drag
    /// </summary>
    public void OnModemDragStart()
    {
        StartCableDrag();
    }
    
    /// <summary>
    /// Método para ser chamado quando solta o mouse
    /// </summary>
    public void OnModemDragEnd()
    {
        EndCableDrag();
    }
    
    /// <summary>
    /// Método para cancelar drag (ESC, clique direito, etc.)
    /// </summary>
    public void OnDragCancel()
    {
        CancelCableDrag();
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (!showCableGizmos) return;
        
        // Desenha posição do modem
        if (modemRectTransform != null)
        {
            Gizmos.color = Color.blue;
            Vector3 modemWorldPos = modemRectTransform.TransformPoint(modemRectTransform.anchoredPosition);
            Gizmos.DrawWireSphere(modemWorldPos, 20f);
        }
        
        // Desenha linha do cabo ativo
        if (isDragging && modemRectTransform != null)
        {
            Gizmos.color = cableColor;
            Vector3 startWorld = modemRectTransform.TransformPoint(dragStartPosition);
            Vector3 endWorld = modemRectTransform.TransformPoint(currentMousePosition);
            Gizmos.DrawLine(startWorld, endWorld);
        }
    }
    
    #endregion
} 