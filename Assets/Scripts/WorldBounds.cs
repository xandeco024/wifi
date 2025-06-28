using UnityEngine;

/// <summary>
/// Define os limites do mundo 3D onde objetos podem ser posicionados
/// Substitui os limites do Canvas no sistema UI
/// </summary>
public class WorldBounds : MonoBehaviour
{
    [Header("Bounds Settings")]
    [SerializeField] private Vector3 minBounds = new Vector3(-10f, -10f, 0f);
    [SerializeField] private Vector3 maxBounds = new Vector3(10f, 10f, 0f);
    
    [Header("Visual")]
    [SerializeField] private bool showBounds = true;
    [SerializeField] private Color boundsColor = Color.green;
    [SerializeField] private bool showCenter = true;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupFromCamera = false;
    [SerializeField] private float cameraMargin = 1f;
    
    // Singleton para fácil acesso
    public static WorldBounds Instance { get; private set; }
    
    // Propriedades públicas
    public Vector3 MinBounds => minBounds;
    public Vector3 MaxBounds => maxBounds;
    public Vector3 Center => (minBounds + maxBounds) * 0.5f;
    public Vector3 Size => maxBounds - minBounds;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"[WorldBounds] Múltiplas instâncias detectadas! Destruindo {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        // Auto-setup baseado na câmera se habilitado
        if (autoSetupFromCamera)
        {
            SetupFromCamera();
        }
    }
    
    void Start()
    {
        // Posiciona este GameObject no centro dos bounds
        transform.position = Center;
        
        Debug.Log($"[WorldBounds] Bounds configurados: Min{minBounds} Max{maxBounds} Size{Size}");
    }
    
    #region Bounds Validation
    
    /// <summary>
    /// Verifica se uma posição está dentro dos bounds
    /// </summary>
    /// <param name="position">Posição a verificar</param>
    /// <returns>True se está dentro dos bounds</returns>
    public bool IsInBounds(Vector3 position)
    {
        return position.x >= minBounds.x && position.x <= maxBounds.x &&
               position.y >= minBounds.y && position.y <= maxBounds.y &&
               position.z >= minBounds.z && position.z <= maxBounds.z;
    }
    
    /// <summary>
    /// Clamp uma posição dentro dos bounds
    /// </summary>
    /// <param name="position">Posição a clampar</param>
    /// <returns>Posição clampada dentro dos bounds</returns>
    public Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(position.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(position.z, minBounds.z, maxBounds.z)
        );
    }
    
    /// <summary>
    /// Retorna uma posição aleatória dentro dos bounds
    /// </summary>
    /// <returns>Posição aleatória válida</returns>
    public Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y),
            Random.Range(minBounds.z, maxBounds.z)
        );
    }
    
    /// <summary>
    /// Retorna uma posição aleatória em um círculo/anel ao redor de um ponto central
    /// </summary>
    /// <param name="center">Centro do círculo</param>
    /// <param name="minRadius">Raio mínimo</param>
    /// <param name="maxRadius">Raio máximo</param>
    /// <returns>Posição aleatória no anel, clampada aos bounds</returns>
    public Vector3 GetRandomPositionInRing(Vector3 center, float minRadius, float maxRadius)
    {
        // Gera ângulo aleatório
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Gera distância aleatória no anel
        float distance = Random.Range(minRadius, maxRadius);
        
        // Calcula posição
        Vector3 randomPosition = center + new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            center.z
        );
        
        // Clamp aos bounds
        return ClampToBounds(randomPosition);
    }
    
    /// <summary>
    /// Verifica se duas posições estão suficientemente distantes
    /// </summary>
    /// <param name="pos1">Primeira posição</param>
    /// <param name="pos2">Segunda posição</param>
    /// <param name="minDistance">Distância mínima requerida</param>
    /// <returns>True se estão suficientemente distantes</returns>
    public bool ArePositionsValid(Vector3 pos1, Vector3 pos2, float minDistance)
    {
        return Vector3.Distance(pos1, pos2) >= minDistance;
    }
    
    #endregion
    
    #region Configuration
    
    /// <summary>
    /// Configura os bounds manualmente
    /// </summary>
    /// <param name="newMinBounds">Novos bounds mínimos</param>
    /// <param name="newMaxBounds">Novos bounds máximos</param>
    public void SetBounds(Vector3 newMinBounds, Vector3 newMaxBounds)
    {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
        
        // Atualiza posição do GameObject
        transform.position = Center;
        
        Debug.Log($"[WorldBounds] Bounds atualizados: Min{minBounds} Max{maxBounds}");
    }
    
    /// <summary>
    /// Configura bounds baseado na visão da câmera
    /// </summary>
    /// <param name="camera">Câmera de referência (se null, usa Camera.main)</param>
    /// <param name="margin">Margem para reduzir a área (padrão: usa cameraMargin)</param>
    public void SetupFromCamera(Camera camera = null, float? margin = null)
    {
        if (camera == null) camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("[WorldBounds] Câmera não encontrada para auto-setup!");
            return;
        }
        
        float actualMargin = margin ?? cameraMargin;
        
        if (camera.orthographic)
        {
            // Câmera orthographic
            float halfHeight = camera.orthographicSize - actualMargin;
            float halfWidth = halfHeight * camera.aspect;
            
            Vector3 cameraPos = camera.transform.position;
            
            minBounds = new Vector3(cameraPos.x - halfWidth, cameraPos.y - halfHeight, 0f);
            maxBounds = new Vector3(cameraPos.x + halfWidth, cameraPos.y + halfHeight, 0f);
        }
        else
        {
            // Câmera perspective - calcula baseado na distância
            float distance = Mathf.Abs(camera.transform.position.z);
            float halfHeight = (Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance) - actualMargin;
            float halfWidth = halfHeight * camera.aspect;
            
            Vector3 cameraPos = camera.transform.position;
            
            minBounds = new Vector3(cameraPos.x - halfWidth, cameraPos.y - halfHeight, 0f);
            maxBounds = new Vector3(cameraPos.x + halfWidth, cameraPos.y + halfHeight, 0f);
        }
        
        // Atualiza posição do GameObject
        transform.position = Center;
        
        Debug.Log($"[WorldBounds] Auto-configurado pela câmera: Min{minBounds} Max{maxBounds}");
    }
    
    #endregion
    
    #region Debug & Gizmos
    
    void OnDrawGizmos()
    {
        if (!showBounds) return;
        
        // Desenha bounds
        Gizmos.color = boundsColor;
        Vector3 center = (minBounds + maxBounds) * 0.5f;
        Vector3 size = maxBounds - minBounds;
        Gizmos.DrawWireCube(center, size);
        
        // Desenha centro
        if (showCenter)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, 0.2f);
        }
        
        // Desenha coordenadas nos cantos
        Gizmos.color = Color.white;
        
        // Cantos do retângulo
        Vector3[] corners = {
            new Vector3(minBounds.x, minBounds.y, minBounds.z),
            new Vector3(maxBounds.x, minBounds.y, minBounds.z),
            new Vector3(maxBounds.x, maxBounds.y, minBounds.z),
            new Vector3(minBounds.x, maxBounds.y, minBounds.z)
        };
        
        foreach (var corner in corners)
        {
            Gizmos.DrawWireSphere(corner, 0.1f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Informações detalhadas quando selecionado
        Gizmos.color = Color.yellow;
        
        // Grid para referência visual
        int gridLines = 5;
        Vector3 size = maxBounds - minBounds;
        
        // Linhas verticais
        for (int i = 0; i <= gridLines; i++)
        {
            float x = minBounds.x + (size.x / gridLines) * i;
            Gizmos.DrawLine(
                new Vector3(x, minBounds.y, minBounds.z),
                new Vector3(x, maxBounds.y, minBounds.z)
            );
        }
        
        // Linhas horizontais
        for (int i = 0; i <= gridLines; i++)
        {
            float y = minBounds.y + (size.y / gridLines) * i;
            Gizmos.DrawLine(
                new Vector3(minBounds.x, y, minBounds.z),
                new Vector3(maxBounds.x, y, minBounds.z)
            );
        }
    }
    
    [ContextMenu("Setup From Camera")]
    private void TestSetupFromCamera()
    {
        SetupFromCamera();
    }
    
    [ContextMenu("Get Random Position")]
    private void TestGetRandomPosition()
    {
        Vector3 randomPos = GetRandomPosition();
        Debug.Log($"[WorldBounds] Posição aleatória: {randomPos}");
    }
    
    [ContextMenu("Print Bounds Info")]
    private void TestPrintBoundsInfo()
    {
        Debug.Log($"[WorldBounds] Min: {minBounds}, Max: {maxBounds}, Center: {Center}, Size: {Size}");
    }
    
    #endregion
} 