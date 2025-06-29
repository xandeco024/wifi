# Exemplo de Uso - Sistema de Popup do Modem

## Configuração Rápida na Cena

### 1. Estrutura de UI Necessária

Crie a seguinte hierarquia na sua cena:

```
Canvas
└── ModemInfoPopup
    ├── Image (Background Overlay - cor: rgba(0,0,0,0.5))
    └── PopupPanel (RectTransform centrado, tamanho: 400x300)
        ├── Image (Background do painel - cor escura)
        ├── CanvasGroup (para animações)
        ├── VerticalLayoutGroup (spacing: 10, padding: 20)
        ├── Title (TextMeshProUGUI) - "Informações do Modem"
        ├── CurrentSpeedText (TextMeshProUGUI) - velocidade atual
        ├── CurrentCablesText (TextMeshProUGUI) - portas atuais
        ├── SpeedUpgradeCostText (TextMeshProUGUI) - custo upgrade velocidade
        ├── CablesUpgradeCostText (TextMeshProUGUI) - custo upgrade portas
        └── CloseButton (Button) - "Fechar"
```

### 2. Adicionar o Script

1. Adicione o script `ModemInfoPopup.cs` ao GameObject "ModemInfoPopup"
2. Configure as referências no Inspector:
   - **Popup Panel**: Arraste o GameObject "PopupPanel"
   - **Current Speed Text**: Arraste o TextMeshProUGUI "CurrentSpeedText"
   - **Current Cables Text**: Arraste o TextMeshProUGUI "CurrentCablesText"
   - **Speed Upgrade Cost Text**: Arraste o TextMeshProUGUI "SpeedUpgradeCostText"
   - **Cables Upgrade Cost Text**: Arraste o TextMeshProUGUI "CablesUpgradeCostText"
   - **Close Button**: Arraste o Button "CloseButton"
   - **Background Overlay**: Arraste a Image de fundo

### 3. Configurar Posições de Animação

No Inspector do `ModemInfoPopup`:
- **Hidden Position**: (0, -1000, 0) - posição quando escondido
- **Visible Position**: (0, 0, 0) - posição quando visível
- **Animation Duration**: 0.3 - duração da animação

## Funcionamento Automático

### Controles do Usuário
- **Clique Direito no Modem**: Abre o popup
- **Clique Fora do Popup**: Fecha o popup  
- **Tecla ESC**: Fecha o popup
- **Botão "Fechar"**: Fecha o popup

### Conteúdo Mostrado
```
Informações do Modem

Velocidade: 20 MB/s
(Nível 2: 20 MB/s)

Portas: 3
(Nível 2: 3 Portas)

Upgrade Velocidade:
120 coins

Upgrade Portas:
80 coins
```

### Atualização Automática
O popup se atualiza automaticamente quando:
- O modem é upgradeado
- O popup já está visível durante um upgrade

## Exemplo de Código (Opcional)

Se quiser controlar o popup programaticamente:

```csharp
public class ExemploUsoPopup : MonoBehaviour
{
    [SerializeField] private Modem meuModem;
    
    void Update()
    {
        // Mostrar popup com tecla P
        if (Input.GetKeyDown(KeyCode.P))
        {
            MostrarPopupDoModem();
        }
        
        // Esconder popup com tecla H
        if (Input.GetKeyDown(KeyCode.H))
        {
            EsconderPopup();
        }
    }
    
    void MostrarPopupDoModem()
    {
        if (ModemInfoPopup.Instance != null && meuModem != null)
        {
            ModemInfoPopup.Instance.ShowPopup(meuModem);
        }
    }
    
    void EsconderPopup()
    {
        if (ModemInfoPopup.Instance != null)
        {
            ModemInfoPopup.Instance.HidePopup();
        }
    }
    
    void VerificarSePopupEstaVisivel()
    {
        bool popupVisivel = ModemInfoPopup.Instance?.IsPopupVisible ?? false;
        Debug.Log($"Popup está visível: {popupVisivel}");
    }
}
```

## Personalização Visual

### Cores Recomendadas
- **Background Overlay**: rgba(0, 0, 0, 0.5) - fundo semi-transparente
- **Panel Background**: rgba(0.2, 0.2, 0.2, 0.9) - painel escuro
- **Text Color**: Branco (#FFFFFF)
- **Button Background**: rgba(0.4, 0.4, 0.4, 1) - cinza médio

### Dimensões Recomendadas
- **Popup Panel**: 400x300 pixels
- **Font Size**: 18 para textos normais, 22 para título
- **Button Height**: 40 pixels
- **Spacing**: 10 pixels entre elementos

## Solução de Problemas

### Popup não aparece
- Verifique se existe um Canvas na cena
- Confirme que o modem tem o componente ClickableObject
- Verifique se todas as referências estão configuradas

### Animação não funciona
- Confirme que DOTween está instalado
- Verifique se o PopupPanel tem RectTransform
- Confirme que há um CanvasGroup no PopupPanel

### Clique direito não detectado
- Verifique se o Input.GetMouseButtonDown(1) está funcionando
- Confirme que o modem está recebendo eventos de clique
- Teste com Debug.Log no método OnModemClicked3D

## Resultado Final

Um popup elegante que:
✅ Sobe suavemente de baixo da tela  
✅ Mostra informações atualizadas do modem  
✅ Fecha ao clicar fora ou pressionar ESC  
✅ Atualiza automaticamente durante upgrades  
✅ Não interfere com funcionalidades existentes 