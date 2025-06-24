using UnityEngine;
using System.Collections;
using System.Reflection;

/// <summary>
/// Script d'intégration pour connecter le système de calibration interactive
/// avec le GameManager et les autres systèmes LogParade
/// </summary>
public class LogParadeCalibrationManager : MonoBehaviour
{
    [Header("Calibration Settings")]
    [SerializeField] private bool enableCalibrationOnStart = true;
    [SerializeField] private bool bypassCalibrationInEditor = false;
    [SerializeField] private float delayBeforeGameStart = 1f;
    
    [Header("References")]
    [SerializeField] private LogParadeCalibrationInteractive calibrationSystem;
    [SerializeField] private LogParadePlayerAvatar playerAvatar;
    [SerializeField] private LogParadeUIManager uiManager;
    [SerializeField] private LogParadeGameController gameController;
    [SerializeField] private LogParadeScoreManager scoreManager; // Référence ajoutée pour le ScoreManager
    
    [Header("Auto-Assignment")]
    [SerializeField] private bool autoFindReferences = true;
      [Header("Game Control")]
    [SerializeField] private bool preventScoreUntilCalibrated = true;
    [SerializeField] private bool preventGameplayUntilCalibrated = true;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private bool showDetailedLogs = true;
    
    private bool calibrationCompleted = false;
    private bool gameStarted = false;

    // Système de verrouillage global
    public static bool IsCalibrationInProgress { get; private set; } = false;
    public static bool IsGameplayAllowed { get; private set; } = false;
    
    void Awake()
    {
        if (autoFindReferences)
        {
            AutoAssignReferences();
        }
    }

