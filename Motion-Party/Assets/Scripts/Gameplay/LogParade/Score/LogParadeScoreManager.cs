using UnityEngine;
using TMPro;

/// <summary>
/// Module de gestion du score pour le mini-jeu LogParade.
/// Basé sur la position du joueur (sur rondin ou non) via PlayerLogCollisionChecker.
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
    [Tooltip("Référence au PlayerLogCollisionChecker pour détecter la position du joueur")]
    public PlayerLogCollisionChecker PlayerLogCollisionChecker;
    
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
    {        // Vérifier si le scoring est autorisé (après calibration)
        if (!LogParadeGameStateController.CanStartScoring())
        {
            // Pas de log ici car c'est appelé à chaque frame
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
        // Trouver automatiquement PlayerLogCollisionChecker via SystemValidator si possible
        if (PlayerLogCollisionChecker == null)
        {
            var validator = LogParadeSystemValidator.Instance;
            if (validator != null)
            {
                PlayerLogCollisionChecker = validator.GetValidatedComponent<PlayerLogCollisionChecker>();
            }
              // Fallback si SystemValidator pas disponible
            if (PlayerLogCollisionChecker == null)
            {
                PlayerLogCollisionChecker = FindFirstObjectByType<PlayerLogCollisionChecker>();
            }
              if (PlayerLogCollisionChecker == null)
            {
                LogParadeLogger.LogError("Aucun PlayerLogCollisionChecker trouvé ! Le système de score ne peut pas fonctionner.");
                return;
            }
            else
            {
                LogParadeLogger.LogVerbose($"PlayerLogCollisionChecker trouvé automatiquement sur {PlayerLogCollisionChecker.gameObject.name}");
            }
        }
        
        // Initialiser l'état
        wasOnLogLastFrame = PlayerLogCollisionChecker.IsPlayerOnLog();
        canReceivePenalty = !wasOnLogLastFrame; // Si on commence dans l'eau, on peut recevoir une pénalité
          if (enableDebugLogs)
        {
            LogParadeLogger.Log($"Système initialisé. Score initial: {CurrentScore}, État initial: {(wasOnLogLastFrame ? "sur rondin" : "dans l'eau")}");
        }
        
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Met à jour la logique de score
    /// </summary>
    private void UpdateScoreLogic()
    {
        if (PlayerLogCollisionChecker == null) return;

        bool isOnLogNow = PlayerLogCollisionChecker.IsPlayerOnLog();
        
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
                    LogParadeLogger.LogVerbose($"+{pointsPerSecond} point(s) gagné(s). Score: {CurrentScore}");
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
                    LogParadeLogger.LogWarning($"PÉNALITÉ! -{penaltyPoints} points. Score: {CurrentScore}");
                }
            }            else
            {
                LogParadeLogger.LogVerbose("Chute détectée mais pénalité déjà appliquée.");
            }
        }
        // Transition : de dans l'eau → sur rondin
        else if (!wasOnLogLastFrame && isOnLogNow)
        {
            // Le joueur vient de remonter sur un rondin
            canReceivePenalty = true; // Réactiver le droit à la pénalité
              if (enableDebugLogs)
            {
                LogParadeLogger.LogVerbose("Joueur remonté sur rondin. Droit à la pénalité réactivé.");
            }
        }
    }    /// <summary>
    /// Ajoute des points au score et notifie l'EventCoordinator
    /// </summary>
    private void AddScore(int points)
    {
        CurrentScore += points;
        CurrentScore = Mathf.Max(0, CurrentScore); // Éviter les scores négatifs
        LogParadeEventCoordinator.TriggerScoreChanged(CurrentScore);
    }

    /// <summary>
    /// Soustrait des points au score et notifie l'EventCoordinator
    /// </summary>
    private void SubtractScore(int points)
    {
        CurrentScore -= points;
        CurrentScore = Mathf.Max(0, CurrentScore); // Éviter les scores négatifs
        LogParadeEventCoordinator.TriggerScoreChanged(CurrentScore);
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
    {        // Vérifier si le scoring est autorisé (après calibration)
        if (!LogParadeGameStateController.CanStartScoring())
        {
            LogParadeLogger.LogWarning("Impossible de démarrer le score : calibration en cours!");
            return;
        }
          IsScoring = true;
        scoreTimer = 0f;
        LogParadeEventCoordinator.TriggerScoringStarted();
          if (enableDebugLogs)
        {
            LogParadeLogger.Log("Système de score démarré.");
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
            LogParadeLogger.Log("Système de score arrêté.");
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
        
        if (PlayerLogCollisionChecker != null)
        {
            wasOnLogLastFrame = PlayerLogCollisionChecker.IsPlayerOnLog();
            canReceivePenalty = !wasOnLogLastFrame;
        }
        
        UpdateScoreDisplay();
          if (enableDebugLogs)
        {
            LogParadeLogger.Log($"Score remis à {initialScore}.");
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
            LogParadeLogger.Log($"Score défini à {CurrentScore}.");
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
            LogParadeLogger.Log($"Bonus +{bonusPoints} points! Score: {CurrentScore}");
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

    /// <summary>
    /// Ajoute des points manuellement (pour debug/test)
    /// </summary>
    /// <param name="points">Nombre de points à ajouter</param>
    public void AddPointsManual(int points)
    {
        AddScore(points);
        UpdateScoreDisplay();
          if (enableDebugLogs)
        {
            LogParadeLogger.Log($"Points ajoutés manuellement: +{points}. Score total: {CurrentScore}");
        }
    }

    /// <summary>
    /// Retire des points manuellement (pour debug/test)
    /// </summary>
    /// <param name="points">Nombre de points à retirer</param>
    public void SubtractPointsManual(int points)
    {
        SubtractScore(points);
        UpdateScoreDisplay();        if (enableDebugLogs)
        {
            LogParadeLogger.Log($"Points retirés manuellement: -{points}. Score total: {CurrentScore}");
        }
    }

    #endregion
}
