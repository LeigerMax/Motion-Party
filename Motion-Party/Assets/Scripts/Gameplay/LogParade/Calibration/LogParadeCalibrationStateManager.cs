using UnityEngine;
using System;

/// <summary>
/// Gestionnaire d'états pour la calibration interactive LogParade.
/// Gère la machine à états de calibration et les transitions avec timing de 3 secondes par lane.
/// </summary>
public class LogParadeCalibrationStateManager
{
    #region Enum et État
    public enum CalibrationState
    {
        NotStarted,
        WaitingForLane1,
        HoldingOnLane1,       
        Lane1Completed,
        WaitingForLane4,
        HoldingOnLane4,        
        Completed,
        Failed
    }

    private CalibrationState currentState = CalibrationState.NotStarted;
    private float stateTimer = 0f;
    private bool isCalibrationActive = false;
    private float timeoutDuration;
    private const float LANE_HOLD_DURATION = 3f;
    #endregion

    #region Events
    public event Action OnCalibrationCompleted;
    public event Action OnCalibrationFailed;
    public event Action<int> OnLaneReached;
    public event Action OnCalibrationTimeout;
    public event Action OnLane1Reached;
    public event Action OnLane4Reached;
    public event Action OnWaitingForLane4Started;
    #endregion
     
    #region Properties
    public CalibrationState CurrentState => currentState;
    public float StateTimer => stateTimer;
    public bool IsCalibrationActive => isCalibrationActive;
    public bool IsCalibrationCompleted => currentState == CalibrationState.Completed && !isCalibrationActive;
    public float LaneHoldDuration => LANE_HOLD_DURATION;
    public float RemainingHoldTime => LANE_HOLD_DURATION - stateTimer; // Temps restant pour le maintien
    #endregion

