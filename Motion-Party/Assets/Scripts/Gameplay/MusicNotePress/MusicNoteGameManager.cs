using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;
using UI.RoundEndScreen;
using Gameplay.Common.Badges;
using Gameplay.Music.Badges;

namespace Gameplay.MusicNotePress
{
    /// <summary>
    /// Gestionnaire principal du mini-jeu MusicNote
    /// Gère le système multijoueur et la coordination avec RoundEndScreen
    /// </summary>
    public class MusicNoteGameManager : MiniGameBase
    {
        [Header("Core References")]
        public MusicNoteGameController gameController;
        public UDPReceive udpReceive;

        [Header("UI References")]
        public RoundEndScreenManager roundEndScreenManager;
        public MusicNotePlayerDisplayUI playerDisplayUI;

        [Header("Badge System")]
        public MusicBadgeAdapter badgeAdapter;

        [Header("Player System")]
        public bool enablePlayerSystem = true;
        public float delayBetweenPlayers = 3f;

        [Header("Session Management")]
        public bool enableGameSessionTransition = true; // Nouvelle option
        public float nextGameTransitionDelay = 4f; // Délai avant transition

        [Header("Debug")]
        public bool enableDebugLogs = true;

        // Système de joueurs
        private Systems.GamePlayerSelector gamePlayerSelector;
        private Systems.PlayerData currentPlayer;
        private bool isMultiPlayerSession = false;
        private Dictionary<string, int> roundScores = new Dictionary<string, int>();

        // Référence au GameSessionManager
        private GameSessionManager gameSessionManager;

        protected override void Launch()
        {
            if (enableDebugLogs)
                Debug.Log("[MusicNoteGameManager] Launch() appelé");

            // Valider les composants avant tout
            ValidateComponents();

            // Initialiser le système de joueurs
            InitializePlayerSystem();

            // Initialiser le jeu
            InitializeGame();
        }

        void Start()
        {
            // Valider les composants au démarrage
            ValidateComponents();

            // Trouver le GameSessionManager
            FindGameSessionManager();

            // S'abonner aux événements du GameController
            if (gameController != null)
            {
                gameController.OnGameFinished += HandleGameFinished;
            }

            Launch();
        }

        private void FindGameSessionManager()
        {
            gameSessionManager = FindFirstObjectByType<GameSessionManager>();
            if (gameSessionManager == null && enableGameSessionTransition)
            {
                Debug.LogWarning("[MusicNoteGameManager] GameSessionManager non trouvé - Transition automatique désactivée");
            }
            else if (enableDebugLogs)
            {
                Debug.Log("[MusicNoteGameManager] GameSessionManager trouvé");
            }
        }

        private void ValidateComponents()
        {
            if (gameController == null)
            {
                gameController = FindFirstObjectByType<MusicNoteGameController>();
                if (gameController == null)
                    Debug.LogError("[MusicNoteGameManager] MusicNoteGameController non trouvé !");
            }

            if (udpReceive == null)
            {
                udpReceive = FindFirstObjectByType<UDPReceive>();
                if (udpReceive == null)
                    Debug.LogError("[MusicNoteGameManager] UDPReceive non trouvé !");
            }

            if (roundEndScreenManager == null)
            {
                roundEndScreenManager = FindFirstObjectByType<RoundEndScreenManager>();
                if (roundEndScreenManager == null)
                    Debug.LogWarning("[MusicNoteGameManager] RoundEndScreenManager non trouvé !");
            }

            if (playerDisplayUI == null)
            {
                playerDisplayUI = FindFirstObjectByType<MusicNotePlayerDisplayUI>();
                if (playerDisplayUI == null)
                    Debug.LogWarning("[MusicNoteGameManager] MusicNotePlayerDisplayUI non trouvé !");
            }

            if (badgeAdapter == null)
            {
                badgeAdapter = GetComponent<MusicBadgeAdapter>();
                if (badgeAdapter == null)
                {
                    badgeAdapter = gameObject.AddComponent<MusicBadgeAdapter>();
                    Debug.Log("[MusicNoteGameManager] MusicBadgeAdapter ajouté automatiquement");
                }
            }
        }

        private void InitializePlayerSystem()
        {
            if (!enablePlayerSystem)
            {
                Debug.Log("[MusicNoteGameManager] Système de joueurs désactivé");
                return;
            }

            gamePlayerSelector = Systems.GamePlayerSelector.Instance;
            if (gamePlayerSelector == null)
            {
                Debug.LogWarning("[MusicNoteGameManager] GamePlayerSelector non trouvé - Mode solo activé");
                isMultiPlayerSession = false;
                return;
            }

            isMultiPlayerSession = gamePlayerSelector.SelectedPlayers.Count > 1;
            currentPlayer = GetNextPlayerToPlay() ?? gamePlayerSelector.CurrentPlayer;

            if (currentPlayer != null)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[MusicNoteGameManager] Système de joueurs initialisé - Joueur actuel: {currentPlayer.Nickname}");
                    if (isMultiPlayerSession)
                    {
                        Debug.Log($"[MusicNoteGameManager] Session multi-joueurs détectée - {gamePlayerSelector.SelectedPlayers.Count} joueurs");
                    }
                }

