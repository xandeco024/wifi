using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



[System.Serializable]
public class SpawnLevel
{
    [Header("Level Requirements")]
    public int minScore = 0;
    public int maxScore = 20;
    
    [Header("Device Settings")]
    [Range(5f, 20f)]
    public float baseTimeLimit = 15f;
    [Range(50, 500)]
    public int minDownloadSize = 50;
    [Range(100, 1000)] 
    public int maxDownloadSize = 150;
    [Range(5, 50)]
    public int baseCoinReward = 10;
    [Range(10, 100)]
    public int basePointsReward = 15;
    
    [Header("Spawn Behavior")]
    [Range(2f, 10f)]
    public float spawnInterval = 5f;
    [Range(0.1f, 1f)]
    public float spawnWeight = 0.3f;
    [Range(1, 20)]
    public int maxSimultaneousDevices = 2;
    
    public bool IsScoreInRange(int currentScore)
    {
        return currentScore >= minScore && currentScore <= maxScore;
    }
}

public class DeviceSpawner : MonoBehaviour
{
    [Header("Spawn Level System")]
    [SerializeField] private SpawnLevel[] spawnLevels = new SpawnLevel[]
    {
        new SpawnLevel { minScore = 0, maxScore = 20, baseTimeLimit = 15f, minDownloadSize = 50, maxDownloadSize = 150, baseCoinReward = 8, basePointsReward = 10, spawnInterval = 6f, spawnWeight = 0.3f, maxSimultaneousDevices = 2 },
        new SpawnLevel { minScore = 21, maxScore = 60, baseTimeLimit = 12f, minDownloadSize = 100, maxDownloadSize = 250, baseCoinReward = 12, basePointsReward = 15, spawnInterval = 5f, spawnWeight = 0.4f, maxSimultaneousDevices = 3 },
        new SpawnLevel { minScore = 61, maxScore = 120, baseTimeLimit = 10f, minDownloadSize = 150, maxDownloadSize = 350, baseCoinReward = 16, basePointsReward = 20, spawnInterval = 4f, spawnWeight = 0.5f, maxSimultaneousDevices = 4 },
        new SpawnLevel { minScore = 121, maxScore = 200, baseTimeLimit = 8f, minDownloadSize = 200, maxDownloadSize = 450, baseCoinReward = 20, basePointsReward = 25, spawnInterval = 3f, spawnWeight = 0.6f, maxSimultaneousDevices = 5 },
        new SpawnLevel { minScore = 201, maxScore = 9999, baseTimeLimit = 6f, minDownloadSize = 300, maxDownloadSize = 600, baseCoinReward = 25, basePointsReward = 30, spawnInterval = 2.5f, spawnWeight = 0.7f, maxSimultaneousDevices = 6 }
    };
    
    [Header("Device Prefab")]
    [SerializeField] private GameObject devicePrefab;
    [SerializeField] private Transform deviceContainer;
    [SerializeField] private WorldBounds worldBounds;
    
    private List<GameObject> activeDevices = new List<GameObject>();
    private float lastLevelSpawnTime = 0f;
    private SpawnLevel currentLevel;
    
    public static DeviceSpawner Instance { get; private set; }
    
    public System.Action<GameObject> OnDeviceSpawned;
    public System.Action<GameObject> OnDeviceDestroyed;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (deviceContainer == null)
        {
            GameObject containerObj = new GameObject("Device Container");
            containerObj.transform.SetParent(transform);
            deviceContainer = containerObj.transform;
        }
        
