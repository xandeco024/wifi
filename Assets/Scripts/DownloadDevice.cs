using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DownloadDevice : Device
{
    [Header("Download Settings")]
    [SerializeField] private int baseDownloadSizeMB = 100;
    
    // Sistema de download
    private float downloadedMB = 0f;
    private bool isDownloading = false;
    private Coroutine downloadCoroutine;
    
    // Enum extendido para incluir o estado de download
    public enum DownloadDeviceState
    {
        Disconnected,
        Connected,
        Failed,
        Connecting,
        Downloading
    }
    
    private DownloadDeviceState downloadState = DownloadDeviceState.Disconnected;
    
    // Propriedades específicas do download
    public float DownloadProgress => downloadedMB / TotalDownloadMB;
    public int DownloadedMB => Mathf.FloorToInt(downloadedMB);
    public int TotalDownloadMB => Mathf.RoundToInt(baseDownloadSizeMB * downloadSizeMultiplier);
    public bool IsDownloading => isDownloading;
    public DownloadDeviceState DownloadState => downloadState;
    
    protected override void InitializeDevice()
    {
        base.InitializeDevice();
        downloadedMB = 0f;
        isDownloading = false;
        downloadState = DownloadDeviceState.Disconnected;
        Debug.Log($"DownloadDevice inicializado: {DeviceName} - {TotalDownloadMB}MB para download (x{downloadSizeMultiplier})");
    }
    
    protected override void CleanupDevice()
    {
        StopDownload();
    }
    
    public override void StartMinigame()
    {
        Debug.Log($"DownloadDevice: Iniciando download de {TotalDownloadMB}MB");
        StartDownload();
    }
    
    protected override void UpdateStatusDisplay()
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
        else if (downloadState == DownloadDeviceState.Downloading)
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
    
    public override void SetSpawnConfig(float timer, int points)
    {
        base.SetSpawnConfig(timer, points);
        // Reset download specific data
        downloadedMB = 0f;
        downloadState = DownloadDeviceState.Disconnected;
    }
    
    public void SetSpawnConfig(float timer, int points, int downloadSizeMB)
    {
        SetSpawnConfig(timer, points);
        baseDownloadSizeMB = downloadSizeMB;
        downloadedMB = 0f;
        UpdateStatusDisplay();
    }
    
    private void StartDownload()
    {
        if (currentState == DeviceState.Connected && !isDownloading)
        {
            downloadState = DownloadDeviceState.Downloading;
            isDownloading = true;
            
            // Atualiza UI para modo download
            UpdateStatusDisplay();
            
            downloadCoroutine = StartCoroutine(DownloadProcess());
            Debug.Log($"Download iniciado: {TotalDownloadMB}MB (x{downloadSizeMultiplier})");
        }
    }
    
    private IEnumerator DownloadProcess()
    {
        while (downloadedMB < TotalDownloadMB && downloadState == DownloadDeviceState.Downloading)
        {
            yield return new WaitForSeconds(0.1f);
            
            // Obtém velocidade do modem
            float downloadSpeed = Modem.Instance != null ? Modem.Instance.InternetSpeed : 10f;
            
            // Calcula quanto baixar neste frame (MB por segundo * 0.1 segundos)
            float downloadThisFrame = downloadSpeed * 0.1f;
            downloadedMB = Mathf.Min(downloadedMB + downloadThisFrame, TotalDownloadMB);
            
            UpdateStatusDisplay();
        }
        
        if (downloadState == DownloadDeviceState.Downloading && downloadedMB >= TotalDownloadMB)
        {
            OnDownloadComplete();
        }
        
        downloadCoroutine = null;
    }
    
    private void OnDownloadComplete()
    {
        downloadState = DownloadDeviceState.Connected;
        isDownloading = false;
        
        Debug.Log($"Download completo: {DeviceName} ({TotalDownloadMB}MB)");
        
        // Usa o método da classe base para completar
        CompleteDevice();
    }
    
    private void StopDownload()
    {
        if (downloadCoroutine != null)
        {
            StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        isDownloading = false;
        downloadState = DownloadDeviceState.Disconnected;
    }
    
    public override int GetDownloadSize()
    {
        return TotalDownloadMB;
    }
} 