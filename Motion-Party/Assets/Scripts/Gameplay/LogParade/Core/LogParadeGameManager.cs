using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Analytics;
using Core.Analytics.Core;
using Core.Audio;
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
        
        // Analytics session
        private string currentAnalyticsSessionId;

        protected override void Launch()
        {
            if (enableDebugLogs)
                Debug.Log("[LogParadeGameManager] Launch() appelé");

            ValidateComponents();
            StartAnalyticsSession();
            // La musique a déjà été démarrée dans Start()
            InitializeGame();
        }

        void Start()
        {
            ValidateComponents();
            FindGameSessionManager();
            
            // Vérifier si on doit attendre l'écran de chargement avant de lancer
            StartCoroutine(CheckForAutoInitialization());
        }

        /// <summary>
        /// Vérifie si on doit s'auto-initialiser en respectant l'écran de chargement
        /// </summary>
        private System.Collections.IEnumerator CheckForAutoInitialization()
        {
            // Si un LoadingScreenManager est présent et actif, attendre qu'il se termine
            if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsShowing)
            {
                if (enableDebugLogs)
                    Debug.Log("[LogParadeGameManager] LoadingScreenManager détecté et actif - Attente de la fin de l'écran de chargement...");
                
                // Attendre que l'écran de chargement se termine
                yield return new WaitUntil(() => LoadingScreenManager.Instance == null || !LoadingScreenManager.Instance.IsShowing);
                
                // Attendre encore un peu pour être sûr que tout est initialisé
                yield return new WaitForSeconds(1f);
                
                if (enableDebugLogs)
                    Debug.Log("[LogParadeGameManager] Écran de chargement terminé - Lancement du jeu");
            }
            else
            {
                // Comportement original si pas d'écran de chargement
                yield return new WaitForSeconds(0.5f);
            }

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
            
            // Terminer la session d'analytics
            EndAnalyticsSession();
            
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

        #region Audio Management

        /// <summary>
        /// Démarre la musique spécifique au mini-jeu LogParade
        #endregion

        #region Analytics Integration

        /// <summary>
        /// Démarre une session d'analytics pour Log Parade
        /// </summary>
        private void StartAnalyticsSession()
        {
            try
            {
                // Ne pas créer de nouvelle session, utiliser celle existante du GameSessionManager
                if (enableDebugLogs)
                    Debug.Log($"[LogParadeGameManager] 📊 Utilisation de la session analytics existante du GameSessionManager");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LogParadeGameManager] ❌ Erreur analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Termine la session analytics et marque le mini-jeu comme complété
        /// </summary>
        private void EndAnalyticsSession()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentAnalyticsSessionId))
                {
                    // Terminer la session
                    var analyticsResult = AnalyticsHelper.EndGameSession(currentAnalyticsSessionId);
                    
                    if (analyticsResult != null && enableDebugLogs)
                    {
                        Debug.Log($"[LogParadeGameManager] 📈 Analytics terminées - Score: {analyticsResult.finalScore}, Actions: {analyticsResult.totalActions}");
                    }
                    
                    currentAnalyticsSessionId = null;
                }

                // Marquer le mini-jeu comme complété dans la session globale
                AnalyticsHelper.CompleteMiniGame("LogParade");
                
                if (enableDebugLogs)
                    Debug.Log("[LogParadeGameManager] ✅ Log Parade marqué comme complété dans la session globale");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LogParadeGameManager] ❌ Erreur fin analytics: {ex.Message}");
            }
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