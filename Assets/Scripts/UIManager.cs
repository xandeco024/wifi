using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Score and Coins UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;
    
    [Header("Life System UI")]
    [SerializeField] private TextMeshProUGUI livesText;
    
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
        
        // Busca ou cria o LifeSystem
        var lifeSystem = LifeSystem.Instance;
        
        if (lifeSystem == null)
        {
            // Cria o LifeSystem se não existir
            GameObject lifeObj = new GameObject("LifeSystem");
            lifeSystem = lifeObj.AddComponent<LifeSystem>();
            Debug.Log("LifeSystem criado automaticamente!");
        }
        
        // Conecta os TextMeshProUGUI aos managers
        ConnectUIToManager(scoreManager);
        ConnectUIToLifeSystem(lifeSystem);
        
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
    
    private void ConnectUIToLifeSystem(LifeSystem lifeSystem)
    {
        // Conecta o texto de vidas usando reflection
        var lifeSystemType = typeof(LifeSystem);
        
        if (livesText != null)
        {
            var livesTextField = lifeSystemType.GetField("livesText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            livesTextField?.SetValue(lifeSystem, livesText);
            Debug.Log("Lives text conectado!");
        }
    }
} 