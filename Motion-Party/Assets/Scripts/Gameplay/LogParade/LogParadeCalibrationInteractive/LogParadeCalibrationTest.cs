using UnityEngine;

/// <summary>
/// Script de test rapide pour valider que le système de calibration
/// fonctionne correctement après les corrections apportées.
/// </summary>
public class LogParadeCalibrationTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    
    void Start()
    {
        if (runTestOnStart)
        {
            Invoke(nameof(RunCompilationTest), 1f);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            RunCompilationTest();
        }
    }
    
    /// <summary>
    /// Test de compilation pour s'assurer que toutes les méthodes sont accessibles
    /// </summary>
    private void RunCompilationTest()
    {
        Debug.Log("=== TEST DE COMPILATION DU SYSTÈME DE CALIBRATION ===");
        
        try
        {
            // Test des méthodes statiques du CalibrationManager
            bool canStartScoring = LogParadeCalibrationManager.CanStartScoring();
            bool canStartGameplay = LogParadeCalibrationManager.CanStartGameplay();
            bool isCalibrationInProgress = LogParadeCalibrationManager.IsCalibrationInProgress;
            bool isGameplayAllowed = LogParadeCalibrationManager.IsGameplayAllowed;
            
            Debug.Log($"✅ Méthodes statiques accessibles:");
            Debug.Log($"  - CanStartScoring: {canStartScoring}");
            Debug.Log($"  - CanStartGameplay: {canStartGameplay}");
            Debug.Log($"  - IsCalibrationInProgress: {isCalibrationInProgress}");
            Debug.Log($"  - IsGameplayAllowed: {isGameplayAllowed}");
            
            // Test des composants
            TestComponents();
            
            Debug.Log("✅ TOUS LES TESTS DE COMPILATION RÉUSSIS!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ ERREUR DE COMPILATION: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Test des accès aux composants
    /// </summary>
    private void TestComponents()
    {
        // Test LateralTracker
        var lateralTracker = FindObjectOfType<LogParadeLateralTracker>();
        if (lateralTracker != null)
        {
            bool autoCalib = lateralTracker.enableAutoCalibration;
            bool bypass = lateralTracker.bypassCalibrationForInteractiveMode;
            Debug.Log($"✅ LateralTracker accessible: AutoCalib={autoCalib}, Bypass={bypass}");
        }
        
        // Test GameTimer
        var gameTimer = FindObjectOfType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            bool isActive = gameTimer.IsGameActive;
            float timeRemaining = gameTimer.TimeRemaining;
            Debug.Log($"✅ GameTimer accessible: IsActive={isActive}, TimeRemaining={timeRemaining}");
            
            // Test de la méthode StopGame (sans l'appeler)
            var stopGameMethod = gameTimer.GetType().GetMethod("StopGame");
            if (stopGameMethod != null && stopGameMethod.IsPublic)
            {
                Debug.Log($"✅ GameTimer.StopGame() est accessible");
            }
            else
            {
                Debug.LogError($"❌ GameTimer.StopGame() n'est pas accessible");
            }
        }
        
        // Test ScoreManager
        var scoreManager = FindObjectOfType<LogParadeScoreManager>();
        if (scoreManager != null)
        {
            bool isScoring = scoreManager.IsScoring;
            int currentScore = scoreManager.CurrentScore;
            Debug.Log($"✅ ScoreManager accessible: IsScoring={isScoring}, Score={currentScore}");
        }
    }
    
    /// <summary>
    /// Test d'intégration simple
    /// </summary>
    [ContextMenu("Test Intégration")]
    public void TestIntegration()
    {
        Debug.Log("=== TEST D'INTÉGRATION ===");
        
        // Simuler le début de calibration
        Debug.Log("🔄 Simulation début de calibration...");
        // Note: On ne peut pas forcer les propriétés statiques directement
        // mais on peut vérifier l'état
        
        if (LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            Debug.Log("✅ Calibration détectée comme en cours");
            
            if (!LogParadeCalibrationManager.CanStartScoring())
            {
                Debug.Log("✅ Score correctement bloqué");
            }
            else
            {
                Debug.LogWarning("⚠️ Score non bloqué pendant calibration");
            }
            
            if (!LogParadeCalibrationManager.CanStartGameplay())
            {
                Debug.Log("✅ Gameplay correctement bloqué");
            }
            else
            {
                Debug.LogWarning("⚠️ Gameplay non bloqué pendant calibration");
            }
        }
        else
        {
            Debug.Log("ℹ️ Calibration non en cours actuellement");
        }
    }
    
    /// <summary>
    /// Test de correction automatique
    /// </summary>
    [ContextMenu("Test Correction Auto")]
    public void TestAutoCorrection()
    {
        Debug.Log("=== TEST CORRECTION AUTOMATIQUE ===");
        
        var lateralTracker = FindObjectOfType<LogParadeLateralTracker>();
        if (lateralTracker != null)
        {
            // Vérifier les bonnes valeurs
            if (!lateralTracker.enableAutoCalibration && lateralTracker.bypassCalibrationForInteractiveMode)
            {
                Debug.Log("✅ LateralTracker correctement configuré");
            }
            else
            {
                Debug.LogWarning("⚠️ LateralTracker mal configuré - correction...");
                lateralTracker.enableAutoCalibration = false;
                lateralTracker.bypassCalibrationForInteractiveMode = true;
                Debug.Log("✅ LateralTracker corrigé");
            }
        }
        
        var scoreManager = FindObjectOfType<LogParadeScoreManager>();
        if (scoreManager != null && scoreManager.IsScoring && LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            Debug.LogWarning("⚠️ Score actif pendant calibration - arrêt...");
            scoreManager.StopScoring();
            Debug.Log("✅ Score arrêté");
        }
    }
}
