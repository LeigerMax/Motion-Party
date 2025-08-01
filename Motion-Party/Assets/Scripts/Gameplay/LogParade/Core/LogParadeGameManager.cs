using UnityEngine;
using System.Collections;
using Core;
using UI.RoundEndScreen;

namespace Gameplay.LogParade.Core
{
    /// <summary>
    /// Gestionnaire principal du mini-jeu LogParade
    /// Gère l'intégration avec le système de session de jeux
    /// </summary>
    public class LogParadeGameManager : MiniGameBase
    {
        [Header("Core References")]
        public LogParadeGameController gameController;
        public LogParadeGameLauncher gameLauncher;

        [Header("UI References")]
        public RoundEndScreenManager roundEndScreenManager;

        [Header("Session Management")]
        public bool enableGameSessionTransition = true;
        public float nextGameTransitionDelay = 3f;

        [Header("Debug")]
        public bool enableDebugLogs = true;

        // Référence au GameSessionManager
        private GameSessionManager gameSessionManager;

        protected override void Launch()
        {
            if (enableDebugLogs)
                Debug.Log("[LogParadeGameManager] Launch() appelé");

            ValidateComponents();
            InitializeGame();
        }

        void Start()
        {
            ValidateComponents();
            FindGameSessionManager();
            Launch();
        }

        private void FindGameSessionManager()
        {
            gameSessionManager = FindFirstObjectByType<GameSessionManager>();
            if (gameSessionManager == null && enableGameSessionTransition)
            {
                Debug.LogWarning("[LogParadeGameManager] GameSessionManager non trouvé - Transition automatique désactivée");
            }
            else if (enableDebugLogs)
            {
                Debug.Log("[LogParadeGameManager] GameSessionManager trouvé");
            }
        }

        private void ValidateComponents()
        {
            if (gameController == null)
            {
                gameController = FindFirstObjectByType<LogParadeGameController>();
                if (gameController == null)
                    Debug.LogError("[LogParadeGameManager] LogParadeGameController non trouvé !");
            }

            if (gameLauncher == null)
            {
                gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
                if (gameLauncher == null)
                    Debug.LogWarning("[LogParadeGameManager] LogParadeGameLauncher non trouvé !");
            }

            if (roundEndScreenManager == null)
            {
                roundEndScreenManager = FindFirstObjectByType<RoundEndScreenManager>();
                if (roundEndScreenManager == null)
                    Debug.LogWarning("[LogParadeGameManager] RoundEndScreenManager non trouvé !");
            }
        }

        private void InitializeGame()
        {
            if (enableDebugLogs)
                Debug.Log("[LogParadeGameManager] Initialisation du jeu LogParade");

            // S'abonner aux événements du GameController
            if (gameController != null)
            {
                gameController.OnGameCompleted += HandleGameFinished;
                Debug.Log("[LogParadeGameManager] Abonné à OnGameCompleted du GameController");
            }
            else
            {
                Debug.LogError("[LogParadeGameManager] GameController null - Impossible de s'abonner à OnGameCompleted !");
            }

            if (gameLauncher != null)
            {
                // Le GameLauncher gère le démarrage du jeu
                if (enableDebugLogs)
                    Debug.Log("[LogParadeGameManager] Démarrage via LogParadeGameLauncher");
            }
            else
            {
                Debug.LogError("[LogParadeGameManager] Impossible de démarrer - GameLauncher manquant");
            }
        }

        /// <summary>
        /// Appelée quand le jeu se termine - configure la transition vers le mini-jeu suivant
        /// </summary>
        private void HandleGameFinished()
        {
            Debug.Log("[LogParadeGameManager] *** EVENEMENT RECU *** HandleGameFinished appelé !");
            
            if (enableDebugLogs)
                Debug.Log("[LogParadeGameManager] Jeu terminé, préparation de la transition");

            ShowNextMiniGameTransition();
        }

        /// <summary>
        /// Affiche l'écran de transition vers le mini-jeu suivant
        /// </summary>
        private void ShowNextMiniGameTransition()
        {
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(true);
                roundEndScreenManager.OnNextMiniGameCallback = OnTransitionToNextMiniGame;
                
                if (enableDebugLogs)
                    Debug.Log("[LogParadeGameManager] Écran de transition affiché");
            }
            else
            {
                // Pas d'écran de transition, passer directement au mini-jeu suivant
                StartCoroutine(DelayedNextMiniGameTransition());
            }
        }

        /// <summary>
        /// Callback appelé pour démarrer la transition vers le mini-jeu suivant
        /// </summary>
        private void OnTransitionToNextMiniGame()
        {
            if (enableDebugLogs)
                Debug.Log("[LogParadeGameManager] Transition vers mini-jeu suivant déclenchée");

            // Désactiver l'écran de fin
            if (roundEndScreenManager != null)
            {
                roundEndScreenManager.gameObject.SetActive(false);
                roundEndScreenManager.OnNextMiniGameCallback = null;
            }

            // Déclencher la transition via GameSessionManager
            if (enableGameSessionTransition && gameSessionManager != null)
            {
                // Utiliser la nouvelle méthode pour déclencher la transition avec délai et écran de chargement
                gameSessionManager.TriggerNextMiniGameTransition(nextGameTransitionDelay);
            }
            else
            {
                // Fallback : GameSessionManager absent, on redirige vers la scène principale
                Debug.LogWarning("[LogParadeGameManager] GameSessionManager non disponible - Redirection automatique vers le menu principal");
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                GameSessionRedirector.ShouldResumeSession = true;
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
                Debug.Log($"[LogParadeGameManager] Délai de {nextGameTransitionDelay}s avant transition automatique");

            yield return new WaitForSeconds(nextGameTransitionDelay);
            OnTransitionToNextMiniGame();
        }

        /// <summary>
        /// Méthode publique pour déclencher la fin du jeu depuis d'autres systèmes
        /// </summary>
        public void TriggerGameFinished()
        {
            HandleGameFinished();
        }

        #region Debug Methods

        [ContextMenu("Force Next MiniGame Transition")]
        private void ForceNextMiniGameTransition()
        {
            OnTransitionToNextMiniGame();
        }

        #endregion
        
        void OnDestroy()
        {
            // Nettoyer les événements
            if (gameController != null)
            {
                gameController.OnGameCompleted -= HandleGameFinished;
            }
        }
    }
}