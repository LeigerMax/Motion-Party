using UnityEngine;
using Gameplay.FireFlyDance.Core;
using Newtonsoft.Json.Linq;
using System.Collections;
using Gameplay.FireFlyDance.Hand;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Scoring;
using Gameplay.FireFlyDance.Utils;
using Core;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Contrôleur principal du mini-jeu "Danse des Lucioles"
    /// Gère le suivi de la main et l'interaction avec les lucioles
    /// </summary>
    public class FireflyDanceGameManager : MiniGameBase
{
    #region Fields

    [Header("Managers")]
    public UDPReceive udpReceive;
    public FireflyDanceStateController stateController;
    public FireflyScoreManager scoreManager;
    public FireflySpawner spawner;

    [Header("Hand Tracking")]
    public HandTracker handTracker;
    public HandInteractor handInteractor;

    [Header("Game Settings")]
    public FireflyDanceConfig config;
    public float startDelay = 1f;
    public bool enableDebugMode = true;

    private bool gameStarted = false;
    private bool gameEnded = false;

    #endregion

    #region Unity Lifecycle

    protected override void Launch()
    {
        InitGame();
    }

    void Start()
    {
        ValidateComponents();
        SetupEventListeners();
        Launch();
    }

    void Update()
    {
        if (gameStarted && !gameEnded)
        {
            UpdateGameplay();
        }
    }

    void OnDestroy()
    {
        CleanupEventListeners();
    }

    #endregion

    #region Game Management

    /// <summary>
    /// Initialise le jeu
    /// </summary>
    private void InitGame()
    {
        if (!ValidateComponents())
        {
            FireflyDanceLogger.LogError("Composants manquants - impossible de démarrer", this);
            return;
        }

        FireflyDanceLogger.Log("Initialisation du jeu Danse des Lucioles");

        // Initialisation des systèmes
        InitializeHandTracking();
        InitializeScoring();
        InitializeSpawning();

        // Démarrage du jeu après délai
        StartCoroutine(StartGameAfterDelay());
    }

    /// <summary>
    /// Démarre le jeu après un délai
    /// </summary>
    private IEnumerator StartGameAfterDelay()
    {
        FireflyDanceLogger.Log($"Démarrage dans {startDelay}s...");
        yield return new WaitForSeconds(startDelay);
        StartGame();
    }

    /// <summary>
    /// Démarre le gameplay principal
    /// </summary>
    public void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        FireflyDanceLogger.Log("Jeu démarré !");

        // Activer les systèmes
        FireflyDanceStateController.StartGameplay();

        if (spawner != null)
            spawner.StartSpawning();

        // Émettre l'événement de démarrage
        FireflyDanceEvents.OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Arrête le jeu
    /// </summary>
    public void EndGame()
    {
        if (!gameStarted || gameEnded) return;

        gameEnded = true;
        FireflyDanceLogger.Log("Jeu terminé !");

        // Désactiver les systèmes
        if (spawner != null)
            spawner.StopSpawning();

        FireflyDanceStateController.EndGameplay();

        // Émettre l'événement de fin
        FireflyDanceEvents.OnGameEnded?.Invoke();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialise le système de suivi de la main
    /// </summary>
    private void InitializeHandTracking()
    {
        if (handTracker != null)
        {
            handTracker.Initialize(config);
            FireflyDanceLogger.Log("Hand Tracker initialisé");
        }

        if (handInteractor != null)
        {
            handInteractor.Initialize(config);
            FireflyDanceLogger.Log("Hand Interactor initialisé");
        }
    }

    /// <summary>
    /// Initialise le système de score
    /// </summary>
    private void InitializeScoring()
    {
        if (scoreManager != null)
        {
            scoreManager.Initialize(config);
            FireflyDanceLogger.Log("Score Manager initialisé");
        }
    }

    /// <summary>
    /// Initialise le système de spawn
    /// </summary>
    private void InitializeSpawning()
    {
        if (spawner != null)
        {
            spawner.Initialize(config);
            FireflyDanceLogger.Log("Spawner initialisé");
        }
    }

    #endregion

    #region Event Management

    /// <summary>
    /// Configure les écouteurs d'événements
    /// </summary>
    private void SetupEventListeners()
    {
        FireflyDanceEvents.OnFireflyCaptured += HandleFireflyCaptured;
        FireflyDanceEvents.OnScoreChanged += HandleScoreChanged;
    }

    /// <summary>
    /// Nettoie les écouteurs d'événements
    /// </summary>
    private void CleanupEventListeners()
    {
        FireflyDanceEvents.OnFireflyCaptured -= HandleFireflyCaptured;
        FireflyDanceEvents.OnScoreChanged -= HandleScoreChanged;
    }

    /// <summary>
    /// Gère la capture d'une luciole
    /// </summary>
    private void HandleFireflyCaptured(FireflyController firefly)
    {
        FireflyDanceLogger.Log($"Luciole capturée ! (Score géré par FireflyScoreManager)");
        
        // Le score est maintenant automatiquement géré par le FireflyScoreManager
        // qui écoute l'événement OnFireflyCaptured
    }

    /// <summary>
    /// Gère le changement de score
    /// </summary>
    private void HandleScoreChanged(int newScore)
    {
        FireflyDanceLogger.Log($"Nouveau score : {newScore}");
    }

    #endregion

    #region Gameplay Update

    /// <summary>
    /// Met à jour le gameplay principal
    /// </summary>
    private void UpdateGameplay()
    {
        // Logique de gameplay additionnelle si nécessaire
        // (déjà gérée par les systèmes individuels)
    }

    #endregion

    #region Validation

    /// <summary>
    /// Valide que tous les composants nécessaires sont présents
    /// </summary>
    private bool ValidateComponents()
    {
        bool isValid = true;

        if (config == null)
        {
            FireflyDanceLogger.LogWarning("FireflyDanceConfig manquant - auto-configuration en cours...");
            FireflyDanceConfigHelper.EnsureConfig(ref config, "GameManager");
        }

        if (udpReceive == null)
        {
            udpReceive = FindFirstObjectByType<UDPReceive>();
            if (udpReceive == null)
            {
                FireflyDanceLogger.LogError("UDPReceive manquant !");
                isValid = false;
            }
        }

        if (handTracker == null)
        {
            FireflyDanceLogger.LogWarning("HandTracker manquant !");
        }

        if (scoreManager == null)
        {
            FireflyDanceLogger.LogWarning("ScoreManager manquant !");
        }

        if (spawner == null)
        {
            FireflyDanceLogger.LogWarning("Spawner manquant !");
        }

        return isValid;
    }

    /// <summary>
    /// Crée une configuration par défaut si aucune n'est assignée
    /// </summary>
    private FireflyDanceConfig CreateDefaultConfig()
    {
        var defaultConfig = FireflyDanceConfig.CreateDefault();
        
        FireflyDanceLogger.Log("Configuration par défaut créée automatiquement");
        return defaultConfig;
    }

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        if (config != null && enableDebugMode)
        {
            // Zone de jeu
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(config.Center.x, config.Center.y, 0);
            Vector3 size = new Vector3(config.Width, config.Height, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }

    #endregion
}
}
