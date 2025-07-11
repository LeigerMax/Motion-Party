using UnityEngine;
using UnityEditor;
using Systems;

namespace Gameplay.FireFlyDance.Analytics.Editor
{
    /// <summary>
    /// Éditeur pour configurer facilement les données démographiques des joueurs
    /// </summary>
    public class PlayerDemographicsEditor : EditorWindow
    {
        private string playerName = "";
        private string playerTeam = "";
        private string birthDate = "";
        private int calculatedAge = 0;
        private string ageGroup = "";

        [MenuItem("GameAnalytics/Configuration Joueurs")]
        public static void ShowWindow()
        {
            GetWindow<PlayerDemographicsEditor>("Configuration Joueurs");
        }

        private void OnGUI()
        {
            GUILayout.Label("Configuration des Données Démographiques", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Informations de base
            GUILayout.Label("Informations Joueur", EditorStyles.boldLabel);
            playerName = EditorGUILayout.TextField("Nom/Pseudo:", playerName);
            playerTeam = EditorGUILayout.TextField("Équipe/Maison:", playerTeam);
            
            GUILayout.Space(10);

            // Date de naissance
            GUILayout.Label("Informations Démographiques", EditorStyles.boldLabel);
            birthDate = EditorGUILayout.TextField("Date naissance (YYYY-MM-DD):", birthDate);
            
            // Validation et calcul de l'âge
            if (!string.IsNullOrEmpty(birthDate))
            {
                try
                {
                    var birth = System.DateTime.ParseExact(birthDate, "yyyy-MM-dd", null);
                    var today = System.DateTime.Today;
                    calculatedAge = today.Year - birth.Year;
                    if (birth.Date > today.AddYears(-calculatedAge))
                        calculatedAge--;

                    ageGroup = DetermineAgeGroup(calculatedAge);

                    EditorGUILayout.LabelField("Âge calculé:", calculatedAge.ToString());
                    EditorGUILayout.LabelField("Groupe d'âge:", ageGroup);
                }
                catch
                {
                    EditorGUILayout.LabelField("❌ Format de date invalide");
                }
            }

            GUILayout.Space(20);

            // Boutons d'action
            if (GUILayout.Button("Créer Nouveau Joueur"))
            {
                CreateNewPlayer();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Test Analytics Démographiques"))
            {
                TestDemographicAnalytics();
            }

            GUILayout.Space(10);

            // Import/Export en masse
            GUILayout.Label("Import/Export Masse", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Générer Template CSV"))
            {
                string templatePath = EditorUtility.SaveFilePanel(
                    "Sauvegarder Template CSV", 
                    Application.dataPath, 
                    "template_joueurs.csv", 
                    "csv");
                
                if (!string.IsNullOrEmpty(templatePath))
                {
                    PlayerDemographicsImporter.GenerateExampleCSV(templatePath);
                }
            }

            if (GUILayout.Button("Importer Joueurs depuis CSV"))
            {
                string csvPath = EditorUtility.OpenFilePanel(
                    "Sélectionner fichier CSV", 
                    Application.dataPath, 
                    "csv");
                
                if (!string.IsNullOrEmpty(csvPath))
                {
                    var players = PlayerDemographicsImporter.ImportPlayersFromCSV(csvPath);
                    PlayerDemographicsImporter.ValidatePlayersData(players);
                    
                    if (players.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Import Réussi", 
                            $"{players.Count} joueurs importés avec succès!\nConsultez la Console pour les détails.", 
                            "OK");
                    }
                }
            }

            GUILayout.Space(20);

            // Informations d'aide
            GUILayout.Label("Aide", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "• Format date: YYYY-MM-DD (ex: 1950-12-25)\n" +
                "• Groupes d'âge: Mineur, Jeune Adulte, Adulte, Adulte Mature, Senior, Senior Avancé\n" +
                "• Les données sont automatiquement utilisées par le système d'analytics",
                MessageType.Info
            );
        }

        private void CreateNewPlayer()
        {
            if (string.IsNullOrEmpty(playerName))
            {
                EditorUtility.DisplayDialog("Erreur", "Le nom du joueur est requis", "OK");
                return;
            }

            var newPlayer = new PlayerData(playerName, playerTeam, birthDate);
            
            Debug.Log($"✅ Joueur créé: {newPlayer.Nickname}");
            if (!string.IsNullOrEmpty(birthDate))
            {
                Debug.Log($"   Âge: {newPlayer.CalculateAge()} ans ({DetermineAgeGroup(newPlayer.CalculateAge())})");
            }
            Debug.Log($"   Équipe: {newPlayer.TeamName}");
            Debug.Log($"   ID: {newPlayer.Id}");

            // Note: Dans un vrai système, vous sauvegarderiez ce joueur dans votre base de données
            EditorUtility.DisplayDialog("Succès", 
                $"Joueur '{playerName}' créé avec succès!\n" +
                (calculatedAge > 0 ? $"Âge: {calculatedAge} ans ({ageGroup})" : ""), 
                "OK");

            // Réinitialiser les champs
            playerName = "";
            playerTeam = "";
            birthDate = "";
            calculatedAge = 0;
            ageGroup = "";
        }

        private void TestDemographicAnalytics()
        {
            Debug.Log("=== TEST ANALYTICS DÉMOGRAPHIQUES ===");

            // Créer des sessions d'exemple avec différents profils d'âge
            var sessions = new System.Collections.Generic.List<SessionData>();

            // Session senior
            var seniorSession = CreateTestSession("Marie Dupont", "1948-06-15", 2.9f, 14, 20);
            sessions.Add(seniorSession);

            // Session adulte mature
            var matureSession = CreateTestSession("Jean Martin", "1970-03-22", 2.2f, 17, 20);
            sessions.Add(matureSession);

            // Session jeune adulte
            var youngSession = CreateTestSession("Sophie Leroy", "1996-11-08", 1.7f, 19, 20);
            sessions.Add(youngSession);

            // Génération des rapports
            foreach (var session in sessions)
            {
                Debug.Log(SessionAnalyzer.GenerateDetailedReport(session));
                Debug.Log("---");
            }

            // Analyse comparative par âge
            Debug.Log(SessionAnalyzer.CompareSessionsByAgeGroup(sessions));

            Debug.Log("Test terminé - Consultez la Console pour les résultats");
        }

        private SessionData CreateTestSession(string name, string birthDate, float avgReaction, int captured, int total)
        {
            var session = new SessionData();
            session.playerName = name;
            session.playerBirthDate = birthDate;
            session.playerAge = session.CalculatePlayerAge();
            session.ageGroup = session.DetermineAgeGroup(session.playerAge);
            session.totalDuration = 120f;
            session.totalFireflies = total;
            session.capturedFireflies = captured;
            session.missedFireflies = total - captured;
            session.averageReactionTime = avgReaction;
            session.minReactionTime = avgReaction * 0.7f;
            session.maxReactionTime = avgReaction * 1.8f;
            session.totalHandClosures = captured + 5;
            session.emptyHandClosures = 5;
            session.handMovementDistance = 180f;

            return session;
        }

        private string DetermineAgeGroup(int age)
        {
            if (age < 18) return "Mineur";
            if (age < 30) return "Jeune Adulte";
            if (age < 50) return "Adulte";
            if (age < 65) return "Adulte Mature";
            if (age < 80) return "Senior";
            return "Senior Avancé";
        }
    }
}
