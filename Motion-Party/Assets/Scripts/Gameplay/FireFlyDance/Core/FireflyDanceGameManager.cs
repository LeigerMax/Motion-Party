using UnityEngine;
using System.Collections;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Hand;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Scoring;
using Gameplay.FireFlyDance.Capture;
using Gameplay.FireFlyDance.Utils;
using Gameplay.FireFlyDance.UI;
using Core;

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

        [Header("Debug Settings")]
        public bool enableDebugMode = true;
        public bool enableDetailedLogs = true;

        // État interne
        private bool isInitialized = false;
        private bool isGameActive = false;
        private bool isLaunchedViaMiniGameBase = false; // Nouveau flag pour contrôler l'initialisation

        #endregion

        #region Unity Lifecycle

        protected override void Launch()
        {
            FireflyDanceLogger.Log("🚀 Launch() appelé - Début du lancement du mini-jeu");
            isLaunchedViaMiniGameBase = true; // Marquer que le lancement se fait via MiniGameBase
            InitializeGameManager();
        }

        void Start()
        {
            // Ne rien faire ici - le lancement se fait uniquement via Launch()
            // Seule la validation des GameManagers multiples est conservée
            var allGameManagers = FindObjectsByType<FireflyDanceGameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allGameManagers.Length > 1)
            {
                FireflyDanceLogger.LogWarning($"MULTIPLE GameManagers détectés ({allGameManagers.Length}) ! Cela peut causer des conflits.");
                for (int i = 0; i < allGameManagers.Length; i++)
                {
                    FireflyDanceLogger.LogWarning($"  GameManager {i+1}: {allGameManagers[i].gameObject.name}");
                }
            }
            
            // Vérifier si on doit s'initialiser automatiquement (pour les tests directs dans la scène)
            // Attendre quelques frames pour laisser le temps au GameSessionManager de nous appeler
            StartCoroutine(CheckForAutoInitialization());
        }


        void OnDestroy()
        {
            CleanupEventListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise le gestionnaire de jeu
        /// </summary>
        private void InitializeGameManager()
        {
            if (isInitialized)
            {
                FireflyDanceLogger.LogWarning("GameManager déjà initialisé");
                return;
            }

            FireflyDanceLogger.Log($"Initialisation du GameManager - Via MiniGameBase: {isLaunchedViaMiniGameBase}");

            // 0. Validation de la configuration
            ValidateConfiguration();

            // 1. Trouver les composants requis
            FindRequiredComponents();

            // 2. Validation des composants
            if (!ValidateComponents())
            {
                FireflyDanceLogger.LogError("Échec d'initialisation - composants manquants");
                return;
            }

            // 3. Configurer les event listeners AVANT de démarrer le jeu
            SetupEventListeners();

            // 4. Initialisation des modules
            InitializeModules();

            isInitialized = true;
            
            if (enableDetailedLogs)
            {
                FireflyDanceLogger.Log("GameManager initialisé avec succès");
            }

            // 5. Démarrer immédiatement le jeu (maintenant que les listeners sont configurés)
            StartGameImmediately();
        }

        /// <summary>
        /// Démarre le jeu immédiatement en passant par les états nécessaires
        /// </summary>
        private void StartGameImmediately()
        {
            if (gameController == null)
            {
                FireflyDanceLogger.LogError("Impossible de démarrer - GameController manquant");
                return;
            }

            // Passer par les états requis : Idle -> Calibrating -> Ready -> Playing
            gameController.StartCalibration();   // Idle -> Calibrating
            gameController.FinishCalibration();  // Calibrating -> Ready  
            gameController.StartGame();          // Ready -> Playing (déclenche OnGameStarted)
            
            FireflyDanceLogger.Log("Jeu démarré automatiquement");
        }

        /// <summary>
        /// Trouve automatiquement les composants requis
        /// </summary>
        private void FindRequiredComponents()
        {
            
            if (gameController == null)
            {
                gameController = FindFirstObjectByType<FireflyDanceGameController>();
            }

            if (spawner == null)
            {
                spawner = FindFirstObjectByType<FireflySpawner>();
            }

            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<FireflyScoreManager>();
            }

            if (handTracker == null)
            {
                handTracker = FindFirstObjectByType<HandTracker>();
            }

            if (fireflyCapture == null)
            {
                fireflyCapture = FindFirstObjectByType<Gameplay.FireFlyDance.Capture.FireflyCapture>();
            }

            if (timer == null)
            {
                timer = FindFirstObjectByType<FireflyDanceTimer>();
            }

            if (udpReceive == null)
            {
                udpReceive = FindFirstObjectByType<UDPReceive>();
            }

            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<FireflyDanceUIManager>();

            }
            

        }

        /// <summary>
        /// Configure les listeners d'événements
        /// </summary>
        private void SetupEventListeners()
        {
            // Événements du contrôleur de jeu
            FireflyDanceEvents.OnGameStarted += OnGameStarted;
            FireflyDanceEvents.OnGameEnded += OnGameEnded;
            FireflyDanceEvents.OnGamePaused += OnGamePaused;

            // Événements des lucioles
            FireflyDanceEvents.OnFireflyCaptured += OnFireflyCaptured;
            FireflyDanceEvents.OnFireflySpawned += OnFireflySpawned;
            FireflyDanceEvents.OnFireflyExpired += OnFireflyExpired;

            // Événements du timer
            FireflyDanceEvents.OnTimerCompleted += OnTimerCompleted;

        }

        /// <summary>
        /// Nettoie les listeners d'événements
        /// </summary>
        private void CleanupEventListeners()
        {
            FireflyDanceEvents.OnGameStarted -= OnGameStarted;
            FireflyDanceEvents.OnGameEnded -= OnGameEnded;
            FireflyDanceEvents.OnGamePaused -= OnGamePaused;
            FireflyDanceEvents.OnFireflyCaptured -= OnFireflyCaptured;
            FireflyDanceEvents.OnFireflySpawned -= OnFireflySpawned;
            FireflyDanceEvents.OnFireflyExpired -= OnFireflyExpired;
            FireflyDanceEvents.OnTimerCompleted -= OnTimerCompleted;
        }

        /// <summary>
        /// Initialise tous les modules du jeu
        /// </summary>
        private void InitializeModules()
        {
            // Initialisation du scoring
            if (scoreManager != null)
            {
                scoreManager.Initialize(config);
                if (enableDetailedLogs)
                    FireflyDanceLogger.Log("Score manager initialisé");
            }

            // Initialisation du spawner
            if (spawner != null && config != null)
            {
                spawner.Initialize(config);
                if (enableDetailedLogs)
                    FireflyDanceLogger.Log("Spawner initialisé");
            }

            // Initialisation du tracking de main
            if (handTracker != null)
            {
                handTracker.StartTracking();
                if (enableDetailedLogs)
                    FireflyDanceLogger.Log("Hand tracker démarré");
            }

            // Initialisation du système de capture (mais désactivé au début)
            if (fireflyCapture != null)
            {
                fireflyCapture.SetCaptureEnabled(false); // Désactivé jusqu'au début du jeu
                if (enableDetailedLogs)
                    FireflyDanceLogger.Log("Système de capture initialisé (désactivé)");
            }

            // Initialisation de l'UI Manager
            if (uiManager != null)
            {
                // L'UI Manager s'initialise automatiquement dans son Start()
                if (enableDetailedLogs)
                    FireflyDanceLogger.Log("UI Manager détecté et prêt");
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


        }

        /// <summary>
        /// Gère la fin du jeu - Désactive TOUS les systèmes pour empêcher toute interaction
        /// </summary>
        private void OnGameEnded()
        {
            isGameActive = false;


            //  1. Arrêter le spawn des lucioles et nettoyer celles existantes
            if (spawner != null)
            {
                spawner.StopSpawning();
                spawner.ClearAllFireflies(); // Nettoyer toutes les lucioles restantes
                FireflyDanceLogger.Log(" Spawn des lucioles arrêté et lucioles nettoyées");
            }

            //  2. Désactiver l'affichage et la logique de la main
            if (handTracker != null)
            {
                handTracker.StopTracking();
                FireflyDanceLogger.Log("Suivi de main désactivé");
            }

            //  3. Désactiver la capture des lucioles
            if (fireflyCapture != null)
            {
                fireflyCapture.SetCaptureEnabled(false);
                FireflyDanceLogger.Log("Système de capture désactivé");
            }

            //  4. Notifier le score manager (qui va cesser d'ajouter des points)
            if (scoreManager != null)
            {
                scoreManager.OnGameEnded();
                FireflyDanceLogger.Log("Score manager notifié de la fin de partie");
            }

            //  5. Arrêter le timer
            if (timer != null)
            {
                timer.StopTimer();
                FireflyDanceLogger.Log("Timer arrêté");
            }

            
            // Terminer le mini-jeu
            FinishMiniGame();
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
            FireflyDanceLogger.Log($"Luciole capturée - Points: {points}");
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
            
            isGameActive = false;
            
            // Arrêter tous les systèmes sans passer par les événements
            if (spawner != null)
            {
                spawner.StopSpawning();
                spawner.ClearAllFireflies();
            }
            
            if (handTracker != null)
            {
                handTracker.StopTracking();
            }
            
            if (fireflyCapture != null)
            {
                fireflyCapture.SetCaptureEnabled(false);
            }
            
            if (timer != null)
            {
                timer.StopTimer();
            }
            
            if (scoreManager != null)
            {
                scoreManager.OnGameEnded();
            }
            
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

        #endregion

        #region Test Coroutines

        /// <summary>
        /// Vérifie si on doit s'auto-initialiser (pour les tests directs dans la scène)
        /// </summary>
        private System.Collections.IEnumerator CheckForAutoInitialization()
        {
            // Attendre 0.5 secondes pour laisser le temps au GameSessionManager de nous appeler
            yield return new WaitForSeconds(0.5f);
            
            // Si Launch() n'a pas été appelé via MiniGameBase, on s'initialise automatiquement
            if (!isLaunchedViaMiniGameBase && !isInitialized)
            {
                FireflyDanceLogger.LogWarning("⚠️ Auto-initialisation détectée - Probablement un test direct dans la scène");
                FireflyDanceLogger.LogWarning("⚠️ Pour un comportement normal, utilisez le GameSessionManager");
                InitializeGameManager();
            }
        }

        #endregion
    }
}