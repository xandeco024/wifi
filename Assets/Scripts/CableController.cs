using System.Collections.Generic;
using UnityEngine;

public class CableController : MonoBehaviour
{
    [Header("Cable Settings")]
    [SerializeField] private GameObject cablePrefab;
    [SerializeField] private float maxCableDistance = 8f;
    [SerializeField] private float connectionRadius = 1f;
    
    [Header("Cable Origin Points")]
    [SerializeField] private float frontOffsetZ = 0.8f; // Offset para cabo da frente
    [SerializeField] private float backOffsetZ = -0.8f; // Offset para cabo de trás
    
    [Header("Cable Curve")]
    [SerializeField] private float curveOffsetDistance = 2f; // Distância do ponto de curva em relação ao modem
    [SerializeField] private int curveResolution = 8; // Número de pontos na curva (mínimo 3)
    
    [Header("Cable Colors")]
    [SerializeField] private Color normalCableColor = Color.yellow;
    [SerializeField] private Color validCableColor = Color.green;
    [SerializeField] private Color invalidCableColor = Color.red;
    
    private Camera mainCamera;
    private Modem modem;
    
    private List<CableInstance> activeCables = new List<CableInstance>();
    private CableInstance currentDragCable;
    
    public static CableController Instance { get; private set; }
    
    public System.Action<Device> OnCableConnected;
    public System.Action OnCableDragStarted;
    public System.Action OnCableDragCanceled;
    
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
        
