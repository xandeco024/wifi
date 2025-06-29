using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ModemInfoPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI currentSpeedText;
    [SerializeField] private TextMeshProUGUI currentCablesText;
    [SerializeField] private TextMeshProUGUI speedUpgradeCostText;
    [SerializeField] private TextMeshProUGUI cablesUpgradeCostText;
    [SerializeField] private Button closeButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Vector3 hiddenPosition = new Vector3(0, -1000, 0);
    [SerializeField] private Vector3 visiblePosition = Vector3.zero;
    
    [Header("Background")]
    [SerializeField] private Image backgroundOverlay;
    
    // Referências
    private Modem targetModem;
    private RectTransform popupRect;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    // Singleton
    public static ModemInfoPopup Instance { get; private set; }
    
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
        
        SetupComponents();
        SetupUI();
    }
    
    void Start()
    {
        // Inicia escondido
        HidePopupImmediate();
    }
    
    void Update()
    {
        // ESC para fechar
        if (isVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopup();
        }
    }
    
    private void SetupComponents()
    {
        // Auto-encontra componentes se não foram atribuídos
        if (popupPanel == null)
        {
            popupPanel = transform.GetChild(0).gameObject; // Assume primeiro filho é o panel
        }
        
        popupRect = popupPanel.GetComponent<RectTransform>();
        
        if (canvasGroup == null)
        {
            canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popupPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Auto-encontra textos
        TextMeshProUGUI[] texts = popupPanel.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 4)
        {
            if (currentSpeedText == null) currentSpeedText = texts[0];
            if (currentCablesText == null) currentCablesText = texts[1];
            if (speedUpgradeCostText == null) speedUpgradeCostText = texts[2];
            if (cablesUpgradeCostText == null) cablesUpgradeCostText = texts[3];
        }
        
        // Auto-encontra botão
        if (closeButton == null)
        {
            closeButton = popupPanel.GetComponentInChildren<Button>();
        }
        
        // Auto-encontra background overlay
        if (backgroundOverlay == null)
        {
            backgroundOverlay = GetComponent<Image>();
        }
    }
    
    private void SetupUI()
    {
        // Configura botão de fechar
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
        
        // Configura background overlay
        if (backgroundOverlay != null)
        {
            backgroundOverlay.color = new Color(0, 0, 0, 0.5f); // Semi-transparente
            backgroundOverlay.raycastTarget = false; // Não bloqueia cliques
        }
    }
    
    public void ShowPopup(Modem modem)
    {
        if (isVisible) return;
        
        targetModem = modem;
        UpdatePopupContent();
        
        // Ativa o popup
        gameObject.SetActive(true);
        isVisible = true;
        
        // Para animações anteriores
        popupRect.DOKill();
        canvasGroup.DOKill();
        
        // Posição inicial da animação
        popupRect.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        
        // Animação de entrada melhorada
        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(popupRect.DOAnchorPos(visiblePosition, animationDuration).SetEase(Ease.OutBack));
        showSequence.Join(canvasGroup.DOFade(1f, animationDuration).SetEase(Ease.OutQuad));
        
        Debug.Log($"Popup do modem mostrado: Cabos {modem.CurrentCableLevelName}, Velocidade {modem.CurrentSpeedLevelName}");
    }
    
    public void HidePopup()
    {
        if (!isVisible) return;
        
        isVisible = false;
        
        // Para animações anteriores
        popupRect.DOKill();
        canvasGroup.DOKill();
        
        // Animação de saída melhorada
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(popupRect.DOAnchorPos(hiddenPosition, animationDuration).SetEase(Ease.InBack));
        hideSequence.Join(canvasGroup.DOFade(0f, animationDuration).SetEase(Ease.InQuad));
        hideSequence.OnComplete(() => {
            gameObject.SetActive(false);
        });
        
        Debug.Log("Popup do modem escondido");
    }
    
    /// <summary>
    /// Método público para fechar o popup - pode ser chamado por botões
    /// </summary>
    public void ClosePopup()
    {
        HidePopup();
    }
    
    private void HidePopupImmediate()
    {
        isVisible = false;
        popupRect.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    private void UpdatePopupContent()
    {
        if (targetModem == null) return;
        
        // Atualiza velocidade atual
        if (currentSpeedText != null)
        {
            currentSpeedText.text = $"Velocidade: {targetModem.InternetSpeed} MB/s";
        }
        
        // Atualiza quantidade de portas atual
        if (currentCablesText != null)
        {
            currentCablesText.text = $"Portas: {targetModem.MaxSimultaneousCables}";
        }
        
        // Atualiza custo do upgrade de velocidade
        if (speedUpgradeCostText != null)
        {
            if (targetModem.CanUpgradeSpeed())
            {
                speedUpgradeCostText.text = $"{targetModem.NextSpeedUpgradeCost}";
            }
            else
            {
                speedUpgradeCostText.text = "Nível máximo!";
            }
        }
        
        // Atualiza custo do upgrade de portas
        if (cablesUpgradeCostText != null)
        {
            if (targetModem.CanUpgradeCables())
            {
                cablesUpgradeCostText.text = $"{targetModem.NextCableUpgradeCost}";
            }
            else
            {
                cablesUpgradeCostText.text = "Nível máximo!";
            }
        }
    }
    

    
    public void RefreshContent()
    {
        if (isVisible && targetModem != null)
        {
            UpdatePopupContent();
        }
    }
    
    public bool IsPopupVisible => isVisible;
    
    void OnDestroy()
    {
        // Limpa tweens
        popupRect?.DOKill();
        canvasGroup?.DOKill();
    }
} 