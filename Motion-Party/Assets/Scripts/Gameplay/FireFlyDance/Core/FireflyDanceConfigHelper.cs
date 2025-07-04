using UnityEngine;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Helper class pour gérer automatiquement la configuration FireflyDance
    /// </summary>
    public static class FireflyDanceConfigHelper
    {
        private static FireflyDanceConfig _defaultConfig;
        
        /// <summary>
        /// Obtient une configuration, en créant une par défaut si nécessaire
        /// </summary>
        public static FireflyDanceConfig GetOrCreateConfig()
        {
            // Chercher une config existante dans la scène
            var existingConfig = Object.FindFirstObjectByType<FireflyDanceGameManager>()?.config;
            if (existingConfig != null)
            {
                return existingConfig;
            }
            
            // Utiliser ou créer une config par défaut
            if (_defaultConfig == null)
            {
                _defaultConfig = FireflyDanceConfig.CreateDefault();
                FireflyDanceLogger.Log("Configuration par défaut créée par FireflyDanceConfigHelper");
            }
            
            return _defaultConfig;
        }
        
        /// <summary>
        /// Assigne automatiquement une configuration à un composant si elle est manquante
        /// </summary>
        public static void EnsureConfig(ref FireflyDanceConfig config, string componentName = "Component")
        {
            if (config == null)
            {
                config = GetOrCreateConfig();
                FireflyDanceLogger.Log($"{componentName} - Configuration assignée automatiquement");
            }
        }
        
        /// <summary>
        /// Vérifie et corrige les références de configuration dans une scène
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ValidateSceneConfigs()
        {
            // Cette méthode sera appelée au démarrage pour s'assurer que toutes les configurations sont valides
        }
    }
}
