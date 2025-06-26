using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Gère le cycle de vie d'une partie de LogParade (démarrage, minuterie, arrêt).
/// Contrôle le score et le mouvement des rondins avec un timer configurable.
/// Intégration avec MiniGameBase pour un démarrage propre.
/// </summary>
public class LogParadeGameTimer : MiniGameBase
{
    [Header("Timer Settings")]
    [Tooltip("Durée de la partie en secondes")]
    [SerializeField] private float gameDurationInSeconds = 60f;
    
    [Tooltip("Délai avant le démarrage du timer (en secondes)")]
    [SerializeField] private float startDelay = 2f;
    
    [Header("Managers References")]
    [Tooltip("Référence au LogParadeScoreManager pour gérer le score")]
    [SerializeField] private LogParadeScoreManager scoreManager;
    
    [Tooltip("Référence au LogParadeLogGenerator pour gérer les rondins")]
    [SerializeField] private LogParadeLogGenerator logGenerator;
    
    [Tooltip("Référence au LogParadeGameController principal")]
    [SerializeField] private LogParadeGameController gameController;
    
    [Header("Events")]
    [Tooltip("Événement déclenché au début de la partie")]
    public UnityEvent OnGameStart = new UnityEvent();
    
    [Tooltip("Événement déclenché à la fin de la partie")]
    public UnityEvent OnGameEnd = new UnityEvent();
    
    [Tooltip("Événement déclenché à chaque seconde (avec temps restant)")]
    public UnityEvent<float> OnTimerTick = new UnityEvent<float>();    [Header("Debug")]
    [Tooltip("Afficher les logs de debug dans la console")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // État du jeu
    private bool gameStarted = false;
    private bool gameEnded = false;
    private float timeRemaining = 0f;
    private Coroutine gameTimerCoroutine;
    
    // Propriétés publiques
    /// <summary>
    /// Indique si la partie est actuellement en cours
    /// </summary>
    public bool IsGameActive => gameStarted && !gameEnded;
    
    /// <summary>
    /// Temps restant en secondes
    /// </summary>
    public float TimeRemaining => timeRemaining;
    
    /// <summary>
    /// Durée totale de la partie
    /// </summary>
    public float GameDuration => gameDurationInSeconds;
      protected override void Launch()
    {
        InitializeTimer();
        LaunchLevel();
    }
    
    void Start()
    {
        // Si on n'est pas lancé par MiniGameBase, on peut démarrer automatiquement
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        
        // Validation des composants au démarrage
        ValidateComponents();
    }
      /// <summary>
    /// Initialise le système de timer
    /// </summary>
    private void InitializeTimer()
    {
        // Reset de l'état
        gameStarted = false;
        gameEnded = false;
        timeRemaining = gameDurationInSeconds;
        
        // Arrêter toute coroutine en cours
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }
          // Nettoyer les rondins existants
        if (logGenerator != null)
        {
            logGenerator.StopGeneration();
            logGenerator.ClearAllLogs();
        }
    }
      /// <summary>
    /// Lance le niveau - démarrage manuel propre
    /// </summary>
    public void LaunchLevel()
    {
        if (gameStarted)
        {
            return;
        }        // Vérifier si le gameplay est autorisé (après calibration)
        if (!LogParadeGameStateController.CanStartGameplay())
        {
            LogParadeLogger.LogWarning("Impossible de lancer le niveau : calibration en cours!");
            return;
        }        // Validation finale des composants
        if (!ValidateComponents())
        {
            LogParadeLogger.LogError("Impossible de lancer le niveau - composants manquants !");
            return;
        }
        
        // Démarrer le processus avec délai
        StartCoroutine(StartGameAfterDelay());
    }
    
