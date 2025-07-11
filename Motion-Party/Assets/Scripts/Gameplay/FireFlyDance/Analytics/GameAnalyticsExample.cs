using UnityEngine;
using Gameplay.FireFlyDance.Analytics;
using System.Collections.Generic;

namespace Gameplay.FireFlyDance.Examples
{
    /// <summary>
    /// Exemple d'utilisation du système GameAnalytics
    /// Montre comment intégrer et utiliser le système de statistiques
    /// </summary>
    public class GameAnalyticsExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStatsRecorder statsRecorder;
        
        [Header("Test Controls")]
        [SerializeField] private bool enableTestMode = false;
        [SerializeField] private string testPlayerName = "Joueur Test";

        private void Start()
        {
            // Trouver automatiquement le GameStatsRecorder si non assigné
            if (statsRecorder == null)
            {
                statsRecorder = FindFirstObjectByType<GameStatsRecorder>();
            }

            if (statsRecorder == null)
            {
                Debug.LogWarning("GameStatsRecorder non trouvé dans la scène");
                return;
            }

            Debug.Log("GameAnalyticsExample initialisé avec succès");

            // Mode test pour démonstration
            if (enableTestMode)
            {
                DemonstrateAnalyticsUsage();
            }
        }

        /// <summary>
        /// Démontre l'utilisation basique du système d'analyse
        /// </summary>
        private void DemonstrateAnalyticsUsage()
        {
            Debug.Log("=== DÉMONSTRATION GameAnalytics ===");

            // 1. Démarrer manuellement l'enregistrement
            if (!statsRecorder.enabled)
            {
                Debug.Log("Activation du GameStatsRecorder...");
                statsRecorder.SetEnabled(true);
            }

            // 2. Démarrer une session d'enregistrement
            Debug.Log($"Démarrage d'une session pour {testPlayerName}");
            statsRecorder.StartRecording(testPlayerName);

            // 3. Programmer l'arrêt après quelques secondes pour la démo
            Invoke(nameof(StopRecordingDemo), 5f);
        }

        /// <summary>
        /// Termine la démonstration d'enregistrement
        /// </summary>
        private void StopRecordingDemo()
        {
            Debug.Log("Arrêt de la session de démonstration...");

            // Terminer l'enregistrement
            statsRecorder.EndRecording();

            // Afficher le résumé
            string summary = statsRecorder.GetCurrentSessionSummary();
            Debug.Log($"Résumé de session:\n{summary}");

            // Exporter les données
            string jsonData = statsRecorder.ExportCurrentSession();
            if (!string.IsNullOrEmpty(jsonData))
            {
                Debug.Log("Données JSON exportées avec succès");
                Debug.Log($"Dossier de sauvegarde: {statsRecorder.GetSaveDirectory()}");
            }
        }

        /// <summary>
        /// Méthodes utiles que vous pouvez appeler depuis d'autres scripts
        /// </summary>
        
        /// <summary>
        /// Démarre l'analyse pour un joueur spécifique
        /// </summary>
        public void StartAnalysisForPlayer(string playerName)
        {
            if (statsRecorder != null)
            {
                statsRecorder.StartRecording(playerName);
                Debug.Log($"Analyse démarrée pour {playerName}");
            }
        }

        /// <summary>
        /// Termine l'analyse et retourne un résumé
        /// </summary>
        public string FinishAnalysisAndGetSummary()
        {
            if (statsRecorder == null) return "Pas de GameStatsRecorder disponible";

            statsRecorder.EndRecording();
            return statsRecorder.GetCurrentSessionSummary();
        }

        /// <summary>
        /// Active ou désactive le système d'analyse
        /// </summary>
        public void ToggleAnalytics(bool enabled)
        {
            if (statsRecorder != null)
            {
                statsRecorder.SetEnabled(enabled);
                Debug.Log($"Analytics {(enabled ? "activé" : "désactivé")}");
            }
        }

        /// <summary>
        /// Force la sauvegarde des données actuelles
        /// </summary>
        public void ForceSaveCurrentSession()
        {
            if (statsRecorder != null)
            {
                statsRecorder.SaveCurrentSession();
                Debug.Log("Session sauvegardée manuellement");
            }
        }

        /// <summary>
        /// Ouvre le dossier de sauvegarde dans l'explorateur (Windows uniquement)
        /// </summary>
        [ContextMenu("Ouvrir Dossier Sauvegarde")]
        public void OpenSaveDirectory()
        {
            if (statsRecorder != null)
            {
                string path = statsRecorder.GetSaveDirectory();
                
                if (System.IO.Directory.Exists(path))
                {
                    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    System.Diagnostics.Process.Start("explorer.exe", path.Replace('/', '\\'));
                    #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    System.Diagnostics.Process.Start("open", path);
                    #endif
                    
                    Debug.Log($"Ouverture du dossier: {path}");
                }
                else
                {
                    Debug.LogWarning($"Dossier non trouvé: {path}");
                }
            }
        }

