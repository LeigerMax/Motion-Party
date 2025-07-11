using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Tracker global pour suivre les performances des joueurs et déclencher l'attribution automatique de badges
    /// Compatible avec tous les mini-jeux de Motion-Party
    /// </summary>
    public class GlobalBadgeTracker : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private GlobalBadgeSystem badgeSystem;
        [SerializeField] private bool enableAutoTracking = true;
        [SerializeField] private bool enableRealTimeValidation = true;
        [SerializeField] private float validationInterval = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showTrackingLogs = true;
        [SerializeField] private bool showValidationDetails = false;

        // Données de suivi par joueur et par jeu
        private Dictionary<string, Dictionary<string, PlayerGameData>> playerGameTracking;

        // Timer pour validation périodique
        private float lastValidationTime;

        // Singleton
        private static GlobalBadgeTracker _instance;
        public static GlobalBadgeTracker Instance => _instance;

        #endregion

        #region Events

        /// <summary>
        /// Événement déclenché quand une métrique est mise à jour
        /// </summary>
        public static Action<string, string, string, float> OnMetricUpdated;

        /// <summary>
        /// Événement déclenché quand une validation automatique est effectuée
        /// </summary>
        public static Action<string, string, int> OnAutoValidationPerformed;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            // Singleton
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTracker();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (enableRealTimeValidation && enableAutoTracking)
            {
                if (Time.time - lastValidationTime >= validationInterval)
                {
                    PerformPeriodicValidation();
                    lastValidationTime = Time.time;
                }
            }
        }

        #endregion

        #region Public Tracking Methods

        /// <summary>
        /// Met à jour une métrique pour un joueur dans un jeu spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="metricName">Nom de la métrique</param>
        /// <param name="value">Nouvelle valeur</param>
        /// <param name="operation">Type d'opération (Set, Add, Max)</param>
        public void UpdateMetric(string playerName, string miniGameId, string metricName, float value, MetricOperation operation = MetricOperation.Set)
        {
            if (!ValidateTrackingInput(playerName, miniGameId, metricName))
                return;

            var gameData = GetOrCreatePlayerGameData(playerName, miniGameId);
            float oldValue = gameData.GetMetric(metricName);
            float newValue = ApplyMetricOperation(oldValue, value, operation);
            
            gameData.SetMetric(metricName, newValue);
            
            LogTracking($"Métrique mise à jour: {playerName}.{miniGameId}.{metricName} = {newValue} (était {oldValue})");
            OnMetricUpdated?.Invoke(playerName, miniGameId, metricName, newValue);

            // Validation immédiate si activée
            if (enableRealTimeValidation)
            {
                ValidatePlayerMetricsForGame(playerName, miniGameId);
            }
        }

        /// <summary>
        /// Incrémente une métrique d'une unité
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="metricName">Nom de la métrique</param>
        public void IncrementMetric(string playerName, string miniGameId, string metricName)
        {
            UpdateMetric(playerName, miniGameId, metricName, 1f, MetricOperation.Add);
        }

        /// <summary>
        /// Ajoute une valeur à une métrique existante
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="metricName">Nom de la métrique</param>
        /// <param name="addValue">Valeur à ajouter</param>
        public void AddToMetric(string playerName, string miniGameId, string metricName, float addValue)
        {
            UpdateMetric(playerName, miniGameId, metricName, addValue, MetricOperation.Add);
        }

        /// <summary>
        /// Met à jour une métrique seulement si la nouvelle valeur est supérieure
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="metricName">Nom de la métrique</param>
        /// <param name="value">Nouvelle valeur potentielle</param>
        public void UpdateMaxMetric(string playerName, string miniGameId, string metricName, float value)
        {
            UpdateMetric(playerName, miniGameId, metricName, value, MetricOperation.Max);
        }

        /// <summary>
        /// Obtient la valeur d'une métrique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="metricName">Nom de la métrique</param>
        /// <returns>Valeur de la métrique</returns>
        public float GetMetric(string playerName, string miniGameId, string metricName)
        {
            var gameData = GetPlayerGameData(playerName, miniGameId);
            return gameData?.GetMetric(metricName) ?? 0f;
        }

        /// <summary>
        /// Obtient toutes les métriques d'un joueur pour un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Dictionnaire des métriques</returns>
        public Dictionary<string, float> GetAllMetrics(string playerName, string miniGameId)
        {
            var gameData = GetPlayerGameData(playerName, miniGameId);
            return gameData?.GetAllMetrics() ?? new Dictionary<string, float>();
        }

        /// <summary>
        /// Remet à zéro toutes les métriques d'un joueur pour un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        public void ResetPlayerGameMetrics(string playerName, string miniGameId)
        {
            var gameData = GetPlayerGameData(playerName, miniGameId);
            gameData?.ResetAllMetrics();
            LogTracking($"Métriques réinitialisées pour {playerName} dans {miniGameId}");
        }

        /// <summary>
        /// Remet à zéro toutes les métriques d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void ResetPlayerAllMetrics(string playerName)
        {
            if (playerGameTracking.ContainsKey(playerName))
            {
                foreach (var gameData in playerGameTracking[playerName].Values)
                {
                    gameData.ResetAllMetrics();
                }
                LogTracking($"Toutes les métriques réinitialisées pour {playerName}");
            }
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Force la validation de tous les badges pour un joueur dans un jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <returns>Nombre de badges attribués</returns>
        public int ValidatePlayerMetricsForGame(string playerName, string miniGameId)
        {
            if (badgeSystem == null)
                return 0;

            int badgesEarned = 0;
            var availableBadges = badgeSystem.GetEarnableBadgesForPlayer(playerName, miniGameId);
            
            foreach (var badgeDefinition in availableBadges)
            {
                // Obtenir la métrique correspondante
                string metricName = GetMetricNameForBadge(badgeDefinition);
                float currentValue = GetMetric(playerName, miniGameId, metricName);
                
                // Tenter d'attribuer le badge
                if (badgeSystem.TryEarnBadge(playerName, miniGameId, badgeDefinition.BadgeId, currentValue))
                {
                    badgesEarned++;
                    LogValidation($"Badge automatique attribué: {badgeDefinition.GetFullBadgeId()} à {playerName}");
                }
            }

            if (badgesEarned > 0)
            {
                OnAutoValidationPerformed?.Invoke(playerName, miniGameId, badgesEarned);
            }

            return badgesEarned;
        }

        /// <summary>
        /// Force la validation de tous les badges pour tous les joueurs
        /// </summary>
        /// <returns>Nombre total de badges attribués</returns>
        public int ValidateAllPlayers()
        {
            int totalBadges = 0;
            
            foreach (var playerName in playerGameTracking.Keys)
            {
                foreach (var miniGameId in playerGameTracking[playerName].Keys)
                {
                    totalBadges += ValidatePlayerMetricsForGame(playerName, miniGameId);
                }
            }
            
            LogValidation($"Validation globale terminée: {totalBadges} badge(s) attribué(s)");
            return totalBadges;
        }

        #endregion

        #region Common Tracking Helpers

        /// <summary>
        /// Met à jour le score d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="score">Nouveau score</param>
        public void UpdateScore(string playerName, string miniGameId, float score)
        {
            UpdateMaxMetric(playerName, miniGameId, "score", score);
        }

        /// <summary>
        /// Met à jour le temps d'un joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="time">Temps en secondes</param>
        public void UpdateTime(string playerName, string miniGameId, float time)
        {
            UpdateMetric(playerName, miniGameId, "time", time);
        }

        /// <summary>
        /// Met à jour un compteur (actions, collectes, etc.)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="counterName">Nom du compteur</param>
        /// <param name="count">Nouvelle valeur</param>
        public void UpdateCounter(string playerName, string miniGameId, string counterName, int count)
        {
            UpdateMetric(playerName, miniGameId, counterName, count);
        }

        /// <summary>
        /// Incrémente un compteur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="counterName">Nom du compteur</param>
        public void IncrementCounter(string playerName, string miniGameId, string counterName)
        {
            IncrementMetric(playerName, miniGameId, counterName);
        }

        /// <summary>
        /// Met à jour une précision (pourcentage entre 0 et 100)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="miniGameId">ID du mini-jeu</param>
        /// <param name="accuracy">Précision en pourcentage</param>
        public void UpdateAccuracy(string playerName, string miniGameId, float accuracy)
        {
            UpdateMaxMetric(playerName, miniGameId, "accuracy", Mathf.Clamp(accuracy, 0f, 100f));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialise le tracker
        /// </summary>
        private void InitializeTracker()
        {
            playerGameTracking = new Dictionary<string, Dictionary<string, PlayerGameData>>();
            
            // Auto-trouver le système de badges
            if (badgeSystem == null)
            {
                badgeSystem = FindFirstObjectByType<GlobalBadgeSystem>();
                if (badgeSystem == null)
                {
                    LogTracking("⚠️ GlobalBadgeSystem non trouvé - tracking sans validation automatique");
                }
            }
            
            LogTracking("GlobalBadgeTracker initialisé");
        }

        /// <summary>
        /// Valide les paramètres d'entrée pour le tracking
        /// </summary>
        private bool ValidateTrackingInput(string playerName, string miniGameId, string metricName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("[GlobalBadgeTracker] Nom de joueur vide");
                return false;
            }

            if (string.IsNullOrEmpty(miniGameId))
            {
                Debug.LogError("[GlobalBadgeTracker] ID de mini-jeu vide");
                return false;
            }

            if (string.IsNullOrEmpty(metricName))
            {
                Debug.LogError("[GlobalBadgeTracker] Nom de métrique vide");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtient ou crée les données de jeu d'un joueur
        /// </summary>
        private PlayerGameData GetOrCreatePlayerGameData(string playerName, string miniGameId)
        {
            if (!playerGameTracking.ContainsKey(playerName))
            {
                playerGameTracking[playerName] = new Dictionary<string, PlayerGameData>();
            }

            if (!playerGameTracking[playerName].ContainsKey(miniGameId))
            {
                playerGameTracking[playerName][miniGameId] = new PlayerGameData(playerName, miniGameId);
            }

            return playerGameTracking[playerName][miniGameId];
        }

        /// <summary>
        /// Obtient les données de jeu d'un joueur (peut retourner null)
        /// </summary>
        private PlayerGameData GetPlayerGameData(string playerName, string miniGameId)
        {
            if (playerGameTracking.ContainsKey(playerName) && 
                playerGameTracking[playerName].ContainsKey(miniGameId))
            {
                return playerGameTracking[playerName][miniGameId];
            }
            return null;
        }

        /// <summary>
        /// Applique une opération sur une métrique
        /// </summary>
        private float ApplyMetricOperation(float oldValue, float newValue, MetricOperation operation)
        {
            switch (operation)
            {
                case MetricOperation.Set:
                    return newValue;
                case MetricOperation.Add:
                    return oldValue + newValue;
                case MetricOperation.Max:
                    return Mathf.Max(oldValue, newValue);
                default:
                    return newValue;
            }
        }

        /// <summary>
        /// Détermine le nom de métrique pour un badge
        /// </summary>
        private string GetMetricNameForBadge(BadgeDefinition badgeDefinition)
        {
            // Mapping basé sur la catégorie du badge
            switch (badgeDefinition.Category)
            {
                case BadgeCategory.Performance:
                    return "score";
                case BadgeCategory.Precision:
                    return "accuracy";
                case BadgeCategory.Exploration:
                    return "collected";
                case BadgeCategory.Endurance:
                    return "duration";
                case BadgeCategory.Special:
                default:
                    return badgeDefinition.BadgeId.ToLower();
            }
        }

        /// <summary>
        /// Effectue une validation périodique
        /// </summary>
        private void PerformPeriodicValidation()
        {
            if (!enableAutoTracking || badgeSystem == null)
                return;

            int totalValidated = 0;
            foreach (var playerName in playerGameTracking.Keys)
            {
                foreach (var miniGameId in playerGameTracking[playerName].Keys)
                {
                    totalValidated += ValidatePlayerMetricsForGame(playerName, miniGameId);
                }
            }

            if (totalValidated > 0 && showValidationDetails)
            {
                LogValidation($"Validation périodique: {totalValidated} badge(s) attribué(s)");
            }
        }

        #endregion

        #region Logging

        private void LogTracking(string message)
        {
            if (showTrackingLogs)
                Debug.Log($"[GlobalBadgeTracker] {message}");
        }

        private void LogValidation(string message)
        {
            if (showValidationDetails)
                Debug.Log($"[GlobalBadgeTracker] 🎖️ {message}");
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Types d'opérations sur les métriques
        /// </summary>
        public enum MetricOperation
        {
            Set,    // Remplace la valeur
            Add,    // Ajoute à la valeur existante
            Max     // Prend le maximum entre l'ancienne et la nouvelle valeur
        }

        /// <summary>
        /// Données de tracking pour un joueur dans un jeu spécifique
        /// </summary>
        [Serializable]
        public class PlayerGameData
        {
            public string playerName;
            public string miniGameId;
            public Dictionary<string, float> metrics;
            public float lastUpdateTime;

            public PlayerGameData(string playerName, string miniGameId)
            {
                this.playerName = playerName;
                this.miniGameId = miniGameId;
                this.metrics = new Dictionary<string, float>();
                this.lastUpdateTime = Time.time;
            }

            public void SetMetric(string metricName, float value)
            {
                metrics[metricName] = value;
                lastUpdateTime = Time.time;
            }

            public float GetMetric(string metricName)
            {
                return metrics.TryGetValue(metricName, out float value) ? value : 0f;
            }

            public Dictionary<string, float> GetAllMetrics()
            {
                return new Dictionary<string, float>(metrics);
            }

            public void ResetAllMetrics()
            {
                metrics.Clear();
                lastUpdateTime = Time.time;
            }
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// Affiche les statistiques de tracking
        /// </summary>
        [ContextMenu("Show Tracking Statistics")]
        public void ShowTrackingStatistics()
        {
            Debug.Log("=== STATISTIQUES DE TRACKING ===");
            Debug.Log($"Joueurs trackés: {playerGameTracking.Count}");
            
            foreach (var playerName in playerGameTracking.Keys)
            {
                Debug.Log($"Joueur: {playerName}");
                foreach (var gameId in playerGameTracking[playerName].Keys)
                {
                    var gameData = playerGameTracking[playerName][gameId];
                    Debug.Log($"  • {gameId}: {gameData.metrics.Count} métrique(s)");
                    foreach (var metric in gameData.metrics)
                    {
                        Debug.Log($"    - {metric.Key}: {metric.Value}");
                    }
                }
            }
        }

        /// <summary>
        /// Teste le tracking avec des données fictives
        /// </summary>
        [ContextMenu("Test Tracking")]
        public void TestTracking()
        {
            UpdateScore("TestPlayer", "firefly", 1500f);
            UpdateTime("TestPlayer", "firefly", 45f);
            IncrementCounter("TestPlayer", "firefly", "collected");
            UpdateAccuracy("TestPlayer", "firefly", 95f);
            
            LogTracking("Test de tracking effectué");
        }
#endif

        #endregion
    }
}
