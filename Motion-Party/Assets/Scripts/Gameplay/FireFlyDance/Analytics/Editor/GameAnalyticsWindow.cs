#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Gameplay.FireFlyDance.Analytics;

namespace Gameplay.FireFlyDance.Analytics.Editor
{
    /// <summary>
    /// Fenêtre d'édition Unity pour gérer et analyser les données GameAnalytics
    /// </summary>
    public class GameAnalyticsWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> sessionFiles = new List<string>();
        private int selectedSessionIndex = -1;
        private SessionData currentSession = null;
        private string reportText = "";

        [MenuItem("Firefly Game/Analytics Dashboard")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameAnalyticsWindow>("Game Analytics");
            window.minSize = new Vector2(600, 400);
            window.RefreshSessionList();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("🔬 Firefly Game Analytics Dashboard", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Boutons d'actions principales
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Actualiser Liste"))
            {
                RefreshSessionList();
            }
            if (GUILayout.Button("📁 Ouvrir Dossier"))
            {
                OpenSaveDirectory();
            }
            if (GUILayout.Button("📊 Analyser Toutes"))
            {
                AnalyzeAllSessions();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Informations sur les sessions trouvées
            EditorGUILayout.LabelField($"Sessions trouvées: {sessionFiles.Count}", EditorStyles.helpBox);

            if (sessionFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("Aucune session trouvée. Jouez au jeu de lucioles pour générer des données.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();

            // Liste des sessions
            EditorGUILayout.LabelField("📋 Sessions Disponibles", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            
            for (int i = 0; i < sessionFiles.Count; i++)
            {
                string filename = Path.GetFileName(sessionFiles[i]);
                bool isSelected = i == selectedSessionIndex;
                
                if (isSelected) GUI.backgroundColor = Color.cyan;
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(filename, EditorStyles.miniButton))
                {
                    selectedSessionIndex = i;
                    LoadSelectedSession();
                }
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    selectedSessionIndex = i;
                    LoadSelectedSession();
                    AnalyzeCurrentSession();
                }
                EditorGUILayout.EndHorizontal();
                
                if (isSelected) GUI.backgroundColor = Color.white;
            }
            
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Actions sur la session sélectionnée
            if (selectedSessionIndex >= 0 && selectedSessionIndex < sessionFiles.Count)
            {
                EditorGUILayout.LabelField("🎯 Session Sélectionnée", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("📖 Charger"))
                {
                    LoadSelectedSession();
                }
                if (GUILayout.Button("📊 Analyser"))
                {
                    AnalyzeCurrentSession();
                }
                if (GUILayout.Button("💾 Exporter CSV"))
                {
                    ExportSessionToCSV();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Affichage du rapport
            if (!string.IsNullOrEmpty(reportText))
            {
                EditorGUILayout.LabelField("📈 Rapport d'Analyse", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                EditorGUILayout.TextArea(reportText, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("💾 Sauvegarder Rapport"))
                {
                    SaveCurrentReport();
                }
                if (GUILayout.Button("📋 Copier"))
                {
                    EditorGUIUtility.systemCopyBuffer = reportText;
                }
                EditorGUILayout.EndHorizontal();
            }

            // Informations système
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ℹ️ Informations Système", EditorStyles.boldLabel);
            string saveDir = Path.Combine(Application.persistentDataPath, "GameAnalytics");
            EditorGUILayout.LabelField($"Dossier: {saveDir}", EditorStyles.miniLabel);
        }

        private void RefreshSessionList()
        {
            sessionFiles = SessionAnalyzer.FindAllSessionFiles();
            selectedSessionIndex = -1;
            currentSession = null;
            reportText = "";
            
            Debug.Log($"GameAnalytics: {sessionFiles.Count} sessions trouvées");
        }

        private void LoadSelectedSession()
        {
            if (selectedSessionIndex < 0 || selectedSessionIndex >= sessionFiles.Count) return;

            string filePath = sessionFiles[selectedSessionIndex];
            currentSession = SessionAnalyzer.LoadSessionFromFile(filePath);

            if (currentSession != null)
            {
                reportText = $"Session chargée: {currentSession.playerName}\n";
                reportText += $"Date: {currentSession.sessionStart}\n";
                reportText += $"Durée: {currentSession.totalDuration:F1}s\n";
                reportText += $"Score: {currentSession.capturedFireflies}/{currentSession.totalFireflies}\n";
                Debug.Log($"Session chargée: {currentSession.sessionId}");
            }
            else
            {
                reportText = "❌ Erreur lors du chargement de la session";
            }
        }

        private void AnalyzeCurrentSession()
        {
            if (currentSession == null)
            {
                LoadSelectedSession();
            }

            if (currentSession != null)
            {
                reportText = SessionAnalyzer.GenerateDetailedReport(currentSession);
                Debug.Log("Analyse détaillée générée");
            }
            else
            {
                reportText = "❌ Impossible d'analyser - session non chargée";
            }
        }

        private void AnalyzeAllSessions()
        {
            if (sessionFiles.Count == 0)
            {
                reportText = "Aucune session à analyser";
                return;
            }

            var sessions = new List<SessionData>();
            foreach (string filePath in sessionFiles)
            {
                var session = SessionAnalyzer.LoadSessionFromFile(filePath);
                if (session != null)
                {
                    sessions.Add(session);
                }
            }

            if (sessions.Count > 0)
            {
                reportText = SessionAnalyzer.CompareMultipleSessions(sessions);
                Debug.Log($"Analyse comparative de {sessions.Count} sessions générée");
            }
            else
            {
                reportText = "❌ Aucune session valide trouvée";
            }
        }

        private void ExportSessionToCSV()
        {
            if (currentSession == null)
            {
                EditorUtility.DisplayDialog("Erreur", "Aucune session sélectionnée", "OK");
                return;
            }

            var sessions = new List<SessionData> { currentSession };
            string csvData = SessionAnalyzer.ExportToCSV(sessions);

            string filename = $"Session_{currentSession.sessionId.Substring(0, 8)}.csv";
            string saveDirectory = Path.Combine(Application.persistentDataPath, "GameAnalytics");
            string fullPath = Path.Combine(saveDirectory, filename);

            try
            {
                File.WriteAllText(fullPath, csvData);
                EditorUtility.DisplayDialog("Export Réussi", $"Fichier CSV sauvegardé:\n{fullPath}", "OK");
                Debug.Log($"CSV exporté: {fullPath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Erreur Export", $"Erreur lors de l'export:\n{e.Message}", "OK");
            }
        }

        private void SaveCurrentReport()
        {
            if (string.IsNullOrEmpty(reportText))
            {
                EditorUtility.DisplayDialog("Erreur", "Aucun rapport à sauvegarder", "OK");
                return;
            }

            string filename = $"Report_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            SessionAnalyzer.SaveReportToFile(reportText, filename);
            EditorUtility.DisplayDialog("Rapport Sauvegardé", $"Rapport sauvegardé: {filename}", "OK");
        }

        private void OpenSaveDirectory()
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, "GameAnalytics");
            
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            EditorUtility.RevealInFinder(saveDirectory);
        }
    }

    /// <summary>
    /// Éditeur personnalisé pour le composant GameStatsRecorder
    /// </summary>
    [CustomEditor(typeof(GameStatsRecorder))]
    public class GameStatsRecorderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GameStatsRecorder recorder = (GameStatsRecorder)target;

            // Affichage par défaut
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("🔧 Contrôles Rapides", EditorStyles.boldLabel);

            // Boutons de contrôle
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("▶️ Test Start"))
            {
                if (Application.isPlaying)
                {
                    recorder.StartRecording("Test Player");
                }
                else
                {
                    EditorUtility.DisplayDialog("Mode Play Requis", "Ce test nécessite le mode Play", "OK");
                }
            }

            if (GUILayout.Button("⏹️ Test Stop"))
            {
                if (Application.isPlaying)
                {
                    recorder.EndRecording();
                }
                else
                {
                    EditorUtility.DisplayDialog("Mode Play Requis", "Ce test nécessite le mode Play", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📁 Ouvrir Dossier"))
            {
                string saveDirectory = Path.Combine(Application.persistentDataPath, "GameAnalytics");
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }
                EditorUtility.RevealInFinder(saveDirectory);
            }

            if (GUILayout.Button("📊 Dashboard"))
            {
                GameAnalyticsWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();

            // Informations en temps réel
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("📱 État Temps Réel", EditorStyles.boldLabel);
                
                bool isRecording = recorder.enabled && recorder.gameObject.activeInHierarchy;
                EditorGUILayout.LabelField($"Enregistrement actif: {(isRecording ? "✅ OUI" : "❌ NON")}");
                
                if (isRecording)
                {
                    string summary = recorder.GetCurrentSessionSummary();
                    if (!string.IsNullOrEmpty(summary))
                    {
                        EditorGUILayout.TextArea(summary, EditorStyles.helpBox);
                    }
                }
            }
        }
    }
}
#endif
