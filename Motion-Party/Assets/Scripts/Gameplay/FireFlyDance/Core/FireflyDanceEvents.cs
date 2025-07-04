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
    /// Déclenché quand la main est détectée ou perdue
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
    public static Action<FireflyController> OnFireflyCaptured;

    /// <summary>
    /// Déclenché quand une luciole disparaît (timeout)
    /// </summary>
    public static Action<FireflyController> OnFireflyExpired;

    /// <summary>
    /// Déclenché quand une luciole commence à clignoter (avant expiration)
    /// </summary>
    public static Action<FireflyController> OnFireflyWarning;

    /// <summary>
    /// Déclenché quand une luciole est détruite
    /// </summary>
    public static Action<FireflyController> OnFireflyDestroyed;

    /// <summary>
    /// Déclenché quand l'état d'une luciole change
    /// </summary>
    public static Action<FireflyController, FireflyController.FireflyState, FireflyController.FireflyState> OnFireflyStateChanged;

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

        // Hand Tracking Events
        OnHandPositionChanged = null;
        OnHandStateChanged = null;
        OnHandDetectionChanged = null;

        // Firefly Events
        OnFireflySpawned = null;
        OnFireflyCaptured = null;
        OnFireflyExpired = null;
        OnFireflyWarning = null;
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
