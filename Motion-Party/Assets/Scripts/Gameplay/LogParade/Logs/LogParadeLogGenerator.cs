using UnityEngine;
using System.Collections;

/// <summary>
/// Générateur de rondins pour le mini-jeu LogParade.
/// Orchestre la génération de patterns et le cycle de vie des rondins.
/// Optimisé pour un public senior avec vitesse lente et espacement confortable.
/// </summary>
public class LogParadeLogGenerator : MiniGameBase
{
    [Header("Configuration des Voies")]
    [SerializeField] private Transform[] lanes = new Transform[4];
    
    [Header("Prefabs de Rondins")]
    [SerializeField] private GameObject[] logPrefabs = new GameObject[3];
    
    [Header("Paramètres de Génération")]
    [SerializeField] private float logSpeed = 1.2f; // Vitesse de descente (unités/seconde)
    [SerializeField] private float verticalSpacing = 2.5f; // Espacement vertical minimum
    [SerializeField] private float generationInterval = 2.5f; // Fréquence de génération (secondes)
    [SerializeField] private float spawnHeight = 10f; // Hauteur de spawn
    [SerializeField] private float destroyHeight = -5f; // Hauteur de destruction des rondins
    
    [Header("Contrôles")]
    [SerializeField] private bool enableGeneration = true;
    [SerializeField] private bool showDebugInfo = false;
    
    // Modules de génération
    private LogParadeLogPatternGenerator patternGenerator;
    private LogParadeLogLifecycleManager lifecycleManager;
    private LogParadeLogConfiguration configuration;
    
    // Coroutine de génération
    private Coroutine generationCoroutine;

    #region Properties
    public int ActiveLogCount => lifecycleManager?.ActiveLogCount ?? 0;
    public bool HasActiveLogs => lifecycleManager?.HasActiveLogs ?? false;
    #endregion