        #region Test Methods (utilisables depuis l'Inspector)

        [ContextMenu("Test - Démarrer Analyse")]
        private void TestStartAnalysis()
        {
            StartAnalysisForPlayer(testPlayerName);
        }

        [ContextMenu("Test - Terminer Analyse")]
        private void TestFinishAnalysis()
        {
            string summary = FinishAnalysisAndGetSummary();
            Debug.Log($"Résumé Test:\n{summary}");
        }

        [ContextMenu("Test - Toggle Analytics")]
        private void TestToggleAnalytics()
        {
            if (statsRecorder != null)
            {
                ToggleAnalytics(!statsRecorder.enabled);
            }
        }

        [ContextMenu("Test - Afficher État")]
        private void TestShowStatus()
        {
            if (statsRecorder == null)
            {
                Debug.Log("❌ GameStatsRecorder non trouvé");
                return;
            }

            Debug.Log($"=== ÉTAT GameAnalytics ===");
            Debug.Log($"Activé: {statsRecorder.enabled}");
            Debug.Log($"En cours d'enregistrement: {statsRecorder.gameObject.activeInHierarchy}");
            Debug.Log($"Dossier sauvegarde: {statsRecorder.GetSaveDirectory()}");
            
            // Vérifier si le dossier existe
            bool folderExists = System.IO.Directory.Exists(statsRecorder.GetSaveDirectory());
            Debug.Log($"Dossier existe: {folderExists}");
            
            if (folderExists)
            {
                var files = System.IO.Directory.GetFiles(statsRecorder.GetSaveDirectory(), "*.json");
                Debug.Log($"Fichiers JSON trouvés: {files.Length}");
            }
        }

        [ContextMenu("Test - Analyse Démographique")]
        private void TestDemographicAnalysis()
        {
            Debug.Log("=== ANALYSE DÉMOGRAPHIQUE ===");

            // Créer des sessions d'exemple avec différents âges
            var sessions = new List<SessionData>();

            // Senior de 72 ans
            var seniorSession = CreateSampleSession("Marie", "1952-03-15");
            seniorSession.averageReactionTime = 3.1f;
            seniorSession.capturedFireflies = 12;
            seniorSession.totalFireflies = 20;
            sessions.Add(seniorSession);

            // Adulte mature de 55 ans
            var matureSession = CreateSampleSession("Jean", "1968-08-22");
            matureSession.averageReactionTime = 2.4f;
            matureSession.capturedFireflies = 16;
            matureSession.totalFireflies = 20;
            sessions.Add(matureSession);

            // Jeune adulte de 28 ans
            var youngSession = CreateSampleSession("Alice", "1995-12-05");
            youngSession.averageReactionTime = 1.9f;
            youngSession.capturedFireflies = 18;
            youngSession.totalFireflies = 20;
            sessions.Add(youngSession);

            // Génération des rapports
            foreach (var session in sessions)
            {
                Debug.Log("\n" + SessionAnalyzer.GenerateDetailedReport(session));
            }

            // Analyse comparative par âge
            Debug.Log("\n" + SessionAnalyzer.CompareSessionsByAgeGroup(sessions));

            // Export CSV avec données démographiques
            string csvData = SessionAnalyzer.ExportToCSV(sessions);
            Debug.Log("\n=== EXPORT CSV ===");
            Debug.Log(csvData);
        }

        /// <summary>
        /// Crée une session d'exemple avec informations démographiques
        /// </summary>
        private SessionData CreateSampleSession(string playerName, string birthDate)
        {
            var session = new SessionData();
            session.playerName = playerName;
            session.playerBirthDate = birthDate;
            session.playerAge = session.CalculatePlayerAge();
            session.ageGroup = session.DetermineAgeGroup(session.playerAge);
            
            // Données de base
            session.totalDuration = 120f;
            session.totalFireflies = 20;
            session.missedFireflies = session.totalFireflies - session.capturedFireflies;
            session.minReactionTime = 1.2f;
            session.maxReactionTime = 4.5f;
            session.totalHandClosures = 25;
            session.emptyHandClosures = 8;
            session.handMovementDistance = 150f;
            session.totalMovements = 180;
            session.emptyMovements = 45;

            return session;
        }

        #endregion
    }
}
