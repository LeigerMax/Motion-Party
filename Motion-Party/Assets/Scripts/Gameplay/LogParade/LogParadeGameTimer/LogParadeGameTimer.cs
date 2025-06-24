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
    public UnityEvent<float> OnTimerTick = new UnityEvent<float>();
      [Header("Debug")]
    [Tooltip("Afficher les logs de debug dans la console")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Tooltip("Position X de l'interface de debug (en pixels depuis le bord gauche)")]
    [SerializeField] private float debugGuiX = -250f; // Valeur négative = depuis le bord droit
    
    [Tooltip("Position Y de l'interface de debug (en pixels depuis le haut)")]
    [SerializeField] private float debugGuiY = 10f;
    
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
        }
        
        // Vérifier si le gameplay est autorisé (après calibration)
        if (!LogParadeCalibrationManager.CanStartGameplay())
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[LogParadeGameTimer] ⚠️ Impossible de lancer le niveau : calibration en cours!");
            }
            return;
        }// Validation finale des composants
        if (!ValidateComponents())
        {
            Debug.LogError("[LogParadeGameTimer] Impossible de lancer le niveau - composants manquants !");
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
        if (gameStarted) return;
        
        // Vérifier si le gameplay est autorisé (après calibration)
        if (!LogParadeCalibrationManager.CanStartGameplay())
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[LogParadeGameTimer] ⚠️ Impossible de démarrer la partie : calibration en cours!");
            }
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
        
        // Déclencher l'événement de fin
        OnGameEnd?.Invoke();
          // Appeler FinishMiniGame de MiniGameBase
        FinishMiniGame();
    }
    
    /// <summary>
    /// Arrête le mouvement de tous les rondins actifs
    /// </summary>
    private void StopAllLogMovement()
    {
        LogParadeLog[] activeLogs = FindObjectsOfType<LogParadeLog>();
        
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
        
        // Validation du ScoreManager
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<LogParadeScoreManager>();
            if (scoreManager == null)
            {
                Debug.LogError("[LogParadeGameTimer] ❌ LogParadeScoreManager non trouvé !");
                allValid = false;
            }            else
            {
                // LogParadeScoreManager trouvé automatiquement, pas besoin de log
            }
        }
        
        // Validation du LogGenerator
        if (logGenerator == null)
        {
            logGenerator = FindObjectOfType<LogParadeLogGenerator>();
            if (logGenerator == null)
            {
                Debug.LogError("[LogParadeGameTimer] ❌ LogParadeLogGenerator non trouvé !");
                allValid = false;
            }            else
            {
                // LogParadeLogGenerator trouvé automatiquement, pas besoin de log
            }
        }
        
        // Validation du GameController (optionnel)
        if (gameController == null)
        {
            gameController = FindObjectOfType<LogParadeGameController>();
            if (gameController == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("[LogParadeGameTimer] ⚠️ LogParadeGameController non trouvé (optionnel)");
                }
            }            else
            {
                // LogParadeGameController trouvé automatiquement, pas besoin de log
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
            if (enableDebugLogs)
            {
                Debug.LogWarning("[LogParadeGameTimer] Impossible de changer la durée pendant une partie en cours");
            }
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
