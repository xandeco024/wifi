using UnityEngine;

/// <summary>
/// Script de teste para verificar se o ClickableObject3D funciona
/// Adicione este script junto com ClickableObject3D para debugar
/// </summary>
public class ClickableTestCube : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private Color originalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;
    
    private ClickableObject3D clickableObject;
    private Renderer objectRenderer;
    private Material originalMaterial;
    
    void Start()
    {
        // Configuração automática
        if (autoSetupOnStart)
        {
            SetupTestCube();
        }
        
        // Referências
        clickableObject = GetComponent<ClickableObject3D>();
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
        
        // Eventos
        if (clickableObject != null)
        {
            clickableObject.OnThisObjectClick += OnCubeClicked;
            clickableObject.OnThisObjectHoverEnter += OnCubeHoverEnter;
            clickableObject.OnThisObjectHoverExit += OnCubeHoverExit;
            clickableObject.OnThisObjectDragStart += OnCubeDragStart;
            clickableObject.OnThisObjectDragEnd += OnCubeDragEnd;
            
            Debug.Log("[ClickableTestCube] Eventos configurados com sucesso!");
        }
        else
        {
            Debug.LogError("[ClickableTestCube] ClickableObject3D não encontrado!");
        }
    }
    
    void OnDestroy()
    {
        // Limpa eventos
        if (clickableObject != null)
        {
            clickableObject.OnThisObjectClick -= OnCubeClicked;
            clickableObject.OnThisObjectHoverEnter -= OnCubeHoverEnter;
            clickableObject.OnThisObjectHoverExit -= OnCubeHoverExit;
            clickableObject.OnThisObjectDragStart -= OnCubeDragStart;
            clickableObject.OnThisObjectDragEnd -= OnCubeDragEnd;
        }
    }
    
    #region Setup
    
    private void SetupTestCube()
    {
        Debug.Log("[ClickableTestCube] Configurando cubo de teste...");
        
        // Adiciona ClickableObject3D se não tiver
        if (GetComponent<ClickableObject3D>() == null)
        {
            gameObject.AddComponent<ClickableObject3D>();
            Debug.Log("[ClickableTestCube] ClickableObject3D adicionado");
        }
        
        // Adiciona Collider se não tiver
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log("[ClickableTestCube] BoxCollider adicionado");
        }
        
        // Adiciona Renderer se não tiver (para visual)
        if (GetComponent<Renderer>() == null)
        {
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            
            // Cria cubo primitivo
            meshFilter.mesh = CreateCubeMesh();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = originalColor;
            
            Debug.Log("[ClickableTestCube] Renderer e MeshFilter adicionados");
        }
        
        // Verifica tag da câmera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                cameras[0].tag = "MainCamera";
                Debug.Log($"[ClickableTestCube] Tag MainCamera adicionada à câmera {cameras[0].name}");
            }
            else
            {
                Debug.LogError("[ClickableTestCube] Nenhuma câmera encontrada na cena!");
            }
        }
        
        Debug.Log("[ClickableTestCube] Setup completo!");
    }
    
    private Mesh CreateCubeMesh()
    {
        // Cria mesh simples de cubo
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(tempCube);
        return cubeMesh;
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnCubeClicked(ClickableObject3D clickedObject)
    {
        Debug.Log("🖱️ [ClickableTestCube] CUBO CLICADO! Sistema funcionando!");
        
        // Feedback visual
        ChangeColor(clickColor);
        Invoke(nameof(ResetColor), 0.2f);
        
        // Feedback físico
        transform.localScale = Vector3.one * 1.1f;
        Invoke(nameof(ResetScale), 0.2f);
    }
    
    private void OnCubeHoverEnter(ClickableObject3D hoveredObject)
    {
        Debug.Log("👆 [ClickableTestCube] Mouse ENTROU no cubo");
        ChangeColor(hoverColor);
    }
    
    private void OnCubeHoverExit(ClickableObject3D hoveredObject)
    {
        Debug.Log("👋 [ClickableTestCube] Mouse SAIU do cubo");
        ResetColor();
    }
    
    private void OnCubeDragStart(ClickableObject3D draggedObject)
    {
        Debug.Log("🖱️ [ClickableTestCube] DRAG INICIADO!");
    }
    
    private void OnCubeDragEnd(ClickableObject3D draggedObject)
    {
        Debug.Log("🖱️ [ClickableTestCube] DRAG FINALIZADO!");
    }
    
    #endregion
    
    #region Visual Feedback
    
    private void ChangeColor(Color newColor)
    {
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = newColor;
        }
    }
    
    private void ResetColor()
    {
        ChangeColor(originalColor);
    }
    
    private void ResetScale()
    {
        transform.localScale = Vector3.one;
    }
    
    #endregion
    
    #region Debug
    
    [ContextMenu("Test Manual Click")]
    private void TestManualClick()
    {
        if (clickableObject != null)
        {
            clickableObject.SimulateClick();
        }
    }
    
    [ContextMenu("Print Status")]
    private void TestPrintStatus()
    {
        Debug.Log($"=== STATUS DO CUBO DE TESTE ===");
        Debug.Log($"ClickableObject3D: {(clickableObject != null ? "OK" : "MISSING")}");
        Debug.Log($"Collider: {(GetComponent<Collider>() != null ? "OK" : "MISSING")}");
        Debug.Log($"Renderer: {(objectRenderer != null ? "OK" : "MISSING")}");
        Debug.Log($"Camera.main: {(Camera.main != null ? Camera.main.name : "NULL")}");
        Debug.Log($"GameObject ativo: {gameObject.activeInHierarchy}");
        Debug.Log($"Posição: {transform.position}");
    }
    
    [ContextMenu("Force Setup")]
    private void TestForceSetup()
    {
        SetupTestCube();
    }
    
    #endregion
} 