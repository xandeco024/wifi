using UnityEngine;

/// <summary>
/// Utilitário para conversão entre coordenadas UI e mundo 3D
/// Facilita a transição gradual entre sistemas UI e 3D
/// </summary>
public static class CoordinateConverter
{
    #region UI to World Conversion
    
    /// <summary>
    /// Converte posição UI (RectTransform.anchoredPosition) para posição mundo 3D
    /// </summary>
    /// <param name="uiPosition">Posição UI em coordenadas do Canvas</param>
    /// <param name="canvas">Canvas de referência</param>
    /// <param name="worldZ">Coordenada Z no mundo (padrão: 0)</param>
    /// <returns>Posição no mundo 3D</returns>
    public static Vector3 UIToWorld(Vector2 uiPosition, Canvas canvas, float worldZ = 0f)
    {
        if (canvas == null)
        {
            Debug.LogError("[CoordinateConverter] Canvas é null!");
            return Vector3.zero;
        }
        
        // Converte posição UI para coordenadas de tela
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = new Vector2(
            (uiPosition.x + canvasRect.sizeDelta.x * 0.5f) / canvasRect.sizeDelta.x,
            (uiPosition.y + canvasRect.sizeDelta.y * 0.5f) / canvasRect.sizeDelta.y
        );
        
        // Converte para coordenadas de tela
        Vector3 screenPosition = new Vector3(
            viewportPosition.x * Screen.width,
            viewportPosition.y * Screen.height,
            0
        );
        
        // Converte para mundo 3D
        return ScreenToWorld(screenPosition, Camera.main, worldZ);
    }
    
    /// <summary>
    /// Converte múltiplas posições UI para mundo 3D
    /// </summary>
    public static Vector3[] UIToWorld(Vector2[] uiPositions, Canvas canvas, float worldZ = 0f)
    {
        Vector3[] worldPositions = new Vector3[uiPositions.Length];
        for (int i = 0; i < uiPositions.Length; i++)
        {
            worldPositions[i] = UIToWorld(uiPositions[i], canvas, worldZ);
        }
        return worldPositions;
    }
    
    #endregion
    
    #region World to UI Conversion
    
    /// <summary>
    /// Converte posição mundo 3D para posição UI (RectTransform.anchoredPosition)
    /// </summary>
    /// <param name="worldPosition">Posição no mundo 3D</param>
    /// <param name="canvas">Canvas de referência</param>
    /// <param name="camera">Câmera para conversão (se null, usa Camera.main)</param>
    /// <returns>Posição UI em coordenadas do Canvas</returns>
    public static Vector2 WorldToUI(Vector3 worldPosition, Canvas canvas, Camera camera = null)
    {
        if (canvas == null)
        {
            Debug.LogError("[CoordinateConverter] Canvas é null!");
            return Vector2.zero;
        }
        
        if (camera == null) camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("[CoordinateConverter] Câmera não encontrada!");
            return Vector2.zero;
        }
        
        // Converte mundo para tela
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
        
