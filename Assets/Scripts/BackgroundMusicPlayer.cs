using UnityEngine;

/// <summary>
/// Toca música de fundo em loop e persiste entre mudanças de cena.
/// Basta colocar este script em um GameObject na cena inicial
/// e atribuir o AudioClip desejado pelo Inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicPlayer : MonoBehaviour
{
    private static BackgroundMusicPlayer instance;

    [Header("Música")]
    [SerializeField] private AudioClip backgroundClip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Garante singleton (apenas um player na vida inteira do app)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        ConfigureAndPlay();
    }

    private void ConfigureAndPlay()
    {
        if (backgroundClip == null)
        {
            Debug.LogWarning("[BackgroundMusicPlayer] Nenhum AudioClip atribuído!");
            return;
        }

        audioSource.clip = backgroundClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }

    /// <summary>
    /// Altera a música em runtime (opcional).
    /// </summary>
    public void ChangeMusic(AudioClip newClip, float newVolume = -1f)
    {
        if (newClip == null) return;

        audioSource.Stop();
        backgroundClip = newClip;
        if (newVolume >= 0f) volume = Mathf.Clamp01(newVolume);
        ConfigureAndPlay();
    }
} 