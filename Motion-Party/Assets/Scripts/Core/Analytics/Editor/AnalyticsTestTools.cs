using UnityEngine;
using UnityEditor;

namespace Core.Analytics.Editor
{
    /// <summary>
    /// Outils d'éditeur pour tester le système d'analytics
    /// </summary>
    public static class AnalyticsTestTools
    {
        [MenuItem("Analytics/Test Generate Report")]
        public static void TestGenerateReport()
        {
            if (Application.isPlaying)
            {
                var gameSessionManager = GameSessionManager.Instance;
                if (gameSessionManager != null)
                {
                    gameSessionManager.TestGenerateAnalyticsReport();
                    UnityEngine.Debug.Log("[AnalyticsTestTools] Test de génération de rapport lancé");
                }
                else
                {
                    UnityEngine.Debug.LogError("[AnalyticsTestTools] GameSessionManager.Instance introuvable");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[AnalyticsTestTools] Le jeu doit être en cours d'exécution pour tester les analytics");
            }
        }

        [MenuItem("Analytics/Debug Session")]
        public static void DebugSession()
        {
            if (Application.isPlaying)
            {
                var gameSessionManager = GameSessionManager.Instance;
                if (gameSessionManager != null)
                {
                    gameSessionManager.DebugSessionMetrics();
                }
                else
                {
                    UnityEngine.Debug.LogError("[AnalyticsTestTools] GameSessionManager.Instance introuvable");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[AnalyticsTestTools] Le jeu doit être en cours d'exécution pour débugger");
            }
        }

        [MenuItem("Analytics/Open Reports Folder")]
        public static void OpenReportsFolder()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            
            if (System.IO.Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path.Replace('/', '\\'));
                UnityEngine.Debug.Log($"[AnalyticsTestTools] Dossier ouvert: {path}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[AnalyticsTestTools] Dossier n'existe pas encore: {path}");
            }
        }
    }
}
