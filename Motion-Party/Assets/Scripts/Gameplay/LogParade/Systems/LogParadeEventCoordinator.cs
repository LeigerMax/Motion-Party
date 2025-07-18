using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Coordinateur centralisé des événements LogParade.
/// Gère les communications inter-composants et orchestre les séquences d'événements complexes.
/// Découple les composants en fournissant un bus d'événements centralisé.
/// </summary>
public class LogParadeEventCoordinator : MonoBehaviour
{
    #region Fields
    [Header("Event Settings")]
    [SerializeField] private bool enableEventLogging = true;
    [SerializeField] private bool logAllEvents = false;
    [SerializeField] private int maxEventHistorySize = 100;
    // Historique des événements pour debug
    private Queue<EventLogEntry> eventHistory = new Queue<EventLogEntry>();
    #endregion

    #region Singleton
    private static LogParadeEventCoordinator instance;
    public static LogParadeEventCoordinator Instance => instance;
    #endregion

    #region Event Declarations
    // Événements de calibration
    public System.Action OnCalibrationStarted;
    public System.Action OnCalibrationCompleted;
    public System.Action OnCalibrationFailed;
    // Événements de lancement du jeu
    public System.Action OnGameLaunchStarted;
    public System.Action OnGameLaunchCompleted;
    public System.Action OnGameLaunchFailed;
    // Événements de gameplay
    public System.Action OnGameStarted;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    public System.Action OnGameEnded;
    // Événements de score
    public System.Action OnScoringStarted;
    public System.Action OnScoringPaused;
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnFinalScoreCalculated;
    // Événements de système
    public System.Action<bool> OnSystemHealthChanged;
    public System.Action<string> OnSystemError;
    public System.Action OnSystemRestart;
    // Événements UI
    public System.Action<string> OnUIStateChanged;
    public System.Action<string> OnUIMessage;
    // Événements de joueur
    public System.Action<int> OnPlayerLaneChanged;
    public System.Action OnPlayerCollisionDetected;
    #endregion

    #region Event Log Structure
    private struct EventLogEntry
    {
        public float timestamp;
        public string eventName;
        public string details;
        public EventLogEntry(string name, string details = "")
        {
            this.timestamp = Time.time;
            this.eventName = name;
            this.details = details;
        }
        public override string ToString()
        {
            return $"[{timestamp:F2}s] {eventName}{(string.IsNullOrEmpty(details) ? "" : $" - {details}")}";
        }
    }
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventCoordinator();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        LogEvent("EventCoordinator initialisé et prêt");
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialise le coordinateur d'événements
    /// </summary>
    private void InitializeEventCoordinator()
    {
        // S'abonner à ses propres événements pour le logging
        if (enableEventLogging)
        {
            SubscribeToAllEventsForLogging();
        }
        LogEvent("EventCoordinator configuré");
    }
    #endregion

