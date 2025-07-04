using System;
using UnityEngine;

namespace Gameplay.FireFlyDance.Player.HandTracker
{
    /// <summary>
    /// Gestionnaire d'événements globaux pour le suivi de la main
    /// Permet à d'autres systèmes de s'abonner aux événements de la main sans référence directe
    /// </summary>
    public static class HandEvents
    {
        /// <summary>
        /// Événement déclenché quand les données de la main sont mises à jour
        /// </summary>
        public static event Action<HandData> OnHandDataUpdated;
        
        /// <summary>
        /// Événement déclenché quand la position de la main change
        /// </summary>
        public static event Action<Vector2> OnHandPositionChanged;
        
        /// <summary>
        /// Événement déclenché quand une main est détectée
        /// </summary>
        public static event Action<Vector2> OnHandDetected;
        
        /// <summary>
        /// Événement déclenché quand une main n'est plus détectée
        /// </summary>
        public static event Action OnHandLost;
        
        /// <summary>
        /// Événement déclenché quand le geste de la main change
        /// </summary>
        public static event Action<string> OnGestureChanged;
        
        /// <summary>
        /// Diffuse une mise à jour des données de la main
        /// </summary>
        /// <param name="handData">Nouvelles données de la main</param>
        public static void BroadcastHandDataUpdate(HandData handData)
        {
            OnHandDataUpdated?.Invoke(handData);
        }
        
        /// <summary>
        /// Diffuse un changement de position de la main
        /// </summary>
        /// <param name="position">Nouvelle position</param>
        public static void BroadcastHandPositionChange(Vector2 position)
        {
            OnHandPositionChanged?.Invoke(position);
        }
        
        /// <summary>
        /// Diffuse une détection de main
        /// </summary>
        /// <param name="position">Position de la main détectée</param>
        public static void BroadcastHandDetected(Vector2 position)
        {
            OnHandDetected?.Invoke(position);
        }
        
        /// <summary>
        /// Diffuse une perte de main
        /// </summary>
        public static void BroadcastHandLost()
        {
            OnHandLost?.Invoke();
        }
        
        /// <summary>
        /// Diffuse un changement de geste
        /// </summary>
        /// <param name="gesture">Nouveau geste</param>
        public static void BroadcastGestureChange(string gesture)
        {
            OnGestureChanged?.Invoke(gesture);
        }
    }
}
