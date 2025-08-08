using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Éditeur pour la gestion globale des badges de Motion-Party
    /// Permet de créer, modifier et tester le système de badges universels
    /// </summary>
    public class GlobalBadgeSystemEditor : EditorWindow
    {
        #region Fields

        private GlobalBadgeDatabase database;
        private GlobalBadgeSystem badgeSystem;
        private GlobalBadgeTracker badgeTracker;
        private GlobalPlayerBadgeStorage playerStorage;

        // Interface
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabs = { "Base de Données", "Système", "Tracking", "Joueurs", "Test" };

        // Données d'interface
        private string newBadgeGameId = "firefly";
        private string newBadgeId = "";
        private string testPlayerName = "TestPlayer";
        private string testGameId = "firefly";
        private string testBadgeId = "";
        private float testValue = 0f;

        // Styles
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized = false;

        #endregion

        #region Menu

        [MenuItem("Window/Gameplay/Global Badge System")]
        public static void ShowWindow()
        {
            var window = GetWindow<GlobalBadgeSystemEditor>("Global Badge System");
            window.titleContent = new GUIContent("🎖️ Badge System");
            window.minSize = new Vector2(600, 400);
        }

        #endregion

        #region Unity Editor

        void OnEnable()
        {
            RefreshReferences();
        }

        void OnGUI()
        {
            InitializeStyles();
            
            EditorGUILayout.LabelField("Motion-Party - Système de Badges Global", headerStyle);
            EditorGUILayout.Space();

            // Status
            DrawSystemStatus();
            EditorGUILayout.Space();

            // Tabs
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0: DrawDatabaseTab(); break;
                case 1: DrawSystemTab(); break;
                case 2: DrawTrackingTab(); break;
                case 3: DrawPlayersTab(); break;
                case 4: DrawTestTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region UI Drawing

        /// <summary>
        /// Affiche le statut du système
        /// </summary>
        private void DrawSystemStatus()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            GUILayout.Label("Status:", GUILayout.Width(50));
            
            // Database
            var dbColor = database != null ? Color.green : Color.red;
            GUI.color = dbColor;
            GUILayout.Label($"DB: {(database != null ? "✓" : "✗")}", GUILayout.Width(40));
            
            // System
            var sysColor = badgeSystem != null ? Color.green : Color.red;
            GUI.color = sysColor;
            GUILayout.Label($"SYS: {(badgeSystem != null ? "✓" : "✗")}", GUILayout.Width(50));
            
            // Tracker
            var trackColor = badgeTracker != null ? Color.green : Color.red;
            GUI.color = trackColor;
            GUILayout.Label($"TRACK: {(badgeTracker != null ? "✓" : "✗")}", GUILayout.Width(60));
            
            // Storage
            var storageColor = playerStorage != null ? Color.green : Color.red;
            GUI.color = storageColor;
            GUILayout.Label($"STORE: {(playerStorage != null ? "✓" : "✗")}", GUILayout.Width(60));
            
            GUI.color = Color.white;
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                RefreshReferences();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Onglet Base de Données
        /// </summary>
        private void DrawDatabaseTab()
        {
            EditorGUILayout.LabelField("📚 Gestion de la Base de Données", EditorStyles.boldLabel);
            
            // Référence
            database = (GlobalBadgeDatabase)EditorGUILayout.ObjectField("Database", database, typeof(GlobalBadgeDatabase), false);
            
            if (database == null)
            {
                EditorGUILayout.HelpBox("Aucune base de données de badges sélectionnée", MessageType.Warning);
                
                if (GUILayout.Button("Créer une nouvelle GlobalBadgeDatabase"))
                {
                    CreateNewDatabase();
                }
                return;
            }
            
            EditorGUILayout.Space();
            
            // Statistiques
            var allBadges = database.GetAllBadges();
            var gameIds = database.GetAllGameIds();
            
            EditorGUILayout.LabelField($"Badges totaux: {allBadges.Count}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Jeux couverts: {gameIds.Count}", EditorStyles.helpBox);
            
            EditorGUILayout.Space();
            
            // Badges par jeu
            EditorGUILayout.LabelField("Badges par Jeu:", EditorStyles.boldLabel);
            foreach (var gameId in gameIds)
            {
                var gameBadges = database.GetBadgesForGame(gameId);
                EditorGUILayout.LabelField($"• {gameId}: {gameBadges.Count} badge(s)");
                
                EditorGUI.indentLevel++;
                foreach (var badge in gameBadges.Take(3)) // Afficher les 3 premiers
                {
                    EditorGUILayout.LabelField($"- {badge.BadgeName} ({badge.Rarity})");
                }
                if (gameBadges.Count > 3)
                {
                    EditorGUILayout.LabelField($"... et {gameBadges.Count - 3} autre(s)");
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Actions
            EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ajouter Badges d'Exemple"))
            {
                database.CreateDefaultBadges();
                EditorUtility.SetDirty(database);
            }
            
            if (GUILayout.Button("Valider Base"))
            {
                database.ValidateDatabase();
            }
            EditorGUILayout.EndHorizontal();
            
            // Création de badge
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Créer un Nouveau Badge:", EditorStyles.boldLabel);
            
            newBadgeGameId = EditorGUILayout.TextField("ID du Jeu", newBadgeGameId);
            newBadgeId = EditorGUILayout.TextField("ID du Badge", newBadgeId);
            
            if (GUILayout.Button("Créer Badge") && !string.IsNullOrEmpty(newBadgeId))
            {
                CreateNewBadge();
            }
        }

        /// <summary>
        /// Onglet Système
        /// </summary>
        private void DrawSystemTab()
        {
            EditorGUILayout.LabelField("⚙️ Configuration du Système", EditorStyles.boldLabel);
            
            // Références
            badgeSystem = (GlobalBadgeSystem)EditorGUILayout.ObjectField("Badge System", badgeSystem, typeof(GlobalBadgeSystem), true);
            
            if (badgeSystem == null)
            {
                EditorGUILayout.HelpBox("GlobalBadgeSystem non trouvé dans la scène", MessageType.Warning);
                
                if (GUILayout.Button("Créer GlobalBadgeSystem"))
                {
                    CreateBadgeSystem();
                }
                return;
            }
            
            EditorGUILayout.Space();
            
            // Configuration
            EditorGUILayout.LabelField("Configuration:", EditorStyles.boldLabel);
            
            SerializedObject so = new SerializedObject(badgeSystem);
            SerializedProperty badgeDatabase = so.FindProperty("badgeDatabase");
            SerializedProperty playerStorage = so.FindProperty("playerStorage");
            SerializedProperty enableDebugLogs = so.FindProperty("enableDebugLogs");
            SerializedProperty autoSave = so.FindProperty("autoSave");
            SerializedProperty validateBadgeConditions = so.FindProperty("validateBadgeConditions");
            
            EditorGUILayout.PropertyField(badgeDatabase);
            EditorGUILayout.PropertyField(playerStorage);
            EditorGUILayout.PropertyField(enableDebugLogs);
            EditorGUILayout.PropertyField(autoSave);
            EditorGUILayout.PropertyField(validateBadgeConditions);
            
            so.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            
            // Actions
            EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Attribution"))
            {
                badgeSystem.TestBadgeAttribution();
            }
            
            if (GUILayout.Button("Statistiques"))
            {
                badgeSystem.ShowSystemStatistics();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Onglet Tracking
        /// </summary>
        private void DrawTrackingTab()
        {
            EditorGUILayout.LabelField("📊 Système de Tracking", EditorStyles.boldLabel);
            
            // Référence
            badgeTracker = (GlobalBadgeTracker)EditorGUILayout.ObjectField("Badge Tracker", badgeTracker, typeof(GlobalBadgeTracker), true);
            
            if (badgeTracker == null)
            {
                EditorGUILayout.HelpBox("GlobalBadgeTracker non trouvé dans la scène", MessageType.Warning);
                
                if (GUILayout.Button("Créer GlobalBadgeTracker"))
                {
                    CreateBadgeTracker();
                }
                return;
            }
            
            EditorGUILayout.Space();
            
            // Configuration
            EditorGUILayout.LabelField("Configuration:", EditorStyles.boldLabel);
            
            SerializedObject so = new SerializedObject(badgeTracker);
            SerializedProperty badgeSystemProp = so.FindProperty("badgeSystem");
            SerializedProperty enableAutoTracking = so.FindProperty("enableAutoTracking");
            SerializedProperty enableRealTimeValidation = so.FindProperty("enableRealTimeValidation");
            SerializedProperty validationInterval = so.FindProperty("validationInterval");
            SerializedProperty showTrackingLogs = so.FindProperty("showTrackingLogs");
            
            EditorGUILayout.PropertyField(badgeSystemProp);
            EditorGUILayout.PropertyField(enableAutoTracking);
            EditorGUILayout.PropertyField(enableRealTimeValidation);
            EditorGUILayout.PropertyField(validationInterval);
            EditorGUILayout.PropertyField(showTrackingLogs);
            
            so.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            
            // Actions
            EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Tracking"))
            {
                badgeTracker.TestTracking();
            }
            
            if (GUILayout.Button("Statistiques"))
            {
                badgeTracker.ShowTrackingStatistics();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Valider Tous les Joueurs"))
            {
                int validated = badgeTracker.ValidateAllPlayers();
                Debug.Log($"Validation terminée: {validated} badge(s) attribué(s)");
            }
        }

        /// <summary>
        /// Onglet Joueurs
        /// </summary>
        private void DrawPlayersTab()
        {
            EditorGUILayout.LabelField("👥 Gestion des Joueurs", EditorStyles.boldLabel);
            
            // Référence
            playerStorage = (GlobalPlayerBadgeStorage)EditorGUILayout.ObjectField("Player Storage", playerStorage, typeof(GlobalPlayerBadgeStorage), true);
            
            if (playerStorage == null)
            {
                EditorGUILayout.HelpBox("GlobalPlayerBadgeStorage non trouvé dans la scène", MessageType.Warning);
                
                if (GUILayout.Button("Créer GlobalPlayerBadgeStorage"))
                {
                    CreatePlayerStorage();
                }
                return;
            }
            
            EditorGUILayout.Space();
            
            // Statistiques globales
            var globalStats = playerStorage.GetGlobalStatistics();
            EditorGUILayout.LabelField("Statistiques Globales:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Joueurs totaux: {globalStats["totalPlayers"]}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Badges totaux: {globalStats["totalBadges"]}", EditorStyles.helpBox);
            
            // Badges par jeu
            if (globalStats["badgesByGame"] is Dictionary<string, int> gameStats)
            {
                EditorGUILayout.LabelField("Badges par jeu:");
                foreach (var kvp in gameStats)
                {
                    EditorGUILayout.LabelField($"• {kvp.Key}: {kvp.Value} badge(s)");
                }
            }
            
            EditorGUILayout.Space();
            
            // Liste des joueurs
            var players = playerStorage.GetPlayersWithBadges();
            EditorGUILayout.LabelField($"Joueurs avec badges ({players.Count}):", EditorStyles.boldLabel);
            
            foreach (var playerName in players.Take(5)) // Afficher les 5 premiers
            {
                var badgeCount = playerStorage.GetPlayerBadgeCount(playerName);
                EditorGUILayout.LabelField($"• {playerName}: {badgeCount} badge(s)");
            }
            
            if (players.Count > 5)
            {
                EditorGUILayout.LabelField($"... et {players.Count - 5} autre(s) joueur(s)");
            }
            
            EditorGUILayout.Space();
            
            // Actions
            EditorGUILayout.LabelField("Actions:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Statistiques Complètes"))
            {
                playerStorage.ShowGlobalBadgeStatistics();
            }
            
            if (GUILayout.Button("Effacer Toutes les Données", buttonStyle))
            {
                if (EditorUtility.DisplayDialog("Confirmation", 
                    "Êtes-vous sûr de vouloir effacer toutes les données de badges ?", 
                    "Oui", "Annuler"))
                {
                    playerStorage.ClearAllBadgeData();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Onglet Test
        /// </summary>
        private void DrawTestTab()
        {
            EditorGUILayout.LabelField("🧪 Tests et Debug", EditorStyles.boldLabel);
            
            if (badgeSystem == null)
            {
                EditorGUILayout.HelpBox("GlobalBadgeSystem requis pour les tests", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.Space();
            
            // Test d'attribution de badge
            EditorGUILayout.LabelField("Test d'Attribution de Badge:", EditorStyles.boldLabel);
            
            testPlayerName = EditorGUILayout.TextField("Nom du Joueur", testPlayerName);
            testGameId = EditorGUILayout.TextField("ID du Jeu", testGameId);
            testBadgeId = EditorGUILayout.TextField("ID du Badge", testBadgeId);
            testValue = EditorGUILayout.FloatField("Valeur de Test", testValue);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Tester Attribution"))
            {
                bool success = badgeSystem.TryEarnBadge(testPlayerName, testGameId, testBadgeId, testValue);
                Debug.Log($"Test d'attribution: {(success ? "SUCCÈS" : "ÉCHEC")}");
            }
            
            if (GUILayout.Button("Attribution Forcée"))
            {
                bool success = badgeSystem.ForceEarnBadge(testPlayerName, testGameId, testBadgeId, testValue);
                Debug.Log($"Attribution forcée: {(success ? "SUCCÈS" : "ÉCHEC")}");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Test de tracking
            if (badgeTracker != null)
            {
                EditorGUILayout.LabelField("Test de Tracking:", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Update Score"))
                {
                    badgeTracker.UpdateScore(testPlayerName, testGameId, testValue);
                }
                
                if (GUILayout.Button("Update Time"))
                {
                    badgeTracker.UpdateTime(testPlayerName, testGameId, testValue);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Increment Counter"))
                {
                    badgeTracker.IncrementCounter(testPlayerName, testGameId, "test_counter");
                }
                
                if (GUILayout.Button("Update Accuracy"))
                {
                    badgeTracker.UpdateAccuracy(testPlayerName, testGameId, testValue);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            // Actions système
            EditorGUILayout.LabelField("Actions Système:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Créer Système Complet"))
            {
                CreateCompleteSystem();
            }
            
            if (GUILayout.Button("Test Intégration"))
            {
                TestSystemIntegration();
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Initialise les styles d'interface
        /// </summary>
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.red },
                fontStyle = FontStyle.Bold
            };
            
            stylesInitialized = true;
        }

        /// <summary>
        /// Actualise les références aux composants
        /// </summary>
        private void RefreshReferences()
        {
            // Rechercher dans les assets
            if (database == null)
            {
                var databases = AssetDatabase.FindAssets("t:GlobalBadgeDatabase");
                if (databases.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(databases[0]);
                    database = AssetDatabase.LoadAssetAtPath<GlobalBadgeDatabase>(path);
                }
            }
            
            // Rechercher dans la scène
            if (badgeSystem == null)
                badgeSystem = FindFirstObjectByType<GlobalBadgeSystem>();
            
            if (badgeTracker == null)
                badgeTracker = FindFirstObjectByType<GlobalBadgeTracker>();
            
            if (playerStorage == null)
                playerStorage = FindFirstObjectByType<GlobalPlayerBadgeStorage>();
        }

        /// <summary>
        /// Crée une nouvelle base de données
        /// </summary>
        private void CreateNewDatabase()
        {
            var newDatabase = CreateInstance<GlobalBadgeDatabase>();
            AssetDatabase.CreateAsset(newDatabase, "Assets/Scripts/Gameplay/Common/BadgeSystem/GlobalBadgeDatabase.asset");
            AssetDatabase.SaveAssets();
            
            database = newDatabase;
            database.CreateDefaultBadges();
            
            Debug.Log("GlobalBadgeDatabase créée avec badges d'exemple");
        }

        /// <summary>
        /// Crée un nouveau badge
        /// </summary>
        private void CreateNewBadge()
        {
            if (database == null) return;
            
            var newBadge = new BadgeDefinition(
                newBadgeId,
                $"Badge {newBadgeId}",
                "Description du badge",
                newBadgeGameId,
                "special",
                1
            );
            
            // Configuration des propriétés supplémentaires
            newBadge.SetCategory(BadgeCategory.Special);
            newBadge.SetRarity(BadgeRarity.Common);
            newBadge.SetRepeatable(false);
            
            database.AddBadge(newBadge);
            EditorUtility.SetDirty(database);
            
            Debug.Log($"Badge créé: {newBadge.GetFullBadgeId()}");
        }

        /// <summary>
        /// Crée le système de badges
        /// </summary>
        private void CreateBadgeSystem()
        {
            var go = new GameObject("GlobalBadgeSystem");
            badgeSystem = go.AddComponent<GlobalBadgeSystem>();
            
            Debug.Log("GlobalBadgeSystem créé dans la scène");
        }

        /// <summary>
        /// Crée le tracker de badges
        /// </summary>
        private void CreateBadgeTracker()
        {
            var go = new GameObject("GlobalBadgeTracker");
            badgeTracker = go.AddComponent<GlobalBadgeTracker>();
            
            Debug.Log("GlobalBadgeTracker créé dans la scène");
        }

        /// <summary>
        /// Crée le stockage des joueurs
        /// </summary>
        private void CreatePlayerStorage()
        {
            var go = new GameObject("GlobalPlayerBadgeStorage");
            playerStorage = go.AddComponent<GlobalPlayerBadgeStorage>();
            
            Debug.Log("GlobalPlayerBadgeStorage créé dans la scène");
        }

        /// <summary>
        /// Crée le système complet
        /// </summary>
        private void CreateCompleteSystem()
        {
            // Créer database si nécessaire
            if (database == null)
            {
                CreateNewDatabase();
            }
            
            // Créer composants si nécessaire
            if (playerStorage == null) CreatePlayerStorage();
            if (badgeSystem == null) CreateBadgeSystem();
            if (badgeTracker == null) CreateBadgeTracker();
            
            // Configurer les liens
            if (badgeSystem != null && database != null)
            {
                SerializedObject so = new SerializedObject(badgeSystem);
                so.FindProperty("badgeDatabase").objectReferenceValue = database;
                so.FindProperty("playerStorage").objectReferenceValue = playerStorage;
                so.ApplyModifiedProperties();
            }
            
            if (badgeTracker != null && badgeSystem != null)
            {
                SerializedObject so = new SerializedObject(badgeTracker);
                so.FindProperty("badgeSystem").objectReferenceValue = badgeSystem;
                so.ApplyModifiedProperties();
            }
            
            Debug.Log("Système de badges complet créé et configuré");
        }

        /// <summary>
        /// Teste l'intégration du système
        /// </summary>
        private void TestSystemIntegration()
        {
            if (badgeSystem == null || badgeTracker == null || playerStorage == null)
            {
                Debug.LogError("Système incomplet pour le test d'intégration");
                return;
            }
            
            // Test complet
            string testPlayer = "IntegrationTest";
            string testGame = "firefly";
            
            // Tracking
            badgeTracker.UpdateScore(testPlayer, testGame, 1500f);
            badgeTracker.UpdateTime(testPlayer, testGame, 30f);
            badgeTracker.IncrementCounter(testPlayer, testGame, "collected");
            
            // Validation
            int badgesEarned = badgeTracker.ValidatePlayerMetricsForGame(testPlayer, testGame);
            
            Debug.Log($"Test d'intégration terminé: {badgesEarned} badge(s) attribué(s) à {testPlayer}");
        }

        #endregion
    }
}
