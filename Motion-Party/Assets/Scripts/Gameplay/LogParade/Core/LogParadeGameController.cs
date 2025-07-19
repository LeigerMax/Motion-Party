using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UI.RoundEndScreen;
using Systems;

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
    public RoundEndScreenManager roundEndScreenManager;

    [Header("Game Settings")]
    public float startDelay = 1f;
    public bool enableDebugMode = true;
    public float delayBetweenPlayers = 2f;

    [Header("Lane Settings")]
    public Transform[] laneMarkers = new Transform[4];
    public Material[] laneMaterials = new Material[4];
    private bool gameStarted = false;
    private bool gameEnded = false;
    private Vector3 currentPlayerPosition;
    private int currentLane = 2;

    // Multijoueur
    private GamePlayerSelector gamePlayerSelector;
    private PlayerData currentPlayer;
    private bool isMultiPlayerSession = false;
    private Dictionary<string, int> roundScores = new Dictionary<string, int>();
    #endregion

#region Unity Lifecycle
    private LogParadeGameTimer gameTimer;

    protected override void Launch()
    {
        ValidateComponents();
        InitializePlayerSystem();
        InitGame();

        // Lier l'événement de fin de timer pour afficher le roundend
        if (gameTimer == null)
            gameTimer = FindFirstObjectByType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            gameTimer.OnGameEnd.RemoveListener(OnTimerEnded_ShowRoundEnd);
            gameTimer.OnGameEnd.AddListener(OnTimerEnded_ShowRoundEnd);
        }
    }
    // Affiche le roundend screen même si le timer atteint 0
    private void OnTimerEnded_ShowRoundEnd()
    {
        // Si le jeu n'est pas déjà terminé, on force la logique de fin de round
        if (!gameEnded)
        {
            int score = scoreManager != null ? scoreManager.CurrentScore : 0;
            HandleGameFinished(score);
        }
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
            udpReceive = FindFirstObjectByType<UDPReceive>();
            if (udpReceive == null)
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
            uiManager = FindFirstObjectByType<LogParadeUIManager>();
            if (uiManager == null)
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
        if (roundEndScreenManager == null)
        {
            roundEndScreenManager = FindFirstObjectByType<RoundEndScreenManager>();
            if (roundEndScreenManager == null)
                LogParadeLogger.LogWarning("RoundEndScreenManager non trouvé !");
        }
    }
    // --- MULTIJOUEUR ---
    private void InitializePlayerSystem()
    {
        gamePlayerSelector = GamePlayerSelector.Instance;
        if (gamePlayerSelector == null)
        {
            LogParadeLogger.LogWarning("GamePlayerSelector non trouvé - Mode solo activé");
            isMultiPlayerSession = false;
            currentPlayer = null;
            return;
        }
        isMultiPlayerSession = gamePlayerSelector.SelectedPlayers.Count > 1;
        currentPlayer = GetNextPlayerToPlay() ?? gamePlayerSelector.CurrentPlayer;
        if (currentPlayer != null)
        {
            LogParadeLogger.Log($"[LogParade] Système de joueurs initialisé - Joueur actuel: {currentPlayer.Nickname}");
            if (isMultiPlayerSession)
                LogParadeLogger.Log($"[LogParade] Session multi-joueurs détectée - {gamePlayerSelector.SelectedPlayers.Count} joueurs");
        }
        else
        {
            LogParadeLogger.LogWarning("Aucun joueur sélectionné - Mode solo activé");
            isMultiPlayerSession = false;
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
        gameStarted = false;
        if (scoreManager != null)
        {
            scoreManager.StopScoring();
        }
        // Fin de round pour le joueur courant
        int score = scoreManager != null ? scoreManager.CurrentScore : 0;
        HandleGameFinished(score);
    }

    // --- LOGIQUE MULTIJOUEUR/ROUNDEND ---
    private void HandleGameFinished(int score)
    {
        LogParadeLogger.Log($"[LogParade] Fin de partie pour {currentPlayer?.Nickname} - Score: {score}");
        SaveCurrentPlayerScore(score);
        // Affichage écran de fin de round si multi
        if (isMultiPlayerSession && HasNextPlayerToPlay())
        {
            ShowRoundEndScreen(score);
            return;
        }
        // En solo, ou dernier joueur : on affiche aussi le roundend si multi (pour cohérence UX)
        if (isMultiPlayerSession && !HasNextPlayerToPlay())
        {
            ShowRoundEndScreen(score); // Affiche le roundend même pour le dernier joueur
            // Le callback OnRoundEndNextPlayer terminera le mini-jeu
            return;
        }
        // Cas solo : fin directe
        LogParadeLogger.Log("[LogParade] Tous les joueurs ont joué - Fin du mini-jeu");
        ShowFinalRanking();
        FinishMiniGame();
    }

    private void ShowRoundEndScreen(int score)
    {
        var nextPlayer = GetNextPlayerToPlay();
        if (roundEndScreenManager != null && currentPlayer != null && nextPlayer != null)
        {
            roundEndScreenManager.gameObject.SetActive(true);
            roundEndScreenManager.ShowEndOfRoundInfo(currentPlayer, nextPlayer, score);
            roundEndScreenManager.OnNextPlayerCallback = OnRoundEndNextPlayer;
        }
        else
        {
            StartCoroutine(StartNextPlayerWithDelay());
        }
    }

    private void OnRoundEndNextPlayer()
    {
        if (roundEndScreenManager != null)
        {
            roundEndScreenManager.gameObject.SetActive(false);
            roundEndScreenManager.OnNextPlayerCallback = null;
        }
        // Si il reste un joueur, on prépare le suivant, sinon on termine le mini-jeu
        if (isMultiPlayerSession && HasNextPlayerToPlay())
        {
            PrepareNextPlayer();
        }
        else
        {
            LogParadeLogger.Log("[LogParade] Tous les joueurs ont joué - Fin du mini-jeu (via roundend)");
            ShowFinalRanking();
            FinishMiniGame();
        }
        // On ne met gameEnded à true qu'à la toute fin
        gameEnded = true;
    }

    private void PrepareNextPlayer()
    {
        currentPlayer = GetNextPlayerToPlay();
        if (currentPlayer == null)
        {
            LogParadeLogger.LogError("[LogParade] Erreur lors du passage au joueur suivant");
            FinishMiniGame();
            return;
        }
        // Réinitialiser le jeu pour le joueur suivant
        if (scoreManager != null) scoreManager.ResetScore();
        if (playerAvatar != null) playerAvatar.SetLaneInstant(2);
        if (uiManager != null) uiManager.InitializeUI();
        StartCoroutine(StartGameAfterDelay());
    }

    private IEnumerator StartNextPlayerWithDelay()
    {
        LogParadeLogger.Log($"[LogParade] Préparation pour {GetNextPlayerToPlay()?.Nickname} - Attente de {delayBetweenPlayers}s");
        yield return new WaitForSeconds(delayBetweenPlayers);
        PrepareNextPlayer();
    }

    private void SaveCurrentPlayerScore(int score)
    {
        if (currentPlayer != null)
        {
            currentPlayer.AddScore(score);
            if (gamePlayerSelector != null)
            {
                gamePlayerSelector.AddScoreToCurrentPlayer(score);
            }
            roundScores[currentPlayer.Nickname] = score;
            LogParadeLogger.Log($"[LogParade] Score enregistré pour {currentPlayer.Nickname}: {score} points");
        }
    }

    private void ShowFinalRanking()
    {
        if (gamePlayerSelector == null || !isMultiPlayerSession) return;
        var ranking = gamePlayerSelector.GetPlayerRanking();
        LogParadeLogger.Log("[LogParade] === CLASSEMENT FINAL ===");
        for (int i = 0; i < ranking.Count; i++)
        {
            var player = ranking[i];
            int roundScore = roundScores.ContainsKey(player.Nickname) ? roundScores[player.Nickname] : 0;
            LogParadeLogger.Log($"{i + 1}. {player.Nickname} - Score du tour: {roundScore} / Score total: {player.TotalScore}");
        }
    }

    private PlayerData GetNextPlayerToPlay()
    {
        if (gamePlayerSelector == null) return null;
        foreach (var player in gamePlayerSelector.SelectedPlayers)
        {
            if (!HasPlayerPlayedThisMinigame(player))
            {
                int playerIndex = gamePlayerSelector.SelectedPlayers.IndexOf(player);
                gamePlayerSelector.SetCurrentPlayer(playerIndex);
                return player;
            }
        }
        return null;
    }

    private bool HasNextPlayerToPlay()
    {
        if (gamePlayerSelector == null) return false;
        foreach (var player in gamePlayerSelector.SelectedPlayers)
        {
            if (!HasPlayerPlayedThisMinigame(player))
            {
                return true;
            }
        }
        return false;
    }

    private bool HasPlayerPlayedThisMinigame(PlayerData player)
    {
        return roundScores.ContainsKey(player.Nickname);
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
