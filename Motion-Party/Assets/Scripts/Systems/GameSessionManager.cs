using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameState { Intro, Playing, Results }

public class GameSessionManager : MonoBehaviour
{
    [SerializeField] private List<MiniGameInfo> miniGames;
    private int currentGameIndex = 0;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(StartNextMiniGameCoroutine());
    }

    private IEnumerator StartNextMiniGameCoroutine()
    {
        if (currentGameIndex >= miniGames.Count)
        {
            Debug.Log("Tous les mini-jeux sont terminés !");
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
}
