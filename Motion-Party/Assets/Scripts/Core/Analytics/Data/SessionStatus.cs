using System.Collections.Generic;

namespace Core.Analytics.Data
{
    /// <summary>
    /// Structure de données pour représenter le statut d'une session analytics
    /// </summary>
    [System.Serializable]
    public struct SessionStatus
    {
        public bool isActive;
        public string sessionId;
        public float progress;
        public bool isComplete;
        public int playerCount;
        public List<string> remainingGames;
        
        /// <summary>
        /// Crée un statut de session inactive
        /// </summary>
        public static SessionStatus Inactive => new SessionStatus
        {
            isActive = false,
            sessionId = null,
            progress = 0f,
            isComplete = false,
            playerCount = 0,
            remainingGames = new List<string>()
        };
    }
}
