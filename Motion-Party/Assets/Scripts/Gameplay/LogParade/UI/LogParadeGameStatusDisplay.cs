using UnityEngine;
using TMPro;

/// <summary>
/// Gestionnaire d'affichage du statut de jeu pour LogParade.
/// Responsable de l'affichage des informations de jeu, position du joueur, et messages de statut.
/// Centralise tout l'affichage textuel non lié à la calibration.
/// 
/// Cette classe n'est pas un MonoBehaviour, elle doit être instanciée et gérée par un contrôleur externe.
/// </summary>
public class LogParadeGameStatusDisplay
{
    #region Dependencies
    private TMP_Text currentLaneText;
    private TMP_Text positionText;
    private TMP_Text gameStatusText;
    private TMP_Text debugInfoText;
    #endregion

    #region State
    private int currentLane = 2;
    private Vector3 currentPosition = Vector3.zero;
    private string currentGameStatus = "Initialisation...";
    private bool isDebugMode = false;
    private float gameStartTime;
    #endregion

    #region Events
    public System.Action<int> OnLaneDisplayUpdated;
    public System.Action<Vector3> OnPositionUpdated;
    public System.Action<string> OnGameStatusUpdated;
    public System.Action<bool> OnDebugModeToggled;
    #endregion

    #region Properties
    public int CurrentLane => currentLane;
    public Vector3 CurrentPosition => currentPosition;
    public string CurrentGameStatus => currentGameStatus;
    public bool IsDebugMode => isDebugMode;
    public bool IsInitialized { get; private set; }
    #endregion

    #region Constructor
    public LogParadeGameStatusDisplay(TMP_Text currentLaneText, TMP_Text positionText, 
        TMP_Text gameStatusText, TMP_Text debugInfoText = null)
    {
        this.currentLaneText = currentLaneText;
        this.positionText = positionText;
        this.gameStatusText = gameStatusText;
        this.debugInfoText = debugInfoText;
        
        Initialize();
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        if (!ValidateComponents())
        {
            LogParadeLogger.LogError("Composants invalides");
            return;
        }

        gameStartTime = Time.time;
        SetupInitialState();

        IsInitialized = true;
        LogParadeLogger.Log("Initialisé avec succès");
    }

    private bool ValidateComponents()
    {
        bool hasAnyComponent = currentLaneText != null || positionText != null || 
                              gameStatusText != null || debugInfoText != null;

        if (!hasAnyComponent)
        {
            LogParadeLogger.LogError("Aucun composant texte assigné");
            return false;
        }

        // Log des composants manquants (non critique)
        if (currentLaneText == null) LogParadeLogger.LogWarning("Texte de voie courante non assigné");
        if (positionText == null) LogParadeLogger.LogWarning("Texte de position non assigné");
        if (gameStatusText == null) LogParadeLogger.LogWarning("Texte de statut non assigné");
        if (debugInfoText == null) LogParadeLogger.LogWarning("Texte de debug non assigné");

        return true;
    }

    private void SetupInitialState()
    {
        // Initialise les textes avec les valeurs par défaut
        UpdateCurrentLaneDisplay(currentLane);
        UpdatePlayerPositionDisplay(currentPosition);
        UpdateGameStatusDisplay(currentGameStatus);
        
        // Debug désactivé par défaut
        SetDebugMode(false);
    }
    #endregion

    #region Lane Display
    /// <summary>
    /// Met à jour l'affichage de la voie actuelle.
    /// </summary>
    public void UpdateCurrentLane(int lane)
    {
        if (!IsInitialized) return;

        currentLane = Mathf.Clamp(lane, 1, 4);
        UpdateCurrentLaneDisplay(currentLane);
        
        OnLaneDisplayUpdated?.Invoke(currentLane);
    }

    private void UpdateCurrentLaneDisplay(int lane)
    {
        if (currentLaneText != null)
        {
            currentLaneText.text = $"Voie: {lane}/4";
        }
    }

    /// <summary>
    /// Obtient la voie actuelle.
    /// </summary>
    public int GetCurrentLane()
    {
        return currentLane;
    }
    #endregion

    #region Position Display
    /// <summary>
    /// Met à jour l'affichage de la position du joueur.
    /// </summary>
    public void UpdatePlayerPosition(Vector3 position)
    {
        if (!IsInitialized) return;

        currentPosition = position;
        UpdatePlayerPositionDisplay(position);
        
        // Met à jour les infos de debug si activées
        if (isDebugMode)
        {
            UpdateDebugInfo();
        }

        OnPositionUpdated?.Invoke(position);
    }

    // Amélioration : affichage XYZ
    private void UpdatePlayerPositionDisplay(Vector3 position)
    {
        if (positionText != null)
        {
            // Affiche X/Y/Z pour plus de clarté
            positionText.text = $"Position: X={position.x:F2} Y={position.y:F2} Z={position.z:F2}";
        }
    }

    /// <summary>
    /// Met à jour l'affichage avec une position formatée personnalisée.
    /// </summary>
    public void UpdatePlayerPosition(Vector3 position, string customFormat)
    {
        if (!IsInitialized) return;

        currentPosition = position;
        
        if (positionText != null)
        {
            positionText.text = customFormat;
        }

        if (isDebugMode)
        {
            UpdateDebugInfo();
        }

        OnPositionUpdated?.Invoke(position);
    }
    #endregion

    #region Game Status
    /// <summary>
    /// Met à jour le statut du jeu.
    /// </summary>
    public void UpdateGameStatus(string status)
    {
        if (!IsInitialized) return;

        currentGameStatus = status;
        UpdateGameStatusDisplay(status);
        
        OnGameStatusUpdated?.Invoke(status);
    }

