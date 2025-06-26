using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Composant simple pour démarrer le jeu LogParade avec calibration depuis l'UI
/// </summary>
public class LogParadeGameStarter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Button calibrateOnlyButton;
    
    [Header("Auto-assignment")]
    [SerializeField] private bool autoFindGameLauncher = true;
    
    private LogParadeGameLauncher gameLauncher;

    void Start()
    {
        if (autoFindGameLauncher)
        {
            gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
        }
        
        SetupButtons();
        UpdateButtonStates();
    }

    void Update()
    {
        // Mettre à jour l'état des boutons en fonction du jeu
        UpdateButtonStates();
    }

    /// <summary>
    /// Configure les événements des boutons
    /// </summary>
    private void SetupButtons()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartCompleteGame);
        }
        
        if (restartGameButton != null)
        {
            restartGameButton.onClick.AddListener(RestartGame);
        }
        
        if (calibrateOnlyButton != null)
        {
            calibrateOnlyButton.onClick.AddListener(StartCalibrationOnly);
        }
    }

    /// <summary>
    /// Met à jour l'état des boutons selon le contexte
    /// </summary>
    private void UpdateButtonStates()
    {
        if (gameLauncher == null) return;
        
        bool isGameStarted = LogParadeGameStateController.IsGameStarted;
        bool isCalibrating = LogParadeGameStateController.IsCalibrationInProgress;
        
        // Bouton "Démarrer" - disponible si pas de jeu en cours
        if (startGameButton != null)
        {
            startGameButton.interactable = !isGameStarted && !isCalibrating;
            
            // Mettre à jour le texte du bouton
            var buttonText = startGameButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (isCalibrating)
                    buttonText.text = "Calibration en cours...";
                else if (isGameStarted)
                    buttonText.text = "Jeu en cours";
                else if (LogParadeGameStateController.CanStartGameplay())
                    buttonText.text = "Démarrer le jeu";
                else
                    buttonText.text = "Calibrer et Jouer";
            }
        }
        
        // Bouton "Redémarrer" - disponible si jeu démarré
        if (restartGameButton != null)
        {
            restartGameButton.interactable = isGameStarted;
        }
        
        // Bouton "Calibrer" - disponible si pas de calibration en cours
        if (calibrateOnlyButton != null)
        {
            calibrateOnlyButton.interactable = !isCalibrating;
        }
    }

    #region Button Actions

    /// <summary>
    /// Démarre le processus complet (calibration + jeu)
    /// </summary>
    public void StartCompleteGame()
    {
        if (gameLauncher == null)
        {
            LogParadeLogger.LogError("GameLauncher introuvable!");
            return;
        }
        
        LogParadeLogger.Log("🎮 Démarrage du jeu LogParade depuis l'UI");
        gameLauncher.StartCompleteGameProcess();
    }

    /// <summary>
    /// Redémarre le jeu complet
    /// </summary>
    public void RestartGame()
    {
        if (gameLauncher == null)
        {
            LogParadeLogger.LogError("GameLauncher introuvable!");
            return;
        }
        
        LogParadeLogger.Log("🔄 Redémarrage du jeu LogParade depuis l'UI");
        gameLauncher.RestartGame();
    }

    /// <summary>
    /// Démarre seulement la calibration
    /// </summary>
    public void StartCalibrationOnly()
    {
        if (gameLauncher == null)
        {
            LogParadeLogger.LogError("GameLauncher introuvable!");
            return;
        }
        
        LogParadeLogger.Log("🎯 Démarrage de la calibration depuis l'UI");
        gameLauncher.StartCalibrationProcess();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Affiche l'état actuel du système dans la console
    /// </summary>
    [ContextMenu("Debug - Show Game State")]
    public void ShowGameState()
    {
        if (gameLauncher != null)
        {
            gameLauncher.DebugShowSystemState();
        }
        else
        {
            LogParadeLogger.LogWarning("GameLauncher non trouvé pour afficher l'état");
        }
    }

    #endregion
}
