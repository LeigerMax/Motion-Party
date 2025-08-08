using System;
using UnityEngine;

namespace Gameplay.FireFlyDance.Analytics
{
    /// <summary>
    /// Énumération des types d'événements de jeu trackés
    /// </summary>
    public enum GameEventType
    {
        FireflySpawned,
        FireflyCaptured,
        FireflyExpired,
        HandClosed,
        HandOpened,
        GameStarted,
        GameEnded,
        HandMovement,
        AttemptedCapture
    }

    /// <summary>
    /// Structure représentant un événement de jeu avec ses données temporelles
    /// </summary>
    [Serializable]
    public struct GameStatEvent
    {
        [Header("Event Core Data")]
        public GameEventType eventType;
        public float timestamp;
        public Vector2 position;

        [Header("Firefly Specific")]
        public int fireflyId;
        public float fireflyLifetime;
        public bool wasSuccessful;
        public string fireflyType; // Type de libellule (Static, SlowMoving, FastMoving)
        public int scoreValue;     // Score attribué pour cette libellule

        [Header("Hand Specific")]
        public bool isHandClosed;
        public int openFingers;
        public Vector2 handPosition;

        [Header("Performance Metrics")]
        public float reactionTime;
        public int attemptNumber;
        public float distanceToTarget;

        public GameStatEvent(GameEventType type, float time, Vector2 pos)
        {
            eventType = type;
            timestamp = time;
            position = pos;
            
            // Valeurs par défaut
            fireflyId = -1;
            fireflyLifetime = 0f;
            wasSuccessful = false;
            fireflyType = "";
            scoreValue = 0;
            isHandClosed = false;
            openFingers = 0;
            handPosition = Vector2.zero;
            reactionTime = 0f;
            attemptNumber = 0;
            distanceToTarget = 0f;
        }

        /// <summary>
        /// Crée un événement de luciole avec données spécifiques
        /// </summary>
        public static GameStatEvent CreateFireflyEvent(GameEventType type, float timestamp, Vector2 position, 
            int fireflyId, float lifetime = 0f, bool successful = false, string fireflyType = "", int scoreValue = 0)
        {
            var evt = new GameStatEvent(type, timestamp, position);
            evt.fireflyId = fireflyId;
            evt.fireflyLifetime = lifetime;
            evt.wasSuccessful = successful;
            evt.fireflyType = fireflyType;
            evt.scoreValue = scoreValue;
            return evt;
        }

        /// <summary>
        /// Crée un événement de main avec données spécifiques
        /// </summary>
        public static GameStatEvent CreateHandEvent(GameEventType type, float timestamp, Vector2 handPos, 
            bool closed, int fingers)
        {
            var evt = new GameStatEvent(type, timestamp, handPos);
            evt.handPosition = handPos;
            evt.isHandClosed = closed;
            evt.openFingers = fingers;
            return evt;
        }

        /// <summary>
        /// Crée un événement de tentative de capture
        /// </summary>
        public static GameStatEvent CreateCaptureAttempt(float timestamp, Vector2 handPos, Vector2 targetPos, 
            int attemptNumber, bool successful)
        {
            var evt = new GameStatEvent(GameEventType.AttemptedCapture, timestamp, targetPos);
            evt.handPosition = handPos;
            evt.attemptNumber = attemptNumber;
            evt.wasSuccessful = successful;
            evt.distanceToTarget = Vector2.Distance(handPos, targetPos);
            return evt;
        }
    }
}
