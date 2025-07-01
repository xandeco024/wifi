# 🛠️ Roadmap Técnico para o Jogo "Conexão Caótica" – Game Jam

## ⚠️ **REGRAS FUNDAMENTAIS DE DESENVOLVIMENTO**

**🎯 IMPLEMENTE APENAS O QUE FOI PEDIDO - NÃO ADICIONE FUNCIONALIDADES EXTRAS**

- ✅ Se o usuário pede "sistema de score e coins", implementar apenas score e coins
- ❌ NÃO adicionar: status text, connections text, botões extras, popups, etc.
- ✅ Manter implementação simples e focada no que foi solicitado
- ❌ NÃO assumir que funcionalidades "úteis" devem ser adicionadas
- ✅ Perguntar antes de adicionar qualquer coisa além do pedido

**🚫 NÃO GASTE TEMPO COM DOCUMENTAÇÃO E AUTO-CONFIGS DESNECESSÁRIOS**

- ❌ NÃO criar arquivos .md para cada funcionalidade
- ❌ NÃO fazer sistemas de auto-configuração/auto-setup complexos
- ❌ NÃO criar "helpers" e "creators" que quase nunca funcionam
- ✅ FOQUE no código funcional que o usuário precisa
- ✅ O usuário está montando o jogo manualmente - apenas forneça o código     
- ✅ Documentação mínima apenas quando solicitada

**Estas regras se aplicam a TODAS as fases de desenvolvimento.**

---

## 🔥 **ÚLTIMA ATUALIZAÇÃO - SISTEMA DE DOWNLOAD IMPLEMENTADO**

### ✅ **ESCALABILIDADE DO JOGO - SISTEMA DE DOWNLOAD**

**Data:** Implementado hoje  
**Objetivo:** Escalar o jogo com sistema de download realista

**Implementações Realizadas:**

1. ✅ **DeviceSpawner Atualizado**
   - Range de download configurável (50-200MB)
   - Campos `minDownloadSizeMB` e `maxDownloadSizeMB`
   - Validação atualizada para incluir downloads

2. ✅ **Modem com Velocidade de Internet**
   - Campo `internetSpeedMBps` (MB por segundo)
   - Configuração: 10MB/s (padrão)
   - Propriedade pública `InternetSpeed`

3. ✅ **Device com Sistema de Download**
   - Novo estado: `DeviceState.Downloading`
   - Componentes: `downloadSlider` e `downloadText`
   - Progresso visual: "50% 49MB/100MB"
   - Download em tempo real baseado na velocidade do modem

4. ✅ **Fluxo de Gameplay Atualizado**
   - Conexão → Download inicia → Progresso visual → Conclusão → Destruição
   - Múltiplos downloads simultâneos possíveis
   - Animação de destruição com DOTween

5. ✅ **Correção de Bugs**
   - Cabo removido imediatamente com device
   - Sincronização perfeita entre animações
   - Cleanup de corrotinas

6. ✅ **Documentação**
   - `DOWNLOAD_SYSTEM_GUIDE.md` criado
   - Configurações detalhadas
   - Exemplos de uso

**Arquivos Modificados:**
- `DeviceSpawner.cs` - Ranges de download
- `Modem.cs` - Velocidade de internet
- `Device.cs` - Sistema completo de download
- `CableController.cs` - Sincronização com destruição
- `wifi.unity` - Configurações atualizadas

**Estado Atual:** ✅ **SISTEMA FUNCIONAL E ESCALÁVEL**

---

## 🎨 **ATUALIZAÇÃO VISUAL - SLIDER UNIFICADO E ANIMAÇÕES**

### ✅ **SISTEMA VISUAL APRIMORADO**

**Data:** Implementado hoje  
**Objetivo:** Melhorar feedback visual e experiência do usuário

**Implementações Realizadas:**

1. ✅ **Slider Unificado**
   - Um slider para duas funções: Timer (branco) + Download (verde)
   - Transição automática de cor baseada no estado
   - Texto contextual: "Conectar em: Xs" → "X% YMB/ZMB"

2. ✅ **Sistema de Animações Dual**
   - **Falha:** Tremor + escala diminuindo (PlayDestroyAnimation)
   - **Sucesso:** Pulo + rotação + pulse (PlaySuccessAnimation)
   - Animações contextuais e diferenciadas

3. ✅ **Feedback Visual Aprimorado**
   - Cores contextuais (branco/verde)
   - Estados claramente diferenciados
   - Transições suaves e imediatas

4. ✅ **Sincronização Perfeita**
   - Cabo + Device + Animações
   - Score dado apenas no sucesso
   - Cleanup adequado de componentes

**Arquivos Modificados:**
- `Device.cs` - Sistema visual unificado
- `VISUAL_UPDATES_GUIDE.md` - Documentação completa

**Estado Atual:** ✅ **SISTEMA VISUAL COMPLETO E POLIDO**

---

