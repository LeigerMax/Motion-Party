using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Générateur de rondins pour le mini-jeu LogParade.
/// Génère des rondins en descente sur 4 voies avec maintien d'un chemin jouable.
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
    
    // Positions X fixes des voies
    private readonly float[] lanePositions = { -3f, -1f, 1f, 3f };
    
    // Système de buffer pour validation des chemins
    private Queue<LogRow> upcomingRows = new Queue<LogRow>();
    private List<LogParadeLog> activeLogs = new List<LogParadeLog>();
    
    // Coroutine de génération
    private Coroutine generationCoroutine;
    
    // Classe interne pour représenter une rangée de rondins
    [System.Serializable]
    private class LogRow
    {
        public bool[] hasLog = new bool[4]; // true si un rondin est présent sur cette voie
        public float spawnTime;
        
        public LogRow()
        {
            hasLog = new bool[4];
        }
        
        public bool IsValidPath()
        {
            // Vérifie qu'il y a au moins un rondin et que les rondins adjacents permettent le passage
            int logCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (hasLog[i]) logCount++;
            }
            
            // Au moins un rondin requis
            if (logCount == 0) return false;
            
            // Vérifie la connectivité - au moins une séquence de rondins adjacents
            bool hasConnectedSequence = false;
            for (int i = 0; i < 4; i++)
            {
                if (hasLog[i])
                {
                    // Vérifie si ce rondin peut être atteint depuis un rondin adjacent
                    bool canReach = true;
                    if (i > 0 && hasLog[i - 1]) canReach = true;
                    else if (i < 3 && hasLog[i + 1]) canReach = true;
                    else if (logCount == 1) canReach = true; // Rondin isolé mais accessible
                    
                    if (canReach) hasConnectedSequence = true;
                }
            }
            
            return hasConnectedSequence;        }
    }
    
    // Pour le debug - démarrage automatique en mode test
    private void Start()
    {
        Debug.Log("[LogParadeLogGenerator] Start() appelé");
        
        // Si nous ne sommes pas lancés par MiniGameBase, on démarre automatiquement
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("[LogParadeLogGenerator] GameObject inactif");
            return;
        }
        
        // Démarrage automatique pour les tests
        Debug.Log("[LogParadeLogGenerator] Démarrage automatique pour test");
        Launch();
    }
    
    protected override void Launch()
    {
        Debug.Log("[LogParadeLogGenerator] Launch() appelé");
        InitializeGenerator();
        StartGeneration();
    }
      private void InitializeGenerator()
    {
        Debug.Log("[LogParadeLogGenerator] Initialisation du générateur...");
        
        // Vérification des références requises
        if (lanes.Length != 4)
        {
            Debug.LogError("[LogParadeLogGenerator] Exactement 4 voies sont requises !");
            return;
        }
        
        if (logPrefabs.Length != 3)
        {
            Debug.LogError("[LogParadeLogGenerator] Exactement 3 prefabs de rondins sont requis !");
            return;
        }
        
        // Vérification que les voies sont assignées
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == null)
            {
                Debug.LogError($"[LogParadeLogGenerator] La voie {i + 1} n'est pas assignée !");
                return;
            }
            else
            {
                Debug.Log($"[LogParadeLogGenerator] Voie {i + 1} assignée : {lanes[i].name}");
            }
        }
        
        // Vérification que les prefabs sont assignés
        for (int i = 0; i < logPrefabs.Length; i++)
        {
            if (logPrefabs[i] == null)
            {
                Debug.LogError($"[LogParadeLogGenerator] Le prefab de rondin {i + 1} n'est pas assigné !");
                return;
            }
            else
            {
                Debug.Log($"[LogParadeLogGenerator] Prefab {i + 1} assigné : {logPrefabs[i].name}");
            }
        }
        
        // Positionnement des voies selon les spécifications
        for (int i = 0; i < lanes.Length; i++)
        {
            Vector3 lanePos = lanes[i].position;
            lanePos.x = lanePositions[i];
            lanes[i].position = lanePos;
        }
          // Initialisation des listes
        upcomingRows.Clear();
        activeLogs.Clear();
        
        Debug.Log("[LogParadeLogGenerator] Générateur initialisé avec succès");
        Debug.Log($"[LogParadeLogGenerator] Paramètres: Speed={logSpeed}, Interval={generationInterval}, EnableGeneration={enableGeneration}");
    }
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
        LogRow newRow = CreateValidLogRow();
        
        if (newRow != null)
        {
            // Ajoute au buffer
            upcomingRows.Enqueue(newRow);
            
            // Spawn immédiat des rondins
            SpawnLogsFromRow(newRow);
            
            if (showDebugInfo)
            {
                string rowInfo = "Rangée générée: ";
                for (int i = 0; i < 4; i++)
                {
                    rowInfo += newRow.hasLog[i] ? "■ " : "□ ";
                }
                Debug.Log($"[LogParadeLogGenerator] {rowInfo}");
            }
        }
    }
    
    private LogRow CreateValidLogRow()
    {
        LogRow row = new LogRow();
        row.spawnTime = Time.time;
        
        // Génère entre 1 et 3 rondins par rangée (jamais 4 pour laisser de la place)
        int logCount = Random.Range(1, 4);
        
        // Essaie plusieurs configurations jusqu'à trouver une valide
        int attempts = 0;
        int maxAttempts = 20;
        
        while (attempts < maxAttempts)
        {
            // Reset de la rangée
            for (int i = 0; i < 4; i++)
                row.hasLog[i] = false;
            
            // Place les rondins aléatoirement
            List<int> availableLanes = new List<int> { 0, 1, 2, 3 };
            
            for (int i = 0; i < logCount; i++)
            {
                if (availableLanes.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableLanes.Count);
                    int laneIndex = availableLanes[randomIndex];
                    row.hasLog[laneIndex] = true;
                    availableLanes.RemoveAt(randomIndex);
                }
            }
            
            // Vérifie si cette configuration est valide
            if (row.IsValidPath())
            {
                return row;
            }
            
            attempts++;
        }
        
        // Si aucune configuration valide trouvée, crée une configuration de secours
        row = new LogRow();
        row.hasLog[1] = true; // Rondin sur voie centrale gauche
        row.hasLog[2] = true; // Rondin sur voie centrale droite
        row.spawnTime = Time.time;
        
        if (showDebugInfo)
        {
            Debug.LogWarning("[LogParadeLogGenerator] Configuration de secours utilisée");
        }
        
        return row;
    }
    
    private void SpawnLogsFromRow(LogRow row)
    {
        for (int i = 0; i < 4; i++)
        {
            if (row.hasLog[i])
            {
                SpawnLog(i);
            }
        }
    }
      private void SpawnLog(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) 
        {
            Debug.LogError($"[LogParadeLogGenerator] Index de voie invalide : {laneIndex}");
            return;
        }
        
        // Sélection aléatoire d'un prefab
        GameObject prefabToSpawn = logPrefabs[Random.Range(0, logPrefabs.Length)];
        
        // Position de spawn
        Vector3 spawnPosition = new Vector3(
            lanePositions[laneIndex],
            spawnHeight,
            lanes[laneIndex].position.z
        );
        
        Debug.Log($"[LogParadeLogGenerator] Spawn rondin sur voie {laneIndex + 1} à la position {spawnPosition}");
        
        // Instanciation du rondin
        GameObject logObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Ajout du composant LogParadeLog
        LogParadeLog logComponent = logObject.GetComponent<LogParadeLog>();
        if (logComponent == null)
        {
            logComponent = logObject.AddComponent<LogParadeLog>();
        }
        
        // Configuration du rondin
        logComponent.Initialize(logSpeed, destroyHeight, laneIndex);
        
        // Ajout à la liste des rondins actifs
        activeLogs.Add(logComponent);
        
        // Callback quand le rondin est détruit
        logComponent.OnDestroyed += () => {
            activeLogs.Remove(logComponent);
        };
        
        Debug.Log($"[LogParadeLogGenerator] Rondin créé avec succès : {logObject.name}");
    }
    
    private void Update()
    {
        // Nettoyage des rondins détruits de la liste
        activeLogs.RemoveAll(log => log == null);
        
        // Debug info
        if (showDebugInfo && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[LogParadeLogGenerator] Rondins actifs: {activeLogs.Count}, Buffer: {upcomingRows.Count}");
        }
    }
    
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
        foreach (var log in activeLogs)
        {
            if (log != null)
            {
                log.DestroyLog();
            }
        }
        
        activeLogs.Clear();
        upcomingRows.Clear();
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
    
    /// <summary>
    /// Méthode de test pour spawn immédiat - Accessible via menu contextuel
    /// </summary>
    [ContextMenu("Test Spawn Immédiat")]
    public void TestSpawnImmediate()
    {
        Debug.Log("[LogParadeLogGenerator] Test spawn immédiat...");
        
        if (logPrefabs.Length == 0 || logPrefabs[0] == null)
        {
            Debug.LogError("[LogParadeLogGenerator] Aucun prefab assigné pour le test !");
            return;
        }
        
        if (lanes.Length == 0 || lanes[0] == null)
        {
            Debug.LogError("[LogParadeLogGenerator] Aucune voie assignée pour le test !");
            return;
        }
        
        // Spawn un rondin sur chaque voie pour test
        for (int i = 0; i < 4 && i < lanes.Length; i++)
        {
            SpawnLog(i);
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
    
    // Méthodes pour le debug et les tests
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
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
}
