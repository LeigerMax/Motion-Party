using UnityEngine;

namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Initialise et vérifie que tous les composants du système de badges sont présents et persistants
    /// </summary>
    public class BadgeSystemInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool initializeOnAwake = true;

        [Header("Required Components")]
        [SerializeField] private GlobalBadgeDatabase badgeDatabase;
        [SerializeField] private GlobalBadgeSystem badgeSystem;
        [SerializeField] private GlobalBadgeTracker badgeTracker;
        [SerializeField] private GlobalPlayerBadgeStorage playerStorage;
        [SerializeField] private SessionBadgeTracker sessionTracker;

        private static BadgeSystemInitializer _instance;
        public static BadgeSystemInitializer Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<GameObject>("BadgeSystem");
                if (prefab != null)
                {
                    var go = Instantiate(prefab);
                    go.name = "BadgeSystem";
                    DontDestroyOnLoad(go);
                    _instance = go.GetComponent<BadgeSystemInitializer>();
                    if (_instance != null)
                    {
                        Debug.Log("BadgeSystem: Prefab instancié avec succès");
                    }
                    else
                    {
                        Debug.LogError("BadgeSystem: Le prefab ne contient pas le composant BadgeSystemInitializer!");
                    }
                }
                else
                {
                    Debug.LogError("BadgeSystem: Prefab 'BadgeSystem' introuvable dans les Resources!");
                }
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                if (initializeOnAwake)
                {
                    InitializeAllBadgeSystems();
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAllBadgeSystems()
        {
            if (debugMode)
                Debug.Log("BadgeSystemInitializer: Initialisation des systèmes de badges...");

            // Vérifier que tous les composants requis sont présents
            if (!ValidateComponents())
            {
                Debug.LogError("BadgeSystemInitializer: Composants requis manquants, initialisation annulée");
                return;
            }

            // Configurer les liens entre composants
            ConfigureComponents();

            if (debugMode)
                Debug.Log("BadgeSystemInitializer: Tous les systèmes de badges initialisés");
        }

        private bool ValidateComponents()
        {
            bool allValid = true;

            if (badgeDatabase == null)
            {
                Debug.LogError("BadgeSystemInitializer: ❌ GlobalBadgeDatabase manquante");
                allValid = false;
            }

            if (badgeSystem == null)
            {
                Debug.LogError("BadgeSystemInitializer: ❌ GlobalBadgeSystem manquant");
                allValid = false;
            }

            if (badgeTracker == null)
            {
                Debug.LogError("BadgeSystemInitializer: ❌ GlobalBadgeTracker manquant");
                allValid = false;
            }

            if (playerStorage == null)
            {
                Debug.LogError("BadgeSystemInitializer: ❌ GlobalPlayerBadgeStorage manquant");
                allValid = false;
            }

            if (sessionTracker == null)
            {
                Debug.LogError("BadgeSystemInitializer: ❌ SessionBadgeTracker manquant");
                allValid = false;
            }

            return allValid;
        }

        private void ConfigureComponents()
        {
            // Configurer GlobalBadgeSystem
            if (badgeSystem != null)
            {
#if UNITY_EDITOR
                var systemSO = new UnityEditor.SerializedObject(badgeSystem);
                systemSO.FindProperty("badgeDatabase").objectReferenceValue = badgeDatabase;
                systemSO.FindProperty("playerStorage").objectReferenceValue = playerStorage;
                systemSO.ApplyModifiedProperties();
#endif
            }

            // Configurer GlobalBadgeTracker
            if (badgeTracker != null)
            {
#if UNITY_EDITOR
                var trackerSO = new UnityEditor.SerializedObject(badgeTracker);
                trackerSO.FindProperty("badgeSystem").objectReferenceValue = badgeSystem;
                trackerSO.ApplyModifiedProperties();
#endif
            }
        }

        [ContextMenu("Check Badge Systems Status")]
        public void CheckBadgeSystemsStatus()
        {
            Debug.Log("=== STATUS DES SYSTÈMES DE BADGES ===");
            
            // Vérifier les instances
            Debug.Log($"GlobalBadgeDatabase: {(badgeDatabase != null ? "✅ OK" : "❌ NULL")}");
            Debug.Log($"GlobalBadgeSystem: {(badgeSystem != null ? "✅ OK" : "❌ NULL")}");
            Debug.Log($"GlobalBadgeTracker: {(badgeTracker != null ? "✅ OK" : "❌ NULL")}");
            Debug.Log($"GlobalPlayerBadgeStorage: {(playerStorage != null ? "✅ OK" : "❌ NULL")}");
            Debug.Log($"SessionBadgeTracker: {(sessionTracker != null ? "✅ OK" : "❌ NULL")}");

            // Vérifier les références
            if (badgeSystem != null)
            {
#if UNITY_EDITOR
                var systemSO = new UnityEditor.SerializedObject(badgeSystem);
                Debug.Log($"  GlobalBadgeSystem.Database: {(systemSO.FindProperty("badgeDatabase").objectReferenceValue != null ? "✅ OK" : "❌ NULL")}");
                Debug.Log($"  GlobalBadgeSystem.Storage: {(systemSO.FindProperty("playerStorage").objectReferenceValue != null ? "✅ OK" : "❌ NULL")}");
#endif
            }

            if (badgeTracker != null)
            {
#if UNITY_EDITOR
                var trackerSO = new UnityEditor.SerializedObject(badgeTracker);
                Debug.Log($"  GlobalBadgeTracker.System: {(trackerSO.FindProperty("badgeSystem").objectReferenceValue != null ? "✅ OK" : "❌ NULL")}");
#endif
            }

            // Vérifier les badges disponibles
            if (badgeDatabase != null)
            {
                var allBadges = badgeDatabase.AllBadges;
                Debug.Log($"\nBadges disponibles: {allBadges.Count}");
                foreach (var badge in allBadges)
                {
                    Debug.Log($"  • {badge.MiniGameId}/{badge.BadgeId}: {badge.BadgeName}");
                }
            }
        }
    }
} 