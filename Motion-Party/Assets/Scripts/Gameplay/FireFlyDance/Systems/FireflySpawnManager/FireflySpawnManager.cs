using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère le spawn automatique des lucioles dans la zone de jeu définie par FireflyDanceConfig
/// Respecte les limites de nombre maximum et la fréquence d'apparition
/// Gère automatiquement la suppression des lucioles après leur durée de vie
/// </summary>
public class FireflySpawnManager : MonoBehaviour
{
    #region Fields

    [Header("Configuration")]
    [SerializeField] private FireflyDanceConfig config;
    [SerializeField] private GameObject fireflyPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private bool showDebugGizmos = true;
    
    [Header("Runtime Info (Read Only)")]
    [SerializeField] private int activeFireflyCount = 0;
    [SerializeField] private bool isSpawning = false;

    // Collections pour gérer les lucioles actives
    private List<GameObject> activeFireflies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    private Coroutine cleanupCoroutine;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        InitializeSpawnManager();
    }

    void OnDestroy()
    {
        StopSpawning();
    }

    void OnDrawGizmos()
    {
        if (showDebugGizmos && config != null)
        {
            DrawSpawnAreaGizmos();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Démarre le spawn automatique des lucioles
    /// </summary>
    public void StartSpawning()
    {
        if (config == null || fireflyPrefab == null)
        {
            Debug.LogError("FireflySpawnManager: Configuration ou prefab manquant!", this);
            return;
        }

        if (isSpawning) return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
        cleanupCoroutine = StartCoroutine(CleanupRoutine());
        
        Debug.Log($"FireflySpawnManager: Spawn démarré - Max: {config.MaxFireflies}, Interval: {config.SpawnInterval}s");
    }

    /// <summary>
    /// Arrête le spawn automatique des lucioles
    /// </summary>
    public void StopSpawning()
    {
        if (!isSpawning) return;

        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        if (cleanupCoroutine != null)
        {
            StopCoroutine(cleanupCoroutine);
            cleanupCoroutine = null;
        }
        
        Debug.Log("FireflySpawnManager: Spawn arrêté");
    }

    /// <summary>
    /// Force le spawn d'une luciole si possible
    /// </summary>
    public void ForceSpawnFirefly()
    {
        if (CanSpawnFirefly())
        {
            SpawnFirefly();
        }
    }

    /// <summary>
    /// Supprime toutes les lucioles actives
    /// </summary>
    public void ClearAllFireflies()
    {
        foreach (GameObject firefly in activeFireflies)
        {
            if (firefly != null)
            {
                DestroyImmediate(firefly);
            }
        }
        
        activeFireflies.Clear();
        activeFireflyCount = 0;
        
        Debug.Log("FireflySpawnManager: Toutes les lucioles supprimées");
    }

    /// <summary>
    /// Retourne le nombre de lucioles actives
    /// </summary>
    public int GetActiveFireflyCount()
    {
        return activeFireflyCount;
    }

    /// <summary>
    /// Vérifie si le spawn manager est en cours d'exécution
    /// </summary>
    public bool IsSpawning()
    {
        return isSpawning;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialise le spawn manager au démarrage
    /// </summary>
    private void InitializeSpawnManager()
    {
        // Validation des références
        if (config == null)
        {
            Debug.LogError("FireflySpawnManager: FireflyDanceConfig manquant!", this);
            return;
        }

        if (fireflyPrefab == null)
        {
            Debug.LogError("FireflySpawnManager: Prefab de luciole manquant!", this);
            return;
        }

        // Vérification que le prefab a un Renderer
        if (fireflyPrefab.GetComponent<Renderer>() == null)
        {
            Debug.LogWarning("FireflySpawnManager: Le prefab luciole n'a pas de Renderer!", this);
        }

        // Démarrage automatique si configuré
        if (autoSpawn)
        {
            StartSpawning();
        }
    }

    /// <summary>
    /// Coroutine principale de spawn des lucioles
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (CanSpawnFirefly())
            {
                SpawnFirefly();
            }
            
            yield return new WaitForSeconds(config.SpawnInterval);
        }
    }

    /// <summary>
    /// Coroutine de nettoyage des lucioles expirées
    /// </summary>
    private IEnumerator CleanupRoutine()
    {
        while (isSpawning)
        {
            CleanupExpiredFireflies();
            yield return new WaitForSeconds(1f); // Vérification chaque seconde
        }
    }

    /// <summary>
    /// Vérifie si on peut spawner une nouvelle luciole
    /// </summary>
    private bool CanSpawnFirefly()
    {
        return activeFireflyCount < config.MaxFireflies && config != null && fireflyPrefab != null;
    }

    /// <summary>
    /// Spawn une nouvelle luciole à une position aléatoire
    /// </summary>
    private void SpawnFirefly()
    {
        Vector2 spawnPosition = config.GetRandomPosition();
        Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y, 0f);
        
        GameObject newFirefly = Instantiate(fireflyPrefab, worldPosition, Quaternion.identity);
        
        // Ajouter un composant pour tracker le temps de vie
        FireflyLifetimeTracker lifetimeTracker = newFirefly.GetComponent<FireflyLifetimeTracker>();
        if (lifetimeTracker == null)
        {
            lifetimeTracker = newFirefly.AddComponent<FireflyLifetimeTracker>();
        }
        lifetimeTracker.Initialize(config.FireflyLifetime);
        
        // Ajouter à la liste des lucioles actives
        activeFireflies.Add(newFirefly);
        activeFireflyCount++;
        
        Debug.Log($"FireflySpawnManager: Luciole spawnée à {worldPosition} - Total: {activeFireflyCount}/{config.MaxFireflies}");
    }

    /// <summary>
    /// Nettoie les lucioles expirées ou nulles
    /// </summary>
    private void CleanupExpiredFireflies()
    {
        for (int i = activeFireflies.Count - 1; i >= 0; i--)
        {
            GameObject firefly = activeFireflies[i];
            
            // Supprimer les références nulles
            if (firefly == null)
            {
                activeFireflies.RemoveAt(i);
                activeFireflyCount--;
                continue;
            }
            
            // Supprimer les lucioles expirées
            FireflyLifetimeTracker lifetimeTracker = firefly.GetComponent<FireflyLifetimeTracker>();
            if (lifetimeTracker != null && lifetimeTracker.IsExpired())
            {
                activeFireflies.RemoveAt(i);
                activeFireflyCount--;
                Destroy(firefly);
                Debug.Log($"FireflySpawnManager: Luciole supprimée (expirée) - Total: {activeFireflyCount}");
            }
        }
    }

    /// <summary>
    /// Dessine les gizmos de la zone de spawn dans l'éditeur
    /// </summary>
    private void DrawSpawnAreaGizmos()
    {
        Gizmos.color = Color.yellow;
        
        Vector3 topLeft = new Vector3(config.TopLeft.x, config.TopLeft.y, 0f);
        Vector3 topRight = new Vector3(config.BottomRight.x, config.TopLeft.y, 0f);
        Vector3 bottomLeft = new Vector3(config.TopLeft.x, config.BottomRight.y, 0f);
        Vector3 bottomRight = new Vector3(config.BottomRight.x, config.BottomRight.y, 0f);
        
        // Dessiner le contour de la zone de spawn
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Dessiner le centre
        Gizmos.color = Color.red;
        Vector3 center = new Vector3(config.Center.x, config.Center.y, 0f);
        Gizmos.DrawWireSphere(center, 0.1f);
    }

    #endregion
}

/// <summary>
/// Composant helper pour tracker la durée de vie d'une luciole
/// </summary>
public class FireflyLifetimeTracker : MonoBehaviour
{
    private float spawnTime;
    private float lifetime;
    private bool isInitialized = false;

    /// <summary>
    /// Initialise le tracker avec la durée de vie spécifiée
    /// </summary>
    public void Initialize(float fireflyLifetime)
    {
        spawnTime = Time.time;
        lifetime = fireflyLifetime;
        isInitialized = true;
    }

    /// <summary>
    /// Vérifie si la luciole a expiré
    /// </summary>
    public bool IsExpired()
    {
        if (!isInitialized) return false;
        return Time.time - spawnTime >= lifetime;
    }

    /// <summary>
    /// Retourne le temps restant avant expiration
    /// </summary>
    public float GetTimeRemaining()
    {
        if (!isInitialized) return 0f;
        return Mathf.Max(0f, lifetime - (Time.time - spawnTime));
    }
}
