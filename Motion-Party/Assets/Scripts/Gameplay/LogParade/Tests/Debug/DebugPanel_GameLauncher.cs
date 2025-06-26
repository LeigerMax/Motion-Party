using UnityEngine;

/// <summary>
/// Panneau de debug pour le système de lancement coordonné LogParade.
/// Affiche l'état des systèmes et permet de contrôler le lancement du jeu.
/// </summary>
public class DebugPanel_GameLauncher : BaseDebugPanel
{
    [Header("Game Launcher References")]
    [Tooltip("Référence au LogParadeGameLauncher")]
    [SerializeField] private LogParadeGameLauncher gameLauncher;
    
    [Tooltip("Recherche automatiquement le GameLauncher")]
    [SerializeField] private bool autoFindGameLauncher = true;
    
    // Données en cache
    private bool isLaunching = false;
    private bool gameStarted = false;
    private bool calibrationCompleted = false;
    private int systemsFound = 0;
    private string lastLaunchStatus = "Non démarré";

    protected override void Start()
    {
        visibleAtStart = true;
        SetVisible(true);
        
        panelTitle = "🚀 Game Launcher";
          // Auto-découverte du GameLauncher
        if (autoFindGameLauncher && gameLauncher == null)
        {
            gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
            if (gameLauncher != null)
            {
                Debug.Log("[DebugPanel_GameLauncher] GameLauncher trouvé automatiquement");
                SubscribeToEvents();
            }
        }
        
        base.Start();
    }

    protected override void RefreshData()
    {
        if (gameLauncher == null) return;
        
        // Récupérer les données via réflexion
        isLaunching = GetIsLaunching();
        gameStarted = GetGameStarted();
        calibrationCompleted = GetCalibrationCompleted();
        systemsFound = CountFoundSystems();
    }

    protected override void DrawPanelContent()
    {
        if (gameLauncher == null)
        {
            GUILayout.Label("<color=red>GameLauncher non trouvé!</color>");
              if (GUILayout.Button("Rechercher"))
            {
                gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
                if (gameLauncher != null)
                {
                    SubscribeToEvents();
                }
            }
            return;
        }
        
        GUILayout.BeginVertical();
        
        // Section État Général
        DrawGeneralStatusSection();
        
        GUILayout.Space(5);
        
        // Section Systèmes
        DrawSystemsSection();
        
        GUILayout.Space(5);
        
        // Section Contrôles
        DrawControlsSection();
        
        GUILayout.EndVertical();
    }

    /// <summary>
    /// Dessine la section état général
    /// </summary>
    private void DrawGeneralStatusSection()
    {
        GUILayout.Label("<b>🎮 ÉTAT GÉNÉRAL</b>");
        
        // Statut de lancement
        string launchingStatus = isLaunching ? "<color=yellow>EN COURS</color>" : "<color=gray>ARRÊTÉ</color>";
        GUILayout.Label($"Lancement: {launchingStatus}");
        
        // Statut du jeu
        string gameStatus = gameStarted ? "<color=green>DÉMARRÉ</color>" : "<color=orange>EN ATTENTE</color>";
        GUILayout.Label($"Jeu: {gameStatus}");
        
        // Statut de calibration
        string calibrationStatus = calibrationCompleted ? "<color=green>TERMINÉE</color>" : "<color=red>REQUISE</color>";
        GUILayout.Label($"Calibration: {calibrationStatus}");
        
        // Dernier statut
        GUILayout.Label($"Dernière action: {lastLaunchStatus}");
    }

    /// <summary>
    /// Dessine la section systèmes
    /// </summary>
    private void DrawSystemsSection()
    {
        GUILayout.Label("<b>🔧 SYSTÈMES</b>");
        
        // Nombre de systèmes trouvés
        string systemsColor = systemsFound >= 4 ? "green" : (systemsFound >= 2 ? "yellow" : "red");
        GUILayout.Label($"Détectés: <color={systemsColor}>{systemsFound}/4</color>");
        
        // Détail des systèmes (si possible)
        if (gameLauncher != null)
        {
            var gameController = GetSystemComponent("gameController");
            var gameTimer = GetSystemComponent("gameTimer");
            var logGenerator = GetSystemComponent("logGenerator");
            var scoreManager = GetSystemComponent("scoreManager");
            
            GUILayout.Label($"• Controller: {GetSystemStatusIcon(gameController)}");
            GUILayout.Label($"• Timer: {GetSystemStatusIcon(gameTimer)}");
            GUILayout.Label($"• Generator: {GetSystemStatusIcon(logGenerator)}");
            GUILayout.Label($"• Score: {GetSystemStatusIcon(scoreManager)}");
        }
    }

    /// <summary>
    /// Dessine la section contrôles
    /// </summary>
    private void DrawControlsSection()
    {
        GUILayout.Label("<b>🎯 CONTRÔLES</b>");
        
        // Bouton de lancement principal
        if (!isLaunching && !gameStarted)
        {
            if (GUILayout.Button("Lancer le Jeu"))
            {
                LaunchGame();
            }
        }
        
        // Bouton de relancement
        if (gameStarted && GUILayout.Button("Relancer"))
        {
            RestartGame();
        }
        
        // Bouton d'arrêt
        if (gameStarted && GUILayout.Button("Arrêter Tout"))
        {
            StopAllSystems();
        }
        
        // Bouton de diagnostic
        if (GUILayout.Button("Diagnostic"))
        {
            RunDiagnostic();
        }
    }

