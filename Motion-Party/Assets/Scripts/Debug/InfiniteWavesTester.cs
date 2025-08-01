using UnityEngine;
using Gameplay.MusicNotePress;

/// <summary>
/// Script de test pour le système de vagues infinies de MusicNote
/// </summary>
public class InfiniteWavesTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool enableAutoTesting = false;
    [SerializeField] private float testInterval = 5f;
    [SerializeField] private bool simulateSuccess = true;
    
    [Header("References")]
    [SerializeField] private MusicNoteGameController gameController;
    [SerializeField] private UIManager uiManager;
    
    private void Start()
    {
        // Auto-trouver les composants si pas assignés
        if (gameController == null)
            gameController = FindFirstObjectByType<MusicNoteGameController>();
            
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
            
        if (enableAutoTesting)
        {
            InvokeRepeating(nameof(SimulateWaveResult), testInterval, testInterval);
        }
    }
    
    [ContextMenu("Start Infinite Waves Game")]
    public void StartInfiniteWavesGame()
    {
        if (gameController == null)
        {
            Debug.LogError("[InfiniteWavesTester] GameController non trouvé !");
            return;
        }
        
        Debug.Log("=== DÉMARRAGE DU TEST DES VAGUES INFINIES ===");
        gameController.StartGame();
    }
    
    [ContextMenu("Simulate Wave Success")]
    public void SimulateWaveSuccess()
    {
        Debug.Log("=== SIMULATION SUCCÈS DE VAGUE ===");
        // Cette méthode simulerait un succès de vague
        // En réalité, cela devrait venir du système de détection de notes
        TestWaveCompletion(true);
    }
    
    [ContextMenu("Simulate Wave Failure")]
    public void SimulateWaveFailure()
    {
        Debug.Log("=== SIMULATION ÉCHEC DE VAGUE ===");
        TestWaveCompletion(false);
    }
    
    [ContextMenu("Get Current Wave Info")]
    public void GetCurrentWaveInfo()
    {
        if (gameController == null) return;
        
        Debug.Log("=== INFORMATIONS DE VAGUE ACTUELLE ===");
        Debug.Log($"Vague actuelle: {gameController.GetCurrentWave()}");
        Debug.Log($"Nombre de notes: {gameController.GetCurrentWaveNotesCount()}");
        Debug.Log($"Score actuel: {gameController.GetCurrentScore()}");
        Debug.Log($"Vagues infinies activées: {gameController.IsInfiniteWavesEnabled()}");
        Debug.Log("==========================================");
    }
    
    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        if (gameController == null) return;
        
        Debug.Log("=== RÉINITIALISATION DU JEU ===");
        gameController.ResetGame();
    }
    
    private void TestWaveCompletion(bool success)
    {
        if (gameController == null) return;
        
        Debug.Log($"Test de complétion de vague - Succès: {success}");
        
        // Afficher les informations avant
        GetCurrentWaveInfo();
        
        // Simuler le résultat (dans un vrai jeu, cela viendrait du NoteInputManager)
        // gameController.HandleSequenceResult(success);
        // Note: HandleSequenceResult est privée, donc on ne peut pas l'appeler directement
        // Il faudrait exposer une méthode publique ou utiliser Events
        
        Debug.Log($"Simulation {(success ? "RÉUSSIE" : "ÉCHOUÉE")}");
    }
    
    private void SimulateWaveResult()
    {
        if (gameController == null) return;
        
        // Alterner entre succès et échec pour les tests
        simulateSuccess = !simulateSuccess;
        TestWaveCompletion(simulateSuccess);
    }
    
    private void OnGUI()
    {
        if (gameController == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== INFINITE WAVES TESTER ===");
        GUILayout.Label($"Vague: {gameController.GetCurrentWave()}");
        GUILayout.Label($"Notes: {gameController.GetCurrentWaveNotesCount()}");
        GUILayout.Label($"Score: {gameController.GetCurrentScore()}");
        GUILayout.Label($"Infinies: {gameController.IsInfiniteWavesEnabled()}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Démarrer Jeu"))
            StartInfiniteWavesGame();
            
        if (GUILayout.Button("Simuler Succès"))
            SimulateWaveSuccess();
            
        if (GUILayout.Button("Simuler Échec"))
            SimulateWaveFailure();
            
        if (GUILayout.Button("Réinitialiser"))
            ResetGame();
            
        GUILayout.EndArea();
    }
}
