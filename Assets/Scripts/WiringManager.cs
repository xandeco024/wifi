using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WiringManager : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float wireThickness = 5f; // espessura do cabo em pixels
    [SerializeField] private Color wireColor = Color.white;

    [Header("Controle de Spawn")]
    [SerializeField] private bool enableWireSpawn = true; // Se falso, cabos não serão criados

    private List<UIClickDrag> draggablePoints = new List<UIClickDrag>();

    [Header("Terminais (atribuir manualmente)")]
    [SerializeField] private List<RectTransform> terminals = new List<RectTransform>();

    private Image activeWire;
    private UIClickDrag activeDragPoint;

    private readonly List<WireConnection> connections = new();

    private int wireCounter = 0;

    // Salva transformações originais dos elementos que se tornam cabos temporariamente
    private struct OriginalState
    {
        public Vector2 anchoredPos;
        public Vector2 size;
        public Quaternion rotation;
        public Vector2 pivot;
        public Vector3 scale;

        public OriginalState(RectTransform rt)
        {
            anchoredPos = rt.anchoredPosition;
            size = rt.sizeDelta;
            rotation = rt.localRotation;
            pivot = rt.pivot;
            scale = rt.localScale;
        }
    }

    private readonly Dictionary<Image, OriginalState> originalStates = new();

    private struct WireConnection
    {
        public Image wire;
        public UIClickDrag source;
        public RectTransform terminal;
    }

    public enum CableVariation
    {
        StraightThrough,
        Crossover
    }

    [Header("Variação do Cabo")]
    [SerializeField] private CableVariation variation = CableVariation.StraightThrough;

    [Header("Pinos (preenchido automaticamente)")]
    [SerializeField] private List<UIClickDrag> pinsOrdered = new List<UIClickDrag>();

    public System.Action<bool> OnMinigameCompleted;

    public static WiringManager Instance { get; private set; }

    private GameObject panelRoot;
    private System.Action<bool> externalCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this; else { Debug.LogWarning("Mais de um WiringManager na cena"); }

        panelRoot = transform.parent != null ? transform.parent.gameObject : gameObject;

        // Se o objeto detectado possuir Canvas, significa que o script está diretamente sob o Canvas;
        // nesse caso o próprio painel é este GameObject (para não desligar Canvas inteiro).
        if (panelRoot.GetComponent<Canvas>() != null)
        {
            panelRoot = gameObject;
        }

        // começa desativado
        if (panelRoot.activeSelf) panelRoot.SetActive(false);

        // Registra automaticamente todos os UIClickDrag que sejam filhos deste manager
        foreach (var point in GetComponentsInChildren<UIClickDrag>(true))
        {
            RegisterDraggablePoint(point);
        }

        // Preenche pinsOrdered automaticamente se vazio ou com quantidade diferente
        if (pinsOrdered.Count != draggablePoints.Count)
        {
            pinsOrdered.Clear();

            // Tenta ordenar por pinNumber se configurado corretamente 1-8
            bool hasPinNumbers = true;
            foreach (var p in draggablePoints)
            {
                if (p.pinNumber < 1 || p.pinNumber > 8) { hasPinNumbers = false; break; }
            }

            if (hasPinNumbers)
            {
                pinsOrdered.AddRange(draggablePoints);
                pinsOrdered.Sort((a,b) => a.pinNumber.CompareTo(b.pinNumber));
            }
            else
            {
                // fallback posição X
                pinsOrdered.AddRange(draggablePoints);
                pinsOrdered.Sort((a,b) => a.transform.position.x.CompareTo(b.transform.position.x));
            }
        }
    }

    private void Update()
    {
        // Monitora todos os pontos draggables
        foreach (var point in draggablePoints)
        {
            // Se encontrar um ponto sendo arrastado
            if (point.IsDragging)
            {
                // Se não tiver cabo ativo, cria um
                if (activeWire == null && enableWireSpawn)
                {
                    CreateWire(point);
                }
                else if (activeWire == null && !enableWireSpawn)
                {
                    CreateExtendedWire(point);
                }
                
                // Atualiza o visual do cabo em direção ao ponteiro
                UpdateWireVisual(Input.mousePosition);
                break;
            }
        }

        // Se o arraste terminou e havia um cabo ativo, verifica conexão
        if (activeWire != null && activeDragPoint != null && !activeDragPoint.IsDragging)
        {
            RectTransform hitTerminal = GetHoveredTerminal(Input.mousePosition);
            if (hitTerminal != null)
            {
                FinalizeWire(hitTerminal);
            }
            else
            {
                CancelActiveWire();
            }

            activeWire = null;
            activeDragPoint = null;
        }
    }

    private void CreateWire(UIClickDrag dragPoint)
    {
        // Cria um GameObject com Image
        GameObject wireObj = new GameObject($"Wire_{++wireCounter}", typeof(RectTransform));
        // Garante que o cabo fique dentro da mesma hierarquia UI
        wireObj.transform.SetParent(transform, false);

        // Configura a Image usando o sprite branco embutido do Unity (não depende de assets)
        activeWire = wireObj.AddComponent<Image>();
        activeWire.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        // Cor herdada do ponto de origem, se houver Image
        var sourceImg = dragPoint.GetComponent<Image>();
        activeWire.color = sourceImg != null ? sourceImg.color : wireColor;

        // Torna o cabo clicável para futura remoção
        WireClickable clickable = wireObj.AddComponent<WireClickable>();
        clickable.Setup(this, activeWire);

        // Ajusta o pivot para o centro para facilitar cálculo
        RectTransform rect = activeWire.rectTransform;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1, wireThickness);

        activeDragPoint = dragPoint;
    }

    private void CreateExtendedWire(UIClickDrag dragPoint)
    {
        // Usa a própria imagem do ponto como fio
        Image img = dragPoint.GetComponent<Image>();
        if (img == null)
        {
            img = dragPoint.gameObject.AddComponent<Image>();
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        // Salva estado original (apenas na primeira vez)
        if (!originalStates.ContainsKey(img))
        {
            originalStates[img] = new OriginalState(img.rectTransform);
        }

        // Ajusta visuais para cabo
        img.raycastTarget = false; // Evita cliques enquanto arrastando
        img.color = img.color; // mantém cor

        RectTransform rect = img.rectTransform;
        rect.pivot = new Vector2(0f, 0.5f); // origem na extremidade esquerda
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, wireThickness);
        rect.localScale = Vector3.one;

        // Adiciona WireClickable caso não exista
        WireClickable clickable = dragPoint.GetComponent<WireClickable>();
        if (clickable == null)
        {
            clickable = dragPoint.gameObject.AddComponent<WireClickable>();
            clickable.Setup(this, img);
        }

        activeWire = img;
        activeDragPoint = dragPoint;
    }

    private void UpdateWireVisual(Vector2 screenEndPos)
    {
        if (activeWire == null) return;

        RectTransform wireRect = activeWire.rectTransform;
        RectTransform parentRect = wireRect.parent as RectTransform;

        Vector2 endLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenEndPos, null, out endLocalPos);

        // Pega a posição do ponto de origem em espaço local do pai do fio
        Vector2 startPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect,
            activeDragPoint.transform.position, null, out startPos);

        // Calcula a direção e distância
        Vector2 direction = endLocalPos - startPos;
        float distance = direction.magnitude;
        if (distance < 0.1f) distance = 0.1f; // evita zero

        // Rotaciona para apontar para o mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        wireRect.localRotation = Quaternion.Euler(0, 0, angle);

        bool isExtended = originalStates.ContainsKey(activeWire);

        if (isExtended)
        {
            // Escala o fio (e consequentemente os filhos)
            float originalWidth = originalStates[activeWire].size.x;
            if (originalWidth <= 0.0001f) originalWidth = 1f;

            float scaleFactor = distance / originalWidth;

            wireRect.sizeDelta = new Vector2(originalWidth, wireThickness);
            wireRect.localScale = new Vector3(scaleFactor, 1f, 1);
        }
        else
        {
            wireRect.sizeDelta = new Vector2(distance, wireRect.sizeDelta.y);
            wireRect.localScale = Vector3.one;
        }

        if (isExtended)
        {
            // Mantém origem fixa na posição original salva
            var st = originalStates[activeWire];
            wireRect.anchoredPosition = st.anchoredPos;
            wireRect.pivot = new Vector2(0f, 0.5f);
        }
        else
        {
            // Posiciona o cabo instanciado no ponto médio
            wireRect.anchoredPosition = startPos + direction * 0.5f;
            wireRect.pivot = new Vector2(0.5f, 0.5f);
        }
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

    private RectTransform GetHoveredTerminal(Vector2 screenPos)
    {
        foreach (var term in terminals)
        {
            if (term == null) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(term, screenPos, null))
            {
                return term;
            }
        }
        return null;
    }

    private void FinalizeWire(RectTransform terminal)
    {
        // Ajusta visual para terminar exatamente no terminal
        Vector2 terminalScreenPos = RectTransformUtility.WorldToScreenPoint(null, terminal.position);
        UpdateWireVisual(terminalScreenPos);

        // Certifica-se de que o clique continue funcional após finalizar
        activeWire.raycastTarget = true;

        // Salva conexão
        connections.Add(new WireConnection
        {
            wire = activeWire,
            source = activeDragPoint,
            terminal = terminal
        });

        Debug.Log($"Cabo {activeWire.gameObject.name} ligado no terminal {terminal.gameObject.name}");
    }

    private void CancelActiveWire()
    {
        if (activeWire == null) return;

        // Se o fio é instanciado (spawn) simplesmente destruir; caso contrário restaurar
        if (originalStates.TryGetValue(activeWire, out var st))
        {
            RestoreImage(activeWire, st);
            originalStates.Remove(activeWire);
        }
        else
        {
            Destroy(activeWire.gameObject);
        }
    }

    private void RestoreImage(Image img, OriginalState st)
    {
        RectTransform rt = img.rectTransform;
        rt.anchoredPosition = st.anchoredPos;
        rt.sizeDelta = st.size;
        rt.localRotation = st.rotation;
        rt.pivot = st.pivot;
        rt.localScale = st.scale;

        img.raycastTarget = true;
    }

    // Chamado por WireClickable
    public void RemoveWire(Image wire)
    {
        // Encontra conexão correspondente
        int index = connections.FindIndex(c => c.wire == wire);
        if (index >= 0)
        {
            var conn = connections[index];
            connections.RemoveAt(index);
            if (originalStates.TryGetValue(conn.wire, out var st))
            {
                RestoreImage(conn.wire, st);
                originalStates.Remove(conn.wire);
            }
            else
            {
                Destroy(conn.wire.gameObject);
            }

            Debug.Log($"Cabo {conn.wire.gameObject.name} removido");
        }
    }

    // --------- Validação ---------

    public bool ValidateConnections()
    {
        // Garante que todas as listas estejam com 8 elementos
        if (pinsOrdered.Count < 8 || terminals.Count < 8) {
            Debug.LogWarning("Listas de pinos ou terminais incompletas.");
            return false;
        }

        // Mapeia pino -> terminal conectado
        int[] mapping = new int[9]; // ignorar índice 0

        foreach (var conn in connections)
        {
            int pinIndex = conn.source != null ? conn.source.pinNumber : -1;
            int termIndex = -1;

            if (conn.terminal != null)
            {
                var tPin = conn.terminal.GetComponent<TerminalPin>();
                if (tPin != null)
                {
                    termIndex = tPin.pinNumber;
                }
            }

            // se ainda não achou, usa posição na lista
            if (termIndex <= 0)
            {
                termIndex = terminals.IndexOf(conn.terminal) + 1;
            }

            if (pinIndex <= 0 || termIndex <= 0) continue;
            mapping[pinIndex] = termIndex;
        }

        // Precisa ter todos preenchidos
        for (int i = 1; i <= 8; i++)
        {
            if (mapping[i] == 0) return false; // falta ligação
        }

        // Define expectativa
        int[] expected = GetExpectedMapping();

        for (int i = 1; i <= 8; i++)
        {
            if (mapping[i] != expected[i])
            {
                Debug.Log($"Falha no pino {i}: ligado ao terminal {mapping[i]} (esperado {expected[i]})");
                return false;
            }
        }

        Debug.Log("Todas as ligações conferem com a variação " + variation);
        return true;
    }

    private int[] GetExpectedMapping()
    {
        // índice 0 inutilizado
        int[] straight = {0,1,2,3,4,5,6,7,8};
        int[] crossover = {0,3,6,1,4,5,2,7,8};

        return variation == CableVariation.StraightThrough ? straight : crossover;
    }

    public void SetVariation(CableVariation v)
    {
        variation = v;
    }

    // Método exposto para UI Button (sem parâmetros)
    public void CheckConnectionsButton()
    {
        bool ok = ValidateConnections();
        Debug.Log(ok ? "Ligação correta!" : "Ligação incorreta.");
        OnMinigameCompleted?.Invoke(ok);
        FinishMinigame(ok);
    }

    // ---------- Abertura / Fechamento ----------
    public void OpenMinigame(CableVariation v, System.Action<bool> callback)
    {
        variation = v;
        externalCallback = callback;
        panelRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    private void FinishMinigame(bool success)
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
        externalCallback?.Invoke(success);
        externalCallback = null;
    }
} 