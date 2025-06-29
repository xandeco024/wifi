using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WiringManager : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private Sprite wireSprite;
    [SerializeField] private Color wireColor = Color.white;

    private List<UIClickDrag> draggablePoints = new List<UIClickDrag>();
    private Image activeWire;
    private UIClickDrag activeDragPoint;

    private void Update()
    {
        // Monitora todos os pontos draggables
        foreach (var point in draggablePoints)
        {
            // Se encontrar um ponto sendo arrastado
            if (point.IsDragging)
            {
                // Se não tiver cabo ativo, cria um
                if (activeWire == null)
                {
                    CreateWire(point);
                }
                
                // Atualiza o visual do cabo
                UpdateWireVisual();
                break;
            }
        }

        // Se nenhum ponto está sendo arrastado mas tem cabo ativo
        if (activeWire != null && activeDragPoint != null && !activeDragPoint.IsDragging)
        {
            Destroy(activeWire.gameObject);
            activeWire = null;
            activeDragPoint = null;
        }
    }

    private void CreateWire(UIClickDrag dragPoint)
    {
        // Cria um GameObject com Image
        GameObject wireObj = new GameObject("Wire");
        wireObj.transform.SetParent(transform);
        
        // Configura a Image
        activeWire = wireObj.AddComponent<Image>();
        activeWire.sprite = wireSprite;
        activeWire.color = wireColor;
        
        activeDragPoint = dragPoint;
    }

    private void UpdateWireVisual()
    {
        if (activeWire == null) return;

        // Pega a posição do mouse na tela
        Vector2 mousePos = Input.mousePosition;
        
        // Pega a posição do ponto de origem
        Vector2 startPos = activeDragPoint.transform.position;
        
        // Calcula a direção e distância
        Vector2 direction = mousePos - startPos;
        float distance = direction.magnitude;
        
        // Posiciona o cabo no ponto médio entre origem e mouse
        activeWire.transform.position = Vector2.Lerp(startPos, mousePos, 0.5f);
        
        // Rotaciona o cabo para apontar para o mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        activeWire.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Escala o cabo baseado na distância
        activeWire.transform.localScale = new Vector3(distance, 1, 1);
    }

    public void RegisterDraggablePoint(UIClickDrag point)
    {
        if (!draggablePoints.Contains(point))
        {
            draggablePoints.Add(point);
        }
    }

    public void UnregisterDraggablePoint(UIClickDrag point)
    {
        draggablePoints.Remove(point);
    }
} 