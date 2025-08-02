using UnityEngine;
using System.IO;
using System.Linq;
using Core.Analytics.Core;

namespace Core.Analytics.Debug
{
    /// <summary>
    /// Outils de débogage et de nettoyage pour le système d'analytics
    /// </summary>
    public static class AnalyticsDebugTools
    {
        /// <summary>
        /// Nettoie tous les fichiers de session pour éviter les conflits
        /// </summary>
        public static void CleanupSessionFiles()
        {
            string analyticsPath = Path.Combine(UnityEngine.Application.persistentDataPath, "AnalyticsReports");
            
            if (!Directory.Exists(analyticsPath))
            {
                UnityEngine.Debug.Log("[AnalyticsDebugTools] Aucun dossier d'analytics à nettoyer");
                return;
            }

            try
            {
                // Nettoyer les dossiers Sessions et Players
                CleanDirectory(Path.Combine(analyticsPath, "Sessions"));
                CleanDirectory(Path.Combine(analyticsPath, "Players"));
                
                UnityEngine.Debug.Log("[AnalyticsDebugTools] Nettoyage terminé avec succès");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsDebugTools] Erreur lors du nettoyage: {e.Message}");
            }
        }
        /// </summary>
        [System.Obsolete("Utilisez cette méthode uniquement pour déboguer")]
        public static void CleanAllSessionFiles()
        {
            string analyticsPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            
            if (!Directory.Exists(analyticsPath))
            {
                UnityEngine.Debug.Log("[AnalyticsDebugTools] Aucun dossier d'analytics à nettoyer");
                return;
            }

            try
            {
                // Nettoyer les dossiers Sessions et Players
                CleanDirectory(Path.Combine(analyticsPath, "Sessions"));
                CleanDirectory(Path.Combine(analyticsPath, "Players"));
                
                UnityEngine.Debug.Log("✅ [AnalyticsDebugTools] Tous les fichiers d'analytics ont été nettoyés");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsDebugTools] Erreur lors du nettoyage: {e.Message}");
            }
        }

        /// <summary>
        /// Affiche les informations sur les fichiers existants
        /// </summary>
        public static void ShowExistingFiles()
        {
            string analyticsPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            
            if (!Directory.Exists(analyticsPath))
            {
                UnityEngine.Debug.Log("[AnalyticsDebugTools] Aucun dossier d'analytics trouvé");
                return;
            }

            UnityEngine.Debug.Log("=== FICHIERS D'ANALYTICS EXISTANTS ===");
            
            // Sessions
            string sessionsPath = Path.Combine(analyticsPath, "Sessions");
            if (Directory.Exists(sessionsPath))
            {
                var sessionFiles = Directory.GetFiles(sessionsPath, "*.json").OrderByDescending(f => File.GetLastWriteTime(f));
                UnityEngine.Debug.Log($"📁 Sessions ({sessionFiles.Count()} fichiers):");
                foreach (var file in sessionFiles.Take(5)) // Afficher seulement les 5 plus récents
                {
                    var info = new FileInfo(file);
                    UnityEngine.Debug.Log($"  - {info.Name} (taille: {info.Length} bytes, modifié: {info.LastWriteTime})");
                }
                if (sessionFiles.Count() > 5)
                {
                    UnityEngine.Debug.Log($"  ... et {sessionFiles.Count() - 5} autre(s) fichier(s)");
                }
            }

            // Players
            string playersPath = Path.Combine(analyticsPath, "Players");
            if (Directory.Exists(playersPath))
            {
                var playerFiles = Directory.GetFiles(playersPath, "*.json").OrderByDescending(f => File.GetLastWriteTime(f));
                UnityEngine.Debug.Log($"👤 Joueurs ({playerFiles.Count()} fichiers):");
                foreach (var file in playerFiles.Take(5)) // Afficher seulement les 5 plus récents
                {
                    var info = new FileInfo(file);
                    UnityEngine.Debug.Log($"  - {info.Name} (taille: {info.Length} bytes, modifié: {info.LastWriteTime})");
                }
                if (playerFiles.Count() > 5)
                {
                    UnityEngine.Debug.Log($"  ... et {playerFiles.Count() - 5} autre(s) fichier(s)");
                }
            }
        }

        /// <summary>
        /// Valide la session analytics actuelle
        /// </summary>
        public static void ValidateCurrentSession()
        {
            if (GameSessionManager.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[AnalyticsDebugTools] GameSessionManager.Instance est null");
                return;
            }

            bool isActive = GameSessionManager.Instance.IsAnalyticsSessionActive();
            UnityEngine.Debug.Log($"🔍 [AnalyticsDebugTools] Session analytics active: {isActive}");

            if (isActive)
            {
                var session = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                if (session != null)
                {
                    var players = session.GetAllPlayers();
                    UnityEngine.Debug.Log($"👥 Joueurs dans la session: {players.Count} ({string.Join(", ", players)})");
                    
                    // Valider les métriques
                    var validationResults = MetricsValidator.ValidateAllActiveSessions();
                    MetricsValidator.LogValidationReport(validationResults);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[AnalyticsDebugTools] Session active mais nulle");
                }
            }
        }

        /// <summary>
        /// Force la génération d'un rapport avec les données actuelles
        /// </summary>
        public static void ForceGenerateReport()
        {
            if (GameSessionManager.Instance == null || !GameSessionManager.Instance.IsAnalyticsSessionActive())
            {
                UnityEngine.Debug.LogWarning("[AnalyticsDebugTools] Aucune session active");
                return;
            }

            try
            {
                GameSessionManager.Instance.CompleteAnalyticsSession();
                UnityEngine.Debug.Log("✅ [AnalyticsDebugTools] Rapport forcé généré");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsDebugTools] Erreur génération rapport: {e.Message}");
            }
        }

        /// <summary>
        /// Affiche les métriques d'un joueur spécifique
        /// </summary>
        public static void ShowPlayerMetrics(string playerId)
        {
            if (AnalyticsManager.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[AnalyticsDebugTools] AnalyticsManager.Instance est null");
                return;
            }

            var playerMetrics = AnalyticsManager.Instance.GetPlayerMetrics(playerId);
            if (playerMetrics == null)
            {
                UnityEngine.Debug.LogWarning($"[AnalyticsDebugTools] Aucune métrique trouvée pour {playerId}");
                return;
            }

            var allMetrics = playerMetrics.GetAllMetrics();
            UnityEngine.Debug.Log($"📊 [AnalyticsDebugTools] Métriques pour {playerId} ({allMetrics.Count()} métriques):");
            
            foreach (var metric in allMetrics.OrderBy(m => m.Key))
            {
                UnityEngine.Debug.Log($"  - {metric.Key}: {metric.Value}");
            }
        }

        #region Private Methods

        private static void CleanDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                UnityEngine.Debug.Log($"[AnalyticsDebugTools] Dossier inexistant: {directoryPath}");
                return;
            }

            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    UnityEngine.Debug.Log($"[AnalyticsDebugTools] Fichier supprimé: {Path.GetFileName(file)}");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[AnalyticsDebugTools] Impossible de supprimer {Path.GetFileName(file)}: {e.Message}");
                }
            }
        }

        #endregion
    }
}
