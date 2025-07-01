using UnityEngine;

[DisallowMultipleComponent]
public class TerminalPin : MonoBehaviour
{
    [Tooltip("Número do terminal (1 a 8)")]
    [Range(1,8)]
    public int pinNumber = 1;

    public RectTransform RectTransform => transform as RectTransform;
} 