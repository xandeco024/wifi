using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Score and Coins UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    
    void Start()
    {
        // Busca ou cria o ScoreAndCoinsManager
        var scoreManager = ScoreAndCoinsManager.Instance;
        
        if (scoreManager == null)
        {
            // Cria o ScoreAndCoinsManager se não existir
            GameObject managerObj = new GameObject("ScoreAndCoinsManager");
            scoreManager = managerObj.AddComponent<ScoreAndCoinsManager>();
            Debug.Log("ScoreAndCoinsManager criado automaticamente!");
        }
        
        // Conecta os TextMeshProUGUI ao manager
        ConnectUIToManager(scoreManager);
        
        Debug.Log("UIManager iniciado!");
    }
    
    private void ConnectUIToManager(ScoreAndCoinsManager scoreManager)
    {
        // Conecta os textos usando reflection
        var scoreManagerType = typeof(ScoreAndCoinsManager);
        
        if (scoreText != null)
        {
            var scoreTextField = scoreManagerType.GetField("scoreText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            scoreTextField?.SetValue(scoreManager, scoreText);
            Debug.Log("Score text conectado!");
        }
        
        if (coinsText != null)
        {
            var coinsTextField = scoreManagerType.GetField("coinsText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            coinsTextField?.SetValue(scoreManager, coinsText);
            Debug.Log("Coins text conectado!");
        }
        }
} 