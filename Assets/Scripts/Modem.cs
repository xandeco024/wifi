using UnityEngine;
using DG.Tweening;

/// <summary>
/// Configuração de upgrade de cabos (portas)
/// </summary>
[System.Serializable]
public class CableUpgradeConfig
{
    [Header("Cable Upgrade Info")]
    public int level = 1;
    public string levelName = "Básico";
    public int upgradeCost = 50;
    
    [Header("Cable Capability")]
    [Range(1, 10)]
    public int maxSimultaneousCables = 2;
    
    public bool IsValid()
    {
        return level > 0 && maxSimultaneousCables > 0 && upgradeCost >= 0;
    }
}

/// <summary>
/// Configuração de upgrade de velocidade
/// </summary>
[System.Serializable]
public class SpeedUpgradeConfig
{
    [Header("Speed Upgrade Info")]
    public int level = 1;
    public string levelName = "Lenta";
    public int upgradeCost = 30;
    
    [Header("Speed Capability")]
    [Range(1f, 100f)]
    public float internetSpeedMBps = 10f;
    
    public bool IsValid()
    {
        return level > 0 && internetSpeedMBps > 0f && upgradeCost >= 0;
    }
}

/// <summary>
/// Modem 3D para o sistema de jogo 3D
/// Versão simplificada sem dependências de UI
/// </summary>
public class Modem : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private bool enableInteraction = true;
    
    [Header("Cable Upgrades")]
    [SerializeField] private CableUpgradeConfig[] cableUpgrades = new CableUpgradeConfig[]
    {
        new CableUpgradeConfig { level = 1, levelName = "2 Portas", upgradeCost = 0, maxSimultaneousCables = 2 },
        new CableUpgradeConfig { level = 2, levelName = "3 Portas", upgradeCost = 40, maxSimultaneousCables = 3 },
        new CableUpgradeConfig { level = 3, levelName = "4 Portas", upgradeCost = 80, maxSimultaneousCables = 4 },
        new CableUpgradeConfig { level = 4, levelName = "5 Portas", upgradeCost = 150, maxSimultaneousCables = 5 },
        new CableUpgradeConfig { level = 5, levelName = "6 Portas", upgradeCost = 300, maxSimultaneousCables = 6 }
    };
    
    [Header("Speed Upgrades")]
    [SerializeField] private SpeedUpgradeConfig[] speedUpgrades = new SpeedUpgradeConfig[]
    {
        new SpeedUpgradeConfig { level = 1, levelName = "10 MB/s", upgradeCost = 0, internetSpeedMBps = 10f },
        new SpeedUpgradeConfig { level = 2, levelName = "20 MB/s", upgradeCost = 60, internetSpeedMBps = 20f },
        new SpeedUpgradeConfig { level = 3, levelName = "35 MB/s", upgradeCost = 120, internetSpeedMBps = 35f },
        new SpeedUpgradeConfig { level = 4, levelName = "50 MB/s", upgradeCost = 200, internetSpeedMBps = 50f },
        new SpeedUpgradeConfig { level = 5, levelName = "80 MB/s", upgradeCost = 350, internetSpeedMBps = 80f }
    };
    
    // Estados
    private bool isActive = false;
    private bool isConnecting = false;
    private bool isUpgrading = false; // Impede múltiplas animações de upgrade
    
    // Sistema de upgrades independentes
    private int currentCableLevelIndex = 0;
    private int currentSpeedLevelIndex = 0;
    
    // Componentes
    private ClickableObject clickableComponent;
    
    // Eventos
    public System.Action<Modem> OnModemClicked;
    public System.Action<Modem> OnModemActivated;
    public System.Action<Modem> OnModemDeactivated;
    
    // Propriedades
    public bool IsActive => isActive;
    public bool IsConnecting => isConnecting;
    public bool IsUpgrading => isUpgrading;
    public Vector3 Position => transform.position;
    
    // Propriedades baseadas nos upgrades atuais
    public float InternetSpeed => GetCurrentSpeedUpgrade().internetSpeedMBps;
    public int MaxSimultaneousCables => GetCurrentCableUpgrade().maxSimultaneousCables;
    
    // Propriedades de Cable Upgrade
    public int CurrentCableLevel => GetCurrentCableUpgrade().level;
    public string CurrentCableLevelName => GetCurrentCableUpgrade().levelName;
    public int NextCableUpgradeCost => CanUpgradeCables() ? GetNextCableUpgrade().upgradeCost : -1;
    public bool CanUpgradeCables() => currentCableLevelIndex < cableUpgrades.Length - 1;
    
    // Propriedades de Speed Upgrade
    public int CurrentSpeedLevel => GetCurrentSpeedUpgrade().level;
    public string CurrentSpeedLevelName => GetCurrentSpeedUpgrade().levelName;
    public int NextSpeedUpgradeCost => CanUpgradeSpeed() ? GetNextSpeedUpgrade().upgradeCost : -1;
    public bool CanUpgradeSpeed() => currentSpeedLevelIndex < speedUpgrades.Length - 1;
    
    // Singleton
    public static Modem Instance { get; private set; }
    
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
        
        SetupComponents();
    }
    
    void Start()
    {
        SetupClickableObject();
        PositionInGridCenter();
    }
    
    void OnDestroy()
    {
        if (clickableComponent != null)
        {
            clickableComponent.OnThisObjectClick -= OnModemClicked3D;
            clickableComponent.OnThisObjectDoubleClick -= OnModemDoubleClicked3D;
            clickableComponent.OnThisObjectHoverEnter -= OnModemHoverEnter;
            clickableComponent.OnThisObjectHoverExit -= OnModemHoverExit;
        }
    }
    
    private void SetupComponents()
    {
        clickableComponent = GetComponent<ClickableObject>();
        if (clickableComponent == null)
        {
            clickableComponent = gameObject.AddComponent<ClickableObject>();
        }
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
        
        if (!gameObject.CompareTag("Modem"))
        {
            gameObject.tag = "Modem";
        }
    }
    
    private void SetupClickableObject()
    {
        if (clickableComponent != null)
        {
            clickableComponent.SetInteractionEnabled(click: true, drag: false, hover: true);
            
            clickableComponent.OnThisObjectClick += OnModemClicked3D;
            clickableComponent.OnThisObjectDoubleClick += OnModemDoubleClicked3D;
            clickableComponent.OnThisObjectHoverEnter += OnModemHoverEnter;
            clickableComponent.OnThisObjectHoverExit += OnModemHoverExit;
        }
    }
    
    /// <summary>
    /// Posiciona o modem na célula central do grid
    /// </summary>
    private void PositionInGridCenter()
    {
        if (WorldBounds.Instance != null)
        {
            // Posiciona na célula central do grid
            Vector3 centerPosition = WorldBounds.Instance.GetCenterCellPosition();
            transform.position = centerPosition;
            
            // Atualiza o centro do grid para a posição atual (caso tenha mudado)
            WorldBounds.Instance.SetGridCenter(centerPosition);
            
            Debug.Log($"[Modem] Posicionado na célula central do grid: {centerPosition}");
        }
        else
        {
            Debug.LogWarning("[Modem] WorldBounds não encontrado - modem não foi posicionado no grid");
        }
    }
    
    private void OnModemClicked3D(ClickableObject clickedObject)
    {
        if (!enableInteraction) return;
        
        // Verifica se é clique direito
        if (Input.GetMouseButtonDown(1))
        {
            // Clique direito - mostra popup de informações
            ShowInfoPopup();
            return;
        }
        
        // Impede clique simples durante animação de upgrade
        if (isUpgrading)
        {
            Debug.Log("Animação de upgrade em andamento - ignorando clique simples");
            return;
        }
        
        // Shake visual ao clicar no modem
        transform.DOShakePosition(0.3f, 0.2f, 10, 90)
                 .SetEase(Ease.OutQuad);
        
        ToggleActive();
        Debug.Log("Modem clicado");
        
        OnModemClicked?.Invoke(this);
    }
    
    private void OnModemDoubleClicked3D(ClickableObject clickedObject)
    {
        if (!enableInteraction) return;
        
        // Duplo clique agora sempre mostra o popup
        Debug.Log("Duplo clique no modem - mostrando popup de informações");
        ShowInfoPopup();
    }
    
    private void OnModemHoverEnter(ClickableObject hoveredObject)
    {
        // Visual feedback pode ser adicionado aqui se necessário
    }
    
    private void OnModemHoverExit(ClickableObject hoveredObject)
    {
        // Visual feedback pode ser adicionado aqui se necessário
    }
    
    public void ToggleActive()
    {
        SetActive(!isActive);
    }
    
    public void SetActive(bool active)
    {
        if (isActive != active)
        {
            isActive = active;
            
            if (isActive)
            {
                OnModemActivated?.Invoke(this);
                Debug.Log("Modem ativado");
            }
            else
            {
                OnModemDeactivated?.Invoke(this);
                Debug.Log("Modem desativado");
            }
        }
    }
    
    public void SetConnecting(bool connecting)
    {
        if (isConnecting != connecting)
        {
            isConnecting = connecting;
            Debug.Log($"Modem {(connecting ? "conectando" : "parou de conectar")}");
        }
    }
    
    public bool IsInRange(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= 2f;
    }
    
    /// <summary>
    /// Força o reposicionamento do modem na célula central do grid
    /// </summary>
    public void ForcePositionInGrid()
    {
        PositionInGridCenter();
    }
    
    /// <summary>
    /// Animação de upgrade: pula e gira rapidamente
    /// </summary>
    private void PlayUpgradeAnimation()
    {
        // Marca como em animação
        isUpgrading = true;
        
        // Garante que está na posição central do grid
        Vector3 centerPosition = WorldBounds.Instance != null ? 
                                WorldBounds.Instance.GetCenterCellPosition() : 
                                transform.position;
        
        Vector3 originalRotation = transform.eulerAngles;
        
        // Para qualquer animação existente no transform
        transform.DOKill();
        
        // Garante posição central antes da animação
        transform.position = centerPosition;
        
        // Sequência de animação
        Sequence upgradeSequence = DOTween.Sequence();
        
        // 1. Pula para cima
        upgradeSequence.Append(transform.DOMoveY(centerPosition.y + 1.5f, 0.3f)
                              .SetEase(Ease.OutQuad));
        
        // 2. Gira rapidamente enquanto está no ar (paralelo ao pulo)
        upgradeSequence.Join(transform.DORotate(originalRotation + new Vector3(0, 720f, 0), 0.6f, RotateMode.FastBeyond360)
                            .SetEase(Ease.OutQuart));
        
        // 3. Volta para baixo (sempre para a posição central)
        upgradeSequence.Append(transform.DOMoveY(centerPosition.y, 0.3f)
                              .SetEase(Ease.InQuad));
        
        // 4. Pequeno bounce ao pousar
        upgradeSequence.Append(transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f)
                              .SetEase(Ease.OutBounce));
        
        // Callback quando terminar
        upgradeSequence.OnComplete(() => {
            // Garante que termina na posição central exata
            transform.position = centerPosition;
            isUpgrading = false; // Libera para nova animação
            Debug.Log("Animação de upgrade concluída!");
        });
        
        // Se a sequência for morta por algum motivo, também libera
        upgradeSequence.OnKill(() => {
            transform.position = centerPosition; // Força posição central
            isUpgrading = false;
        });
        
        // Efeito sonoro visual adicional - shake da câmera
        if (Camera.main != null)
        {
            Camera.main.DOShakePosition(0.4f, 0.1f, 8, 90)
                      .SetEase(Ease.OutQuad);
        }
    }
    
    private CableUpgradeConfig GetCurrentCableUpgrade()
    {
        if (currentCableLevelIndex >= 0 && currentCableLevelIndex < cableUpgrades.Length)
        {
            return cableUpgrades[currentCableLevelIndex];
        }
        
        // Fallback para nível 1 se índice inválido
        return cableUpgrades[0];
    }
    
    private CableUpgradeConfig GetNextCableUpgrade()
    {
        if (CanUpgradeCables())
        {
            return cableUpgrades[currentCableLevelIndex + 1];
        }
        
        // Retorna o nível atual se não pode fazer upgrade
        return GetCurrentCableUpgrade();
    }
    
    private SpeedUpgradeConfig GetCurrentSpeedUpgrade()
    {
        if (currentSpeedLevelIndex >= 0 && currentSpeedLevelIndex < speedUpgrades.Length)
        {
            return speedUpgrades[currentSpeedLevelIndex];
        }
        
        // Fallback para nível 1 se índice inválido
        return speedUpgrades[0];
    }
    
    private SpeedUpgradeConfig GetNextSpeedUpgrade()
    {
        if (CanUpgradeSpeed())
        {
            return speedUpgrades[currentSpeedLevelIndex + 1];
        }
        
        // Retorna o nível atual se não pode fazer upgrade
        return GetCurrentSpeedUpgrade();
    }
    
    private void TryUpgradeCables()
    {
        if (!CanUpgradeCables())
        {
            Debug.Log($"[Modem] Cabos já estão no nível máximo: {CurrentCableLevelName}");
            return;
        }
        
        CableUpgradeConfig nextCableLevel = GetNextCableUpgrade();
        
        // Verifica se tem dinheiro suficiente
        if (ScoreAndCoinsManager.Instance != null)
        {
            if (ScoreAndCoinsManager.Instance.GetCurrentCoins() >= nextCableLevel.upgradeCost)
            {
                // Cobra o upgrade
                ScoreAndCoinsManager.Instance.SpendCoins(nextCableLevel.upgradeCost);
                
                // Faz o upgrade
                currentCableLevelIndex++;
                
                // Animação de upgrade
                PlayUpgradeAnimation();
                
                Debug.Log($"[Modem] Upgrade de cabos realizado! Nível {CurrentCableLevel}: {CurrentCableLevelName} " +
                         $"(Cabos Simultâneos: {MaxSimultaneousCables})");
                
                // Atualiza popup se estiver visível
                if (ModemInfoPopup.Instance != null && ModemInfoPopup.Instance.IsPopupVisible)
                {
                    ModemInfoPopup.Instance.RefreshContent();
                }
            }
            else
            {
                int coinsNeeded = nextCableLevel.upgradeCost - ScoreAndCoinsManager.Instance.GetCurrentCoins();
                Debug.Log($"[Modem] Dinheiro insuficiente para upgrade de cabos! Precisa de mais {coinsNeeded} coins para {nextCableLevel.levelName}");
            }
        }
        else
        {
            Debug.LogWarning("[Modem] ScoreAndCoinsManager não encontrado!");
        }
    }
    
    private void TryUpgradeSpeed()
    {
        if (!CanUpgradeSpeed())
        {
            Debug.Log($"[Modem] Velocidade já está no nível máximo: {CurrentSpeedLevelName}");
            return;
        }
        
        SpeedUpgradeConfig nextSpeedLevel = GetNextSpeedUpgrade();
        
        // Verifica se tem dinheiro suficiente
        if (ScoreAndCoinsManager.Instance != null)
        {
            if (ScoreAndCoinsManager.Instance.GetCurrentCoins() >= nextSpeedLevel.upgradeCost)
            {
                // Cobra o upgrade
                ScoreAndCoinsManager.Instance.SpendCoins(nextSpeedLevel.upgradeCost);
                
                // Faz o upgrade
                currentSpeedLevelIndex++;
                
                // Animação de upgrade
                PlayUpgradeAnimation();
                
                Debug.Log($"[Modem] Upgrade de velocidade realizado! Nível {CurrentSpeedLevel}: {CurrentSpeedLevelName} " +
                         $"(Velocidade: {InternetSpeed}MB/s)");
                
                // Atualiza popup se estiver visível
                if (ModemInfoPopup.Instance != null && ModemInfoPopup.Instance.IsPopupVisible)
                {
                    ModemInfoPopup.Instance.RefreshContent();
                }
            }
            else
            {
                int coinsNeeded = nextSpeedLevel.upgradeCost - ScoreAndCoinsManager.Instance.GetCurrentCoins();
                Debug.Log($"[Modem] Dinheiro insuficiente para upgrade de velocidade! Precisa de mais {coinsNeeded} coins para {nextSpeedLevel.levelName}");
            }
        }
        else
        {
            Debug.LogWarning("[Modem] ScoreAndCoinsManager não encontrado!");
        }
    }
    
    public string GetUpgradeInfo()
    {
        string cableInfo = "";
        string speedInfo = "";
        
        // Informações de upgrade de cabos
        if (CanUpgradeCables())
        {
            CableUpgradeConfig nextCableLevel = GetNextCableUpgrade();
            cableInfo = $"Cabos: {nextCableLevel.levelName} - {nextCableLevel.upgradeCost} coins\n";
        }
        else
        {
            cableInfo = $"Cabos: Nível máximo ({CurrentCableLevelName})\n";
        }
        
        // Informações de upgrade de velocidade
        if (CanUpgradeSpeed())
        {
            SpeedUpgradeConfig nextSpeedLevel = GetNextSpeedUpgrade();
            speedInfo = $"Velocidade: {nextSpeedLevel.levelName} - {nextSpeedLevel.upgradeCost} coins";
        }
        else
        {
            speedInfo = $"Velocidade: Nível máximo ({CurrentSpeedLevelName})";
        }
        
        return cableInfo + speedInfo;
    }
    
    public string GetCableUpgradeInfo()
    {
        if (!CanUpgradeCables())
        {
            return $"Cabos no nível máximo: {CurrentCableLevelName}";
        }
        
        CableUpgradeConfig nextLevel = GetNextCableUpgrade();
        return $"Próximo upgrade de cabos: {nextLevel.levelName} - {nextLevel.upgradeCost} coins\n" +
               $"Cabos simultâneos: {nextLevel.maxSimultaneousCables}";
    }
    
    public string GetSpeedUpgradeInfo()
    {
        if (!CanUpgradeSpeed())
        {
            return $"Velocidade no nível máximo: {CurrentSpeedLevelName}";
        }
        
        SpeedUpgradeConfig nextLevel = GetNextSpeedUpgrade();
        return $"Próximo upgrade de velocidade: {nextLevel.levelName} - {nextLevel.upgradeCost} coins\n" +
               $"Velocidade: {nextLevel.internetSpeedMBps}MB/s";
    }
    
    /// <summary>
    /// Método público para upgrade de cabos - pode ser chamado por botões da UI
    /// </summary>
    public void UpgradeCables()
    {
        if (!enableInteraction)
        {
            Debug.Log("[Modem] Interação desabilitada - upgrade de cabos cancelado");
            return;
        }
        
        // Impede múltiplas animações simultâneas
        if (isUpgrading)
        {
            Debug.Log("[Modem] Animação de upgrade em andamento - ignorando upgrade de cabos");
            return;
        }
        
        Debug.Log("[Modem] Upgrade de cabos solicitado via botão");
        TryUpgradeCables();
    }
    
    /// <summary>
    /// Método público para upgrade de velocidade - pode ser chamado por botões da UI
    /// </summary>
    public void UpgradeSpeed()
    {
        if (!enableInteraction)
        {
            Debug.Log("[Modem] Interação desabilitada - upgrade de velocidade cancelado");
            return;
        }
        
        // Impede múltiplas animações simultâneas
        if (isUpgrading)
        {
            Debug.Log("[Modem] Animação de upgrade em andamento - ignorando upgrade de velocidade");
            return;
        }
        
        Debug.Log("[Modem] Upgrade de velocidade solicitado via botão");
        TryUpgradeSpeed();
    }
    
    /// <summary>
    /// Método público para mostrar popup - pode ser chamado por botões da UI
    /// </summary>
    public void ShowModemInfo()
    {
        ShowInfoPopup();
    }
    
    private void ShowInfoPopup()
    {
        if (ModemInfoPopup.Instance != null)
        {
            ModemInfoPopup.Instance.ShowPopup(this);
            Debug.Log("[Modem] Popup de informações mostrado");
        }
        else
        {
            Debug.LogWarning("[Modem] ModemInfoPopup não encontrado na cena!");
        }
    }
} 