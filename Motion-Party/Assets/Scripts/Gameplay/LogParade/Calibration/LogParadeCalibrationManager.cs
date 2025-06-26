using UnityEngine;
using System.Collections;

/// <summary>
/// Gestionnaire de calibration LogParade.
/// Se concentre uniquement sur l'orchestration de la calibration et la communication
/// avec le système de calibration interactif. Délègue la gestion d'état au GameStateController
/// et le démarrage du jeu au GameLauncher.
/// </summary>
public class LogParadeCalibrationManager : MonoBehaviour
{    [Header("Calibration Settings")]
    [SerializeField] private bool enableCalibrationOnStart = false; // Toujours false, le lancement auto est géré par le GameLauncher
    
    [Header("References")]
    [SerializeField] private LogParadeCalibrationInteractive calibrationSystem;
    [SerializeField] private LogParadePlayerAvatar playerAvatar;
    [SerializeField] private LogParadeGameLauncher gameLauncher;
    
    [Header("Auto-Assignment")]
    [SerializeField] private bool autoFindReferences = true;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDetailedLogs = true;
    
    // État local de la calibration
    private bool isCalibrating = false;
    // Flag statique pour empêcher le double lancement global
    private static bool calibrationHasStarted = false;
      // Événements
    public System.Action OnCalibrationStarted;
    public System.Action OnCalibrationSucceeded;
    public System.Action OnCalibrationFailedEvent;

    void Awake()
    {
        // Toujours désactiver le lancement auto ici, centralisé dans le GameLauncher
        enableCalibrationOnStart = false;
        if (autoFindReferences)
        {
            AutoAssignReferences();
        }
    }

    void Start()
    {
        // S'abonner aux événements de calibration
        if (calibrationSystem != null)
        {
            LogParadeCalibrationInteractive.OnCalibrationCompleted += OnCalibrationCompleted;
            LogParadeCalibrationInteractive.OnCalibrationFailed += OnCalibrationFailed;
            LogParadeCalibrationInteractive.OnLaneReached += OnLaneReached;
        }
        
        // Démarrer le processus si requis
        if (enableCalibrationOnStart)
        {
            StartCalibrationProcess();
        }
    }

    void OnDestroy()
    {
        // Se désabonner des événements
        if (calibrationSystem != null)
        {
            LogParadeCalibrationInteractive.OnCalibrationCompleted -= OnCalibrationCompleted;
            LogParadeCalibrationInteractive.OnCalibrationFailed -= OnCalibrationFailed;
            LogParadeCalibrationInteractive.OnLaneReached -= OnLaneReached;
        }
    }

    /// <summary>
    /// Trouve automatiquement les références dans la scène
    /// </summary>
    private void AutoAssignReferences()
    {
        // Recherche directe des composants dans la scène
        if (calibrationSystem == null)
            calibrationSystem = FindFirstObjectByType<LogParadeCalibrationInteractive>();

        if (playerAvatar == null)
            playerAvatar = FindFirstObjectByType<LogParadePlayerAvatar>();

        if (gameLauncher == null)
            gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();

        LogStatus($"Auto-assignment: Calibration={calibrationSystem != null}, Player={playerAvatar != null}, Launcher={gameLauncher != null}");
    }

    /// <summary>
    /// Démarre le processus de calibration
    /// </summary>
    public void StartCalibrationProcess()
    {
        if (isCalibrating || calibrationHasStarted)
        {
            LogWarning("Calibration déjà en cours ou déjà lancée");
            return;
        }
        calibrationHasStarted = true;
        // Informer que la calibration démarre
        LogStatus("🎯 Démarrage de la calibration...");
        
        // Informer le GameStateController que la calibration démarre
        //LogParadeGameStateController.StartCalibration();
        
        // Valider les composants requis
        if (calibrationSystem == null)
        {
            LogError("Système de calibration non trouvé! Bypass de la calibration.");
            CompleteCalibration();
            return;
        }

        // Démarrer la calibration
        isCalibrating = true;
        OnCalibrationStarted?.Invoke();
        
        // Préparer le joueur
        PreparePlayerForCalibration();
        
        // Lancer le système de calibration
        calibrationSystem.StartCalibration();
        
        LogStatus("🎯 Processus de calibration démarré");
    }   
    
     /// <summary>
    /// Redémarre la calibration
    /// </summary>
    public void RestartCalibration()
    {
        LogStatus("🔄 Redémarrage de la calibration...");
        calibrationHasStarted = false; // Permet un nouveau lancement

        // Informer le GameStateController du redémarrage
        LogParadeGameStateController.RestartCalibration();

        // Arrêter la calibration actuelle
        if (isCalibrating && calibrationSystem != null)
        {
            calibrationSystem.StopCalibration();
        }

        // Redémarrer après un délai
        StartCoroutine(RestartCalibrationCoroutine());
    }