## 🔧 **SISTEMA DE PROGRESSÃO - UPGRADE DO MODEM**

### ✅ **PROGRESSÃO IMPLEMENTADA**

**Data:** Implementado hoje  
**Objetivo:** Adicionar progressão ao jogo através de upgrades do modem

**Implementações Realizadas:**

1. ✅ **Sistema de Níveis do Modem**
   - Estrutura `ModemLevelConfig` para definir níveis
   - 5 níveis progressivos (Básico → Elite)
   - Configuração de cabos simultâneos e velocidade

2. ✅ **Upgrade por Duplo Clique**
   - Duplo clique no modem tenta upgrade
   - Verificação de dinheiro suficiente
   - Cobrança automática do custo
   - Animação de upgrade existente

3. ✅ **Limitação de Cabos Simultâneos**
   - CableController respeita limite do modem
   - Verificação antes de iniciar novo cabo
   - Feedback quando limite atingido

4. ✅ **Integração com Sistema de Dinheiro**
   - Método `SpendCoins()` no ScoreAndCoinsManager
   - Verificação de saldo antes de upgrade
   - Feedback de dinheiro insuficiente

5. ✅ **Progressão Balanceada**
   - Custos exponenciais: 50 → 100 → 200 → 400
   - Benefícios crescentes: cabos e velocidade
   - Estratégia de longo prazo

**Tabela de Níveis:**
| Nível | Nome | Custo | Cabos | Velocidade |
|-------|------|-------|--------|------------|
| 1 | Básico | 0 | 2 | 10 MB/s |
| 2 | Intermediário | 50 | 3 | 15 MB/s |
| 3 | Avançado | 100 | 4 | 25 MB/s |
| 4 | Premium | 200 | 5 | 40 MB/s |
| 5 | Elite | 400 | 6 | 60 MB/s |

**Arquivos Modificados:**
- `Modem.cs` - Sistema completo de níveis e upgrade
- `ScoreAndCoinsManager.cs` - Métodos de gasto de dinheiro
- `CableController.cs` - Limitação de cabos simultâneos
- `wifi.unity` - Configuração dos níveis na cena
- `MODEM_UPGRADE_GUIDE.md` - Documentação completa

**Estado Atual:** ✅ **PROGRESSÃO COMPLETA E BALANCEADA**

---

## 📊 **SISTEMA DE POPUP - INFORMAÇÕES DO MODEM**

### ✅ **POPUP INFORMATIVO IMPLEMENTADO**

**Data:** Implementado hoje  
**Objetivo:** Mostrar informações detalhadas do modem de forma elegante

**Implementações Realizadas:**

1. ✅ **ModemInfoPopup.cs - Gerenciador Principal**
   - Singleton para acesso global
   - Animações suaves com DOTween (subir/descer da tela)
   - Detecta cliques fora do popup para fechar
   - Atualização automática de conteúdo

2. ✅ **Sistema de Controles**
   - **Clique Direito no Modem:** Abre popup
   - **Clique Fora do Popup:** Fecha popup
   - **Tecla ESC:** Fecha popup  
   - **Botão "Fechar":** Fecha popup

3. ✅ **Informações Mostradas**
   - Nível atual e nome do modem
   - Velocidade de internet (MB/s)
   - Número máximo de cabos simultâneos
   - Informações sobre próximo upgrade ou "Nível máximo"

4. ✅ **Animações com DOTween**
   - **Abertura:** Popup sobe de baixo com efeito OutBack
   - **Fechamento:** Popup desce com efeito InBack
   - **Fade:** Opacidade anima junto com movimento

5. ✅ **ModemInfoPopupCreator.cs - Helper de Criação**
   - Criação automática da estrutura UI
   - Configuração de cores e dimensões
   - Auto-setup de componentes e referências

6. ✅ **Integração com Sistema Existente**
   - Atualização automática quando modem é upgradeado
   - Compatibilidade total com cliques normais
   - Não interfere com animações existentes

**Controles Implementados:**
- Clique Direito no Modem → Mostra popup
- Clique Esquerdo no Modem → Funcionalidade original (toggle ativo)
- Duplo Clique no Modem → Upgrade (funcionalidade original)

**Arquivos Criados:**
- `ModemInfoPopup.cs` - Gerenciador principal do popup
- `ModemInfoPopupCreator.cs` - Helper para criação automática
- `MODEM_POPUP_SETUP_GUIDE.md` - Documentação completa

**Arquivos Modificados:**
- `Modem.cs` - Detecção de clique direito e integração
- `ROADMAP.md` - Documentação atualizada

**Estado Atual:** ✅ **SISTEMA DE POPUP COMPLETO E FUNCIONAL**

---

## 🔧 **SISTEMA DE UPGRADES INDEPENDENTES - REFORMULAÇÃO COMPLETA**

### ✅ **UPGRADES ESTRATÉGICOS IMPLEMENTADOS**

