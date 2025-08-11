using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Hand;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Scoring;
using Gameplay.FireFlyDance.Capture;
using Gameplay.FireFlyDance.Utils;
using Gameplay.FireFlyDance.UI;
using Core;
using Core.Analytics;
using Core.Analytics.Core;
using Core.Audio;
using Gameplay.Common.Badges;
using Gameplay.Firefly.Badges;
using UI.RoundEndScreen;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Gestionnaire de logique du mini-jeu "Danse des Lucioles"
    /// Coordonne tous les modules du jeu une fois qu'il est lancé
    /// Gère les lucioles, le score, les interactions et les feedbacks
    /// </summary>
public class FireflyDanceGameManager : MiniGameBase

    {

        #region Fields & References

        [Header("Core References")]
        public UDPReceive udpReceive;
        public FireflyDanceGameController gameController;
        public FireflyDanceConfig config;

        [Header("Game Modules")]
        public FireflySpawner spawner;
        public FireflyScoreManager scoreManager;
        public FireflyDanceTimer timer;

        [Header("Hand Tracking")]
        public HandTracker handTracker;

        [Header("Capture System")]
        public Gameplay.FireFlyDance.Capture.FireflyCapture fireflyCapture;

        [Header("UI & Feedback")]
        public FireflyDanceUIManager uiManager;
        
        [Header("Encouragement System")]
        public EncouragementManager encouragementManager;

        [Header("Player Integration")]
        public FireflyScoreManagerPlayerIntegration scoreIntegration;
        public Gameplay.FireFlyDance.UI.FireflyPlayerDisplayUI playerDisplayUI;

        [Header("Round End Screen")]
        public RoundEndScreenManager roundEndScreenManager;

        [Header("Badge System")]
        public FireflyBadgeAdapter badgeAdapter;

        [Header("Debug Settings")]
        public bool enableDebugMode = true;
        public bool enableDetailedLogs = true;

        [Header("Player System")]
        public bool enablePlayerSystem = true;
        public float delayBetweenPlayers = 3f; // Délai entre les joueurs en secondes

        [Header("Analytics System")]
        // Système d'analytics unifié via AnalyticsHelper

        // Nouveau système d'analytics modulaire
        private string currentAnalyticsSessionId;

        // État interne
        #pragma warning disable CS0414
        private bool isInitialized = false;
        #pragma warning restore CS0414
        private bool isGameActive = false;
        private bool isLaunchedViaMiniGameBase = false; 
        
        // Système de joueurs
        private Systems.GamePlayerSelector gamePlayerSelector;
        private Systems.PlayerData currentPlayer;
        private bool isMultiPlayerSession = false;
        private int currentPlayerScore = 0;

        // Ajout : dictionnaire pour stocker le score du tour de chaque joueur
        private System.Collections.Generic.Dictionary<string, int> roundScores = new System.Collections.Generic.Dictionary<string, int>();

        #endregion

        #region Unity Lifecycle

        protected override void Launch()
        {
            FireflyDanceLogger.Log("🚀 Launch() appelé - Début du lancement du mini-jeu");
            isLaunchedViaMiniGameBase = true;
            
            // Démarrer la session analytics
            StartAnalyticsSession();
            
            // La musique a déjà été démarrée dans Start()
            
            // Initialiser le système de joueurs
            InitializePlayerSystem();
            
            // Initialiser complètement le jeu
            InitializeCompleteGame();
        }

        void Start()
        {
            // Vérifier s'il y a des GameManagers multiples
            var allGameManagers = FindObjectsByType<FireflyDanceGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allGameManagers.Length > 1)
            {
                FireflyDanceLogger.LogWarning($"MULTIPLE GameManagers détectés ({allGameManagers.Length}) !");
                for (int i = 0; i < allGameManagers.Length; i++)
                {
                    FireflyDanceLogger.LogWarning($"  GameManager {i+1}: {allGameManagers[i].gameObject.name}");
                }
            }

            // Auto-initialisation pour les tests directs dans la scène
            StartCoroutine(CheckForAutoInitialization());
        }
        
        /// <summary>
        /// Vérifie si on doit s'auto-initialiser (pour les tests directs dans la scène)
        /// Respecte l'écran de chargement s'il est présent
        /// </summary>
        private System.Collections.IEnumerator CheckForAutoInitialization()
        {
            // Si un LoadingScreenManager est présent et actif, attendre qu'il se termine
            if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsShowing)
            {
                FireflyDanceLogger.LogWarning("⚠️ LoadingScreenManager détecté et actif - Attente de la fin de l'écran de chargement...");

                // Attendre que l'écran de chargement se termine
                yield return new WaitUntil(() => LoadingScreenManager.Instance == null || !LoadingScreenManager.Instance.IsShowing);

                // Attendre encore un peu pour être sûr que tout est initialisé
                yield return new WaitForSeconds(1f);

                FireflyDanceLogger.LogWarning("⚠️ Écran de chargement terminé - Vérification de l'auto-initialisation");
            }
            else
            {
                // Comportement original si pas d'écran de chargement
                yield return new WaitForSeconds(0.5f);
            }

            if (!isLaunchedViaMiniGameBase)
            {
                FireflyDanceLogger.LogWarning("⚠️ Auto-initialisation détectée - Test direct dans la scène");
                Launch();
            }
        }


        void OnDestroy()
        {
            CleanupEventListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise complètement le jeu pour un joueur
        /// </summary>
        private void InitializeCompleteGame()
        {
            FireflyDanceLogger.Log($"🚀 Initialisation complète pour {currentPlayer?.Nickname ?? "Joueur inconnu"}");

            // 1. Réinitialiser l'état interne
            isGameActive = false;
            currentPlayerScore = 0;

            // 2. Trouver tous les composants
            FindAllComponents();

            // 3. Valider les composants essentiels
            if (!ValidateEssentialComponents())
            {
                FireflyDanceLogger.LogError("❌ Composants essentiels manquants - impossible de continuer");
                return;
            }

            // 4. Réinitialiser tous les modules à l'état initial
            ResetAllModulesToInitialState();

            // 5. Configurer les événements
            ConfigureEventListeners();

            // 6. Initialiser le système de badges
            InitializeBadgeSystem();

            // 7. Démarrer le jeu
            StartGameFromScratch();
        }

        /// <summary>
        /// Trouve tous les composants nécessaires
        /// </summary>
        private void FindAllComponents()
        {
            if (gameController == null)
                gameController = FindFirstObjectByType<FireflyDanceGameController>();
            if (spawner == null)
                spawner = FindFirstObjectByType<FireflySpawner>();
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<FireflyScoreManager>();
            if (handTracker == null)
                handTracker = FindFirstObjectByType<HandTracker>();
            if (fireflyCapture == null)
                fireflyCapture = FindFirstObjectByType<Gameplay.FireFlyDance.Capture.FireflyCapture>();
            if (timer == null)
                timer = FindFirstObjectByType<FireflyDanceTimer>();
            if (uiManager == null)
                uiManager = FindFirstObjectByType<FireflyDanceUIManager>();
            if (scoreIntegration == null)
                scoreIntegration = FindFirstObjectByType<FireflyScoreManagerPlayerIntegration>();
            if (playerDisplayUI == null)
                playerDisplayUI = FindFirstObjectByType<Gameplay.FireFlyDance.UI.FireflyPlayerDisplayUI>();
            // Analytics unifié déjà intégré
            FireflyDanceLogger.Log("✅ Analytics système unifié prêt");
            if (roundEndScreenManager == null)
                roundEndScreenManager = FindFirstObjectByType<RoundEndScreenManager>();
        }

        /// <summary>
        /// Valide que les composants essentiels sont présents
        /// </summary>
        private bool ValidateEssentialComponents()
        {
            bool isValid = gameController != null && config != null;
            
            FireflyDanceLogger.Log($"🔍 Validation: GameController={gameController != null}, Config={config != null}");
            
            if (!isValid)
            {
                FireflyDanceLogger.LogError("❌ GameController ou Config manquant");
            }
            
            return isValid;
        }

        /// <summary>
        /// Remet tous les modules à l'état initial
        /// </summary>
        private void ResetAllModulesToInitialState()
        {
            FireflyDanceLogger.Log("🔄 Réinitialisation de tous les modules...");

            // Arrêter tout d'abord
            StopAllSystems();

            // Réinitialiser le GameController à l'état Idle
            if (gameController != null)
            {
                ForceGameControllerToIdle();
            }

            // Réinitialiser le score
            if (scoreManager != null)
            {
                scoreManager.Initialize(config);
                ResetScoreToZero();
                FireflyDanceLogger.Log("✅ ScoreManager réinitialisé");
            }

            // Réinitialiser le spawner
            if (spawner != null)
            {
                spawner.Initialize(config);
                spawner.ClearAllFireflies();
                FireflyDanceLogger.Log("✅ Spawner réinitialisé");
            }

            // Réinitialiser le timer
            if (timer != null)
            {
                timer.ResetTimer();
                FireflyDanceLogger.Log("✅ Timer réinitialisé");
            }

            // Réinitialiser le système de capture
            if (fireflyCapture != null)
            {
                fireflyCapture.SetCaptureEnabled(false);
                FireflyDanceLogger.Log("✅ Capture réinitialisé");
            }

            // Réinitialiser le hand tracker
            if (handTracker != null)
            {
                handTracker.StopTracking();
                handTracker.StartTracking();
                FireflyDanceLogger.Log("✅ HandTracker réinitialisé");
            }

            FireflyDanceLogger.Log("✅ Tous les modules réinitialisés");
        }

        /// <summary>
        /// Force le GameController à l'état Idle
        /// </summary>
        private void ForceGameControllerToIdle()
        {
            try
            {
                FireflyDanceLogger.Log($"🔄 État actuel du GameController: {gameController.CurrentState}");
                
                // Essayer différentes méthodes pour remettre à Idle
                var type = gameController.GetType();
                
                // Méthode 1: Reset direct si disponible
                var resetMethod = type.GetMethod("Reset") ?? type.GetMethod("ResetToIdle");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(gameController, null);
                    FireflyDanceLogger.Log("✅ GameController réinitialisé via Reset()");
                    return;
                }

                // Méthode 2: Forcer l'état via réflection
                var stateField = type.GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (stateField != null)
                {
                    // Essayer de définir l'état à Idle (généralement 0)
                    stateField.SetValue(gameController, 0);
                    FireflyDanceLogger.Log("✅ GameController forcé à Idle via réflection");
                    return;
                }

                FireflyDanceLogger.LogWarning("⚠️ Impossible de réinitialiser le GameController automatiquement");
                
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogWarning($"⚠️ Erreur lors de la réinitialisation du GameController: {ex.Message}");
            }
        }

        /// <summary>
        /// Arrête tous les systèmes
        /// </summary>
        private void StopAllSystems()
        {
            if (spawner != null)
            {
                spawner.StopSpawning();
                spawner.ClearAllFireflies();
            }
            
            if (timer != null)
            {
                timer.StopTimer();
            }
            
            if (fireflyCapture != null)
            {
                fireflyCapture.SetCaptureEnabled(false);
            }
            
            if (handTracker != null)
            {
                handTracker.StopTracking();
            }
        }

        /// <summary>
        /// Remet le score à zéro
        /// </summary>
        private void ResetScoreToZero()
        {
            if (scoreIntegration != null)
            {
                scoreIntegration.ResetScore();
            }
            else
            {
                // Fallback direct
                ResetScoreInManager();
            }
            currentPlayerScore = 0;
        }

        /// <summary>
        /// Configure les event listeners (nettoie d'abord les anciens)
        /// </summary>
        private void ConfigureEventListeners()
        {
            // Nettoyer d'abord
            CleanupEventListeners();
            
            // Reconfigurer
            FireflyDanceEvents.OnGameStarted += OnGameStarted;
            FireflyDanceEvents.OnGameEnded += OnGameEnded;
            FireflyDanceEvents.OnFireflyCaptured += OnFireflyCaptured;
            FireflyDanceEvents.OnTimerCompleted += OnTimerCompleted;
            
            FireflyDanceLogger.Log("✅ Event listeners configurés");
        }

        /// <summary>
        /// Démarre le jeu depuis le début
        /// </summary>
        private void StartGameFromScratch()
        {
            if (gameController == null)
            {
                FireflyDanceLogger.LogError("❌ Impossible de démarrer - GameController manquant");
                return;
            }

            try
            {
                FireflyDanceLogger.Log($"🎮 Démarrage du jeu - État: {gameController.CurrentState}");

                // Utiliser la session analytics existante du GameSessionManager
                string playerName = currentPlayer?.Nickname ?? "Joueur Anonyme";
                FireflyDanceLogger.Log($"📈 Utilisation de la session analytics existante pour {playerName}");

                // Toujours remettre le GameController à Idle avant de démarrer
                gameController.ResetGame();

                // Forcer le passage par tous les états
                gameController.StartCalibration();
                gameController.FinishCalibration();
                gameController.StartGame();

                FireflyDanceLogger.Log("✅ Jeu démarré avec succès");
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"❌ Erreur lors du démarrage: {ex.Message}");
                // En cas d'échec, passer au joueur suivant
                HandleGameStartFailure();
            }
        }

        /// <summary>
        /// Gère l'échec de démarrage du jeu
        /// </summary>
        private void HandleGameStartFailure()
        {
            FireflyDanceLogger.LogWarning("⚠️ Échec du démarrage - passage au joueur suivant");
            
            if (enablePlayerSystem && isMultiPlayerSession)
            {
                // Attendre un peu puis passer au suivant
                StartCoroutine(DelayedNextPlayer());
            }
            else
            {
                // Plus de joueurs - Transition vers le mini-jeu suivant
                ShowNextMiniGameTransition();
            }
        }

        /// <summary>
        /// Passage au joueur suivant avec délai
        /// </summary>
        private System.Collections.IEnumerator DelayedNextPlayer()
        {
            yield return new WaitForSeconds(1f);
            CheckAndRestartForNextPlayer();
        }

        #endregion

        #region Player System

        /// <summary>
        /// Initialise le système de joueurs
        /// </summary>
        private void InitializePlayerSystem()
        {
            if (!enablePlayerSystem)
            {
                FireflyDanceLogger.Log("Système de joueurs désactivé");
                return;
            }

            gamePlayerSelector = Systems.GamePlayerSelector.Instance;
            if (gamePlayerSelector == null)
            {
                FireflyDanceLogger.LogWarning("GamePlayerSelector non trouvé - Mode solo activé");
                isMultiPlayerSession = false;
                return;
            }

            isMultiPlayerSession = gamePlayerSelector.SelectedPlayers.Count > 1;
            
            // Trouver le premier joueur qui n'a pas encore joué, ou prendre le joueur actuel
            currentPlayer = GetNextPlayerToPlay() ?? gamePlayerSelector.CurrentPlayer;

            if (currentPlayer != null)
            {
                FireflyDanceLogger.Log($"Système de joueurs initialisé - Joueur actuel: {currentPlayer.Nickname}");
                if (isMultiPlayerSession)
                {
                    FireflyDanceLogger.Log($"Session multi-joueurs détectée - {gamePlayerSelector.SelectedPlayers.Count} joueurs");
                    
                    // Afficher qui a déjà joué et qui va jouer
                    int playersAlreadyPlayed = 0;
                    foreach (var player in gamePlayerSelector.SelectedPlayers)
                    {
                        if (HasPlayerPlayedThisMinigame(player))
                        {
                            playersAlreadyPlayed++;
                            FireflyDanceLogger.Log($"  ✅ {player.Nickname} a déjà joué");
                        }
                        else
                        {
                            FireflyDanceLogger.Log($"  ⏳ {player.Nickname} va jouer");
                        }
                    }
                    
                    FireflyDanceLogger.Log($"Progression: {playersAlreadyPlayed}/{gamePlayerSelector.SelectedPlayers.Count} joueurs ont joué");
                }
            }
            else
            {
                FireflyDanceLogger.LogWarning("Aucun joueur sélectionné - Mode solo activé");
                isMultiPlayerSession = false;
            }
        }

        /// <summary>
        /// Vérifie s'il y a un autre joueur et redémarre le jeu pour lui
        /// </summary>
        private void CheckAndRestartForNextPlayer()
        {
            if (!isMultiPlayerSession || gamePlayerSelector == null)
            {
                // Pas de multi-joueurs, afficher l'écran de transition avant de finir le mini-jeu
                FireflyDanceLogger.Log("🏁 Fin du mini-jeu - Mode solo ou plus de joueurs");
                ShowNextMiniGameTransition();
                return;
            }

            // Enregistrer le score du joueur actuel
            SaveCurrentPlayerScore();

            // Vérifier s'il reste des joueurs qui n'ont pas encore joué
            if (!HasNextPlayerToPlay())
            {
                // Tous les joueurs ont joué une fois, afficher l'écran de transition avant de finir le mini-jeu
                FireflyDanceLogger.Log("🏁 Tous les joueurs ont joué une fois - Fin du mini-jeu");
                ShowFinalRanking();
                ShowNextMiniGameTransition();
                return;
            }

            // Passer au joueur suivant qui n'a pas encore joué
            var previousPlayer = currentPlayer;
            int previousPlayerRoundScore = currentPlayerScore;
            currentPlayer = GetNextPlayerToPlay();

            if (currentPlayer == null)
            {
                // Sécurité : aucun joueur trouvé, terminer
                FireflyDanceLogger.Log("🏁 Aucun joueur suivant trouvé - Fin du mini-jeu");
                ShowFinalRanking();
                ShowNextMiniGameTransition();
                return;
            }

            FireflyDanceLogger.Log($"🔄 Changement de joueur: {previousPlayer?.Nickname} -> {currentPlayer.Nickname}");

            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(true);
                roundEndScreenManager.ShowEndOfRoundInfo(previousPlayer, currentPlayer, previousPlayerRoundScore);
                roundEndScreenManager.OnNextPlayerCallback = OnRoundEndNextPlayer;
                return;
            }

            // Si pas d'écran de round end, fallback : redémarrer après un délai
            StartCoroutine(RestartForNextPlayerWithDelay());
        }

        // Méthode appelée par l'écran de fin de manche pour passer au joueur suivant
        private void OnRoundEndNextPlayer()
        {
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(false);
                roundEndScreenManager.OnNextPlayerCallback = null;
            }
            // Redémarrer le jeu pour le nouveau joueur
            RestartCompleteGameForCurrentPlayer();
        }

        /// <summary>
        /// Sauvegarde le score du joueur actuel
        /// </summary>
        private void SaveCurrentPlayerScore()
        {
            if (currentPlayer != null)
            {
                // Ajouter le score même s'il est à 0 (pour marquer que le joueur a joué)
                currentPlayer.AddScore(currentPlayerScore);
                gamePlayerSelector.AddScoreToCurrentPlayer(currentPlayerScore);

                // Enregistre le score du tour pour le classement final
                roundScores[currentPlayer.Nickname] = currentPlayerScore;

                // Marquer le joueur comme ayant joué
                MarkCurrentPlayerAsPlayed();

                FireflyDanceLogger.Log($"💾 Score enregistré pour {currentPlayer.Nickname}: {currentPlayerScore} points");
            }
        }

        /// <summary>
        /// Affiche le classement final
        /// </summary>
        private void ShowFinalRanking()
        {
            if (gamePlayerSelector == null) return;

            var ranking = gamePlayerSelector.GetPlayerRanking();
            FireflyDanceLogger.Log($"🏆 === CLASSEMENT FINAL ===");

            for (int i = 0; i < ranking.Count; i++)
            {
                var player = ranking[i];
                int roundScore = roundScores.ContainsKey(player.Nickname) ? roundScores[player.Nickname] : 0;
                FireflyDanceLogger.Log($"🏆 {i + 1}. {player.Nickname} - Score du tour : {roundScore} / Score total : {player.TotalScore}");
            }
        }

        /// <summary>
        /// Redémarre le jeu pour le joueur suivant avec délai et message
        /// </summary>
        private System.Collections.IEnumerator RestartForNextPlayerWithDelay()
        {
            FireflyDanceLogger.Log($"⏱️ Préparation pour {currentPlayer.Nickname} - Attente de {delayBetweenPlayers}s");
            
            // Afficher un message de transition
            ShowTransitionMessage();

            // Attendre le délai
            yield return new WaitForSeconds(delayBetweenPlayers);

            // Redémarrer complètement le jeu pour le nouveau joueur
            RestartCompleteGameForCurrentPlayer();
        }

        /// <summary>
        /// Affiche un message de transition
        /// </summary>
        private void ShowTransitionMessage()
        {
            if (playerDisplayUI != null)
            {
                playerDisplayUI.ShowTransitionMessage($"🎯 Au tour de {currentPlayer.Nickname} !", delayBetweenPlayers);
            }
        }

        /// <summary>
        /// Redémarre complètement le jeu pour le joueur actuel
        /// </summary>
        private void RestartCompleteGameForCurrentPlayer()
        {
            FireflyDanceLogger.Log($"🔄 Redémarrage complet pour {currentPlayer?.Nickname}");
            
            // Mettre à jour l'affichage du joueur
            UpdateCurrentPlayerUI();
            
            // Réinitialiser complètement le jeu
            InitializeCompleteGame();
        }

        /// <summary>
        /// Met à jour l'affichage du joueur actuel dans l'UI
        /// </summary>
        private void UpdateCurrentPlayerUI()
        {
            if (playerDisplayUI != null)
            {
                playerDisplayUI.UpdateDisplay();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Gère le démarrage du jeu - Active TOUS les systèmes nécessaires
        /// </summary>
        private void OnGameStarted()
        {
            isGameActive = true;

            // Mettre à jour l'UI avec le joueur actuel
            UpdateCurrentPlayerUI();

            // 1. Démarrer le spawning des lucioles
            if (spawner != null)
            {
                spawner.StartSpawning();
                FireflyDanceLogger.Log(" Spawn des lucioles activé");
            }

            // 2. Activer le suivi de main
            if (handTracker != null)
            {
                handTracker.StartTracking();
                FireflyDanceLogger.Log(" Suivi de main activé");
            }

            // 3. Activer la capture des lucioles
            if (fireflyCapture != null)
            {
                fireflyCapture.SetCaptureEnabled(true);
                FireflyDanceLogger.Log(" Système de capture activé");
            }

            // 4. Démarrer le timer
            if (timer != null)
            {
                timer.StartTimer();
                FireflyDanceLogger.Log(" Timer démarré");
            }

            // 5. Notifier le score manager
            if(scoreManager != null)
            {
                scoreManager.OnGameStarted();
                FireflyDanceLogger.Log(" Score manager activé");
            }

            FireflyDanceLogger.Log($"🎮 Jeu démarré pour {currentPlayer?.Nickname ?? "Joueur inconnu"}");
            
            // Démarrer le système d'encouragement
            if (encouragementManager != null)
            {
                encouragementManager.StartEncouragement();
                FireflyDanceLogger.Log("🎉 Système d'encouragement activé");
            }
        }

        /// <summary>
        /// Gère la fin du jeu - Récupère le score et passe au joueur suivant
        /// </summary>
        private void OnGameEnded()
        {
            isGameActive = false;

            // Récupérer le score final du tour
            if (scoreManager != null)
            {
                if (scoreIntegration != null)
                {
                    currentPlayerScore = scoreIntegration.GetCurrentScore();
                }
                else
                {
                    currentPlayerScore = GetScoreFromManager();
                }
            }

            FireflyDanceLogger.Log($"🏁 Fin de tour pour {currentPlayer?.Nickname ?? "Joueur inconnu"} - Score: {currentPlayerScore}");

            // ANALYTICS: Enregistrer le score final dans les analytics
            if (currentPlayer != null)
            {
                AnalyticsHelper.RecordFireflyScore(currentPlayer.Id, currentPlayerScore);
                FireflyDanceLogger.Log($"📊 Score final {currentPlayerScore} enregistré dans les analytics pour {currentPlayer.Nickname}");
            }

            // Terminer le système d'analytics et enregistrer le score final
            EndAnalyticsSession();

            // Finaliser le tracking des badges
            if (badgeAdapter != null && currentPlayer != null)
            {
                FinalizeBadgeTracking();
            }

            // Arrêter tous les systèmes
            StopAllSystems();
            
            // Arrêter le système d'encouragement
            if (encouragementManager != null)
            {
                encouragementManager.StopEncouragement();
                FireflyDanceLogger.Log("🎉 Système d'encouragement arrêté");
            }

            // Vérifier s'il y a un autre joueur ou terminer
            if (enablePlayerSystem)
            {
                CheckAndRestartForNextPlayer();
            }
            else
            {
                // Mode solo, terminer le mini-jeu
                ShowNextMiniGameTransition();
            }
        }

        /// <summary>
        /// Nettoie les listeners d'événements
        /// </summary>
        private void CleanupEventListeners()
        {
            FireflyDanceEvents.OnGameStarted -= OnGameStarted;
            FireflyDanceEvents.OnGameEnded -= OnGameEnded;
            FireflyDanceEvents.OnFireflyCaptured -= OnFireflyCaptured;
            FireflyDanceEvents.OnTimerCompleted -= OnTimerCompleted;
        }

        /// <summary>
        /// Gère la pause/reprise du jeu - Inclut la gestion de la capture
        /// </summary>
        /// <param name="isPaused">État de pause</param>
        private void OnGamePaused(bool isPaused)
        {
            if (isPaused)
            {
                FireflyDanceLogger.Log(" Jeu mis en pause - Désactivation temporaire des systèmes");
                
                // Pause du spawning
                if (spawner != null)
                {
                    spawner.PauseSpawning();
                    FireflyDanceLogger.Log(" Spawn mis en pause");
                }
                
                // Désactiver la capture pendant la pause
                if (fireflyCapture != null)
                {
                    fireflyCapture.SetCaptureEnabled(false);
                    FireflyDanceLogger.Log(" Capture désactivée pendant la pause");
                }
                
                // Pause du timer
                if (timer != null)
                {
                    timer.PauseTimer(true);
                    FireflyDanceLogger.Log(" Timer mis en pause");
                }
            }
            else
            {
                FireflyDanceLogger.Log(" Jeu repris - Réactivation des systèmes");
                
                // Reprendre le spawning
                if (spawner != null)
                {
                    spawner.ResumeSpawning();
                    FireflyDanceLogger.Log(" Spawn repris");
                }
                
                // Réactiver la capture après la pause
                if (fireflyCapture != null)
                {
                    fireflyCapture.SetCaptureEnabled(true);
                    FireflyDanceLogger.Log(" Capture réactivée");
                }
                
                // Reprendre le timer
                if (timer != null)
                {
                    timer.ResumeTimer();
                    FireflyDanceLogger.Log(" Timer repris");
                }
            }
        }

        /// <summary>
        /// Gère la capture d'une luciole (NOTIFICATION UNIQUEMENT - le score est déjà géré par FireflyCapture)
        /// </summary>
        /// <param name="firefly">Luciole capturée</param>
        /// <param name="points">Points gagnés</param>
        private void OnFireflyCaptured(FireflyController firefly, int points)
        {
            // Mettre à jour le score local du joueur actuel
            currentPlayerScore += points;
            
            // Note: L'analytics est déjà géré dans FireflyCapture.cs pour éviter la duplication
            
            // Tracking pour les badges
            if (badgeAdapter != null && currentPlayer != null)
            {
                try
                {
                    badgeAdapter.IncrementFirefliesCollected(currentPlayer.Nickname);
                    badgeAdapter.UpdatePlayerScore(currentPlayer.Nickname, currentPlayerScore);
                    
                    if (enableDetailedLogs)
                    {
                        FireflyDanceLogger.LogVerbose($"🏆 Badge tracking: +1 luciole, score={currentPlayerScore}");
                    }
                }
                catch (System.Exception ex)
                {
                    FireflyDanceLogger.LogError($"❌ Erreur badge tracking: {ex.Message}");
                }
            }
            
            FireflyDanceLogger.Log($"Luciole capturée par {currentPlayer?.Nickname ?? "Joueur"} - Points: {points} (Total: {currentPlayerScore})");
        }

        /// <summary>
        /// Gère l'apparition d'une luciole
        /// </summary>
        /// <param name="firefly">Luciole créée</param>
        private void OnFireflySpawned(FireflyController firefly)
        {
            if (enableDetailedLogs)
            {
                FireflyDanceLogger.LogVerbose($"Luciole apparue à {firefly.transform.position}");
            }
        }

        /// <summary>
        /// Gère l'expiration d'une luciole
        /// </summary>
        /// <param name="firefly">Luciole expirée</param>
        private void OnFireflyExpired(FireflyController firefly)
        {
            if (enableDetailedLogs)
            {
                FireflyDanceLogger.LogVerbose("Luciole expirée");
            }
        }

        /// <summary>
        /// Gère la fin du timer
        /// </summary>
        private void OnTimerCompleted()
        {
            FireflyDanceLogger.Log("Timer terminé - Fin du jeu");
            
            if (gameController != null)
            {
                gameController.EndGame();
            }
        }

        #endregion

        #region Validation & Utility

        /// <summary>
        /// Valide la configuration du manager
        /// </summary>
        private void ValidateConfiguration()
        {
            bool isValid = true;

            if (config == null)
            {
                FireflyDanceLogger.LogError("Configuration manquante");
                isValid = false;
            }

            if (gameController == null)
            {
                FireflyDanceLogger.LogError("GameController manquant");
                isValid = false;
            }

            if (!isValid)
            {
                FireflyDanceLogger.LogError("Configuration invalide - certaines fonctionnalités peuvent ne pas marcher");
            }
        }

        /// <summary>
        /// Valide la présence des composants essentiels
        /// </summary>
        /// <returns>True si tous les composants essentiels sont présents</returns>
        private bool ValidateComponents()
        {
            bool hasEssentials = gameController != null && config != null;
            
            FireflyDanceLogger.Log($"🔍 Validation des composants essentiels:");
            FireflyDanceLogger.Log($"  GameController: {(gameController != null ? "✅" : "❌")}");
            FireflyDanceLogger.Log($"  Config: {(config != null ? "✅" : "❌")}");
            
            // Validation du système d'encouragement (optionnel)
            if (encouragementManager == null)
            {
                encouragementManager = FindFirstObjectByType<EncouragementManager>();
                if (encouragementManager == null)
                    FireflyDanceLogger.LogWarning("EncouragementManager non trouvé - Messages d'encouragement désactivés");
                else
                    FireflyDanceLogger.Log("  EncouragementManager: ✅ (trouvé automatiquement)");
            }
            else
            {
                FireflyDanceLogger.Log("  EncouragementManager: ✅");
            }
            
            if (!hasEssentials)
            {
                string missing = "";
                if (gameController == null) missing += "GameController ";
                if (config == null) missing += "Config ";
                
                FireflyDanceLogger.LogError($"❌ Composants essentiels manquants: {missing}");
                FireflyDanceLogger.LogError("⚠️ Le jeu ne peut pas démarrer sans ces composants");
            }
            else
            {
                FireflyDanceLogger.Log("✅ Tous les composants essentiels sont présents");
            }

            return hasEssentials;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Force la fin du jeu
        /// </summary>
        public void ForceEndGame()
        {
            if (gameController != null)
            {
                gameController.EndGame();
            }
        }

        /// <summary>
        /// 🛑 Force l'arrêt complet et immédiat de tous les systèmes
        /// Utilisé en cas d'urgence ou pour nettoyer proprement
        /// </summary>
        public void ForceShutdownAllSystems()
        {
            FireflyDanceLogger.Log("🛑 Arrêt forcé de tous les systèmes FireflyDance");
            StopAllSystems();
            if (scoreManager != null)
            {
                scoreManager.OnGameEnded();
            }
            isGameActive = false;
            FireflyDanceLogger.Log("✅ Tous les systèmes ont été arrêtés");
        }

        /// <summary>
        /// Met le jeu en pause
        /// </summary>
        public void PauseGame()
        {
            if (gameController != null)
            {
                gameController.PauseGame();
            }
        }

        /// <summary>
        /// Reprend le jeu
        /// </summary>
        public void ResumeGame()
        {
            if (gameController != null)
            {
                gameController.ResumeGame();
            }
        }

        /// <summary>
        /// Obtient le score actuel
        /// </summary>
        /// <returns>Score actuel</returns>
        public int GetCurrentScore()
        {
            return scoreManager != null ? scoreManager.CurrentScore : 0;
        }

        /// <summary>
        /// Indique si le jeu est actif
        /// </summary>
        /// <returns>True si le jeu est en cours</returns>
        public bool IsGameActive()
        {
            return isGameActive && gameController != null && gameController.IsPlaying;
        }

        /// <summary>
        /// Obtient l'adaptateur de badges (pour accès externe)
        /// </summary>
        /// <returns>FireflyBadgeAdapter ou null si non configuré</returns>
        public FireflyBadgeAdapter GetBadgeAdapter()
        {
            return badgeAdapter;
        }

        /// <summary>
        /// Force la validation des badges pour le joueur actuel (pour debug/test)
        /// </summary>
        [ContextMenu("Force Badge Validation")]
        public void ForceBadgeValidation()
        {
            if (badgeAdapter != null && currentPlayer != null)
            {
                try
                {
                    int badges = badgeAdapter.ValidatePlayerBadges(currentPlayer.Nickname);
                    FireflyDanceLogger.Log($"🏆 Validation forcée: {badges} badges attribués à {currentPlayer.Nickname}");
                }
                catch (System.Exception ex)
                {
                    FireflyDanceLogger.LogError($"❌ Erreur validation forcée: {ex.Message}");
                }
            }
            else
            {
                FireflyDanceLogger.LogWarning("⚠️ Badge adapter ou joueur non configuré");
            }
        }

        /// <summary>
        /// Affiche les statistiques de badges du joueur actuel (pour debug)
        /// </summary>
        [ContextMenu("Debug Player Badges")]
        public void DebugPlayerBadges()
        {
            if (currentPlayer != null)
            {
                var storage = FindFirstObjectByType<GlobalPlayerBadgeStorage>();
                if (storage != null)
                {
                    var badges = storage.GetPlayerBadges(currentPlayer.Nickname);
                    FireflyDanceLogger.Log($"🏆 {currentPlayer.Nickname} possède {badges.Count} badges:");
                    foreach (var badge in badges)
                    {
                        FireflyDanceLogger.Log($"  - {badge.GetFullBadgeId()} (obtenu le {badge.EarnedDate})");
                    }
                }
                else
                {
                    FireflyDanceLogger.LogWarning("⚠️ GlobalPlayerBadgeStorage non trouvé");
                }
            }
            else
            {
                FireflyDanceLogger.LogWarning("⚠️ Aucun joueur actuel");
            }
        }

        /// <summary>
        /// Force le passage au joueur suivant (pour debug/test)
        /// </summary>
        [ContextMenu("Force Next Player")]
        public void ForceNextPlayer()
        {
            if (enablePlayerSystem && isMultiPlayerSession)
            {
                CheckAndRestartForNextPlayer();
            }
            else
            {
                FireflyDanceLogger.LogWarning("Pas en mode multi-joueurs ou système désactivé");
            }
        }

        /// <summary>
        /// Force le redémarrage complet pour le joueur actuel (pour debug)
        /// </summary>
        [ContextMenu("Force Restart Current Player")]
        public void ForceRestartCurrentPlayer()
        {
            FireflyDanceLogger.Log("🔄 Redémarrage forcé pour le joueur actuel");
            RestartCompleteGameForCurrentPlayer();
        }

        /// <summary>
        /// Affiche les informations sur le joueur actuel (pour debug)
        /// </summary>
        [ContextMenu("Debug Current Player")]
        public void DebugCurrentPlayer()
        {
            if (currentPlayer != null)
            {
                FireflyDanceLogger.Log($"=== JOUEUR ACTUEL ===");
                FireflyDanceLogger.Log($"Nom: {currentPlayer.Nickname}");
                FireflyDanceLogger.Log($"Score du tour: {currentPlayerScore}");
                FireflyDanceLogger.Log($"Score total: {currentPlayer.TotalScore}");
                
                if (gamePlayerSelector != null)
                {
                    FireflyDanceLogger.Log($"Index: {gamePlayerSelector.CurrentPlayerIndex + 1}/{gamePlayerSelector.SelectedPlayers.Count}");
                }
            }
            else
            {
                FireflyDanceLogger.Log("Aucun joueur actuel");
            }
        }

        /// <summary>
        /// Affiche l'état détaillé du GameController (pour debug)
        /// </summary>
        [ContextMenu("Debug GameController State")]
        public void DebugGameControllerState()
        {
            if (gameController != null)
            {
                FireflyDanceLogger.Log($"=== ÉTAT GAMECONTROLLER ===");
                FireflyDanceLogger.Log($"État actuel: {gameController.CurrentState}");
                FireflyDanceLogger.Log($"GameObject actif: {gameController.gameObject.activeInHierarchy}");
                FireflyDanceLogger.Log($"Composant activé: {gameController.enabled}");
            }
            else
            {
                FireflyDanceLogger.Log("GameController est null");
            }
        }

        #endregion

        #region Fallback Methods

        /// <summary>
        /// Récupère le score depuis le ScoreManager (méthode fallback)
        /// </summary>
        private int GetScoreFromManager()
        {
            if (scoreManager == null) return 0;

            try
            {
                // Essayer différentes méthodes pour accéder au score
                var type = typeof(FireflyScoreManager);
                
                // Méthode 1: Propriété publique
                var scoreProperty = type.GetProperty("CurrentScore");
                if (scoreProperty != null)
                {
                    return (int)scoreProperty.GetValue(scoreManager);
                }

                // Méthode 2: Champ public
                var scoreField = type.GetField("currentScore");
                if (scoreField != null)
                {
                    return (int)scoreField.GetValue(scoreManager);
                }

                // Méthode 3: Champ privé
                scoreField = type.GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (scoreField != null)
                {
                    return (int)scoreField.GetValue(scoreManager);
                }

                FireflyDanceLogger.LogWarning("Impossible de récupérer le score depuis FireflyScoreManager");
                return 0;
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"Erreur lors de la récupération du score: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Remet le score à zéro dans le ScoreManager (méthode fallback)
        /// </summary>
        private void ResetScoreInManager()
        {
            if (scoreManager == null) return;

            try
            {
                var type = typeof(FireflyScoreManager);
                
                // Méthode 1: Méthode Reset
                var resetMethod = type.GetMethod("ResetScore") ?? type.GetMethod("Reset");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(scoreManager, null);
                    FireflyDanceLogger.Log("Score réinitialisé via méthode Reset");
                    return;
                }

                // Méthode 2: Propriété settable
                var scoreProperty = type.GetProperty("CurrentScore");
                if (scoreProperty != null && scoreProperty.CanWrite)
                {
                    scoreProperty.SetValue(scoreManager, 0);
                    FireflyDanceLogger.Log("Score réinitialisé via propriété");
                    return;
                }

                // Méthode 3: Champ direct
                var scoreField = type.GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (scoreField != null)
                {
                    scoreField.SetValue(scoreManager, 0);
                    FireflyDanceLogger.Log("Score réinitialisé via champ direct");
                    return;
                }

                FireflyDanceLogger.LogWarning("Impossible de réinitialiser le score de FireflyScoreManager");
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"Erreur lors de la réinitialisation du score: {ex.Message}");
            }
        }

        #endregion

        #region One Turn Per Player Logic

        /// <summary>
        /// Vérifie s'il reste des joueurs qui n'ont pas encore joué ce mini-jeu
        /// </summary>
        private bool HasNextPlayerToPlay()
        {
            if (gamePlayerSelector == null) return false;

            // Parcourir tous les joueurs pour voir s'il y en a un qui n'a pas encore joué
            foreach (var player in gamePlayerSelector.SelectedPlayers)
            {
                if (!HasPlayerPlayedThisMinigame(player))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Récupère le prochain joueur qui n'a pas encore joué ce mini-jeu
        /// </summary>
        private Systems.PlayerData GetNextPlayerToPlay()
        {
            if (gamePlayerSelector == null) return null;

            // Trouver le premier joueur qui n'a pas encore joué
            foreach (var player in gamePlayerSelector.SelectedPlayers)
            {
                if (!HasPlayerPlayedThisMinigame(player))
                {
                    // Mettre à jour l'index du joueur actuel dans le sélecteur
                    int playerIndex = gamePlayerSelector.SelectedPlayers.IndexOf(player);
                    gamePlayerSelector.SetCurrentPlayer(playerIndex);
                    return player;
                }
            }

            return null;
        }

        /// <summary>
        /// Vérifie si un joueur a déjà joué ce mini-jeu
        /// </summary>
        private bool HasPlayerPlayedThisMinigame(Systems.PlayerData player)
        {
            // Pour l'instant, on utilise une logique simple : 
            // un joueur a joué s'il a un score > 0 pour cette session
            // Vous pouvez améliorer cette logique selon vos besoins
            return player.TotalScore > 0;
        }

        /// <summary>
        /// Marque le joueur actuel comme ayant joué ce mini-jeu
        /// </summary>
        private void MarkCurrentPlayerAsPlayed()
        {
            if (currentPlayer != null)
            {
                // Cette méthode peut être étendue pour marquer explicitement
                // qu'un joueur a joué ce mini-jeu spécifique
                FireflyDanceLogger.Log($"🎯 {currentPlayer.Nickname} a terminé son tour");
            }
        }

        /// <summary>
        /// Callback appelé pour démarrer la transition vers le mini-jeu suivant (Option A - Direct)
        /// </summary>
        private void OnTransitionToNextMiniGame()
        {
            var gsm = FindFirstObjectByType<GameSessionManager>();
            if (gsm != null)
            {
                Debug.Log("[FireflyDanceGameManager] GameSessionManager trouvé - Utilisation de la transition directe (Option A)");
                // Utiliser la nouvelle méthode directe (Option A) au lieu de l'ancienne
                gsm.LoadNextMiniGameWithLoadingScreen();
                return;
            }
            // Fallback : GameSessionManager absent, on redirige vers la scène principale et on relance la session au bon index
            Debug.LogWarning("[FireflyDanceGameManager] GameSessionManager non disponible - Redirection automatique vers le menu principal pour reprise de session.");
            int currentMiniGameIndex = -1;
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[FireflyDanceGameManager] Fallback: currentMiniGameIndex={{currentMiniGameIndex}}, scene={{currentScene}}");
            GameSessionRedirector.ShouldResumeSession = true;
            GameSessionRedirector.ResumeMiniGameIndex = currentMiniGameIndex;
            GameSessionRedirector.ResumeMiniGameSceneName = currentScene;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameManager", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }


        
        /// <summary>
        /// Affiche l'écran de transition vers le mini-jeu suivant (ou fallback si GameSessionManager absent)
        /// </summary>
        private void ShowNextMiniGameTransition()
        {
            if (roundEndScreenManager != null && currentPlayer != null)
            {
                roundEndScreenManager.OnNextMiniGameCallback = OnTransitionToNextMiniGame;
                roundEndScreenManager.gameObject.SetActive(true);
                roundEndScreenManager.ShowNextMiniGameTransition(currentPlayer, currentPlayerScore);
            }
            else
            {
                // Fallback : transition directe
                OnTransitionToNextMiniGame();
            }
        }

    

        #endregion

        #region Badge System

        /// <summary>
        /// Initialise le système de badges pour le joueur actuel
        /// </summary>
        private void InitializeBadgeSystem()
        {
            try
            {
                // Trouver l'adaptateur si pas assigné
                if (badgeAdapter == null)
                {
                    badgeAdapter = FindFirstObjectByType<FireflyBadgeAdapter>();
                    if (badgeAdapter == null)
                    {
                        FireflyDanceLogger.LogWarning("⚠️ FireflyBadgeAdapter non trouvé - système de badges désactivé");
                        return;
                    }
                }

                // Configurer le joueur actuel
                if (currentPlayer != null)
                {
                    badgeAdapter.SetCurrentPlayer(currentPlayer.Nickname);
                    FireflyDanceLogger.Log($"🏆 Système de badges initialisé pour {currentPlayer.Nickname}");
                }
                else
                {
                    FireflyDanceLogger.LogWarning("⚠️ Aucun joueur actuel - système de badges non configuré");
                }
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"❌ Erreur lors de l'initialisation du système de badges: {ex.Message}");
            }
        }

        /// <summary>
        /// Finalise le tracking des badges en fin de partie
        /// </summary>
        private void FinalizeBadgeTracking()
        {
            try
            {
                var playerName = currentPlayer.Nickname;
                
                // Mise à jour finale du score
                badgeAdapter.UpdatePlayerScore(playerName, currentPlayerScore);
                
                // Calculer et mettre à jour la durée de jeu
                float gameDuration = 0f;
                if (timer != null)
                {
                    gameDuration = timer.TimeElapsed;
                    badgeAdapter.UpdateGameDuration(playerName, gameDuration);
                }
                
                // Calculer et mettre à jour la précision si disponible
                float accuracy = CalculateAccuracy();
                if (accuracy > 0f)
                {
                    badgeAdapter.UpdatePlayerAccuracy(playerName, accuracy);
                }
                
                // Validation finale et attribution des badges
                int badgesEarned = badgeAdapter.ValidatePlayerBadges(playerName);
                
                FireflyDanceLogger.Log($"🏆 Badges - Joueur: {playerName}, Score: {currentPlayerScore}, Durée: {gameDuration:F1}s, Précision: {accuracy:F1}%, Badges gagnés: {badgesEarned}");
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"❌ Erreur lors de la finalisation des badges: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcule la précision du joueur basée sur les statistiques disponibles
        /// </summary>
        /// <returns>Précision en pourcentage (0-100)</returns>
        private float CalculateAccuracy()
        {
            try
            {
                // Méthode 1: Utiliser les données du ScoreManager
                if (scoreManager != null)
                {
                    int firefliesCaptured = scoreManager.FirefliesCaptured;
                    int currentScore = scoreManager.CurrentScore;
                    
                    // Calculer un score de base estimé (selon la config ou valeur par défaut)
                    int expectedScorePerFirefly = 100; // Valeur par défaut
                    if (config != null)
                    {
                        // Tenter de récupérer depuis la config si disponible
                        expectedScorePerFirefly = 100; // À adapter selon FireflyDanceConfig
                    }
                    
                    if (firefliesCaptured > 0)
                    {
                        // Calculer la précision basée sur le score moyen par luciole
                        float averageScorePerFirefly = (float)currentScore / firefliesCaptured;
                        float accuracy = Mathf.Clamp01(averageScorePerFirefly / expectedScorePerFirefly) * 100f;
                        return accuracy;
                    }
                }
                
                // Méthode 2: Estimation basée sur le temps et le score
                if (timer != null && currentPlayerScore > 0)
                {
                    float gameTime = timer.TimeElapsed;
                    float totalGameTime = timer.TotalDuration;
                    
                    if (totalGameTime > 0 && gameTime > 0)
                    {
                        // Score par seconde comme indicateur de performance
                        float scorePerSecond = currentPlayerScore / gameTime;
                        
                        // Estimation: un bon joueur fait ~50-150 points par seconde
                        float estimatedMaxScorePerSecond = 150f;
                        float accuracy = Mathf.Clamp01(scorePerSecond / estimatedMaxScorePerSecond) * 100f;
                        
                        return Mathf.Clamp(accuracy, 0f, 100f);
                    }
                }
                
                // Méthode 3: Fallback simple basé sur le score
                if (currentPlayerScore > 0)
                {
                    // Estimation basique: plus le score est élevé, meilleure est la précision supposée
                    float estimatedMaxScore = 2000f; // Score maximal raisonnable
                    float accuracy = Mathf.Clamp01(currentPlayerScore / estimatedMaxScore) * 80f + 20f; // Entre 20% et 100%
                    return accuracy;
                }
                
                return 0f; // Aucune donnée disponible
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"❌ Erreur calcul précision: {ex.Message}");
                return 0f;
            }
        }

        #endregion

        #region Analytics Integration

        /// <summary>
        /// Démarre une session d'analytics pour le jeu Firefly Dance
        /// </summary>
        private void StartAnalyticsSession()
        {
            try
            {
                string playerName = currentPlayer?.Nickname ?? "Player_Unknown";
                // Ne pas créer de nouvelle session, utiliser celle existante du GameSessionManager
                
                FireflyDanceLogger.Log($"📊 Utilisation de la session analytics existante pour {playerName}");
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"❌ Erreur analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Termine la session analytics et marque le mini-jeu comme complété
        /// </summary>
        private void EndAnalyticsSession()
        {
            try
            {
                // Utiliser notre nouveau système de métriques qui se base sur la session du GameSessionManager
                FireflyDanceLogger.Log($"📈 Fin de jeu - Score final: {currentPlayerScore}");
                
                // Marquer le mini-jeu comme complété dans la session globale
                AnalyticsHelper.CompleteMiniGame("FireflyDance");
                
                FireflyDanceLogger.Log("✅ Firefly Dance marqué comme complété dans la session globale");
            }
            catch (System.Exception ex)
            {
                FireflyDanceLogger.LogError($"❌ Erreur fin analytics: {ex.Message}");
            }
        }

        #endregion
    }
}