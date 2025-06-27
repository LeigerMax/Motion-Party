using UnityEngine;
using TMPro;
using System.Collections;
using System;

/// <summary>
/// Système de calibration interactive "Déplacez-vous" pour LogParade - Version Simplifiée.
/// Utilise CalibrationTextUI pour l'interface et coordonne les modules de calibration.
/// Le joueur doit se déplacer successivement vers la lane 1 puis la lane 4.
/// </summary>
public class LogParadeCalibrationInteractive : MonoBehaviour
{    [Header("Configuration de Calibration")]
    [SerializeField] private float timeoutDuration = 15f;
    [SerializeField] private float laneDetectionTolerance = 0.5f;
    [SerializeField] private bool autoStartCalibration = true; // Nouveau paramètre
    // Note: showDebugInfo field is currently unused but kept for future debugging needs
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Références Player")]
    [SerializeField] private LogParadePlayerAvatar playerAvatar;
    [SerializeField] private LogParadeLateralTracker lateralTracker;
    
    [Header("Références Lanes")]
    [SerializeField] private Transform[] laneTransforms = new Transform[4];
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private Transform[] calibrationLogs = new Transform[4];
    
    [Header("UI References")]
    [SerializeField] private CalibrationTextUI calibrationTextUI;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Material highlightMaterial;
    
    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip timeoutSound;
    [SerializeField] private AudioSource audioSource;

    #region Modules
    private LogParadeCalibrationStateManager stateManager;
    private LogParadeCalibrationPlayerDetector playerDetector;
    private LogParadeCalibrationVisualFeedback visualFeedback;
    #endregion
    #region Events (Interface publique)
    public static event Action OnCalibrationCompleted;
    // Note: OnCalibrationFailed event is declared but not currently used
    public static event Action OnCalibrationFailed;
    public static event Action<int> OnLaneReached;
    #endregion

    #region Properties
    public bool IsCalibrationCompleted => stateManager?.IsCalibrationCompleted ?? false;
    public bool IsCalibrationActive => stateManager?.IsCalibrationActive ?? false;
    public string GetCalibrationStatus => stateManager?.GetCalibrationStatus() ?? "NotInitialized";
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeCalibration();
        
