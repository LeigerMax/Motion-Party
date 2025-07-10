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

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
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
            // Retourner au menu principal quand la session est terminée
            ReturnToMainMenu();
            yield break;
        }

        string sceneName = miniGames[currentGameIndex].sceneName;

        Debug.Log($"Chargement de la scène du mini-jeu : {sceneName}");
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return loadOp;

        yield return null; // attendre une frame que tout soit bien initialisé

        MiniGameBase loadedMiniGame = FindFirstObjectByType<MiniGameBase>();
        if (loadedMiniGame != null)
        {
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
        var loadOp = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
        yield return loadOp;
    }
}
