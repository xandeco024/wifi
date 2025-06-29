using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Game UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI livesText;
    
    [Header("UI Settings")]
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string coinsPrefix = "Coins: ";
    [SerializeField] private string livesPrefix = "Vidas: ";
    
    void Start()
    {
        // Conecta aos eventos do GameManager
        ConnectToGameManager();
        
        Debug.Log("✅ UIManager iniciado - conectado ao GameManager!");
    }
    
    void OnDestroy()
    {
        // Desconecta eventos
        DisconnectFromGameManager();
    }
    
    private void ConnectToGameManager()
    {
        if (GameManager.Instance != null)
        {
            // Conecta aos eventos do GameManager
            GameManager.Instance.OnScoreChanged += UpdateScoreText;
            GameManager.Instance.OnCoinsChanged += UpdateCoinsText;
            GameManager.Instance.OnLivesChanged += UpdateLivesText;
            GameManager.Instance.OnGameOver += OnGameOver;
            GameManager.Instance.OnGameRestart += OnGameRestart;
            
            // Atualiza UI inicial com valores atuais
            UpdateScoreText(GameManager.Instance.Score);
            UpdateCoinsText(GameManager.Instance.Coins);
            UpdateLivesText(GameManager.Instance.Lives);
            
            Debug.Log("UIManager conectado aos eventos do GameManager");
        }
        else
        {
            // Tenta conectar depois se GameManager ainda não existe
            Invoke(nameof(RetryConnection), 0.5f);
        }
    }
    
    private void DisconnectFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScoreText;
            GameManager.Instance.OnCoinsChanged -= UpdateCoinsText;
            GameManager.Instance.OnLivesChanged -= UpdateLivesText;
            GameManager.Instance.OnGameOver -= OnGameOver;
            GameManager.Instance.OnGameRestart -= OnGameRestart;
        }
    }
    
    private void RetryConnection()
    {
        if (GameManager.Instance != null)
        {
            ConnectToGameManager();
        }
        else
        {
            Invoke(nameof(RetryConnection), 0.5f);
        }
    }
    
    #region UI Update Methods
    
    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + score.ToString();
        }
    }
    
    private void UpdateCoinsText(int coins)
    {
        if (coinsText != null)
        {
            coinsText.text = coinsPrefix + coins.ToString();
        }
    }
    
    private void UpdateLivesText(int lives)
    {
        if (livesText != null)
        {
            livesText.text = livesPrefix + lives.ToString();
        }
    }
    
    #endregion
    
    #region Game Event Handlers
    
    private void OnGameOver()
    {
        Debug.Log("🎮 UIManager: Game Over detectado!");
        // Aqui você pode adicionar efeitos visuais de game over
        // Por exemplo: piscar texto de vidas, mostrar popup, etc.
    }
    
    private void OnGameRestart()
    {
        Debug.Log("🎮 UIManager: Game Restart detectado!");
        // Aqui você pode adicionar efeitos visuais de restart
        // Por exemplo: animação de reset dos valores, etc.
    }
    
    #endregion
} 