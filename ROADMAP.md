# 🛠️ Roadmap Técnico para o Jogo "Conexão Caótica" – Game Jam

## 🔄 TRANSIÇÃO: UI → MUNDO 3D

**ESTADO ATUAL**: Jogo funcional 100% UI/Canvas  
**OBJETIVO**: Migrar para mundo 3D com câmera, efeitos e objetos 3D

**ARQUITETURA MANTIDA:**
- ✅ Sistema de eventos (CableController, ScoreManager, etc.)
- ✅ Lógica de negócio (spawning, timers, pontuação)
- ✅ Estrutura de classes existente

**MUDANÇAS NECESSÁRIAS:**
- 🔄 Coordenadas: `RectTransform.anchoredPosition` → `Transform.position`
- 🔄 Raycast: `EventSystem.RaycastAll()` → `Physics.Raycast()`
- 🔄 Câmera: Canvas → Camera 3D com controles
- 🔄 Visuals: UI Elements → 3D Models/Sprites

## 🎮 Visão Geral do Jogo

Você é um técnico de TI responsável por manter a rede de um escritório em crescimento. PCs com problemas de conexão aparecem de tempos em tempos, e o jogador precisa agir rapidamente para conectá-los ao modem, resolver o problema e evitar que a rede entre em colapso.

O objetivo é sobreviver o máximo possível e ganhar pontos por conexões bem-sucedidas. O jogo é **infinito** e só termina quando muitos PCs não são atendidos (morte súbita ou "game over").

---

## 🗺️ ROADMAP DE TRANSIÇÃO PARA 3D

### 📋 **FASE 1: Preparação e Setup Inicial**

#### 🎯 **Objetivo:** Setup básico do mundo 3D mantendo funcionalidade atual

**Tarefas:**
1. **Configurar Câmera 3D**
   - Criar Main Camera com posição isométrica/top-down
   - Configurar projection (Orthographic recomendado para início)
   - Script básico de controle de câmera (zoom, pan)

2. **Criar Plano de Jogo**
   - Plane 3D como "mesa" do escritório
   - Boundaries invisíveis para spawning
   - Lighting básico

3. **Sistema de Coordenadas Híbrido**
   - Manter UI atual funcionando (HUD, Score)
   - Adicionar conversores UI ↔ World coordinates
   - Camera.ScreenToWorldPoint() para input

**Código de Referência:**
```csharp
// Conversão de coordenadas
Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
worldPos.z = 0; // Fixar Z para 2.5D

// Spawning 3D
GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity);

// Boundaries (mundo)
bool isInBounds = worldPos.x >= minX && worldPos.x <= maxX && 
                  worldPos.y >= minY && worldPos.y <= maxY;
```

---

### 📋 **FASE 2: Migração dos PCs para 3D**

#### 🎯 **Objetivo:** Converter PCSpawner e ComputerBehavior para mundo 3D

**Tarefas:**
1. **Criar PC Prefab 3D**
   - GameObject com SpriteRenderer ou modelo 3D simples
   - Collider para raycast 3D
   - Canvas filho para UI do timer (WorldSpace)

2. **Adaptar PCSpawner**
   - Mudar de `RectTransform.anchoredPosition` para `Transform.position`
   - Usar coordenadas mundo para spawning
   - Manter lógica de posicionamento inteligente

3. **Adaptar ComputerBehavior**
   - Manter timer e lógica de estado
   - UI do timer como Canvas WorldSpace
   - Adicionar feedback visual 3D (scale, rotation, materials)

**Scripts a Modificar:**
- `PCSpawner.cs` - conversão de coordenadas
- `ComputerBehavior.cs` - manter lógica, adaptar visual

---

### 📋 **FASE 3: Sistema de Câmera e Controles**

#### 🎯 **Objetivo:** Câmera interativa e controles de mundo

**Tarefas:**
1. **CameraController Script**
   - Zoom com scroll do mouse
   - Pan com drag do mouse/WASD
   - Limites de câmera
   - Smooth movement

2. **Input System**
   - Mouse para raycast 3D
   - Keyboard para controles de câmera
   - Touch support (futuro mobile)

**Novo Script:**
```csharp
public class CameraController : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minZoom = 3f;
    public float maxZoom = 10f;
    
    [Header("Pan")]
    public float panSpeed = 2f;
    public Vector2 panLimits = new Vector2(10f, 10f);
    
    // Implementar zoom, pan, limites
}
```

---

### 📋 **FASE 4: Migração do Sistema de Cabos para 3D**

#### 🎯 **Objetivo:** Cabos 3D dinâmicos com LineRenderer

