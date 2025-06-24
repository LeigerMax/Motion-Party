using UnityEngine;

/// <summary>
/// Panneau de debug pour afficher les informations de score LogParade.
/// Affiche le score actuel, les gains par seconde, et les pénalités.
/// </summary>
public class DebugPanel_Score : BaseDebugPanel
{
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

    protected override void Start()
    {
        panelTitle = "Score Debug";
        
        // Auto-découverte du ScoreManager
        if (autoFindScoreManager && scoreManager == null)
        {
            scoreManager = FindObjectOfType<LogParadeScoreManager>();
            if (scoreManager != null)
            {
                Debug.Log("[DebugPanel_Score] ScoreManager trouvé automatiquement");
            }
        }
        
        base.Start();
    }

    protected override void RefreshData()
    {
        if (scoreManager == null) return;
        
        // Récupérer les données via réflexion ou propriétés publiques
        currentScore = GetCurrentScore();
        pointsPerSecond = GetPointsPerSecond();
        penaltyPoints = GetPenaltyPoints();
        isScoring = GetIsScoringActive();
        scoreRate = GetCurrentScoreRate();
    }

    protected override void DrawPanelContent()
    {
        if (scoreManager == null)
        {
            GUILayout.Label("<color=red>Score Manager non trouvé!</color>");
            
            if (GUILayout.Button("Rechercher"))
            {
                scoreManager = FindObjectOfType<LogParadeScoreManager>();
            }
            return;
        }
        
        // Affichage du score
        GUILayout.BeginVertical();
        
        // Score actuel
        GUILayout.Label($"<b>Score actuel:</b> <color=yellow>{currentScore}</color>");
        
        // État du scoring
        string scoringState = isScoring ? "<color=green>ACTIF</color>" : "<color=orange>INACTIF</color>";
        GUILayout.Label($"<b>Scoring:</b> {scoringState}");
        
        // Taux de gain
        GUILayout.Label($"<b>Points/sec:</b> {pointsPerSecond}");
        GUILayout.Label($"<b>Pénalité:</b> {penaltyPoints}");
        
        // Séparateur
        GUILayout.Space(5);
        
        // Statistiques en temps réel
        GUILayout.Label("<b>Temps réel:</b>");
        GUILayout.Label($"Taux actuel: {FormatNumber(scoreRate, 1)}/sec");
          // Boutons de test
        GUILayout.Space(5);
        GUILayout.Label("<b>🎮 Tests Score:</b>");
        
        // Boutons d'ajout/retrait rapide
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1"))
        {
            AddTestScore(1);
        }
        if (GUILayout.Button("+10"))
        {
            AddTestScore(10);
        }
        if (GUILayout.Button("+50"))
        {
            AddTestScore(50);
        }
        GUILayout.EndHorizontal();
          GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1"))
        {
            SubtractTestScore(1);
        }
        if (GUILayout.Button("-5"))
        {
            SubtractTestScore(5);
        }
        if (GUILayout.Button("-10"))
        {
            SubtractTestScore(10);
        }
        GUILayout.EndHorizontal();
          // Boutons de contrôle
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Score"))
        {
            ResetScore();
        }
        if (GUILayout.Button("Score x2"))
        {
            MultiplyScore(2);
        }
        GUILayout.EndHorizontal();
        
        // Boutons de test avancés
        GUILayout.Space(3);
        if (GUILayout.Button("🎯 Test Scoring ON/OFF"))
        {
            ToggleScoring();
        }
        
        GUILayout.EndVertical();
    }    protected override float GetEstimatedHeight()
    {
        return scoreManager != null ? 240f : 80f; // Augmenté pour les nouveaux boutons
    }

