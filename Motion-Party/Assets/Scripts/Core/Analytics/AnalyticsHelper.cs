using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Core.Analytics.Core;
using Core.Analytics.Data;
using Systems;

namespace Core.Analytics
{
    /// <summary>
    /// Utilitaire simplifié pour intégrer facilement le système d'analytics
    /// dans tous les mini-jeux de Motion Party
    /// </summary>
    public static class AnalyticsHelper
    {
        private static AnalyticsManager Analytics => AnalyticsManager.Instance;

        #region Session Management

        /// <summary>
        /// Récupère le joueur actuellement en train de jouer
        /// </summary>
        public static string GetCurrentPlayerFromSession()
        {
            // Priorité 1 : Utiliser GamePlayerSelector pour le joueur actuel
            if (GamePlayerSelector.Instance != null)
            {
                var currentPlayer = GamePlayerSelector.Instance.CurrentPlayer;
                if (currentPlayer != null && !string.IsNullOrEmpty(currentPlayer.Id))
                {
                    return currentPlayer.Id;
                }
            }
            
            // Priorité 2 : Fallback sur GameSessionManager
            if (GameSessionManager.EnsureInstance() != null && GameSessionManager.Instance.IsAnalyticsSessionActive())
            {
                var players = GameSessionManager.Instance.GetCurrentSessionPlayers();
                if (players != null && players.Count > 0)
                {
                    // Retourner le premier joueur de la session si aucun joueur actuel défini
                    return players[0];
                }
            }
            
            // Fallback final si aucun joueur trouvé
            return "Unknown_Player";
        }

        /// <summary>
        /// Démarre une session d'analyse pour un mini-jeu
        /// </summary>
        public static void StartGameSession(string gameId, params string[] playerIds)
        {
            StartGameSession(gameId, new List<string>(playerIds));
        }

        /// <summary>
        /// Démarre une session d'analyse pour un mini-jeu
        /// </summary>
        public static void StartGameSession(string gameId, List<string> playerIds)
        {
            if (Analytics != null)
            {
                Analytics.StartSession(gameId, playerIds);
                UnityEngine.Debug.Log($"[Analytics] Session démarrée pour {gameId} avec {playerIds.Count} joueur(s)");
            }

            // Ajouter les joueurs à la session globale via GameSessionManager
            if (GameSessionManager.EnsureInstance() != null)
            {
                foreach (string playerId in playerIds)
                {
                    GameSessionManager.Instance.AddPlayerToAnalyticsSession(playerId);
                }
            }
        }

        /// <summary>
        /// Démarre une session d'analyse pour un mini-jeu et retourne un ID de session
        /// </summary>
        public static string StartGameSession(string playerName)
        {
            string sessionId = System.Guid.NewGuid().ToString();
            StartGameSession("GenericGame", new List<string> { playerName });
            
            // Ajouter à la session globale via GameSessionManager
            if (GameSessionManager.EnsureInstance() != null)
            {
                GameSessionManager.Instance.AddPlayerToAnalyticsSession(playerName);
            }
            
            return sessionId;
        }

        /// <summary>
        /// Termine la session d'analyse en cours
        /// </summary>
        public static void EndGameSession()
        {
            if (Analytics != null)
            {
                Analytics.EndSession();
                UnityEngine.Debug.Log("[Analytics] Session terminée");
            }
        }

        /// <summary>
        /// Termine un mini-jeu et marque comme complété dans la session globale
        /// </summary>
        public static void CompleteMiniGame(string miniGameId)
        {
            if (GameSessionManager.EnsureInstance() != null)
            {
                GameSessionManager.Instance.CompleteMiniGameAnalytics(miniGameId);
                UnityEngine.Debug.Log($"[AnalyticsHelper] Mini-jeu {miniGameId} marqué comme complété");
            }
        }

        #endregion

        #region Quick Event Recording

