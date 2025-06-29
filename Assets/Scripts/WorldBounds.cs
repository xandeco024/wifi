using UnityEngine;

/// <summary>
/// Sistema de Grid 3D para spawn de devices
/// Grid sempre ímpar com célula central reservada para o modem
/// </summary>
public class WorldBounds : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridColumns = 9; // Sempre ímpar (colunas no eixo X)
    [SerializeField] private int gridRows = 9; // Sempre ímpar (linhas no eixo Z)
    [SerializeField] private float cellSize = 2f; // Tamanho de cada célula
    [SerializeField] private Vector3 gridCenter = Vector3.zero; // Centro do grid (posição do modem)
    [SerializeField] private float gridYPosition = 0f; // Altura fixa do grid
    
    [Header("Visual")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = Color.cyan;
    [SerializeField] private Color occupiedColor = Color.red;
    [SerializeField] private Color centerColor = Color.yellow;
    
    // Singleton para fácil acesso
    public static WorldBounds Instance { get; private set; }
    
    // Grid system
    private bool[,] gridOccupied; // Controla quais células estão ocupadas
    private Vector3[,] gridPositions; // Posições mundiais de cada célula
    private Vector2Int centerCell; // Coordenadas da célula central (modem)
    
    // Propriedades públicas
    public int GridColumns => gridColumns;
    public int GridRows => gridRows;
    public float CellSize => cellSize;
    public Vector3 GridCenter => gridCenter;
    public int TotalGridCells => gridColumns * gridRows;
    public Vector2Int CenterCell => centerCell;
    
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
        
        // Força valores ímpares
        EnsureOddGridSize();
        
        // Inicializa grid imediatamente no Awake
        InitializeGrid();
        
        Debug.Log($"[WorldBounds] Grid inicializado no Awake: {gridColumns}x{gridRows} células, tamanho {cellSize}");
        Debug.Log($"[WorldBounds] Célula central (modem): {centerCell}");
    }
    
    void Start()
    {
        // Posiciona este GameObject no centro do grid
        transform.position = gridCenter;
    }
    
    /// <summary>
    /// Garante que o grid sempre tenha tamanho ímpar
    /// </summary>
    private void EnsureOddGridSize()
    {
        if (gridColumns % 2 == 0)
        {
            gridColumns++;
            Debug.LogWarning($"[WorldBounds] Colunas ajustadas para {gridColumns} (deve ser ímpar)");
        }
        
        if (gridRows % 2 == 0)
        {
            gridRows++;
            Debug.LogWarning($"[WorldBounds] Linhas ajustadas para {gridRows} (deve ser ímpar)");
        }
    }
    
    /// <summary>
    /// Inicializa o sistema de grid
    /// </summary>
    private void InitializeGrid()
    {
        gridOccupied = new bool[gridColumns, gridRows];
        gridPositions = new Vector3[gridColumns, gridRows];
        
        // Calcula célula central
        centerCell = new Vector2Int(gridColumns / 2, gridRows / 2);
        
        // Calcula posições de cada célula
        for (int x = 0; x < gridColumns; x++)
        {
            for (int z = 0; z < gridRows; z++)
            {
                // Calcula posição mundial da célula
                float worldX = gridCenter.x + (x - gridColumns / 2) * cellSize;
                float worldZ = gridCenter.z + (z - gridRows / 2) * cellSize;
                
                gridPositions[x, z] = new Vector3(worldX, gridYPosition, worldZ);
                
                // Marca célula central como ocupada (modem)
                if (x == centerCell.x && z == centerCell.y)
                {
                    gridOccupied[x, z] = true;
                }
                else
                {
                    gridOccupied[x, z] = false;
                }
            }
        }
        
        Debug.Log($"[WorldBounds] Grid inicializado com {gridColumns * gridRows} células");
        Debug.Log($"[WorldBounds] Célula central {centerCell} reservada para modem");
    }
    
    /// <summary>
    /// Atualiza o centro do grid (geralmente posição do modem)
    /// </summary>
    /// <param name="newCenter">Nova posição central</param>
    public void SetGridCenter(Vector3 newCenter)
    {
        gridCenter = newCenter;
        InitializeGrid(); // Recalcula posições
        transform.position = gridCenter;
    }
    
    /// <summary>
    /// Retorna uma posição livre aleatória no grid (exceto célula central)
    /// </summary>
    /// <returns>Posição livre ou Vector3.zero se não houver</returns>
    public Vector3 GetRandomGridPosition()
    {
        // Lista de células livres (exceto central)
        System.Collections.Generic.List<Vector2Int> freeCells = new System.Collections.Generic.List<Vector2Int>();
        
        for (int x = 0; x < gridColumns; x++)
        {
            for (int z = 0; z < gridRows; z++)
            {
                // Pula célula central e células ocupadas
                if ((x == centerCell.x && z == centerCell.y) || gridOccupied[x, z])
                    continue;
                
                freeCells.Add(new Vector2Int(x, z));
            }
        }
        
        if (freeCells.Count == 0)
        {
            Debug.LogWarning("[WorldBounds] Nenhuma célula livre no grid!");
            return Vector3.zero;
        }
        
        // Seleciona célula aleatória
        Vector2Int selectedCell = freeCells[Random.Range(0, freeCells.Count)];
        return gridPositions[selectedCell.x, selectedCell.y];
    }
    
    /// <summary>
    /// Ocupa uma célula do grid na posição mais próxima
    /// </summary>
    /// <param name="worldPosition">Posição mundial</param>
    /// <returns>True se conseguiu ocupar</returns>
    public bool OccupyGridCell(Vector3 worldPosition)
    {
        Vector2Int gridCoord = WorldToGridCoordinates(worldPosition);
        
        if (IsValidGridCoordinate(gridCoord) && !gridOccupied[gridCoord.x, gridCoord.y])
        {
            gridOccupied[gridCoord.x, gridCoord.y] = true;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Libera uma célula do grid
    /// </summary>
    /// <param name="worldPosition">Posição mundial</param>
    public void FreeGridCell(Vector3 worldPosition)
    {
        Vector2Int gridCoord = WorldToGridCoordinates(worldPosition);
        
        if (IsValidGridCoordinate(gridCoord))
        {
            // Não permite liberar a célula central
            if (gridCoord.x == centerCell.x && gridCoord.y == centerCell.y)
                return;
                
            gridOccupied[gridCoord.x, gridCoord.y] = false;
        }
    }
    
    /// <summary>
    /// Converte posição mundial para coordenadas do grid
    /// </summary>
    /// <param name="worldPosition">Posição mundial</param>
    /// <returns>Coordenadas do grid</returns>
    public Vector2Int WorldToGridCoordinates(Vector3 worldPosition)
    {
        float relativeX = worldPosition.x - gridCenter.x;
        float relativeZ = worldPosition.z - gridCenter.z;
        
        int gridX = Mathf.RoundToInt(relativeX / cellSize + gridColumns / 2);
        int gridZ = Mathf.RoundToInt(relativeZ / cellSize + gridRows / 2);
        
        return new Vector2Int(gridX, gridZ);
    }
    
    /// <summary>
    /// Verifica se as coordenadas do grid são válidas
    /// </summary>
    /// <param name="gridCoord">Coordenadas do grid</param>
    /// <returns>True se válidas</returns>
    public bool IsValidGridCoordinate(Vector2Int gridCoord)
    {
        return gridCoord.x >= 0 && gridCoord.x < gridColumns &&
               gridCoord.y >= 0 && gridCoord.y < gridRows;
    }
    
    /// <summary>
    /// Retorna quantas células estão ocupadas (incluindo central)
    /// </summary>
    /// <returns>Número de células ocupadas</returns>
    public int GetOccupiedCellsCount()
    {
        int count = 0;
        for (int x = 0; x < gridColumns; x++)
        {
            for (int z = 0; z < gridRows; z++)
            {
                if (gridOccupied[x, z]) count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Retorna quantas células estão livres para spawn (exceto central)
    /// </summary>
    /// <returns>Número de células livres</returns>
    public int GetFreeCellsCount()
    {
        return TotalGridCells - GetOccupiedCellsCount();
    }
    
    /// <summary>
    /// Retorna a posição mundial da célula central (modem)
    /// </summary>
    /// <returns>Posição mundial do centro</returns>
    public Vector3 GetCenterCellPosition()
    {
        if (gridPositions == null)
        {
            Debug.LogWarning("[WorldBounds] Grid não inicializado - retornando gridCenter");
            return gridCenter;
        }
        return gridPositions[centerCell.x, centerCell.y];
    }
    
    #region Debug & Gizmos
    
    void OnDrawGizmos()
    {
        if (!showGrid) return;
        
        // Se grid não foi inicializado, calcula posições temporárias para preview
        if (gridPositions == null)
        {
            DrawPreviewGrid();
            return;
        }
        
        DrawGrid();
    }
    
    private void DrawPreviewGrid()
    {
        // Preview do grid no editor antes da inicialização
        EnsureOddGridSize();
        
        Vector2Int previewCenter = new Vector2Int(gridColumns / 2, gridRows / 2);
        
        // Grid cells preview
        for (int x = 0; x < gridColumns; x++)
        {
            for (int z = 0; z < gridRows; z++)
            {
                float worldX = gridCenter.x + (x - gridColumns / 2) * cellSize;
                float worldZ = gridCenter.z + (z - gridRows / 2) * cellSize;
                Vector3 cellPosition = new Vector3(worldX, gridYPosition, worldZ);
                
                // Cor baseada no tipo de célula
                if (x == previewCenter.x && z == previewCenter.y)
                {
                    Gizmos.color = centerColor; // Célula central (modem)
                    Gizmos.DrawCube(cellPosition, Vector3.one * (cellSize * 0.9f));
                }
                else
                {
                    Gizmos.color = gridColor; // Células livres
                    Gizmos.DrawWireCube(cellPosition, Vector3.one * cellSize);
                }
            }
        }
        
        // Centro do grid
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gridCenter, 0.3f);
    }
    
    private void DrawGrid()
    {
        // Centro do grid
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gridCenter, 0.3f);
        
        // Grid cells
        for (int x = 0; x < gridColumns; x++)
        {
            for (int z = 0; z < gridRows; z++)
            {
                Vector3 cellPosition = gridPositions[x, z];
                
                // Cor baseada no estado da célula
                if (x == centerCell.x && z == centerCell.y)
                {
                    // Célula central (modem)
                    Gizmos.color = centerColor;
                    Gizmos.DrawCube(cellPosition, Vector3.one * (cellSize * 0.9f));
                }
                else if (gridOccupied[x, z])
                {
                    // Célula ocupada
                    Gizmos.color = occupiedColor;
                    Gizmos.DrawCube(cellPosition, Vector3.one * (cellSize * 0.8f));
                }
                else
                {
                    // Célula livre
                    Gizmos.color = gridColor;
                    Gizmos.DrawWireCube(cellPosition, Vector3.one * cellSize);
                }
                
                // Ponto central da célula
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(cellPosition, 0.05f);
            }
        }
        
        // Linhas do grid
        Gizmos.color = gridColor * 0.3f;
        DrawGridLines();
    }
    
    private void DrawGridLines()
    {
        float halfWidth = (gridColumns * cellSize) / 2f;
        float halfHeight = (gridRows * cellSize) / 2f;
        
        // Linhas horizontais (X)
        for (int z = 0; z <= gridRows; z++)
        {
            Vector3 start = new Vector3(
                gridCenter.x - halfWidth,
                gridYPosition,
                gridCenter.z - halfHeight + z * cellSize
            );
            Vector3 end = new Vector3(
                gridCenter.x + halfWidth,
                gridYPosition,
                gridCenter.z - halfHeight + z * cellSize
            );
            Gizmos.DrawLine(start, end);
        }
        
        // Linhas verticais (Z)
        for (int x = 0; x <= gridColumns; x++)
        {
            Vector3 start = new Vector3(
                gridCenter.x - halfWidth + x * cellSize,
                gridYPosition,
                gridCenter.z - halfHeight
            );
            Vector3 end = new Vector3(
                gridCenter.x - halfWidth + x * cellSize,
                gridYPosition,
                gridCenter.z + halfHeight
            );
            Gizmos.DrawLine(start, end);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (gridPositions == null) return;
        
        // Informações detalhadas quando selecionado
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            gridCenter + Vector3.up * 2f,
            $"Grid: {GetFreeCellsCount()}/{TotalGridCells - 1} livres\n" +
            $"Centro: {centerCell}\n" +
            $"Tamanho: {gridColumns}x{gridRows}"
        );
        #endif
    }
    
    [ContextMenu("Reset Grid")]
    private void ResetGrid()
    {
        InitializeGrid();
        Debug.Log("[WorldBounds] Grid resetado");
    }
    
    [ContextMenu("Print Grid Info")]
    private void PrintGridInfo()
    {
        Debug.Log($"[WorldBounds] Grid Info:\n" +
                  $"Tamanho: {gridColumns}x{gridRows}\n" +
                  $"Células totais: {TotalGridCells}\n" +
                  $"Células livres: {GetFreeCellsCount()}\n" +
                  $"Células ocupadas: {GetOccupiedCellsCount()}\n" +
                  $"Centro: {centerCell} @ {GetCenterCellPosition()}");
    }
    
    #endregion
} 