using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contrôleur d'interface de calibration pour LogParade.
/// Responsable de l'affichage et de la gestion de l'interface de calibration.
/// Gère la progression, les messages et l'état visuel de la calibration.
/// </summary>
public class LogParadeCalibrationUIController
{
    #region Dependencies
    private GameObject calibrationPanel;
    private Slider calibrationProgressSlider;
    private TMP_Text calibrationText;
    #endregion

    #region State
    private bool isCalibrationActive;
    private float currentProgress;
    private string currentCalibrationMessage;
    #endregion

    #region Events
    public System.Action OnCalibrationUIShown;
    public System.Action OnCalibrationUIHidden;
    public System.Action<float> OnProgressUpdated;
    public System.Action<string> OnMessageUpdated;
    #endregion

    #region Properties
    public bool IsCalibrationActive => isCalibrationActive;
    public float CurrentProgress => currentProgress;
    public string CurrentMessage => currentCalibrationMessage;
    public bool IsInitialized { get; private set; }
    #endregion

    #region Logging
    private void Log(string message)
    {
        LogParadeLogger.Log($"[CalibrationUI] {message}");
    }
    private void LogWarning(string message)
    {
        LogParadeLogger.LogWarning($"[CalibrationUI] {message}");
    }
    private void LogError(string message)
    {
        LogParadeLogger.LogError($"[CalibrationUI] {message}");
    }
    #endregion

    #region Constructor
    public LogParadeCalibrationUIController(GameObject calibrationPanel, Slider calibrationProgressSlider, TMP_Text calibrationText)
    {
        this.calibrationPanel = calibrationPanel;
        this.calibrationProgressSlider = calibrationProgressSlider;
        this.calibrationText = calibrationText;

        Initialize();
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        if (!ValidateComponents())
        {
            LogError("Composants invalides");
            return;
        }

        // État initial
        isCalibrationActive = false;
        currentProgress = 0f;
        currentCalibrationMessage = "Calibration prête";

        // Configuration initiale
        SetupInitialState();

        IsInitialized = true;
        Log("Initialisé avec succès");
    }    private bool ValidateComponents()
    {
        if (calibrationPanel == null)
        {
            LogWarning("Panel de calibration non assigné");
        }

        if (calibrationProgressSlider == null)
        {
            // LogVerbose supprimé (inutile)
        }

        if (calibrationText == null)
        {
            LogWarning("Texte de calibration non assigné");
        }

        // Au moins un composant doit être assigné pour être fonctionnel
        return calibrationPanel != null || calibrationText != null;
    }

    private void SetupInitialState()
    {
        // Masque le panel initialement
        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(false);
        }

        // Initialise la progression à 0
        if (calibrationProgressSlider != null)
        {
            calibrationProgressSlider.value = 0f;
            calibrationProgressSlider.minValue = 0f;
            calibrationProgressSlider.maxValue = 1f;
        }

