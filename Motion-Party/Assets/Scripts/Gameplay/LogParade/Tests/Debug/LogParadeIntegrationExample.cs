using UnityEngine;

/// <summary>
/// Exemple d'intégration avec les nouveaux modules SystemValidator et EventCoordinator.
/// Démontre comment s'abonner aux événements et utiliser la validation centralisée.
/// </summary>
public class LogParadeIntegrationExample : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDetailedLogs = true;
    
    void Start()
    {
        // S'abonner aux événements centralisés
        SubscribeToEvents();
        
        // Exemple d'utilisation du SystemValidator
        ValidateSystemExample();
    }

    void OnDestroy()
    {
        // Se désabonner des événements
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Exemple d'abonnement aux événements centralisés
    /// </summary>
    private void SubscribeToEvents()
    {
        var eventCoordinator = LogParadeEventCoordinator.Instance;
        if (eventCoordinator != null)
        {
            // Événements de calibration
            eventCoordinator.OnCalibrationStarted += OnCalibrationStarted;
            eventCoordinator.OnCalibrationCompleted += OnCalibrationCompleted;
            eventCoordinator.OnCalibrationFailed += OnCalibrationFailed;
            
            // Événements de lancement
            eventCoordinator.OnGameLaunchStarted += OnGameLaunchStarted;
            eventCoordinator.OnGameLaunchCompleted += OnGameLaunchCompleted;
            eventCoordinator.OnGameLaunchFailed += OnGameLaunchFailed;
            
            // Événements de gameplay
            eventCoordinator.OnGameStarted += OnGameStarted;
            eventCoordinator.OnGameEnded += OnGameEnded;
            
            // Événements de score
            eventCoordinator.OnScoreChanged += OnScoreChanged;
            eventCoordinator.OnFinalScoreCalculated += OnFinalScoreCalculated;
            
            // Événements système
            eventCoordinator.OnSystemHealthChanged += OnSystemHealthChanged;
            
            LogMessage("✅ Abonné aux événements centralisés");
        }
        else
        {
            LogMessage("⚠️ EventCoordinator non disponible");
        }
    }

    /// <summary>
    /// Se désabonne des événements
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        var eventCoordinator = LogParadeEventCoordinator.Instance;
        if (eventCoordinator != null)
        {
            eventCoordinator.OnCalibrationStarted -= OnCalibrationStarted;
            eventCoordinator.OnCalibrationCompleted -= OnCalibrationCompleted;
            eventCoordinator.OnCalibrationFailed -= OnCalibrationFailed;
            eventCoordinator.OnGameLaunchStarted -= OnGameLaunchStarted;
            eventCoordinator.OnGameLaunchCompleted -= OnGameLaunchCompleted;
            eventCoordinator.OnGameLaunchFailed -= OnGameLaunchFailed;
            eventCoordinator.OnGameStarted -= OnGameStarted;
            eventCoordinator.OnGameEnded -= OnGameEnded;
            eventCoordinator.OnScoreChanged -= OnScoreChanged;
            eventCoordinator.OnFinalScoreCalculated -= OnFinalScoreCalculated;
            eventCoordinator.OnSystemHealthChanged -= OnSystemHealthChanged;
        }
    }

    /// <summary>
    /// Exemple d'utilisation du SystemValidator
    /// </summary>
    private void ValidateSystemExample()
    {
        var validator = LogParadeSystemValidator.Instance;
        if (validator != null)
        {
            // Forcer une validation
            bool isSystemHealthy = validator.ForceValidation();
            LogMessage($"🔍 Validation système: {(isSystemHealthy ? "✅ SAIN" : "❌ PROBLÈMES")}");
            
            // Obtenir des composants spécifiques
            var gameController = validator.GetValidatedComponent<LogParadeGameController>();
            var scoreManager = validator.GetValidatedComponent<LogParadeScoreManager>();
            
            LogMessage($"📋 Composants trouvés: GameController={gameController != null}, ScoreManager={scoreManager != null}");
        }
        else
        {
            LogMessage("⚠️ SystemValidator non disponible");
        }
    }

    #region Gestionnaires d'événements

    private void OnCalibrationStarted()
    {
        LogMessage("🎯 Calibration démarrée");
    }

    private void OnCalibrationCompleted()
    {
        LogMessage("✅ Calibration terminée");
    }

    private void OnCalibrationFailed()
    {
        LogMessage("❌ Calibration échouée");
    }

    private void OnGameLaunchStarted()
    {
        LogMessage("🚀 Lancement du jeu démarré");
    }

    private void OnGameLaunchCompleted()
    {
        LogMessage("✅ Lancement du jeu terminé");
    }

    private void OnGameLaunchFailed()
    {
        LogMessage("❌ Lancement du jeu échoué");
    }

    private void OnGameStarted()
    {
        LogMessage("🎮 Jeu démarré");
    }

    private void OnGameEnded()
    {
        LogMessage("🏁 Jeu terminé");
    }

    private void OnScoreChanged(int newScore)
    {
        LogMessage($"📊 Score modifié: {newScore}");
    }

    private void OnFinalScoreCalculated(int finalScore)
    {
        LogMessage($"🎯 Score final: {finalScore}");
    }

    private void OnSystemHealthChanged(bool isHealthy)
    {
        LogMessage($"🔧 Santé système: {(isHealthy ? "✅ SAIN" : "❌ PROBLÈMES")}");
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test manuel des événements (pour debug)
    /// </summary>
    [ContextMenu("Test Event Sequence")]
    public void TestEventSequence()
    {
        StartCoroutine(TestEventSequenceCoroutine());
    }

    private System.Collections.IEnumerator TestEventSequenceCoroutine()
    {
        LogMessage("🧪 Test de séquence d'événements démarré");
        
        LogParadeEventCoordinator.TriggerCalibrationStarted();
        yield return new WaitForSeconds(1f);
        
        LogParadeEventCoordinator.TriggerCalibrationCompleted();
        yield return new WaitForSeconds(0.5f);
        
        LogParadeEventCoordinator.TriggerGameLaunchStarted();
        yield return new WaitForSeconds(1f);
        
        LogParadeEventCoordinator.TriggerGameLaunchCompleted();
        yield return new WaitForSeconds(0.5f);
        
        LogParadeEventCoordinator.TriggerGameStarted();
        yield return new WaitForSeconds(2f);
        
        for (int i = 1; i <= 5; i++)
        {
            LogParadeEventCoordinator.TriggerScoreChanged(i * 10);
            yield return new WaitForSeconds(0.5f);
        }
        
        LogParadeEventCoordinator.TriggerGameEnded();
        yield return new WaitForSeconds(0.5f);
        
        LogParadeEventCoordinator.TriggerFinalScoreCalculated(50);
        
        LogMessage("🧪 Test de séquence d'événements terminé");
    }

    /// <summary>
    /// Test de validation système (pour debug)
    /// </summary>
    [ContextMenu("Test System Validation")]
    public void TestSystemValidation()
    {
        var validator = LogParadeSystemValidator.Instance;
        if (validator != null)
        {
            LogMessage("🔍 Test de validation système");
            
            // Afficher le rapport complet
            string report = validator.GenerateSystemReport();
            Debug.Log(report);
            
            // Vider le cache et re-valider
            validator.ClearComponentCache();
            bool isHealthy = validator.ForceValidation();
            
            LogMessage($"🔍 Validation après vidage cache: {(isHealthy ? "✅ SAIN" : "❌ PROBLÈMES")}");
        }
        else
        {
            LogMessage("❌ SystemValidator non disponible pour le test");
        }
    }

    #endregion

    private void LogMessage(string message)
    {
        if (enableDetailedLogs)
            Debug.Log($"[IntegrationExample] {message}");
    }
}
