using UnityEngine;

/// <summary>
/// Script de diagnostic pour vérifier que le système de calibration 
/// et la correction des problèmes fonctionnent correctement.
/// </summary>
public class LogParadeCalibrationDiagnostic : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runDiagnosticOnStart = true;
    [SerializeField] private bool continuousDiagnostic = true;
    [SerializeField] private float diagnosticInterval = 5f;
    
    [Header("Referencias")]
    [SerializeField] private LogParadePlayerAvatar playerAvatar;
    [SerializeField] private LogParadeLateralTracker lateralTracker;
    [SerializeField] private LogParadeGameController gameController;
    [SerializeField] private LogParadeScoreManager scoreManager;
    [SerializeField] private LogParadeCalibrationManager calibrationManager;
    
    private float diagnosticTimer = 0f;
    
    void Start()
    {
        if (runDiagnosticOnStart)
        {
            RunFullDiagnostic();
        }
    }
    
    void Update()
    {
        if (continuousDiagnostic)
        {
            diagnosticTimer += Time.deltaTime;
            if (diagnosticTimer >= diagnosticInterval)
            {
                RunQuickDiagnostic();
                diagnosticTimer = 0f;
            }
        }
    }
    
    /// <summary>
    /// Lance un diagnostic complet au démarrage
    /// </summary>
    private void RunFullDiagnostic()
    {
        Debug.Log("=== DIAGNOSTIC COMPLET DU SYSTÈME DE CALIBRATION ===");
        
        // Auto-trouver les composants si non assignés
        FindComponents();
        
        // Vérifier les systèmes
        CheckPlayerMovement();
        CheckCalibrationState();
        CheckGameplayPrevention();
        CheckScorePrevention();
        
        Debug.Log("=== FIN DU DIAGNOSTIC ===");
    }
    
    /// <summary>
    /// Lance un diagnostic rapide périodique
    /// </summary>
    private void RunQuickDiagnostic()
    {
        if (LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            Debug.Log($"[Diagnostic] Calibration en cours - Player peut bouger: {CanPlayerMove()} - Score bloqué: {!LogParadeCalibrationManager.CanStartScoring()}");
        }
    }
    
    /// <summary>
    /// Trouve automatiquement les composants
    /// </summary>
    private void FindComponents()
    {
        if (playerAvatar == null) playerAvatar = FindObjectOfType<LogParadePlayerAvatar>();
        if (lateralTracker == null) lateralTracker = FindObjectOfType<LogParadeLateralTracker>();
        if (gameController == null) gameController = FindObjectOfType<LogParadeGameController>();
        if (scoreManager == null) scoreManager = FindObjectOfType<LogParadeScoreManager>();
        if (calibrationManager == null) calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        
        Debug.Log($"[Diagnostic] Composants trouvés:");
        Debug.Log($"  PlayerAvatar: {(playerAvatar != null ? "✅" : "❌")}");
        Debug.Log($"  LateralTracker: {(lateralTracker != null ? "✅" : "❌")}");
        Debug.Log($"  GameController: {(gameController != null ? "✅" : "❌")}");
        Debug.Log($"  ScoreManager: {(scoreManager != null ? "✅" : "❌")}");
        Debug.Log($"  CalibrationManager: {(calibrationManager != null ? "✅" : "❌")}");
    }
    
    /// <summary>
    /// Vérifie si le joueur peut bouger pendant la calibration
    /// </summary>
    private void CheckPlayerMovement()
    {
        Debug.Log($"[Diagnostic] === MOUVEMENT DU JOUEUR ===");
        
        if (lateralTracker != null)
        {
            // Vérifier les paramètres du tracker
            bool autoCalibDisabled = !lateralTracker.enableAutoCalibration;
            bool bypassEnabled = lateralTracker.bypassCalibrationForInteractiveMode;
            
            Debug.Log($"  Auto-calibration désactivée: {(autoCalibDisabled ? "✅" : "❌")}");
            Debug.Log($"  Bypass pour calibration interactive: {(bypassEnabled ? "✅" : "❌")}");
            
            if (autoCalibDisabled && bypassEnabled)
            {
                Debug.Log($"  ✅ Le joueur PEUT bouger pendant la calibration");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ Le joueur pourrait NE PAS pouvoir bouger pendant la calibration!");
            }
        }
        else
        {
            Debug.LogError($"  ❌ LateralTracker non trouvé - impossible de vérifier le mouvement");
        }
    }
    
    /// <summary>
    /// Vérifie l'état de la calibration
    /// </summary>
    private void CheckCalibrationState()
    {
        Debug.Log($"[Diagnostic] === ÉTAT DE LA CALIBRATION ===");
        Debug.Log($"  Calibration en cours: {LogParadeCalibrationManager.IsCalibrationInProgress}");
        Debug.Log($"  Gameplay autorisé: {LogParadeCalibrationManager.IsGameplayAllowed}");
        Debug.Log($"  Peut démarrer score: {LogParadeCalibrationManager.CanStartScoring()}");
        Debug.Log($"  Peut démarrer gameplay: {LogParadeCalibrationManager.CanStartGameplay()}");
    }
    
    /// <summary>
    /// Vérifie que le gameplay est bloqué pendant la calibration
    /// </summary>
    private void CheckGameplayPrevention()
    {
        Debug.Log($"[Diagnostic] === PRÉVENTION GAMEPLAY ===");
        
        bool gameplayBlocked = !LogParadeCalibrationManager.CanStartGameplay();
        Debug.Log($"  Gameplay bloqué pendant calibration: {(gameplayBlocked && LogParadeCalibrationManager.IsCalibrationInProgress ? "✅" : "❌")}");
        
        if (gameController != null)
        {
            // Note: On ne peut pas vérifier gameStarted car c'est private
            Debug.Log($"  GameController présent: ✅");
        }
        else
        {
            Debug.LogWarning($"  GameController absent: ⚠️");
        }
    }
    
    /// <summary>
    /// Vérifie que le score est bloqué pendant la calibration
    /// </summary>
    private void CheckScorePrevention()
    {
        Debug.Log($"[Diagnostic] === PRÉVENTION SCORE ===");
        
        bool scoreBlocked = !LogParadeCalibrationManager.CanStartScoring();
        Debug.Log($"  Score bloqué pendant calibration: {(scoreBlocked && LogParadeCalibrationManager.IsCalibrationInProgress ? "✅" : "❌")}");
        
        if (scoreManager != null)
        {
            bool isScoring = scoreManager.IsScoring;
            Debug.Log($"  ScoreManager en mode scoring: {(isScoring ? "❌ PROBLÈME!" : "✅ OK")}");
            
            if (isScoring && LogParadeCalibrationManager.IsCalibrationInProgress)
            {
                Debug.LogError($"  🚨 ERREUR: Le score fonctionne pendant la calibration!");
            }
        }
        else
        {
            Debug.LogWarning($"  ScoreManager absent: ⚠️");
        }
    }
    
    /// <summary>
    /// Vérifie si le joueur peut bouger
    /// </summary>
    private bool CanPlayerMove()
    {
        if (lateralTracker == null) return false;
        
        // Le joueur peut bouger si:
        // 1. La calibration automatique est désactivée OU
        // 2. Le bypass pour calibration interactive est activé
        return !lateralTracker.enableAutoCalibration || lateralTracker.bypassCalibrationForInteractiveMode;
    }
    
    /// <summary>
    /// Force la résolution des problèmes détectés
    /// </summary>
    [ContextMenu("Forcer la correction des problèmes")]
    public void ForceFixIssues()
    {
        Debug.Log("[Diagnostic] 🔧 Correction forcée des problèmes...");
        
        // Corriger le tracker si nécessaire
        if (lateralTracker != null)
        {
            lateralTracker.enableAutoCalibration = false;
            lateralTracker.bypassCalibrationForInteractiveMode = true;
            Debug.Log("[Diagnostic] ✅ LateralTracker corrigé");
        }
        
        // S'assurer que le score est arrêté si calibration en cours
        if (scoreManager != null && LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            if (scoreManager.IsScoring)
            {
                scoreManager.StopScoring();
                Debug.Log("[Diagnostic] ✅ Score arrêté pendant calibration");
            }
        }
        
        Debug.Log("[Diagnostic] 🔧 Correction terminée");
    }
    
    void OnGUI()
    {
        if (!runDiagnosticOnStart) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("DIAGNOSTIC CALIBRATION");
          if (LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            GUILayout.Label("🟡 Calibration EN COURS");
        }
        else if (LogParadeCalibrationManager.IsGameplayAllowed)
        {
            GUILayout.Label("🟢 Gameplay AUTORISÉ");
        }
        else
        {
            GUILayout.Label("🔴 État INCONNU");
        }
        
        GUILayout.Label($"Mouvement: {(CanPlayerMove() ? "✅" : "❌")}");
        GUILayout.Label($"Score bloqué: {(!LogParadeCalibrationManager.CanStartScoring() ? "✅" : "❌")}");
        
        if (GUILayout.Button("Relancer Diagnostic"))
        {
            RunFullDiagnostic();
        }
        
        if (GUILayout.Button("Corriger Problèmes"))
        {
            ForceFixIssues();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
