using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire centralisé pour coordonner le démarrage de tous les systèmes LogParade
/// après la calibration. Assure le bon ordre d'initialisation et le démarrage synchronisé.
/// </summary>
public class LogParadeGameLauncher : MonoBehaviour
{
    [Header("Références des Systèmes")]
    [Tooltip("Contrôleur principal du jeu")]
    [SerializeField] private LogParadeGameController gameController;
    
    [Tooltip("Gestionnaire du timer de jeu")]
    [SerializeField] private LogParadeGameTimer gameTimer;
    
    [Tooltip("Générateur de rondins")]
    [SerializeField] private LogParadeLogGenerator logGenerator;
    
    [Tooltip("Gestionnaire de score")]
    [SerializeField] private LogParadeScoreManager scoreManager;
    
    [Tooltip("Gestionnaire de calibration")]
    [SerializeField] private LogParadeCalibrationManager calibrationManager;
    
    [Header("Paramètres de Démarrage")]
    [Tooltip("Délai avant le démarrage du jeu après calibration (secondes)")]
    [SerializeField] private float delayAfterCalibration = 1.5f;
    
    [Tooltip("Recherche automatique des composants")]
    [SerializeField] private bool autoFindComponents = true;
    
    [Header("Debug")]
    [Tooltip("Logs détaillés du processus de démarrage")]
    [SerializeField] private bool enableDetailedLogs = true;
    
    // État du lanceur
    private bool isLaunching = false;
    private bool gameFullyStarted = false;
    
    // Événements
    public System.Action OnGameLaunchStarted;
    public System.Action OnGameLaunchCompleted;
    public System.Action OnGameLaunchFailed;

    void Awake()
    {
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
    }

    void Start()
    {
        // S'abonner aux événements de calibration
        if (calibrationManager != null)
        {
            // Utiliser la réflexion pour s'abonner aux événements privés si nécessaire
            SubscribeToCalibrationEvents();
        }
        
        LogStatus("LogParadeGameLauncher initialisé et prêt");
    }

