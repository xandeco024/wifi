# 🏆 Guia de Configuração do Sistema de Score e Coins

## 📋 Visão Geral

Sistema completo de pontuação e dinheiro que se integra automaticamente com o sistema de devices. Cada conexão bem-sucedida dá **pontos** e **dinheiro**, com bonus por velocidade!

## 🔧 Configuração Rápida

### 1. **DeviceSpawner - Configuração de Recompensas**

No DeviceSpawner, cada configuração de device agora tem:

```csharp
[Header("Device Settings")]
public float timeLimit = 10f;        // Tempo limite
public int pointsOnConnection = 10;  // Pontos por conexão
public int bonusPoints = 5;          // Bonus points (legacy)
public int coinReward = 8;           // 💰 NOVO: Dinheiro por conexão
```

**✅ Na cena, configure as recompensas para cada tipo de device:**
- **PC Básico**: 10 pontos, 5 coins
- **PC Gamer**: 20 pontos, 10 coins  
- **Servidor**: 50 pontos, 25 coins

### 2. **UI Setup - TextMeshProUGUI**

Crie elementos UI com **TextMeshPro**:

```
Canvas (Screen Space - Overlay)
├── ScoreText (TextMeshProUGUI) ← "Score: 0"
├── CoinsText (TextMeshProUGUI) ← "Coins: 0"
├── ConnectionsText (TextMeshProUGUI) ← "Conexões: 0"
└── StatusText (TextMeshProUGUI) ← Mensagens de feedback
```

### 3. **Conectar UI ao Sistema**

Adicione o script **UIManager** a um GameObject e arraste os elementos UI:

```csharp
// No Inspector do UIManager:
Score Text → ScoreText (TextMeshProUGUI)
Coins Text → CoinsText (TextMeshProUGUI)
Connections Text → ConnectionsText (TextMeshProUGUI)
Status Text → StatusText (TextMeshProUGUI)
```

**✅ O UIManager conecta automaticamente tudo ao ScoreAndCoinsManager!**

## 🎮 Como Funciona

### **Fluxo Automático:**
1. **Device spawna** → Conecta eventos automaticamente
2. **Player conecta device** → Calcula recompensas
3. **Bonus por velocidade** → Se conectou rápido (>70% tempo restante)
4. **Atualiza UI** → Animações automáticas
5. **Feedback visual** → Popups e efeitos

### **Recompensas Calculadas:**
```csharp
// Recompensa base
int baseScore = device.GetConnectionPoints();  // Do spawner config
int baseCoins = device.GetCoinReward();        // Do spawner config

// Speed bonus (se >70% tempo restante)
if (hasSpeedBonus) {
    finalScore = baseScore * 1.5f;
    finalCoins = baseCoins * 1.5f;
    // Mostra "SPEED BONUS!" na UI
}
```

## 🎯 Configuração Detalhada

### **1. DeviceSpawnConfig Completa**

```csharp
[System.Serializable]
public class DeviceSpawnConfig
{
    [Header("Device Prefab")]
    public GameObject devicePrefab;
    
    [Header("Spawn Settings")]
    public float spawnWeight = 1f;      // Probabilidade
    public float spawnInterval = 5f;    // Intervalo
    
    [Header("Recompensas")] 
    public float timeLimit = 10f;       // Tempo limite
    public int pointsOnConnection = 10; // 🏆 Pontos
    public int bonusPoints = 5;         // Bonus legacy
    public int coinReward = 8;          // 💰 Dinheiro
}
```

### **2. Múltiplas Configurações**

```csharp
// Exemplo: 3 tipos de device, mesmo prefab, recompensas diferentes
DeviceSpawnConfig[] configs = {
    // PC Básico - Fácil
    {
        devicePrefab = PCPrefab,
        timeLimit = 15f,
        pointsOnConnection = 10,
        coinReward = 5,
        spawnInterval = 3f
    },
    
    // PC Gamer - Médio  
    {
        devicePrefab = PCPrefab,        // MESMO PREFAB!
        timeLimit = 10f,
        pointsOnConnection = 20,
        coinReward = 10,
        spawnInterval = 5f
    },
    
    // Servidor - Difícil
    {
        devicePrefab = PCPrefab,        // MESMO PREFAB!
        timeLimit = 8f,
        pointsOnConnection = 50,
        coinReward = 25,
        spawnInterval = 8f
    }
};
```

### **3. UI Responsiva**

```csharp
// Animações automáticas quando valores mudam:
- Punch scale effect nos textos
- Color flash (amarelo para score, verde para coins) 
- Counter animation (números sobem suavemente)
- Speed bonus destaque (texto fica amarelo)
```

## 🔥 Features Avançadas

