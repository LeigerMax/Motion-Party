using UnityEngine;

/// <summary>
/// Script de test pour vérifier le fonctionnement du LoadingScreenManager
/// </summary>
public class LoadingScreenTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool autoTestOnStart = false;
    [SerializeField] private string testSceneName = "MiniGame_LogParade";
    [SerializeField] private string testTipId = "default";
    
    private void Start()
    {
        if (autoTestOnStart)
        {
            Invoke(nameof(TestLoadingScreen), 2f); // Délai pour éviter les conflits de démarrage
        }
    }
    
    [ContextMenu("Test Loading Screen Creation")]
    public void TestLoadingScreenCreation()
    {
        Debug.Log("=== TEST LOADING SCREEN CREATION ===");
        
        // Vérifier l'état initial
        Debug.Log($"LoadingScreenManager disponible: {LoadingScreenManager.Instance != null}");
        
        // Forcer la création
        var gameSessionManager = FindFirstObjectByType<GameSessionManager>();
        if (gameSessionManager != null)
        {
            // Appeler la méthode publique de debug pour déclencher EnsureLoadingScreenManager
            gameSessionManager.DebugCurrentState();
        }
        else
        {
            Debug.LogError("GameSessionManager introuvable pour le test !");
        }
        
        Debug.Log($"LoadingScreenManager disponible après test: {LoadingScreenManager.Instance != null}");
        Debug.Log("=== FIN TEST LOADING SCREEN CREATION ===");
    }
    
    [ContextMenu("Test Loading Screen Usage")]
    public void TestLoadingScreen()
    {
        Debug.Log("=== TEST LOADING SCREEN USAGE ===");
        
        if (LoadingScreenManager.Instance == null)
        {
            Debug.LogWarning("LoadingScreenManager non disponible, tentative de création...");
            TestLoadingScreenCreation();
        }
        
        if (LoadingScreenManager.Instance != null)
        {
            Debug.Log($"Test de chargement de la scène: {testSceneName}");
            
            LoadingScreenManager.Instance.ShowAndLoadScene(
                testSceneName,
                testTipId,
                () => {
                    Debug.Log($"Scène {testSceneName} chargée avec succès via LoadingScreenManager !");
                },
                "Test Scene",
                "Test de chargement automatique"
            );
        }
        else
        {
            Debug.LogError("Impossible de créer le LoadingScreenManager pour le test !");
        }
        
        Debug.Log("=== FIN TEST LOADING SCREEN USAGE ===");
    }
    
    [ContextMenu("Force Create LoadingScreen Prefab")]
    public void ForceCreateLoadingScreenFromPrefab()
    {
        Debug.Log("=== FORCE CREATE LOADING SCREEN FROM PREFAB ===");
        
        // Détruire l'instance existante si elle existe
        if (LoadingScreenManager.Instance != null)
        {
            Debug.Log("Destruction de l'instance existante de LoadingScreenManager");
            DestroyImmediate(LoadingScreenManager.Instance.gameObject);
        }
        
        // Essayer de charger le prefab
        GameObject loadingScreenPrefab = Resources.Load<GameObject>("LoadingScreenCanvas");
        if (loadingScreenPrefab != null)
        {
            Debug.Log("Prefab LoadingScreenCanvas trouvé dans Resources, instanciation...");
            GameObject instance = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(instance);
            Debug.Log($"LoadingScreenCanvas instancié: {LoadingScreenManager.Instance != null}");
        }
        else
        {
            Debug.LogError("Impossible de trouver le prefab LoadingScreenCanvas dans Resources !");
        }
        
        Debug.Log("=== FIN FORCE CREATE LOADING SCREEN FROM PREFAB ===");
    }
}