    #region Unity Lifecycle    // Pour le debug - démarrage automatique en mode test
    private void Start()
    {        LogParadeLogger.LogVerbose("Start() appelé");
        
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
    }    protected override void Launch()
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
            if (stats != null)
            {
                Debug.Log($"[LogParadeLogGenerator] Rondins actifs: {stats.totalActiveLogs}, Voies occupées: {stats.lanesWithLogs}");
            }
        }
    }    private void OnDisable()
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
        Debug.Log("[LogParadeLogGenerator] Initialisation du générateur...");

        // Auto-création de la configuration si nécessaire
        configuration = GetComponent<LogParadeLogConfiguration>();
        if (configuration == null)
        {
            configuration = gameObject.AddComponent<LogParadeLogConfiguration>();
        }

        // Copier nos paramètres vers la configuration
        CopyParametersToConfiguration();

        // Validation de la configuration
        if (!configuration.ValidateConfiguration())
        {
            Debug.LogWarning("[LogParadeLogGenerator] Configuration invalide - tentative d'auto-correction...");
            AutoCorrectConfiguration();
        }

        // Initialisation des modules
        patternGenerator = new LogParadeLogPatternGenerator();
        lifecycleManager = new LogParadeLogLifecycleManager(lanes, logPrefabs, configuration);

        // Configuration des événements
        SetupEventHandlers();

        Debug.Log("[LogParadeLogGenerator] Générateur initialisé avec succès");
        Debug.Log($"[LogParadeLogGenerator] Paramètres: Speed={logSpeed}, Interval={generationInterval}, EnableGeneration={enableGeneration}");
    }    private void SetupConfigurationParameters()
    {
        // Cette méthode copie les paramètres du LogGenerator vers la LogConfiguration
        Debug.Log("[LogParadeLogGenerator] Configuration des paramètres...");
    }
    
    /// <summary>
    /// Copie nos paramètres vers la configuration
    /// </summary>
    private void CopyParametersToConfiguration()
    {
        if (configuration != null)
        {
            // Utiliser la réflexion pour copier les paramètres
            var configType = typeof(LogParadeLogConfiguration);
            
            // Copier les lanes
            var lanesField = configType.GetField("lanes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (lanesField != null)
                lanesField.SetValue(configuration, lanes);
                
            // Copier les prefabs
            var prefabsField = configType.GetField("logPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prefabsField != null)
                prefabsField.SetValue(configuration, logPrefabs);
                
            Debug.Log("[LogParadeLogGenerator] Paramètres copiés vers la configuration");
        }
    }
    
    /// <summary>
    /// Tente de corriger automatiquement la configuration
    /// </summary>
    private void AutoCorrectConfiguration()
    {
        Debug.Log("[LogParadeLogGenerator] Tentative d'auto-correction...");
        
        // Si les lanes ne sont pas assignées, les chercher dans la scène
        if (lanes == null || lanes.Length != 4 || System.Array.Exists(lanes, l => l == null))
        {
            AutoFindLanes();
        }
        
        // Si les prefabs ne sont pas assignés, créer des cubes par défaut
        if (logPrefabs == null || logPrefabs.Length != 3 || System.Array.Exists(logPrefabs, p => p == null))
        {
            CreateDefaultLogPrefabs();
        }
        
        // Recopier les paramètres
        CopyParametersToConfiguration();
        
        Debug.Log("[LogParadeLogGenerator] Auto-correction terminée");
    }
    
    /// <summary>
    /// Cherche automatiquement les lanes dans la scène
    /// </summary>
    private void AutoFindLanes()
    {
        // Chercher par tag
        GameObject[] laneObjects = GameObject.FindGameObjectsWithTag("Lane");
        if (laneObjects.Length >= 4)
        {
            lanes = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                lanes[i] = laneObjects[i].transform;
            }
            Debug.Log($"[LogParadeLogGenerator] {laneObjects.Length} lanes trouvées par tag");
            return;
        }
        
        // Créer des lanes par défaut
        CreateDefaultLanes();
    }
    
    /// <summary>
    /// Crée des lanes par défaut
    /// </summary>
    private void CreateDefaultLanes()
    {
        Debug.Log("[LogParadeLogGenerator] Création de lanes par défaut...");
        
        lanes = new Transform[4];
        float[] positions = { -3f, -1f, 1f, 3f };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject laneGO = new GameObject($"Lane_{i + 1}");
            laneGO.tag = "Lane";
            laneGO.transform.position = new Vector3(positions[i], 0f, 0f);
            lanes[i] = laneGO.transform;
        }
        
        Debug.Log("[LogParadeLogGenerator] 4 lanes créées par défaut");
    }
    
    /// <summary>
    /// Crée des prefabs de rondins par défaut
    /// </summary>
    private void CreateDefaultLogPrefabs()
    {
        Debug.Log("[LogParadeLogGenerator] Création de prefabs par défaut...");
        
        logPrefabs = new GameObject[3];
        
        for (int i = 0; i < 3; i++)
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prefab.name = $"DefaultLogPrefab_{i + 1}";
            prefab.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
            
            // Ajouter un Rigidbody pour la physique
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            if (rb == null)
                rb = prefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            
            logPrefabs[i] = prefab;
        }
        
        Debug.Log("[LogParadeLogGenerator] 3 prefabs créés par défaut");
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
    private void StartGeneration()
    {
        Debug.Log("[LogParadeLogGenerator] StartGeneration() appelé");
        
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
        }
        
        if (!enableGeneration)
        {
            Debug.LogWarning("[LogParadeLogGenerator] Génération désactivée (enableGeneration = false)");
            return;
        }
        
        Debug.Log("[LogParadeLogGenerator] Démarrage de la coroutine de génération");
        generationCoroutine = StartCoroutine(GenerationLoop());
    }

    private IEnumerator GenerationLoop()
    {
        Debug.Log("[LogParadeLogGenerator] Boucle de génération démarrée");
        
        while (enableGeneration)
        {
            Debug.Log("[LogParadeLogGenerator] Génération d'une nouvelle rangée...");
            
            // Génère une nouvelle rangée de rondins
            GenerateLogRow();
            
            // Attend la prochaine génération
            yield return new WaitForSeconds(generationInterval);
        }
        
        Debug.Log("[LogParadeLogGenerator] Boucle de génération terminée");
    }

    private void GenerateLogRow()
    {
        // Vérifier que la calibration est terminée avant de générer des rondins
        if (LogParadeGameStateController.IsCalibrationInProgress)
        {
            if (showDebugInfo)
            {
                Debug.Log("[LogParadeLogGenerator] Génération suspendue - Calibration en cours");
            }
            return;
        }

        if (patternGenerator == null || lifecycleManager == null)
        {
            Debug.LogWarning("[LogParadeLogGenerator] Modules non initialisés");
            return;
        }

        LogRow newRow = patternGenerator.GenerateValidLogRow();
        
        if (newRow != null)
        {
            // Spawn des rondins via le lifecycle manager avec délai pour éviter la superposition
            StartCoroutine(lifecycleManager.SpawnLogsFromRowCoroutine(newRow, 0.1f));

            if (showDebugInfo)
            {
                Debug.Log($"[LogParadeLogGenerator] {newRow.GetPatternString()}");
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
        Debug.Log("[LogParadeLogGenerator] StartLogGeneration() appelé (contrôle externe)");
        StartGeneration();
    }
    #endregion

    #region Event Handlers
    private void OnPatternGenerated(LogRow row)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[LogParadeLogGenerator] Pattern généré: {row.GetPatternString()}");
        }
    }

    private void OnPatternGenerationFailed(string reason)
    {
        Debug.LogWarning($"[LogParadeLogGenerator] Échec génération pattern: {reason}");
    }

    private void OnLogSpawned(LogParadeLog log)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[LogParadeLogGenerator] Rondin spawné: {log.name}");
        }
    }

    private void OnLogDestroyed(LogParadeLog log)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[LogParadeLogGenerator] Rondin détruit: {log?.name ?? "null"}");
        }
    }

    private void OnActiveLogCountChanged(int newCount)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[LogParadeLogGenerator] Nombre de rondins actifs: {newCount}");
        }
    }
    #endregion

    #region Debug/Test Methods
    /// <summary>
    /// Méthode de test pour spawn immédiat - Accessible via menu contextuel
    /// </summary>
    [ContextMenu("Test Spawn Immédiat")]
    public void TestSpawnImmediate()
    {
        Debug.Log("[LogParadeLogGenerator] Test spawn immédiat...");
        
        if (lifecycleManager == null)
        {
            Debug.LogError("[LogParadeLogGenerator] LifecycleManager non initialisé pour le test !");
            InitializeGenerator();
            if (lifecycleManager == null) return;
        }
        
        // Spawn un rondin sur chaque voie pour test
        for (int i = 0; i < 4; i++)
        {
            lifecycleManager.SpawnLogInLane(i);
        }
    }
    
    /// <summary>
    /// Force une génération de rangée immédiate - Pour debug
    /// </summary>
    [ContextMenu("Forcer Génération Rangée")]
    public void ForceGenerateRow()
    {
        Debug.Log("[LogParadeLogGenerator] Force génération rangée...");
        GenerateLogRow();
    }
    #endregion

    #region Debug Gizmos
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Positions X fixes des voies
        float[] lanePositions = { -3f, -1f, 1f, 3f };
        
        // Dessine les voies
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 4; i++)
        {
            Vector3 startPos = new Vector3(lanePositions[i], -2f, 0f);
            Vector3 endPos = new Vector3(lanePositions[i], 12f, 0f);
            Gizmos.DrawLine(startPos, endPos);
        }
        
        // Dessine la ligne de spawn
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-4f, spawnHeight, 0f), new Vector3(4f, spawnHeight, 0f));
        
        // Dessine la ligne de destruction
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-4f, destroyHeight, 0f), new Vector3(4f, destroyHeight, 0f));
    }
    #endif
    #endregion
}
