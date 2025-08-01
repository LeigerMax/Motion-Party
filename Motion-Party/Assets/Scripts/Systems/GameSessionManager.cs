using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameState { Intro, Playing, Results }


public class GameSessionManager : MonoBehaviour
{
    [Header("Mini-Games Configuration")]
    [SerializeField] private List<MiniGameInfo> miniGames;
    [SerializeField] private string mainMenuSceneName = "MiniGameManager"; // Nom de la scène du menu principal
    
    [Header("Transition Settings")]
    [SerializeField] private float defaultTransitionDelay = 1f; // Délai par défaut entre les jeux
    [SerializeField] private bool enableDetailedLogs = true; // Logs détaillés pour debug
    [SerializeField] private bool forceDebugMode = false; // Force les logs même si enableDetailedLogs est false
    
    private int currentGameIndex = 0;

    // Singleton
    public static GameSessionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // Rien ici, tout est dans Awake
    }

    /// <summary>
    /// Méthode publique pour déclencher la transition vers le mini-jeu suivant avec un délai optionnel
    /// Appelée par les GameManager via OnTransitionToNextMiniGame
    /// Cette méthode force le retour à la scène principale pour utiliser le LoadingScreenManager
    /// </summary>
    public void TriggerNextMiniGameTransition(float delay = -1f)
    {
        if (delay < 0) delay = defaultTransitionDelay;
        
        LogWithContext($"Transition déclenchée vers le mini-jeu suivant avec délai de {delay}s", true);
        
        // Utiliser la méthode commune pour la transition
        TriggerTransitionToNextGame(delay);
    }

    /// <summary>
    /// Charge directement le mini-jeu suivant avec l'écran de chargement (Option A)
    /// Méthode utilisée par tous les mini-jeux pour une transition directe
    /// </summary>
    public void LoadNextMiniGameWithLoadingScreen()
    {
        LogWithContext("*** LoadNextMiniGameWithLoadingScreen appelée - OPTION A ***", true);
        LogWithContext($"Index actuel: {currentGameIndex}, Total mini-jeux: {miniGames?.Count ?? 0}", true);
        
        // S'assurer que le LoadingScreenManager existe
        EnsureLoadingScreenManager();
        
        // Incrémenter pour passer au mini-jeu suivant
        currentGameIndex++;
        
        // Vérifier s'il reste des mini-jeux
        if (currentGameIndex >= miniGames.Count)
        {
            LogWithContext("Tous les mini-jeux ont été joués - Fin de session", true);
            // Utiliser la même logique que StartNextMiniGameCoroutine()
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "session_complete", null);
            }
            else
            {
                LogWithContext("LoadingScreenManager introuvable - Chargement direct vers menu principal", true);
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            return;
        }
        
        var nextMiniGame = miniGames[currentGameIndex];
        LogWithContext($"*** CHARGEMENT DIRECT du mini-jeu {currentGameIndex + 1}/{miniGames.Count} : {nextMiniGame.sceneName} ***", true);
        
        // Utiliser LoadingScreenManager directement
        if (LoadingScreenManager.Instance != null)
        {
            LogWithContext("*** UTILISATION DIRECTE DE LOADINGSCREENMANAGER ***", true);
            
            string tipId = !string.IsNullOrEmpty(nextMiniGame.loadingTipId) 
                ? nextMiniGame.loadingTipId 
                : "default";
            
            LoadingScreenManager.Instance.ShowAndLoadScene(
                nextMiniGame.sceneName,
                tipId,
                () => { 
                    LogWithContext($"Mini-jeu suivant {nextMiniGame.sceneName} chargé avec succès", true);
                },
                nextMiniGame.displayName,
                nextMiniGame.description
            );
        }
        else
        {
            LogWithContext("*** ERREUR: LoadingScreenManager non disponible après création - Utilisation du fallback ***", true);
            // Fallback : utiliser l'ancienne méthode
            TriggerTransitionToNextGame(2f);
        }
    }

    /// <summary>
    /// S'assure que le LoadingScreenManager existe et est configuré
    /// </summary>
    private void EnsureLoadingScreenManager()
    {
        if (LoadingScreenManager.Instance == null)
        {
            LogWithContext("*** CRÉATION AUTOMATIQUE DU LOADINGSCREENMANAGER ***", true);
            
            // Rechercher d'abord si un LoadingScreenManager existe quelque part
            LoadingScreenManager existingManager = FindFirstObjectByType<LoadingScreenManager>();
            if (existingManager != null)
            {
                LogWithContext("*** LoadingScreenManager trouvé dans la scène ***", true);
                return;
            }
            
            // Essayer de charger le prefab LoadingScreenCanvas depuis Resources
            GameObject loadingScreenPrefab = Resources.Load<GameObject>("LoadingScreenCanvas");
            if (loadingScreenPrefab == null)
            {
                // Essayer un autre chemin
                loadingScreenPrefab = Resources.Load<GameObject>("Prefabs/LoadingScreenCanvas");
            }
            
            if (loadingScreenPrefab != null)
            {
                LogWithContext("*** Instanciation du prefab LoadingScreenCanvas ***", true);
                GameObject loadingScreenInstance = Instantiate(loadingScreenPrefab);
                DontDestroyOnLoad(loadingScreenInstance);
                LogWithContext("*** LoadingScreenCanvas instancié et configuré comme persistant ***", true);
            }
            else
            {
                LogWithContext("*** ERREUR: Impossible de trouver le prefab LoadingScreenCanvas ***", true);
                // Créer un LoadingScreenManager basique
                CreateBasicLoadingScreenManager();
            }
        }
        else
        {
            LogWithContext("*** LoadingScreenManager déjà disponible ***", true);
        }
    }

    /// <summary>
    /// Crée un LoadingScreenManager basique en cas d'urgence
    /// </summary>
    private void CreateBasicLoadingScreenManager()
    {
        LogWithContext("*** Création d'un LoadingScreenManager basique ***", true);
        
        GameObject loadingManagerObj = new GameObject("LoadingScreenManager");
        LoadingScreenManager loadingManager = loadingManagerObj.AddComponent<LoadingScreenManager>();
        
        // Le marquer comme persistant
        DontDestroyOnLoad(loadingManagerObj);
        
        LogWithContext("*** LoadingScreenManager basique créé (sans UI) ***", true);
    }

    /// <summary>
    /// Méthode commune pour gérer les transitions vers le mini-jeu suivant
    /// </summary>
    private void TriggerTransitionToNextGame(float delay)
    {
        // Incrémenter l'index pour le prochain jeu
        currentGameIndex++;
        
        LogWithContext($"Transition vers mini-jeu suivant (index: {currentGameIndex}) avec délai de {delay}s", true);
            
        // Configurer le redirector pour reprendre automatiquement au bon endroit
        GameSessionRedirector.ShouldResumeSession = true;
        GameSessionRedirector.ResumeMiniGameIndex = currentGameIndex - 1; // -1 car ResumeSessionAt va l'incrémenter
        GameSessionRedirector.TransitionDelay = delay;
        
        LogWithContext($"Retour à la scène principale: {mainMenuSceneName} pour transition", true);
        
        // Charger la scène principale
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Reprend la session de mini-jeux à l'index donné (utilisé pour le fallback automatique)
    /// </summary>
    public void ResumeSessionAt(int index)
    {
        if (index < 0 || index >= miniGames.Count)
        {
            Debug.LogWarning($"[GameSessionManager] Index de mini-jeu invalide pour la reprise ({index}), la session recommence au début.");
            StartGameSession();
            return;
        }
        currentGameIndex = index;
        Debug.Log($"[GameSessionManager] Reprise de la session au mini-jeu index {index} : {miniGames[index].sceneName}");
        StartCoroutine(StartNextMiniGameCoroutine());
    }


    /// <summary>
    /// Lance une session de mini-jeux (appelé depuis le menu principal)
    /// </summary>
    public void StartGameSession()
    {
        currentGameIndex = 0; // Reset l'index
        StartCoroutine(StartNextMiniGameCoroutine());
    }

    private IEnumerator StartNextMiniGameCoroutine()
    {
        if (currentGameIndex >= miniGames.Count)
        {
            Debug.Log("[GameSessionManager] Tous les mini-jeux sont terminés !");
            // Utiliser l'écran de chargement pour le retour au menu final
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "session_complete", null);
            }
            else
            {
                Debug.LogWarning("[GameSessionManager] LoadingScreenManager introuvable - Chargement direct");
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            yield break;
        }

        var currentMiniGame = miniGames[currentGameIndex];
        string sceneName = currentMiniGame.sceneName;
        string tipId = !string.IsNullOrEmpty(currentMiniGame.loadingTipId) 
            ? currentMiniGame.loadingTipId 
            : "default";

        if (enableDetailedLogs)
            LogWithContext($"Chargement du mini-jeu {currentGameIndex + 1}/{miniGames.Count} : {sceneName}", true);
        
        // Utiliser l'écran de chargement (doit être disponible dans la scène principale)
        if (LoadingScreenManager.Instance != null)
        {
            LogWithContext("Utilisation de LoadingScreenManager pour charger la scène", true);

            bool sceneLoaded = false;

            // Afficher l'écran de chargement et charger la scène avec titre et description
            LoadingScreenManager.Instance.ShowAndLoadScene(
                sceneName,
                tipId,
                () => { 
                    sceneLoaded = true;
                    LogWithContext($"Scène {sceneName} chargée avec succès", true);
                },
                currentMiniGame.displayName,
                currentMiniGame.description
            );

            // Attendre que la scène soit chargée
            yield return new WaitUntil(() => sceneLoaded);
        }
        else
        {
            // Fallback : chargement classique sans écran de chargement
            Debug.LogWarning("[GameSessionManager] LoadingScreenManager non disponible, chargement classique");
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            yield return loadOp;
        }

        yield return null; // attendre une frame que tout soit bien initialisé

        MiniGameBase loadedMiniGame = FindFirstObjectByType<MiniGameBase>();
        if (loadedMiniGame != null)
        {
            LogWithContext($"MiniGameBase trouvé: {loadedMiniGame.GetType().Name} sur {loadedMiniGame.gameObject.name}, actif: {loadedMiniGame.gameObject.activeInHierarchy}", true);
            loadedMiniGame.StartMiniGame(OnMiniGameFinished);
        }
        else
        {
            Debug.LogError("[GameSessionManager] MiniGameBase introuvable dans la scène chargée !");
        }
    }

    private void OnMiniGameFinished()
    {
        LogWithContext("Mini-jeu terminé via MiniGameBase.FinishMiniGame()", true);
            
        // Utiliser la méthode commune pour la transition
        TriggerTransitionToNextGame(defaultTransitionDelay);
    }

    /// <summary>
    /// Retourne la liste des noms de scènes de tous les mini-jeux de la session.
    /// </summary>
    public List<string> GetMiniGameSceneNames()
    {
        List<string> names = new List<string>();
        foreach (var mg in miniGames)
        {
            if (mg != null && !string.IsNullOrEmpty(mg.sceneName))
                names.Add(mg.sceneName);
        }
        return names;
    }

    /// <summary>
    /// Méthode pour logger avec contexte
    /// </summary>
    private void LogWithContext(string message, bool forceLog = false)
    {
        if (enableDetailedLogs || forceDebugMode || forceLog)
        {
            Debug.Log($"[GameSessionManager][Frame:{Time.frameCount}] {message}");
        }
    }

    /// <summary>
    /// Méthode de debug pour afficher l'état actuel
    /// </summary>
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("=== GAME SESSION MANAGER STATE ===");
        Debug.Log($"Current Game Index: {currentGameIndex}");
        Debug.Log($"Total Mini Games: {miniGames?.Count ?? 0}");
        Debug.Log($"LoadingScreenManager Available: {LoadingScreenManager.Instance != null}");
        Debug.Log($"Active Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Tests de création automatique du LoadingScreenManager
        if (LoadingScreenManager.Instance == null)
        {
            Debug.Log("*** TEST: Tentative de création automatique du LoadingScreenManager ***");
            EnsureLoadingScreenManager();
            Debug.Log($"LoadingScreenManager Available après création: {LoadingScreenManager.Instance != null}");
        }
        
        if (miniGames != null && currentGameIndex < miniGames.Count)
        {
            var current = miniGames[currentGameIndex];
            Debug.Log($"Next Mini Game: {current.sceneName} ({current.displayName})");
        }
        Debug.Log("==================================");
    }
}
