using UnityEngine;

/// <summary>
/// Panneau de debug pour afficher les informations de score LogParade.
/// Affiche le score actuel, les gains par seconde, et les pénalités.
/// </summary>
public class DebugPanel_Score : BaseDebugPanel
{
#region Champs & Références
    [Header("Score References")]
    [Tooltip("Référence au LogParadeScoreManager")]
    [SerializeField] private LogParadeScoreManager scoreManager;
    [Tooltip("Recherche automatiquement le ScoreManager dans la scène")]
    [SerializeField] private bool autoFindScoreManager = true;
    // Données en cache
    private int currentScore = 0;
    private int pointsPerSecond = 0;
    private int penaltyPoints = 0;
    private bool isScoring = false;
    private float scoreRate = 0f;
#endregion

#region Initialisation
    protected override void Start()
    {
        visibleAtStart = true;
        SetVisible(true);
        panelTitle = "Score Debug";
        if (autoFindScoreManager && scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<LogParadeScoreManager>();
            if (scoreManager != null)
            {
                LogParadeLogger.Log("[DebugPanel_Score] ScoreManager trouvé automatiquement");
            }
        }
        base.Start();
    }
#endregion

#region Rafraîchissement
    protected override void RefreshData()
    {
        if (scoreManager == null) return;
        currentScore = GetCurrentScore();
        pointsPerSecond = GetPointsPerSecond();
        penaltyPoints = GetPenaltyPoints();
        isScoring = GetIsScoringActive();
        scoreRate = GetCurrentScoreRate();
    }
#endregion

#region Affichage
    protected override void DrawPanelContent()
    {
        if (scoreManager == null)
        {
            GUILayout.Label("<color=red>Score Manager non trouvé!</color>");
            if (GUILayout.Button("Rechercher"))
            {
                scoreManager = FindFirstObjectByType<LogParadeScoreManager>();
            }
            return;
        }
        GUILayout.BeginVertical();
        GUILayout.Label($"<b>Score actuel:</b> <color=yellow>{currentScore}</color>");
        string scoringState = isScoring ? "<color=green>ACTIF</color>" : "<color=orange>INACTIF</color>";
        GUILayout.Label($"<b>Scoring:</b> {scoringState}");
        GUILayout.Label($"<b>Points/sec:</b> {pointsPerSecond}");
        GUILayout.Label($"<b>Pénalité:</b> {penaltyPoints}");
        GUILayout.Space(5);
        GUILayout.Label("<b>Temps réel:</b>");
        GUILayout.Label($"Taux actuel: {FormatNumber(scoreRate, 1)}/sec");
        GUILayout.Space(5);
        GUILayout.Label("<b>🎮 Tests Score:</b>");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1")) { AddTestScore(1); }
        if (GUILayout.Button("+10")) { AddTestScore(10); }
        if (GUILayout.Button("+50")) { AddTestScore(50); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1")) { SubtractTestScore(1); }
        if (GUILayout.Button("-5")) { SubtractTestScore(5); }
        if (GUILayout.Button("-10")) { SubtractTestScore(10); }
        GUILayout.EndHorizontal();
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Score")) { ResetScore(); }
        if (GUILayout.Button("Score x2")) { MultiplyScore(2); }
        GUILayout.EndHorizontal();
        GUILayout.Space(3);
        if (GUILayout.Button("🎯 Test Scoring ON/OFF")) { ToggleScoring(); }
        GUILayout.EndVertical();
    }
    protected override float GetEstimatedHeight()
    {
        return scoreManager != null ? 240f : 80f;
    }
#endregion

#region Accès Données (Réflexion)
    private int GetCurrentScore()
    {
        if (scoreManager == null) return 0;
        var scoreProperty = scoreManager.GetType().GetProperty("CurrentScore");
        if (scoreProperty != null && scoreProperty.PropertyType == typeof(int))
        {
            return (int)scoreProperty.GetValue(scoreManager);
        }
        var scoreMethod = scoreManager.GetType().GetMethod("GetCurrentScore");
        if (scoreMethod != null && scoreMethod.ReturnType == typeof(int))
        {
            return (int)scoreMethod.Invoke(scoreManager, null);
        }
        var scoreField = scoreManager.GetType().GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (scoreField != null && scoreField.FieldType == typeof(int))
        {
            return (int)scoreField.GetValue(scoreManager);
        }
        return 0;
    }
    private int GetPointsPerSecond()
    {
        if (scoreManager == null) return 0;
        var field = scoreManager.GetType().GetField("pointsPerSecond");
        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(scoreManager);
        }
        return 0;
    }
    private int GetPenaltyPoints()
    {
        if (scoreManager == null) return 0;
        var field = scoreManager.GetType().GetField("penaltyPoints");
        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(scoreManager);
        }
        return 0;
    }
    private bool GetIsScoringActive()
    {
        if (scoreManager == null) return false;
        var isScoringProperty = scoreManager.GetType().GetProperty("IsScoring");
        if (isScoringProperty != null && isScoringProperty.PropertyType == typeof(bool))
        {
            return (bool)isScoringProperty.GetValue(scoreManager);
        }
        var field = scoreManager.GetType().GetField("isScoringEnabled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
        {
            return (bool)field.GetValue(scoreManager);
        }
        return false;
    }
    private float GetCurrentScoreRate()
    {
        if (!isScoring) return 0f;
        return pointsPerSecond;
    }
#endregion

#region Actions Score
    private void AddTestScore(int points)
    {
        if (scoreManager == null)
        {
            LogParadeLogger.LogWarning("[DebugPanel_Score] ScoreManager non trouvé - impossible d'ajouter des points");
            return;
        }
        if (points == 0) return;
        if (points < 0)
        {
            LogParadeLogger.LogWarning("[DebugPanel_Score] Valeur négative détectée, utilisez SubtractTestScore() à la place");
            SubtractTestScore(-points);
            return;
        }
        var addManualMethod = scoreManager.GetType().GetMethod("AddPointsManual");
        if (addManualMethod != null)
        {
            addManualMethod.Invoke(scoreManager, new object[] { points });
            LogParadeLogger.Log($"[DebugPanel_Score] {points} points ajoutés via AddPointsManual() - Score: {GetCurrentScore()}");
            return;
        }
        var addMethod = scoreManager.GetType().GetMethod("AddPoints");
        if (addMethod != null)
        {
            addMethod.Invoke(scoreManager, new object[] { points });
            LogParadeLogger.Log($"[DebugPanel_Score] {points} points ajoutés via AddPoints() - Score: {GetCurrentScore()}");
            return;
        }
        var addScoreMethod = scoreManager.GetType().GetMethod("AddScore");
        if (addScoreMethod != null)
        {
            addScoreMethod.Invoke(scoreManager, new object[] { points });
            LogParadeLogger.Log($"[DebugPanel_Score] {points} points ajoutés via AddScore() - Score: {GetCurrentScore()}");
            return;
        }
        LogParadeLogger.LogError("[DebugPanel_Score] ❌ Aucune méthode d'ajout de points trouvée !");
    }
    private void SubtractTestScore(int points)
    {
        if (scoreManager == null)
        {
            LogParadeLogger.LogWarning("[DebugPanel_Score] ScoreManager non trouvé - impossible de retirer des points");
            return;
        }
        if (points <= 0)
        {
            LogParadeLogger.LogWarning("[DebugPanel_Score] Valeur invalide pour retirer des points (doit être > 0)");
            return;
        }
        var subtractManualMethod = scoreManager.GetType().GetMethod("SubtractPointsManual");
        if (subtractManualMethod != null)
        {
            subtractManualMethod.Invoke(scoreManager, new object[] { points });
            LogParadeLogger.Log($"[DebugPanel_Score] {points} points retirés via SubtractPointsManual() - Score: {GetCurrentScore()}");
            return;
        }
        var addMethod = scoreManager.GetType().GetMethod("AddPoints");
        if (addMethod != null)
        {
            addMethod.Invoke(scoreManager, new object[] { -points });
            LogParadeLogger.Log($"[DebugPanel_Score] {points} points retirés via AddPoints(-{points}) - Score: {GetCurrentScore()}");
            return;
        }
        LogParadeLogger.LogError("[DebugPanel_Score] ❌ Aucune méthode de retrait de points trouvée !");
    }
    private void ResetScore()
    {
        if (scoreManager == null) return;
        var resetMethod = scoreManager.GetType().GetMethod("ResetScore");
        if (resetMethod != null)
        {
            resetMethod.Invoke(scoreManager, null);
            LogParadeLogger.Log("[DebugPanel_Score] Score réinitialisé");
        }
        else
        {
            var setMethod = scoreManager.GetType().GetMethod("SetScore");
            if (setMethod != null)
            {
                setMethod.Invoke(scoreManager, new object[] { 0 });
                LogParadeLogger.Log("[DebugPanel_Score] Score mis à 0");
            }
            else
            {
                LogParadeLogger.LogWarning("[DebugPanel_Score] Aucune méthode de reset trouvée");
            }
        }
    }
    private void MultiplyScore(int multiplier)
    {
        if (scoreManager == null) return;
        int currentScore = GetCurrentScore();
        int newScore = currentScore * multiplier;
        // Attention : reset puis ajout (si score négatif, résultat = 0)
        ResetScore();
        if (newScore > 0)
        {
            AddTestScore(newScore);
        }
        LogParadeLogger.Log($"[DebugPanel_Score] Score multiplié par {multiplier}: {currentScore} → {newScore}");
    }
    private void ToggleScoring()
    {
        if (scoreManager == null) return;
        bool currentlyScoring = GetIsScoringActive();
        if (currentlyScoring)
        {
            var stopMethod = scoreManager.GetType().GetMethod("StopScoring");
            if (stopMethod != null)
            {
                stopMethod.Invoke(scoreManager, null);
                LogParadeLogger.Log("[DebugPanel_Score] 🛑 Scoring arrêté");
            }
        }
        else
        {
            var startMethod = scoreManager.GetType().GetMethod("StartScoring");
            if (startMethod != null)
            {
                startMethod.Invoke(scoreManager, null);
                LogParadeLogger.Log("[DebugPanel_Score] ▶️ Scoring démarré");
            }
        }
    }
#endregion

#region Utilitaires
    private new string FormatNumber(float value, int decimals)
    {
        return value.ToString($"F{decimals}");
    }
#endregion
}
