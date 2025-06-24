using UnityEngine;

/// <summary>
/// Panneau de debug pour afficher les informations générales de statut LogParade.
/// Affiche IsOnLog, lane actuelle, timer, nombre de rondins, etc.
/// </summary>
public class DebugPanel_Status : BaseDebugPanel
{
    [Header("Game References")]
    [Tooltip("Référence au LogParadeGameController")]
    [SerializeField] private LogParadeGameController gameController;
    
    [Tooltip("Référence au LogParadeGameTimer")]
    [SerializeField] private LogParadeGameTimer gameTimer;
    
    [Tooltip("Référence au PlayerOnLogChecker")]
    [SerializeField] private PlayerOnLogChecker playerOnLogChecker;
    
    [Tooltip("Référence au LogParadeLateralTracker")]
    [SerializeField] private LogParadeLateralTracker lateralTracker;
    
    [Header("Auto-Discovery")]
    [Tooltip("Recherche automatiquement les composants dans la scène")]
    [SerializeField] private bool autoFindComponents = true;
    
    // Données en cache
    private bool isOnLog = false;
    private int currentLane = 0;
    private float timeRemaining = 0f;
    private int activeLogsCount = 0;
    private bool gameStarted = false;
    private bool debugModeActive = false;
    private Vector3 playerPosition = Vector3.zero;

    protected override void Start()
    {
        panelTitle = "Game Status";
        
        // Auto-découverte des composants
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
        
        base.Start();
    }

    protected override void RefreshData()
    {
        // Rafraîchir les données depuis les différents composants
        RefreshPlayerStatus();
        RefreshGameStatus();
        RefreshTimerStatus();
        RefreshLogStatus();
    }

    protected override void DrawPanelContent()
    {
        GUILayout.BeginVertical();
        
        // Section État du joueur
        DrawPlayerSection();
        
        GUILayout.Space(5);
        
        // Section État du jeu
        DrawGameSection();
        
        GUILayout.Space(5);
        
        // Section Timer
        DrawTimerSection();
        
        GUILayout.Space(5);
        
        // Section Rondins
        DrawLogsSection();
        
        GUILayout.EndVertical();
    }

    /// <summary>
    /// Dessine la section état du joueur
    /// </summary>
    private void DrawPlayerSection()
    {
        GUILayout.Label("<b>🧑 JOUEUR</b>");
        
        // IsOnLog - élément principal demandé
        string onLogStatus = isOnLog ? "<color=green>SUR RONDIN</color>" : "<color=red>DANS L'EAU</color>";
        GUILayout.Label($"État: {onLogStatus}");
        
        // Lane actuelle
        string laneColor = GetLaneColor(currentLane);
        GUILayout.Label($"Lane: <color={laneColor}>{currentLane}</color>");
        
        // Position
        GUILayout.Label($"Position: ({FormatNumber(playerPosition.x, 1)}, {FormatNumber(playerPosition.z, 1)})");
    }

    /// <summary>
    /// Dessine la section état du jeu
    /// </summary>
    private void DrawGameSection()
    {
        GUILayout.Label("<b>🎮 JEU</b>");
        
        // État du jeu
        string gameStatus = gameStarted ? "<color=green>DÉMARRÉ</color>" : "<color=orange>EN ATTENTE</color>";
        GUILayout.Label($"État: {gameStatus}");
        
        // Mode debug
        string debugStatus = debugModeActive ? "<color=yellow>ACTIF</color>" : "<color=gray>INACTIF</color>";
        GUILayout.Label($"Debug: {debugStatus}");
    }

    /// <summary>
    /// Dessine la section timer
    /// </summary>
    private void DrawTimerSection()
    {
        GUILayout.Label("<b>⏱️ TIMER</b>");
        
        if (gameTimer != null)
        {
            // Temps restant
            string timeColor = timeRemaining > 10f ? "white" : "red";
            GUILayout.Label($"Restant: <color={timeColor}>{FormatTime(timeRemaining)}</color>");
            
            // Progression (barre textuelle)
            float totalTime = GetTotalGameTime();
            if (totalTime > 0)
            {
                float progress = 1f - (timeRemaining / totalTime);
                GUILayout.Label($"Progrès: {FormatProgress(progress)}");
            }
        }
        else
        {
            GUILayout.Label("<color=red>Timer non trouvé</color>");
        }
    }

    /// <summary>
    /// Dessine la section rondins
    /// </summary>
    private void DrawLogsSection()
    {
        GUILayout.Label("<b>🪵 RONDINS</b>");
        
        // Nombre de rondins actifs
        string logsColor = activeLogsCount > 0 ? "green" : "red";
        GUILayout.Label($"Actifs: <color={logsColor}>{activeLogsCount}</color>");
        
        // Bouton de recomptage
        if (GUILayout.Button("Recompter"))
        {
            RefreshLogStatus();
        }
    }

    protected override float GetEstimatedHeight()
    {
        return 220f;
    }

