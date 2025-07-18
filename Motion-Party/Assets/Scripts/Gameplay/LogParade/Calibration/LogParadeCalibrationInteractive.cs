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
{
    [Header("Configuration de Calibration")]
    [SerializeField] private float timeoutDuration = 15f;
    [SerializeField] private float laneDetectionTolerance = 0.5f;
    [SerializeField] private bool autoStartCalibration = true; 
    
    [Header("Références Player")]
    [SerializeField] private LogParadePlayerAvatar playerAvatar;
    [SerializeField] private LogParadeLateralTracker lateralTracker;
    
    [Header("Références Lanes")]
    [SerializeField] private Transform[] laneTransforms = new Transform[4];
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private Transform[] calibrationLogs = new Transform[4];
    
    [Header("UI References")]
    [SerializeField] private CalibrationTextUI calibrationTextUI;
    

    #region Modules
    private LogParadeCalibrationStateManager stateManager;
    private LogParadeCalibrationPlayerDetector playerDetector;
    private LogParadeCalibrationVisualFeedback visualFeedback;
    #endregion
    #region Events (Interface publique)
    public static event Action OnCalibrationCompleted;
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
                Invoke(nameof(StartCalibration), 2f); 
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

        // Configurer le rodin de calibration
        visualFeedback?.SetupCalibrationLog();
        
        // Configurer l'UI
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ResetUI();
        }
    }

    /// <summary>
    /// Valide que toutes les références nécessaires sont présentes.
    /// </summary>
    private bool ValidateReferences()
    {
        bool hasErrors = false;

        // Validation des lanes (critique)
        if (laneTransforms == null || laneTransforms.Length < 4)
        {
            LogParadeLogger.LogError($"laneTransforms doit avoir 4 éléments ! Trouvés: {laneTransforms?.Length ?? 0}");
            hasErrors = true;
        }
        else
        {
            // Vérifier que les lanes critiques sont assignées (1 et 4)
            if (laneTransforms[0] == null || laneTransforms[3] == null)
            {
                LogParadeLogger.LogError("Les lanes 1 et 4 (indices 0 et 3) sont requises pour la calibration interactive!");
                hasErrors = true;
            }
        }

        // Validation du CalibrationTextUI (critique)
        if (calibrationTextUI == null)
        {
            LogParadeLogger.LogError("CalibrationTextUI non assigné!");
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
            LogParadeLogger.LogError(" Configuration invalide de LogParadeCalibrationInteractive");
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
        }
        
        
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
    public void CleanupCalibrationLog()
    {
        visualFeedback?.CleanupCalibrationLog();
    }
    #endregion

    #region Event Handlers
    private void HandleCalibrationCompleted()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.OnCalibrationCompleted();
        }
        OnCalibrationCompleted?.Invoke();
    }

    private void HandleCalibrationFailed()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.OnCalibrationFailed();
        }
        
        OnCalibrationFailed?.Invoke();
        
    }

    private void HandleLaneReached(int laneNumber)
    {
        OnLaneReached?.Invoke(laneNumber);
    }

    private void HandleCalibrationTimeout()
    {
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowTimeoutMessage("Timeout... Recommençons!");
        }
        
        // Relancer après délai
        StartCoroutine(RestartAfterTimeout());
    }

    private void HandleLane1Reached()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowSuccessMessage("Très bien ! Maintenant, allez sur la lane 4");
        }
    }

    private void HandleLane4Reached()
    {
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowSuccessMessage("Parfait ! Calibration terminée 🎉");
        }
    }

    private void HandleWaitingForLane4Started()
    {
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowInstruction("Allez sur la lane 4...", true);
        }
        
    }
    #endregion

    #region Private Coroutines
    /// <summary>
    /// Coroutine pour redémarrer après timeout.
    /// </summary>
    private IEnumerator RestartAfterTimeout()
    {
        yield return new WaitForSeconds(2f);
        
        
        if (calibrationTextUI != null)
        {
            calibrationTextUI.ShowInstruction("Placez-vous sur la lane 1...", true);
        }
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
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.HoldingOnLane1:
                float remainingTime1 = stateManager.RemainingHoldTime;
                calibrationTextUI.ShowInstruction($"Restez sur la LANE 1 encore {Mathf.Ceil(remainingTime1)} secondes", true);
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.WaitingForLane4:
                calibrationTextUI.ShowInstruction("Maintenant, placez-vous sur la LANE 4 et restez-y 3 secondes", true);
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.HoldingOnLane4:
                float remainingTime4 = stateManager.RemainingHoldTime;
                calibrationTextUI.ShowInstruction($"Restez sur la LANE 4 encore {Mathf.Ceil(remainingTime4)} secondes", true);
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.Lane1Completed:
                calibrationTextUI.ShowSuccessMessage("Lane 1 terminée ! Dirigez-vous vers la lane 4");
                break;
                
            case LogParadeCalibrationStateManager.CalibrationState.Completed:
                calibrationTextUI.ShowSuccessMessage("Calibration terminée ! Le jeu va commencer...");
                break;
        }
    }
    #endregion
}
