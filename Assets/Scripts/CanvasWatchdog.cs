using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class CanvasWatchdog : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("CanvasWatchdog: não há Canvas neste GameObject.");
        }
    }

    void OnEnable()
    {
        Debug.Log("[CanvasWatchdog] Canvas ATIVADO");
    }

    void OnDisable()
    {
        Debug.LogWarning($"[CanvasWatchdog] Canvas DESATIVADO por {GetCaller()}");
    }

    private string GetCaller()
    {
        // Captura  stackTrace, ignora origem interna do watchdog
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        for (int i = 2; i < st.FrameCount; i++) // pular 0 (GetCaller) e 1 (OnDisable)
        {
            var frame = st.GetFrame(i);
            var method = frame.GetMethod();
            if (method.DeclaringType == typeof(CanvasWatchdog)) continue;
            return $"{method.DeclaringType?.Name}.{method.Name} (linha {frame.GetFileLineNumber()})";
        }
        return "desconhecido";
    }
} 