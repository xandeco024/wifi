using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Device : MonoBehaviour
{
    [Header("Device Components")]
    [SerializeField] private GameObject object3D; // Objeto 3D que deve girar
    [SerializeField] private Canvas timerCanvas;
    [SerializeField] private Slider progressSlider; // Timer (branco) + Download (verde)
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    [Header("Device Settings")]
    [SerializeField] private string deviceName = "PC";
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 30f;
    
    // Configurações definidas pelo spawner
    private float timeLimit = 10f;
    private int pointsOnConnection = 10;
    private int totalDownloadSizeMB = 100;
    
    // Estado interno
    private float currentTime;
    private Camera mainCamera;
    
    // Sistema de download
    private float downloadedMB = 0f;
    private bool isDownloading = false;
    private Coroutine downloadCoroutine;
    
    public enum DeviceState
    {
        Disconnected,
        Connected,
        Failed,
        Connecting,
        Downloading
    }
    
    [SerializeField] private DeviceState currentState = DeviceState.Disconnected;
    
    private ClickableObject clickableComponent;
    private Coroutine timerCoroutine;
    
    public float TimeRemaining => currentTime;
    public float TimeProgress => 1f - (currentTime / timeLimit);
    public DeviceState CurrentState => currentState;
    public bool IsTimerActive => timerCoroutine != null;
    public string DeviceName => deviceName;
    
    // Propriedades do download
    public float DownloadProgress => downloadedMB / totalDownloadSizeMB;
    public int DownloadedMB => Mathf.FloorToInt(downloadedMB);
    public int TotalDownloadMB => totalDownloadSizeMB;
    public bool IsDownloading => isDownloading;
    
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
        StopDownload();
    }
    
    private void SetupComponents()
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
    
    private void InitializeDevice()
    {
        currentTime = timeLimit;
        currentState = DeviceState.Disconnected;
        downloadedMB = 0f;
        isDownloading = false;
        gameObject.name = $"Device_{deviceName}";
        UpdateStatusDisplay();
        Debug.Log($"Device inicializado: {deviceName} - {totalDownloadSizeMB}MB para download, {pointsOnConnection} pontos");
    }
    
    private void SetupClickableObject()
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
    
    private void UpdateStatusDisplay()
    {
        if (progressSlider == null) return;
        
        if (currentState == DeviceState.Disconnected || currentState == DeviceState.Connecting)
        {
            // Modo timer - mostra tempo restante
            float timeProgress = currentTime / timeLimit;
            progressSlider.value = timeProgress;
            
            // Cor branca para tempo limite
            if (progressSlider.fillRect != null)
            {
                Image fillImage = progressSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = Color.white;
                }
            }
            
            // Texto do tempo restante
            if (statusText != null)
            {
                int secondsLeft = Mathf.CeilToInt(currentTime);
                statusText.text = $"Conectar em: {secondsLeft}s";
            }
        }
        else if (currentState == DeviceState.Downloading)
        {
            // Modo download - mostra progresso
            float downloadProgress = DownloadProgress;
            progressSlider.value = downloadProgress;
            
            // Cor verde para download
            if (progressSlider.fillRect != null)
            {
                Image fillImage = progressSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = Color.green;
                }
            }
            
            // Texto do download
            if (statusText != null)
            {
                int percentage = Mathf.FloorToInt(downloadProgress * 100f);
                statusText.text = $"{percentage}% {DownloadedMB}MB/{TotalDownloadMB}MB";
            }
        }
    }
    
    private void HandleTimerExpired()
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
    
    private void OnDeviceClicked(ClickableObject clickedObject)
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
            
            // Inicia download
            StartDownload();
        }
    }
    
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
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Notifica antes da animação de destruição para que cabo seja removido
        OnDeviceDestroyed?.Invoke(gameObject);
        
        // Animação de destruição com DOTween
        yield return StartCoroutine(PlayDestroyAnimation());
        
        // Destroi o gameObject pai completo (canvas + objeto 3D + tudo)
        Destroy(gameObject);
    }
    
    private IEnumerator PlayDestroyAnimation()
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
    
    private IEnumerator PlaySuccessAnimation()
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
    
    public void StartDownload()
    {
        if (currentState == DeviceState.Connected && !isDownloading)
        {
            currentState = DeviceState.Downloading;
            isDownloading = true;
            
            // Atualiza UI para modo download
            UpdateStatusDisplay();
            
            downloadCoroutine = StartCoroutine(DownloadProcess());
            Debug.Log($"Download iniciado: {totalDownloadSizeMB}MB");
        }
    }
    
    private IEnumerator DownloadProcess()
    {
        while (downloadedMB < totalDownloadSizeMB && currentState == DeviceState.Downloading)
        {
            yield return new WaitForSeconds(0.1f);
            
            // Obtém velocidade do modem
            float downloadSpeed = Modem.Instance != null ? Modem.Instance.InternetSpeed : 10f;
            
            // Calcula quanto baixar neste frame (MB por segundo * 0.1 segundos)
            float downloadThisFrame = downloadSpeed * 0.1f;
            downloadedMB = Mathf.Min(downloadedMB + downloadThisFrame, totalDownloadSizeMB);
            
            UpdateStatusDisplay();
        }
        
        if (currentState == DeviceState.Downloading && downloadedMB >= totalDownloadSizeMB)
        {
            OnDownloadComplete();
        }
        
        downloadCoroutine = null;
    }
    
    private void OnDownloadComplete()
    {
        currentState = DeviceState.Connected;
        isDownloading = false;
        
        Debug.Log($"Download completo: {deviceName}");
        
        // BUGFIX: Notifica conclusão ANTES da animação para dar coins/score
        OnDeviceCompleted?.Invoke(this);
        
        // Animação de sucesso (girar e pular)
        StartCoroutine(PlaySuccessAnimation());
    }
    
    public void StopDownload()
    {
        if (downloadCoroutine != null)
        {
            StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        isDownloading = false;
    }
    
    public void SetSpawnConfig(float timer, int points, int downloadSizeMB)
    {
        timeLimit = timer;
        pointsOnConnection = points;
        totalDownloadSizeMB = downloadSizeMB;
        currentTime = timeLimit;
        downloadedMB = 0f;
        UpdateStatusDisplay();
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
        StopDownload();
        OnDeviceDestroyed?.Invoke(gameObject);
        Destroy(gameObject);
    }
    
    // Getters para compatibilidade
    public int GetConnectionPoints() => pointsOnConnection;
    public int GetBonusPoints() => 5; // Valor fixo
} 