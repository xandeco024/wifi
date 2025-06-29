# Guia - Sistema de Upgrades por Botões do Modem

## Visão Geral
O sistema de upgrade do modem foi atualizado para usar **botões da UI** ao invés de duplo clique. Agora o duplo clique no modem sempre abre o popup de informações.

## Mudanças de Comportamento

### ❌ Sistema Anterior
- **Duplo Clique Esquerdo**: Upgrade de cabos
- **Duplo Clique Direito**: Upgrade de velocidade
- **Clique Direito**: Popup de informações

### ✅ Sistema Atual
- **Duplo Clique (qualquer)**: Popup de informações
- **Clique Direito**: Popup de informações
- **Botões da UI**: Upgrades de cabos e velocidade

## Métodos Públicos do Modem

### 🔌 Upgrade de Cabos
```csharp
/// <summary>
/// Método público para upgrade de cabos - pode ser chamado por botões da UI
/// </summary>
public void UpgradeCables()
```

**Uso em Botão:**
1. Arraste o GameObject do Modem para o campo "Object" do evento OnClick
2. Selecione `Modem.UpgradeCables()` no dropdown
3. O botão automaticamente fará upgrade de cabos quando clicado

### ⚡ Upgrade de Velocidade
```csharp
/// <summary>
/// Método público para upgrade de velocidade - pode ser chamado por botões da UI
/// </summary>
public void UpgradeSpeed()
```

**Uso em Botão:**
1. Arraste o GameObject do Modem para o campo "Object" do evento OnClick
2. Selecione `Modem.UpgradeSpeed()` no dropdown
3. O botão automaticamente fará upgrade de velocidade quando clicado

### 📊 Mostrar Informações
```csharp
/// <summary>
/// Método público para mostrar popup - pode ser chamado por botões da UI
/// </summary>
public void ShowModemInfo()
```

**Uso em Botão:**
1. Arraste o GameObject do Modem para o campo "Object" do evento OnClick
2. Selecione `Modem.ShowModemInfo()` no dropdown
3. O botão automaticamente abrirá o popup de informações

## Configuração de Botões na UI

### Exemplo de Setup de Botão de Upgrade de Cabos

1. **Crie um Botão** na sua UI Canvas
2. **Configure o Texto** do botão (ex: "Upgrade Cabos")
3. **No componente Button**, encontre a seção "On Click ()"
4. **Clique no "+"** para adicionar um novo evento
5. **Arraste o GameObject do Modem** para o campo "Object"
6. **No dropdown**, selecione `Modem.UpgradeCables()`

### Exemplo de Setup de Botão de Upgrade de Velocidade

1. **Crie um Botão** na sua UI Canvas
2. **Configure o Texto** do botão (ex: "Upgrade Velocidade")
3. **No componente Button**, encontre a seção "On Click ()"
4. **Clique no "+"** para adicionar um novo evento
5. **Arraste o GameObject do Modem** para o campo "Object"
6. **No dropdown**, selecione `Modem.UpgradeSpeed()`

### Exemplo de Setup de Botão de Informações

1. **Crie um Botão** na sua UI Canvas
2. **Configure o Texto** do botão (ex: "Info Modem")
3. **No componente Button**, encontre a seção "On Click ()"
4. **Clique no "+"** para adicionar um novo evento
5. **Arraste o GameObject do Modem** para o campo "Object"
6. **No dropdown**, selecione `Modem.ShowModemInfo()`

## Validações de Segurança

### Proteções Implementadas
- ✅ **Interação Desabilitada**: Não executa se `enableInteraction = false`
- ✅ **Animação em Andamento**: Não executa se já está fazendo upgrade
- ✅ **Dinheiro Insuficiente**: Não executa se não tem coins suficientes
- ✅ **Nível Máximo**: Não executa se já está no nível máximo

### Logs de Debug
```
[Modem] Upgrade de cabos solicitado via botão
[Modem] Upgrade de velocidade solicitado via botão
[Modem] Interação desabilitada - upgrade de cabos cancelado
[Modem] Animação de upgrade em andamento - ignorando upgrade de velocidade
```

## Popup Atualizado

### Conteúdo Mostrado
```
Informações do Modem

Cabos: Nível 2 (3 Portas)
Velocidade: Nível 1 (10 MB/s)
Velocidade Atual: 10 MB/s
Cabos Simultâneos: 3

🔌 Upgrade Cabos: 80 coins
   Use botão de upgrade na UI

⚡ Upgrade Velocidade: 60 coins
   Use botão de upgrade na UI

Duplo clique ou ESC para fechar
```

