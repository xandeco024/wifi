# Guia - Sistema de Upgrades Independentes do Modem

## Visão Geral
O sistema de upgrade do modem foi reformulado para permitir upgrades independentes de **Cabos (Portas)** e **Velocidade**. Agora você pode escolher estrategicamente qual aspecto melhorar primeiro.

## Mudanças Principais

### ❌ Sistema Antigo (Un sistema único)
- Um nível geral que aumentava cabos e velocidade juntos
- Duplo clique único para upgrade
- Progressão linear obrigatória

### ✅ Sistema Novo (Dois sistemas independentes)
- **Sistema de Cabos**: Controla quantas conexões simultâneas
- **Sistema de Velocidade**: Controla velocidade de download
- **Controles separados** para cada tipo de upgrade
- **Progressão estratégica** baseada em necessidade

## Estruturas de Dados

### CableUpgradeConfig
```csharp
[System.Serializable]
public class CableUpgradeConfig
{
    public int level = 1;
    public string levelName = "2 Portas";
    public int upgradeCost = 40;
    public int maxSimultaneousCables = 2;
}
```

### SpeedUpgradeConfig
```csharp
[System.Serializable]
public class SpeedUpgradeConfig
{
    public int level = 1;
    public string levelName = "10 MB/s";
    public int upgradeCost = 60;
    public float internetSpeedMBps = 10f;
}
```

## Controles de Upgrade

### 🎮 Botões da UI
- **Função**: Upgrades de Cabos e Velocidade
- **Método**: `Modem.UpgradeCables()` e `Modem.UpgradeSpeed()`
- **Configuração**: Arraste o Modem para o evento OnClick do botão

### 🖱️ Duplo Clique (Qualquer Botão)
- **Função**: Abre popup de informações
- **Efeito**: Mostra níveis atuais e custos de upgrade
- **Sempre disponível**: Não faz mais upgrades

### 🖱️ Clique Direito Simples
- **Função**: Abre popup de informações
- **Mostra**: Níveis atuais e custos de upgrade

## Configurações Padrão

### 🔌 Upgrades de Cabos
| Nível | Nome | Custo | Cabos |
|-------|------|-------|--------|
| 1 | 2 Portas | 0 | 2 |
| 2 | 3 Portas | 40 | 3 |
| 3 | 4 Portas | 80 | 4 |
| 4 | 5 Portas | 150 | 5 |
| 5 | 6 Portas | 300 | 6 |

### ⚡ Upgrades de Velocidade
| Nível | Nome | Custo | Velocidade |
|-------|------|-------|------------|
| 1 | 10 MB/s | 0 | 10 MB/s |
| 2 | 20 MB/s | 60 | 20 MB/s |
| 3 | 35 MB/s | 120 | 35 MB/s |
| 4 | 50 MB/s | 200 | 50 MB/s |
| 5 | 80 MB/s | 350 | 80 MB/s |

## Propriedades do Modem

### Propriedades Atuais
```csharp
// Valores atuais
public float InternetSpeed;           // Velocidade atual
public int MaxSimultaneousCables;     // Cabos atuais

// Informações de Cabos
public int CurrentCableLevel;         // Nível atual de cabos
public string CurrentCableLevelName;  // Nome do nível de cabos
public int NextCableUpgradeCost;      // Custo próximo upgrade cabos
public bool CanUpgradeCables();       // Pode fazer upgrade cabos?

// Informações de Velocidade
public int CurrentSpeedLevel;         // Nível atual de velocidade
public string CurrentSpeedLevelName;  // Nome do nível de velocidade
public int NextSpeedUpgradeCost;      // Custo próximo upgrade velocidade
public bool CanUpgradeSpeed();        // Pode fazer upgrade velocidade?
```

## Popup de Informações

### Conteúdo Exibido
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

## Estratégias de Jogo

### 🎯 Foco em Cabos
- **Quando usar**: Muitos devices spawnando simultaneamente
- **Vantagem**: Mais conexões paralelas
- **Desvantagem**: Downloads mais lentos

### 🎯 Foco em Velocidade
- **Quando usar**: Downloads grandes, poucos devices
- **Vantagem**: Downloads muito rápidos
- **Desvantagem**: Gargalo de conexões simultâneas