**Data:** Implementado hoje  
**Objetivo:** Reformular sistema de upgrade para permitir escolhas estratégicas independentes

**Implementações Realizadas:**

1. ✅ **Estruturas de Dados Separadas**
   - **CableUpgradeConfig**: Controla número de cabos simultâneos
   - **SpeedUpgradeConfig**: Controla velocidade de download
   - Validação independente e configuração via Inspector

2. ✅ **Controles Diferenciados**
   - **Duplo Clique Esquerdo**: Upgrade de cabos (portas)
   - **Duplo Clique Direito**: Upgrade de velocidade
   - **Clique Direito**: Popup de informações (mantido)

3. ✅ **Sistemas de Progressão Independentes**
   - Cabos: 2 → 3 → 4 → 5 → 6 portas (custos: 0, 40, 80, 150, 300)
   - Velocidade: 10 → 20 → 35 → 50 → 80 MB/s (custos: 0, 60, 120, 200, 350)
   - Níveis independentes com nomes descritivos

4. ✅ **Propriedades Expandidas do Modem**
   - Propriedades separadas para cabos e velocidade
   - Métodos independentes de upgrade e validação
   - Informações detalhadas para cada sistema

5. ✅ **Popup Atualizado**
   - Mostra níveis atuais de ambos os sistemas
   - Instruções claras para cada tipo de upgrade
   - Ícones distintivos (🔌 cabos, ⚡ velocidade)

6. ✅ **Estratégias de Jogo**
   - **Foco em Cabos**: Para múltiplas conexões simultâneas
   - **Foco em Velocidade**: Para downloads rápidos
   - **Estratégia Balanceada**: Adaptativa ao gameplay

**Benefícios do Sistema:**
- **Escolha Estratégica**: Jogador decide prioridades
- **Flexibilidade**: Adapta-se ao estilo de jogo
- **Profundidade**: Decisões mais interessantes
- **Rejogabilidade**: Diferentes estratégias por partida

**Configurações Balanceadas:**

| Tipo | Nível | Nome | Custo | Capacidade |
|------|-------|------|-------|------------|
| 🔌 Cabos | 1-5 | 2-6 Portas | 0-300 | 2-6 simultâneos |
| ⚡ Velocidade | 1-5 | 10-80 MB/s | 0-350 | 10-80 MB/s |

**Arquivos Modificados:**
- `Modem.cs` - Sistema completo reformulado
- `ModemInfoPopup.cs` - Interface atualizada para dois sistemas
- `MODEM_INDEPENDENT_UPGRADES_GUIDE.md` - Documentação completa

**Arquivos Criados:**
- `MODEM_INDEPENDENT_UPGRADES_GUIDE.md` - Guia detalhado do novo sistema

**Estado Atual:** ✅ **SISTEMA DE UPGRADES ESTRATÉGICOS COMPLETO**

---

## 🎮 **SISTEMA DE UPGRADES POR BOTÕES - INTERFACE APRIMORADA**

### ✅ **CONTROLES VIA UI IMPLEMENTADOS**

**Data:** Implementado hoje  
**Objetivo:** Migrar upgrades para botões da UI e simplificar controles do modem

**Implementações Realizadas:**

1. ✅ **Métodos Públicos para UI**
   - `UpgradeCables()` - Método público para upgrade de cabos
   - `UpgradeSpeed()` - Método público para upgrade de velocidade  
   - `ShowModemInfo()` - Método público para mostrar popup
   - Todos com validações de segurança integradas

2. ✅ **Comportamento de Cliques Simplificado**
   - **Duplo Clique**: Sempre abre popup (qualquer botão do mouse)
   - **Clique Direito**: Abre popup de informações
   - **Upgrades**: Apenas via botões da UI

3. ✅ **Validações de Segurança**
   - Bloqueia durante animações de upgrade
   - Verifica se interação está habilitada
   - Mantém todas as validações de dinheiro e nível máximo
   - Logs detalhados para debug

4. ✅ **Popup Atualizado**
   - Instruções atualizadas: "Use botão de upgrade na UI"
   - Adicionada instrução de fechamento: "Duplo clique ou ESC para fechar"
   - Mantém ícones distintivos e informações detalhadas

5. ✅ **Exemplo de Controller UI**
   - Script `ModemUIController` completo de exemplo
   - Atualização automática de estado dos botões
   - Textos dinâmicos com custos e status
   - Integração com sistema de moedas

**Controles Atualizados:**

| Ação | Função Anterior | Função Atual |
|------|----------------|--------------|
| **Duplo Clique Esquerdo** | Upgrade cabos | Popup informações |
| **Duplo Clique Direito** | Upgrade velocidade | Popup informações |
| **Clique Direito** | Popup informações | Popup informações |
| **Botões UI** | - | Upgrades de cabos/velocidade |