    protected override float GetEstimatedHeight()
    {
        return gameLauncher != null ? 220f : 80f;
    }

    /// <summary>
    /// S'abonne aux événements du GameLauncher
    /// </summary>
    private void SubscribeToEvents()
    {
        if (gameLauncher == null) return;
        
        // Essayer de s'abonner aux événements via réflexion
        try
        {
            var onLaunchStartedEvent = gameLauncher.GetType().GetField("OnGameLaunchStarted");
            if (onLaunchStartedEvent != null)
            {
                var eventValue = onLaunchStartedEvent.GetValue(gameLauncher) as System.Action;
                if (eventValue != null)
                {
                    eventValue += OnLaunchStarted;
                }
            }
            
            var onLaunchCompletedEvent = gameLauncher.GetType().GetField("OnGameLaunchCompleted");
            if (onLaunchCompletedEvent != null)
            {
                var eventValue = onLaunchCompletedEvent.GetValue(gameLauncher) as System.Action;
                if (eventValue != null)
                {
                    eventValue += OnLaunchCompleted;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[DebugPanel_GameLauncher] Impossible de s'abonner aux événements: {ex.Message}");
        }
    }

    /// <summary>
    /// Méthodes d'accès aux données privées
    /// </summary>
    private bool GetIsLaunching()
    {
        if (gameLauncher == null) return false;
        
        var field = gameLauncher.GetType().GetField("isLaunching", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(gameLauncher);
        }
        
        return false;
    }

    private bool GetGameStarted()
    {
        if (gameLauncher == null) return false;
        
        var field = gameLauncher.GetType().GetField("gameFullyStarted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(gameLauncher);
        }
        
        return false;
    }

    private bool GetCalibrationCompleted()
    {
        if (gameLauncher == null) return false;
        
        // Utiliser la méthode IsCalibrationCompleted du GameLauncher
        var method = gameLauncher.GetType().GetMethod("IsCalibrationCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            return (bool)method.Invoke(gameLauncher, null);
        }
        
        return false;
    }

    private int CountFoundSystems()
    {
        if (gameLauncher == null) return 0;
        
        int count = 0;
        var systemFields = new string[] { "gameController", "gameTimer", "logGenerator", "scoreManager" };
        
        foreach (var fieldName in systemFields)
        {
            var field = gameLauncher.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var value = field.GetValue(gameLauncher);
                if (value != null) count++;
            }
        }
        
        return count;
    }

    private object GetSystemComponent(string fieldName)
    {
        if (gameLauncher == null) return null;
        
        var field = gameLauncher.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        return field?.GetValue(gameLauncher);
    }

    private string GetSystemStatusIcon(object component)
    {
        if (component == null) return "<color=red>❌</color>";
        
        var unityObj = component as UnityEngine.Object;
        if (unityObj == null) return "<color=red>❌</color>";
        
        var gameObject = unityObj as GameObject;
        if (gameObject != null)
        {
            return gameObject.activeInHierarchy ? "<color=green>✅</color>" : "<color=yellow>⚠️</color>";
        }
        
        var monoBehaviour = unityObj as MonoBehaviour;
        if (monoBehaviour != null)
        {
            return (monoBehaviour.enabled && monoBehaviour.gameObject.activeInHierarchy) ? 
                "<color=green>✅</color>" : "<color=yellow>⚠️</color>";
        }
        
        return "<color=green>✅</color>";
    }

    /// <summary>
    /// Actions de contrôle
    /// </summary>
    private void LaunchGame()
    {
        if (gameLauncher == null) return;
        
        lastLaunchStatus = "Lancement demandé";
        gameLauncher.LaunchFullGame();
        Debug.Log("[DebugPanel_GameLauncher] Lancement du jeu demandé");
    }

    private void RestartGame()
    {
        if (gameLauncher == null) return;
        
        lastLaunchStatus = "Redémarrage demandé";
        gameLauncher.RestartGame();
        Debug.Log("[DebugPanel_GameLauncher] Redémarrage du jeu demandé");
    }    private void StopAllSystems()
    {
        if (gameLauncher == null) return;
        
        lastLaunchStatus = "Redémarrage demandé";
        gameLauncher.RestartGame();
        Debug.Log("[DebugPanel_GameLauncher] Redémarrage du jeu demandé");
    }

    private void RunDiagnostic()
    {
        if (gameLauncher == null) return;
        
        lastLaunchStatus = "Diagnostic effectué";
        
        Debug.Log("=== DIAGNOSTIC LOGPARADE GAME LAUNCHER ===");
        Debug.Log($"GameLauncher trouvé: {gameLauncher != null}");
        Debug.Log($"Systèmes détectés: {systemsFound}/4");
        Debug.Log($"En cours de lancement: {isLaunching}");
        Debug.Log($"Jeu démarré: {gameStarted}");
        Debug.Log($"Calibration terminée: {calibrationCompleted}");
        Debug.Log("=== FIN DIAGNOSTIC ===");
    }

    /// <summary>
    /// Gestionnaires d'événements
    /// </summary>
    private void OnLaunchStarted()
    {
        lastLaunchStatus = "Lancement démarré";
    }

    private void OnLaunchCompleted()
    {
        lastLaunchStatus = "Lancement terminé";
    }
}
