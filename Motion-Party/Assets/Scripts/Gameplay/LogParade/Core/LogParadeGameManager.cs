using UnityEngine;
using Systems;
using UnityEngine.SceneManagement;
using UI.RoundEndScreen;
using UnityEngine.Events;
using Gameplay.LogParade.UI;
using Gameplay.LogParade.Core;

namespace Gameplay.LogParade.Core
{
    /// <summary>
    /// Gestionnaire principal du mini-jeu LogParade avec support multijoueur
    /// </summary>
    public class LogParadeGameManager : MiniGameBase
    {
        [Header("Components")]
        [SerializeField] private LogParadeGameController gameController;
        [SerializeField] private LogParadeGameLauncher gameLauncher;
        [SerializeField] private RoundEndScreenManager roundEndScreenManager;

        [Header("Configuration")]
        [SerializeField] private bool enableDebugMode = false;
        [SerializeField] private bool enablePlayerSystem = true;
        [SerializeField] private float delayBetweenPlayers = 3f;

        private GamePlayerSelector playerSelector;
        private bool isRoundInProgress = false;

        private void Start()
        {
            Debug.Log("LogParadeGameManager Start -------");
            // Initialiser le sélecteur de joueurs
            playerSelector = GamePlayerSelector.Instance;
            if (playerSelector == null && enablePlayerSystem)
            {
                Debug.LogError("[LogParadeGameManager] GamePlayerSelector non trouvé!");
                return;
            }
            else
            {
                Debug.Log($"[LogParadeGameManager] Démarrage avec {(playerSelector?.PlayerCount ?? 0)} joueurs");
            }

            // Configurer le RoundEndScreen
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(false);
                // Correction : UnityEvent n'est pas un Component, il faut s'abonner à l'événement public du RoundEndScreenManager
                // Supposons qu'il expose un UnityEvent nommé OnNextPlayerButtonClicked
                var eventField = roundEndScreenManager.GetType().GetField("OnNextPlayerButtonClicked");
                if (eventField != null)
                {
                    var unityEvent = eventField.GetValue(roundEndScreenManager) as UnityEvent;
                    if (unityEvent != null)
                    {
                        unityEvent.AddListener(OnNextPlayerButtonClicked);
                        Debug.Log("[LogParadeGameManager] Abonné à OnNextPlayerButtonClicked");
                    }
                    else
                    {
                        Debug.LogWarning("[LogParadeGameManager] Le champ OnNextPlayerButtonClicked n'est pas un UnityEvent valide.");
                    }
                }
                else
                {
                    Debug.LogWarning("[LogParadeGameManager] Aucun champ OnNextPlayerButtonClicked trouvé sur RoundEndScreenManager.");
                }
                Debug.Log("[LogParadeGameManager] RoundEndScreenManager configuré");
            }

            if (enableDebugMode)
            {
                Debug.Log($"[LogParadeGameManager] Démarrage avec {(playerSelector?.PlayerCount ?? 0)} joueurs");
            }

            // Lancer le jeu
            Launch();

        }

        protected override void Launch()
        {
            Debug.Log("LogParadeGameManager Launch -------");
            if (!enablePlayerSystem || playerSelector == null)
            {
                StartSinglePlayerGame();
                return;
            }
            StartMultiPlayerGame();
        }

        /// <summary>
        /// Méthode publique pour lancer le jeu depuis l'extérieur (GameLauncher)
        /// </summary>
        public void LaunchGameManager()
        {
            Debug.Log("LogParadeGameManager LaunchGameManager -------");
            Launch();
        }

        private void StartSinglePlayerGame()
        {
            if (enableDebugMode)
                Debug.Log("[LogParadeGameManager] Démarrage mode solo");

            StartRound();
        }

        private void StartMultiPlayerGame()
        {
            if (enableDebugMode)
                Debug.Log($"[LogParadeGameManager] Démarrage mode multijoueur avec {playerSelector.PlayerCount} joueurs");

            // Démarrer avec le premier joueur
            StartRound();
        }

        private void StartRound()
        {
            Debug.Log("[StartRound] Démarrage du round --------");
            if (isRoundInProgress)
                return;

            isRoundInProgress = true;

            // Configurer le jeu pour le joueur actuel
            if (enablePlayerSystem && playerSelector != null)
            {
                var currentPlayer = playerSelector.CurrentPlayer;
                if (currentPlayer != null && enableDebugMode)
                {
                    Debug.Log($"[LogParadeGameManager] Tour de {currentPlayer.Nickname}");
                }
            }

            // Démarrer le jeu
            if (gameLauncher != null)
            {
                gameLauncher.AutoStartCalibrationCoroutine();
            }
        }

        public void OnGameOver(float score, float duration)
        {
            isRoundInProgress = false;

            if (enablePlayerSystem && playerSelector != null)
            {
                // Ajouter le score au joueur actuel
                playerSelector.AddScoreToCurrentPlayer(Mathf.RoundToInt(score));

                // Afficher l'écran de fin de round
                if (roundEndScreenManager != null)
                {
                    var currentPlayer = playerSelector.CurrentPlayer;
                    roundEndScreenManager.gameObject.SetActive(true);
                    roundEndScreenManager.GetComponent<RoundEndScreenUI>()?.DisplayResults(
                        currentPlayer?.Nickname ?? "Joueur",
                        score,
                        duration,
                        playerSelector.IsGameActive && playerSelector.GetPlayerIndex(currentPlayer) < playerSelector.PlayerCount - 1
                    );
                }
                else
                {
                    // Passer directement au joueur suivant
                    ProcessNextPlayer();
                }
            }
            else
            {
                // Mode solo : terminer le jeu
                EndGame();
            }
        }

        private void OnNextPlayerButtonClicked()
        {
            ProcessNextPlayer();
        }

        private void ProcessNextPlayer()
        {
            if (!enablePlayerSystem || playerSelector == null)
            {
                EndGame();
                return;
            }

            // Cacher l'écran de fin de round
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(false);
            }

            // Passer au joueur suivant
            var nextPlayer = playerSelector.NextPlayer();
            if (nextPlayer != null)
            {
                // Il reste des joueurs
                Invoke(nameof(StartRound), delayBetweenPlayers);
            }
            else
            {
                // Tous les joueurs ont joué
                EndGame();
            }
        }

        private void EndGame()
        {
            if (enablePlayerSystem && playerSelector != null)
            {
                playerSelector.EndGame();
            }

            if (onGameFinished != null)
            {
                onGameFinished.Invoke();
            }
        }
    }
} 