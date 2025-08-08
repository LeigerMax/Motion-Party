using UnityEngine;

namespace UI.Menu
{
    /// <summary>
    /// Gestionnaire des paramètres utilisateur pour Motion Party
    /// Gère la sauvegarde et le chargement des préférences (volume, résolution, etc.)
    /// </summary>
    [System.Serializable]
    public static class GameSettingsManager
    {
        // Clés PlayerPrefs
        private const string VOLUME_KEY = "MotionParty_MasterVolume";
        private const string RESOLUTION_INDEX_KEY = "MotionParty_ResolutionIndex";
        private const string FULLSCREEN_KEY = "MotionParty_Fullscreen";
        private const string FIRST_LAUNCH_KEY = "MotionParty_FirstLaunch";
        private const string ACCESSIBILITY_MODE_KEY = "MotionParty_AccessibilityMode";

        // Valeurs par défaut optimisées pour les seniors
        private const float DEFAULT_VOLUME = 0.8f;
        private const int DEFAULT_RESOLUTION_INDEX = 2; // 1366x768 par défaut
        private const bool DEFAULT_FULLSCREEN = true;
        private const bool DEFAULT_ACCESSIBILITY_MODE = true;

        /// <summary>
        /// Initialise les paramètres par défaut au premier lancement
        /// </summary>
        public static void InitializeDefaultSettings()
        {
            if (IsFirstLaunch())
            {
                Debug.Log("[GameSettingsManager] Premier lancement détecté - Initialisation des paramètres par défaut");
                
                SetVolume(DEFAULT_VOLUME);
                SetResolutionIndex(DEFAULT_RESOLUTION_INDEX);
                SetFullscreen(DEFAULT_FULLSCREEN);
                SetAccessibilityMode(DEFAULT_ACCESSIBILITY_MODE);
                
                // Marquer que ce n'est plus le premier lancement
                PlayerPrefs.SetInt(FIRST_LAUNCH_KEY, 1);
                PlayerPrefs.Save();
                
                Debug.Log("[GameSettingsManager] Paramètres par défaut appliqués");
            }
            else
            {
                Debug.Log("[GameSettingsManager] Chargement des paramètres existants");
            }
        }

        #region Volume Settings

        /// <summary>
        /// Définit le volume principal du jeu
        /// </summary>
        public static void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(VOLUME_KEY, volume);
            AudioListener.volume = volume;
            
            Debug.Log($"[GameSettingsManager] Volume défini à {volume:F2} ({Mathf.RoundToInt(volume * 100)}%)");
        }

        /// <summary>
        /// Récupère le volume principal du jeu
        /// </summary>
        public static float GetVolume()
        {
            return PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        }

        /// <summary>
        /// Applique le volume sauvegardé au système audio
        /// </summary>
        public static void ApplyVolumeSettings()
        {
            float savedVolume = GetVolume();
            AudioListener.volume = savedVolume;
            
            // Mettre à jour l'AudioManager si disponible
            if (Core.Audio.AudioManager.Instance != null)
            {
                Core.Audio.AudioManager.Instance.SetSFXVolume(savedVolume);
            }
        }

        #endregion

        #region Resolution Settings

