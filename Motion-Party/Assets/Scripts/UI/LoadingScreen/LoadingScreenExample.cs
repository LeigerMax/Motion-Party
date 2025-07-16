using UnityEngine;

/// <summary>
/// Script d'exemple montrant comment utiliser le système LoadingScreen
/// </summary>
public class LoadingScreenExample : MonoBehaviour
{
    [Header("Test du système")]
    [SerializeField] private string testSceneName = "TestScene";
    [SerializeField] private string testTipId = "movement_game";
    
    [Header("Boutons de test (utilisez les méthodes dans l'Inspector)")]
    [SerializeField] private bool showTestButtons = true;
    
    private void Start()
    {
        // Exemple d'utilisation au démarrage
        if (LoadingScreenManager.Instance == null)
        {
            Debug.LogWarning("LoadingScreenManager non trouvé ! Assurez-vous qu'il est dans la scène.");
        }
    }
    
    /// <summary>
    /// Exemple : Afficher un écran de chargement simple
    /// </summary>
    [ContextMenu("Test - Afficher écran de chargement")]
    public void TestShowLoadingScreen()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Show(testTipId);
            
            // Simuler un chargement et cacher après 3 secondes
            Invoke(nameof(TestHideLoadingScreen), 3f);
        }
    }
    
    /// <summary>
    /// Exemple : Cacher l'écran de chargement
    /// </summary>
    [ContextMenu("Test - Cacher écran de chargement")]
    public void TestHideLoadingScreen()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Hide();
        }
    }
    
    /// <summary>
    /// Exemple : Charger une scène avec écran de chargement
    /// </summary>
    [ContextMenu("Test - Charger scène avec loading")]
    public void TestLoadSceneWithLoading()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.ShowAndLoadScene(testSceneName, testTipId, () =>
            {
                Debug.Log("Scène chargée avec succès !");
            });
        }
    }
    
    /// <summary>
    /// Exemple : Mettre à jour la progression manuellement
    /// </summary>
    [ContextMenu("Test - Simuler progression")]
    public void TestUpdateProgress()
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.Show(testTipId);
            StartCoroutine(SimulateProgress());
        }
    }
    
    /// <summary>
    /// Simule une progression de chargement
    /// </summary>
    private System.Collections.IEnumerator SimulateProgress()
    {
        for (float i = 0f; i <= 1f; i += 0.1f)
        {
            LoadingScreenManager.Instance.UpdateProgress(i);
            yield return new WaitForSeconds(0.2f);
        }
        
        yield return new WaitForSeconds(1f);
        LoadingScreenManager.Instance.Hide();
    }
    
    /// <summary>
    /// Exemple d'intégration avec un système de mini-jeux personnalisé
    /// </summary>
    public void LoadNextMiniGame(string sceneName, string tipId)
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.ShowAndLoadScene(sceneName, tipId, OnMiniGameLoaded);
        }
        else
        {
            // Fallback si le système n'est pas disponible
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// Callback appelé quand un mini-jeu est chargé
    /// </summary>
    private void OnMiniGameLoaded()
    {
        Debug.Log("Mini-jeu chargé ! Initialisation...");
        
        // Ici, vous pourriez :
        // - Initialiser le mini-jeu
        // - Configurer les paramètres
        // - Démarrer la logique de jeu
        
        // Exemple : Chercher et démarrer le mini-jeu
        var miniGame = FindFirstObjectByType<MiniGameBase>();
        if (miniGame != null)
        {
            miniGame.StartMiniGame(() =>
            {
                Debug.Log("Mini-jeu terminé !");
                // Charger le prochain niveau ou retourner au menu
            });
        }
    }
    
    /// <summary>
    /// Exemple de configuration des données de chargement au runtime
    /// </summary>
    [ContextMenu("Test - Créer données de test")]
    public void CreateTestLoadingData()
    {
        // Créer des données de test
        var loadingData = ScriptableObject.CreateInstance<LoadingScreenData>();
        
        loadingData.defaultTipText = "Préparez-vous pour l'action !";
        loadingData.tips = new LoadingScreenData.LoadingTip[]
        {
            new LoadingScreenData.LoadingTip
            {
                tipId = "test_tip",
                tipText = "Ceci est un test du système de chargement !",
                backgroundColor = Color.cyan
            }
        };
        
        Debug.Log("Données de test créées (runtime seulement)");
    }
    
    // Méthodes pour l'interface Unity Inspector
    private void OnGUI()
    {
        if (!showTestButtons) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("LoadingScreen Test Panel");
        
        if (GUILayout.Button("Afficher Loading Screen"))
            TestShowLoadingScreen();
            
        if (GUILayout.Button("Cacher Loading Screen"))
            TestHideLoadingScreen();
            
        if (GUILayout.Button("Simuler Progression"))
            TestUpdateProgress();
            
        GUILayout.EndArea();
    }
}
