using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public enum DeviceType
{
    Download,
    Crossover
}

[System.Serializable]
public class DeviceSpawnConfig
{
    [Header("Device Info")]
    public DeviceType deviceType = DeviceType.Download;
    public GameObject devicePrefab;
    public string deviceName = "PC";
    
    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    public float spawnWeight = 0.5f;
    
    [Header("Device-Specific Settings")]
    [Range(50, 1000)]
    public int minDownloadSize = 100;
    [Range(100, 1500)]
    public int maxDownloadSize = 300;
}

[System.Serializable]
public class SpawnLevel
{
    [Header("Level Requirements")]
    public int minScore = 0;
    public int maxScore = 20;
    
    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public GameObject[] devicePrefabs;
    [Tooltip("Número máximo de devices ativos neste nível")]
    public int maxActiveDevices = 3;

    [Header("Multiplicadores de Recompensa")]
    [Tooltip("Multiplicador de ouro para este nível")]
    [Range(1f, 5f)]
    public float goldMultiplier = 1f;
    
    [Tooltip("Multiplicador de pontos para este nível")]
    [Range(1f, 5f)]
    public float scoreMultiplier = 1f;
    
    [Tooltip("Multiplicador do tamanho de download para este nível")]
    [Range(1f, 5f)]
    public float downloadSizeMultiplier = 1f;

    public bool IsScoreInRange(int score)
    {
        return score >= minScore && score <= maxScore;
    }
}

public class DeviceSpawner : MonoBehaviour
{
    public static DeviceSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private SpawnLevel[] spawnLevels;
    [SerializeField] private Transform spawnContainer;

    private SpawnLevel currentLevel;
    private List<GameObject> activeDevices = new List<GameObject>();
    private float nextSpawnTime;
    private WorldBounds worldBounds;

    // Propriedades públicas para UI ou debug
    public int ActiveDeviceCount => activeDevices.Count;
    public int CurrentLevelMaxDevices => currentLevel != null ? currentLevel.maxActiveDevices : 0;
    
    // Propriedades dos multiplicadores atuais
    public float CurrentGoldMultiplier => currentLevel != null ? currentLevel.goldMultiplier : 1f;
    public float CurrentScoreMultiplier => currentLevel != null ? currentLevel.scoreMultiplier : 1f;
    public float CurrentDownloadMultiplier => currentLevel != null ? currentLevel.downloadSizeMultiplier : 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Encontra o WorldBounds
        worldBounds = WorldBounds.Instance;
        if (worldBounds == null)
        {
            Debug.LogError("WorldBounds não encontrado! DeviceSpawner desabilitado.");
            enabled = false;
            return;
        }

        if (spawnLevels == null || spawnLevels.Length == 0)
        {
            Debug.LogError("Nenhum nível de spawn configurado!");
            enabled = false;
            return;
        }

        UpdateCurrentLevel();
        nextSpawnTime = Time.time + currentLevel.spawnInterval;
        
        Debug.Log($"DeviceSpawner iniciado! Grid: {worldBounds.GridColumns}x{worldBounds.GridRows}");
    }

    private void Update()
    {
        UpdateCurrentLevel();

        // Só spawna se não atingiu o limite de devices e tem células livres
        if (Time.time >= nextSpawnTime && currentLevel != null && CanSpawnMoreDevices())
        {
            SpawnDevice();
            nextSpawnTime = Time.time + currentLevel.spawnInterval;
        }
    }

    private bool CanSpawnMoreDevices()
    {
        // Remove devices nulos da lista
        activeDevices.RemoveAll(device => device == null);
        
        // Verifica se ainda pode spawnar mais devices
        if (activeDevices.Count >= currentLevel.maxActiveDevices)
            return false;
            
        // Verifica se tem células livres no grid
        if (worldBounds.GetFreeCellsCount() <= 0)
        {
            Debug.LogWarning("Não há células livres no grid para spawn!");
            return false;
        }
        
        return true;
    }

    private void UpdateCurrentLevel()
    {
        int currentScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;

        foreach (var level in spawnLevels)
        {
            if (level.IsScoreInRange(currentScore))
            {
                if (currentLevel != level)
                {
                    currentLevel = level;
                    Debug.Log($"Nível atualizado! Multiplicadores - Ouro: {level.goldMultiplier}x, Score: {level.scoreMultiplier}x, Download: {level.downloadSizeMultiplier}x");
                }
                return;
            }
        }
    }

    private void SpawnDevice()
    {
        if (currentLevel.devicePrefabs == null || currentLevel.devicePrefabs.Length == 0)
            return;

        // Pega uma posição livre no grid
        Vector3 spawnPosition = worldBounds.GetRandomGridPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("Não foi possível encontrar posição livre no grid!");
            return;
        }

        // Escolhe um prefab aleatório do nível atual
        int randomIndex = Random.Range(0, currentLevel.devicePrefabs.Length);
        GameObject prefab = currentLevel.devicePrefabs[randomIndex];

        // Spawna o device
        GameObject newDevice = Instantiate(prefab, spawnPosition, Quaternion.identity, spawnContainer);
        activeDevices.Add(newDevice);

        // Ocupa a célula no grid
        worldBounds.OccupyGridCell(spawnPosition);

        // Configura o device
        Device device = newDevice.GetComponent<Device>();
        if (device != null)
        {
            // Aplica os multiplicadores do nível atual
            device.ApplyLevelMultipliers(
                currentLevel.scoreMultiplier,
                currentLevel.goldMultiplier,
                currentLevel.downloadSizeMultiplier
            );
            
            device.OnDeviceDestroyed += OnDeviceDestroyed;
            device.OnDeviceTimerExpired += HandleDeviceTimerExpired;
        }

        // SFX: som de spawn
        SFXManager.Play("spawn");

        Debug.Log($"Device spawnado em {spawnPosition}! ({activeDevices.Count}/{currentLevel.maxActiveDevices})");
    }

    private void OnDeviceDestroyed(GameObject device)
    {
        // Libera a célula no grid
        if (device != null)
        {
            worldBounds.FreeGridCell(device.transform.position);
        }
        
        activeDevices.Remove(device);
        Debug.Log($"Device destruído! ({activeDevices.Count}/{currentLevel.maxActiveDevices})");
    }

    private void HandleDeviceTimerExpired(Device device)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
    }

    public void DestroyAllDevices()
    {
        foreach (var device in activeDevices.ToArray())
        {
            if (device != null)
            {
                // Libera a célula no grid
                worldBounds.FreeGridCell(device.transform.position);
                
                Device deviceComponent = device.GetComponent<Device>();
                if (deviceComponent != null)
                {
                    deviceComponent.ForceDestroy();
                }
            }
        }
        activeDevices.Clear();
        Debug.Log("Todos os devices foram destruídos e células liberadas!");
    }
} 