                // Mettre à jour l'UI du joueur immédiatement
                UpdatePlayerUI();

                // Configurer le joueur dans l'adaptateur de badges
                if (badgeAdapter != null)
                {
                    badgeAdapter.SetCurrentPlayer(currentPlayer.Nickname);
                }
            }
            else
            {
                Debug.LogWarning("[MusicNoteGameManager] Aucun joueur sélectionné - Mode solo activé");
                isMultiPlayerSession = false;
            }
        }

        private void InitializeGame()
        {
            if (enableDebugLogs)
                Debug.Log($"[MusicNoteGameManager] Initialisation du jeu pour {currentPlayer?.Nickname ?? "joueur inconnu"}");

            if (gameController != null)
            {
                // Démarrer le jeu
                gameController.StartGame();
            }
            else
            {
                Debug.LogError("[MusicNoteGameManager] Impossible d'initialiser le jeu - GameController manquant");
            }
        }

        private void HandleGameFinished(int score)
        {
            if (enableDebugLogs)
                Debug.Log($"[MusicNoteGameManager] Fin de partie pour {currentPlayer?.Nickname} - Score: {score}");

            // Sauvegarder le score
            SaveCurrentPlayerScore(score);

            // Mettre à jour les badges
            if (badgeAdapter != null && currentPlayer != null)
            {
                var result = new MusicBadgeAdapter.MusicGameResult(
                    score: score,
                    duration: Time.time,
                    hit: score / 100, // Estimation basée sur le score
                    missed: 0, // On n'a pas cette info pour l'instant
                    acc: 100f * (score / (score + 1)), // Estimation
                    combo: score / 50, // Estimation
                    perfect: score / 200, // Estimation
                    bpm: 120f, // Valeur par défaut
                    diff: 1, // Niveau de base
                    name: "MusicNote Level"
                );
                badgeAdapter.UpdateSongResult(currentPlayer.Nickname, result);
            }

            // Vérifier s'il y a d'autres joueurs
            if (isMultiPlayerSession && HasNextPlayerToPlay())
            {
                // Il y a encore des joueurs à faire jouer
                ShowRoundEndScreen(score);
            }
            else
            {
                // Plus de joueurs à faire jouer :
                // - Si GameSessionManager existe et il reste des mini-jeux, passage au suivant
                // - Sinon, retour au menu principal (géré par GameSessionManager)
                if (enableDebugLogs)
                    Debug.Log("[MusicNoteGameManager] Tous les joueurs ont joué - Transition vers mini-jeu suivant ou retour menu principal");

                ShowFinalRanking();
                ShowNextMiniGameTransition(score);
            }
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
                // Fallback si pas de RoundEndScreen
                StartCoroutine(StartNextPlayerWithDelay());
            }
        }

        /// <summary>
        /// Nouvelle méthode : Affiche l'écran de transition vers le mini-jeu suivant
        /// </summary>
        private void ShowNextMiniGameTransition(int finalScore)
        {
            if (roundEndScreenManager != null && currentPlayer != null)
            {
                // Configurer les callbacks pour la transition
                roundEndScreenManager.OnNextMiniGameCallback = OnTransitionToNextMiniGame;
                
                roundEndScreenManager.gameObject.SetActive(true);
                roundEndScreenManager.ShowNextMiniGameTransition(currentPlayer, finalScore);
            }
            else
            {
                // Fallback : transition directe après délai
                StartCoroutine(DelayedNextMiniGameTransition());
            }
        }

        /// <summary>
        /// Callback appelé pour démarrer la transition vers le mini-jeu suivant
        /// </summary>
        private void OnTransitionToNextMiniGame()
        {
            if (enableDebugLogs)
                Debug.Log("[MusicNoteGameManager] Transition vers mini-jeu suivant déclenchée");

            // Désactiver l'écran de fin
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(false);
                roundEndScreenManager.OnNextMiniGameCallback = null;
            }

            // Déclencher la transition via GameSessionManager
            if (enableGameSessionTransition && gameSessionManager != null)
            {
                Debug.Log("[MusicNoteGameManager] GameSessionManager trouvé - Utilisation de la transition directe (Option A)");
                // Utiliser la nouvelle méthode directe (Option A) au lieu de l'ancienne
                gameSessionManager.LoadNextMiniGameWithLoadingScreen();
            }
            else
            {
                // Fallback : GameSessionManager absent, on redirige vers la scène principale et on relance la session au bon index
                Debug.LogWarning("[MusicNoteGameManager] GameSessionManager non disponible - Redirection automatique vers le menu principal pour reprise de session.");
                // On tente de retrouver l'index du mini-jeu courant dans la liste du GameSessionManager (en utilisant le nom de la scène active)
                int currentMiniGameIndex = -1;
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                Debug.Log($"[MusicNoteGameManager] Fallback: currentMiniGameIndex={currentMiniGameIndex}, scene={currentScene}");
                GameSessionRedirector.ShouldResumeSession = true;
                GameSessionRedirector.ResumeMiniGameIndex = currentMiniGameIndex;
                GameSessionRedirector.ResumeMiniGameSceneName = currentScene;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameManager", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        /// <summary>
        /// Coroutine de fallback pour transition automatique
        /// </summary>
        private IEnumerator DelayedNextMiniGameTransition()
        {
            if (enableDebugLogs)
                Debug.Log($"[MusicNoteGameManager] Transition automatique dans {nextGameTransitionDelay}s");

            yield return new WaitForSeconds(nextGameTransitionDelay);
            OnTransitionToNextMiniGame();
        }

        private void OnRoundEndNextPlayer()
        {
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(false);
                roundEndScreenManager.OnNextPlayerCallback = null;
            }

            PrepareNextPlayer();
        }

        private void PrepareNextPlayer()
        {
            // Passer au joueur suivant
            currentPlayer = GetNextPlayerToPlay();
            if (currentPlayer == null)
            {
                Debug.LogError("[MusicNoteGameManager] Erreur lors du passage au joueur suivant");
                ShowNextMiniGameTransition(0); // Score par défaut
                return;
            }

            // Mettre à jour l'UI
            UpdatePlayerUI();

            // Configurer le joueur dans l'adaptateur de badges
            if (badgeAdapter != null)
            {
                badgeAdapter.SetCurrentPlayer(currentPlayer.Nickname);
            }

            // Réinitialiser et redémarrer le jeu
            if (gameController != null)
            {
                gameController.ResetGame();
                gameController.StartGame();
            }
        }

        private IEnumerator StartNextPlayerWithDelay()
        {
            if (enableDebugLogs)
                Debug.Log($"[MusicNoteGameManager] Préparation pour {GetNextPlayerToPlay()?.Nickname} - Attente de {delayBetweenPlayers}s");

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

                if (enableDebugLogs)
                    Debug.Log($"[MusicNoteGameManager] Score enregistré pour {currentPlayer.Nickname}: {score} points");
            }
        }

        private void ShowFinalRanking()
        {
            if (gamePlayerSelector == null || !isMultiPlayerSession) return;

            var ranking = gamePlayerSelector.GetPlayerRanking();
            if (enableDebugLogs)
            {
                Debug.Log("[MusicNoteGameManager] === CLASSEMENT FINAL ===");
                for (int i = 0; i < ranking.Count; i++)
                {
                    var player = ranking[i];
                    int roundScore = roundScores.ContainsKey(player.Nickname) ? roundScores[player.Nickname] : 0;
                    Debug.Log($"{i + 1}. {player.Nickname} - Score du tour: {roundScore} / Score total: {player.TotalScore}");
                }
            }
        }

        private void UpdatePlayerUI()
        {
            if (playerDisplayUI != null && currentPlayer != null)
            {
                playerDisplayUI.gameObject.SetActive(true);
                playerDisplayUI.UpdateDisplay(currentPlayer, gamePlayerSelector);
            }
        }

        private Systems.PlayerData GetNextPlayerToPlay()
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

        private bool HasPlayerPlayedThisMinigame(Systems.PlayerData player)
        {
            return roundScores.ContainsKey(player.Nickname);
        }

        private void OnDestroy()
        {
            if (gameController != null)
            {
                gameController.OnGameFinished -= HandleGameFinished;
            }
        }

        #region Debug Methods

        [ContextMenu("Debug Player Badges")]
        private void DebugPlayerBadges()
        {
            if (currentPlayer != null && badgeAdapter != null)
            {
                var badges = badgeAdapter.GetPlayerBadges(currentPlayer.Nickname);
                Debug.Log($"=== BADGES DE {currentPlayer.Nickname} ===");
                foreach (var badge in badges)
                {
                    Debug.Log($"- {badge.BadgeDefinition.BadgeName} ({badge.EarnedDate})");
                }
            }
        }

        [ContextMenu("Force Badge Validation")]
        private void ForceBadgeValidation()
        {
            if (currentPlayer != null && badgeAdapter != null)
            {
                int earned = badgeAdapter.ValidatePlayerBadges(currentPlayer.Nickname);
                Debug.Log($"Validation forcée : {earned} badge(s) attribué(s) à {currentPlayer.Nickname}");
            }
        }

        [ContextMenu("Force Next MiniGame Transition")]
        private void ForceNextMiniGameTransition()
        {
            ShowNextMiniGameTransition(999); // Score de test
        }

        #endregion
    }
}