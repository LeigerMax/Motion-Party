using UnityEngine;
using Gameplay.LogParade.Player;
using Gameplay.LogParade.Systems;

namespace Gameplay.LogParade.Utils
{
    /// <summary>
    /// Configuration globale pour le mini-jeu LogParade
    /// ScriptableObject pour stocker les paramètres réutilisables
    /// </summary>
    [CreateAssetMenu(fileName = "LogParadeConfig", menuName = "LogParade/Game Configuration")]
    public class LogParadeConfig : ScriptableObject
    {
        #region Fields
        [Header("Tracking Settings")]
        [Range(0.1f, 1.0f)]
        public float defaultSmoothingFactor = 0.8f;
        
        [Range(-5.0f, 5.0f)]
        public float defaultLeftBoundary = -1.5f;
        
        [Range(-5.0f, 5.0f)]
        public float defaultRightBoundary = 1.5f;
        
        [Header("Calibration")]
        public bool enableAutoCalibration = true;
        public float calibrationTime = 3.0f;
        
        [Header("Avatar Movement")]
        public float avatarMoveSpeed = 5f;
        public float laneWidth = 2f;
        public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        

        public AudioClip backgroundMusic;
        
        [Header("Debug")]
        public bool enableInputSimulator = false;
        #endregion

        #region Public Methods
        /// <summary>
        /// Applique cette configuration aux composants LogParade
        /// </summary>
        public void ApplyToComponents()
        {
            // Trouver et configurer le tracker
            LogParadeLateralTracker tracker = FindFirstObjectByType<LogParadeLateralTracker>();
            if (tracker != null)
            {
                tracker.smoothingFactor = defaultSmoothingFactor;
                tracker.leftBoundary = defaultLeftBoundary;
                tracker.rightBoundary = defaultRightBoundary;
                tracker.enableAutoCalibration = enableAutoCalibration;
                tracker.calibrationTime = calibrationTime;
            }

            // Trouver et configurer l'avatar
            LogParadePlayerAvatar avatar = FindFirstObjectByType<LogParadePlayerAvatar>();
            if (avatar != null)
            {
                avatar.moveSpeed = avatarMoveSpeed;
                avatar.laneWidth = laneWidth;
                avatar.movementCurve = movementCurve;
            }

            // Trouver et configurer le visualisateur
            LogParadeLaneVisualizer visualizer = FindFirstObjectByType<LogParadeLaneVisualizer>();
            if (visualizer != null)
            {
                visualizer.laneWidth = laneWidth;
            } 

            // Configurer le simulateur si nécessaire
            LogParadeInputSimulator simulator = FindFirstObjectByType<LogParadeInputSimulator>();
            if (simulator != null)
            {
                simulator.enableSimulation = enableInputSimulator;
            }
            

        }
        
        /// <summary>
        /// Réinitialise les valeurs par défaut
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            defaultSmoothingFactor = 0.8f;
            defaultLeftBoundary = -1.5f;
            defaultRightBoundary = 1.5f;
            enableAutoCalibration = true;
            calibrationTime = 3.0f;
            avatarMoveSpeed = 5f;
            laneWidth = 2f;
            movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            

            enableInputSimulator = false;
        }
        #endregion

        #region Unity Callbacks
        void OnValidate()
        {
            // S'assurer que les boundaries sont logiques
            if (defaultLeftBoundary >= defaultRightBoundary)
            {
                defaultRightBoundary = defaultLeftBoundary + 1f;
            }
            
        }
        #endregion
    }
}