using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System.Collections;

/// <summary>
/// Contrôleur principal du mini-jeu "Le Défilé des Rondins"
/// Gère le tracking latéral du joueur et son mapping sur 4 voies verticales
/// </summary>
public class LogParadeGameController : MiniGameBase
{
    #region Champs

    [Header("Managers")]
    public UDPReceive udpReceive;
    public LogParadeUIManager uiManager;
    public LogParadeLateralTracker lateralTracker;
    public LogParadePlayerAvatar playerAvatar;
    public LogParadeScoreManager scoreManager;

    [Header("Game Settings")]
    public float startDelay = 1f;
    public bool enableDebugMode = true;

    [Header("Lane Settings")]
    public Transform[] laneMarkers = new Transform[4];
    public Material[] laneMaterials = new Material[4];
    private bool gameStarted = false;
    private bool gameEnded = false;
    private Vector3 currentPlayerPosition;
    private int currentLane = 2;
#endregion

#region Unity Lifecycle
    protected override void Launch()
    {
        InitGame();
    }
    void Start()
    {
        ValidateComponents();
        if (lateralTracker != null)
        {
            lateralTracker.OnLaneChanged += HandleLaneChanged;
            lateralTracker.OnPositionUpdated += HandlePositionUpdated;
        }
        Launch();
    }
    
    void Update()
    {
        ProcessMediaPipeData();
    }

    private void OnDestroy()
    {
        if (lateralTracker != null)
        {
            lateralTracker.OnLaneChanged -= HandleLaneChanged;
            lateralTracker.OnPositionUpdated -= HandlePositionUpdated;
        }
    }
#endregion

#region Initialisation
    /// <summary>
    /// Initialise le jeu et l'état de départ
    /// </summary>
    private void InitGame()
    {
        gameStarted = false;
        gameEnded = false;
        currentLane = 2;
        if (uiManager != null)
        {
            uiManager.InitializeUI();
        }
        if (playerAvatar != null)
        {
            playerAvatar.SetLaneInstant(2);
        }
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        StartCoroutine(StartGameAfterDelay());
    }

    /// <summary>
    /// Lance le jeu après un délai de démarrage
    /// </summary>
    private IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        gameStarted = true;
        if (uiManager != null)
        {
            uiManager.ShowGameStartMessage();
        }
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
        }
        else
        {
            LogParadeLogger.LogError("ScoreManager non assigné ! Le score ne sera pas comptabilisé. Vérifiez qu'un GameObject avec LogParadeScoreManager existe dans la scène");
        }
    }

    /// <summary>
    /// Vérifie et assigne les composants critiques du jeu
    /// </summary>
    private void ValidateComponents()
    {
        if (udpReceive == null)
        {
            LogParadeLogger.LogError("UDPReceive n'est pas assigné dans LogParadeGameController !");
        }
        if (lateralTracker == null)
        {
            lateralTracker = FindFirstObjectByType<LogParadeLateralTracker>();
            if (lateralTracker == null)
                LogParadeLogger.LogError("LogParadeLateralTracker non trouvé !");
        }
        if (playerAvatar == null)
        {
            playerAvatar = FindFirstObjectByType<LogParadePlayerAvatar>();
            if (playerAvatar == null)
                LogParadeLogger.LogError("LogParadePlayerAvatar non trouvé !");
        }
        if (uiManager == null)
        {
            LogParadeLogger.LogWarning("LogParadeUIManager n'est pas assigné !");
        }
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<LogParadeScoreManager>();
            if (scoreManager == null)
            {
                LogParadeLogger.LogError("LogParadeScoreManager non trouvé dans la scène ! Le score ne sera pas comptabilisé. Solution : Ajoutez un GameObject avec le composant LogParadeScoreManager à votre scène");
            }
        }
    }
#endregion

#region Gameplay
    /// <summary>
    /// Traite les données MediaPipe reçues pour le tracking
    /// </summary>
    private void ProcessMediaPipeData()
    {
        if (udpReceive == null) return;
        string data = udpReceive.data;
        if (string.IsNullOrEmpty(data)) return;
        try
        {
            JObject jsonData = JObject.Parse(data);
        }
        catch (System.Exception ex)
        {
            if (enableDebugMode)
                LogParadeLogger.LogWarning($"Erreur lors du parsing des données MediaPipe : {ex.Message}");
        }
    }

    /// <summary>
    /// Callback lors d'un changement de voie détecté par le tracker
    /// </summary>
    private void HandleLaneChanged(int newLane)
    {
        currentLane = newLane;
        if (playerAvatar != null)
        {
            playerAvatar.SetTargetLane(newLane);
        }
        if (uiManager != null)
        {
            uiManager.UpdateCurrentLane(newLane);
        }
    }

    /// <summary>
    /// Callback lors d'une mise à jour de position du tracker
    /// </summary>
    private void HandlePositionUpdated(Vector3 position)
    {
        currentPlayerPosition = position;
        if (uiManager != null)
        {
            uiManager.UpdatePlayerPosition(position);
        }
    }
    #endregion

    #region API publique

    /// <summary>
    /// Retourne la voie actuelle du joueur
    /// </summary>
    public int GetCurrentLane()
    {
        return currentLane;
    }

    /// <summary>
    /// Retourne la position actuelle du joueur
    /// </summary>
    public Vector3 GetCurrentPlayerPosition()
    {
        return currentPlayerPosition;
    }
    
    /// <summary>
    /// Indique si le jeu est en cours
    /// </summary>
    public bool IsGameActive()
    {
        return gameStarted && !gameEnded;
    }

    /// <summary>
    /// Arrête le jeu et le système de score
    /// </summary>
    public void StopGame()
    {
        gameEnded = true;
        gameStarted = false;
        if (scoreManager != null)
        {
            scoreManager.StopScoring();
        }
    }

    /// <summary>
    /// Force le démarrage immédiat du jeu (pour la calibration)
    /// </summary>
    public void ForceStartGame()
    {
        if (gameStarted)
        {
            return;
        }
        StopAllCoroutines();
        gameStarted = true;
        gameEnded = false;
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
        }
        if (uiManager != null)
        {
            uiManager.ShowGameUI();
        }
    }
#endregion
}
