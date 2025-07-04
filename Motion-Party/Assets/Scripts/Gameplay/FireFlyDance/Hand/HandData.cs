using System;
using UnityEngine;

namespace Gameplay.FireFlyDance.Hand
{
    /// <summary>
    /// Structure de données pour les informations de la main reçues via UDP
    /// Version refactorisée pour l'architecture FireflyDance
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
        
        /// <summary>
        /// Vérifie si la main est fermée (seuil configurable)
        /// </summary>
        public bool IsHandClosed(int closedThreshold = 2)
        {
            return isDetected && openFingers <= closedThreshold;
        }
        
        /// <summary>
        /// Vérifie si la main est ouverte
        /// </summary>
        public bool IsHandOpen(int openThreshold = 3)
        {
            return isDetected && openFingers >= openThreshold;
        }
        
        /// <summary>
        /// Retourne une représentation string des données
        /// </summary>
        public override string ToString()
        {
            return $"HandData(Pos: {position}, Gesture: {gesture}, Fingers: {openFingers}, Detected: {isDetected})";
        }
        
        /// <summary>
        /// Vérifie l'égalité avec un autre HandData
        /// </summary>
        public bool Equals(HandData other)
        {
            return position.Equals(other.position) && 
                   gesture == other.gesture && 
                   openFingers == other.openFingers && 
                   isDetected == other.isDetected;
        }
        
        /// <summary>
        /// Override de Equals pour Object
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is HandData other && Equals(other);
        }
        
        /// <summary>
        /// Override de GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(position, gesture, openFingers, isDetected);
        }
        
        /// <summary>
        /// Opérateur d'égalité
        /// </summary>
        public static bool operator ==(HandData left, HandData right)
        {
            return left.Equals(right);
        }
        
        /// <summary>
        /// Opérateur d'inégalité
        /// </summary>
        public static bool operator !=(HandData left, HandData right)
        {
            return !left.Equals(right);
        }
    }
}
