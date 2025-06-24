using UnityEngine;

/// <summary>
/// Script de test pour valider que la séquence complète de calibration fonctionne correctement
/// À utiliser pour déboguer et valider les corrections apportées
/// </summary>
public class LogParadeCalibrationValidationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool showDetailedLogs = true;
    
    private LogParadeCalibrationManager calibrationManager;
    private LogParadeCalibrationInteractive calibrationSystem;
    private LogParadeLogGenerator logGenerator;
    private LogParadeGameTimer gameTimer;
    private LogParadeScoreManager scoreManager;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunValidationTests());
        }
    }
    
    private System.Collections.IEnumerator RunValidationTests()
    {
        Log("=== DÉMARRAGE DES TESTS DE VALIDATION DE CALIBRATION ===");
        
        // 1. Rechercher tous les composants nécessaires
        yield return StartCoroutine(FindAllComponents());
        
        // 2. Vérifier l'état initial
        yield return StartCoroutine(ValidateInitialState());
        
        // 3. Monitorer la calibration si elle est active
        if (calibrationManager != null && LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            yield return StartCoroutine(MonitorCalibrationProcess());
        }
        
        // 4. Vérifier l'état final après calibration
        yield return StartCoroutine(ValidateFinalState());
        
        Log("=== TESTS DE VALIDATION TERMINÉS ===");
    }
    
    private System.Collections.IEnumerator FindAllComponents()
    {
        Log("🔍 Recherche des composants...");
        
        calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        Log($"LogParadeCalibrationManager: {(calibrationManager != null ? "✅ Trouvé" : "❌ Manquant")}");
        
        calibrationSystem = FindObjectOfType<LogParadeCalibrationInteractive>();
        Log($"LogParadeCalibrationInteractive: {(calibrationSystem != null ? "✅ Trouvé" : "❌ Manquant")}");
        
        logGenerator = FindObjectOfType<LogParadeLogGenerator>();
        Log($"LogParadeLogGenerator: {(logGenerator != null ? "✅ Trouvé" : "❌ Manquant")}");
        
        gameTimer = FindObjectOfType<LogParadeGameTimer>();
        Log($"LogParadeGameTimer: {(gameTimer != null ? "✅ Trouvé" : "❌ Manquant")}");
        
        scoreManager = FindObjectOfType<LogParadeScoreManager>();
        Log($"LogParadeScoreManager: {(scoreManager != null ? "✅ Trouvé" : "❌ Manquant")}");
        
        yield return null;
    }
    
    private System.Collections.IEnumerator ValidateInitialState()
    {
        Log("🎯 Validation de l'état initial...");
        
        // Vérifier les verrous de calibration
        bool calibrationInProgress = LogParadeCalibrationManager.IsCalibrationInProgress;
        bool gameplayAllowed = LogParadeCalibrationManager.IsGameplayAllowed;
        
        Log($"Calibration en cours: {(calibrationInProgress ? "✅ Oui" : "❌ Non")}");
        Log($"Gameplay autorisé: {(gameplayAllowed ? "❌ Oui (problème!)" : "✅ Non")}");
        
        // Vérifier que les systèmes de jeu ne sont pas actifs
        if (logGenerator != null)
        {
            bool generatorActive = logGenerator.gameObject.activeInHierarchy;
            Log($"LogGenerator actif: {(generatorActive ? "⚠️ Oui (peut être normal)" : "✅ Non")}");
        }
        
        yield return null;
    }
    
    private System.Collections.IEnumerator MonitorCalibrationProcess()
    {
        Log("📊 Monitoring du processus de calibration...");
        
        float timeout = 60f; // 60 secondes max
        float elapsed = 0f;
        
        while (LogParadeCalibrationManager.IsCalibrationInProgress && elapsed < timeout)
        {
            // Log périodique du statut
            if (elapsed % 5f < Time.deltaTime) // Toutes les 5 secondes
            {
                if (calibrationSystem != null)
                {
                    var currentStep = calibrationSystem.GetType().GetField("currentStep", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (currentStep != null)
                    {
                        Log($"Étape de calibration actuelle: {currentStep.GetValue(calibrationSystem)}");
                    }
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (elapsed >= timeout)
        {
            Log("⏰ TIMEOUT: La calibration a pris plus de 60 secondes");
        }
        else
        {
            Log("✅ Calibration terminée avec succès");
        }
    }
    
    private System.Collections.IEnumerator ValidateFinalState()
    {
        Log("🎉 Validation de l'état final...");
        
        // Attendre un peu pour que tous les systèmes se mettent en place
        yield return new WaitForSeconds(2f);
        
        // Vérifier les verrous
        bool calibrationInProgress = LogParadeCalibrationManager.IsCalibrationInProgress;
        bool gameplayAllowed = LogParadeCalibrationManager.IsGameplayAllowed;
        
        Log($"Calibration terminée: {(!calibrationInProgress ? "✅ Oui" : "❌ Non (problème!)")}");
        Log($"Gameplay autorisé: {(gameplayAllowed ? "✅ Oui" : "❌ Non (problème!)")}");
        
        // Vérifier que les systèmes de jeu sont actifs
        if (logGenerator != null)
        {
            bool generatorActive = logGenerator.gameObject.activeInHierarchy;
            Log($"LogGenerator actif après calibration: {(generatorActive ? "✅ Oui" : "❌ Non (problème!)")}");
        }
        
        if (gameTimer != null)
        {
            bool timerActive = gameTimer.gameObject.activeInHierarchy;
            Log($"GameTimer actif après calibration: {(timerActive ? "✅ Oui" : "❌ Non (problème!)")}");
        }
        
        if (scoreManager != null)
        {
            bool scoringActive = scoreManager.IsScoring;
            Log($"Score actif après calibration: {(scoringActive ? "✅ Oui" : "❌ Non (problème!)")}");
        }
        
        // Vérifier qu'il n'y a plus de rondins de calibration
        var calibrationLogs = GameObject.FindGameObjectsWithTag("CalibrationLog");
        Log($"Rondins de calibration restants: {calibrationLogs.Length} {(calibrationLogs.Length == 0 ? "✅" : "❌")}");
        
        Log("🎯 Validation finale terminée");
    }
    
    /// <summary>
    /// Méthode publique pour lancer les tests manuellement
    /// </summary>
    [ContextMenu("Run Validation Tests")]
    public void RunTestsManually()
    {
        StartCoroutine(RunValidationTests());
    }
    
    private void Log(string message)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[CalibrationValidationTest] {message}");
        }
    }
}
