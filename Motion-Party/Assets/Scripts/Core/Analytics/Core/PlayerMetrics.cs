using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Analytics.Interfaces;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Implémentation des métriques de joueur en temps réel
    /// </summary>
    [Serializable]
    public class PlayerMetrics : IPlayerMetrics
    {
        [Header("Player Info")]
        [SerializeField] private string playerId;
        
        [Header("Session Metrics")]
        [SerializeField] private Dictionary<string, float> sessionMetrics;
        
        [Header("Performance Tracking")]
        [SerializeField] private float lastUpdateTime;
        [SerializeField] private int totalActions;
        [SerializeField] private float sessionStartTime;

        // Métriques standard
        private const string SCORE_METRIC = "score";
        private const string ACCURACY_METRIC = "accuracy";
        private const string REACTION_TIME_METRIC = "avg_reaction_time";
        private const string COMPLETION_TIME_METRIC = "completion_time";
        private const string ACTIONS_COUNT_METRIC = "actions_count";
        private const string SUCCESS_RATE_METRIC = "success_rate";

        public string PlayerId => playerId;

        public PlayerMetrics(string id)
        {
            playerId = id;
            sessionMetrics = new Dictionary<string, float>();
            sessionStartTime = Time.time;
            InitializeStandardMetrics();
        }

        #region IPlayerMetrics Implementation

        public float GetMetric(string metricName)
        {
            return sessionMetrics.TryGetValue(metricName, out float value) ? value : 0f;
        }

        public void SetMetric(string metricName, float value)
        {
            sessionMetrics[metricName] = value;
            lastUpdateTime = Time.time;
            
            DebugLog($"Métrique '{metricName}' mise à jour: {value:F2}");
        }

        public void IncrementMetric(string metricName, float amount = 1f)
        {
            float currentValue = GetMetric(metricName);
            SetMetric(metricName, currentValue + amount);
        }

        public Dictionary<string, float> GetAllMetrics()
        {
            return new Dictionary<string, float>(sessionMetrics);
        }

        public void ResetMetrics()
        {
            sessionMetrics.Clear();
            InitializeStandardMetrics();
            sessionStartTime = Time.time;
            totalActions = 0;
            
            DebugLog("Métriques réinitialisées");
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Réinitialise uniquement les métriques de session, pas les totaux
        /// </summary>
        public void ResetSessionMetrics()
        {
            var persistentMetrics = new Dictionary<string, float>();
            
            // Garder certaines métriques entre les sessions si nécessaire
            // (peut être étendu selon les besoins)
            
            sessionMetrics.Clear();
            InitializeStandardMetrics();
            
            // Restaurer les métriques persistantes
            foreach (var metric in persistentMetrics)
            {
                sessionMetrics[metric.Key] = metric.Value;
            }
            
            sessionStartTime = Time.time;
            totalActions = 0;
        }

        /// <summary>
        /// Obtient la durée de la session actuelle
        /// </summary>
        public float GetSessionDuration()
        {
            return Time.time - sessionStartTime;
        }

        #endregion

        #region Standard Metrics Helpers

        /// <summary>
        /// Ajoute des points au score
        /// </summary>
        public void AddScore(float points)
        {
            IncrementMetric(SCORE_METRIC, points);
        }

        /// <summary>
        /// Met à jour la précision moyenne
        /// </summary>
        public void UpdateAccuracy(float accuracy)
        {
            float currentAccuracy = GetMetric(ACCURACY_METRIC);
            float currentCount = GetMetric(ACTIONS_COUNT_METRIC);
            
            if (currentCount == 0)
            {
                SetMetric(ACCURACY_METRIC, accuracy);
            }
            else
            {
                float newAccuracy = (currentAccuracy * currentCount + accuracy) / (currentCount + 1);
                SetMetric(ACCURACY_METRIC, newAccuracy);
            }
            
            IncrementMetric(ACTIONS_COUNT_METRIC);
            totalActions++;
        }

        /// <summary>
        /// Met à jour le temps de réaction moyen
        /// </summary>
        public void UpdateReactionTime(float reactionTime)
        {
            float currentReactionTime = GetMetric(REACTION_TIME_METRIC);
            float currentCount = GetMetric(ACTIONS_COUNT_METRIC);
            
            if (currentCount == 0)
            {
                SetMetric(REACTION_TIME_METRIC, reactionTime);
            }
            else
            {
                float newReactionTime = (currentReactionTime * currentCount + reactionTime) / (currentCount + 1);
                SetMetric(REACTION_TIME_METRIC, newReactionTime);
            }
        }

        /// <summary>
        /// Met à jour le taux de succès
        /// </summary>
        public void UpdateSuccessRate(bool wasSuccessful)
        {
            float currentSuccesses = GetMetric("total_successes");
            float currentAttempts = GetMetric("total_attempts");
            
            IncrementMetric("total_attempts");
            if (wasSuccessful)
            {
                IncrementMetric("total_successes");
                currentSuccesses++;
            }
            
            currentAttempts++;
            float successRate = currentAttempts > 0 ? currentSuccesses / currentAttempts : 0f;
            SetMetric(SUCCESS_RATE_METRIC, successRate);
        }

        /// <summary>
        /// Définit le temps de completion
        /// </summary>
        public void SetCompletionTime(float completionTime)
        {
            SetMetric(COMPLETION_TIME_METRIC, completionTime);
        }

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Calcule la performance globale du joueur (0-1)
        /// </summary>
        public float CalculateOverallPerformance()
        {
            float score = Mathf.Clamp01(GetMetric(SCORE_METRIC) / 1000f); // Normaliser sur 1000 points
            float accuracy = Mathf.Clamp01(GetMetric(ACCURACY_METRIC) / 100f); // Accuracy en %
            float reactionTime = Mathf.Clamp01(1f - GetMetric(REACTION_TIME_METRIC) / 5f); // Inverse, 5s = 0
            float successRate = GetMetric(SUCCESS_RATE_METRIC);
            
            // Moyenne pondérée
            return (score * 0.3f + accuracy * 0.3f + reactionTime * 0.2f + successRate * 0.2f);
        }

        /// <summary>
        /// Obtient les métriques de performance formatées
        /// </summary>
        public Dictionary<string, string> GetFormattedMetrics()
        {
            var formatted = new Dictionary<string, string>();
            
            foreach (var metric in sessionMetrics)
            {
                string formattedValue = FormatMetricValue(metric.Key, metric.Value);
                formatted[metric.Key] = formattedValue;
            }
            
            return formatted;
        }

        #endregion

        #region Private Methods

        private void InitializeStandardMetrics()
        {
            sessionMetrics[SCORE_METRIC] = 0f;
            sessionMetrics[ACCURACY_METRIC] = 0f;
            sessionMetrics[REACTION_TIME_METRIC] = 0f;
            sessionMetrics[COMPLETION_TIME_METRIC] = 0f;
            sessionMetrics[ACTIONS_COUNT_METRIC] = 0f;
            sessionMetrics[SUCCESS_RATE_METRIC] = 0f;
            sessionMetrics["total_successes"] = 0f;
            sessionMetrics["total_attempts"] = 0f;
        }

        private string FormatMetricValue(string metricName, float value)
        {
            switch (metricName)
            {
                case SCORE_METRIC:
                    return $"{value:F0} pts";
                case ACCURACY_METRIC:
                case SUCCESS_RATE_METRIC:
                    return $"{value:F1}%";
                case REACTION_TIME_METRIC:
                case COMPLETION_TIME_METRIC:
                    return $"{value:F2}s";
                case ACTIONS_COUNT_METRIC:
                    return $"{value:F0}";
                default:
                    return $"{value:F2}";
            }
        }

        private void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"[PlayerMetrics-{playerId}] {message}");
        }

        #endregion

        #region Custom Metrics Extensions

        /// <summary>
        /// Ajoute une métrique personnalisée avec validation
        /// </summary>
        public void AddCustomMetric(string metricName, float value, bool canOverwrite = true)
        {
            if (string.IsNullOrEmpty(metricName))
            {
                UnityEngine.Debug.LogWarning($"[PlayerMetrics-{playerId}] Nom de métrique vide");
                return;
            }

            if (!canOverwrite && sessionMetrics.ContainsKey(metricName))
            {
                UnityEngine.Debug.LogWarning($"[PlayerMetrics-{playerId}] Métrique '{metricName}' existe déjà");
                return;
            }

            SetMetric(metricName, value);
        }

        /// <summary>
        /// Supprime une métrique personnalisée
        /// </summary>
        public bool RemoveCustomMetric(string metricName)
        {
            if (IsStandardMetric(metricName))
            {
                UnityEngine.Debug.LogWarning($"[PlayerMetrics-{playerId}] Impossible de supprimer la métrique standard '{metricName}'");
                return false;
            }

            return sessionMetrics.Remove(metricName);
        }

        private bool IsStandardMetric(string metricName)
        {
            return metricName == SCORE_METRIC || 
                   metricName == ACCURACY_METRIC || 
                   metricName == REACTION_TIME_METRIC || 
                   metricName == COMPLETION_TIME_METRIC || 
                   metricName == ACTIONS_COUNT_METRIC || 
                   metricName == SUCCESS_RATE_METRIC ||
                   metricName == "total_successes" ||
                   metricName == "total_attempts";
        }

        #endregion
    }
}