    #region Event Triggers
    // Calibration
    public static void TriggerCalibrationStarted()
    {
        Instance?.OnCalibrationStarted?.Invoke();
        Instance?.LogEvent("CalibrationStarted");
    }
    public static void TriggerCalibrationCompleted()
    {
        Instance?.OnCalibrationCompleted?.Invoke();
        Instance?.LogEvent("CalibrationCompleted");
    }
    public static void TriggerCalibrationFailed()
    {
        Instance?.OnCalibrationFailed?.Invoke();
        Instance?.LogEvent("CalibrationFailed");
    }
    // Lancement du jeu
    public static void TriggerGameLaunchStarted()
    {
        Instance?.OnGameLaunchStarted?.Invoke();
        Instance?.LogEvent("GameLaunchStarted");
    }
    public static void TriggerGameLaunchCompleted()
    {
        Instance?.OnGameLaunchCompleted?.Invoke();
        Instance?.LogEvent("GameLaunchCompleted");
    }
    public static void TriggerGameLaunchFailed()
    {
        Instance?.OnGameLaunchFailed?.Invoke();
        Instance?.LogEvent("GameLaunchFailed");
    }
    // Gameplay
    public static void TriggerGameStarted()
    {
        Instance?.OnGameStarted?.Invoke();
        Instance?.LogEvent("GameStarted");
    }
    public static void TriggerGamePaused()
    {
        Instance?.OnGamePaused?.Invoke();
        Instance?.LogEvent("GamePaused");
    }
    public static void TriggerGameResumed()
    {
        Instance?.OnGameResumed?.Invoke();
        Instance?.LogEvent("GameResumed");
    }
    public static void TriggerGameEnded()
    {
        Instance?.OnGameEnded?.Invoke();
        Instance?.LogEvent("GameEnded");
    }
    // Score
    public static void TriggerScoringStarted()
    {
        Instance?.OnScoringStarted?.Invoke();
        Instance?.LogEvent("ScoringStarted");
    }
    public static void TriggerScoringPaused()
    {
        Instance?.OnScoringPaused?.Invoke();
        Instance?.LogEvent("ScoringPaused");
    }
    public static void TriggerScoreChanged(int newScore)
    {
        Instance?.OnScoreChanged?.Invoke(newScore);
        Instance?.LogEvent("ScoreChanged", $"Score: {newScore}");
    }
    public static void TriggerFinalScoreCalculated(int finalScore)
    {
        Instance?.OnFinalScoreCalculated?.Invoke(finalScore);
        Instance?.LogEvent("FinalScoreCalculated", $"Score final: {finalScore}");
    }
    // Système
    public static void TriggerSystemHealthChanged(bool isHealthy)
    {
        Instance?.OnSystemHealthChanged?.Invoke(isHealthy);
        Instance?.LogEvent("SystemHealthChanged", $"Healthy: {isHealthy}");
    }
    public static void TriggerSystemError(string errorMessage)
    {
        Instance?.OnSystemError?.Invoke(errorMessage);
        Instance?.LogEvent("SystemError", errorMessage);
    }
    public static void TriggerSystemRestart()
    {
        Instance?.OnSystemRestart?.Invoke();
        Instance?.LogEvent("SystemRestart");
    }
    // UI
    public static void TriggerUIStateChanged(string newState)
    {
        Instance?.OnUIStateChanged?.Invoke(newState);
        Instance?.LogEvent("UIStateChanged", $"State: {newState}");
    }
    public static void TriggerUIMessage(string message)
    {
        Instance?.OnUIMessage?.Invoke(message);
        Instance?.LogEvent("UIMessage", message);
    }
    // Joueur
    public static void TriggerPlayerLaneChanged(int newLane)
    {
        Instance?.OnPlayerLaneChanged?.Invoke(newLane);
        Instance?.LogEvent("PlayerLaneChanged", $"Lane: {newLane}");
    }
    public static void TriggerPlayerCollisionDetected()
    {
        Instance?.OnPlayerCollisionDetected?.Invoke();
        Instance?.LogEvent("PlayerCollisionDetected");
    }
    #endregion

    #region Orchestrated Sequences
    /// <summary>
    /// Orchestre la séquence complète calibration → lancement → jeu
    /// </summary>
    public void StartFullGameSequence()
    {
        LogEvent("Séquence complète démarrée");
        // Déclencher la calibration
        TriggerCalibrationStarted();
        // Les autres événements seront déclenchés par les composants respectifs
        // via les événements centralisés
    }
    /// <summary>
    /// Orchestre la séquence de fin de jeu
    /// </summary>
    public void StartGameEndSequence()
    {
        LogEvent("🏁 Séquence de fin de jeu démarrée");
        // Arrêter le scoring
        TriggerScoringPaused();
        // Terminer le jeu
        TriggerGameEnded();
    }
    /// <summary>
    /// Orchestre la séquence de redémarrage
    /// </summary>
    public void StartRestartSequence()
    {
        LogEvent("🔄 Séquence de redémarrage démarrée");
        // Déclencher le redémarrage système
        TriggerSystemRestart();
        // Après un délai, relancer la séquence complète
        StartCoroutine(RestartSequenceCoroutine());
    }
    #endregion

