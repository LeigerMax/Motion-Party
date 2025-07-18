using UnityEngine;
using System;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Player
{
    /// <summary>
    /// Représentation visuelle du joueur dans LogParade
    /// </summary>
    public class LogParadePlayerAvatar : MonoBehaviour
    {
        [Header("Position Settings")]
        [SerializeField] private Vector3 startPosition = new Vector3(0, 1, 0);
        public float laneWidth = 2f;
        [SerializeField] private int defaultLane = 2;

        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool enableSmoothMovement = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private int currentLane;
        private bool isMoving = false;
        private float movementProgress = 0f;
        private Vector3 startMovePosition;
        private Vector3 targetPosition;

        private void Start()
        {
            ResetPosition();
        }

        private void Update()
        {
            if (isMoving)
            {
                UpdateMovement();
            }
        }

        public void ResetPosition()
        {
            currentLane = defaultLane;
            transform.position = startPosition + Vector3.right * (currentLane - 2) * laneWidth;
            isMoving = false;
            movementProgress = 0f;

            if (enableDebugLogs)
            {
                LogParadeLogger.LogVerbose($"Position réinitialisée - Lane: {currentLane}");
            }
        }

        public void MoveTo(int lane)
        {
            if (lane < 1 || lane > 4)
            {
                LogParadeLogger.LogWarning($"Lane invalide: {lane}");
                return;
            }

            currentLane = lane;
            if (enableSmoothMovement)
            {
                StartMovement(lane);
            }
            else
            {
                SetLaneInstant(lane);
            }

            if (enableDebugLogs)
            {
                LogParadeLogger.LogVerbose($"Déplacement vers lane {lane}");
            }
        }

        public void SetLaneInstant(int lane)
        {
            currentLane = lane;
            Vector3 newPosition = startPosition + Vector3.right * (lane - 2) * laneWidth;
            transform.position = newPosition;
            isMoving = false;
            movementProgress = 0f;
        }

        private void StartMovement(int targetLane)
        {
            startMovePosition = transform.position;
            targetPosition = startPosition + Vector3.right * (targetLane - 2) * laneWidth;
            isMoving = true;
            movementProgress = 0f;
        }

        private void UpdateMovement()
        {
            movementProgress += Time.deltaTime * moveSpeed;
            
            if (movementProgress >= 1f)
            {
                movementProgress = 1f;
                isMoving = false;
                transform.position = targetPosition;
            }
            else
            {
                float curveValue = movementCurve.Evaluate(movementProgress);
                transform.position = Vector3.Lerp(startMovePosition, targetPosition, curveValue);
            }
        }

        public int GetCurrentLane()
        {
            return currentLane;
        }

        public bool IsMoving()
        {
            return isMoving;
        }
    }
}