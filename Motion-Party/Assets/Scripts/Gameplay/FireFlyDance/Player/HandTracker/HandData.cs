using System;
using UnityEngine;

namespace Gameplay.FireFlyDance.Player.HandTracker
{
    /// <summary>
    /// Structure de données pour les informations de la main reçues via UDP
    /// </summary>
    [Serializable]
    public struct HandData
    {
        /// <summary>
        /// Position du point de la main dans le monde Unity
        /// </summary>
        public Vector2 position;
        
        /// <summary>
        /// Geste détecté de la main
        /// </summary>
        public string gesture;
        
        /// <summary>
        /// Nombre de doigts ouverts
        /// </summary>
        public int openFingers;
        
        /// <summary>
        /// Indique si la main est détectée
        /// </summary>
        public bool isDetected;
        
        /// <summary>
        /// Constructeur pour initialiser les données de la main
        /// </summary>
        /// <param name="position">Position dans le monde Unity</param>
        /// <param name="gesture">Geste détecté</param>
        /// <param name="openFingers">Nombre de doigts ouverts</param>
        /// <param name="isDetected">État de détection</param>
        public HandData(Vector2 position, string gesture, int openFingers, bool isDetected)
        {
            this.position = position;
            this.gesture = gesture;
            this.openFingers = openFingers;
            this.isDetected = isDetected;
        }
        
        /// <summary>
        /// Données par défaut pour une main non détectée
        /// </summary>
        public static HandData Empty => new HandData(Vector2.zero, "hand_open", 0, false);
    }
}
