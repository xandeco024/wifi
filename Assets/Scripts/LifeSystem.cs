using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LifeSystem : MonoBehaviour
{
    [Header("Life Settings")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives = 3;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private string livesPrefix = "Vidas: ";
    
    [Header("Game Over Settings")]
    [SerializeField] private float restartDelay = 2f;
    
    public static LifeSystem Instance { get; private set; }
    
    // Eventos
    public System.Action<int> OnLivesChanged;
    public System.Action OnGameOver;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Inicializa vidas
        currentLives = maxLives;
    }
    
    void Start()
    {
        // Conecta aos eventos do sistema
        ConnectToDeviceEvents();
        
        // Atualiza UI inicial
        UpdateLifeDisplay();
        
        Debug.Log($"LifeSystem iniciado com {currentLives} vidas");
    }
    
    void OnDestroy()
    {
        DisconnectFromDeviceEvents();
    }
    
    private void ConnectToDeviceEvents()
    {
        if (DeviceSpawner.Instance != null)
        {
            DeviceSpawner.Instance.OnDeviceSpawned += OnDeviceSpawned;
        }
        else
        {
            // Tenta conectar depois se ainda não existe
            Invoke(nameof(RetryConnection), 1f);
        }
    }
    
    private void DisconnectFromDeviceEvents()
    {
        if (DeviceSpawner.Instance != null)
        {
            DeviceSpawner.Instance.OnDeviceSpawned -= OnDeviceSpawned;
        }
    }
    
    private void RetryConnection()
    {
        if (DeviceSpawner.Instance != null)
        {
            ConnectToDeviceEvents();
        }
        else
        {
            Invoke(nameof(RetryConnection), 1f);
        }
    }
    
    private void OnDeviceSpawned(GameObject device)
    {
        Device deviceComponent = device.GetComponent<Device>();
        if (deviceComponent != null)
        {
            // Conecta ao evento de timer expirado
            deviceComponent.OnDeviceTimerExpired += OnDeviceTimerExpired;
        }
    }
    
    private void OnDeviceTimerExpired(Device device)
    {
        LoseLife();
        Debug.Log($"Vida perdida! Device {device.DeviceName} expirou");
    }
    
    public void LoseLife()
    {
        if (currentLives <= 0) return;
        
        currentLives--;
        UpdateLifeDisplay();
        
        OnLivesChanged?.Invoke(currentLives);
        
        Debug.Log($"💔 Vida perdida! Vidas restantes: {currentLives}/{maxLives}");
        
        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }
    
    public void AddLife()
    {
        if (currentLives >= maxLives) return;
        
        currentLives++;
        UpdateLifeDisplay();
        
        OnLivesChanged?.Invoke(currentLives);
        
        Debug.Log($"❤️ Vida recuperada! Vidas: {currentLives}/{maxLives}");
    }
    
    private void TriggerGameOver()
    {
        Debug.Log("💀 GAME OVER - Reiniciando...");
        
        OnGameOver?.Invoke();
        
        // Para o spawn de novos devices
        if (DeviceSpawner.Instance != null)
        {
            DeviceSpawner.Instance.enabled = false;
        }
        
        // Reinicia após delay
        Invoke(nameof(RestartGame), restartDelay);
    }
    
    private void RestartGame()
    {
        // Reseta score e coins
        if (ScoreAndCoinsManager.Instance != null)
        {
            ScoreAndCoinsManager.Instance.ResetGame();
        }
        
        // Destroi todos os devices
        if (DeviceSpawner.Instance != null)
        {
            DeviceSpawner.Instance.DestroyAllDevices();
            DeviceSpawner.Instance.enabled = true;
        }
        
        // Reseta modem para nível básico
        if (Modem.Instance != null)
        {
            Modem.Instance.ResetToBasicLevel();
        }
        
        // Reseta vidas
        currentLives = maxLives;
        UpdateLifeDisplay();
        
        Debug.Log("🔄 Jogo reiniciado completamente!");
    }
    
    private void UpdateLifeDisplay()
    {
        if (livesText != null)
        {
            livesText.text = livesPrefix + currentLives.ToString();
        }
    }
    
    // Getters públicos
    public int GetCurrentLives() => currentLives;
    public int GetMaxLives() => maxLives;
    public bool IsGameOver() => currentLives <= 0;
    
    // Método para UI
    public string GetLifeInfo()
    {
        return $"{currentLives}/{maxLives} vidas";
    }
    
    // Método de teste para forçar perda de vida (pode ser chamado por botão)
    public void TestLoseLife()
    {
        LoseLife();
        Debug.Log("🧪 Teste: Vida perdida manualmente");
    }
} 