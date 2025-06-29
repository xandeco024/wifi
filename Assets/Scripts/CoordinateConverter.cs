using UnityEngine;

/// <summary>
/// Utilitário para conversão de coordenadas entre sistemas UI e 3D
/// Facilita a transição gradual do sistema UI para 3D
/// </summary>
public static class CoordinateConverter
{
    /// <summary>
    /// Converte posição UI (Canvas) para posição 3D no mundo
    /// </summary>
    /// <param name="uiPos">Posição no Canvas (anchoredPosition)</param>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <returns>Posição 3D no mundo</returns>
    public static Vector3 UIToWorld(Vector2 uiPos, Camera camera = null)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return Vector3.zero;
        
        // Converte posição UI para posição de tela
        Vector3 screenPos = new Vector3(uiPos.x + Screen.width * 0.5f, uiPos.y + Screen.height * 0.5f, 0f);
        
        // Converte para posição 3D
        Vector3 worldPos = camera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f; // Fixar Z para 2.5D
        
        return worldPos;
    }
    
    /// <summary>
    /// Converte posição 3D do mundo para posição UI (Canvas)
    /// </summary>
    /// <param name="worldPos">Posição 3D no mundo</param>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <returns>Posição no Canvas (anchoredPosition)</returns>
    public static Vector2 WorldToUI(Vector3 worldPos, Camera camera = null)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return Vector2.zero;
        
        // Converte posição 3D para posição de tela
        Vector3 screenPos = camera.WorldToScreenPoint(worldPos);
        
        // Converte para posição UI (anchoredPosition)
        Vector2 uiPos = new Vector2(screenPos.x - Screen.width * 0.5f, screenPos.y - Screen.height * 0.5f);
        
        return uiPos;
    }
    
    /// <summary>
    /// Converte posição de tela para posição 3D no mundo
    /// </summary>
    /// <param name="screenPos">Posição na tela (Input.mousePosition)</param>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <param name="z">Valor Z desejado (padrão: 0)</param>
    /// <returns>Posição 3D no mundo</returns>
    public static Vector3 ScreenToWorld(Vector2 screenPos, Camera camera = null, float z = 0f)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return Vector3.zero;
        
        Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(camera.transform.position.z - z)));
        worldPos.z = z;
        
        return worldPos;
    }
    
    /// <summary>
    /// Converte posição 3D do mundo para posição de tela
    /// </summary>
    /// <param name="worldPos">Posição 3D no mundo</param>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <returns>Posição na tela</returns>
    public static Vector2 WorldToScreen(Vector3 worldPos, Camera camera = null)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return Vector2.zero;
        
        Vector3 screenPos = camera.WorldToScreenPoint(worldPos);
        return new Vector2(screenPos.x, screenPos.y);
    }
    
    /// <summary>
    /// Verifica se uma posição 3D está visível na tela
    /// </summary>
    /// <param name="worldPos">Posição 3D no mundo</param>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <param name="margin">Margem de tolerância (0-1)</param>
    /// <returns>True se está visível na tela</returns>
    public static bool IsVisibleOnScreen(Vector3 worldPos, Camera camera = null, float margin = 0.1f)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return false;
        
        Vector3 screenPos = camera.WorldToViewportPoint(worldPos);
        
        return screenPos.x >= -margin && screenPos.x <= 1f + margin &&
               screenPos.y >= -margin && screenPos.y <= 1f + margin &&
               screenPos.z > 0f; // Deve estar na frente da câmera
    }
    
    /// <summary>
    /// Clamp uma posição 3D para ficar visível na tela
    /// </summary>
    /// <param name="worldPos">Posição 3D no mundo</param>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <param name="margin">Margem de tolerância (0-1)</param>
    /// <returns>Posição 3D clampada para ficar visível</returns>
    public static Vector3 ClampToScreen(Vector3 worldPos, Camera camera = null, float margin = 0.1f)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return worldPos;
        
        Vector3 screenPos = camera.WorldToViewportPoint(worldPos);
        
        // Clamp viewport coordinates
        screenPos.x = Mathf.Clamp(screenPos.x, margin, 1f - margin);
        screenPos.y = Mathf.Clamp(screenPos.y, margin, 1f - margin);
        
        // Converte de volta para world position
        Vector3 clampedWorldPos = camera.ViewportToWorldPoint(screenPos);
        clampedWorldPos.z = worldPos.z; // Mantém Z original
        
        return clampedWorldPos;
    }
    
    /// <summary>
    /// Calcula a distância entre duas posições considerando apenas X e Y
    /// </summary>
    /// <param name="pos1">Primeira posição</param>
    /// <param name="pos2">Segunda posição</param>
    /// <returns>Distância 2D</returns>
    public static float Distance2D(Vector3 pos1, Vector3 pos2)
    {
        return Vector2.Distance(new Vector2(pos1.x, pos1.y), new Vector2(pos2.x, pos2.y));
    }
    
    /// <summary>
    /// Interpola entre duas posições 3D considerando apenas X e Y
    /// </summary>
    /// <param name="from">Posição inicial</param>
    /// <param name="to">Posição final</param>
    /// <param name="t">Valor de interpolação (0-1)</param>
    /// <returns>Posição interpolada</returns>
    public static Vector3 Lerp2D(Vector3 from, Vector3 to, float t)
    {
        Vector2 from2D = new Vector2(from.x, from.y);
        Vector2 to2D = new Vector2(to.x, to.y);
        Vector2 result2D = Vector2.Lerp(from2D, to2D, t);
        
        return new Vector3(result2D.x, result2D.y, from.z);
    }
} 