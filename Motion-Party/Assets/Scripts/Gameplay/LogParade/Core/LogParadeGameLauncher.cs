using UnityEngine;
using System.Collections;

/// <summary>
/// Gestionnaire centralisé pour coordonner le démarrage de tous les systèmes LogParade
/// après la calibration. Assure le bon ordre d'initialisation et le démarrage synchronisé.
/// Utilise LogParadeGameStateController pour la gestion d'état centralisée.
/// </summary>
public class LogParadeGameLauncher : MonoBehaviour
{
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

    void Awake()
    {
        if (autoFindComponents)
        {
            AutoFindComponents();
        }    }
    
    void Start()
    {
        LogStatus("LogParadeGameLauncher initialisé et prêt");
        
        // Démarrage automatique simplifié - force l'utilisation de StartCalibrationProcess
        if (autoStartCalibrationOnStart)
        {
            LogStatus("🎯 Démarrage automatique activé - lancement de la calibration dans 2 secondes...");
            StartCoroutine(AutoStartCalibrationCoroutine());
        }
        else
        {
            LogStatus("ℹ️ Démarrage automatique désactivé");
            LogStatus("💡 Utilisez 'Debug - Force Start Calibration NOW' pour démarrer manuellement");
        }
    }/// <summary>
    /// Coroutine pour démarrer automatiquement la calibration après initialisation
    /// </summary>
    private IEnumerator AutoStartCalibrationCoroutine()
    {
        // Attendre que tous les systèmes soient initialisés
        yield return new WaitForSeconds(2f);
        
        // Vérifier qu'on n'a pas déjà une calibration ou un jeu en cours
        if (!LogParadeGameStateController.IsGameStarted && !LogParadeGameStateController.IsCalibrationInProgress)
        {
            LogStatus("✨ Lancement automatique de la calibration via StartCalibrationProcess()");
            StartCalibrationProcess(); // Utiliser directement la méthode de calibration
        }
        else
        {
            LogStatus($"Démarrage automatique annulé - État: {LogParadeGameStateController.GetCurrentStatusText()}");
        }
    }    /// <summary>
    /// Force le démarrage immédiat de la calibration (pour debug ou UI)
    /// </summary>
    public void ForceStartCalibrationNow()
    {
        LogStatus("🚀 FORÇAGE IMMÉDIAT DE LA CALIBRATION");
        
        // Arrêter toute coroutine en cours
        StopAllCoroutines();
        
        // Reset de l'état si nécessaire
        if (LogParadeGameStateController.IsGameStarted)
        {
            LogStatus("Reset du jeu en cours...");
            LogParadeGameStateController.ResetGameState();
            isLaunching = false;
        }
        
        // Démarrer immédiatement la calibration
        StartCalibrationProcess();
    }

    /// <summary>
    /// Démarre le processus de calibration complet
    /// </summary>
    public void StartCalibrationProcess()
    {
        LogStatus("🎯 Démarrage du processus de calibration...");
        
        // Vérifier qu'on n'est pas déjà en cours de jeu
        if (LogParadeGameStateController.IsGameStarted)
        {
            LogWarning("Jeu déjà démarré - redémarrage nécessaire pour recalibrer");
            RestartGame();
            return;
        }
        
        // Rechercher le CalibrationManager
        var calibrationManager = FindFirstObjectByType<LogParadeCalibrationManager>();
        if (calibrationManager != null)
        {
            LogStatus("  → CalibrationManager trouvé, démarrage de la calibration");
            calibrationManager.StartCalibrationProcess();
        }
        else
        {
            LogError("❌ CalibrationManager introuvable - impossible de démarrer la calibration");
            LogParadeLogger.LogError("💡 SOLUTION: Ajoutez un GameObject avec le composant LogParadeCalibrationManager dans la scène");
        }
    }

