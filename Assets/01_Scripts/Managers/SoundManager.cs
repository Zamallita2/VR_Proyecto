using UnityEngine;

/// <summary>
/// Singleton centralizado de audio. Asigna todos tus AudioClips aquí
/// en el Inspector y los demás scripts los pedirán mediante métodos estáticos.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    // ── Fuentes de audio ──────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // ── Música ────────────────────────────────────────────────────
    [Header("Música de Fondo")]
    [SerializeField] private AudioClip backgroundMusic;

    // ── Gato ──────────────────────────────────────────────────────
    [Header("Sonidos del Gato")]
    [SerializeField] public AudioClip catMeow;
    [SerializeField] public AudioClip catEat;
    [SerializeField] public AudioClip catPlay;
    [SerializeField] public AudioClip catComplain;
    [SerializeField] public AudioClip catWalk;

    // ── Jugador ───────────────────────────────────────────────────
    [Header("Sonidos del Jugador")]
    [SerializeField] public AudioClip playerWalk;
    [SerializeField] public AudioClip playerGrab;
    [SerializeField] public AudioClip playerDrop;
    [SerializeField] public AudioClip playerPlace;
    [SerializeField] public AudioClip playerShovel;
    [SerializeField] public AudioClip playerTrash;
    [SerializeField] public AudioClip playerFeedFood;
    [SerializeField] public AudioClip playerFeedSand;

    // ── Tienda / Objetos ──────────────────────────────────────────
    [Header("Sonidos de Tienda y Objetos")]
    [SerializeField] public AudioClip shopBuy;
    [SerializeField] public AudioClip shopCancel;
    [SerializeField] public AudioClip lootBoxOpen;
    [SerializeField] public AudioClip lootBoxItemPop;
    [SerializeField] public AudioClip notificationDonation;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    /// <summary>Reproduce un AudioClip de efecto de sonido (no en bucle).</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>Reproduce un AudioClip en una posición 3D del mundo.</summary>
    public void PlaySFXAt(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position);
    }

    /// <summary>
    /// Controla un AudioSource en bucle (para pasos). 
    /// Pasa loop=true para activar y loop=false para detener.
    /// </summary>
    public void SetLoopingSFX(AudioSource source, AudioClip clip, bool playing)
    {
        if (source == null || clip == null) return;

        if (playing && !source.isPlaying)
        {
            source.clip = clip;
            source.loop = true;
            source.Play();
        }
        else if (!playing && source.isPlaying)
        {
            source.Stop();
        }
    }
}
