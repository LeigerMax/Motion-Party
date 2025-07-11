using UnityEngine;
using Gameplay.Common.Badges;

namespace Gameplay.Balloon.Badges
{
    /// <summary>
    /// Adaptateur pour intégrer le système de badges global avec le jeu Balloon Pop
    /// Simplifie l'utilisation et fournit des méthodes spécifiques au jeu de ballons
    /// </summary>
    public class BalloonBadgeAdapter : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private bool enableAutoTracking = true;
        [SerializeField] private bool enableDebugLogs = true;

        // Références au système global
        private GlobalBadgeSystem globalBadgeSystem;
        private GlobalBadgeTracker globalBadgeTracker;

        // Constantes du jeu
        private const string GAME_ID = "balloon";

        // Cache pour les événements
        private string currentPlayerName;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            InitializeAdapter();
            
            if (enableAutoTracking)
            {
                SubscribeToBalloonEvents();
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromBalloonEvents();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Définit le joueur actuel pour le tracking automatique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void SetCurrentPlayer(string playerName)
        {
            currentPlayerName = playerName;
            LogDebug($"Joueur actuel défini: {playerName}");
        }

        /// <summary>
        /// Met à jour le score du joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="score">Nouveau score</param>
        public void UpdatePlayerScore(string playerName, float score)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateScore(playerName, GAME_ID, score);
                LogDebug($"Score mis à jour: {playerName} = {score}");
            }
        }

        /// <summary>
        /// Met à jour le temps de jeu du joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="timeInSeconds">Temps en secondes</param>
        public void UpdatePlayerTime(string playerName, float timeInSeconds)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateTime(playerName, GAME_ID, timeInSeconds);
                LogDebug($"Temps mis à jour: {playerName} = {timeInSeconds}s");
            }
        }

        /// <summary>
        /// Incrémente le nombre de ballons éclatés
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="popped">Nombre éclaté (défaut: 1)</param>
        public void IncrementBalloonsPopped(string playerName, int popped = 1)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.AddToMetric(playerName, GAME_ID, "popped", popped);
                LogDebug($"Ballons éclatés: {playerName} +{popped}");
            }
        }

        /// <summary>
        /// Met à jour la précision du joueur (ratio succès/tentatives)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="accuracy">Précision en pourcentage (0-100)</param>
        public void UpdatePlayerAccuracy(string playerName, float accuracy)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateAccuracy(playerName, GAME_ID, accuracy);
                LogDebug($"Précision mise à jour: {playerName} = {accuracy}%");
            }
        }

        /// <summary>
        /// Met à jour la vitesse maximale atteinte
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="speed">Vitesse (ballons par seconde)</param>
        public void UpdateMaxSpeed(string playerName, float speed)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "maxspeed", speed);
                LogDebug($"Vitesse max mise à jour: {playerName} = {speed} ballons/s");
            }
        }

        /// <summary>
        /// Incrémente le nombre d'éclats en chaîne
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="chainLength">Longueur de la chaîne</param>
        public void RegisterChainPop(string playerName, int chainLength)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "maxchain", chainLength);
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "chains");
                LogDebug($"Chaîne d'éclats: {playerName} = {chainLength}");
            }
        }

        /// <summary>
        /// Marque un ballon spécial éclaté
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="balloonType">Type de ballon spécial</param>
        public void PopSpecialBalloon(string playerName, string balloonType)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, $"special_{balloonType.ToLower()}");
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "special_total");
                LogDebug($"Ballon spécial éclaté: {playerName} - {balloonType}");
            }
        }

        /// <summary>
        /// Met à jour le nombre de ballons manqués
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void IncrementMissedBalloons(string playerName)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "missed");
                LogDebug($"Ballon manqué: {playerName}");
            }
        }

        /// <summary>
        /// Attribue manuellement un badge spécifique
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="badgeId">ID du badge</param>
        /// <param name="value">Valeur à valider (optionnel)</param>
        /// <returns>True si attribué avec succès</returns>
        public bool AwardBadge(string playerName, string badgeId, float value = 0f)
        {
            if (globalBadgeSystem != null)
            {
                bool success = globalBadgeSystem.TryEarnBadge(playerName, GAME_ID, badgeId, value);
                LogDebug($"Attribution manuelle {badgeId}: {(success ? "SUCCÈS" : "ÉCHEC")}");
                return success;
            }
            return false;
        }

        /// <summary>
        /// Force l'attribution d'un badge (ignore les conditions)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="badgeId">ID du badge</param>
        /// <returns>True si forcé avec succès</returns>
        public bool ForceAwardBadge(string playerName, string badgeId)
        {
            if (globalBadgeSystem != null)
            {
                bool success = globalBadgeSystem.ForceEarnBadge(playerName, GAME_ID, badgeId);
                LogDebug($"Attribution forcée {badgeId}: {(success ? "SUCCÈS" : "ÉCHEC")}");
                return success;
            }
            return false;
        }

        /// <summary>
        /// Obtient les badges du joueur pour Balloon Pop
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Liste des badges obtenus</returns>
        public System.Collections.Generic.List<BadgeInstance> GetPlayerBadges(string playerName)
        {
            if (globalBadgeSystem != null)
            {
                return globalBadgeSystem.GetPlayerBadgesForGame(playerName, GAME_ID);
            }
            return new System.Collections.Generic.List<BadgeInstance>();
        }

        /// <summary>
        /// Obtient la progression du joueur (pourcentage)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Pourcentage de progression (0-100)</returns>
        public float GetPlayerProgression(string playerName)
        {
            if (globalBadgeSystem != null)
            {
                return globalBadgeSystem.GetPlayerProgressionForGame(playerName, GAME_ID);
            }
            return 0f;
        }

        /// <summary>
        /// Valide immédiatement tous les badges du joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Nombre de badges attribués</returns>
        public int ValidatePlayerBadges(string playerName)
        {
            if (globalBadgeTracker != null)
            {
                int earned = globalBadgeTracker.ValidatePlayerMetricsForGame(playerName, GAME_ID);
                LogDebug($"Validation forcée: {earned} badge(s) attribué(s)");
                return earned;
            }
            return 0;
        }

        #endregion

        #region Balloon-Specific Methods

        /// <summary>
        /// Met à jour toutes les statistiques en une fois après une partie
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="gameResult">Résultat de la partie</param>
        public void UpdateGameResult(string playerName, BalloonGameResult gameResult)
        {
            if (globalBadgeTracker == null) return;

            // Mettre à jour toutes les métriques
            globalBadgeTracker.UpdateScore(playerName, GAME_ID, gameResult.finalScore);
            globalBadgeTracker.UpdateTime(playerName, GAME_ID, gameResult.gameTime);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "popped", gameResult.balloonsPopped);
            globalBadgeTracker.UpdateAccuracy(playerName, GAME_ID, gameResult.accuracy);
            globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "maxspeed", gameResult.maxSpeed);
            globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "maxchain", gameResult.maxChain);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "special_total", gameResult.specialBalloonsPopped);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "missed", gameResult.balloonsMissed);

            LogDebug($"Résultat complet mis à jour pour {playerName}");

            // Validation automatique
            if (enableAutoTracking)
            {
                ValidatePlayerBadges(playerName);
            }
        }

        /// <summary>
        /// Événement appelé quand le joueur atteint un nouveau record
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="recordType">Type de record</param>
        /// <param name="value">Valeur du record</param>
        public void OnNewRecord(string playerName, string recordType, float value)
        {
            // Awards spéciaux pour les records
            switch (recordType.ToLower())
            {
                case "score":
                    if (value >= 5000f) ForceAwardBadge(playerName, "balloon_master");
                    break;
                case "speed":
                    if (value >= 10f) ForceAwardBadge(playerName, "lightning_hands");
                    break;
                case "chain":
                    if (value >= 15f) ForceAwardBadge(playerName, "chain_master");
                    break;
                case "accuracy":
                    if (value >= 95f) ForceAwardBadge(playerName, "sharpshooter");
                    break;
            }

            LogDebug($"Nouveau record {recordType}: {playerName} = {value}");
        }

        /// <summary>
        /// Événement appelé pour des réalisations spéciales
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="achievement">Type de réalisation</param>
        public void OnSpecialAchievement(string playerName, string achievement)
        {
            switch (achievement.ToLower())
            {
                case "perfect_round":
                    ForceAwardBadge(playerName, "sharpshooter");
                    break;
                case "speed_demon":
                    ForceAwardBadge(playerName, "lightning_hands");
                    break;
                case "chain_reaction":
                    ForceAwardBadge(playerName, "chain_master");
                    break;
                case "balloon_legend":
                    ForceAwardBadge(playerName, "balloon_master");
                    break;
            }

            LogDebug($"Réalisation spéciale: {playerName} - {achievement}");
        }

        /// <summary>
        /// Gère les différents types de ballons spéciaux
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="balloonType">Type de ballon</param>
        /// <param name="points">Points gagnés</param>
        public void HandleSpecialBalloon(string playerName, BalloonType balloonType, int points)
        {
            PopSpecialBalloon(playerName, balloonType.ToString());
            
            // Bonus de score pour les ballons spéciaux
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.AddToMetric(playerName, GAME_ID, "bonus_points", points);
            }

            // Achievements spéciaux selon le type
            switch (balloonType)
            {
                case BalloonType.Golden:
                    if (globalBadgeTracker.GetMetric(playerName, GAME_ID, "special_golden") >= 5)
                    {
                        ForceAwardBadge(playerName, "golden_touch");
                    }
                    break;
                case BalloonType.Explosive:
                    if (globalBadgeTracker.GetMetric(playerName, GAME_ID, "special_explosive") >= 10)
                    {
                        ForceAwardBadge(playerName, "demolition_expert");
                    }
                    break;
                case BalloonType.Rainbow:
                    if (globalBadgeTracker.GetMetric(playerName, GAME_ID, "special_rainbow") >= 3)
                    {
                        ForceAwardBadge(playerName, "rainbow_hunter");
                    }
                    break;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// S'abonne aux événements de Balloon Pop
        /// </summary>
        private void SubscribeToBalloonEvents()
        {
            // TODO: S'abonner aux événements spécifiques du jeu Balloon
            // BalloonEvents.OnBalloonPopped += OnBalloonPopped;
            // BalloonEvents.OnScoreUpdated += OnScoreUpdated;
            // BalloonEvents.OnGameCompleted += OnGameCompleted;
            // BalloonEvents.OnChainReaction += OnChainReaction;
            
            LogDebug("Abonnement aux événements Balloon activé");
        }

        /// <summary>
        /// Se désabonne des événements de Balloon Pop
        /// </summary>
        private void UnsubscribeFromBalloonEvents()
        {
            // TODO: Se désabonner des événements spécifiques du jeu Balloon
            // BalloonEvents.OnBalloonPopped -= OnBalloonPopped;
            // BalloonEvents.OnScoreUpdated -= OnScoreUpdated;
            // BalloonEvents.OnGameCompleted -= OnGameCompleted;
            // BalloonEvents.OnChainReaction -= OnChainReaction;
            
            LogDebug("Désabonnement des événements Balloon");
        }

        // Handlers d'événements (à adapter selon les vrais événements de Balloon)
        private void OnBalloonPopped(string playerName, BalloonType balloonType, int points)
        {
            IncrementBalloonsPopped(playerName);
            if (balloonType != BalloonType.Normal)
            {
                HandleSpecialBalloon(playerName, balloonType, points);
            }
        }

        private void OnScoreUpdated(string playerName, float score)
        {
            UpdatePlayerScore(playerName, score);
        }

        private void OnGameCompleted(string playerName, BalloonGameResult result)
        {
            UpdateGameResult(playerName, result);
        }

        private void OnChainReaction(string playerName, int chainLength)
        {
            RegisterChainPop(playerName, chainLength);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialise l'adaptateur
        /// </summary>
        private void InitializeAdapter()
        {
            // Trouver les systèmes globaux
            globalBadgeSystem = GlobalBadgeSystem.Instance;
            globalBadgeTracker = GlobalBadgeTracker.Instance;

            if (globalBadgeSystem == null)
            {
                Debug.LogError("[BalloonBadgeAdapter] GlobalBadgeSystem non trouvé!");
            }

            if (globalBadgeTracker == null)
            {
                Debug.LogError("[BalloonBadgeAdapter] GlobalBadgeTracker non trouvé!");
            }

            if (globalBadgeSystem != null && globalBadgeTracker != null)
            {
                LogDebug("BalloonBadgeAdapter initialisé avec succès");
            }
        }

        /// <summary>
        /// Log de debug
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[BalloonBadgeAdapter] {message}");
        }

        #endregion

        #region Public Static Helpers

        /// <summary>
        /// Raccourci statique pour mettre à jour le score
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="score">Score</param>
        public static void UpdateScore(string playerName, float score)
        {
            var adapter = FindFirstObjectByType<BalloonBadgeAdapter>();
            adapter?.UpdatePlayerScore(playerName, score);
        }

        /// <summary>
        /// Raccourci statique pour éclater un ballon
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="balloonType">Type de ballon</param>
        public static void PopBalloon(string playerName, BalloonType balloonType = BalloonType.Normal)
        {
            var adapter = FindFirstObjectByType<BalloonBadgeAdapter>();
            if (adapter != null)
            {
                adapter.IncrementBalloonsPopped(playerName);
                if (balloonType != BalloonType.Normal)
                {
                    adapter.PopSpecialBalloon(playerName, balloonType.ToString());
                }
            }
        }

        /// <summary>
        /// Raccourci statique pour valider les badges
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Nombre de badges attribués</returns>
        public static int ValidateBadges(string playerName)
        {
            var adapter = FindFirstObjectByType<BalloonBadgeAdapter>();
            return adapter?.ValidatePlayerBadges(playerName) ?? 0;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Types de ballons disponibles
        /// </summary>
        public enum BalloonType
        {
            Normal,
            Golden,
            Explosive,
            Rainbow,
            Bonus
        }

        /// <summary>
        /// Structure pour représenter le résultat d'une partie de Balloon Pop
        /// </summary>
        [System.Serializable]
        public class BalloonGameResult
        {
            public float finalScore;
            public float gameTime;
            public int balloonsPopped;
            public float accuracy;
            public float maxSpeed;
            public int maxChain;
            public int specialBalloonsPopped;
            public int balloonsMissed;

            public BalloonGameResult(float score, float time, int popped, float acc, float speed, int chain, int special, int missed)
            {
                finalScore = score;
                gameTime = time;
                balloonsPopped = popped;
                accuracy = acc;
                maxSpeed = speed;
                maxChain = chain;
                specialBalloonsPopped = special;
                balloonsMissed = missed;
            }
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// Teste l'adaptateur avec des données fictives
        /// </summary>
        [ContextMenu("Test Balloon Adapter")]
        public void TestAdapter()
        {
            string testPlayer = "TestBalloonPlayer";
            
            // Test des métriques
            UpdatePlayerScore(testPlayer, 3500f);
            UpdatePlayerTime(testPlayer, 60f);
            IncrementBalloonsPopped(testPlayer, 50);
            UpdatePlayerAccuracy(testPlayer, 88f);
            UpdateMaxSpeed(testPlayer, 8.5f);
            RegisterChainPop(testPlayer, 12);
            PopSpecialBalloon(testPlayer, "golden");
            
            // Test de validation
            int badges = ValidatePlayerBadges(testPlayer);
            
            Debug.Log($"Test Balloon terminé: {badges} badge(s) attribué(s)");
        }

        /// <summary>
        /// Simule une partie complète
        /// </summary>
        [ContextMenu("Simulate Full Game")]
        public void SimulateFullGame()
        {
            string testPlayer = "SimulatedBalloonPlayer";
            
            var gameResult = new BalloonGameResult(
                score: 4200f,
                time: 90f,
                popped: 75,
                acc: 92f,
                speed: 9.2f,
                chain: 18,
                special: 8,
                missed: 6
            );
            
            UpdateGameResult(testPlayer, gameResult);
            
            Debug.Log("Simulation de partie Balloon complète terminée");
        }
#endif

        #endregion
    }
}
