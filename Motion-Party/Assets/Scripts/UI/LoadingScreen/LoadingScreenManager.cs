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
    [SerializeField] private bool lockMinDisplayTimeAt15Seconds = true; // Verrouiller à 15s
    
    // Variables de contrôle
    private bool isCurrentlyLoading = false;
    private float loadingStartTime = 0f;
    
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
    
    /// <summary>
    /// Indique si l'écran de chargement est actuellement affiché
    /// </summary>
    public bool IsShowing => (loadingUI != null && loadingUI.IsShowing) || isCurrentlyLoading;
    
    /// <summary>
    /// Durée minimale d'affichage de l'écran de chargement (lecture seule)
    /// </summary>
    public float MinDisplayTime => minDisplayTime;
    
    /// <summary>
    /// Force la durée minimale d'affichage (pour debugging/tests)
    /// </summary>
    public void SetMinDisplayTime(float newTime)
    {
        if (newTime > 0)
        {
            minDisplayTime = newTime;
            Debug.Log($"[LoadingScreenManager] Durée minimale modifiée à {minDisplayTime}s");
        }
        else
        {
            Debug.LogWarning($"[LoadingScreenManager] Tentative de définir une durée invalide: {newTime}s");
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
        
        // Forcer la durée minimale à 15 secondes si elle a été modifiée accidentellement
        if (lockMinDisplayTimeAt15Seconds || minDisplayTime < 15f)
        {
            if (minDisplayTime != 15f)
            {
                Debug.LogWarning($"[LoadingScreenManager] minDisplayTime était configuré à {minDisplayTime}s, forcé à 15s");
                minDisplayTime = 15f;
            }
        }
        
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
        // Essayer de trouver et utiliser MotionPartyLoadingTips
        var motionPartyTips = Resources.Load<MotionPartyLoadingTips>("MotionPartyLoadingTips");
        if (motionPartyTips != null)
        {
            Debug.Log("LoadingScreenManager: Utilisation de MotionPartyLoadingTips trouvé dans Resources");
            loadingData = motionPartyTips.CreateLoadingScreenData();
        }
        else
        {
            Debug.LogWarning("LoadingScreenManager: MotionPartyLoadingTips non trouvé, création de données basiques");
            
            // Fallback : créer des données basiques avec les IDs corrects
            loadingData = ScriptableObject.CreateInstance<LoadingScreenData>();
            loadingData.defaultTipText = "Préparez-vous pour le prochain défi !";
            loadingData.tips = new LoadingScreenData.LoadingTip[]
            {
                new LoadingScreenData.LoadingTip
                {
                    tipId = "firefly_dance",
                    tipText = "Fermez doucement vos mains pour attraper les lucioles !",
                    backgroundColor = new Color(0.1f, 0.1f, 0.3f)
                },
                new LoadingScreenData.LoadingTip
                {
                    tipId = "log_parade",
                    tipText = "Bougez vos bras de gauche à droite pour guider les bûches !",
                    backgroundColor = new Color(0.3f, 0.2f, 0.1f)
                },
                new LoadingScreenData.LoadingTip
                {
                    tipId = "music_note",
                    tipText = "Retiens les notes pour réussir la séquence !",
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f)
                },
                new LoadingScreenData.LoadingTip
                {
                    tipId = "default",
                    tipText = "Chargement en cours...",
                    backgroundColor = Color.black
                }
            };
        }
        
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

        // Marquer le début du chargement
        isCurrentlyLoading = true;
        loadingStartTime = Time.unscaledTime;
        Debug.Log($"[LoadingScreenManager] Show() - tipId reçu: '{tipId}', début du chargement marqué à {loadingStartTime}");

        var tipData = loadingData.GetTip(tipId);
        Debug.Log($"[LoadingScreenManager] Tip sélectionné: '{tipData.tipText}' (ID: {tipData.tipId})");
        loadingUI.Show(tipData, title, description);
    }    /// <summary>
    /// Cache l'écran de chargement
    /// </summary>
    public void Hide()
    {
        // Vérifier si on peut fermer l'écran de chargement
        if (isCurrentlyLoading)
        {
            float elapsedTime = Time.unscaledTime - loadingStartTime;
            if (elapsedTime < minDisplayTime)
            {
                Debug.LogWarning($"[LoadingScreenManager] Tentative de fermeture prématurée de l'écran de chargement ignorée. Temps écoulé: {elapsedTime:F2}s, Minimum requis: {minDisplayTime}s");
                return;
            }
        }
        
        Debug.Log($"[LoadingScreenManager] Hide() appelé et autorisé. Stack trace:\n{System.Environment.StackTrace}");
        
        isCurrentlyLoading = false;
        
        if (loadingUI != null)
            loadingUI.Hide();
    }
    
    /// <summary>
    /// Met à jour la progression du chargement
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (loadingUI != null)
        {
            loadingUI.UpdateProgress(progress);
        }
        else
        {
            Debug.LogWarning("[LoadingScreenManager] loadingUI est null - impossible de mettre à jour la progression");
        }
    }
    
    /// <summary>
    /// Coroutine principale pour charger une scène avec écran de chargement
    /// </summary>
    private IEnumerator LoadSceneWithLoadingScreen(string sceneName, string tipId, Action onSceneLoaded, string title = null, string description = null)
    {
        float startTime = Time.unscaledTime;
        
        Debug.Log($"[LoadingScreenManager] Début chargement de {sceneName}. minDisplayTime configuré: {minDisplayTime}s");
        
        // Vérifier que l'UI de chargement est disponible
        if (loadingUI == null)
        {
            Debug.LogError("[LoadingScreenManager] loadingUI est null - impossible d'afficher l'écran de chargement");
            
            // Fallback: chargement direct
            SceneManager.LoadScene(sceneName);
            onSceneLoaded?.Invoke();
            yield break;
        }
        
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
        Debug.Log($"[LoadingScreenManager] Temps écoulé: {elapsedTime:F2}s, Temps minimum requis: {minDisplayTime}s");
        
        if (elapsedTime < minDisplayTime)
        {
            float waitTime = minDisplayTime - elapsedTime;
            Debug.Log($"[LoadingScreenManager] Attente supplémentaire de {waitTime:F2}s pour respecter le temps minimum");
            yield return new WaitForSecondsRealtime(waitTime);
            Debug.Log($"[LoadingScreenManager] Attente terminée. Temps total d'affichage: {Time.unscaledTime - startTime:F2}s");
        }
        else
        {
            Debug.Log($"[LoadingScreenManager] Temps minimum déjà écoulé, pas d'attente supplémentaire");
        }
        
        // Masquer l'écran de chargement
        Debug.Log("[LoadingScreenManager] Appel de Hide() depuis la coroutine principale");
        isCurrentlyLoading = false; // Autoriser explicitement la fermeture
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
        Debug.Log($"[LoadingScreenManager] Tentative de chargement des données: '{resourcePath}'");
        
        // Essayer de charger LoadingScreenData
        var data = Resources.Load<LoadingScreenData>(resourcePath);
        if (data != null)
        {
            loadingData = data;
            Debug.Log($"[LoadingScreenManager] LoadingScreenData chargé depuis Resources avec {data.tips?.Length ?? 0} astuces");
            return;
        }
        
        Debug.Log("[LoadingScreenManager] LoadingScreenData non trouvé, tentative de chargement de MotionPartyLoadingTips");
        
        // Sinon essayer de charger MotionPartyLoadingTips
        var motionPartyTips = Resources.Load<MotionPartyLoadingTips>("MotionPartyLoadingTips");
        if (motionPartyTips != null)
        {
            Debug.Log($"[LoadingScreenManager] MotionPartyLoadingTips chargé depuis Resources avec {motionPartyTips.gameSpecificTips.Length} astuces:");
            foreach (var tip in motionPartyTips.gameSpecificTips)
            {
                Debug.Log($"  - {tip.tipId}: {tip.tipText}");
            }
            loadingData = motionPartyTips.CreateLoadingScreenData();
            Debug.Log("[LoadingScreenManager] LoadingScreenData créé à partir de MotionPartyLoadingTips");
            return;
        }
        
        Debug.LogWarning($"[LoadingScreenManager] Impossible de charger les données depuis {resourcePath} ou MotionPartyLoadingTips");
    }
    
    /// <summary>
    /// Force l'affichage de l'écran de chargement pendant une durée spécifique (pour tests)
    /// </summary>
    [ContextMenu("Test Écran de Chargement 15s")]
    public void TestLoadingScreen()
    {
        StartCoroutine(TestLoadingScreenCoroutine());
    }
    
    /// <summary>
    /// Vérifie et corrige la configuration de l'écran de chargement
    /// </summary>
    [ContextMenu("Vérifier/Corriger Configuration")]
    public void ValidateAndFixConfiguration()
    {
        Debug.Log($"[LoadingScreenManager] Configuration actuelle:");
        Debug.Log($"  - minDisplayTime: {minDisplayTime}s");
        Debug.Log($"  - lockMinDisplayTimeAt15Seconds: {lockMinDisplayTimeAt15Seconds}");
        Debug.Log($"  - loadingUI: {(loadingUI != null ? "Assigné" : "MANQUANT")}");
        Debug.Log($"  - loadingData: {(loadingData != null ? "Assigné" : "MANQUANT")}");
        
        if (loadingData != null)
        {
            Debug.Log($"  - Tips disponibles: {loadingData.tips?.Length ?? 0}");
            if (loadingData.tips != null)
            {
                foreach (var tip in loadingData.tips)
                {
                    Debug.Log($"    * {tip.tipId}: \"{tip.tipText}\"");
                }
            }
        }
        
        if (lockMinDisplayTimeAt15Seconds || minDisplayTime < 15f)
        {
            if (minDisplayTime != 15f)
            {
                Debug.LogWarning($"[LoadingScreenManager] CORRECTION: minDisplayTime était {minDisplayTime}s, forcé à 15s");
                minDisplayTime = 15f;
            }
        }
        
        ValidateComponents();
        
        Debug.Log("[LoadingScreenManager] Vérification terminée.");
    }
    
    /// <summary>
    /// Force le rechargement des données depuis MotionPartyLoadingTips
    /// </summary>
    [ContextMenu("Recharger Tips depuis MotionPartyLoadingTips")]
    public void ReloadFromMotionPartyTips()
    {
        var motionPartyTips = Resources.Load<MotionPartyLoadingTips>("MotionPartyLoadingTips");
        if (motionPartyTips != null)
        {
            loadingData = motionPartyTips.CreateLoadingScreenData();
            Debug.Log($"[LoadingScreenManager] Tips rechargés depuis MotionPartyLoadingTips. {loadingData.tips.Length} tips disponibles.");
            ValidateAndFixConfiguration();
        }
        else
        {
            Debug.LogError("[LoadingScreenManager] MotionPartyLoadingTips non trouvé dans Resources !");
        }
    }
    
    private IEnumerator TestLoadingScreenCoroutine()
    {
        Debug.Log("[LoadingScreenManager] TEST: Affichage de l'écran de chargement pour 15 secondes");
        Show("default", "Test", "Écran de chargement de test - 15 secondes");
        
        yield return new WaitForSecondsRealtime(minDisplayTime);
        
        Debug.Log("[LoadingScreenManager] TEST: Fin du test, fermeture de l'écran");
        isCurrentlyLoading = false;
        Hide();
    }
}
