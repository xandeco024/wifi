# 🔧 Sistema de Upgrade do Modem - Guia Completo

## 🎯 **Visão Geral**

O sistema de upgrade do modem permite melhorar as capacidades do modem através de níveis progressivos, aumentando o número de cabos simultâneos e a velocidade de internet.

## 🚀 **Como Funciona**

### **1. Níveis do Modem**
O modem possui 5 níveis configuráveis:

| Nível | Nome | Custo Upgrade | Cabos Simultâneos | Velocidade (MB/s) |
|-------|------|---------------|-------------------|-------------------|
| 1 | Básico | 0 | 2 | 10 |
| 2 | Intermediário | 50 | 3 | 15 |
| 3 | Avançado | 100 | 4 | 25 |
| 4 | Premium | 200 | 5 | 40 |
| 5 | Elite | 400 | 6 | 60 |

### **2. Processo de Upgrade**
1. **Duplo clique** no modem
2. Sistema verifica se há dinheiro suficiente
3. Se sim: Cobra o custo + faz upgrade + toca animação
4. Se não: Mostra mensagem de dinheiro insuficiente

### **3. Limitação de Cabos**
- Ao tentar iniciar um cabo, verifica limite atual
- Se atingiu o máximo, bloqueia criação de novos cabos
- Upgrade do modem aumenta esse limite

## ⚙️ **Configuração**

### **ModemLevelConfig (Estrutura de Dados)**
```csharp
[System.Serializable]
public class ModemLevelConfig
{
    [Header("Level Info")]
    public int level = 1;
    public string levelName = "Básico";
    
    [Header("Upgrade Cost")]
    public int upgradeCost = 50;
    
    [Header("Capabilities")]
    [Range(1, 10)]
    public int maxSimultaneousCables = 2;
    [Range(1f, 100f)]
    public float internetSpeedMBps = 10f;
}
```

### **Modem.cs (Propriedades)**
```csharp
// Propriedades baseadas no nível atual
public float InternetSpeed => GetCurrentLevel().internetSpeedMBps;
public int MaxSimultaneousCables => GetCurrentLevel().maxSimultaneousCables;
public int CurrentLevel => GetCurrentLevel().level;
public string CurrentLevelName => GetCurrentLevel().levelName;
public int NextUpgradeCost => CanUpgrade() ? GetNextLevel().upgradeCost : -1;
public bool CanUpgrade() => currentLevelIndex < modemLevels.Length - 1;
```

## 🎮 **Gameplay**

### **Progressão Estratégica:**
- **Início:** 2 cabos simultâneos, 10MB/s
- **Estratégia:** Economizar dinheiro para primeiro upgrade
- **Benefícios:** Mais cabos = mais devices simultâneos = mais dinheiro
- **Velocidade:** Downloads mais rápidos = rotatividade maior

### **Balanceamento:**
- Custos crescem exponencialmente (50 → 100 → 200 → 400)
- Benefícios também crescem (cabos e velocidade)
- Incentiva progressão gradual

## 🔧 **Implementação Técnica**

### **Arquivos Modificados:**
1. **Modem.cs**
   - Estrutura `ModemLevelConfig`
   - Sistema de níveis e upgrade
   - Integração com duplo clique

2. **ScoreAndCoinsManager.cs**
   - Método `SpendCoins(int amount)`
   - Método `GetCurrentCoins()`

3. **CableController.cs**
   - Verificação de limite de cabos
   - Método `GetConnectedCablesCount()`

### **Integração com Sistemas Existentes:**
- **Download:** Usa `modem.InternetSpeed`
- **Cabos:** Respeita `modem.MaxSimultaneousCables`
- **UI:** Integra com `ScoreAndCoinsManager`
- **Animação:** Usa animação existente do modem

## 🎨 **Feedback Visual**

### **Animação de Upgrade:**
- Pulo + rotação do modem
- Shake da câmera
- Efeitos visuais de sucesso

### **Limitação de Cabos:**
- Debug log quando limite atingido
- Bloqueio visual do arraste de cabos

## 🔄 **Fluxo do Sistema**

### **Upgrade Bem-sucedido:**
```
Duplo clique → Verifica dinheiro → Cobra custo → 
Incrementa nível → Animação → Log de sucesso
```

### **Upgrade Falhado:**
```
Duplo clique → Verifica dinheiro → 
Insuficiente → Log de erro
```

### **Limitação de Cabos:**
```
Clique no modem → Conta cabos conectados → 
Se >= limite → Bloqueia → Log de limite
```

## 📊 **Monitoramento**

### **Debug Logs:**
- Upgrade realizado com detalhes
- Dinheiro insuficiente
- Limite de cabos atingido
- Nível máximo atingido

### **Propriedades Públicas:**
```csharp
// Para inspecionar no editor ou via código
modem.CurrentLevel
modem.CurrentLevelName
modem.MaxSimultaneousCables
modem.InternetSpeed
modem.NextUpgradeCost
modem.CanUpgrade()
```

## ✅ **Checklist de Funcionamento**

- [ ] Duplo clique no modem tenta upgrade
- [ ] Verifica dinheiro antes de fazer upgrade
- [ ] Cobra custo correto do upgrade
- [ ] Incrementa nível após pagamento
- [ ] Toca animação de upgrade
- [ ] Limita cabos simultâneos corretamente
- [ ] Velocidade de download atualizada
- [ ] Debug logs informativos
- [ ] Progressão balanceada

## 🎯 **Exemplo de Progressão**

### **Cenário Típico:**
1. **Início:** Nível 1 - 2 cabos, 10MB/s
2. **50 coins:** Upgrade para Nível 2 - 3 cabos, 15MB/s
3. **+100 coins:** Upgrade para Nível 3 - 4 cabos, 25MB/s
4. **Continue...** até Nível 5 Elite

### **Benefício Composto:**
- Mais cabos → Mais devices simultâneos
- Velocidade maior → Downloads mais rápidos
- Resultado: Muito mais dinheiro por minuto

---

**🎉 Sistema de progressão completo que escalona o gameplay e mantém o jogador engajado!** 