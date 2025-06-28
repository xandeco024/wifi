using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Versão 3D do Modem - usa ClickableObject3D para mundo 3D
/// Mantém a mesma lógica de eventos do Modem original
/// </summary>
public class Modem3D : MonoBehaviour
{
    [Header("Modem Settings")]
    [SerializeField] private bool isConnectionActive = false;
    
    [Header("3D Settings")]
    [SerializeField] private bool requireCollider = true;
    
    private ClickableObject3D clickableComponent;
    
    void Awake()
    {
        // Garante que tem o componente ClickableObject3D
        clickableComponent = GetComponent<ClickableObject3D>();
        if (clickableComponent == null)
        {
            clickableComponent = gameObject.AddComponent<ClickableObject3D>();
        }
        
        // Verifica se tem collider para interação 3D
        if (requireCollider && GetComponent<Collider>() == null)
        {
            // Adiciona BoxCollider padrão se não tiver nenhum
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log($"[Modem3D] BoxCollider adicionado automaticamente a {gameObject.name}");
        }
    }
    
    void Start()
    {
        // Configura o ClickableObject3D para o modem (com drag habilitado)
        clickableComponent.SetInteractionEnabled(click: true, drag: true, hover: true);
        
        // Inscreve nos eventos do ClickableObject3D
        clickableComponent.OnThisObjectClick += OnModemClicked;
        clickableComponent.OnThisObjectHoverEnter += OnModemHoverEnter;
        clickableComponent.OnThisObjectHoverExit += OnModemHoverExit;
        clickableComponent.OnThisObjectDragStart += OnModemDragStart;
        clickableComponent.OnThisObjectDragEnd += OnModemDragEnd;
        
        // Inscreve nos eventos do CableController (se disponível)
        if (CableController.Instance != null)
        {
            CableController.Instance.OnCableConnected += OnCableSuccessfullyConnected;
            CableController.Instance.OnCableDragCanceled += OnCableDragCanceled;
        }
        
        Debug.Log($"[Modem3D] Modem 3D inicializado na posição {transform.position}");
    }
    
    void OnDestroy()
    {
        // Limpa os eventos quando o objeto é destruído
        if (clickableComponent != null)
        {
            clickableComponent.OnThisObjectClick -= OnModemClicked;
            clickableComponent.OnThisObjectHoverEnter -= OnModemHoverEnter;
            clickableComponent.OnThisObjectHoverExit -= OnModemHoverExit;
            clickableComponent.OnThisObjectDragStart -= OnModemDragStart;
            clickableComponent.OnThisObjectDragEnd -= OnModemDragEnd;
        }
        
        // Limpa eventos do CableController
        if (CableController.Instance != null)
        {
            CableController.Instance.OnCableConnected -= OnCableSuccessfullyConnected;
            CableController.Instance.OnCableDragCanceled -= OnCableDragCanceled;
        }
    }
    
    #region Event Handlers
    
    private void OnModemClicked(ClickableObject3D clickedObject)
    {
        Debug.Log("[Modem3D] Iniciando conexão por cabo...");
        StartCableConnection();
    }
    
    private void OnModemHoverEnter(ClickableObject3D hoveredObject)
    {
        Debug.Log("[Modem3D] Mouse sobre o modem - pronto para conectar");
        // Aqui pode adicionar feedback visual como highlight, scale, etc.
    }
    
    private void OnModemHoverExit(ClickableObject3D hoveredObject)
    {
        Debug.Log("[Modem3D] Mouse saiu do modem");
        // Aqui pode remover o feedback visual
    }
    
    private void OnModemDragStart(ClickableObject3D draggedObject)
    {
        Debug.Log("[Modem3D] Iniciando drag do cabo...");
        
        // Inicia o sistema de cabos
        if (CableController.Instance != null)
        {
            CableController.Instance.StartCableDrag();
        }
        else
        {
            Debug.LogWarning("[Modem3D] CableController não encontrado!");
        }
    }
    
    private void OnModemDragEnd(ClickableObject3D draggedObject)
    {
        Debug.Log("[Modem3D] Finalizando drag do cabo");
        
        // Finaliza o sistema de cabos
        if (CableController.Instance != null)
        {
            CableController.Instance.EndCableDrag();
        }
    }
    
    private void OnCableSuccessfullyConnected(ComputerBehavior connectedPC)
    {
        Debug.Log($"[Modem3D] Cabo conectado com sucesso ao PC {connectedPC.name}");
        
        // Finaliza a conexão do modem
        EndCableConnection();
    }
    
    private void OnCableDragCanceled()
    {
        Debug.Log("[Modem3D] Drag do cabo cancelado");
        
        // Finaliza a conexão do modem sem sucesso
        EndCableConnection();
    }
    
    #endregion
    
    #region Cable Connection Logic
    
    private void StartCableConnection()
    {
        if (isConnectionActive)
        {
            Debug.LogWarning("[Modem3D] Conexão já está ativa!");
            return;
        }
        
        isConnectionActive = true;
        
        // Integração com CableController
        Debug.Log("[Modem3D] Conexão iniciada - arraste para criar cabo!");
        
        // Notifica outros sistemas que uma conexão foi iniciada
        OnConnectionStarted?.Invoke();
    }
    
    public void EndCableConnection()
    {
        isConnectionActive = false;
        Debug.Log("[Modem3D] Conexão por cabo finalizada");
        
        // Notifica outros sistemas que a conexão terminou
        OnConnectionEnded?.Invoke();
    }
    
    #endregion
    
    #region Public Properties & Events
    
    /// <summary>
    /// Verifica se o modem está atualmente em modo de conexão
    /// </summary>
    public bool IsConnectionActive => isConnectionActive;
    
    /// <summary>
    /// Evento disparado quando uma conexão é iniciada
    /// </summary>
    public System.Action OnConnectionStarted;
    
    /// <summary>
    /// Evento disparado quando uma conexão é finalizada
    /// </summary>
    public System.Action OnConnectionEnded;
    
    /// <summary>
    /// Força o fim de uma conexão ativa (útil para cancelamentos)
    /// </summary>
    public void ForceEndConnection()
    {
        if (isConnectionActive)
        {
            EndCableConnection();
        }
    }
    
    /// <summary>
    /// Retorna a posição 3D do modem
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
    
    /// <summary>
    /// Configura a posição 3D do modem
    /// </summary>
    public void SetWorldPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        Debug.Log($"[Modem3D] Posição alterada para {newPosition}");
    }
    
    #endregion
    
    #region Debug & Gizmos
    
    void OnDrawGizmosSelected()
    {
        // Desenha conexão ativa
        if (isConnectionActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        
        // Desenha área de interação
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
        
        // Desenha posição central
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
    
    [ContextMenu("Test Connection")]
    private void TestConnection()
    {
        if (isConnectionActive)
        {
            EndCableConnection();
        }
        else
        {
            StartCableConnection();
        }
    }
    
    [ContextMenu("Force End Connection")]
    private void TestForceEndConnection()
    {
        ForceEndConnection();
    }
    
    [ContextMenu("Print Position")]
    private void TestPrintPosition()
    {
        Debug.Log($"[Modem3D] Posição atual: {transform.position}");
    }
    
    #endregion
} 