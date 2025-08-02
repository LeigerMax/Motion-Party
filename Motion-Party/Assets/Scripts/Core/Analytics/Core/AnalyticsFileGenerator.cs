using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Core.Analytics.Core;
using Core.Analytics.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Générateur automatique de fichiers de résultats
    /// </summary>
    public class AnalyticsFileGenerator : MonoBehaviour
    {
        [Header("Configuration Fichiers")]
        [SerializeField] private string outputDirectory = "AnalyticsReports";
        [SerializeField] private bool generateCSV = false; // Désactivé - CSV non nécessaire
        [SerializeField] private bool generateJSON = true;
        [SerializeField] private bool generateHTML = false; // Généré seulement à la fin de session

        private static AnalyticsFileGenerator _instance;
        public static AnalyticsFileGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AnalyticsFileGenerator>();
                    if (_instance == null)
                    {
                        var go = new GameObject("AnalyticsFileGenerator");
                        _instance = go.AddComponent<AnalyticsFileGenerator>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeOutputDirectory();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Dynamic File Management

        /// <summary>
        /// Initialise les fichiers de session dès le début pour écriture dynamique
        /// DÉSACTIVÉ - Utilisation du système JSON LIVE uniquement
        /// </summary>
        public void InitializeSessionFiles(string sessionId, SessionAnalyzer session)
        {
            // Désactivé - Le système JSON LIVE se charge de tout
            UnityEngine.Debug.Log($"[AnalyticsFileGenerator] InitializeSessionFiles ignoré - Utilisation du système JSON LIVE pour session {sessionId}");
            return;

            /*
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string sessionFileName = $"Session_{sessionId}_{timestamp}";
                
                // Créer le fichier de session globale
                InitializeGlobalSessionFile(sessionFileName, session);
                
                // Créer les fichiers individuels pour chaque joueur
                foreach (string playerId in session.GetAllPlayers())
                {
                    InitializePlayerFile(sessionId, playerId, timestamp);
                }
                
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Fichiers initialisés pour session {sessionId}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur initialisation fichiers: {e.Message}");
            }
            */
        }

        /// <summary>
        /// Initialise le fichier de session globale
        /// </summary>
        private void InitializeGlobalSessionFile(string baseFileName, SessionAnalyzer session)
        {
            if (generateCSV)
            {
                string filePath = GetOutputPath("Sessions", $"{baseFileName}.csv");
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("# Rapport de Session Motion Party - Données en temps réel");
                    writer.WriteLine($"# Session ID: {session.GetSessionId()}");
                    writer.WriteLine($"# Démarrage: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine("# Ce fichier est mis à jour dynamiquement après chaque mini-jeu");
                    writer.WriteLine();
                    writer.WriteLine("# === DONNÉES GLOBALES DE SESSION ===");
                    writer.WriteLine("Timestamp,Event,Data");
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},SESSION_START,Démarrage de la session");
                }
            }
        }

        /// <summary>
        /// Initialise un fichier joueur individuel
        /// </summary>
        private void InitializePlayerFile(string sessionId, string playerId, string timestamp)
        {
            if (generateCSV)
            {
                string fileName = $"Player_{playerId}_{sessionId}_{timestamp}.csv";
                string filePath = GetOutputPath("Players", fileName);
                
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("# Rapport Joueur Motion Party - Données en temps réel");
                    writer.WriteLine($"# Joueur: {playerId}");
                    writer.WriteLine($"# Session: {sessionId}");
                    writer.WriteLine($"# Démarrage: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine("# Ce fichier est mis à jour après chaque mini-jeu");
                    writer.WriteLine();
                    writer.WriteLine("# === DONNÉES INDIVIDUELLES ===");
                    writer.WriteLine("Timestamp,MiniGame,Metric,Value,Details");
                }
            }
        }

        /// <summary>
        /// Met à jour les fichiers après chaque mini-jeu
        /// DÉSACTIVÉ - Utilisation du système JSON LIVE uniquement
        /// </summary>
        public void UpdateSessionFilesAfterMiniGame(string sessionId, SessionAnalyzer session, string miniGameName)
        {
            // Désactivé - Le système JSON LIVE se charge de tout
            UnityEngine.Debug.Log($"[AnalyticsFileGenerator] UpdateSessionFilesAfterMiniGame ignoré - Utilisation du système JSON LIVE pour {miniGameName}");
            return;

            /*
            try
            {
                // Mettre à jour le fichier global
                UpdateGlobalSessionFile(sessionId, session, miniGameName);
                
                // Mettre à jour les fichiers de chaque joueur
                foreach (string playerId in session.GetAllPlayers())
                {
                    UpdatePlayerFileAfterMiniGame(sessionId, playerId, session, miniGameName);
                }
                
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Fichiers mis à jour après {miniGameName}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur mise à jour fichiers: {e.Message}");
            }
            */
        }

        /// <summary>
        /// Met à jour le fichier de session globale
        /// </summary>
        private void UpdateGlobalSessionFile(string sessionId, SessionAnalyzer session, string miniGameName)
        {
            // Trouver le fichier de session existant
            string sessionPattern = $"Session_{sessionId}_*.csv";
            string sessionDir = GetOutputPath("Sessions", "");
            
            var sessionFiles = Directory.GetFiles(sessionDir, sessionPattern);
            if (sessionFiles.Length > 0)
            {
                string filePath = sessionFiles[0]; // Prendre le premier trouvé
                
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    var analysis = session.AnalyzeSession();
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},MINIGAME_COMPLETED,{miniGameName}");
                    
                    // Ajouter les moyennes actuelles
                    foreach (var avg in analysis.sessionAverages)
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},SESSION_AVERAGE,{avg.Key}={avg.Value:F2}");
                    }
                    
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},PLAYER_COUNT,{session.GetAllPlayers().Count}");
                }
            }
        }

        /// <summary>
        /// Met à jour le fichier d'un joueur spécifique
        /// </summary>
        private void UpdatePlayerFileAfterMiniGame(string sessionId, string playerId, SessionAnalyzer session, string miniGameName)
        {
            // Trouver le fichier du joueur existant
            string playerPattern = $"Player_{playerId}_{sessionId}_*.csv";
            string playerDir = GetOutputPath("Players", "");
            
            var playerFiles = Directory.GetFiles(playerDir, playerPattern);
            if (playerFiles.Length > 0)
            {
                string filePath = playerFiles[0]; // Prendre le premier trouvé
                
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    var playerAnalysis = session.AnalyzePlayer(playerId);
                    if (playerAnalysis != null)
                    {
                        // Ajouter les métriques du joueur pour ce mini-jeu
                        foreach (var metric in playerAnalysis.playerMetrics)
                        {
                            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{miniGameName},{metric.Key},{metric.Value:F2},Updated after mini-game");
                        }
                        
                        // Ajouter les performances spécifiques au mini-jeu si disponibles
                        if (playerAnalysis.miniGamePerformances.TryGetValue(miniGameName, out var gamePerf))
                        {
                            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{miniGameName},Score,{gamePerf.score:F1},Mini-game score");
                            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{miniGameName},Accuracy,{gamePerf.accuracy * 100:F1}%,Mini-game accuracy");
                            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{miniGameName},CompletionTime,{gamePerf.completionTime:F1}s,Mini-game duration");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalise les fichiers de session avec les données complètes de fin
        /// DÉSACTIVÉ - Utilisation du système JSON LIVE uniquement
        /// </summary>
        public void FinalizeSessionFiles(string sessionId, SessionAnalysisResult finalAnalysis)
        {
            // Désactivé - Le système JSON LIVE se charge de tout
            UnityEngine.Debug.Log($"[AnalyticsFileGenerator] FinalizeSessionFiles ignoré - Utilisation du système JSON LIVE pour session {sessionId}");
            return;

            /*
            try
            {
                // Finaliser le fichier de session globale
                FinalizeGlobalSessionFile(sessionId, finalAnalysis);
                
                // Finaliser les fichiers individuels pour chaque joueur
                foreach (string playerId in finalAnalysis.playerIds)
                {
                    var playerAnalysis = finalAnalysis.playerAnalyses.TryGetValue(playerId, out var analysis) ? analysis : null;
                    if (playerAnalysis != null)
                    {
                        FinalizePlayerFile(sessionId, playerId, playerAnalysis, finalAnalysis);
                    }
                }
                
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Fichiers finalisés pour session {sessionId}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur finalisation fichiers: {e.Message}");
            }
            */
        }

        /// <summary>
        /// Finalise le fichier de session globale avec les données complètes
        /// </summary>
        private void FinalizeGlobalSessionFile(string sessionId, SessionAnalysisResult finalAnalysis)
        {
            // Trouver le fichier de session existant
            string sessionPattern = $"Session_{sessionId}_*.csv";
            string sessionDir = GetOutputPath("Sessions", "");
            
            var sessionFiles = Directory.GetFiles(sessionDir, sessionPattern);
            if (sessionFiles.Length > 0)
            {
                string filePath = sessionFiles[0];
                
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine();
                    writer.WriteLine("# === DONNÉES FINALES DE SESSION ===");
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},SESSION_END,Session terminée");
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},TOTAL_DURATION,{finalAnalysis.sessionDuration:F1} minutes");
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},COMPLETION_RATE,{finalAnalysis.completionRate * 100:F1}%");
                    
                    // Moyennes finales de session
                    writer.WriteLine();
                    writer.WriteLine("# Moyennes finales");
                    foreach (var avg in finalAnalysis.sessionAverages)
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},FINAL_AVERAGE,{avg.Key}={avg.Value:F2}");
                    }
                    
                    // Insights de session
                    writer.WriteLine();
                    writer.WriteLine("# Insights de session");
                    foreach (var insight in finalAnalysis.sessionInsights)
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},INSIGHT,\"{insight.description}\"");
                    }
                }
            }
        }

        /// <summary>
        /// Finalise le fichier d'un joueur avec ses données complètes
        /// </summary>
        private void FinalizePlayerFile(string sessionId, string playerId, PlayerAnalysisResult playerAnalysis, SessionAnalysisResult sessionAnalysis)
        {
            // Trouver le fichier du joueur existant
            string playerPattern = $"Player_{playerId}_{sessionId}_*.csv";
            string playerDir = GetOutputPath("Players", "");
            
            var playerFiles = Directory.GetFiles(playerDir, playerPattern);
            if (playerFiles.Length > 0)
            {
                string filePath = playerFiles[0];
                
                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    writer.WriteLine();
                    writer.WriteLine("# === DONNÉES FINALES JOUEUR ===");
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},SESSION_END,Session terminée pour {playerId}");
                    
                    // Métriques finales du joueur
                    writer.WriteLine();
                    writer.WriteLine("# Métriques finales");
                    foreach (var metric in playerAnalysis.playerMetrics)
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},FINAL_METRIC,{metric.Key},{metric.Value:F2},Final value");
                    }
                    
                    // Performance vs moyenne
                    writer.WriteLine();
                    writer.WriteLine("# Performance vs moyenne session");
                    foreach (var vs in playerAnalysis.performanceVsAverage)
                    {
                        string comparison = vs.Value > 1.1f ? "Above" : vs.Value < 0.9f ? "Below" : "Average";
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},VS_AVERAGE,{vs.Key},{vs.Value:F2}x,{comparison}");
                    }
                    
                    // Recommandations personnalisées
                    writer.WriteLine();
                    writer.WriteLine("# Recommandations personnalisées");
                    foreach (var recommendation in playerAnalysis.recommendations)
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},RECOMMENDATION,\"{recommendation}\"");
                    }
                }
            }
        }

        #endregion

        #region Directory Management

        /// <summary>
        /// Initialise le répertoire de sortie
        /// </summary>
        private void InitializeOutputDirectory()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, outputDirectory);
            
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Répertoire créé: {fullPath}");
            }

            // Créer les sous-répertoires
            CreateSubDirectory("Sessions");
            CreateSubDirectory("Players");
            CreateSubDirectory("Archives");
        }

        private void CreateSubDirectory(string subDir)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, outputDirectory, subDir);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private string GetOutputPath(string subDirectory, string fileName)
        {
            return Path.Combine(Application.persistentDataPath, outputDirectory, subDirectory, fileName);
        }

        #endregion

        #region Session Reports

        /// <summary>
        /// Génère un rapport complet de session
        /// </summary>
        public void GenerateSessionReport(SessionAnalysisResult sessionResult)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string baseFileName = $"Session_{sessionResult.sessionId}_{timestamp}";

            try
            {
                if (generateCSV)
                {
                    GenerateSessionCSV(sessionResult, baseFileName);
                }

                if (generateJSON)
                {
                    GenerateSessionJSON(sessionResult, baseFileName);
                }

                if (generateHTML)
                {
                    GenerateSessionHTML(sessionResult, baseFileName);
                }

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Rapport de session généré: {sessionResult.sessionId}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur génération rapport session: {e.Message}");
            }
        }

        /// <summary>
        /// Génère un fichier CSV pour la session
        /// </summary>
        private void GenerateSessionCSV(SessionAnalysisResult sessionResult, string baseFileName)
        {
            string filePath = GetOutputPath("Sessions", $"{baseFileName}.csv");
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // En-têtes
                writer.WriteLine("Rapport de Session Motion Party");
                writer.WriteLine($"Session ID,{sessionResult.sessionId}");
                writer.WriteLine($"Date Début,{sessionResult.startTime:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Date Fin,{sessionResult.endTime:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Durée (min),{sessionResult.sessionDuration:F2}");
                writer.WriteLine($"Nombre de Joueurs,{sessionResult.playerCount}");
                writer.WriteLine($"Taux de Completion,{sessionResult.completionRate * 100:F1}%");
                writer.WriteLine();

                // Moyennes de session
                writer.WriteLine("Moyennes de Session");
                writer.WriteLine("Métrique,Valeur Moyenne");
                foreach (var avg in sessionResult.sessionAverages)
                {
                    writer.WriteLine($"{avg.Key},{avg.Value:F2}");
                }
                writer.WriteLine();

                // Performances par joueur
                writer.WriteLine("Performances par Joueur");
                writer.WriteLine("Joueur ID,Score,Précision,Temps Completion,Performance Globale");
                
                foreach (var playerAnalysis in sessionResult.playerAnalyses)
                {
                    var metrics = playerAnalysis.Value.playerMetrics;
                    float score = metrics.TryGetValue("score", out float s) ? s : 0;
                    float accuracy = metrics.TryGetValue("accuracy", out float a) ? a : 0;
                    float completion = metrics.TryGetValue("completion_time", out float c) ? c : 0;
                    float performance = CalculateOverallPerformance(metrics);
                    
                    writer.WriteLine($"{playerAnalysis.Key},{score:F1},{accuracy * 100:F1}%,{completion:F1}s,{performance * 100:F1}%");
                }

                // Insights de session
                writer.WriteLine();
                writer.WriteLine("Insights de Session");
                foreach (var insight in sessionResult.sessionInsights)
                {
                    writer.WriteLine($"\"{insight}\"");
                }
            }
        }

        /// <summary>
        /// Génère un fichier JSON pour la session
        /// </summary>
        private void GenerateSessionJSON(SessionAnalysisResult sessionResult, string baseFileName)
        {
            string filePath = GetOutputPath("Sessions", $"{baseFileName}.json");
            string jsonData = JsonUtility.ToJson(sessionResult, true);
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Génère un rapport HTML pour la session
        /// </summary>
        private void GenerateSessionHTML(SessionAnalysisResult sessionResult, string baseFileName)
        {
            string filePath = GetOutputPath("Sessions", $"{baseFileName}.html");
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html><head>");
                writer.WriteLine("<title>Rapport Session Motion Party</title>");
                writer.WriteLine("<style>");
                writer.WriteLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                writer.WriteLine("h1 { color: #2c3e50; }");
                writer.WriteLine("h2 { color: #3498db; }");
                writer.WriteLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
                writer.WriteLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                writer.WriteLine("th { background-color: #f2f2f2; }");
                writer.WriteLine(".insight { background-color: #f8f9fa; padding: 10px; margin: 10px 0; border-left: 4px solid #007bff; }");
                writer.WriteLine("</style>");
                writer.WriteLine("</head><body>");

                // Titre et infos générales
                writer.WriteLine($"<h1>🎮 Rapport Session Motion Party</h1>");
                writer.WriteLine($"<p><strong>Session ID:</strong> {sessionResult.sessionId}</p>");
                writer.WriteLine($"<p><strong>Période:</strong> {sessionResult.startTime:yyyy-MM-dd HH:mm:ss} → {sessionResult.endTime:yyyy-MM-dd HH:mm:ss}</p>");
                writer.WriteLine($"<p><strong>Durée:</strong> {sessionResult.sessionDuration:F1} minutes</p>");
                writer.WriteLine($"<p><strong>Joueurs:</strong> {sessionResult.playerCount}</p>");
                writer.WriteLine($"<p><strong>Completion:</strong> {sessionResult.completionRate * 100:F1}%</p>");

                // Moyennes de session
                writer.WriteLine("<h2>📊 Moyennes de Session</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Métrique</th><th>Valeur Moyenne</th></tr>");
                foreach (var avg in sessionResult.sessionAverages)
                {
                    writer.WriteLine($"<tr><td>{avg.Key}</td><td>{avg.Value:F2}</td></tr>");
                }
                writer.WriteLine("</table>");

                // Performances joueurs
                writer.WriteLine("<h2>👥 Performances des Joueurs</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Joueur</th><th>Score</th><th>Précision</th><th>Temps</th><th>Performance</th></tr>");
                
                foreach (var playerAnalysis in sessionResult.playerAnalyses)
                {
                    var metrics = playerAnalysis.Value.playerMetrics;
                    float score = metrics.TryGetValue("score", out float s) ? s : 0;
                    float accuracy = metrics.TryGetValue("accuracy", out float a) ? a : 0;
                    float completion = metrics.TryGetValue("completion_time", out float c) ? c : 0;
                    float performance = CalculateOverallPerformance(metrics);
                    
                    writer.WriteLine($"<tr>");
                    writer.WriteLine($"<td>{playerAnalysis.Key}</td>");
                    writer.WriteLine($"<td>{score:F1}</td>");
                    writer.WriteLine($"<td>{accuracy * 100:F1}%</td>");
                    writer.WriteLine($"<td>{completion:F1}s</td>");
                    writer.WriteLine($"<td>{performance * 100:F1}%</td>");
                    writer.WriteLine($"</tr>");
                }
                writer.WriteLine("</table>");

                // Insights
                writer.WriteLine("<h2>💡 Insights de Session</h2>");
                foreach (var insight in sessionResult.sessionInsights)
                {
                    writer.WriteLine($"<div class='insight'>{insight}</div>");
                }

                writer.WriteLine("</body></html>");
            }
        }

        #endregion

        #region Player Reports

        /// <summary>
        /// Génère un rapport HTML final pour un joueur à partir du JSON LIVE
        /// </summary>
        public void GeneratePlayerReport(PlayerAnalysisResult playerResult)
        {
            try
            {
                // Générer seulement le rapport HTML final à partir du JSON LIVE
                GeneratePlayerHtmlFromLiveJson(playerResult.playerId, playerResult.sessionId);
                
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Rapport HTML final généré pour: {playerResult.playerId}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur génération rapport HTML final: {e.Message}");
            }
        }

        /// <summary>
        /// Génère un rapport HTML final à partir du fichier JSON LIVE existant
        /// </summary>
        private void GeneratePlayerHtmlFromLiveJson(string playerId, string sessionId)
        {
            try
            {
                string liveJsonPath = Path.Combine(GetOutputPath("Players", ""), $"Player_{playerId}_{sessionId}_LIVE.json");
                
                if (!File.Exists(liveJsonPath))
                {
                    UnityEngine.Debug.LogWarning($"[AnalyticsFileGenerator] Fichier JSON LIVE introuvable: {liveJsonPath}");
                    return;
                }

                // Lire le JSON LIVE
                string jsonContent = File.ReadAllText(liveJsonPath);
                var playerData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);

                if (playerData == null)
                {
                    UnityEngine.Debug.LogError("[AnalyticsFileGenerator] Impossible de désérialiser le JSON LIVE");
                    return;
                }

                // Extraire les métriques
                var metrics = new Dictionary<string, object>();
                if (playerData.ContainsKey("metrics"))
                {
                    if (playerData["metrics"] is Newtonsoft.Json.Linq.JObject jObj)
                    {
                        metrics = jObj.ToObject<Dictionary<string, object>>();
                    }
                }

                var gameStats = new Dictionary<string, object>();
                if (playerData.ContainsKey("gameStats"))
                {
                    if (playerData["gameStats"] is Newtonsoft.Json.Linq.JObject jObj)
                    {
                        gameStats = jObj.ToObject<Dictionary<string, object>>();
                    }
                }

                // Générer le HTML final
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string htmlFileName = $"Player_{playerId}_FINAL_{timestamp}.html";
                string htmlPath = Path.Combine(GetOutputPath("Players", ""), htmlFileName);

                GeneratePlayerHtmlFromData(playerData, playerId, sessionId, htmlPath, metrics, gameStats);
                
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] ✅ Rapport HTML final créé: {htmlFileName}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur création HTML final: {e.Message}");
            }
        }

        /// <summary>
        /// Génère le contenu HTML avec les vraies données du joueur
        /// </summary>
        private void GeneratePlayerHtmlFromData(Dictionary<string, object> playerData, string playerId, string sessionId, string htmlPath, Dictionary<string, object> metrics, Dictionary<string, object> gameStats)
        {
            var html = new System.Text.StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>Rapport Final Motion Party</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #333; }");
            html.AppendLine(".container { background: white; padding: 30px; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.3); max-width: 1200px; margin: 0 auto; }");
            html.AppendLine("h1 { color: #2c3e50; text-align: center; margin-bottom: 30px; }");
            html.AppendLine("h2 { color: #3498db; border-bottom: 2px solid #3498db; padding-bottom: 10px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine("th { background-color: #3498db; color: white; }");
            html.AppendLine(".game-section { background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 10px; border-left: 5px solid #3498db; }");
            html.AppendLine(".metric-value { font-weight: bold; color: #27ae60; }");
            html.AppendLine(".score-highlight { background-color: #ffeaa7; padding: 5px 10px; border-radius: 5px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<div class='container'>");
            
            html.AppendLine($"<h1>🎮 Rapport Final Motion Party</h1>");
            html.AppendLine($"<p><strong>👤 Joueur:</strong> {playerId}</p>");
            html.AppendLine($"<p><strong>🎯 Session:</strong> {sessionId}</p>");
            html.AppendLine($"<p><strong>📅 Généré le:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            // Analyser les données par mini-jeu
            AnalyzeAndDisplayGameMetrics(html, metrics, gameStats);

            html.AppendLine("</div>");
            html.AppendLine("</body></html>");

            File.WriteAllText(htmlPath, html.ToString());
        }

        /// <summary>
        /// Analyse et affiche les métriques par mini-jeu
        /// </summary>
        private void AnalyzeAndDisplayGameMetrics(System.Text.StringBuilder html, Dictionary<string, object> metrics, Dictionary<string, object> gameStats)
        {
            html.AppendLine("<h2>📊 Résultats par Mini-Jeu</h2>");

            // FireflyDance
            DisplayFireflyDanceMetrics(html, metrics, gameStats);
            
            // LogParade  
            DisplayLogParadeMetrics(html, metrics, gameStats);
            
            // MusicNote
            DisplayMusicNoteMetrics(html, metrics, gameStats);

            // Résumé global
            DisplayGlobalSummary(html, metrics, gameStats);
        }

        private void DisplayFireflyDanceMetrics(System.Text.StringBuilder html, Dictionary<string, object> metrics, Dictionary<string, object> gameStats)
        {
            html.AppendLine("<div class='game-section'>");
            html.AppendLine("<h3>🔥 FireflyDance - Capture de Lucioles</h3>");
            
            var attrapees = GetMetricValue(metrics, "FireflyDance_nombre_lucioles_attrapees", 0f);
            var ratees = GetMetricValue(metrics, "FireflyDance_nombre_lucioles_ratees", 0f);
            var fermetures = GetMetricValue(metrics, "FireflyDance_nombre_fermetures_main", 0f);
            var reussites = GetMetricValue(metrics, "FireflyDance_nombre_reussites", 0f);
            var echecs = GetMetricValue(metrics, "FireflyDance_nombre_echecs", 0f);
            var scoreFinal = GetMetricValue(metrics, "FireflyDance_score_final", 0f);

            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Métrique</th><th>Valeur</th></tr>");
            html.AppendLine($"<tr><td>🟢 Lucioles Attrapées</td><td class='metric-value'>{attrapees}</td></tr>");
            html.AppendLine($"<tr><td>🔴 Lucioles Ratées</td><td class='metric-value'>{ratees}</td></tr>");
            html.AppendLine($"<tr><td>✋ Fermetures de Main</td><td class='metric-value'>{fermetures}</td></tr>");
            html.AppendLine($"<tr><td>✅ Succès</td><td class='metric-value'>{reussites}</td></tr>");
            html.AppendLine($"<tr><td>❌ Échecs</td><td class='metric-value'>{echecs}</td></tr>");
            html.AppendLine($"<tr><td>🏆 Score Final</td><td class='score-highlight'>{scoreFinal} points</td></tr>");
            html.AppendLine("</table>");

            // Calculer le taux de réussite
            float totalTentatives = attrapees + ratees;
            if (totalTentatives > 0)
            {
                float tauxReussite = (attrapees / totalTentatives) * 100;
                html.AppendLine($"<p><strong>📈 Taux de Réussite:</strong> {tauxReussite:F1}%</p>");
            }

            html.AppendLine("</div>");
        }

        private void DisplayLogParadeMetrics(System.Text.StringBuilder html, Dictionary<string, object> metrics, Dictionary<string, object> gameStats)
        {
            html.AppendLine("<div class='game-section'>");
            html.AppendLine("<h3>🌊 LogParade - Équilibre sur Rondins</h3>");
            
            var chutes = GetMetricValue(metrics, "LogParade_nombre_chutes_eau", 0f);

            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Métrique</th><th>Valeur</th></tr>");
            html.AppendLine($"<tr><td>💧 Chutes dans l'Eau</td><td class='metric-value'>{chutes}</td></tr>");
            html.AppendLine("</table>");

            if (chutes == 0)
            {
                html.AppendLine("<p><strong>🏆 Excellent!</strong> Aucune chute dans l'eau!</p>");
            }
            else if (chutes <= 3)
            {
                html.AppendLine("<p><strong>👍 Bien joué!</strong> Peu de chutes, bon équilibre!</p>");
            }
            else
            {
                html.AppendLine("<p><strong>💪 Continue!</strong> L'équilibre s'améliore avec la pratique!</p>");
            }

            html.AppendLine("</div>");
        }

        private void DisplayMusicNoteMetrics(System.Text.StringBuilder html, Dictionary<string, object> metrics, Dictionary<string, object> gameStats)
        {
            html.AppendLine("<div class='game-section'>");
            html.AppendLine("<h3>🎵 MusicNote - Rythme Musical</h3>");
            
            var vagues = GetMetricValue(metrics, "MusicNote_nombre_vagues_jouees", 0f);
            var handClosure = GetMetricValue(metrics, "MusicNote_hand_closure", 0f);
            var totalCaptures = GetMetricValue(metrics, "MusicNote_total_captures", 0f);
            var scoreFinal = GetMetricValue(metrics, "MusicNote_score_final", 0f);

            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Métrique</th><th>Valeur</th></tr>");
            html.AppendLine($"<tr><td>🌊 Vagues Jouées</td><td class='metric-value'>{vagues}</td></tr>");
            html.AppendLine($"<tr><td>✋ Fermetures Main</td><td class='metric-value'>{handClosure}</td></tr>");
            html.AppendLine($"<tr><td>🎯 Total Captures</td><td class='metric-value'>{totalCaptures}</td></tr>");
            html.AppendLine($"<tr><td>🏆 Score Final</td><td class='score-highlight'>{scoreFinal} points</td></tr>");
            html.AppendLine("</table>");

            html.AppendLine("</div>");
        }

        private void DisplayGlobalSummary(System.Text.StringBuilder html, Dictionary<string, object> metrics, Dictionary<string, object> gameStats)
        {
            html.AppendLine("<div class='game-section'>");
            html.AppendLine("<h3>🏆 Résumé Global</h3>");

            var fireflyScore = GetMetricValue(metrics, "FireflyDance_score_final", 0f);
            var musicScore = GetMetricValue(metrics, "MusicNote_score_final", 0f);
            var totalScore = fireflyScore + musicScore;

            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Mini-Jeu</th><th>Score</th></tr>");
            html.AppendLine($"<tr><td>🔥 FireflyDance</td><td class='metric-value'>{fireflyScore} pts</td></tr>");
            html.AppendLine($"<tr><td>🎵 MusicNote</td><td class='metric-value'>{musicScore} pts</td></tr>");
            html.AppendLine($"<tr><td><strong>🏆 TOTAL</strong></td><td class='score-highlight'><strong>{totalScore} points</strong></td></tr>");
            html.AppendLine("</table>");

            // Recommandations
            html.AppendLine("<h4>💡 Recommandations</h4>");
            if (totalScore >= 50)
            {
                html.AppendLine("<p>🌟 <strong>Excellent joueur!</strong> Continuez sur cette lancée!</p>");
            }
            else if (totalScore >= 20)
            {
                html.AppendLine("<p>👍 <strong>Bon travail!</strong> Vous progressez bien!</p>");
            }
            else
            {
                html.AppendLine("<p>💪 <strong>Continuez à vous entraîner!</strong> Chaque partie vous fait progresser!</p>");
            }

            html.AppendLine("</div>");
        }

        /// <summary>
        /// Utilitaire pour récupérer une valeur métrique de façon sécurisée
        /// </summary>
        private float GetMetricValue(Dictionary<string, object> metrics, string key, float defaultValue)
        {
            if (metrics.ContainsKey(key))
            {
                if (float.TryParse(metrics[key].ToString(), out float value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        private void GeneratePlayerCSV(PlayerAnalysisResult playerResult, string baseFileName)
        {
            string filePath = GetOutputPath("Players", $"{baseFileName}.csv");
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Rapport Joueur Motion Party");
                writer.WriteLine($"Joueur ID,{playerResult.playerId}");
                writer.WriteLine($"Session ID,{playerResult.sessionId}");
                writer.WriteLine($"Date Analyse,{playerResult.analysisTime:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                // Métriques du joueur
                writer.WriteLine("Métriques du Joueur");
                writer.WriteLine("Métrique,Valeur,Performance vs Moyenne");
                foreach (var metric in playerResult.playerMetrics)
                {
                    float vsAvg = playerResult.performanceVsAverage.TryGetValue(metric.Key, out float vs) ? vs : 1.0f;
                    string comparison = vsAvg > 1.1f ? "Au-dessus" : vsAvg < 0.9f ? "En-dessous" : "Moyenne";
                    writer.WriteLine($"{metric.Key},{metric.Value:F2},{comparison} ({vsAvg:F2}x)");
                }
                writer.WriteLine();

                // Performances par mini-jeu
                writer.WriteLine("Performances par Mini-Jeu");
                writer.WriteLine("Jeu,Score,Précision,Temps");
                foreach (var miniGame in playerResult.miniGamePerformances)
                {
                    writer.WriteLine($"{miniGame.Key},{miniGame.Value.score:F1},{miniGame.Value.accuracy * 100:F1}%,{miniGame.Value.completionTime:F1}s");
                }
                writer.WriteLine();

                // Recommandations
                writer.WriteLine("Recommandations Personnalisées");
                foreach (var recommendation in playerResult.recommendations)
                {
                    writer.WriteLine($"\"{recommendation}\"");
                }
            }
        }

        private void GeneratePlayerJSON(PlayerAnalysisResult playerResult, string baseFileName)
        {
            string filePath = GetOutputPath("Players", $"{baseFileName}.json");
            string jsonData = JsonUtility.ToJson(playerResult, true);
            File.WriteAllText(filePath, jsonData);
        }

        private void GeneratePlayerHTML(PlayerAnalysisResult playerResult, string baseFileName)
        {
            string filePath = GetOutputPath("Players", $"{baseFileName}.html");
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html><head>");
                writer.WriteLine("<title>Rapport Joueur Motion Party</title>");
                writer.WriteLine("<style>");
                writer.WriteLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                writer.WriteLine("h1 { color: #2c3e50; }");
                writer.WriteLine("h2 { color: #3498db; }");
                writer.WriteLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
                writer.WriteLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                writer.WriteLine("th { background-color: #f2f2f2; }");
                writer.WriteLine(".recommendation { background-color: #e8f5e8; padding: 10px; margin: 10px 0; border-left: 4px solid #28a745; }");
                writer.WriteLine(".above-avg { color: #28a745; font-weight: bold; }");
                writer.WriteLine(".below-avg { color: #dc3545; font-weight: bold; }");
                writer.WriteLine("</style>");
                writer.WriteLine("</head><body>");

                writer.WriteLine($"<h1>👤 Rapport Joueur: {playerResult.playerId}</h1>");
                writer.WriteLine($"<p><strong>Session:</strong> {playerResult.sessionId}</p>");
                writer.WriteLine($"<p><strong>Analysé le:</strong> {playerResult.analysisTime:yyyy-MM-dd HH:mm:ss}</p>");

                // Métriques vs moyenne
                writer.WriteLine("<h2>📊 Performance vs Moyenne de Session</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Métrique</th><th>Votre Performance</th><th>vs Moyenne</th></tr>");
                foreach (var metric in playerResult.playerMetrics)
                {
                    float vsAvg = playerResult.performanceVsAverage.TryGetValue(metric.Key, out float vs) ? vs : 1.0f;
                    string cssClass = vsAvg > 1.1f ? "above-avg" : vsAvg < 0.9f ? "below-avg" : "";
                    string comparison = vsAvg > 1.1f ? "🔥 Au-dessus" : vsAvg < 0.9f ? "📈 En-dessous" : "📊 Moyenne";
                    
                    writer.WriteLine($"<tr>");
                    writer.WriteLine($"<td>{metric.Key}</td>");
                    writer.WriteLine($"<td>{metric.Value:F2}</td>");
                    writer.WriteLine($"<td class='{cssClass}'>{comparison} ({vsAvg:F2}x)</td>");
                    writer.WriteLine($"</tr>");
                }
                writer.WriteLine("</table>");

                // Performances par mini-jeu
                writer.WriteLine("<h2>🎮 Performances par Mini-Jeu</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Jeu</th><th>Score</th><th>Précision</th><th>Temps</th></tr>");
                foreach (var miniGame in playerResult.miniGamePerformances)
                {
                    writer.WriteLine($"<tr>");
                    writer.WriteLine($"<td>{miniGame.Key}</td>");
                    writer.WriteLine($"<td>{miniGame.Value.score:F1}</td>");
                    writer.WriteLine($"<td>{miniGame.Value.accuracy * 100:F1}%</td>");
                    writer.WriteLine($"<td>{miniGame.Value.completionTime:F1}s</td>");
                    writer.WriteLine($"</tr>");
                }
                writer.WriteLine("</table>");

                // Recommandations
                writer.WriteLine("<h2>💡 Recommandations Personnalisées</h2>");
                foreach (var recommendation in playerResult.recommendations)
                {
                    writer.WriteLine($"<div class='recommendation'>{recommendation}</div>");
                }

                writer.WriteLine("</body></html>");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calcule la performance globale basée sur les métriques
        /// </summary>
        private float CalculateOverallPerformance(Dictionary<string, float> metrics)
        {
            float score = metrics.TryGetValue("score", out float s) ? Mathf.Clamp01(s / 1000f) : 0f;
            float accuracy = metrics.TryGetValue("accuracy", out float a) ? a : 0f;
            float reactionTime = metrics.TryGetValue("avg_reaction_time", out float r) ? Mathf.Clamp01(2f - r) / 2f : 0f;
            
            return (score * 0.4f + accuracy * 0.4f + reactionTime * 0.2f);
        }

        /// <summary>
        /// Archive les anciens rapports
        /// </summary>
        public void ArchiveOldReports(int daysToKeep = 30)
        {
            // Implémenter la logique d'archivage si nécessaire
        }

        #endregion

        #region Incremental JSON System

        private string _currentSessionJsonPath = "";

        /// <summary>
        /// Crée le fichier JSON de session incrémental
        /// </summary>
        public void CreateIncrementalSessionJson(string sessionId, SessionAnalyzer session)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"Session_{sessionId}_{timestamp}_LIVE.json";
                _currentSessionJsonPath = GetOutputPath("Sessions", fileName);

                var sessionData = new
                {
                    sessionId = sessionId,
                    startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    players = session.GetAllPlayers(),
                    status = "IN_PROGRESS",
                    miniGames = new List<object>(),
                    currentMiniGame = "",
                    metrics = new Dictionary<string, object>()
                };

                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sessionData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_currentSessionJsonPath, jsonData);

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Fichier JSON incrémental créé: {_currentSessionJsonPath}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur création JSON incrémental: {e.Message}");
            }
        }

        /// <summary>
        /// Met à jour le fichier JSON de session avec les données d'un mini-jeu
        /// </summary>
        public void UpdateIncrementalSessionJson(string sessionId, string miniGameName, Dictionary<string, object> miniGameData)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentSessionJsonPath) || !File.Exists(_currentSessionJsonPath))
                {
                    UnityEngine.Debug.LogWarning($"[AnalyticsFileGenerator] Fichier JSON incrémental introuvable, création d'un nouveau fichier");
                    return;
                }

                // Lire le JSON existant
                string existingJson = File.ReadAllText(_currentSessionJsonPath);
                var sessionData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(existingJson);

                // Mettre à jour avec les nouvelles données
                sessionData["currentMiniGame"] = miniGameName;
                sessionData["lastUpdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Ajouter les données du mini-jeu
                var miniGames = sessionData.ContainsKey("miniGames") 
                    ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(sessionData["miniGames"].ToString())
                    : new List<object>();

                var miniGameEntry = new
                {
                    name = miniGameName,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    data = miniGameData
                };

                miniGames.Add(miniGameEntry);
                sessionData["miniGames"] = miniGames;

                // Sauvegarder le JSON mis à jour
                string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(sessionData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_currentSessionJsonPath, updatedJson);

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] JSON incrémental mis à jour avec {miniGameName}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur mise à jour JSON incrémental: {e.Message}");
            }
        }

        /// <summary>
        /// Finalise la session et génère le rapport HTML final
        /// </summary>
        public void FinalizeIncrementalSession(string sessionId, SessionAnalyzer session)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentSessionJsonPath) || !File.Exists(_currentSessionJsonPath))
                {
                    UnityEngine.Debug.LogWarning($"[AnalyticsFileGenerator] Aucun fichier JSON incrémental à finaliser");
                    return;
                }

                // Lire le JSON final
                string existingJson = File.ReadAllText(_currentSessionJsonPath);
                var sessionData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(existingJson);

                // Marquer comme terminé
                sessionData["status"] = "COMPLETED";
                sessionData["endTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sessionData["finalizedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Sauvegarder le JSON final
                string finalJson = Newtonsoft.Json.JsonConvert.SerializeObject(sessionData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_currentSessionJsonPath, finalJson);

                // Note: Le rapport HTML final sera généré uniquement à la fin de session via GeneratePlayerReport()
                // GenerateHtmlFromIncrementalJson(sessionId, sessionData); // Commenté - HTML généré seulement à la fin

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Session finalisée - JSON LIVE mis à jour");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur finalisation session: {e.Message}");
            }
        }

        /// <summary>
        /// Génère un rapport HTML basé sur le JSON incrémental
        /// </summary>
        private void GenerateHtmlFromIncrementalJson(string sessionId, Dictionary<string, object> sessionData)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string htmlFileName = $"Session_{sessionId}_{timestamp}_FINAL.html";
                string htmlPath = GetOutputPath("Sessions", htmlFileName);

                using (StreamWriter writer = new StreamWriter(htmlPath))
                {
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine("<html><head>");
                    writer.WriteLine("<title>Motion Party - Rapport Final de Session</title>");
                    writer.WriteLine("<style>");
                    writer.WriteLine("body { font-family: Arial, sans-serif; margin: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #333; }");
                    writer.WriteLine(".container { background: white; padding: 30px; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.2); }");
                    writer.WriteLine("h1 { color: #4a5568; text-align: center; font-size: 2.5em; margin-bottom: 30px; }");
                    writer.WriteLine("h2 { color: #2d3748; border-bottom: 3px solid #4299e1; padding-bottom: 10px; }");
                    writer.WriteLine("h3 { color: #4a5568; }");
                    writer.WriteLine(".game-section { background: #f7fafc; padding: 20px; margin: 20px 0; border-radius: 10px; border-left: 5px solid #4299e1; }");
                    writer.WriteLine(".metric { background: #edf2f7; padding: 10px; margin: 5px 0; border-radius: 5px; display: flex; justify-content: space-between; }");
                    writer.WriteLine(".metric-label { font-weight: bold; }");
                    writer.WriteLine(".metric-value { color: #2b6cb0; font-weight: bold; }");
                    writer.WriteLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
                    writer.WriteLine("th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e2e8f0; }");
                    writer.WriteLine("th { background: #4299e1; color: white; }");
                    writer.WriteLine("tr:hover { background: #f7fafc; }");
                    writer.WriteLine(".status { display: inline-block; padding: 5px 15px; border-radius: 20px; font-weight: bold; }");
                    writer.WriteLine(".status.completed { background: #c6f6d5; color: #22543d; }");
                    writer.WriteLine("</style>");
                    writer.WriteLine("</head><body>");
                    writer.WriteLine("<div class='container'>");

                    // En-tête
                    writer.WriteLine($"<h1>🎮 Motion Party - Session {sessionData.GetValueOrDefault("sessionId", "Unknown")}</h1>");
                    writer.WriteLine($"<div class='status completed'>Session Terminée</div>");
                    
                    // Informations générales
                    writer.WriteLine("<h2>📊 Informations de Session</h2>");
                    writer.WriteLine("<div class='metric'>");
                    writer.WriteLine($"<span class='metric-label'>Démarrage:</span><span class='metric-value'>{sessionData.GetValueOrDefault("startTime", "N/A")}</span>");
                    writer.WriteLine("</div>");
                    writer.WriteLine("<div class='metric'>");
                    writer.WriteLine($"<span class='metric-label'>Fin:</span><span class='metric-value'>{sessionData.GetValueOrDefault("endTime", "N/A")}</span>");
                    writer.WriteLine("</div>");

                    // Joueurs
                    var players = sessionData.ContainsKey("players") ? 
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(sessionData["players"].ToString()) : 
                        new List<string>();
                    
                    writer.WriteLine("<h2>👥 Joueurs</h2>");
                    writer.WriteLine("<table>");
                    writer.WriteLine("<tr><th>Joueur</th><th>Statut</th></tr>");
                    foreach (var player in players)
                    {
                        writer.WriteLine($"<tr><td>{player}</td><td><span class='status completed'>Actif</span></td></tr>");
                    }
                    writer.WriteLine("</table>");

                    // Mini-jeux
                    var miniGames = sessionData.ContainsKey("miniGames") ? 
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(sessionData["miniGames"].ToString()) : 
                        new List<object>();

                    writer.WriteLine("<h2>🎯 Mini-Jeux Joués</h2>");
                    
                    foreach (var miniGameObj in miniGames)
                    {
                        var miniGame = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(miniGameObj.ToString());
                        string gameName = miniGame.GetValueOrDefault("name", "Unknown").ToString();
                        string gameTime = miniGame.GetValueOrDefault("timestamp", "Unknown").ToString();
                        
                        writer.WriteLine($"<div class='game-section'>");
                        writer.WriteLine($"<h3>🎮 {gameName}</h3>");
                        writer.WriteLine($"<p><strong>Heure:</strong> {gameTime}</p>");
                        
                        if (miniGame.ContainsKey("data"))
                        {
                            var gameData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(miniGame["data"].ToString());
                            foreach (var kvp in gameData)
                            {
                                writer.WriteLine("<div class='metric'>");
                                writer.WriteLine($"<span class='metric-label'>{kvp.Key}:</span><span class='metric-value'>{kvp.Value}</span>");
                                writer.WriteLine("</div>");
                            }
                        }
                        writer.WriteLine("</div>");
                    }

                    writer.WriteLine("</div></body></html>");
                }

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Rapport HTML final généré: {htmlPath}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur génération HTML final: {e.Message}");
            }
        }

        /// <summary>
        /// Met à jour les fichiers JSON des joueurs en temps réel
        /// </summary>
        public void UpdatePlayerJsonIncrementally(string sessionId, string playerId, string gameId, string metricName, float value)
        {
            try
            {
                // Utiliser un nom de fichier fixe pour permettre les mises à jour incrémentales
                string fileName = $"Player_{playerId}_{sessionId}_LIVE.json";
                string filePath = GetOutputPath("Players", fileName);

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 🔧 DÉBUT UpdatePlayerJsonIncrementally - {gameId}.{metricName} = {value}");
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 📁 Fichier: {filePath}");
                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 📄 Fichier existe: {File.Exists(filePath)}");

                // Structure des données joueur incrémentales
                var playerData = new Dictionary<string, object>();

                // Charger les données existantes si le fichier existe
                if (File.Exists(filePath))
                {
                    try
                    {
                        string existingJson = File.ReadAllText(filePath);
                        UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 📖 JSON existant lu: {existingJson.Length} caractères");
                        playerData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(existingJson) ?? new Dictionary<string, object>();
                        UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 📊 Données existantes chargées: {playerData.Count} clés");
                        
                        // Afficher les métriques existantes
                        if (playerData.ContainsKey("metrics"))
                        {
                            var existingMetrics = playerData["metrics"] as Dictionary<string, object>;
                            UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 📈 Métriques existantes: {existingMetrics?.Count ?? 0}");
                            if (existingMetrics != null)
                            {
                                foreach (var metric in existingMetrics)
                                {
                                    UnityEngine.Debug.Log($"[AnalyticsFileGenerator]   - {metric.Key}: {metric.Value}");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogWarning($"[AnalyticsFileGenerator] Erreur lecture fichier joueur existant: {e.Message}");
                        playerData = new Dictionary<string, object>();
                    }
                }
                else
                {
                    UnityEngine.Debug.Log($"[AnalyticsFileGenerator] 🆕 Création nouveau fichier joueur");
                    // Initialiser le fichier joueur
                    playerData["playerId"] = playerId;
                    playerData["sessionId"] = sessionId;
                    playerData["lastUpdated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    playerData["metrics"] = new Dictionary<string, object>();
                    playerData["gameStats"] = new Dictionary<string, object>();
                }

                // Mettre à jour les métriques - CORRECTION du problème de désérialisation
                var metrics = new Dictionary<string, object>();
                if (playerData.ContainsKey("metrics"))
                {
                    try
                    {
                        // Convertir depuis JObject vers Dictionary si nécessaire
                        if (playerData["metrics"] is Newtonsoft.Json.Linq.JObject jObj)
                        {
                            metrics = jObj.ToObject<Dictionary<string, object>>();
                        }
                        else if (playerData["metrics"] is Dictionary<string, object> dict)
                        {
                            metrics = dict;
                        }
                        else
                        {
                            // Fallback: re-sérialiser/désérialiser pour forcer la conversion
                            string metricsJson = Newtonsoft.Json.JsonConvert.SerializeObject(playerData["metrics"]);
                            metrics = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(metricsJson);
                        }
                        UnityEngine.Debug.Log($"[AnalyticsFileGenerator] ✅ Métriques récupérées: {metrics.Count} éléments");
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[AnalyticsFileGenerator] ⚠️ Erreur récupération métriques: {ex.Message}");
                        metrics = new Dictionary<string, object>();
                    }
                }

                var gameStats = new Dictionary<string, object>();
                if (playerData.ContainsKey("gameStats"))
                {
                    try
                    {
                        // Même logique pour gameStats
                        if (playerData["gameStats"] is Newtonsoft.Json.Linq.JObject jObj)
                        {
                            gameStats = jObj.ToObject<Dictionary<string, object>>();
                        }
                        else if (playerData["gameStats"] is Dictionary<string, object> dict)
                        {
                            gameStats = dict;
                        }
                        else
                        {
                            string gameStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(playerData["gameStats"]);
                            gameStats = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(gameStatsJson);
                        }
                        UnityEngine.Debug.Log($"[AnalyticsFileGenerator] ✅ GameStats récupérées: {gameStats.Count} éléments");
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[AnalyticsFileGenerator] ⚠️ Erreur récupération gameStats: {ex.Message}");
                        gameStats = new Dictionary<string, object>();
                    }
                }

                // Ajouter la nouvelle métrique avec logique de cumul améliorée
                string metricKey = $"{gameId}_{metricName}";
                
                // Liste des métriques cumulatives pour FireflyDance, LogParade et MusicNote (doublons supprimés)
                bool isCumulativeMetric = metricName.Contains("total") || 
                                        metricName.Contains("count") || 
                                        metricName.Contains("nombre") ||
                                        metricName.Contains("lucioles") ||
                                        metricName.Contains("ratees") ||
                                        metricName.Contains("attrapees") ||
                                        metricName.Contains("reussies") ||
                                        metricName.Contains("echecs") ||
                                        metricName.Contains("chutes") ||
                                        metricName.Contains("vagues") ||
                                        metricName.Contains("notes") ||
                                        metricName == "hand_closure" ||
                                        metricName == "firefly_captured" ||
                                        metricName == "firefly_missed" ||
                                        metricName == "hand_success" ||
                                        metricName == "hand_failure" ||
                                        metricName == "total_captures" ||
                                        metricName == "points_lucioles";
                
                if (metrics.ContainsKey(metricKey))
                {
                    if (isCumulativeMetric)
                    {
                        // Additionner pour les métriques cumulatives
                        metrics[metricKey] = Convert.ToSingle(metrics[metricKey]) + value;
                        UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Métrique cumulative {metricKey}: {Convert.ToSingle(metrics[metricKey]) - value} + {value} = {metrics[metricKey]}");
                    }
                    else
                    {
                        // Remplacer pour les métriques non-cumulatives (scores finaux, etc.)
                        metrics[metricKey] = value;
                        UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Métrique remplacée {metricKey}: {value}");
                    }
                }
                else
                {
                    metrics[metricKey] = value;
                    UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Nouvelle métrique {metricKey}: {value}");
                }

                // Mettre à jour les statistiques par jeu avec la même logique
                if (!gameStats.ContainsKey(gameId))
                {
                    gameStats[gameId] = new Dictionary<string, object>();
                }
                var gameMetrics = gameStats[gameId] as Dictionary<string, object>;
                if (gameMetrics == null) gameMetrics = new Dictionary<string, object>();

                // Appliquer la même logique cumulative pour gameStats
                if (gameMetrics.ContainsKey(metricName))
                {
                    if (isCumulativeMetric)
                    {
                        gameMetrics[metricName] = Convert.ToSingle(gameMetrics[metricName]) + value;
                    }
                    else
                    {
                        gameMetrics[metricName] = value;
                    }
                }
                else
                {
                    gameMetrics[metricName] = value;
                }
                
                gameStats[gameId] = gameMetrics;

                // Sauvegarder les données mises à jour
                playerData["metrics"] = metrics;
                playerData["gameStats"] = gameStats;
                playerData["lastUpdated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(playerData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, jsonContent);

                UnityEngine.Debug.Log($"[AnalyticsFileGenerator] Fichier joueur mis à jour: {fileName} - {metricName}: {value}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AnalyticsFileGenerator] Erreur mise à jour fichier joueur: {e.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Extensions pour les dictionnaires
    /// </summary>
    public static class DictionaryExtensions
    {
        public static T GetValueOrDefault<T>(this Dictionary<string, object> dict, string key, T defaultValue = default(T))
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                try
                {
                    return (T)Convert.ChangeType(dict[key], typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}
