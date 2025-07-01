using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SFXManager facilita tocar efeitos sonoros pontuais a partir de qualquer script.
/// 1) Coloque este script em um GameObject na cena inicial (ex.: "SFXManager").
/// 2) Cadastre no Inspector a lista de clipes (par chave/AudioClip).
/// 3) Chame <c>SFXManager.Play("nome")</c> de qualquer lugar.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    [System.Serializable]
    public struct NamedClip
    {
        public string key;     // Ex.: "click", "success", "error"
        public AudioClip clip; // Arquivo de áudio correspondente
    }

    [Header("Clipes Registrados")]
    [SerializeField] private List<NamedClip> clips = new();

    [Header("Configurações")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 1f;

    private static SFXManager instance;
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> clipDict;

    private void Awake()
    {
        // Singleton simples
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Constrói dicionário para acesso rápido
        clipDict = new Dictionary<string, AudioClip>();
        foreach (var nc in clips)
        {
            if (!string.IsNullOrEmpty(nc.key) && nc.clip != null)
            {
                clipDict[nc.key] = nc.clip;
            }
        }
    }

    /// <summary>
    /// Toca o efeito sonoro registrado pela chave.
    /// </summary>
    public static void Play(string key, float volume = -1f)
    {
        if (instance == null)
        {
            Debug.LogWarning("[SFXManager] Nenhum instance na cena.");
            return;
        }
        instance.PlayInternal(key, volume);
    }

    /// <summary>
    /// Toca diretamente um AudioClip sem registro prévio.
    /// </summary>
    public static void PlayClip(AudioClip clip, float volume = -1f)
    {
        if (instance == null || clip == null) return;
        instance.audioSource.PlayOneShot(clip, volume >= 0f ? volume : instance.defaultVolume);
    }

    private void PlayInternal(string key, float volume)
    {
        if (clipDict.TryGetValue(key, out var clip))
        {
            audioSource.PlayOneShot(clip, volume >= 0f ? volume : defaultVolume);
        }
        else
        {
            Debug.LogWarning($"[SFXManager] Clip com chave '{key}' não encontrado.");
        }
    }
} 