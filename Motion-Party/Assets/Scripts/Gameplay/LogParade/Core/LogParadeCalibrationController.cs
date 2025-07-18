using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôleur amélioré pour la séquence complète de calibration et lancement du jeu LogParade.
/// Assure une coordination parfaite entre calibration, timeout, redémarrage et lancement du jeu.
/// </summary>
public class LogParadeCalibrationController : MonoBehaviour
{
#region Champs
    [Header("Références Système")]
    [SerializeField] private LogParadeCalibrationManager calibrationManager;
    [SerializeField] private LogParadeCalibrationInteractive calibrationInteractive;
    [SerializeField] private LogParadeGameLauncher gameLauncher;

    [Header("Paramètres")]
    [SerializeField] private bool autoStartCalibration = true;
    [SerializeField] private float delayBeforeGameLaunch = 2f;
    [SerializeField] private float delayBeforeRestart = 3f;
    [SerializeField] private int maxCalibrationAttempts = 3;

    [Header("Debug")]
    [SerializeField] private bool enableDetailedLogs = true;
    private int currentAttempt = 0;
    private bool isProcessingCalibration = false;
#endregion

#region Unity Lifecycle
    void Start()
    {
        InitializeReferences();
        SetupEventHandlers();
        if (autoStartCalibration)
        {
            StartCoroutine(StartCalibrationDelayed());
        }
    }
    void OnDestroy()
    {
        CleanupEventHandlers();
    }
#endregion

#region Initialisation
    /// <summary>
    /// Initialise automatiquement les références si elles ne sont pas assignées
    /// </summary>
    private void InitializeReferences()
    {
        if (calibrationManager == null)
            calibrationManager = FindFirstObjectByType<LogParadeCalibrationManager>();
        if (calibrationInteractive == null)
            calibrationInteractive = FindFirstObjectByType<LogParadeCalibrationInteractive>();
        if (gameLauncher == null)
            gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
        if (calibrationManager == null)
            LogParadeLogger.LogError("LogParadeCalibrationManager manquant!");
        if (calibrationInteractive == null)
            LogParadeLogger.LogError("LogParadeCalibrationInteractive manquant!");
        if (gameLauncher == null)
            LogParadeLogger.LogError("LogParadeGameLauncher manquant!");
    }

    /// <summary>
    /// Configure les gestionnaires d'événements pour coordonner calibration et lancement
    /// </summary>
    private void SetupEventHandlers()
    {
        // Événements de calibration
        LogParadeCalibrationInteractive.OnCalibrationCompleted += OnCalibrationSucceeded;
        LogParadeCalibrationInteractive.OnCalibrationFailed += OnCalibrationFailed;
        if (enableDetailedLogs)
        {
            LogParadeCalibrationInteractive.OnLaneReached += OnLaneReached;
        }
    }

    /// <summary>
    /// Nettoie les gestionnaires d'événements
    /// </summary>
    private void CleanupEventHandlers()
    {
        LogParadeCalibrationInteractive.OnCalibrationCompleted -= OnCalibrationSucceeded;
        LogParadeCalibrationInteractive.OnCalibrationFailed -= OnCalibrationFailed;
        if (enableDetailedLogs)
        {
            LogParadeCalibrationInteractive.OnLaneReached -= OnLaneReached;
        }
    }
#endregion

#region Calibration Process
    /// <summary>
    /// Démarre la calibration avec un petit délai
    /// </summary>
    private IEnumerator StartCalibrationDelayed()
    {
        yield return new WaitForSeconds(1f);
        StartCalibrationProcess();
    }

    /// <summary>
    /// Démarre le processus de calibration
    /// </summary>
    public void StartCalibrationProcess()
    {
        if (isProcessingCalibration)
        {
            LogParadeLogger.LogWarning("Calibration déjà en cours de traitement");
            return;
        }
        isProcessingCalibration = true;
        currentAttempt++;
        if (calibrationManager != null)
        {
            calibrationManager.StartCalibrationProcess();
        }
        else if (calibrationInteractive != null)
        {
            calibrationInteractive.StartCalibration();
        }
        else
        {
            LogParadeLogger.LogError("Aucun système de calibration disponible!");
            isProcessingCalibration = false;
        }
    }

    /// <summary>
    /// Appelé quand la calibration réussit
    /// </summary>
    private void OnCalibrationSucceeded()
    {
        if (!isProcessingCalibration) return;
        currentAttempt = 0;
        StartCoroutine(LaunchGameAfterDelay());
    }

    /// <summary>
    /// Appelé quand la calibration échoue
    /// </summary>
    private void OnCalibrationFailed()
    {
        if (!isProcessingCalibration) return;
        LogParadeLogger.LogWarning($"Calibration échouée (tentative {currentAttempt}/{maxCalibrationAttempts})");
        if (currentAttempt < maxCalibrationAttempts)
        {
            StartCoroutine(RestartCalibrationAfterDelay());
        }
        else
        {
            LogParadeLogger.LogError($"Calibration échouée après {maxCalibrationAttempts} tentatives");
            HandleMaxAttemptsReached();
        }
    }

    /// <summary>
    /// Appelé lors de l'atteinte d'une lane (pour debug)
    /// </summary>
    private void OnLaneReached(int laneNumber)
    {
        if (enableDetailedLogs)
        {
            LogParadeLogger.Log($"Lane {laneNumber} atteinte pendant la calibration");
        }
    }

    /// <summary>
    /// Lance le jeu après un délai
    /// </summary>
    private IEnumerator LaunchGameAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeGameLaunch);
        isProcessingCalibration = false;
        if (gameLauncher != null)
        {
            gameLauncher.LaunchFullGame();
        }
        else
        {
            LogParadeLogger.LogError("GameLauncher non disponible pour lancer le jeu!");
        }
    }

    /// <summary>
    /// Redémarre la calibration après un délai
    /// </summary>
    private IEnumerator RestartCalibrationAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeRestart);
        LogParadeLogger.Log("Redémarrage automatique de la calibration...");
        isProcessingCalibration = false;
        StartCalibrationProcess();
    }

    /// <summary>
    /// Gère le cas où le nombre max de tentatives est atteint
    /// </summary>
    private void HandleMaxAttemptsReached()
    {
        isProcessingCalibration = false;
        LogParadeLogger.LogError($" Calibration impossible après {maxCalibrationAttempts} tentatives");
        StartCoroutine(FinalRestartAttempt());
    }

    /// <summary>
    /// Tentative finale de redémarrage avec délai plus long
    /// </summary>
    private IEnumerator FinalRestartAttempt()
    {
        LogParadeLogger.Log("Tentative finale de calibration dans 10 secondes...");
        yield return new WaitForSeconds(10f);
        currentAttempt = 0;
        StartCalibrationProcess();
    }
#endregion

#region Debug/ContextMenu
    /// <summary>
    /// Force le lancement du jeu sans calibration (pour debug)
    /// </summary>
    [ContextMenu("Force Launch Game")]
    public void ForceLaunchGame()
    {
        LogParadeLogger.Log("Lancement forcé du jeu (sans calibration)");
        isProcessingCalibration = false;
        if (gameLauncher != null)
        {
            gameLauncher.LaunchFullGame();
        }
        else
        {
            LogParadeLogger.LogError(" GameLauncher non disponible!");
        }
    }

#endregion
}
