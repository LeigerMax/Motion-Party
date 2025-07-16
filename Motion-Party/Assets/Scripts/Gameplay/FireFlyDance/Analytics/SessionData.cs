using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.FireFlyDance.Analytics
{
    /// <summary>
    /// Données de session de jeu contenant toutes les statistiques collectées
    /// </summary>
    [Serializable]
    public class SessionData
    {
        [Header("Session Info")]
        public string sessionId;
        public string playerName;
        public DateTime sessionStart;
        public DateTime sessionEnd;
        public float totalDuration;

        [Header("Player Demographics")]
        public DateTime playerBirthDate;
        public int playerAge;
        public string ageGroup; // "Jeune", "Adulte", "Senior", etc.

        [Header("Game Events")]
        public List<GameStatEvent> events;

        [Header("Basic Stats")]
        public int totalFireflies;
        public int capturedFireflies;
        public int missedFireflies;
        public int totalHandClosures;
        public int emptyHandClosures;

        [Header("Performance Metrics")]
        public float averageReactionTime;
        public float minReactionTime;
        public float maxReactionTime;
        public float averageCaptureTime;
        public int totalMovements;
        public int emptyMovements;

        [Header("Advanced Stats")]
        public float handMovementDistance;
        public List<float> fireflyLifetimes;
        public List<int> attemptsPerFirefly;
        public Dictionary<int, float> fireflyReactionTimes;

        public SessionData()
        {
            sessionId = System.Guid.NewGuid().ToString();
            events = new List<GameStatEvent>();
            fireflyLifetimes = new List<float>();
            attemptsPerFirefly = new List<int>();
            fireflyReactionTimes = new Dictionary<int, float>();
            sessionStart = DateTime.Now;
        }

        /// <summary>
        /// Calcule l'âge du joueur et détermine son groupe d'âge
        /// </summary>
        public void CalculatePlayerAge()
        {
            if (playerBirthDate != default(DateTime))
            {
                var today = DateTime.Now;
                playerAge = today.Year - playerBirthDate.Year;
                
                // Ajuster si l'anniversaire n'est pas encore passé cette année
                if (playerBirthDate.Date > today.AddYears(-playerAge))
                    playerAge--;

                // Déterminer le groupe d'âge
                ageGroup = DetermineAgeGroup(playerAge);
            }
            else
            {
                playerAge = 0;
                ageGroup = "Non spécifié";
            }
        }

        /// <summary>
        /// Détermine le groupe d'âge basé sur l'âge
        /// </summary>
        public string DetermineAgeGroup(int age)
        {
            if (age < 18) return "Mineur";
            if (age < 25) return "Jeune Adulte";
            if (age < 40) return "Adulte";
            if (age < 60) return "Adulte Mature";
            if (age < 75) return "Senior";
            return "Senior Avancé";
        }
        public void FinalizeSession()
        {
            sessionEnd = DateTime.Now;
            totalDuration = (float)(sessionEnd - sessionStart).TotalSeconds;
            CalculatePlayerAge(); // Calculer l'âge avant les autres stats
            CalculateAdvancedStats();
        }

        /// <summary>
        /// Calcule les statistiques avancées basées sur les événements
        /// </summary>
        private void CalculateAdvancedStats()
        {
            if (events.Count == 0) return;

            List<float> reactionTimes = new List<float>();
            List<float> captureTimes = new List<float>();
            Vector2 lastHandPosition = Vector2.zero;
            float totalDistance = 0f;
            int movementCount = 0;

            Dictionary<int, float> fireflySpawnTimes = new Dictionary<int, float>();
            Dictionary<int, int> fireflyAttempts = new Dictionary<int, int>();

            foreach (var evt in events)
            {
                switch (evt.eventType)
                {
                    case GameEventType.FireflySpawned:
                        if (evt.fireflyId >= 0)
                            fireflySpawnTimes[evt.fireflyId] = evt.timestamp;
                        break;

                    case GameEventType.FireflyCaptured:
                        if (evt.fireflyId >= 0 && fireflySpawnTimes.ContainsKey(evt.fireflyId))
                        {
                            float reactionTime = evt.timestamp - fireflySpawnTimes[evt.fireflyId];
                            reactionTimes.Add(reactionTime);
                            fireflyReactionTimes[evt.fireflyId] = reactionTime;
                            captureTimes.Add(evt.fireflyLifetime);
                        }
                        break;

                    case GameEventType.AttemptedCapture:
                        if (evt.fireflyId >= 0)
                        {
                            if (!fireflyAttempts.ContainsKey(evt.fireflyId))
                                fireflyAttempts[evt.fireflyId] = 0;
                            fireflyAttempts[evt.fireflyId]++;
                        }
                        break;

                    case GameEventType.HandMovement:
                        if (movementCount > 0)
                        {
                            totalDistance += Vector2.Distance(lastHandPosition, evt.handPosition);
                        }
                        lastHandPosition = evt.handPosition;
                        movementCount++;
                        break;
                }
            }

            // Calculer les moyennes
            if (reactionTimes.Count > 0)
            {
                reactionTimes.Sort();
                averageReactionTime = CalculateAverage(reactionTimes);
                minReactionTime = reactionTimes[0];
                maxReactionTime = reactionTimes[reactionTimes.Count - 1];
            }

            if (captureTimes.Count > 0)
            {
                averageCaptureTime = CalculateAverage(captureTimes);
            }

            handMovementDistance = totalDistance;
            totalMovements = movementCount;

            // Convertir les tentatives par luciole
            attemptsPerFirefly.Clear();
            foreach (var attempts in fireflyAttempts.Values)
            {
                attemptsPerFirefly.Add(attempts);
            }

            // Calculer les mouvements vides (estimation basée sur les échecs)
            emptyMovements = Mathf.Max(0, totalMovements - (capturedFireflies * 2)); // Estimation approximative
            emptyHandClosures = totalHandClosures - capturedFireflies;
        }

        /// <summary>
        /// Calcule la moyenne d'une liste de valeurs
        /// </summary>
        private float CalculateAverage(List<float> values)
        {
            if (values.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (float value in values)
                sum += value;
            
            return sum / values.Count;
        }

        /// <summary>
        /// Exporte les données en format JSON lisible
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// Crée un résumé textuel des statistiques principales
        /// </summary>
        public string GetSummary()
        {
            var summary = $"=== RÉSUMÉ SESSION {sessionId.Substring(0, 8)} ===\n";
            summary += $"Joueur: {playerName}\n";
            
            // Informations démographiques
            if (playerAge > 0)
            {
                summary += $"Âge: {playerAge} ans ({ageGroup})\n";
            }
            
            summary += $"Durée: {totalDuration:F1}s\n";
            summary += $"Lucioles: {capturedFireflies}/{totalFireflies} ({(totalFireflies > 0 ? (float)capturedFireflies/totalFireflies*100 : 0):F1}%)\n";
            summary += $"Temps réaction moyen: {averageReactionTime:F2}s\n";
            summary += $"Temps réaction min/max: {minReactionTime:F2}s / {maxReactionTime:F2}s\n";
            summary += $"Clics vides: {emptyHandClosures}\n";
            summary += $"Distance main: {handMovementDistance:F1} unités\n";
            summary += $"Tentatives par luciole: {(attemptsPerFirefly.Count > 0 ? CalculateAverage(attemptsPerFirefly.ConvertAll(x => (float)x)):0):F1}\n";
            
            // Analyse spécifique à l'âge
            if (playerAge > 0)
            {
                summary += GetAgeSpecificAnalysis();
            }
            
            return summary;
        }

        /// <summary>
        /// Génère une analyse spécifique à l'âge du joueur
        /// </summary>
        private string GetAgeSpecificAnalysis()
        {
            var analysis = "\n=== ANALYSE PAR ÂGE ===\n";
            
            // Références selon l'âge pour les temps de réaction
            float expectedReactionTime = GetExpectedReactionTimeForAge(playerAge);
            string reactionComparison = averageReactionTime <= expectedReactionTime ? "Excellent" : 
                                      averageReactionTime <= expectedReactionTime * 1.2f ? "Bon" :
                                      averageReactionTime <= expectedReactionTime * 1.5f ? "Moyen" : "À améliorer";
            
            analysis += $"Temps réaction attendu pour {playerAge} ans: {expectedReactionTime:F2}s\n";
            analysis += $"Performance: {reactionComparison}\n";
            
            // Recommandations selon le groupe d'âge
            analysis += GetAgeGroupRecommendations();
            
            return analysis;
        }

        /// <summary>
        /// Obtient le temps de réaction attendu selon l'âge
        /// </summary>
        private float GetExpectedReactionTimeForAge(int age)
        {
            // Temps de réaction moyens basés sur la recherche scientifique
            if (age < 25) return 1.8f;  // Jeunes adultes
            if (age < 40) return 2.0f;  // Adultes
            if (age < 60) return 2.3f;  // Adultes matures
            if (age < 75) return 2.8f;  // Seniors
            return 3.2f;                // Seniors avancés
        }

        /// <summary>
        /// Génère des recommandations spécifiques au groupe d'âge
        /// </summary>
        private string GetAgeGroupRecommendations()
        {
            var recommendations = "Recommandations: ";
            
            switch (ageGroup)
            {
                case "Senior":
                case "Senior Avancé":
                    recommendations += "Excellente stimulation cognitive ! ";
                    if (averageReactionTime > 3.0f)
                        recommendations += "Continuez à pratiquer pour maintenir les réflexes.";
                    else
                        recommendations += "Très bons réflexes pour votre âge !";
                    break;
                    
                case "Adulte Mature":
                    recommendations += "Bon équilibre vitesse/précision. ";
                    if (emptyHandClosures > capturedFireflies)
                        recommendations += "Prenez plus de temps pour viser.";
                    break;
                    
                case "Adulte":
                case "Jeune Adulte":
                    recommendations += "Potentiel pour des temps plus rapides. ";
                    if (averageReactionTime > 2.5f)
                        recommendations += "Concentration et entraînement peuvent améliorer vos temps.";
                    break;
                    
                default:
                    recommendations += "Continuez à pratiquer pour améliorer vos performances !";
                    break;
            }
            
            return recommendations + "\n";
        }
    }
}
