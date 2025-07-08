using UnityEngine;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Hand;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Capture
{
    /// <summary>
    /// Script dédié uniquement à la capture des lucioles
    /// Système simple et fiable : main fermée = capture + destruction + score
    /// </summary>
    public class FireflyCapture : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        [SerializeField] private HandTracker handTracker;
        [SerializeField] private Gameplay.FireFlyDance.Scoring.FireflyScoreManager scoreManager; 

        [Header("Capture Settings")]
        [SerializeField] private float captureRadius = 1.0f;
        [SerializeField] private LayerMask fireflyLayer = -1;
        [SerializeField] private int scorePerCapture = 10;


        // État interne
        private bool isInitialized = false;
        private bool wasHandClosed = false;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            if (!isInitialized) return;

            UpdateCapture();
        }


        #endregion

        #region Initialization

        private void Initialize()
        {
            // Auto-find components si pas assignés
            if (config == null)
                config = FindFirstObjectByType<FireflyDanceConfig>();

            if (handTracker == null)
                handTracker = FindFirstObjectByType<HandTracker>();

            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<Gameplay.FireFlyDance.Scoring.FireflyScoreManager>();

            // Validation
            if (config == null)
            {
                FireflyDanceLogger.LogError("Configuration manquante !", this);
                return;
            }

            if (handTracker == null)
            {
                FireflyDanceLogger.LogError("HandTracker manquant !", this);
                return;
            }

            if (scoreManager == null)
            {
                FireflyDanceLogger.LogWarning("ScoreManager manquant ! Le score ne sera pas mis à jour.", this);
            }

            // Utiliser les valeurs de la config
            captureRadius = config.CaptureRadius;
            scorePerCapture = config.ScorePerFirefly;

            isInitialized = true;
            FireflyDanceLogger.Log("FireflyCapture initialisé avec succès", this);
        }

        #endregion

        #region Capture Logic

        private void UpdateCapture()
        {
            // Vérifier si la main est détectée
            if (!handTracker.IsHandDetected)
            {
                wasHandClosed = false;
                return;
            }

            bool isHandClosed = handTracker.IsHandClosed;


            // Détection du moment où la main se ferme (transition ouverte -> fermée)
            if (isHandClosed && !wasHandClosed)
            {
                TryCapture();
            }

            wasHandClosed = isHandClosed;
        }

        private void TryCapture()
        {
            // Obtenir la position 3D de la main
            Vector3 handPos3D = new Vector3(handTracker.CurrentPosition.x, handTracker.CurrentPosition.y, 0);

            // Méthode plus robuste : chercher toutes les lucioles actives et vérifier la distance
            FireflyController[] allFireflies = FindObjectsByType<FireflyController>(FindObjectsSortMode.None);
            int capturedCount = 0;

            foreach (var firefly in allFireflies)
            {
                if (firefly == null || !firefly.IsActive) continue;

                // Vérifier la distance entre la main et la luciole
                float distance = Vector3.Distance(handPos3D, firefly.transform.position);

                if (distance <= captureRadius)
                {
                    capturedCount++;
                    CaptureFirefly(firefly);
                }
            }

            if (capturedCount > 0)
            {
                FireflyDanceLogger.LogCapture($" {capturedCount} luciole(s) capturée(s) à la position {handPos3D}");
            }
        }

        private void CaptureFirefly(FireflyController firefly)
        {
            // Vérification supplémentaire pour éviter les captures multiples
            if (firefly == null || !firefly.IsActive)
            {
                return;
            }

            FireflyDanceLogger.LogCapture($"Capture luciole: {firefly.name} - Score: {scorePerCapture}");

            if (scoreManager != null)
            {
                scoreManager.AddScore(scorePerCapture);
            }
            else
            {
                FireflyDanceLogger.LogWarning("Impossible d'ajouter le score - ScoreManager manquant", this);
            }

            // Émettre l'événement pour notification uniquement
            FireflyDanceEvents.OnFireflyCaptured?.Invoke(firefly, scorePerCapture);

            // Détruire la luciole immédiatement
            firefly.OnCaptured();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Active ou désactive le système de capture
        /// </summary>
        public void SetCaptureEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        /// <summary>
        /// Change le rayon de capture
        /// </summary>
        public void SetCaptureRadius(float radius)
        {
            captureRadius = Mathf.Max(0.1f, radius);
        }

        /// <summary>
        /// Change le score par capture
        /// </summary>
        public void SetScorePerCapture(int score)
        {
            scorePerCapture = Mathf.Max(1, score);
        }
        
        #endregion
    }
}
