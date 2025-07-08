using UnityEngine;

namespace CameraTransitions
{
    /// <summary>
    /// Utilitaire pour gérer automatiquement les Audio Listeners dans la scène.
    /// Résout le problème "There are 2 audio listeners in the scene".
    /// </summary>
    public class AudioListenerManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoCleanOnStart = true;
        [SerializeField] private bool showDebugLogs = true;
        
        private void Start()
        {
            if (autoCleanOnStart)
                CleanupAudioListeners();
        }
        
        /// <summary>
        /// Nettoie automatiquement les Audio Listeners en gardant seulement celui de la caméra principale
        /// </summary>
        [ContextMenu("Cleanup Audio Listeners")]
        public void CleanupAudioListeners()
        {
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            
            if (allListeners.Length <= 1)
            {
                if (showDebugLogs)
                    Debug.Log($"AudioListenerManager: {allListeners.Length} Audio Listener trouvé, aucun nettoyage nécessaire");
                return;
            }
            
            if (showDebugLogs)
                Debug.Log($"AudioListenerManager: {allListeners.Length} Audio Listeners trouvés, nettoyage en cours...");
            
            // Trouver la caméra principale
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            AudioListener mainListener = null;
            
            if (mainCamera != null)
                mainListener = mainCamera.GetComponent<AudioListener>();
            
            // Si pas de listener sur la caméra principale, garder le premier trouvé
            if (mainListener == null && allListeners.Length > 0)
                mainListener = allListeners[0];
            
            // Désactiver tous les autres listeners
            int disabledCount = 0;
            foreach (AudioListener listener in allListeners)
            {
                if (listener != mainListener)
                {
                    listener.enabled = false;
                    disabledCount++;
                    
                    if (showDebugLogs)
                        Debug.Log($"AudioListenerManager: Audio Listener désactivé sur {listener.gameObject.name}");
                }
            }
            
            // S'assurer que le listener principal est actif
            if (mainListener != null)
            {
                mainListener.enabled = true;
                if (showDebugLogs)
                    Debug.Log($"AudioListenerManager: Audio Listener principal activé sur {mainListener.gameObject.name}");
            }
            
            if (showDebugLogs)
                Debug.Log($"AudioListenerManager: Nettoyage terminé. {disabledCount} listeners désactivés, 1 actif");
        }
        
        /// <summary>
        /// Vérifie le nombre d'Audio Listeners actifs dans la scène
        /// </summary>
        public int GetActiveAudioListenerCount()
        {
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            int activeCount = 0;
            
            foreach (AudioListener listener in allListeners)
            {
                if (listener.enabled)
                    activeCount++;
            }
            
            return activeCount;
        }
        
        /// <summary>
        /// Affiche un rapport sur les Audio Listeners de la scène
        /// </summary>
        [ContextMenu("Audio Listener Report")]
        public void ShowAudioListenerReport()
        {
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            int activeCount = 0;
            int inactiveCount = 0;
            
            Debug.Log("=== RAPPORT AUDIO LISTENERS ===");
            
            foreach (AudioListener listener in allListeners)
            {
                string status = listener.enabled ? "ACTIF" : "INACTIF";
                Debug.Log($"- {listener.gameObject.name}: {status}");
                
                if (listener.enabled)
                    activeCount++;
                else
                    inactiveCount++;
            }
            
            Debug.Log($"Total: {allListeners.Length} | Actifs: {activeCount} | Inactifs: {inactiveCount}");
            
            if (activeCount > 1)
                Debug.LogWarning("⚠️ ATTENTION: Plus d'un Audio Listener actif détecté !");
            else if (activeCount == 1)
                Debug.Log("✅ Configuration Audio Listener correcte");
            else
                Debug.LogWarning("⚠️ ATTENTION: Aucun Audio Listener actif !");
        }
        
        private void OnValidate()
        {
            // Vérification automatique dans l'éditeur
            #if UNITY_EDITOR
            if (Application.isPlaying && showDebugLogs)
            {
                int activeCount = GetActiveAudioListenerCount();
                if (activeCount > 1)
                    Debug.LogWarning($"AudioListenerManager: {activeCount} Audio Listeners actifs détectés sur {gameObject.name}");
            }
            #endif
        }
    }
}
