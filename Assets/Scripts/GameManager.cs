using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float restartDelay = 2f;
    
    // Estado do jogo (centralizado)
    private int currentScore = 0;
    private int currentCoins = 0;
    private int currentLives = 3;
    
    // Singleton
    public static GameManager Instance { get; private set; }
    
    // Eventos públicos para UI e outros sistemas
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action OnGameOver;
    public System.Action OnGameRestart;
    
    // Propriedades públicas (read-only)
    public int Score => currentScore;
    public int Coins => currentCoins;
    public int Lives => currentLives;
    public int MaxLives => maxLives;
    public bool IsGameOver => currentLives <= 0;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        // Notifica UI inicial
        NotifyAllUIUpdates();
        
        Debug.Log($"🎮 GameManager iniciado! Score: {currentScore}, Coins: {currentCoins}, Lives: {currentLives}");
    }
    
    #region Score Management
    
    public void AddScore(int points)
    {
        if (points <= 0) return;
        
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        Debug.Log($"📈 Score: {currentScore} (+{points})");
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    #endregion
    
    #region Coins Management
    
    public void AddCoins(int coins)
    {
        if (coins <= 0) return;
        
        currentCoins += coins;
        OnCoinsChanged?.Invoke(currentCoins);
        
        Debug.Log($"💰 Coins: {currentCoins} (+{coins})");
    }
    
    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            OnCoinsChanged?.Invoke(currentCoins);
            Debug.Log($"💸 Gastou {amount} coins. Saldo: {currentCoins}");
            return true;
        }
        
        Debug.Log($"❌ Coins insuficientes! Precisa: {amount}, Tem: {currentCoins}");
        return false;
    }
    
    public void ResetCoins()
    {
        currentCoins = 0;
        OnCoinsChanged?.Invoke(currentCoins);
    }
    
    #endregion
    
    #region Lives Management
    
    public void LoseLife()
    {
        if (currentLives <= 0) return;
        
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);
        
        Debug.Log($"💔 Vida perdida! Vidas: {currentLives}/{maxLives}");
        
        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }
    
    public void AddLife()
    {
        if (currentLives >= maxLives) return;
        
        currentLives++;
        OnLivesChanged?.Invoke(currentLives);
        
        Debug.Log($"❤️ Vida recuperada! Vidas: {currentLives}/{maxLives}");
    }
    
    public void ResetLives()
    {
        currentLives = maxLives;
        OnLivesChanged?.Invoke(currentLives);
    }
    
    #endregion
    
    #region Game Over & Restart
    
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
    
    public void RestartGame()
    {
        Debug.Log("🔄 Reiniciando jogo...");
        
        // Reseta todos os valores
        ResetScore();
        ResetCoins();
        ResetLives();
        
        // Destroi todos os devices
        if (DeviceSpawner.Instance != null)
        {
            DeviceSpawner.Instance.DestroyAllDevices();
            DeviceSpawner.Instance.enabled = true;
        }
        
        OnGameRestart?.Invoke();
        
        Debug.Log("🔄 Jogo reiniciado completamente!");
    }
    
    #endregion
    
    private void NotifyAllUIUpdates()
    {
        OnScoreChanged?.Invoke(currentScore);
        OnCoinsChanged?.Invoke(currentCoins);
        OnLivesChanged?.Invoke(currentLives);
    }
} 