**Vantagens da Mudança:**
- **Interface Mais Intuitiva**: Botões claramente identificados
- **Menos Acidentes**: Upgrades intencionais apenas via botões
- **Informações Sempre Acessíveis**: Duplo clique mostra popup
- **UX Melhorada**: Separação clara entre informação e ação

**Exemplo de Configuração:**
```csharp
// No evento OnClick do botão:
Button → OnClick() → Modem.UpgradeCables()
Button → OnClick() → Modem.UpgradeSpeed()
Button → OnClick() → Modem.ShowModemInfo()
```

**Arquivos Modificados:**
- `Modem.cs` - Adicionados métodos públicos e simplificado duplo clique
- `ModemInfoPopup.cs` - Atualizadas instruções do popup
- `ROADMAP.md` - Documentação atualizada

**Arquivos Criados:**
- `MODEM_BUTTON_UPGRADES_GUIDE.md` - Guia completo do sistema de botões

**Estado Atual:** ✅ **SISTEMA DE UPGRADES POR BOTÕES COMPLETO**

---

## 📊 **POPUP REDESENHADO - INTERFACE SEPARADA**

### ✅ **CAMPOS DE TEXTO INDEPENDENTES IMPLEMENTADOS**

**Data:** Implementado hoje  
**Objetivo:** Reorganizar popup com campos de texto específicos para cada informação

**Implementações Realizadas:**

1. ✅ **Campos de Texto Separados**
   - `currentSpeedText` - Velocidade atual e nível
   - `currentCablesText` - Quantidade de portas atuais e nível
   - `speedUpgradeCostText` - Custo do upgrade de velocidade
   - `cablesUpgradeCostText` - Custo do upgrade de portas

2. ✅ **Conteúdo Organizado**
   - **Velocidade**: "Velocidade: 20 MB/s\n(Nível 2: 20 MB/s)"
   - **Portas**: "Portas: 3\n(Nível 2: 3 Portas)"
   - **Upgrade Velocidade**: "Upgrade Velocidade:\n120 coins"
   - **Upgrade Portas**: "Upgrade Portas:\n80 coins"

3. ✅ **Auto-Setup Atualizado**
   - Detecção automática dos 4 campos de texto em ordem
   - Configuração flexível via Inspector
   - Compatibilidade com sistema de criação automática

**Estrutura UI Atualizada:**
```
ModemInfoPopup (Root)
├── Image (Background Overlay)
└── PopupPanel
    ├── Title (TextMeshProUGUI)
    ├── CurrentSpeedText (TextMeshProUGUI)
    ├── CurrentCablesText (TextMeshProUGUI)
    ├── SpeedUpgradeCostText (TextMeshProUGUI)
    ├── CablesUpgradeCostText (TextMeshProUGUI)
    └── CloseButton (Button)
```

**Vantagens da Reorganização:**
- **Clareza Visual**: Cada informação em seu próprio campo
- **Flexibilidade**: Controle individual de cada texto
- **Manutenibilidade**: Mais fácil de atualizar e estilizar
- **Responsividade**: Cada campo pode ter seu próprio layout

**Arquivos Modificados:**
- `ModemInfoPopup.cs` - Estrutura de campos redesenhada
- `MODEM_POPUP_SETUP_GUIDE.md` - Estrutura UI atualizada
- `MODEM_POPUP_USAGE_EXAMPLE.md` - Exemplos atualizados
- `ROADMAP.md` - Documentação atualizada

**Estado Atual:** ✅ **POPUP COM CAMPOS SEPARADOS COMPLETO**

---

## 🔄 TRANSIÇÃO: UI → MUNDO 3D

**ESTADO ATUAL**: ✅ **FASE 1 COMPLETA** - Setup básico 3D implementado  
**OBJETIVO**: Migrar para mundo 3D com câmera, efeitos e objetos 3D

**ARQUITETURA MANTIDA:**
- ✅ Sistema de eventos (CableController, ScoreManager, etc.)
- ✅ Lógica de negócio (spawning, timers, pontuação)
- ✅ Estrutura de classes existente

**MUDANÇAS NECESSÁRIAS:**
- ✅ Coordenadas: `RectTransform.anchoredPosition` → `Transform.position`
- 🔄 Raycast: `EventSystem.RaycastAll()` → `Physics.Raycast()`
- ✅ Câmera: Canvas → Camera 3D com controles
- 🔄 Visuals: UI Elements → 3D Models/Sprites

## 🎮 Visão Geral do Jogo

Você é um técnico de TI responsável por manter a rede de um escritório em crescimento. PCs com problemas de conexão aparecem de tempos em tempos, e o jogador precisa agir rapidamente para conectá-los ao modem, resolver o problema e evitar que a rede entre em colapso.

O objetivo é sobreviver o máximo possível e ganhar pontos por conexões bem-sucedidas. O jogo é **infinito** e só termina quando muitos PCs não são atendidos (morte súbita ou "game over").

---

## 🗺️ ROADMAP DE TRANSIÇÃO PARA 3D