        /// <summary>
        /// Enregistre un score pour un joueur
        /// </summary>
        public static void RecordScore(string playerId, string gameId, int score)
        {
            // Enregistrer dans l'AnalyticsManager local
            var evt = GameAnalyticsEvent.CreateScoreEvent(playerId, gameId, score);
            RecordEvent(evt);
            
            // Enregistrer aussi dans la session globale via GameSessionManager
            if (GameSessionManager.EnsureInstance() != null)
            {
                GameSessionManager.Instance.RecordPlayerScore(playerId, gameId, score);
            }
        }

        /// <summary>
        /// Enregistre un score avec un sessionId
        /// </summary>
        public static void RecordScore(string sessionId, int score)
        {
            string playerId = GetCurrentPlayerFromSession();
            RecordScore(playerId, "GenericGame", score);
        }

        /// <summary>
        /// Enregistre une métrique de performance pour un joueur
        /// </summary>
        public static void RecordPlayerMetric(string playerId, string gameId, string metricName, float value)
        {
            UnityEngine.Debug.Log($"[AnalyticsHelper] RecordPlayerMetric appelé - Player: {playerId}, Game: {gameId}, Metric: {metricName}, Value: {value}");
            
            // Enregistrer dans la session globale via GameSessionManager
            if (GameSessionManager.EnsureInstance() != null)
            {
                GameSessionManager.Instance.RecordPlayerMetric(playerId, gameId, metricName, value);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[AnalyticsHelper] GameSessionManager.Instance est null - impossible d'enregistrer la métrique");
            }
        }

        /// <summary>
        /// Enregistre une précision pour un joueur
        /// </summary>
        public static void RecordAccuracy(string playerId, string gameId, float accuracy)
        {
            RecordPlayerMetric(playerId, gameId, "accuracy", accuracy);
        }

        /// <summary>
        /// Enregistre un temps de réaction pour un joueur
        /// </summary>
        public static void RecordReactionTime(string playerId, string gameId, float reactionTime)
        {
            RecordPlayerMetric(playerId, gameId, "reaction_time", reactionTime);
        }

        /// <summary>
        /// Enregistre une action réussie avec précision
        /// </summary>
        public static void RecordSuccessfulAction(string playerId, string gameId, float accuracy = 100f, float reactionTime = 0f)
        {
            var evt = GameAnalyticsEvent.CreatePerformanceEvent(playerId, gameId, 1f, accuracy, reactionTime);
            RecordEvent(evt);
        }

        /// <summary>
        /// Enregistre une action échouée
        /// </summary>
        public static void RecordFailedAction(string playerId, string gameId, float reactionTime = 0f)
        {
            var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ObjectiveFailed, playerId, gameId);
            if (reactionTime > 0) evt.reactionTime = reactionTime;
            RecordEvent(evt);
        }

