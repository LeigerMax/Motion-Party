using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LoadingScreenData loadingData;
    [SerializeField] private LoadingScreenUI loadingUI;
    
    [Header("Durée minimale d'affichage")]
    [SerializeField] private float minDisplayTime = 15f; // 15 secondes comme demandé
    
    // Singleton
    private static LoadingScreenManager _instance;
    public static LoadingScreenManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<LoadingScreenManager>();
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Singleton pattern avec persistance
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // S'assurer que le Canvas parent est aussi persistant
            if (loadingUI != null && loadingUI.transform.parent != null)
            {
                DontDestroyOnLoad(loadingUI.transform.root.gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Validation des composants
        ValidateComponents();
        
        // Initialisation automatique des données si nécessaire
        InitializeDefaultData();
    }
    
    /// <summary>
    /// Initialise les données par défaut si elles ne sont pas assignées
    /// </summary>
    private void InitializeDefaultData()
    {
        if (loadingData == null)
        {
            // Essayer de charger depuis Resources
            LoadDataFromResources();
            
            // Si toujours pas de données, créer des données par défaut en runtime
            if (loadingData == null)
            {
                CreateRuntimeDefaultData();
            }
        }
    }
    
    /// <summary>
    /// Crée des données par défaut en runtime si aucune n'est trouvée
    /// </summary>
    private void CreateRuntimeDefaultData()
    {
        loadingData = ScriptableObject.CreateInstance<LoadingScreenData>();
        loadingData.defaultTipText = "Préparez-vous pour le prochain défi !";
        loadingData.tips = new LoadingScreenData.LoadingTip[]
        {
            new LoadingScreenData.LoadingTip
            {
                tipId = "default",
                tipText = "Chargement en cours...",
                backgroundColor = Color.black
            }
        };
        
        Debug.Log("LoadingScreenManager: Données par défaut créées en runtime");
    }
    
    /// <summary>
    /// Affiche l'écran de chargement et charge la scène suivante
    /// </summary>
    /// <param name="sceneName">Nom de la scène à charger</param>
    /// <param name="tipId">ID de l'astuce à afficher</param>
    /// <param name="onSceneLoaded">Callback appelé quand la scène est chargée</param>
    public void ShowAndLoadScene(string sceneName, string tipId = "", Action onSceneLoaded = null, string title = null, string description = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadingScreenManager: Nom de scène vide !");
            return;
        }
        
        // Ajout des paramètres optionnels title et description
        StartCoroutine(LoadSceneWithLoadingScreen(sceneName, tipId, onSceneLoaded, title, description));
    }
    
    /// <summary>
    /// Affiche simplement l'écran de chargement
    /// </summary>
    /// <param name="tipId">ID de l'astuce à afficher</param>
    public void Show(string tipId = "", string title = null, string description = null)
    {
        if (loadingUI == null || loadingData == null)
        {
            Debug.LogError("LoadingScreenManager: Composants manquants !");
            return;
        }
        
        var tipData = loadingData.GetTip(tipId);
        loadingUI.Show(tipData, title, description);
    }
    
    /// <summary>
    /// Cache l'écran de chargement
    /// </summary>
    public void Hide()
    {
        if (loadingUI != null)
            loadingUI.Hide();
    }
    
    /// <summary>
    /// Met à jour la progression du chargement
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (loadingUI != null)
            loadingUI.UpdateProgress(progress);
    }
    
    /// <summary>
    /// Coroutine principale pour charger une scène avec écran de chargement
    /// </summary>
    private IEnumerator LoadSceneWithLoadingScreen(string sceneName, string tipId, Action onSceneLoaded, string title = null, string description = null)
    {
        float startTime = Time.unscaledTime;
        
        // Afficher l'écran de chargement avec titre et description
        Show(tipId, title, description);
        
        // Attendre une frame pour que l'UI s'affiche
        yield return null;
        
        // Démarrer le chargement asynchrone de la scène
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Mettre à jour la progression
        while (!asyncLoad.isDone)
        {
            // La progression va de 0 à 0.9 pendant le chargement
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateProgress(progress);
            
            // Quand le chargement est à 90%, Unity attend notre autorisation
            if (asyncLoad.progress >= 0.9f)
            {
                UpdateProgress(1f);
                break;
            }
            
            yield return null;
        }
        
        // S'assurer que l'écran de chargement est affiché pendant le temps minimum
        float elapsedTime = Time.unscaledTime - startTime;
        if (elapsedTime < minDisplayTime)
        {
            yield return new WaitForSecondsRealtime(minDisplayTime - elapsedTime);
        }
        
        // Masquer l'écran de chargement
        Hide();
        
        // Attendre que l'animation de disparition se termine
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Activer la scène
        asyncLoad.allowSceneActivation = true;
        
        // Attendre que la scène soit complètement chargée
        yield return asyncLoad;
        
        // Appeler le callback si fourni
        onSceneLoaded?.Invoke();
    }
    
    /// <summary>
    /// Valide que tous les composants nécessaires sont présents
    /// </summary>
    private void ValidateComponents()
    {
        if (loadingUI == null)
        {
            Debug.LogError("LoadingScreenManager: LoadingScreenUI manquant !");
        }
        
        if (loadingData == null)
        {
            Debug.LogError("LoadingScreenManager: LoadingScreenData manquant !");
        }
    }
    
    /// <summary>
    /// Charge les données de chargement depuis les Resources (optionnel)
    /// </summary>
    public void LoadDataFromResources(string resourcePath = "LoadingScreenData")
    {
        var data = Resources.Load<LoadingScreenData>(resourcePath);
        if (data != null)
        {
            loadingData = data;
            Debug.Log("LoadingScreenManager: Données chargées depuis Resources");
        }
        else
        {
            Debug.LogWarning($"LoadingScreenManager: Impossible de charger les données depuis {resourcePath}");
        }
    }
}
