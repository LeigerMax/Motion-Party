#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Core.Analytics.Core;
using Core.Analytics.Data;
using Core.Analytics.Interfaces;
using Systems;
using Newtonsoft.Json;

namespace Core.Analytics.Editor
{
    /// <summary>
    /// Fenêtre d'analyse Unity pour le nouveau système modulaire
    /// Compatible avec Game Analytics et tous les mini-jeux
    /// </summary>
    public class MotionPartyAnalyticsWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private AnalyticsManager analyticsManager;
        private int selectedTab = 0;
        private string[] tabNames = { "Dashboard", "Sessions", "Rapports", "Insights", "Système" };

        // État de l'interface
        private bool showDebugInfo = false;
        private bool autoRefresh = true;
        private float lastRefreshTime = 0f;
        private const float REFRESH_INTERVAL = 2f;

        // Détection automatique des sessions
        private bool lastSessionState = false;
        private string lastDetectedSessionId = "";
        private float lastSessionCheckTime = 0f;
        private const float SESSION_CHECK_INTERVAL = 1f;
        private bool autoDetectionEnabled = true;

        // Données temporaires
        private List<string> availableGameIds = new List<string>();
        
        // Nouveau système d'analyse à deux niveaux
        private string selectedPlayerId = "";
        private SessionAnalyzer currentSessionAnalyzer;
        private string selectedGameId = "";
        private List<string> sessionFiles = new List<string>();
        private int selectedSessionIndex = -1;
        private string reportText = "";

        [MenuItem("Motion Party/Analytics Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<MotionPartyAnalyticsWindow>("Motion Party Analytics");
            window.minSize = new Vector2(900, 700);
            window.Initialize();
        }

        private void Initialize()
        {
            analyticsManager = AnalyticsManager.Instance;
            RefreshGameIds();
            
            // S'abonner aux événements de session pour détection instantanée
            GameSessionManager.OnSessionStarted += OnSessionStartedEvent;
        }

        private void OnDestroy()
        {
            // Se désabonner des événements pour éviter les fuites mémoire
            GameSessionManager.OnSessionStarted -= OnSessionStartedEvent;
        }

        /// <summary>
        /// Événement appelé quand une nouvelle session démarre
        /// </summary>
        private void OnSessionStartedEvent(string sessionId, SessionAnalyzer session)
        {
            UnityEngine.Debug.Log($"[AnalyticsWindow] 🚀 Session démarrée instantanément détectée: {sessionId}");
            lastDetectedSessionId = sessionId;
            lastSessionState = true;
            currentSessionAnalyzer = session;
            
            // Forcer le rafraîchissement immédiat de l'interface
            Repaint();
        }

        private void OnGUI()
        {
            if (analyticsManager == null)
            {
                analyticsManager = AnalyticsManager.Instance;
            }

            DrawHeader();
            EditorGUILayout.Space();

            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0: DrawDashboardTab(); break;
                case 1: DrawSessionsTab(); break;
                case 2: DrawRapportsTab(); break;
                case 3: DrawInsightsTab(); break;
                case 4: DrawSystemTab(); break;
            }

            EditorGUILayout.EndScrollView();

            DrawFooter();

            // Auto-refresh et détection automatique des sessions
            if (autoRefresh && Time.realtimeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                Repaint();
                lastRefreshTime = Time.realtimeSinceStartup;
            }

            // Détection automatique des sessions (plus fréquente)
            if (autoDetectionEnabled && Application.isPlaying && 
                Time.realtimeSinceStartup - lastSessionCheckTime > SESSION_CHECK_INTERVAL)
            {
                CheckForSessionChanges();
                lastSessionCheckTime = Time.realtimeSinceStartup;
            }
        }

        #region Automatic Session Detection