    /// <summary>
    /// Démarre le jeu complet après la calibration
    /// </summary>
    public void LaunchFullGame()
    {
        // Vérifier l'état via GameStateController
        if (!LogParadeGameStateController.CanStartGameplay())
        {
            LogWarning($"Impossible de démarrer - État: {LogParadeGameStateController.GetCurrentStatusText()}");
            if (LogParadeGameStateController.IsCalibrationInProgress)
            {
                LogStatus("La calibration est en cours, le jeu démarrera automatiquement une fois terminée");
            }
            else
            {
                LogStatus("💡 Démarrez d'abord la calibration avec StartCalibrationProcess()");
            }
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

    /// <summary>
    /// Séquence coordonnée de démarrage du jeu
    /// </summary>
    private IEnumerator LaunchGameSequence()
    {
        isLaunching = true;
        OnGameLaunchStarted?.Invoke();
        LogParadeEventCoordinator.TriggerGameLaunchStarted();
        
        LogStatus("🚀 DÉMARRAGE DU JEU LOGPARADE");
        
        // Informer GameStateController
        LogParadeGameStateController.StartGame();
        
        // Phase 1: Validation
        LogStatus("Phase 1: Validation des composants...");
        if (!ValidateAllComponents())
        {
            LogError("❌ Validation des composants échouée");
            HandleLaunchFailure();
            yield break;
        }
        
        // Phase 2: Délai
        LogStatus($"Phase 2: Attente de {delayAfterCalibration}s...");
        yield return new WaitForSeconds(delayAfterCalibration);
        
        // Phase 3: Démarrage des systèmes
        LogStatus("Phase 3: Démarrage des systèmes...");
        yield return StartCoroutine(StartAllSystems());
        
        // Phase 4: Finalisation
        LogStatus("Phase 4: Finalisation...");
        yield return StartCoroutine(FinalizeGameStart());
          // Finalisation
        isLaunching = false;
        LogStatus("✅ JEU LOGPARADE ENTIÈREMENT DÉMARRÉ !");
        OnGameLaunchCompleted?.Invoke();
        LogParadeEventCoordinator.TriggerGameLaunchCompleted();
    }

    /// <summary>
    /// Gère l'échec du lancement
    /// </summary>
    private void HandleLaunchFailure()
    {
        isLaunching = false;
        OnGameLaunchFailed?.Invoke();
        LogParadeEventCoordinator.TriggerGameLaunchFailed();
    }

    /// <summary>
    /// Démarre tous les systèmes dans l'ordre correct
    /// </summary>
    private IEnumerator StartAllSystems()
    {
        // Démarrer le contrôleur principal
        if (gameController != null)
        {
            LogStatus("  → Démarrage GameController...");
            TryStartGameController();
            yield return new WaitForSeconds(0.2f);
        }
        
        // Démarrer le générateur de rondins
        if (logGenerator != null)
        {
            LogStatus("  → Démarrage LogGenerator...");
            TryStartLogGenerator();
            yield return new WaitForSeconds(0.2f);
        }
        
        // Démarrer le timer
        if (gameTimer != null)
        {
            LogStatus("  → Démarrage GameTimer...");
            TryStartGameTimer();
            yield return new WaitForSeconds(0.2f);
        }
        
        // Activer le scoring
        if (scoreManager != null)
        {
            LogStatus("  → Activation ScoreManager...");
            TryStartScoring();
            yield return new WaitForSeconds(0.2f);
        }
        
        LogStatus("  ✅ Tous les systèmes démarrés");
    }

    /// <summary>
    /// Finalise le démarrage du jeu
    /// </summary>
    private IEnumerator FinalizeGameStart()
    {
        // Initialiser l'UI si présente
        if (uiManager != null)
        {
            LogStatus("  → Initialisation UI...");
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
        LogStatus("  ✅ Finalisation terminée");
    }

    /// <summary>
    /// Tente de démarrer le contrôleur de jeu
    /// </summary>
    private void TryStartGameController()
    {
        try
        {
            // Méthode privilégiée
            var forceStartMethod = gameController.GetType().GetMethod("ForceStartGame");
            if (forceStartMethod != null)
            {
                forceStartMethod.Invoke(gameController, null);
                LogStatus("    ✅ GameController démarré via ForceStartGame");
                return;
            }
            
            // Fallback
            gameController.gameObject.SetActive(true);
            LogStatus("    ✅ GameController activé");
        }
        catch (System.Exception ex)
        {
            LogError($"    ❌ Erreur GameController: {ex.Message}");
        }
    }

    /// <summary>
    /// Tente de démarrer le générateur de rondins
    /// </summary>
    private void TryStartLogGenerator()
    {
        try
        {
            // Activer la génération
            var enableField = logGenerator.GetType().GetField("enableGeneration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enableField != null)
            {
                enableField.SetValue(logGenerator, true);
            }
            
            // Démarrer la génération
            var startMethod = logGenerator.GetType().GetMethod("StartGeneration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (startMethod != null)
            {
                startMethod.Invoke(logGenerator, null);
                LogStatus("    ✅ LogGenerator démarré via StartGeneration");
                return;
            }
            
            // Fallback Launch
            var launchMethod = logGenerator.GetType().GetMethod("Launch");
            if (launchMethod != null)
            {
                launchMethod.Invoke(logGenerator, null);
                LogStatus("    ✅ LogGenerator démarré via Launch");
                return;
            }
            
            // Fallback activation
            logGenerator.gameObject.SetActive(true);
            LogStatus("    ✅ LogGenerator activé");
        }
        catch (System.Exception ex)
        {
            LogError($"    ❌ Erreur LogGenerator: {ex.Message}");
        }
    }

    /// <summary>
    /// Tente de démarrer le timer de jeu
    /// </summary>
    private void TryStartGameTimer()
    {
        try
        {
            // Méthode LaunchLevel
            var launchLevelMethod = gameTimer.GetType().GetMethod("LaunchLevel");
            if (launchLevelMethod != null)
            {
                launchLevelMethod.Invoke(gameTimer, null);
                LogStatus("    ✅ GameTimer démarré via LaunchLevel");
                return;
            }
            
            // Fallback Launch
            var launchMethod = gameTimer.GetType().GetMethod("Launch");
            if (launchMethod != null)
            {
                launchMethod.Invoke(gameTimer, null);
                LogStatus("    ✅ GameTimer démarré via Launch");
                return;
            }
            
            // Fallback activation
            gameTimer.gameObject.SetActive(true);
            LogStatus("    ✅ GameTimer activé");
        }
        catch (System.Exception ex)
        {
            LogError($"    ❌ Erreur GameTimer: {ex.Message}");
        }
    }

    /// <summary>
    /// Tente d'activer le système de score
    /// </summary>
    private void TryStartScoring()
    {
        try
        {
            var startScoringMethod = scoreManager.GetType().GetMethod("StartScoring");
            if (startScoringMethod != null)
            {
                startScoringMethod.Invoke(scoreManager, null);
                LogStatus("    ✅ ScoreManager démarré");
            }
            else
            {
                scoreManager.gameObject.SetActive(true);
                LogStatus("    ✅ ScoreManager activé");
            }
        }
        catch (System.Exception ex)
        {
            LogError($"    ❌ Erreur ScoreManager: {ex.Message}");
        }
    }

    /// <summary>
    /// Valide que tous les composants nécessaires sont présents
    /// </summary>
    private bool ValidateAllComponents()
    {
        bool isValid = true;
        
        if (gameController == null)
        {
            LogError("❌ GameController manquant");
            isValid = false;
        }
        
        if (gameTimer == null)
        {
            LogError("❌ GameTimer manquant");
            isValid = false;
        }
        
        if (logGenerator == null)
        {
            LogError("❌ LogGenerator manquant");
            isValid = false;
        }
        
        if (scoreManager == null)
        {
            LogError("❌ ScoreManager manquant");
            isValid = false;
        }
        
        if (isValid)
        {
            LogStatus("✅ Tous les composants requis sont présents");
        }
        
        return isValid;
    }    /// <summary>
    /// Recherche automatique des composants via SystemValidator
    /// </summary>
    private void AutoFindComponents()
    {
        var validator = LogParadeSystemValidator.Instance;
        
        if (validator != null)
        {
            // Utiliser SystemValidator pour récupération optimisée
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
            // Fallback si SystemValidator pas disponible
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
        
        LogStatus("Recherche automatique des composants terminée");
    }

    /// <summary>
    /// Redémarre le jeu complet
    /// </summary>
    public void RestartGame()
    {
        LogStatus("🔄 Redémarrage du jeu...");
        
        // Informer GameStateController
        LogParadeGameStateController.RestartGame();
        
        // Relancer après un délai
        StartCoroutine(RestartGameCoroutine());
    }

    private IEnumerator RestartGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);        LaunchFullGame();
    }

    /// <summary>
    /// Démarre le processus complet : calibration puis jeu
    /// </summary>
    public void StartCompleteGameProcess()
    {
        LogStatus("🚀 DÉMARRAGE DU PROCESSUS COMPLET LOGPARADE");
        
        // Si le jeu est déjà démarré, on redémarre tout
        if (LogParadeGameStateController.IsGameStarted)
        {
            LogStatus("Jeu en cours - redémarrage complet...");
            RestartGame();
            return;
        }
        
        // Si la calibration est en cours, attendre
        if (LogParadeGameStateController.IsCalibrationInProgress)
        {
            LogStatus("Calibration déjà en cours - attente de la fin...");
            return;
        }
        
        // Si on peut déjà jouer (calibration déjà faite), lancer directement
        if (LogParadeGameStateController.CanStartGameplay())
        {
            LogStatus("Calibration déjà terminée - lancement direct du jeu");
            LaunchFullGame();
            return;
        }
        
        // Sinon, démarrer la calibration
        LogStatus("Démarrage de la calibration...");
        StartCalibrationProcess();
    }    // Méthodes de logging utilisant le système centralisé
    private void LogStatus(string message)
    {
        if (enableDetailedLogs)
            LogParadeLogger.Log($"[GameLauncher] {message}");
    }

    private void LogWarning(string message)
    {
        LogParadeLogger.LogWarning($"[GameLauncher] {message}");
    }    private void LogError(string message)
    {
        LogParadeLogger.LogError($"[GameLauncher] {message}");
    }

    #region Debug Methods

    /// <summary>
    /// Debug: Force le démarrage immédiat de la calibration
    /// </summary>
    [ContextMenu("Debug - Force Start Calibration NOW")]
    public void DebugForceStartCalibrationNow()
    {
        LogParadeLogger.Log("🐛 DEBUG: Forçage immédiat de la calibration");
        ForceStartCalibrationNow();
    }
    
    /// <summary>
    /// Debug: Démarre le processus complet (calibration + jeu)
    /// </summary>
    [ContextMenu("Debug - Start Complete Process")]
    public void DebugStartCompleteProcess()
    {
        LogParadeLogger.Log("🐛 DEBUG: Démarrage du processus complet");
        StartCompleteGameProcess();
    }
    
    /// <summary>
    /// Debug: Force le lancement du jeu (bypass calibration)
    /// </summary>
    [ContextMenu("Debug - Force Launch Game")]
    public void DebugForceLaunchGame()
    {
        LogParadeLogger.LogWarning("🐛 DEBUG: Forçage du lancement du jeu (bypass calibration)");
        LogParadeGameStateController.ForceEnableGameplay();
        LaunchFullGame();
    }
    
    /// <summary>
    /// Debug: Affiche l'état actuel du système
    /// </summary>
    [ContextMenu("Debug - Show System State")]
    public void DebugShowSystemState()
    {
        LogParadeLogger.Log($"🐛 DEBUG - État du système:\n" +
                  $"- Calibration en cours: {LogParadeGameStateController.IsCalibrationInProgress}\n" +
                  $"- Gameplay autorisé: {LogParadeGameStateController.IsGameplayAllowed}\n" +
                  $"- Jeu démarré: {LogParadeGameStateController.IsGameStarted}\n" +
                  $"- Peut démarrer: {LogParadeGameStateController.CanStartGameplay()}\n" +
                  $"- Statut: {LogParadeGameStateController.GetCurrentStatusText()}\n" +
                  $"- Launcher en cours: {isLaunching}");
    }
    
    #endregion
}