        // Converte tela para UI
        return ScreenToUI(screenPosition, canvas);
    }
    
    #endregion
    
    #region Screen Conversion
    
    /// <summary>
    /// Converte coordenadas de tela para mundo 3D
    /// </summary>
    /// <param name="screenPosition">Posição na tela (Input.mousePosition)</param>
    /// <param name="camera">Câmera para conversão</param>
    /// <param name="worldZ">Coordenada Z no mundo</param>
    /// <returns>Posição no mundo 3D</returns>
    public static Vector3 ScreenToWorld(Vector3 screenPosition, Camera camera, float worldZ = 0f)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("[CoordinateConverter] Câmera não encontrada!");
            return Vector3.zero;
        }
        
        // Para câmeras orthographic, usa distância da câmera + offset Z
        if (camera.orthographic)
        {
            screenPosition.z = camera.nearClipPlane + (camera.transform.position.z - worldZ);
        }
        else
        {
            // Para câmeras perspective, calcula distância baseada no Z desejado
            screenPosition.z = Vector3.Distance(camera.transform.position, new Vector3(0, 0, worldZ));
        }
        
        return camera.ScreenToWorldPoint(screenPosition);
    }
    
    /// <summary>
    /// Converte coordenadas de tela para UI
    /// </summary>
    /// <param name="screenPosition">Posição na tela</param>
    /// <param name="canvas">Canvas de referência</param>
    /// <returns>Posição UI em coordenadas do Canvas</returns>
    public static Vector2 ScreenToUI(Vector3 screenPosition, Canvas canvas)
    {
        if (canvas == null)
        {
            Debug.LogError("[CoordinateConverter] Canvas é null!");
            return Vector2.zero;
        }
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        // Converte para viewport (0-1)
        Vector2 viewportPosition = new Vector2(
            screenPosition.x / Screen.width,
            screenPosition.y / Screen.height
        );
        
        // Converte para coordenadas UI (-width/2 a +width/2)
        return new Vector2(
            (viewportPosition.x - 0.5f) * canvasRect.sizeDelta.x,
            (viewportPosition.y - 0.5f) * canvasRect.sizeDelta.y
        );
    }
    
    #endregion
    
    #region Mouse Utilities
    
    /// <summary>
    /// Retorna posição do mouse no mundo 3D
    /// </summary>
    /// <param name="worldZ">Coordenada Z no mundo</param>
    /// <param name="camera">Câmera para conversão</param>
    /// <returns>Posição do mouse no mundo 3D</returns>
    public static Vector3 GetMouseWorldPosition(float worldZ = 0f, Camera camera = null)
    {
        return ScreenToWorld(Input.mousePosition, camera, worldZ);
    }
    
    /// <summary>
    /// Retorna posição do mouse em coordenadas UI
    /// </summary>
    /// <param name="canvas">Canvas de referência</param>
    /// <returns>Posição do mouse em coordenadas UI</returns>
    public static Vector2 GetMouseUIPosition(Canvas canvas)
    {
        return ScreenToUI(Input.mousePosition, canvas);
    }
    
    #endregion
    
    #region Validation & Debug
    
    /// <summary>
    /// Valida se uma posição mundo está dentro dos bounds especificados
    /// </summary>
    /// <param name="worldPosition">Posição a validar</param>
    /// <param name="minBounds">Bounds mínimos</param>
    /// <param name="maxBounds">Bounds máximos</param>
    /// <returns>True se está dentro dos bounds</returns>
    public static bool IsWorldPositionInBounds(Vector3 worldPosition, Vector3 minBounds, Vector3 maxBounds)
    {
        return worldPosition.x >= minBounds.x && worldPosition.x <= maxBounds.x &&
               worldPosition.y >= minBounds.y && worldPosition.y <= maxBounds.y &&
               worldPosition.z >= minBounds.z && worldPosition.z <= maxBounds.z;
    }
    
    /// <summary>
    /// Clamp uma posição mundo dentro dos bounds especificados
    /// </summary>
    /// <param name="worldPosition">Posição a clampar</param>
    /// <param name="minBounds">Bounds mínimos</param>
    /// <param name="maxBounds">Bounds máximos</param>
    /// <returns>Posição clampada</returns>
    public static Vector3 ClampWorldPosition(Vector3 worldPosition, Vector3 minBounds, Vector3 maxBounds)
    {
        return new Vector3(
            Mathf.Clamp(worldPosition.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(worldPosition.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(worldPosition.z, minBounds.z, maxBounds.z)
        );
    }
    
    /// <summary>
    /// Retorna informações de debug sobre conversões
    /// </summary>
    /// <param name="testPosition">Posição para testar conversões</param>
    /// <param name="canvas">Canvas de referência</param>
    /// <param name="camera">Câmera de referência</param>
    /// <returns>String com informações de debug</returns>
    public static string GetDebugInfo(Vector3 testPosition, Canvas canvas, Camera camera = null)
    {
        if (camera == null) camera = Camera.main;
        
        Vector3 screenPos = camera.WorldToScreenPoint(testPosition);
        Vector2 uiPos = WorldToUI(testPosition, canvas, camera);
        Vector3 backToWorld = UIToWorld(uiPos, canvas);
        
        return $"=== Coordinate Conversion Debug ===\n" +
               $"Original World: {testPosition}\n" +
               $"Screen: {screenPos}\n" +
               $"UI: {uiPos}\n" +
               $"Back to World: {backToWorld}\n" +
               $"Conversion Error: {Vector3.Distance(testPosition, backToWorld):F4}\n" +
               $"Camera: {(camera != null ? camera.name : "null")}\n" +
               $"Canvas: {(canvas != null ? canvas.name : "null")}";
    }
    
    #endregion
} 