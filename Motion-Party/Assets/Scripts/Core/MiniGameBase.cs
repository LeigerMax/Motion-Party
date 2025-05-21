using UnityEngine;
using System;

public abstract class MiniGameBase : MonoBehaviour
{
     protected Action onGameFinished;

    // Appelé par le MiniGameManager
    public virtual void StartMiniGame(Action onFinishedCallback)
    {
        onGameFinished = onFinishedCallback;
        gameObject.SetActive(true);
        Launch(); // chaque mini-jeu implémentera ça
    }

    // Chaque mini-jeu doit définir sa propre logique de lancement
    protected abstract void Launch();

    // À appeler quand le mini-jeu est terminé
    protected void FinishMiniGame()
    {
        gameObject.SetActive(false);
        onGameFinished?.Invoke();
    }

    // Ajoute cette propriété pour lier le nom de la scène au mini-jeu
    public virtual string SceneName => gameObject.scene.name;

}
