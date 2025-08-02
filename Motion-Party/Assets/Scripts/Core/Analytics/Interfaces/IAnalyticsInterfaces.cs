using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Analytics.Data;

namespace Core.Analytics.Interfaces
{
    /// <summary>
    /// Interface pour les métriques de performance de base d'un joueur
    /// </summary>
    public interface IPlayerMetrics
    {
        string PlayerId { get; }
        float GetMetric(string metricName);
        void SetMetric(string metricName, float value);
        void IncrementMetric(string metricName, float amount = 1f);
        Dictionary<string, float> GetAllMetrics();
        void ResetMetrics();
        
        // Méthodes de convenance pour les métriques courantes
        void AddScore(float points);
        void UpdateAccuracy(float accuracy);
        void UpdateReactionTime(float reactionTime);
        void UpdateSuccessRate(bool wasSuccessful);
    }

    /// <summary>
    /// Interface pour les analyseurs de performance de mini-jeux
    /// </summary>
    public interface IGameAnalyzer
    {
        string GameId { get; }
        void StartSession(List<string> playerIds);
        void EndSession();
        void RecordEvent(GameAnalyticsEvent gameEvent);
        SessionAnalysisResult AnalyzeSession();
        bool IsSessionActive { get; }
    }

    /// <summary>
    /// Interface pour les processeurs d'analyse en temps réel
    /// </summary>
    public interface IAnalyticsProcessor
    {
        string ProcessorId { get; }
        int Priority { get; }
        bool CanProcess(GameAnalyticsEvent gameEvent);
        void ProcessEvent(GameAnalyticsEvent gameEvent, IPlayerMetrics playerMetrics);
        AnalysisInsight GenerateInsight(IPlayerMetrics playerMetrics);
    }

    /// <summary>
    /// Interface pour les fournisseurs de données externes (Unity Analytics, etc.)
    /// </summary>
    public interface IAnalyticsProvider
    {
        string ProviderId { get; }
        bool IsEnabled { get; }
        void Initialize();
        void SendEvent(string eventName, Dictionary<string, object> parameters);
        void SendSessionData(SessionAnalysisResult sessionResult);
        void Shutdown();
    }

    /// <summary>
    /// Interface pour la comparaison de performances entre joueurs
    /// </summary>
    public interface IPlayerComparator
    {
        PlayerComparisonResult ComparePlayer(string playerId, List<string> otherPlayerIds, string gameId);
        List<PlayerRanking> RankPlayers(List<string> playerIds, string gameId, string metricName);
        float CalculateRelativePerformance(string playerId, string gameId);
    }
}
