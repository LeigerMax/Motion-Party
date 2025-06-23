using UnityEngine;

/// <summary>
/// Script de test pour démontrer l'utilisation du LogParadeScoreManager
/// Attachez ce script à un GameObject dans votre scène de test
/// </summary>
public class TestLogParadeScoreManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Référence au LogParadeScoreManager à tester")]
    public LogParadeScoreManager scoreManager;
    
    void Start()
    {
        // Trouver automatiquement le score manager si non assigné
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<LogParadeScoreManager>();
        }
        
        if (scoreManager == null)
        {
            Debug.LogError("[TestLogParadeScoreManager] Aucun LogParadeScoreManager trouvé dans la scène!");
            return;
        }
        
        // Démarrer le système de score après 2 secondes
        Invoke(nameof(StartScoring), 2f);
        
        Debug.Log("[TestLogParadeScoreManager] Test initialisé. Le score commencera dans 2 secondes.");
    }
    
    void Update()
    {
        if (scoreManager == null) return;
        
        // Tests avec les touches du clavier (pour les tests en éditeur)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            scoreManager.StartScoring();
            Debug.Log("[TEST] Score system started");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            scoreManager.StopScoring();
            Debug.Log("[TEST] Score system stopped");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            scoreManager.ResetScore();
            Debug.Log("[TEST] Score reset");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            scoreManager.AddBonusPoints(10);
            Debug.Log("[TEST] Added 10 bonus points");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            int currentScore = scoreManager.GetCurrentScore();
            Debug.Log($"[TEST] Current score: {currentScore}");
        }
    }
    
    private void StartScoring()
    {
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
            Debug.Log("[TestLogParadeScoreManager] ✅ Système de score démarré automatiquement.");
        }
    }
    
    void OnGUI()
    {
        // Affichage des instructions de test
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== Test LogParadeScoreManager ===");
        GUILayout.Label("Touches de test:");
        GUILayout.Label("1 - Démarrer le score");
        GUILayout.Label("2 - Arrêter le score");
        GUILayout.Label("3 - Reset le score");
        GUILayout.Label("4 - Ajouter 10 points bonus");
        GUILayout.Label("5 - Afficher le score actuel");
        GUILayout.Label("");
        if (scoreManager != null)
        {
            GUILayout.Label($"Score actuel: {scoreManager.GetCurrentScore()}");
            GUILayout.Label($"Système actif: {scoreManager.IsScoring}");
        }
        GUILayout.EndArea();
    }
}
