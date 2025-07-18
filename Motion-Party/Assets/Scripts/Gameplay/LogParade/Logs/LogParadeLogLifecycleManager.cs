using UnityEngine;
using System.Collections.Generic;
using System.Collections;


namespace Gameplay.LogParade.Logs
{
    /// <summary>
    /// Gestionnaire du cycle de vie des rondins dans LogParade.
    /// Responsable du spawn, du suivi, et de la destruction des rondins actifs.
    /// Optimisé pour les performances avec un système de pooling et de nettoyage automatique.
    /// </summary>
    public class LogParadeLogLifecycleManager
    {
        #region Configuration
        private readonly float[] lanePositions = { -3f, -1f, 1f, 3f };
        private readonly int laneCount = 4;
        #endregion

        #region Dependencies
        private readonly Transform[] lanes;
        private readonly GameObject[] logPrefabs;
        private readonly LogParadeLogConfiguration config;
        #endregion

        #region State
        private List<LogParadeLog> activeLogs = new List<LogParadeLog>();
        private Queue<LogRow> upcomingRows = new Queue<LogRow>();
        private Dictionary<int, List<LogParadeLog>> logsByLane = new Dictionary<int, List<LogParadeLog>>();
        #endregion

        #region Events
        public System.Action<LogParadeLog> OnLogSpawned;
        public System.Action<LogParadeLog> OnLogDestroyed;
        public System.Action<int> OnActiveLogCountChanged;
        #endregion

        #region Properties
        public int ActiveLogCount => activeLogs.Count;
        public List<LogParadeLog> ActiveLogs => new List<LogParadeLog>(activeLogs);
        public bool HasActiveLogs => activeLogs.Count > 0;
        #endregion

        #region Constructor
        public LogParadeLogLifecycleManager(Transform[] lanes, GameObject[] logPrefabs, LogParadeLogConfiguration config)
        {
            this.lanes = lanes ?? throw new System.ArgumentNullException(nameof(lanes));
            this.logPrefabs = logPrefabs ?? throw new System.ArgumentNullException(nameof(logPrefabs));
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));

            InitializeLaneTracking();

