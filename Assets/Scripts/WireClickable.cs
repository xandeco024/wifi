using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WireClickable : MonoBehaviour, IPointerClickHandler
{
    private WiringManager manager;
    private Image wireImage;

    public void Setup(WiringManager manager, Image img)
    {
        this.manager = manager;
        wireImage = img;

        // Garante que a imagem possa receber eventos de clique
        if (wireImage != null)
        {
            wireImage.raycastTarget = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null && wireImage != null)
        {
            manager.RemoveWire(wireImage);
        }
    }
} 