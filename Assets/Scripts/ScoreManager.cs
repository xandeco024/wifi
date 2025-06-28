using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Score Settings")]
    [SerializeField] private int pointsPerConnection = 100;
    [SerializeField] private int currentScore = 0;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Singleton para fácil acesso
    public static ScoreManager Instance { get; private set; }
    
    // Propriedades públicas
    public int CurrentScore => currentScore;
    
    // Eventos
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnPointsAdded;
    
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
        // Conecta ao sistema de cabos
        if (CableController.Instance != null)
        {
            CableController.Instance.OnCableConnected += OnPCConnected;
        }
        
        // Inicializa o display
        UpdateScoreDisplay();
        
        DebugLog("ScoreManager inicializado");
    }
    
    void OnDestroy()
    {
        // Limpa eventos
        if (CableController.Instance != null)
        {
            CableController.Instance.OnCableConnected -= OnPCConnected;
        }
    }
    
    #region Score System
    
    private void OnPCConnected(ComputerBehavior connectedPC)
    {
        AddPoints(pointsPerConnection);
        DebugLog($"PC conectado: +{pointsPerConnection} pontos");
    }
    
    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
        
        // Dispara eventos
        OnPointsAdded?.Invoke(points);
        OnScoreChanged?.Invoke(currentScore);
        
        DebugLog($"Pontos adicionados: +{points} (Total: {currentScore})");
    }
    
    public void SubtractPoints(int points)
    {
        currentScore = Mathf.Max(0, currentScore - points);
        UpdateScoreDisplay();
        
        OnScoreChanged?.Invoke(currentScore);
        
        DebugLog($"Pontos subtraídos: -{points} (Total: {currentScore})");
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
        
        OnScoreChanged?.Invoke(currentScore);
        
        DebugLog("Score resetado");
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
        else
        {
            DebugLog("AVISO: scoreText não está configurado!");
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Configura o texto de score dinamicamente
    /// </summary>
    public void SetScoreText(TextMeshProUGUI newScoreText)
    {
        scoreText = newScoreText;
        UpdateScoreDisplay();
        DebugLog("Score text configurado dinamicamente");
    }
    
    /// <summary>
    /// Configura pontos por conexão
    /// </summary>
    public void SetPointsPerConnection(int points)
    {
        pointsPerConnection = points;
        DebugLog($"Pontos por conexão alterados para: {points}");
    }
    
    #endregion
    
    #region Debug
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ScoreManager] {message}");
        }
    }
    
    [ContextMenu("Add 100 Points")]
    private void TestAddPoints()
    {
        AddPoints(100);
    }
    
    [ContextMenu("Reset Score")]
    private void TestResetScore()
    {
        ResetScore();
    }
    
    #endregion
} 