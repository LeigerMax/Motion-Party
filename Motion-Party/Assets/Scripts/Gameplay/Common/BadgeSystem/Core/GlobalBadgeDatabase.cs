using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Base de données globale des badges pour tous les mini-jeux
    /// Permet de gérer tous les badges de Motion-Party dans un seul endroit
    /// </summary>
    [CreateAssetMenu(fileName = "MotionPartyBadgeDatabase", menuName = "Gameplay/Common/Badge Database")]
    public class GlobalBadgeDatabase : ScriptableObject
    {
        #region Fields

        [Header("Global Badge Database")]
        [SerializeField] private List<BadgeDefinition> allBadges = new List<BadgeDefinition>();
        
        [Header("Database Info")]
        [SerializeField] private string databaseVersion = "1.0";
        [SerializeField] private string lastModified;

        #endregion

        #region Properties

        /// <summary>
        /// Liste de tous les badges disponibles
        /// </summary>
        public IReadOnlyList<BadgeDefinition> AllBadges => allBadges;

        /// <summary>
        /// Version de la base de données
        /// </summary>
        public string DatabaseVersion => databaseVersion;

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtient tous les badges d'un mini-jeu spécifique
        /// </summary>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Liste des badges du mini-jeu</returns>
        public List<BadgeDefinition> GetBadgesForGame(string miniGameId)
        {
            return allBadges.Where(badge => badge.BelongsToGame(miniGameId)).ToList();
        }

        /// <summary>
        /// Recherche un badge par son ID complet
        /// </summary>
        /// <param name="fullBadgeId">ID complet du badge (gameId_badgeId)</param>
        /// <returns>Le badge trouvé ou null</returns>
        public BadgeDefinition GetBadgeByFullId(string fullBadgeId)
        {
            return allBadges.Find(badge => badge.GetFullId() == fullBadgeId);
        }

        /// <summary>
        /// Recherche un badge par son ID simple dans un jeu spécifique
        /// </summary>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID simple du badge</param>
        /// <returns>Le badge trouvé ou null</returns>
        public BadgeDefinition GetBadgeById(string miniGameId, string badgeId)
        {
            return allBadges.Find(badge => 
                badge.BelongsToGame(miniGameId) && badge.BadgeId == badgeId);
        }

        /// <summary>
        /// Obtient tous les badges d'un type spécifique dans un jeu
        /// </summary>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeType">Type de badge</param>
        /// <returns>Liste des badges du type spécifié</returns>
        public List<BadgeDefinition> GetBadgesByType(string miniGameId, string badgeType)
        {
            return allBadges.Where(badge => 
                badge.BelongsToGame(miniGameId) && badge.BadgeType == badgeType).ToList();
        }

        /// <summary>
        /// Obtient tous les badges d'une catégorie spécifique
        /// </summary>
        /// <param name="category">Catégorie recherchée</param>
        /// <returns>Liste des badges de la catégorie</returns>
        public List<BadgeDefinition> GetBadgesByCategory(BadgeCategory category)
        {
            return allBadges.Where(badge => badge.Category == category).ToList();
        }

        /// <summary>
        /// Obtient tous les badges d'une rareté spécifique
        /// </summary>
        /// <param name="rarity">Rareté recherchée</param>
        /// <returns>Liste des badges de la rareté</returns>
        public List<BadgeDefinition> GetBadgesByRarity(BadgeRarity rarity)
        {
            return allBadges.Where(badge => badge.Rarity == rarity).ToList();
        }

        /// <summary>
        /// Obtient la liste des mini-jeux ayant des badges
        /// </summary>
        /// <returns>Liste des IDs de mini-jeux</returns>
        public List<string> GetMiniGameIds()
        {
            return allBadges.Select(badge => badge.MiniGameId)
                           .Distinct()
                           .Where(id => !string.IsNullOrEmpty(id))
                           .ToList();
        }

        /// <summary>
        /// Ajoute un badge à la base de données
        /// </summary>
        /// <param name="badge">Badge à ajouter</param>
        public void AddBadge(BadgeDefinition badge)
        {
            if (badge != null && !allBadges.Contains(badge))
            {
                allBadges.Add(badge);
                UpdateLastModified();
            }
        }

        /// <summary>
        /// Supprime un badge de la base de données
        /// </summary>
        /// <param name="fullBadgeId">ID complet du badge à supprimer</param>
        /// <returns>True si supprimé avec succès</returns>
        public bool RemoveBadge(string fullBadgeId)
        {
            var badge = GetBadgeByFullId(fullBadgeId);
            if (badge != null)
            {
                allBadges.Remove(badge);
                UpdateLastModified();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Vérifie si un badge existe
        /// </summary>
        /// <param name="fullBadgeId">ID complet du badge</param>
        /// <returns>True si le badge existe</returns>
        public bool HasBadge(string fullBadgeId)
        {
            return GetBadgeByFullId(fullBadgeId) != null;
        }

        /// <summary>
        /// Obtient le nombre total de badges
        /// </summary>
        /// <returns>Nombre de badges dans la base</returns>
        public int GetTotalBadgeCount()
        {
            return allBadges.Count;
        }

        /// <summary>
        /// Obtient le nombre de badges pour un mini-jeu
        /// </summary>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Nombre de badges pour ce jeu</returns>
        public int GetBadgeCountForGame(string miniGameId)
        {
            return GetBadgesForGame(miniGameId).Count;
        }

        /// <summary>
        /// Alias pour GetBadgeById - pour compatibilité
        /// </summary>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID du badge</param>
        /// <returns>Le badge trouvé ou null</returns>
        public BadgeDefinition GetBadge(string miniGameId, string badgeId)
        {
            return GetBadgeById(miniGameId, badgeId);
        }

        /// <summary>
        /// Alias pour AllBadges - pour compatibilité
        /// </summary>
        /// <returns>Liste de tous les badges</returns>
        public List<BadgeDefinition> GetAllBadges()
        {
            return allBadges.ToList();
        }

        /// <summary>
        /// Alias pour GetMiniGameIds - pour compatibilité
        /// </summary>
        /// <returns>Liste des IDs de jeux</returns>
        public List<string> GetAllGameIds()
        {
            return GetMiniGameIds();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Met à jour la date de dernière modification
        /// </summary>
        private void UpdateLastModified()
        {
            lastModified = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        #endregion

        #region Unity Editor Methods

#if UNITY_EDITOR
        /// <summary>
        /// Initialise la base de données avec des badges par défaut pour tous les jeux
        /// </summary>
        [ContextMenu("Initialize All Default Badges")]
        public void InitializeAllDefaultBadges()
        {
            allBadges.Clear();
            
            // Badges FireflyDance
            InitializeFireflyBadges();
            
            // Badges Balloon (exemple)
            InitializeBalloonBadges();
            
            // Badges MusicNotePress (exemple)
            InitializeMusicBadges();
            
            UpdateLastModified();
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Alias pour InitializeAllDefaultBadges - pour compatibilité
        /// </summary>
        [ContextMenu("Create Default Badges")]
        public void CreateDefaultBadges()
        {
            InitializeAllDefaultBadges();
        }

        /// <summary>
        /// Initialise les badges spécifiques à FireflyDance
        /// </summary>
        [ContextMenu("Initialize Firefly Badges")]
        public void InitializeFireflyBadges()
        {
            // Supprimer les anciens badges firefly
            allBadges.RemoveAll(badge => badge.BelongsToGame("firefly"));
            
            // Badges de performance
            var firstFirefly = new BadgeDefinition("first_firefly", "Première Luciole", "Capturer sa première luciole", "firefly", "captures", 1);
            firstFirefly.SetCategory(BadgeCategory.Performance);
            firstFirefly.SetRarity(BadgeRarity.Common);
            allBadges.Add(firstFirefly);
            
            var lightningHunter = new BadgeDefinition("lightning_hunter", "Chasseur Éclair", "Capturer 10 lucioles en une partie", "firefly", "captures", 10);
            lightningHunter.SetCategory(BadgeCategory.Performance);
            lightningHunter.SetRarity(BadgeRarity.Uncommon);
            allBadges.Add(lightningHunter);
            
            var fireflyMaster = new BadgeDefinition("firefly_master", "Maître des Lucioles", "Capturer 25 lucioles en une partie", "firefly", "captures", 25);
            fireflyMaster.SetCategory(BadgeCategory.Performance);
            fireflyMaster.SetRarity(BadgeRarity.Rare);
            allBadges.Add(fireflyMaster);
            
            // Badges de précision
            var zeroError = new BadgeDefinition("zero_error", "Zéro Faute", "Aucune fausse détection pendant 60 secondes", "firefly", "no_errors", 60);
            zeroError.SetCategory(BadgeCategory.Precision);
            zeroError.SetRarity(BadgeRarity.Uncommon);
            allBadges.Add(zeroError);
            
            var perfectStreak = new BadgeDefinition("perfect_streak", "Série Parfaite", "Capturer 5 lucioles consécutives sans erreur", "firefly", "streak", 5);
            perfectStreak.SetCategory(BadgeCategory.Precision);
            perfectStreak.SetRarity(BadgeRarity.Rare);
            allBadges.Add(perfectStreak);
            
            var precisionMaster = new BadgeDefinition("precision_master", "Maître de Précision", "Atteindre 90% de précision", "firefly", "accuracy", 90);
            precisionMaster.SetCategory(BadgeCategory.Precision);
            precisionMaster.SetRarity(BadgeRarity.Epic);
            allBadges.Add(precisionMaster);
            
            // Badges d'exploration
            var explorer = new BadgeDefinition("explorer", "Explorateur", "Capturer une luciole dans chaque coin de la zone", "firefly", "zones", 4);
            explorer.SetCategory(BadgeCategory.Exploration);
            explorer.SetRarity(BadgeRarity.Uncommon);
            allBadges.Add(explorer);
            
            // Badges de vitesse
            var speedDemon = new BadgeDefinition("speed_demon", "Démon de Vitesse", "Capturer une luciole en moins de 0.5 seconde", "firefly", "speed", 1);
            speedDemon.SetCategory(BadgeCategory.Performance);
            speedDemon.SetRarity(BadgeRarity.Epic);
            allBadges.Add(speedDemon);
            
            UpdateLastModified();
        }

        /// <summary>
        /// Initialise les badges pour le jeu Balloon
        /// </summary>
        [ContextMenu("Initialize Balloon Badges")]
        public void InitializeBalloonBadges()
        {
            // Supprimer les anciens badges balloon
            allBadges.RemoveAll(badge => badge.BelongsToGame("balloon"));
            
            // Badges balloon
            var firstPop = new BadgeDefinition("first_pop", "Premier Éclat", "Faire éclater son premier ballon", "balloon", "pops", 1);
            firstPop.SetCategory(BadgeCategory.Performance);
            firstPop.SetRarity(BadgeRarity.Common);
            allBadges.Add(firstPop);
            
            var balloonBurst = new BadgeDefinition("balloon_burst", "Rafale de Ballons", "Faire éclater 15 ballons en une partie", "balloon", "pops", 15);
            balloonBurst.SetCategory(BadgeCategory.Performance);
            balloonBurst.SetRarity(BadgeRarity.Uncommon);
            allBadges.Add(balloonBurst);
            
            var precisionPopper = new BadgeDefinition("precision_popper", "Précision Parfaite", "90% de précision sur les ballons", "balloon", "accuracy", 90);
            precisionPopper.SetCategory(BadgeCategory.Precision);
            precisionPopper.SetRarity(BadgeRarity.Epic);
            allBadges.Add(precisionPopper);
            
            var speedPopper = new BadgeDefinition("speed_popper", "Éclatement Rapide", "Éclater un ballon en moins de 1 seconde", "balloon", "speed", 1);
            speedPopper.SetCategory(BadgeCategory.Performance);
            speedPopper.SetRarity(BadgeRarity.Rare);
            allBadges.Add(speedPopper);
            
            UpdateLastModified();
        }

        /// <summary>
        /// Initialise les badges pour le jeu MusicNotePress
        /// </summary>
        [ContextMenu("Initialize Music Badges")]
        public void InitializeMusicBadges()
        {
            // Supprimer les anciens badges music
            allBadges.RemoveAll(badge => badge.BelongsToGame("music"));
            
            // Badges musique
            var firstNote = new BadgeDefinition("first_note", "Première Note", "Jouer sa première note correctement", "music", "notes", 1);
            firstNote.SetCategory(BadgeCategory.Performance);
            firstNote.SetRarity(BadgeRarity.Common);
            allBadges.Add(firstNote);
            
            var melodyMaster = new BadgeDefinition("melody_master", "Maître de Mélodie", "Jouer 20 notes correctes en une partie", "music", "notes", 20);
            melodyMaster.SetCategory(BadgeCategory.Performance);
            melodyMaster.SetRarity(BadgeRarity.Uncommon);
            allBadges.Add(melodyMaster);
            
            var perfectRhythm = new BadgeDefinition("perfect_rhythm", "Rythme Parfait", "Maintenir le rythme pendant 30 secondes", "music", "rhythm", 30);
            perfectRhythm.SetCategory(BadgeCategory.Precision);
            perfectRhythm.SetRarity(BadgeRarity.Rare);
            allBadges.Add(perfectRhythm);
            
            var fingerVirtuoso = new BadgeDefinition("finger_virtuoso", "Virtuose des Doigts", "Utiliser tous les doigts dans une séquence", "music", "fingers", 5);
            fingerVirtuoso.SetCategory(BadgeCategory.Exploration);
            fingerVirtuoso.SetRarity(BadgeRarity.Epic);
            allBadges.Add(fingerVirtuoso);
            
            UpdateLastModified();
        }

        /// <summary>
        /// Valide la cohérence de la base de données
        /// </summary>
        [ContextMenu("Validate Database")]
        public void ValidateDatabase()
        {
            var duplicateIds = new HashSet<string>();
            var foundIds = new HashSet<string>();
            
            foreach (var badge in allBadges)
            {
                var fullId = badge.GetFullId();
                
                if (string.IsNullOrEmpty(fullId))
                {
                    Debug.LogWarning($"Badge sans ID complet trouvé: {badge.BadgeName}");
                    continue;
                }
                
                if (foundIds.Contains(fullId))
                {
                    duplicateIds.Add(fullId);
                }
                else
                {
                    foundIds.Add(fullId);
                }
            }
            
            if (duplicateIds.Count > 0)
            {
                Debug.LogError($"IDs de badges dupliqués trouvés: {string.Join(", ", duplicateIds)}");
            }
            else
            {
                Debug.Log($"✅ Base de données validée avec succès ! {allBadges.Count} badges pour {GetMiniGameIds().Count} mini-jeux.");
            }
        }

        /// <summary>
        /// Affiche les statistiques de la base de données
        /// </summary>
        [ContextMenu("Show Database Statistics")]
        public void ShowDatabaseStatistics()
        {
            Debug.Log("=== STATISTIQUES BASE DE DONNÉES BADGES ===");
            Debug.Log($"Total badges: {GetTotalBadgeCount()}");
            
            var gameIds = GetMiniGameIds();
            Debug.Log($"Mini-jeux: {gameIds.Count}");
            
            foreach (var gameId in gameIds)
            {
                var count = GetBadgeCountForGame(gameId);
                Debug.Log($"  • {gameId}: {count} badge(s)");
            }
        }
#endif

        #endregion
    }
}