    /// <summary>
    /// Auto-découverte des composants
    /// </summary>
    private void AutoFindComponents()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<LogParadeGameController>();
        }
        
        if (gameTimer == null)
        {
            gameTimer = FindObjectOfType<LogParadeGameTimer>();
        }
        
        if (playerOnLogChecker == null)
        {
            playerOnLogChecker = FindObjectOfType<PlayerOnLogChecker>();
        }
        
        if (lateralTracker == null)
        {
            lateralTracker = FindObjectOfType<LogParadeLateralTracker>();
        }
        
        Debug.Log($"[DebugPanel_Status] Composants trouvés - " +
                  $"Controller: {gameController != null}, " +
                  $"Timer: {gameTimer != null}, " +
                  $"OnLogChecker: {playerOnLogChecker != null}, " +
                  $"LateralTracker: {lateralTracker != null}");
    }

    /// <summary>
    /// Rafraîchit les données du joueur
    /// </summary>
    private void RefreshPlayerStatus()
    {
        // IsOnLog
        if (playerOnLogChecker != null)
        {
            isOnLog = playerOnLogChecker.IsOnLog;
        }
        
        // Lane actuelle
        if (lateralTracker != null)
        {
            currentLane = GetCurrentLane();
        }
        
        // Position du joueur
        if (gameController != null)
        {
            playerPosition = GetPlayerPosition();
        }
    }

    /// <summary>
    /// Rafraîchit les données du jeu
    /// </summary>
    private void RefreshGameStatus()
    {
        if (gameController != null)
        {
            gameStarted = GetGameStartedState();
            debugModeActive = GetDebugModeState();
        }
    }

    /// <summary>
    /// Rafraîchit les données du timer
    /// </summary>
    private void RefreshTimerStatus()
    {
        if (gameTimer != null)
        {
            timeRemaining = GetTimeRemaining();
        }
    }    /// <summary>
    /// Rafraîchit le nombre de rondins
    /// </summary>
    private void RefreshLogStatus()
    {
        activeLogsCount = 0;
        
        // Méthode 1: Accès direct au LogGenerator (méthode la plus fiable)
        var logGenerator = FindObjectOfType<LogParadeLogGenerator>();
        if (logGenerator != null)
        {
            // Accéder à la liste activeLogs via réflexion
            var activeLogsField = logGenerator.GetType().GetField("activeLogs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (activeLogsField != null)
            {
                var activeLogsList = activeLogsField.GetValue(logGenerator) as System.Collections.IList;
                if (activeLogsList != null)
                {
                    // Compter les rondins non-null
                    int count = 0;
                    foreach (var log in activeLogsList)
                    {
                        if (log != null) count++;
                    }
                    activeLogsCount = count;
                    return;
                }
            }
        }
        
        // Méthode 2: Recherche par composant LogParadeLog
        var logComponents = FindObjectsOfType<LogParadeLog>();
        if (logComponents.Length > 0)
        {
            activeLogsCount = logComponents.Length;
            return;
        }
        
        // Méthode 3: Recherche par tag "Log"
        var logsByTag = GameObject.FindGameObjectsWithTag("Log");
        if (logsByTag.Length > 0)
        {
            activeLogsCount = logsByTag.Length;
            return;
        }
        
        // Méthode 4: Recherche par nom contenant "log" (insensible à la casse)
        var allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.name.ToLower().Contains("log"))
            {
                // Exclure les objets de debug ou UI
                if (!obj.name.ToLower().Contains("debug") &&                    !obj.name.ToLower().Contains("ui") &&
                    !obj.name.ToLower().Contains("manager") &&
                    !obj.name.ToLower().Contains("generator"))
                {
                    activeLogsCount++;
                }
            }
        }
    }

    /// <summary>
    /// Méthodes utilitaires pour accéder aux données privées
    /// </summary>
    private int GetCurrentLane()
    {
        if (lateralTracker == null) return 0;
        
        var field = lateralTracker.GetType().GetField("currentLane", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (int)field.GetValue(lateralTracker);
        }
        
        return 0;
    }

    private Vector3 GetPlayerPosition()
    {
        if (gameController == null) return Vector3.zero;
        
        var field = gameController.GetType().GetField("currentPlayerPosition", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (Vector3)field.GetValue(gameController);
        }
        
        return Vector3.zero;
    }

    private bool GetGameStartedState()
    {
        if (gameController == null) return false;
        
        var field = gameController.GetType().GetField("gameStarted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(gameController);
        }
        
        return false;
    }

    private bool GetDebugModeState()
    {
        if (gameController == null) return false;
        
        var field = gameController.GetType().GetField("enableDebugMode");
        if (field != null)
        {
            return (bool)field.GetValue(gameController);
        }
        
        return false;
    }

    private float GetTimeRemaining()
    {
        if (gameTimer == null) return 0f;
        
        var field = gameTimer.GetType().GetField("timeRemaining", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (float)field.GetValue(gameTimer);
        }
        
        return 0f;
    }

    private float GetTotalGameTime()
    {
        if (gameTimer == null) return 0f;
        
        var field = gameTimer.GetType().GetField("gameDurationInSeconds", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (float)field.GetValue(gameTimer);
        }
        
        return 60f; // Valeur par défaut
    }

    /// <summary>
    /// Formate le temps en mm:ss
    /// </summary>
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

    /// <summary>
    /// Formate la progression en barre textuelle
    /// </summary>
    private string FormatProgress(float progress)
    {
        int barLength = 10;
        int filled = Mathf.RoundToInt(progress * barLength);
        string bar = "";
        
        for (int i = 0; i < barLength; i++)
        {
            bar += i < filled ? "█" : "░";
        }
        
        return $"{bar} {(progress * 100f):F0}%";
    }

    /// <summary>
    /// Obtient la couleur pour une lane
    /// </summary>
    private string GetLaneColor(int lane)
    {
        switch (lane)
        {
            case 1: return "red";
            case 2: return "yellow";
            case 3: return "green";
            case 4: return "blue";
            default: return "white";
        }
    }
}
