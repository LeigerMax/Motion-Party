using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.UI;

namespace Gameplay.LogParade.Player
{
    /// <summary>
    /// Responsable du tracking latéral du joueur et du mapping sur les 4 voies
    /// Récupère les données MediaPipe pour détecter la position X du torse/tête
    /// Inspiré de HandTracking mais adapté pour le mouvement latéral
    /// </summary>
    public class LogParadeLateralTracker : MonoBehaviour
    {
        #region Fields
        [Header("UDP Settings")]
        public UDPReceive udpReceive;

        [Header("Tracking Settings")]
        [Range(0.1f, 1.0f)]
        public float smoothingFactor = 0.8f;
        [Range(-2.0f, 2.0f)]
        public float leftBoundary = -1.5f;
        [Range(-2.0f, 2.0f)]
        public float rightBoundary = 1.5f;

        [Header("Calibration")]
        public bool enableAutoCalibration = false; // Désactivé par défaut pour la calibration interactive
        public float calibrationTime = 1.0f; 
        public bool continuousCalibration = true; 

        // Events - similaire à NoteSequenceManager
        public event Action<int> OnLaneChanged;
        public event Action<Vector3> OnPositionUpdated;

        // Private fields
        private Vector3 currentPosition;
        private Vector3 smoothedPosition;
        private int currentLane = 2; // Démarre au centre (lane 1-4)
        private bool isCalibrated = false;
        private float calibrationTimer = 0f;
        private Vector3 calibrationCenter;
        private int calibrationSamples = 0;

        // Offset similaire à HandTracking
        private int offset = 70;
        #endregion

        #region Unity Lifecycle
        void Start()
        {
            if (udpReceive == null)
            {
                LogParadeLogger.LogError("UDPReceive n'est pas assigné dans LogParadeLateralTracker !");
                return;
            }

            // Initialiser la position au centre
            smoothedPosition = Vector3.zero;
            currentPosition = Vector3.zero;

            if (enableAutoCalibration)
            {
                StartCalibration();
            }
            else
            {
                // Si la calibration automatique est désactivée, on considère que le système est calibré
                isCalibrated = true;
            }
        }

        void Update()
        {
        
            if (!ProcessUDPData()) return;

            // Toujours mettre à jour la position, même pendant la calibration
            UpdatePosition();
            UpdateLane();

            // Gérer la calibration en parallèle
            if (enableAutoCalibration && !isCalibrated)
            {
                UpdateCalibration();
            }
        }
        #endregion

        #region UDP Processing
        /// <summary>
        /// Traite les données UDP reçues de MediaPipe
        /// Utilise la position de la tête (landmark 0) pour le tracking latéral
        /// </summary>
        private bool ProcessUDPData()
        {
            string data = udpReceive.data;
            if (string.IsNullOrEmpty(data)) return false;
            try
            {

                // Nettoyer le JSON au cas où il y aurait des caractères invisibles
                data = data.Trim();

                // Parsing du JSON des données MediaPipe
                JObject jsonData = JObject.Parse(data);

                // Priorité 1: Utiliser les landmarks de pose (tête = landmark 0)
                JArray poseLandmarks = (JArray)jsonData["pose_landmarks"];
                if (poseLandmarks != null && poseLandmarks.Count > 0)
                {
                    // Utilise la tête/nez (landmark 0) pour le tracking latéral
                    float x = 7 - (float)poseLandmarks[0][0] / this.offset; // Position X inversée comme HandTracking
                    float y = (float)poseLandmarks[0][1] / this.offset;     // Position Y
                    float z = (float)poseLandmarks[0][2] / this.offset;     // Profondeur

                    currentPosition = new Vector3(x, y, z);
                    return true;
                }

                // Fallback: Utiliser les données de main si la pose n'est pas disponible
                JArray handPositions = (JArray)jsonData["hand_positions"];
                if (handPositions != null && handPositions.Count > 0)
                {
                    // Utilise le poignet comme référence pour la position latérale
                    float x = 7 - (float)handPositions[0][0] / this.offset;
                    float y = (float)handPositions[0][1] / this.offset;
                    float z = (float)handPositions[0][2] / this.offset;

                    currentPosition = new Vector3(x, y, z);

                    return true;
                }

            }
            catch (Exception e)
            {
                LogParadeLogger.LogWarning($"Erreur lors du parsing des données UDP : {e.Message}");
            }

            return false;
        }
        #endregion

