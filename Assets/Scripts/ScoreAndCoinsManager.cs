using UnityEngine;
using TMPro;

public class ScoreAndCoinsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    
    [Header("Display Settings")]
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string coinsPrefix = "Coins: ";
    
    // Estado atual
    private int currentScore = 0;
    private int currentCoins = 0;
    
    // Singleton
    public static ScoreAndCoinsManager Instance { get; private set; }
    
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
    }
    
    void Start()
    {
        // Conecta aos eventos do sistema
        ConnectToDeviceEvents();
        
        // Atualiza display inicial
        UpdateDisplay();
        
        Debug.Log("ScoreAndCoinsManager iniciado!");
    }
    
    void OnDestroy()
    {
        // Desconecta eventos
        DisconnectFromDeviceEvents();
    }
    
    private void ConnectToDeviceEvents()
    {
        // Conecta aos eventos do DeviceSpawner
        if (DeviceSpawner.Instance != null)
        {
            DeviceSpawner.Instance.OnDeviceSpawned += OnDeviceSpawned;
        }
        
        // Se o DeviceSpawner ainda não existir, tenta conectar depois
        if (DeviceSpawner.Instance == null)
        {
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
        // Conecta aos eventos do device individual
        Device deviceComponent = device.GetComponent<Device>();
        if (deviceComponent != null)
        {
            deviceComponent.OnDeviceConnected += OnDeviceConnected;
        }
    }
    
    private void OnDeviceConnected(Device device)
    {
        if (device == null) return;
        
        // Quando conecta, apenas adiciona o evento de completar (se ainda não tiver)
        device.OnDeviceCompleted += OnDeviceCompleted;
        
        Debug.Log($"{device.DeviceName} conectado!");
    }
    
    public void OnDeviceCompleted(Device device)
    {
        if (device == null) return;
        
        // Pega recompensas
        int scoreToAdd = device.GetConnectionPoints();
        int coinsToAdd = GetCoinRewardForDevice(device.gameObject);
        
        // Adiciona recompensas
        AddScore(scoreToAdd);
        AddCoins(coinsToAdd);
        
        Debug.Log($"{device.DeviceName} completado! +{scoreToAdd} score, +{coinsToAdd} coins");
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateDisplay();
    }
    
    public void AddCoins(int coins)
    {
        currentCoins += coins;
        UpdateDisplay();
    }
    
    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateDisplay();
            Debug.Log($"Gastou {amount} coins. Saldo restante: {currentCoins}");
            return true;
        }
        
        Debug.Log($"Não é possível gastar {amount} coins. Saldo atual: {currentCoins}");
        return false;
    }
    
    public int GetCurrentCoins()
    {
        return currentCoins;
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    private void UpdateDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore.ToString();
        }
        
        if (coinsText != null)
        {
            coinsText.text = coinsPrefix + currentCoins.ToString();
        }
    }
    
    private int GetCoinRewardForDevice(GameObject device)
    {
        // Busca a recompensa de coins no spawner
        if (DeviceSpawner.Instance != null)
        {
            return DeviceSpawner.Instance.GetCoinRewardForDevice(device);
        }
        
        return 5; // Valor padrão
    }
    
    public void ResetGame()
    {
        currentScore = 0;
        currentCoins = 0;
        UpdateDisplay();
        Debug.Log("Score e Coins resetados para o restart");
    }
} 