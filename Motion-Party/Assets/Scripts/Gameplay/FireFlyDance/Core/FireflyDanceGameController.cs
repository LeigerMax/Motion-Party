using UnityEngine;
using System;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Contrôleur d'état principal du mini-jeu Danse des Lucioles
    /// Gère les états du jeu et les transitions entre eux
    /// Utilise le système d'événements FireflyDanceEvents pour la communication
    /// </summary>
    public class FireflyDanceGameController : MonoBehaviour
    {
        #region Enums

        /// <summary>
        /// États possibles du jeu Danse des Lucioles
        /// </summary>
        public enum GameState
        {
            Idle,          // Jeu en attente
            Calibrating,   // Phase de calibration
            Ready,         // Prêt à démarrer
            Playing,       // Jeu en cours
            Paused,        // Jeu en pause
            Finished,      // Jeu terminé
            Error          // Erreur système
        }

        #endregion

        #region Fields & Properties

        [Header("Game Controller Settings")]
        [SerializeField] private bool enableDetailedLogs = true;

        /// <summary>
        /// État actuel du jeu
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Idle;

        /// <summary>
        /// Indique si le jeu est en cours d'exécution
        /// </summary>
        public bool IsPlaying => CurrentState == GameState.Playing;

        /// <summary>
        /// Indique si le jeu est terminé
        /// </summary>
        public bool IsFinished => CurrentState == GameState.Finished;

        /// <summary>
        /// Indique si le jeu est en pause
        /// </summary>
        public bool IsPaused => CurrentState == GameState.Paused;

        /// <summary>
        /// Indique si le jeu est en phase de calibration
        /// </summary>
        public bool IsCalibrating => CurrentState == GameState.Calibrating;

        /// <summary>
        /// Indique si le jeu est prêt à démarrer
        /// </summary>
        public bool IsReady => CurrentState == GameState.Ready;

        /// <summary>
        /// Indique si une erreur est survenue
        /// </summary>
        public bool HasError => CurrentState == GameState.Error;

        // Instance singleton pour accès global
        private static FireflyDanceGameController instance;
        public static FireflyDanceGameController Instance => instance;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            // Pattern singleton
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeController();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise le contrôleur d'état
        /// </summary>
        private void InitializeController()
        {
            ChangeState(GameState.Idle);
            
            if (enableDetailedLogs)
            {
                FireflyDanceLogger.Log("GameController initialisé - État: Idle");
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// Change l'état du jeu et notifie via les événements
        /// </summary>
        /// <param name="newState">Nouvel état</param>
        private void ChangeState(GameState newState)
        {
            GameState previousState = CurrentState;
            CurrentState = newState;

            if (enableDetailedLogs)
            {
                FireflyDanceLogger.Log($"Transition d'état: {previousState} -> {newState}");
            }

            // Notification via les événements
            NotifyStateChange(previousState, newState);
        }

        /// <summary>
        /// Notifie le changement d'état via les événements
        /// </summary>
        /// <param name="previousState">État précédent</param>
        /// <param name="newState">Nouvel état</param>
        private void NotifyStateChange(GameState previousState, GameState newState)
        {
            // Événements spécifiques
            switch (newState)
            {
                case GameState.Playing:
                    FireflyDanceLogger.Log("🎯 GameController: Émission OnGameStarted");
                    FireflyDanceEvents.OnGameStarted?.Invoke();
                    break;
                    
                case GameState.Finished:
                    FireflyDanceLogger.Log("🏁 GameController: Émission OnGameEnded");
                    FireflyDanceEvents.OnGameEnded?.Invoke();
                    break;
                    
                case GameState.Paused:
                    FireflyDanceLogger.Log("⏸️ GameController: Émission OnGamePaused(true)");
                    FireflyDanceEvents.OnGamePaused?.Invoke(true);
                    break;
                    
                case GameState.Error:
                    FireflyDanceLogger.Log("❌ GameController: Émission OnGameError");
                    FireflyDanceEvents.OnGameError?.Invoke($"Erreur système - État: {newState}");
                    break;
            }

            // Si on sort de pause
            if (previousState == GameState.Paused && newState != GameState.Paused)
            {
                FireflyDanceLogger.Log("▶️ GameController: Émission OnGamePaused(false)");
                FireflyDanceEvents.OnGamePaused?.Invoke(false);
            }

            // Événement général de changement d'état
            FireflyDanceEvents.OnGameStateChanged?.Invoke(newState);
        }

        #endregion

        #region Public Methods - State Transitions

        /// <summary>
        /// Démarre la phase de calibration
        /// </summary>
        public void StartCalibration()
        {
            if (CurrentState == GameState.Idle || CurrentState == GameState.Error)
            {
                ChangeState(GameState.Calibrating);
                FireflyDanceLogger.Log("Début de la calibration");
            }
            else
            {
                FireflyDanceLogger.LogWarning($"Impossible de démarrer la calibration depuis l'état {CurrentState}");
            }
        }

        /// <summary>
        /// Termine la calibration avec succès
        /// </summary>
        public void FinishCalibration()
        {
            if (CurrentState == GameState.Calibrating)
            {
                ChangeState(GameState.Ready);
                FireflyDanceLogger.Log("Calibration terminée - Prêt à jouer");
            }
            else
            {
                FireflyDanceLogger.LogWarning($"Impossible de finir la calibration depuis l'état {CurrentState}");
            }
        }

        /// <summary>
        /// Démarre le jeu
        /// </summary>
        public void StartGame()
        {
            if (CurrentState == GameState.Ready)
            {
                ChangeState(GameState.Playing);
                FireflyDanceLogger.Log("Jeu démarré");
            }
            else
            {
                FireflyDanceLogger.LogWarning($"Impossible de démarrer le jeu depuis l'état {CurrentState}");
            }
        }

        /// <summary>
        /// Met le jeu en pause
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                FireflyDanceLogger.Log("Jeu mis en pause");
            }
            else
            {
                FireflyDanceLogger.LogWarning($"Impossible de mettre en pause depuis l'état {CurrentState}");
            }
        }

        /// <summary>
        /// Reprend le jeu depuis la pause
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                FireflyDanceLogger.Log("Jeu repris");
            }
            else
            {
                FireflyDanceLogger.LogWarning($"Impossible de reprendre depuis l'état {CurrentState}");
            }
        }

        /// <summary>
        /// Termine le jeu
        /// </summary>
        public void EndGame()
        {
            if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Finished);
                FireflyDanceLogger.Log("Jeu terminé");
            }
            else
            {
                FireflyDanceLogger.LogWarning($"Impossible de terminer le jeu depuis l'état {CurrentState}");
            }
        }

        /// <summary>
        /// Remet le jeu à l'état initial
        /// </summary>
        public void ResetGame()
        {
            ChangeState(GameState.Idle);
            FireflyDanceLogger.Log("Jeu remis à zéro");
        }

        #endregion


    }
}