        // Initialise le texte
        if (calibrationText != null)
        {
            calibrationText.text = currentCalibrationMessage;
        }
    }
    #endregion

    #region Calibration Control
    /// <summary>
    /// Affiche l'interface de calibration avec une progression spécifique.
    /// </summary>
    public void ShowCalibrationUI(float progress)
    {
        if (!IsInitialized) return;

        isCalibrationActive = true;
        currentProgress = Mathf.Clamp01(progress);

        // Active le panel
        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(true);
        }

        // Met à jour la progression
        UpdateProgress(currentProgress);

        // Met à jour le message
        string progressMessage = $"Calibration... {currentProgress:P0}";
        UpdateMessage(progressMessage);

        OnCalibrationUIShown?.Invoke();
    }

    /// <summary>
    /// Affiche l'interface de calibration (overload pour bool).
    /// </summary>
    public void ShowCalibrationUI(bool show)
    {
        if (!IsInitialized) return;

        if (show)
        {
            isCalibrationActive = true;

            if (calibrationPanel != null)
            {
                calibrationPanel.SetActive(true);
            }

            UpdateMessage("Calibration en cours...");
            OnCalibrationUIShown?.Invoke();
        }
        else
        {
            HideCalibrationUI();
        }
    }

    /// <summary>
    /// Masque l'interface de calibration.
    /// </summary>
    public void HideCalibrationUI()
    {
        if (!IsInitialized) return;

        isCalibrationActive = false;

        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(false);
        }

        OnCalibrationUIHidden?.Invoke();
    }

    /// <summary>
    /// Met à jour la progression de la calibration.
    /// </summary>
    public void UpdateCalibrationProgress(float progress)
    {
        if (!IsInitialized) return;

        currentProgress = Mathf.Clamp01(progress);
        UpdateProgress(currentProgress);

        // Met à jour automatiquement le message de progression
        if (isCalibrationActive)
        {
            string progressMessage = $"Calibration... {currentProgress:P0}";
            UpdateMessage(progressMessage);
        }
    }

    /// <summary>
    /// Met à jour le texte de calibration.
    /// </summary>
    public void UpdateCalibrationText(string text)
    {
        if (!IsInitialized) return;
        UpdateMessage(text);
    }
    #endregion

    #region Internal Updates
    private void UpdateProgress(float progress)
    {
        if (calibrationProgressSlider != null)
        {
            calibrationProgressSlider.value = progress;
        }

        OnProgressUpdated?.Invoke(progress);
    }

    private void UpdateMessage(string message)
    {
        currentCalibrationMessage = message;

        if (calibrationText != null)
        {
            calibrationText.text = message;
        }

        OnMessageUpdated?.Invoke(message);
    }
    #endregion

    #region Calibration States
    /// <summary>
    /// Affiche l'état de démarrage de la calibration.
    /// </summary>
    public void ShowCalibrationStartState()
    {
        ShowCalibrationUI(true);
        UpdateMessage("Calibration démarrée. Suivez les instructions...");
        UpdateProgress(0f);
    }

    /// <summary>
    /// Affiche l'état de progression de la calibration.
    /// </summary>
    public void ShowCalibrationProgressState(float progress, string customMessage = null)
    {
        ShowCalibrationUI(progress);
        
        if (!string.IsNullOrEmpty(customMessage))
        {
            UpdateMessage(customMessage);
        }
    }

    /// <summary>
    /// Affiche l'état de fin de calibration.
    /// </summary>
    public void ShowCalibrationCompleteState()
    {
        UpdateProgress(1f);
        UpdateMessage("Calibration terminée avec succès !");
        
        // Masquage automatique recommandé (à implémenter côté appelant si besoin)
    }

    /// <summary>
    /// Affiche l'état d'erreur de calibration.
    /// </summary>
    public void ShowCalibrationErrorState(string errorMessage)
    {
        ShowCalibrationUI(true);
        UpdateMessage($"Erreur de calibration: {errorMessage}");
        UpdateProgress(0f);
    }
    #endregion

    #region Calibration Steps
    /// <summary>
    /// Affiche les instructions pour une étape de calibration spécifique.
    /// </summary>
    public void ShowCalibrationStep(int step, int totalSteps, string instruction)
    {
        float stepProgress = (float)step / totalSteps;
        string stepMessage = $"Étape {step}/{totalSteps}: {instruction}";
        
        ShowCalibrationProgressState(stepProgress, stepMessage);
    }

    /// <summary>
    /// Affiche les instructions pour la calibration des voies.
    /// </summary>
    public void ShowLaneCalibrationInstruction(int lane, string instruction)
    {
        string laneMessage = $"Voie {lane}: {instruction}";
        UpdateMessage(laneMessage);
    }
    #endregion

    #region Analytics
    /// <summary>
    /// Génère des statistiques sur l'interface de calibration.
    /// </summary>
    public CalibrationUIStats GenerateStats()
    {
        return new CalibrationUIStats
        {
            isActive = isCalibrationActive,
            currentProgress = currentProgress,
            currentMessage = currentCalibrationMessage,
            hasPanel = calibrationPanel != null,
            hasProgressSlider = calibrationProgressSlider != null,
            hasTextDisplay = calibrationText != null
        };
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Nettoie les ressources et reset l'état.
    /// </summary>
    public void Cleanup()
    {
        HideCalibrationUI();
        currentProgress = 0f;
        currentCalibrationMessage = "Calibration prête";
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Statistiques de l'interface de calibration.
/// </summary>
[System.Serializable]
public class CalibrationUIStats
{
    public bool isActive;
    public float currentProgress;
    public string currentMessage;
    public bool hasPanel;
    public bool hasProgressSlider;
    public bool hasTextDisplay;
}
#endregion