    /// <summary>
    /// Démarre le jeu après le délai configuré
    /// </summary>
    private IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        
        StartGame();
    }
      /// <summary>
    /// Démarre effectivement la partie
    /// </summary>
    private void StartGame()
    {
        if (gameStarted) return;        // Vérifier si le gameplay est autorisé (après calibration)
        if (!LogParadeGameStateController.CanStartGameplay())
        {
            LogParadeLogger.LogWarning("Impossible de démarrer la partie : calibration en cours!");
            return;
        }
          gameStarted = true;
        gameEnded = false;
        timeRemaining = gameDurationInSeconds;
        
        // Démarrer le score
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
        }
          // Démarrer la génération des rondins
        if (logGenerator != null)
        {
            logGenerator.StartLogGeneration();
        }
        
        // Déclencher l'événement de début
        OnGameStart?.Invoke();
        
        // Démarrer le timer principal
        gameTimerCoroutine = StartCoroutine(GameTimerLoop());
    }
    
    /// <summary>
    /// Boucle principale du timer de jeu
    /// </summary>
    private IEnumerator GameTimerLoop()
    {
        while (timeRemaining > 0f && !gameEnded)
        {
            // Attendre une seconde
            yield return new WaitForSeconds(1f);
            
            // Décrémenter le temps
            timeRemaining = Mathf.Max(0f, timeRemaining - 1f);
              // Déclencher l'événement de tick
            OnTimerTick?.Invoke(timeRemaining);
        }
        
        // Temps écoulé - fin de partie
        EndGame();
    }
    
    /// <summary>
    /// Termine la partie
    /// </summary>
    private void EndGame()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        gameStarted = false;
        timeRemaining = 0f;
        
        // Arrêter le score
        if (scoreManager != null)
        {
            scoreManager.StopScoring();
        }
        
        // Arrêter la génération des rondins
        if (logGenerator != null)
        {
            logGenerator.StopGeneration();
        }
        
        // Arrêter le mouvement de tous les rondins existants
        StopAllLogMovement();
        
        // Arrêter le timer
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
            gameTimerCoroutine = null;
        }
          // Déclencher les événements de fin
        OnGameEnd?.Invoke();
        LogParadeEventCoordinator.TriggerGameEnded();
        
        // Calculer et notifier le score final
        if (scoreManager != null)
        {
            int finalScore = scoreManager.GetCurrentScore();
            LogParadeEventCoordinator.TriggerFinalScoreCalculated(finalScore);
        }
        
        // Appeler FinishMiniGame de MiniGameBase
        FinishMiniGame();
    }
    
    /// <summary>
    /// Arrête le mouvement de tous les rondins actifs
    /// </summary>
    private void StopAllLogMovement()
    {
        LogParadeLog[] activeLogs = FindObjectsByType<LogParadeLog>(FindObjectsSortMode.None);
        
        if (activeLogs.Length > 0)
        {
            foreach (LogParadeLog log in activeLogs)
            {
                if (log != null)
                {
                    log.StopMovement();
                }            }
        }
    }
    
    /// <summary>
    /// Valide que tous les composants nécessaires sont présents
    /// </summary>
    private bool ValidateComponents()
    {
        bool allValid = true;
          // Validation du ScoreManager via SystemValidator
        if (scoreManager == null)
        {
            var validator = LogParadeSystemValidator.Instance;
            if (validator != null)
            {
                scoreManager = validator.GetValidatedComponent<LogParadeScoreManager>();
            }
              // Fallback si SystemValidator pas disponible
            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<LogParadeScoreManager>();
            }
              if (scoreManager == null)
            {
                LogParadeLogger.LogError("LogParadeScoreManager non trouvé !");
                allValid = false;
            }
        }
          // Validation du LogGenerator
        if (logGenerator == null)
        {            logGenerator = FindFirstObjectByType<LogParadeLogGenerator>();
            if (logGenerator == null)
            {
                LogParadeLogger.LogError("LogParadeLogGenerator non trouvé !");
                allValid = false;
            }
        }        // Validation du GameController (optionnel)
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<LogParadeGameController>();
            if (gameController == null)
            {
                LogParadeLogger.LogVerbose("LogParadeGameController non trouvé (optionnel)");
            }
        }
        
        return allValid;
    }
    
    /// <summary>
    /// Arrête manuellement la partie (pour tests ou UI)
    /// </summary>
    public void StopGame()
    {
        if (!gameStarted || gameEnded)
        {
            return;
        }
        
        EndGame();
    }
      /// <summary>
    /// Redémarre une nouvelle partie
    /// </summary>
    public void RestartGame()
    {
        // Arrêter la partie en cours si nécessaire
        if (gameStarted && !gameEnded)
        {
            StopGame();
        }
        
        // Réinitialiser et relancer
        InitializeTimer();
        LaunchLevel();
    }
      /// <summary>
    /// Change la durée de la partie (seulement si pas en cours)
    /// </summary>
    public void SetGameDuration(float newDuration)
    {
        if (gameStarted && !gameEnded)
        {
            LogParadeLogger.LogWarning("Impossible de changer la durée pendant une partie en cours");
            return;
        }
        
        gameDurationInSeconds = Mathf.Max(1f, newDuration);
    }
    
    /// <summary>
    /// Obtient le score actuel (si disponible)
    /// </summary>
    public int GetCurrentScore()
    {
        if (scoreManager != null)
        {
            return scoreManager.GetCurrentScore();
        }
        return 0;
    }
    
    void OnDestroy()
    {        // Nettoyer les coroutines
        if (gameTimerCoroutine != null)
        {
            StopCoroutine(gameTimerCoroutine);
        }
    }
}
