using UnityEngine;
using System.Linq;
using Gameplay.Common.Badges;

namespace Gameplay.Firefly.Badges
{
    /// <summary>
    /// Adaptateur pour intégrer le système de badges global avec le jeu Firefly Dance
    /// Simplifie l'utilisation et fournit des méthodes spécifiques au jeu
    /// </summary>
    public class FireflyBadgeAdapter : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private bool enableAutoTracking = true;
        [SerializeField] private bool enableDebugLogs = true;

        // Références au système global
        private GlobalBadgeSystem globalBadgeSystem;
        private GlobalBadgeTracker globalBadgeTracker;

        // Constantes du jeu
        private const string GAME_ID = "firefly";

        // Cache pour les événements
        private string currentPlayerName;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            InitializeAdapter();
            
            if (enableAutoTracking)
            {
                SubscribeToFireflyEvents();
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromFireflyEvents();
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
        /// Incrémente le nombre de lucioles collectées
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="collected">Nombre collecté (défaut: 1)</param>
        public void IncrementFirefliesCollected(string playerName, int collected = 1)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.AddToMetric(playerName, GAME_ID, "collected", collected);
                LogDebug($"Lucioles collectées: {playerName} +{collected}");
            }
        }

        /// <summary>
        /// Met à jour la précision du joueur
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
        /// Met à jour la durée de jeu
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="duration">Durée en secondes</param>
        public void UpdateGameDuration(string playerName, float duration)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "duration", duration);
                LogDebug($"Durée mise à jour: {playerName} = {duration}s");
            }
        }

        /// <summary>
        /// Incrémente le combo du joueur
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void IncrementCombo(string playerName)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "combo");
                LogDebug($"Combo incrémenté: {playerName}");
            }
        }

        /// <summary>
        /// Met à jour le combo maximum atteint
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="maxCombo">Combo maximum</param>
        public void UpdateMaxCombo(string playerName, int maxCombo)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "maxcombo", maxCombo);
                LogDebug($"Combo max mis à jour: {playerName} = {maxCombo}");
            }
        }

        /// <summary>
        /// Marque une luciole parfaite (timing parfait)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void MarkPerfectFirefly(string playerName)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "perfect");
                LogDebug($"Luciole parfaite: {playerName}");
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
        /// Obtient les badges du joueur pour Firefly Dance
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

        #region Firefly-Specific Methods

        /// <summary>
        /// Met à jour toutes les statistiques en une fois après une partie
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="gameResult">Résultat de la partie</param>
        public void UpdateGameResult(string playerName, FireflyGameResult gameResult)
        {
            if (globalBadgeTracker == null) return;

            // Mettre à jour toutes les métriques
            globalBadgeTracker.UpdateScore(playerName, GAME_ID, gameResult.finalScore);
            globalBadgeTracker.UpdateTime(playerName, GAME_ID, gameResult.gameTime);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "collected", gameResult.firefliesCollected);
            globalBadgeTracker.UpdateAccuracy(playerName, GAME_ID, gameResult.accuracy);
            globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "maxcombo", gameResult.maxCombo);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "perfect", gameResult.perfectFireflies);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "duration", gameResult.totalDuration);

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
                    if (value >= 2000f) ForceAwardBadge(playerName, "score_legend");
                    break;
                case "time":
                    if (value <= 30f) ForceAwardBadge(playerName, "speedster");
                    break;
                case "accuracy":
                    if (value >= 95f) ForceAwardBadge(playerName, "perfectionist");
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
                case "no_miss":
                    ForceAwardBadge(playerName, "perfectionist");
                    break;
                case "speed_run":
                    ForceAwardBadge(playerName, "speedster");
                    break;
                case "marathon":
                    ForceAwardBadge(playerName, "endurance_master");
                    break;
                case "combo_master":
                    ForceAwardBadge(playerName, "combo_master");
                    break;
            }

            LogDebug($"Réalisation spéciale: {playerName} - {achievement}");
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// S'abonne aux événements de Firefly Dance
        /// </summary>
        private void SubscribeToFireflyEvents()
        {
            // TODO: S'abonner aux événements spécifiques du jeu Firefly
            // FireflyDanceEvents.OnScoreUpdated += OnScoreUpdated;
            // FireflyDanceEvents.OnFireflyCollected += OnFireflyCollected;
            // FireflyDanceEvents.OnGameCompleted += OnGameCompleted;
            
            LogDebug("Abonnement aux événements Firefly activé");
        }

        /// <summary>
        /// Se désabonne des événements de Firefly Dance
        /// </summary>
        private void UnsubscribeFromFireflyEvents()
        {
            // TODO: Se désabonner des événements spécifiques du jeu Firefly
            // FireflyDanceEvents.OnScoreUpdated -= OnScoreUpdated;
            // FireflyDanceEvents.OnFireflyCollected -= OnFireflyCollected;
            // FireflyDanceEvents.OnGameCompleted -= OnGameCompleted;
            
            LogDebug("Désabonnement des événements Firefly");
        }

        // Handlers d'événements (à adapter selon les vrais événements de Firefly)
        private void OnScoreUpdated(string playerName, float score)
        {
            UpdatePlayerScore(playerName, score);
        }

        private void OnFireflyCollected(string playerName)
        {
            IncrementFirefliesCollected(playerName);
        }

        private void OnGameCompleted(string playerName, FireflyGameResult result)
        {
            UpdateGameResult(playerName, result);
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
                Debug.LogError("[FireflyBadgeAdapter] GlobalBadgeSystem non trouvé!");
            }

            if (globalBadgeTracker == null)
            {
                Debug.LogError("[FireflyBadgeAdapter] GlobalBadgeTracker non trouvé!");
            }

            if (globalBadgeSystem != null && globalBadgeTracker != null)
            {
                LogDebug("FireflyBadgeAdapter initialisé avec succès");
            }
        }

        /// <summary>
        /// Log de debug
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[FireflyBadgeAdapter] {message}");
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
            var adapter = FindFirstObjectByType<FireflyBadgeAdapter>();
            adapter?.UpdatePlayerScore(playerName, score);
        }

        /// <summary>
        /// Raccourci statique pour incrémenter les collectes
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public static void CollectFirefly(string playerName)
        {
            var adapter = FindFirstObjectByType<FireflyBadgeAdapter>();
            adapter?.IncrementFirefliesCollected(playerName);
        }

        /// <summary>
        /// Raccourci statique pour valider les badges
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Nombre de badges attribués</returns>
        public static int ValidateBadges(string playerName)
        {
            var adapter = FindFirstObjectByType<FireflyBadgeAdapter>();
            return adapter?.ValidatePlayerBadges(playerName) ?? 0;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Structure pour représenter le résultat d'une partie de Firefly Dance
        /// </summary>
        [System.Serializable]
        public class FireflyGameResult
        {
            public float finalScore;
            public float gameTime;
            public int firefliesCollected;
            public float accuracy;
            public int maxCombo;
            public int perfectFireflies;
            public float totalDuration;

            public FireflyGameResult(float score, float time, int collected, float acc, int combo, int perfect, float duration)
            {
                finalScore = score;
                gameTime = time;
                firefliesCollected = collected;
                accuracy = acc;
                maxCombo = combo;
                perfectFireflies = perfect;
                totalDuration = duration;
            }
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// Teste l'adaptateur avec des données fictives
        /// </summary>
        [ContextMenu("Test Firefly Adapter")]
        public void TestAdapter()
        {
            string testPlayer = "TestFireflyPlayer";
            
            // Test des métriques
            UpdatePlayerScore(testPlayer, 1500f);
            UpdatePlayerTime(testPlayer, 45f);
            IncrementFirefliesCollected(testPlayer, 10);
            UpdatePlayerAccuracy(testPlayer, 92f);
            UpdateMaxCombo(testPlayer, 15);
            
            // Test de validation
            int badges = ValidatePlayerBadges(testPlayer);
            
            Debug.Log($"Test Firefly terminé: {badges} badge(s) attribué(s)");
        }

        /// <summary>
        /// Simule une partie complète
        /// </summary>
        [ContextMenu("Simulate Full Game")]
        public void SimulateFullGame()
        {
            string testPlayer = "SimulatedPlayer";
            
            var gameResult = new FireflyGameResult(
                score: 1800f,
                time: 38f,
                collected: 20,
                acc: 95f,
                combo: 25,
                perfect: 15,
                duration: 60f
            );
            
            UpdateGameResult(testPlayer, gameResult);
            
            Debug.Log("Simulation de partie complète terminée");
        }

        /// <summary>
        /// Test complet du système de badges pour Firefly (debug)
        /// </summary>
        [ContextMenu("Test Complete Firefly Badge System")]
        public void TestCompleteBadgeSystem()
        {
            var testPlayer = "TestPlayer";
            
            Debug.Log($"🧪 TEST COMPLET SYSTÈME BADGES FIREFLY");
            
            // 1. Vérifier l'initialisation
            if (globalBadgeSystem == null || globalBadgeTracker == null)
            {
                Debug.LogError("❌ Système non initialisé !");
                InitializeAdapter();
            }
            
            // 2. Tester l'attribution du premier badge
            Debug.Log($"🎯 Test attribution 'first_firefly'");
            IncrementFirefliesCollected(testPlayer, 1);
            
            // 3. Forcer la validation
            int badges = ValidatePlayerBadges(testPlayer);
            Debug.Log($"✅ Validation terminée: {badges} badges attribués");
            
            // 4. Afficher les résultats
            var storage = FindFirstObjectByType<GlobalPlayerBadgeStorage>();
            if (storage != null)
            {
                storage.ShowPlayerBadges(testPlayer);
            }
        }

        /// <summary>
        /// Test d'attribution forcée d'un badge (debug)
        /// </summary>
        [ContextMenu("Force First Firefly Badge")]
        public void ForceFirstFireflyBadge()
        {
            var testPlayer = "TestPlayer";
            
            if (globalBadgeSystem != null)
            {
                bool success = globalBadgeSystem.ForceEarnBadge(testPlayer, GAME_ID, "first_firefly");
                Debug.Log($"🎯 Attribution forcée 'first_firefly': {(success ? "SUCCÈS" : "ÉCHEC")}");
                
                var storage = FindFirstObjectByType<GlobalPlayerBadgeStorage>();
                storage?.ShowPlayerBadges(testPlayer);
            }
        }

        /// <summary>
        /// Diagnostic complet de l'adaptateur
        /// </summary>
        [ContextMenu("Diagnostic Firefly Adapter")]
        public void DiagnosticAdapter()
        {
            Debug.Log($"🔍 DIAGNOSTIC FIREFLY BADGE ADAPTER");
            Debug.Log($"   Système global: {(globalBadgeSystem != null ? "✓ OK" : "❌ NULL")}");
            Debug.Log($"   Tracker global: {(globalBadgeTracker != null ? "✓ OK" : "❌ NULL")}");
            Debug.Log($"   Auto tracking: {(enableAutoTracking ? "✓ ACTIVÉ" : "❌ DÉSACTIVÉ")}");
            Debug.Log($"   Debug logs: {(enableDebugLogs ? "✓ ACTIVÉ" : "❌ DÉSACTIVÉ")}");
            Debug.Log($"   Joueur actuel: {currentPlayerName ?? "NON DÉFINI"}");
            
            // Vérifier la base de données
            if (globalBadgeSystem != null)
            {
                var database = globalBadgeSystem.GetDatabase();
                if (database != null)
                {
                    var fireflyBadges = database.GetBadgesForGame(GAME_ID);
                    Debug.Log($"   Badges Firefly disponibles: {fireflyBadges.Count}");
                    
                    var firstFireflyBadge = fireflyBadges.FirstOrDefault(b => b.BadgeId == "first_firefly");
                    if (firstFireflyBadge != null)
                    {
                        Debug.Log($"   ✓ Badge 'first_firefly' trouvé: {firstFireflyBadge.BadgeName} (cible: {firstFireflyBadge.TargetValue})");
                    }
                    else
                    {
                        Debug.LogError($"   ❌ Badge 'first_firefly' INTROUVABLE !");
                    }
                }
                else
                {
                    Debug.LogError($"   ❌ Database non trouvée !");
                }
            }
        }
#endif

        #endregion
    }
}
