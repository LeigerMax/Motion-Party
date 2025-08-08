using UnityEngine;

namespace Core.Analytics.Debug
{
    /// <summary>
    /// Logger spécialisé pour le système d'analytics
    /// </summary>
    public static class AnalyticsDebugLogger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log($"[Analytics] {message}");
        }
        
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"[Analytics] {message}");
        }
        
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[Analytics] {message}");
        }
    }
    
    /// <summary>
    /// Méthodes statiques pour compatibilité avec l'ancien système
    /// </summary>
    public static class Log
    {
        public static void Info(string message)
        {
            AnalyticsDebugLogger.Log(message);
        }
    }
    
    public static class LogWarning
    {
        public static void Write(string message)
        {
            AnalyticsDebugLogger.LogWarning(message);
        }
    }
    
    public static class LogError
    {
        public static void Write(string message)
        {
            AnalyticsDebugLogger.LogError(message);
        }
    }
}
