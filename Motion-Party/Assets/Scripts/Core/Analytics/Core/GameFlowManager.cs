using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Analytics.Core;
using Core.Analytics.Data;

namespace Core.Analytics.Core
{
    /// <summary>
    /// Gestionnaire simplifié pour les analytics de session - S'intègre avec GameSessionManager existant
    /// Se concentre uniquement sur la collecte et l'analyse des données
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Analytics Configuration")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool showCompletionSummary = true;
        
        [Header("Session Management")]
        [SerializeField] private string currentSessionId;
        [SerializeField] private SessionAnalyzer currentSession;
        [SerializeField] private bool sessionActive = false;

        private static GameFlowManager _instance;
        public static GameFlowManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameFlowManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameFlowManager");
                        _instance = go.AddComponent<GameFlowManager>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }

        // Events
        public static event Action<string> OnMiniGameCompleted;
        public static event Action<SessionAnalysisResult> OnSessionCompleted;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // S'abonner aux événements du GameSessionManager existant
            SubscribeToGameSessionEvents();
        }

        #region Integration with GameSessionManager

        /// <summary>
        /// S'abonne aux événements du GameSessionManager pour la collecte automatique de données
        /// </summary>
        private void SubscribeToGameSessionEvents()
        {
            // Note: Ces événements devront être ajoutés au GameSessionManager
            // Exemple d'intégration pour plus tard
            if (GameSessionManager.Instance != null)
            {
                UnityEngine.Debug.Log("[GameFlowManager] Intégration avec GameSessionManager détectée");
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Démarre une nouvelle session analytics
        /// </summary>
        public void StartNewSession()
        {
            if (!enableAnalytics) return;
            
            if (sessionActive)
            {
                UnityEngine.Debug.LogWarning("[GameFlowManager] Une session analytics est déjà active");
                return;
            }

            currentSessionId = GenerateSessionId();
            currentSession = new SessionAnalyzer(currentSessionId);
            sessionActive = true;

            UnityEngine.Debug.Log($"[GameFlowManager] Session analytics démarrée: {currentSessionId}");
        }

        /// <summary>
        /// Ajoute un joueur à la session analytics
        /// </summary>
        public void AddPlayerToSession(string playerId)
        {
            if (!enableAnalytics || !sessionActive || currentSession == null)
            {
                return;
            }

            currentSession.AddPlayer(playerId);
            UnityEngine.Debug.Log($"[GameFlowManager] Joueur {playerId} ajouté à la session analytics {currentSessionId}");
        }

        /// <summary>
        /// Marque un mini-jeu comme complété dans les analytics
        /// </summary>
        public void CompleteMiniGame(string miniGameName)
        {
            if (!enableAnalytics || !sessionActive || currentSession == null)
            {
                return;
            }

            currentSession.MarkMiniGameCompleted(miniGameName);
            OnMiniGameCompleted?.Invoke(miniGameName);

            UnityEngine.Debug.Log($"[GameFlowManager] Analytics - Mini-jeu {miniGameName} complété");
        }

        /// <summary>
        /// Finalise la session analytics et génère les rapports
        /// </summary>
        public void CompleteSession()
        {
            if (!enableAnalytics || !sessionActive || currentSession == null)
            {
                return;
            }

            UnityEngine.Debug.Log("[GameFlowManager] Finalisation de la session analytics...");

            // Générer l'analyse finale
            var finalAnalysis = currentSession.AnalyzeSession();
            OnSessionCompleted?.Invoke(finalAnalysis);

            // Générer les fichiers de rapport
            if (AnalyticsFileGenerator.Instance != null)
            {
                AnalyticsFileGenerator.Instance.GenerateSessionReport(finalAnalysis);
            }

            // Afficher le résumé si configuré
            if (showCompletionSummary)
            {
                ShowCompletionSummary(finalAnalysis);
            }

            // Nettoyer la session
            sessionActive = false;
            currentSession = null;
            currentSessionId = null;

            UnityEngine.Debug.Log("[GameFlowManager] Session analytics terminée");
        }

        /// <summary>
        /// Obtient la session analytics actuelle
        /// </summary>
        public SessionAnalyzer GetCurrentSession()
        {
            return currentSession;
        }

        /// <summary>
        /// Vérifie si une session analytics est active
        /// </summary>
        public bool IsSessionActive()
        {
            return sessionActive && currentSession != null;
        }

        #endregion

        #region UI Summary

        /// <summary>
        /// Affiche le résumé de completion
        /// </summary>
        private void ShowCompletionSummary(SessionAnalysisResult analysis)
        {
            // Créer un UI temporaire pour afficher le résumé
            var summaryGO = new GameObject("SessionSummary");
            var summaryCanvas = summaryGO.AddComponent<Canvas>();
            summaryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            summaryCanvas.sortingOrder = 1000;

            // Ajouter le composant de résumé (maintenant dans Scripts/UI)
            var summaryComponent = summaryGO.AddComponent<UI.SessionSummaryUI>();
            summaryComponent.Initialize(currentSession);

            UnityEngine.Debug.Log("[GameFlowManager] Affichage du résumé analytics...");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Génère un ID unique pour la session
        /// </summary>
        private string GenerateSessionId()
        {
            return $"SESSION_{DateTime.Now:yyyyMMdd_HHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// Force la fin d'une session analytics (en cas d'erreur)
        /// </summary>
        public void ForceEndSession()
        {
            if (sessionActive && currentSession != null)
            {
                UnityEngine.Debug.Log("[GameFlowManager] Fin forcée de la session analytics");
                
                try
                {
                    var partialAnalysis = currentSession.AnalyzeSession();
                    OnSessionCompleted?.Invoke(partialAnalysis);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[GameFlowManager] Erreur lors de l'analyse partielle: {e.Message}");
                }

                sessionActive = false;
                currentSession = null;
                currentSessionId = null;
            }
        }

        /// <summary>
        /// Obtient le statut de la session analytics
        /// </summary>
        public SessionStatus GetSessionStatus()
        {
            if (!sessionActive || currentSession == null)
                return SessionStatus.Inactive;

            return new SessionStatus
            {
                isActive = true,
                sessionId = currentSessionId,
                progress = currentSession.SessionProgress,
                isComplete = currentSession.IsSessionComplete,
                playerCount = currentSession.GetAllPlayers().Count,
                remainingGames = currentSession.GetRemainingMiniGames()
            };
        }

        #endregion

        #region Events

        private void OnDestroy()
        {
            if (_instance == this)
            {
                OnMiniGameCompleted = null;
                OnSessionCompleted = null;
            }
        }

        #endregion
    }
}
