using UnityEngine;
using System.Collections;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Core;

namespace Gameplay.LogParade.Logs
{

    /// <summary>
    /// Générateur de rondins pour le mini-jeu LogParade.
    /// Orchestre la génération de patterns et le cycle de vie des rondins.
    /// </summary>
    public class LogParadeLogGenerator : MiniGameBase
    {
        #region Fields

        [Header("Contrôles")]
        [SerializeField] private bool enableGeneration = true;
        [SerializeField] private bool showDebugInfo = false;
        private LogParadeLogPatternGenerator patternGenerator;
        private LogParadeLogLifecycleManager lifecycleManager;
        private LogParadeLogConfiguration configuration;
        private Coroutine generationCoroutine;
        #endregion

        #region Properties
        public int ActiveLogCount => lifecycleManager?.ActiveLogCount ?? 0;
        public bool HasActiveLogs => lifecycleManager?.HasActiveLogs ?? false;
        #endregion

        #region Unity Lifecycle
        // Pour le debug - démarrage automatique en mode test
        private void Start()
        {
            LogParadeLogger.LogVerbose("Start() appelé");

            // Si nous ne sommes pas lancés par MiniGameBase, on démarre automatiquement
            if (!gameObject.activeInHierarchy)
            {
                LogParadeLogger.LogVerbose("GameObject inactif");
                return;
            }

            // Initialisation seulement, pas de démarrage automatique
            // Le démarrage sera contrôlé par LogParadeGameTimer
            LogParadeLogger.LogVerbose("Initialisation sans démarrage automatique");
            InitializeGenerator();
        }   
        
        protected override void Launch()
        {
            LogParadeLogger.Log("Launch() appelé");
            InitializeGenerator();
            StartGeneration();
        }

        private void Update()
        {
            // Nettoyage automatique des références nulles
            lifecycleManager?.CleanupNullReferences();
            
            // Debug info
            if (showDebugInfo && Input.GetKeyDown(KeyCode.Space))
            {
                var stats = lifecycleManager?.GenerateStats();
            }
        }

        private void OnDisable()
        {
            StopGeneration();
        }

        private void OnDestroy()
        {
            StopGeneration();
            ClearAllLogs();
        }
        
        #endregion

        #region Initialization      
        private void InitializeGenerator()
        {

            // Récupération ou création de la configuration centralisée
            configuration = GetComponent<LogParadeLogConfiguration>();
            if (configuration == null)
            {
                configuration = gameObject.AddComponent<LogParadeLogConfiguration>();
            }


            // Initialisation des modules
            patternGenerator = new LogParadeLogPatternGenerator();
            lifecycleManager = new LogParadeLogLifecycleManager(
                configuration.Lanes,
                configuration.LogPrefabs,
                configuration
            );

            // Configuration des événements
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            if (patternGenerator != null)
            {
                patternGenerator.OnPatternGenerated += OnPatternGenerated;
                patternGenerator.OnPatternGenerationFailed += OnPatternGenerationFailed;
            }

            if (lifecycleManager != null)
            {
                lifecycleManager.OnLogSpawned += OnLogSpawned;
                lifecycleManager.OnLogDestroyed += OnLogDestroyed;
                lifecycleManager.OnActiveLogCountChanged += OnActiveLogCountChanged;
            }
        }
        #endregion

        #region Generation Control
        /// <summary>
        /// Démarre la génération des rondins
        /// </summary>
        public void StartGeneration()
        {
            if (!enableGeneration)
            {
                LogParadeLogger.LogWarning("Génération désactivée");
                return;
            }

            if (generationCoroutine != null)
            {
                StopCoroutine(generationCoroutine);
            }

            generationCoroutine = StartCoroutine(GenerationLoop());
            LogParadeLogger.LogVerbose("Génération démarrée");
        }

        private IEnumerator GenerationLoop()
        {
            while (enableGeneration)
            {
                GenerateLogRow();
                yield return new WaitForSeconds(configuration.GenerationInterval);
            }
        }

        private void GenerateLogRow()
        {
            if (LogParadeGameStateController.IsCalibrationInProgress)
            {
                if (showDebugInfo)
                {
                    LogParadeLogger.LogVerbose("[LogParadeLogGenerator] Génération suspendue - Calibration en cours");
                }
                return;
            }
            if (patternGenerator == null || lifecycleManager == null)
            {
                LogParadeLogger.LogWarning("[LogParadeLogGenerator] Modules non initialisés");
                return;
            }
            LogRow newRow = patternGenerator.GenerateValidLogRow();
            if (newRow != null)
            {
                StartCoroutine(lifecycleManager.SpawnLogsFromRowCoroutine(newRow, 0.1f));
                if (showDebugInfo)
                {
                    LogParadeLogger.LogVerbose($"[LogParadeLogGenerator] {newRow.GetPatternString()}");
                }
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Arrête la génération de rondins
        /// </summary>
        public void StopGeneration()
        {
            enableGeneration = false;
            
            if (generationCoroutine != null)
            {
                StopCoroutine(generationCoroutine);
                generationCoroutine = null;
            }
        }
        
        /// <summary>
        /// Reprend la génération de rondins
        /// </summary>
        public void ResumeGeneration()
        {
            if (!enableGeneration)
            {
                enableGeneration = true;
                StartGeneration();
            }
        }
        
        /// <summary>
        /// Détruit tous les rondins actifs
        /// </summary>
        public void ClearAllLogs()
        {
            lifecycleManager?.ClearAllLogs();
        }

        /// <summary>
        /// Démarre la génération de rondins (méthode publique pour contrôle externe)
        /// </summary>
        public void StartLogGeneration()
        {
            LogParadeLogger.LogVerbose("[LogParadeLogGenerator] StartLogGeneration() appelé (contrôle externe)");
            StartGeneration();
        }
        #endregion

        #region Event Handlers
        private void OnPatternGenerated(LogRow row)
        {
            if (showDebugInfo)
            {
                LogParadeLogger.LogVerbose($"[LogParadeLogGenerator] Pattern généré: {row.GetPatternString()}");
            }
        }

        private void OnPatternGenerationFailed(string reason)
        {
            LogParadeLogger.LogWarning($"[LogParadeLogGenerator] Échec génération pattern: {reason}");
        }

        private void OnLogSpawned(LogParadeLog log)
        {
            if (showDebugInfo)
            {
                LogParadeLogger.LogVerbose($"[LogParadeLogGenerator] Rondin spawné: {log.name}");
            }
        }

        private void OnLogDestroyed(LogParadeLog log)
        {
            if (showDebugInfo)
            {
                LogParadeLogger.LogVerbose($"[LogParadeLogGenerator] Rondin détruit: {log?.name ?? "null"}");
            }
        }

        private void OnActiveLogCountChanged(int newCount)
        {
            if (showDebugInfo)
            {
                LogParadeLogger.LogVerbose($"[LogParadeLogGenerator] Nombre de rondins actifs: {newCount}");
            }
        }
        #endregion

    }
}