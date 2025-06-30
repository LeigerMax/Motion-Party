using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Composant UI pour afficher l'état de la calibration LogParade en temps réel
/// </summary>
public class LogParadeCalibrationStatusUI : MonoBehaviour
{
    #region Champs & Références
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button startCalibrationButton;
    [SerializeField] private Button resetCalibrationButton;
    [SerializeField] private Button bypassButton;
    [Header("Settings")]
    [SerializeField] private bool autoFindGameLauncher = true;
    [SerializeField] private float updateInterval = 0.1f;
    private LogParadeGameLauncher gameLauncher;
    private float lastUpdateTime;
    #endregion

    #region Initialisation & Cycle de Vie
    void Start()
    {
        if (autoFindGameLauncher)
        {
            gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
        }
        
        SetupButtons();
        UpdateStatus();
    }
    
    void Update()
    {
        // Mettre à jour le statut régulièrement
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateStatus();
            lastUpdateTime = Time.time;
        }
    }
    #endregion

    #region Affichage & Statut
    /// <summary>
    /// Met à jour l'affichage du statut
    /// </summary>
    private void UpdateStatus()
    {
        // Texte de statut
        if (statusText != null)
        {
            string status = GetStatusText();
            statusText.text = status;
            
            // Couleur selon l'état
            if (LogParadeGameStateController.IsGameStarted)
                statusText.color = Color.green;
            else if (LogParadeGameStateController.IsCalibrationInProgress)
                statusText.color = Color.yellow;
            else if (LogParadeGameStateController.CanStartGameplay())
                statusText.color = Color.cyan;
            else
                statusText.color = Color.white;
        }
        
        // Texte de progression
        if (progressText != null)
        {
            string progress = GetProgressText();
            progressText.text = progress;
        }
        
        // Slider de progression
        if (progressSlider != null)
        {
            float progressValue = GetProgressValue();
            progressSlider.value = progressValue;
        }
        
        // État des boutons
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Obtient le texte de statut
    /// </summary>
    private string GetStatusText()
    {
        if (LogParadeGameStateController.IsGameStarted)
            return "JEU EN COURS";
        else if (LogParadeGameStateController.IsCalibrationInProgress)
            return "CALIBRATION EN COURS";
        else if (LogParadeGameStateController.CanStartGameplay())
            return "CALIBRÉ - PRÊT À JOUER";
        else
            return "EN ATTENTE DE CALIBRATION";
    }
    
    /// <summary>
    /// Obtient le texte de progression
    /// </summary>
    private string GetProgressText()
    {
        if (LogParadeGameStateController.IsGameStarted)
            return "Gameplay actif";
        else if (LogParadeGameStateController.IsCalibrationInProgress)
        {
            // Essayer d'obtenir des détails de la calibration
            var calibSystem = FindFirstObjectByType<LogParadeCalibrationInteractive>();
            if (calibSystem != null)
            {
                string calibStatus = calibSystem.GetCalibrationStatus;
                return $"Calibration: {calibStatus}";
            }
            return "Calibration en cours...";
        }
        else if (LogParadeGameStateController.CanStartGameplay())
            return "Calibration terminée ✓";
        else
            return "Aucune calibration";
    }
    
    /// <summary>
    /// Obtient la valeur de progression (0-1)
    /// </summary>
    private float GetProgressValue()
    {
        if (LogParadeGameStateController.IsGameStarted)
            return 1f;
        else if (LogParadeGameStateController.CanStartGameplay())
            return 1f;
        else if (LogParadeGameStateController.IsCalibrationInProgress)
            return 0.5f;
        else
            return 0f;
    }
    
    /// <summary>
    /// Met à jour l'état des boutons
    /// </summary>
    private void UpdateButtonStates()
    {
        bool isCalibrating = LogParadeGameStateController.IsCalibrationInProgress;
        bool isGameStarted = LogParadeGameStateController.IsGameStarted;
        bool canStart = LogParadeGameStateController.CanStartGameplay();
        
        if (startCalibrationButton != null)
        {
            startCalibrationButton.interactable = !isCalibrating && !isGameStarted;
            
            var buttonText = startCalibrationButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (isGameStarted)
                    buttonText.text = "Jeu Actif";
                else if (isCalibrating)
                    buttonText.text = "Calibration...";
                else if (canStart)
                    buttonText.text = "Démarrer Jeu";
                else
                    buttonText.text = "Démarrer Calibration";
            }
        }
        
        if (resetCalibrationButton != null)
        {
            resetCalibrationButton.interactable = !isCalibrating;
        }
        
        if (bypassButton != null)
        {
            bypassButton.interactable = !isGameStarted && !canStart;
        }
    }
    #endregion

    #region Setup Boutons
    /// <summary>
    /// Configure les boutons
    /// </summary>
    private void SetupButtons()
    {
        if (startCalibrationButton != null)
        {
            startCalibrationButton.onClick.AddListener(StartCalibration);
        }
        
        if (resetCalibrationButton != null)
        {
            resetCalibrationButton.onClick.AddListener(ResetCalibration);
        }
        
        if (bypassButton != null)
        {
            bypassButton.onClick.AddListener(BypassCalibration);
        }
    }
    #endregion

    #region Actions Boutons
    /// <summary>
    /// Démarre la calibration
    /// </summary>
    public void StartCalibration()
    {
        if (gameLauncher == null)
        {
            LogParadeLogger.LogError("GameLauncher non trouvé!");
            return;
        }
        
        if (LogParadeGameStateController.CanStartGameplay())
        {
            LogParadeLogger.Log("Calibration déjà terminée - démarrage du jeu");
            gameLauncher.LaunchFullGame();
        }
        else
        {
            LogParadeLogger.Log("Démarrage de la calibration depuis l'UI");
            gameLauncher.StartCompleteGameProcess();
        }
    }
    
    /// <summary>
    /// Redémarre la calibration
    /// </summary>
    public void ResetCalibration()
    {
        if (gameLauncher == null)
        {
            LogParadeLogger.LogError("GameLauncher non trouvé!");
            return;
        }
        
        LogParadeLogger.Log("Reset de la calibration depuis l'UI");
        LogParadeGameStateController.ResetGameState();
        gameLauncher.StartCompleteGameProcess();
    }
    
    /// <summary>
    /// Bypass la calibration (debug)
    /// </summary>
    public void BypassCalibration()
    {
        if (gameLauncher == null)
        {
            LogParadeLogger.LogError("GameLauncher non trouvé!");
            return;
        }
        
        LogParadeLogger.LogWarning("Bypass de la calibration depuis l'UI (DEBUG)");
        gameLauncher.DebugForceLaunchGame();
    }
    #endregion

    #region Utilitaires
    /// <summary>
    /// Force la mise à jour immédiate
    /// </summary>
    public void ForceUpdate()
    {
        UpdateStatus();
    }
    #endregion
}