### **1. Sistema de Bonus**
- **Speed Bonus**: 1.5x recompensa se conectar com >70% tempo restante
- **Streak System**: Tracking de conexões consecutivas
- **Customizável**: Altere thresholds e multiplicadores

### **2. Persistência de Dados**
```csharp
// Salva automaticamente no PlayerPrefs
scoreManager.SaveData();  // Salva score, coins, conexões
scoreManager.LoadData();  // Carrega dados salvos
```

### **3. Popups Visuais (Opcional)**
- Crie prefabs de popup com TextMeshProUGUI
- Animações automáticas: scale, move up, fade out
- Popups separados para score e coins

### **4. Eventos para Integração**
```csharp
// Conecte outros sistemas aos eventos:
scoreManager.OnScoreChanged += (score) => UpdateLeaderboard(score);
scoreManager.OnCoinsChanged += (coins) => UpdateShop(coins);
scoreManager.OnSpeedBonusEarned += (bonus) => PlayBonusEffect();
```

## 🛠️ Setup Passo a Passo

### **Passo 1: Configure DeviceSpawner**
1. No **Game Manager** (DeviceSpawner), ajuste as configurações:
   - `timeLimit = 12.1f`
   - `pointsOnConnection = 10`
   - `coinReward = 8`

### **Passo 2: Crie UI Canvas**
1. **Create → UI → Canvas**
2. **Create → UI → Text - TextMeshPro** (4x)
3. Nomeie: ScoreText, CoinsText, ConnectionsText, StatusText
4. Posicione na tela

### **Passo 3: Adicione UIManager**
1. **Create → Empty GameObject → "UI Manager"**
2. **Add Component → UIManager**
3. Arraste os TextMeshProUGUI para os campos

### **Passo 4: Teste**
1. **Execute o jogo**
2. **Conecte devices** → Score e coins devem aparecer
3. **Conecte rapidamente** → Speed bonus deve ativar

## 🎨 Customização

### **Textos e Prefixos**
```csharp
// No ScoreAndCoinsManager:
scorePrefix = "Pontuação: ";
coinsPrefix = "Dinheiro: ";

// No UIManager:
connectionsText.text = $"Conexões: {total}";
```

### **Bonus Thresholds**
```csharp
// No ScoreAndCoinsManager:
speedBonusThreshold = 0.7f;        // 70% tempo restante
speedBonusMultiplier = 1.5f;       // 150% recompensa
```

### **Animações**
```csharp
// No ScoreAndCoinsManager:
useAnimations = true;
animationDuration = 0.3f;
```

## 📊 Estatísticas Disponíveis

```csharp
// Acesso público às estatísticas:
int currentScore = scoreManager.CurrentScore;
int currentCoins = scoreManager.CurrentCoins;
int totalConnections = scoreManager.TotalConnections;
int consecutiveConnections = scoreManager.ConsecutiveConnections;
```

## 🐛 Troubleshooting

### **Score/Coins não atualizam**
- ✅ Verifique se UIManager está na cena
- ✅ Confirme que TextMeshProUGUI estão conectados
- ✅ Veja logs: "Score text conectado!", "Coins text conectado!"

### **Recompensas erradas**
- ✅ Verifique `coinReward > 0` no DeviceSpawner config
- ✅ Confirme que `IsValid()` retorna true
- ✅ Veja logs de conexão no console

### **Bonus não ativa**
- ✅ Conecte devices rapidamente (>70% tempo restante)
- ✅ Veja logs: "SPEED BONUS!" 
- ✅ Ajuste `speedBonusThreshold` se necessário

### **UI não aparece**
- ✅ Use **TextMeshPro**, não Text legacy
- ✅ Canvas deve estar configurado corretamente
- ✅ Verifique se elementos estão ativos na hierarquia

## 🎯 Exemplo Completo

```csharp
// DeviceSpawner Config:
timeLimit = 10f
pointsOnConnection = 15  
coinReward = 8

// Player conecta com 80% tempo restante:
baseScore = 15
baseCoins = 8
hasSpeedBonus = true (80% > 70%)

// Resultado final:
finalScore = 15 * 1.5 = 22 pontos
finalCoins = 8 * 1.5 = 12 coins

// UI mostra:
"Score: 22" (com animação amarela)
"Coins: 12" (com animação verde)  
"SPEED BONUS! +7" (status text)
```

## 🚀 Sistema Pronto!

Agora você tem um sistema completo de pontuação e dinheiro que:

✅ **Se integra automaticamente** com devices  
✅ **Usa TextMeshProUGUI** para UI moderna  
✅ **Animações e feedback** visual  
✅ **Sistema de bonus** por velocidade  
✅ **Persistência** de dados  
✅ **Eventos** para integração  
✅ **Configuração fácil** no spawner  

**🎮 Conecte devices e ganhe pontos e dinheiro automaticamente!** 