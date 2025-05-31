using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    [Header("Músicas")]
    public AudioClip musicHorde1;
    public AudioClip musicPeriferiaCombat; // <<< NOVO: Música para combate na Periferia
    public AudioClip musicExploration;
    public AudioClip musicBossDefeat;
    public AudioClip musicGameComplete;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        ApplySavedVolume();
    }

    private void ApplySavedVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        audioSource.volume = savedVolume;
        // Não precisa de Debug.Log aqui toda vez
    }

    private void PlayMusic(AudioClip clipToPlay)
    {
        if (clipToPlay != null)
        {
            if (audioSource.clip == clipToPlay && audioSource.isPlaying)
            {
                return;
            }
            audioSource.clip = clipToPlay;
            ApplySavedVolume(); // Garante que o volume está correto
            audioSource.Play();
            Debug.Log("Tocando música: " + clipToPlay.name);
        }
        else
        {
            audioSource.Stop();
            Debug.LogWarning("Tentativa de tocar música nula. Música parada.");
        }
    }

    // --- Funções Públicas para chamar pelos Eventos ---

    public void PlayMusicHorde1()
    {
        PlayMusic(musicHorde1);
    }

    // <<< NOVA FUNÇÃO >>>
    public void PlayMusicPeriferiaCombat()
    {
        PlayMusic(musicPeriferiaCombat);
    }

    public void PlayMusicExploration()
    {
        PlayMusic(musicExploration);
    }

    public void PlayMusicBossDefeat()
    {
        PlayMusic(musicBossDefeat);
    }

    public void PlayMusicGameComplete()
    {
        PlayMusic(musicGameComplete);
    }

    public void StopMusic()
    {
        audioSource.Stop();
        Debug.Log("Música parada.");
    }
}
