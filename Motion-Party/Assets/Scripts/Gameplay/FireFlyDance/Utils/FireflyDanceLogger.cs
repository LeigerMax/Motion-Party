using UnityEngine;

namespace Gameplay.FireFlyDance.Utils
{
    /// <summary>
    /// Système de logging centralisé pour le mini-jeu Danse des Lucioles
    /// Permet de contrôler globalement l'affichage des logs de debug
    /// </summary>
    public static class FireflyDanceLogger
{
    #region Log Categories

    /// <summary>
    /// Catégories de logs disponibles
    /// </summary>
    public enum LogCategory
    {
        General,
        Tracking,
        Spawn,
        Capture,
        Score,
        State,
        Events
    }

    #endregion

    #region Fields

    private static bool _enableDebugLogs = true;
    private static bool _enableVerboseLogs = false;
    private static bool _enableTrackingLogs = false;
    private static bool _enableSpawnLogs = true;
    private static bool _enableCaptureLogs = true;
    private static bool _enableScoreLogs = true;
    private static bool _enableStateLogs = true;
    private static bool _enableEventLogs = false;

    #endregion

    #region Properties

    /// <summary>
    /// Active/désactive tous les logs FireflyDance
    /// </summary>
    public static bool EnableDebugLogs
    {
        get => _enableDebugLogs;
        set => _enableDebugLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs verbeux (pour Update, triggers fréquents)
    /// </summary>
    public static bool EnableVerboseLogs
    {
        get => _enableVerboseLogs;
        set => _enableVerboseLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs de tracking (position main, etc.)
    /// </summary>
    public static bool EnableTrackingLogs
    {
        get => _enableTrackingLogs;
        set => _enableTrackingLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs de spawn
    /// </summary>
    public static bool EnableSpawnLogs
    {
        get => _enableSpawnLogs;
        set => _enableSpawnLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs de capture
    /// </summary>
    public static bool EnableCaptureLogs
    {
        get => _enableCaptureLogs;
        set => _enableCaptureLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs de score
    /// </summary>
    public static bool EnableScoreLogs
    {
        get => _enableScoreLogs;
        set => _enableScoreLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs d'état
    /// </summary>
    public static bool EnableStateLogs
    {
        get => _enableStateLogs;
        set => _enableStateLogs = value;
    }

    /// <summary>
    /// Active/désactive les logs d'événements
    /// </summary>
    public static bool EnableEventLogs
    {
        get => _enableEventLogs;
        set => _enableEventLogs = value;
    }

    #endregion

    #region General Logging Methods

    /// <summary>
    /// Log normal avec tag [FireflyDance]
    /// </summary>
    public static void Log(string message, Object context = null)
    {
        if (_enableDebugLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance] {message}", context);
        }
    }

    /// <summary>
    /// Log verbeux (fréquent) avec tag [FireflyDance-V]
    /// </summary>
    public static void LogVerbose(string message, Object context = null)
    {
        if (_enableVerboseLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-V] {message}", context);
        }
    }

    /// <summary>
    /// Warning avec tag [FireflyDance]
    /// </summary>
    public static void LogWarning(string message, Object context = null)
    {
        if (_enableDebugLogs && !string.IsNullOrEmpty(message))
        {
            Debug.LogWarning($"[FireflyDance] {message}", context);
        }
    }

    /// <summary>
    /// Error avec tag [FireflyDance] (toujours affiché)
    /// </summary>
    public static void LogError(string message, Object context = null)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Debug.LogError($"[FireflyDance] {message}", context);
        }
    }

    #endregion

    #region Category-Specific Logging

    /// <summary>
    /// Log spécifique au tracking
    /// </summary>
    public static void LogTracking(string message, Object context = null)
    {
        if (_enableDebugLogs && _enableTrackingLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-Tracking] {message}", context);
        }
    }

    /// <summary>
    /// Log spécifique au spawn
    /// </summary>
    public static void LogSpawn(string message, Object context = null)
    {
        if (_enableDebugLogs && _enableSpawnLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-Spawn] {message}", context);
        }
    }

    /// <summary>
    /// Log spécifique aux captures
    /// </summary>
    public static void LogCapture(string message, Object context = null)
    {
        if (_enableDebugLogs && _enableCaptureLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-Capture] {message}", context);
        }
    }

