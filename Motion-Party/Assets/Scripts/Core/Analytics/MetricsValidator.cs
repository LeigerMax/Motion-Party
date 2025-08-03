using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Core.Analytics.Core;

namespace Core.Analytics
{
    /// <summary>
    /// Validateur pour s'assurer que toutes les métriques obligatoires sont présentes
    /// </summary>
    public static class MetricsValidator
    {
        #region Required Metrics Definitions

        /// <summary>
        /// Métriques obligatoires pour FireflyDance
        /// </summary>
        public static readonly Dictionary<string, string> FireflyDanceRequiredMetrics = new Dictionary<string, string>
        {
            { "lucioles_attrapees", "Nombre de lucioles attrapées" },
            { "temps_reaction_moyen", "Temps de réaction moyen" },
            { "score_final", "Score final" }
        };

        /// <summary>
        /// Métriques obligatoires pour LogParade
        /// </summary>
        public static readonly Dictionary<string, string> LogParadeRequiredMetrics = new Dictionary<string, string>
        {
            { "rondins_navigues", "Nombre de rondins navigués" },
            { "chutes_eau", "Nombre de chutes dans l'eau" },
            { "score_final", "Score final" }
        };

        /// <summary>
        /// Métriques obligatoires pour MusicNote
        /// </summary>
        public static readonly Dictionary<string, string> MusicNoteRequiredMetrics = new Dictionary<string, string>
        {
            { "vagues_jouees", "Nombre de vagues jouées" },
            { "nombre_vagues_jouees", "Nombre de vagues jouées (alias)" },
            { "notes_reussies", "Nombre de notes réussies" },
            { "nombre_notes_reussies", "Nombre de notes réussies (alias)" },
            { "score_final", "Score final" }
        };

        #endregion

        #region Validation Methods

        /// <summary>
        /// Valide que toutes les métriques obligatoires sont présentes pour un joueur
        /// </summary>
        public static ValidationResult ValidatePlayerMetrics(string playerId, string gameId)
        {
            var result = new ValidationResult { PlayerId = playerId, GameId = gameId };
            
            var requiredMetrics = GetRequiredMetricsForGame(gameId);
            if (requiredMetrics == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Jeu non reconnu: {gameId}");
                return result;
            }

            // Obtenir les métriques du joueur depuis l'AnalyticsManager
            var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(playerId);
            if (playerMetrics == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Métriques non trouvées pour le joueur {playerId}");
                return result;
            }

            var allPlayerMetrics = playerMetrics.GetAllMetrics();
            
            // Vérifier chaque métrique obligatoire
            foreach (var requiredMetric in requiredMetrics)
            {
                string metricKey = $"{gameId}_{requiredMetric.Key}";
                bool found = allPlayerMetrics.ContainsKey(metricKey) || allPlayerMetrics.ContainsKey(requiredMetric.Key);
                
                if (!found)
                {
                    result.MissingMetrics.Add(requiredMetric.Key);
                    result.Errors.Add($"Métrique manquante: {requiredMetric.Key} ({requiredMetric.Value})");
                }
                else
                {
                    result.PresentMetrics.Add(requiredMetric.Key);
                }
            }

            result.IsValid = result.MissingMetrics.Count == 0;
            return result;
        }

        /// <summary>
        /// Valide toutes les sessions actives
        /// </summary>
        public static List<ValidationResult> ValidateAllActiveSessions()
        {
            var results = new List<ValidationResult>();
            
            if (GameSessionManager.Instance == null || !GameSessionManager.Instance.IsAnalyticsSessionActive())
            {
                UnityEngine.Debug.LogWarning("[MetricsValidator] Aucune session analytics active");
                return results;
            }

            var currentSession = GameSessionManager.Instance.GetCurrentAnalyticsSession();
            if (currentSession == null)
            {
                UnityEngine.Debug.LogWarning("[MetricsValidator] Session analytics nulle");
                return results;
            }

            var players = currentSession.GetAllPlayers();
            foreach (string playerId in players)
            {
                // Valider pour chaque jeu
                foreach (string gameId in new[] { "FireflyDance", "LogParade", "MusicNote" })
                {
                    var validation = ValidatePlayerMetrics(playerId, gameId);
                    if (!validation.IsValid || validation.MissingMetrics.Count > 0)
                    {
                        results.Add(validation);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Ajoute les métriques manquantes avec des valeurs par défaut
        /// </summary>
        public static void AddMissingMetricsWithDefaults(string playerId, string gameId)
        {
            var validation = ValidatePlayerMetrics(playerId, gameId);
            if (validation.IsValid) return;

            UnityEngine.Debug.Log($"[MetricsValidator] Ajout de {validation.MissingMetrics.Count} métriques manquantes pour {playerId} dans {gameId}");

            foreach (string missingMetric in validation.MissingMetrics)
            {
                // Ajouter la métrique avec une valeur par défaut (0)
                AnalyticsHelper.RecordPlayerMetric(playerId, gameId, missingMetric, 0f);
                UnityEngine.Debug.Log($"[MetricsValidator] Métrique ajoutée: {missingMetric} = 0");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Obtient les métriques obligatoires pour un jeu donné
        /// </summary>
        private static Dictionary<string, string> GetRequiredMetricsForGame(string gameId)
        {
            switch (gameId.ToLower().Replace("minigame_", ""))
            {
                case "fireflydance":
                case "firefly":
                    return FireflyDanceRequiredMetrics;
                case "logparade":
                case "log":
                    return LogParadeRequiredMetrics;
                case "musicnote":
                case "music":
                    return MusicNoteRequiredMetrics;
                default:
                    UnityEngine.Debug.LogWarning($"[MetricsValidator] Jeu non reconnu: {gameId}");
                    return null;
            }
        }

        /// <summary>
        /// Affiche un rapport de validation dans les logs
        /// </summary>
        public static void LogValidationReport(List<ValidationResult> results)
        {
            if (results.Count == 0)
            {
                UnityEngine.Debug.Log("✅ [MetricsValidator] Toutes les métriques requises sont présentes");
                return;
            }

            UnityEngine.Debug.LogWarning($"⚠️ [MetricsValidator] {results.Count} problème(s) de métriques détecté(s):");
            
            foreach (var result in results)
            {
                UnityEngine.Debug.LogWarning($"🎮 {result.GameId} - 👤 {result.PlayerId}:");
                UnityEngine.Debug.LogWarning($"  ✅ Présentes: {result.PresentMetrics.Count}");
                UnityEngine.Debug.LogWarning($"  ❌ Manquantes: {result.MissingMetrics.Count} ({string.Join(", ", result.MissingMetrics)})");
                
                foreach (string error in result.Errors)
                {
                    UnityEngine.Debug.LogWarning($"    - {error}");
                }
            }
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Résultat de validation des métriques
    /// </summary>
    public class ValidationResult
    {
        public string PlayerId { get; set; }
        public string GameId { get; set; }
        public bool IsValid { get; set; }
        public List<string> MissingMetrics { get; set; } = new List<string>();
        public List<string> PresentMetrics { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    #endregion
}