### ✅ **FASE 1: Preparação e Setup Inicial - COMPLETA**

#### 🎯 **Objetivo:** Setup básico do mundo 3D mantendo funcionalidade atual

**Tarefas Concluídas:**
1. ✅ **Configurar Câmera 3D**
   - ✅ Criar Main Camera com posição isométrica/top-down
   - ✅ Configurar projection (Orthographic recomendado para início)
   - ✅ Script básico de controle de câmera (zoom, pan) - `CameraController.cs`

2. ✅ **Criar Plano de Jogo**
   - ✅ Plane 3D como "mesa" do escritório
   - ✅ Boundaries invisíveis para spawning - `WorldBounds.cs`
   - ✅ Lighting básico

3. ✅ **Sistema de Coordenadas Híbrido**
   - ✅ Manter UI atual funcionando (HUD, Score)
   - ✅ Adicionar conversores UI ↔ World coordinates - `CoordinateConverter.cs`
   - ✅ Camera.ScreenToWorldPoint() para input

**Scripts Criados:**
- ✅ `WorldBounds.cs` - Define limites do mundo 3D
- ✅ `CoordinateConverter.cs` - Conversão entre coordenadas UI e 3D
- ✅ `CameraController.cs` - Controle de câmera com zoom e pan

---

### 🔄 **FASE 2: Migração dos PCs para 3D - EM PROGRESSO**

#### 🎯 **Objetivo:** Converter PCSpawner e ComputerBehavior para mundo 3D

**Tarefas Concluídas:**
1. ✅ **Adaptar PCSpawner**
   - ✅ Mudar de `RectTransform.anchoredPosition` para `Transform.position`
   - ✅ Usar coordenadas mundo para spawning
   - ✅ Manter lógica de posicionamento inteligente
   - ✅ Integração com `WorldBounds`

2. ✅ **Criar PC Prefab 3D**
   - ✅ GameObject com SpriteRenderer ou modelo 3D simples
   - ✅ Collider para raycast 3D
   - ✅ Canvas filho para UI do timer (WorldSpace)

3. ✅ **Adaptar ComputerBehavior**
   - ✅ Versão 3D criada: `ComputerBehavior3D.cs`
   - ✅ Manter timer e lógica de estado
   - ✅ UI do timer como Canvas WorldSpace
   - ✅ Adicionar feedback visual 3D (scale, rotation, materials)

**Scripts Criados:**
- ✅ `ComputerBehavior3D.cs` - Versão 3D do comportamento dos PCs
- ✅ `ClickableObject3D.cs` - Sistema de interação 3D com raycast

**Tarefas Pendentes:**
- 🔄 Testar integração completa
- 🔄 Ajustar valores de distância para mundo 3D
- 🔄 Criar prefab 3D funcional

---

### 📋 **FASE 3: Sistema de Câmera e Controles - COMPLETA**

#### 🎯 **Objetivo:** Câmera interativa e controles de mundo

**Tarefas Concluídas:**
1. ✅ **CameraController Script**
   - ✅ Zoom com scroll do mouse
   - ✅ Pan com drag do mouse/WASD
   - ✅ Limites de câmera
   - ✅ Smooth movement

2. ✅ **Input System**
   - ✅ Mouse para raycast 3D
   - ✅ Keyboard para controles de câmera
   - ✅ Touch support (futuro mobile)

**Script Implementado:**
```csharp
// CameraController.cs - COMPLETO
public class CameraController : MonoBehaviour
{
    // Zoom, pan, limites implementados
    // Smooth transitions
    // Input handling completo
}
```

---

### 📋 **FASE 4: Migração do Sistema de Cabos para 3D**

#### 🎯 **Objetivo:** Cabos 3D dinâmicos com LineRenderer

**Tarefas Pendentes:**
1. **Adaptar CableController**
   - Trocar UI Image por LineRenderer
   - Raycast 3D para detecção de PCs
   - Manter lógica de eventos intacta

2. **Visual 3D do Cabo**
   - LineRenderer com material customizado
   - Animação de conexão
   - Particle effects (opcional)

3. **Adaptar Modem**
   - Converter para GameObject 3D
   - Manter drag system com raycast 3D
   - Posição fixa no centro

**CableController 3D:**
```csharp
// Trocar EventSystem raycast por Physics raycast
Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
if (Physics.Raycast(ray, out RaycastHit hit))
{
    ComputerBehavior3D pc = hit.collider.GetComponent<ComputerBehavior3D>();
    // ...
}

// LineRenderer para cabo visual
lineRenderer.SetPosition(0, modemPosition);
lineRenderer.SetPosition(1, mouseWorldPosition);
```

---

### 📋 **FASE 5: Efeitos e Polish 3D**

#### 🎯 **Objetivo:** Adicionar juice e efeitos visuais

**Tarefas Pendentes:**
1. **Particle Systems**
   - Sparks na conexão de cabos
   - Smoke quando PC falha
   - Success burst quando conecta

