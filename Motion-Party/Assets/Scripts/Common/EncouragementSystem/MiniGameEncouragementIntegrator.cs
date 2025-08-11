using UnityEngine;

/// <summary>
/// Composant d'intégration du système d'encouragement pour les mini-jeux
/// Peut être ajouté à n'importe quel mini-jeu pour activer les messages d'encouragement
/// </summary>
public class MiniGameEncouragementIntegrator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool autoStartOnGameStart = true;
    [SerializeField] private bool autoStopOnGameEnd = true;
    
    [Header("Références")]
    [SerializeField] private EncouragementManager encouragementManager;
    
    [Header("Événements du jeu (optionnel)")]
    [SerializeField] private string gameStartEventName = "OnGameStarted";
    [SerializeField] private string gameEndEventName = "OnGameCompleted";

    private bool isSystemActive = false;

    #region Unity Lifecycle
    void Start()
    {
        // Rechercher le manager d'encouragement si non assigné
        if (encouragementManager == null)
        {
            encouragementManager = FindFirstObjectByType<EncouragementManager>();
            if (encouragementManager == null)
            {
                // Créer automatiquement un manager d'encouragement
                GameObject managerGO = new GameObject("EncouragementManager");
                managerGO.transform.SetParent(transform);
                encouragementManager = managerGO.AddComponent<EncouragementManager>();
                
                Debug.Log("[MiniGameEncouragementIntegrator] EncouragementManager créé automatiquement");
            }
        }

        // Si auto-start est activé, chercher les événements du jeu
        if (autoStartOnGameStart)
        {
            TrySubscribeToGameEvents();
        }
    }

    void OnDestroy()
    {
        // Arrêter le système si actif
        if (isSystemActive && encouragementManager != null)
        {
            encouragementManager.StopEncouragement();
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Démarre manuellement le système d'encouragement
    /// </summary>
    public void StartEncouragement()
    {
        if (encouragementManager != null && !isSystemActive)
        {
            encouragementManager.StartEncouragement();
            isSystemActive = true;
            Debug.Log("[MiniGameEncouragementIntegrator] Système d'encouragement démarré");
        }
    }

    /// <summary>
    /// Arrête manuellement le système d'encouragement
    /// </summary>
    public void StopEncouragement()
    {
        if (encouragementManager != null && isSystemActive)
        {
            encouragementManager.StopEncouragement();
            isSystemActive = false;
            Debug.Log("[MiniGameEncouragementIntegrator] Système d'encouragement arrêté");
        }
    }

    /// <summary>
    /// Affiche immédiatement un message d'encouragement
    /// </summary>
    public void ShowImmediateEncouragement()
    {
        if (encouragementManager != null)
        {
            encouragementManager.ShowRandomMessage();
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Essaie de s'abonner automatiquement aux événements du jeu
    /// </summary>
    private void TrySubscribeToGameEvents()
    {
        // Rechercher des composants de type MiniGameBase ou similaires
        var miniGameBase = GetComponent<MiniGameBase>();
        if (miniGameBase != null)
        {
            // TODO: S'abonner aux événements de MiniGameBase si disponibles
            Debug.Log("[MiniGameEncouragementIntegrator] MiniGameBase détecté");
        }

        // Rechercher des gestionnaires de jeu spécifiques
        TrySubscribeToLogParade();
        TrySubscribeToFirefly();
        TrySubscribeToMusicNote();
    }

    /// <summary>
    /// Tentative d'abonnement aux événements LogParade
    /// </summary>
    private void TrySubscribeToLogParade()
    {
        // Rechercher le LogParadeGameController sans référence directe au type
        var allComponents = FindObjectsOfType<MonoBehaviour>();
        MonoBehaviour logParadeController = null;
        
        foreach (var component in allComponents)
        {
            if (component.GetType().Name == "LogParadeGameController")
            {
                logParadeController = component;
                break;
            }
        }

        if (logParadeController != null)
        {
            // Utiliser la réflexion pour s'abonner aux événements
            var gameStartedField = logParadeController.GetType().GetField("OnGameStarted");
            var gameCompletedField = logParadeController.GetType().GetField("OnGameCompleted");

            if (gameStartedField != null)
            {
                var gameStartedAction = gameStartedField.GetValue(logParadeController) as System.Action;
                gameStartedAction += OnGameStarted;
                gameStartedField.SetValue(logParadeController, gameStartedAction);
            }

            if (gameCompletedField != null)
            {
                var gameCompletedAction = gameCompletedField.GetValue(logParadeController) as System.Action;
                gameCompletedAction += OnGameEnded;
                gameCompletedField.SetValue(logParadeController, gameCompletedAction);
            }

            Debug.Log("[MiniGameEncouragementIntegrator] Abonné aux événements LogParade");
        }
    }

    /// <summary>
    /// Tentative d'abonnement aux événements Firefly
    /// </summary>
    private void TrySubscribeToFirefly()
    {
        // TODO: Implémenter quand les composants Firefly seront disponibles
        var fireflyController = FindFirstObjectByType<MonoBehaviour>();
        // Rechercher un composant avec "Firefly" dans le nom
        var allComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (var component in allComponents)
        {
            if (component.GetType().Name.Contains("Firefly") && 
                component.GetType().Name.Contains("Controller"))
            {
                Debug.Log($"[MiniGameEncouragementIntegrator] Composant Firefly détecté: {component.GetType().Name}");
                // TODO: S'abonner aux événements spécifiques
                break;
            }
        }
    }

    /// <summary>
    /// Tentative d'abonnement aux événements MusicNote
    /// </summary>
    private void TrySubscribeToMusicNote()
    {
        // TODO: Implémenter quand les composants MusicNote seront disponibles
        var allComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (var component in allComponents)
        {
            if (component.GetType().Name.Contains("MusicNote") && 
                component.GetType().Name.Contains("Controller"))
            {
                Debug.Log($"[MiniGameEncouragementIntegrator] Composant MusicNote détecté: {component.GetType().Name}");
                // TODO: S'abonner aux événements spécifiques
                break;
            }
        }
    }

    /// <summary>
    /// Callback appelé quand le jeu démarre
    /// </summary>
    private void OnGameStarted()
    {
        if (autoStartOnGameStart)
        {
            StartEncouragement();
        }
    }

    /// <summary>
    /// Callback appelé quand le jeu se termine
    /// </summary>
    private void OnGameEnded()
    {
        if (autoStopOnGameEnd)
        {
            StopEncouragement();
        }
    }
    #endregion

    #region Editor Helpers
    #if UNITY_EDITOR
    [Header("Tests en éditeur")]
    [SerializeField] private bool testInEditor = false;
    
    void Update()
    {
        if (testInEditor && Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ShowImmediateEncouragement();
            }
        }
    }
    #endif
    #endregion
}