### 🎯 Estratégia Balanceada
- **Quando usar**: Gameplay geral
- **Alterna entre** upgrades de cabos e velocidade
- **Adapta-se** às situações do jogo

## Implementação Técnica

### Métodos Principais
```csharp
// Métodos públicos (chamáveis via UI)
public void UpgradeCables()
public void UpgradeSpeed()
public void ShowModemInfo()

// Upgrade de cabos (internos)
private void TryUpgradeCables()
private CableUpgradeConfig GetCurrentCableUpgrade()
private CableUpgradeConfig GetNextCableUpgrade()

// Upgrade de velocidade (internos)
private void TryUpgradeSpeed()
private SpeedUpgradeConfig GetCurrentSpeedUpgrade()
private SpeedUpgradeConfig GetNextSpeedUpgrade()

// Informações
public string GetCableUpgradeInfo()
public string GetSpeedUpgradeInfo()
```

### Estados Independentes
```csharp
private int currentCableLevelIndex = 0;   // Nível atual de cabos
private int currentSpeedLevelIndex = 0;   // Nível atual de velocidade
```

### Uso via Botões da UI
```csharp
// Configuração no Inspector do Button:
// 1. OnClick() → Adicionar evento (+)
// 2. Arraste o GameObject do Modem para "Object"
// 3. Selecione função no dropdown:
//    - Modem.UpgradeCables() para upgrade de cabos
//    - Modem.UpgradeSpeed() para upgrade de velocidade
//    - Modem.ShowModemInfo() para mostrar popup

// Uso programático:
if (Modem.Instance != null)
{
    Modem.Instance.UpgradeCables();    // Upgrade cabos
    Modem.Instance.UpgradeSpeed();     // Upgrade velocidade
    Modem.Instance.ShowModemInfo();    // Mostrar popup
}
```

## Compatibilidade

### ✅ Mantido
- Animações de upgrade
- Sistema de moedas
- Singleton pattern
- Popup de informações
- Logs de debug

### ✅ Melhorado
- Controles mais intuitivos
- Informações mais detalhadas
- Estratégia de jogo mais profunda
- Flexibilidade de upgrade

## Logs de Debug

### Upgrade de Cabos
```
[Modem] Upgrade de cabos realizado! Nível 3: 4 Portas (Cabos Simultâneos: 4)
[Modem] Dinheiro insuficiente para upgrade de cabos! Precisa de mais 20 coins para 4 Portas
```

### Upgrade de Velocidade
```
[Modem] Upgrade de velocidade realizado! Nível 2: 20 MB/s (Velocidade: 20MB/s)
[Modem] Dinheiro insuficiente para upgrade de velocidade! Precisa de mais 30 coins para 20 MB/s
```

## Configuração na Cena

### Inspector do Modem
1. **Cable Upgrades**: Array com configurações de cabos
2. **Speed Upgrades**: Array com configurações de velocidade
3. Ambos configuráveis no Inspector
4. Validação automática dos dados

### Exemplo de Configuração
```csharp
// No Inspector, configure:
Cable Upgrades[5]:
  [0] level=1, levelName="2 Portas", upgradeCost=0, maxSimultaneousCables=2
  [1] level=2, levelName="3 Portas", upgradeCost=40, maxSimultaneousCables=3
  // ... etc

Speed Upgrades[5]:
  [0] level=1, levelName="10 MB/s", upgradeCost=0, internetSpeedMBps=10
  [1] level=2, levelName="20 MB/s", upgradeCost=60, internetSpeedMBps=20
  // ... etc
```

## Benefícios do Sistema

### 🎮 Para o Jogador
- **Escolha estratégica**: Decide que aspecto melhorar
- **Flexibilidade**: Adapta-se ao estilo de jogo
- **Progressão**: Dois caminhos de evolução

### 🔧 Para o Desenvolvedor
- **Balanceamento**: Custos independentes
- **Configurabilidade**: Fácil ajuste via Inspector
- **Extensibilidade**: Fácil adicionar mais tipos de upgrade

### 🎯 Para o Gameplay
- **Profundidade**: Decisões mais interessantes
- **Rejogabilidade**: Diferentes estratégias por partida
- **Progressão**: Sensação de evolução contínua 