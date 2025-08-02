using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Analytics.Interfaces;
using Core.Analytics.Data;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Gestionnaire principal d'analyse en temps réel pour tous les mini-jeux
    /// Système modulaire et générique pour l'analyse de performance
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private bool enableRealTimeAnalysis = true;
        [SerializeField] private float analysisInterval = 1f;

        [Header("Session Management")]
        [SerializeField] private bool autoStartSessions = true;
        [SerializeField] private float sessionTimeoutSeconds = 300f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool showInsights = true;

        // Composants du système
        private Dictionary<string, IGameAnalyzer> gameAnalyzers;
        private Dictionary<string, PlayerMetrics> playerMetrics;
        private List<IAnalyticsProcessor> processors;
        private List<IAnalyticsProvider> providers;
        private PlayerComparator playerComparator;

        // État de session
        private SessionAnalysisResult currentSession;
        private float lastAnalysisTime;
        private bool isSessionActive;

        // Événements
        public static event System.Action<SessionAnalysisResult> OnSessionCompleted;
        public static event System.Action<AnalysisInsight> OnInsightGenerated;
        public static event System.Action<PlayerComparisonResult> OnPlayerComparison;

        #region Unity Lifecycle

        private void Awake()
        {
            if (!isEnabled) return;

            InitializeSystem();
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!isEnabled || !isSessionActive) return;

            // Analyse en temps réel
            if (enableRealTimeAnalysis && Time.time - lastAnalysisTime >= analysisInterval)
            {
                PerformRealtimeAnalysis();
                lastAnalysisTime = Time.time;
            }

            // Vérification du timeout de session
            CheckSessionTimeout();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isSessionActive)
            {
                EndSession();
            }
        }

        private void OnDestroy()
        {
            if (isSessionActive)
            {
                EndSession();
            }
            
            // Shutdown des providers
            foreach (var provider in providers)
            {
                provider?.Shutdown();
            }
        }

        #endregion

        #region System Initialization

        private void InitializeSystem()
        {
            gameAnalyzers = new Dictionary<string, IGameAnalyzer>();
            playerMetrics = new Dictionary<string, PlayerMetrics>();
            processors = new List<IAnalyticsProcessor>();
            providers = new List<IAnalyticsProvider>();
            playerComparator = new PlayerComparator();

            // Initialiser les processeurs par défaut
            RegisterDefaultProcessors();

            // Initialiser les providers
            InitializeProviders();

            DebugLog("AnalyticsManager initialisé");
        }

        private void RegisterDefaultProcessors()
        {
            // Les processeurs seront ajoutés quand ils seront implémentés
            // processors.Add(new PerformanceProcessor());
            // processors.Add(new AccuracyProcessor());
            // processors.Add(new ReactionTimeProcessor());
            // processors.Add(new ScoreProcessor());

            // Trier par priorité
            processors = processors.OrderBy(p => p.Priority).ToList();
        }

        private void InitializeProviders()
        {
            // Ajouter ici les providers spécifiques (Unity Analytics, etc.)
            // providers.Add(new UnityAnalyticsProvider());
        }

        #endregion

        #region Public API

        /// <summary>
        /// Démarre une nouvelle session d'analyse
        /// </summary>
        public void StartSession(string gameId, List<string> playerIds)
        {
            if (isSessionActive)
            {
                EndSession();
            }

            currentSession = new SessionAnalysisResult
            {
                gameId = gameId,
                playerIds = new List<string>(playerIds),
                sessionStart = DateTime.Now
            };

            // Initialiser les métriques des joueurs
            foreach (string playerId in playerIds)
            {
                if (!playerMetrics.ContainsKey(playerId))
                {
                    playerMetrics[playerId] = new PlayerMetrics(playerId);
                }
                else
                {
                    playerMetrics[playerId].ResetSessionMetrics();
                }
            }

            // Obtenir ou créer l'analyseur de jeu
            if (!gameAnalyzers.ContainsKey(gameId))
            {
                gameAnalyzers[gameId] = new GenericGameAnalyzer(gameId);
            }

            gameAnalyzers[gameId].StartSession(playerIds);
            isSessionActive = true;

            DebugLog($"Session démarrée pour {gameId} avec {playerIds.Count} joueur(s)");
        }

        /// <summary>
        /// Termine la session en cours
        /// </summary>
        public void EndSession()
        {
            if (!isSessionActive) return;

            currentSession.sessionEnd = DateTime.Now;
            currentSession.totalDuration = (float)(currentSession.sessionEnd - currentSession.sessionStart).TotalSeconds;

            // Analyser la session complète
            var sessionResult = AnalyzeCompleteSession();
            
            // Envoyer aux providers
            foreach (var provider in providers.Where(p => p.IsEnabled))
            {
                provider.SendSessionData(sessionResult);
            }

            OnSessionCompleted?.Invoke(sessionResult);
            isSessionActive = false;

            DebugLog($"Session terminée - Durée: {currentSession.totalDuration:F1}s");
        }

        /// <summary>
        /// Enregistre un événement d'analyse
        /// </summary>
        public void RecordEvent(GameAnalyticsEvent gameEvent)
        {
            if (!isEnabled || !isSessionActive) return;

            // Traiter l'événement avec tous les processeurs appropriés
            if (playerMetrics.ContainsKey(gameEvent.playerId))
            {
                var metrics = playerMetrics[gameEvent.playerId];
                
                foreach (var processor in processors.Where(p => p.CanProcess(gameEvent)))
                {
                    processor.ProcessEvent(gameEvent, metrics);
                }
            }

            // Enregistrer dans l'analyseur de jeu
            if (gameAnalyzers.ContainsKey(gameEvent.gameId))
            {
                gameAnalyzers[gameEvent.gameId].RecordEvent(gameEvent);
            }

            // Envoyer aux providers externes
            SendEventToProviders(gameEvent);
        }

        /// <summary>
        /// Obtient les métriques d'un joueur
        /// </summary>
        public IPlayerMetrics GetPlayerMetrics(string playerId)
        {
            return playerMetrics.ContainsKey(playerId) ? playerMetrics[playerId] : null;
        }

        /// <summary>
        /// Compare les performances entre joueurs
        /// </summary>
        public PlayerComparisonResult ComparePlayersInSession()
        {
            if (!isSessionActive || currentSession.playerIds.Count < 2)
                return new PlayerComparisonResult();

            var result = playerComparator.ComparePlayer(
                currentSession.playerIds[0], 
                currentSession.playerIds.Skip(1).ToList(), 
                currentSession.gameId
            );

            OnPlayerComparison?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Force une analyse en temps réel
        /// </summary>
        public void ForceRealtimeAnalysis()
        {
            if (isSessionActive)
            {
                PerformRealtimeAnalysis();
            }
        }

        #endregion

        #region Private Methods

        private void PerformRealtimeAnalysis()
        {
            if (currentSession.playerIds.Count == 0) return;

            // Générer des insights pour chaque joueur
            foreach (string playerId in currentSession.playerIds)
            {
                if (playerMetrics.ContainsKey(playerId))
                {
                    var metrics = playerMetrics[playerId];
                    
                    // Générer des insights avec chaque processeur
                    foreach (var processor in processors)
                    {
                        var insight = processor.GenerateInsight(metrics);
                        if (insight != null)
                        {
                            if (showInsights)
                            {
                                OnInsightGenerated?.Invoke(insight);
                            }
                        }
                    }
                }
            }

            // Comparaison entre joueurs si multijoueur
            if (currentSession.playerIds.Count > 1)
            {
                ComparePlayersInSession();
            }
        }

        private SessionAnalysisResult AnalyzeCompleteSession()
        {
            // Analyser avec l'analyseur de jeu
            if (gameAnalyzers.ContainsKey(currentSession.gameId))
            {
                var gameResult = gameAnalyzers[currentSession.gameId].AnalyzeSession();
                currentSession.playerSummaries = gameResult.playerSummaries;
                currentSession.insights.AddRange(gameResult.insights);
            }

            // Comparaison finale des joueurs
            if (currentSession.playerIds.Count > 1)
            {
                currentSession.playerComparison = ComparePlayersInSession();
            }

            // Calculer les métriques globales
            CalculateSessionMetrics();

            return currentSession;
        }

        private void CalculateSessionMetrics()
        {
            if (currentSession.playerSummaries.Count == 0) return;

            var performances = currentSession.playerSummaries.Values
                .Select(s => s.overallPerformance)
                .ToList();

            if (performances.Count > 0)
            {
                currentSession.averagePerformance = performances.Average();
                
                if (performances.Count > 1)
                {
                    var variance = performances.Select(p => Mathf.Pow(p - currentSession.averagePerformance, 2)).Average();
                    currentSession.performanceVariation = Mathf.Sqrt(variance);
                }
            }
        }

        private void SendEventToProviders(GameAnalyticsEvent gameEvent)
        {
            var eventData = new Dictionary<string, object>
            {
                ["eventType"] = gameEvent.eventType.ToString(),
                ["playerId"] = gameEvent.playerId,
                ["gameId"] = gameEvent.gameId,
                ["value"] = gameEvent.value,
                ["accuracy"] = gameEvent.accuracy,
                ["reactionTime"] = gameEvent.reactionTime,
                ["score"] = gameEvent.score,
                ["isMultiplayer"] = gameEvent.isMultiplayer
            };

            foreach (var provider in providers.Where(p => p.IsEnabled))
            {
                provider.SendEvent($"game_{gameEvent.eventType}", eventData);
            }
        }

        private void CheckSessionTimeout()
        {
            if (currentSession != null && 
                (DateTime.Now - currentSession.sessionStart).TotalSeconds > sessionTimeoutSeconds)
            {
                DebugLog("Session timeout - Fin automatique");
                EndSession();
            }
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                UnityEngine.Debug.Log($"[AnalyticsManager] {message}");
            }
        }

        #endregion

        #region Static Access

        private static AnalyticsManager instance;
        public static AnalyticsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AnalyticsManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("AnalyticsManager");
                        instance = go.AddComponent<AnalyticsManager>();
                    }
                }
                return instance;
            }
        }

        #endregion
    }
}