    #region Constructor
    public LogParadeCalibrationStateManager(float timeout = 15f)
    {
        timeoutDuration = timeout;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Démarre la calibration.
    /// </summary>
    public void StartCalibration()
    {        if (isCalibrationActive)
        {
            LogParadeLogger.LogWarning("Calibration déjà en cours!");
            return;
        }

        isCalibrationActive = true;
        currentState = CalibrationState.WaitingForLane1;
        stateTimer = 0f;
    }

    /// <summary>
    /// Arrête la calibration.
    /// </summary>
    public void StopCalibration()
    {
        isCalibrationActive = false;
        currentState = CalibrationState.NotStarted;
        stateTimer = 0f;
        
        LogParadeLogger.Log("Calibration arrêtée.");
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
    /// Met à jour l'état de la calibration (à appeler dans Update).
    /// </summary>
    public void UpdateState(bool isPlayerOnLane1, bool isPlayerOnLane4)
    {
        if (!isCalibrationActive) return;

        UpdateTimer();
        UpdateCalibrationLogic(isPlayerOnLane1, isPlayerOnLane4);
    }

    /// <summary>
    /// Obtient le statut de calibration sous forme de string.
    /// </summary>
    public string GetCalibrationStatus()
    {
        return currentState.ToString();
    }
    #endregion

    #region Private Methods    
    /// <summary>
    /// Met à jour la logique de calibration selon l'état actuel.
    /// </summary>
    private void UpdateCalibrationLogic(bool isPlayerOnLane1, bool isPlayerOnLane4)
    {
        switch (currentState)
        {
            case CalibrationState.WaitingForLane1:
                if (isPlayerOnLane1)
                {
                    StartHoldingOnLane1();
                }
                break;

            case CalibrationState.HoldingOnLane1:
                if (!isPlayerOnLane1)
                {
                    // Joueur a quitté la lane, revenir à l'attente
                    ReturnToWaitingForLane1();
                }
                else if (stateTimer >= LANE_HOLD_DURATION)
                {
                    // 3 secondes écoulées sur lane 1
                    CompleteLane1();
                }
                break;

            case CalibrationState.Lane1Completed:
                // Transition automatique vers lane 4 après affichage du succès
                if (stateTimer > 1f)
                {
                    StartWaitingForLane4();
                }
                break;

            case CalibrationState.WaitingForLane4:
                if (isPlayerOnLane4)
                {
                    StartHoldingOnLane4();
                }
                break;

            case CalibrationState.HoldingOnLane4:
                if (!isPlayerOnLane4)
                {
                    // Joueur a quitté la lane, revenir à l'attente
                    ReturnToWaitingForLane4();
                }
                else if (stateTimer >= LANE_HOLD_DURATION)
                {
                    // 3 secondes écoulées sur lane 4
                    CompleteLane4();
                }
                break;

            case CalibrationState.Completed:
                if (stateTimer > 2f) // Afficher le succès pendant 2 secondes
                {
                    CompleteCalibration();
                }
                break;
        }
    }  

    private void SetState(CalibrationState newState, Action onEnter = null)
    {
        currentState = newState;
        stateTimer = 0f;
        onEnter?.Invoke();
    }
    
    /// <summary>
    /// Met à jour le timer et gère les timeouts.
    /// </summary>
    private void UpdateTimer()
    {
        stateTimer += Time.deltaTime;

        // Vérifier le timeout pour les états d'attente (pas pour les états de maintien)
        if ((currentState == CalibrationState.WaitingForLane1 || currentState == CalibrationState.WaitingForLane4)
            && stateTimer >= timeoutDuration)
        {
            HandleTimeout();
        }
    }

    /// <summary>
    /// Démarre le maintien sur lane 1.
    /// </summary>
    private void StartHoldingOnLane1()
    {
        SetState(CalibrationState.HoldingOnLane1, () => OnLane1Reached?.Invoke());
    }

    /// <summary>
    /// Retourne à l'attente de lane 1 si le joueur quitte la lane.
    /// </summary>
    private void ReturnToWaitingForLane1()
    {
        SetState(CalibrationState.WaitingForLane1);
    }

    /// <summary>
    /// Termine avec succès le maintien sur lane 1.
    /// </summary>
    private void CompleteLane1()
    {
        SetState(CalibrationState.Lane1Completed, () => OnLane1Reached?.Invoke());
    }

    /// <summary>
    /// Démarre l'attente de la lane 4.
    /// </summary>
    private void StartWaitingForLane4()
    {
       SetState(CalibrationState.WaitingForLane4, () => OnWaitingForLane4Started?.Invoke());
    }

    /// <summary>
    /// Démarre le maintien sur lane 4.
    /// </summary>
    private void StartHoldingOnLane4()
    {
        SetState(CalibrationState.HoldingOnLane4, () => OnLaneReached?.Invoke(4));
    }

    /// <summary>
    /// Retourne à l'attente de lane 4 si le joueur quitte la lane.
    /// </summary>
    private void ReturnToWaitingForLane4()
    {
        SetState(CalibrationState.WaitingForLane4);
    }

    /// <summary>
    /// Termine avec succès le maintien sur lane 4.
    /// </summary>
    private void CompleteLane4()
    {
        SetState(CalibrationState.Completed, () => OnLane4Reached?.Invoke());
    }

    /// <summary>
    /// Finalise la calibration.
    /// </summary>
    private void CompleteCalibration()
    {
        isCalibrationActive = false;
        
        OnCalibrationCompleted?.Invoke();
        
        LogParadeLogger.Log("Calibration interactive terminée avec succès!");
    }

    /// <summary>
    /// Gère le timeout de calibration.
    /// </summary>
    private void HandleTimeout()
    {
        OnCalibrationTimeout?.Invoke();
        
        // Redémarrer après timeout
        currentState = CalibrationState.WaitingForLane1;
        stateTimer = 0f;
        
        LogParadeLogger.Log("Timeout de calibration - relancement...");
    }
    #endregion
}
