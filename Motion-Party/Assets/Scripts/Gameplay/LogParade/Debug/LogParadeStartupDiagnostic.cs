using UnityEngine;

/// <summary>
/// Script de diagnostic pour identifier pourquoi la calibration ne démarre pas automatiquement
/// </summary>
public class LogParadeStartupDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool runDiagnosticOnStart = true;
    [SerializeField] private bool showDetailedInfo = true;
    
    void Start()
    {
        if (runDiagnosticOnStart)
        {
            StartCoroutine(RunDiagnostic());
        }
    }
    
    private System.Collections.IEnumerator RunDiagnostic()
    {
        yield return new WaitForSeconds(0.5f); // Attendre que tout soit initialisé
        
        LogParadeLogger.Log("🔍 === DIAGNOSTIC DE DÉMARRAGE LOGPARADE ===");
        
        // 1. Vérifier GameStateController
        DiagnoseGameStateController();
        
        // 2. Vérifier GameLauncher
        DiagnoseGameLauncher();
        
        // 3. Vérifier CalibrationManager
        DiagnoseCalibrationManager();
        
        // 4. Vérifier CalibrationInteractive
        DiagnoseCalibrationInteractive();
        
        // 5. Vérifier PlayerAvatar
        DiagnosePlayerAvatar();
        
        LogParadeLogger.Log("🔍 === FIN DU DIAGNOSTIC ===");
        
        // Recommandations
        yield return new WaitForSeconds(1f);
        ProvideRecommendations();
    }
    
    private void DiagnoseGameStateController()
    {
        LogParadeLogger.Log("📊 DIAGNOSTIC: GameStateController");
        
        var controller = FindFirstObjectByType<LogParadeGameStateController>();
        if (controller != null)
        {
            LogParadeLogger.Log($"  ✅ GameStateController trouvé");
            LogParadeLogger.Log($"  - Calibration en cours: {LogParadeGameStateController.IsCalibrationInProgress}");
            LogParadeLogger.Log($"  - Gameplay autorisé: {LogParadeGameStateController.IsGameplayAllowed}");
            LogParadeLogger.Log($"  - Jeu démarré: {LogParadeGameStateController.IsGameStarted}");
            LogParadeLogger.Log($"  - Statut: {LogParadeGameStateController.GetCurrentStatusText()}");
        }
        else
        {
            LogParadeLogger.LogError("  ❌ GameStateController MANQUANT!");
        }
    }
    
    private void DiagnoseGameLauncher()
    {
        LogParadeLogger.Log("🚀 DIAGNOSTIC: GameLauncher");
        
        var launcher = FindFirstObjectByType<LogParadeGameLauncher>();
        if (launcher != null)
        {
            LogParadeLogger.Log($"  ✅ GameLauncher trouvé");
            
            // Utiliser la réflexion pour vérifier autoStartCalibrationOnStart
            var autoStartField = launcher.GetType().GetField("autoStartCalibrationOnStart", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (autoStartField != null)
            {
                bool autoStart = (bool)autoStartField.GetValue(launcher);
                LogParadeLogger.Log($"  - Auto-start activé: {autoStart}");
            }
            
            // Vérifier isLaunching
            var isLaunchingField = launcher.GetType().GetField("isLaunching", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (isLaunchingField != null)
            {
                bool isLaunching = (bool)isLaunchingField.GetValue(launcher);
                LogParadeLogger.Log($"  - En cours de lancement: {isLaunching}");
            }
        }
        else
        {
            LogParadeLogger.LogError("  ❌ GameLauncher MANQUANT!");
        }
    }
    
    private void DiagnoseCalibrationManager()
    {
        LogParadeLogger.Log("🎯 DIAGNOSTIC: CalibrationManager");
        
        var manager = FindFirstObjectByType<LogParadeCalibrationManager>();
        if (manager != null)
        {
            LogParadeLogger.Log($"  ✅ CalibrationManager trouvé");
            
            // Vérifier enableCalibrationOnStart
            var enableField = manager.GetType().GetField("enableCalibrationOnStart", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (enableField != null)
            {
                bool enableOnStart = (bool)enableField.GetValue(manager);
                LogParadeLogger.Log($"  - Démarrage auto activé: {enableOnStart}");
            }
            
            // Vérifier isCalibrating
            var isCalibratingField = manager.GetType().GetField("isCalibrating", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (isCalibratingField != null)
            {
                bool isCalibrating = (bool)isCalibratingField.GetValue(manager);
                LogParadeLogger.Log($"  - En cours de calibration: {isCalibrating}");
            }
        }
        else
        {
            LogParadeLogger.LogError("  ❌ CalibrationManager MANQUANT!");
        }
    }
    
    private void DiagnoseCalibrationInteractive()
    {
        LogParadeLogger.Log("🎮 DIAGNOSTIC: CalibrationInteractive");
        
        var interactive = FindFirstObjectByType<LogParadeCalibrationInteractive>();
        if (interactive != null)
        {
            LogParadeLogger.Log($"  ✅ CalibrationInteractive trouvé");
            LogParadeLogger.Log($"  - Calibration terminée: {interactive.IsCalibrationCompleted}");
            LogParadeLogger.Log($"  - Calibration active: {interactive.IsCalibrationActive}");
            LogParadeLogger.Log($"  - Statut: {interactive.GetCalibrationStatus}");
        }
        else
        {
            LogParadeLogger.LogError("  ❌ CalibrationInteractive MANQUANT!");
        }
    }
    
    private void DiagnosePlayerAvatar()
    {
        LogParadeLogger.Log("👤 DIAGNOSTIC: PlayerAvatar");
        
        var player = FindFirstObjectByType<LogParadePlayerAvatar>();
        if (player != null)
        {
            LogParadeLogger.Log($"  ✅ PlayerAvatar trouvé");
            LogParadeLogger.Log($"  - GameObject actif: {player.gameObject.activeInHierarchy}");
        }
        else
        {
            LogParadeLogger.LogError("  ❌ PlayerAvatar MANQUANT!");
        }
    }
    
    private void ProvideRecommendations()
    {
        LogParadeLogger.Log("💡 === RECOMMANDATIONS ===");
        
        var launcher = FindFirstObjectByType<LogParadeGameLauncher>();
        var manager = FindFirstObjectByType<LogParadeCalibrationManager>();
        
        if (launcher == null)
        {
            LogParadeLogger.LogError("🔧 AJOUTEZ un GameObject avec LogParadeGameLauncher dans la scène");
        }
        
        if (manager == null)
        {
            LogParadeLogger.LogError("🔧 AJOUTEZ un GameObject avec LogParadeCalibrationManager dans la scène");
        }
        
        if (launcher != null && manager != null)
        {
            LogParadeLogger.Log("🔧 Pour démarrer manuellement la calibration:");
            LogParadeLogger.Log("   - Clic droit sur GameLauncher → 'Debug - Force Start Calibration NOW'");
            LogParadeLogger.Log("   - Ou appelez launcher.ForceStartCalibrationNow() depuis un script");
        }
        
        LogParadeLogger.Log("💡 === FIN DES RECOMMANDATIONS ===");
    }
    
    /// <summary>
    /// Lance le diagnostic manuellement
    /// </summary>
    [ContextMenu("Run Diagnostic Now")]
    public void RunDiagnosticNow()
    {
        StartCoroutine(RunDiagnostic());
    }
}
