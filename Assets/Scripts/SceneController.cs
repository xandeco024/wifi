using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// Controlador de cenas com métodos públicos para botões da UI
/// </summary>
public class SceneController : MonoBehaviour
{
    [Header("Transição")]
    [SerializeField] private bool useTransitionEffect = true;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private CanvasGroup fadeCanvas;
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private UnityEngine.UI.Slider loadingBar;
    [SerializeField] private TMPro.TextMeshProUGUI loadingText;
    
    public static SceneController Instance { get; private set; }
    
    // Eventos
    public System.Action<string> OnSceneLoadStarted;
    public System.Action<string> OnSceneLoadCompleted;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Se há um fadeCanvas, garante que comece transparente
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.gameObject.SetActive(false);
        }
        
        // Se há painel de loading, garante que comece desativado
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    #region Métodos Públicos para Botões
    
    /// <summary>
    /// Carrega uma cena pelo nome
    /// </summary>
    /// <param name="sceneName">Nome da cena</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneController] Nome da cena não pode ser vazio!");
            return;
        }
        
        Debug.Log($"[SceneController] Carregando cena: {sceneName}");
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    /// <summary>
    /// Reinicia a cena atual
    /// </summary>
    public void RestartCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[SceneController] Reiniciando cena atual: {currentSceneName}");
        LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// Carrega a próxima cena na Build Settings
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[SceneController] Carregando próxima cena (índice {nextIndex})");
            LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("[SceneController] Não há próxima cena disponível!");
        }
    }
    
    /// <summary>
    /// Carrega a cena anterior na Build Settings
    /// </summary>
    public void LoadPreviousScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int previousIndex = currentIndex - 1;
        
        if (previousIndex >= 0)
        {
            Debug.Log($"[SceneController] Carregando cena anterior (índice {previousIndex})");
            LoadScene(previousIndex);
        }
        else
        {
            Debug.LogWarning("[SceneController] Não há cena anterior disponível!");
        }
    }
    
    /// <summary>
    /// Carrega o menu principal
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Carrega a cena do jogo principal
    /// </summary>
    public void LoadGameScene()
    {
        LoadScene("wifi");
    }
    
    /// <summary>
    /// Sai do jogo
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[SceneController] Saindo do jogo...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Pausa o jogo
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("[SceneController] Jogo pausado");
    }
    
    /// <summary>
    /// Despausa o jogo
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("[SceneController] Jogo despausado");
    }
    
    /// <summary>
    /// Alterna entre pausar/despausar
    /// </summary>
    public void TogglePause()
    {
        if (Time.timeScale == 0f)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Versão estática utilitária para encerrar o jogo via UI Button sem precisar de referência ao objeto na cena.
    /// </summary>
    public static void QuitGameStatic()
    {
        if (Instance != null)
        {
            Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    
    #endregion
    
    #region Métodos Sobrecarregados
    
    /// <summary>
    /// Carrega uma cena pelo índice
    /// </summary>
    /// <param name="sceneIndex">Índice da cena</param>
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[SceneController] Índice de cena inválido: {sceneIndex}");
            return;
        }
        
        Debug.Log($"[SceneController] Carregando cena por índice: {sceneIndex}");
        StartCoroutine(LoadSceneCoroutine(sceneIndex));
    }
    
    #endregion
    
    #region Corrotinas de Loading
    
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        OnSceneLoadStarted?.Invoke(sceneName);
        
        // Efeito de fade out
        if (useTransitionEffect)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Mostra painel de loading
        ShowLoadingPanel(true);
        
        // Carrega a cena assincronamente
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Atualiza barra de progresso
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateLoadingProgress(progress, $"Carregando {sceneName}...");
            
            // Quando chegar a 90%, permite ativação da cena
            if (asyncLoad.progress >= 0.9f)
            {
                UpdateLoadingProgress(1f, "Finalizando...");
                yield return new WaitForSeconds(0.5f); // Pequena pausa para mostrar 100%
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Esconde painel de loading
        ShowLoadingPanel(false);
        
        // Efeito de fade in
        if (useTransitionEffect)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        OnSceneLoadCompleted?.Invoke(sceneName);
        Debug.Log($"[SceneController] Cena {sceneName} carregada com sucesso!");
    }
    
    private IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        string sceneName = $"Scene_{sceneIndex}";
        OnSceneLoadStarted?.Invoke(sceneName);
        
        // Efeito de fade out
        if (useTransitionEffect)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Mostra painel de loading
        ShowLoadingPanel(true);
        
        // Carrega a cena assincronamente
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;
        
        // Atualiza barra de progresso
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateLoadingProgress(progress, $"Carregando cena {sceneIndex}...");
            
            // Quando chegar a 90%, permite ativação da cena
            if (asyncLoad.progress >= 0.9f)
            {
                UpdateLoadingProgress(1f, "Finalizando...");
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Esconde painel de loading
        ShowLoadingPanel(false);
        
        // Efeito de fade in
        if (useTransitionEffect)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        OnSceneLoadCompleted?.Invoke(sceneName);
        Debug.Log($"[SceneController] Cena {sceneIndex} carregada com sucesso!");
    }
    
    #endregion
    
    #region Efeitos de Transição
    
    private IEnumerator FadeOut()
    {
        if (fadeCanvas == null) yield break;
        
        fadeCanvas.gameObject.SetActive(true);
        yield return fadeCanvas.DOFade(1f, transitionDuration).WaitForCompletion();
    }
    
    private IEnumerator FadeIn()
    {
        if (fadeCanvas == null) yield break;
        
        yield return fadeCanvas.DOFade(0f, transitionDuration).WaitForCompletion();
        fadeCanvas.gameObject.SetActive(false);
    }
    
    #endregion
    
    #region UI de Loading
    
    private void ShowLoadingPanel(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
            
            if (show)
            {
                UpdateLoadingProgress(0f, "Iniciando...");
            }
        }
    }
    
    private void UpdateLoadingProgress(float progress, string text)
    {
        if (loadingBar != null)
        {
            loadingBar.value = progress;
        }
        
        if (loadingText != null)
        {
            loadingText.text = text;
        }
    }
    
    #endregion
    
    #region Métodos Utilitários
    
    /// <summary>
    /// Retorna o nome da cena atual
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Retorna o índice da cena atual
    /// </summary>
    public int GetCurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
    
    /// <summary>
    /// Verifica se uma cena existe nas Build Settings
    /// </summary>
    public bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    #endregion
} 