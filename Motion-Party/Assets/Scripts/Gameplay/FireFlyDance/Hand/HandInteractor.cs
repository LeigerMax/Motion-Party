using UnityEngine;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Hand
{
    /// <summary>
    /// Gère les interactions de la main avec les lucioles
    /// Détecte les captures quand la main fermée touche une luciole
    /// </summary>
    public class HandInteractor : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        [SerializeField] private HandTracker handTracker;
        
        [Header("Interaction Settings")]
        [SerializeField] private LayerMask fireflyLayer = -1;
        [SerializeField] private bool enableDebugGizmos = false;

        // État interne
        private bool isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (config != null)
            {
                Initialize(config);
            }
        }

        private void Update()
        {
            if (!isInitialized || !FireflyDanceStateController.CanInteract()) return;
            
            CheckForFireflyCapture();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise l'interacteur avec une configuration
        /// </summary>
        public void Initialize(FireflyDanceConfig danceConfig)
        {
            config = danceConfig;
            
            if (!ValidateComponents())
            {
                FireflyDanceLogger.LogError("HandInteractor - Impossible d'initialiser, composants manquants", this);
                return;
            }
            
            isInitialized = true;
            FireflyDanceLogger.Log("HandInteractor initialisé");
        }

        #endregion

        #region Interaction Detection

        /// <summary>
        /// Vérifie s'il y a capture d'une luciole
        /// </summary>
        private void CheckForFireflyCapture()
        {
            // Vérifier que la main est détectée et fermée
            if (!handTracker.IsHandDetected || !handTracker.IsHandClosed)
            {
                return;
            }

            Vector2 handPosition = handTracker.CurrentPosition;
            
            // Chercher les lucioles dans le rayon de capture
            Collider2D[] nearbyFireflies = Physics2D.OverlapCircleAll(
                handPosition, 
                config.CaptureRadius, 
                fireflyLayer
            );

            // Traiter les captures
            foreach (var collider in nearbyFireflies)
            {
                FireflyController firefly = collider.GetComponent<FireflyController>();
                if (firefly != null && firefly.IsActive)
                {
                    CaptureFirefly(firefly);
                }
            }
        }

        /// <summary>
        /// Traite la capture d'une luciole
        /// </summary>
        private void CaptureFirefly(FireflyController firefly)
        {
            FireflyDanceLogger.LogCapture($"Luciole capturée en position {firefly.transform.position}");
            
            // Marquer la luciole comme capturée
            firefly.OnCaptured();
            
            // Émettre l'événement de capture
            FireflyDanceEvents.OnFireflyCaptured?.Invoke(firefly);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Valide les composants requis
        /// </summary>
        private bool ValidateComponents()
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("HandInteractor - FireflyDanceConfig manquant");
                return false;
            }

            if (handTracker == null)
            {
                handTracker = GetComponent<HandTracker>();
                if (handTracker == null)
                {
                    handTracker = FindFirstObjectByType<HandTracker>();
                    if (handTracker == null)
                    {
                        FireflyDanceLogger.LogError("HandInteractor - HandTracker introuvable");
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Vérifie manuellement s'il y a capture à une position donnée
        /// </summary>
        public bool CheckCaptureAtPosition(Vector2 position)
        {
            if (!FireflyDanceStateController.CanInteract()) return false;
            
            Collider2D[] nearbyFireflies = Physics2D.OverlapCircleAll(
                position, 
                config.CaptureRadius, 
                fireflyLayer
            );

            return nearbyFireflies.Length > 0;
        }

        /// <summary>
        /// Obtient toutes les lucioles dans le rayon de capture
        /// </summary>
        public FireflyController[] GetFirefliesInRange(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                position, 
                config.CaptureRadius, 
                fireflyLayer
            );

            FireflyController[] fireflies = new FireflyController[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                fireflies[i] = colliders[i].GetComponent<FireflyController>();
            }

            return fireflies;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!enableDebugGizmos || config == null || handTracker == null) return;
            
            if (handTracker.IsHandDetected)
            {
                // Zone de capture
                Gizmos.color = handTracker.IsHandClosed ? Color.red : Color.green;
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
                
                Vector3 handPos = new Vector3(
                    handTracker.CurrentPosition.x, 
                    handTracker.CurrentPosition.y, 
                    0
                );
                
                Gizmos.DrawSphere(handPos, config.CaptureRadius);
                
                // Contour de la zone
                Gizmos.color = handTracker.IsHandClosed ? Color.red : Color.green;
                Gizmos.DrawWireSphere(handPos, config.CaptureRadius);
            }
        }

        #endregion
    }
}