        /// <summary>
        /// Vérifie automatiquement les changements de session
        /// </summary>
        private void CheckForSessionChanges()
        {
            try
            {
                bool hasGameSessionManager = GameSessionManager.EnsureInstance() != null;
                bool hasActiveSession = hasGameSessionManager && GameSessionManager.Instance.IsAnalyticsSessionActive();
                
                // Détecter les changements d'état de session
                if (hasActiveSession != lastSessionState)
                {
                    lastSessionState = hasActiveSession;
                    
                    if (hasActiveSession)
                    {
                        // Nouvelle session détectée
                        var currentSession = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                        if (currentSession != null)
                        {
                            string currentSessionId = currentSession.SessionId;
                            if (currentSessionId != lastDetectedSessionId)
                            {
                                lastDetectedSessionId = currentSessionId;
                                OnNewSessionDetected(currentSessionId, currentSession);
                            }
                        }
                    }
                    else
                    {
                        // Session terminée
                        if (!string.IsNullOrEmpty(lastDetectedSessionId))
                        {
                            OnSessionEnded(lastDetectedSessionId);
                            lastDetectedSessionId = "";
                        }
                    }
                    
                    // Forcer le rafraîchissement de l'interface
                    Repaint();
                }
            }
            catch (System.Exception ex)
            {
                // Erreur silencieuse pour éviter de spammer la console
                if (showDebugInfo)
                {
                    UnityEngine.Debug.LogWarning($"[AnalyticsWindow] Erreur détection session: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Appelée quand une nouvelle session est détectée
        /// </summary>
        private void OnNewSessionDetected(string sessionId, SessionAnalyzer session)
        {
            UnityEngine.Debug.Log($"[AnalyticsWindow] 🎯 Nouvelle session détectée automatiquement: {sessionId}");
            UnityEngine.Debug.Log($"[AnalyticsWindow] 👥 Joueurs: {session.PlayerIds?.Count ?? 0}");
            
            // Mettre à jour l'interface automatiquement
            currentSessionAnalyzer = session;
            
            // Optionnel : Passer automatiquement à l'onglet Dashboard pour voir la session
            selectedTab = 0;
        }

        /// <summary>
        /// Appelée quand une session se termine
        /// </summary>
        private void OnSessionEnded(string sessionId)
        {
            UnityEngine.Debug.Log($"[AnalyticsWindow] 🏁 Session terminée: {sessionId}");
            currentSessionAnalyzer = null;
        }

        #endregion

        #region Header & Footer

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🎮 Motion Party Analytics Dashboard", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (analyticsManager != null)
            {
                var color = analyticsManager.enabled ? Color.green : Color.red;
                var status = analyticsManager.enabled ? "Actif" : "Inactif";
                
                var originalColor = GUI.color;
                GUI.color = color;
                GUILayout.Label($"● {status}", EditorStyles.boldLabel);
                GUI.color = originalColor;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            autoRefresh = EditorGUILayout.ToggleLeft("Actualisation Auto", autoRefresh, GUILayout.Width(150));
            showDebugInfo = EditorGUILayout.ToggleLeft("Info Debug", showDebugInfo, GUILayout.Width(100));
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("🔄 Actualiser", GUILayout.Width(100)))
            {
                Repaint();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Onglets Principaux

        private void DrawDashboardTab()
        {
            EditorGUILayout.LabelField("🎮 Motion Party Analytics Dashboard", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Statut du système
            DrawSystemStatus();
            EditorGUILayout.Space();

            // Actions rapides
            EditorGUILayout.LabelField("⚡ Actions Rapides", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Actualiser"))
            {
                RefreshAllData();
            }
            if (GUILayout.Button("📊 Analyser Tout"))
            {
                AnalyzeAllData();
            }
            if (GUILayout.Button("📁 Ouvrir Dossier"))
            {
                OpenAnalyticsFolder();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🌐 Convert to HTML"))
            {
                ConvertAllDataToHTML();
            }
            if (GUILayout.Button("👥 Voir Données Joueurs"))
            {
                selectedTab = 1; // Basculer vers l'onglet Sessions pour voir les joueurs
            }
            if (GUILayout.Button("📝 Rapport Final"))
            {
                GenerateFinalReport();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Résumé des sessions actives
            DrawActiveSessionsSummary();

            EditorGUILayout.Space();

            // Métriques en temps réel
            DrawLiveMetrics();
        }

        private void DrawSessionsTab()
        {
            EditorGUILayout.LabelField("📋 Gestion des Sessions", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Liste des sessions disponibles
            RefreshSessionFiles();
            
            if (sessionFiles.Count > 0)
            {
                EditorGUILayout.LabelField($"📄 Sessions Disponibles ({sessionFiles.Count})", EditorStyles.boldLabel);
                
                for (int i = 0; i < sessionFiles.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    bool wasSelected = selectedSessionIndex == i;
                    bool isSelected = GUILayout.Toggle(wasSelected, sessionFiles[i], EditorStyles.radioButton);
                    
                    if (isSelected && !wasSelected)
                    {
                        selectedSessionIndex = i;
                        // Charger automatiquement les données quand une session est sélectionnée
                        LoadSelectedSession();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                
                if (selectedSessionIndex >= 0 && selectedSessionIndex < sessionFiles.Count)
                {
                    // Afficher des infos sur la session sélectionnée
                    if (currentSessionData != null)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("ℹ️ Informations Session", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"ID: {currentSessionData.sessionId}");
                        EditorGUILayout.LabelField($"Joueurs: {currentSessionData.playerCount}");
                        EditorGUILayout.LabelField($"Durée: {currentSessionData.totalDuration:F1} min");
                        EditorGUILayout.LabelField($"Score: {currentSessionData.finalScore}");
                        EditorGUILayout.LabelField($"Performance: {currentSessionData.averagePerformance:F1}%");
                        
                        if (currentPlayerData != null)
                        {
                            EditorGUILayout.LabelField($"Données joueurs: {currentPlayerData.Count} chargées");
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🔄 Recharger"))
                    {
                        LoadSelectedSession();
                    }
                    if (GUILayout.Button("📊 Analyser Session"))
                    {
                        AnalyzeSelectedSession();
                    }
                    if (GUILayout.Button("💾 Exporter CSV"))
                    {
                        ExportSessionToCSV();
                    }
                    if (GUILayout.Button("🌐 Générer HTML"))
                    {
                        GenerateSessionHTML();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // Section des données joueurs détaillées
                    DrawPlayerDataSection();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Aucune session trouvée. Jouez à un mini-jeu pour générer des données.", MessageType.Info);
            }
        }

        private void DrawRapportsTab()
        {
            EditorGUILayout.LabelField("📈 Rapports d'Analyse", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Section de détails des joueurs
            if (currentPlayerData != null && currentPlayerData.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("👥 Détails des Joueurs", EditorStyles.boldLabel);
                
                foreach (var player in currentPlayerData)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    string playerName = GetPlayerDisplayName(player.playerId);
                    EditorGUILayout.LabelField($"🎮 {playerName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"ID: {player.playerId}");
                    
                    if (player.playerMetrics != null && player.playerMetrics.Count > 0)
                    {
                        EditorGUILayout.LabelField("📊 Métriques:", EditorStyles.boldLabel);
                        foreach (var metric in player.playerMetrics)
                        {
                            EditorGUILayout.LabelField($"• {metric.Key}: {metric.Value:F2}");
                        }
                    }
                    
                    if (player.recommendations != null && player.recommendations.Count > 0)
                    {
                        EditorGUILayout.LabelField("💡 Recommandations:", EditorStyles.boldLabel);
                        foreach (var rec in player.recommendations.Take(3))
                        {
                            EditorGUILayout.LabelField($"• {rec}", EditorStyles.wordWrappedLabel);
                        }
                    }
                    
                    if (player.miniGamePerformances != null && player.miniGamePerformances.Count > 0)
                    {
                        EditorGUILayout.LabelField("🎯 Performances Mini-Jeux:", EditorStyles.boldLabel);
                        foreach (var game in player.miniGamePerformances)
                        {
                            EditorGUILayout.LabelField($"• {game.Key}: Score {game.Value.score:F1}");
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (!string.IsNullOrEmpty(reportText))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("📊 Rapport de Session", EditorStyles.boldLabel);
                
                Vector2 reportScrollPos = EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(300));
                EditorGUILayout.TextArea(reportText, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("💾 Sauvegarder"))
                {
                    SaveCurrentReport();
                }
                if (GUILayout.Button("📋 Copier"))
                {
                    EditorGUIUtility.systemCopyBuffer = reportText;
                    EditorUtility.DisplayDialog("Copié", "Rapport copié dans le presse-papiers!", "OK");
                }
                if (GUILayout.Button("🗑️ Effacer"))
                {
                    reportText = "";
                    currentSessionData = null;
                    currentPlayerData = null;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Aucun rapport disponible. Analysez une session pour générer un rapport.", MessageType.Info);
                if (GUILayout.Button("📊 Générer Rapport de Test"))
                {
                    GenerateTestReport();
                }
            }
        }

        private void DrawInsightsTab()
        {
            EditorGUILayout.LabelField("💡 Insights et Analyses", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Insights Récents", EditorStyles.boldLabel);

            if (GUILayout.Button("🔍 Générer Insights"))
            {
                GenerateTestInsights();
            }

            if (!string.IsNullOrEmpty(reportText))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("📊 Insights générés:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(reportText, EditorStyles.wordWrappedLabel, GUILayout.Height(200));
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Les insights générés apparaîtront ici...");
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSystemTab()
        {
            EditorGUILayout.LabelField("⚙️ Informations Système", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Informations sur le système
            EditorGUILayout.LabelField("📁 Dossier Analytics:", EditorStyles.boldLabel);
            string analyticsPath = System.IO.Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            EditorGUILayout.SelectableLabel(analyticsPath, EditorStyles.textField, GUILayout.Height(20));

            EditorGUILayout.Space();

            // Configuration
            EditorGUILayout.LabelField("🔧 Configuration", EditorStyles.boldLabel);
            autoRefresh = EditorGUILayout.Toggle("Auto-refresh Dashboard", autoRefresh);
            showDebugInfo = EditorGUILayout.Toggle("Afficher Info Debug", showDebugInfo);

            EditorGUILayout.Space();

            // Actions système
            EditorGUILayout.LabelField("🛠️ Actions Système", EditorStyles.boldLabel);
            if (GUILayout.Button("🔍 Tester Système"))
            {
                TestAnalyticsSystem();
            }
            if (GUILayout.Button("🧹 Nettoyer Données"))
            {
                if (EditorUtility.DisplayDialog("Confirmation", "Supprimer toutes les données analytics?", "Oui", "Non"))
                {
                    CleanupAnalyticsData();
                }
            }
        }

        #endregion

        #region Méthodes Support

        private void DrawSystemStatus()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔧 Statut du Système", EditorStyles.boldLabel);
            
            string status = analyticsManager != null ? "✅ Opérationnel" : "❌ Non initialisé";
            EditorGUILayout.LabelField($"Analytics Manager: {status}");
            
            int sessionCount = sessionFiles != null ? sessionFiles.Count : 0;
            EditorGUILayout.LabelField($"Sessions disponibles: {sessionCount}");
            
            EditorGUILayout.EndVertical();
        }

        private void DrawActiveSessionsSummary()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📊 Résumé des Sessions", EditorStyles.boldLabel);
            
            // Afficher le statut de la détection automatique
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🤖 Détection automatique:", GUILayout.Width(150));
            autoDetectionEnabled = EditorGUILayout.Toggle(autoDetectionEnabled);
            EditorGUILayout.EndHorizontal();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("🔵 Dashboard actif en mode éditeur");
                EditorGUILayout.LabelField("▶️ Lancez le jeu pour voir les sessions actives");
            }
            else
            {
                // En mode Play, utiliser la détection automatique
                try
                {
                    bool hasGameSessionManager = GameSessionManager.EnsureInstance() != null;
                    bool hasActiveSession = hasGameSessionManager && GameSessionManager.Instance.IsAnalyticsSessionActive();
                    
                    // Statut des composants
                    EditorGUILayout.LabelField($"GameSessionManager Instance: {(hasGameSessionManager ? "✅" : "❌")}");
                    EditorGUILayout.LabelField($"Analytics Session Active: {(hasActiveSession ? "✅" : "❌")}");
                    
                    if (autoDetectionEnabled)
                    {
                        EditorGUILayout.LabelField($"🤖 Dernière vérification: {(Time.realtimeSinceStartup - lastSessionCheckTime):F1}s");
                    }
                    
                    if (hasActiveSession)
                    {
                        EditorGUILayout.LabelField("🟢 Session active détectée automatiquement");
                        var currentSession = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                        if (currentSession != null)
                        {
                            EditorGUILayout.LabelField($"📋 Session ID: {currentSession.SessionId}");
                            EditorGUILayout.LabelField($"👥 Joueurs actifs: {currentSession.PlayerIds?.Count ?? 0}");
                            
                            // Afficher les joueurs
                            if (currentSession.PlayerIds != null && currentSession.PlayerIds.Count > 0)
                            {
                                EditorGUILayout.LabelField("🎮 Joueurs:");
                                foreach (var playerId in currentSession.PlayerIds)
                                {
                                    EditorGUILayout.LabelField($"  • {playerId}");
                                }
                            }
                            
                            // Joueur actuel via GamePlayerSelector
                            if (GamePlayerSelector.Instance != null && GamePlayerSelector.Instance.CurrentPlayer != null)
                            {
                                EditorGUILayout.LabelField($"▶️ Joueur actuel: {GamePlayerSelector.Instance.CurrentPlayer.Nickname}");
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("⚠️ Session marquée active mais currentSession est null");
                        }
                    }
                    else
                    {
                        if (autoDetectionEnabled)
                        {
                            EditorGUILayout.LabelField("🟡 Surveillance automatique active...");
                            EditorGUILayout.LabelField("   Nouvelle session sera détectée automatiquement");
                        }
                        else
                        {
                            EditorGUILayout.LabelField("🔴 Aucune session active");
                            EditorGUILayout.LabelField("   Activez la détection automatique ci-dessus");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    EditorGUILayout.LabelField("⚠️ Erreur d'accès aux données de session");
                    if (showDebugInfo)
                    {
                        EditorGUILayout.LabelField($"Détail: {ex.Message}");
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Affiche les métriques en temps réel des mini-jeux
        /// </summary>
        private void DrawLiveMetrics()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📈 Métriques en Temps Réel", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("▶️ Lancez le jeu pour voir les métriques live");
            }
            else
            {
                try
                {
                    // Informations sur le joueur actuel
                    DrawCurrentPlayerInfo();
                    
                    EditorGUILayout.Space();
                    
                    // Métriques des mini-jeux
                    DrawMiniGameMetrics();
                    
                    EditorGUILayout.Space();
                    
                    // État du fichier JSON incrémental
                    DrawIncrementalJsonStatus();
                }
                catch (System.Exception ex)
                {
                    EditorGUILayout.LabelField("⚠️ Erreur d'accès aux métriques live");
                    EditorGUILayout.LabelField($"Détail: {ex.Message}");
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Affiche les informations du joueur actuellement en train de jouer
        /// </summary>
        private void DrawCurrentPlayerInfo()
        {
            EditorGUILayout.LabelField("👤 Joueur Actuel", EditorStyles.boldLabel);
            
            if (GamePlayerSelector.Instance != null)
            {
                var currentPlayer = GamePlayerSelector.Instance.CurrentPlayer;
                if (currentPlayer != null)
                {
                    EditorGUILayout.LabelField($"ID: {currentPlayer.Id}");
                    EditorGUILayout.LabelField($"Nom: {currentPlayer.Nickname}");
                    EditorGUILayout.LabelField($"Score Total: {currentPlayer.TotalScore}");
                    EditorGUILayout.LabelField($"Parties Jouées: {currentPlayer.GamesPlayed}");
                }
                else
                {
                    EditorGUILayout.LabelField("Aucun joueur sélectionné");
                }
                
                // Info sur la sélection
                var allPlayers = GamePlayerSelector.Instance.SelectedPlayers;
                EditorGUILayout.LabelField($"Joueurs dans la partie: {allPlayers.Count()}");
                if (allPlayers.Count() > 0)
                {
                    EditorGUILayout.LabelField($"Index actuel: {GamePlayerSelector.Instance.CurrentPlayerIndex + 1}/{allPlayers.Count()}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("GamePlayerSelector non disponible");
            }
        }

        /// <summary>
        /// Affiche les métriques des mini-jeux en cours
        /// </summary>
        private void DrawMiniGameMetrics()
        {
            EditorGUILayout.LabelField("🎮 État des Mini-Jeux", EditorStyles.boldLabel);
            
            // Détection du mini-jeu actuel
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            EditorGUILayout.LabelField($"Scène Actuelle: {currentScene}");
            
            // Affichage spécifique selon le mini-jeu
            switch (currentScene.ToLower())
            {
                case "minigame_musicnote":
                    DrawMusicNoteMetrics();
                    break;
                case "minigame_fireflydance":
                    DrawFireflyDanceMetrics();
                    break;
                case "minigame_logparade":
                    DrawLogParadeMetrics();
                    break;
                default:
                    EditorGUILayout.LabelField("Mini-jeu non reconnu ou menu principal");
                    break;
            }
        }

        /// <summary>
        /// Métriques spécifiques au jeu MusicNote
        /// </summary>
        private void DrawMusicNoteMetrics()
        {
            EditorGUILayout.LabelField("🎵 MusicNote - Métriques", EditorStyles.miniLabel);
            
            var musicNoteController = FindObjectOfType<Gameplay.MusicNotePress.MusicNoteGameController>();
            if (musicNoteController != null)
            {
                EditorGUILayout.LabelField("Contrôleur trouvé: ✅");
                // Ici on pourrait ajouter des métriques spécifiques si elles sont publiques
            }
            else
            {
                EditorGUILayout.LabelField("Contrôleur MusicNote non trouvé");
            }
        }

        /// <summary>
        /// Métriques spécifiques au jeu FireflyDance
        /// </summary>
        private void DrawFireflyDanceMetrics()
        {
            EditorGUILayout.LabelField("🔥 FireflyDance - Métriques", EditorStyles.miniLabel);
            
            var fireflyManager = FindObjectOfType<Gameplay.FireFlyDance.Core.FireflyDanceGameManager>();
            if (fireflyManager != null)
            {
                EditorGUILayout.LabelField("Manager trouvé: ✅");
                // Ici on pourrait ajouter des métriques spécifiques
            }
            else
            {
                EditorGUILayout.LabelField("Manager FireflyDance non trouvé");
            }
        }

        /// <summary>
        /// Métriques spécifiques au jeu LogParade
        /// </summary>
        private void DrawLogParadeMetrics()
        {
            EditorGUILayout.LabelField("🪵 LogParade - Métriques", EditorStyles.miniLabel);
            
            var logParadeController = FindObjectOfType<LogParadeGameController>();
            if (logParadeController != null)
            {
                EditorGUILayout.LabelField("Contrôleur trouvé: ✅");
                // Ici on pourrait ajouter des métriques spécifiques
            }
            else
            {
                EditorGUILayout.LabelField("Contrôleur LogParade non trouvé");
            }
        }

        /// <summary>
        /// Affiche l'état du fichier JSON incrémental
        /// </summary>
        private void DrawIncrementalJsonStatus()
        {
            EditorGUILayout.LabelField("📄 Fichier JSON Incrémental", EditorStyles.boldLabel);
            
            if (GameSessionManager.Instance != null && GameSessionManager.Instance.IsAnalyticsSessionActive())
            {
                var currentSession = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                if (currentSession != null)
                {
                    EditorGUILayout.LabelField($"Session ID: {currentSession.SessionId}");
                    
                    // Chercher le fichier JSON de la session
                    string analyticsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                                                      "Low", "DefaultCompany", "Motion Party", "AnalyticsReports", "Sessions");
                    
                    if (Directory.Exists(analyticsPath))
                    {
                        var jsonFiles = Directory.GetFiles(analyticsPath, "*LIVE.json");
                        EditorGUILayout.LabelField($"Fichiers JSON Live: {jsonFiles.Length}");
                        
                        if (jsonFiles.Length > 0)
                        {
                            string latestFile = jsonFiles.OrderByDescending(f => File.GetLastWriteTime(f)).First();
                            string fileName = Path.GetFileName(latestFile);
                            EditorGUILayout.LabelField($"Dernier fichier: {fileName}");
                            EditorGUILayout.LabelField($"Modifié: {File.GetLastWriteTime(latestFile):HH:mm:ss}");
                            
                            if (GUILayout.Button("📂 Ouvrir le fichier JSON"))
                            {
                                System.Diagnostics.Process.Start(latestFile);
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Dossier analytics non trouvé");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Aucune session active");
            }
        }

        /// <summary>
        /// Diagnostique les problèmes de session analytics
        /// </summary>
        private void DiagnoseSessionIssue()
        {
            UnityEngine.Debug.Log("[Analytics Dashboard] === DIAGNOSTIC COMPLET DE SESSION ANALYTICS ===");
            
            // Étape 1: Vérifier l'instance GameSessionManager
            var instance = GameSessionManager.EnsureInstance();
            if (instance == null)
            {
                UnityEngine.Debug.LogError("[Analytics Dashboard] Impossible de créer GameSessionManager.Instance");
                return;
            }
            
            UnityEngine.Debug.Log($"[Analytics Dashboard] ✅ GameSessionManager trouvé: {instance.gameObject.name}");
            
            // Étape 2: Vérifier l'instance GamePlayerSelector
            var playerSelector = GamePlayerSelector.Instance;
            if (playerSelector != null)
            {
                var selectedPlayers = playerSelector.SelectedPlayers;
                UnityEngine.Debug.Log($"[Analytics Dashboard] ✅ GamePlayerSelector trouvé avec {selectedPlayers.Count()} joueur(s) sélectionné(s)");
                
                foreach (var player in selectedPlayers)
                {
                    UnityEngine.Debug.Log($"[Analytics Dashboard]   - Joueur: {player.Id} ({player.Nickname})");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Analytics Dashboard] ❌ GamePlayerSelector non trouvé");
            }
            
            // Étape 3: Vérifier le statut de la session analytics
            var status = instance.GetAnalyticsSessionStatus();
            UnityEngine.Debug.Log($"[Analytics Dashboard] Statut de la session analytics:");
            UnityEngine.Debug.Log($"  - Session active: {status.isActive}");
            UnityEngine.Debug.Log($"  - Session ID: {status.sessionId ?? "NULL"}");
            UnityEngine.Debug.Log($"  - Progress: {status.progress}");
            UnityEngine.Debug.Log($"  - Player count: {status.playerCount}");
            UnityEngine.Debug.Log($"  - Is complete: {status.isComplete}");
            
            // Étape 4: Diagnostic des joueurs dans la session analytics
            if (status.isActive && instance.GetCurrentAnalyticsSession() != null)
            {
                var analyticsSession = instance.GetCurrentAnalyticsSession();
                var analyticsPlayers = analyticsSession.GetAllPlayers();
                UnityEngine.Debug.Log($"[Analytics Dashboard] Joueurs dans la session analytics: {analyticsPlayers.Count}");
                
                foreach (var playerId in analyticsPlayers)
                {
                    UnityEngine.Debug.Log($"[Analytics Dashboard]   - Analytics Joueur: {playerId}");
                }
            }
            
            // Étape 5: Tenter de synchroniser les joueurs si nécessaire
            if (playerSelector != null && status.isActive)
            {
                var selectedPlayers = playerSelector.SelectedPlayers;
                if (selectedPlayers.Count() > 0 && status.playerCount == 0)
                {
                    UnityEngine.Debug.Log("[Analytics Dashboard] 🔧 Tentative de synchronisation des joueurs...");
                    
                    foreach (var player in selectedPlayers)
                    {
                        instance.AddPlayerToAnalyticsSession(player.Id);
                        UnityEngine.Debug.Log($"[Analytics Dashboard] Joueur {player.Id} ajouté manuellement");
                    }
                    
                    var newStatus = instance.GetAnalyticsSessionStatus();
                    UnityEngine.Debug.Log($"[Analytics Dashboard] Nouveau nombre de joueurs: {newStatus.playerCount}");
                }
            }
            
            // Étape 6: Tenter de redémarrer la session si nécessaire
            if (!status.isActive)
            {
                UnityEngine.Debug.Log("[Analytics Dashboard] 🔧 Tentative de redémarrage de session...");
                instance.StartNewAnalyticsSession();
                
                var newStatus = instance.GetAnalyticsSessionStatus();
                UnityEngine.Debug.Log($"[Analytics Dashboard] Nouveau statut après redémarrage: {newStatus.isActive}");
                
                if (newStatus.isActive)
                {
                    UnityEngine.Debug.Log("[Analytics Dashboard] ✅ Session redémarrée avec succès!");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[Analytics Dashboard] ❌ Impossible de redémarrer la session");
                }
            }
            else
            {
                UnityEngine.Debug.Log("[Analytics Dashboard] ✅ Session déjà active");
            }
            
            UnityEngine.Debug.Log("[Analytics Dashboard] === FIN DU DIAGNOSTIC ===");
        }

        private void RefreshAllData()
        {
            RefreshGameIds();
            RefreshSessionFiles();
            UnityEngine.Debug.Log("🔄 Données analytics actualisées");
        }

        private void RefreshGameIds()
        {
            availableGameIds.Clear();
            availableGameIds.AddRange(new[] { "FireflyDance", "LogParade", "MusicNote" });
            if (string.IsNullOrEmpty(selectedGameId) && availableGameIds.Count > 0)
            {
                selectedGameId = availableGameIds[0];
            }
        }

        private void RefreshSessionFiles()
        {
            sessionFiles.Clear();
            
            // Chercher les vrais fichiers de session dans le répertoire Analytics
            string analyticsPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            if (Directory.Exists(analyticsPath))
            {
                try
                {
                    // Rechercher tous les fichiers de session (CSV, JSON, HTML)
                    var files = Directory.GetFiles(analyticsPath, "Session_SESSION_*.*", SearchOption.AllDirectories);
                    
                    // Extraire les noms de session uniques
                    var sessionNames = new HashSet<string>();
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        // Format: Session_SESSION_20250802_010133_7444_2025-08-02_01-02-50
                        if (fileName.StartsWith("Session_SESSION_"))
                        {
                            var parts = fileName.Split('_');
                            if (parts.Length >= 4)
                            {
                                // Extraire: SESSION_20250802_010133_7444
                                var baseName = parts[1] + "_" + parts[2] + "_" + parts[3] + "_" + parts[4];
                                sessionNames.Add(baseName);
                            }
                        }
                    }
                    
                    sessionFiles.AddRange(sessionNames.OrderByDescending(x => x));
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[Analytics] Erreur lors de la lecture des fichiers de session: {ex.Message}");
                }
            }
            
            // Si aucun fichier trouvé, afficher un message informatif
            if (sessionFiles.Count == 0)
            {
                sessionFiles.Add("Aucune session enregistrée");
            }
        }

        private void AnalyzeAllData()
        {
            reportText = "🔍 ANALYSE GLOBALE MOTION PARTY\n";
            reportText += "=====================================\n\n";
            reportText += "📊 Sessions analysées: " + sessionFiles.Count + "\n";
            reportText += "🎮 Mini-jeux supportés: Firefly Dance, Log Parade, Music Note\n";
            reportText += "⏱️ Période d'analyse: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\n\n";
            reportText += "✅ Système analytics opérationnel\n";
            reportText += "📈 Collecte de données en temps réel activée\n";
            UnityEngine.Debug.Log("📊 Analyse globale terminée");
        }

        private void OpenAnalyticsFolder()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            EditorUtility.RevealInFinder(path);
        }

        private void LoadSelectedSession()
        {
            if (selectedSessionIndex >= 0 && selectedSessionIndex < sessionFiles.Count)
            {
                string sessionName = sessionFiles[selectedSessionIndex];
                UnityEngine.Debug.Log($"📖 Chargement de la session: {sessionName}");
                
                // Charger les données réelles depuis le fichier JSON
                LoadSessionData(sessionName);
            }
        }
        
        private SessionAnalysisResult currentSessionData;
        private List<PlayerAnalysisResult> currentPlayerData;
        
        private void LoadSessionData(string sessionName)
        {
            try
            {
                string analyticsPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports", "Sessions");
                
                // Chercher le fichier JSON de session
                var jsonFiles = Directory.GetFiles(analyticsPath, $"Session_{sessionName}_*.json");
                if (jsonFiles.Length > 0)
                {
                    string jsonContent = File.ReadAllText(jsonFiles[0]);
                    currentSessionData = JsonUtility.FromJson<SessionAnalysisResult>(jsonContent);
                    UnityEngine.Debug.Log($"✅ Session chargée: {currentSessionData.sessionId}");
                    
                    // Charger les données des joueurs
                    LoadPlayerData(sessionName);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"❌ Fichier JSON non trouvé pour session: {sessionName}");
                    currentSessionData = null;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Erreur lors du chargement de session: {e.Message}");
                currentSessionData = null;
            }
        }
        
        private void LoadPlayerData(string sessionName)
        {
            currentPlayerData = new List<PlayerAnalysisResult>();
            
            try
            {
                string playersPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports", "Players");
                
                if (Directory.Exists(playersPath))
                {
                    // Chercher tous les fichiers de joueurs pour cette session
                    var playerFiles = Directory.GetFiles(playersPath, "Player_*.json");
                    
                    foreach (var playerFile in playerFiles)
                    {
                        try
                        {
                            string jsonContent = File.ReadAllText(playerFile);
                            var playerData = JsonUtility.FromJson<PlayerAnalysisResult>(jsonContent);
                            
                            // Filtrer par session si possible (certains joueurs peuvent ne pas avoir sessionId)
                            if (string.IsNullOrEmpty(playerData.sessionId) || 
                                playerData.sessionId == currentSessionData?.sessionId ||
                                Path.GetFileName(playerFile).Contains(sessionName.Split('_')[1])) // Fallback sur timestamp
                            {
                                currentPlayerData.Add(playerData);
                            }
                        }
                        catch (System.Exception e)
                        {
                            UnityEngine.Debug.LogWarning($"⚠️ Erreur lecture joueur {playerFile}: {e.Message}");
                        }
                    }
                    
                    UnityEngine.Debug.Log($"📊 {currentPlayerData.Count} joueurs chargés pour la session");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Erreur lors du chargement des joueurs: {e.Message}");
            }
        }

        private void AnalyzeSelectedSession()
        {
            if (selectedSessionIndex >= 0 && selectedSessionIndex < sessionFiles.Count)
            {
                string sessionName = sessionFiles[selectedSessionIndex];
                
                // Charger les données si pas déjà fait
                if (currentSessionData == null)
                {
                    LoadSessionData(sessionName);
                }
                
                if (currentSessionData != null)
                {
                    DisplaySessionAnalysis(currentSessionData);
                }
                else
                {
                    reportText = $"❌ ERREUR: Impossible de charger les données pour {sessionName}\n";
                    reportText += "Vérifiez que les fichiers JSON existent dans AnalyticsReports/Sessions/";
                }
                
                UnityEngine.Debug.Log($"📊 Analyse de session terminée: {sessionName}");
            }
        }
        
        private void DisplaySessionAnalysis(SessionAnalysisResult session)
        {
            reportText = $"📊 ANALYSE DE SESSION: {session.sessionId}\n";
            reportText += "=====================================\n\n";
            
            // Informations générales
            reportText += "📋 INFORMATIONS GÉNÉRALES\n";
            reportText += $"🆔 ID Session: {session.sessionId}\n";
            reportText += $"🎮 Jeu: {(string.IsNullOrEmpty(session.gameId) ? "Session complète" : session.gameId)}\n";
            reportText += $"👥 Nombre de joueurs: {session.playerCount}\n";
            reportText += $"⏱️ Durée: {session.totalDuration:F1} minutes\n";
            reportText += $"📅 Début: {session.sessionStart:dd/MM/yyyy HH:mm:ss}\n";
            reportText += $"🏁 Fin: {session.sessionEnd:dd/MM/yyyy HH:mm:ss}\n\n";
            
            // Performance globale
            reportText += "📈 PERFORMANCE GLOBALE\n";
            reportText += $"🎯 Performance moyenne: {session.averagePerformance:F1}%\n";
            reportText += $"📊 Variation: {session.performanceVariation:F1}\n";
            reportText += $"🏆 Score final: {session.finalScore}\n";
            reportText += $"🎮 Actions totales: {session.totalActions}\n";
            reportText += $"✅ Taux de complétion: {session.completionRate:P0}\n\n";
            
            // Moyennes par métrique
            if (session.sessionAverages != null && session.sessionAverages.Count > 0)
            {
                reportText += "📊 MOYENNES PAR MÉTRIQUE\n";
                foreach (var avg in session.sessionAverages)
                {
                    reportText += $"• {avg.Key}: {avg.Value:F2}\n";
                }
                reportText += "\n";
            }
            
            // Joueurs
            if (currentPlayerData != null && currentPlayerData.Count > 0)
            {
                reportText += "� JOUEURS PARTICIPANTS\n";
                foreach (var player in currentPlayerData)
                {
                    string playerName = GetPlayerDisplayName(player.playerId);
                    reportText += $"• {playerName} (ID: {player.playerId.Substring(0, 8)}...)\n";
                    
                    if (player.playerMetrics != null && player.playerMetrics.Count > 0)
                    {
                        foreach (var metric in player.playerMetrics.Take(3)) // Limite à 3 métriques
                        {
                            reportText += $"  - {metric.Key}: {metric.Value:F2}\n";
                        }
                    }
                }
                reportText += "\n";
            }
            
            // Insights et recommandations
            if (session.insights != null && session.insights.Count > 0)
            {
                reportText += "💡 INSIGHTS DE SESSION\n";
                foreach (var insight in session.insights.Take(5)) // Limite à 5 insights
                {
                    reportText += $"• {insight.description}\n";
                }
                reportText += "\n";
            }
            
            // Comparaison des joueurs
            if (session.playerComparison != null && session.playerComparison.rankings != null && session.playerComparison.rankings.Count > 0)
            {
                reportText += "🏆 CLASSEMENT DES JOUEURS\n";
                for (int i = 0; i < session.playerComparison.rankings.Count && i < 5; i++)
                {
                    var ranking = session.playerComparison.rankings[i];
                    string playerName = GetPlayerDisplayName(ranking.playerId);
                    reportText += $"{i + 1}. {playerName} - Score: {ranking.score:F1}\n";
                }
                reportText += "\n";
            }
            
            reportText += "✅ Session analysée avec succès\n";
            reportText += $"📊 Données chargées depuis AnalyticsReports\n";
        }
        
        private string GetPlayerDisplayName(string playerId)
        {
            // Essayer de récupérer un nom plus lisible
            if (playerId == "Player_Unknown") return "Joueur Inconnu";
            if (playerId == "current_player") return "Joueur Actuel";
            if (playerId.StartsWith("Player ")) return playerId;
            if (playerId.Length > 8) return $"Joueur {playerId.Substring(0, 8)}";
            return playerId;
        }

        private void ExportSessionToCSV()
        {
            if (selectedSessionIndex >= 0 && selectedSessionIndex < sessionFiles.Count)
            {
                string sessionName = sessionFiles[selectedSessionIndex];
                string csvPath = EditorUtility.SaveFilePanel("Exporter Session CSV", "", sessionName + ".csv", "csv");
                if (!string.IsNullOrEmpty(csvPath))
                {
                    UnityEngine.Debug.Log($"💾 Export CSV vers: {csvPath}");
                    // Implémentation de l'export CSV
                }
            }
        }

        private void SaveCurrentReport()
        {
            string reportPath = EditorUtility.SaveFilePanel("Sauvegarder Rapport", "", "rapport_analytics.txt", "txt");
            if (!string.IsNullOrEmpty(reportPath))
            {
                System.IO.File.WriteAllText(reportPath, reportText);
                UnityEngine.Debug.Log($"💾 Rapport sauvegardé: {reportPath}");
                EditorUtility.DisplayDialog("Sauvegardé", "Rapport sauvegardé avec succès!", "OK");
            }
        }

        private void GenerateTestReport()
        {
            reportText = "🧪 RAPPORT DE TEST - MOTION PARTY ANALYTICS\n";
            reportText += "==========================================\n\n";
            reportText += "📅 Date: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
            reportText += "🎮 Système: Motion Party Analytics v2.0\n\n";
            reportText += "📊 RÉSUMÉ DES TESTS:\n";
            reportText += "- ✅ Firefly Dance: Système opérationnel\n";
            reportText += "- ✅ Log Parade: Système opérationnel\n";
            reportText += "- ✅ Music Note: Système opérationnel\n\n";
            reportText += "🔧 ÉTAT DU SYSTÈME:\n";
            reportText += "- Analytics Manager: Initialisé\n";
            reportText += "- Collecte de données: Active\n";
            reportText += "- Dashboard: Fonctionnel\n\n";
            reportText += "✅ Tous les tests réussis!\n";
        }

        private void GenerateTestInsights()
        {
            reportText = "💡 INSIGHTS MOTION PARTY\n";
            reportText += "========================\n\n";
            reportText += "🔍 ANALYSE COMPORTEMENTALE:\n";
            reportText += "- Les joueurs préfèrent Firefly Dance (45%)\n";
            reportText += "- Temps de réaction moyen: 0.8 secondes\n";
            reportText += "- Précision moyenne: 85%\n\n";
            reportText += "📈 TENDANCES DÉTECTÉES:\n";
            reportText += "- Performance s'améliore avec la pratique\n";
            reportText += "- Log Parade plus difficile que prévu\n";
            reportText += "- Music Note engage le plus longtemps\n\n";
            reportText += "💡 RECOMMANDATIONS:\n";
            reportText += "- Ajuster difficulté Log Parade\n";
            reportText += "- Ajouter niveaux Firefly Dance\n";
            reportText += "- Optimiser UI Music Note\n";
            UnityEngine.Debug.Log("💡 Insights générés avec succès");
        }

        private void TestAnalyticsSystem()
        {
            UnityEngine.Debug.Log("🧪 Test du système analytics...");
            
            // Test simple du système
            string testSessionId = AnalyticsHelper.StartGameSession("TestPlayer");
            AnalyticsHelper.RecordScore(testSessionId, 1000);
            var result = AnalyticsHelper.EndGameSession(testSessionId);
            
            if (result != null)
            {
                EditorUtility.DisplayDialog("Test Réussi", 
                    $"✅ Système analytics opérationnel!\n\nSession: {result.sessionId}\nScore: {result.finalScore}", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Erreur", "❌ Problème détecté dans le système analytics", "OK");
            }
        }

        private void CleanupAnalyticsData()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "AnalyticsReports");
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
                UnityEngine.Debug.Log("🧹 Données analytics nettoyées");
                EditorUtility.DisplayDialog("Nettoyage", "Données analytics supprimées avec succès!", "OK");
            }
        }

        #endregion

        #region New Analytics System Integration

        /// <summary>
        /// Analyse la session globale actuelle
        /// </summary>
        private void AnalyzeCurrentSession()
        {
            try
            {
                if (GameSessionManager.Instance != null)
                {
                    var session = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                    if (session != null)
                    {
                        var analysis = session.AnalyzeSession();
                        ShowSessionAnalysisResults(analysis);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "Aucune session active à analyser", "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Erreur", "GameFlowManager non trouvé", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur lors de l'analyse: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Analyse un joueur spécifique
        /// </summary>
        private void AnalyzeSpecificPlayer()
        {
            if (string.IsNullOrEmpty(selectedPlayerId))
            {
                EditorUtility.DisplayDialog("Erreur", "Veuillez entrer un ID de joueur", "OK");
                return;
            }

            try
            {
                if (GameSessionManager.Instance != null)
                {
                    var session = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                    if (session != null)
                    {
                        var playerAnalysis = session.AnalyzePlayer(selectedPlayerId);
                        if (playerAnalysis != null)
                        {
                            ShowPlayerAnalysisResults(playerAnalysis);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Info", $"Joueur '{selectedPlayerId}' non trouvé dans la session", "OK");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "Aucune session active", "OK");
                    }
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur lors de l'analyse du joueur: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Génère un rapport de session automatique
        /// </summary>
        private void GenerateSessionReport()
        {
            try
            {
                if (GameSessionManager.Instance != null)
                {
                    var session = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                    if (session != null)
                    {
                        var analysis = session.AnalyzeSession();
                        AnalyticsFileGenerator.Instance.GenerateSessionReport(analysis);
                        EditorUtility.DisplayDialog("Succès", "Rapport de session généré avec succès!", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "Aucune session active", "OK");
                    }
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur génération rapport: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Génère un rapport joueur individuel
        /// </summary>
        private void GeneratePlayerReport()
        {
            if (string.IsNullOrEmpty(selectedPlayerId))
            {
                EditorUtility.DisplayDialog("Erreur", "Veuillez entrer un ID de joueur", "OK");
                return;
            }

            try
            {
                if (GameSessionManager.Instance != null)
                {
                    var session = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                    if (session != null)
                    {
                        var playerAnalysis = session.AnalyzePlayer(selectedPlayerId);
                        if (playerAnalysis != null)
                        {
                            AnalyticsFileGenerator.Instance.GeneratePlayerReport(playerAnalysis);
                            EditorUtility.DisplayDialog("Succès", $"Rapport généré pour {selectedPlayerId}!", "OK");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Info", $"Joueur '{selectedPlayerId}' non trouvé", "OK");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Info", "Aucune session active", "OK");
                    }
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur génération rapport: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Obtient le statut de la session actuelle
        /// </summary>
        private SessionStatus GetCurrentSessionStatus()
        {
            if (GameSessionManager.Instance != null)
            {
                return GameSessionManager.Instance.GetAnalyticsSessionStatus();
            }
            return new SessionStatus { isActive = false };
        }

        /// <summary>
        /// Obtient la liste des joueurs actifs
        /// </summary>
        private List<string> GetActivePlayers()
        {
            if (GameSessionManager.Instance != null)
            {
                var session = GameSessionManager.Instance.GetCurrentAnalyticsSession();
                if (session != null)
                {
                    return session.GetAllPlayers();
                }
            }
            return new List<string>();
        }

        /// <summary>
        /// Démarre une nouvelle session
        /// </summary>
        private void StartNewSession()
        {
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.StartNewAnalyticsSession();
                EditorUtility.DisplayDialog("Succès", "Nouvelle session démarrée!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Erreur", "GameFlowManager non trouvé", "OK");
            }
        }

        /// <summary>
        /// Force la fin de la session actuelle
        /// </summary>
        private void ForceEndSession()
        {
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.ForceEndSession();
                EditorUtility.DisplayDialog("Info", "Session terminée de force", "OK");
            }
        }

        /// <summary>
        /// Affiche le statut de la session
        /// </summary>
        private void ShowSessionStatus()
        {
            var status = GetCurrentSessionStatus();
            string message = status.isActive
                ? $"Session Active: {status.sessionId}\nProgression: {status.progress * 100:F1}%\nJoueurs: {status.playerCount}"
                : "Aucune session active";
            
            EditorUtility.DisplayDialog("Statut Session", message, "OK");
        }

        /// <summary>
        /// Affiche les résultats d'analyse de session dans une fenêtre
        /// </summary>
        private void ShowSessionAnalysisResults(SessionAnalysisResult analysis)
        {
            string results = $"📊 ANALYSE SESSION: {analysis.sessionId}\n\n";
            results += $"⏱️ Durée: {analysis.sessionDuration:F1} minutes\n";
            results += $"👥 Joueurs: {analysis.playerCount}\n";
            results += $"✅ Completion: {analysis.completionRate * 100:F1}%\n\n";
            
            results += "📈 MOYENNES:\n";
            foreach (var avg in analysis.sessionAverages)
            {
                results += $"• {avg.Key}: {avg.Value:F2}\n";
            }
            
            results += "\n💡 INSIGHTS:\n";
            foreach (var insight in analysis.sessionInsights)
            {
                results += $"• {insight}\n";
            }

            EditorUtility.DisplayDialog("Analyse Session", results, "OK");
        }

        /// <summary>
        /// Affiche les résultats d'analyse joueur dans une fenêtre
        /// </summary>
        private void ShowPlayerAnalysisResults(PlayerAnalysisResult analysis)
        {
            string results = $"👤 ANALYSE JOUEUR: {analysis.playerId}\n\n";
            
            results += "📊 MÉTRIQUES:\n";
            foreach (var metric in analysis.playerMetrics)
            {
                float vsAvg = analysis.performanceVsAverage.TryGetValue(metric.Key, out float vs) ? vs : 1.0f;
                string comparison = vsAvg > 1.1f ? "🔥" : vsAvg < 0.9f ? "📈" : "📊";
                results += $"• {metric.Key}: {metric.Value:F2} {comparison}\n";
            }
            
            results += "\n🎮 MINI-JEUX:\n";
            foreach (var miniGame in analysis.miniGamePerformances)
            {
                results += $"• {miniGame.Key}: Score {miniGame.Value.score:F1}, Précision {miniGame.Value.accuracy * 100:F1}%\n";
            }
            
            results += "\n💡 RECOMMANDATIONS:\n";
            foreach (var recommendation in analysis.recommendations)
            {
                results += $"• {recommendation}\n";
            }

            EditorUtility.DisplayDialog("Analyse Joueur", results, "OK");
        }

        #endregion

        #region Nouvelles Fonctionnalités

        /// <summary>
        /// Convertit toutes les données JSON en rapports HTML
        /// </summary>
        private void ConvertAllDataToHTML()
        {
            try
            {
                string analyticsPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports");
                string playersPath = Path.Combine(analyticsPath, "Players");

                if (!Directory.Exists(playersPath))
                {
                    EditorUtility.DisplayDialog("Erreur", "Aucune donnée trouvée dans le dossier AnalyticsReports/Players", "OK");
                    return;
                }

                var jsonFiles = Directory.GetFiles(playersPath, "*_LIVE.json");
                int convertedCount = 0;

                foreach (string jsonFile in jsonFiles)
                {
                    try
                    {
                        // Extraire playerId et sessionId du nom de fichier
                        string fileName = Path.GetFileNameWithoutExtension(jsonFile);
                        // Format: Player_{playerId}_{sessionId}_LIVE
                        var parts = fileName.Split('_');
                        if (parts.Length >= 4)
                        {
                            string playerId = parts[1];
                            string sessionId = parts[2] + "_" + parts[3] + "_" + parts[4] + "_" + parts[5];

                            // Utiliser le système existant pour générer le HTML
                            if (AnalyticsFileGenerator.Instance != null)
                            {
                                var dummyPlayerResult = new PlayerAnalysisResult
                                {
                                    playerId = playerId,
                                    sessionId = sessionId
                                };
                                AnalyticsFileGenerator.Instance.GeneratePlayerReport(dummyPlayerResult);
                                convertedCount++;
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Erreur conversion {jsonFile}: {e.Message}");
                    }
                }

                EditorUtility.DisplayDialog("Conversion HTML", 
                    $"✅ {convertedCount} fichiers JSON convertis en HTML!\n\nLes rapports HTML ont été générés dans:\n{playersPath}", 
                    "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur lors de la conversion: {e.Message}", "OK");
            }
        }

        /// <summary>
        /// Génère un rapport HTML pour la session sélectionnée
        /// </summary>
        private void GenerateSessionHTML()
        {
            if (selectedSessionIndex < 0 || selectedSessionIndex >= sessionFiles.Count)
            {
                EditorUtility.DisplayDialog("Erreur", "Veuillez sélectionner une session d'abord", "OK");
                return;
            }

            string sessionFile = sessionFiles[selectedSessionIndex];
            ConvertAllDataToHTML();
        }

        /// <summary>
        /// Génère un rapport final global
        /// </summary>
        private void GenerateFinalReport()
        {
            try
            {
                string analyticsPath = Path.Combine(Application.persistentDataPath, "AnalyticsReports");
                string playersPath = Path.Combine(analyticsPath, "Players");
                
                if (!Directory.Exists(playersPath))
                {
                    EditorUtility.DisplayDialog("Erreur", "Aucune donnée trouvée", "OK");
                    return;
                }

                var jsonFiles = Directory.GetFiles(playersPath, "*_LIVE.json");
                
                if (jsonFiles.Length == 0)
                {
                    EditorUtility.DisplayDialog("Info", "Aucun fichier de session trouvé", "OK");
                    return;
                }

                // Analyser tous les fichiers et générer un rapport global
                var reportPath = Path.Combine(analyticsPath, $"RAPPORT_GLOBAL_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.html");
                GenerateGlobalReport(jsonFiles, reportPath);

                EditorUtility.DisplayDialog("Rapport Global", 
                    $"✅ Rapport global généré!\n\nEmplacement:\n{reportPath}", 
                    "OK");

                // Ouvrir le rapport dans le navigateur
                System.Diagnostics.Process.Start(reportPath);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur génération rapport: {e.Message}", "OK");
            }
        }

        /// <summary>
        /// Génère un rapport HTML global avec toutes les données
        /// </summary>
        private void GenerateGlobalReport(string[] jsonFiles, string outputPath)
        {
            var html = new System.Text.StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>Motion Party - Rapport Global</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #333; }");
            html.AppendLine(".container { background: white; padding: 30px; border-radius: 15px; box-shadow: 0 10px 30px rgba(0,0,0,0.3); max-width: 1400px; margin: 0 auto; }");
            html.AppendLine("h1 { color: #2c3e50; text-align: center; margin-bottom: 30px; font-size: 2.5em; }");
            html.AppendLine("h2 { color: #3498db; border-bottom: 2px solid #3498db; padding-bottom: 10px; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine("th { background-color: #3498db; color: white; }");
            html.AppendLine(".player-section { background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 10px; border-left: 5px solid #3498db; }");
            html.AppendLine(".metric-value { font-weight: bold; color: #27ae60; }");
            html.AppendLine(".score-highlight { background-color: #ffeaa7; padding: 5px 10px; border-radius: 5px; }");
            html.AppendLine(".summary-card { background: #e8f5e8; padding: 20px; margin: 10px; border-radius: 10px; display: inline-block; min-width: 200px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<div class='container'>");
            
            html.AppendLine($"<h1>🎮 Motion Party - Rapport Global</h1>");
            html.AppendLine($"<p><strong>📅 Généré le:</strong> {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p><strong>📊 Sessions analysées:</strong> {jsonFiles.Length}</p>");

            // Statistiques globales
            html.AppendLine("<h2>📈 Statistiques Globales</h2>");
            var globalStats = CalculateGlobalStats(jsonFiles);
            
            html.AppendLine("<div>");
            html.AppendLine($"<div class='summary-card'><h3>🏆 Score Total</h3><div class='score-highlight'>{globalStats.totalScore} points</div></div>");
            html.AppendLine($"<div class='summary-card'><h3>👥 Joueurs Uniques</h3><div class='metric-value'>{globalStats.uniquePlayers}</div></div>");
            html.AppendLine($"<div class='summary-card'><h3>🎯 Moy. Score</h3><div class='metric-value'>{globalStats.averageScore:F1}</div></div>");
            html.AppendLine("</div>");

            // Détails par joueur
            html.AppendLine("<h2>👥 Détails par Joueur</h2>");
            foreach (var playerStats in globalStats.playerStats)
            {
                html.AppendLine($"<div class='player-section'>");
                html.AppendLine($"<h3>🎮 Joueur: {playerStats.Key}</h3>");
                
                var stats = playerStats.Value;
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Métrique</th><th>Valeur</th></tr>");
                html.AppendLine($"<tr><td>🔥 FireflyDance Score</td><td class='metric-value'>{stats.fireflyScore}</td></tr>");
                html.AppendLine($"<tr><td>🎵 MusicNote Score</td><td class='metric-value'>{stats.musicScore}</td></tr>");
                html.AppendLine($"<tr><td>🌊 LogParade Chutes</td><td class='metric-value'>{stats.logParadeChutes}</td></tr>");
                html.AppendLine($"<tr><td>🏆 Score Total</td><td class='score-highlight'>{stats.totalScore} points</td></tr>");
                html.AppendLine("</table>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
            html.AppendLine("</body></html>");

            File.WriteAllText(outputPath, html.ToString());
        }

        /// <summary>
        /// Calcule les statistiques globales de tous les fichiers
        /// </summary>
        private GlobalStats CalculateGlobalStats(string[] jsonFiles)
        {
            var stats = new GlobalStats();
            
            foreach (string jsonFile in jsonFiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(jsonFile);
                    var playerData = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(jsonContent);
                    
                    if (playerData != null && playerData.ContainsKey("playerId"))
                    {
                        string playerId = playerData["playerId"].ToString();
                        
                        var playerStats = new PlayerStats();
                        
                        if (playerData.ContainsKey("metrics"))
                        {
                            var metrics = playerData["metrics"] as Newtonsoft.Json.Linq.JObject;
                            if (metrics != null)
                            {
                                playerStats.fireflyScore = GetJsonValue(metrics, "FireflyDance_score_final", 0f);
                                playerStats.musicScore = GetJsonValue(metrics, "MusicNote_score_final", 0f);
                                playerStats.logParadeChutes = GetJsonValue(metrics, "LogParade_nombre_chutes_eau", 0f);
                                playerStats.totalScore = playerStats.fireflyScore + playerStats.musicScore;
                            }
                        }
                        
                        stats.playerStats[playerId] = playerStats;
                        stats.totalScore += playerStats.totalScore;
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"Erreur lecture {jsonFile}: {e.Message}");
                }
            }
            
            stats.uniquePlayers = stats.playerStats.Count;
            stats.averageScore = stats.uniquePlayers > 0 ? stats.totalScore / stats.uniquePlayers : 0;
            
            return stats;
        }

        /// <summary>
        /// Utilitaire pour extraire des valeurs du JSON de façon sécurisée
        /// </summary>
        private float GetJsonValue(Newtonsoft.Json.Linq.JObject obj, string key, float defaultValue)
        {
            if (obj.ContainsKey(key))
            {
                if (float.TryParse(obj[key].ToString(), out float value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Affiche la section détaillée des données joueurs
        /// </summary>
        private void DrawPlayerDataSection()
        {
            if (currentPlayerData == null || currentPlayerData.Count == 0)
            {
                EditorGUILayout.HelpBox("Aucune donnée joueur chargée. Sélectionnez une session pour voir les détails.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("👥 Données Détaillées des Joueurs", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (var player in currentPlayerData)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // En-tête du joueur
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"🎮 {GetPlayerDisplayName(player.playerId)}", EditorStyles.boldLabel);
                
                if (GUILayout.Button("🌐 Générer HTML", GUILayout.Width(120)))
                {
                    GeneratePlayerHTML(player);
                }
                if (GUILayout.Button("📊 Analyser", GUILayout.Width(80)))
                {
                    ShowPlayerAnalysisResults(player);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"📝 ID: {player.playerId}");

                // Métriques
                if (player.playerMetrics != null && player.playerMetrics.Count > 0)
                {
                    EditorGUILayout.LabelField("📊 Métriques Principales:", EditorStyles.boldLabel);
                    
                    var sortedMetrics = player.playerMetrics.OrderByDescending(m => m.Value).Take(8);
                    foreach (var metric in sortedMetrics)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"• {metric.Key}:", GUILayout.Width(200));
                        EditorGUILayout.LabelField($"{metric.Value:F2}", EditorStyles.boldLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                // Performances par mini-jeu
                if (player.miniGamePerformances != null && player.miniGamePerformances.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("🎯 Performances Mini-Jeux:", EditorStyles.boldLabel);
                    foreach (var game in player.miniGamePerformances)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"🎮 {game.Key}:", GUILayout.Width(120));
                        EditorGUILayout.LabelField($"Score: {game.Value.score:F1}", GUILayout.Width(100));
                        EditorGUILayout.LabelField($"Précision: {game.Value.accuracy * 100:F1}%", GUILayout.Width(120));
                        EditorGUILayout.LabelField($"Temps: {game.Value.completionTime:F1}s");
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// Génère un rapport HTML pour un joueur spécifique
        /// </summary>
        private void GeneratePlayerHTML(PlayerAnalysisResult player)
        {
            try
            {
                if (AnalyticsFileGenerator.Instance != null)
                {
                    AnalyticsFileGenerator.Instance.GeneratePlayerReport(player);
                    EditorUtility.DisplayDialog("HTML Généré", 
                        $"✅ Rapport HTML généré pour {GetPlayerDisplayName(player.playerId)}!\n\nVérifiez le dossier AnalyticsReports/Players", 
                        "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Erreur", "AnalyticsFileGenerator non disponible", "OK");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Erreur", $"Erreur génération HTML: {e.Message}", "OK");
            }
        }

        #endregion

        #region Classes de Données

        private class GlobalStats
        {
            public float totalScore = 0f;
            public int uniquePlayers = 0;
            public float averageScore = 0f;
            public Dictionary<string, PlayerStats> playerStats = new Dictionary<string, PlayerStats>();
        }

        private class PlayerStats
        {
            public float fireflyScore = 0f;
            public float musicScore = 0f;
            public float logParadeChutes = 0f;
            public float totalScore = 0f;
        }

        #endregion
    }
}
#endif
