using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComputerBehavior : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float maxTimeLimit = 10f; // Tempo máximo para resolver o PC
    [SerializeField] private float currentTime; // Tempo atual (só leitura no Inspector)
    
    [Header("UI References")]
    [SerializeField] private Slider timerSlider; // Referência ao slider de tempo
    [SerializeField] private Image pcImage; // Imagem do PC para feedback visual
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private float warningThreshold = 0.5f; // 50% do tempo
    [SerializeField] private float criticalThreshold = 0.2f; // 20% do tempo
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private string pcName = "";
    
    // Estados do PC
    public enum PCState
    {
        Disconnected,  // Aguardando conexão
        Connected,     // Conectado com sucesso
        Failed,        // Tempo esgotado
        Connecting     // Em processo de conexão
    }
    
    [Header("State (Read Only)")]
    [SerializeField] private PCState currentState = PCState.Disconnected;
    
    // Componentes
    private ClickableObject clickableComponent;
    private Coroutine timerCoroutine;
    
    // Propriedades públicas
    public float TimeRemaining => currentTime;
    public float TimeProgress => 1f - (currentTime / maxTimeLimit);
    public PCState CurrentState => currentState;
    public bool IsTimerActive => timerCoroutine != null;
    
    // Eventos
    public System.Action<GameObject> OnPCDestroyed;
    public System.Action<ComputerBehavior> OnPCTimerExpired;
    public System.Action<ComputerBehavior> OnPCConnected;
    public System.Action<ComputerBehavior, float> OnPCTimerUpdate;
    
    void Awake()
    {
        // Auto-configuração do nome se não foi definido
        if (string.IsNullOrEmpty(pcName))
            pcName = gameObject.name;
            
        // Garante que tem o componente ClickableObject
        clickableComponent = GetComponent<ClickableObject>();
        if (clickableComponent == null)
        {
            clickableComponent = gameObject.AddComponent<ClickableObject>();
        }
        
        // Tenta encontrar o slider automaticamente se não foi definido
        if (timerSlider == null)
            timerSlider = GetComponentInChildren<Slider>();
            
        // Tenta encontrar a imagem automaticamente se não foi definida
        if (pcImage == null)
            pcImage = GetComponent<Image>();
    }
    
    void Start()
    {
        // Configuração inicial
        InitializePC();
        
        // Configuração do ClickableObject
        SetupClickableObject();
        
        // Inicia o timer
        StartTimer();
    }
    
    void OnDestroy()
    {
        // Limpa eventos
        if (clickableComponent != null)
        {
            clickableComponent.OnThisObjectClick -= OnPCClicked;
            clickableComponent.OnThisObjectHoverEnter -= OnPCHoverEnter;
            clickableComponent.OnThisObjectHoverExit -= OnPCHoverExit;
        }
        
        // Para o timer se estiver rodando
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }
    
    #region Initialization
    
    private void InitializePC()
    {
        // Configuração inicial do tempo
        currentTime = maxTimeLimit;
        currentState = PCState.Disconnected;
        
        // Configuração inicial do slider
        if (timerSlider != null)
        {
            timerSlider.maxValue = 1f;
            timerSlider.value = 1f;
        }
        
        // Configuração inicial da cor
        UpdateVisualFeedback();
        
        DebugLog($"PC inicializado com {maxTimeLimit}s de tempo limite");
    }
    
    private void SetupClickableObject()
    {
        if (clickableComponent != null)
        {
            // Configura tipos de interação para PCs
            clickableComponent.SetInteractionEnabled(click: true, drag: false, hover: true);
            
            // Inscreve nos eventos
            clickableComponent.OnThisObjectClick += OnPCClicked;
            clickableComponent.OnThisObjectHoverEnter += OnPCHoverEnter;
            clickableComponent.OnThisObjectHoverExit += OnPCHoverExit;
        }
    }
    
    #endregion
    
    #region Timer System
    
    public void StartTimer()
    {
        if (timerCoroutine == null && currentState == PCState.Disconnected)
        {
            timerCoroutine = StartCoroutine(TimerCountdown());
            DebugLog("Timer iniciado");
        }
    }
    
    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
            DebugLog("Timer parado");
        }
    }
    
    public void PauseTimer()
    {
        StopTimer();
        DebugLog("Timer pausado");
    }
    
    public void ResumeTimer()
    {
        if (currentState == PCState.Disconnected)
        {
            StartTimer();
            DebugLog("Timer retomado");
        }
    }
    
    private IEnumerator TimerCountdown()
    {
        while (currentTime > 0 && currentState == PCState.Disconnected)
        {
            currentTime -= Time.deltaTime;
            
            // Atualiza UI
            UpdateSlider();
            UpdateVisualFeedback();
            
            // Dispara evento de atualização
            OnPCTimerUpdate?.Invoke(this, currentTime);
            
            yield return null;
        }
        
        // Tempo esgotado
        if (currentTime <= 0 && currentState == PCState.Disconnected)
        {
            HandleTimerExpired();
        }
        
        timerCoroutine = null;
    }
    
    private void UpdateSlider()
    {
        if (timerSlider != null)
        {
            float sliderValue = currentTime / maxTimeLimit;
            timerSlider.value = Mathf.Clamp01(sliderValue);
        }
    }
    
    private void HandleTimerExpired()
    {
        currentState = PCState.Failed;
        DebugLog("Tempo esgotado - PC falhou");
        
        // Dispara evento
        OnPCTimerExpired?.Invoke(this);
        
        // Feedback visual de falha
        UpdateVisualFeedback();
        
        // Auto-destruição após um pequeno delay
        StartCoroutine(DestroyAfterDelay(1f));
    }
    
    #endregion
    
    #region Visual Feedback
    
    private void UpdateVisualFeedback()
    {
        if (pcImage == null) return;
        
        Color targetColor = normalColor;
        
        switch (currentState)
        {
            case PCState.Disconnected:
                float timeRatio = currentTime / maxTimeLimit;
                if (timeRatio <= criticalThreshold)
                    targetColor = criticalColor;
                else if (timeRatio <= warningThreshold)
                    targetColor = warningColor;
                else
                    targetColor = normalColor;
                break;
                
            case PCState.Connected:
                targetColor = connectedColor;
                break;
                
            case PCState.Failed:
                targetColor = criticalColor;
                break;
                
            case PCState.Connecting:
                targetColor = Color.Lerp(normalColor, connectedColor, 0.5f);
                break;
        }
        
        pcImage.color = targetColor;
    }
    
    #endregion
    
    #region Click Events
    
    private void OnPCClicked(ClickableObject clickedObject)
    {
        DebugLog("PC clicado");
        
        // Aqui será implementada a lógica de conexão com o cabo futuramente
        // Por agora, só loga e verifica se pode conectar
        
        if (currentState == PCState.Disconnected)
        {
            DebugLog("PC está disponível para conexão");
            // Futuramente: verificar se há cabo ativo do modem
        }
        else
        {
            DebugLog($"PC não pode ser conectado - Estado atual: {currentState}");
        }
    }
    
    private void OnPCHoverEnter(ClickableObject hoveredObject)
    {
        DebugLog("Mouse entrou no PC");
        // Aqui pode adicionar feedback visual de hover
    }
    
    private void OnPCHoverExit(ClickableObject hoveredObject)
    {
        DebugLog("Mouse saiu do PC");
        // Aqui pode remover feedback visual de hover
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Conecta o PC com sucesso (chamado pelo sistema de cabos)
    /// </summary>
    public void ConnectPC()
    {
        if (currentState == PCState.Disconnected)
        {
            currentState = PCState.Connected;
            StopTimer();
            UpdateVisualFeedback();
            
            DebugLog("PC conectado com sucesso!");
            
            // Dispara evento
            OnPCConnected?.Invoke(this);
            
            // Auto-destruição após um delay (PC foi resolvido)
            StartCoroutine(DestroyAfterDelay(2f));
        }
    }
    
    /// <summary>
    /// Define o PC em estado de "conectando" (feedback visual)
    /// </summary>
    public void SetConnecting(bool isConnecting)
    {
        if (currentState == PCState.Disconnected)
        {
            currentState = isConnecting ? PCState.Connecting : PCState.Disconnected;
            UpdateVisualFeedback();
            
            DebugLog(isConnecting ? "PC entrando em modo de conexão" : "PC saindo do modo de conexão");
        }
    }
    
    /// <summary>
    /// Adiciona tempo extra ao PC (power-up futuro)
    /// </summary>
    public void AddExtraTime(float extraTime)
    {
        if (currentState == PCState.Disconnected)
        {
            currentTime += extraTime;
            currentTime = Mathf.Min(currentTime, maxTimeLimit); // Não excede o máximo
            UpdateSlider();
            
            DebugLog($"Tempo extra adicionado: +{extraTime}s (Total: {currentTime:F1}s)");
        }
    }
    
    /// <summary>
    /// Força a destruição do PC
    /// </summary>
    public void ForceDestroy()
    {
        DebugLog("Destruição forçada");
        StartCoroutine(DestroyAfterDelay(0f));
    }
    
    /// <summary>
    /// Retorna informações de debug do PC
    /// </summary>
    public string GetPCInfo()
    {
        return $"{pcName} - State: {currentState}, Time: {currentTime:F1}s/{maxTimeLimit}s";
    }
    
    #endregion
    
    #region Utility Methods
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ComputerBehavior] {pcName}: {message}");
        }
    }
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        
        // Notifica o spawner antes de destruir
        OnPCDestroyed?.Invoke(gameObject);
        
        DebugLog("PC sendo destruído");
        Destroy(gameObject);
    }
    
    #endregion
    
    #region Editor Utilities
    
    /// <summary>
    /// Método para testar conexão no editor (botão personalizado)
    /// </summary>
    [ContextMenu("Test Connect PC")]
    private void TestConnectPC()
    {
        ConnectPC();
    }
    
    /// <summary>
    /// Método para testar falha no editor
    /// </summary>
    [ContextMenu("Test Fail PC")]
    private void TestFailPC()
    {
        currentTime = 0;
    }
    
    /// <summary>
    /// Método para adicionar tempo no editor
    /// </summary>
    [ContextMenu("Add 5s Extra Time")]
    private void TestAddExtraTime()
    {
        AddExtraTime(5f);
    }
    
    #endregion
} 