            if (!ValidateConfiguration())
            {
                throw new System.InvalidOperationException("Configuration invalide pour LogLifecycleManager");
            }
        }
        #endregion

        #region Initialization
        private void InitializeLaneTracking()
        {
            logsByLane.Clear();
            for (int i = 0; i < laneCount; i++)
            {
                logsByLane[i] = new List<LogParadeLog>();
            }
        }

        private bool ValidateConfiguration()
        {
            if (lanes.Length != laneCount)
            {
                Debug.LogError($"[LogLifecycleManager] {laneCount} voies requises, {lanes.Length} fournies");
                return false;
            }

            if (logPrefabs.Length == 0)
            {
                Debug.LogError("[LogLifecycleManager] Aucun prefab de rondin fourni");
                return false;
            }

            for (int i = 0; i < lanes.Length; i++)
            {
                if (lanes[i] == null)
                {
                    Debug.LogError($"[LogLifecycleManager] Voie {i} non assignée");
                    return false;
                }
            }

            for (int i = 0; i < logPrefabs.Length; i++)
            {
                if (logPrefabs[i] == null)
                {
                    Debug.LogError($"[LogLifecycleManager] Prefab {i} non assigné");
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Log Spawning
        /// <summary>
        /// Spawn les rondins d'une rangée complète avec un délai entre chaque log pour éviter la superposition.
        /// </summary>
        public IEnumerator SpawnLogsFromRowCoroutine(LogRow row, float delayBetweenLogs = 0.1f)
        {
            if (row == null)
            {
                Debug.LogWarning("[LogLifecycleManager] Tentative de spawn d'une rangée null");
                yield break;
            }

            upcomingRows.Enqueue(row);

            for (int i = 0; i < laneCount; i++)
            {
                if (row.hasLog[i])
                {
                    SpawnLogInLane(i);
                    yield return new WaitForSeconds(delayBetweenLogs);
                }
            }
        }

        /// <summary>
        /// Spawn un rondin dans une voie spécifique.
        /// </summary>
        public LogParadeLog SpawnLogInLane(int laneIndex)
        {
            if (!IsValidLaneIndex(laneIndex))
            {
                Debug.LogError($"[LogLifecycleManager] Index de voie invalide : {laneIndex}");
                return null;
            }

            // Sélection aléatoire d'un prefab
            int prefabIndex = Random.Range(0, logPrefabs.Length);
            GameObject prefabToSpawn = logPrefabs[prefabIndex];

            // Déterminer la taille du rondin (Short, Medium, Long)
            float logLength = 100f; // Short par défaut
            if (prefabIndex == 1) logLength = 200f; // Medium
            else if (prefabIndex == 2) logLength = 300f; // Long

            // Position de spawn : lane sur X, hauteur sur Y, spawn sur Z = 23
            Vector3 spawnPosition = new Vector3(
                lanePositions[laneIndex],
                lanes[laneIndex].position.y, 
                23f // Z = spawn à 23
            );

            // Rotation des logs : x=0, y=90, z=0
            Quaternion logRotation = Quaternion.Euler(90f, 90f, 0f);

            // Instanciation avec la rotation personnalisée
            GameObject logObject = Object.Instantiate(prefabToSpawn, spawnPosition, logRotation);
            // Appliquer l'échelle selon le type
            logObject.transform.localScale = new Vector3(logLength, logObject.transform.localScale.y, logObject.transform.localScale.z);
            
            // Configuration du composant LogParadeLog
            LogParadeLog logComponent = SetupLogComponent(logObject, laneIndex);

            // Enregistrement
            RegisterLog(logComponent, laneIndex);

            OnLogSpawned?.Invoke(logComponent);
            OnActiveLogCountChanged?.Invoke(activeLogs.Count);

            return logComponent;
        }


        private LogParadeLog SetupLogComponent(GameObject logObject, int laneIndex)
        {
            LogParadeLog logComponent = logObject.GetComponent<LogParadeLog>();
            if (logComponent == null)
            {
                logComponent = logObject.AddComponent<LogParadeLog>();
            }

            // Configuration du rondin
            logComponent.Initialize(config.LogSpeed, laneIndex);

            // Callback de destruction
            logComponent.OnDestroyed += () => OnLogDestroyedCallback(logComponent, laneIndex);

            return logComponent;
        }
        #endregion

        #region Log Management
        private void RegisterLog(LogParadeLog log, int laneIndex)
        {
            activeLogs.Add(log);
            logsByLane[laneIndex].Add(log);
        }

        private void OnLogDestroyedCallback(LogParadeLog log, int laneIndex)
        {
            UnregisterLog(log, laneIndex);
            OnLogDestroyed?.Invoke(log);
            OnActiveLogCountChanged?.Invoke(activeLogs.Count);
        }

        private void UnregisterLog(LogParadeLog log, int laneIndex)
        {
            activeLogs.Remove(log);
            if (logsByLane.ContainsKey(laneIndex))
            {
                logsByLane[laneIndex].Remove(log);
            }
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Détruit tous les rondins actifs.
        /// </summary>
        public void ClearAllLogs()
        {
            // Créer une copie pour éviter l'exception de modification pendant l'énumération
            var logsToDestroy = new List<LogParadeLog>(activeLogs);

            foreach (var log in logsToDestroy)
            {
                if (log != null)
                {
                    log.DestroyLog();
                }
            }

            // Nettoyage des collections
            activeLogs.Clear();
            foreach (var laneList in logsByLane.Values)
            {
                laneList.Clear();
            }
            upcomingRows.Clear();

            OnActiveLogCountChanged?.Invoke(0);
        }

        /// <summary>
        /// Nettoie les références nulles dans les listes.
        /// </summary>
        public void CleanupNullReferences()
        {
            int removedCount = 0;

            // Nettoyage de la liste principale
            removedCount += activeLogs.RemoveAll(log => log == null);

            // Nettoyage des listes par voie
            foreach (var kvp in logsByLane)
            {
                removedCount += kvp.Value.RemoveAll(log => log == null);
            }

            if (removedCount > 0)
            {
                OnActiveLogCountChanged?.Invoke(activeLogs.Count);
            }
        }

        #endregion

        #region Query Methods
        /// <summary>
        /// Retourne tous les rondins d'une voie spécifique.
        /// </summary>
        public List<LogParadeLog> GetLogsInLane(int laneIndex)
        {
            if (!IsValidLaneIndex(laneIndex) || !logsByLane.ContainsKey(laneIndex))
            {
                return new List<LogParadeLog>();
            }

            return new List<LogParadeLog>(logsByLane[laneIndex]);
        }

        /// <summary>
        /// Compte les rondins actifs par voie.
        /// </summary>
        public Dictionary<int, int> GetLogCountByLane()
        {
            var counts = new Dictionary<int, int>();
            foreach (var kvp in logsByLane)
            {
                counts[kvp.Key] = kvp.Value.Count;
            }
            return counts;
        }
        #endregion

        #region Utilities
        private bool IsValidLaneIndex(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < laneCount;
        }

        /// <summary>
        /// Génère des statistiques sur les rondins actifs.
        /// </summary>
        public LogLifecycleStats GenerateStats()
        {
            var stats = new LogLifecycleStats();
            stats.totalActiveLogs = activeLogs.Count;
            stats.logCountByLane = GetLogCountByLane();
            stats.upcomingRowsCount = upcomingRows.Count;

            // Calcul de la répartition par voie
            foreach (var count in stats.logCountByLane.Values)
            {
                if (count > 0)
                {
                    stats.lanesWithLogs++;
                }
            }

            return stats;
        }
        #endregion
    }

    #region Data Structures
    /// <summary>
    /// Statistiques du cycle de vie des rondins.
    /// </summary>
    [System.Serializable]
    public class LogLifecycleStats
    {
        public int totalActiveLogs;
        public Dictionary<int, int> logCountByLane = new Dictionary<int, int>();
        public int upcomingRowsCount;
        public int lanesWithLogs;
    }
    #endregion
}