        #region Calibration
        /// <summary>
        /// Démarre la calibration automatique
        /// </summary>
        private void StartCalibration()
        {
            isCalibrated = false;
            calibrationTimer = 0f;
            calibrationCenter = Vector3.zero;
            calibrationSamples = 0;
        }

        /// <summary>
        /// Met à jour la calibration
        /// </summary>
        private void UpdateCalibration()
        {
            calibrationTimer += Time.deltaTime;

            // Accumuler les échantillons pour calculer la position centrale
            calibrationCenter += currentPosition;
            calibrationSamples++;

            // Notifier l'UI du progrès
            float progress = calibrationTimer / calibrationTime;
            var uiManager = FindFirstObjectByType<LogParadeUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowCalibrationUI(progress);
            }

            if (calibrationTimer >= calibrationTime)
            {
                // Finaliser la calibration
                if (calibrationSamples > 0)
                {
                    calibrationCenter /= calibrationSamples;
                    isCalibrated = true;

                    if (uiManager != null)
                    {
                        uiManager.HideCalibrationUI();
                    }

                }
            }
        }
        #endregion

        #region Position & Lane
        /// <summary>
        /// Met à jour la position lissée
        /// </summary>
        private void UpdatePosition()
        {
            // Calibration continue : ajuster automatiquement le centre
            if (continuousCalibration && !enableAutoCalibration)
            {
                // Ajustement lent du centre pour compenser les dérives
                calibrationCenter = Vector3.Lerp(calibrationCenter, currentPosition, 0.001f);
            }

            // Appliquer le centre de calibration
            Vector3 adjustedPosition = currentPosition - calibrationCenter;

            // Lisser la position pour éviter les tremblements
            smoothedPosition = Vector3.Lerp(smoothedPosition, adjustedPosition, smoothingFactor);

            OnPositionUpdated?.Invoke(smoothedPosition);
        }

        /// <summary>
        /// Calcule et met à jour la voie actuelle basée sur la position X
        /// Similaire à la logique de NoteSequenceManager mais pour les voies
        /// </summary>
        private void UpdateLane()
        {
            float normalizedX = smoothedPosition.x;

            // Mapper la position X sur les 4 voies
            int newLane;

            if (normalizedX < leftBoundary * 0.5f)
                newLane = 1; // Voie la plus à gauche
            else if (normalizedX < 0f)
                newLane = 2; // Voie centre-gauche
            else if (normalizedX < rightBoundary * 0.5f)
                newLane = 3; // Voie centre-droite
            else
                newLane = 4; // Voie la plus à droite

            // Déclencher l'event si la voie a changé
            if (newLane != currentLane)
            {
                currentLane = newLane;
                OnLaneChanged?.Invoke(currentLane);
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Force la recalibration
        /// </summary>
        public void Recalibrate()
        {
            if (enableAutoCalibration)
            {
                StartCalibration();
            }
        }

        /// <summary>
        /// Obtient la voie actuelle (1-4)
        /// </summary>
        public int GetCurrentLane()
        {
            return currentLane;
        }

        /// <summary>
        /// Obtient la position lissée actuelle
        /// </summary>
        public Vector3 GetSmoothedPosition()
        {
            return smoothedPosition;
        }

        /// <summary>
        /// Vérifie si le tracker est calibré
        /// </summary>
        public bool IsCalibrated()
        {
            return isCalibrated;
        }
        #endregion
    }
}