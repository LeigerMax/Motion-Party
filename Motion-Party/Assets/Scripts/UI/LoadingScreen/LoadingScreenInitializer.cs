using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script d'initialisation automatique du système LoadingScreen
/// À placer dans la scène MiniGameManager
/// </summary>
public class LoadingScreenInitializer : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool autoInitialize = true;
    [SerializeField] private bool createPrefabIfMissing = true;
    
    [Header("Ressources")]
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private LoadingScreenData loadingData;
    
    private void Awake()
    {
        // S'assurer que cet initializer ne se duplique pas
        if (FindObjectsOfType<LoadingScreenInitializer>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        if (autoInitialize)
        {
            InitializeLoadingScreen();
        }
    }
    
    /// <summary>
    /// Initialise le système d'écrans de chargement
    /// </summary>
    [ContextMenu("Initialiser LoadingScreen")]
    public void InitializeLoadingScreen()
    {
        // Vérifier si le système existe déjà
        if (LoadingScreenManager.Instance != null)
        {
            Debug.Log("LoadingScreenManager déjà initialisé");
            return;
        }
        
        // Créer le système si nécessaire
        if (loadingScreenPrefab != null)
        {
            CreateFromPrefab();
        }
        else if (createPrefabIfMissing)
        {
            CreateFromScratch();
        }
        else
        {
            Debug.LogWarning("LoadingScreenInitializer: Aucun prefab assigné et création automatique désactivée");
        }
    }
    
    /// <summary>
    /// Crée le système depuis un prefab
    /// </summary>
    private void CreateFromPrefab()
    {
        GameObject loadingInstance = Instantiate(loadingScreenPrefab);
        loadingInstance.name = "LoadingScreen_System";
        
        // Configurer les données si disponibles
        var manager = loadingInstance.GetComponent<LoadingScreenManager>();
        if (manager != null && loadingData != null)
        {
            // Utiliser la réflexion pour assigner les données
            var dataField = typeof(LoadingScreenManager).GetField("loadingData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dataField?.SetValue(manager, loadingData);
        }
        
        Debug.Log("LoadingScreenManager créé depuis prefab");
    }
    
    /// <summary>
    /// Crée le système depuis zéro
    /// </summary>
    private void CreateFromScratch()
    {
        // Utiliser le LoadingScreenPrefabCreator
        var creator = gameObject.AddComponent<LoadingScreenPrefabCreator>();
        creator.CreateLoadingScreenPrefab();
        
        // Nettoyer le créateur temporaire
        Destroy(creator);
        
        Debug.Log("LoadingScreenManager créé automatiquement");
    }
    
    /// <summary>
    /// Vérifie l'état du système
    /// </summary>
    [ContextMenu("Vérifier état LoadingScreen")]
    public void CheckLoadingScreenState()
    {
        var manager = LoadingScreenManager.Instance;
        if (manager != null)
        {
            Debug.Log($"✅ LoadingScreenManager: OK");
            Debug.Log($"   - GameObject: {manager.gameObject.name}");
            Debug.Log($"   - DontDestroyOnLoad: {manager.gameObject.scene.name == "DontDestroyOnLoad"}");
        }
        else
        {
            Debug.LogWarning("❌ LoadingScreenManager: Non trouvé");
        }
    }
    
    /// <summary>
    /// Test rapide du système
    /// </summary>
    [ContextMenu("Test LoadingScreen")]
    public void TestLoadingScreen()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Show("test");
            
            // Cacher après 3 secondes
            Invoke(nameof(HideLoadingScreen), 3f);
        }
        else
        {
            Debug.LogError("LoadingScreenManager non initialisé");
        }
    }
    
    private void HideLoadingScreen()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Hide();
        }
    }
}
