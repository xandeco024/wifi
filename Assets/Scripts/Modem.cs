using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modem : MonoBehaviour
{
    [Header("Modem Settings")]
    [SerializeField] private bool isConnectionActive = false;
    
    private ClickableObject clickableComponent;
    
    void Awake()
    {
        // Garante que tem o componente ClickableObject
        clickableComponent = GetComponent<ClickableObject>();
        if (clickableComponent == null)
        {
            clickableComponent = gameObject.AddComponent<ClickableObject>();
        }
    }
    
    void Start()
    {
        // Configura o ClickableObject para o modem (agora com drag habilitado)
        clickableComponent.SetInteractionEnabled(click: true, drag: true, hover: true);
        
        // Inscreve nos eventos do ClickableObject
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
    
    private void OnModemClicked(ClickableObject clickedObject)
    {
        Debug.Log("[Modem] Iniciando conexão por cabo...");
        StartCableConnection();
    }
    
    private void OnModemHoverEnter(ClickableObject hoveredObject)
    {
        Debug.Log("[Modem] Mouse sobre o modem - pronto para conectar");
        // Aqui pode adicionar feedback visual como highlight
    }
    
    private void OnModemHoverExit(ClickableObject hoveredObject)
    {
        Debug.Log("[Modem] Mouse saiu do modem");
        // Aqui pode remover o feedback visual
    }
    
    private void OnModemDragStart(ClickableObject draggedObject)
    {
        Debug.Log("[Modem] Iniciando drag do cabo...");
        
        // Inicia o sistema de cabos
        if (CableController.Instance != null)
        {
            CableController.Instance.StartCableDrag();
        }
        else
        {
            Debug.LogWarning("[Modem] CableController não encontrado!");
        }
    }
    
    private void OnModemDragEnd(ClickableObject draggedObject)
    {
        Debug.Log("[Modem] Finalizando drag do cabo");
        
        // Finaliza o sistema de cabos
        if (CableController.Instance != null)
        {
            CableController.Instance.EndCableDrag();
        }
    }
    
    private void OnCableSuccessfullyConnected(ComputerBehavior connectedPC)
    {
        Debug.Log($"[Modem] Cabo conectado com sucesso ao PC {connectedPC.name}");
        
        // Finaliza a conexão do modem
        EndCableConnection();
    }
    
    private void OnCableDragCanceled()
    {
        Debug.Log("[Modem] Drag do cabo cancelado");
        
        // Finaliza a conexão do modem sem sucesso
        EndCableConnection();
    }
    
    #endregion
    
    #region Cable Connection Logic
    
    private void StartCableConnection()
    {
        if (isConnectionActive)
        {
            Debug.LogWarning("[Modem] Conexão já está ativa!");
            return;
        }
        
        isConnectionActive = true;
        
        // Integração com CableController
        Debug.Log("[Modem] Conexão iniciada - arraste para criar cabo!");
        
        // Notifica outros sistemas que uma conexão foi iniciada
        OnConnectionStarted?.Invoke();
    }
    
    public void EndCableConnection()
    {
        isConnectionActive = false;
        Debug.Log("[Modem] Conexão por cabo finalizada");
        
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
    
    #endregion
}
