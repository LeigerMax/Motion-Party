using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestionnaire d'interface utilisateur pour le mini-jeu "Le Défilé des Rondins"
/// Affiche les informations de tracking, la voie actuelle, et les messages de jeu
/// </summary>
public class LogParadeUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text currentLaneText;
    public TMP_Text positionText;
    public TMP_Text gameStatusText;
    public TMP_Text debugInfoText;
    
    [Header("Lane Visualization")]
    public RectTransform[] laneIndicators = new RectTransform[4];
    public Image[] laneHighlights = new Image[4];
    public Color activeLaneColor = Color.green;
    public Color inactiveLaneColor = Color.gray;
    
    [Header("Calibration UI")]
    public GameObject calibrationPanel;
    public Slider calibrationProgressSlider;
    public TMP_Text calibrationText;
    
    [Header("Debug Panel")]
    public GameObject debugPanel;
    public Toggle debugToggle;

    [Header("Lane Validation UI (NEW)")]
    public GameObject laneValidationPanel;
    public TMP_Text validationText;
    public Slider validationProgressSlider;
    public Image validationTargetLaneImage;    [Header("Calibration Center Guide (NEW)")]
    public GameObject centerGuidePanel;
    public Image centerGuideCircle;
    public TMP_Text centerGuideText;
    public Button recalibrateButton; // Nouveau : bouton pour recalibrage manuel
    public Color centerGuideColor = Color.green;
    
    [Header("Camera Presets (NEW)")]
    public TMP_Dropdown cameraPresetDropdown; // Dropdown pour choisir la résolution caméra

    private int currentLane = 2;
    private Vector3 currentPosition;
    private bool isDebugMode = false;    void Start()
    {
        // Initialiser l'état de debug
        if (debugToggle != null)
        {
            debugToggle.onValueChanged.AddListener(ToggleDebugMode);
        }
          // Configurer le bouton de recalibrage
        if (recalibrateButton != null)
        {
            recalibrateButton.onClick.AddListener(RequestRecalibration);
        }
        
        // Configurer le dropdown des presets de caméra
        if (cameraPresetDropdown != null)
        {
            cameraPresetDropdown.onValueChanged.AddListener(OnCameraPresetChanged);
            
            // Ajouter les options si le dropdown est vide
            if (cameraPresetDropdown.options.Count == 0)
            {
                cameraPresetDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "640x480 (VGA)",
                    "1280x720 (HD)",
                    "1920x1080 (Full HD)"
                });
            }
        }
    }

    /// <summary>
    /// Initialise l'interface utilisateur
    /// </summary>
    public void InitializeUI()
    {
        // Initialiser les textes
        if (gameStatusText != null)
        {
            gameStatusText.text = "Initialisation...";
        }

        // Initialiser les indicateurs de voie
        InitializeLaneIndicators();
        
        // Mettre à jour l'affichage de la voie actuelle
        UpdateCurrentLane(2); // Commence au centre
        
        // Masquer le panel de calibration initialement
        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(false);
        }

        // Masquer le panneau de validation de voie
        if (laneValidationPanel != null)
        {
            laneValidationPanel.SetActive(false);
        }

        // Masquer le guide de calibration centrale
        if (centerGuidePanel != null)
        {
            centerGuidePanel.SetActive(false);
        }

        Debug.Log("UI du LogParade initialisée.");
    }

    /// <summary>
    /// Initialise les indicateurs visuels des voies
    /// </summary>
    private void InitializeLaneIndicators()
    {
        for (int i = 0; i < 4; i++)
        {
            if (laneHighlights[i] != null)
            {
                laneHighlights[i].color = inactiveLaneColor;
            }
        }
    }

    /// <summary>
    /// Met à jour l'affichage de la voie actuelle
    /// </summary>
    public void UpdateCurrentLane(int lane)
    {
        currentLane = Mathf.Clamp(lane, 1, 4);
        
        // Mettre à jour le texte
        if (currentLaneText != null)
        {
            currentLaneText.text = $"Voie: {currentLane}/4";
        }

        // Mettre à jour les indicateurs visuels
        UpdateLaneHighlights();
    }

    /// <summary>
    /// Met à jour les surbrillances des voies
    /// </summary>
    private void UpdateLaneHighlights()
    {
        for (int i = 0; i < 4; i++)
        {
            if (laneHighlights[i] != null)
            {
                // Voie 1 = index 0, voie 2 = index 1, etc.
                bool isActive = (i + 1) == currentLane;
                laneHighlights[i].color = isActive ? activeLaneColor : inactiveLaneColor;
            }
        }
    }

    /// <summary>
    /// Met à jour l'affichage de la position du joueur
    /// </summary>
    public void UpdatePlayerPosition(Vector3 position)
    {
        currentPosition = position;
        
        if (positionText != null)
        {
            positionText.text = $"Position: X={position.x:F2}";
        }

        UpdateDebugInfo();
    }

    /// <summary>
    /// Affiche le message de début de jeu
    /// </summary>
    public void ShowGameStartMessage()
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = "Bougez latéralement pour contrôler l'avatar !";
        }
    }

    /// <summary>
    /// Affiche l'interface de calibration
    /// </summary>
    public void ShowCalibrationUI(float progress)
    {
        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(true);
        }

        if (calibrationProgressSlider != null)
        {
            calibrationProgressSlider.value = progress;
        }

        if (calibrationText != null)
        {
            calibrationText.text = $"Calibration... {progress:P0}";
        }
    }

    /// <summary>
    /// Masque l'interface de calibration
    /// </summary>
    public void HideCalibrationUI()
    {
        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Affiche le panneau de validation de changement de voie
    /// </summary>
    public void ShowLaneValidationUI(int targetLane, float progress)
    {
        if (laneValidationPanel != null)
        {
            laneValidationPanel.SetActive(true);
        }

        if (validationText != null)
        {
            validationText.text = $"Changement de voie vers la voie {targetLane}...";
        }

        if (validationProgressSlider != null)
        {
            validationProgressSlider.value = progress;
        }

        // Mettre à jour la couleur de la voie cible
        if (validationTargetLaneImage != null)
        {
            validationTargetLaneImage.color = (targetLane == currentLane) ? activeLaneColor : inactiveLaneColor;
        }
    }

    /// <summary>
    /// Masque le panneau de validation de changement de voie
    /// </summary>
    public void HideLaneValidationUI()
    {
        if (laneValidationPanel != null)
        {
            laneValidationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Affiche la validation de changement de voie en cours
    /// </summary>
    public void ShowLaneValidation(int targetLane, float progress)
    {
        if (laneValidationPanel != null)
        {
            laneValidationPanel.SetActive(true);
        }
        
        if (validationText != null)
        {
            validationText.text = $"Changement vers voie {targetLane}...";
        }
        
        if (validationProgressSlider != null)
        {
            validationProgressSlider.value = progress;
        }
        
        if (validationTargetLaneImage != null)
        {
            // Colorer l'image selon la voie cible
            Color[] laneColors = { Color.red, Color.yellow, Color.green, Color.blue };
            if (targetLane >= 1 && targetLane <= 4)
            {
                validationTargetLaneImage.color = laneColors[targetLane - 1];
            }
        }
    }
    
    /// <summary>
    /// Cache l'interface de validation
    /// </summary>
    public void HideLaneValidation()
    {
        if (laneValidationPanel != null)
        {
            laneValidationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Affiche le guide de calibration centrale
    /// </summary>
    public void ShowCenterGuide()
    {
        if (centerGuidePanel != null)
        {
            centerGuidePanel.SetActive(true);
        }
        
        if (centerGuideText != null)
        {
            centerGuideText.text = "🎯 Placez-vous au CENTRE et restez immobile !";
        }
        
        if (centerGuideCircle != null)
        {
            centerGuideCircle.color = centerGuideColor;
        }
    }
    
    /// <summary>
    /// Cache le guide de calibration centrale
    /// </summary>
    public void HideCenterGuide()
    {
        if (centerGuidePanel != null)
        {
            centerGuidePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Met à jour le guide avec le progrès de calibration
    /// </summary>
    public void UpdateCenterGuide(float progress)
    {
        if (centerGuideText != null)
        {
            centerGuideText.text = $"🎯 Calibration... {progress:P0}\nRestez au centre !";
        }
        
        if (centerGuideCircle != null)
        {
            // Changer la couleur selon le progrès
            float alpha = 0.3f + (progress * 0.7f);
            Color color = centerGuideColor;
            color.a = alpha;
            centerGuideCircle.color = color;
        }
    }

    /// <summary>
    /// Active/Désactive le mode debug
    /// </summary>
    public void ToggleDebugMode(bool enabled)
    {
        isDebugMode = enabled;
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(enabled);
        }

        UpdateDebugInfo();
    }

    /// <summary>
    /// Met à jour les informations de debug
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (!isDebugMode || debugInfoText == null) return;

        string debugInfo = $"=== DEBUG INFO ===\n";
        debugInfo += $"Voie actuelle: {currentLane}\n";
        debugInfo += $"Position: {currentPosition}\n";
        debugInfo += $"Temps: {Time.time:F1}s\n";

        debugInfoText.text = debugInfo;
    }

    /// <summary>
    /// Met à jour le statut du jeu
    /// </summary>
    public void UpdateGameStatus(string status)
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = status;
        }
    }    /// <summary>
    /// Obtient la voie actuelle
    /// </summary>
    public int GetCurrentLane()
    {
        return currentLane;
    }
      /// <summary>
    /// Demande une recalibration via le tracker
    /// </summary>
    private void RequestRecalibration()
    {
        var tracker = FindObjectOfType<LogParadeLateralTracker>();
        if (tracker != null)
        {
            tracker.Recalibrate();
            UpdateGameStatus("🔄 Recalibration demandée...");
        }
        else
        {
            Debug.LogWarning("⚠️ LogParadeLateralTracker non trouvé pour la recalibration !");
        }
    }
    
    /// <summary>
    /// Gère le changement de preset de caméra via le dropdown
    /// </summary>
    private void OnCameraPresetChanged(int index)
    {
        var tracker = FindObjectOfType<LogParadeLateralTracker>();
        if (tracker != null)
        {
            string[] presets = { "640x480", "1280x720", "1920x1080" };
            if (index >= 0 && index < presets.Length)
            {
                tracker.SetCameraPreset(presets[index]);
                UpdateGameStatus($"📷 Caméra configurée : {presets[index]}");
                
                // Forcer une recalibration après changement de preset
                tracker.Recalibrate();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ LogParadeLateralTracker non trouvé pour changer le preset caméra !");
        }
    }
}