        // Si aucun CalibrationManager n'est présent et autoStart est activé, démarrer automatiquement
        if (autoStartCalibration)
        {
            LogParadeCalibrationManager calibManager = FindFirstObjectByType<LogParadeCalibrationManager>();
            if (calibManager == null)
            {
                LogParadeLogger.Log("🎯 Aucun CalibrationManager trouvé - démarrage automatique de la calibration");
                Invoke(nameof(StartCalibration), 2f); // Délai pour laisser le temps à l'initialisation
            }
            else
            {
                LogParadeLogger.Log("✅ CalibrationManager trouvé - il gèrera le démarrage");
            }
        }
    }

    void Update()
    {
        if (stateManager != null && stateManager.IsCalibrationActive)
        {
            // Obtenir l'état du joueur
            bool isOnLane1 = playerDetector?.IsPlayerOnLane(1) ?? false;
            bool isOnLane4 = playerDetector?.IsPlayerOnLane(4) ?? false;
            
            // Mettre à jour l'état
            stateManager.UpdateState(isOnLane1, isOnLane4);
            
            // Mettre à jour l'affichage des instructions avec compte à rebours
            UpdateInstructionDisplay();
            
            // Mettre à jour le timer UI
            if (calibrationTextUI != null)
            {
                calibrationTextUI.UpdateTimer(stateManager.StateTimer, timeoutDuration);
            }
        }
    }

    void OnDrawGizmos()
    {
        // Dessiner les zones de détection des lanes
        if (laneTransforms != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < 4; i++)
            {
                if (laneTransforms[i] != null)
                {
                    Gizmos.DrawWireSphere(laneTransforms[i].position, laneDetectionTolerance);
                }
            }
        }

        // Mettre en évidence la lane cible si calibration active
        if (Application.isPlaying && stateManager != null && stateManager.IsCalibrationActive)
        {
            int targetLane = -1;
            if (stateManager.CurrentState == LogParadeCalibrationStateManager.CalibrationState.WaitingForLane1) 
                targetLane = 0;
            else if (stateManager.CurrentState == LogParadeCalibrationStateManager.CalibrationState.WaitingForLane4) 
                targetLane = 3;

            if (targetLane >= 0 && laneTransforms[targetLane] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(laneTransforms[targetLane].position, 0.3f);
            }
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialise le système de calibration et tous ses modules.
    /// </summary>
    private void InitializeCalibration()
    {
        // Validation des références
        if (!ValidateReferences())
        {
            LogParadeLogger.LogError("Références manquantes!");
            return;
        }

        // Initialiser les modules
        InitializeModules();
        
        // Configurer les événements
        SetupEventHandlers();
        
        // Configuration initiale
        SetupComponents();
        
        LogParadeLogger.Log("Système de calibration initialisé avec succès.");
    }

    /// <summary>
    /// Initialise tous les modules de calibration.
    /// </summary>
    private void InitializeModules()
    {
        // State Manager
        stateManager = new LogParadeCalibrationStateManager(timeoutDuration);
        
        // Player Detector
        playerDetector = new LogParadeCalibrationPlayerDetector(
            playerAvatar,
            lateralTracker,
            laneTransforms,
            laneDetectionTolerance
        );
        
        // Visual Feedback
        visualFeedback = new LogParadeCalibrationVisualFeedback(
            calibrationLogs,
            logPrefab,
            laneTransforms,
            audioSource,
            highlightColor,
            completedColor,
            highlightMaterial,
            successSound,
            timeoutSound,
            this // MonoBehaviour pour les coroutines
        );
    }

    /// <summary>
    /// Configure les gestionnaires d'événements entre modules.
    /// </summary>
    private void SetupEventHandlers()
    {
        if (stateManager != null)
        {
            stateManager.OnCalibrationCompleted += HandleCalibrationCompleted;
            stateManager.OnCalibrationFailed += HandleCalibrationFailed;
            stateManager.OnLaneReached += HandleLaneReached;
            stateManager.OnCalibrationTimeout += HandleCalibrationTimeout;
            stateManager.OnLane1Reached += HandleLane1Reached;
            stateManager.OnLane4Reached += HandleLane4Reached;
            stateManager.OnWaitingForLane4Started += HandleWaitingForLane4Started;
        }
    }

    /// <summary>
    /// Configuration des composants initiaux.
    /// </summary>
    private void SetupComponents()
    {
        // Configurer l'AudioSource
        visualFeedback?.EnsureAudioSource(gameObject);
        
        // Configurer les rondins de calibration
        visualFeedback?.SetupCalibrationLogs();
        
        // Configurer l'UI
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ResetUI();
        }
    }    /// <summary>
    /// Valide que toutes les références nécessaires sont présentes.
    /// </summary>
    private bool ValidateReferences()
    {
        bool hasErrors = false;
        
        // Validation des lanes (critique)
        if (laneTransforms == null || laneTransforms.Length < 4)
        {
            LogParadeLogger.LogError($"laneTransforms doit avoir 4 éléments ! Trouvés: {laneTransforms?.Length ?? 0}");
            LogParadeLogger.Log("💡 SOLUTION: Assignez 4 Transforms dans l'array laneTransforms (positions des 4 lanes)");
            hasErrors = true;
        }
        else
        {
            // Vérifier que les lanes critiques sont assignées (1 et 4)
            if (laneTransforms[0] == null || laneTransforms[3] == null)
            {
                LogParadeLogger.LogError("Les lanes 1 et 4 (indices 0 et 3) sont requises pour la calibration interactive!");
                LogParadeLogger.Log("💡 SOLUTION: Assignez au minimum laneTransforms[0] et laneTransforms[3]");
                hasErrors = true;
            }
        }
        
        // Validation du CalibrationTextUI (critique)
        if (calibrationTextUI == null)
        {
            LogParadeLogger.LogError("CalibrationTextUI non assigné!");
            LogParadeLogger.Log("💡 SOLUTION: Créez un GameObject avec CalibrationTextUI et assignez-le, ou désactivez la calibration interactive");
            hasErrors = true;
        }
        
        // Validation des autres composants (warnings seulement)
        if (playerAvatar == null)
        {
            LogParadeLogger.LogWarning("PlayerAvatar non assigné - sera recherché automatiquement");
        }
        
        if (lateralTracker == null)
        {
            LogParadeLogger.LogWarning("LateralTracker non assigné - sera recherché automatiquement");
        }
        
        if (logPrefab == null)
        {
            LogParadeLogger.LogWarning("LogPrefab non assigné - calibration sans rondins visuels");
        }

        if (hasErrors)
        {
            LogParadeLogger.LogError("❌ Configuration invalide de LogParadeCalibrationInteractive");
            LogParadeLogger.Log("📖 Consultez le README.md section 'Erreur IndexOutOfRangeException' pour plus d'aide");
        }

        return !hasErrors;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Démarre la séquence de calibration.
    /// </summary>
    public void StartCalibration()
    {
        if (stateManager?.IsCalibrationActive == true)
        {
            LogParadeLogger.LogWarning("Calibration déjà en cours!");
            return;
        }

        // Démarrer la calibration
        stateManager?.StartCalibration();
          // Configurer l'UI
        if (calibrationTextUI != null)
        {
            calibrationTextUI.OnCalibrationStarted();
            calibrationTextUI.ShowInstruction("Placez-vous sur la LANE 1 et restez-y 3 secondes", true);
            calibrationTextUI.HighlightLane(0); // Lane 1
            calibrationTextUI.UpdateStatus("Calibration démarrée - Allez sur la lane 1");
        }
        
        // Feedback visuel
        visualFeedback?.HighlightLane(0); // Lane 1
        
        // S'assurer que le joueur est visible
        if (playerAvatar != null)
        {
            playerAvatar.gameObject.SetActive(true);
        }

        LogParadeLogger.Log("Calibration interactive démarrée");
    }

    /// <summary>
    /// Arrête la calibration.
    /// </summary>
    public void StopCalibration()
    {
        stateManager?.StopCalibration();
        visualFeedback?.RestoreOriginalMaterials();
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.HideCalibrationUI();
        }
        
        Debug.Log("[LogParadeCalibrationInteractive] Calibration arrêtée.");
    }

    /// <summary>
    /// Redémarre la calibration.
    /// </summary>
    public void RestartCalibration()
    {
        StopCalibration();
        StartCalibration();
    }

    /// <summary>
    /// Nettoie les ressources de calibration.
    /// </summary>
    public void CleanupCalibrationLogs()
    {
        visualFeedback?.CleanupCalibrationLogs();
    }
    #endregion

    #region Event Handlers
    private void HandleCalibrationCompleted()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.OnCalibrationCompleted();
        }
        StartCoroutine(StartGameAfterCalibration());
        OnCalibrationCompleted?.Invoke();
        Debug.Log("[LogParadeCalibrationInteractive] Calibration terminée avec succès!");
    }

    private void HandleCalibrationFailed()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.OnCalibrationFailed();
        }
        
        // Propager l'événement public
        OnCalibrationFailed?.Invoke();
        
        Debug.Log("[LogParadeCalibrationInteractive] Calibration échouée!");
    }

    private void HandleLaneReached(int laneNumber)
    {
        // Propager l'événement public
        OnLaneReached?.Invoke(laneNumber);
        
        Debug.Log($"[LogParadeCalibrationInteractive] Lane {laneNumber} atteinte!");
    }

    private void HandleCalibrationTimeout()
    {
        visualFeedback?.PlayTimeoutSound();
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowTimeoutMessage("Timeout... Recommençons!");
        }
        
        // Relancer après délai
        StartCoroutine(RestartAfterTimeout());
        
        Debug.Log("[LogParadeCalibrationInteractive] Timeout de calibration");
    }

    private void HandleLane1Reached()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowSuccessMessage("Très bien ! Maintenant, allez sur la lane 4");
            calibrationTextUI.SetLaneCompleted(0);
            calibrationTextUI.UpdateStatus("Lane 1 complétée !");
        }
        
        visualFeedback?.PlaySuccessSound();
        visualFeedback?.SetLaneCompleted(0);
        
        Debug.Log("[LogParadeCalibrationInteractive] Lane 1 atteinte avec succès!");
    }

    private void HandleLane4Reached()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowSuccessMessage("Parfait ! Calibration terminée 🎉");
            calibrationTextUI.SetLaneCompleted(3);
            calibrationTextUI.UpdateStatus("Calibration terminée !");
        }
        
        visualFeedback?.PlaySuccessSound();
        visualFeedback?.SetLaneCompleted(3);
        
        Debug.Log("[LogParadeCalibrationInteractive] Lane 4 atteinte - Calibration terminée!");
    }

    private void HandleWaitingForLane4Started()
    {
        visualFeedback?.HighlightLane(3); // Lane 4
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowInstruction("Allez sur la lane 4...", true);
            calibrationTextUI.HighlightLane(3);
            calibrationTextUI.UpdateStatus("En attente de la lane 4");
        }
        
        Debug.Log("[LogParadeCalibrationInteractive] Attente de la lane 4 démarrée");
    }
    #endregion

    #region Private Coroutines
    /// <summary>
    /// Coroutine pour redémarrer après timeout.
    /// </summary>
    private IEnumerator RestartAfterTimeout()
    {
        yield return new WaitForSeconds(2f);
        
        visualFeedback?.RestoreOriginalMaterials();
        visualFeedback?.HighlightLane(0);
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowInstruction("Placez-vous sur la lane 1...", true);
            calibrationTextUI.HighlightLane(0);
            calibrationTextUI.UpdateStatus("En attente de la lane 1");
        }
    }

    private IEnumerator StartGameAfterCalibration()
    {
        yield return new WaitForSeconds(2f);
        // Démarrer la partie normalement (timer, score, etc.)
        // ... (ajouter ici l'appel à la logique de démarrage du jeu)
        // Rendre les rondins de calibration mobiles
        visualFeedback?.MakeCalibrationLogsMobile(5f); // 5f = vitesse, à ajuster si besoin
    }
    #endregion

    #region Debug
    /// <summary>
    /// Obtient des informations de debug détaillées.
    /// </summary>
    /// <returns>Informations de debug</returns>
    public string GetDetailedDebugInfo()
    {
        if (!showDebugInfo) return "";

        string info = "=== CALIBRATION DEBUG ===\n";
        info += $"State: {stateManager?.GetCalibrationStatus()}\n";
        info += $"Timer: {stateManager?.StateTimer:F1}s\n";
        info += $"Active: {stateManager?.IsCalibrationActive}\n\n";
        
        if (playerDetector != null)
            info += playerDetector.GetDebugInfo() + "\n\n";
            
        if (visualFeedback != null)
            info += visualFeedback.GetDebugInfo() + "\n\n";
            
        info += $"CalibrationTextUI: {(calibrationTextUI != null ? "OK" : "Manquant")}";
        
        return info;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Met à jour l'affichage des instructions selon l'état de calibration
    /// </summary>
    private void UpdateInstructionDisplay()
    {
        if (calibrationTextUI == null || stateManager == null) return;
        
        switch (stateManager.CurrentState)
        {
            case LogParadeCalibrationStateManager.CalibrationState.WaitingForLane1:
                calibrationTextUI.ShowInstruction("Placez-vous sur la LANE 1 et restez-y 3 secondes", true);
                calibrationTextUI.UpdateStatus("En attente de la lane 1");
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.HoldingOnLane1:
                float remainingTime1 = stateManager.RemainingHoldTime;
                calibrationTextUI.ShowInstruction($"Restez sur la LANE 1 encore {Mathf.Ceil(remainingTime1)} secondes", true);
                calibrationTextUI.UpdateStatus($"Maintien lane 1: {Mathf.Ceil(remainingTime1)}s");
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.WaitingForLane4:
                calibrationTextUI.ShowInstruction("Maintenant, placez-vous sur la LANE 4 et restez-y 3 secondes", true);
                calibrationTextUI.UpdateStatus("En attente de la lane 4");
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.HoldingOnLane4:
                float remainingTime4 = stateManager.RemainingHoldTime;
                calibrationTextUI.ShowInstruction($"Restez sur la LANE 4 encore {Mathf.Ceil(remainingTime4)} secondes", true);
                calibrationTextUI.UpdateStatus($"Maintien lane 4: {Mathf.Ceil(remainingTime4)}s");
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.Lane1Completed:
                calibrationTextUI.ShowSuccessMessage("Lane 1 terminée ! Dirigez-vous vers la lane 4");
                calibrationTextUI.UpdateStatus("Lane 1 complétée !");
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.Completed:
                calibrationTextUI.ShowSuccessMessage("Calibration terminée ! Le jeu va commencer...");
                calibrationTextUI.UpdateStatus("Calibration réussie !");
                break;
        }
    }
    #endregion
}
