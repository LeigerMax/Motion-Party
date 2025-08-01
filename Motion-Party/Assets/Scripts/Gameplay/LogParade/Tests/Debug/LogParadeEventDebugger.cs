using UnityEngine;
using Gameplay.LogParade.Core;

/// <summary>
/// Script de debug pour tester la connexion entre LogParadeGameController et LogParadeGameManager
/// </summary>
public class LogParadeEventDebugger : MonoBehaviour
{
    [Header("Debug Components")]
    public LogParadeGameController gameController;
    public LogParadeGameManager gameManager;
    
    [Header("Debug Actions")]
    public bool testEventConnection = false;
    public bool forceGameEnd = false;

    void Start()
    {
        if (gameController == null)
            gameController = FindFirstObjectByType<LogParadeGameController>();
        
        if (gameManager == null)
            gameManager = FindFirstObjectByType<LogParadeGameManager>();
        
        Debug.Log($"[EventDebugger] GameController trouvé: {gameController != null}");
        Debug.Log($"[EventDebugger] GameManager trouvé: {gameManager != null}");
        
        if (gameController != null)
        {
            gameController.OnGameCompleted += OnGameCompletedReceived;
            Debug.Log("[EventDebugger] Abonné à OnGameCompleted du GameController");
        }
    }
    
    void Update()
    {
        if (testEventConnection)
        {
            testEventConnection = false;
            TestEventConnection();
        }
        
        if (forceGameEnd)
        {
            forceGameEnd = false;
            ForceGameEnd();
        }
    }
    
    private void TestEventConnection()
    {
        Debug.Log("[EventDebugger] Test de la connexion d'événement...");
        if (gameController != null)
        {
            Debug.Log("[EventDebugger] Déclenchement manuel de OnGameCompleted");
            gameController.OnGameCompleted?.Invoke();
        }
    }
    
    private void ForceGameEnd()
    {
        Debug.Log("[EventDebugger] Forcer la fin du jeu...");
        if (gameController != null)
        {
            gameController.StopGame();
        }
    }
    
    private void OnGameCompletedReceived()
    {
        Debug.Log("[EventDebugger] *** EVENEMENT RECU *** OnGameCompleted déclenché !");
    }
    
    void OnDestroy()
    {
        if (gameController != null)
        {
            gameController.OnGameCompleted -= OnGameCompletedReceived;
        }
    }
}
