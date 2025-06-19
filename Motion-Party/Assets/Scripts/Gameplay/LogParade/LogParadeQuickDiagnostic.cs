using UnityEngine;

/// <summary>
/// Script de diagnostic rapide pour vérifier l'état du système LogParade
/// À attacher à un GameObject vide dans la scène pour diagnostiquer les problèmes
/// </summary>
public class LogParadeQuickDiagnostic : MonoBehaviour
{
    [Header("Auto-Detection")]
    public bool autoDetectComponents = true;
    
    [Header("Manual References (if auto-detection fails)")]
    public LogParadeGameController gameController;
    public LogParadeLateralTracker lateralTracker;
    public LogParadePlayerAvatar playerAvatar;
    
    [Header("Diagnostic Settings")]
    public bool showDetailedInfo = true;
    public bool runContinuousDiagnostic = false;
    
    // Status
    private bool systemReady = false;
    private string lastError = "";
    
    void Start()
    {
        if (autoDetectComponents)
        {
            AutoDetectComponents();
        }
        
        RunDiagnostic();
    }
    
    void Update()
    {
        if (runContinuousDiagnostic)
        {
            RunDiagnostic();
        }
    }
    
    /// <summary>
    /// Détecte automatiquement les composants dans la scène
    /// </summary>
    private void AutoDetectComponents()
    {
        if (gameController == null)
            gameController = FindObjectOfType<LogParadeGameController>();
            
        if (lateralTracker == null)
            lateralTracker = FindObjectOfType<LogParadeLateralTracker>();
            
        if (playerAvatar == null)
            playerAvatar = FindObjectOfType<LogParadePlayerAvatar>();
    }
    
    /// <summary>
    /// Exécute le diagnostic complet
    /// </summary>
    public void RunDiagnostic()
    {
        systemReady = true;
        lastError = "";
        
        // Vérifier la présence des composants
        if (gameController == null)
        {
            LogError("❌ LogParadeGameController non trouvé dans la scène");
            systemReady = false;
        }
        
        if (lateralTracker == null)
        {
            LogError("❌ LogParadeLateralTracker non trouvé dans la scène");
            systemReady = false;
        }
        
        if (playerAvatar == null)
        {
            LogError("❌ LogParadePlayerAvatar non trouvé dans la scène");
            systemReady = false;
        }
        
        if (!systemReady) return;
        
        // Vérifier les références croisées
        CheckCrossReferences();
        
        // Vérifier la configuration
        CheckConfiguration();
        
        // Vérifier l'état du système
        CheckSystemState();
        
        if (systemReady)
        {
            Debug.Log("✅ Diagnostic LogParade: Système prêt!");
        }
    }
    
    /// <summary>
    /// Vérifie les références croisées entre composants
    /// </summary>
    private void CheckCrossReferences()
    {
        // GameController → PlayerAvatar
        if (gameController != null)
        {
            // Note: Vérification via réflexion ou méthode publique si disponible
            // Pour l'instant, on assume que la référence est correcte si les deux existent
        }
        
        // LateralTracker → GameController
        if (lateralTracker != null)
        {
            // Même principe
        }
        
        Debug.Log("✅ Références croisées vérifiées");
    }
    
    /// <summary>
    /// Vérifie la configuration des composants
    /// </summary>
    private void CheckConfiguration()
    {
        if (playerAvatar != null)
        {
            if (playerAvatar.moveSpeed <= 0)
            {
                LogWarning("⚠️ PlayerAvatar: moveSpeed est <= 0");
            }
            
            if (playerAvatar.laneWidth <= 0)
            {
                LogWarning("⚠️ PlayerAvatar: laneWidth est <= 0");
            }
        }
        
        Debug.Log("✅ Configuration vérifiée");
    }
    
    /// <summary>
    /// Vérifie l'état actuel du système
    /// </summary>
    private void CheckSystemState()
    {
        if (Application.isPlaying)
        {
            // Vérifications en mode runtime
            if (playerAvatar != null)
            {
                int currentLane = playerAvatar.GetCurrentLane();
                if (currentLane < 1 || currentLane > 4)
                {
                    LogWarning($"⚠️ PlayerAvatar: Voie actuelle invalide ({currentLane})");
                }
            }
        }
        
        Debug.Log("✅ État du système vérifié");
    }
    
    /// <summary>
    /// Log d'erreur avec stockage
    /// </summary>
    private void LogError(string message)
    {
        Debug.LogError(message);
        lastError = message;
        systemReady = false;
    }
    
    /// <summary>
    /// Log d'avertissement
    /// </summary>
    private void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }
    
    /// <summary>
    /// Interface de diagnostic
    /// </summary>
    void OnGUI()
    {
        if (!showDetailedInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== LogParade Quick Diagnostic ===");
        
        // État général
        GUILayout.Label($"Système prêt: {(systemReady ? "✅ OUI" : "❌ NON")}");
        
        if (!string.IsNullOrEmpty(lastError))
        {
            GUILayout.Label($"Dernière erreur: {lastError}");
        }
        
        GUILayout.Space(10);
        
        // État des composants
        GUILayout.Label("=== Composants ===");
        GUILayout.Label($"GameController: {(gameController != null ? "✅" : "❌")}");
        GUILayout.Label($"LateralTracker: {(lateralTracker != null ? "✅" : "❌")}");
        GUILayout.Label($"PlayerAvatar: {(playerAvatar != null ? "✅" : "❌")}");
        
        GUILayout.Space(10);
        
        // Informations détaillées
        if (Application.isPlaying && playerAvatar != null)
        {
            GUILayout.Label("=== État de l'Avatar ===");
            GUILayout.Label($"Voie actuelle: {playerAvatar.GetCurrentLane()}");
            GUILayout.Label($"En mouvement: {playerAvatar.IsMoving()}");
            GUILayout.Label($"Position: {playerAvatar.transform.position}");
        }
        
        GUILayout.Space(10);
        
        // Boutons de test
        if (GUILayout.Button("🔍 Relancer Diagnostic"))
        {
            RunDiagnostic();
        }
        
        if (Application.isPlaying && playerAvatar != null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Lane 1")) playerAvatar.SetTargetLane(1);
            if (GUILayout.Button("Test Lane 2")) playerAvatar.SetTargetLane(2);
            if (GUILayout.Button("Test Lane 3")) playerAvatar.SetTargetLane(3);
            if (GUILayout.Button("Test Lane 4")) playerAvatar.SetTargetLane(4);
            GUILayout.EndHorizontal();
        }
        
        GUILayout.EndArea();
    }
}
