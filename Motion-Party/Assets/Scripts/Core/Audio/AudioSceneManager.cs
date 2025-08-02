using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Audio
{
    /// <summary>
    /// Gestionnaire qui recharge automatiquement la configuration audio à chaque changement de scène
    /// S'assure que l'AudioManager garde ses sons même après les transitions de scène
    /// </summary>
    public class AudioSceneManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private void Awake()
        {
            // S'abonner aux événements de changement de scène
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (enableDebugLogs)
            {
                Debug.Log("🔄 AudioSceneManager initialisé - Surveillance des changements de scène activée");
            }
        }

        private void OnDestroy()
        {
            // Se désabonner pour éviter les fuites mémoire
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Appelé à chaque fois qu'une nouvelle scène est chargée
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"🔄 Scène chargée : {scene.name} - Rechargement de la configuration audio...");
            }

            // Recharger la configuration audio de l'AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ReloadConfiguration();
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("⚠️ AudioManager non trouvé lors du rechargement de scène");
                }
            }
        }

        /// <summary>
        /// Force le rechargement de la configuration audio
        /// Utile pour les tests ou débuggage
        /// </summary>
        [ContextMenu("Force Reload Audio Config")]
        public void ForceReloadAudioConfig()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ReloadConfiguration();
                Debug.Log("🔄 Configuration audio rechargée manuellement");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager non trouvé pour le rechargement manuel");
            }
        }
    }
}
