using UnityEngine;
using UnityEditor;

/// <summary>
/// Script utilitaire pour configurer automatiquement une scène LogParade
/// Facilite la création de la hiérarchie d'objets et la configuration
/// </summary>
public class LogParadeSceneSetup : MonoBehaviour
{
    [Header("Configuration")]
    public LogParadeConfig config;
    
    [Header("Auto-Setup Options")]
    public bool createPlayerAvatar = true;
    public bool createLaneVisualizer = true;
    public bool createInputSimulator = false;
    public bool createUICanvas = true;
    
    [Header("Prefabs (Optional)")]
    public GameObject playerAvatarPrefab;
    public GameObject uiCanvasPrefab;

    
    /// <summary>
    /// Configure automatiquement la scène LogParade
    /// </summary>
    [ContextMenu("Setup LogParade Scene")]
    public void SetupScene()
    {
        Debug.Log("Configuration automatique de la scène LogParade...");
        
        // Créer le manager principal s'il n'existe pas
        CreateMainManager();
        
        // Créer les composants selon les options
        if (createPlayerAvatar) CreatePlayerAvatar();
        if (createLaneVisualizer) CreateLaneVisualizer();
        if (createInputSimulator) CreateInputSimulator();
        if (createUICanvas) CreateUICanvas();
        
        // Appliquer la configuration si disponible
        if (config != null)
        {
            config.ApplyToComponents();
        }
        
        // Connecter les références
        ConnectReferences();
        
        Debug.Log("Configuration de la scène LogParade terminée !");
    }
    
    /// <summary>
    /// Crée le manager principal avec les composants de base
    /// </summary>
    private void CreateMainManager()
    {
        GameObject manager = GameObject.Find("LogParadeManager");
        if (manager == null)
        {
            manager = new GameObject("LogParadeManager");
        }
        
        // Ajouter LogParadeGameController
        if (manager.GetComponent<LogParadeGameController>() == null)
        {
            manager.AddComponent<LogParadeGameController>();
        }
        
        // Ajouter UDPReceive
        if (manager.GetComponent<Core.UDPReceive>() == null)
        {
            manager.AddComponent<Core.UDPReceive>();
        }
        
        // Créer le tracker
        GameObject tracker = GameObject.Find("LateralTracker");
        if (tracker == null)
        {
            tracker = new GameObject("LateralTracker");
            tracker.transform.SetParent(manager.transform);
        }
        
        if (tracker.GetComponent<LogParadeLateralTracker>() == null)
        {
            tracker.AddComponent<LogParadeLateralTracker>();
        }
    }
    
    /// <summary>
    /// Crée l'avatar du joueur
    /// </summary>
    private void CreatePlayerAvatar()
    {
        GameObject avatar = GameObject.Find("PlayerAvatar");
        if (avatar == null)
        {
            if (playerAvatarPrefab != null)
            {
                avatar = Instantiate(playerAvatarPrefab);
                avatar.name = "PlayerAvatar";
            }
            else
            {
                // Créer un avatar simple avec un cube
                avatar = new GameObject("PlayerAvatar");
                
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.SetParent(avatar.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one * 0.5f;
                visual.name = "AvatarVisual";
                
                // Ajouter un material coloré
                Renderer renderer = visual.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.cyan;
                renderer.material = mat;
            }
        }
        
        if (avatar.GetComponent<LogParadePlayerAvatar>() == null)
        {
            avatar.AddComponent<LogParadePlayerAvatar>();
        }
    }
    
    /// <summary>
    /// Crée le visualisateur de voies
    /// </summary>
    private void CreateLaneVisualizer()
    {
        GameObject visualizer = GameObject.Find("LaneVisualizer");
        if (visualizer == null)
        {
            visualizer = new GameObject("LaneVisualizer");
        }
        
        LogParadeLaneVisualizer viz = visualizer.GetComponent<LogParadeLaneVisualizer>();
        if (viz == null)
        {
            viz = visualizer.AddComponent<LogParadeLaneVisualizer>();
        }
        
        // Générer les voies automatiquement
        viz.GenerateLanes();
    }
    
    /// <summary>
    /// Crée le simulateur d'entrée pour les tests
    /// </summary>
    private void CreateInputSimulator()
    {
        GameObject simulator = GameObject.Find("InputSimulator");
        if (simulator == null)
        {
            simulator = new GameObject("InputSimulator");
        }
        
        if (simulator.GetComponent<LogParadeInputSimulator>() == null)
        {
            simulator.AddComponent<LogParadeInputSimulator>();
        }
    }
    
    /// <summary>
    /// Crée le canvas UI
    /// </summary>
    private void CreateUICanvas()
    {
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null && existingCanvas.name.Contains("LogParade"))
        {
            return; // Canvas déjà créé
        }
        
        GameObject canvas;
        if (uiCanvasPrefab != null)
        {
            canvas = Instantiate(uiCanvasPrefab);
            canvas.name = "LogParadeUI";
        }
        else
        {
            // Créer un canvas basique
            canvas = new GameObject("LogParadeUI");
            Canvas canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Ajouter le UIManager
        if (canvas.GetComponent<LogParadeUIManager>() == null)
        {
            canvas.AddComponent<LogParadeUIManager>();
        }
    }
    
    /// <summary>
    /// Connecte les références entre les composants
    /// </summary>
    private void ConnectReferences()
    {
        // Trouver les composants
        LogParadeGameController gameController = FindObjectOfType<LogParadeGameController>();
        LogParadeLateralTracker tracker = FindObjectOfType<LogParadeLateralTracker>();
        LogParadePlayerAvatar avatar = FindObjectOfType<LogParadePlayerAvatar>();
        LogParadeUIManager uiManager = FindObjectOfType<LogParadeUIManager>();
        LogParadeInputSimulator simulator = FindObjectOfType<LogParadeInputSimulator>();
        Core.UDPReceive udpReceive = FindObjectOfType<Core.UDPReceive>();
        
        // Connecter GameController
        if (gameController != null)
        {
            gameController.udpReceive = udpReceive;
            gameController.uiManager = uiManager;
            gameController.lateralTracker = tracker;
            gameController.playerAvatar = avatar;
        }
        
        // Connecter Tracker
        if (tracker != null)
        {
            tracker.udpReceive = udpReceive;
        }
        
        // Connecter Simulator
        if (simulator != null)
        {
            simulator.targetUDPReceive = udpReceive;
        }
    }
    
    /// <summary>
    /// Nettoie la scène en supprimant tous les objets LogParade
    /// </summary>
    [ContextMenu("Clean LogParade Scene")]
    public void CleanScene()
    {
        string[] objectsToRemove = {
            "LogParadeManager", "PlayerAvatar", "LaneVisualizer", 
            "InputSimulator", "LogParadeUI", "LateralTracker"
        };
        
        foreach (string objName in objectsToRemove)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
        }
        
        Debug.Log("Scène LogParade nettoyée.");
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Interface de setup rapide
        GUILayout.BeginArea(new Rect(10, Screen.height - 100, 200, 90));
        GUILayout.Label("=== SCENE SETUP ===");
        
        if (GUILayout.Button("Setup Scene"))
        {
            SetupScene();
        }
        
        if (GUILayout.Button("Clean Scene"))
        {
            CleanScene();
        }
        
        GUILayout.EndArea();
    }
}