        /// <summary>
        /// Définit l'index de résolution sélectionné
        /// </summary>
        public static void SetResolutionIndex(int index)
        {
            PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, index);
            Debug.Log($"[GameSettingsManager] Index de résolution défini à {index}");
        }

        /// <summary>
        /// Récupère l'index de résolution sauvegardé
        /// </summary>
        public static int GetResolutionIndex()
        {
            return PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, DEFAULT_RESOLUTION_INDEX);
        }

        /// <summary>
        /// Définit le mode plein écran
        /// </summary>
        public static void SetFullscreen(bool fullscreen)
        {
            PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreen ? 1 : 0);
            Screen.fullScreen = fullscreen;
            Debug.Log($"[GameSettingsManager] Mode plein écran: {fullscreen}");
        }

        /// <summary>
        /// Récupère le mode plein écran sauvegardé
        /// </summary>
        public static bool GetFullscreen()
        {
            return PlayerPrefs.GetInt(FULLSCREEN_KEY, DEFAULT_FULLSCREEN ? 1 : 0) == 1;
        }

        /// <summary>
        /// Applique la résolution sauvegardée
        /// </summary>
        public static void ApplyResolutionSettings(string[] availableResolutions)
        {
            int resolutionIndex = GetResolutionIndex();
            bool fullscreen = GetFullscreen();
            
            if (resolutionIndex >= 0 && resolutionIndex < availableResolutions.Length)
            {
                var parts = availableResolutions[resolutionIndex].Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
                {
                    Screen.SetResolution(width, height, fullscreen);
                    Debug.Log($"[GameSettingsManager] Résolution appliquée: {width}x{height}, Plein écran: {fullscreen}");
                }
            }
        }

        #endregion

        #region Accessibility Settings

        /// <summary>
        /// Active/désactive le mode accessibilité (interfaces agrandies, contrastes élevés)
        /// </summary>
        public static void SetAccessibilityMode(bool enabled)
        {
            PlayerPrefs.SetInt(ACCESSIBILITY_MODE_KEY, enabled ? 1 : 0);
            Debug.Log($"[GameSettingsManager] Mode accessibilité: {enabled}");
        }

        /// <summary>
        /// Récupère l'état du mode accessibilité
        /// </summary>
        public static bool GetAccessibilityMode()
        {
            return PlayerPrefs.GetInt(ACCESSIBILITY_MODE_KEY, DEFAULT_ACCESSIBILITY_MODE ? 1 : 0) == 1;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Vérifie si c'est le premier lancement du jeu
        /// </summary>
        public static bool IsFirstLaunch()
        {
            return !PlayerPrefs.HasKey(FIRST_LAUNCH_KEY);
        }

        /// <summary>
        /// Sauvegarde tous les paramètres
        /// </summary>
        public static void SaveAllSettings()
        {
            PlayerPrefs.Save();
            Debug.Log("[GameSettingsManager] Tous les paramètres sauvegardés");
        }

        /// <summary>
        /// Remet tous les paramètres aux valeurs par défaut
        /// </summary>
        public static void ResetToDefaults()
        {
            Debug.Log("[GameSettingsManager] Réinitialisation des paramètres par défaut");
            
            SetVolume(DEFAULT_VOLUME);
            SetResolutionIndex(DEFAULT_RESOLUTION_INDEX);
            SetFullscreen(DEFAULT_FULLSCREEN);
            SetAccessibilityMode(DEFAULT_ACCESSIBILITY_MODE);
            
            SaveAllSettings();
        }

        /// <summary>
        /// Supprime tous les paramètres sauvegardés
        /// </summary>
        public static void ClearAllSettings()
        {
            PlayerPrefs.DeleteKey(VOLUME_KEY);
            PlayerPrefs.DeleteKey(RESOLUTION_INDEX_KEY);
            PlayerPrefs.DeleteKey(FULLSCREEN_KEY);
            PlayerPrefs.DeleteKey(FIRST_LAUNCH_KEY);
            PlayerPrefs.DeleteKey(ACCESSIBILITY_MODE_KEY);
            PlayerPrefs.Save();
            
            Debug.Log("[GameSettingsManager] Tous les paramètres supprimés");
        }

        /// <summary>
        /// Affiche un résumé de tous les paramètres actuels
        /// </summary>
        public static void LogCurrentSettings()
        {
            Debug.Log("=== PARAMÈTRES ACTUELS ===");
            Debug.Log($"Volume: {GetVolume():F2} ({Mathf.RoundToInt(GetVolume() * 100)}%)");
            Debug.Log($"Index résolution: {GetResolutionIndex()}");
            Debug.Log($"Plein écran: {GetFullscreen()}");
            Debug.Log($"Mode accessibilité: {GetAccessibilityMode()}");
            Debug.Log($"Premier lancement: {IsFirstLaunch()}");
            Debug.Log($"Résolution écran actuelle: {Screen.width}x{Screen.height}");
        }

        #endregion

        #region Validation

        /// <summary>
        /// Valide que tous les paramètres sont dans des plages acceptables
        /// </summary>
        public static bool ValidateSettings()
        {
            bool isValid = true;
            
            float volume = GetVolume();
            if (volume < 0f || volume > 1f)
            {
                Debug.LogWarning($"[GameSettingsManager] Volume invalide: {volume} - Correction à {DEFAULT_VOLUME}");
                SetVolume(DEFAULT_VOLUME);
                isValid = false;
            }
            
            int resolutionIndex = GetResolutionIndex();
            if (resolutionIndex < 0)
            {
                Debug.LogWarning($"[GameSettingsManager] Index résolution invalide: {resolutionIndex} - Correction à {DEFAULT_RESOLUTION_INDEX}");
                SetResolutionIndex(DEFAULT_RESOLUTION_INDEX);
                isValid = false;
            }
            
            return isValid;
        }

        #endregion
    }
}
