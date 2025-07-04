using UnityEngine;
using Gameplay.FireFlyDance.Player.HandTracker;

namespace Gameplay.FireFlyDance.Player.HandTracker
{
    /// <summary>
    /// Exemple d'utilisation du HandTracker
    /// Démontre comment écouter les événements et utiliser les données de la main
    /// </summary>
    public class HandTrackerExample : MonoBehaviour
    {
        [Header("Références")]
        [SerializeField] private HandTracker handTracker;
        
        [Header("Debug")]
        [SerializeField] private bool logHandData = true;
        
        private void Start()
        {
            // Recherche automatique du HandTracker si non assigné
            if (handTracker == null)
            {
                handTracker = FindObjectOfType<HandTracker>();
            }
            
            // S'abonner aux événements
            if (handTracker != null)
            {
                handTracker.OnHandDataUpdated += OnHandDataUpdated;
                handTracker.OnHandPositionChanged += OnHandPositionChanged;
            }
        }
        
        private void OnDestroy()
        {
            // Se désabonner des événements
            if (handTracker != null)
            {
                handTracker.OnHandDataUpdated -= OnHandDataUpdated;
                handTracker.OnHandPositionChanged -= OnHandPositionChanged;
            }
        }
        
        /// <summary>
        /// Appelé quand les données de la main sont mises à jour
        /// </summary>
        /// <param name="handData">Nouvelles données de la main</param>
        private void OnHandDataUpdated(HandData handData)
        {
            if (logHandData)
            {
                if (handData.isDetected)
                {
                    Debug.Log($"[HandTrackerExample] Main détectée - Position: {handData.position}, " +
                             $"Geste: {handData.gesture}, Doigts ouverts: {handData.openFingers}");
                }
                else
                {
                    Debug.Log("[HandTrackerExample] Main non détectée");
                }
            }
        }
        
        /// <summary>
        /// Appelé quand la position de la main change
        /// </summary>
        /// <param name="position">Nouvelle position de la main</param>
        private void OnHandPositionChanged(Vector2 position)
        {
            // Exemple : vérifier si la main est dans une zone spécifique
            Vector2 targetZone = Vector2.zero; // Centre de la zone
            float targetRadius = 1f;
            
            if (handTracker.IsHandInArea(targetZone, targetRadius))
            {
                // La main est dans la zone cible
                // Ici on pourrait déclencher des actions spécifiques
            }
        }
        
        private void Update()
        {
            // Exemple d'utilisation des méthodes publiques
            if (handTracker != null && handTracker.IsHandDetected)
            {
                Vector2 currentPos = handTracker.GetCurrentPosition();
                
                // Utiliser la position pour votre logique de jeu
                // Par exemple : détecter les collisions avec des lucioles
            }
        }
    }
}
