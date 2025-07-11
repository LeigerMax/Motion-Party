using System;
using UnityEngine;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Définition générique d'un badge pour tous les mini-jeux
    /// Chaque mini-jeu peut avoir ses propres types de badges
    /// </summary>
    [Serializable]
    public class BadgeDefinition
    {
        #region Fields

        [Header("Badge Information")]
        [SerializeField] private string badgeId;
        [SerializeField] private string badgeName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        
        [Header("Game Association")]
        [SerializeField] private string miniGameId; // ID du mini-jeu (ex: "firefly", "balloon", "music")
        
        [Header("Badge Properties")]
        [SerializeField] private BadgeRarity rarity = BadgeRarity.Common;
        [SerializeField] private BadgeCategory category = BadgeCategory.Performance;
        
        [Header("Unlock Conditions")]
        [SerializeField] private string badgeType; // Type libre par mini-jeu
        [SerializeField] private int targetValue;
        [SerializeField] private float timeLimit = -1f; // -1 = pas de limite de temps
        [SerializeField] private bool isRepeatable = false;

        #endregion

        #region Properties

        /// <summary>
        /// Identifiant unique du badge
        /// </summary>
        public string BadgeId => badgeId;

        /// <summary>
        /// Nom affiché du badge
        /// </summary>
        public string BadgeName => badgeName;

        /// <summary>
        /// Description du badge
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Icône du badge
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// ID du mini-jeu associé
        /// </summary>
        public string MiniGameId => miniGameId;

        /// <summary>
        /// Rareté du badge
        /// </summary>
        public BadgeRarity Rarity => rarity;

        /// <summary>
        /// Catégorie du badge
        /// </summary>
        public BadgeCategory Category => category;

        /// <summary>
        /// Type de badge (défini par chaque mini-jeu)
        /// </summary>
        public string BadgeType => badgeType;

        /// <summary>
        /// Valeur cible à atteindre
        /// </summary>
        public int TargetValue => targetValue;

        /// <summary>
        /// Limite de temps pour accomplir le badge (-1 = pas de limite)
        /// </summary>
        public float TimeLimit => timeLimit;

        /// <summary>
        /// Le badge peut-il être obtenu plusieurs fois
        /// </summary>
        public bool IsRepeatable => isRepeatable;

        #endregion

        #region Constructor

        public BadgeDefinition(string id, string name, string desc, string gameId, string type, int value)
        {
            badgeId = id;
            badgeName = name;
            description = desc;
            miniGameId = gameId;
            badgeType = type;
            targetValue = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Vérifie si ce badge appartient à un mini-jeu spécifique
        /// </summary>
        /// <param name="gameId">ID du mini-jeu</param>
        /// <returns>True si le badge appartient à ce jeu</returns>
        public bool BelongsToGame(string gameId)
        {
            return string.Equals(miniGameId, gameId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtient l'ID complet du badge (préfixé par le jeu)
        /// </summary>
        /// <returns>ID complet : "gameId_badgeId"</returns>
        public string GetFullId()
        {
            return $"{miniGameId}_{badgeId}";
        }

        /// <summary>
        /// Obtient l'ID complet du badge (alias pour GetFullId)
        /// </summary>
        /// <returns>ID complet : "gameId_badgeId"</returns>
        public string GetFullBadgeId()
        {
            return GetFullId();
        }

        #endregion

        #region Public Setter Methods (for Editor)

        /// <summary>
        /// Définit la catégorie du badge (usage éditeur)
        /// </summary>
        public void SetCategory(BadgeCategory newCategory)
        {
            category = newCategory;
        }

        /// <summary>
        /// Définit la rareté du badge (usage éditeur)
        /// </summary>
        public void SetRarity(BadgeRarity newRarity)
        {
            rarity = newRarity;
        }

        /// <summary>
        /// Définit si le badge est répétable (usage éditeur)
        /// </summary>
        public void SetRepeatable(bool repeatable)
        {
            isRepeatable = repeatable;
        }

        /// <summary>
        /// Définit la limite de temps (usage éditeur)
        /// </summary>
        public void SetTimeLimit(float timeLimit)
        {
            this.timeLimit = timeLimit;
        }

        /// <summary>
        /// Définit l'icône du badge (usage éditeur)
        /// </summary>
        public void SetIcon(Sprite newIcon)
        {
            icon = newIcon;
        }

        #endregion
    }

    /// <summary>
    /// Rareté des badges (commun à tous les mini-jeux)
    /// </summary>
    public enum BadgeRarity
    {
        Common,     // Commun - facile à obtenir
        Uncommon,   // Peu commun
        Rare,       // Rare
        Epic,       // Épique
        Legendary   // Légendaire - très difficile à obtenir
    }

    /// <summary>
    /// Catégories de badges (communes à tous les mini-jeux)
    /// </summary>
    public enum BadgeCategory
    {
        Performance,    // Performance de jeu
        Precision,      // Précision
        Exploration,    // Exploration de l'interface
        Endurance,      // Endurance/persévérance
        Creativity,     // Créativité
        Social,         // Aspects sociaux/multijoueur
        Special         // Événements spéciaux
    }
}
