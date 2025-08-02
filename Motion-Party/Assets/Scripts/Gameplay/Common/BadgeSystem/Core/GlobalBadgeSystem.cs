using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Système de badge global pour tous les mini-jeux de Motion-Party
    /// Gère l'attribution, la validation et le suivi des badges
    /// </summary>
    public class GlobalBadgeSystem : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private GlobalBadgeDatabase badgeDatabase;
        [SerializeField] private GlobalPlayerBadgeStorage playerStorage;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool autoSave = true;

        [Header("Validation Settings")]
        [SerializeField] private bool validateBadgeConditions = true;
        [SerializeField] private float validationCooldown = 0.1f;

        // Cache pour les dernières validations (éviter le spam)
        private Dictionary<string, float> lastValidationTime = new Dictionary<string, float>();

        // Singleton
        private static GlobalBadgeSystem _instance;
        public static GlobalBadgeSystem Instance => _instance;

        #endregion

        #region Events

        /// <summary>
        /// Événement déclenché quand un badge est attribué
        /// </summary>
        public static Action<string, BadgeInstance, bool> OnBadgeEarned;

        /// <summary>
        /// Événement déclenché quand l'attribution d'un badge échoue
        /// </summary>
        public static Action<string, BadgeDefinition, string> OnBadgeEarnFailed;

        /// <summary>
        /// Événement déclenché quand des conditions sont validées
        /// </summary>
        public static Action<string, string, float> OnConditionsValidated;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            // Singleton
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            ValidateConfiguration();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tente d'attribuer un badge à un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID du badge</param>
        /// <param name="currentValue">Valeur actuelle à valider</param>
        /// <returns>True si le badge a été attribué</returns>
        public bool TryEarnBadge(string playerName, string miniGameId, string badgeId, float currentValue = 0f)
        {
            if (!ValidateInput(playerName, miniGameId, badgeId))
                return false;

            // Cooldown de validation
            var validationKey = $"{playerName}_{miniGameId}_{badgeId}";
            if (IsInCooldown(validationKey))
                return false;

            // Obtenir la définition du badge
            var badgeDefinition = badgeDatabase.GetBadge(miniGameId, badgeId);
            if (badgeDefinition == null)
            {
                LogError($"Badge non trouvé: {miniGameId}_{badgeId}");
                OnBadgeEarnFailed?.Invoke(playerName, null, "Badge non trouvé");
                return false;
            }

            // Vérifier si le joueur peut obtenir ce badge
            if (!CanPlayerEarnBadge(playerName, badgeDefinition))
            {
                LogDebug($"Joueur {playerName} ne peut pas obtenir le badge {badgeDefinition.GetFullBadgeId()}");
                return false;
            }

            // Valider les conditions
            if (validateBadgeConditions && !ValidateBadgeConditions(badgeDefinition, currentValue))
            {
                //LogDebug($"Conditions non remplies pour {badgeDefinition.GetFullBadgeId()}: {currentValue}/{badgeDefinition.TargetValue}");
                OnConditionsValidated?.Invoke(playerName, badgeDefinition.GetFullBadgeId(), currentValue);
                return false;
            }

            // Créer l'instance du badge
            var badgeInstance = CreateBadgeInstance(badgeDefinition, currentValue);

            // Attribuer le badge
            bool success = playerStorage.AddBadgeToPlayer(playerName, badgeInstance);
            if (success)
            {
                LogSuccess($"Badge attribué: {playerName} a obtenu {badgeDefinition.GetFullBadgeId()}");
                OnBadgeEarned?.Invoke(playerName, badgeInstance, true);
                
                // Sauvegarder si activé
                if (autoSave)
                {
                    // La sauvegarde est gérée par GlobalPlayerBadgeStorage
                }
            }
            else
            {
                LogError($"Échec de l'attribution du badge {badgeDefinition.GetFullBadgeId()} à {playerName}");
                OnBadgeEarnFailed?.Invoke(playerName, badgeDefinition, "Échec de l'attribution");
            }

            // Mettre à jour le cooldown
            lastValidationTime[validationKey] = Time.time;

            return success;
        }

        /// <summary>
        /// Tente d'attribuer un badge en mode automatique (sans validation de valeur)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID du badge</param>
        /// <returns>True si le badge a été attribué</returns>
        public bool TryEarnBadgeAuto(string playerName, string miniGameId, string badgeId)
        {
            var badgeDefinition = badgeDatabase.GetBadge(miniGameId, badgeId);
            if (badgeDefinition == null)
                return false;

            // Mode automatique utilise la valeur cible comme valeur de réussite
            return TryEarnBadge(playerName, miniGameId, badgeId, badgeDefinition.TargetValue);
        }

        /// <summary>
        /// Vérifie si un joueur peut obtenir un badge spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="badgeDefinition">Définition du badge</param>
        /// <returns>True si le joueur peut obtenir le badge</returns>
        public bool CanPlayerEarnBadge(string playerName, BadgeDefinition badgeDefinition)
        {
            if (badgeDefinition == null)
                return false;

            // Si le badge n'est pas répétable, vérifier qu'il n'est pas déjà possédé
            if (!badgeDefinition.IsRepeatable)
            {
                bool alreadyHas = playerStorage.PlayerHasBadge(playerName, badgeDefinition.GetFullBadgeId());
                if (alreadyHas)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Obtient tous les badges disponibles pour un mini-jeu
        /// </summary>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Liste des badges disponibles</returns>
        public List<BadgeDefinition> GetAvailableBadgesForGame(string miniGameId)
        {
            return badgeDatabase.GetBadgesForGame(miniGameId);
        }

        /// <summary>
        /// Obtient la base de données de badges
        /// </summary>
        /// <returns>La base de données globale des badges</returns>
        public GlobalBadgeDatabase GetDatabase()
        {
            return badgeDatabase;
        }

        /// <summary>
        /// Obtient les badges qu'un joueur peut encore obtenir pour un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Liste des badges obtenables</returns>
        public List<BadgeDefinition> GetEarnableBadgesForPlayer(string playerName, string miniGameId)
        {
            var allBadges = GetAvailableBadgesForGame(miniGameId);
            var earnableBadges = new List<BadgeDefinition>();

            foreach (var badge in allBadges)
            {
                if (CanPlayerEarnBadge(playerName, badge))
                {
                    earnableBadges.Add(badge);
                }
            }

            return earnableBadges;
        }

        /// <summary>
        /// Obtient les badges obtenus par un joueur pour un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Liste des badges obtenus</returns>
        public List<BadgeInstance> GetPlayerBadgesForGame(string playerName, string miniGameId)
        {
            return playerStorage.GetPlayerBadgesForGame(playerName, miniGameId);
        }

        /// <summary>
        /// Obtient le pourcentage de progression d'un joueur pour un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Pourcentage entre 0 et 100</returns>
        public float GetPlayerProgressionForGame(string playerName, string miniGameId)
        {
            var totalBadges = GetAvailableBadgesForGame(miniGameId).Count;
            if (totalBadges == 0)
                return 100f;

            var earnedBadges = GetPlayerBadgesForGame(playerName, miniGameId).Count;
            return (float)earnedBadges / totalBadges * 100f;
        }

        /// <summary>
        /// Obtient les statistiques d'un joueur pour un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Dictionnaire des statistiques</returns>
        public Dictionary<string, object> GetPlayerGameStatistics(string playerName, string miniGameId)
        {
            var stats = new Dictionary<string, object>();
            
            var totalBadges = GetAvailableBadgesForGame(miniGameId);
            var earnedBadges = GetPlayerBadgesForGame(playerName, miniGameId);
            
            stats["totalAvailable"] = totalBadges.Count;
            stats["totalEarned"] = earnedBadges.Count;
            stats["progression"] = GetPlayerProgressionForGame(playerName, miniGameId);
            
            // Statistiques par catégorie
            var categoryStats = new Dictionary<string, int>();
            foreach (var badge in earnedBadges)
            {
                var category = badge.BadgeDefinition.Category.ToString();
                if (!categoryStats.ContainsKey(category))
                    categoryStats[category] = 0;
                categoryStats[category]++;
            }
            stats["byCategory"] = categoryStats;
            
            // Statistiques par rareté
            var rarityStats = new Dictionary<string, int>();
            foreach (var badge in earnedBadges)
            {
                var rarity = badge.BadgeDefinition.Rarity.ToString();
                if (!rarityStats.ContainsKey(rarity))
                    rarityStats[rarity] = 0;
                rarityStats[rarity]++;
            }
            stats["byRarity"] = rarityStats;
            
            return stats;
        }

        /// <summary>
        /// Force l'attribution d'un badge (ignore les conditions)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID du badge</param>
        /// <param name="overrideValue">Valeur à enregistrer</param>
        /// <returns>True si forcé avec succès</returns>
        public bool ForceEarnBadge(string playerName, string miniGameId, string badgeId, float overrideValue = 0f)
        {
            LogDebug($"[FORCE] Attribution forcée du badge {miniGameId}_{badgeId} à {playerName}");
            
            var oldValidation = validateBadgeConditions;
            validateBadgeConditions = false;
            
            bool result = TryEarnBadge(playerName, miniGameId, badgeId, overrideValue);
            
            validateBadgeConditions = oldValidation;
            
            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialise le système
        /// </summary>
        private void InitializeSystem()
        {
            // Auto-trouver les composants si non assignés
            if (badgeDatabase == null)
            {
                var dbObjects = Resources.FindObjectsOfTypeAll<GlobalBadgeDatabase>();
                if (dbObjects.Length > 0)
                {
                    badgeDatabase = dbObjects[0];
                    LogDebug("GlobalBadgeDatabase trouvée automatiquement");
                }
            }

            if (playerStorage == null)
            {
                playerStorage = FindFirstObjectByType<GlobalPlayerBadgeStorage>();
                if (playerStorage == null)
                {
                    // Créer automatiquement si pas trouvé
                    var storageGO = new GameObject("GlobalPlayerBadgeStorage");
                    playerStorage = storageGO.AddComponent<GlobalPlayerBadgeStorage>();
                    LogDebug("GlobalPlayerBadgeStorage créé automatiquement");
                }
            }
        }

        /// <summary>
        /// Valide la configuration du système
        /// </summary>
        private void ValidateConfiguration()
        {
            if (badgeDatabase == null)
            {
                LogError("GlobalBadgeDatabase non assignée!");
                return;
            }

            if (playerStorage == null)
            {
                LogError("GlobalPlayerBadgeStorage non trouvé!");
                return;
            }

            LogSuccess("GlobalBadgeSystem initialisé avec succès");
        }

        /// <summary>
        /// Valide les paramètres d'entrée
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="badgeId">ID du badge</param>
        /// <returns>True si valide</returns>
        private bool ValidateInput(string playerName, string miniGameId, string badgeId)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                LogError("Nom de joueur vide");
                return false;
            }

            if (string.IsNullOrEmpty(miniGameId))
            {
                LogError("ID de mini-jeu vide");
                return false;
            }

            if (string.IsNullOrEmpty(badgeId))
            {
                LogError("ID de badge vide");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Vérifie si une validation est en cooldown
        /// </summary>
        /// <param name="key">Clé de validation</param>
        /// <returns>True si en cooldown</returns>
        private bool IsInCooldown(string key)
        {
            if (lastValidationTime.TryGetValue(key, out float lastTime))
            {
                return (Time.time - lastTime) < validationCooldown;
            }
            return false;
        }

        /// <summary>
        /// Valide les conditions d'obtention d'un badge
        /// </summary>
        /// <param name="badgeDefinition">Définition du badge</param>
        /// <param name="currentValue">Valeur actuelle</param>
        /// <returns>True si conditions remplies</returns>
        private bool ValidateBadgeConditions(BadgeDefinition badgeDefinition, float currentValue)
        {
            return currentValue >= badgeDefinition.TargetValue;
        }

        /// <summary>
        /// Crée une instance de badge
        /// </summary>
        /// <param name="definition">Définition du badge</param>
        /// <param name="value">Valeur obtenue</param>
        /// <returns>Instance du badge</returns>
        private BadgeInstance CreateBadgeInstance(BadgeDefinition definition, float value)
        {
            return new BadgeInstance(definition, value);
        }

        #endregion

        #region Logging

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[GlobalBadgeSystem] {message}");
        }

        private void LogSuccess(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[GlobalBadgeSystem] ✓ {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GlobalBadgeSystem] ✗ {message}");
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// Teste l'attribution d'un badge en mode debug
        /// </summary>
        [ContextMenu("Test Badge Attribution")]
        public void TestBadgeAttribution()
        {
            if (badgeDatabase == null)
            {
                LogError("Aucune base de données de badges");
                return;
            }

            // Tester avec le premier badge disponible
            var allBadges = badgeDatabase.GetAllBadges();
            if (allBadges.Count > 0)
            {
                var testBadge = allBadges[0];
                bool success = TryEarnBadge("TestPlayer", testBadge.MiniGameId, testBadge.BadgeId, testBadge.TargetValue);
                LogDebug($"Test d'attribution: {(success ? "SUCCÈS" : "ÉCHEC")}");
            }
        }

        /// <summary>
        /// Affiche les statistiques du système
        /// </summary>
        [ContextMenu("Show System Statistics")]
        public void ShowSystemStatistics()
        {
            if (badgeDatabase == null || playerStorage == null)
            {
                LogError("Système non configuré");
                return;
            }

            Debug.Log("=== STATISTIQUES DU SYSTÈME DE BADGES ===");
            Debug.Log($"Badges totaux en base: {badgeDatabase.GetAllBadges().Count}");
            
            var gameIds = badgeDatabase.GetAllGameIds();
            foreach (var gameId in gameIds)
            {
                var badges = badgeDatabase.GetBadgesForGame(gameId);
                Debug.Log($"• {gameId}: {badges.Count} badge(s)");
            }
            
            var players = playerStorage.GetPlayersWithBadges();
            Debug.Log($"Joueurs avec badges: {players.Count}");
        }
#endif

        #endregion
    }
}
