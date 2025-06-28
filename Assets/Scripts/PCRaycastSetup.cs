using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script utilitário para garantir que os PCs estejam configurados corretamente para raycast UI
/// Adicione este script aos GameObjects dos PCs para auto-configuração
/// </summary>
[RequireComponent(typeof(Image))]
public class PCRaycastSetup : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool autoFixOnAwake = true;
    
    void Awake()
    {
        if (autoFixOnAwake)
        {
            SetupPCForRaycast();
        }
    }
    
    /// <summary>
    /// Configura o PC para ser detectado por raycast UI
    /// </summary>
    [ContextMenu("Setup PC for Raycast")]
    public void SetupPCForRaycast()
    {
        bool hasChanges = false;
        
        // Verifica e configura o componente Image
        Image pcImage = GetComponent<Image>();
        if (pcImage == null)
        {
            pcImage = gameObject.AddComponent<Image>();
            DebugLog("Componente Image adicionado");
            hasChanges = true;
        }
        
        // Garante que Raycast Target está habilitado
        if (!pcImage.raycastTarget)
        {
            pcImage.raycastTarget = true;
            DebugLog("Raycast Target habilitado");
            hasChanges = true;
        }
        
        // Verifica se tem uma imagem/sprite configurada
        if (pcImage.sprite == null)
        {
            DebugLog("AVISO: PC não tem sprite configurado - pode não ser visível");
        }
        
        // Verifica se está em um Canvas
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            DebugLog("ERRO: PC não está dentro de um Canvas!");
        }
        else
        {
            // Verifica se o Canvas tem GraphicRaycaster
            GraphicRaycaster raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                DebugLog("AVISO: Canvas pai não tem GraphicRaycaster - raycast pode não funcionar");
            }
        }
        
        // Verifica RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            DebugLog("ERRO: PC não tem RectTransform - necessário para UI");
        }
        
        if (hasChanges)
        {
            DebugLog($"PC '{gameObject.name}' configurado para raycast");
        }
        else
        {
            DebugLog($"PC '{gameObject.name}' já estava configurado corretamente");
        }
    }
    
    /// <summary>
    /// Testa se este PC pode ser detectado por raycast na posição atual
    /// </summary>
    [ContextMenu("Test Raycast Detection")]
    public void TestRaycastDetection()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Converte posição do PC para coordenadas de tela
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;
        
        Camera uiCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, rectTransform.position);
        
        DebugLog($"Posição do PC em coordenadas de tela: {screenPosition}");
        
        // Simula o mesmo raycast que o CableController usa
        UnityEngine.EventSystems.PointerEventData pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = screenPosition
        };
        
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
        
        DebugLog($"Raycast encontrou {results.Count} objetos");
        
        bool foundSelf = false;
        foreach (var result in results)
        {
            DebugLog($"  - {result.gameObject.name}");
            if (result.gameObject == gameObject)
            {
                foundSelf = true;
            }
        }
        
        if (foundSelf)
        {
            DebugLog("✓ PC foi detectado pelo raycast!");
        }
        else
        {
            DebugLog("✗ PC NÃO foi detectado pelo raycast");
        }
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PCRaycastSetup - {gameObject.name}] {message}");
        }
    }
} 