    void Start()
    {
        // S'abonner aux événements de calibration
        LogParadeCalibrationInteractive.OnCalibrationCompleted += OnCalibrationCompleted;
        LogParadeCalibrationInteractive.OnCalibrationFailed += OnCalibrationFailed;
        LogParadeCalibrationInteractive.OnLaneReached += OnLaneReached;
        
        // Démarrer le processus
        if (enableCalibrationOnStart)
        {
            StartCalibrationProcess();
        }
    }    void OnDestroy()
    {
        // Se désabonner des événements de calibration
        LogParadeCalibrationInteractive.OnCalibrationCompleted -= OnCalibrationCompleted;
        LogParadeCalibrationInteractive.OnCalibrationFailed -= OnCalibrationFailed;
        LogParadeCalibrationInteractive.OnLaneReached -= OnLaneReached;
        
        // Se désabonner de l'événement de fin de partie
        var gameTimer = FindObjectOfType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            gameTimer.OnGameEnd.RemoveListener(OnGameEndDetected);
        }
    }

    /// <summary>
    /// Trouve automatiquement les références dans la scène
    /// </summary>
    private void AutoAssignReferences()
    {
        if (calibrationSystem == null)
        {
            calibrationSystem = FindObjectOfType<LogParadeCalibrationInteractive>();
        }
        
        if (playerAvatar == null)
        {
            playerAvatar = FindObjectOfType<LogParadePlayerAvatar>();
        }
        
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<LogParadeUIManager>();
        }
        
        if (gameController == null)
        {
            gameController = FindObjectOfType<LogParadeGameController>();
        }
        
        if (scoreManager == null) // Auto-assignation du ScoreManager
        {
            scoreManager = FindObjectOfType<LogParadeScoreManager>();
        }
        
        Debug.Log($"Auto-assignment: Calibration={calibrationSystem != null}, Player={playerAvatar != null}, UI={uiManager != null}, Game={gameController != null}, ScoreManager={scoreManager != null}");
    }    /// <summary>
    /// Démarre le processus de calibration
    /// </summary>
    public void StartCalibrationProcess()
    {
        // Bloquer le gameplay et le score pendant la calibration
        IsCalibrationInProgress = true;
        IsGameplayAllowed = false;
        
        // Vérifier si on doit bypasser en mode éditeur
        if (bypassCalibrationInEditor && Application.isEditor)
        {
            Debug.Log("Calibration bypassée en mode éditeur");
            OnCalibrationCompleted();
            return;
        }

        // S'assurer que le système de calibration est disponible
        if (calibrationSystem == null)
        {
            Debug.LogError("Système de calibration non trouvé! Démarrage du jeu sans calibration.");
            OnCalibrationCompleted();
            return;
        }

        // Initialiser l'UI pour la calibration
        if (uiManager != null)
        {
            uiManager.ShowCalibrationUI(true);
        }

        // Préparer le joueur pour la calibration
        PreparePlayerForCalibration();
        
        // Démarrer la calibration
        calibrationSystem.StartCalibration();
        
        Debug.Log("Processus de calibration démarré");
    }

    /// <summary>
    /// Prépare le joueur pour la calibration
    /// </summary>
    private void PreparePlayerForCalibration()
    {
        if (playerAvatar != null)
        {
            // S'assurer que le joueur est activé et visible
            playerAvatar.gameObject.SetActive(true);
            
            // Positionner le joueur au centre (lane 2)
            playerAvatar.SetLaneInstant(2);
            
            Debug.Log("Joueur préparé pour la calibration");
        }
    }    /// <summary>
    /// Appelé quand la calibration est terminée avec succès
    /// </summary>
    private void OnCalibrationCompleted()
    {
        if (calibrationCompleted) return;
        
        calibrationCompleted = true;
          // Débloquer le gameplay et permettre le score
        IsCalibrationInProgress = false;
        IsGameplayAllowed = true;
        
        if (showDetailedLogs)
        {
            Debug.Log("Calibration terminée - Score et gameplay débloqués - Démarrage du jeu");
        }
          // Masquer l'UI de calibration
        if (uiManager != null)
        {
            uiManager.ShowCalibrationUI(false);
        }
          // Nettoyer les rondins de calibration
        if (calibrationSystem != null)
        {
            calibrationSystem.CleanupCalibrationLogs();
            if (showDetailedLogs)
            {
                Debug.Log("Rondins de calibration supprimés");
            }
        }
        
        // Démarrer le jeu après un délai
        StartCoroutine(StartGameAfterDelay());
    }

    /// <summary>
    /// Appelé quand la calibration échoue
    /// </summary>
    private void OnCalibrationFailed()
    {
        Debug.Log("Calibration échouée - le système va redemander");
        // Le système de calibration gère automatiquement la relance
    }

    /// <summary>
    /// Appelé quand le joueur atteint une lane pendant la calibration
    /// </summary>
    private void OnLaneReached(int laneNumber)
    {
        Debug.Log($"Lane {laneNumber} atteinte pendant la calibration");
        
        // Optionnel: feedback supplémentaire via l'UI
        if (uiManager != null)
        {
            // Vous pouvez ajouter un feedback spécifique ici
        }
    }    /// <summary>
    /// Démarre le jeu principal après un délai
    /// </summary>
    private IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeGameStart);
        
        if (!gameStarted)
        {
            StartMainGame();
        }
    }/// <summary>
    /// Démarre le jeu principal LogParade
    /// </summary>
    private void StartMainGame()
    {
        StartCoroutine(StartMainGameCoroutine());
    }
      /// <summary>
    /// Démarre le jeu principal LogParade (version coroutine)
    /// </summary>
    private IEnumerator StartMainGameCoroutine()
    {
        gameStarted = true;        // Activer le contrôleur de jeu principal
        if (gameController != null)
        {
            gameController.gameObject.SetActive(true);
            
            // Utiliser la méthode ForceStartGame() pour un démarrage immédiat après calibration
            bool forceStartSucceeded = false;
            try
            {
                gameController.ForceStartGame();
                Debug.Log("Jeu LogParade lancé avec succès via ForceStartGame()");
                forceStartSucceeded = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors du lancement du jeu: {ex.Message}");
                forceStartSucceeded = false;
            }
            
            // Si ForceStartGame a échoué, essayer le fallback
            if (!forceStartSucceeded)
            {
                Debug.Log("Tentative de relancement via activation/désactivation...");
                yield return StartCoroutine(RestartGameControllerCoroutine());
            }
        }        // Initialiser l'UI principale du jeu
        if (uiManager != null)
        {
            uiManager.InitializeUI();
            uiManager.ShowGameUI(); // Afficher l'UI de jeu après calibration
            if (showDetailedLogs)
            {
                Debug.Log("UI de jeu affichée après calibration");
            }
        }        // Démarrer le GameTimer si présent
        var gameTimer = FindObjectOfType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            try
            {
                // S'assurer que le timer est activé
                gameTimer.gameObject.SetActive(true);
                
                // Activer tous les composants UI enfants du timer
                var timerUIComponents = gameTimer.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                foreach (var uiText in timerUIComponents)
                {
                    uiText.gameObject.SetActive(true);
                }
                
                var timerImageComponents = gameTimer.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var uiImage in timerImageComponents)
                {
                    uiImage.gameObject.SetActive(true);
                }
                  // Lancer le niveau
                gameTimer.LaunchLevel();
                
                // S'abonner à l'événement de fin de partie
                gameTimer.OnGameEnd.AddListener(OnGameEndDetected);
                
                if (showDetailedLogs)
                {
                    Debug.Log("GameTimer activé et lancé avec succès");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"GameTimer non lancé: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("GameTimer non trouvé dans la scène");
        }// Démarrer le ScoreManager si présent et pas déjà actif
        if (scoreManager != null && !scoreManager.IsScoring)
        {
            try
            {
                scoreManager.StartScoring();
                if (showDetailedLogs)
                {
                    Debug.Log("ScoreManager lancé avec succès");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ScoreManager non lancé: {ex.Message}");
            }
        }        // Démarrer le LogParadeLogGenerator pour la génération des rondins de gameplay
        var logGenerator = FindObjectOfType<LogParadeLogGenerator>();
        if (logGenerator != null)
        {
            try
            {
                logGenerator.gameObject.SetActive(true);
                logGenerator.StartLogGeneration();
                if (showDetailedLogs)
                {
                    Debug.Log("LogParadeLogGenerator activé et génération des rondins démarrée avec succès");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"LogParadeLogGenerator non lancé: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("LogParadeLogGenerator non trouvé dans la scène");
        }
          Debug.Log("Jeu LogParade démarré!");
        
        // Valider que tout a bien démarré
        ValidateGameStarted();
        
        // Optionnel: Envoyer un événement global
        SendGameStartedEvent();

        // Valider le démarrage du jeu
        ValidateGameStarted();
    }

    /// <summary>
    /// Envoie un événement global que le jeu a démarré
    /// </summary>
    private void SendGameStartedEvent()
    {
        // Si vous avez un système d'événements global, l'utiliser ici
        // Exemple: EventManager.Instance.TriggerEvent("GameStarted");
        
        // Pour l'instant, juste un log
        Debug.Log("Événement 'Jeu Démarré' envoyé");
    }    /// <summary>
    /// Méthode publique pour redémarrer la calibration
    /// </summary>
    public void RestartCalibration()
    {
        calibrationCompleted = false;
        gameStarted = false;
        
        // Relancer les verrous
        IsCalibrationInProgress = true;
        IsGameplayAllowed = false;
        
        if (calibrationSystem != null)
        {
            calibrationSystem.RestartCalibration();
        }
        
        Debug.Log("Calibration redémarrée");
    }

    /// <summary>
    /// Méthode publique pour bypasser la calibration (pour les tests)
    /// </summary>
    public void BypassCalibration()
    {
        Debug.Log("Calibration bypassée manuellement");
        OnCalibrationCompleted();
    }

    /// <summary>
    /// Vérifie si la calibration est terminée
    /// </summary>
    public bool IsCalibrationCompleted()
    {
        return calibrationCompleted;
    }

    /// <summary>
    /// Vérifie si le jeu a démarré
    /// </summary>
    public bool IsGameStarted()
    {
        return gameStarted;
    }

    /// <summary>
    /// Obtient le statut actuel du processus
    /// </summary>
    public string GetCurrentStatus()
    {
        if (!calibrationCompleted)
        {
            if (calibrationSystem != null && calibrationSystem.IsCalibrationActive())
            {
                return $"Calibration en cours: {calibrationSystem.GetCalibrationStatus()}";
            }
            return "En attente de calibration";
        }
        else if (!gameStarted)
        {
            return "Calibration terminée, démarrage du jeu...";
        }
        else
        {
            return "Jeu en cours";
        }
    }

    /// <summary>
    /// Vérifie si le scoring peut être démarré (après calibration)
    /// </summary>
    public static bool CanStartScoring()
    {
        return IsGameplayAllowed && !IsCalibrationInProgress;
    }
    
    /// <summary>
    /// Vérifie si le gameplay peut être démarré (après calibration)
    /// </summary>
    public static bool CanStartGameplay()
    {
        return IsGameplayAllowed && !IsCalibrationInProgress;
    }
    
    /// <summary>
    /// Force l'activation du gameplay (pour debug uniquement)
    /// </summary>
    [System.Obsolete("Utiliser uniquement pour les tests")]
    public static void ForceEnableGameplay()
    {
        IsCalibrationInProgress = false;
        IsGameplayAllowed = true;
        Debug.LogWarning("[LogParadeCalibrationManager] ⚠️ GAMEPLAY FORCÉ - Pour debug uniquement!");
    }    // Interface de debug

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!showDebugUI) return;

        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 150));
        GUILayout.Label("=== Calibration Manager ===");
        GUILayout.Label($"Status: {GetCurrentStatus()}");
        GUILayout.Label($"Calibration OK: {calibrationCompleted}");
        GUILayout.Label($"Jeu démarré: {gameStarted}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Start Calibration"))
        {
            StartCalibrationProcess();
        }
        
        if (GUILayout.Button("Bypass Calibration"))
        {
            BypassCalibration();
        }
        
        if (GUILayout.Button("Restart Calibration"))
        {
            RestartCalibration();
        }
        
        GUILayout.EndArea();
    }
