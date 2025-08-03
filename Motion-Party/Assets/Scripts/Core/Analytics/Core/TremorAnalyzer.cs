using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Analytics.Interfaces;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Analyseur de tremblements basé sur les variations de position des mains
    /// Utile pour détecter des indicateurs moteurs chez les résidents de maisons de repos
    /// </summary>
    [Serializable]
    public class TremorAnalyzer
    {
        [Header("Configuration Tremor Analysis")]
        [SerializeField] private int bufferSize = 30; // Nombre de positions à analyser (environ 1 seconde à 30 FPS)
        [SerializeField] private float tremorThreshold = 0.02f; // Seuil de détection des tremblements (en unités Unity)
        [SerializeField] private float minTremorFrequency = 3f; // Fréquence minimale pour considérer un tremblement (Hz)
        [SerializeField] private float maxTremorFrequency = 12f; // Fréquence maximale pour considérer un tremblement (Hz)
        
        // Buffers pour stocker l'historique des positions
        private Queue<Vector2> positionHistory;
        private Queue<float> timeHistory;
        private Queue<float> velocityHistory;
        
        // Métriques calculées
        private float currentTremorIntensity;
        private float currentTremorFrequency;
        private int tremorEpisodesCount;
        private float totalTremorTime;
        private float lastTremorDetectionTime;
        
        // Configuration temporelle
        private float analysisStartTime;
        private bool isAnalyzing;

        public TremorAnalyzer()
        {
            positionHistory = new Queue<Vector2>();
            timeHistory = new Queue<float>();
            velocityHistory = new Queue<float>();
            Reset();
        }

        /// <summary>
        /// Démarre l'analyse des tremblements
        /// </summary>
        public void StartAnalysis()
        {
            Reset();
            isAnalyzing = true;
            analysisStartTime = Time.time;
        }

        /// <summary>
        /// Arrête l'analyse des tremblements
        /// </summary>
        public void StopAnalysis()
        {
            isAnalyzing = false;
        }

        /// <summary>
        /// Ajoute une nouvelle position de main pour l'analyse
        /// </summary>
        public void AddHandPosition(Vector2 position, bool isHandDetected)
        {
            if (!isAnalyzing || !isHandDetected) return;

            float currentTime = Time.time;
            
            // Ajouter la position actuelle
            positionHistory.Enqueue(position);
            timeHistory.Enqueue(currentTime);
            
            // Calculer la vitesse si on a assez de données
            if (positionHistory.Count > 1)
            {
                Vector2 previousPosition = GetPreviousPosition(1);
                float previousTime = GetPreviousTime(1);
                float velocity = Vector2.Distance(position, previousPosition) / (currentTime - previousTime);
                velocityHistory.Enqueue(velocity);
            }

            // Maintenir la taille du buffer
            if (positionHistory.Count > bufferSize)
            {
                positionHistory.Dequeue();
                timeHistory.Dequeue();
            }
            
            if (velocityHistory.Count > bufferSize - 1)
            {
                velocityHistory.Dequeue();
            }

            // Analyser les tremblements si on a assez de données
            if (positionHistory.Count >= bufferSize)
            {
                AnalyzeTremor();
            }
        }

        /// <summary>
        /// Analyse les tremblements basés sur les données collectées
        /// </summary>
        private void AnalyzeTremor()
        {
            var positions = positionHistory.ToArray();
            var times = timeHistory.ToArray();
            var velocities = velocityHistory.ToArray();

            // Calculer la variance des positions (indicateur de stabilité)
            Vector2 meanPosition = CalculateMeanPosition(positions);
            float variance = CalculatePositionVariance(positions, meanPosition);
            
            // Calculer la variation de vitesse (indicateur de tremblements)
            float velocityVariance = CalculateVelocityVariance(velocities);
            
            // Calculer la fréquence dominante des mouvements
            float dominantFrequency = CalculateDominantFrequency(positions, times);
            
            // Déterminer l'intensité du tremblement
            currentTremorIntensity = Mathf.Sqrt(variance) * 100f; // Convertir en pourcentage
            currentTremorFrequency = dominantFrequency;
            
            // Détecter un épisode de tremblement
            bool isTremorDetected = variance > tremorThreshold * tremorThreshold && 
                                   dominantFrequency >= minTremorFrequency && 
                                   dominantFrequency <= maxTremorFrequency &&
                                   velocityVariance > 0.001f;

            if (isTremorDetected)
            {
                if (Time.time - lastTremorDetectionTime > 1f) // Nouvel épisode si gap > 1 seconde
                {
                    tremorEpisodesCount++;
                }
                
                totalTremorTime += Time.deltaTime;
                lastTremorDetectionTime = Time.time;
            }
        }

        /// <summary>
        /// Calcule la position moyenne
        /// </summary>
        private Vector2 CalculateMeanPosition(Vector2[] positions)
        {
            Vector2 sum = Vector2.zero;
            foreach (var pos in positions)
            {
                sum += pos;
            }
            return sum / positions.Length;
        }

        /// <summary>
        /// Calcule la variance des positions
        /// </summary>
        private float CalculatePositionVariance(Vector2[] positions, Vector2 mean)
        {
            float variance = 0f;
            foreach (var pos in positions)
            {
                variance += Vector2.SqrMagnitude(pos - mean);
            }
            return variance / positions.Length;
        }

        /// <summary>
        /// Calcule la variance de la vitesse
        /// </summary>
        private float CalculateVelocityVariance(float[] velocities)
        {
            if (velocities.Length == 0) return 0f;
            
            float mean = 0f;
            foreach (var vel in velocities)
            {
                mean += vel;
            }
            mean /= velocities.Length;
            
            float variance = 0f;
            foreach (var vel in velocities)
            {
                variance += (vel - mean) * (vel - mean);
            }
            return variance / velocities.Length;
        }

        /// <summary>
        /// Calcule la fréquence dominante des mouvements (analyse simplifiée)
        /// </summary>
        private float CalculateDominantFrequency(Vector2[] positions, float[] times)
        {
            if (positions.Length < 3) return 0f;

            // Compter les pics de changement de direction
            int directionChanges = 0;
            Vector2 previousDirection = Vector2.zero;
            
            for (int i = 1; i < positions.Length - 1; i++)
            {
                Vector2 currentDirection = (positions[i + 1] - positions[i]).normalized;
                
                if (previousDirection != Vector2.zero)
                {
                    float dot = Vector2.Dot(currentDirection, previousDirection);
                    if (dot < 0.5f) // Changement de direction significatif
                    {
                        directionChanges++;
                    }
                }
                
                previousDirection = currentDirection;
            }
            
            // Calculer la fréquence basée sur les changements de direction
            float timeSpan = times[times.Length - 1] - times[0];
            return directionChanges / (2f * timeSpan); // Divisé par 2 car un cycle = 2 changements
        }

        /// <summary>
        /// Obtient une position précédente dans l'historique
        /// </summary>
        private Vector2 GetPreviousPosition(int stepsBack)
        {
            var positions = positionHistory.ToArray();
            if (positions.Length <= stepsBack) return Vector2.zero;
            return positions[positions.Length - 1 - stepsBack];
        }

        /// <summary>
        /// Obtient un temps précédent dans l'historique
        /// </summary>
        private float GetPreviousTime(int stepsBack)
        {
            var times = timeHistory.ToArray();
            if (times.Length <= stepsBack) return 0f;
            return times[times.Length - 1 - stepsBack];
        }

        /// <summary>
        /// Remet à zéro toutes les métriques
        /// </summary>
        public void Reset()
        {
            positionHistory.Clear();
            timeHistory.Clear();
            velocityHistory.Clear();
            
            currentTremorIntensity = 0f;
            currentTremorFrequency = 0f;
            tremorEpisodesCount = 0;
            totalTremorTime = 0f;
            lastTremorDetectionTime = 0f;
        }

        /// <summary>
        /// Obtient les métriques de tremblement calculées
        /// </summary>
        public TremorMetrics GetTremorMetrics()
        {
            float analysisTime = isAnalyzing ? Time.time - analysisStartTime : 0f;
            float tremorPercentage = analysisTime > 0f ? (totalTremorTime / analysisTime) * 100f : 0f;
            
            return new TremorMetrics
            {
                tremorIntensity = currentTremorIntensity,
                tremorFrequency = currentTremorFrequency,
                tremorEpisodesCount = tremorEpisodesCount,
                tremorTimePercentage = tremorPercentage,
                averageTremorDuration = tremorEpisodesCount > 0 ? totalTremorTime / tremorEpisodesCount : 0f,
                isCurrentlyTremoring = Time.time - lastTremorDetectionTime < 0.5f
            };
        }

        /// <summary>
        /// Met à jour les métriques du joueur avec les données de tremblement
        /// </summary>
        public void UpdatePlayerMetrics(IPlayerMetrics playerMetrics)
        {
            var metrics = GetTremorMetrics();
            
            playerMetrics.SetMetric("tremor_intensity", metrics.tremorIntensity);
            playerMetrics.SetMetric("tremor_frequency", metrics.tremorFrequency);
            playerMetrics.SetMetric("tremor_episodes_count", metrics.tremorEpisodesCount);
            playerMetrics.SetMetric("tremor_time_percentage", metrics.tremorTimePercentage);
            playerMetrics.SetMetric("average_tremor_duration", metrics.averageTremorDuration);
            playerMetrics.SetMetric("is_tremoring", metrics.isCurrentlyTremoring ? 1f : 0f);
        }
    }

    /// <summary>
    /// Structure contenant toutes les métriques de tremblement
    /// </summary>
    [Serializable]
    public struct TremorMetrics
    {
        public float tremorIntensity;           // Intensité moyenne des tremblements (0-100)
        public float tremorFrequency;           // Fréquence dominante des tremblements (Hz)
        public int tremorEpisodesCount;         // Nombre d'épisodes de tremblement détectés
        public float tremorTimePercentage;     // Pourcentage du temps avec tremblements
        public float averageTremorDuration;    // Durée moyenne d'un épisode de tremblement
        public bool isCurrentlyTremoring;      // Tremblement actuellement détecté
    }
}
