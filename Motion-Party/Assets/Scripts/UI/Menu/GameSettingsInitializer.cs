using UnityEngine;
using Core.Audio;

namespace UI.Menu
{
    /// <summary>
    /// Initialise automatiquement les paramètres du jeu au démarrage
    /// Charge les préférences sauvegardées et les applique au système
    /// </summary>
    public class GameSettingsInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoLoadOnAwake = true;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Paramètres par défaut (Seniors)")]
        [SerializeField] private float defaultVolume = 0.8f;
        [SerializeField] private string defaultResolution = "1366x768";

        private void Awake()
        {
            if (autoLoadOnAwake)
            {
                InitializeGameSettings();
            }
        }

        /// <summary>
        /// Initialise tous les paramètres du jeu
        /// </summary>
        [ContextMenu("Initialiser Paramètres")]
        public void InitializeGameSettings()
        {
            if (enableDebugLogs)
            {
                Debug.Log("[GameSettingsInitializer] Début de l'initialisation des paramètres");
            }

            // Initialiser les paramètres par défaut si c'est le premier lancement
            GameSettingsManager.InitializeDefaultSettings();

            // Charger et appliquer les paramètres audio
            LoadAndApplyAudioSettings();

            // Charger et appliquer les paramètres d'affichage
            LoadAndApplyDisplaySettings();

            // Valider que tous les paramètres sont corrects
            ValidateSettings();

            if (enableDebugLogs)
            {
                Debug.Log("[GameSettingsInitializer] Initialisation des paramètres terminée");
                GameSettingsManager.LogCurrentSettings();
            }
        }

        /// <summary>
        /// Charge et applique les paramètres audio
        /// </summary>
        private void LoadAndApplyAudioSettings()
        {
            float savedVolume = GameSettingsManager.GetVolume();
            
            // Appliquer le volume principal
            AudioListener.volume = savedVolume;
            
            // Mettre à jour l'AudioManager si disponible
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(savedVolume);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[GameSettingsInitializer] Volume appliqué via AudioManager: {savedVolume:F2}");
                }
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[GameSettingsInitializer] Volume appliqué directement: {savedVolume:F2}");
                }
            }
        }

        /// <summary>
        /// Charge et applique les paramètres d'affichage
        /// </summary>
        private void LoadAndApplyDisplaySettings()
        {
            // Résolutions recommandées pour seniors
            string[] recommendedResolutions = {
                "1024x768",    // 4:3 - Petite
                "1280x720",    // 16:9 - HD
                "1366x768",    // 16:9 - Populaire
                "1440x900",    // 16:10 - Moyenne
                "1920x1080",   // 16:9 - Full HD
                "2560x1440"    // 16:9 - Grande
            };

            // Appliquer la résolution sauvegardée
            GameSettingsManager.ApplyResolutionSettings(recommendedResolutions);

            // Appliquer le mode plein écran
            bool fullscreen = GameSettingsManager.GetFullscreen();
            if (Screen.fullScreen != fullscreen)
            {
                Screen.fullScreen = fullscreen;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[GameSettingsInitializer] Mode plein écran appliqué: {fullscreen}");
                }
            }
        }

        /// <summary>
        /// Valide que tous les paramètres sont dans des plages acceptables
        /// </summary>
        private void ValidateSettings()
        {
            bool isValid = GameSettingsManager.ValidateSettings();
            
            if (!isValid && enableDebugLogs)
            {
                Debug.LogWarning("[GameSettingsInitializer] Certains paramètres ont été corrigés");
            }
        }

        /// <summary>
        /// Remet les paramètres aux valeurs par défaut optimisées pour seniors
        /// </summary>
        [ContextMenu("Réinitialiser Paramètres Seniors")]
        public void ResetToSeniorDefaults()
        {
            if (enableDebugLogs)
            {
                Debug.Log("[GameSettingsInitializer] Réinitialisation aux paramètres par défaut pour seniors");
            }

            // Paramètres audio optimisés
            GameSettingsManager.SetVolume(defaultVolume);
            
            // Paramètres d'affichage optimisés
            GameSettingsManager.SetFullscreen(true); // Plein écran pour une meilleure visibilité
            GameSettingsManager.SetAccessibilityMode(true);
            
            // Trouver l'index de la résolution par défaut
            string[] resolutions = {
                "1024x768", "1280x720", "1366x768", "1440x900", "1920x1080", "2560x1440"
            };
            
            int defaultIndex = 2; // 1366x768 par défaut
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i] == defaultResolution)
                {
                    defaultIndex = i;
                    break;
                }
            }
            
            GameSettingsManager.SetResolutionIndex(defaultIndex);
            
            // Sauvegarder les changements
            GameSettingsManager.SaveAllSettings();
            
            // Appliquer immédiatement
            InitializeGameSettings();
            
            if (enableDebugLogs)
            {
                Debug.Log("[GameSettingsInitializer] Paramètres par défaut pour seniors appliqués");
            }
        }

        /// <summary>
        /// Configure les paramètres spécifiquement pour une utilisation senior
        /// </summary>
        [ContextMenu("Optimiser pour Seniors")]
        public void OptimizeForSeniors()
        {
            // Volume plus élevé par défaut
            GameSettingsManager.SetVolume(0.9f);
            
            // Mode accessibilité activé
            GameSettingsManager.SetAccessibilityMode(true);
            
            // Plein écran pour éviter les confusions avec les fenêtres
            GameSettingsManager.SetFullscreen(true);
            
            // Résolution moyenne pour un bon équilibre lisibilité/performance
            GameSettingsManager.SetResolutionIndex(2); // 1366x768
            
            GameSettingsManager.SaveAllSettings();
            InitializeGameSettings();
            
            Debug.Log("[GameSettingsInitializer] Optimisation pour seniors appliquée");
        }

        /// <summary>
        /// Teste les paramètres actuels et affiche un rapport
        /// </summary>
        [ContextMenu("Tester Paramètres")]
        public void TestCurrentSettings()
        {
            Debug.Log("=== TEST DES PARAMÈTRES ACTUELS ===");
            
            // Test du volume
            float volume = AudioListener.volume;
            Debug.Log($"Volume AudioListener: {volume:F2} ({Mathf.RoundToInt(volume * 100)}%)");
            
            // Test de la résolution
            Debug.Log($"Résolution écran: {Screen.width}x{Screen.height}");
            Debug.Log($"Mode plein écran: {Screen.fullScreen}");
            Debug.Log($"Fréquence de rafraîchissement: {Screen.currentResolution.refreshRate}Hz");
            
            // Test de l'AudioManager
            if (AudioManager.Instance != null)
            {
                Debug.Log("✅ AudioManager disponible");
            }
            else
            {
                Debug.LogWarning("⚠️ AudioManager non disponible");
            }
            
            // Afficher les paramètres sauvegardés
            GameSettingsManager.LogCurrentSettings();
        }

        /// <summary>
        /// Nettoie tous les paramètres et redémarre avec les valeurs par défaut
        /// </summary>
        [ContextMenu("Nettoyage Complet")]
        public void CleanResetSettings()
        {
            Debug.Log("[GameSettingsInitializer] Nettoyage complet des paramètres");
            
            GameSettingsManager.ClearAllSettings();
            ResetToSeniorDefaults();
            
            Debug.Log("[GameSettingsInitializer] Nettoyage terminé - Paramètres réinitialisés");
        }
    }
}
