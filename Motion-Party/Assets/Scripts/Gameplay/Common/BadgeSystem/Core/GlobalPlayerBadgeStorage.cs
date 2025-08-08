using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Stockage global des badges obtenus par tous les joueurs pour tous les mini-jeux
    /// Centralise la gestion des badges de Motion-Party
    /// </summary>
    public class GlobalPlayerBadgeStorage : MonoBehaviour
    {
        #region Fields

        [Header("Storage Configuration")]
        [SerializeField] private bool enablePersistence = true;
        [SerializeField] private string storageKey = "MotionParty_AllPlayerBadges";

        [Header("Runtime Data (Read Only)")]
        [SerializeField] private List<PlayerBadgeData> allPlayersBadges = new List<PlayerBadgeData>();

        // Cache pour un accès rapide
        private Dictionary<string, PlayerBadgeData> playersCache = new Dictionary<string, PlayerBadgeData>();

        // Singleton pour accès global
        private static GlobalPlayerBadgeStorage _instance;
        public static GlobalPlayerBadgeStorage Instance => _instance;

        #endregion

        #region Events

        /// <summary>
        /// Événement déclenché quand un joueur obtient un nouveau badge
        /// </summary>
        public static Action<string, BadgeInstance> OnPlayerBadgeEarned;

        /// <summary>
        /// Événement déclenché quand les badges d'un joueur sont mis à jour
        /// </summary>
        public static Action<string, List<BadgeInstance>> OnPlayerBadgesUpdated;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            // Singleton
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Construire le cache
                RebuildCache();
                
                // Charger les données sauvegardées
                if (enablePersistence)
                {
                    LoadPlayerBadges();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            // Sauvegarder les données
            if (enablePersistence && _instance == this)
            {
                SavePlayerBadges();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && enablePersistence)
            {
                SavePlayerBadges();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && enablePersistence)
            {
                SavePlayerBadges();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Ajoute un badge à un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="badgeInstance">Instance du badge à ajouter</param>
        /// <returns>True si le badge a été ajouté avec succès</returns>
        public bool AddBadgeToPlayer(string playerName, BadgeInstance badgeInstance)
        {
            if (string.IsNullOrEmpty(playerName) || badgeInstance == null)
                return false;

            var playerData = GetOrCreatePlayerData(playerName);
            
            // Vérifier si le badge peut être ajouté (pas de doublons sauf si répétable)
            if (!badgeInstance.BadgeDefinition.IsRepeatable)
            {
                var existingBadge = playerData.badges.Find(b => 
                    b.GetFullBadgeId() == badgeInstance.GetFullBadgeId());
                if (existingBadge != null)
                {
                    Debug.LogWarning($"Badge {badgeInstance.GetFullBadgeId()} déjà possédé par {playerName}");
                    return false;
                }
            }

            // Ajouter le badge
            playerData.badges.Add(badgeInstance);
            
            // Déclencher les événements
            OnPlayerBadgeEarned?.Invoke(playerName, badgeInstance);
            OnPlayerBadgesUpdated?.Invoke(playerName, new List<BadgeInstance>(playerData.badges));

            return true;
        }

        /// <summary>
        /// Obtient tous les badges d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Liste des badges du joueur</returns>
        public List<BadgeInstance> GetPlayerBadges(string playerName)
        {
            if (playersCache.TryGetValue(playerName, out var playerData))
            {
                return new List<BadgeInstance>(playerData.badges);
            }
            return new List<BadgeInstance>();
        }

        /// <summary>
        /// Obtient les badges d'un joueur pour un mini-jeu spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Liste des badges du joueur pour ce jeu</returns>
        public List<BadgeInstance> GetPlayerBadgesForGame(string playerName, string miniGameId)
        {
            var allBadges = GetPlayerBadges(playerName);
            return allBadges.FindAll(badge => badge.MiniGameId == miniGameId);
        }

        /// <summary>
        /// Vérifie si un joueur possède un badge spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="fullBadgeId">ID complet du badge</param>
        /// <returns>True si le joueur possède le badge</returns>
        public bool PlayerHasBadge(string playerName, string fullBadgeId)
        {
            var badges = GetPlayerBadges(playerName);
            return badges.Exists(badge => badge.GetFullBadgeId() == fullBadgeId);
        }

        /// <summary>
        /// Vérifie si un joueur possède un badge dans un jeu spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID simple du badge</param>
        /// <returns>True si le joueur possède le badge</returns>
        public bool PlayerHasBadgeInGame(string playerName, string miniGameId, string badgeId)
        {
            var fullId = $"{miniGameId}_{badgeId}";
            return PlayerHasBadge(playerName, fullId);
        }

        /// <summary>
        /// Obtient le nombre total de badges d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Nombre de badges possédés</returns>
        public int GetPlayerBadgeCount(string playerName)
        {
            return GetPlayerBadges(playerName).Count;
        }

        /// <summary>
        /// Obtient le nombre de badges d'un joueur pour un jeu spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Nombre de badges pour ce jeu</returns>
        public int GetPlayerBadgeCountForGame(string playerName, string miniGameId)
        {
            return GetPlayerBadgesForGame(playerName, miniGameId).Count;
        }

        /// <summary>
        /// Obtient les badges d'un joueur par catégorie
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="category">Catégorie recherchée</param>
        /// <returns>Liste des badges de la catégorie</returns>
        public List<BadgeInstance> GetPlayerBadgesByCategory(string playerName, BadgeCategory category)
        {
            var badges = GetPlayerBadges(playerName);
            return badges.FindAll(badge => badge.BadgeDefinition.Category == category);
        }

        /// <summary>
        /// Obtient les badges d'un joueur par rareté
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="rarity">Rareté recherchée</param>
        /// <returns>Liste des badges de la rareté</returns>
        public List<BadgeInstance> GetPlayerBadgesByRarity(string playerName, BadgeRarity rarity)
        {
            var badges = GetPlayerBadges(playerName);
            return badges.FindAll(badge => badge.BadgeDefinition.Rarity == rarity);
        }

        /// <summary>
        /// Supprime tous les badges d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void ClearPlayerBadges(string playerName)
        {
            if (playersCache.TryGetValue(playerName, out var playerData))
            {
                playerData.badges.Clear();
                OnPlayerBadgesUpdated?.Invoke(playerName, new List<BadgeInstance>());
            }
        }

        /// <summary>
        /// Supprime tous les badges d'un joueur pour un jeu spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        public void ClearPlayerBadgesForGame(string playerName, string miniGameId)
        {
            if (playersCache.TryGetValue(playerName, out var playerData))
            {
                playerData.badges.RemoveAll(badge => badge.MiniGameId == miniGameId);
                OnPlayerBadgesUpdated?.Invoke(playerName, new List<BadgeInstance>(playerData.badges));
            }
        }

        /// <summary>
        /// Obtient la liste de tous les joueurs ayant des badges
        /// </summary>
        /// <returns>Liste des noms de joueurs</returns>
        public List<string> GetPlayersWithBadges()
        {
            return new List<string>(playersCache.Keys);
        }

        /// <summary>
        /// Obtient les statistiques globales des badges
        /// </summary>
        /// <returns>Dictionnaire des statistiques</returns>
        public Dictionary<string, object> GetGlobalStatistics()
        {
            var stats = new Dictionary<string, object>();
            
            stats["totalPlayers"] = playersCache.Count;
            stats["totalBadges"] = allPlayersBadges.Sum(p => p.badges.Count);
            
            var gameStats = new Dictionary<string, int>();
            foreach (var playerData in allPlayersBadges)
            {
                foreach (var badge in playerData.badges)
                {
                    var gameId = badge.MiniGameId;
                    if (!gameStats.ContainsKey(gameId))
                        gameStats[gameId] = 0;
                    gameStats[gameId]++;
                }
            }
            stats["badgesByGame"] = gameStats;
            
            return stats;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Obtient ou crée les données d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Données du joueur</returns>
        private PlayerBadgeData GetOrCreatePlayerData(string playerName)
        {
            if (!playersCache.TryGetValue(playerName, out var playerData))
            {
                playerData = new PlayerBadgeData
                {
                    playerName = playerName,
                    badges = new List<BadgeInstance>()
                };
                
                allPlayersBadges.Add(playerData);
                playersCache[playerName] = playerData;
            }
            
            return playerData;
        }

        /// <summary>
        /// Reconstruit le cache à partir des données sérialisées
        /// </summary>
        private void RebuildCache()
        {
            playersCache.Clear();
            
            foreach (var playerData in allPlayersBadges)
            {
                if (!string.IsNullOrEmpty(playerData.playerName))
                {
                    playersCache[playerData.playerName] = playerData;
                }
            }
        }

        /// <summary>
        /// Sauvegarde les badges des joueurs
        /// </summary>
        private void SavePlayerBadges()
        {
            try
            {
                var jsonData = JsonUtility.ToJson(new SerializablePlayerBadgeList { players = allPlayersBadges });
                PlayerPrefs.SetString(storageKey, jsonData);
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de la sauvegarde des badges: {ex.Message}");
            }
        }

        /// <summary>
        /// Charge les badges des joueurs
        /// </summary>
        private void LoadPlayerBadges()
        {
            try
            {
                if (PlayerPrefs.HasKey(storageKey))
                {
                    var jsonData = PlayerPrefs.GetString(storageKey);
                    var loadedData = JsonUtility.FromJson<SerializablePlayerBadgeList>(jsonData);
                    
                    if (loadedData != null && loadedData.players != null)
                    {
                        allPlayersBadges = loadedData.players;
                        RebuildCache();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors du chargement des badges: {ex.Message}");
            }
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Données des badges d'un joueur
        /// </summary>
        [Serializable]
        public class PlayerBadgeData
        {
            public string playerName;
            public List<BadgeInstance> badges = new List<BadgeInstance>();
        }

        /// <summary>
        /// Wrapper pour la sérialisation JSON
        /// </summary>
        [Serializable]
        private class SerializablePlayerBadgeList
        {
            public List<PlayerBadgeData> players;
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// Affiche les statistiques globales des badges dans la console
        /// </summary>
        [ContextMenu("Show Global Badge Statistics")]
        public void ShowGlobalBadgeStatistics()
        {
            Debug.Log("=== STATISTIQUES GLOBALES DES BADGES ===");
            
            var stats = GetGlobalStatistics();
            Debug.Log($"Joueurs totaux: {stats["totalPlayers"]}");
            Debug.Log($"Badges totaux: {stats["totalBadges"]}");
            
            var gameStats = (Dictionary<string, int>)stats["badgesByGame"];
            Debug.Log("Badges par jeu:");
            foreach (var kvp in gameStats)
            {
                Debug.Log($"  • {kvp.Key}: {kvp.Value} badge(s)");
            }
            
            foreach (var playerData in allPlayersBadges)
            {
                Debug.Log($"Joueur: {playerData.playerName} - {playerData.badges.Count} badge(s)");
            }
        }

        /// <summary>
        /// Efface toutes les données de badges
        /// </summary>
        [ContextMenu("Clear All Badge Data")]
        public void ClearAllBadgeData()
        {
            allPlayersBadges.Clear();
            playersCache.Clear();
            
            if (enablePersistence)
            {
                PlayerPrefs.DeleteKey(storageKey);
                PlayerPrefs.Save();
            }
            
            Debug.Log("Toutes les données de badges ont été effacées");
        }

        /// <summary>
        /// Affiche tous les badges d'un joueur dans la console (pour debug)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void ShowPlayerBadges(string playerName)
        {
            var badges = GetPlayerBadges(playerName);
            Debug.Log($"🏆 {playerName} possède {badges.Count} badges:");
            
            if (badges.Count == 0)
            {
                Debug.Log($"   Aucun badge trouvé pour {playerName}");
                return;
            }
            
            // Grouper par jeu
            var gameGroups = badges.GroupBy(b => b.BadgeDefinition?.MiniGameId ?? "unknown");
            
            foreach (var group in gameGroups)
            {
                Debug.Log($"   🎮 {group.Key.ToUpper()}:");
                foreach (var badge in group)
                {
                    var def = badge.BadgeDefinition;
                    if (def != null)
                    {
                        Debug.Log($"      ✓ {def.BadgeName} ({def.BadgeId}) - {badge.EarnedDate}");
                    }
                }
            }
        }

        /// <summary>
        /// Obtient la liste des badges d'un joueur pour affichage UI
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Liste formatée des badges</returns>
        public List<string> GetPlayerBadgeDisplayList(string playerName)
        {
            var badges = GetPlayerBadges(playerName);
            var displayList = new List<string>();
            
            foreach (var badge in badges)
            {
                var def = badge.BadgeDefinition;
                if (def != null)
                {
                    displayList.Add($"{def.BadgeName} - {def.Description}");
                }
            }
            
            return displayList;
        }

        /// <summary>
        /// Diagnostic complet du système pour un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        [ContextMenu("Diagnostic Player")]
        public void DiagnosticPlayer()
        {
            DiagnosticPlayer("TestPlayer");
        }

        public void DiagnosticPlayer(string playerName)
        {
            Debug.Log($"🔍 DIAGNOSTIC SYSTÈME BADGES - {playerName}");
            Debug.Log($"📊 Statut des composants:");
            
            // Vérifier les composants
            var badgeSystem = FindFirstObjectByType<GlobalBadgeSystem>();
            var badgeTracker = FindFirstObjectByType<GlobalBadgeTracker>();
            var database = badgeSystem?.GetDatabase();
            
            Debug.Log($"   GlobalBadgeSystem: {(badgeSystem != null ? "✓ OK" : "❌ MANQUANT")}");
            Debug.Log($"   GlobalBadgeTracker: {(badgeTracker != null ? "✓ OK" : "❌ MANQUANT")}");
            Debug.Log($"   GlobalPlayerBadgeStorage: {(this != null ? "✓ OK" : "❌ MANQUANT")}");
            Debug.Log($"   Badge Database: {(database != null ? "✓ OK" : "❌ MANQUANT")}");
            
            if (database != null)
            {
                var fireflyBadges = database.GetBadgesForGame("firefly");
                Debug.Log($"   Badges Firefly disponibles: {fireflyBadges.Count}");
                foreach (var badge in fireflyBadges)
                {
                    Debug.Log($"      - {badge.BadgeId}: {badge.BadgeName} (cible: {badge.TargetValue})");
                }
            }
            
            // Vérifier les données du joueur
            ShowPlayerBadges(playerName);
            
            // Vérifier les métriques
            if (badgeTracker != null)
            {
                Debug.Log($"📈 Métriques de {playerName}:");
                // Ici on pourrait afficher les métriques si elles sont accessibles
            }
        }
#endif

        #endregion
    }
}
