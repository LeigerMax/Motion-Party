using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Fireflies
{
    /// <summary>
    /// Gestionnaire de spawn des lucioles pour le mini-jeu Danse des Lucioles
    /// </summary>
    public class FireflySpawner : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        [SerializeField] private GameObject fireflyPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private bool autoSpawn = true;
        [SerializeField] private Transform fireflyParent;
        [SerializeField] private bool enableDetailedLogging = false;
        
        [Header("Runtime Info (Read Only)")]
        [SerializeField] private int activeFireflyCount = 0;
        [SerializeField] private bool isSpawning = false;

        // Collections pour gérer les lucioles actives
        private List<FireflyController> activeFireflies = new List<FireflyController>();
        private Coroutine spawnCoroutine;
        private bool isInitialized = false;

        // Propriétés publiques
        public int ActiveFireflyCount => activeFireflyCount;
        public bool IsSpawning => isSpawning;
        public List<FireflyController> ActiveFireflies => new List<FireflyController>(activeFireflies);

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            if (config != null)
            {
                Initialize(config);
            }
        }

        void OnDestroy()
        {
            StopSpawning();
            CleanupEventListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise le spawner avec une configuration
        /// </summary>
        public void Initialize(FireflyDanceConfig danceConfig)
        {
            config = danceConfig;
            
            if (!ValidateComponents())
            {
                FireflyDanceLogger.LogError("FireflySpawner - Impossible d'initialiser, composants manquants", this);
                return;
            }
            
            SetupFireflyParent();
            SetupEventListeners();
            
            isInitialized = true;
            FireflyDanceLogger.LogSpawn("FireflySpawner initialisé");
            
            if (autoSpawn)
            {
                StartSpawning();
            }
        }

        /// <summary>
        /// Configure le parent des lucioles
        /// </summary>
        private void SetupFireflyParent()
        {
            if (fireflyParent == null)
            {
                GameObject parentGO = new GameObject("Fireflies");
                parentGO.transform.SetParent(transform);
                fireflyParent = parentGO.transform;
            }
        }

        /// <summary>
        /// Configure les écouteurs d'événements
        /// </summary>
        private void SetupEventListeners()
        {
            FireflyDanceEvents.OnFireflyDestroyed += HandleFireflyDestroyed;
        }

        /// <summary>
        /// Nettoie les écouteurs d'événements
        /// </summary>
        private void CleanupEventListeners()
        {
            FireflyDanceEvents.OnFireflyDestroyed -= HandleFireflyDestroyed;
        }

        /// <summary>
        /// Valide les composants requis
        /// </summary>
        private bool ValidateComponents()
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("FireflySpawner - Configuration manquante");
                return false;
            }

            if (fireflyPrefab == null)
            {
                FireflyDanceLogger.LogWarning("FireflySpawner - Prefab manquant, tentative de création automatique...");
                return TryCreateDefaultFireflyPrefab();
            }

            return true;
        }

        /// <summary>
        /// Tente de créer un prefab de luciole par défaut
        /// </summary>
        private bool TryCreateDefaultFireflyPrefab()
        {
            try
            {
                // Créer un prefab de luciole basique en runtime
                GameObject firefly = CreateBasicFireflyPrefab();
                
                if (firefly != null)
                {
                    fireflyPrefab = firefly;
                    FireflyDanceLogger.Log("FireflySpawner - Prefab de base créé automatiquement");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                FireflyDanceLogger.LogError($"Impossible de créer le prefab automatique : {e.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Crée un prefab de luciole basique
        /// </summary>
        private GameObject CreateBasicFireflyPrefab()
        {
            // Créer le GameObject principal
            GameObject firefly = new GameObject("BasicFirefly");
            
            // Ajouter FireflyController
            firefly.AddComponent<FireflyController>();
            
            // Ajouter Collider
            var collider = firefly.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
            
            // Ajouter Rigidbody
            var rigidbody = firefly.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            
            // Créer l'effet visuel basique
            CreateBasicVisual(firefly);
            
            // Le marquer comme prefab de base
            DontDestroyOnLoad(firefly);
            
            return firefly;
        }

        /// <summary>
        /// Crée un effet visuel basique pour la luciole
        /// </summary>
        private void CreateBasicVisual(GameObject parent)
        {
            // Créer une sphère simple
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "FireflyVisual";
            sphere.transform.SetParent(parent.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.1f;
            
            // Supprimer le collider de la sphère (on utilise celui du parent)
            DestroyImmediate(sphere.GetComponent<SphereCollider>());
            
            // Créer un matériau émissif simple
            var renderer = sphere.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.yellow;
            
            // Activer l'émission si possible
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.yellow * 2f);
            }
            
            renderer.material = material;
            
            // Ajouter une lumière simple
            GameObject lightObj = new GameObject("FireflyLight");
            lightObj.transform.SetParent(parent.transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.yellow;
            light.intensity = 1f;
            light.range = 2f;
            light.shadows = LightShadows.None;
        }

        #endregion

        #region Spawn Management

        /// <summary>
        /// Démarre le spawn automatique des lucioles
        /// </summary>
        public void StartSpawning()
        {
            if (!isInitialized || isSpawning) return;
            
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnRoutine());
            FireflyDanceLogger.LogSpawn("Spawn des lucioles démarré");
        }

        /// <summary>
        /// Arrête le spawn automatique
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
            FireflyDanceLogger.LogSpawn("Spawn des lucioles arrêté");
        }

        /// <summary>
        /// Met en pause le spawn des lucioles
        /// </summary>
        public void PauseSpawning()
        {
            if (isSpawning && spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
                isSpawning = false;
                FireflyDanceLogger.LogSpawn("Spawn des lucioles mis en pause");
            }
        }

        /// <summary>
        /// Reprend le spawn des lucioles après une pause
        /// </summary>
        public void ResumeSpawning()
        {
            if (!isSpawning && isInitialized && autoSpawn)
            {
                StartSpawning();
                FireflyDanceLogger.LogSpawn("Spawn des lucioles repris");
            }
        }

        /// <summary>
        /// Coroutine de spawn automatique
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            while (isSpawning && isInitialized)
            {
                if (activeFireflies.Count < config.MaxFireflies)
                {
                    SpawnFirefly();
                }
                
                yield return new WaitForSeconds(config.SpawnInterval);
            }
        }

        /// <summary>
        /// Spawne une nouvelle luciole
        /// </summary>
        private void SpawnFirefly()
        {
            if (fireflyPrefab == null)
            {
                FireflyDanceLogger.LogError("Impossible de spawner - fireflyPrefab manquant", this);
                return;
            }

            Vector2 spawnPosition = config.GetRandomPosition();
            // Utiliser toute la zone de jeu définie dans la config
            float randomZ = Random.Range(-config.Depth/2f, config.Depth/2f);
            Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y, randomZ);

            GameObject fireflyGO = Instantiate(fireflyPrefab, worldPosition, Quaternion.identity, fireflyParent);
            FireflyController fireflyController = fireflyGO.GetComponent<FireflyController>();

            if (fireflyController != null)
            {
                // Sélectionner aléatoirement le type de libellule
                var fireflyType = config.GetRandomFireflyType();
                
                // Utiliser la nouvelle méthode d'initialisation qui force la configuration
                fireflyController.InitializeFromSpawner(config, fireflyType);
                
                activeFireflies.Add(fireflyController);
                activeFireflyCount = activeFireflies.Count;
                
                FireflyDanceEvents.OnFireflySpawned?.Invoke(fireflyController);
                
                if (enableDetailedLogging)
                    FireflyDanceLogger.LogSpawn($"Libellule {fireflyType} spawnée à {worldPosition} avec configuration forcée");
            }
            else
            {
                FireflyDanceLogger.LogError("FireflyController manquant sur le prefab spawné");
                Destroy(fireflyGO);
            }
        }

        #endregion

        #region Firefly Management

        /// <summary>
        /// Gère la destruction d'une luciole
        /// </summary>
        private void HandleFireflyDestroyed(FireflyController firefly)
        {
            if (activeFireflies.Contains(firefly))
            {
                activeFireflies.Remove(firefly);
                activeFireflyCount = activeFireflies.Count;
            }
        }

        /// <summary>
        ///  Nettoie toutes les lucioles actives (utilisé à la fin du jeu)
        /// </summary>
        public void ClearAllFireflies()
        {
            if (activeFireflies.Count == 0) return;

            FireflyDanceLogger.LogSpawn($" Nettoyage de {activeFireflies.Count} lucioles actives");

            // Créer une copie de la liste pour éviter les modifications pendant l'itération
            var firefliesCopy = new List<FireflyController>(activeFireflies);
            
            foreach (var firefly in firefliesCopy)
            {
                if (firefly != null)
                {
                    // Détruire la luciole directement
                    if (firefly.gameObject != null)
                        Destroy(firefly.gameObject);
                }
            }

            // Nettoyer la liste
            activeFireflies.Clear();
            activeFireflyCount = 0;

            FireflyDanceLogger.LogSpawn("Toutes les lucioles ont été supprimées");
        }

        #endregion

    }
}
