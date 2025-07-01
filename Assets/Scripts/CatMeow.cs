using UnityEngine;

[RequireComponent(typeof(ClickableObject))]
public class CatMeow : MonoBehaviour
{
    [SerializeField] private string sfxName = "meow";

    private ClickableObject clickable;

    void Awake()
    {
        clickable = GetComponent<ClickableObject>();
        if (clickable != null)
        {
            clickable.OnThisObjectClick += HandleClick;
        }
        else
        {
            Debug.LogError("CatMeow: ClickableObject não encontrado.");
        }
    }

    private void HandleClick(ClickableObject obj)
    {
        SFXManager.Play(sfxName);
    }

    void OnDestroy()
    {
        if (clickable != null)
        {
            clickable.OnThisObjectClick -= HandleClick;
        }
    }
} 