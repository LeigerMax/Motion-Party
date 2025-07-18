using UnityEngine;

/// <summary>
/// Panneau de debug pour afficher les informations générales de statut LogParade.
/// Affiche IsOnLog, lane actuelle, timer, nombre de rondins, etc.
/// </summary>
public class DebugPanel_Status : BaseDebugPanel
{
#region Champs & Références
    [Header("Game References")]
    [Tooltip("Référence au LogParadeGameController")]
    [SerializeField] private LogParadeGameController gameController;
    [Tooltip("Référence au LogParadeGameTimer")]
    [SerializeField] private LogParadeGameTimer gameTimer;
    [Tooltip("Référence au PlayerLogCollisionChecker")]
    [SerializeField] private PlayerLogCollisionChecker PlayerLogCollisionChecker;
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
#endregion

#region Initialisation
    protected override void Start()
    {
        visibleAtStart = true;
        SetVisible(true);
        panelTitle = "Game Status";
        if (autoFindComponents)
        {
            AutoFindComponents();
        }
        base.Start();
    }
#endregion

#region Rafraîchissement
    protected override void RefreshData()
    {
        RefreshPlayerStatus();
        RefreshGameStatus();
        RefreshTimerStatus();
        RefreshLogStatus();
    }
#endregion

#region Affichage
    protected override void DrawPanelContent()
    {
        GUILayout.BeginVertical();
        DrawPlayerSection();
        GUILayout.Space(5);
        DrawGameSection();
        GUILayout.Space(5);
        DrawTimerSection();
        GUILayout.Space(5);
        DrawLogsSection();
        GUILayout.EndVertical();
    }
    private void DrawPlayerSection()
    {
        GUILayout.Label("<b>🧑 JOUEUR</b>");
        string onLogStatus = isOnLog ? "<color=green>SUR RONDIN</color>" : "<color=red>DANS L'EAU</color>";
        GUILayout.Label($"État: {onLogStatus}");
        string laneColor = GetLaneColor(currentLane);
        GUILayout.Label($"Lane: <color={laneColor}>{currentLane}</color>");
        GUILayout.Label($"Position: ({FormatNumber(playerPosition.x, 1)}, {FormatNumber(playerPosition.z, 1)})");
    }
    private void DrawGameSection()
    {
        GUILayout.Label("<b>🎮 JEU</b>");
        string gameStatus = gameStarted ? "<color=green>DÉMARRÉ</color>" : "<color=orange>EN ATTENTE</color>";
        GUILayout.Label($"État: {gameStatus}");
        string debugStatus = debugModeActive ? "<color=yellow>ACTIF</color>" : "<color=gray>INACTIF</color>";
        GUILayout.Label($"Debug: {debugStatus}");
    }
    private void DrawTimerSection()
    {
        GUILayout.Label("<b>⏱️ TIMER</b>");
        if (gameTimer != null)
        {
            string timeColor = timeRemaining > 10f ? "white" : "red";
            GUILayout.Label($"Restant: <color={timeColor}>{FormatTime(timeRemaining)}</color>");
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
    private void DrawLogsSection()
    {
        GUILayout.Label("<b>🪵 RONDINS</b>");
        string logsColor = activeLogsCount > 0 ? "green" : "red";
        GUILayout.Label($"Actifs: <color={logsColor}>{activeLogsCount}</color>");
        if (GUILayout.Button("Recompter"))
        {
            RefreshLogStatus();
        }
    }
    protected override float GetEstimatedHeight()
    {
        return 220f;
    }
#endregion

#region Découverte & Rafraîchissement
    private void AutoFindComponents()
    {
        if (gameController == null)
            gameController = FindFirstObjectByType<LogParadeGameController>();
        if (gameTimer == null)
            gameTimer = FindFirstObjectByType<LogParadeGameTimer>();
        if (PlayerLogCollisionChecker == null)
            PlayerLogCollisionChecker = FindFirstObjectByType<PlayerLogCollisionChecker>();
        if (lateralTracker == null)
            lateralTracker = FindFirstObjectByType<LogParadeLateralTracker>();
    }
    private void RefreshPlayerStatus()
    {
        if (PlayerLogCollisionChecker != null)
            isOnLog = PlayerLogCollisionChecker.IsOnLog;
        if (lateralTracker != null)
            currentLane = GetCurrentLane();
        if (gameController != null)
            playerPosition = GetPlayerPosition();
    }
    private void RefreshGameStatus()
    {
        if (gameController != null)
        {
            gameStarted = GetGameStartedState();
            debugModeActive = GetDebugModeState();
        }
    }
    private void RefreshTimerStatus()
    {
        if (gameTimer != null)
            timeRemaining = GetTimeRemaining();
    }
    private void RefreshLogStatus()
    {
        activeLogsCount = 0;
        var logGenerator = FindFirstObjectByType<LogParadeLogGenerator>();
        if (logGenerator != null)
        {
            var activeLogsField = logGenerator.GetType().GetField("activeLogs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (activeLogsField != null)
            {
                var activeLogsList = activeLogsField.GetValue(logGenerator) as System.Collections.IList;
                if (activeLogsList != null)
                {
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
        var logComponents = FindObjectsByType<LogParadeLog>(FindObjectsSortMode.None);
        if (logComponents.Length > 0)
        {
            activeLogsCount = logComponents.Length;
            return;
        }
        var logsByTag = GameObject.FindGameObjectsWithTag("Log");
        if (logsByTag.Length > 0)
        {
            activeLogsCount = logsByTag.Length;
            return;
        }
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.name.ToLower().Contains("log"))
            {
                if (!obj.name.ToLower().Contains("debug") && !obj.name.ToLower().Contains("ui") && !obj.name.ToLower().Contains("manager") && !obj.name.ToLower().Contains("generator"))
                {
                    activeLogsCount++;
                }
            }
        }
    }
#endregion

#region Accès Données (Réflexion)
    private int GetCurrentLane()
    {
        if (lateralTracker == null) return 0;
        var field = lateralTracker.GetType().GetField("currentLane", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (int)field.GetValue(lateralTracker);
        return 0;
    }
    private Vector3 GetPlayerPosition()
    {
        if (gameController == null) return Vector3.zero;
        var field = gameController.GetType().GetField("currentPlayerPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (Vector3)field.GetValue(gameController);
        return Vector3.zero;
    }
    private bool GetGameStartedState()
    {
        if (gameController == null) return false;
        var field = gameController.GetType().GetField("gameStarted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (bool)field.GetValue(gameController);
        return false;
    }
    private bool GetDebugModeState()
    {
        if (gameController == null) return false;
        var field = gameController.GetType().GetField("enableDebugMode");
        if (field != null)
            return (bool)field.GetValue(gameController);
        return false;
    }
    private float GetTimeRemaining()
    {
        if (gameTimer == null) return 0f;
        var field = gameTimer.GetType().GetField("timeRemaining", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(gameTimer);
        return 0f;
    }
    private float GetTotalGameTime()
    {
        if (gameTimer == null) return 0f;
        var field = gameTimer.GetType().GetField("gameDurationInSeconds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(gameTimer);
        return 60f;
    }
#endregion

#region Utilitaires Affichage
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }
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
#endregion
}