    /// <summary>
    /// Récupère le score actuel
    /// </summary>
    private int GetCurrentScore()
    {
        if (scoreManager == null) return 0;
        
        // Première priorité : propriété publique CurrentScore (identifiée dans le code source)
        var scoreProperty = scoreManager.GetType().GetProperty("CurrentScore");
        if (scoreProperty != null && scoreProperty.PropertyType == typeof(int))
        {
            return (int)scoreProperty.GetValue(scoreManager);
        }
        
        // Méthode alternative : méthode publique GetCurrentScore()
        var scoreMethod = scoreManager.GetType().GetMethod("GetCurrentScore");
        if (scoreMethod != null && scoreMethod.ReturnType == typeof(int))
        {
            return (int)scoreMethod.Invoke(scoreManager, null);
        }
        
        // Fallback : champ privé currentScore
        var scoreField = scoreManager.GetType().GetField("currentScore", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (scoreField != null && scoreField.FieldType == typeof(int))
        {
            return (int)scoreField.GetValue(scoreManager);
        }
        
        return 0;
    }

    /// <summary>
    /// Récupère les points par seconde
    /// </summary>
    private int GetPointsPerSecond()
    {
        if (scoreManager == null) return 0;
        
        // Champ public pointsPerSecond (identifié dans le code source)
        var field = scoreManager.GetType().GetField("pointsPerSecond");
        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(scoreManager);
        }
        
        return 0;
    }

    /// <summary>
    /// Récupère les points de pénalité
    /// </summary>
    private int GetPenaltyPoints()
    {
        if (scoreManager == null) return 0;
        
        // Champ public penaltyPoints (identifié dans le code source)
        var field = scoreManager.GetType().GetField("penaltyPoints");
        if (field != null && field.FieldType == typeof(int))
        {
            return (int)field.GetValue(scoreManager);
        }
        
        return 0;
    }

    /// <summary>
    /// Vérifie si le scoring est actif
    /// </summary>
    private bool GetIsScoringActive()
    {
        if (scoreManager == null) return false;
        
        // Propriété publique IsScoring (identifiée dans le code source)
        var isScoringProperty = scoreManager.GetType().GetProperty("IsScoring");
        if (isScoringProperty != null && isScoringProperty.PropertyType == typeof(bool))
        {
            return (bool)isScoringProperty.GetValue(scoreManager);
        }
        
        // Fallback : champ privé isScoringEnabled
        var field = scoreManager.GetType().GetField("isScoringEnabled", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null && field.FieldType == typeof(bool))
        {
            return (bool)field.GetValue(scoreManager);
        }
        
        return false;
    }

    /// <summary>
    /// Calcule le taux de score actuel
    /// </summary>
    private float GetCurrentScoreRate()
    {
        // Calcul basé sur l'état du joueur et les paramètres
        if (!isScoring) return 0f;
        
        // Simplicité: retourner pointsPerSecond si actif
        return pointsPerSecond;
    }    /// <summary>
    /// Ajoute des points de test
    /// </summary>
    private void AddTestScore(int points)
    {
        if (scoreManager == null) 
        {
            Debug.LogWarning("[DebugPanel_Score] ScoreManager non trouvé - impossible d'ajouter des points");
            return;
        }
        
        if (points == 0)
        {
            Debug.Log("[DebugPanel_Score] Aucun point à ajouter (valeur = 0)");
            return;
        }
        
        if (points < 0)
        {
            Debug.LogWarning("[DebugPanel_Score] Valeur négative détectée, utilisez SubtractTestScore() à la place");
            SubtractTestScore(-points);
            return;
        }
          // Utiliser la méthode AddPointsManual
        var addManualMethod = scoreManager.GetType().GetMethod("AddPointsManual");
        if (addManualMethod != null)
        {
            addManualMethod.Invoke(scoreManager, new object[] { points });
            Debug.Log($"[DebugPanel_Score] ✅ {points} points ajoutés via AddPointsManual() - Score: {GetCurrentScore()}");
            return;
        }
        
        // Fallback : essayer d'autres méthodes
        var addMethod = scoreManager.GetType().GetMethod("AddPoints");
        if (addMethod != null)
        {
            addMethod.Invoke(scoreManager, new object[] { points });
            Debug.Log($"[DebugPanel_Score] ✅ {points} points ajoutés via AddPoints() - Score: {GetCurrentScore()}");
            return;
        }
        
        // Méthode alternative directe
        var addScoreMethod = scoreManager.GetType().GetMethod("AddScore");
        if (addScoreMethod != null)
        {
            addScoreMethod.Invoke(scoreManager, new object[] { points });
            Debug.Log($"[DebugPanel_Score] ✅ {points} points ajoutés via AddScore() - Score: {GetCurrentScore()}");
            return;
        }
        
        Debug.LogError("[DebugPanel_Score] ❌ Aucune méthode d'ajout de points trouvée !");
    }