    #region Private Coroutines
    private System.Collections.IEnumerator RestartSequenceCoroutine()
    {
        yield return new WaitForSeconds(1f);
        StartFullGameSequence();
    }
    private System.Collections.IEnumerator TestSequenceCoroutine()
    {
        LogParadeLogger.LogVerbose("---Séquence de test démarrée---");
        TriggerCalibrationStarted();
        yield return new WaitForSeconds(1f);
        TriggerCalibrationCompleted();
        yield return new WaitForSeconds(0.5f);
        TriggerGameLaunchStarted();
        yield return new WaitForSeconds(1f);
        TriggerGameLaunchCompleted();
        yield return new WaitForSeconds(0.5f);
        TriggerGameStarted();
        yield return new WaitForSeconds(2f);
        TriggerScoreChanged(100);
        yield return new WaitForSeconds(1f);
        TriggerGameEnded();
        yield return new WaitForSeconds(0.5f);
        TriggerFinalScoreCalculated(100);
        LogParadeLogger.LogVerbose("--- Séquence de test terminée ---");
    }
    #endregion

    #region Logging & Debugging
    /// <summary>
    /// S'abonne à tous les événements pour le logging
    /// </summary>
    private void SubscribeToAllEventsForLogging()
    {
        // Les événements se logguent déjà automatiquement via les méthodes Trigger
        // Cette méthode peut être étendue pour des logs plus avancés
    }
    /// <summary>
    /// Enregistre un événement dans l'historique
    /// </summary>
    private void LogEvent(string eventName, string details = "")
    {
        // Ajouter à l'historique
        var entry = new EventLogEntry(eventName, details);
        eventHistory.Enqueue(entry);
        // Limiter la taille de l'historique
        while (eventHistory.Count > maxEventHistorySize)
        {
            eventHistory.Dequeue();
        }
        // Logger si activé
        if (enableEventLogging && (logAllEvents || eventName.Contains("Started") || eventName.Contains("Completed") || eventName.Contains("Failed")))
        {
            LogParadeLogger.LogVerbose($"{entry}");
        }
    }
    /// <summary>
    /// Génère un rapport de l'historique des événements
    /// </summary>
    public string GenerateEventHistoryReport()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("--HISTORIQUE DES ÉVÉNEMENTS LOGPARADE--");
        report.AppendLine($"Nombre d'événements: {eventHistory.Count}");
        report.AppendLine($"Logging activé: {enableEventLogging}");
        report.AppendLine();
        foreach (var entry in eventHistory)
        {
            report.AppendLine(entry.ToString());
        }
        return report.ToString();
    }
    /// <summary>
    /// Vide l'historique des événements
    /// </summary>
    public void ClearEventHistory()
    {
        eventHistory.Clear();
        LogEvent("EventHistoryCleared");
    }
    #endregion

    #region Public API
    /// <summary>
    /// Active/désactive le logging des événements
    /// </summary>
    public void SetEventLogging(bool enable)
    {
        enableEventLogging = enable;
        LogEvent($"EventLogging{(enable ? "Enabled" : "Disabled")}");
    }
    /// <summary>
    /// Obtient le nombre d'événements dans l'historique
    /// </summary>
    public int GetEventHistoryCount()
    {
        return eventHistory.Count;
    }
    /// <summary>
    /// Vérifie si un événement spécifique a été déclenché récemment
    /// </summary>
    public bool HasRecentEvent(string eventName, float withinSeconds = 5f)
    {
        float currentTime = Time.time;
        foreach (var entry in eventHistory)
        {
            if (entry.eventName == eventName && (currentTime - entry.timestamp) <= withinSeconds)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Debug Methods
    /// <summary>
    /// Debug: affiche l'historique complet des événements
    /// </summary>
    [ContextMenu("Debug Event History")]
    public void DebugEventHistory()
    {
        LogParadeLogger.Log(GenerateEventHistoryReport());
    }
    /// <summary>
    /// Debug: déclenche une séquence de test
    /// </summary>
    [ContextMenu("Debug Test Sequence")]
    public void DebugTestSequence()
    {
        StartCoroutine(TestSequenceCoroutine());
    }
    #endregion
}
