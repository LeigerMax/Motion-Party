using UnityEngine;

/// <summary>
/// Système de logging centralisé pour LogParade.
/// Permet de contrôler globalement l'affichage des logs de debug.
/// </summary>
public static class LogParadeLogger
{
    #region Fields
    private static bool _enableDebugLogs = true;
    private static bool _enableVerboseLogs = false;
    #endregion

    #region Properties
    /// <summary>
    /// Active/désactive tous les logs LogParade
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
    #endregion

    #region Logging Methods
    /// <summary>
    /// Log normal avec tag [LogParade]
    /// </summary>
    public static void Log(string message, Object context = null)
    {
        if (_enableDebugLogs)
        {
            Debug.Log($"[LogParade] {message}", context);
        }
    }
    
    /// <summary>
    /// Log verbeux (fréquent) avec tag [LogParade-V]
    /// </summary>
    public static void LogVerbose(string message, Object context = null)
    {
        if (_enableVerboseLogs)
        {
            Debug.Log($"[LogParade-V] {message}", context);
        }
    }
    
    /// <summary>
    /// Warning avec tag [LogParade]
    /// </summary>
    public static void LogWarning(string message, Object context = null)
    {
        if (_enableDebugLogs)
        {
            Debug.LogWarning($"[LogParade] {message}", context);
        }
    }
    
    /// <summary>
    /// Error avec tag [LogParade] (toujours affiché)
    /// </summary>
    public static void LogError(string message, Object context = null)
    {
        Debug.LogError($"[LogParade] {message}", context);
    }
    #endregion

    #region Toggle Methods
    /// <summary>
    /// Toggle des logs via input (F1 par défaut)
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        Application.logMessageReceived += OnLogReceived;
    }
    
    private static void OnLogReceived(string logString, string stackTrace, LogType type)
    {
        // Écouter les commandes de toggle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugLogs();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleVerboseLogs();
        }
    }
    
    public static void ToggleDebugLogs()
    {
        _enableDebugLogs = !_enableDebugLogs;
        Debug.Log($"[LogParade] Debug logs: {(_enableDebugLogs ? "ENABLED" : "DISABLED")} (F1 to toggle)");
    }
    
    public static void ToggleVerboseLogs()
    {
        _enableVerboseLogs = !_enableVerboseLogs;
        Debug.Log($"[LogParade] Verbose logs: {(_enableVerboseLogs ? "ENABLED" : "DISABLED")} (F2 to toggle)");
    }
    #endregion
}
