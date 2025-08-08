using UnityEngine;
using System.Collections;

#region Description
/// <summary>
/// Gestionnaire centralisé pour coordonner le démarrage de tous les systèmes LogParade
/// après la calibration. Assure le bon ordre d'initialisation et le démarrage synchronisé.
/// Utilise LogParadeGameStateController pour la gestion d'état centralisée.
/// </summary>
#endregion
public class LogParadeGameLauncher : MonoBehaviour
{
    #region Champs & Références
    [Header("Références des Systèmes")]
    [SerializeField] private LogParadeGameController gameController;
    [SerializeField] private LogParadeGameTimer gameTimer;
    [SerializeField] private LogParadeLogGenerator logGenerator;
    [SerializeField] private LogParadeScoreManager scoreManager;
    [SerializeField] private LogParadeUIManager uiManager;
    [Header("Paramètres")]
    [SerializeField] private float delayAfterCalibration = 1.5f;
    [SerializeField] private bool autoFindComponents = true;
    [SerializeField] private bool enableDetailedLogs = true;
    [SerializeField] private bool autoStartCalibrationOnStart = true;
    // État
    private bool isLaunching = false;
    // Événements
    public System.Action OnGameLaunchStarted;
    public System.Action OnGameLaunchCompleted;
    public System.Action OnGameLaunchFailed;
    #endregion

    #region Initialisation
    void Awake()
    {
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
    }
    void Start()
    {
        if (autoStartCalibrationOnStart)
        {
            StartCoroutine(AutoStartCalibrationCoroutine());
        }
    }
    #endregion

    #region Lancement Calibration & Jeu
    private IEnumerator AutoStartCalibrationCoroutine()
    {
        // Si un LoadingScreenManager est présent et actif, attendre qu'il se termine
        if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsShowing)
        {
            LogStatus("LoadingScreenManager détecté et actif - Attente de la fin de l'écran de chargement...");
            
            // Attendre que l'écran de chargement se termine
            yield return new WaitUntil(() => LoadingScreenManager.Instance == null || !LoadingScreenManager.Instance.IsShowing);
            
            // Attendre encore un peu pour être sûr que tout est initialisé
            yield return new WaitForSeconds(1f);
            
            LogStatus("Écran de chargement terminé - Vérification de la calibration automatique");
        }
        else
        {
            // Comportement original si pas d'écran de chargement
            yield return new WaitForSeconds(2f);
        }
        
