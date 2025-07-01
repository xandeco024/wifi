using UnityEngine;

/// <summary>
/// MenuManager lida com a exibição do Canvas de Créditos.
/// Vincule os métodos ShowCredits / HideCredits aos botões correspondentes no Inspector.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Canvas (ou GameObject raiz) que contém o layout de créditos")] 
    [SerializeField] private GameObject creditsCanvas;

    [Header("Configurações")]
    [SerializeField] private bool startHidden = true;

    void Awake()
    {
        if (creditsCanvas == null)
        {
            Debug.LogWarning("[MenuManager] Canvas de créditos não atribuído!");
            return;
        }

        // Garante estado inicial
        if (startHidden)
        {
            creditsCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Exibe o canvas de créditos
    /// </summary>
    public void ShowCredits()
    {
        if (creditsCanvas != null)
        {
            creditsCanvas.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta o canvas de créditos
    /// </summary>
    public void HideCredits()
    {
        if (creditsCanvas != null)
        {
            creditsCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Alterna entre mostrar/ocultar o canvas de créditos
    /// </summary>
    public void ToggleCredits()
    {
        if (creditsCanvas != null)
        {
            creditsCanvas.SetActive(!creditsCanvas.activeSelf);
        }
    }
} 