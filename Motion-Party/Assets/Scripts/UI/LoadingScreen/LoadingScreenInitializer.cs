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
        

    }
    
    private void HideLoadingScreen()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Hide();
        }
    }
}
