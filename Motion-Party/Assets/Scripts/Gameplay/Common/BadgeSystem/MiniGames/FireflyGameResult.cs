using UnityEngine;

namespace Gameplay.Firefly.Badges
{
    /// <summary>
    /// Structure de données pour encapsuler les résultats d'une partie de Firefly Dance
    /// Utilisée pour le tracking des badges
    /// </summary>
    [System.Serializable]
    public class FireflyGameResult
    {
        [Header("Score & Performance")]
        public float finalScore;
        public float gameTime;
        public int firefliesCollected;
        public float accuracy;
        
        [Header("Combos & Special")]
        public int maxCombo;
        public int perfectFireflies;
        public float totalDuration;
        
        [Header("Additional Metrics")]
        public int missedFireflies;
        public float averageResponseTime;
        public bool completedWithoutErrors;
        
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public FireflyGameResult()
        {
            finalScore = 0f;
            gameTime = 0f;
            firefliesCollected = 0;
            accuracy = 0f;
            maxCombo = 0;
            perfectFireflies = 0;
            totalDuration = 0f;
            missedFireflies = 0;
            averageResponseTime = 0f;
            completedWithoutErrors = false;
        }
        
        /// <summary>
        /// Constructeur avec les valeurs essentielles
        /// </summary>
        /// <param name="score">Score final</param>
        /// <param name="time">Temps de jeu</param>
        /// <param name="collected">Lucioles collectées</param>
        /// <param name="acc">Précision</param>
        public FireflyGameResult(float score, float time, int collected, float acc)
        {
            finalScore = score;
            gameTime = time;
            firefliesCollected = collected;
            accuracy = acc;
            maxCombo = 0;
            perfectFireflies = 0;
            totalDuration = time;
            missedFireflies = 0;
            averageResponseTime = 0f;
            completedWithoutErrors = false;
        }
        
        /// <summary>
        /// Convertit les résultats en chaîne pour debug
        /// </summary>
        /// <returns>Représentation textuelle des résultats</returns>
        public override string ToString()
        {
            return $"FireflyGameResult [Score: {finalScore}, Temps: {gameTime}s, Collectées: {firefliesCollected}, Précision: {accuracy:F1}%]";
        }
    }
}