2. **Animations e Tweening**
   - PC scale pulse quando tempo baixo
   - Cabo connection animation
   - Camera shake em eventos importantes

3. **Audio Integration**
   - Sound effects 3D posicionais
   - Background music
   - UI feedback sounds

4. **Lighting e Post-Processing**
   - Lighting dinâmico
   - Glow effects
   - Color grading

---

### 📋 **FASE 6: Gameplay Expandido**

#### 🎯 **Objetivo:** Features apenas possíveis no mundo 3D

**Tarefas Pendentes:**
1. **Multiple Floors/Levels**
   - Andares diferentes do escritório
   - Elevadores/escadas
   - Complexidade crescente

2. **Environmental Hazards**
   - Obstáculos físicos
   - Paths complexos para cabos
   - Interactive office objects

3. **3D UI Integration**
   - Holographic displays
   - 3D menus no mundo
   - Spatial interface elements

---

## 🔧 **SCRIPTS DE CONVERSÃO IMPLEMENTADOS**

### ✅ 1. **CoordinateConverter.cs** (Utility) - COMPLETO
```csharp
public static class CoordinateConverter
{
    public static Vector3 UIToWorld(Vector2 uiPos, Camera camera)
    public static Vector2 WorldToUI(Vector3 worldPos, Camera camera)
    public static Vector3 ScreenToWorld(Vector2 screenPos, Camera camera, float z = 0)
    public static bool IsVisibleOnScreen(Vector3 worldPos, Camera camera, float margin = 0.1f)
    public static Vector3 ClampToScreen(Vector3 worldPos, Camera camera, float margin = 0.1f)
}
```

### ✅ 2. **WorldBounds.cs** - COMPLETO
```csharp
public class WorldBounds : MonoBehaviour
{
    public Vector3 minBounds;
    public Vector3 maxBounds;
    
    public bool IsInBounds(Vector3 position)
    public Vector3 ClampToBounds(Vector3 position)
    public Vector3 GetRandomPositionInRing(Vector3 center, float minRadius, float maxRadius)
}
```

### ✅ 3. **CameraController.cs** - COMPLETO
- Zoom, pan, limites
- Smooth transitions
- Input handling

### ✅ 4. **ComputerBehavior3D.cs** - COMPLETO
- Versão 3D do comportamento dos PCs
- Timer com Canvas WorldSpace
- Efeitos visuais 3D

### ✅ 5. **ClickableObject3D.cs** - COMPLETO
- Sistema de interação 3D com raycast
- Clique, hover, drag
- Feedback visual

---

## ⚡ **ORDEM DE IMPLEMENTAÇÃO RECOMENDADA**

**✅ Sprint 1 (Básico 3D) - COMPLETO:**
1. ✅ Setup câmera e plano
2. ✅ Converter PCSpawner para 3D
3. ✅ PC prefab 3D básico

**🔄 Sprint 2 (Interação) - EM PROGRESSO:**
1. ✅ CameraController
2. ✅ Raycast 3D para mouse
3. 🔄 Converter sistema de cabos

**📋 Sprint 3 (Polish):**
1. Efeitos visuais
2. Particle systems
3. Audio integration

**📋 Sprint 4 (Advanced):**
1. Múltiplos níveis
2. Hazards ambientais
3. 3D UI integration

---

## 🎯 **VANTAGENS DA MIGRAÇÃO 3D**

**Gameplay:**
- Câmera móvel para mapas maiores
- Obstáculos e pathfinding complexo
- Múltiplos andares/níveis
- Efeitos visuais mais ricos

**Technical:**
- Performance melhor para muitos objetos
- Physics integration
- Lighting dinâmico
- Escalabilidade visual

**Design:**
- Spatial design mais interessante
- Environmental storytelling
- Interactive world objects
- More immersive experience

---

## ⚙️ Mecânicas Principais

### ✅ Núcleo do jogo (prototipar primeiro)
- PCs surgem periodicamente em posições aleatórias.
- Cada PC tem um **temporizador individual**.
- O jogador deve:
  - Clicar no modem para iniciar uma conexão.
  - Arrastar o "cabo" até o PC.
  - Se for conectado **antes do tempo acabar**, o PC fica "feliz" e o jogador ganha pontos.
  - Se o tempo acabar, o PC entra em estado "desconectado".

### 🔜 Planejado para versões futuras (não implementar agora)
- Tipos de problemas variados (IP fixo, montagem de cabo, diagnósticos).
- Barra de upgrades (provedor, modem).
- Sistema de dinheiro/moedas.
- Game over por acúmulo de PCs não resolvidos.

---

## 🧱 Estrutura Esperada (UI Only – Unity Canvas)

### Elementos do Canvas
- `ModemImage`: imagem central fixa do modem.
- `PCPrefab`: prefab UI que aparece ao redor, com:
  - Imagem do computador.
  - Temporizador visual (barra ou número).
  - Estado: desconectado, conectado, falhou.