        if (!LogParadeGameStateController.IsGameStarted && !LogParadeGameStateController.IsCalibrationInProgress)
        {
            LogStatus("Lancement automatique de la calibration via StartCalibrationProcess()");
            StartCalibrationProcess();
        }
        else
        {
            LogStatus($"Démarrage automatique annulé - État: {LogParadeGameStateController.GetCurrentStatusText()}");
        }
    }
    public void ForceStartCalibrationNow()
    {
        StopAllCoroutines();
        if (LogParadeGameStateController.IsGameStarted)
        {
            LogStatus("Reset du jeu en cours...");
            LogParadeGameStateController.ResetGameState();
            isLaunching = false;
        }
        StartCalibrationProcess();
    }
    public void StartCalibrationProcess()
    {
        if (LogParadeGameStateController.IsGameStarted)
        {
            LogWarning("Jeu déjà démarré - redémarrage nécessaire pour recalibrer");
            RestartGame();
            return;
        }
        var calibrationManager = FindFirstObjectByType<LogParadeCalibrationManager>();
        if (calibrationManager != null)
        {
            LogStatus(" CalibrationManager trouvé, démarrage de la calibration");
            calibrationManager.StartCalibrationProcess();
        }
        else
        {
            LogError(" CalibrationManager introuvable - impossible de démarrer la calibration");
        }
    }
    public void LaunchFullGame()
    {
        if (!LogParadeGameStateController.CanStartGameplay())
        {
            LogWarning($"Impossible de démarrer - État: {LogParadeGameStateController.GetCurrentStatusText()}");
            return;
        }
        if (isLaunching)
        {
            LogWarning("Lancement déjà en cours");
            return;
        }
        if (LogParadeGameStateController.IsGameStarted)
        {
            LogWarning("Jeu déjà démarré");
            return;
        }
        StartCoroutine(LaunchGameSequence());
    }
    private IEnumerator LaunchGameSequence()
    {
        isLaunching = true;
        OnGameLaunchStarted?.Invoke();
        LogParadeEventCoordinator.TriggerGameLaunchStarted();
        LogParadeGameStateController.StartGame();
        if (!ValidateAllComponents())
        {
            LogError("Validation des composants échouée");
            HandleLaunchFailure();
            yield break;
        }
        yield return new WaitForSeconds(delayAfterCalibration);
        yield return StartCoroutine(StartAllSystems());
        yield return StartCoroutine(FinalizeGameStart());
        isLaunching = false;
        OnGameLaunchCompleted?.Invoke();
        LogParadeEventCoordinator.TriggerGameLaunchCompleted();
    }
    private void HandleLaunchFailure()
    {
        isLaunching = false;
        OnGameLaunchFailed?.Invoke();
        LogParadeEventCoordinator.TriggerGameLaunchFailed();
    }
    #endregion

    #region Démarrage des Systèmes
    private IEnumerator StartAllSystems()
    {
        if (gameController != null)
        {
            TryStartGameController();
            yield return new WaitForSeconds(0.2f);
        }
        if (logGenerator != null)
        {
            TryStartLogGenerator();
            yield return new WaitForSeconds(0.2f);
        }
        if (gameTimer != null)
        {
            TryStartGameTimer();
            yield return new WaitForSeconds(0.2f);
        }
        if (scoreManager != null)
        {
            TryStartScoring();
            yield return new WaitForSeconds(0.2f);
        }

    }
    private IEnumerator FinalizeGameStart()
    {
        if (uiManager != null)
        {
            var initUIMethod = uiManager.GetType().GetMethod("InitializeGameUI");
            if (initUIMethod != null)
            {
                try
                {
                    initUIMethod.Invoke(uiManager, null);
                }
                catch (System.Exception ex)
                {
                    LogWarning($"Erreur init UI: {ex.Message}");
                }
            }
        }
        yield return new WaitForSeconds(0.1f);
    }
    #endregion

    #region Méthodes TryStart (Démarrage Composants)
    private void TryStartGameController()
    {
        try
        {
            var forceStartMethod = gameController.GetType().GetMethod("ForceStartGame");
            if (forceStartMethod != null)
            {
                forceStartMethod.Invoke(gameController, null);

                return;
            }
            gameController.gameObject.SetActive(true);
        }
        catch (System.Exception ex)
        {
            LogError($" Erreur GameController: {ex.Message}");
        }
    }
    private void TryStartLogGenerator()
    {
        try
        {
            var enableField = logGenerator.GetType().GetField("enableGeneration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enableField != null)
            {
                enableField.SetValue(logGenerator, true);
            }
            var startMethod = logGenerator.GetType().GetMethod("StartGeneration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null)
            {
                startMethod.Invoke(logGenerator, null);
                return;
            }
            var launchMethod = logGenerator.GetType().GetMethod("Launch");
            if (launchMethod != null)
            {
                launchMethod.Invoke(logGenerator, null);
                return;
            }
            logGenerator.gameObject.SetActive(true);
        }
        catch (System.Exception ex)
        {
            LogError($" Erreur LogGenerator: {ex.Message}");
        }
    }
    private void TryStartGameTimer()
    {
        try
        {
            var launchLevelMethod = gameTimer.GetType().GetMethod("LaunchLevel");
            if (launchLevelMethod != null)
            {
                launchLevelMethod.Invoke(gameTimer, null);
                return;
            }
            var launchMethod = gameTimer.GetType().GetMethod("Launch");
            if (launchMethod != null)
            {
                launchMethod.Invoke(gameTimer, null);
                return;
            }
            gameTimer.gameObject.SetActive(true);
        }
        catch (System.Exception ex)
        {
            LogError($"  Erreur GameTimer: {ex.Message}");
        }
    }

    private void TryStartScoring()
    {
        try
        {
            var startScoringMethod = scoreManager.GetType().GetMethod("StartScoring");
            if (startScoringMethod != null)
            {
                startScoringMethod.Invoke(scoreManager, null);
            }
            else
            {
                scoreManager.gameObject.SetActive(true);
            }
        }
        catch (System.Exception ex)
        {
            LogError($" Erreur ScoreManager: {ex.Message}");
        }
    }
    #endregion

    #region Validation & AutoFind
    private bool ValidateAllComponents()
    {
        bool isValid = true;
        if (gameController == null)
        {
            LogError("GameController manquant");
            isValid = false;
        }
        if (gameTimer == null)
        {
            LogError("GameTimer manquant");
            isValid = false;
        }
        if (logGenerator == null)
        {
            LogError("LogGenerator manquant");
            isValid = false;
        }
        if (scoreManager == null)
        {
            LogError("ScoreManager manquant");
            isValid = false;
        }
        if (isValid)
        {
            LogStatus("Tous les composants requis sont présents");
        }
        return isValid;
    }
    private void AutoFindComponents()
    {
        var validator = LogParadeSystemValidator.Instance;
        if (validator != null)
        {
            if (gameController == null)
                gameController = validator.GetValidatedComponent<LogParadeGameController>();
            if (gameTimer == null)
                gameTimer = validator.GetValidatedComponent<LogParadeGameTimer>();
            if (logGenerator == null)
                logGenerator = validator.GetValidatedComponent<LogParadeLogGenerator>();
            if (scoreManager == null)
                scoreManager = validator.GetValidatedComponent<LogParadeScoreManager>();
            if (uiManager == null)
                uiManager = validator.GetValidatedComponent<LogParadeUIManager>();
        }
        else
        {
            LogWarning("SystemValidator non disponible, utilisation de FindObjectOfType en fallback");
            if (gameController == null)
                gameController = FindFirstObjectByType<LogParadeGameController>();
            if (gameTimer == null)
                gameTimer = FindFirstObjectByType<LogParadeGameTimer>();
            if (logGenerator == null)
                logGenerator = FindFirstObjectByType<LogParadeLogGenerator>();
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<LogParadeScoreManager>();
            if (uiManager == null)
                uiManager = FindFirstObjectByType<LogParadeUIManager>();
        }

    }
    #endregion

    #region Restart & Processus Complet
    public void RestartGame()
    {
        LogStatus("Redémarrage du jeu...");
        LogParadeGameStateController.RestartGame();
        StartCoroutine(RestartGameCoroutine());
    }

    private IEnumerator RestartGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        LaunchFullGame();
    }

    public void StartCompleteGameProcess()
    {
        if (LogParadeGameStateController.IsGameStarted)
        {
            RestartGame();
            return;
        }
        if (LogParadeGameStateController.IsCalibrationInProgress)
        {
            return;
        }
        if (LogParadeGameStateController.CanStartGameplay())
        {
            LaunchFullGame();
            return;
        }
        StartCalibrationProcess();
    }
    #endregion

    #region Logging
    private void LogStatus(string message)
    {
        if (enableDetailedLogs)
            LogParadeLogger.Log($"[GameLauncher] {message}");
    }

    private void LogWarning(string message)
    {
        LogParadeLogger.LogWarning($"[GameLauncher] {message}");
    }

    private void LogError(string message)
    {
        LogParadeLogger.LogError($"[GameLauncher] {message}");
    }
    #endregion

    #region Debug Methods

    [ContextMenu("Debug - Force Launch Game")]
    public void DebugForceLaunchGame()
    {
        LogParadeLogger.LogWarning(" DEBUG: Forçage du lancement du jeu (bypass calibration)");
        LogParadeGameStateController.ForceEnableGameplay();
        LaunchFullGame();
    }

    #endregion
}
