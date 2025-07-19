using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameState { Intro, Playing, Results }


public class GameSessionManager : MonoBehaviour
{

    [SerializeField] private List<MiniGameInfo> miniGames;
    [SerializeField] private string mainMenuSceneName = "MiniGameManager"; // Nom de la scène du menu principal
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
            Debug.Log("Tous les mini-jeux sont terminés !");
            // Utiliser l'écran de chargement pour le retour au menu
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "session_complete", null);
            }
            else
            {
                ReturnToMainMenu();
            }
            yield break;
        }


        var currentMiniGame = miniGames[currentGameIndex];
        string sceneName = currentMiniGame.sceneName;
        string tipId = !string.IsNullOrEmpty(currentMiniGame.loadingTipId) 
            ? currentMiniGame.loadingTipId 
            : "default";

        Debug.Log($"Chargement de la scène du mini-jeu : {sceneName}");
        
        // Utiliser l'écran de chargement si disponible
        if (LoadingScreenManager.Instance != null)
        {
            Debug.Log("Utilisation de LoadingScreenManager pour charger la scène");

            bool sceneLoaded = false;

            // Afficher l'écran de chargement et charger la scène avec titre et description
            LoadingScreenManager.Instance.ShowAndLoadScene(
                sceneName,
                tipId,
                () => { sceneLoaded = true; },
                currentMiniGame.displayName,
                currentMiniGame.description
            );

            // Attendre que la scène soit chargée
            yield return new WaitUntil(() => sceneLoaded);
        }
        else
        {
            // Fallback : chargement classique sans écran de chargement
            Debug.LogWarning("LoadingScreenManager non disponible, chargement classique");
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            yield return loadOp;
        }

        yield return null; // attendre une frame que tout soit bien initialisé

        MiniGameBase loadedMiniGame = FindFirstObjectByType<MiniGameBase>();
        if (loadedMiniGame != null)
        {
            Debug.Log($"[GameSessionManager] MiniGameBase trouvé: {loadedMiniGame.GetType().Name} sur {loadedMiniGame.gameObject.name}, actif: {loadedMiniGame.gameObject.activeInHierarchy}");
            loadedMiniGame.StartMiniGame(OnMiniGameFinished);
        }
        else
        {
            Debug.LogError("MiniGameBase introuvable dans la scène chargée !");
        }
    }

    private void OnMiniGameFinished()
    {
        currentGameIndex++;
        StartCoroutine(StartNextMiniGameCoroutine());
    }

    /// <summary>
    /// Retourne au menu principal (ou à la scène principale)
    /// </summary>
    private void ReturnToMainMenu()
    {
        StartCoroutine(LoadMainMenuScene());
    }

    private IEnumerator LoadMainMenuScene()
    {
        Debug.Log($"Retour au menu principal : {mainMenuSceneName}");
        
        // Utiliser l'écran de chargement pour le retour au menu aussi
        if (LoadingScreenManager.Instance != null)
        {
            bool sceneLoaded = false;
            
            LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "menu_return", () =>
            {
                sceneLoaded = true;
            });
            
            yield return new WaitUntil(() => sceneLoaded);
        }
        else
        {
            // Fallback : chargement classique
            var loadOp = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
            yield return loadOp;
        }
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
}
