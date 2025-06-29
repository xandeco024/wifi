using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class Device : MonoBehaviour
{
    [Header("Device Components")]
    [SerializeField] protected GameObject object3D; // Objeto 3D que deve girar
    [SerializeField] protected Canvas timerCanvas;
    [SerializeField] protected Slider progressSlider; // Timer (branco) + Download (verde)
    [SerializeField] protected TMPro.TextMeshProUGUI statusText;
    
    [Header("Device Settings")]
    [SerializeField] protected string deviceName = "PC";
    [SerializeField] protected bool enableRotation = true;
    [SerializeField] protected float rotationSpeed = 30f;
    
    // Configurações definidas pelo spawner
    protected float timeLimit = 10f;
    protected int pointsOnConnection = 10;
    
    // Estado interno
    protected float currentTime;
    protected Camera mainCamera;
    
    [Header("Reward Settings")]
    [SerializeField] protected int baseScoreValue = 10;
    [SerializeField] protected int baseGoldValue = 5;
    
    // Multiplicadores de recompensa
    protected float scoreMultiplier = 1f;
    protected float goldMultiplier = 1f;
    protected float downloadSizeMultiplier = 1f;
    
    public enum DeviceState
    {
        Disconnected,
        Connected,
        Failed,
        Connecting
    }
    
    [SerializeField] protected DeviceState currentState = DeviceState.Disconnected;
    
    protected ClickableObject clickableComponent;
    protected Coroutine timerCoroutine;
    
    public float TimeRemaining => currentTime;
    public float TimeProgress => 1f - (currentTime / timeLimit);
    public DeviceState CurrentState => currentState;
    public bool IsTimerActive => timerCoroutine != null;
    public string DeviceName => deviceName;
    
    // Propriedades de recompensa com multiplicadores
    public int ScoreValue => Mathf.RoundToInt(baseScoreValue * scoreMultiplier);
    public int GoldValue => Mathf.RoundToInt(baseGoldValue * goldMultiplier);
    
    public System.Action<GameObject> OnDeviceDestroyed;
    public System.Action<Device> OnDeviceTimerExpired;
    public System.Action<Device> OnDeviceConnected;
    public System.Action<Device> OnDeviceCompleted;
    public System.Action<Device, float> OnDeviceTimerUpdate;
    
    void Awake()
    {
        SetupComponents();
    }
    
    void Start()
    {
        InitializeDevice();
        SetupClickableObject();
        StartTimer();
    }
    
    void Update()
    {
        // Rotação contínua apenas do objeto 3D
        if (object3D != null && enableRotation && rotationSpeed > 0f)
        {
            object3D.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        }
        
        // Faz canvas sempre olhar para a câmera
        if (timerCanvas != null && mainCamera != null)
        {
            timerCanvas.transform.LookAt(timerCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                       mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    void OnDestroy()
    {
        if (clickableComponent != null)
        {
            clickableComponent.OnThisObjectClick -= OnDeviceClicked;
        }
        
        StopAllCoroutines();
        CleanupDevice();
    }
    
    protected virtual void CleanupDevice()
    {
        // Override in child classes if needed
    }
    
    protected virtual void SetupComponents()
    {
        // Encontra câmera principal
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Auto-encontra o objeto 3D se não foi definido
        if (object3D == null)
        {
            // Procura por um filho que não seja Canvas
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Canvas>() == null)
                {
                    object3D = child.gameObject;
                    break;
                }
            }
        }
        
        // Auto-encontra canvas, slider e texto
        if (timerCanvas == null)
        {
            timerCanvas = GetComponentInChildren<Canvas>();
        }
        
        if (progressSlider == null && timerCanvas != null)
        {
            progressSlider = timerCanvas.GetComponentInChildren<Slider>();
        }
        
        if (statusText == null && timerCanvas != null)
        {
            statusText = timerCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }
        
        // Setup do ClickableObject no gameObject pai
        clickableComponent = GetComponent<ClickableObject>();
        if (clickableComponent == null)
        {
            clickableComponent = gameObject.AddComponent<ClickableObject>();
        }
        
        // Collider no gameObject pai
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
    
    protected virtual void InitializeDevice()
    {
        currentTime = timeLimit;
        currentState = DeviceState.Disconnected;
        gameObject.name = $"Device_{deviceName}";
        UpdateStatusDisplay();
        Debug.Log($"Device inicializado: {deviceName} - {pointsOnConnection} pontos");
    }
    
    protected virtual void SetupClickableObject()
    {
        if (clickableComponent != null)
        {
            clickableComponent.SetInteractionEnabled(click: true, drag: false, hover: true);
            clickableComponent.OnThisObjectClick += OnDeviceClicked;
        }
    }
    
    public void StartTimer()
    {
        if (timerCoroutine == null && currentState == DeviceState.Disconnected)
        {
            timerCoroutine = StartCoroutine(TimerCountdown());
        }
    }
    
    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }
    
    private IEnumerator TimerCountdown()
    {
        while (currentTime > 0 && currentState == DeviceState.Disconnected)
        {
            yield return new WaitForSeconds(0.1f);
            currentTime -= 0.1f;
            UpdateStatusDisplay();
            OnDeviceTimerUpdate?.Invoke(this, currentTime);
        }
        
        if (currentState == DeviceState.Disconnected)
        {
            HandleTimerExpired();
        }
        
        timerCoroutine = null;
    }
    
    protected abstract void UpdateStatusDisplay();
    
    protected virtual void HandleTimerExpired()
    {
        currentState = DeviceState.Failed;
        StopTimer();
        
        // Shake intenso quando falha
        if (Camera.main != null)
        {
            Camera.main.DOShakePosition(0.5f, 0.3f, 10, 90);
        }
        
        OnDeviceTimerExpired?.Invoke(this);
        
        // Destroi o device após um pequeno delay
        StartCoroutine(DestroyAfterDelay(1f));
    }
    
    protected virtual void OnDeviceClicked(ClickableObject clickedObject)
    {
        Debug.Log($"Device {deviceName} foi clicado!");
    }
    
    public void ConnectDevice()
    {
        if (currentState == DeviceState.Disconnected || currentState == DeviceState.Connecting)
        {
            currentState = DeviceState.Connected;
            StopTimer();
            
            // Efeito visual de conexão
            if (object3D != null)
            {
                object3D.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5, 0.3f);
            }
            
            OnDeviceConnected?.Invoke(this);
            
            // Inicia minigame específico do tipo de device
            StartMinigame();
        }
    }
    
    /// <summary>
    /// Inicia o minigame específico do device. Deve ser implementado por cada tipo.
    /// </summary>
    public abstract void StartMinigame();
    
    public void SetConnecting(bool isConnecting)
    {
        if (isConnecting && currentState == DeviceState.Disconnected)
        {
            currentState = DeviceState.Connecting;
        }
        else if (!isConnecting && currentState == DeviceState.Connecting)
        {
            currentState = DeviceState.Disconnected;
        }
    }
    
    public void AddExtraTime(float extraTime)
    {
        currentTime = Mathf.Min(currentTime + extraTime, timeLimit);
        UpdateStatusDisplay();
    }
    
    protected IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Notifica antes da animação de destruição para que cabo seja removido
        OnDeviceDestroyed?.Invoke(gameObject);
        
        // Animação de destruição com DOTween
        yield return StartCoroutine(PlayDestroyAnimation());
        
        // Destroi o gameObject pai completo (canvas + objeto 3D + tudo)
        Destroy(gameObject);
    }
    
    protected IEnumerator PlayDestroyAnimation()
    {
        // Desabilita interação durante animação
        if (clickableComponent != null)
        {
            clickableComponent.SetInteractionEnabled(false, false, false);
        }
        
        // Animação de tremor (shake)
        if (object3D != null)
        {
            object3D.transform.DOShakePosition(0.5f, 0.3f, 10, 90);
            object3D.transform.DOShakeRotation(0.5f, 30f, 10, 90);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Animação de diminuir escala até sumir
        if (object3D != null)
        {
            object3D.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        }
        
        // Fade out do canvas se existir
        if (timerCanvas != null)
        {
            CanvasGroup canvasGroup = timerCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = timerCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.DOFade(0f, 0.3f);
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    protected IEnumerator PlaySuccessAnimation()
    {
        // Desabilita interação durante animação
        if (clickableComponent != null)
        {
            clickableComponent.SetInteractionEnabled(false, false, false);
        }
        
        if (object3D != null)
        {
            // Animação de pulo
            object3D.transform.DOJump(object3D.transform.position, 2f, 1, 0.8f).SetEase(Ease.OutQuad);
            
            // Animação de rotação (giro completo)
            object3D.transform.DORotate(new Vector3(0, 360, 0), 0.8f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack);
            
            // Animação de escala (pulse de sucesso)
            object3D.transform.DOPunchScale(Vector3.one * 0.4f, 0.8f, 8, 0.6f).SetEase(Ease.OutElastic);
        }
        
        // Fade out do canvas durante a animação
        if (timerCanvas != null)
        {
            CanvasGroup canvasGroup = timerCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = timerCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.DOFade(0f, 0.6f).SetDelay(0.2f);
        }
        
        yield return new WaitForSeconds(0.8f);
        
        // Notifica que foi destruído e destroi
        OnDeviceDestroyed?.Invoke(gameObject);
        Destroy(gameObject);
    }
    
    public virtual void SetSpawnConfig(float timer, int points)
    {
        timeLimit = timer;
        pointsOnConnection = points;
        currentTime = timeLimit;
        UpdateStatusDisplay();
    }
    
    /// <summary>
    /// Completa o device com sucesso
    /// </summary>
    protected void CompleteDevice()
    {
        if (currentState == DeviceState.Connected)
        {
            StopTimer();
            currentState = DeviceState.Failed;
            
            // Usa os valores com multiplicadores
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(ScoreValue);
                GameManager.Instance.AddCoins(GoldValue);
            }
            
            OnDeviceCompleted?.Invoke(this);
            StartCoroutine(PlaySuccessAnimation());
            
            Debug.Log($"✨ Device completado! +{ScoreValue} pontos, +{GoldValue} moedas");
        }
    }
    
    /// <summary>
    /// Falha o device
    /// </summary>
    protected void FailDevice()
    {
        currentState = DeviceState.Failed;
        Debug.Log($"Device falhou: {deviceName}");
        StartCoroutine(DestroyAfterDelay(1f));
    }
    
    /// <summary>
    /// Define manualmente o objeto 3D que deve girar
    /// </summary>
    public void SetObject3D(GameObject obj)
    {
        object3D = obj;
    }
    
    /// <summary>
    /// Força a destruição imediata do device
    /// </summary>
    public void ForceDestroy()
    {
        StopAllCoroutines();
        CleanupDevice();
        OnDeviceDestroyed?.Invoke(gameObject);
        Destroy(gameObject);
    }
    
    // Getters para compatibilidade
    public int GetConnectionPoints() => Mathf.RoundToInt(pointsOnConnection * scoreMultiplier);
    public int GetBonusPoints() => 5; // Valor fixo
    
    public void ApplyLevelMultipliers(float score, float gold, float download)
    {
        scoreMultiplier = score;
        goldMultiplier = gold;
        downloadSizeMultiplier = download;
        
        Debug.Log($"Multiplicadores aplicados em {deviceName}: Score {score}x, Gold {gold}x, Download {download}x");
    }
    
    public virtual int GetDownloadSize()
    {
        return 0; // Sobrescrever em classes que usam download
    }
} 