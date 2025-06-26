using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestionnaire d'interface utilisateur pour le mini-jeu "Le Défilé des Rondins".
/// Orchestre les différents modules d'affichage UI et fournit une interface unifiée.
/// Refactorisé pour utiliser une architecture modulaire et découplée.
/// </summary>
public class LogParadeUIManager : MonoBehaviour
{    [Header("UI Elements")]
    public TMP_Text currentLaneText;
    public TMP_Text positionText;
    public TMP_Text gameStatusText;
    public TMP_Text debugInfoText;
      // REMOVED: Lane Indicators and Highlights system completely removed for simplification
      [Header("Calibration UI")]
    public GameObject calibrationPanel;
    [SerializeField] private Slider calibrationProgressSlider; // Optionnel - peut être null
    public TMP_Text calibrationText;
    
    [Header("Debug Panel")]
    public GameObject debugPanel;
    public Toggle debugToggle;

    // Modules UI
    // REMOVED: Lane Indicator Manager system completely removed
    private LogParadeCalibrationUIController calibrationUIController;
    private LogParadeGameStatusDisplay gameStatusDisplay;

    #region Properties
    public bool IsInitialized { get; private set; }
    public int CurrentLane => gameStatusDisplay?.CurrentLane ?? 2;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        // Configuration du toggle debug
        if (debugToggle != null)
        {
            debugToggle.isOn = false;
            debugToggle.onValueChanged.AddListener(SetDebugMode);
        }

        // Validation et conseils de configuration
        ValidateAndProvideSetupGuidance();
        
        InitializeUI();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialise l'interface utilisateur et tous les modules.
    /// </summary>
    public void InitializeUI()
    {
        LogParadeLogger.Log("Initialisation de l'interface utilisateur...");

        // Initialisation des modules
        InitializeModules();
        
        // Configuration des événements inter-modules
        SetupEventHandlers();

        // État initial
        SetupInitialState();

        IsInitialized = true;
        LogParadeLogger.Log("Interface utilisateur initialisée avec succès");
    }    private void InitializeModules()
    {
        // Les indicateurs de voies ont été supprimés pour simplifier le système
        LogParadeLogger.Log("Lane indicators désactivés - système simplifié");

        // Initialise le contrôleur d'UI de calibration seulement si les composants sont disponibles
        if (HasValidCalibrationUI())
        {
            calibrationUIController = new LogParadeCalibrationUIController(
                calibrationPanel,
                calibrationProgressSlider,
                calibrationText
            );
        }
        else
        {
            LogParadeLogger.LogWarning("UI de calibration non configurée - module désactivé");
        }

        // Initialise l'affichage du statut de jeu seulement si au moins un composant est disponible
        if (HasValidStatusDisplay())
        {
            gameStatusDisplay = new LogParadeGameStatusDisplay(
                currentLaneText,
                positionText,
                gameStatusText,
                debugInfoText
            );
        }
        else
        {
            LogParadeLogger.LogWarning("Affichage de statut non configuré - module désactivé");
        }
    }

    private void SetupEventHandlers()
    {        // REMOVED: Lane Indicator Manager events removed

        // Événements du contrôleur de calibration
        if (calibrationUIController != null)
        {
            calibrationUIController.OnCalibrationUIShown += OnCalibrationUIShown;
            calibrationUIController.OnCalibrationUIHidden += OnCalibrationUIHidden;
        }

        // Événements de l'affichage de statut
        if (gameStatusDisplay != null)
        {
            gameStatusDisplay.OnLaneDisplayUpdated += OnLaneDisplayUpdated;
            gameStatusDisplay.OnDebugModeToggled += OnDebugModeToggled;
        }
    }

    private void SetupInitialState()
    {
        // État initial : voie centrale, message d'initialisation
        UpdateCurrentLane(2);
        UpdateGameStatus("Initialisation...");
        
        // Masque le panel de debug initialement
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }
    #endregion

    #region Public API - Lane Management
    /// <summary>
    /// Met à jour l'affichage de la voie actuelle.
    /// </summary>
    public void UpdateCurrentLane(int lane)
    {
        if (!IsInitialized) return;

        // Délègue aux modules appropriés
        // REMOVED: Lane Indicator Manager method removed
        gameStatusDisplay?.UpdateCurrentLane(lane);
    }

    /// <summary>
    /// Obtient la voie actuelle.
    /// </summary>
    public int GetCurrentLane()
    {
        return CurrentLane;
    }
    #endregion

    #region Public API - Position Management
    /// <summary>
    /// Met à jour l'affichage de la position du joueur.
    /// </summary>
    public void UpdatePlayerPosition(Vector3 position)
    {
        if (!IsInitialized) return;
        gameStatusDisplay?.UpdatePlayerPosition(position);
    }
    #endregion

    #region Public API - Game Status
    /// <summary>
    /// Met à jour le statut du jeu.
    /// </summary>
    public void UpdateGameStatus(string status)
    {
        if (!IsInitialized) return;
        gameStatusDisplay?.UpdateGameStatus(status);
    }

    /// <summary>
    /// Affiche le message de début de jeu.
    /// </summary>
    public void ShowGameStartMessage()
    {
        if (!IsInitialized) return;
        gameStatusDisplay?.ShowGameStartMessage();
    }

    /// <summary>
    /// Affiche l'interface de jeu (alias pour ShowGameStartMessage).
    /// </summary>
    public void ShowGameUI()
    {
        ShowGameStartMessage();
    }
    #endregion

    #region Public API - Calibration
    /// <summary>
    /// Affiche l'interface de calibration avec une progression spécifique.
    /// </summary>
    public void ShowCalibrationUI(float progress)
    {
        if (!IsInitialized) return;
        calibrationUIController?.ShowCalibrationUI(progress);
    }

