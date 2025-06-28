using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Exemplo de como outros scripts podem usar o sistema ClickableObject
/// Este script monitora TODOS os objetos clicáveis do jogo
/// </summary>
public class InteractionManager : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showGlobalInteractions = true;
    
    [Header("Statistics (Read Only)")]
    [SerializeField] private int totalClicks = 0;
    [SerializeField] private int totalDrags = 0;
    [SerializeField] private int activeObjects = 0;
    
    // Lista de todos os objetos atualmente sendo interagidos
    private List<ClickableObject> hoveredObjects = new List<ClickableObject>();
    private List<ClickableObject> draggedObjects = new List<ClickableObject>();
    
    void OnEnable()
    {
        // Inscreve nos eventos GLOBAIS do ClickableObject
        ClickableObject.OnAnyObjectClick += OnAnyObjectClicked;
        ClickableObject.OnAnyObjectHoverEnter += OnAnyObjectHoverEnter;
        ClickableObject.OnAnyObjectHoverExit += OnAnyObjectHoverExit;
        ClickableObject.OnAnyObjectDragStart += OnAnyObjectDragStart;
        ClickableObject.OnAnyObjectDragEnd += OnAnyObjectDragEnd;
        
        Debug.Log("[InteractionManager] Sistema de interação ativo e monitorando todos os objetos clicáveis");
    }
    
    void OnDisable()
    {
        // Limpa as inscrições quando o script é desabilitado
        ClickableObject.OnAnyObjectClick -= OnAnyObjectClicked;
        ClickableObject.OnAnyObjectHoverEnter -= OnAnyObjectHoverEnter;
        ClickableObject.OnAnyObjectHoverExit -= OnAnyObjectHoverExit;
        ClickableObject.OnAnyObjectDragStart -= OnAnyObjectDragStart;
        ClickableObject.OnAnyObjectDragEnd -= OnAnyObjectDragEnd;
    }
    
    #region Global Event Handlers
    
    private void OnAnyObjectClicked(ClickableObject clickedObject)
    {
        totalClicks++;
        
        if (showGlobalInteractions)
        {
            Debug.Log($"[InteractionManager] GLOBAL CLICK detectado em {clickedObject.name} (Total: {totalClicks})");
        }
        
        // Exemplo: Aqui você pode implementar lógica global de cliques
        // Como tocar sons, efeitos visuais, etc.
        HandleGlobalClick(clickedObject);
    }
    
    private void OnAnyObjectHoverEnter(ClickableObject hoveredObject)
    {
        if (!hoveredObjects.Contains(hoveredObject))
        {
            hoveredObjects.Add(hoveredObject);
        }
        
        activeObjects = hoveredObjects.Count;
        
        if (showGlobalInteractions)
        {
            Debug.Log($"[InteractionManager] HOVER ENTER em {hoveredObject.name} (Objetos ativos: {activeObjects})");
        }
    }
    
    private void OnAnyObjectHoverExit(ClickableObject hoveredObject)
    {
        if (hoveredObjects.Contains(hoveredObject))
        {
            hoveredObjects.Remove(hoveredObject);
        }
        
        activeObjects = hoveredObjects.Count;
        
        if (showGlobalInteractions)
        {
            Debug.Log($"[InteractionManager] HOVER EXIT de {hoveredObject.name} (Objetos ativos: {activeObjects})");
        }
    }
    
    private void OnAnyObjectDragStart(ClickableObject draggedObject)
    {
        totalDrags++;
        
        if (!draggedObjects.Contains(draggedObject))
        {
            draggedObjects.Add(draggedObject);
        }
        
        if (showGlobalInteractions)
        {
            Debug.Log($"[InteractionManager] DRAG STARTED em {draggedObject.name} (Total drags: {totalDrags})");
        }
        
        // Exemplo: Aqui você pode pausar outros sistemas, tocar som, etc.
        HandleGlobalDragStart(draggedObject);
    }
    
    private void OnAnyObjectDragEnd(ClickableObject draggedObject)
    {
        if (draggedObjects.Contains(draggedObject))
        {
            draggedObjects.Remove(draggedObject);
        }
        
        if (showGlobalInteractions)
        {
            Debug.Log($"[InteractionManager] DRAG ENDED em {draggedObject.name}");
        }
        
        // Exemplo: Aqui você pode retomar sistemas pausados, etc.
        HandleGlobalDragEnd(draggedObject);
    }
    
    #endregion
    
    #region Game-Specific Logic Examples
    
    private void HandleGlobalClick(ClickableObject clickedObject)
    {
        // Exemplo: Verificar se é um modem
        if (clickedObject.GetComponent<Modem>() != null)
        {
            Debug.Log("[InteractionManager] Modem clicado - preparando sistema de cabos");
            // Aqui você pode ativar o cursor de cabo, desabilitar outros clicks, etc.
        }
        
        // Exemplo: Verificar se é um PC
        var pc = clickedObject.GetComponent<ComputerBehavior>();
        if (pc != null)
        {
            Debug.Log($"[InteractionManager] PC clicado - Estado: {pc.CurrentState}, Tempo restante: {pc.TimeRemaining:F1}s");
            // Aqui você pode verificar se há cabo ativo do modem
        }
    }
    
    private void HandleGlobalDragStart(ClickableObject draggedObject)
    {
        // Exemplo: Pausar spawning de novos PCs durante drag
        // PCSpawner.Instance?.PauseSpawning();
        
        // Exemplo: Ativar visual de áreas válidas para drop
        // HighlightValidDropZones(true);
    }
    
    private void HandleGlobalDragEnd(ClickableObject draggedObject)
    {
        // Exemplo: Retomar spawning
        // PCSpawner.Instance?.ResumeSpawning();
        
        // Exemplo: Desativar visual de áreas válidas
        // HighlightValidDropZones(false);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Retorna quantos objetos estão sendo interagidos atualmente
    /// </summary>
    public int GetActiveInteractionCount()
    {
        return hoveredObjects.Count + draggedObjects.Count;
    }
    
    /// <summary>
    /// Verifica se algum objeto está sendo draggado
    /// </summary>
    public bool IsAnyObjectBeingDragged()
    {
        return draggedObjects.Count > 0;
    }
    
    /// <summary>
    /// Retorna estatísticas de uso
    /// </summary>
    public string GetUsageStats()
    {
        return $"Clicks: {totalClicks}, Drags: {totalDrags}, Active: {activeObjects}";
    }
    
    /// <summary>
    /// Reseta todas as estatísticas
    /// </summary>
    public void ResetStats()
    {
        totalClicks = 0;
        totalDrags = 0;
        hoveredObjects.Clear();
        draggedObjects.Clear();
        activeObjects = 0;
        
        Debug.Log("[InteractionManager] Estatísticas resetadas");
    }
    
    #endregion
} 