# Guia de Setup - Popup de Informações do Modem

## Visão Geral
Sistema de popup que mostra informações detalhadas do modem ao clicar com o botão direito nele. O popup sobe de baixo da tela com animação suave e fecha ao clicar fora dele.

## Scripts Envolvidos

### 1. ModemInfoPopup.cs
- **Função**: Gerenciador principal do popup
- **Características**:
  - Singleton para acesso global
  - Animações com DOTween (subir/descer da tela)
  - Detecta cliques fora do popup para fechar
  - Atualiza conteúdo dinamicamente

### 2. ModemInfoPopupCreator.cs
- **Função**: Helper para criar a UI automaticamente
- **Uso**: Adicione a um GameObject vazio e use o botão "Create Popup UI" no Inspector

### 3. Modem.cs (Modificado)
- **Adicionado**: Detecção de clique direito
- **Integração**: Chama ShowInfoPopup() no clique direito

## Setup Automático

### Opção 1: Usando o Creator (Recomendado)
1. Crie um GameObject vazio na cena
2. Adicione o script `ModemInfoPopupCreator`
3. Configure as opções se necessário:
   - `Target Canvas`: Canvas onde criar o popup
   - `Background Color`: Cor de fundo do popup
   - `Text Color`: Cor do texto
   - `Font Size`: Tamanho da fonte
   - `Popup Size`: Dimensões do popup
4. Clique em "Create Popup UI" no Inspector
5. O script se auto-remove após criar a UI

### Opção 2: Auto-Criação no Start
1. Marque `Auto Create On Start` no `ModemInfoPopupCreator`
2. A UI será criada automaticamente ao iniciar a cena

## Funcionalidades

### Controles
- **Clique Direito no Modem**: Abre o popup
- **Clique Fora do Popup**: Fecha o popup
- **Tecla ESC**: Fecha o popup
- **Botão "Fechar"**: Fecha o popup

### Informações Mostradas
- **Nível**: Nível atual e nome do modem
- **Velocidade**: Velocidade de internet em MB/s
- **Cabos**: Número máximo de cabos simultâneos
- **Upgrade**: Informações sobre próximo upgrade ou "Nível máximo"

### Animações
- **Abertura**: Popup sobe de baixo da tela com efeito OutBack
- **Fechamento**: Popup desce para baixo da tela com efeito InBack
- **Fade**: Opacidade anima junto com o movimento

## Integração com Sistema Existente

### Atualização Automática
- O popup atualiza automaticamente quando o modem é upgradeado
- Mantém sincronia com o estado atual do modem

### Compatibilidade
- Funciona com o sistema de upgrade existente
- Não interfere com cliques normais (esquerdo) no modem
- Compatible com animações existentes do modem

## Configurações Avançadas

### No Inspector (ModemInfoPopup)
- **Animation Duration**: Duração das animações (padrão: 0.3s)
- **Hidden Position**: Posição quando escondido (padrão: 0, -1000, 0)
- **Visible Position**: Posição quando visível (padrão: 0, 0, 0)

### Personalização de Cores
- **Background Color**: Cor de fundo do popup
- **Text Color**: Cor do texto
- **Overlay Color**: Cor do fundo semi-transparente

## Estrutura da UI Criada

```
ModemInfoPopup (Root)
├── Image (Background Overlay)
└── PopupPanel
    ├── Image (Panel Background)
    ├── CanvasGroup (Para animações)
    ├── VerticalLayoutGroup (Layout)
    ├── Title (TextMeshProUGUI) - "Informações do Modem"
    ├── CurrentSpeedText (TextMeshProUGUI) - Velocidade atual
    ├── CurrentCablesText (TextMeshProUGUI) - Quantidade de portas atual
    ├── SpeedUpgradeCostText (TextMeshProUGUI) - Custo upgrade velocidade
    ├── CablesUpgradeCostText (TextMeshProUGUI) - Custo upgrade portas
    └── CloseButton (Button)
```

## Debugging

### Logs Importantes
- `"[Modem] Popup de informações mostrado"`
- `"Popup do modem mostrado: [Nome do Nível]"`
- `"Popup do modem escondido"`
- `"[Modem] ModemInfoPopup não encontrado na cena!"`

### Problemas Comuns
1. **Popup não aparece**: Verifique se existe Canvas na cena
2. **Clique direito não funciona**: Verifique se o modem tem ClickableObject
3. **Animação não funciona**: Verifique se DOTween está instalado
4. **Referências quebradas**: Use o Creator para recriar a UI

## Exemplo de Uso

```csharp
// Mostrar popup programaticamente
if (ModemInfoPopup.Instance != null)
{
    ModemInfoPopup.Instance.ShowPopup(modemReference);
}

// Esconder popup
if (ModemInfoPopup.Instance != null)
{
    ModemInfoPopup.Instance.HidePopup();
}

// Verificar se está visível
bool isVisible = ModemInfoPopup.Instance?.IsPopupVisible ?? false;
```

## Requisitos
- Unity 2021.3 ou superior
- DOTween (para animações)
- TextMeshPro (para textos)
- Canvas na cena

## Notas Importantes
- O popup é um Singleton - apenas uma instância por cena
- Usa DOTween para animações suaves
- Respeita o padrão de UI do jogo
- Atualiza automaticamente com mudanças no modem 