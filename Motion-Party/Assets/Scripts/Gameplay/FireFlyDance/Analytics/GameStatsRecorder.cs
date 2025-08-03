using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Analytics
{
    /// <summary>
    /// Système d'enregistrement et d'analyse des statistiques de performance du jeu de lucioles
    /// S'intègre aux événements existants pour collecter les données automatiquement
    /// </summary>
    public class GameStatsRecorder : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private bool enableDetailedLogging = false;
        [SerializeField] private bool autoSaveOnGameEnd = true;
        [SerializeField] private string saveDirectory = "GameAnalytics";

        [Header("Tracking Settings")]
        [SerializeField] private float handMovementThreshold = 0.1f; // Distance minimum pour compter un mouvement
        [SerializeField] private float captureDistanceThreshold = 2f; // Distance max pour considérer une tentative de capture
        [SerializeField] private bool trackHandMovements = true;

        [Header("Runtime Info")]
        [SerializeField] private SessionData currentSession;
        [SerializeField] private bool isRecording = false;
        [SerializeField] private int eventsRecorded = 0;

        // État interne
        private Dictionary<int, float> fireflySpawnTimes = new Dictionary<int, float>();
        private Dictionary<int, int> fireflyAttemptCounts = new Dictionary<int, int>();
        private Vector2 lastHandPosition = Vector2.zero;
        private float lastHandMoveTime = 0f;
        private bool wasHandClosed = false;
        private float gameStartTime = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            if (!isEnabled) return;
            
            FireflyDanceLogger.Log("GameStatsRecorder - Initialisation");
            
            // Créer le dossier de sauvegarde si nécessaire
            CreateSaveDirectory();
        }

        private void OnEnable()
        {
            if (!isEnabled) return;
            
            SubscribeToEvents();
            FireflyDanceLogger.Log("GameStatsRecorder - Événements souscrits");
        }

        private void OnDisable()
        {
            if (!isEnabled) return;
            
            UnsubscribeFromEvents();
            
            // Sauvegarder la session en cours si elle existe
            if (isRecording && currentSession != null)
            {
                EndRecording();
            }
            
            FireflyDanceLogger.Log("GameStatsRecorder - Désactivé");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            // Événements de jeu
            FireflyDanceEvents.OnGameStarted += OnGameStarted;
            FireflyDanceEvents.OnGameEnded += OnGameEnded;

            // Événements de lucioles
            FireflyDanceEvents.OnFireflySpawned += OnFireflySpawned;
            FireflyDanceEvents.OnFireflyCaptured += OnFireflyCaptured;
            FireflyDanceEvents.OnFireflyExpired += OnFireflyExpired;

            // Événements de main
            FireflyDanceEvents.OnHandPositionChanged += OnHandPositionChanged;
            FireflyDanceEvents.OnHandStateChanged += OnHandStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            // Événements de jeu
            FireflyDanceEvents.OnGameStarted -= OnGameStarted;
            FireflyDanceEvents.OnGameEnded -= OnGameEnded;

            // Événements de lucioles
            FireflyDanceEvents.OnFireflySpawned -= OnFireflySpawned;
            FireflyDanceEvents.OnFireflyCaptured -= OnFireflyCaptured;
            FireflyDanceEvents.OnFireflyExpired -= OnFireflyExpired;

            // Événements de main
            FireflyDanceEvents.OnHandPositionChanged -= OnHandPositionChanged;
            FireflyDanceEvents.OnHandStateChanged -= OnHandStateChanged;
        }

        #endregion

        #region Event Handlers

        private void OnGameStarted()
        {
            if (!isEnabled) return;
            
            StartRecording();
            FireflyDanceLogger.Log("GameStatsRecorder - Enregistrement démarré");
        }

        private void OnGameEnded()
        {
            if (!isEnabled || !isRecording) return;
            
            EndRecording();
            FireflyDanceLogger.Log("GameStatsRecorder - Enregistrement terminé");
        }

        private void OnFireflySpawned(FireflyController firefly)
        {
            if (!isEnabled || !isRecording || firefly == null) return;

            int fireflyId = firefly.GetInstanceID();
            float currentTime = GetGameTime();
            Vector2 position = firefly.transform.position;

            // Enregistrer le temps d'apparition
            fireflySpawnTimes[fireflyId] = currentTime;
            fireflyAttemptCounts[fireflyId] = 0;

            // Créer l'événement avec le type de libellule
            var statEvent = GameStatEvent.CreateFireflyEvent(
                GameEventType.FireflySpawned, 
                currentTime, 
                position, 
                fireflyId,
                0f,
                false,
                firefly.Type.ToString(), // Ajouter le type
                firefly.ScoreValue       // Ajouter la valeur de score
            );

            RecordEvent(statEvent);
            currentSession.totalFireflies++;

            if (enableDetailedLogging)
                FireflyDanceLogger.Log($"Luciole {firefly.Type} apparue: ID={fireflyId}, Pos={position}");
        }

        private void OnFireflyCaptured(FireflyController firefly, int score)
        {
            if (!isEnabled || !isRecording || firefly == null) return;

            int fireflyId = firefly.GetInstanceID();
            float currentTime = GetGameTime();
            Vector2 position = firefly.transform.position;

            // Calculer le temps de réaction
            float reactionTime = 0f;
            if (fireflySpawnTimes.ContainsKey(fireflyId))
            {
                reactionTime = currentTime - fireflySpawnTimes[fireflyId];
            }

            // Créer l'événement avec type et score
            var statEvent = GameStatEvent.CreateFireflyEvent(
                GameEventType.FireflyCaptured,
                currentTime,
                position,
                fireflyId,
                reactionTime,
                true,
                firefly.Type.ToString(), // Ajouter le type
                score                    // Score réel attribué
            );

            RecordEvent(statEvent);
            currentSession.capturedFireflies++;

            // Nettoyer les données temporaires
            fireflySpawnTimes.Remove(fireflyId);

            if (enableDetailedLogging)
                FireflyDanceLogger.Log($"Luciole {firefly.Type} capturée: ID={fireflyId}, Temps={reactionTime:F2}s, Score={score}");
        }

        private void OnFireflyExpired(FireflyController firefly)
        {
            if (!isEnabled || !isRecording || firefly == null) return;

            int fireflyId = firefly.GetInstanceID();
            float currentTime = GetGameTime();
            Vector2 position = firefly.transform.position;

            // Calculer la durée de vie
            float lifetime = 0f;
            if (fireflySpawnTimes.ContainsKey(fireflyId))
            {
                lifetime = currentTime - fireflySpawnTimes[fireflyId];
            }

            // Créer l'événement avec type
            var statEvent = GameStatEvent.CreateFireflyEvent(
                GameEventType.FireflyExpired,
                currentTime,
                position,
                fireflyId,
                lifetime,
                false,
                firefly.Type.ToString(), // Ajouter le type
                0                        // Pas de score pour les expirées
            );

            RecordEvent(statEvent);
            currentSession.missedFireflies++;

            // Nettoyer les données temporaires
            fireflySpawnTimes.Remove(fireflyId);

            if (enableDetailedLogging)
                FireflyDanceLogger.Log($"Luciole {firefly.Type} expirée: ID={fireflyId}, Durée={lifetime:F2}s");
        }

        private void OnHandPositionChanged(Vector2 newPosition)
        {
            if (!isEnabled || !isRecording || !trackHandMovements) return;

            float currentTime = GetGameTime();
            
            // Vérifier si le mouvement est significatif
            if (Vector2.Distance(lastHandPosition, newPosition) >= handMovementThreshold)
            {
                // Créer l'événement de mouvement
                var statEvent = GameStatEvent.CreateHandEvent(
                    GameEventType.HandMovement,
                    currentTime,
                    newPosition,
                    wasHandClosed,
                    0 // Les doigts ouverts peuvent être ajoutés si disponibles
                );

                RecordEvent(statEvent);
                
                lastHandPosition = newPosition;
                lastHandMoveTime = currentTime;

                // Vérifier s'il y a une tentative de capture à proximité d'une luciole
                CheckForCaptureAttempt(newPosition, currentTime);
            }
        }

        private void OnHandStateChanged(bool isClosed)
        {
            if (!isEnabled || !isRecording) return;

            float currentTime = GetGameTime();
            Vector2 handPosition = lastHandPosition;

            if (isClosed != wasHandClosed)
            {
                GameEventType eventType = isClosed ? GameEventType.HandClosed : GameEventType.HandOpened;
                
                var statEvent = GameStatEvent.CreateHandEvent(
                    eventType,
                    currentTime,
                    handPosition,
                    isClosed,
                    0
                );

                RecordEvent(statEvent);
                
                currentSession.totalHandClosures++;
                
                // Si la main se ferme, vérifier s'il y a une luciole à proximité
                if (isClosed)
                {
                    bool nearFirefly = CheckForNearbyFirefly(handPosition);
                    if (!nearFirefly)
                    {
                        currentSession.emptyHandClosures++;
                    }
                }

                wasHandClosed = isClosed;

                if (enableDetailedLogging)
                    FireflyDanceLogger.Log($"Main {(isClosed ? "fermée" : "ouverte")} à {handPosition}");
            }
        }

        #endregion

        #region Core Recording Methods

        /// <summary>
        /// Démarre l'enregistrement d'une nouvelle session
        /// </summary>
        public void StartRecording(string playerName = "Joueur Anonyme")
        {
            if (isRecording)
            {
                FireflyDanceLogger.LogWarning("Tentative de démarrage d'enregistrement alors qu'une session est déjà en cours");
                return;
            }

            currentSession = new SessionData();
            
            // Essayer d'obtenir les informations du joueur actuel depuis le système de joueurs
            var gamePlayerSelector = Systems.GamePlayerSelector.Instance;
            if (gamePlayerSelector != null && gamePlayerSelector.CurrentPlayer != null)
            {
                var currentPlayer = gamePlayerSelector.CurrentPlayer;
                currentSession.playerName = currentPlayer.Nickname;
                
                // Parser la date de naissance string en DateTime
                if (!string.IsNullOrEmpty(currentPlayer.BirthDate))
                {
                    if (DateTime.TryParseExact(currentPlayer.BirthDate, "yyyy-MM-dd", null, 
                        System.Globalization.DateTimeStyles.None, out DateTime parsedBirthDate))
                    {
                        currentSession.playerBirthDate = parsedBirthDate;
                    }
                }
                
                // Calculer l'âge et le groupe d'âge
                currentSession.CalculatePlayerAge();
                
                FireflyDanceLogger.Log($"Informations joueur récupérées: {currentPlayer.Nickname}, âge: {currentSession.playerAge}");
            }
            else
            {
                // Fallback si pas de système de joueur
                currentSession.playerName = playerName;
                FireflyDanceLogger.LogWarning("Aucun joueur actuel trouvé, utilisation du nom par défaut");
            }
            
            isRecording = true;
            eventsRecorded = 0;
            gameStartTime = Time.time;

            // Réinitialiser les données temporaires
            fireflySpawnTimes.Clear();
            fireflyAttemptCounts.Clear();
            lastHandPosition = Vector2.zero;
            wasHandClosed = false;

            FireflyDanceLogger.Log($"Session d'analyse démarrée pour {currentSession.playerName}");
        }

        /// <summary>
        /// Termine l'enregistrement et finalise la session
        /// </summary>
        public void EndRecording()
        {
            if (!isRecording)
            {
                FireflyDanceLogger.LogWarning("Tentative d'arrêt d'enregistrement sans session active");
                return;
            }

            currentSession.FinalizeSession();
            isRecording = false;

            FireflyDanceLogger.Log($"Session terminée: {eventsRecorded} événements enregistrés");

            if (autoSaveOnGameEnd)
            {
                SaveCurrentSession();
            }

            // Afficher le résumé
            if (enableDetailedLogging)
            {
                Debug.Log(currentSession.GetSummary());
            }
        }

        /// <summary>
        /// Enregistre un événement dans la session courante
        /// </summary>
        private void RecordEvent(GameStatEvent statEvent)
        {
            if (!isRecording || currentSession == null) return;

            currentSession.events.Add(statEvent);
            eventsRecorded++;

            if (enableDetailedLogging)
            {
                FireflyDanceLogger.LogVerbose($"Événement enregistré: {statEvent.eventType} à {statEvent.timestamp:F2}s");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Obtient le temps de jeu relatif depuis le début de la session
        /// </summary>
        private float GetGameTime()
        {
            return Time.time - gameStartTime;
        }

        /// <summary>
        /// Vérifie s'il y a une tentative de capture près d'une luciole
        /// </summary>
        private void CheckForCaptureAttempt(Vector2 handPosition, float currentTime)
        {
            // Trouver toutes les lucioles actives dans la scène
            var fireflies = FindObjectsByType<FireflyController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            foreach (var firefly in fireflies)
            {
                if (firefly == null) continue;
                
                Vector2 fireflyPos = firefly.transform.position;
                float distance = Vector2.Distance(handPosition, fireflyPos);
                
                if (distance <= captureDistanceThreshold)
                {
                    int fireflyId = firefly.GetInstanceID();
                    
                    // Incrémenter le compteur de tentatives
                    if (!fireflyAttemptCounts.ContainsKey(fireflyId))
                        fireflyAttemptCounts[fireflyId] = 0;
                    
                    fireflyAttemptCounts[fireflyId]++;
                    
                    // Créer l'événement de tentative
                    var attemptEvent = GameStatEvent.CreateCaptureAttempt(
                        currentTime,
                        handPosition,
                        fireflyPos,
                        fireflyAttemptCounts[fireflyId],
                        false // On ne sait pas encore si ça va réussir
                    );
                    
                    RecordEvent(attemptEvent);
                    break; // Une seule tentative par mouvement
                }
            }
        }

        /// <summary>
        /// Vérifie s'il y a une luciole à proximité de la position donnée
        /// </summary>
        private bool CheckForNearbyFirefly(Vector2 position)
        {
            var fireflies = FindObjectsByType<FireflyController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            foreach (var firefly in fireflies)
            {
                if (firefly == null) continue;
                
                float distance = Vector2.Distance(position, firefly.transform.position);
                if (distance <= captureDistanceThreshold)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Crée le dossier de sauvegarde s'il n'existe pas
        /// </summary>
        private void CreateSaveDirectory()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, saveDirectory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                FireflyDanceLogger.Log($"Dossier d'analyse créé: {fullPath}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sauvegarde la session courante en JSON
        /// </summary>
        public void SaveCurrentSession()
        {
            if (currentSession == null)
            {
                FireflyDanceLogger.LogWarning("Aucune session à sauvegarder");
                return;
            }

            string fileName = $"FireflySession_{DateTime.Now:yyyyMMdd_HHmmss}_{currentSession.sessionId.Substring(0, 8)}.json";
            string fullPath = Path.Combine(Application.persistentDataPath, saveDirectory, fileName);

            try
            {
                string jsonData = currentSession.ToJson();
                File.WriteAllText(fullPath, jsonData);
                FireflyDanceLogger.Log($"Session sauvegardée: {fullPath}");
            }
            catch (Exception e)
            {
                FireflyDanceLogger.LogError($"Erreur lors de la sauvegarde: {e.Message}");
            }
        }

        /// <summary>
        /// Exporte la session courante et retourne les données JSON
        /// </summary>
        public string ExportCurrentSession()
        {
            if (currentSession == null)
            {
                FireflyDanceLogger.LogWarning("Aucune session à exporter");
                return null;
            }

            return currentSession.ToJson();
        }

        /// <summary>
        /// Obtient le résumé de la session courante
        /// </summary>
        public string GetCurrentSessionSummary()
        {
            if (currentSession == null) return "Aucune session active";
            return currentSession.GetSummary();
        }

        /// <summary>
        /// Active ou désactive l'enregistrement
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (isEnabled == enabled) return;
            
            isEnabled = enabled;
            
            if (!enabled && isRecording)
            {
                EndRecording();
            }
            
            FireflyDanceLogger.Log($"GameStatsRecorder {(enabled ? "activé" : "désactivé")}");
        }

        /// <summary>
        /// Obtient le chemin du dossier de sauvegarde
        /// </summary>
        public string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, saveDirectory);
        }

        #endregion

        #region Debug & Inspector

        /// <summary>
        /// Méthode pour tester l'enregistrement (appelable depuis l'Inspector)
        /// </summary>
        [ContextMenu("Test Recording")]
        private void TestRecording()
        {
            if (!isRecording)
            {
                StartRecording("Test Player");
                FireflyDanceLogger.Log("Test d'enregistrement démarré");
            }
            else
            {
                EndRecording();
                FireflyDanceLogger.Log("Test d'enregistrement terminé");
            }
        }

        /// <summary>
        /// Force la sauvegarde (appelable depuis l'Inspector)
        /// </summary>
        [ContextMenu("Force Save")]
        private void ForceSave()
        {
            SaveCurrentSession();
        }

        /// <summary>
        /// Affiche le résumé de la session (appelable depuis l'Inspector)
        /// </summary>
        [ContextMenu("Show Session Summary")]
        private void ShowSessionSummary()
        {
            Debug.Log(GetCurrentSessionSummary());
        }

        #endregion
    }
}
