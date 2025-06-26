using UnityEngine;
using System.Collections;

/// <summary>
/// Contrôleur amélioré pour la séquence complète de calibration et lancement du jeu LogParade.
/// Assure une coordination parfaite entre calibration, timeout, redémarrage et lancement du jeu.
/// </summary>
public class LogParadeCalibrationController : MonoBehaviour
{
    [Header("Références Système")]
    [SerializeField] private LogParadeCalibrationManager calibrationManager;
    [SerializeField] private LogParadeCalibrationInteractive calibrationInteractive;
    [SerializeField] private LogParadeGameLauncher gameLauncher;
    
    [Header("Paramètres")]
    [SerializeField] private bool autoStartCalibration = true;
    [SerializeField] private float delayBeforeGameLaunch = 2f;
    [SerializeField] private float delayBeforeRestart = 3f;
    [SerializeField] private int maxCalibrationAttempts = 3;
    
    [Header("Debug")]
    [SerializeField] private bool enableDetailedLogs = true;
    
    // État
    private int currentAttempt = 0;
    private bool isProcessingCalibration = false;
    
    void Start()
    {
        InitializeReferences();
        SetupEventHandlers();
        
        if (autoStartCalibration)
        {
            StartCoroutine(StartCalibrationDelayed());
        }
    }
    
    void OnDestroy()
    {
        CleanupEventHandlers();
    }
    
    /// <summary>
    /// Initialise automatiquement les références si elles ne sont pas assignées
    /// </summary>
    private void InitializeReferences()
    {
        if (calibrationManager == null)
            calibrationManager = FindFirstObjectByType<LogParadeCalibrationManager>();
            
        if (calibrationInteractive == null)
            calibrationInteractive = FindFirstObjectByType<LogParadeCalibrationInteractive>();
            
        if (gameLauncher == null)
            gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
        
        // Validation
        if (calibrationManager == null)
            LogParadeLogger.LogError("LogParadeCalibrationManager manquant!");
            
        if (calibrationInteractive == null)
            LogParadeLogger.LogError("LogParadeCalibrationInteractive manquant!");
            
        if (gameLauncher == null)
            LogParadeLogger.LogError("LogParadeGameLauncher manquant!");
    }
    
    /// <summary>
    /// Configure les gestionnaires d'événements pour coordonner calibration et lancement
    /// </summary>
    private void SetupEventHandlers()
    {
        // Événements de calibration
        LogParadeCalibrationInteractive.OnCalibrationCompleted += OnCalibrationSucceeded;
        LogParadeCalibrationInteractive.OnCalibrationFailed += OnCalibrationFailed;
        
        if (enableDetailedLogs)
        {
            LogParadeCalibrationInteractive.OnLaneReached += OnLaneReached;
        }
    }
    
    /// <summary>
    /// Nettoie les gestionnaires d'événements
    /// </summary>
    private void CleanupEventHandlers()
    {
        LogParadeCalibrationInteractive.OnCalibrationCompleted -= OnCalibrationSucceeded;
        LogParadeCalibrationInteractive.OnCalibrationFailed -= OnCalibrationFailed;
        
        if (enableDetailedLogs)
        {
            LogParadeCalibrationInteractive.OnLaneReached -= OnLaneReached;
        }
    }
    
    /// <summary>
    /// Démarre la calibration avec un petit délai
    /// </summary>
    private IEnumerator StartCalibrationDelayed()
    {
        yield return new WaitForSeconds(1f); // Laisser le temps à l'initialisation
        
        LogParadeLogger.Log("🎯 LogParadeCalibrationController - Démarrage de la calibration...");
        StartCalibrationProcess();
    }
    
    /// <summary>
    /// Démarre le processus de calibration
    /// </summary>
    public void StartCalibrationProcess()
    {
        if (isProcessingCalibration)
        {
            LogParadeLogger.LogWarning("Calibration déjà en cours de traitement");
            return;
        }
        
        isProcessingCalibration = true;
        currentAttempt++;
        
        LogParadeLogger.Log($"🎯 Démarrage de la calibration (tentative {currentAttempt}/{maxCalibrationAttempts})");
        
        // Démarrer via le manager si disponible, sinon directement
        if (calibrationManager != null)
        {
            calibrationManager.StartCalibrationProcess();
        }
        else if (calibrationInteractive != null)
        {
            calibrationInteractive.StartCalibration();
        }
        else
        {
            LogParadeLogger.LogError("❌ Aucun système de calibration disponible!");
            isProcessingCalibration = false;
        }
    }
    
    /// <summary>
    /// Appelé quand la calibration réussit
    /// </summary>
    private void OnCalibrationSucceeded()
    {
        if (!isProcessingCalibration) return;
        
        LogParadeLogger.Log("✅ Calibration réussie! Lancement du jeu...");
        
        // Réinitialiser les tentatives
        currentAttempt = 0;
        
        // Lancer le jeu après délai
        StartCoroutine(LaunchGameAfterDelay());
    }
    