## Controles de Acesso

### Via Singleton
```csharp
// Upgrade de cabos programaticamente
if (Modem.Instance != null)
{
    Modem.Instance.UpgradeCables();
}

// Upgrade de velocidade programaticamente  
if (Modem.Instance != null)
{
    Modem.Instance.UpgradeSpeed();
}

// Mostrar informações programaticamente
if (Modem.Instance != null)
{
    Modem.Instance.ShowModemInfo();
}
```

## Exemplo de UI Completa

### Estrutura Recomendada
```
Canvas
├── ScoreCoinsPanel (Score e Coins)
├── ModemControlPanel
│   ├── UpgradeCablesButton
│   ├── UpgradeSpeedButton
│   └── ModemInfoButton
└── ModemInfoPopup (Popup existente)
```

### Script de Exemplo para Atualização de Botões
```csharp
public class ModemUIController : MonoBehaviour
{
    [Header("Modem UI Buttons")]
    [SerializeField] private Button upgradeCablesButton;
    [SerializeField] private Button upgradeSpeedButton;
    [SerializeField] private Button modemInfoButton;
    
    [Header("Button Texts")]
    [SerializeField] private TextMeshProUGUI cablesButtonText;
    [SerializeField] private TextMeshProUGUI speedButtonText;
    
    void Start()
    {
        // Configura eventos dos botões
        upgradeCablesButton.onClick.AddListener(() => {
            if (Modem.Instance != null)
                Modem.Instance.UpgradeCables();
        });
        
        upgradeSpeedButton.onClick.AddListener(() => {
            if (Modem.Instance != null)
                Modem.Instance.UpgradeSpeed();
        });
        
        modemInfoButton.onClick.AddListener(() => {
            if (Modem.Instance != null)
                Modem.Instance.ShowModemInfo();
        });
    }
    
    void Update()
    {
        UpdateButtonsState();
    }
    
    void UpdateButtonsState()
    {
        if (Modem.Instance == null) return;
        
        // Atualiza estado do botão de cabos
        bool canUpgradeCables = Modem.Instance.CanUpgradeCables() && 
                               ScoreAndCoinsManager.Instance.GetCurrentCoins() >= Modem.Instance.NextCableUpgradeCost;
        upgradeCablesButton.interactable = canUpgradeCables && !Modem.Instance.IsUpgrading;
        
        if (cablesButtonText != null)
        {
            if (Modem.Instance.CanUpgradeCables())
            {
                cablesButtonText.text = $"Upgrade Cabos ({Modem.Instance.NextCableUpgradeCost}c)";
            }
            else
            {
                cablesButtonText.text = "Cabos MAX";
            }
        }
        
        // Atualiza estado do botão de velocidade
        bool canUpgradeSpeed = Modem.Instance.CanUpgradeSpeed() && 
                              ScoreAndCoinsManager.Instance.GetCurrentCoins() >= Modem.Instance.NextSpeedUpgradeCost;
        upgradeSpeedButton.interactable = canUpgradeSpeed && !Modem.Instance.IsUpgrading;
        
        if (speedButtonText != null)
        {
            if (Modem.Instance.CanUpgradeSpeed())
            {
                speedButtonText.text = $"Upgrade Velocidade ({Modem.Instance.NextSpeedUpgradeCost}c)";
            }
            else
            {
                speedButtonText.text = "Velocidade MAX";
            }
        }
    }
}
```

## Vantagens do Sistema

### 🎮 Para o Jogador
- **Interface Mais Intuitiva**: Botões claramente identificados
- **Menos Acidental**: Não faz upgrade por engano
- **Informações Sempre Acessíveis**: Duplo clique mostra popup

### 🔧 Para o Desenvolvedor  
- **Controle Mais Preciso**: Métodos públicos chamáveis
- **UI Flexível**: Botões podem ser posicionados onde quiser
- **Debug Mais Fácil**: Logs específicos para cada ação

### 🎯 Para o Gameplay
- **Decisões Mais Conscientes**: Upgrades intencionais via botões
- **Feedback Visual**: Botões podem mostrar custos e disponibilidade
- **UX Melhorada**: Separação clara entre informação e ação

## Compatibilidade

### ✅ Mantido
- Todas as funcionalidades de upgrade existentes
- Animações de upgrade
- Sistema de validação de moedas
- Popup de informações
- Singleton pattern

### ✅ Melhorado
- Controles mais explícitos via botões
- Popup sempre acessível via duplo clique
- Métodos públicos para integração com UI
- Logs mais detalhados 