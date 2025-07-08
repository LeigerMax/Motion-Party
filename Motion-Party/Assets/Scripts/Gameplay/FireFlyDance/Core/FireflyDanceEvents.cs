using System;
using UnityEngine;
using Gameplay.FireFlyDance.Fireflies;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Système d'événements centralisé pour le mini-jeu Danse des Lucioles
    /// Permet la communication découplée entre les différents systèmes
    /// </summary>
    public static class FireflyDanceEvents
    {
        #region Game State Events

        /// <summary>
        /// Déclenché quand le jeu démarre
        /// </summary>
        public static Action OnGameStarted;

        /// <summary>
        /// Déclenché quand le jeu se termine
        /// </summary>
        public static Action OnGameEnded;

        /// <summary>
        /// Déclenché quand le jeu est en pause
        /// </summary>
        public static Action<bool> OnGamePaused;

        /// <summary>
        /// Déclenché quand l'état du jeu change
        /// </summary>
        public static Action<FireflyDanceGameController.GameState> OnGameStateChanged;

        /// <summary>
        /// Déclenché quand une erreur survient
        /// </summary>
        public static Action<string> OnGameError;

        /// <summary>
        /// Déclenché quand le timer du jeu se termine
        /// </summary>
        public static Action OnTimerCompleted;

        /// <summary>
        /// Déclenché à chaque mise à jour du timer (temps restant)
        /// </summary>
        public static Action<float> OnTimerTick;

        #endregion

        #region Calibration Events

        /// <summary>
        /// Déclenché quand la calibration commence
        /// </summary>
        public static Action OnCalibrationStarted;

        /// <summary>
        /// Déclenché quand la calibration se termine avec succès
        /// </summary>
        public static Action OnCalibrationCompleted;

        /// <summary>
        /// Déclenché quand la calibration échoue
        /// </summary>
        public static Action<string> OnCalibrationFailed;

        #endregion

        #region Hand Tracking Events

        /// <summary>
        /// Déclenché quand la position de la main change
        /// </summary>
        public static Action<Vector2> OnHandPositionChanged;

        /// <summary>
        /// Déclenché quand l'état de la main change (ouverte/fermée)
        /// </summary>
        public static Action<bool> OnHandStateChanged;

        /// <summary>
        /// Déclenché quand la main est détectée
        /// </summary>
        public static Action<Vector2> OnHandDetected;

        /// <summary>
        /// Déclenché quand la main est perdue
        /// </summary>
        public static Action OnHandLost;

        /// <summary>
        /// Déclenché quand l'état de détection change
        /// </summary>
        public static Action<bool> OnHandDetectionChanged;

        #endregion

        #region Firefly Events

        /// <summary>
        /// Déclenché quand une luciole apparaît
        /// </summary>
        public static Action<FireflyController> OnFireflySpawned;

        /// <summary>
        /// Déclenché quand une luciole est capturée
        /// </summary>
        public static Action<FireflyController, int> OnFireflyCaptured;

        /// <summary>
        /// Déclenché quand une luciole disparaît (timeout)
        /// </summary>
        public static Action<FireflyController> OnFireflyExpired;

        /// <summary>
        /// Déclenché quand une luciole est détruite
        /// </summary>
        public static Action<FireflyController> OnFireflyDestroyed;

        /// <summary>
        /// Déclenché quand l'état d'une luciole change
        /// </summary>
        public static Action<FireflyController, object, object> OnFireflyStateChanged;

        #endregion

        #region Scoring Events

        /// <summary>
        /// Déclenché quand le score change
        /// </summary>
        public static Action<int> OnScoreChanged;

        /// <summary>
        /// Déclenché quand un score record est battu
        /// </summary>
        public static Action<int> OnNewHighScore;

        #endregion

        #region Spawn Events

        /// <summary>
        /// Déclenché quand le spawn démarre
        /// </summary>
        public static Action OnSpawnStarted;

        /// <summary>
        /// Déclenché quand le spawn s'arrête
        /// </summary>
        public static Action OnSpawnStopped;

        /// <summary>
        /// Déclenché quand le nombre maximum de lucioles est atteint
        /// </summary>
        public static Action OnMaxFirefliesReached;

        #endregion

        #region Utility Methods

        /// <summary>
        /// Nettoie tous les événements (utile pour les changements de scène)
        /// </summary>
        public static void ClearAllEvents()
        {
            // Game State Events
            OnGameStarted = null;
            OnGameEnded = null;
            OnGamePaused = null;
            OnGameStateChanged = null;
            OnGameError = null;
            OnTimerCompleted = null;
            OnTimerTick = null;

            // Calibration Events
            OnCalibrationStarted = null;
            OnCalibrationCompleted = null;
            OnCalibrationFailed = null;

            // Hand Tracking Events
            OnHandPositionChanged = null;
            OnHandStateChanged = null;
            OnHandDetected = null;
            OnHandLost = null;
            OnHandDetectionChanged = null;

            // Firefly Events
            OnFireflySpawned = null;
            OnFireflyCaptured = null;
            OnFireflyExpired = null;
            OnFireflyDestroyed = null;
            OnFireflyStateChanged = null;

            // Scoring Events
            OnScoreChanged = null;
            OnNewHighScore = null;

            // Spawn Events
            OnSpawnStarted = null;
            OnSpawnStopped = null;
            OnMaxFirefliesReached = null;
        }

        #endregion
    }
}
