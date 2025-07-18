using UnityEngine;

namespace Gameplay.LogParade.Utils
{
    /// <summary>
    /// Utilitaire de logging centralisé pour le mini-jeu LogParade
    /// </summary>
    public static class LogParadeLogger
    {
        private const string LOG_PREFIX = "[LogParade] ";
        private static bool enableVerboseLogging = false;

        /// <summary>
        /// Active ou désactive les logs verbeux
        /// </summary>
        public static void SetVerboseLogging(bool enable)
        {
            enableVerboseLogging = enable;
        }

        /// <summary>
        /// Log un message d'information standard
        /// </summary>
        public static void Log(string message)
        {
            Debug.Log($"{LOG_PREFIX}{message}");
        }

        /// <summary>
        /// Log un message d'avertissement
        /// </summary>
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{LOG_PREFIX}{message}");
        }

        /// <summary>
        /// Log un message d'erreur
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError($"{LOG_PREFIX}{message}");
        }

        /// <summary>
        /// Log un message détaillé (uniquement si le mode verbeux est activé)
        /// </summary>
        public static void LogVerbose(string message)
        {
            if (enableVerboseLogging)
            {
                Debug.Log($"{LOG_PREFIX}[VERBOSE] {message}");
            }
        }

        /// <summary>
        /// Log un message de debug (uniquement en mode développement)
        /// </summary>
        public static void LogDebug(string message)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"{LOG_PREFIX}[DEBUG] {message}");
            #endif
        }
    }
}