**Tarefas:**
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
    ComputerBehavior pc = hit.collider.GetComponent<ComputerBehavior>();
    // ...
}

// LineRenderer para cabo visual
lineRenderer.SetPosition(0, modemPosition);
lineRenderer.SetPosition(1, mouseWorldPosition);
```

---

### 📋 **FASE 5: Efeitos e Polish 3D**

#### 🎯 **Objetivo:** Adicionar juice e efeitos visuais

**Tarefas:**
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

**Tarefas:**
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

## 🔧 **SCRIPTS DE CONVERSÃO NECESSÁRIOS**

### 1. **CoordinateConverter.cs** (Utility)
```csharp
public static class CoordinateConverter
{
    public static Vector3 UIToWorld(Vector2 uiPos, Camera camera)
    public static Vector2 WorldToUI(Vector3 worldPos, Camera camera)
    public static Vector3 ScreenToWorld(Vector2 screenPos, Camera camera, float z = 0)
}
```

### 2. **WorldBounds.cs**
```csharp
public class WorldBounds : MonoBehaviour
{
    public Vector2 minBounds;
    public Vector2 maxBounds;
    
    public bool IsInBounds(Vector3 position)
    public Vector3 ClampToBounds(Vector3 position)
}
```

### 3. **CameraController.cs**
- Zoom, pan, limites
- Smooth transitions
- Input handling

---

## ⚡ **ORDEM DE IMPLEMENTAÇÃO RECOMENDADA**

**Sprint 1 (Básico 3D):**
1. Setup câmera e plano
2. Converter PCSpawner para 3D
3. PC prefab 3D básico

**Sprint 2 (Interação):**
1. CameraController
2. Raycast 3D para mouse
3. Converter sistema de cabos

**Sprint 3 (Polish):**
1. Efeitos visuais
2. Particle systems
3. Audio integration

**Sprint 4 (Advanced):**
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
- Criar um sistema que instancie `PCPrefab` em posições aleatórias ao redor do modem a cada X segundos (ex: 5s).
- Garantir que eles sejam filhos do Canvas.
- Salvar referência dos PCs instanciados.

#### ✅ 2. Temporizador do PC
- Cada PC tem um timer regressivo (ex: 10 segundos).
- Exibe o tempo restante (como número ou barra).
- Se o tempo chegar a zero sem conexão, muda o estado para "desconectado".

#### ✅ 3. Conexão com o Cabo (COMPLETO)
- ✅ Ao clicar e arrastar no modem, cria cabo visual dinamicamente.
- ✅ Cabo se extende e rotaciona seguindo o mouse.
- ✅ Se soltar sobre PC válido e o tempo ainda não acabou:
  - ✅ Marca como conectado.
  - ✅ Cabo fica fixo e muda para verde.
  - ✅ PC conectado auto-destroi após cooldown.
- ✅ Se soltar fora ou em PC inválido, cabo é cancelado automaticamente.

#### ✅ 4. Pontuação
- Pontuação simples: +10 por conexão bem-sucedida.
- Atualizar o `ScoreText` no HUD.

---

## 🧑‍💻 Scripts implementados

1. ✅ `PCSpawner.cs` - **COMPLETO**
   - Spawna computadores ao redor do modem, a cada X segundos.
   - Sistema inteligente de posicionamento com detecção de colisão.

2. ✅ `ComputerBehavior.cs` - **COMPLETO**
   - Controla timer do PC com feedback visual dinâmico.
   - Estados: desconectado → conectando → conectado → falhou.
   - Sistema de eventos e auto-destruição.

3. ✅ `CableController.cs` - **COMPLETO**
   - Sistema completo de drag & drop de cabos.
   - Cabo visual dinâmico que se extende e rotaciona.
   - Detecção de PCs via UI raycast.
   - Cabo fixo após conexão bem-sucedida.

4. ✅ `Modem.cs` - **COMPLETO**
   - Integração com sistema de cabos via drag.
   - Eventos sincronizados com CableController.

5. ✅ `ClickableObject.cs` - **COMPLETO**
   - Sistema modular de interação (click, hover, drag).
   - Eventos tipados e debugs avançados.

6. 🔜 `GameManager.cs` - **PRÓXIMO**
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
- Implementar Fase 1 (Câmera 3D + Plano básico)
- Criar CoordinateConverter para facilitar transição
- Setup WorldBounds para delimitar área de jogo

**Esta semana:**  
- Migrar PCSpawner para 3D (Fase 2)
- Implementar CameraController básico (Fase 3)
- Manter sistema UI funcionando em paralelo

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