    /// <summary>
    /// Log spécifique au score
    /// </summary>
    public static void LogScore(string message, Object context = null)
    {
        if (_enableDebugLogs && _enableScoreLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-Score] {message}", context);
        }
    }

    /// <summary>
    /// Log spécifique aux états
    /// </summary>
    public static void LogState(string message, Object context = null)
    {
        if (_enableDebugLogs && _enableStateLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-State] {message}", context);
        }
    }

    /// <summary>
    /// Log spécifique aux événements
    /// </summary>
    public static void LogEvent(string message, Object context = null)
    {
        if (_enableDebugLogs && _enableEventLogs && !string.IsNullOrEmpty(message))
        {
            Debug.Log($"[FireflyDance-Event] {message}", context);
        }
    }

    #endregion

    #region Toggle Methods

    /// <summary>
    /// Active/désactive les logs de debug
    /// </summary>
    public static void ToggleDebugLogs()
    {
        _enableDebugLogs = !_enableDebugLogs;
        Debug.Log($"[FireflyDance] Debug logs: {(_enableDebugLogs ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Active/désactive les logs verbeux
    /// </summary>
    public static void ToggleVerboseLogs()
    {
        _enableVerboseLogs = !_enableVerboseLogs;
        Debug.Log($"[FireflyDance] Verbose logs: {(_enableVerboseLogs ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Active/désactive une catégorie spécifique
    /// </summary>
    public static void ToggleCategory(LogCategory category)
    {
        switch (category)
        {
            case LogCategory.Tracking:
                _enableTrackingLogs = !_enableTrackingLogs;
                Debug.Log($"[FireflyDance] Tracking logs: {(_enableTrackingLogs ? "ON" : "OFF")}");
                break;
            case LogCategory.Spawn:
                _enableSpawnLogs = !_enableSpawnLogs;
                Debug.Log($"[FireflyDance] Spawn logs: {(_enableSpawnLogs ? "ON" : "OFF")}");
                break;
            case LogCategory.Capture:
                _enableCaptureLogs = !_enableCaptureLogs;
                Debug.Log($"[FireflyDance] Capture logs: {(_enableCaptureLogs ? "ON" : "OFF")}");
                break;
            case LogCategory.Score:
                _enableScoreLogs = !_enableScoreLogs;
                Debug.Log($"[FireflyDance] Score logs: {(_enableScoreLogs ? "ON" : "OFF")}");
                break;
            case LogCategory.State:
                _enableStateLogs = !_enableStateLogs;
                Debug.Log($"[FireflyDance] State logs: {(_enableStateLogs ? "ON" : "OFF")}");
                break;
            case LogCategory.Events:
                _enableEventLogs = !_enableEventLogs;
                Debug.Log($"[FireflyDance] Event logs: {(_enableEventLogs ? "ON" : "OFF")}");
                break;
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Affiche l'état actuel de tous les logs
    /// </summary>
    public static void LogCurrentSettings()
    {
        Debug.Log($"[FireflyDance] État des logs - Debug: {_enableDebugLogs}, Verbose: {_enableVerboseLogs}, Tracking: {_enableTrackingLogs}, Spawn: {_enableSpawnLogs}, Capture: {_enableCaptureLogs}, Score: {_enableScoreLogs}, State: {_enableStateLogs}, Events: {_enableEventLogs}");
    }

    /// <summary>
    /// Remet tous les paramètres de log aux valeurs par défaut
    /// </summary>
    public static void ResetToDefaults()
    {
        _enableDebugLogs = true;
        _enableVerboseLogs = false;
        _enableTrackingLogs = false;
        _enableSpawnLogs = true;
        _enableCaptureLogs = true;
        _enableScoreLogs = true;
        _enableStateLogs = true;
        _enableEventLogs = false;
        
        Debug.Log("[FireflyDance] Paramètres de log remis aux valeurs par défaut");
    }

    /// <summary>
    /// Désactive tous les logs sauf les erreurs (mode silencieux)
    /// </summary>
    public static void SetSilentMode(bool silent)
    {
        if (silent)
        {
            _enableDebugLogs = false;
            _enableVerboseLogs = false;
            _enableTrackingLogs = false;
            _enableSpawnLogs = false;
            _enableCaptureLogs = false;
            _enableScoreLogs = false;
            _enableStateLogs = false;
            _enableEventLogs = false;
            Debug.Log("[FireflyDance] Mode silencieux activé - seules les erreurs seront affichées");
        }
        else
        {
            ResetToDefaults();
        }
    }

    #endregion
}
}
