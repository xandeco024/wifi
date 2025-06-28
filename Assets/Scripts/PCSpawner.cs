using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pcPrefab;
    [SerializeField] private Transform modemTransform; // Mudança: RectTransform → Transform para 3D
    [SerializeField] private Transform pcContainer; // Parent para organizar os PCs na hierarquia (3D)
    [SerializeField] private WorldBounds worldBounds; // Bounds do mundo 3D
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f; // Tempo entre spawns
    [SerializeField] private float minDistanceFromModem = 150f; // Distância mínima do modem
    [SerializeField] private float maxDistanceFromModem = 400f; // Distância máxima do modem
    [SerializeField] private float minDistanceBetweenPCs = 100f; // Distância mínima entre PCs
    [SerializeField] private int maxActivePCs = 8; // Máximo de PCs ativos simultaneamente
    
    [Header("Screen Bounds")]
    [SerializeField] private float screenMargin = 50f; // Margem da borda da tela
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showSpawnGizmos = true;
    [SerializeField] private bool isSpawningPaused = false;
    
    // Listas de controle
    private List<GameObject> activePCs = new List<GameObject>();
    private List<Vector2> occupiedPositions = new List<Vector2>();
    
    // Propriedades do mundo 3D
    private Vector2 canvasSize; // Manter compatibilidade (não usado mais)
    private Vector3 worldSize; // Tamanho do mundo 3D (substitui canvasRectTransform.sizeDelta)
    
    // Controle de spawn
    private Coroutine spawnCoroutine;
    
    // Singleton para fácil acesso
    public static PCSpawner Instance { get; private set; }
    
    // Eventos
    public System.Action<GameObject> OnPCSpawned;
    public System.Action<GameObject> OnPCDespawned;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Configuração inicial do Canvas
                // Auto-configura WorldBounds se não foi definido
        if (worldBounds == null)
        {
            worldBounds = WorldBounds.Instance;
            if (worldBounds == null)
            {
                worldBounds = FindObjectOfType<WorldBounds>();
            }
        }
        
                if (worldBounds != null)
        {
            worldSize = worldBounds.Size;
        }
            
        UpdateCanvasBounds();
    }
    
    void Start()
    {
        // Validações iniciais
        if (ValidateReferences())
        {
            StartSpawning();
        }
    }
    
    void Update()
    {
        // Atualiza bounds do canvas se necessário (para suporte a mudança de resolução)
        UpdateCanvasBounds();
        
        // Remove PCs destruídos da lista
        CleanupDestroyedPCs();
        
        // Debug info
        if (enableDebugLogs && Input.GetKeyDown(KeyCode.Space))
        {
            DebugLog($"PCs ativos: {activePCs.Count}/{maxActivePCs}");
        }
    }
    
    #region Spawn System
    
    public void StartSpawning()
    {
        if (spawnCoroutine == null && !isSpawningPaused)
        {
            spawnCoroutine = StartCoroutine(SpawnLoop());
            DebugLog("Sistema de spawn iniciado");
        }
    }
    
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            DebugLog("Sistema de spawn parado");
        }
    }
    
    public void PauseSpawning()
    {
        isSpawningPaused = true;
        DebugLog("Spawn pausado");
    }
    
    public void ResumeSpawning()
    {
        isSpawningPaused = false;
        if (spawnCoroutine == null)
            StartSpawning();
        DebugLog("Spawn retomado");
    }
    
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (!isSpawningPaused && CanSpawnNewPC())
            {
                SpawnPC();
            }
        }
    }
    
    private bool CanSpawnNewPC()
    {
        return activePCs.Count < maxActivePCs && pcPrefab != null;
    }
    
    private void SpawnPC()
    {
        Vector2 spawnPosition = FindValidSpawnPosition();
        
        if (spawnPosition != Vector2.zero)
        {
            GameObject newPC = Instantiate(pcPrefab);
            
            // Define o parent para organização
            if (pcContainer != null)
                newPC.transform.SetParent(pcContainer, false);
            
                    // Configura posição usando Transform 3D
        Transform pcTransform = newPC.transform;
        if (pcTransform != null)
        {
            pcTransform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0f);
            }
            
            // Adiciona à lista de controle
            activePCs.Add(newPC);
            occupiedPositions.Add(spawnPosition);
            
            // Configura o PC
            ComputerBehavior pcBehavior = newPC.GetComponent<ComputerBehavior>();
            if (pcBehavior != null)
            {
                pcBehavior.OnPCDestroyed += OnPCWasDestroyed;
            }
            
            DebugLog($"PC spawnado em {spawnPosition} (Total: {activePCs.Count})");
            
            // Dispara evento
            OnPCSpawned?.Invoke(newPC);
        }
        else
        {
            DebugLog("Não foi possível encontrar posição válida para spawn");
        }
    }
    
    private Vector2 FindValidSpawnPosition()
    {
        int maxAttempts = 30;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidatePosition = GenerateRandomPosition();
            
            if (IsPositionValid(candidatePosition))
            {
                return candidatePosition;
            }
        }
        
        return Vector2.zero; // Posição inválida
    }
    
    private Vector2 GenerateRandomPosition()
    {
        // Gera posição em anel ao redor do modem
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minDistanceFromModem, maxDistanceFromModem);
        
        Vector2 modemPos = new Vector2(modemTransform.position.x, modemTransform.position.y);
        Vector2 randomPosition = modemPos + new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance
        );
        
        return randomPosition;
    }
    
    private bool IsPositionValid(Vector2 position)
    {
        // Verifica se está dentro da tela
        if (!IsWithinScreenBounds(position))
            return false;
            
        // Verifica distância de outros PCs
        foreach (Vector2 occupiedPos in occupiedPositions)
        {
            if (Vector2.Distance(position, occupiedPos) < minDistanceBetweenPCs)
                return false;
        }
        
        return true;
    }
    
    private bool IsWithinScreenBounds(Vector2 position)
    {
        if (worldBounds == null) return false;
        
        // Converte para coordenadas locais do canvas
        Vector2 localPos = position;
        
        // Verifica se está dentro dos limites do canvas considerando as margens
        // Usa WorldBounds ao invés de cálculos manuais
        Vector3 worldPos = new Vector3(position.x, position.y, 0f);
        return worldBounds.IsWithinBounds(worldPos);
                // Função completa - return já feito acima
    }
    
    #endregion
    
    #region PC Management
    
    private void OnPCWasDestroyed(GameObject destroyedPC)
    {
        if (activePCs.Contains(destroyedPC))
        {
            // Remove da lista de PCs ativos
            activePCs.Remove(destroyedPC);
            
            // Remove posição ocupada
            Transform pcTransform = destroyedPC.transform;
            if (pcTransform != null)
            {
                Vector2 pcPosition = new Vector2(pcTransform.position.x, pcTransform.position.y);
                occupiedPositions.RemoveAll(pos => Vector2.Distance(pos, pcPosition) < 10f);
            }
            
            DebugLog($"PC removido das listas (Restam: {activePCs.Count})");
            
            // Dispara evento
            OnPCDespawned?.Invoke(destroyedPC);
        }
    }
    
    private void CleanupDestroyedPCs()
    {
        for (int i = activePCs.Count - 1; i >= 0; i--)
        {
            if (activePCs[i] == null)
            {
                activePCs.RemoveAt(i);
                if (i < occupiedPositions.Count)
                    occupiedPositions.RemoveAt(i);
            }
        }
    }
    
    public void DestroyAllPCs()
    {
        foreach (GameObject pc in activePCs)
        {
            if (pc != null)
                Destroy(pc);
        }
        
        activePCs.Clear();
        occupiedPositions.Clear();
        DebugLog("Todos os PCs foram destruídos");
    }
    
    #endregion
    
    #region Utility Methods
    
    private bool ValidateReferences()
    {
        bool isValid = true;
        
        if (pcPrefab == null)
        {
            Debug.LogError("[PCSpawner] PC Prefab não está definido!");
            isValid = false;
        }
        
        if (modemTransform == null)
        {
            Debug.LogError("[PCSpawner] Modem Transform não está definido!");
            isValid = false;
        }
        
        if (pcContainer == null)
        {
            DebugLog("PC Container não definido - PCs serão criados na raiz");
        }
        
        return isValid;
    }
    
    private void UpdateCanvasBounds()
    {
        if (worldBounds != null)
        {
            worldSize = worldBounds.Size;
        }
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PCSpawner] {message}");
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Força o spawn de um PC imediatamente
    /// </summary>
    public void ForceSpawnPC()
    {
        if (CanSpawnNewPC())
        {
            SpawnPC();
        }
        else
        {
            DebugLog("Não é possível forçar spawn - limite atingido ou prefab inválido");
        }
    }
    
    /// <summary>
    /// Altera a velocidade de spawn em runtime
    /// </summary>
    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = Mathf.Max(0.1f, newInterval);
        DebugLog($"Intervalo de spawn alterado para {spawnInterval}s");
    }
    
    /// <summary>
    /// Retorna informações de debug
    /// </summary>
    public string GetSpawnInfo()
    {
        return $"PCs: {activePCs.Count}/{maxActivePCs}, Interval: {spawnInterval}s, Paused: {isSpawningPaused}";
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (!showSpawnGizmos || modemTransform == null) return;
        
        // Usa diretamente a posição 3D do modem
        Vector3 modemWorldPos = modemTransform.position;
        
        // Área mínima de spawn (ao redor do modem)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(modemWorldPos, minDistanceFromModem);
        
        // Área máxima de spawn
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(modemWorldPos, maxDistanceFromModem);
        
        // Posições ocupadas
        Gizmos.color = Color.yellow;
        foreach (Vector2 pos in occupiedPositions)
        {
            // Converte posição UI para mundo para desenhar gizmo
            Vector3 worldPos = new Vector3(pos.x, pos.y, 0f);
            Gizmos.DrawWireSphere(worldPos, minDistanceBetweenPCs / 2f);
        }
    }
    
    #endregion
    
    #region Debug & Testing (3D)
    
    [ContextMenu("Testar Spawn 3D")]
    private void TestSpawn3D()
    {
        if (Application.isPlaying)
        {
            SpawnPC();
            Debug.Log("[PCSpawner] Teste de spawn 3D executado!");
        }
        else
        {
            Debug.LogWarning("[PCSpawner] Teste só funciona no Play Mode!");
        }
    }
    
    [ContextMenu("Validar Setup 3D")]
    private void ValidateSetup3D()
    {
        Debug.Log("=== VALIDAÇÃO PCSpawner 3D ===");
        Debug.Log($"modemTransform: {(modemTransform != null ? "✓" : "✗ FALTANDO")}");
        Debug.Log($"pcContainer: {(pcContainer != null ? "✓" : "✗ FALTANDO")}");
        Debug.Log($"worldBounds: {(worldBounds != null ? "✓" : "✗ FALTANDO")}");
        Debug.Log($"pcPrefab: {(pcPrefab != null ? "✓" : "✗ FALTANDO")}");
        
        if (worldBounds != null)
        {
            Debug.Log($"World Size: {worldBounds.Size}");
        }
    }
    
    #endregion
} 