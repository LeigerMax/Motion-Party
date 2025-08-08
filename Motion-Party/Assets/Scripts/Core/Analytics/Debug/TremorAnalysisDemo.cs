using UnityEngine;
using System.Collections.Generic;
using Core.Analytics;
using Core.Analytics.Core;
using Core.Analytics.Data;

namespace Core.Analytics.Debug
{
    /// <summary>
    /// Script de test et de démonstration pour l'analyseur de tremblements
    /// Utile pour valider le fonctionnement et calibrer les paramètres
    /// </summary>
    public class TremorAnalysisDemo : MonoBehaviour
    {
        [Header("Configuration de Test")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private bool showRealTimeMetrics = true;
        [SerializeField] private float updateInterval = 1f;
        
        [Header("Simulation de Tremblements")]
        [SerializeField] private bool simulateTremor = false;
        [SerializeField] private float tremorAmplitude = 0.05f;
        [SerializeField] private float tremorFrequency = 6f;
        
        private TremorAnalyzer tremorAnalyzer;
        private float lastUpdateTime;
        private Vector2 basePosition = Vector2.zero;
        private string testPlayerId = "test_player";

        private void Start()
        {
            InitializeTremorAnalysis();
        }

        private void Update()
        {
            if (enableAutoTest)
            {
                SimulateHandMovement();
            }
            
            if (showRealTimeMetrics && Time.time - lastUpdateTime >= updateInterval)
            {
                DisplayMetrics();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Initialise l'analyseur de tremblements pour les tests
        /// </summary>
        private void InitializeTremorAnalysis()
        {
            tremorAnalyzer = new TremorAnalyzer();
            tremorAnalyzer.StartAnalysis();
            
            // Créer un joueur de test dans le système d'analytics
            AnalyticsHelper.StartGameSession("TremorTest", new List<string> { testPlayerId });
            
            UnityEngine.Debug.Log("[TremorAnalysisDemo] Analyseur de tremblements initialisé pour les tests");
        }

        /// <summary>
        /// Simule des mouvements de main avec ou sans tremblements
        /// </summary>
        private void SimulateHandMovement()
        {
            Vector2 simulatedPosition = basePosition;
            
            if (simulateTremor)
            {
                // Ajouter des tremblements sinusoïdaux
                float tremorX = Mathf.Sin(Time.time * tremorFrequency * 2f * Mathf.PI) * tremorAmplitude;
                float tremorY = Mathf.Cos(Time.time * tremorFrequency * 1.8f * Mathf.PI) * tremorAmplitude * 0.7f;
                simulatedPosition += new Vector2(tremorX, tremorY);
            }
            
            // Ajouter un mouvement de base lent
            basePosition.x = Mathf.Sin(Time.time * 0.5f) * 0.2f;
            basePosition.y = Mathf.Cos(Time.time * 0.3f) * 0.15f;
            
            // Envoyer la position à l'analyseur
            tremorAnalyzer.AddHandPosition(simulatedPosition, true);
        }

        /// <summary>
        /// Affiche les métriques en temps réel
        /// </summary>
        private void DisplayMetrics()
        {
            if (tremorAnalyzer == null) return;

            var metrics = tremorAnalyzer.GetTremorMetrics();
            
            UnityEngine.Debug.Log($"[TremorAnalysisDemo] Métriques de Tremblements:\n" +
                     $"  - Intensité: {metrics.tremorIntensity:F2}\n" +
                     $"  - Fréquence: {metrics.tremorFrequency:F2} Hz\n" +
                     $"  - Épisodes: {metrics.tremorEpisodesCount}\n" +
                     $"  - Temps affecté: {metrics.tremorTimePercentage:F1}%\n" +
                     $"  - Durée moyenne: {metrics.averageTremorDuration:F2}s\n" +
                     $"  - Actuellement: {(metrics.isCurrentlyTremoring ? "Oui" : "Non")}");
            
            // Mettre à jour les métriques du joueur
            UpdateAnalyticsMetrics(metrics);
        }

        /// <summary>
        /// Met à jour les métriques dans le système d'analytics
        /// </summary>
        private void UpdateAnalyticsMetrics(TremorMetrics metrics)
        {
            var playerMetrics = AnalyticsManager.Instance?.GetPlayerMetrics(testPlayerId);
            if (playerMetrics != null)
            {
                tremorAnalyzer.UpdatePlayerMetrics(playerMetrics);
            }
        }

        /// <summary>
        /// Génère un rapport complet des tremblements
        /// </summary>
        [ContextMenu("Générer Rapport Tremblements")]
        public void GenerateTremorReport()
        {
            var summary = AnalyticsHelper.GetPlayerTremorSummary(testPlayerId);
            var severity = summary.GetSeverity();
            
            UnityEngine.Debug.Log($"[TremorAnalysisDemo] Rapport Complet:\n" +
                     $"{summary.GenerateReport()}\n" +
                     $"Sévérité: {severity}\n" +
                     $"Recommandations: {GetRecommendations(severity)}");
        }

        /// <summary>
        /// Fournit des recommandations basées sur la sévérité des tremblements
        /// </summary>
        private string GetRecommendations(TremorSeverity severity)
        {
            switch (severity)
            {
                case TremorSeverity.Minimal:
                    return "Tremblements négligeables. Maintenir l'activité physique régulière.";
                
                case TremorSeverity.Mild:
                    return "Tremblements légers détectés. Surveiller l'évolution et envisager des exercices de coordination.";
                
                case TremorSeverity.Moderate:
                    return "Tremblements modérés. Recommander une évaluation médicale et des activités de rééducation motrice.";
                
                case TremorSeverity.Severe:
                    return "Tremblements sévères détectés. Consultation médicale urgente recommandée.";
                
                default:
                    return "Évaluation non disponible.";
            }
        }

        /// <summary>
        /// Réinitialise l'analyse pour un nouveau test
        /// </summary>
        [ContextMenu("Réinitialiser Analyse")]
        public void ResetAnalysis()
        {
            if (tremorAnalyzer != null)
            {
                tremorAnalyzer.Reset();
                tremorAnalyzer.StartAnalysis();
                UnityEngine.Debug.Log("[TremorAnalysisDemo] Analyse réinitialisée");
            }
        }

        /// <summary>
        /// Active/désactive la simulation de tremblements
        /// </summary>
        [ContextMenu("Toggle Simulation Tremblements")]
        public void ToggleTremorSimulation()
        {
            simulateTremor = !simulateTremor;
            UnityEngine.Debug.Log($"[TremorAnalysisDemo] Simulation de tremblements: {(simulateTremor ? "Activée" : "Désactivée")}");
        }

        private void OnGUI()
        {
            if (!showRealTimeMetrics) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Analyse des Tremblements", GUI.skin.box);
            
            if (tremorAnalyzer != null)
            {
                var metrics = tremorAnalyzer.GetTremorMetrics();
                GUILayout.Label($"Intensité: {metrics.tremorIntensity:F2}");
                GUILayout.Label($"Fréquence: {metrics.tremorFrequency:F2} Hz");
                GUILayout.Label($"Épisodes: {metrics.tremorEpisodesCount}");
                GUILayout.Label($"Temps affecté: {metrics.tremorTimePercentage:F1}%");
                GUILayout.Label($"Tremblement actuel: {(metrics.isCurrentlyTremoring ? "Oui" : "Non")}");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Toggle Simulation"))
            {
                ToggleTremorSimulation();
            }
            
            if (GUILayout.Button("Générer Rapport"))
            {
                GenerateTremorReport();
            }
            
            if (GUILayout.Button("Réinitialiser"))
            {
                ResetAnalysis();
            }
            
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (tremorAnalyzer != null)
            {
                tremorAnalyzer.StopAnalysis();
            }
        }
    }
}