        SetupComponents();
        CreateCablePrefab();
    }
    
    void Start()
    {
        SetupModemEvents();
    }
    
    void Update()
    {
        HandleInput();
        UpdateCurrentCable();
    }
    
    private void SetupComponents()
    {
        // Camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Modem
        modem = Modem.Instance;
        if (modem == null)
        {
            modem = FindObjectOfType<Modem>();
        }
    }
    
    private void CreateCablePrefab()
    {
        if (cablePrefab == null)
        {
            // Cria prefab básico se não foi definido
            cablePrefab = new GameObject("CablePrefab");
            LineRenderer lr = cablePrefab.AddComponent<LineRenderer>();
            ConfigureLineRenderer(lr);
            cablePrefab.SetActive(false);
        }
    }
    
    private void ConfigureLineRenderer(LineRenderer lineRenderer)
    {
        // Garante mínimo de 3 pontos
        int points = Mathf.Max(3, curveResolution);
        lineRenderer.positionCount = points;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = true;
        
        // Material básico
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        lineRenderer.material.color = normalCableColor;
    }
    
    private void SetupModemEvents()
    {
        if (modem != null)
        {
            modem.OnModemClicked += OnModemClicked;
            Debug.Log("CableController conectado ao modem");
        }
        else
        {
            Debug.LogWarning("CableController: Modem não encontrado!");
        }
    }
    
    private void HandleInput()
    {
        // Cancela com clique direito ou ESC
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) && currentDragCable != null)
        {
            CancelCurrentCable();
        }
        
        // Termina drag com release do botão esquerdo
        if (Input.GetMouseButtonUp(0) && currentDragCable != null)
        {
            EndCurrentCable();
        }
    }
    
    private void UpdateCurrentCable()
    {
        if (currentDragCable == null) return;
        
        // Atualiza posição final baseada no mouse
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 endPosition = mouseWorldPos;
        
        // Determina origem do cabo baseado na direção do mouse em relação ao modem
        Vector3 modemToMouse = mouseWorldPos - modem.Position;
        Vector3 newStartPosition = GetCableOriginPosition(modemToMouse);
        
        // Atualiza posição de início do cabo
        currentDragCable.startPosition = newStartPosition;
        
        // Limita distância máxima baseada na nova posição de início
        float distance = Vector3.Distance(currentDragCable.startPosition, endPosition);
        if (distance > maxCableDistance)
        {
            Vector3 direction = (endPosition - currentDragCable.startPosition).normalized;
            endPosition = currentDragCable.startPosition + direction * maxCableDistance;
        }
        
        currentDragCable.endPosition = endPosition;
        
        // Atualiza cabo com curva suave
        UpdateCableCurve(currentDragCable);
        
        // Verifica se há device próximo e muda cor
        Device nearbyDevice = GetDeviceAtPosition(endPosition);
        if (nearbyDevice != null && CanConnectToDevice(nearbyDevice))
        {
            currentDragCable.lineRenderer.material.color = validCableColor;
            // Snap para o device
            currentDragCable.endPosition = nearbyDevice.transform.position;
            UpdateCableCurve(currentDragCable);
        }
        else if (distance >= maxCableDistance)
        {
            currentDragCable.lineRenderer.material.color = invalidCableColor;
        }
        else
        {
            currentDragCable.lineRenderer.material.color = normalCableColor;
        }
    }
    
    /// <summary>
    /// Determina a posição de origem do cabo baseado na direção do mouse e rotação do modem
    /// </summary>
    /// <param name="directionToMouse">Direção do modem para o mouse</param>
    /// <returns>Posição mundial de onde o cabo deve sair</returns>
    private Vector3 GetCableOriginPosition(Vector3 directionToMouse)
    {
        Vector3 modemPos = modem.Position;
        
        // Obtém a direção "forward" do modem baseado na sua rotação
        Vector3 modemForward = modem.transform.forward;
        
        // Projeta a direção do mouse no plano X-Z
        Vector3 mouseDirectionXZ = new Vector3(directionToMouse.x, 0, directionToMouse.z).normalized;
        
        // Calcula se o mouse está na frente ou atrás do modem
        float dotProduct = Vector3.Dot(modemForward, mouseDirectionXZ);
        
        // Se dot product > 0, mouse está na frente; se < 0, está atrás
        bool mouseInFront = dotProduct > 0;
        
        // Calcula offset baseado na rotação do modem
        Vector3 offset = modemForward * (mouseInFront ? frontOffsetZ : backOffsetZ);
        
        return modemPos + offset;
    }
    
    private void UpdateCableCurve(CableInstance cable)
    {
        Vector3 start = cable.startPosition;
        Vector3 end = cable.endPosition;
        Vector3 modemPos = modem.Position;
        
        // Determina direção da curva baseado na origem do cabo
        Vector3 startToModem = start - modemPos;
        Vector3 modemForward = modem.transform.forward;
        
        // Verifica se cabo sai da frente ou de trás
        bool cableFromFront = Vector3.Dot(startToModem, modemForward) > 0;
        
        // Ponto de controle da curva (perpendicular à direção do cabo)
        Vector3 curveDirection = cableFromFront ? modemForward : -modemForward;
        Vector3 curvePoint = modemPos + curveDirection * curveOffsetDistance;
        
        // Gera curva suave usando interpolação quadrática de Bézier
        int pointCount = cable.lineRenderer.positionCount;
        
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1); // Normaliza de 0 a 1
            Vector3 curvePosition = CalculateBezierPoint(start, curvePoint, end, t);
            cable.lineRenderer.SetPosition(i, curvePosition);
        }
    }
    
    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        // Curva quadrática de Bézier: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0; // (1-t)² * P0
        point += 2f * u * t * p1; // 2(1-t)t * P1
        point += tt * p2; // t² * P2
        
        return point;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;
        
        // Raycast do mouse para o plano Y=0 (plano do grid)
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        // Fallback para método anterior se raycast falhar
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.y = 0; // Força Y=0 para manter no plano do grid
        
        return worldPos;
    }
    

    
    private Device GetDeviceAtPosition(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, connectionRadius);
        
        foreach (Collider col in colliders)
        {
            Device device = col.GetComponent<Device>();
            if (device != null)
            {
                return device;
            }
        }
        
        return null;
    }
    
    private bool CanConnectToDevice(Device device)
    {
        return device != null && 
               device.CurrentState == Device.DeviceState.Disconnected &&
               device.TimeRemaining > 0;
    }
    
    private void OnModemClicked(Modem clickedModem)
    {
        if (currentDragCable == null)
        {
            // Verifica limite de cabos simultâneos
            if (GetConnectedCablesCount() >= clickedModem.MaxSimultaneousCables)
            {
                Debug.Log($"Limite de cabos atingido! Máximo: {clickedModem.MaxSimultaneousCables}");
                return;
            }
            
            StartNewCable(clickedModem.Position);
        }
    }
    
    private void StartNewCable(Vector3 modemPos)
    {
        // Posição inicial será atualizada dinamicamente no Update
        Vector3 startPosition = modemPos;
        
        // Cria nova instância do cabo
        GameObject cableObj = Instantiate(cablePrefab, transform);
        cableObj.SetActive(true);
        
        CableInstance newCable = new CableInstance
        {
            gameObject = cableObj,
            lineRenderer = cableObj.GetComponent<LineRenderer>(),
            startPosition = startPosition,
            endPosition = startPosition,
            isConnected = false
        };
        
        // Configura LineRenderer
        ConfigureLineRenderer(newCable.lineRenderer);
        
        activeCables.Add(newCable);
        currentDragCable = newCable;
        
        Debug.Log("Cabo iniciado - origem dinâmica baseada na posição do mouse");
        OnCableDragStarted?.Invoke();
    }
    
    private void EndCurrentCable()
    {
        if (currentDragCable == null) return;
        
        // Verifica se pode conectar
        Device targetDevice = GetDeviceAtPosition(currentDragCable.endPosition);
        
        if (targetDevice != null && CanConnectToDevice(targetDevice))
        {
            ConnectCableToDevice(currentDragCable, targetDevice);
        }
        else
        {
            CancelCurrentCable();
        }
    }
    
    private void CancelCurrentCable()
    {
        if (currentDragCable == null) return;
        
        activeCables.Remove(currentDragCable);
        Destroy(currentDragCable.gameObject);
        currentDragCable = null;
        
        Debug.Log("Cabo cancelado");
        OnCableDragCanceled?.Invoke();
    }
    
    private void ConnectCableToDevice(CableInstance cable, Device device)
    {
        cable.isConnected = true;
        cable.connectedDevice = device;
        cable.endPosition = device.transform.position;
        
        // Atualiza visual final
        cable.lineRenderer.material.color = validCableColor;
        UpdateCableCurve(cable);
        
        // Escuta evento de destruição do device para remover cabo no momento certo
        device.OnDeviceDestroyed += OnConnectedDeviceDestroyed;
        
        // Conecta o device
        device.ConnectDevice();
        
        Debug.Log($"Cabo conectado ao device: {device.name}");
        OnCableConnected?.Invoke(device);
        
        currentDragCable = null;
    }
    
    private void OnConnectedDeviceDestroyed(GameObject destroyedDevice)
    {
        // Encontra o cabo conectado a este device
        CableInstance cableToRemove = null;
        foreach (CableInstance cable in activeCables)
        {
            if (cable.isConnected && cable.connectedDevice != null && 
                cable.connectedDevice.gameObject == destroyedDevice)
            {
                cableToRemove = cable;
                break;
            }
        }
        
        // Remove o cabo imediatamente
        if (cableToRemove != null)
        {
            activeCables.Remove(cableToRemove);
            Destroy(cableToRemove.gameObject);
            Debug.Log("Cabo removido junto com device destruído");
        }
    }
    
    private System.Collections.IEnumerator RemoveCableAfterDelay(CableInstance cable, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeCables.Contains(cable))
        {
            activeCables.Remove(cable);
            Destroy(cable.gameObject);
        }
    }
    
    public void DestroyAllCables()
    {
        foreach (CableInstance cable in activeCables)
        {
            if (cable.gameObject != null)
            {
                Destroy(cable.gameObject);
            }
        }
        
        activeCables.Clear();
        currentDragCable = null;
        Debug.Log("Todos os cabos destruídos");
    }
    
    public int GetConnectedCablesCount()
    {
        int count = 0;
        foreach (CableInstance cable in activeCables)
        {
            if (cable.isConnected && cable.connectedDevice != null)
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetMaxSimultaneousCables()
    {
        return modem != null ? modem.MaxSimultaneousCables : 2;
    }
    
    void OnDestroy()
    {
        if (modem != null)
        {
            modem.OnModemClicked -= OnModemClicked;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (modem == null) return;
        
        // Distância máxima do cabo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(modem.Position, maxCableDistance);
        
        // Pontos de saída dos cabos baseados na rotação do modem
        Gizmos.color = Color.cyan;
        Vector3 modemForward = modem.transform.forward;
        Vector3 frontOffset = modem.Position + modemForward * frontOffsetZ;
        Vector3 backOffset = modem.Position + modemForward * backOffsetZ;
        Gizmos.DrawWireSphere(frontOffset, 0.2f);
        Gizmos.DrawWireSphere(backOffset, 0.2f);
        
        // Direção do modem (forward)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(modem.Position, modemForward * 2f);
        
        // Pontos de controle da curva (frente e trás)
        Gizmos.color = Color.magenta;
        Vector3 curvePointFront = modem.Position + modemForward * curveOffsetDistance;
        Vector3 curvePointBack = modem.Position - modemForward * curveOffsetDistance;
        Gizmos.DrawWireSphere(curvePointFront, 0.3f);
        Gizmos.DrawWireSphere(curvePointBack, 0.3f);
        
        // Raio de conexão
        if (currentDragCable != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentDragCable.endPosition, connectionRadius);
        }
    }
    
    [System.Serializable]
    public class CableInstance
    {
        public GameObject gameObject;
        public LineRenderer lineRenderer;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public bool isConnected;
        public Device connectedDevice;
    }
} 