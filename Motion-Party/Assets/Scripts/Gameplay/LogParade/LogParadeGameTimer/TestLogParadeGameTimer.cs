using UnityEngine;

/// <summary>
/// Script de test pour démontrer l'utilisation du LogParadeGameTimer
/// Attachez ce script à un GameObject dans votre scène de test
/// </summary>
public class TestLogParadeGameTimer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Référence au LogParadeGameTimer à tester")]
    public LogParadeGameTimer gameTimer;
    
    [Header("Test Settings")]
    [Tooltip("Durée de test en secondes")]
    public float testDuration = 30f;
    
    void Start()
    {
        // Trouver automatiquement le game timer si non assigné
        if (gameTimer == null)
        {
            gameTimer = FindObjectOfType<LogParadeGameTimer>();
        }
        
        if (gameTimer == null)
        {
            Debug.LogError("[TestLogParadeGameTimer] Aucun LogParadeGameTimer trouvé dans la scène!");
            return;
        }
        
        // Configurer la durée de test
        gameTimer.SetGameDuration(testDuration);
        
        // S'abonner aux événements pour les tests
        gameTimer.OnGameStart.AddListener(OnGameStarted);
        gameTimer.OnGameEnd.AddListener(OnGameEnded);
        gameTimer.OnTimerTick.AddListener(OnTimerTick);
        
        Debug.Log("[TestLogParadeGameTimer] Test initialisé. Utilisez les touches pour tester.");
        Debug.Log("Touches de test:");
        Debug.Log("1 - Lancer une partie");
        Debug.Log("2 - Arrêter la partie");
        Debug.Log("3 - Redémarrer une partie");
        Debug.Log("4 - Changer durée à 15s");
        Debug.Log("5 - Changer durée à 60s");
    }
    
    void Update()
    {
        if (gameTimer == null) return;
        
        // Tests avec les touches du clavier
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameTimer.LaunchLevel();
            Debug.Log("[TEST] Lancement de partie demandé");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameTimer.StopGame();
            Debug.Log("[TEST] Arrêt de partie demandé");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gameTimer.RestartGame();
            Debug.Log("[TEST] Redémarrage de partie demandé");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gameTimer.SetGameDuration(15f);
            Debug.Log("[TEST] Durée changée à 15 secondes");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            gameTimer.SetGameDuration(60f);
            Debug.Log("[TEST] Durée changée à 60 secondes");
        }
    }
    
    // Événements de test
    private void OnGameStarted()
    {
        Debug.Log("[TEST EVENT] 🚀 Partie démarrée !");
        Debug.Log($"[TEST EVENT] Durée configurée: {gameTimer.GameDuration}s");
    }
    
    private void OnGameEnded()
    {
        Debug.Log("[TEST EVENT] 🏁 Partie terminée !");
        Debug.Log($"[TEST EVENT] Score final: {gameTimer.GetCurrentScore()}");
    }
    
    private void OnTimerTick(float timeRemaining)
    {
        // Afficher seulement certains moments pour éviter le spam
        if (timeRemaining <= 10f || timeRemaining % 10f == 0f)
        {
            Debug.Log($"[TEST EVENT] ⏰ Temps restant: {timeRemaining}s");
        }
    }
    
    void OnGUI()
    {
        // Interface de test dans le coin inférieur gauche
        GUILayout.BeginArea(new Rect(10, Screen.height - 250, 300, 240));
        GUILayout.Label("=== Test LogParadeGameTimer ===");
        
        if (gameTimer != null)
        {
            GUILayout.Label($"État: {(gameTimer.IsGameActive ? "EN COURS" : "ARRÊTÉ")}");
            GUILayout.Label($"Temps restant: {gameTimer.TimeRemaining:F1}s");
            GUILayout.Label($"Durée totale: {gameTimer.GameDuration}s");
            GUILayout.Label($"Score actuel: {gameTimer.GetCurrentScore()}");
            
            GUILayout.Space(10);
            GUILayout.Label("Contrôles:");
            GUILayout.Label("1 - Lancer partie");
            GUILayout.Label("2 - Arrêter partie");
            GUILayout.Label("3 - Redémarrer");
            GUILayout.Label("4 - Durée 15s");
            GUILayout.Label("5 - Durée 60s");
            
            GUILayout.Space(10);
            
            // Boutons GUI
            if (!gameTimer.IsGameActive)
            {
                if (GUILayout.Button("Lancer Partie"))
                {
                    gameTimer.LaunchLevel();
                }
            }
            else
            {
                if (GUILayout.Button("Arrêter Partie"))
                {
                    gameTimer.StopGame();
                }
            }
            
            if (GUILayout.Button("Redémarrer"))
            {
                gameTimer.RestartGame();
            }
        }
        else
        {
            GUILayout.Label("❌ LogParadeGameTimer non trouvé !");
        }
        
        GUILayout.EndArea();
    }
    
    void OnDestroy()
    {
        // Nettoyer les événements
        if (gameTimer != null)
        {
            gameTimer.OnGameStart.RemoveListener(OnGameStarted);
            gameTimer.OnGameEnd.RemoveListener(OnGameEnded);
            gameTimer.OnTimerTick.RemoveListener(OnTimerTick);
        }
    }
}
