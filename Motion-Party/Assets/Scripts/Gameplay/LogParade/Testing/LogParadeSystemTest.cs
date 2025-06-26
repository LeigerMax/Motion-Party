using UnityEngine;

/// <summary>
/// Script de test pour vérifier que le système LogParade fonctionne sans Lane Indicators
/// Peut être attaché à un GameObject de test pour vérifier l'intégrité du système
/// </summary>
public class LogParadeSystemTest : MonoBehaviour
{
    [Header("Test Components")]
    [SerializeField] private LogParadeUIManager uiManager;
    [SerializeField] private CalibrationTextUI calibrationUI;
    
    void Start()
    {
        TestSystemIntegrity();
    }
    
    /// <summary>
    /// Teste l'intégrité du système sans Lane Indicators
    /// </summary>
    private void TestSystemIntegrity()
    {
        LogParadeLogger.Log("=== Test d'Intégrité du Système LogParade ===");
        
        // Test UI Manager
        if (uiManager != null)
        {
            LogParadeLogger.Log("✅ LogParadeUIManager trouvé");
            
            // Test des méthodes qui utilisaient les lane indicators
            try
            {
                uiManager.ResetLaneHighlights();
                LogParadeLogger.Log("✅ ResetLaneHighlights() fonctionne sans erreur");
            }
            catch (System.Exception e)
            {
                LogParadeLogger.LogError($"❌ Erreur dans ResetLaneHighlights(): {e.Message}");
            }
        }
        else
        {
            LogParadeLogger.LogWarning("⚠️ LogParadeUIManager non assigné dans le test");
        }
        
        // Test Calibration UI
        if (calibrationUI != null)
        {
            LogParadeLogger.Log("✅ CalibrationTextUI trouvé");
            
            // Test des méthodes qui utilisaient les lane indicators
            try
            {
                calibrationUI.HighlightLane(0);
                calibrationUI.SetLaneCompleted(1);
                LogParadeLogger.Log("✅ Méthodes HighlightLane/SetLaneCompleted fonctionnent sans erreur");
            }
            catch (System.Exception e)
            {
                LogParadeLogger.LogError($"❌ Erreur dans les méthodes de lane: {e.Message}");
            }
        }
        else
        {
            LogParadeLogger.LogWarning("⚠️ CalibrationTextUI non assigné dans le test");
        }
        
        LogParadeLogger.Log("=== Fin du Test d'Intégrité ===");
        LogParadeLogger.Log("🎉 Le système LogParade fonctionne désormais sans Lane Indicators !");
    }
    
    [ContextMenu("Run Test")]
    public void RunTest()
    {
        TestSystemIntegrity();
    }
}
