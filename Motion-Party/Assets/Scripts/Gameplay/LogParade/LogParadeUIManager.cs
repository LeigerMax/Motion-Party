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

    private int currentLane = 2;
    private Vector3 currentPosition;
    private bool isDebugMode = false;

    void Start()
    {
        // Initialiser l'état de debug
        if (debugToggle != null)
        {
            debugToggle.onValueChanged.AddListener(ToggleDebugMode);
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
    }

    /// <summary>
    /// Obtient la voie actuelle
    /// </summary>
    public int GetCurrentLane()
    {
        return currentLane;
    }
}
