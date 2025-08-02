using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Analytics.Interfaces;
using Core.Analytics.Data;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Comparateur de performances entre joueurs en temps réel
    /// </summary>
    public class PlayerComparator : IPlayerComparator
    {
        private const float MIN_COMPARISON_THRESHOLD = 0.01f;

        public PlayerComparisonResult ComparePlayer(string playerId, List<string> otherPlayerIds, string gameId)
        {
            var result = new PlayerComparisonResult();
            var analytics = AnalyticsManager.Instance;
            
            if (analytics == null) return result;

            var playerMetrics = analytics.GetPlayerMetrics(playerId);
            if (playerMetrics == null) return result;

            var allPlayerIds = new List<string> { playerId };
            allPlayerIds.AddRange(otherPlayerIds);

            // Générer les classements pour chaque métrique importante
            var importantMetrics = new[] { "score", "accuracy", "avg_reaction_time", "success_rate" };
            
            foreach (string metric in importantMetrics)
            {
                var rankings = RankPlayers(allPlayerIds, gameId, metric);
                result.rankings.AddRange(rankings);
            }

            // Calculer les écarts de performance
            CalculatePerformanceGaps(result, playerId, otherPlayerIds, analytics);

            // Générer des insights de comparaison
            GenerateComparisonInsights(result, playerId, otherPlayerIds, analytics);

            return result;
        }

        public List<PlayerRanking> RankPlayers(List<string> playerIds, string gameId, string metricName)
        {
            var analytics = AnalyticsManager.Instance;
            if (analytics == null) return new List<PlayerRanking>();

            var playerScores = new List<(string playerId, float score)>();

            // Collecter les scores de tous les joueurs
            foreach (string playerId in playerIds)
            {
                var metrics = analytics.GetPlayerMetrics(playerId);
                if (metrics != null)
                {
                    float score = metrics.GetMetric(metricName);
                    playerScores.Add((playerId, score));
                }
            }

            // Trier selon le type de métrique
            bool isReversed = IsLowerBetterMetric(metricName);
            var sortedScores = isReversed 
                ? playerScores.OrderBy(x => x.score).ToList()
                : playerScores.OrderByDescending(x => x.score).ToList();

            // Créer les classements
            var rankings = new List<PlayerRanking>();
            for (int i = 0; i < sortedScores.Count; i++)
            {
                var (playerId, score) = sortedScores[i];
                var ranking = new PlayerRanking
                {
                    playerId = playerId,
                    rank = i + 1,
                    score = score,
                    metricName = metricName,
                    percentile = CalculatePercentile(i, sortedScores.Count)
                };
                rankings.Add(ranking);
            }

            return rankings;
        }

        public float CalculateRelativePerformance(string playerId, string gameId)
        {
            var analytics = AnalyticsManager.Instance;
            if (analytics == null) return 0f;

            var playerMetrics = analytics.GetPlayerMetrics(playerId);
            if (playerMetrics == null) return 0f;

            // Calculer la performance relative basée sur plusieurs métriques
            float scorePerf = NormalizeMetric("score", playerMetrics.GetMetric("score"));
            float accuracyPerf = NormalizeMetric("accuracy", playerMetrics.GetMetric("accuracy"));
            float reactionPerf = NormalizeMetric("avg_reaction_time", playerMetrics.GetMetric("avg_reaction_time"));
            float successPerf = NormalizeMetric("success_rate", playerMetrics.GetMetric("success_rate"));

            // Moyenne pondérée
            return (scorePerf * 0.3f + accuracyPerf * 0.3f + reactionPerf * 0.2f + successPerf * 0.2f);
        }

        #region Private Methods

        private void CalculatePerformanceGaps(PlayerComparisonResult result, string mainPlayerId, 
            List<string> otherPlayerIds, AnalyticsManager analytics)
        {
            var mainMetrics = analytics.GetPlayerMetrics(mainPlayerId);
            if (mainMetrics == null) return;

            foreach (string otherPlayerId in otherPlayerIds)
            {
                var otherMetrics = analytics.GetPlayerMetrics(otherPlayerId);
                if (otherMetrics == null) continue;

                // Calculer l'écart de performance globale
                float mainPerf = CalculateOverallPerformance(mainMetrics);
                float otherPerf = CalculateOverallPerformance(otherMetrics);
                float gap = mainPerf - otherPerf;

                result.performanceGaps[$"{mainPlayerId}_vs_{otherPlayerId}"] = gap;
            }
        }

        private void GenerateComparisonInsights(PlayerComparisonResult result, string mainPlayerId, 
            List<string> otherPlayerIds, AnalyticsManager analytics)
        {
            var mainMetrics = analytics.GetPlayerMetrics(mainPlayerId);
            if (mainMetrics == null) return;

            foreach (string otherPlayerId in otherPlayerIds)
            {
                var otherMetrics = analytics.GetPlayerMetrics(otherPlayerId);
                if (otherMetrics == null) continue;

                // Comparer les métriques importantes
                var insights = CompareSpecificMetrics(mainPlayerId, otherPlayerId, mainMetrics, otherMetrics);
                result.insights.AddRange(insights);
            }
        }

        private List<ComparisonInsight> CompareSpecificMetrics(string player1Id, string player2Id, 
            IPlayerMetrics metrics1, IPlayerMetrics metrics2)
        {
            var insights = new List<ComparisonInsight>();

            // Comparer le score
            float score1 = metrics1.GetMetric("score");
            float score2 = metrics2.GetMetric("score");
            if (Mathf.Abs(score1 - score2) > MIN_COMPARISON_THRESHOLD)
            {
                var (stronger, weaker, diff) = score1 > score2 
                    ? (player1Id, player2Id, score1 - score2)
                    : (player2Id, player1Id, score2 - score1);
                
                insights.Add(new ComparisonInsight(
                    "Différence de Score",
                    $"{stronger} a un score supérieur de {diff:F0} points",
                    stronger, weaker, diff
                ));
            }

            // Comparer la précision
            float acc1 = metrics1.GetMetric("accuracy");
            float acc2 = metrics2.GetMetric("accuracy");
            if (Mathf.Abs(acc1 - acc2) > MIN_COMPARISON_THRESHOLD)
            {
                var (stronger, weaker, diff) = acc1 > acc2 
                    ? (player1Id, player2Id, acc1 - acc2)
                    : (player2Id, player1Id, acc2 - acc1);
                
                insights.Add(new ComparisonInsight(
                    "Différence de Précision",
                    $"{stronger} est plus précis de {diff:F1}%",
                    stronger, weaker, diff
                ));
            }

            // Comparer les temps de réaction
            float reaction1 = metrics1.GetMetric("avg_reaction_time");
            float reaction2 = metrics2.GetMetric("avg_reaction_time");
            if (Mathf.Abs(reaction1 - reaction2) > MIN_COMPARISON_THRESHOLD)
            {
                var (stronger, weaker, diff) = reaction1 < reaction2 // Plus rapide = meilleur
                    ? (player1Id, player2Id, reaction2 - reaction1)
                    : (player2Id, player1Id, reaction1 - reaction2);
                
                insights.Add(new ComparisonInsight(
                    "Différence de Réactivité",
                    $"{stronger} réagit plus rapidement de {diff:F2}s",
                    stronger, weaker, diff
                ));
            }

            return insights;
        }

        private float CalculateOverallPerformance(IPlayerMetrics metrics)
        {
            float score = NormalizeMetric("score", metrics.GetMetric("score"));
            float accuracy = NormalizeMetric("accuracy", metrics.GetMetric("accuracy"));
            float reactionTime = NormalizeMetric("avg_reaction_time", metrics.GetMetric("avg_reaction_time"));
            float successRate = NormalizeMetric("success_rate", metrics.GetMetric("success_rate"));

            return (score * 0.3f + accuracy * 0.3f + reactionTime * 0.2f + successRate * 0.2f);
        }

        private float NormalizeMetric(string metricName, float value)
        {
            switch (metricName)
            {
                case "score":
                    return Mathf.Clamp01(value / 1000f); // Score sur 1000
                case "accuracy":
                case "success_rate":
                    return Mathf.Clamp01(value / 100f); // Pourcentage
                case "avg_reaction_time":
                    return Mathf.Clamp01(1f - (value / 5f)); // Inverse, 5s = 0
                default:
                    return Mathf.Clamp01(value);
            }
        }

        private bool IsLowerBetterMetric(string metricName)
        {
            // Pour certaines métriques, une valeur plus faible est meilleure
            return metricName == "avg_reaction_time" || 
                   metricName == "completion_time" ||
                   metricName.Contains("error") ||
                   metricName.Contains("miss");
        }

        private float CalculatePercentile(int rank, int totalPlayers)
        {
            if (totalPlayers <= 1) return 100f;
            return ((float)(totalPlayers - rank) / (totalPlayers - 1)) * 100f;
        }

        #endregion

        #region Advanced Comparison Features

        /// <summary>
        /// Compare les tendances de performance pendant la session
        /// </summary>
        public List<ComparisonInsight> CompareTrends(List<string> playerIds, string gameId)
        {
            var insights = new List<ComparisonInsight>();
            var analytics = AnalyticsManager.Instance;
            
            if (analytics == null || playerIds.Count < 2) return insights;

            // Analyser l'évolution des performances (nécessiterait un historique des métriques)
            // Cette fonctionnalité peut être étendue pour tracker les changements dans le temps

            return insights;
        }

        /// <summary>
        /// Identifie les forces et faiblesses relatives de chaque joueur
        /// </summary>
        public Dictionary<string, List<string>> AnalyzePlayerStrengths(List<string> playerIds, string gameId)
        {
            var strengths = new Dictionary<string, List<string>>();
            var analytics = AnalyticsManager.Instance;
            
            if (analytics == null) return strengths;

            var metrics = new[] { "score", "accuracy", "avg_reaction_time", "success_rate" };

            foreach (string playerId in playerIds)
            {
                strengths[playerId] = new List<string>();
                
                foreach (string metric in metrics)
                {
                    var rankings = RankPlayers(playerIds, gameId, metric);
                    var playerRank = rankings.FirstOrDefault(r => r.playerId == playerId);
                    
                    if (playerRank != null && playerRank.rank == 1) // Premier dans cette métrique
                    {
                        strengths[playerId].Add(GetMetricDisplayName(metric));
                    }
                }
            }

            return strengths;
        }

        private string GetMetricDisplayName(string metricName)
        {
            switch (metricName)
            {
                case "score": return "Score";
                case "accuracy": return "Précision";
                case "avg_reaction_time": return "Réactivité";
                case "success_rate": return "Taux de Réussite";
                default: return metricName;
            }
        }

        #endregion
    }
}