    /// <summary>
    /// Appelé quand la calibration échoue
    /// </summary>
    private void OnCalibrationFailed()
    {
        if (!isProcessingCalibration) return;
        
        LogParadeLogger.LogWarning($"❌ Calibration échouée (tentative {currentAttempt}/{maxCalibrationAttempts})");
        
        if (currentAttempt < maxCalibrationAttempts)
        {
            // Redémarrer automatiquement
            StartCoroutine(RestartCalibrationAfterDelay());
        }
        else
        {
            // Trop de tentatives
            LogParadeLogger.LogError($"❌ Calibration échouée après {maxCalibrationAttempts} tentatives");
            HandleMaxAttemptsReached();
        }
    }
    
    /// <summary>
    /// Appelé lors de l'atteinte d'une lane (pour debug)
    /// </summary>
    private void OnLaneReached(int laneNumber)
    {
        if (enableDetailedLogs)
        {
            LogParadeLogger.Log($"🎯 Lane {laneNumber} atteinte pendant la calibration");
        }
    }
    
    /// <summary>
    /// Lance le jeu après un délai
    /// </summary>
    private IEnumerator LaunchGameAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeGameLaunch);
        
        isProcessingCalibration = false;
        
        if (gameLauncher != null)
        {
            LogParadeLogger.Log("🚀 Lancement du jeu LogParade...");
            gameLauncher.LaunchFullGame();
        }
        else
        {
            LogParadeLogger.LogError("❌ GameLauncher non disponible pour lancer le jeu!");
        }
    }
    
    /// <summary>
    /// Redémarre la calibration après un délai
    /// </summary>
    private IEnumerator RestartCalibrationAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeRestart);
        
        LogParadeLogger.Log("🔄 Redémarrage automatique de la calibration...");
        
        isProcessingCalibration = false;
        StartCalibrationProcess();
    }
    
    /// <summary>
    /// Gère le cas où le nombre max de tentatives est atteint
    /// </summary>
    private void HandleMaxAttemptsReached()
    {
        isProcessingCalibration = false;
        
        LogParadeLogger.LogError($"❌ Calibration impossible après {maxCalibrationAttempts} tentatives");
        
        // Optionnel: afficher un message à l'utilisateur ou prendre d'autres actions
        // Ici on peut soit:
        // 1. Redémarrer avec un délai plus long
        // 2. Lancer le jeu sans calibration
        // 3. Afficher un écran d'erreur
        
        // Pour l'instant, on redémarre après un délai plus long
        StartCoroutine(FinalRestartAttempt());
    }
    
    /// <summary>
    /// Tentative finale de redémarrage avec délai plus long
    /// </summary>
    private IEnumerator FinalRestartAttempt()
    {
        LogParadeLogger.Log("🔄 Tentative finale de calibration dans 10 secondes...");
        yield return new WaitForSeconds(10f);
        
        // Réinitialiser le compteur pour une nouvelle série de tentatives
        currentAttempt = 0;
        StartCalibrationProcess();
    }
    
    /// <summary>
    /// Force le lancement du jeu sans calibration (pour debug)
    /// </summary>
    [ContextMenu("Force Launch Game")]
    public void ForceLaunchGame()
    {
        LogParadeLogger.Log("🧪 Lancement forcé du jeu (sans calibration)");
        
        isProcessingCalibration = false;
        
        if (gameLauncher != null)
        {
            gameLauncher.LaunchFullGame();
        }
        else
        {
            LogParadeLogger.LogError("❌ GameLauncher non disponible!");
        }
    }
    
    /// <summary>
    /// Force le redémarrage de la calibration (pour debug)
    /// </summary>
    [ContextMenu("Force Restart Calibration")]
    public void ForceRestartCalibration()
    {
        LogParadeLogger.Log("🧪 Redémarrage forcé de la calibration");
        
        // Arrêter la calibration actuelle
        if (calibrationInteractive != null)
        {
            calibrationInteractive.StopCalibration();
        }
        
        // Réinitialiser l'état
        isProcessingCalibration = false;
        currentAttempt = 0;
        
        // Redémarrer
        StartCalibrationProcess();
    }
    
    /// <summary>
    /// Affiche l'état actuel du système
    /// </summary>
    [ContextMenu("Show Status")]
    public void ShowStatus()
    {
        LogParadeLogger.Log("📊 État du LogParadeCalibrationController:");
        LogParadeLogger.Log($"   En cours de traitement: {isProcessingCalibration}");
        LogParadeLogger.Log($"   Tentative actuelle: {currentAttempt}/{maxCalibrationAttempts}");
        LogParadeLogger.Log($"   CalibrationManager: {(calibrationManager != null ? "✅" : "❌")}");
        LogParadeLogger.Log($"   CalibrationInteractive: {(calibrationInteractive != null ? "✅" : "❌")}");
        LogParadeLogger.Log($"   GameLauncher: {(gameLauncher != null ? "✅" : "❌")}");
        
        if (calibrationInteractive != null)
        {
            LogParadeLogger.Log($"   État calibration: {calibrationInteractive.GetCalibrationStatus}");
            LogParadeLogger.Log($"   Calibration active: {calibrationInteractive.IsCalibrationActive}");
            LogParadeLogger.Log($"   Calibration terminée: {calibrationInteractive.IsCalibrationCompleted}");
        }
    }
}
