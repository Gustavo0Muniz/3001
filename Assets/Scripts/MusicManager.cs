using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    [Header("M�sicas")]
    public AudioClip musicHorde1;
    public AudioClip musicPeriferiaCombat; 
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
            ApplySavedVolume(); 
            audioSource.Play();
            Debug.Log("Tocando m�sica: " + clipToPlay.name);
        }
        else
        {
            audioSource.Stop();
            Debug.LogWarning("Tentativa de tocar m�sica nula. M�sica parada.");
        }
    }


    public void PlayMusicHorde1()
    {
        PlayMusic(musicHorde1);
    }

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
        Debug.Log("M�sica parada.");
    }
}
