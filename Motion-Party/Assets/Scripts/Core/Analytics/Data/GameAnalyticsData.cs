using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Analytics.Core;

namespace Core.Analytics.Data
{
    /// <summary>
    /// Types d'événements analytiques génériques
    /// </summary>
    public enum GameAnalyticsEventType
    {
        // Événements de session
        SessionStart,
        SessionEnd,
        GameStart,
        GameEnd,
        
        // Événements de performance
        ActionPerformed,
        ObjectiveCompleted,
        ObjectiveFailed,
        ScoreChanged,
        AccuracyMeasured,
        
        // Événements de temps
        ReactionTime,
        CompletionTime,
        TimeBonus,
        
        // Événements d'interaction
        InputReceived,
        MovementDetected,
        InteractionStarted,
        InteractionCompleted,
        
        // Événements de comparaison
        PlayerComparison,
        PerformanceThreshold,
        
        // Événements personnalisés
        Custom
    }

    /// <summary>
    /// Événement analytique générique pour tous les mini-jeux
    /// </summary>
    [Serializable]
    public struct GameAnalyticsEvent
    {
        [Header("Core Event Data")]
        public GameAnalyticsEventType eventType;
        public string playerId;
        public string gameId;
        public float timestamp;
        public Vector3 position;

        [Header("Performance Metrics")]
        public float value;
        public float accuracy;
        public float reactionTime;
        public int score;

        [Header("Context Data")]
        public string context;
        public Dictionary<string, object> customData;

        [Header("Multiplayer Data")]
        public List<string> otherPlayerIds;
        public bool isMultiplayer;

        public GameAnalyticsEvent(GameAnalyticsEventType type, string player, string game)
        {
            eventType = type;
            playerId = player;
            gameId = game;
            timestamp = Time.time;
            position = Vector3.zero;
            value = 0f;
            accuracy = 0f;
            reactionTime = 0f;
            score = 0;
            context = string.Empty;
            customData = new Dictionary<string, object>();
            otherPlayerIds = new List<string>();
            isMultiplayer = false;
        }

        /// <summary>
        /// Crée un événement de performance
        /// </summary>
        public static GameAnalyticsEvent CreatePerformanceEvent(string playerId, string gameId, 
            float value, float accuracy = 0f, float reactionTime = 0f)
        {
            var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ActionPerformed, playerId, gameId);
            evt.value = value;
            evt.accuracy = accuracy;
            evt.reactionTime = reactionTime;
            return evt;
        }