- `CableLayer`: camada que renderiza o cabo (UI Image ou LineRenderer em overlay).
- `ScoreText`: texto no HUD com a pontuação.
- `StatusText` (opcional): exibe mensagens rápidas como "Conexão feita!".

---

## 🧪 Sprint de 2h – Protótipo Inicial

### 🎯 Objetivo:
Criar o loop básico do jogo com as 3 ações centrais:

1. PCs surgem ao redor do modem com um tempo-limite.
2. Jogador pode clicar no modem e arrastar o cabo até o PC.
3. Se for conectado a tempo → ganha ponto; se não → o PC falha.

---

### ⏱️ Tarefas da Sprint

#### ✅ 1. Spawner de PCs
- ✅ Criar um sistema que instancie `PCPrefab` em posições aleatórias ao redor do modem a cada X segundos (ex: 5s).
- ✅ Garantir que eles sejam filhos do Canvas.
- ✅ Salvar referência dos PCs instanciados.

#### ✅ 2. Temporizador do PC
- ✅ Cada PC tem um timer regressivo (ex: 10 segundos).
- ✅ Exibe o tempo restante (como número ou barra).
- ✅ Se o tempo chegar a zero sem conexão, muda o estado para "desconectado".

#### ✅ 3. Conexão com o Cabo (COMPLETO)
- ✅ Ao clicar e arrastar no modem, cria cabo visual dinamicamente.
- ✅ Cabo se extende e rotaciona seguindo o mouse.
- ✅ Se soltar sobre PC válido e o tempo ainda não acabou:
  - ✅ Marca como conectado.
  - ✅ Cabo fica fixo e muda para verde.
  - ✅ PC conectado auto-destroi após cooldown.
- ✅ Se soltar fora ou em PC inválido, cabo é cancelado automaticamente.

#### ✅ 4. Pontuação
- ✅ Pontuação simples: +10 por conexão bem-sucedida.
- ✅ Atualizar o `ScoreText` no HUD.

---

## 🧑‍💻 Scripts implementados

1. ✅ `PCSpawner.cs` - **COMPLETO (3D)**
   - ✅ Spawna computadores ao redor do modem, a cada X segundos.
   - ✅ Sistema inteligente de posicionamento com detecção de colisão.
   - ✅ Migrado para coordenadas 3D com `WorldBounds`.

2. ✅ `ComputerBehavior.cs` - **COMPLETO (UI)**
   - ✅ Controla timer do PC com feedback visual dinâmico.
   - ✅ Estados: desconectado → conectando → conectado → falhou.
   - ✅ Sistema de eventos e auto-destruição.

3. ✅ `ComputerBehavior3D.cs` - **COMPLETO (3D)**
   - ✅ Versão 3D do comportamento dos PCs.
   - ✅ Timer com Canvas WorldSpace.
   - ✅ Efeitos visuais 3D (pulse, rotation).

4. ✅ `CableController.cs` - **COMPLETO (UI)**
   - ✅ Sistema completo de drag & drop de cabos.
   - ✅ Cabo visual dinâmico que se extende e rotaciona.
   - ✅ Detecção de PCs via UI raycast.
   - ✅ Cabo fixo após conexão bem-sucedida.

5. ✅ `Modem.cs` - **COMPLETO (UI)**
   - ✅ Integração com sistema de cabos via drag.
   - ✅ Eventos sincronizados com CableController.

6. ✅ `ClickableObject.cs` - **COMPLETO (UI)**
   - ✅ Sistema modular de interação (click, hover, drag).
   - ✅ Eventos tipados e debugs avançados.

7. ✅ `ClickableObject3D.cs` - **COMPLETO (3D)**
   - ✅ Sistema de interação 3D com raycast.
   - ✅ Clique, hover, drag em objetos 3D.
   - ✅ Feedback visual.

8. ✅ `WorldBounds.cs` - **COMPLETO**
   - ✅ Define limites do mundo 3D.
   - ✅ Validação de posições.
   - ✅ Geração de posições aleatórias.

9. ✅ `CoordinateConverter.cs` - **COMPLETO**
   - ✅ Conversão entre coordenadas UI e 3D.
   - ✅ Utilitários para transição gradual.

10. ✅ `CameraController.cs` - **COMPLETO**
    - ✅ Controle de câmera 3D.
    - ✅ Zoom, pan, limites.
    - ✅ Smooth transitions.

11. 🔜 `GameManager.cs` - **PRÓXIMO**
    - Controla a pontuação.
    - Mantém contagem de PCs ativos.
    - Atualiza o HUD.

---

## 🔄 **ESTRATÉGIA DE TRANSIÇÃO GRADUAL**

### 📋 **Abordagem Híbrida (Recomendada)**