    /// <summary>
    /// Retire des points de test (méthode séparée pour plus de clarté)
    /// </summary>
    private void SubtractTestScore(int points)
    {
        if (scoreManager == null) 
        {
            Debug.LogWarning("[DebugPanel_Score] ScoreManager non trouvé - impossible de retirer des points");
            return;
        }
        
        if (points <= 0)
        {
            Debug.LogWarning("[DebugPanel_Score] Valeur invalide pour retirer des points (doit être > 0)");
            return;
        }
        
        // Utiliser la méthode SubtractPointsManual
        var subtractManualMethod = scoreManager.GetType().GetMethod("SubtractPointsManual");
        if (subtractManualMethod != null)
        {
            subtractManualMethod.Invoke(scoreManager, new object[] { points });
            Debug.Log($"[DebugPanel_Score] ✅ {points} points retirés via SubtractPointsManual() - Score: {GetCurrentScore()}");
            return;
        }
        
        // Fallback : essayer d'autres méthodes
        var addMethod = scoreManager.GetType().GetMethod("AddPoints");
        if (addMethod != null)
        {
            addMethod.Invoke(scoreManager, new object[] { -points });
            Debug.Log($"[DebugPanel_Score] ✅ {points} points retirés via AddPoints(-{points}) - Score: {GetCurrentScore()}");
            return;
        }
        
        Debug.LogError("[DebugPanel_Score] ❌ Aucune méthode de retrait de points trouvée !");
    }

    /// <summary>
    /// Remet le score à zéro
    /// </summary>
    private void ResetScore()
    {
        if (scoreManager == null) return;
        
        // Essayer d'appeler une méthode de reset
        var resetMethod = scoreManager.GetType().GetMethod("ResetScore");
        if (resetMethod != null)
        {
            resetMethod.Invoke(scoreManager, null);
            Debug.Log("[DebugPanel_Score] Score réinitialisé");
        }
        else
        {
            // Méthode alternative : setter le score à 0
            var setMethod = scoreManager.GetType().GetMethod("SetScore");
            if (setMethod != null)
            {
                setMethod.Invoke(scoreManager, new object[] { 0 });
                Debug.Log("[DebugPanel_Score] Score mis à 0");
            }
            else
            {
                Debug.LogWarning("[DebugPanel_Score] Aucune méthode de reset trouvée");
            }
        }
    }

    /// <summary>
    /// Multiplie le score actuel
    /// </summary>
    private void MultiplyScore(int multiplier)
    {
        if (scoreManager == null) return;
        
        int currentScore = GetCurrentScore();
        int newScore = currentScore * multiplier;
        
        // Réinitialiser puis ajouter le nouveau score
        ResetScore();
        if (newScore > 0)
        {
            AddTestScore(newScore);
        }
        
        Debug.Log($"[DebugPanel_Score] Score multiplié par {multiplier}: {currentScore} → {newScore}");
    }

    /// <summary>
    /// Active/désactive le système de scoring
    /// </summary>
    private void ToggleScoring()
    {
        if (scoreManager == null) return;
        
        bool currentlyScoring = GetIsScoringActive();
        
        if (currentlyScoring)
        {
            // Arrêter le scoring
            var stopMethod = scoreManager.GetType().GetMethod("StopScoring");
            if (stopMethod != null)
            {
                stopMethod.Invoke(scoreManager, null);
                Debug.Log("[DebugPanel_Score] 🛑 Scoring arrêté");
            }
        }
        else
        {
            // Démarrer le scoring
            var startMethod = scoreManager.GetType().GetMethod("StartScoring");
            if (startMethod != null)
            {
                startMethod.Invoke(scoreManager, null);
                Debug.Log("[DebugPanel_Score] ▶️ Scoring démarré");
            }
        }
    }

    /// <summary>
    /// Formate un nombre avec décimales
    /// </summary>
    private string FormatNumber(float value, int decimals)
    {
        return value.ToString($"F{decimals}");
    }
}
