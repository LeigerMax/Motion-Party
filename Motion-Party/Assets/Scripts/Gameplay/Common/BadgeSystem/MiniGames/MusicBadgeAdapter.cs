using UnityEngine;
using Gameplay.Common.Badges;

namespace Gameplay.Music.Badges
{
    /// <summary>
    /// Adaptateur pour intégrer le système de badges global avec le jeu Music Note Press
    /// Simplifie l'utilisation et fournit des méthodes spécifiques au jeu musical
    /// </summary>
    public class MusicBadgeAdapter : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private bool enableAutoTracking = true;
        [SerializeField] private bool enableDebugLogs = true;

        // Références au système global
        private GlobalBadgeSystem globalBadgeSystem;
        private GlobalBadgeTracker globalBadgeTracker;

        // Constantes du jeu
        private const string GAME_ID = "music";

        // Cache pour les événements
        private string currentPlayerName;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            InitializeAdapter();
            
            if (enableAutoTracking)
            {
                SubscribeToMusicEvents();
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromMusicEvents();
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
        /// Met à jour le temps de la chanson
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="timeInSeconds">Temps en secondes</param>
        public void UpdateSongTime(string playerName, float timeInSeconds)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateTime(playerName, GAME_ID, timeInSeconds);
                LogDebug($"Temps de chanson mis à jour: {playerName} = {timeInSeconds}s");
            }
        }

        /// <summary>
        /// Incrémente le nombre de notes réussies
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="hits">Nombre de notes réussies (défaut: 1)</param>
        public void IncrementNotesHit(string playerName, int hits = 1)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.AddToMetric(playerName, GAME_ID, "notes_hit", hits);
                LogDebug($"Notes réussies: {playerName} +{hits}");
            }
        }

        /// <summary>
        /// Incrémente le nombre de notes manquées
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="misses">Nombre de notes manquées (défaut: 1)</param>
        public void IncrementNotesMissed(string playerName, int misses = 1)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.AddToMetric(playerName, GAME_ID, "notes_missed", misses);
                LogDebug($"Notes manquées: {playerName} +{misses}");
            }
        }

        /// <summary>
        /// Met à jour la précision du joueur (ratio réussites/total)
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
        /// Met à jour le combo actuel et maximum
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="currentCombo">Combo actuel</param>
        /// <param name="maxCombo">Combo maximum</param>
        public void UpdateCombo(string playerName, int currentCombo, int maxCombo)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "combo", currentCombo);
                globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "max_combo", maxCombo);
                LogDebug($"Combo mis à jour: {playerName} = {currentCombo} (max: {maxCombo})");
            }
        }

        /// <summary>
        /// Enregistre une note parfaite (timing excellent)
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        public void RegisterPerfectNote(string playerName)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "perfect_notes");
                LogDebug($"Note parfaite: {playerName}");
            }
        }

        /// <summary>
        /// Met à jour le BPM de la chanson jouée
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="bpm">Battements par minute</param>
        public void UpdateSongBPM(string playerName, float bpm)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "max_bpm", bpm);
                LogDebug($"BPM de chanson: {playerName} = {bpm}");
            }
        }

        /// <summary>
        /// Enregistre la difficulté de la chanson jouée
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="difficulty">Niveau de difficulté (1-5)</param>
        public void UpdateSongDifficulty(string playerName, int difficulty)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "max_difficulty", difficulty);
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, $"difficulty_{difficulty}");
                LogDebug($"Difficulté de chanson: {playerName} = {difficulty}");
            }
        }

        /// <summary>
        /// Marque une chanson comme complétée
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="songName">Nom de la chanson</param>
        public void CompleteSong(string playerName, string songName)
        {
            if (globalBadgeTracker != null)
            {
                globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "songs_completed");
                LogDebug($"Chanson complétée: {playerName} - {songName}");
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
        /// Obtient les badges du joueur pour Music Note Press
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

        #region Music-Specific Methods

        /// <summary>
        /// Met à jour toutes les statistiques en une fois après une chanson
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="songResult">Résultat de la chanson</param>
        public void UpdateSongResult(string playerName, MusicGameResult songResult)
        {
            if (globalBadgeTracker == null) return;

            // Mettre à jour toutes les métriques
            globalBadgeTracker.UpdateScore(playerName, GAME_ID, songResult.finalScore);
            globalBadgeTracker.UpdateTime(playerName, GAME_ID, songResult.songDuration);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "notes_hit", songResult.notesHit);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "notes_missed", songResult.notesMissed);
            globalBadgeTracker.UpdateAccuracy(playerName, GAME_ID, songResult.accuracy);
            globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "max_combo", songResult.maxCombo);
            globalBadgeTracker.UpdateMetric(playerName, GAME_ID, "perfect_notes", songResult.perfectNotes);
            globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "max_bpm", songResult.songBPM);
            globalBadgeTracker.UpdateMaxMetric(playerName, GAME_ID, "max_difficulty", songResult.difficulty);
            globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "songs_completed");

            LogDebug($"Résultat de chanson complet mis à jour pour {playerName}");

            // Validation automatique
            if (enableAutoTracking)
            {
                ValidatePlayerBadges(playerName);
            }
        }

        /// <summary>
        /// Gère les différents types de notes spéciales
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="noteType">Type de note</param>
        /// <param name="timing">Qualité du timing</param>
        public void HandleSpecialNote(string playerName, NoteType noteType, NoteTiming timing)
        {
            if (globalBadgeTracker == null) return;

            // Incrémenter le compteur du type de note
            globalBadgeTracker.IncrementMetric(playerName, GAME_ID, $"note_{noteType.ToString().ToLower()}");
            
            // Gérer le timing
            switch (timing)
            {
                case NoteTiming.Perfect:
                    RegisterPerfectNote(playerName);
                    globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "perfect_timing");
                    break;
                case NoteTiming.Great:
                    globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "great_timing");
                    break;
                case NoteTiming.Good:
                    globalBadgeTracker.IncrementMetric(playerName, GAME_ID, "good_timing");
                    break;
            }

            LogDebug($"Note spéciale: {playerName} - {noteType} ({timing})");

            // Achievements spéciaux
            CheckSpecialNoteAchievements(playerName, noteType);
        }

        /// <summary>
        /// Vérifie les achievements liés aux notes spéciales
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="noteType">Type de note</param>
        private void CheckSpecialNoteAchievements(string playerName, NoteType noteType)
        {
            if (globalBadgeTracker == null) return;

            switch (noteType)
            {
                case NoteType.Hold:
                    if (globalBadgeTracker.GetMetric(playerName, GAME_ID, "note_hold") >= 50)
                    {
                        ForceAwardBadge(playerName, "hold_master");
                    }
                    break;
                case NoteType.Slide:
                    if (globalBadgeTracker.GetMetric(playerName, GAME_ID, "note_slide") >= 30)
                    {
                        ForceAwardBadge(playerName, "slide_expert");
                    }
                    break;
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
                    if (value >= 100000f) ForceAwardBadge(playerName, "rhythm_legend");
                    break;
                case "accuracy":
                    if (value >= 98f) ForceAwardBadge(playerName, "perfect_pitch");
                    break;
                case "combo":
                    if (value >= 500f) ForceAwardBadge(playerName, "combo_king");
                    break;
                case "perfect_notes":
                    if (value >= 1000f) ForceAwardBadge(playerName, "note_perfectionist");
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
                case "full_combo":
                    ForceAwardBadge(playerName, "combo_king");
                    break;
                case "perfect_score":
                    ForceAwardBadge(playerName, "perfect_pitch");
                    break;
                case "speed_demon":
                    ForceAwardBadge(playerName, "rhythm_legend");
                    break;
                case "note_master":
                    ForceAwardBadge(playerName, "note_perfectionist");
                    break;
            }

            LogDebug($"Réalisation spéciale: {playerName} - {achievement}");
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// S'abonne aux événements de Music Note Press
        /// </summary>
        private void SubscribeToMusicEvents()
        {
            // TODO: S'abonner aux événements spécifiques du jeu Music
            // MusicEvents.OnNoteHit += OnNoteHit;
            // MusicEvents.OnNoteMissed += OnNoteMissed;
            // MusicEvents.OnScoreUpdated += OnScoreUpdated;
            // MusicEvents.OnSongCompleted += OnSongCompleted;
            // MusicEvents.OnComboChanged += OnComboChanged;
            
            LogDebug("Abonnement aux événements Music activé");
        }

        /// <summary>
        /// Se désabonne des événements de Music Note Press
        /// </summary>
        private void UnsubscribeFromMusicEvents()
        {
            // TODO: Se désabonner des événements spécifiques du jeu Music
            // MusicEvents.OnNoteHit -= OnNoteHit;
            // MusicEvents.OnNoteMissed -= OnNoteMissed;
            // MusicEvents.OnScoreUpdated -= OnScoreUpdated;
            // MusicEvents.OnSongCompleted -= OnSongCompleted;
            // MusicEvents.OnComboChanged -= OnComboChanged;
            
            LogDebug("Désabonnement des événements Music");
        }

        // Handlers d'événements (à adapter selon les vrais événements de Music)
        private void OnNoteHit(string playerName, NoteType noteType, NoteTiming timing)
        {
            IncrementNotesHit(playerName);
            HandleSpecialNote(playerName, noteType, timing);
        }

        private void OnNoteMissed(string playerName)
        {
            IncrementNotesMissed(playerName);
        }

        private void OnScoreUpdated(string playerName, float score)
        {
            UpdatePlayerScore(playerName, score);
        }

        private void OnSongCompleted(string playerName, MusicGameResult result)
        {
            UpdateSongResult(playerName, result);
        }

        private void OnComboChanged(string playerName, int currentCombo, int maxCombo)
        {
            UpdateCombo(playerName, currentCombo, maxCombo);
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
                Debug.LogError("[MusicBadgeAdapter] GlobalBadgeSystem non trouvé!");
            }

            if (globalBadgeTracker == null)
            {
                Debug.LogError("[MusicBadgeAdapter] GlobalBadgeTracker non trouvé!");
            }

            if (globalBadgeSystem != null && globalBadgeTracker != null)
            {
                LogDebug("MusicBadgeAdapter initialisé avec succès");
            }
        }

        /// <summary>
        /// Log de debug
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[MusicBadgeAdapter] {message}");
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
            var adapter = FindFirstObjectByType<MusicBadgeAdapter>();
            adapter?.UpdatePlayerScore(playerName, score);
        }

        /// <summary>
        /// Raccourci statique pour une note réussie
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <param name="noteType">Type de note</param>
        /// <param name="timing">Qualité du timing</param>
        public static void HitNote(string playerName, NoteType noteType = NoteType.Normal, NoteTiming timing = NoteTiming.Good)
        {
            var adapter = FindFirstObjectByType<MusicBadgeAdapter>();
            if (adapter != null)
            {
                adapter.IncrementNotesHit(playerName);
                adapter.HandleSpecialNote(playerName, noteType, timing);
            }
        }

        /// <summary>
        /// Raccourci statique pour valider les badges
        /// </summary>
        /// <param name="playerName">Nom du joueur</param>
        /// <returns>Nombre de badges attribués</returns>
        public static int ValidateBadges(string playerName)
        {
            var adapter = FindFirstObjectByType<MusicBadgeAdapter>();
            return adapter?.ValidatePlayerBadges(playerName) ?? 0;
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Types de notes musicales
        /// </summary>
        public enum NoteType
        {
            Normal,
            Hold,
            Slide,
            Tap,
            Special
        }

        /// <summary>
        /// Qualité du timing d'une note
        /// </summary>
        public enum NoteTiming
        {
            Miss,
            Good,
            Great,
            Perfect
        }

        /// <summary>
        /// Structure pour représenter le résultat d'une chanson
        /// </summary>
        [System.Serializable]
        public class MusicGameResult
        {
            public float finalScore;
            public float songDuration;
            public int notesHit;
            public int notesMissed;
            public float accuracy;
            public int maxCombo;
            public int perfectNotes;
            public float songBPM;
            public int difficulty;
            public string songName;

            public MusicGameResult(float score, float duration, int hit, int missed, float acc, int combo, int perfect, float bpm, int diff, string name)
            {
                finalScore = score;
                songDuration = duration;
                notesHit = hit;
                notesMissed = missed;
                accuracy = acc;
                maxCombo = combo;
                perfectNotes = perfect;
                songBPM = bpm;
                difficulty = diff;
                songName = name;
            }
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// Teste l'adaptateur avec des données fictives
        /// </summary>
        [ContextMenu("Test Music Adapter")]
        public void TestAdapter()
        {
            string testPlayer = "TestMusicPlayer";
            
            // Test des métriques
            UpdatePlayerScore(testPlayer, 85000f);
            UpdateSongTime(testPlayer, 180f);
            IncrementNotesHit(testPlayer, 200);
            IncrementNotesMissed(testPlayer, 5);
            UpdatePlayerAccuracy(testPlayer, 96f);
            UpdateCombo(testPlayer, 150, 200);
            RegisterPerfectNote(testPlayer);
            UpdateSongBPM(testPlayer, 140f);
            UpdateSongDifficulty(testPlayer, 4);
            CompleteSong(testPlayer, "Test Song");
            
            // Test de validation
            int badges = ValidatePlayerBadges(testPlayer);
            
            Debug.Log($"Test Music terminé: {badges} badge(s) attribué(s)");
        }

        /// <summary>
        /// Simule une chanson complète
        /// </summary>
        [ContextMenu("Simulate Full Song")]
        public void SimulateFullSong()
        {
            string testPlayer = "SimulatedMusicPlayer";
            
            var songResult = new MusicGameResult(
                score: 98500f,
                duration: 240f,
                hit: 450,
                missed: 8,
                acc: 98.2f,
                combo: 380,
                perfect: 200,
                bpm: 160f,
                diff: 5,
                name: "Expert Test Song"
            );
            
            UpdateSongResult(testPlayer, songResult);
            
            Debug.Log("Simulation de chanson complète terminée");
        }
#endif

        #endregion
    }
}