    private IEnumerator RestartCalibrationCoroutine()
    {
        isCalibrating = false;
        yield return new WaitForSeconds(0.5f);
        StartCalibrationProcess();
    }

    /// <summary>
    /// Prépare le joueur pour la calibration
    /// </summary>
    private void PreparePlayerForCalibration()
    {
        if (playerAvatar != null)
        {
            // S'assurer que le joueur est activé et visible
            playerAvatar.gameObject.SetActive(true);
            
            // Positionner le joueur au centre (lane 2)
            playerAvatar.SetLaneInstant(2);
            
            LogStatus("  → Joueur préparé pour la calibration (lane 2)");
        }
        else
        {
            LogWarning("PlayerAvatar non trouvé pour la préparation");
        }
    }   
    
     /// <summary>
    /// Complète la calibration avec succès
    /// </summary>
    private void CompleteCalibration()
    {
        if (!isCalibrating && calibrationSystem != null)
        {
            LogWarning("Calibration déjà terminée");
            return;
        }
        isCalibrating = false;
        calibrationHasStarted = false; // Permet un nouveau lancement après succès

        // Informer le GameStateController que la calibration est terminée
        LogParadeGameStateController.CompleteCalibration();

        // Nettoyer les rondins de calibration
        if (calibrationSystem != null)
        {
            calibrationSystem.CleanupCalibrationLogs();
            LogStatus("  → Rondins de calibration supprimés");
        }

        // Déclencher l'événement local
        OnCalibrationSucceeded?.Invoke();

        LogStatus("✅ Calibration terminée avec succès");

        // Démarrer le jeu via le GameLauncher
        if (gameLauncher != null)
        {
            LogStatus("  → Lancement du jeu principal...");
            gameLauncher.LaunchFullGame();
        }
        else
        {
            LogError("GameLauncher non trouvé - impossible de démarrer le jeu!");
        }
    }

    #region Événements de calibration

    /// <summary>
    /// Appelé quand la calibration est terminée avec succès
    /// </summary>
    private void OnCalibrationCompleted()
    {
        LogStatus("📍 Calibration interactive terminée");
        CompleteCalibration();
    }

    /// <summary>
    /// Appelé quand la calibration échoue
    /// </summary>
    private void OnCalibrationFailed()
    {
        LogWarning("❌ Calibration échouée - le système va redemander");
        OnCalibrationFailedEvent?.Invoke();
        // Le système de calibration interactif gère automatiquement la relance
    }

    /// <summary>
    /// Appelé quand le joueur atteint une lane pendant la calibration
    /// </summary>
    private void OnLaneReached(int laneNumber)
    {
        LogStatus($"📍 Lane {laneNumber} atteinte pendant la calibration");
        // Feedback simple, l'UI est gérée par l'UIManager
    }

    #endregion

    #region Méthodes d'état public

    /// <summary>
    /// Vérifie si la calibration est en cours
    /// </summary>
    public bool IsCalibrating()
    {
        return isCalibrating;
    }

    /// <summary>
    /// Vérifie si la calibration est disponible
    /// </summary>
    public bool IsCalibrationSystemAvailable()
    {
        return calibrationSystem != null;
    }

    /// <summary>
    /// Force l'arrêt de la calibration (pour debug)
    /// </summary>
    [System.Obsolete("Pour debug uniquement")]
    public void ForceStopCalibration()
    {
        if (isCalibrating)
        {
            isCalibrating = false;
            if (calibrationSystem != null)
            {
                calibrationSystem.StopCalibration();
            }
            LogStatus("⚠️ Calibration arrêtée de force");        }
    }

    #endregion

    #region Méthodes de logging

    private void LogStatus(string message)
    {
        if (showDetailedLogs)
            LogParadeLogger.LogVerbose(message);
    }

    private void LogWarning(string message)
    {
        LogParadeLogger.LogWarning(message);
    }

    private void LogError(string message)
    {
        LogParadeLogger.LogError(message);
    }

    #endregion

    #region Debug Methods    /// <summary>
    /// Debug: affiche l'état actuel de la calibration
    /// </summary>    [ContextMenu("Debug Calibration State")]
    public void DebugCalibrationState()
    {
        LogParadeLogger.Log($"État de la calibration:\n" +
                  $"- En cours: {isCalibrating}\n" +
                  $"- Système disponible: {IsCalibrationSystemAvailable()}\n" +
                  $"- Calibration system: {(calibrationSystem != null ? calibrationSystem.GetCalibrationStatus : "Non trouvé")}\n" +
                  $"- Player Avatar: {(playerAvatar != null ? "Disponible" : "Non trouvé")}\n" +
                  $"- Game Launcher: {(gameLauncher != null ? "Disponible" : "Non trouvé")}");
    }

    #endregion
}
