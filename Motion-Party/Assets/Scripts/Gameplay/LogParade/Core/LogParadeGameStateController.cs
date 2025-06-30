using UnityEngine;

/// <summary>
/// Contrôleur centralisé de l'état global du mini-jeu LogParade.
/// Gère les verrous de calibration et les autorisations de gameplay/scoring.
/// </summary>
public class LogParadeGameStateController : MonoBehaviour
{
#region Champs & Singleton
    [Header("State Settings")]
    [SerializeField] private bool showDetailedLogs = true;
    /// <summary>
    /// Indique si la calibration est actuellement en cours
    /// </summary>
    public static bool IsCalibrationInProgress { get; private set; } = false;
    
    /// <summary>
    /// Indique si le gameplay est autorisé (après calibration)
    /// </summary>
    public static bool IsGameplayAllowed { get; private set; } = false;
    
    /// <summary>
    /// Indique si le jeu principal a démarré
    /// </summary>
    public static bool IsGameStarted { get; private set; } = false;
    
    // Instance singleton pour accès facilitée
    private static LogParadeGameStateController instance;
    public static LogParadeGameStateController Instance => instance;
#endregion

#region Unity Lifecycle
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeState();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
#endregion

#region Initialisation
    /// <summary>
    /// Initialise l'état par défaut du jeu
    /// </summary>
    private void InitializeState()
    {
        IsCalibrationInProgress = false;
        IsGameplayAllowed = false;
        IsGameStarted = false;
        if (showDetailedLogs)
        {
            LogParadeLogger.LogVerbose("État initialisé - Calibration et gameplay désactivés");
        }
    }
#endregion

#region Calibration State Management
    /// <summary>
    /// Démarre la phase de calibration
    /// </summary>
    public static void StartCalibration()
    {
        IsCalibrationInProgress = true;
        IsGameplayAllowed = false;
        IsGameStarted = false;
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.LogVerbose("Calibration démarrée - Gameplay bloqué");
        }
    }
    
    /// <summary>
    /// Termine la phase de calibration avec succès
    /// </summary>
    public static void CompleteCalibration()
    {
        IsCalibrationInProgress = false;
        IsGameplayAllowed = true;
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.Log("Calibration terminée - Gameplay autorisé");
        }
    }
    
    /// <summary>
    /// Redémarre la calibration (en cas d'échec)
    /// </summary>
    public static void RestartCalibration()
    {
        IsCalibrationInProgress = true;
        IsGameplayAllowed = false;
        IsGameStarted = false;
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.LogVerbose("Calibration redémarrée");
        }
    }
#endregion

#region Game State Management
    /// <summary>
    /// Démarre le jeu principal
    /// </summary>
    public static void StartGame()
    {
        if (!IsGameplayAllowed)
        {
            LogParadeLogger.LogWarning("Tentative de démarrage du jeu avant autorisation (calibration non terminée)");
            return;
        }
        IsGameStarted = true;
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.Log("Jeu principal démarré");
        }
    }
    
    /// <summary>
    /// Arrête le jeu (fin de partie)
    /// </summary>
    public static void StopGame()
    {
        IsGameStarted = false;
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.Log("Jeu arrêté");
        }
    }
    
    /// <summary>
    /// Reset complet de l'état (pour redémarrage)
    /// </summary>
    public static void ResetGameState()
    {
        IsCalibrationInProgress = false;
        IsGameplayAllowed = false;
        IsGameStarted = false;
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.LogVerbose("État du jeu réinitialisé");
        }
    }
    
    /// <summary>
    /// Redémarre le jeu complet (reset + restart)
    /// </summary>
    public static void RestartGame()
    {
        ResetGameState();
        if (instance?.showDetailedLogs == true)
        {
            LogParadeLogger.LogVerbose("Jeu redémarré - état réinitialisé");
        }
    }
#endregion

#region State Queries
    /// <summary>
    /// Vérifie si le scoring peut être démarré (après calibration)
    /// </summary>
    public static bool CanStartScoring()
    {
        bool canStart = IsGameplayAllowed && !IsCalibrationInProgress;
        if (instance?.showDetailedLogs == true && !canStart)
        {
            string reason = IsCalibrationInProgress ? "calibration en cours" : "gameplay non autorisé";
            LogParadeLogger.LogVerbose($"Score bloqué - {reason}");
        }
        return canStart;
    }
    
    /// <summary>
    /// Vérifie si le gameplay peut être démarré (après calibration)
    /// </summary>
    public static bool CanStartGameplay()
    {
        bool canStart = IsGameplayAllowed && !IsCalibrationInProgress;
        if (instance?.showDetailedLogs == true && !canStart)
        {
            string reason = IsCalibrationInProgress ? "calibration en cours" : "gameplay non autorisé";
            LogParadeLogger.LogVerbose($"Gameplay bloqué - {reason}");
        }
        return canStart;
    }
    
    /// <summary>
    /// Obtient le statut actuel sous forme de texte
    /// </summary>
    public static string GetCurrentStatusText()
    {
        if (IsCalibrationInProgress)
            return "Calibration en cours";
        else if (!IsGameplayAllowed)
            return "En attente de calibration";
        else if (!IsGameStarted)
            return "Calibration terminée, prêt à démarrer";
        else
            return "Jeu en cours";
    }
#endregion

#region Debug Methods
    /// <summary>
    /// Force l'activation du gameplay (pour debug uniquement)
    /// </summary>
    [System.Obsolete("Utiliser uniquement pour les tests")]
    public static void ForceEnableGameplay()
    {
        IsCalibrationInProgress = false;
        IsGameplayAllowed = true;
        LogParadeLogger.LogWarning("GAMEPLAY FORCÉ - Pour debug uniquement!");
    }
    
    /// <summary>
    /// Affiche l'état actuel dans la console
    /// </summary>
    [ContextMenu("Debug State")]
    public void DebugCurrentState()
    {
        LogParadeLogger.Log($"État actuel:\n" +
                  $"- Calibration en cours: {IsCalibrationInProgress}\n" +
                  $"- Gameplay autorisé: {IsGameplayAllowed}\n" +
                  $"- Jeu démarré: {IsGameStarted}\n" +
                  $"- Peut scorer: {CanStartScoring()}\n" +
                  $"- Peut jouer: {CanStartGameplay()}\n" +
                  $"- Statut: {GetCurrentStatusText()}");
    }
#endregion
}
