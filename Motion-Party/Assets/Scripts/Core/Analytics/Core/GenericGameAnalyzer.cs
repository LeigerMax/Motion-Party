using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Analytics.Interfaces;
using Core.Analytics.Data;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Analyseur de jeu générique pour tous les mini-jeux
    /// </summary>
    public class GenericGameAnalyzer : IGameAnalyzer
    {
        public string GameId { get; private set; }
        public bool IsSessionActive { get; private set; }

        private List<GameAnalyticsEvent> sessionEvents;
        private List<string> activePlayerIds;
        private DateTime sessionStartTime;
        private DateTime sessionEndTime;

        public GenericGameAnalyzer(string gameId)
        {
            GameId = gameId;
            sessionEvents = new List<GameAnalyticsEvent>();
            activePlayerIds = new List<string>();
        }

        #region IGameAnalyzer Implementation

        public void StartSession(List<string> playerIds)
        {
            if (IsSessionActive)
            {
                EndSession();
            }

            activePlayerIds = new List<string>(playerIds);
            sessionEvents.Clear();
            sessionStartTime = DateTime.Now;
            IsSessionActive = true;

            UnityEngine.Debug.Log($"[GenericGameAnalyzer-{GameId}] Session démarrée avec {playerIds.Count} joueur(s)");
        }

        public void EndSession()
        {
            if (!IsSessionActive) return;

            sessionEndTime = DateTime.Now;
            IsSessionActive = false;

            UnityEngine.Debug.Log($"[GenericGameAnalyzer-{GameId}] Session terminée - {sessionEvents.Count} événements enregistrés");
        }

        public void RecordEvent(GameAnalyticsEvent gameEvent)
        {
            if (!IsSessionActive) return;

            sessionEvents.Add(gameEvent);
            
            UnityEngine.Debug.Log($"[GenericGameAnalyzer-{GameId}] Événement enregistré: {gameEvent.eventType} pour {gameEvent.playerId}");
        }

        public SessionAnalysisResult AnalyzeSession()
        {
            if (!IsSessionActive && sessionEvents.Count == 0)
                return new SessionAnalysisResult();

            var result = new SessionAnalysisResult
            {
                gameId = GameId,
                playerIds = new List<string>(activePlayerIds),
                sessionStart = sessionStartTime,
                sessionEnd = IsSessionActive ? DateTime.Now : sessionEndTime,
                totalDuration = (float)(DateTime.Now - sessionStartTime).TotalSeconds
            };

            // Analyser chaque joueur
            foreach (string playerId in activePlayerIds)
            {
                var playerSummary = AnalyzePlayerPerformance(playerId);
                result.playerSummaries[playerId] = playerSummary;
            }

            // Générer des insights globaux
            result.insights.AddRange(GenerateSessionInsights());

            // Analyser les comparaisons multijoueur
            if (activePlayerIds.Count > 1)
            {
                result.playerComparison = AnalyzePlayerComparisons();
            }

            return result;
        }

        #endregion

        #region Player Analysis

        private PlayerPerformanceSummary AnalyzePlayerPerformance(string playerId)
        {
            var summary = new PlayerPerformanceSummary(playerId);
            var playerEvents = sessionEvents.Where(e => e.playerId == playerId).ToList();

            if (playerEvents.Count == 0)
            {
                summary.overallPerformance = 0f;
                return summary;
            }

            // Calculer les métriques de base
            CalculateBasicMetrics(summary, playerEvents);

            // Calculer les métriques avancées
            CalculateAdvancedMetrics(summary, playerEvents);

            // Déterminer les réalisations
            DetermineAchievements(summary);

            // Identifier les améliorations possibles
            IdentifyImprovements(summary);

            // Calculer la performance globale
            summary.overallPerformance = CalculateOverallPerformance(summary.metrics);

            return summary;
        }

        private void CalculateBasicMetrics(PlayerPerformanceSummary summary, List<GameAnalyticsEvent> playerEvents)
        {
            var metrics = summary.metrics;

            // Compter les événements par type
            metrics["total_events"] = playerEvents.Count;
            metrics["actions_performed"] = playerEvents.Count(e => e.eventType == GameAnalyticsEventType.ActionPerformed);
            metrics["objectives_completed"] = playerEvents.Count(e => e.eventType == GameAnalyticsEventType.ObjectiveCompleted);
            metrics["objectives_failed"] = playerEvents.Count(e => e.eventType == GameAnalyticsEventType.ObjectiveFailed);

            // Score total
            var scoreEvents = playerEvents.Where(e => e.score > 0).ToList();
            metrics["total_score"] = scoreEvents.Sum(e => e.score);
            metrics["max_score"] = scoreEvents.Count > 0 ? scoreEvents.Max(e => e.score) : 0;

            // Précision moyenne
            var accuracyEvents = playerEvents.Where(e => e.accuracy > 0).ToList();
            if (accuracyEvents.Count > 0)
            {
                metrics["average_accuracy"] = accuracyEvents.Average(e => e.accuracy);
                metrics["max_accuracy"] = accuracyEvents.Max(e => e.accuracy);
            }

            // Temps de réaction
            var reactionEvents = playerEvents.Where(e => e.reactionTime > 0).ToList();
            if (reactionEvents.Count > 0)
            {
                metrics["average_reaction_time"] = reactionEvents.Average(e => e.reactionTime);
                metrics["min_reaction_time"] = reactionEvents.Min(e => e.reactionTime);
                metrics["max_reaction_time"] = reactionEvents.Max(e => e.reactionTime);
            }
        }

        private void CalculateAdvancedMetrics(PlayerPerformanceSummary summary, List<GameAnalyticsEvent> playerEvents)
        {
            var metrics = summary.metrics;

            // Taux de succès
            float completedObjectives = metrics.GetValueOrDefault("objectives_completed", 0);
            float failedObjectives = metrics.GetValueOrDefault("objectives_failed", 0);
            float totalObjectives = completedObjectives + failedObjectives;
            
            if (totalObjectives > 0)
            {
                metrics["success_rate"] = (completedObjectives / totalObjectives) * 100f;
            }

            // Consistance (écart-type des performances)
            var performanceValues = playerEvents.Where(e => e.value > 0).Select(e => e.value).ToList();
            if (performanceValues.Count > 1)
            {
                float avg = performanceValues.Average();
                float variance = performanceValues.Select(v => Mathf.Pow(v - avg, 2)).Average();
                metrics["performance_consistency"] = 100f - (Mathf.Sqrt(variance) / avg * 100f); // Inverse de CV%
            }

            // Progression (amélioration au cours de la session)
            CalculateProgressionMetrics(summary, playerEvents);

            // Efficacité (score par action)
            float totalActions = metrics.GetValueOrDefault("actions_performed", 0);
            float totalScore = metrics.GetValueOrDefault("total_score", 0);
            if (totalActions > 0)
            {
                metrics["efficiency"] = totalScore / totalActions;
            }
        }

        private void CalculateProgressionMetrics(PlayerPerformanceSummary summary, List<GameAnalyticsEvent> playerEvents)
        {
            var orderedEvents = playerEvents.OrderBy(e => e.timestamp).ToList();
            if (orderedEvents.Count < 4) return; // Pas assez de données pour analyser la progression

            // Diviser la session en segments
            int segmentSize = orderedEvents.Count / 3;
            var firstSegment = orderedEvents.Take(segmentSize).ToList();
            var lastSegment = orderedEvents.Skip(orderedEvents.Count - segmentSize).ToList();

            // Comparer les performances du début et de la fin
            float firstSegmentPerf = CalculateSegmentPerformance(firstSegment);
            float lastSegmentPerf = CalculateSegmentPerformance(lastSegment);

            if (firstSegmentPerf > 0)
            {
                float progressionRate = ((lastSegmentPerf - firstSegmentPerf) / firstSegmentPerf) * 100f;
                summary.metrics["progression_rate"] = progressionRate;
            }
        }

        private float CalculateSegmentPerformance(List<GameAnalyticsEvent> segmentEvents)
        {
            if (segmentEvents.Count == 0) return 0f;

            float score = segmentEvents.Where(e => e.score > 0).Sum(e => e.score);
            float accuracy = segmentEvents.Where(e => e.accuracy > 0).DefaultIfEmpty().Average(e => e.accuracy);
            float actionCount = segmentEvents.Count(e => e.eventType == GameAnalyticsEventType.ActionPerformed);

            return (score + accuracy * 10f) / Mathf.Max(actionCount, 1f);
        }

        #endregion

        #region Achievements and Improvements

        private void DetermineAchievements(PlayerPerformanceSummary summary)
        {
            var metrics = summary.metrics;
            var achievements = summary.achievements;

            // Score élevé
            if (metrics.GetValueOrDefault("total_score", 0) >= 1000)
                achievements.Add("Score Millénaire");

            // Précision excellente
            if (metrics.GetValueOrDefault("average_accuracy", 0) >= 90)
                achievements.Add("Maître de la Précision");

            // Réactivité exceptionnelle
            if (metrics.GetValueOrDefault("average_reaction_time", 999) <= 1.0f)
                achievements.Add("Réflexes d'Élite");

            // Taux de succès parfait
            if (metrics.GetValueOrDefault("success_rate", 0) >= 95)
                achievements.Add("Quasi Perfection");

            // Consistance remarquable
            if (metrics.GetValueOrDefault("performance_consistency", 0) >= 85)
                achievements.Add("Régularité Exemplaire");

            // Progression forte
            if (metrics.GetValueOrDefault("progression_rate", -100) >= 20)
                achievements.Add("Amélioration Rapide");

            // Efficacité élevée
            if (metrics.GetValueOrDefault("efficiency", 0) >= 50)
                achievements.Add("Efficacité Maximale");
        }

        private void IdentifyImprovements(PlayerPerformanceSummary summary)
        {
            var metrics = summary.metrics;
            var improvements = summary.improvements;

            // Précision à améliorer
            if (metrics.GetValueOrDefault("average_accuracy", 100) < 70)
                improvements.Add("Travailler la précision des actions");

            // Temps de réaction lent
            if (metrics.GetValueOrDefault("average_reaction_time", 0) > 2.5f)
                improvements.Add("Améliorer la rapidité de réaction");

            // Faible taux de succès
            if (metrics.GetValueOrDefault("success_rate", 100) < 60)
                improvements.Add("Se concentrer sur la complétion des objectifs");

            // Inconsistance
            if (metrics.GetValueOrDefault("performance_consistency", 100) < 60)
                improvements.Add("Maintenir un niveau de performance constant");

            // Efficacité faible
            if (metrics.GetValueOrDefault("efficiency", 100) < 20)
                improvements.Add("Optimiser l'efficacité des actions");

            // Progression négative
            if (metrics.GetValueOrDefault("progression_rate", 0) < -10)
                improvements.Add("Maintenir la concentration tout au long de la session");
        }

        #endregion

        #region Session Insights

        private List<AnalysisInsight> GenerateSessionInsights()
        {
            var insights = new List<AnalysisInsight>();

            // Insight sur la durée de session
            float sessionDuration = (float)(DateTime.Now - sessionStartTime).TotalMinutes;
            if (sessionDuration > 10)
            {
                insights.Add(new AnalysisInsight(
                    "Session Prolongée",
                    $"Session de {sessionDuration:F1} minutes - les joueurs montrent de l'engagement",
                    InsightType.SessionTrend,
                    0.8f
                ));
            }

            // Insight sur l'activité générale
            float eventRate = sessionEvents.Count / Math.Max(sessionDuration, 1f);
            if (eventRate > 5)
            {
                insights.Add(new AnalysisInsight(
                    "Activité Intense",
                    $"Rythme d'activité élevé ({eventRate:F1} actions/min)",
                    InsightType.SessionTrend,
                    0.9f
                ));
            }

            return insights;
        }

        #endregion

        #region Player Comparisons

        private PlayerComparisonResult AnalyzePlayerComparisons()
        {
            var result = new PlayerComparisonResult();
            
            if (activePlayerIds.Count < 2) return result;

            var analytics = AnalyticsManager.Instance;
            if (analytics == null) return result;

            var comparator = new PlayerComparator();

            // Classements par métrique
            var metrics = new[] { "score", "accuracy", "avg_reaction_time", "success_rate" };
            foreach (string metric in metrics)
            {
                var rankings = comparator.RankPlayers(activePlayerIds, GameId, metric);
                result.rankings.AddRange(rankings);
            }

            // Écarts de performance
            for (int i = 0; i < activePlayerIds.Count; i++)
            {
                for (int j = i + 1; j < activePlayerIds.Count; j++)
                {
                    string player1 = activePlayerIds[i];
                    string player2 = activePlayerIds[j];
                    
                    var comparison = comparator.ComparePlayer(player1, new List<string> { player2 }, GameId);
                    result.insights.AddRange(comparison.insights);
                }
            }

            return result;
        }

        #endregion

        #region Utility Methods

        private float CalculateOverallPerformance(Dictionary<string, float> metrics)
        {
            float score = NormalizeMetric(metrics.GetValueOrDefault("total_score", 0), 0, 1000);
            float accuracy = NormalizeMetric(metrics.GetValueOrDefault("average_accuracy", 0), 0, 100);
            float successRate = NormalizeMetric(metrics.GetValueOrDefault("success_rate", 0), 0, 100);
            float efficiency = NormalizeMetric(metrics.GetValueOrDefault("efficiency", 0), 0, 100);

            // Réaction time (inverse - plus bas = meilleur)
            float reactionTime = metrics.GetValueOrDefault("average_reaction_time", 0);
            float normalizedReactionTime = reactionTime > 0 ? 1f - NormalizeMetric(reactionTime, 0.5f, 3f) : 0f;

            // Moyenne pondérée
            return (score * 0.25f + accuracy * 0.25f + successRate * 0.2f + 
                   normalizedReactionTime * 0.15f + efficiency * 0.15f);
        }

        private float NormalizeMetric(float value, float min, float max)
        {
            return Mathf.Clamp01((value - min) / (max - min));
        }

        #endregion
    }
}

// Extension pour Dictionary
public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
    }
}
