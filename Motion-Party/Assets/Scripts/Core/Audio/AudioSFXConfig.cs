using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Configuration audio persistante pour Motion Party
    /// Stocke les références aux AudioClips qui seront chargés automatiquement
    /// </summary>
    [CreateAssetMenu(fileName = "AudioSFXConfig", menuName = "Motion Party/Audio/SFX Configuration")]
    public class AudioSFXConfig : ScriptableObject
    {
        [Header("Sound Effects")]
        [SerializeField] private AudioClip waterSplashSound;
        [SerializeField] private AudioClip fireflyCaptureSound;
        
        [Header("Volume Settings")]
        [SerializeField] private float sfxVolume = 0.8f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Propriétés publiques pour accéder aux sons
        public AudioClip WaterSplashSound => waterSplashSound;
        public AudioClip FireflyCaptureSound => fireflyCaptureSound;
        public float SfxVolume => sfxVolume;
        public bool EnableDebugLogs => enableDebugLogs;

        /// <summary>
        /// Valide que tous les AudioClips nécessaires sont assignés
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;
            
            if (waterSplashSound == null)
            {
                Debug.LogWarning("⚠️ AudioSFXConfig: Water Splash Sound n'est pas assigné !");
                isValid = false;
            }
            
            if (fireflyCaptureSound == null)
            {
                Debug.LogWarning("⚠️ AudioSFXConfig: Firefly Capture Sound n'est pas assigné !");
                isValid = false;
            }
            
            return isValid;
        }

        /// <summary>
        /// Crée une configuration par défaut
        /// </summary>
        public static AudioSFXConfig CreateDefault()
        {
            var config = CreateInstance<AudioSFXConfig>();
            config.sfxVolume = 0.8f;
            config.enableDebugLogs = true;
            return config;
        }

        /// <summary>
        /// Charge la configuration depuis les Resources
        /// </summary>
        public static AudioSFXConfig LoadFromResources()
        {
            var config = Resources.Load<AudioSFXConfig>("AudioSFXConfig");
            if (config == null)
            {
                Debug.LogWarning("⚠️ AudioSFXConfig non trouvé dans Resources/ - Création d'une configuration par défaut");
                config = CreateDefault();
            }
            return config;
        }
    }
}