        /// <summary>
        /// Crée un événement de score
        /// </summary>
        public static GameAnalyticsEvent CreateScoreEvent(string playerId, string gameId, int score)
        {
            var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ScoreChanged, playerId, gameId);
            evt.score = score;
            evt.value = score;
            return evt;
        }

        /// <summary>
        /// Crée un événement de session multijoueur
        /// </summary>
        public static GameAnalyticsEvent CreateMultiplayerEvent(GameAnalyticsEventType type, 
            string playerId, string gameId, List<string> otherPlayers)
        {
            var evt = new GameAnalyticsEvent(type, playerId, gameId);
            evt.isMultiplayer = true;
            evt.otherPlayerIds = new List<string>(otherPlayers);
            return evt;
        }
    }

    /// <summary>
    /// Résultat d'analyse de session
    /// </summary>
    [Serializable]
    public class SessionAnalysisResult
    {
        [Header("Session Info")]
        public string sessionId;
        public string gameId;
        public List<string> playerIds;
        public DateTime sessionStart;
        public DateTime sessionEnd;
        public float totalDuration;

        [Header("Performance Summary")]
        public Dictionary<string, PlayerPerformanceSummary> playerSummaries;
        public PlayerComparisonResult playerComparison;
        
        [Header("Player Analyses")]
        public Dictionary<string, PlayerAnalysisResult> playerAnalyses;

        [Header("Session Analytics")]
        public float averagePerformance;
        public float performanceVariation;
        public List<AnalysisInsight> insights;
        
        [Header("Quick Access Properties")]
        public int finalScore;
        public int totalActions;

        // Champs supplémentaires nécessaires
        public DateTime startTime { get => sessionStart; set => sessionStart = value; }
        public DateTime endTime { get => sessionEnd; set => sessionEnd = value; }
        public int playerCount => playerIds?.Count ?? 0;
        public float sessionDuration => (float)(sessionEnd - sessionStart).TotalMinutes;
        public float completionRate => playerCount > 0 ? (float)playerAnalyses.Count / playerCount : 0f;
        public Dictionary<string, float> sessionAverages;
        public List<AnalysisInsight> sessionInsights { get => insights; set => insights = value; }

        public SessionAnalysisResult()
        {
            sessionId = Guid.NewGuid().ToString();
            playerIds = new List<string>();
            playerSummaries = new Dictionary<string, PlayerPerformanceSummary>();
            playerAnalyses = new Dictionary<string, PlayerAnalysisResult>();
            insights = new List<AnalysisInsight>();
            sessionAverages = new Dictionary<string, float>();
            sessionStart = DateTime.Now;
        }
    }

    /// <summary>
    /// Résultat d'analyse d'un joueur individuel
    /// </summary>
    [Serializable]
    public class PlayerAnalysisResult
    {
        public string playerId;
        public string sessionId;
        public DateTime analysisTime;
        public Dictionary<string, float> playerMetrics;
        public Dictionary<string, float> performanceVsAverage;
        public Dictionary<string, MiniGamePerformance> miniGamePerformances;
        public List<string> recommendations;

        public PlayerAnalysisResult()
        {
            playerMetrics = new Dictionary<string, float>();
            performanceVsAverage = new Dictionary<string, float>();
            miniGamePerformances = new Dictionary<string, MiniGamePerformance>();
            recommendations = new List<string>();
            analysisTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Performance dans un mini-jeu spécifique
    /// </summary>
    [Serializable]
    public class MiniGamePerformance
    {
        public string gameName;
        public float score;
        public float accuracy;
        public float completionTime;
        public Dictionary<string, float> specialMetrics;

        public MiniGamePerformance()
        {
            specialMetrics = new Dictionary<string, float>();
        }
    }

    /// <summary>
    /// Résumé de performance d'un joueur
    /// </summary>
    [Serializable]
    public class PlayerPerformanceSummary
    {
        public string playerId;
        public Dictionary<string, float> metrics;
        public float overallPerformance;
        public List<string> achievements;
        public List<string> improvements;

        public PlayerPerformanceSummary(string id)
        {
            playerId = id;
            metrics = new Dictionary<string, float>();
            achievements = new List<string>();
            improvements = new List<string>();
        }
    }

    /// <summary>
    /// Résultat de comparaison entre joueurs
    /// </summary>
    [Serializable]
    public class PlayerComparisonResult
    {
        public List<PlayerRanking> rankings;
        public Dictionary<string, float> performanceGaps;
        public List<ComparisonInsight> insights;

        public PlayerComparisonResult()
        {
            rankings = new List<PlayerRanking>();
            performanceGaps = new Dictionary<string, float>();
            insights = new List<ComparisonInsight>();
        }
    }

    /// <summary>
    /// Classement d'un joueur
    /// </summary>
    [Serializable]
    public class PlayerRanking
    {
        public string playerId;
        public int rank;
        public float score;
        public string metricName;
        public float percentile;
    }

    /// <summary>
    /// Insight d'analyse
    /// </summary>
    [Serializable]
    public class AnalysisInsight
    {
        public string title;
        public string description;
        public InsightType type;
        public float confidence;
        public Dictionary<string, object> data;

        public AnalysisInsight(string title, string description, InsightType type, float confidence = 1f)
        {
            this.title = title;
            this.description = description;
            this.type = type;
            this.confidence = confidence;
            this.data = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Insight de comparaison entre joueurs
    /// </summary>
    [Serializable]
    public class ComparisonInsight : AnalysisInsight
    {
        public string strongerPlayerId;
        public string weakerPlayerId;
        public float performanceDifference;

        public ComparisonInsight(string title, string description, string stronger, string weaker, float difference) 
            : base(title, description, InsightType.PlayerComparison)
        {
            strongerPlayerId = stronger;
            weakerPlayerId = weaker;
            performanceDifference = difference;
        }
    }

    /// <summary>
    /// Types d'insights
    /// </summary>
    public enum InsightType
    {
        Performance,
        Improvement,
        Achievement,
        PlayerComparison,
        SessionTrend,
        Custom
    }
}
