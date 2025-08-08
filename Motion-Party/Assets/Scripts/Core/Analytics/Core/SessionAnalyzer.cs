using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Analytics.Interfaces;
using Core.Analytics.Data;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Analyseur de session globale avec moyennes des joueurs
    /// </summary>
    [Serializable]
    public class SessionAnalyzer
    {
        [Header("Session Info")]
        [SerializeField] private string sessionId;
        [SerializeField] private DateTime sessionStartTime;
        [SerializeField] private DateTime sessionEndTime;
        [SerializeField] private List<string> playerIds;
        
        [Header("Global Metrics")]
        [SerializeField] private Dictionary<string, float> sessionAverages;
        [SerializeField] private Dictionary<string, PlayerAnalysisResult> playerAnalyses;
        
        [Header("Mini-Game Progress")]
        [SerializeField] private Dictionary<string, bool> miniGameCompletion;
        [SerializeField] private int totalMiniGames = 3; // Firefly, Log, Music
        
        private const string FIREFLY_GAME = "FireflyDance";
        private const string LOG_GAME = "LogParade";
        private const string MUSIC_GAME = "MusicNotePress";

        public string SessionId => sessionId;
        public DateTime StartTime => sessionStartTime;
        public DateTime EndTime => sessionEndTime;
        public bool IsSessionComplete => miniGameCompletion.Values.All(x => x);
        public float SessionProgress => miniGameCompletion.Values.Count(x => x) / (float)totalMiniGames;
        public List<string> PlayerIds => new List<string>(playerIds);

        public SessionAnalyzer(string id)
        {
            sessionId = id;
            sessionStartTime = DateTime.Now;
            playerIds = new List<string>();
            sessionAverages = new Dictionary<string, float>();
            playerAnalyses = new Dictionary<string, PlayerAnalysisResult>();
            
            // Initialiser le suivi des mini-jeux
            miniGameCompletion = new Dictionary<string, bool>
            {
                { FIREFLY_GAME, false },
                { LOG_GAME, false },
                { MUSIC_GAME, false }
            };
        }

        #region Player Management

        /// <summary>
        /// Ajoute un joueur à la session
        /// </summary>
        public void AddPlayer(string playerId)
        {
            if (!playerIds.Contains(playerId))
            {
                playerIds.Add(playerId);
                UnityEngine.Debug.Log($"[SessionAnalyzer] Joueur {playerId} ajouté à la session {sessionId}. Total joueurs: {playerIds.Count}");
            }
            else
            {
                UnityEngine.Debug.Log($"[SessionAnalyzer] Joueur {playerId} déjà présent dans la session {sessionId}. Total joueurs: {playerIds.Count}");
            }
        }

        /// <summary>
        /// Obtient tous les joueurs de la session
        /// </summary>
        public List<string> GetAllPlayers()
        {
            return new List<string>(playerIds);
        }
        
        /// <summary>
        /// Obtient l'ID de la session
        /// </summary>
        public string GetSessionId()
        {
            return sessionId;
        }
        
        /// <summary>
        /// Marque un mini-jeu comme complété
        /// </summary>
        public void MarkMiniGameCompleted(string miniGameName)
        {
            // Normaliser le nom du mini-jeu
            string normalizedName = NormalizeMiniGameName(miniGameName);
            
            if (miniGameCompletion.ContainsKey(normalizedName))
            {
                miniGameCompletion[normalizedName] = true;
                UnityEngine.Debug.Log($"[SessionAnalyzer] Mini-jeu {normalizedName} marqué comme complété");
                
                // Vérifier si tous les mini-jeux sont terminés
                if (IsSessionComplete)
                {
                    CompleteSession();
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[SessionAnalyzer] Mini-jeu inconnu: {miniGameName} (normalisé: {normalizedName})");
            }
        }
        
        /// <summary>
        /// Normalise les noms de mini-jeux pour correspondre aux constantes
        /// </summary>
        private string NormalizeMiniGameName(string rawName)
        {
            if (rawName.Contains("Firefly") || rawName.Contains("firefly"))
                return FIREFLY_GAME;
            if (rawName.Contains("Log") || rawName.Contains("log"))
                return LOG_GAME;
            if (rawName.Contains("Music") || rawName.Contains("music") || rawName.Contains("Note"))
                return MUSIC_GAME;
                
            return rawName; // Retourner tel quel si pas de correspondance
        }

        #endregion

        #region Mini-Game Progress

        /// <summary>
        /// Vérifie si un mini-jeu est complété
        /// </summary>
        public bool IsMiniGameCompleted(string miniGameName)
        {
            return miniGameCompletion.TryGetValue(miniGameName, out bool completed) && completed;
        }

        /// <summary>
        /// Obtient la liste des mini-jeux restants
        /// </summary>
        public List<string> GetRemainingMiniGames()
        {
            return miniGameCompletion
                .Where(kvp => !kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        #endregion

        #region Performance Data Collection

        /// <summary>
        /// Enregistre un score pour un joueur dans un mini-jeu spécifique
        /// </summary>
        public void RecordPlayerScore(string playerId, string miniGameName, float score)
        {
            if (!playerIds.Contains(playerId))
            {
                AddPlayer(playerId);
            }

            // Créer la clé pour identifier la métrique
            string metricKey = $"{miniGameName}_score_{playerId}";
            
            // Stocker le score dans les moyennes de session (on peut l'étendre plus tard)
            if (!sessionAverages.ContainsKey(metricKey))
            {
                sessionAverages[metricKey] = score;
            }
            else
            {
                // Moyenne simple pour l'instant
                sessionAverages[metricKey] = (sessionAverages[metricKey] + score) / 2f;
            }

            UnityEngine.Debug.Log($"[SessionAnalyzer] Score enregistré - Joueur: {playerId}, Jeu: {miniGameName}, Score: {score}");
        }

        /// <summary>
        /// Enregistre une métrique de performance générale
        /// </summary>
        public void RecordPerformanceMetric(string playerId, string miniGameName, string metricName, float value)
        {
            if (!playerIds.Contains(playerId))
            {
                AddPlayer(playerId);
            }

            string metricKey = $"{miniGameName}_{metricName}_{playerId}";
            sessionAverages[metricKey] = value;

            UnityEngine.Debug.Log($"[SessionAnalyzer] Métrique enregistrée - Joueur: {playerId}, Jeu: {miniGameName}, {metricName}: {value}");
        }

        /// <summary>
        /// Enregistre des données d'analyse pour un joueur après un mini-jeu
        /// </summary>
        public void RecordPlayerAnalysis(string playerId, PlayerAnalysisResult analysis)
        {
            if (!playerIds.Contains(playerId))
            {
                AddPlayer(playerId);
            }

            playerAnalyses[playerId] = analysis;
            UnityEngine.Debug.Log($"[SessionAnalyzer] Analyse joueur enregistrée pour {playerId}");
        }

        /// <summary>
        /// Obtient les métriques stockées pour debug
        /// </summary>
        public Dictionary<string, float> GetStoredMetrics()
        {
            return new Dictionary<string, float>(sessionAverages);
        }

        #endregion

        #region Session Analysis

        /// <summary>
        /// Analyse complète de la session avec moyennes
        /// </summary>
        public SessionAnalysisResult AnalyzeSession()
        {
            UnityEngine.Debug.Log($"[SessionAnalyzer] Début d'analyse de session {sessionId}");
            UnityEngine.Debug.Log($"[SessionAnalyzer] Joueurs dans la session: {playerIds.Count} - {string.Join(", ", playerIds)}");
            UnityEngine.Debug.Log($"[SessionAnalyzer] Métriques disponibles: {sessionAverages.Count}");
            
            var result = new SessionAnalysisResult
            {
                sessionId = sessionId,
                sessionStart = sessionStartTime,
                sessionEnd = sessionEndTime
            };

            // Calculer les moyennes globales
            CalculateSessionAverages();
            result.sessionAverages = new Dictionary<string, float>(sessionAverages);
            UnityEngine.Debug.Log($"[SessionAnalyzer] Moyennes calculées: {result.sessionAverages.Count} métriques");

            // Analyser chaque joueur individuellement
            result.playerAnalyses = new Dictionary<string, PlayerAnalysisResult>();
            foreach (string playerId in playerIds)
            {
                var playerAnalysis = AnalyzePlayer(playerId);
                if (playerAnalysis != null && !string.IsNullOrEmpty(playerAnalysis.playerId))
                {
                    result.playerAnalyses[playerId] = playerAnalysis;
                }
            }

            // Générer insights de session (convertir string en AnalysisInsight)
            var insights = GenerateSessionInsights();
            result.sessionInsights = insights.Select(insight => new AnalysisInsight(
                "Session Insight", 
                insight, 
                InsightType.Performance, 
                1.0f
            )).ToList();

            return result;
        }

        /// <summary>
        /// Analyse spécifique d'un joueur
        /// </summary>
        public PlayerAnalysisResult AnalyzePlayer(string playerId)
        {
            if (!playerIds.Contains(playerId))
            {
                UnityEngine.Debug.LogWarning($"[SessionAnalyzer] Joueur {playerId} non trouvé dans la session");
                return new PlayerAnalysisResult { playerId = playerId };
            }

            var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
            if (playerMetrics == null)
            {
                UnityEngine.Debug.LogWarning($"[SessionAnalyzer] Métriques non trouvées pour le joueur {playerId}");
                return new PlayerAnalysisResult { playerId = playerId };
            }

            var result = new PlayerAnalysisResult
            {
                playerId = playerId,
                sessionId = sessionId,
                analysisTime = DateTime.Now
            };

            // Récupérer toutes les métriques du joueur
            var allMetrics = playerMetrics.GetAllMetrics();
            result.playerMetrics = new Dictionary<string, float>(allMetrics);

            // Calculer les performances relatives à la moyenne
            result.performanceVsAverage = CalculatePerformanceVsAverage(allMetrics);

            // Analyser par mini-jeu
            result.miniGamePerformances = AnalyzeMiniGamePerformances(playerId);

            // Générer recommandations personnalisées
            result.recommendations = GeneratePlayerRecommendations(allMetrics);

            // Stocker l'analyse
            playerAnalyses[playerId] = result;

            return result;
        }

        #endregion

        #region Private Analysis Methods

        /// <summary>
        /// Calcule les moyennes de session pour tous les joueurs
        /// </summary>
        private void CalculateSessionAverages()
        {
            sessionAverages.Clear();
            
            if (playerIds.Count == 0) return;

            var allMetricNames = new HashSet<string>();
            
            // Collecter tous les noms de métriques
            foreach (string playerId in playerIds)
            {
                var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
                if (playerMetrics != null)
                {
                    foreach (var metricName in playerMetrics.GetAllMetrics().Keys)
                    {
                        allMetricNames.Add(metricName);
                    }
                }
            }

            // Calculer la moyenne pour chaque métrique
            foreach (string metricName in allMetricNames)
            {
                float sum = 0f;
                int count = 0;

                foreach (string playerId in playerIds)
                {
                    var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
                    if (playerMetrics != null)
                    {
                        float value = playerMetrics.GetMetric(metricName);
                        if (value > 0) // Ignorer les valeurs nulles/par défaut
                        {
                            sum += value;
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                    sessionAverages[metricName] = sum / count;
                }
            }
        }

        /// <summary>
        /// Compare les performances d'un joueur à la moyenne de session
        /// </summary>
        private Dictionary<string, float> CalculatePerformanceVsAverage(Dictionary<string, float> playerMetrics)
        {
            var comparison = new Dictionary<string, float>();

            foreach (var metric in playerMetrics)
            {
                if (sessionAverages.TryGetValue(metric.Key, out float average) && average > 0)
                {
                    // Ratio performance joueur / moyenne session
                    comparison[metric.Key] = metric.Value / average;
                }
            }

            return comparison;
        }

        /// <summary>
        /// Analyse les performances par mini-jeu
        /// </summary>
        private Dictionary<string, MiniGamePerformance> AnalyzeMiniGamePerformances(string playerId)
        {
            var performances = new Dictionary<string, MiniGamePerformance>();

            // Analyser chaque mini-jeu
            performances[FIREFLY_GAME] = AnalyzeFireflyPerformance(playerId);
            performances[LOG_GAME] = AnalyzeLogParadePerformance(playerId);
            performances[MUSIC_GAME] = AnalyzeMusicNotePerformance(playerId);

            return performances;
        }

        /// <summary>
        /// Génère des insights globaux de session
        /// </summary>
        private List<string> GenerateSessionInsights()
        {
            var insights = new List<string>();

            // Insight sur la completion
            if (IsSessionComplete)
            {
                insights.Add($"✅ Session complète - Tous les mini-jeux terminés en {(sessionEndTime - sessionStartTime).TotalMinutes:F1} minutes");
            }
            else
            {
                var remaining = GetRemainingMiniGames();
                insights.Add($"🎮 Session en cours - {remaining.Count} mini-jeu(s) restant(s): {string.Join(", ", remaining)}");
            }

            // Insight sur la participation
            insights.Add($"👥 {playerIds.Count} joueur(s) actifs dans cette session");

            // Insight sur les performances moyennes
            if (sessionAverages.TryGetValue("score", out float avgScore))
            {
                insights.Add($"📊 Score moyen de la session: {avgScore:F1} points");
            }

            if (sessionAverages.TryGetValue("accuracy", out float avgAccuracy))
            {
                insights.Add($"🎯 Précision moyenne: {avgAccuracy * 100:F1}%");
            }

            return insights;
        }

        #endregion

        #region Session Completion

        /// <summary>
        /// Marque la session comme terminée et génère les rapports finaux
        /// </summary>
        private void CompleteSession()
        {
            sessionEndTime = DateTime.Now;
            UnityEngine.Debug.Log($"[SessionAnalyzer] Session {sessionId} terminée - Génération des rapports...");
            
            // Générer automatiquement les fichiers de résultats
            var finalAnalysis = AnalyzeSession();
            AnalyticsFileGenerator.Instance.GenerateSessionReport(finalAnalysis);
            
            // Générer les rapports individuels pour chaque joueur
            foreach (string playerId in playerIds)
            {
                var playerAnalysis = AnalyzePlayer(playerId);
                if (playerAnalysis != null && !string.IsNullOrEmpty(playerAnalysis.playerId))
                {
                    AnalyticsFileGenerator.Instance.GeneratePlayerReport(playerAnalysis);
                }
            }

            // Déclencher le retour au menu principal
            TriggerReturnToMainMenu();
        }

        /// <summary>
        /// Déclenche le retour automatique au menu principal
        /// </summary>
        private void TriggerReturnToMainMenu()
        {
            UnityEngine.Debug.Log("[SessionAnalyzer] Tous les mini-jeux terminés - Retour au menu principal");
            
            // Le retour au menu sera géré par GameSessionManager
            // Plus besoin d'appeler GameFlowManager ici
            UnityEngine.Debug.Log("[SessionAnalyzer] Fin d'analyse - GameSessionManager gère le retour au menu");
        }

        #endregion

        #region Mini-Game Specific Analysis

        private MiniGamePerformance AnalyzeFireflyPerformance(string playerId)
        {
            var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
            if (playerMetrics == null) return new MiniGamePerformance { gameName = FIREFLY_GAME };

            return new MiniGamePerformance
            {
                gameName = FIREFLY_GAME,
                score = playerMetrics.GetMetric("score"),
                accuracy = playerMetrics.GetMetric("accuracy"),
                completionTime = playerMetrics.GetMetric("completion_time"),
                specialMetrics = new Dictionary<string, float>
                {
                    { "fireflies_captured", playerMetrics.GetMetric("fireflies_captured") },
                    { "avg_reaction_time", playerMetrics.GetMetric("avg_reaction_time") },
                    { "hand_precision", playerMetrics.GetMetric("hand_precision") }
                }
            };
        }

        private MiniGamePerformance AnalyzeLogParadePerformance(string playerId)
        {
            var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
            if (playerMetrics == null) return new MiniGamePerformance { gameName = LOG_GAME };

            return new MiniGamePerformance
            {
                gameName = LOG_GAME,
                score = playerMetrics.GetMetric("score"),
                accuracy = playerMetrics.GetMetric("accuracy"),
                completionTime = playerMetrics.GetMetric("completion_time"),
                specialMetrics = new Dictionary<string, float>
                {
                    { "logs_navigated", playerMetrics.GetMetric("logs_navigated") },
                    { "balance_stability", playerMetrics.GetMetric("balance_stability") }
                }
            };
        }

        private MiniGamePerformance AnalyzeMusicNotePerformance(string playerId)
        {
            var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
            if (playerMetrics == null) return new MiniGamePerformance { gameName = MUSIC_GAME };

            return new MiniGamePerformance
            {
                gameName = MUSIC_GAME,
                score = playerMetrics.GetMetric("score"),
                accuracy = playerMetrics.GetMetric("accuracy"),
                completionTime = playerMetrics.GetMetric("completion_time"),
                specialMetrics = new Dictionary<string, float>
                {
                    { "notes_hit", playerMetrics.GetMetric("notes_hit") },
                    { "rhythm_precision", playerMetrics.GetMetric("rhythm_precision") },
                    { "combo_max", playerMetrics.GetMetric("combo_max") }
                }
            };
        }

        private List<string> GeneratePlayerRecommendations(Dictionary<string, float> metrics)
        {
            var recommendations = new List<string>();

            // Recommandations basées sur l'accuracy
            if (metrics.TryGetValue("accuracy", out float accuracy))
            {
                if (accuracy < 0.6f)
                {
                    recommendations.Add("💡 Prenez votre temps pour améliorer la précision");
                }
                else if (accuracy > 0.9f)
                {
                    recommendations.Add("🏆 Excellente précision ! Essayez d'augmenter la vitesse");
                }
            }

            // Recommandations sur le temps de réaction
            if (metrics.TryGetValue("avg_reaction_time", out float reactionTime))
            {
                if (reactionTime > 1.0f)
                {
                    recommendations.Add("⚡ Essayez de réagir plus rapidement aux stimuli");
                }
                else if (reactionTime < 0.3f)
                {
                    recommendations.Add("🚀 Temps de réaction excellent !");
                }
            }

            return recommendations;
        }

        #endregion
    }

    #region Data Structures

    // Note: SessionAnalysisResult et PlayerAnalysisResult déplacés vers Core.Analytics.Data.GameAnalyticsData.cs
    // pour éviter les conflits de namespace et unifier les définitions

    #endregion
}