        /// <summary>
        /// Enregistre un objectif complété
        /// </summary>
        public static void RecordObjectiveCompleted(string playerId, string gameId, float value = 1f, float bonus = 0f)
        {
            var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ObjectiveCompleted, playerId, gameId);
            evt.value = value;
            if (bonus > 0) evt.score = (int)bonus;
            RecordEvent(evt);
        }

        /// <summary>
        /// Enregistre une interaction générique
        /// </summary>
        public static void RecordInteraction(string playerId, string gameId, Vector3 position, string context = "")
        {
            var evt = new GameAnalyticsEvent(GameAnalyticsEventType.InteractionStarted, playerId, gameId);
            evt.position = position;
            evt.context = context;
            RecordEvent(evt);
        }

        #endregion

        #region Advanced Recording

        /// <summary>
        /// Enregistre un événement personnalisé avec données additionnelles
        /// </summary>
        public static void RecordCustomEvent(string playerId, string gameId, string context, 
            Dictionary<string, object> customData = null)
        {
            var evt = new GameAnalyticsEvent(GameAnalyticsEventType.Custom, playerId, gameId);
            evt.context = context;
            if (customData != null)
            {
                evt.customData = new Dictionary<string, object>(customData);
            }
            RecordEvent(evt);
        }

        /// <summary>
        /// Enregistre une session multijoueur
        /// </summary>
        public static void RecordMultiplayerEvent(GameAnalyticsEventType eventType, string playerId, 
            string gameId, List<string> otherPlayers)
        {
            var evt = GameAnalyticsEvent.CreateMultiplayerEvent(eventType, playerId, gameId, otherPlayers);
            RecordEvent(evt);
        }

        #endregion

        #region Player Metrics Access

        /// <summary>
        /// Obtient une métrique spécifique d'un joueur
        /// </summary>
        public static float GetPlayerMetric(string playerId, string metricName)
        {
            if (Analytics != null)
            {
                var playerMetrics = Analytics.GetPlayerMetrics(playerId);
                return playerMetrics?.GetMetric(metricName) ?? 0f;
            }
            return 0f;
        }

        /// <summary>
        /// Obtient le score actuel d'un joueur
        /// </summary>
        public static float GetPlayerScore(string playerId)
        {
            return GetPlayerMetric(playerId, "score");
        }

        /// <summary>
        /// Obtient la précision moyenne d'un joueur
        /// </summary>
        public static float GetPlayerAccuracy(string playerId)
        {
            return GetPlayerMetric(playerId, "accuracy");
        }

        /// <summary>
        /// Obtient le temps de réaction moyen d'un joueur
        /// </summary>
        public static float GetPlayerReactionTime(string playerId)
        {
            return GetPlayerMetric(playerId, "avg_reaction_time");
        }

        /// <summary>
        /// Obtient toutes les métriques d'un joueur
        /// </summary>
        public static Dictionary<string, float> GetAllPlayerMetrics(string playerId)
        {
            if (Analytics != null)
            {
                var playerMetrics = Analytics.GetPlayerMetrics(playerId);
                return playerMetrics?.GetAllMetrics() ?? new Dictionary<string, float>();
            }
            return new Dictionary<string, float>();
        }

        #endregion

        #region Comparisons

        /// <summary>
        /// Compare les performances de deux joueurs
        /// </summary>
        public static bool IsPlayerBetter(string playerId1, string playerId2, string metricName = "score")
        {
            float metric1 = GetPlayerMetric(playerId1, metricName);
            float metric2 = GetPlayerMetric(playerId2, metricName);
            
            // Pour les temps de réaction, plus bas = meilleur
            if (metricName.Contains("time") || metricName.Contains("reaction"))
            {
                return metric1 < metric2;
            }
            
            return metric1 > metric2;
        }

        /// <summary>
        /// Obtient le classement d'un joueur par rapport aux autres
        /// </summary>
        public static int GetPlayerRank(string playerId, List<string> allPlayerIds, string metricName = "score")
        {
            if (Analytics == null) return 0;

            var comparator = new PlayerComparator();
            var rankings = comparator.RankPlayers(allPlayerIds, "current_game", metricName);
            var playerRanking = rankings.FirstOrDefault(r => r.playerId == playerId);
            
            return playerRanking?.rank ?? allPlayerIds.Count;
        }

        /// <summary>
        /// Obtient l'écart de performance entre deux joueurs
        /// </summary>
        public static float GetPerformanceGap(string playerId1, string playerId2, string metricName = "score")
        {
            float metric1 = GetPlayerMetric(playerId1, metricName);
            float metric2 = GetPlayerMetric(playerId2, metricName);
            
            return Mathf.Abs(metric1 - metric2);
        }

        #endregion

        #region Game-Specific Helpers

        /// <summary>
        /// Helpers spécifiques pour le jeu Firefly Dance
        /// </summary>
        public static class FireflyDance
        {
            private const string GAME_ID = "firefly_dance";

            public static void RecordFireflyCaptured(string playerId, int fireflyId, float reactionTime, Vector3 position)
            {
                var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ObjectiveCompleted, playerId, GAME_ID);
                evt.reactionTime = reactionTime;
                evt.position = position;
                evt.value = 1f; // Une luciole capturée
                evt.customData = new Dictionary<string, object> { ["fireflyId"] = fireflyId };
                RecordEvent(evt);
            }

            public static void RecordFireflyMissed(string playerId, int fireflyId, Vector3 position)
            {
                var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ObjectiveFailed, playerId, GAME_ID);
                evt.position = position;
                evt.customData = new Dictionary<string, object> { ["fireflyId"] = fireflyId };
                RecordEvent(evt);
            }

            public static void RecordHandMovement(string playerId, Vector3 handPosition, bool isGrasping)
            {
                var evt = new GameAnalyticsEvent(GameAnalyticsEventType.MovementDetected, playerId, GAME_ID);
                evt.position = handPosition;
                evt.customData = new Dictionary<string, object> { ["isGrasping"] = isGrasping };
                RecordEvent(evt);
            }
        }

        /// <summary>
        /// Helpers spécifiques pour le jeu Log Parade
        /// </summary>
        public static class LogParade
        {
            private const string GAME_ID = "log_parade";

            public static void RecordLogDetected(string playerId, string logType, float confidence, Vector3 position)
            {
                var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ObjectiveCompleted, playerId, GAME_ID);
                evt.accuracy = confidence * 100f;
                evt.position = position;
                evt.customData = new Dictionary<string, object> 
                { 
                    ["logType"] = logType,
                    ["confidence"] = confidence 
                };
                RecordEvent(evt);
            }

            public static void RecordLogMissed(string playerId, string expectedLogType, Vector3 position)
            {
                var evt = new GameAnalyticsEvent(GameAnalyticsEventType.ObjectiveFailed, playerId, GAME_ID);
                evt.position = position;
                evt.customData = new Dictionary<string, object> { ["expectedLogType"] = expectedLogType };
                RecordEvent(evt);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Enregistre un événement dans le système d'analytics
        /// </summary>
        private static void RecordEvent(GameAnalyticsEvent evt)
        {
            if (Analytics != null)
            {
                Analytics.RecordEvent(evt);
            }
        }

        /// <summary>
        /// Vérifie si le système d'analytics est disponible et actif
        /// </summary>
        public static bool IsAnalyticsAvailable()
        {
            return Analytics != null && Analytics.enabled;
        }

        /// <summary>
        /// Force une analyse en temps réel
        /// </summary>
        public static void ForceAnalysis()
        {
            if (Analytics != null)
            {
                Analytics.ForceRealtimeAnalysis();
            }
        }

        /// <summary>
        /// Obtient le nombre de joueurs dans la session active
        /// </summary>
        public static int GetActivePlayerCount()
        {
            // Cette méthode nécessiterait un accès aux données de session dans AnalyticsManager
            return 0; // Placeholder
        }

        /// <summary>
        /// Vérifie si une session est actuellement active
        /// </summary>
        public static bool IsSessionActive()
        {
            // Cette méthode nécessiterait un accès aux données de session dans AnalyticsManager
            return false; // Placeholder
        }

        #endregion

        #region Debug Helpers

#if UNITY_EDITOR
        /// <summary>
        /// Note: Génération de données de test supprimée - Seules les vraies données sont utilisées
        /// </summary>
        [System.Obsolete("Génération de données de test supprimée. Utilisez uniquement les vraies données de jeu.")]
        public static void GenerateTestData(string gameId, List<string> playerIds, int eventCount = 10)
        {
            UnityEngine.Debug.LogWarning("[Analytics] GenerateTestData est obsolète. Le système utilise uniquement des vraies données.");
            // Méthode supprimée - Pas de génération de fausses données
        }

        /// <summary>
        /// Affiche un résumé des métriques de tous les joueurs
        /// </summary>
        public static void DebugPlayerMetrics(List<string> playerIds)
        {
            foreach (string playerId in playerIds)
            {
                var metrics = GetAllPlayerMetrics(playerId);
                UnityEngine.Debug.Log($"[Analytics] {playerId}: {metrics.Count} métriques");
                
                foreach (var metric in metrics)
                {
                    UnityEngine.Debug.Log($"  {metric.Key}: {metric.Value:F2}");
                }
            }
        }
#endif

        #endregion

        #region Game-Specific Helpers

        /// <summary>
        /// Démarre une session pour Firefly Dance - Utilise la session globale existante
        /// </summary>
        public static string StartFireflyDanceSession(string playerName)
        {
            string sessionId = System.Guid.NewGuid().ToString();
            
            // Ne pas créer une nouvelle session globale, juste s'assurer que le joueur est ajouté
            if (GameSessionManager.EnsureInstance() != null)
            {
                GameSessionManager.Instance.AddPlayerToAnalyticsSession(playerName);
                UnityEngine.Debug.Log($"[AnalyticsHelper] Joueur {playerName} ajouté à la session FireflyDance");
            }
            
            return sessionId;
        }

        /// <summary>
        /// Démarre une session pour Log Parade - Utilise la session globale existante
        /// </summary>
        public static string StartLogParadeSession(string playerName)
        {
            string sessionId = System.Guid.NewGuid().ToString();
            
            // Ne pas créer une nouvelle session globale, juste s'assurer que le joueur est ajouté
            if (GameSessionManager.EnsureInstance() != null)
            {
                GameSessionManager.Instance.AddPlayerToAnalyticsSession(playerName);
                UnityEngine.Debug.Log($"[AnalyticsHelper] Joueur {playerName} ajouté à la session LogParade");
            }
            
            return sessionId;
        }

        /// <summary>
        /// Enregistre une luciole capturée avec détails
        /// </summary>
        public static void RecordFireflyCollected(string sessionId, int points)
        {
            string playerId = GetCurrentPlayerFromSession();
            
            // Enregistrer l'action de capture
            RecordSuccessfulAction(playerId, "FireflyDance", 100f, 0f);
            
            // Enregistrer les métriques spécifiques FireflyDance
            RecordPlayerMetric(playerId, "FireflyDance", "lucioles_attrapees", 1f);
            RecordPlayerMetric(playerId, "FireflyDance", "nombre_lucioles_attrapees", 1f);
            RecordPlayerMetric(playerId, "FireflyDance", "points_lucioles", points);
        }

        /// <summary>
        /// Enregistre une luciole manquée
        /// </summary>
        public static void RecordFireflyMissed(string sessionId)
        {
            string playerId = GetCurrentPlayerFromSession();
            RecordPlayerMetric(playerId, "FireflyDance", "lucioles_ratees", 1f);
            RecordPlayerMetric(playerId, "FireflyDance", "nombre_lucioles_ratees", 1f);
        }
        
        /// <summary>
        /// Enregistre une fermeture de main avec résultat détaillé
        /// </summary>
        public static void RecordHandClosure(string sessionId, bool wasSuccessful)
        {
            string playerId = GetCurrentPlayerFromSession();
            
            // Enregistrer le nombre total de fermetures
            RecordPlayerMetric(playerId, "FireflyDance", "nombre_fermetures_main", 1f);
            
            if (wasSuccessful)
            {
                // Fermeture réussie (fermeture + luciole)
                RecordPlayerMetric(playerId, "FireflyDance", "fermetures_reussies", 1f);
                RecordPlayerMetric(playerId, "FireflyDance", "nombre_reussites", 1f);
                UnityEngine.Debug.Log($"[AnalyticsHelper] Fermeture réussie enregistrée pour {playerId}");
            }
            else
            {
                // Fermeture échouée (fermeture sans luciole)
                RecordPlayerMetric(playerId, "FireflyDance", "fermetures_echecs", 1f);
                RecordPlayerMetric(playerId, "FireflyDance", "nombre_echecs", 1f);
                UnityEngine.Debug.Log($"[AnalyticsHelper] Fermeture échouée enregistrée pour {playerId}");
            }
        }

        /// <summary>
        /// Enregistre un score final FireflyDance
        /// </summary>
        public static void RecordFireflyScore(string playerId, int score)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = GetCurrentPlayerFromSession();
            }
            RecordScore(playerId, "FireflyDance", score);
            RecordPlayerMetric(playerId, "FireflyDance", "score_final", score);
            UnityEngine.Debug.Log($"[AnalyticsHelper] Score final FireflyDance enregistré: {score} pour {playerId}");
        }

        #region LogParade Analytics
        
        /// <summary>
        /// Enregistre un score final LogParade
        /// </summary>
        public static void RecordLogParadeScore(string playerId, int score)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = GetCurrentPlayerFromSession();
            }
            RecordScore(playerId, "LogParade", score);
            RecordPlayerMetric(playerId, "LogParade", "score_final", score);
            UnityEngine.Debug.Log($"[AnalyticsHelper] Score final LogParade enregistré: {score} pour {playerId}");
        }
        
        /// <summary>
        /// Enregistre une chute dans l'eau pour LogParade
        /// </summary>
        public static void RecordLogParadeFall(string sessionId)
        {
            string playerId = GetCurrentPlayerFromSession();
            RecordPlayerMetric(playerId, "LogParade", "nombre_chutes_eau", 1f);
            UnityEngine.Debug.Log($"[AnalyticsHelper] Chute dans l'eau enregistrée pour {playerId}");
        }

        #endregion

        #region MusicNote Analytics
        
        /// <summary>
        /// Enregistre une vague de musique jouée pour MusicNote
        /// </summary>
        public static void RecordMusicWavePlayed(string sessionId)
        {
            string playerId = GetCurrentPlayerFromSession();
            RecordPlayerMetric(playerId, "MusicNote", "nombre_vagues_jouees", 1f);
            UnityEngine.Debug.Log($"[AnalyticsHelper] Vague musicale enregistrée pour {playerId}");
        }
        
        /// <summary>
        /// Alias pour RecordMusicWavePlayed (compatibilité)
        /// </summary>
        public static void RecordMusicNoteWave(string sessionId)
        {
            RecordMusicWavePlayed(sessionId);
        }
        
        /// <summary>
        /// Enregistre une note réussie pour MusicNote
        /// </summary>
        public static void RecordMusicNoteHit(string sessionId)
        {
            string playerId = GetCurrentPlayerFromSession();
            RecordPlayerMetric(playerId, "MusicNote", "notes_reussies", 1f);
            RecordPlayerMetric(playerId, "MusicNote", "nombre_notes_reussies", 1f);
            UnityEngine.Debug.Log($"[AnalyticsHelper] Note réussie enregistrée pour {playerId}");
        }
        
        /// <summary>
        /// Alias pour RecordMusicNoteHit (compatibilité)
        /// </summary>
        public static void RecordMusicNoteSuccess(string sessionId)
        {
            RecordMusicNoteHit(sessionId);
        }
        
        /// <summary>
        /// Enregistre un score final MusicNote
        /// </summary>
        public static void RecordMusicNoteScore(string playerId, int score)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = GetCurrentPlayerFromSession();
            }
            RecordScore(playerId, "MusicNote", score);
            RecordPlayerMetric(playerId, "MusicNote", "score_final", score);
            UnityEngine.Debug.Log($"[AnalyticsHelper] Score final MusicNote enregistré: {score} pour {playerId}");
        }
        
        /// <summary>
        /// Enregistre une capture de main (hand closure) dans MusicNote
        /// </summary>
        public static void RecordMusicNoteHandClosure(string playerId, int fingerCount)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = GetCurrentPlayerFromSession();
            }
            
            RecordPlayerMetric(playerId, "MusicNote", "hand_closure", fingerCount);
            
            // Compter aussi le nombre total de captures
            RecordPlayerMetric(playerId, "MusicNote", "total_captures", 1f);
        }
        
        #endregion

        /// <summary>
        /// Termine une session de jeu et retourne les résultats
        /// </summary>
        public static SessionAnalysisResult EndGameSession(string sessionId)
        {
            EndGameSession();
            
            // Simuler un résultat pour l'instant
            return new SessionAnalysisResult
            {
                sessionId = sessionId,
                gameId = "Unknown",
                sessionStart = System.DateTime.Now.AddMinutes(-2),
                sessionEnd = System.DateTime.Now,
                totalDuration = 120f, // 2 minutes
                finalScore = 0,
                totalActions = 1
            };
        }

        #endregion

        #region Game-Specific Methods

        /// <summary>
        /// Démarre une session Music Note
        /// </summary>
        public static void StartMusicNoteSession(List<string> playerIds)
        {
            StartGameSession("MusicNote", playerIds);
        }

        #endregion
    }
}