    private void UpdateGameStatusDisplay(string status)
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = status;
        }
    }

    /// <summary>
    /// Affiche le message de début de jeu.
    /// </summary>
    public void ShowGameStartMessage()
    {
        UpdateGameStatus("Bougez latéralement pour contrôler l'avatar !");
    }

    /// <summary>
    /// Affiche le message de fin de jeu.
    /// </summary>
    public void ShowGameEndMessage(bool success)
    {
        string message = success ? "Jeu terminé avec succès !" : "Jeu terminé";
        UpdateGameStatus(message);
    }

    /// <summary>
    /// Affiche un message de pause.
    /// </summary>
    public void ShowPauseMessage()
    {
        UpdateGameStatus("Jeu en pause");
    }

    /// <summary>
    /// Affiche un message d'erreur.
    /// </summary>
    public void ShowErrorMessage(string error)
    {
        UpdateGameStatus($"Erreur: {error}");
    }
    #endregion

    #region Debug Display
    /// <summary>
    /// Active/Désactive le mode debug.
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
        if (!IsInitialized) return;

        isDebugMode = enabled;
        
        if (debugInfoText != null)
        {
            debugInfoText.gameObject.SetActive(enabled);
        }

        if (enabled)
        {
            UpdateDebugInfo();
        }

        OnDebugModeToggled?.Invoke(enabled);
    }

    /// <summary>
    /// Bascule le mode debug.
    /// </summary>
    public void ToggleDebugMode()
    {
        SetDebugMode(!isDebugMode);
    }

    /// <summary>
    /// Met à jour les informations de debug.
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (!isDebugMode || debugInfoText == null) return;

        float currentTime = Time.time;
        float elapsedTime = currentTime - gameStartTime;

        string debugInfo = $"=== DEBUG INFO ===\n";
        debugInfo += $"Voie actuelle: {currentLane}\n";
        debugInfo += $"Position: {currentPosition}\n";
        debugInfo += $"Temps écoulé: {elapsedTime:F1}s\n";
        debugInfo += $"Timestamp: {currentTime:F1}s\n";
        debugInfo += $"Statut: {currentGameStatus}\n";

        debugInfoText.text = debugInfo;
    }

    /// <summary>
    /// Ajoute une information custom au debug.
    /// </summary>
    public void AddDebugInfo(string key, string value)
    {
        if (!isDebugMode || debugInfoText == null) return;

        debugInfoText.text = ReplaceOrAddLine(debugInfoText.text, key, value);
    }

    #region Private Helpers
    /// <summary>
    /// Remplace ou ajoute une ligne clé: valeur dans un texte multi-lignes.
    /// </summary>
    private string ReplaceOrAddLine(string text, string key, string value)
    {
        string newLine = $"{key}: {value}";
        var lines = text.Split('\n');
        bool found = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(key + ":"))
            {
                lines[i] = newLine;
                found = true;
                break;
            }
        }
        if (!found)
        {
            return text + newLine + "\n";
        }
        return string.Join("\n", lines);
    }
    #endregion
    #endregion

    #region Preset Messages
    /// <summary>
    /// Affiche des messages prédéfinis pour différents états du jeu.
    /// </summary>
    public void ShowPresetMessage(GameStatusPreset preset)
    {
        string message = preset switch
        {
            GameStatusPreset.Initializing => "Initialisation...",
            GameStatusPreset.WaitingForPlayer => "En attente du joueur...",
            GameStatusPreset.GameStarting => "Le jeu commence !",
            GameStatusPreset.GameInProgress => "Bougez latéralement pour contrôler l'avatar !",
            GameStatusPreset.GamePaused => "Jeu en pause",
            GameStatusPreset.GameEnding => "Fin du jeu...",
            GameStatusPreset.GameCompleted => "Jeu terminé avec succès !",
            GameStatusPreset.CalibrationRequired => "Calibration requise",
            GameStatusPreset.SystemError => "Erreur système",
            _ => "Statut inconnu"
        };

        UpdateGameStatus(message);
    }
    #endregion

    #region Analytics
    /// <summary>
    /// Génère des statistiques d'affichage.
    /// </summary>
    public GameStatusDisplayStats GenerateStats()
    {
        return new GameStatusDisplayStats
        {
            currentLane = currentLane,
            currentPosition = currentPosition,
            currentGameStatus = currentGameStatus,
            isDebugMode = isDebugMode,
            gameStartTime = gameStartTime,
            elapsedTime = Time.time - gameStartTime,
            hasLaneText = currentLaneText != null,
            hasPositionText = positionText != null,
            hasStatusText = gameStatusText != null,
            hasDebugText = debugInfoText != null
        };
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Nettoie l'affichage et reset les états.
    /// </summary>
    public void Cleanup()
    {
        SetDebugMode(false);
        UpdateGameStatus("Nettoyage terminé");
        // LogParadeLogger.LogVerbose("Nettoyage terminé"); // Suppression du log inutile
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Messages prédéfinis pour différents états du jeu.
/// </summary>
public enum GameStatusPreset
{
    Initializing,
    WaitingForPlayer,
    GameStarting,
    GameInProgress,
    GamePaused,
    GameEnding,
    GameCompleted,
    CalibrationRequired,
    SystemError
}

/// <summary>
/// Statistiques d'affichage du statut de jeu.
/// </summary>
[System.Serializable]
public class GameStatusDisplayStats
{
    public int currentLane;
    public Vector3 currentPosition;
    public string currentGameStatus;
    public bool isDebugMode;
    public float gameStartTime;
    public float elapsedTime;
    public bool hasLaneText;
    public bool hasPositionText;
    public bool hasStatusText;
    public bool hasDebugText;
}
#endregion
