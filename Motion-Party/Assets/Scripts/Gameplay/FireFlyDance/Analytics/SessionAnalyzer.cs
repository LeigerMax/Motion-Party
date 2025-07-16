using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Gameplay.FireFlyDance.Analytics
{
    /// <summary>
    /// Utilitaire pour analyser les fichiers de sessions sauvegardées
    /// Permet de charger et analyser les données de performance
    /// </summary>
    public static class SessionAnalyzer
    {
        /// <summary>
        /// Charge et analyse une session depuis un fichier JSON
        /// </summary>
        public static SessionData LoadSessionFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Fichier non trouvé: {filePath}");
                return null;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                SessionData session = JsonUtility.FromJson<SessionData>(jsonContent);
                return session;
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors du chargement: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Trouve tous les fichiers de session dans le dossier par défaut
        /// </summary>
        public static List<string> FindAllSessionFiles()
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, "GameAnalytics");
            List<string> files = new List<string>();

            if (!Directory.Exists(saveDirectory))
            {
                Debug.LogWarning($"Dossier d'analytics non trouvé: {saveDirectory}");
                return files;
            }

            string[] jsonFiles = Directory.GetFiles(saveDirectory, "*.json");
            files.AddRange(jsonFiles);

            return files;
        }

        /// <summary>
        /// Analyse comparative de plusieurs sessions
        /// </summary>
        public static string CompareMultipleSessions(List<SessionData> sessions)
        {
            if (sessions == null || sessions.Count == 0)
                return "Aucune session à comparer";

            var report = "=== ANALYSE COMPARATIVE ===\n\n";

            // Stats générales
            float avgTotalDuration = 0f;
            float avgReactionTime = 0f;
            float avgSuccessRate = 0f;
            int totalSessions = sessions.Count;

            foreach (var session in sessions)
            {
                avgTotalDuration += session.totalDuration;
                avgReactionTime += session.averageReactionTime;
                if (session.totalFireflies > 0)
                    avgSuccessRate += (float)session.capturedFireflies / session.totalFireflies;
            }

            avgTotalDuration /= totalSessions;
            avgReactionTime /= totalSessions;
            avgSuccessRate /= totalSessions;

            report += $"📊 MOYENNES GÉNÉRALES ({totalSessions} sessions)\n";
            report += $"Durée moyenne: {avgTotalDuration:F1}s\n";
            report += $"Temps réaction moyen: {avgReactionTime:F2}s\n";
            report += $"Taux succès moyen: {avgSuccessRate * 100:F1}%\n\n";

            // Évolution dans le temps
            sessions.Sort((a, b) => a.sessionStart.CompareTo(b.sessionStart));
            
            report += "📈 ÉVOLUTION TEMPORELLE\n";
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                float successRate = session.totalFireflies > 0 ? 
                    (float)session.capturedFireflies / session.totalFireflies * 100 : 0;
                
                report += $"Session {i + 1}: {session.sessionStart:yyyy-MM-dd HH:mm} - ";
                report += $"Succès: {successRate:F1}% - Réaction: {session.averageReactionTime:F2}s\n";
            }

            return report;
        }

        /// <summary>
        /// Génère un rapport détaillé pour une session
        /// </summary>
        public static string GenerateDetailedReport(SessionData session)
        {
            if (session == null) return "Session invalide";

            var report = $"=== RAPPORT DÉTAILLÉ ===\n";
            report += $"Session: {session.sessionId.Substring(0, 8)}\n";
            report += $"Joueur: {session.playerName}\n";
            
            // Informations démographiques
            if (session.playerAge > 0)
            {
                report += $"Âge: {session.playerAge} ans ({session.ageGroup})\n";
                if (session.playerBirthDate != default(DateTime))
                {
                    report += $"Date naissance: {session.playerBirthDate:yyyy-MM-dd}\n";
                }
            }
            
            report += $"Date session: {session.sessionStart:yyyy-MM-dd HH:mm:ss}\n";
            report += $"Durée: {session.totalDuration:F1}s\n\n";

            // Performance générale
            float successRate = session.totalFireflies > 0 ? 
                (float)session.capturedFireflies / session.totalFireflies * 100 : 0;

            report += "🎯 PERFORMANCE GÉNÉRALE\n";
            report += $"Lucioles capturées: {session.capturedFireflies}/{session.totalFireflies} ({successRate:F1}%)\n";
            report += $"Lucioles manquées: {session.missedFireflies}\n";
            report += $"Temps réaction moyen: {session.averageReactionTime:F2}s\n";
            report += $"Temps réaction min/max: {session.minReactionTime:F2}s / {session.maxReactionTime:F2}s\n";
            
            // Analyse comparative par âge
            if (session.playerAge > 0)
            {
                report += GetAgePerformanceAnalysis(session);
            }
            report += "\n";

            // Analyse des mouvements
            report += "✋ ANALYSE DES MOUVEMENTS\n";
            report += $"Total fermetures main: {session.totalHandClosures}\n";
            report += $"Fermetures vides: {session.emptyHandClosures}\n";
            report += $"Précision: {(session.totalHandClosures > 0 ? (float)(session.totalHandClosures - session.emptyHandClosures) / session.totalHandClosures * 100 : 0):F1}%\n";
            report += $"Distance parcourue: {session.handMovementDistance:F1} unités\n";
            report += $"Mouvements totaux: {session.totalMovements}\n";
            report += $"Mouvements vides estimés: {session.emptyMovements}\n\n";

            // Analyse des tentatives
            if (session.attemptsPerFirefly.Count > 0)
            {
                float avgAttempts = 0f;
                foreach (int attempts in session.attemptsPerFirefly)
                    avgAttempts += attempts;
                avgAttempts /= session.attemptsPerFirefly.Count;

                report += "🔄 ANALYSE DES TENTATIVES\n";
                report += $"Tentatives moyennes par luciole: {avgAttempts:F1}\n";
                report += $"Lucioles capturées du premier coup: {CountFirstTryCaptures(session)}\n";
            }

            // Analyse temporelle
            if (session.events.Count > 0)
            {
                report += "\n⏰ ANALYSE TEMPORELLE\n";
                report += $"Événements enregistrés: {session.events.Count}\n";
                report += AnalyzeEventDistribution(session);
            }

            return report;
        }

        /// <summary>
        /// Compte les captures réussies au premier essai
        /// </summary>
        private static int CountFirstTryCaptures(SessionData session)
        {
            int firstTryCount = 0;
            foreach (int attempts in session.attemptsPerFirefly)
            {
                if (attempts <= 1) firstTryCount++;
            }
            return firstTryCount;
        }

        /// <summary>
        /// Analyse la distribution des événements dans le temps
        /// </summary>
        private static string AnalyzeEventDistribution(SessionData session)
        {
            if (session.events.Count == 0) return "Aucun événement";

            var distribution = "";
            
            // Découper la session en quartiles
            float sessionDuration = session.totalDuration;
            int[] quartileEvents = new int[4];
            
            foreach (var evt in session.events)
            {
                float progress = evt.timestamp / sessionDuration;
                int quartile = Mathf.Clamp((int)(progress * 4), 0, 3);
                quartileEvents[quartile]++;
            }

            distribution += "Distribution par quartile:\n";
            for (int i = 0; i < 4; i++)
            {
                float percentage = (float)quartileEvents[i] / session.events.Count * 100;
                distribution += $"  Q{i + 1}: {quartileEvents[i]} événements ({percentage:F1}%)\n";
            }

            return distribution;
        }

        /// <summary>
        /// Exporte les données en format CSV pour Excel
        /// </summary>
        public static string ExportToCSV(List<SessionData> sessions)
        {
            if (sessions == null || sessions.Count == 0)
                return "Aucune donnée à exporter";

            var csv = "SessionID,PlayerName,Age,AgeGroup,BirthDate,Date,Duration,TotalFireflies,CapturedFireflies,MissedFireflies,SuccessRate,AvgReactionTime,MinReactionTime,MaxReactionTime,HandClosures,EmptyClosures,Precision,MovementDistance\n";

            foreach (var session in sessions)
            {
                float successRate = session.totalFireflies > 0 ? 
                    (float)session.capturedFireflies / session.totalFireflies * 100 : 0;
                
                float precision = session.totalHandClosures > 0 ? 
                    (float)(session.totalHandClosures - session.emptyHandClosures) / session.totalHandClosures * 100 : 0;

                csv += $"{session.sessionId},{session.playerName},{session.playerAge},{session.ageGroup},";
                csv += $"{session.playerBirthDate},{session.sessionStart:yyyy-MM-dd HH:mm:ss},";
                csv += $"{session.totalDuration:F1},{session.totalFireflies},{session.capturedFireflies},{session.missedFireflies},";
                csv += $"{successRate:F1},{session.averageReactionTime:F2},{session.minReactionTime:F2},{session.maxReactionTime:F2},";
                csv += $"{session.totalHandClosures},{session.emptyHandClosures},{precision:F1},{session.handMovementDistance:F1}\n";
            }

            return csv;
        }

        /// <summary>
        /// Sauvegarde un rapport d'analyse dans un fichier
        /// </summary>
        public static void SaveReportToFile(string report, string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"AnalyticsReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            }

            string saveDirectory = Path.Combine(Application.persistentDataPath, "GameAnalytics");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            string fullPath = Path.Combine(saveDirectory, filename);

            try
            {
                File.WriteAllText(fullPath, report);
                Debug.Log($"Rapport sauvegardé: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur sauvegarde rapport: {e.Message}");
            }
        }

        /// <summary>
        /// Génère une analyse spécifique à l'âge du joueur
        /// </summary>
        private static string GetAgePerformanceAnalysis(SessionData session)
        {
            var analysis = "\n👤 ANALYSE DÉMOGRAPHIQUE\n";
            
            // Comparaison avec les normes d'âge
            float expectedReactionTime = GetExpectedReactionTimeForAge(session.playerAge);
            string performanceLevel = GetPerformanceLevel(session.averageReactionTime, expectedReactionTime);
            
            analysis += $"Temps réaction attendu ({session.ageGroup}): {expectedReactionTime:F2}s\n";
            analysis += $"Performance relative: {performanceLevel}\n";
            analysis += $"Écart: {(session.averageReactionTime - expectedReactionTime):+0.00}s\n";
            
            // Recommandations spécifiques
            analysis += GetAgeSpecificRecommendations(session);
            
            return analysis;
        }

        /// <summary>
        /// Obtient le temps de réaction attendu selon l'âge
        /// </summary>
        private static float GetExpectedReactionTimeForAge(int age)
        {
            // Temps de réaction moyens basés sur la recherche scientifique
            if (age < 18) return 2.2f;  // Mineurs
            if (age < 30) return 1.8f;  // Jeunes adultes
            if (age < 50) return 2.0f;  // Adultes
            if (age < 65) return 2.3f;  // Adultes matures
            if (age < 80) return 2.8f;  // Seniors
            return 3.2f;                // Seniors avancés
        }

        /// <summary>
        /// Détermine le niveau de performance par rapport à l'âge
        /// </summary>
        private static string GetPerformanceLevel(float actualTime, float expectedTime)
        {
            float ratio = actualTime / expectedTime;
            
            if (ratio <= 0.8f) return "Excellent (bien au-dessus de la moyenne)";
            if (ratio <= 1.0f) return "Très bon (au-dessus de la moyenne)";
            if (ratio <= 1.2f) return "Bon (dans la moyenne)";
            if (ratio <= 1.5f) return "Moyen (légèrement en-dessous)";
            return "À améliorer (en-dessous de la moyenne)";
        }

        /// <summary>
        /// Génère des recommandations spécifiques au groupe d'âge
        /// </summary>
        private static string GetAgeSpecificRecommendations(SessionData session)
        {
            var recommendations = "Recommandations: ";
            
            switch (session.ageGroup)
            {
                case "Senior":
                case "Senior Avancé":
                    recommendations += "Excellente activité de stimulation cognitive ! ";
                    if (session.averageReactionTime > 3.5f)
                        recommendations += "Continuez l'entraînement pour maintenir vos réflexes.";
                    else if (session.averageReactionTime < 2.5f)
                        recommendations += "Réflexes exceptionnels pour votre groupe d'âge !";
                    else
                        recommendations += "Performance très satisfaisante.";
                    
                    if (session.emptyHandClosures > session.capturedFireflies)
                        recommendations += " Privilégiez la précision à la vitesse.";
                    break;
                    
                case "Adulte Mature":
                    recommendations += "Bon équilibre expérience/agilité. ";
                    if (session.averageReactionTime > 2.8f)
                        recommendations += "Entraînement régulier recommandé.";
                    else
                        recommendations += "Très bonne performance !";
                    break;
                    
                case "Adulte":
                    recommendations += "Dans la force de l'âge ! ";
                    if (session.averageReactionTime > 2.5f)
                        recommendations += "Potentiel d'amélioration important.";
                    else
                        recommendations += "Excellents réflexes.";
                    break;
                    
                case "Jeune Adulte":
                    recommendations += "Réflexes à leur apogée ! ";
                    if (session.averageReactionTime > 2.2f)
                        recommendations += "Concentration et pratique peuvent optimiser vos temps.";
                    else
                        recommendations += "Performance de haut niveau !";
                    break;
                    
                case "Mineur":
                    recommendations += "Développement des capacités motrices en cours. ";
                    recommendations += "Continuez à jouer pour améliorer coordination et réflexes !";
                    break;
                    
                default:
                    recommendations += "Continuez l'entraînement pour progresser !";
                    break;
            }
            
            return recommendations + "\n";
        }

        /// <summary>
        /// Analyse comparative de sessions par groupe d'âge
        /// </summary>
        public static string CompareSessionsByAgeGroup(List<SessionData> sessions)
        {
            if (sessions == null || sessions.Count == 0)
                return "Aucune session à analyser";

            var ageGroups = new Dictionary<string, List<SessionData>>();
            
            // Grouper les sessions par âge
            foreach (var session in sessions)
            {
                if (session.playerAge > 0)
                {
                    if (!ageGroups.ContainsKey(session.ageGroup))
                        ageGroups[session.ageGroup] = new List<SessionData>();
                    ageGroups[session.ageGroup].Add(session);
                }
            }

            var report = "=== ANALYSE PAR GROUPE D'ÂGE ===\n\n";

            foreach (var group in ageGroups)
            {
                report += $"📊 {group.Key.ToUpper()} ({group.Value.Count} sessions)\n";
                
                float avgReaction = 0f;
                float avgSuccess = 0f;
                
                foreach (var session in group.Value)
                {
                    avgReaction += session.averageReactionTime;
                    if (session.totalFireflies > 0)
                        avgSuccess += (float)session.capturedFireflies / session.totalFireflies;
                }
                
                avgReaction /= group.Value.Count;
                avgSuccess /= group.Value.Count;
                
                report += $"   Temps réaction moyen: {avgReaction:F2}s\n";
                report += $"   Taux succès moyen: {avgSuccess * 100:F1}%\n";
                report += $"   Performance: {GetPerformanceLevel(avgReaction, GetExpectedReactionTimeForAge(group.Value[0].playerAge))}\n\n";
            }

            return report;
        }

    }
}
