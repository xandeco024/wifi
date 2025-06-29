# Guia de Configuração do Sistema de Devices - Spawner-Controlled

## Visão Geral

O sistema foi **reorganizado para centralizar configurações no Spawner**:

- **Device.cs**: Apenas lógica de funcionamento + `deviceName` e `enableRotation`
- **DeviceSpawner.cs**: **TODAS** as configurações de gameplay (timer, pontos, cor, rotação)
- **Canvas Billboard**: Canvas sempre olha para a câmera automaticamente

**✅ Spawner controla tudo - Device só executa!**

## Estrutura do Prefab

Cada device deve seguir esta estrutura hierárquica:

```
Device_GameObject (pai)
├── Canvas (UI do timer) ← SEMPRE OLHA PARA A CÂMERA
│   └── Slider (barra de tempo)
└── Objeto3D (modelo 3D que gira)
    └── MeshRenderer/outros componentes
```

### Componentes Obrigatórios no GameObject Pai:
- **Device.cs** (apenas lógica + deviceName + enableRotation)
- **Collider** (para detecção de clique/conexão)
- **ClickableObject.cs** (adicionado automaticamente)

## Configuração do Device.cs (Mínima)

O Device.cs agora só tem configurações básicas:

```csharp
[Header("Device Settings")]
public string deviceName = "PC"; // Nome padrão (pode ser sobrescrito pelo spawner)
public bool enableRotation = true; // Se o objeto 3D deve girar

// TUDO MAIS É CONFIGURADO PELO SPAWNER:
// - timeLimit
// - timerColor  
// - pointsOnConnection
// - bonusPoints
// - rotationSpeed
```

## Configuração do DeviceSpawner (Completa)

**TODAS** as configurações de gameplay ficam no spawner:

```csharp
[System.Serializable]
public class DeviceSpawnConfig
{
    [Header("Device Prefab")]
    public GameObject devicePrefab; // Prefab com Device.cs
    
    [Header("Spawn Settings")]
    public float spawnWeight = 1f; // Probabilidade de spawn
    public float spawnInterval = 5f; // Intervalo entre spawns
    
    [Header("Device Configuration")]
    public string deviceName = "PC"; // Nome do device
    public float timeLimit = 10f; // Tempo limite
    public Color timerColor = Color.green; // Cor do timer
    public int pointsOnConnection = 10; // Pontos por conexão
    public int bonusPoints = 5; // Bonus por velocidade
    
    [Header("Animation Settings")]
    public float rotationSpeed = 30f; // Velocidade de rotação
}
```

## Como o Sistema Funciona

### 1. Criação de Device
1. **Crie prefab** com estrutura hierárquica + Device.cs básico
2. **Configure spawner** com TODAS as configurações de gameplay
3. **Spawner aplica configurações** no device quando spawna

### 2. Canvas Billboard
- **Canvas sempre olha para a câmera** automaticamente
- **Não precisa configurar nada** - funciona automaticamente
- **Detecta câmera principal** ou primeira câmera encontrada

### 3. Spawn e Configuração
1. Spawner instancia prefab
2. **Spawner configura device** com `ConfigureDevice()`
3. Device funciona com configurações aplicadas
4. Canvas se orienta para câmera automaticamente

## Fluxo de Configuração Simplificado

### 1. Crie o Prefab Base
```csharp
// Device.cs no prefab - configuração mínima:
deviceName = "PC Base" // (será sobrescrito pelo spawner)
enableRotation = true
```

### 2. Configure Spawner com Variações
```csharp
// Config 1 - PC Básico
{
    devicePrefab = PC_Prefab,
    spawnWeight = 1f,
    spawnInterval = 5f,
    deviceName = "PC Básico",
    timeLimit = 10f,
    timerColor = Color.green,
    pointsOnConnection = 10,
    bonusPoints = 5,
    rotationSpeed = 30f
}

// Config 2 - PC Gamer (mesmo prefab, configurações diferentes)
{
    devicePrefab = PC_Prefab, // MESMO PREFAB!
    spawnWeight = 0.3f,
    spawnInterval = 8f,
    deviceName = "PC Gamer",
    timeLimit = 15f,
    timerColor = Color.blue,
    pointsOnConnection = 20,
    bonusPoints = 10,
    rotationSpeed = 45f
}
```

## Exemplo Prático Completo

### 1. Prefab "Device_Base"
```csharp
// Device.cs - configuração mínima:
deviceName = "Device" // Será sobrescrito
enableRotation = true // Controla se pode girar
```

### 2. Spawner Configurations
```csharp
// Array no DeviceSpawner:
DeviceSpawnConfig[] configs = {
    // PC Rápido
    new DeviceSpawnConfig {
        devicePrefab = Device_Base_Prefab,
        spawnWeight = 1f,
        spawnInterval = 3f,
        deviceName = "PC Rápido",
        timeLimit = 8f,
        timerColor = Color.yellow,
        pointsOnConnection = 15,
        bonusPoints = 8,
        rotationSpeed = 60f
    },
    
    // Servidor Lento
    new DeviceSpawnConfig {
        devicePrefab = Device_Base_Prefab, // MESMO PREFAB
        spawnWeight = 0.2f,
        spawnInterval = 10f,
        deviceName = "Servidor",
        timeLimit = 20f,
        timerColor = Color.red,
        pointsOnConnection = 50,
        bonusPoints = 25,
        rotationSpeed = 15f
    }
};
```

## Canvas Billboard Automático

O canvas sempre olha para a câmera:

```csharp
// No Device.cs - Update():
if (timerCanvas != null && mainCamera != null)
{
    timerCanvas.transform.LookAt(
        timerCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
        mainCamera.transform.rotation * Vector3.up
    );
}
```

**✅ Funciona automaticamente - não precisa configurar nada!**

## Métodos Úteis

### Device.cs
- `ConfigureDevice(name, timer, points, bonus, color, rotSpeed)` - Configuração completa (chamado pelo spawner)
- `SetObject3D(GameObject obj)` - Define objeto 3D manualmente
- `ForceDestroy()` - Destruição imediata
- `DeviceName` - Nome atual do device

### DeviceSpawner.cs
- `GetActiveDeviceCount()` - Devices ativos
- `GetSpawnStats()` - Estatísticas para debug
- `DestroyAllDevices()` - Limpa todos devices

## Vantagens da Centralização no Spawner

✅ **Controle centralizado** - Todas configurações em um lugar  
✅ **Reutilização de prefabs** - Um prefab, múltiplas configurações  
✅ **Balanceamento fácil** - Ajusta valores sem tocar nos prefabs  
✅ **Canvas billboard** - Sempre visível para o jogador  
✅ **Device simples** - Apenas lógica, sem configurações  
✅ **Configuração visual** - Ranges e tooltips no spawner  

## Troubleshooting

### Canvas não olha para câmera
- Sistema detecta automaticamente Camera.main ou primeira câmera
- Verifique se há uma câmera na cena com tag "MainCamera"

### Device não gira
- Verifique `enableRotation = true` no Device.cs do prefab
- Verifique `rotationSpeed > 0` na configuração do spawner

### Device não spawna
- Verifique se todas as configurações são válidas (nome não vazio, valores > 0)
- Veja logs do spawner para erros de validação

### Configurações não aplicam
- Spawner sempre sobrescreve configurações do prefab
- Use `GetDebugInfo()` para verificar valores aplicados

### Timer não aparece
- Verifique estrutura hierárquica: Canvas filho do GameObject pai
- Canvas deve ter Slider como filho 