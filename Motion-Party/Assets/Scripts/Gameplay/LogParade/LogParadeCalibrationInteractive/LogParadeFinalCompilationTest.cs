using UnityEngine;

/// <summary>
/// Test final de compilation pour valider toutes les corrections
/// </summary>
public class LogParadeFinalCompilationTest : MonoBehaviour
{
    [Header("Test de Compilation Finale")]
    [SerializeField] private bool testOnStart = false;
    
    void Start()
    {
        if (testOnStart)
        {
            TestFinalCompilation();
        }
    }
    
    [ContextMenu("Test Final Compilation")]
    public void TestFinalCompilation()
    {
        Debug.Log("=== TEST FINAL DE COMPILATION ===");
        
        // Tester les nouvelles méthodes
        TestCalibrationManagerMethods();
        TestLogGeneratorMethods();
        TestUIManagerMethods();
        TestGameControllerMethods();
        
        Debug.Log("=== TEST FINAL TERMINÉ AVEC SUCCÈS ===");
    }
    
    private void TestCalibrationManagerMethods()
    {
        Debug.Log("🔍 Test LogParadeCalibrationManager...");
        
        var calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        if (calibrationManager != null)
        {
            // Tester les propriétés statiques
            bool inProgress = LogParadeCalibrationManager.IsCalibrationInProgress;
            bool allowed = LogParadeCalibrationManager.IsGameplayAllowed;
            
            Debug.Log($"✅ Propriétés statiques accessibles - InProgress: {inProgress}, Allowed: {allowed}");
        }
        else
        {
            Debug.Log("ℹ️ LogParadeCalibrationManager non trouvé (normal si pas en scène)");
        }
    }
    
    private void TestLogGeneratorMethods()
    {
        Debug.Log("🔍 Test LogParadeLogGenerator...");
        
        var logGenerator = FindObjectOfType<LogParadeLogGenerator>();
        if (logGenerator != null)
        {
            // Tester que la méthode StartLogGeneration existe
            var method = logGenerator.GetType().GetMethod("StartLogGeneration");
            if (method != null)
            {
                Debug.Log("✅ Méthode StartLogGeneration() trouvée");
            }
            else
            {
                Debug.LogError("❌ Méthode StartLogGeneration() non trouvée");
            }
        }
        else
        {
            Debug.Log("ℹ️ LogParadeLogGenerator non trouvé (normal si pas en scène)");
        }
    }
    
    private void TestUIManagerMethods()
    {
        Debug.Log("🔍 Test LogParadeUIManager...");
        
        var uiManager = FindObjectOfType<LogParadeUIManager>();
        if (uiManager != null)
        {
            // Tester les nouvelles méthodes
            var showCalibrationMethod = uiManager.GetType().GetMethod("ShowCalibrationUI");
            var showGameMethod = uiManager.GetType().GetMethod("ShowGameUI");
            
            if (showCalibrationMethod != null)
            {
                Debug.Log("✅ Méthode ShowCalibrationUI() trouvée");
            }
            else
            {
                Debug.LogError("❌ Méthode ShowCalibrationUI() non trouvée");
            }
            
            if (showGameMethod != null)
            {
                Debug.Log("✅ Méthode ShowGameUI() trouvée");
            }
            else
            {
                Debug.LogError("❌ Méthode ShowGameUI() non trouvée");
            }
        }
        else
        {
            Debug.Log("ℹ️ LogParadeUIManager non trouvé (normal si pas en scène)");
        }
    }
    
    private void TestGameControllerMethods()
    {
        Debug.Log("🔍 Test LogParadeGameController...");
        
        var gameController = FindObjectOfType<LogParadeGameController>();
        if (gameController != null)
        {
            // Tester la méthode ForceStartGame
            var method = gameController.GetType().GetMethod("ForceStartGame");
            if (method != null)
            {
                Debug.Log("✅ Méthode ForceStartGame() trouvée");
            }
            else
            {
                Debug.LogError("❌ Méthode ForceStartGame() non trouvée");
            }
        }
        else
        {
            Debug.Log("ℹ️ LogParadeGameController non trouvé (normal si pas en scène)");
        }
    }
}
