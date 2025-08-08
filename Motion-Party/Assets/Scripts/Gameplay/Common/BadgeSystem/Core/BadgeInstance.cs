using System;
using UnityEngine;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Instance d'un badge obtenu par un joueur
    /// Version générique pour tous les mini-jeux
    /// </summary>
    [Serializable]
    public class BadgeInstance
    {
        #region Fields

        [SerializeField] private BadgeDefinition badgeDefinition;
        [SerializeField] private string earnedDate;
        [SerializeField] private float completionTime;
        [SerializeField] private int achievedValue;
        [SerializeField] private string gameSessionId;
        [SerializeField] private string miniGameId;

        #endregion

        #region Properties

        /// <summary>
        /// Définition du badge
        /// </summary>
        public BadgeDefinition BadgeDefinition => badgeDefinition;

        /// <summary>
        /// Date d'obtention du badge
        /// </summary>
        public string EarnedDate => earnedDate;

        /// <summary>
        /// Temps mis pour obtenir le badge (en secondes)
        /// </summary>
        public float CompletionTime => completionTime;

        /// <summary>
        /// Valeur atteinte pour obtenir le badge
        /// </summary>
        public int AchievedValue => achievedValue;

        /// <summary>
        /// ID de la session de jeu où le badge a été obtenu
        /// </summary>
        public string GameSessionId => gameSessionId;

        /// <summary>
        /// ID du mini-jeu où le badge a été obtenu
        /// </summary>
        public string MiniGameId => miniGameId;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructeur pour créer une instance de badge
        /// </summary>
        /// <param name="definition">Définition du badge</param>
        /// <param name="sessionId">ID de la session de jeu</param>
        /// <param name="achievedVal">Valeur atteinte</param>
        /// <param name="completionTimeSeconds">Temps de completion</param>
        public BadgeInstance(BadgeDefinition definition, string sessionId = "", int achievedVal = 0, float completionTimeSeconds = 0f)
        {
            badgeDefinition = definition;
            earnedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            gameSessionId = sessionId;
            achievedValue = achievedVal;
            completionTime = completionTimeSeconds;
            miniGameId = definition?.MiniGameId ?? "";
        }

        /// <summary>
        /// Constructeur pour créer une instance de badge avec valeur float
        /// </summary>
        /// <param name="definition">Définition du badge</param>
        /// <param name="achievedValueFloat">Valeur atteinte (float)</param>
        public BadgeInstance(BadgeDefinition definition, float achievedValueFloat)
        {
            badgeDefinition = definition;
            earnedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            gameSessionId = "";
            achievedValue = Mathf.RoundToInt(achievedValueFloat);
            completionTime = 0f;
            miniGameId = definition?.MiniGameId ?? "";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtient une description formatée de l'obtention du badge
        /// </summary>
        /// <returns>Description formatée</returns>
        public string GetFormattedDescription()
        {
            var baseDesc = badgeDefinition.Description;
            
            if (achievedValue > badgeDefinition.TargetValue)
            {
                baseDesc += $" (Dépassé: {achievedValue}/{badgeDefinition.TargetValue})";
            }
            
            if (completionTime > 0)
            {
                baseDesc += $" en {completionTime:F1}s";
            }
            
            return baseDesc;
        }

        /// <summary>
        /// Vérifie si ce badge a été obtenu récemment
        /// </summary>
        /// <param name="hoursAgo">Nombre d'heures pour considérer comme récent</param>
        /// <returns>True si obtenu récemment</returns>
        public bool IsRecentlyEarned(int hoursAgo = 24)
        {
            if (DateTime.TryParse(earnedDate, out var earned))
            {
                return (DateTime.Now - earned).TotalHours <= hoursAgo;
            }
            return false;
        }

        /// <summary>
        /// Obtient l'ID complet du badge
        /// </summary>
        /// <returns>ID complet du badge</returns>
        public string GetFullBadgeId()
        {
            return badgeDefinition?.GetFullId() ?? "";
        }

        #endregion
    }
}
