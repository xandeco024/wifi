# 📥 Sistema de Download - Guia Completo

## 🎯 **Visão Geral**

O sistema de download transforma a experiência do jogo, onde os devices agora precisam fazer downloads de arquivos ao invés de apenas serem conectados dentro de um tempo limite.

## 🚀 **Como Funciona**

### 1. **Spawning de Devices**
- Cada device é criado com um tamanho de download aleatório (configurável no DeviceSpawner)
- Range de download: 50MB - 200MB (configurável)
- Timer de conexão continua existindo (tempo limite para conectar)

### 2. **Conexão**
- Quando conectado com cabo, o device **não é destruído imediatamente**
- Inicia o processo de download na velocidade da internet do modem
- Muda para estado `Downloading`

### 3. **Processo de Download**
- Velocidade configurável no Modem (MB/s)
- Progresso em tempo real no slider e texto
- Formato do texto: `"50% 49MB/100MB"`
- Slider mostra progresso do download (0-100%)

### 4. **Finalização**
- Quando download completa, device é destruído
- Ganha pontos e dinheiro
- Efeito visual de conclusão

## ⚙️ **Configuração**

### **DeviceSpawner**
```yaml
Download Settings:
  minDownloadSizeMB: 50      # Tamanho mínimo do download
  maxDownloadSizeMB: 200     # Tamanho máximo do download
```

### **Modem**
```yaml
Internet Speed:
  internetSpeedMBps: 10      # Velocidade em MB por segundo
```

### **Device (Automático)**
- `downloadSlider`: Mostra progresso do download
- `downloadText`: Texto com "X% YMB/ZMB"
- Estados: `Disconnected`, `Connecting`, `Downloading`, `Connected`, `Failed`

## 🎮 **Gameplay**

### **Fluxo do Jogo:**
1. Device spawna com download de 50-200MB
2. Jogador tem tempo limite para conectar
3. Conecta cabo → download inicia
4. Progresso visual em tempo real
5. Download completa → device destruído + pontos

### **Estratégia:**
- Devices com downloads menores = mais rápidos
- Velocidade do modem afeta todos os downloads
- Múltiplos downloads simultâneos possíveis

## 🔧 **Implementação Técnica**

### **Novos Campos no DeviceSpawnConfig:**
```csharp
[Header("Download Settings")]
[Range(10, 500)]
public int minDownloadSizeMB = 50;
[Range(10, 500)]
public int maxDownloadSizeMB = 200;
```

### **Novos Campos no Modem:**
```csharp
[Header("Internet Speed")]
[SerializeField] private float internetSpeedMBps = 10f;
```

### **Novos Campos no Device:**
```csharp
[SerializeField] private Slider downloadSlider;
[SerializeField] private TMPro.TextMeshProUGUI downloadText;
private float downloadedMB = 0f;
private bool isDownloading = false;
```

### **Novos Estados:**
```csharp
public enum DeviceState
{
    Disconnected,
    Connected,
    Failed,
    Connecting,
    Downloading  // NOVO
}
```

## 📊 **Interface Visual**

### **Slider:**
- Valor: 0.0 - 1.0 (progresso do download)
- Cor: Vermelho → Verde conforme progresso
- Smooth transitions

### **Texto:**
- Formato: `"50% 49MB/100MB"`
- Atualização em tempo real
- Font: TextMeshProUGUI

## 🔄 **Eventos e Callbacks**

### **Novos Eventos:**
- `OnDownloadStarted`: Quando download inicia
- `OnDownloadProgress`: Progresso do download
- `OnDownloadComplete`: Download finalizado

### **Compatibilidade:**
- Mantém todos os eventos existentes
- `OnDeviceConnected` ainda é chamado
- `OnDeviceDestroyed` quando download completa

## 🚫 **Tratamento de Erros**

### **Casos Especiais:**
- Modem não encontrado: velocidade padrão 10MB/s
- Componentes UI não encontrados: auto-busca
- Device destruído durante download: corrotina limpa

## 📈 **Escalabilidade**

### **Valores Recomendados:**
- **Iniciante:** 5-10MB/s, downloads 20-50MB
- **Intermediário:** 10-20MB/s, downloads 50-100MB
- **Avançado:** 20-50MB/s, downloads 100-300MB

### **Progressão:**
- Upgrade do modem aumenta velocidade
- Downloads maiores = mais pontos
- Balanceamento baseado em tempo vs recompensa

## 🎯 **Exemplo de Uso**

```csharp
// Configuração no DeviceSpawner
minDownloadSizeMB = 50;
maxDownloadSizeMB = 200;

// Configuração no Modem
internetSpeedMBps = 15f;

// Resultado:
// Device com 100MB download
// Velocidade 15MB/s
// Tempo de download: ~6.7 segundos
```

## ✅ **Checklist de Setup**

- [ ] DeviceSpawner com ranges de download configurados
- [ ] Modem com velocidade de internet definida
- [ ] Device prefab com Slider e TextMeshProUGUI
- [ ] Cena atualizada com novas configurações
- [ ] Testes de download em diferentes velocidades

---

**🎉 Agora o jogo está escalado com um sistema de download completo e visual!** 