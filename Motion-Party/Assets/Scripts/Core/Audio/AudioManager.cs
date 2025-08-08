using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Gestionnaire audio simplifié pour Motion Party
    /// Gère uniquement les effets sonores : water splash et firefly capture
    /// Les musiques sont gérées manuellement sur chaque scène
    /// Utilise AudioSFXConfig pour persister les sons entre les scènes
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AudioSFXConfig audioConfig;

        [Header("Audio Source")]
        [SerializeField] private AudioSource sfxSource;

        // Variables internes pour les sons chargés
        private AudioClip waterSplashSound;
        private AudioClip fireflyCaptureSound;
        private float sfxVolume = 0.8f;
        private bool enableDebugLogs = true;

        // Singleton pattern
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject audioManagerObject = new GameObject("AudioManager");
                        _instance = audioManagerObject.AddComponent<AudioManager>();
                        DontDestroyOnLoad(audioManagerObject);
                    }
                }
                return _instance;
            }
        }

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAudioConfiguration();
            InitializeAudioSource();
        }

        #endregion

        #region Configuration Loading

        /// <summary>
        /// Charge la configuration audio depuis ScriptableObject ou Resources
        /// </summary>
        private void LoadAudioConfiguration()
        {
            // Essayer d'utiliser la config assignée dans l'Inspector d'abord
            if (audioConfig == null)
            {
                // Si pas assignée, charger depuis Resources
                audioConfig = AudioSFXConfig.LoadFromResources();
            }

            if (audioConfig != null)
            {
                // Appliquer la configuration
                waterSplashSound = audioConfig.WaterSplashSound;
                fireflyCaptureSound = audioConfig.FireflyCaptureSound;
                sfxVolume = audioConfig.SfxVolume;
                enableDebugLogs = audioConfig.EnableDebugLogs;

                // Valider la configuration
                if (audioConfig.ValidateConfiguration())
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log("🔊 AudioSFXConfig chargé avec succès !");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ AudioSFXConfig incomplet - Certains sons peuvent être manquants");
                }
            }
            else
            {
                Debug.LogError("❌ Impossible de charger AudioSFXConfig !");
            }
        }

        /// <summary>
        /// Recharge la configuration audio (utile après changement de scène)
        /// </summary>
        public void ReloadConfiguration()
        {
            LoadAudioConfiguration();
            
            // Mettre à jour le volume de l'AudioSource si elle existe
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }

            if (enableDebugLogs)
            {
                Debug.Log("🔄 Configuration audio rechargée");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise l'AudioSource pour les effets sonores
        /// </summary>
        private void InitializeAudioSource()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.volume = sfxVolume;
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;

            if (enableDebugLogs)
            {
                Debug.Log("🔊 AudioManager initialisé - Prêt pour les effets sonores");
            }
        }

        #endregion

        #region Sound Effects

        /// <summary>
        /// Joue le son d'éclaboussure d'eau
        /// </summary>
        public void PlayWaterSplash()
        {
            PlaySoundEffect(waterSplashSound, "Water Splash");
        }

        /// <summary>
        /// Joue le son de capture de libellule
        /// </summary>
        public void PlayFireflyCapture()
        {
            PlaySoundEffect(fireflyCaptureSound, "Firefly Capture");
        }

        /// <summary>
        /// Joue un effet sonore spécifique
        /// </summary>
        private void PlaySoundEffect(AudioClip clip, string effectName)
        {
            if (clip == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"⚠️ AudioClip pour '{effectName}' est null ! Tentative de rechargement...");
                }
                
                // Tenter de recharger la configuration
                ReloadConfiguration();
                
                // Réessayer avec le clip rechargé
                if (effectName == "Water Splash" && waterSplashSound != null)
                {
                    clip = waterSplashSound;
                }
                else if (effectName == "Firefly Capture" && fireflyCaptureSound != null)
                {
                    clip = fireflyCaptureSound;
                }
                
                if (clip == null)
                {
                    Debug.LogError($"❌ Impossible de charger '{effectName}' même après rechargement !");
                    return;
                }
            }

            if (sfxSource == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("⚠️ SFX AudioSource est null ! Réinitialisation...");
                }
                InitializeAudioSource();
            }

            sfxSource.PlayOneShot(clip, sfxVolume);

            if (enableDebugLogs)
            {
                Debug.Log($"🔊 Effet sonore joué : {effectName}");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Configure le volume des effets sonores
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }

            if (enableDebugLogs)
            {
                Debug.Log($"🔊 Volume SFX mis à jour : {sfxVolume:F2}");
            }
        }

        /// <summary>
        /// Active/désactive les logs de debug
        /// </summary>
        public void SetDebugLogging(bool enabled)
        {
            enableDebugLogs = enabled;
            Debug.Log($"🔊 Debug logs AudioManager : {(enabled ? "activés" : "désactivés")}");
        }

        #endregion
    }
}