using UnityEngine;

/// <summary>
/// Bootstrap pour s'assurer que la calibration est correctement initialisée
/// au démarrage du jeu et que tous les systèmes respectent les règles de calibration.
/// </summary>
public class LogParadeCalibrationBootstrap : MonoBehaviour
{
    [Header("Bootstrap Settings")]
    [SerializeField] private bool enableCalibrationOnStart = true;
    [SerializeField] private bool validateSystemsOnStart = true;
    [SerializeField] private bool showBootstrapLogs = true;
    
    [Header("Auto-Detection")]
    [SerializeField] private bool autoFindSystems = true;
    
    // Références aux systèmes
    private LogParadeCalibrationManager calibrationManager;
    private LogParadeGameController gameController;
    private LogParadeScoreManager scoreManager;
    private LogParadeGameTimer gameTimer;
    
    void Awake()
    {
        // S'exécuter en premier
        if (showBootstrapLogs)
        {
            Debug.Log("[LogParadeCalibrationBootstrap] 🚀 Initialisation du système de calibration...");
        }
        
        if (autoFindSystems)
        {
            FindSystems();
        }
        
        if (validateSystemsOnStart)
        {
            ValidateSystems();
        }
        
        if (enableCalibrationOnStart)
        {
            InitializeCalibration();
        }
    }
    
    void Start()
    {
        // Vérifier que la calibration est bien active
        if (enableCalibrationOnStart)
        {
            CheckCalibrationStatus();
        }
    }
    
    /// <summary>
    /// Trouve automatiquement tous les systèmes dans la scène
    /// </summary>
    private void FindSystems()
    {
        calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        gameController = FindObjectOfType<LogParadeGameController>();
        scoreManager = FindObjectOfType<LogParadeScoreManager>();
        gameTimer = FindObjectOfType<LogParadeGameTimer>();
        
        if (showBootstrapLogs)
        {
            Debug.Log($"[LogParadeCalibrationBootstrap] Systèmes trouvés:");
            Debug.Log($"  - CalibrationManager: {(calibrationManager != null ? "✅" : "❌")}");
            Debug.Log($"  - GameController: {(gameController != null ? "✅" : "❌")}");
            Debug.Log($"  - ScoreManager: {(scoreManager != null ? "✅" : "❌")}");
            Debug.Log($"  - GameTimer: {(gameTimer != null ? "✅" : "❌")}");
        }
    }
    
    /// <summary>
    /// Valide que tous les systèmes respectent les règles de calibration
    /// </summary>
    private void ValidateSystems()
    {
        bool allValid = true;
        
        // Vérifier que le GameController ne démarre pas immédiatement
        if (gameController != null)
        {
            // Désactiver temporairement le GameController jusqu'à la fin de la calibration
            if (gameController.gameObject.activeInHierarchy)
            {
                if (showBootstrapLogs)
                {
                    Debug.Log("[LogParadeCalibrationBootstrap] GameController trouvé actif - OK");
                }
            }
        }
        else
        {
            Debug.LogWarning("[LogParadeCalibrationBootstrap] ⚠️ GameController non trouvé!");
            allValid = false;
        }
        
        // Vérifier que le ScoreManager ne démarre pas le scoring
        if (scoreManager != null)
        {
            if (scoreManager.IsScoring)
            {
                Debug.LogWarning("[LogParadeCalibrationBootstrap] ⚠️ ScoreManager déjà en mode scoring - arrêt forcé");
                scoreManager.StopScoring();
            }
        }
          // Vérifier que le GameTimer n'est pas démarré
        if (gameTimer != null)
        {
            if (gameTimer.IsGameActive)
            {
                Debug.LogWarning("[LogParadeCalibrationBootstrap] ⚠️ GameTimer déjà actif - arrêt forcé");
                gameTimer.StopGame();
            }
        }
        
        if (showBootstrapLogs)
        {
            Debug.Log($"[LogParadeCalibrationBootstrap] Validation des systèmes: {(allValid ? "✅ OK" : "⚠️ Problèmes détectés")}");
        }
    }
    
    /// <summary>
    /// Initialise le système de calibration
    /// </summary>
    private void InitializeCalibration()
    {
        if (calibrationManager == null)
        {
            Debug.LogError("[LogParadeCalibrationBootstrap] ❌ CalibrationManager non trouvé - création automatique...");
            CreateCalibrationManager();
        }
        
        if (calibrationManager != null)
        {
            // Démarrer le processus de calibration
            calibrationManager.StartCalibrationProcess();
            
            if (showBootstrapLogs)
            {
                Debug.Log("[LogParadeCalibrationBootstrap] ✅ Calibration initialisée");
            }
        }
        else
        {
            Debug.LogError("[LogParadeCalibrationBootstrap] ❌ Impossible d'initialiser la calibration!");
        }
    }
    
    /// <summary>
    /// Crée automatiquement un CalibrationManager si absent
    /// </summary>
    private void CreateCalibrationManager()
    {
        GameObject calibrationGO = new GameObject("CalibrationManager_Auto");
        calibrationManager = calibrationGO.AddComponent<LogParadeCalibrationManager>();
        
        if (showBootstrapLogs)
        {
            Debug.Log("[LogParadeCalibrationBootstrap] CalibrationManager créé automatiquement");
        }
    }
    
    /// <summary>
    /// Vérifie l'état de la calibration
    /// </summary>
    private void CheckCalibrationStatus()
    {
        if (showBootstrapLogs)
        {
            Debug.Log($"[LogParadeCalibrationBootstrap] État de calibration:");
            Debug.Log($"  - Calibration en cours: {LogParadeCalibrationManager.IsCalibrationInProgress}");
            Debug.Log($"  - Gameplay autorisé: {LogParadeCalibrationManager.IsGameplayAllowed}");
            Debug.Log($"  - Peut démarrer score: {LogParadeCalibrationManager.CanStartScoring()}");
            Debug.Log($"  - Peut démarrer gameplay: {LogParadeCalibrationManager.CanStartGameplay()}");
        }
    }
    
    /// <summary>
    /// Force le démarrage du gameplay (pour debug uniquement)
    /// </summary>
    [System.Obsolete("Utiliser uniquement pour les tests")]
    public void ForceStartGameplay()
    {
        Debug.LogWarning("[LogParadeCalibrationBootstrap] ⚠️ FORCE START GAMEPLAY - Pour debug uniquement!");
        LogParadeCalibrationManager.ForceEnableGameplay();
        
        if (gameController != null)
        {
            gameController.gameObject.SetActive(true);
        }
        
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
        }
        
        if (gameTimer != null)
        {
            gameTimer.LaunchLevel();
        }
    }
    
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Afficher l'état visuel en mode éditeur
        if (LogParadeCalibrationManager.IsCalibrationInProgress)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        else if (LogParadeCalibrationManager.IsGameplayAllowed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
    #endif
}
