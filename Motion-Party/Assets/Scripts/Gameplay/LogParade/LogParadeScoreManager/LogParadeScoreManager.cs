using UnityEngine;
using TMPro;

/// <summary>
/// Module de gestion du score pour le mini-jeu LogParade.
/// Basé sur la position du joueur (sur rondin ou non) via PlayerOnLogChecker.
/// Adapté au public senior avec un système tolérant et stable.
/// </summary>
public class LogParadeScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("Points gagnés par seconde quand le joueur est sur un rondin")]
    public int pointsPerSecond = 1;
    
    [Tooltip("Points perdus quand le joueur tombe à l'eau")]
    public int penaltyPoints = 5;
    
    [Tooltip("Score initial du joueur")]
    public int initialScore = 0;
      [Header("References")]
    [Tooltip("Référence au PlayerOnLogChecker pour détecter la position du joueur")]
    public PlayerOnLogChecker playerOnLogChecker;
    
    [Tooltip("(Optionnel) Champ texte UI pour afficher le score")]
    public TextMeshProUGUI scoreDisplayText;
    
    [Header("Debug")]
    [Tooltip("Afficher les logs de debug dans la console")]
    public bool enableDebugLogs = true;

    // Propriétés publiques
    /// <summary>
    /// Score actuel du joueur
    /// </summary>
    public int CurrentScore { get; private set; }
    
    /// <summary>
    /// Indique si le système de score est actif
    /// </summary>
    public bool IsScoring { get; private set; } = false;

    // Variables privées
    private bool wasOnLogLastFrame = false;
    private bool canReceivePenalty = true;
    private float scoreTimer = 0f;

    void Start()
    {
        InitializeScoreSystem();
    }

    void Update()
    {
        // Vérifier si le scoring est autorisé (après calibration)
        if (!LogParadeCalibrationManager.CanStartScoring())
        {
            if (enableDebugLogs && IsScoring)
            {
                Debug.Log("[LogParadeScoreManager] Score suspendu pendant la calibration");
            }
            return;
        }
        
        if (!IsScoring) return;
        
        UpdateScoreLogic();
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Initialise le système de score
    /// </summary>
    private void InitializeScoreSystem()
    {
        CurrentScore = initialScore;
        
        // Trouver automatiquement PlayerOnLogChecker si non assigné
        if (playerOnLogChecker == null)
        {
            playerOnLogChecker = FindObjectOfType<PlayerOnLogChecker>();
            
            if (playerOnLogChecker == null)
            {
                Debug.LogError("[LogParadeScoreManager] Aucun PlayerOnLogChecker trouvé ! Le système de score ne peut pas fonctionner.", this);
                return;
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[LogParadeScoreManager] PlayerOnLogChecker trouvé automatiquement sur {playerOnLogChecker.gameObject.name}");
                }
            }
        }
        
        // Initialiser l'état
        wasOnLogLastFrame = playerOnLogChecker.IsPlayerOnLog();
        canReceivePenalty = !wasOnLogLastFrame; // Si on commence dans l'eau, on peut recevoir une pénalité
        
        if (enableDebugLogs)
        {
            Debug.Log($"[LogParadeScoreManager] Système initialisé. Score initial: {CurrentScore}, État initial: {(wasOnLogLastFrame ? "sur rondin" : "dans l'eau")}");
        }
        
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Met à jour la logique de score
    /// </summary>
    private void UpdateScoreLogic()
    {
        if (playerOnLogChecker == null) return;

        bool isOnLogNow = playerOnLogChecker.IsPlayerOnLog();
        
        // Gestion du gain de score (+1 point/seconde sur rondin)
        if (isOnLogNow)
        {
            scoreTimer += Time.deltaTime;
            
            if (scoreTimer >= 1f)
            {
                AddScore(pointsPerSecond);
                scoreTimer = 0f;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[LogParadeScoreManager] +{pointsPerSecond} point(s) gagné(s). Score: {CurrentScore}");
                }
            }
        }
        else
        {
            scoreTimer = 0f; // Reset du timer si pas sur rondin
        }
        
        // Gestion des pénalités
        HandlePenaltyLogic(isOnLogNow);
        
        // Mettre à jour l'état pour la prochaine frame
        wasOnLogLastFrame = isOnLogNow;
    }

    /// <summary>
    /// Gère la logique des pénalités
    /// </summary>
    private void HandlePenaltyLogic(bool isOnLogNow)
    {
        // Transition : de sur rondin → dans l'eau
        if (wasOnLogLastFrame && !isOnLogNow)
        {
            // Le joueur vient de tomber
            if (canReceivePenalty)
            {
                SubtractScore(penaltyPoints);
                canReceivePenalty = false; // Empêcher les pénalités répétées
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[LogParadeScoreManager] ❌ PÉNALITÉ! -{penaltyPoints} points. Score: {CurrentScore}");
                }
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[LogParadeScoreManager] Chute détectée mais pénalité déjà appliquée.");
                }
            }
        }
        // Transition : de dans l'eau → sur rondin
        else if (!wasOnLogLastFrame && isOnLogNow)
        {
            // Le joueur vient de remonter sur un rondin
            canReceivePenalty = true; // Réactiver le droit à la pénalité
            
            if (enableDebugLogs)
            {
                Debug.Log("[LogParadeScoreManager] ✅ Joueur remonté sur rondin. Droit à la pénalité réactivé.");
            }
        }
    }

    /// <summary>
    /// Ajoute des points au score
    /// </summary>
    private void AddScore(int points)
    {
        CurrentScore += points;
        CurrentScore = Mathf.Max(0, CurrentScore); // Éviter les scores négatifs
    }

    /// <summary>
    /// Soustrait des points au score
    /// </summary>
    private void SubtractScore(int points)
    {
        CurrentScore -= points;
        CurrentScore = Mathf.Max(0, CurrentScore); // Éviter les scores négatifs
    }

    /// <summary>
    /// Met à jour l'affichage du score
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreDisplayText != null)
        {
            scoreDisplayText.text = $"Score: {CurrentScore}";
        }
    }

    #region API Publique    /// <summary>
    /// Démarre le système de score
    /// </summary>
    public void StartScoring()
    {
        // Vérifier si le scoring est autorisé (après calibration)
        if (!LogParadeCalibrationManager.CanStartScoring())
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[LogParadeScoreManager] ⚠️ Impossible de démarrer le score : calibration en cours!");
            }
            return;
        }
        
        IsScoring = true;
        scoreTimer = 0f;
        
        if (enableDebugLogs)
        {
            Debug.Log("[LogParadeScoreManager] ✅ Système de score démarré.");
        }
    }

    /// <summary>
    /// Arrête le système de score
    /// </summary>
    public void StopScoring()
    {
        IsScoring = false;
        scoreTimer = 0f;
        
        if (enableDebugLogs)
        {
            Debug.Log("[LogParadeScoreManager] ⏹️ Système de score arrêté.");
        }
    }

    /// <summary>
    /// Remet le score à zéro
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = initialScore;
        scoreTimer = 0f;
        canReceivePenalty = true;
        
        if (playerOnLogChecker != null)
        {
            wasOnLogLastFrame = playerOnLogChecker.IsPlayerOnLog();
            canReceivePenalty = !wasOnLogLastFrame;
        }
        
        UpdateScoreDisplay();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[LogParadeScoreManager] 🔄 Score remis à {initialScore}.");
        }
    }

    /// <summary>
    /// Retourne le score actuel
    /// </summary>
    /// <returns>Score actuel du joueur</returns>
    public int GetCurrentScore()
    {
        return CurrentScore;
    }

    /// <summary>
    /// Définit un nouveau score
    /// </summary>
    /// <param name="newScore">Nouveau score à définir</param>
    public void SetScore(int newScore)
    {
        CurrentScore = Mathf.Max(0, newScore);
        UpdateScoreDisplay();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[LogParadeScoreManager] Score défini à {CurrentScore}.");
        }
    }

    /// <summary>
    /// Ajoute des points bonus au score
    /// </summary>
    /// <param name="bonusPoints">Points bonus à ajouter</param>
    public void AddBonusPoints(int bonusPoints)
    {
        AddScore(bonusPoints);
        UpdateScoreDisplay();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[LogParadeScoreManager] 🎉 Bonus +{bonusPoints} points! Score: {CurrentScore}");
        }
    }

    /// <summary>
    /// Indique si le joueur peut actuellement recevoir une pénalité
    /// </summary>
    /// <returns>True si une pénalité peut être appliquée</returns>
    public bool CanReceivePenalty()
    {
        return canReceivePenalty;
    }

    #endregion

    #region Debug

    void OnGUI()
    {
        if (!enableDebugLogs) return;

        // Affichage des informations de debug
        GUILayout.BeginArea(new Rect(320, Screen.height - 180, 300, 170));
        GUILayout.Label("=== LogParade Score Manager ===");
        GUILayout.Label($"Score: {CurrentScore}");
        GUILayout.Label($"Système actif: {(IsScoring ? "OUI" : "NON")}");
        GUILayout.Label($"Sur rondin: {(playerOnLogChecker != null && playerOnLogChecker.IsPlayerOnLog() ? "OUI" : "NON")}");
        GUILayout.Label($"Peut pénalité: {(canReceivePenalty ? "OUI" : "NON")}");
        GUILayout.Label($"Timer: {scoreTimer:F1}s");
        GUILayout.EndArea();
    }

    #endregion
}