    /// <summary>
    /// Démarre le jeu complet après la calibration
    /// </summary>
    public void LaunchFullGame()
    {
        if (isLaunching)
        {
            LogWarning("Lancement déjà en cours, ignorer la demande");
            return;
        }
        
        if (gameFullyStarted)
        {
            LogWarning("Jeu déjà démarré, ignorer la demande");
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
        
        LogStatus("🚀 DÉMARRAGE DU JEU LOGPARADE");
        
        // Phase 1: Validation des composants
        LogStatus("Phase 1: Validation des composants...");
        if (!ValidateAllComponents())
        {
            LogError("❌ Validation des composants échouée");
            isLaunching = false;
            OnGameLaunchFailed?.Invoke();
            yield break;
        }
        
        // Phase 2: Initialisation des systèmes
        LogStatus("Phase 2: Initialisation des systèmes...");
        yield return StartCoroutine(InitializeAllSystems());
        
        // Phase 3: Attendre le délai configuré
        LogStatus($"Phase 3: Attente de {delayAfterCalibration}s...");
        yield return new WaitForSeconds(delayAfterCalibration);
        
        // Phase 4: Démarrage du générateur de rondins
        LogStatus("Phase 4: Démarrage du générateur de rondins...");
        yield return StartCoroutine(StartLogGeneration());
        
        // Phase 5: Démarrage du timer
        LogStatus("Phase 5: Démarrage du timer de jeu...");
        yield return StartCoroutine(StartGameTimer());
        
        // Phase 6: Activation du scoring
        LogStatus("Phase 6: Activation du système de score...");
        yield return StartCoroutine(StartScoring());
        
        // Phase 7: Finalisation
        LogStatus("Phase 7: Finalisation du lancement...");
        yield return StartCoroutine(FinalizeGameStart());
        
        // Fin du processus
        isLaunching = false;
        gameFullyStarted = true;
        
        LogStatus("✅ JEU LOGPARADE ENTIÈREMENT DÉMARRÉ !");
        OnGameLaunchCompleted?.Invoke();
    }

    /// <summary>
    /// Initialise tous les systèmes dans l'ordre correct
    /// </summary>
    private IEnumerator InitializeAllSystems()
    {
        // Initialiser le contrôleur de jeu
        if (gameController != null)
        {
            LogStatus("  → Initialisation GameController...");
            gameController.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }
        
        // Initialiser le score manager
        if (scoreManager != null)
        {
            LogStatus("  → Initialisation ScoreManager...");
            scoreManager.ResetScore();
            yield return new WaitForEndOfFrame();
        }
        
        // Initialiser le générateur de rondins
        if (logGenerator != null)
        {
            LogStatus("  → Initialisation LogGenerator...");
            logGenerator.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }
        
        // Initialiser le timer
        if (gameTimer != null)
        {
            LogStatus("  → Initialisation GameTimer...");
            gameTimer.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }
        
        LogStatus("  ✅ Tous les systèmes initialisés");
    }    /// <summary>
    /// Démarre la génération des rondins
    /// </summary>
    private IEnumerator StartLogGeneration()
    {
        if (logGenerator == null)
        {
            LogError("❌ LogGenerator non trouvé");
            yield break;
        }
        
        bool success = false;
        
        // Activer la génération
        var enableGenerationField = logGenerator.GetType().GetField("enableGeneration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (enableGenerationField != null)
        {
            enableGenerationField.SetValue(logGenerator, true);
            LogStatus("  → Génération de rondins activée");
        }
        
        // Forcer le démarrage de la génération
        var startGenerationMethod = logGenerator.GetType().GetMethod("StartGeneration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (startGenerationMethod != null)
        {
            try
            {
                startGenerationMethod.Invoke(logGenerator, null);
                LogStatus("  → Génération de rondins démarrée");
                success = true;
            }
            catch (System.Exception ex)
            {
                LogError($"❌ Erreur StartGeneration: {ex.Message}");
            }
        }
        
        if (!success)
        {
            // Méthode alternative : via Launch() ou LaunchLevel()
            var launchMethod = logGenerator.GetType().GetMethod("Launch");
            if (launchMethod != null)
            {
                try
                {
                    launchMethod.Invoke(logGenerator, null);
                    LogStatus("  → Génération lancée via Launch()");
                    success = true;
                }
                catch (System.Exception ex)
                {
                    LogError($"❌ Erreur Launch: {ex.Message}");
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (success)
        {
            LogStatus("  ✅ Générateur de rondins opérationnel");
        }
        else
        {
            LogError("❌ Impossible de démarrer le générateur de rondins");
        }
    }    /// <summary>
    /// Démarre le timer de jeu
    /// </summary>
    private IEnumerator StartGameTimer()
    {
        if (gameTimer == null)
        {
            LogError("❌ GameTimer non trouvé");
            yield break;
        }
        
        bool success = false;
        
        // Essayer différentes méthodes de démarrage
        var launchLevelMethod = gameTimer.GetType().GetMethod("LaunchLevel");
        if (launchLevelMethod != null)
        {
            try
            {
                launchLevelMethod.Invoke(gameTimer, null);
                LogStatus("  → Timer démarré via LaunchLevel()");
                success = true;
            }
            catch (System.Exception ex)
            {
                LogError($"❌ Erreur LaunchLevel: {ex.Message}");
            }
        }
        
        if (!success)
        {
            var launchMethod = gameTimer.GetType().GetMethod("Launch");
            if (launchMethod != null)
            {
                try
                {
                    launchMethod.Invoke(gameTimer, null);
                    LogStatus("  → Timer démarré via Launch()");
                    success = true;
                }
                catch (System.Exception ex)
                {
                    LogError($"❌ Erreur Launch: {ex.Message}");
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (success)
        {
            LogStatus("  ✅ Timer de jeu démarré");
        }
        else
        {
            LogError("❌ Impossible de démarrer le timer");
        }
    }    /// <summary>
    /// Active le système de score
    /// </summary>
    private IEnumerator StartScoring()
    {
        if (scoreManager == null)
        {
            LogError("❌ ScoreManager non trouvé");
            yield break;
        }
        
        bool success = false;
        
        // Activer le scoring
        var startScoringMethod = scoreManager.GetType().GetMethod("StartScoring");
        if (startScoringMethod != null)
        {
            try
            {
                startScoringMethod.Invoke(scoreManager, null);
                LogStatus("  → Système de score activé");
                success = true;
            }
            catch (System.Exception ex)
            {
                LogError($"❌ Erreur StartScoring: {ex.Message}");
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        
        if (success)
        {
            LogStatus("  ✅ Système de score opérationnel");
        }
        else
        {
            LogStatus("  ⚠️ StartScoring non disponible, le score sera géré automatiquement");
        }
    }    /// <summary>
    /// Finalise le démarrage du jeu
    /// </summary>
    private IEnumerator FinalizeGameStart()
    {
        // Forcer le démarrage du contrôleur principal si nécessaire
        if (gameController != null)
        {
            var forceStartMethod = gameController.GetType().GetMethod("ForceStartGame");
            if (forceStartMethod != null)
            {
                try
                {
                    forceStartMethod.Invoke(gameController, null);
                    LogStatus("  → Contrôleur principal forcé au démarrage");
                }
                catch (System.Exception ex)
                {
                    LogStatus($"  → ForceStartGame erreur: {ex.Message}");
                }
            }
            else
            {
                LogStatus("  → ForceStartGame non disponible");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        LogStatus("  ✅ Jeu entièrement opérationnel");
    }

    /// <summary>
    /// Valide que tous les composants requis sont présents
    /// </summary>
    private bool ValidateAllComponents()
    {
        var issues = new List<string>();
        
        if (gameController == null) issues.Add("GameController manquant");
        if (logGenerator == null) issues.Add("LogGenerator manquant");
        if (scoreManager == null) issues.Add("ScoreManager manquant");
        if (gameTimer == null) issues.Add("GameTimer manquant");
        
        if (issues.Count > 0)
        {
            LogError($"Composants manquants: {string.Join(", ", issues)}");
            return false;
        }
        
        LogStatus("  ✅ Tous les composants sont présents");
        return true;
    }

    /// <summary>
    /// Recherche automatique des composants
    /// </summary>
    private void AutoFindComponents()
    {
        if (gameController == null)
            gameController = FindObjectOfType<LogParadeGameController>();
        
        if (gameTimer == null)
            gameTimer = FindObjectOfType<LogParadeGameTimer>();
        
        if (logGenerator == null)
            logGenerator = FindObjectOfType<LogParadeLogGenerator>();
        
        if (scoreManager == null)
            scoreManager = FindObjectOfType<LogParadeScoreManager>();
        
        if (calibrationManager == null)
            calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        
        LogStatus($"Auto-découverte: " +
                  $"GameController={gameController != null}, " +
                  $"GameTimer={gameTimer != null}, " +
                  $"LogGenerator={logGenerator != null}, " +
                  $"ScoreManager={scoreManager != null}, " +
                  $"CalibrationManager={calibrationManager != null}");
    }

    /// <summary>
    /// S'abonne aux événements de calibration
    /// </summary>
    private void SubscribeToCalibrationEvents()
    {
        // Essayer de s'abonner à l'événement de fin de calibration
        // Comme l'événement pourrait être privé, on utilise d'autres méthodes
        
        // Méthode 1: Polling du statut de calibration
        StartCoroutine(MonitorCalibrationStatus());
    }

    /// <summary>
    /// Surveille le statut de calibration et lance le jeu quand elle est terminée
    /// </summary>
    private IEnumerator MonitorCalibrationStatus()
    {
        bool wasCalibrated = false;
        
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (calibrationManager == null) continue;
            
            // Vérifier le statut de calibration
            bool isCalibrated = IsCalibrationCompleted();
            
            if (!wasCalibrated && isCalibrated)
            {
                LogStatus("🎯 Calibration détectée comme terminée - Lancement du jeu");
                yield return new WaitForSeconds(0.5f); // Petit délai pour s'assurer
                LaunchFullGame();
                break;
            }
            
            wasCalibrated = isCalibrated;
        }
    }

    /// <summary>
    /// Vérifie si la calibration est terminée
    /// </summary>
    private bool IsCalibrationCompleted()
    {
        if (calibrationManager == null) return false;
        
        // Essayer la méthode publique
        var method = calibrationManager.GetType().GetMethod("IsCalibrationCompleted");
        if (method != null)
        {
            return (bool)method.Invoke(calibrationManager, null);
        }
        
        // Essayer le champ privé
        var field = calibrationManager.GetType().GetField("calibrationCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(calibrationManager);
        }
        
        return false;
    }

    /// <summary>
    /// Méthodes de logging
    /// </summary>
    private void LogStatus(string message)
    {
        if (enableDetailedLogs)
        {
            Debug.Log($"[LogParadeGameLauncher] {message}");
        }
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[LogParadeGameLauncher] ⚠️ {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[LogParadeGameLauncher] ❌ {message}");
    }

    /// <summary>
    /// Redémarre le jeu complet
    /// </summary>
    public void RestartGame()
    {
        gameFullyStarted = false;
        LaunchFullGame();
    }

    /// <summary>
    /// Arrête tous les systèmes
    /// </summary>
    public void StopAllSystems()
    {
        gameFullyStarted = false;
        
        // Arrêter le générateur
        if (logGenerator != null)
        {
            var stopMethod = logGenerator.GetType().GetMethod("StopGeneration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stopMethod?.Invoke(logGenerator, null);
        }
        
        // Arrêter le score
        if (scoreManager != null)
        {
            var stopMethod = scoreManager.GetType().GetMethod("StopScoring");
            stopMethod?.Invoke(scoreManager, null);
        }
        
        LogStatus("🛑 Tous les systèmes arrêtés");
    }    /// <summary>
    /// Interface de debug OnGUI
    /// </summary>
    void OnGUI()
    {
        if (!enableDetailedLogs) return;
        
        // Vérifier l'état GUI avant de commencer
        if (Event.current == null || Event.current.type == EventType.Used)
            return;
            
        var rect = new Rect(10, 10, 300, 150);
        bool areaStarted = false;
        
        try
        {
            GUI.Box(rect, "LogParade Game Launcher");
            
            var areaRect = new Rect(rect.x + 5, rect.y + 25, rect.width - 10, rect.height - 30);
            GUILayout.BeginArea(areaRect);
            areaStarted = true;
            
            GUILayout.Label($"Launching: {isLaunching}");
            GUILayout.Label($"Game Started: {gameFullyStarted}");
            GUILayout.Label($"Calibrated: {IsCalibrationCompleted()}");
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Force Launch Game"))
            {
                LaunchFullGame();
            }
            
            if (GUILayout.Button("Restart Game"))
            {
                RestartGame();
            }
            
            if (GUILayout.Button("Stop All"))
            {
                StopAllSystems();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LogParadeGameLauncher] Erreur OnGUI: {ex}");
            
            // Tenter de nettoyer l'état GUI si possible
            if (areaStarted)
            {
                try
                {
                    GUILayout.EndArea();
                    areaStarted = false;
                }
                catch { }
            }
            
            // Sortie propre du GUI
            GUIUtility.ExitGUI();
        }
        finally
        {
            // Assurer que EndArea() est appelé si BeginArea() a été appelé
            if (areaStarted)
            {
                try
                {
                    GUILayout.EndArea();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[LogParadeGameLauncher] Erreur EndArea: {ex}");
                }
            }
        }
    }
}
