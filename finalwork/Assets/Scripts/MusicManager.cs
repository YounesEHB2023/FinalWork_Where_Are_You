using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Music Clips")]
    public AudioClip menuAndLobbyMusic;
    public AudioClip prehistorieMusic;
    public AudioClip oudheidMusic;
    public AudioClip middeleeuwenMusic;

    [Header("Settings")]
    public float volume = 0.025f;
    public float fadeDuration = 1.5f;

    private AudioClip currentClip;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        AudioClip targetClip = GetMusicForScene(sceneName);

        if (targetClip == null) return;
        if (targetClip == currentClip) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeToMusic(targetClip));
    }

    AudioClip GetMusicForScene(string sceneName)
    {
        if (sceneName == "MainMenu" || sceneName == "Lobby")
            return menuAndLobbyMusic;

        if (sceneName == "MultiplayerPhrehistoricPuzzle1")
            return prehistorieMusic;

        if (sceneName == "MultiplayerOudheidPuzzle1")
            return oudheidMusic;

        if (sceneName == "MiddeleeuwenPuzzle1")
            return middeleeuwenMusic;

        return currentClip;
    }

    IEnumerator FadeToMusic(AudioClip newClip)
    {
        float startVolume = audioSource.volume;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();

        currentClip = newClip;

        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
    }
}