        if (worldBounds == null)
        {
            worldBounds = WorldBounds.Instance;
        }
    }
    
    void Start()
    {
        if (ValidateSetup())
        {
            // Inicializa o level atual
            UpdateCurrentLevel();
            
            Debug.Log($"DeviceSpawner iniciado - {GetCurrentLevelInfo()}");
        }
    }
    
    void Update()
    {
        // Atualiza o level atual baseado no score
        UpdateCurrentLevel();
        
        // Tenta spawnar baseado no level atual
        if (currentLevel != null && CanSpawnForCurrentLevel())
        {
            TrySpawnDeviceForLevel();
        }
    }
    
    private bool ValidateSetup()
    {
        if (devicePrefab == null)
        {
            Debug.LogError("DeviceSpawner: Device prefab não definido!");
            return false;
        }
        
        if (spawnLevels == null || spawnLevels.Length == 0)
        {
            Debug.LogError("DeviceSpawner: Nenhum spawn level configurado!");
            return false;
        }
        
        if (worldBounds == null)
        {
            Debug.LogError("DeviceSpawner: WorldBounds não encontrado!");
            return false;
        }
        
        return true;
    }
    
    private void UpdateCurrentLevel()
    {
        int currentScore = ScoreAndCoinsManager.Instance != null ? ScoreAndCoinsManager.Instance.GetCurrentScore() : 0;
        
        SpawnLevel newLevel = null;
        foreach (SpawnLevel level in spawnLevels)
        {
            if (level.IsScoreInRange(currentScore))
            {
                newLevel = level;
                break;
            }
        }
        
        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            Debug.Log($"Level mudou (Score: {currentScore}, Range: {currentLevel?.minScore}-{currentLevel?.maxScore})");
        }
    }
    
    private bool CanSpawnForCurrentLevel()
    {
        if (currentLevel == null) return false;
        
        float timeSinceLastSpawn = Time.time - lastLevelSpawnTime;
        return timeSinceLastSpawn >= currentLevel.spawnInterval;
    }
    
    private void TrySpawnDeviceForLevel()
    {
        if (currentLevel == null || devicePrefab == null) return;
        
        // Verifica limite de devices simultâneos para o level atual
        int currentDeviceCount = GetActiveDeviceCount();
        Debug.Log($"Verificando limite: {currentDeviceCount}/{currentLevel.maxSimultaneousDevices} devices");
        
        if (currentDeviceCount >= currentLevel.maxSimultaneousDevices)
        {
            Debug.Log($"🚫 SPAWN REJEITADO - Limite atingido: {currentDeviceCount}/{currentLevel.maxSimultaneousDevices} devices");
            return;
        }
        
        float randomValue = Random.value;
        Debug.Log($"TrySpawn: random={randomValue:F2}, weight={currentLevel.spawnWeight}, devices={currentDeviceCount}/{currentLevel.maxSimultaneousDevices}");
        
        if (randomValue > currentLevel.spawnWeight) 
        {
            Debug.Log("Spawn rejeitado por peso");
            return;
        }
        
        Vector3 spawnPosition = worldBounds.GetRandomGridPosition();
        Debug.Log($"Posição de spawn obtida: {spawnPosition}");
        
        if (spawnPosition != Vector3.zero)
        {
            SpawnDeviceForLevel(spawnPosition);
            lastLevelSpawnTime = Time.time;
        }
        else
        {
            Debug.LogWarning("Nenhuma posição livre para spawn");
        }
    }
    

    
    private void SpawnDeviceForLevel(Vector3 position)
    {
        if (currentLevel == null || devicePrefab == null) return;
        
        GameObject newDevice = Instantiate(devicePrefab, position, Quaternion.identity);
        
        newDevice.transform.SetParent(deviceContainer, false);
        newDevice.transform.position = position;
        newDevice.transform.rotation = Quaternion.identity;
        
        worldBounds.OccupyGridCell(position);
        
        int downloadSize = Random.Range(currentLevel.minDownloadSize, currentLevel.maxDownloadSize + 1);
        
        Device deviceComponent = newDevice.GetComponent<Device>();
        if (deviceComponent != null)
        {
            // Usa configurações do level atual em vez do config base
            deviceComponent.SetSpawnConfig(currentLevel.baseTimeLimit, currentLevel.basePointsReward, downloadSize);
            deviceComponent.OnDeviceDestroyed += OnDeviceWasDestroyed;
            deviceComponent.OnDeviceCompleted += OnDeviceWasCompleted;
            Debug.Log($"Eventos configurados para device {newDevice.name}");
        }
        else
        {
            Debug.LogWarning($"Device {newDevice.name} não tem componente Device!");
        }
        
        activeDevices.Add(newDevice);
        Debug.Log($"Device adicionado à lista. Total devices: {activeDevices.Count}");
        
        newDevice.transform.localScale = Vector3.zero;
        newDevice.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
        
        if (Camera.main != null)
        {
            Camera.main.DOShakePosition(0.2f, 0.1f, 5, 90);
        }
        
        Debug.Log($"Device spawnado em {position} - Download: {downloadSize}MB, Timer: {currentLevel.baseTimeLimit}s");
        OnDeviceSpawned?.Invoke(newDevice);
    }
    
    private void OnDeviceWasDestroyed(GameObject destroyedDevice)
    {
        int beforeRemove = activeDevices.Count;
        
        if (destroyedDevice != null)
        {
            worldBounds.FreeGridCell(destroyedDevice.transform.position);
        }
        
        bool removed = activeDevices.Remove(destroyedDevice);
        int afterRemove = activeDevices.Count;
        
        Debug.Log($"Device destruído: {(removed ? "REMOVIDO" : "NÃO ENCONTRADO")} da lista. Devices: {beforeRemove} -> {afterRemove}");
        
        OnDeviceDestroyed?.Invoke(destroyedDevice);
    }
    
    private void OnDeviceWasCompleted(Device completedDevice)
    {
        // Notifica o ScoreAndCoinsManager para dar recompensa
        if (ScoreAndCoinsManager.Instance != null)
        {
            ScoreAndCoinsManager.Instance.OnDeviceCompleted(completedDevice);
        }
    }
    
    public void DestroyAllDevices()
    {
        foreach (GameObject device in activeDevices)
        {
            if (device != null)
            {
                Device deviceComponent = device.GetComponent<Device>();
                if (deviceComponent != null)
                {
                    deviceComponent.ForceDestroy();
                }
                else
                {
                    Destroy(device);
                }
            }
        }
        activeDevices.Clear();
    }
    
    public int GetActiveDeviceCount()
    {
        int beforeCleanup = activeDevices.Count;
        activeDevices.RemoveAll(device => device == null);
        int afterCleanup = activeDevices.Count;
        
        if (beforeCleanup != afterCleanup)
        {
            Debug.Log($"Cleanup devices: {beforeCleanup} -> {afterCleanup} (removidos {beforeCleanup - afterCleanup} nulls)");
        }
        
        return activeDevices.Count;
    }
    
    /// <summary>
    /// Retorna estatísticas de spawn para debug
    /// </summary>
    public string GetSpawnStats()
    {
        return $"Devices ativos: {GetActiveDeviceCount()}\nCélulas livres: {worldBounds?.GetFreeCellsCount() ?? 0}";
    }
    
    public int GetBonusPointsForDevice(GameObject device)
    {
        return currentLevel?.basePointsReward ?? 10;
    }
    
    public int GetCoinRewardForDevice(GameObject device)
    {
        return currentLevel?.baseCoinReward ?? 8;
    }
    
    /// <summary>
    /// Retorna o level atual baseado no score
    /// </summary>
    public SpawnLevel GetCurrentLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// Retorna o limite máximo de devices para o level atual
    /// </summary>
    public int GetMaxDevicesForCurrentLevel()
    {
        return currentLevel?.maxSimultaneousDevices ?? 2;
    }
    
    /// <summary>
    /// Retorna informações do level atual para UI
    /// </summary>
    public string GetCurrentLevelInfo()
    {
        if (currentLevel == null) return "Nenhum level ativo";
        
        int currentScore = ScoreAndCoinsManager.Instance?.GetCurrentScore() ?? 0;
        int activeDevices = GetActiveDeviceCount();
        return $"Score: {currentScore}/{currentLevel.maxScore} - Devices: {activeDevices}/{currentLevel.maxSimultaneousDevices}";
    }
    
    void OnDrawGizmosSelected()
    {
        if (currentLevel != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
} 