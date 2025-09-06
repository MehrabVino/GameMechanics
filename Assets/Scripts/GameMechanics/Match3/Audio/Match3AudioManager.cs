using UnityEngine;
using System.Collections.Generic;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Manages audio for Match-3 game, including music and sound effects.
    /// Uses ScriptableObject for configuration and AudioSource pooling for performance.
    /// </summary>
    public sealed class Match3AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSettingsConfig audioConfig;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private bool playMusicOnStart = true;

        private readonly List<AudioSource> audioPool = new List<AudioSource>();
        private readonly List<AudioSource> activeSources = new List<AudioSource>();
        private static Match3AudioManager instance;

        public static Match3AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<Match3AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject(nameof(Match3AudioManager));
                        instance = go.AddComponent<Match3AudioManager>();
                    }
                }
                return instance;
            }
        }

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }

        private void Start()
        {
            if (playMusicOnStart && audioConfig.BackgroundMusic != null)
            {
                PlayBackgroundMusic();
            }
        }

        private void Update()
        {
            UpdateAudioPool();
        }

        #endregion

        #region Initialization

        private void InitializeAudio()
        {
            // Initialize audio pool
            for (int i = 0; i < 10; i++)
            {
                AudioSource source = CreatePooledAudioSource(i);
                audioPool.Add(source);
            }

            // Set initial volumes
            UpdateVolumes();
        }

        private AudioSource CreatePooledAudioSource(int index)
        {
            GameObject audioObj = new GameObject($"PooledAudioSource_{index}");
            audioObj.transform.SetParent(transform);
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private void UpdateVolumes()
        {
            if (musicSource != null) musicSource.volume = audioConfig.MusicVolume;
            if (sfxSource != null) sfxSource.volume = audioConfig.SfxVolume;
            if (uiSource != null) uiSource.volume = audioConfig.UiVolume;
        }

        #endregion

        #region Music Control

        public void PlayBackgroundMusic()
        {
            PlayMusic(audioConfig.BackgroundMusic, loop: true);
        }

        public void PlayVictoryMusic()
        {
            PlayMusic(audioConfig.VictoryMusic, loop: false);
        }

        public void PlayGameOverMusic()
        {
            PlayMusic(audioConfig.GameOverMusic, loop: false);
        }

        public void StopMusic() => musicSource?.Stop();

        public void PauseMusic() => musicSource?.Pause();

        public void ResumeMusic() => musicSource?.UnPause();

        private void PlayMusic(AudioClip clip, bool loop)
        {
            if (musicSource == null || clip == null) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = audioConfig.MusicVolume;
            musicSource.Play();
        }

        #endregion

        #region Sound Effects

        public void PlayTileSwapSound() => PlaySfx(audioConfig.TileSwapSound);
        public void PlayTileMatchSound() => PlaySfx(audioConfig.TileMatchSound);
        public void PlayTileClearSound() => PlaySfx(audioConfig.TileClearSound);
        public void PlaySpecialTileCreateSound() => PlaySfx(audioConfig.SpecialTileCreateSound);
        public void PlaySpecialTileActivateSound() => PlaySfx(audioConfig.SpecialTileActivateSound);
        public void PlayPowerUpUseSound() => PlaySfx(audioConfig.PowerUpUseSound);
        public void PlayObjectiveCompleteSound() => PlaySfx(audioConfig.ObjectiveCompleteSound);
        public void PlayButtonClickSound() => PlayUi(audioConfig.ButtonClickSound);
        public void PlayErrorSound() => PlayUi(audioConfig.ErrorSound);

        private void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volumeScale * audioConfig.SfxVolume);
            }
            else
            {
                PlayPooledSfx(clip, volumeScale);
            }
        }

        private void PlayUi(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            if (uiSource != null)
            {
                uiSource.PlayOneShot(clip, volumeScale * audioConfig.UiVolume);
            }
            else
            {
                PlayPooledSfx(clip, volumeScale);
            }
        }

        private void PlayPooledSfx(AudioClip clip, float volumeScale)
        {
            AudioSource source = GetAvailableAudioSource();
            if (source == null) return;

            source.clip = clip;
            source.volume = volumeScale * audioConfig.SfxVolume;
            source.Play();
            activeSources.Add(source);
        }

        private AudioSource GetAvailableAudioSource()
        {
            foreach (AudioSource source in audioPool)
            {
                if (!activeSources.Contains(source))
                {
                    return source;
                }
            }
            return null;
        }

        #endregion

        #region Volume Control

        public void SetMusicVolume(float volume)
        {
            audioConfig.MusicVolume = Mathf.Clamp01(volume);
            if (musicSource != null) musicSource.volume = audioConfig.MusicVolume;
        }

        public void SetSfxVolume(float volume)
        {
            audioConfig.SfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null) sfxSource.volume = audioConfig.SfxVolume;
        }

        public void SetUiVolume(float volume)
        {
            audioConfig.UiVolume = Mathf.Clamp01(volume);
            if (uiSource != null) uiSource.volume = audioConfig.UiVolume;
        }

        public float MusicVolume => audioConfig.MusicVolume;
        public float SfxVolume => audioConfig.SfxVolume;
        public float UiVolume => audioConfig.UiVolume;

        #endregion

        #region Audio Pool Management

        private void UpdateAudioPool()
        {
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                if (!activeSources[i].isPlaying)
                {
                    activeSources.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Test All Sounds")]
        private void TestAllSounds()
        {
            PlayTileSwapSound();
            Invoke(nameof(PlayTileMatchSound), 0.5f);
            Invoke(nameof(PlayTileClearSound), 1f);
            Invoke(nameof(PlaySpecialTileCreateSound), 1.5f);
            Invoke(nameof(PlaySpecialTileActivateSound), 2f);
            Invoke(nameof(PlayPowerUpUseSound), 2.5f);
            Invoke(nameof(PlayObjectiveCompleteSound), 3f);
            Invoke(nameof(PlayButtonClickSound), 3.5f);
            Invoke(nameof(PlayErrorSound), 4f);
        }

        #endregion
    }

    /// <summary>
    /// ScriptableObject to store audio configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioSettingsConfig", menuName = "MechanicGames/Audio Settings", order = 1)]
    public class AudioSettingsConfig : ScriptableObject
    {
        [Header("Music")]
        public AudioClip BackgroundMusic;
        public AudioClip VictoryMusic;
        public AudioClip GameOverMusic;

        [Header("Sound Effects")]
        public AudioClip TileSwapSound;
        public AudioClip TileMatchSound;
        public AudioClip TileClearSound;
        public AudioClip SpecialTileCreateSound;
        public AudioClip SpecialTileActivateSound;
        public AudioClip PowerUpUseSound;
        public AudioClip ObjectiveCompleteSound;
        public AudioClip ButtonClickSound;
        public AudioClip ErrorSound;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float MusicVolume = 0.7f;
        [Range(0f, 1f)] public float SfxVolume = 0.8f;
        [Range(0f, 1f)] public float UiVolume = 0.6f;
    }
}