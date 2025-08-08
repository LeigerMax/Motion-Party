using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Classe utilitaire pour accéder aux badges depuis l'interface utilisateur
    /// Fournit des méthodes simples pour afficher les badges des joueurs
    /// </summary>
    public static class BadgeUIHelper
    {
        /// <summary>
        /// Obtient tous les badges d'un joueur pour affichage UI
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Liste des badges avec informations d'affichage</returns>
        public static List<BadgeDisplayInfo> GetPlayerBadgesForUI(string playerName)
        {
            var storage = GlobalPlayerBadgeStorage.Instance;
            if (storage == null)
            {
                Debug.LogWarning("GlobalPlayerBadgeStorage non trouvé");
                return new List<BadgeDisplayInfo>();
            }

            var badges = storage.GetPlayerBadges(playerName);
            var displayList = new List<BadgeDisplayInfo>();

            foreach (var badge in badges)
            {
                var def = badge.BadgeDefinition;
                if (def != null)
                {
                    displayList.Add(new BadgeDisplayInfo
                    {
                        BadgeId = def.BadgeId,
                        BadgeName = def.BadgeName,
                        Description = def.Description,
                        GameId = def.MiniGameId,
                        GameName = GetGameDisplayName(def.MiniGameId),
                        Rarity = def.Rarity.ToString(),
                        Category = def.Category.ToString(),
                        EarnedDate = badge.EarnedDate,
                        Icon = def.Icon
                    });
                }
            }

            return displayList.OrderByDescending(b => b.EarnedDate).ToList();
        }

        /// <summary>
        /// Obtient les badges d'un joueur pour un jeu spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="gameId">ID du jeu</param>
        /// <returns>Liste des badges du jeu</returns>
        public static List<BadgeDisplayInfo> GetPlayerBadgesForGame(string playerName, string gameId)
        {
            var allBadges = GetPlayerBadgesForUI(playerName);
            return allBadges.Where(b => b.GameId == gameId).ToList();
        }

        /// <summary>
        /// Obtient les statistiques de badges d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Statistiques des badges</returns>
        public static PlayerBadgeStats GetPlayerBadgeStats(string playerName)
        {
            var storage = GlobalPlayerBadgeStorage.Instance;
            if (storage == null) return new PlayerBadgeStats();

            var badges = storage.GetPlayerBadges(playerName);
            var stats = new PlayerBadgeStats
            {
                TotalBadges = badges.Count,
                CommonBadges = badges.Count(b => b.BadgeDefinition?.Rarity == BadgeRarity.Common),
                UncommonBadges = badges.Count(b => b.BadgeDefinition?.Rarity == BadgeRarity.Uncommon),
                RareBadges = badges.Count(b => b.BadgeDefinition?.Rarity == BadgeRarity.Rare),
                EpicBadges = badges.Count(b => b.BadgeDefinition?.Rarity == BadgeRarity.Epic),
                LegendaryBadges = badges.Count(b => b.BadgeDefinition?.Rarity == BadgeRarity.Legendary)
            };

            // Compter par jeu
            var gameGroups = badges.GroupBy(b => b.BadgeDefinition?.MiniGameId);
            foreach (var group in gameGroups)
            {
                switch (group.Key)
                {
                    case "firefly":
                        stats.FireflyBadges = group.Count();
                        break;
                    case "balloon":
                        stats.BalloonBadges = group.Count();
                        break;
                    case "music":
                        stats.MusicBadges = group.Count();
                        break;
                }
            }

            return stats;
        }

        /// <summary>
        /// Vérifie si un joueur possède un badge spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="gameId">ID du jeu</param>
        /// <param name="badgeId">ID du badge</param>
        /// <returns>True si le joueur possède le badge</returns>
        public static bool HasPlayerBadge(string playerName, string gameId, string badgeId)
        {
            var storage = GlobalPlayerBadgeStorage.Instance;
            if (storage == null) return false;

            string fullBadgeId = $"{gameId}.{badgeId}";
            return storage.PlayerHasBadge(playerName, fullBadgeId);
        }

        /// <summary>
        /// Affiche tous les badges d'un joueur dans la console (utile pour debug)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public static void DebugShowPlayerBadges(string playerName)
        {
            var badges = GetPlayerBadgesForUI(playerName);
            
            Debug.Log($"🏆 BADGES DE {playerName.ToUpper()} ({badges.Count} total):");
            
            if (badges.Count == 0)
            {
                Debug.Log("   Aucun badge obtenu");
                return;
            }

            var gameGroups = badges.GroupBy(b => b.GameId);
            foreach (var group in gameGroups)
            {
                Debug.Log($"   🎮 {group.Key.ToUpper()} ({group.Count()} badges):");
                foreach (var badge in group)
                {
                    Debug.Log($"      ✓ {badge.BadgeName} - {badge.Description} ({badge.EarnedDate})");
                }
            }
        }

        /// <summary>
        /// Obtient le nom d'affichage d'un jeu
        /// </summary>
        /// <param name="gameId">ID du jeu</param>
        /// <returns>Nom d'affichage</returns>
        private static string GetGameDisplayName(string gameId)
        {
            return gameId switch
            {
                "firefly" => "Danse des Lucioles",
                "balloon" => "Éclateur de Ballons",
                "music" => "Presse-Notes Musical",
                _ => gameId
            };
        }
    }

    /// <summary>
    /// Informations d'affichage pour un badge
    /// </summary>
    [System.Serializable]
    public class BadgeDisplayInfo
    {
        public string BadgeId;
        public string BadgeName;
        public string Description;
        public string GameId;
        public string GameName;
        public string Rarity;
        public string Category;
        public string EarnedDate;
        public Sprite Icon;
    }

    /// <summary>
    /// Statistiques des badges d'un joueur
    /// </summary>
    [System.Serializable]
    public class PlayerBadgeStats
    {
        public int TotalBadges;
        public int CommonBadges;
        public int UncommonBadges;
        public int RareBadges;
        public int EpicBadges;
        public int LegendaryBadges;
        public int FireflyBadges;
        public int BalloonBadges;
        public int MusicBadges;
    }
}