#endif

    /// <summary>
    /// Valide que tous les systèmes de jeu ont bien démarré
    /// </summary>
    private void ValidateGameStarted()
    {
        StartCoroutine(ValidateGameStartedCoroutine());
    }
    
    /// <summary>
    /// Coroutine pour valider le démarrage du jeu avec un délai
    /// </summary>
    private System.Collections.IEnumerator ValidateGameStartedCoroutine()
    {
        yield return new WaitForSeconds(2f); // Attendre 2 secondes
        
        bool gameStartedSuccessfully = true;
        System.Text.StringBuilder issues = new System.Text.StringBuilder();
        
        // Vérifier le GameController
        if (gameController != null)
        {
            if (!gameController.IsGameActive())
            {
                gameStartedSuccessfully = false;
                issues.AppendLine("- GameController non actif");
            }
        }
        
        // Vérifier le ScoreManager
        var scoreManager = FindObjectOfType<LogParadeScoreManager>();
        if (scoreManager != null)
        {
            if (!scoreManager.IsScoring)
            {
                gameStartedSuccessfully = false;
                issues.AppendLine("- ScoreManager non actif");
            }
        }
        
        // Vérifier le GameTimer
        var gameTimer = FindObjectOfType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            if (!gameTimer.IsGameActive)
            {
                gameStartedSuccessfully = false;
                issues.AppendLine("- GameTimer non actif");
            }
        }
        
        if (gameStartedSuccessfully)
        {
            Debug.Log("✅ Tous les systèmes de jeu ont démarré avec succès!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Problèmes détectés après le démarrage du jeu:\n{issues.ToString()}");
            
            // Tentative de redémarrage
            Debug.Log("Tentative de redémarrage des systèmes défaillants...");
            if (issues.ToString().Contains("GameController"))
            {
                RestartGameController();
            }
            if (issues.ToString().Contains("ScoreManager"))
            {
                RestartScoreManager();
            }
            if (issues.ToString().Contains("GameTimer"))
            {
                RestartGameTimer();
            }
        }
    }
    
    /// <summary>
    /// Redémarre le GameController
    /// </summary>
    private void RestartGameController()
    {
        if (gameController != null)
        {
            Debug.Log("Redémarrage du GameController...");
            gameController.gameObject.SetActive(false);
            gameController.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Redémarre le ScoreManager
    /// </summary>
    private void RestartScoreManager()
    {
        var scoreManager = FindObjectOfType<LogParadeScoreManager>();
        if (scoreManager != null)
        {
            Debug.Log("Redémarrage du ScoreManager...");
            scoreManager.StopScoring();
            scoreManager.StartScoring();
        }
    }
    
    /// <summary>
    /// Redémarre le GameTimer
    /// </summary>
    private void RestartGameTimer()
    {
        var gameTimer = FindObjectOfType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            Debug.Log("Redémarrage du GameTimer...");
            gameTimer.LaunchLevel();
        }
    }
      /// <summary>
    /// Coroutine pour redémarrer le GameController avec yield
    /// </summary>
    private IEnumerator RestartGameControllerCoroutine()
    {
        bool needsRestart = true;
        
        // Première étape: désactiver
        try
        {
            gameController.gameObject.SetActive(false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur lors de la désactivation: {ex.Message}");
            needsRestart = false;
        }
        
        // Attendre une frame (en dehors du try-catch)
        if (needsRestart)
        {
            yield return null;
        }
        
        // Deuxième étape: réactiver
        if (needsRestart)
        {
            try
            {
                gameController.gameObject.SetActive(true);
                Debug.Log("GameController relancé via activation/désactivation");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de la réactivation: {ex.Message}");
                // Dernier recours: essayer une réactivation simple
                gameController.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Redémarrage du GameController échoué");
        }
    }

    /// <summary>
    /// Appelé quand le timer détecte la fin de partie
    /// </summary>
    private void OnGameEndDetected()
    {
        if (showDetailedLogs)
        {
            Debug.Log("🎉 Fin de partie détectée par le timer!");
        }
        
        // Arrêter la génération de rondins
        var logGenerator = FindObjectOfType<LogParadeLogGenerator>();
        if (logGenerator != null)
        {
            try
            {
                // Si le générateur a une méthode pour arrêter
                var stopMethod = logGenerator.GetType().GetMethod("StopLogGeneration");
                if (stopMethod != null)
                {
                    stopMethod.Invoke(logGenerator, null);
                    if (showDetailedLogs)
                    {
                        Debug.Log("Génération de rondins arrêtée");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Erreur lors de l'arrêt du générateur: {ex.Message}");
            }
        }
        
        // Finaliser le score
        if (scoreManager != null)
        {
            try
            {
                scoreManager.StopScoring();
                if (showDetailedLogs)
                {
                    Debug.Log("Score finalisé");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Erreur lors de l'arrêt du score: {ex.Message}");
            }
        }
        
        // Afficher l'écran de fin de partie si disponible
        if (uiManager != null)
        {
            try
            {
                // Chercher une méthode pour afficher l'écran de fin
                var showEndScreenMethod = uiManager.GetType().GetMethod("ShowEndGameScreen");
                if (showEndScreenMethod != null)
                {
                    showEndScreenMethod.Invoke(uiManager, null);
                }
                else
                {
                    // Fallback : masquer l'UI de jeu
                    var hideGameUIMethod = uiManager.GetType().GetMethod("HideGameUI");
                    if (hideGameUIMethod != null)
                    {
                        hideGameUIMethod.Invoke(uiManager, null);
                    }
                }
                
                if (showDetailedLogs)
                {
                    Debug.Log("UI de fin de partie affichée");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Erreur lors de l'affichage de l'écran de fin: {ex.Message}");
            }
        }
        
        if (showDetailedLogs)
        {
            Debug.Log("🏁 Fin de partie traitée avec succès!");
        }
    }
}