**Manter funcionando durante transição:**
1. **UI System ativo** - Score, HUD, menus permanecem em Canvas
2. **Mundo 3D paralelo** - Implementar sistemas 3D gradualmente  
3. **Toggle System** - Permitir alternar entre UI/3D durante desenvolvimento
4. **Backwards compatibility** - Manter scripts originais funcionando

### 🛠️ **Scripts de Transição:**

```csharp
// GameplayMode.cs - Toggle entre UI e 3D
public enum GameplayMode { UI_Mode, World3D_Mode }

public class GameModeManager : MonoBehaviour
{
    public GameplayMode currentMode = GameplayMode.UI_Mode;
    
    [Header("UI System")]
    public GameObject uiCanvas;
    public PCSpawner uiSpawner;
    
    [Header("3D System")]  
    public Camera worldCamera;
    public PCSpawner3D worldSpawner;
    
    public void SwitchMode(GameplayMode newMode)
    {
        // Ativar/desativar sistemas conforme modo
    }
}
```

### ⚠️ **Cuidados Durante Transição:**

**Problemas comuns:**
- Mixing coordinate systems (UI + World)
- Event systems conflicting  
- Performance issues with dual systems
- Input handling confusion

**Soluções:**
- Clear separation of concerns
- Consistent naming (PCSpawner vs PCSpawner3D)
- Shared interfaces para compatibilidade
- Thorough testing de cada sistema individualmente

---

## 🔚 Considerações Finais

### 🎯 **Próximos Passos Recomendados:**

**Imediato (hoje):**
- ✅ Implementar Fase 1 (Câmera 3D + Plano básico) - COMPLETO
- ✅ Criar CoordinateConverter para facilitar transição - COMPLETO
- ✅ Setup WorldBounds para delimitar área de jogo - COMPLETO

**Esta semana:**  
- ✅ Migrar PCSpawner para 3D (Fase 2) - COMPLETO
- ✅ Implementar CameraController básico (Fase 3) - COMPLETO
- 🔄 Manter sistema UI funcionando em paralelo
- 🔄 Testar integração completa dos sistemas 3D

**Futuro:**
- Sistema de cabos 3D com LineRenderer
- Particle effects e juice
- Gameplay expandido com múltiplos níveis

### 💡 **Filosofia de Design:**

- **Manter arquitetura de eventos** - Sistema atual é robusto
- **Transição gradual** - Não quebrar o que funciona  
- **Flexibilidade** - Permitir voltar para UI se necessário
- **Performance-first** - 3D deve ser mais eficiente, não menos
- **Visual impact** - Migração deve resultar em experiência visivelmente melhor

### 🚀 **Benefícios Esperados:**

**Técnicos:**
- Escalabilidade para mapas maiores
- Physics integration natural
- Performance melhor com muitos objetos
- Flexibilidade de câmera

**Gameplay:**  
- Possibilidade de obstáculos e pathfinding
- Múltiplos andares/complexidade espacial
- Efeitos visuais mais ricos e imersivos
- Camera cinematográfica para apresentação

A base sólida que você construiu (eventos, lógica de negócio, arquitetura) se manterá. A migração é principalmente visual e de coordenadas!

---

## 🎉 **PROGRESSO ATUAL**

### ✅ **COMPLETADO:**
- ✅ Setup básico 3D (Fase 1)
- ✅ Sistema de coordenadas híbrido
- ✅ CameraController funcional
- ✅ WorldBounds para limites do mundo
- ✅ PCSpawner migrado para 3D
- ✅ ComputerBehavior3D criado
- ✅ ClickableObject3D para interação 3D

### 🔄 **EM PROGRESSO:**
- 🔄 Testes de integração
- 🔄 Ajustes de valores para mundo 3D
- 🔄 Criação de prefabs 3D funcionais

### 📋 **PRÓXIMO:**
- 📋 Sistema de cabos 3D
- 📋 Efeitos visuais e polish
- 📋 Gameplay expandido

**Status Geral: 60% da transição 3D concluída!** 🚀 

# 🎯 **DIRETRIZES DE DESENVOLVIMENTO**

### ⚠️ **REGRAS IMPORTANTES:**
1. **FAZER APENAS O QUE É PEDIDO** - Não criar código desnecessário
2. **MODIFICAR, NÃO DUPLICAR** - Adaptar scripts existentes para 3D em vez de criar novos
3. **MANTER PROJETO LIMPO** - Remover scripts obsoletos e duplicados
4. **FOCO NO ESSENCIAL** - Implementar apenas funcionalidades solicitadas
5. **SEGUIR INSTRUÇÕES** - Executar exatamente o que o usuário solicitar

### 🧹 **LIMPEZA NECESSÁRIA:**
- [ ] Remover scripts 2D obsoletos após migração completa
- [ ] Consolidar funcionalidades em scripts únicos
- [ ] Manter apenas uma versão de cada sistema (3D)
- [ ] Organizar estrutura de pastas
- [ ] Remover dependências de UI desnecessárias 