    /// <summary>
    /// Affiche l'interface de calibration (overload pour bool).
    /// </summary>
    public void ShowCalibrationUI(bool show)
    {
        if (!IsInitialized) return;
        calibrationUIController?.ShowCalibrationUI(show);
    }

    /// <summary>
    /// Masque l'interface de calibration.
    /// </summary>
    public void HideCalibrationUI()
    {
        if (!IsInitialized) return;
        calibrationUIController?.HideCalibrationUI();
    }

    /// <summary>
    /// Met à jour le texte de calibration.
    /// </summary>
    public void UpdateCalibrationText(string text)
    {
        if (!IsInitialized) return;
        calibrationUIController?.UpdateCalibrationText(text);
    }

    /// <summary>
    /// Met à jour la progression de la calibration.
    /// </summary>
    public void UpdateCalibrationProgress(float progress)
    {
        if (!IsInitialized) return;
        calibrationUIController?.UpdateCalibrationProgress(progress);
    }

    /// <summary>
    /// Met en évidence une voie spécifique pour la calibration.
    /// </summary>
    public void HighlightLaneForCalibration(int lane, bool highlight)
    {
        if (!IsInitialized) return;
        // REMOVED: Lane Indicator Manager method removed
    }

    /// <summary>
    /// Marque une voie comme complétée pour la calibration.
    /// </summary>
    public void SetLaneCompletedForCalibration(int lane)
    {
        if (!IsInitialized) return;
        // REMOVED: Lane Indicator Manager method removed
    }    /// <summary>
    /// REMOVED: Lane highlights system has been completely removed.
    /// </summary>
    public void ResetLaneHighlights()
    {
        LogParadeLogger.Log("Lane highlights system has been removed - no action needed");
    }
    #endregion

    #region Public API - Debug
    /// <summary>
    /// Active/Désactive le mode debug.
    /// </summary>
    public void ToggleDebugMode(bool enabled)
    {
        if (!IsInitialized) return;
        
        gameStatusDisplay?.SetDebugMode(enabled);
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(enabled);
        }
    }

    private void SetDebugMode(bool enabled)
    {
        ToggleDebugMode(enabled);
    }
    #endregion

    #region Component Validation    /// <summary>
    /// REMOVED: Lane indicators validation no longer needed (system removed)
    /// </summary>
    private bool HasValidLaneIndicators()
    {
        // Lane indicators system has been completely removed
        return true; // Always return true to avoid breaking existing code
    }

    /// <summary>
    /// Vérifie si l'UI de calibration est correctement configurée
    /// </summary>
    private bool HasValidCalibrationUI()
    {
        // Au moins le panel ou le texte doit être assigné
        return calibrationPanel != null || calibrationText != null;
    }

    /// <summary>
    /// Vérifie si l'affichage de statut est correctement configuré
    /// </summary>
    private bool HasValidStatusDisplay()
    {
        // Au moins un des textes doit être assigné
        return currentLaneText != null || 
               positionText != null || 
               gameStatusText != null || 
               debugInfoText != null;
    }

    /// <summary>
    /// Valide la configuration et fournit des conseils pour corriger les problèmes
    /// </summary>
    private void ValidateAndProvideSetupGuidance()
    {
        LogParadeLogger.Log("=== Validation de la configuration UI LogParade ===");
        
        int issues = 0;
          // REMOVED: Lane indicators validation (system removed)
        LogParadeLogger.Log("✅ Lane indicators system removed - no configuration needed");
        
        // Vérification de l'UI de calibration
        if (!HasValidCalibrationUI())
        {
            issues++;
            LogParadeLogger.LogWarning("❌ UI de calibration non configurée");
            LogParadeLogger.Log("💡 SOLUTION: Assignez au moins 'calibrationPanel' ou 'calibrationText' dans l'inspecteur Unity");
        }
        else
        {
            LogParadeLogger.Log("✅ UI de calibration configurée");
        }
        
        // Vérification de l'affichage de statut
        if (!HasValidStatusDisplay())
        {
            issues++;
            LogParadeLogger.LogWarning("❌ Affichage de statut non configuré");
            LogParadeLogger.Log("💡 SOLUTION: Assignez au moins un des champs texte (currentLaneText, gameStatusText, etc.) dans l'inspecteur Unity");
        }
        else
        {
            LogParadeLogger.Log("✅ Affichage de statut configuré");
        }
        
        if (issues == 0)
        {
            LogParadeLogger.Log("🎉 Configuration UI parfaite ! Tous les modules sont opérationnels.");
        }
        else
        {
            LogParadeLogger.LogWarning($"⚠️ {issues} problème(s) de configuration détecté(s). Le jeu fonctionnera avec les fonctionnalités disponibles.");
            LogParadeLogger.Log("📖 Consultez le README.md pour des instructions détaillées de configuration.");
        }
        
        LogParadeLogger.Log("=== Fin de la validation ===");
    }
    #endregion

    #region Event Handlers    // REMOVED: Lane event handlers (LogParadeLaneIndicatorManager system removed)

    private void OnCalibrationUIShown()
    {
        LogParadeLogger.LogVerbose("Interface de calibration affichée");
    }

    private void OnCalibrationUIHidden()
    {
        LogParadeLogger.LogVerbose("Interface de calibration masquée");
    }

    private void OnLaneDisplayUpdated(int lane)
    {
        LogParadeLogger.LogVerbose($"Affichage voie mis à jour: {lane}");
    }

    private void OnDebugModeToggled(bool enabled)
    {
        LogParadeLogger.LogVerbose($"Mode debug: {enabled}");
    }
    #endregion
}
