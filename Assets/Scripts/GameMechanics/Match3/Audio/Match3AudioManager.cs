using UnityEngine;
using System.Collections.Generic;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Audio manager for Match-3 game sounds and music.
    /// </summary>
    public sealed class Match3AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip gameOverMusic;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip tileSwapSound;
        [SerializeField] private AudioClip tileMatchSound;
        [SerializeField] private AudioClip tileClearSound;
        [SerializeField] private AudioClip specialTileCreateSound;
        [SerializeField] private AudioClip specialTileActivateSound;
        [SerializeField] private AudioClip powerUpUseSound;
        [SerializeField] private AudioClip objectiveCompleteSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip errorSound;

        [Header("Audio Settings")]
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 0.8f;
        [SerializeField] private float uiVolume = 0.6f;
        [SerializeField] private bool playMusicOnStart = true;

        // Audio pools for performance
        private Queue<AudioSource> audioSourcePool;
        private List<AudioSource> activeAudioSources;

        // Singleton pattern
        public static Match3AudioManager Instance { get; private set; }

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (playMusicOnStart && backgroundMusic != null)
            {
                PlayBackgroundMusic();
            }
        }

        private void Update()
        {
            UpdateActiveAudioSources();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the audio system.
        /// </summary>
        private void InitializeAudio()
        {
            // Create audio source pool
            audioSourcePool = new Queue<AudioSource>();
            activeAudioSources = new List<AudioSource>();

            // Create additional audio sources for pooling
            for (int i = 0; i < 10; i++)
            {
                GameObject audioObj = new GameObject($"PooledAudioSource_{i}");
                audioObj.transform.SetParent(transform);
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                audioSourcePool.Enqueue(source);
            }

            // Set initial volumes
            if (musicSource != null) musicSource.volume = musicVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (uiSource != null) uiSource.volume = uiVolume;

            Debug.Log("Match3AudioManager: Audio system initialized");
        }

        #endregion

        #region Music

        /// <summary>
        /// Play background music.
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
                musicSource.Play();
                Debug.Log("Match3AudioManager: Playing background music");
            }
        }

        /// <summary>
        /// Play victory music.
        /// </summary>
        public void PlayVictoryMusic()
        {
            if (musicSource != null && victoryMusic != null)
            {
                musicSource.clip = victoryMusic;
                musicSource.loop = false;
                musicSource.volume = musicVolume;
                musicSource.Play();
                Debug.Log("Match3AudioManager: Playing victory music");
            }
        }

        /// <summary>
        /// Play game over music.
        /// </summary>
        public void PlayGameOverMusic()
        {
            if (musicSource != null && gameOverMusic != null)
            {
                musicSource.clip = gameOverMusic;
                musicSource.loop = false;
                musicSource.volume = musicVolume;
                musicSource.Play();
                Debug.Log("Match3AudioManager: Playing game over music");
            }
        }

        /// <summary>
        /// Stop background music.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Pause background music.
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }

        /// <summary>
        /// Resume background music.
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Play tile swap sound.
        /// </summary>
        public void PlayTileSwapSound()
        {
            PlaySFX(tileSwapSound);
        }

        /// <summary>
        /// Play tile match sound.
        /// </summary>
        public void PlayTileMatchSound()
        {
            PlaySFX(tileMatchSound);
        }

        /// <summary>
        /// Play tile clear sound.
        /// </summary>
        public void PlayTileClearSound()
        {
            PlaySFX(tileClearSound);
        }

        /// <summary>
        /// Play special tile creation sound.
        /// </summary>
        public void PlaySpecialTileCreateSound()
        {
            PlaySFX(specialTileCreateSound);
        }

        /// <summary>
        /// Play special tile activation sound.
        /// </summary>
        public void PlaySpecialTileActivateSound()
        {
            PlaySFX(specialTileActivateSound);
        }

        /// <summary>
        /// Play power-up use sound.
        /// </summary>
        public void PlayPowerUpUseSound()
        {
            PlaySFX(powerUpUseSound);
        }

        /// <summary>
        /// Play objective complete sound.
        /// </summary>
        public void PlayObjectiveCompleteSound()
        {
            PlaySFX(objectiveCompleteSound);
        }

        /// <summary>
        /// Play button click sound.
        /// </summary>
        public void PlayButtonClickSound()
        {
            PlayUI(buttonClickSound);
        }

        /// <summary>
        /// Play error sound.
        /// </summary>
        public void PlayErrorSound()
        {
            PlayUI(errorSound);
        }

        /// <summary>
        /// Play a sound effect.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume * sfxVolume);
            }
            else
            {
                PlayPooledSFX(clip, volume);
            }
        }

        /// <summary>
        /// Play a UI sound.
        /// </summary>
        public void PlayUI(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            if (uiSource != null)
            {
                uiSource.PlayOneShot(clip, volume * uiVolume);
            }
            else
            {
                PlayPooledSFX(clip, volume);
            }
        }

        /// <summary>
        /// Play a sound using the audio source pool.
        /// </summary>
        private void PlayPooledSFX(AudioClip clip, float volume = 1f)
        {
            if (audioSourcePool.Count > 0)
            {
                AudioSource source = audioSourcePool.Dequeue();
                source.clip = clip;
                source.volume = volume * sfxVolume;
                source.Play();
                activeAudioSources.Add(source);
            }
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }

        /// <summary>
        /// Set UI volume.
        /// </summary>
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            if (uiSource != null)
            {
                uiSource.volume = uiVolume;
            }
        }

        /// <summary>
        /// Get current music volume.
        /// </summary>
        public float MusicVolume => musicVolume;

        /// <summary>
        /// Get current SFX volume.
        /// </summary>
        public float SFXVolume => sfxVolume;

        /// <summary>
        /// Get current UI volume.
        /// </summary>
        public float UIVolume => uiVolume;

        #endregion

        #region Audio Source Pool Management

        /// <summary>
        /// Update active audio sources and return finished ones to pool.
        /// </summary>
        private void UpdateActiveAudioSources()
        {
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = activeAudioSources[i];
                if (!source.isPlaying)
                {
                    activeAudioSources.RemoveAt(i);
                    audioSourcePool.Enqueue(source);
                }
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Test all sound effects.
        /// </summary>
        [ContextMenu("Test All Sounds")]
        public void TestAllSounds()
        {
            Debug.Log("Match3AudioManager: Testing all sounds...");
            
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
}
