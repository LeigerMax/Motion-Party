using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Systems;

namespace Gameplay.FireFlyDance.Analytics
{
    /// <summary>
    /// Utilitaire pour l'import en masse de données démographiques des joueurs
    /// Permet de charger des listes de joueurs depuis des fichiers CSV
    /// </summary>
    public static class PlayerDemographicsImporter
    {
        /// <summary>
        /// Import des joueurs depuis un fichier CSV
        /// Format: Nom,Equipe,DateNaissance
        /// Exemple: "Marie Dupont,Maison du Bonheur,1952-03-15"
        /// </summary>
        public static List<PlayerData> ImportPlayersFromCSV(string filePath)
        {
            var players = new List<PlayerData>();

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Fichier non trouvé: {filePath}");
                return players;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                
                // Ignorer la première ligne si c'est un en-tête
                int startIndex = 0;
                if (lines.Length > 0 && lines[0].ToLower().Contains("nom"))
                {
                    startIndex = 1;
                    Debug.Log("En-tête détecté, ignoré pour l'import");
                }

                for (int i = startIndex; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var player = ParsePlayerFromCSVLine(line, i + 1);
                    if (player != null)
                    {
                        players.Add(player);
                    }
                }

                Debug.Log($"✅ Import réussi: {players.Count} joueurs chargés depuis {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors de l'import: {e.Message}");
            }

            return players;
        }

        /// <summary>
        /// Parse une ligne CSV pour créer un PlayerData
        /// </summary>
        private static PlayerData ParsePlayerFromCSVLine(string csvLine, int lineNumber)
        {
            try
            {
                // Séparer par virgule (attention aux virgules dans les guillemets)
                var parts = SplitCSVLine(csvLine);

                if (parts.Count < 2)
                {
                    Debug.LogWarning($"Ligne {lineNumber}: Format invalide, au moins Nom et Équipe requis");
                    return null;
                }

                string name = parts[0].Trim().Trim('"');
                string team = parts[1].Trim().Trim('"');
                string birthDate = parts.Count > 2 ? parts[2].Trim().Trim('"') : "";

                // Validation de la date de naissance
                if (!string.IsNullOrEmpty(birthDate) && !IsValidDateFormat(birthDate))
                {
                    Debug.LogWarning($"Ligne {lineNumber}: Date de naissance invalide '{birthDate}', ignorée");
                    birthDate = "";
                }

                var player = new PlayerData(name, team, birthDate);
                
                // Log détaillé pour debug
                if (!string.IsNullOrEmpty(birthDate))
                {
                    int age = player.CalculateAge();
                    Debug.Log($"Joueur importé: {name}, {age} ans, équipe: {team}");
                }
                else
                {
                    Debug.Log($"Joueur importé: {name}, équipe: {team} (pas de date de naissance)");
                }

                return player;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur ligne {lineNumber}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sépare une ligne CSV en gérant les guillemets
        /// </summary>
        private static List<string> SplitCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentField = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result;
        }

        /// <summary>
        /// Vérifie si une date est au format YYYY-MM-DD
        /// </summary>
        private static bool IsValidDateFormat(string dateString)
        {
            try
            {
                System.DateTime.ParseExact(dateString, "yyyy-MM-dd", null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Exporte une liste de joueurs vers un fichier CSV
        /// </summary>
        public static void ExportPlayersToCSV(List<PlayerData> players, string filePath)
        {
            try
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Nom,Equipe,DateNaissance,Age,GroupeAge");

                foreach (var player in players)
                {
                    int age = player.CalculateAge();
                    string ageGroup = DetermineAgeGroup(age);
                    
                    csv.AppendLine($"\"{player.Nickname}\",\"{player.TeamName}\",\"{player.BirthDate}\",{age},\"{ageGroup}\"");
                }

                File.WriteAllText(filePath, csv.ToString());
                Debug.Log($"✅ Export réussi: {players.Count} joueurs exportés vers {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors de l'export: {e.Message}");
            }
        }

        /// <summary>
        /// Génère un template CSV d'exemple
        /// </summary>
        public static void GenerateExampleCSV(string filePath)
        {
            var exampleCSV = @"Nom,Equipe,DateNaissance
""Marie Dupont"",""Maison du Bonheur"",""1952-03-15""
""Jean Martin"",""Résidence des Lilas"",""1945-08-22""
""Sophie Leroy"",""Villa Sérénité"",""1960-11-08""
""Claude Moreau"",""Maison du Bonheur"",""1955-02-14""
""Françoise Durand"",""Résidence des Lilas"",""1948-09-30""";

            try
            {
                File.WriteAllText(filePath, exampleCSV);
                Debug.Log($"✅ Template CSV créé: {filePath}");
                Debug.Log("Modifiez ce fichier avec vos données et utilisez ImportPlayersFromCSV()");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur création template: {e.Message}");
            }
        }

        /// <summary>
        /// Détermine le groupe d'âge selon l'âge
        /// </summary>
        private static string DetermineAgeGroup(int age)
        {
            if (age < 18) return "Mineur";
            if (age < 30) return "Jeune Adulte";
            if (age < 50) return "Adulte";
            if (age < 65) return "Adulte Mature";
            if (age < 80) return "Senior";
            return "Senior Avancé";
        }

        /// <summary>
        /// Valide une liste de joueurs et affiche un rapport
        /// </summary>
        public static void ValidatePlayersData(List<PlayerData> players)
        {
            Debug.Log("=== VALIDATION DONNÉES JOUEURS ===");
            
            int withBirthDate = 0;
            int withoutBirthDate = 0;
            var ageGroups = new Dictionary<string, int>();

            foreach (var player in players)
            {
                if (!string.IsNullOrEmpty(player.BirthDate))
                {
                    withBirthDate++;
                    int age = player.CalculateAge();
                    string group = DetermineAgeGroup(age);
                    
                    if (!ageGroups.ContainsKey(group))
                        ageGroups[group] = 0;
                    ageGroups[group]++;
                }
                else
                {
                    withoutBirthDate++;
                }
            }

            Debug.Log($"Total joueurs: {players.Count}");
            Debug.Log($"Avec date de naissance: {withBirthDate}");
            Debug.Log($"Sans date de naissance: {withoutBirthDate}");
            
            Debug.Log("\nRépartition par âge:");
            foreach (var group in ageGroups)
            {
                Debug.Log($"  {group.Key}: {group.Value} joueurs");
            }
        }
    }
}
