using UnityEngine;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Utils;
using Gameplay.FireFlyDance.Core;

namespace Gameplay.FireFlyDance.Scoring
{
    /// <summary>
    /// Gestionnaire de score pour le mini-jeu Danse des Lucioles
    /// Gère le score, les records et les événements de scoring
    /// </summary>
    public class FireflyScoreManager : MonoBehaviour
{
    #region Fields

    [Header("Configuration")]
    [SerializeField] private FireflyDanceConfig config;
    
    [Header("Score Settings")]
    [SerializeField] private bool saveHighScore = true;
    [SerializeField] private string highScoreKey = "FireflyDance_HighScore";
    
    [Header("Runtime Info (Read Only)")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private int firefliesCaptured = 0;
    [SerializeField] private float gameStartTime = 0f;

    // État interne
    private bool isInitialized = false;
    private bool isGameActive = false;

    // Propriétés publiques
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int FirefliesCaptured => firefliesCaptured;
    public float GameDuration => isGameActive ? Time.time - gameStartTime : 0f;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        if (config != null)
        {
            Initialize(config);
        }
    }

    void OnDestroy()
    {
        CleanupEventListeners();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialise le gestionnaire de score
    /// </summary>
    public void Initialize(FireflyDanceConfig danceConfig)
    {
        config = danceConfig;
        
        LoadHighScore();
        SetupEventListeners();
        ResetScore();
        
        isInitialized = true;
        FireflyDanceLogger.LogScore($"ScoreManager initialisé - Record: {highScore}");
    }

    /// <summary>
    /// Configure les écouteurs d'événements
    /// </summary>
    private void SetupEventListeners()
    {
        FireflyDanceEvents.OnGameStarted += OnGameStarted;
        FireflyDanceEvents.OnGameEnded += OnGameEnded;
        FireflyDanceEvents.OnFireflyCaptured += OnFireflyCaptured;
    }

    /// <summary>
    /// Nettoie les écouteurs d'événements
    /// </summary>
    private void CleanupEventListeners()
    {
        FireflyDanceEvents.OnGameStarted -= OnGameStarted;
        FireflyDanceEvents.OnGameEnded -= OnGameEnded;
        FireflyDanceEvents.OnFireflyCaptured -= OnFireflyCaptured;
    }

    #endregion

    #region Score Management

    /// <summary>
    /// Ajoute des points au score
    /// </summary>
    public void AddScore(int points)
    {
        if (!isInitialized || !isGameActive)
        {
            FireflyDanceLogger.LogWarning("Impossible d'ajouter des points - jeu inactif");
            return;
        }

        int oldScore = currentScore;
        currentScore += points;
        
        FireflyDanceLogger.LogScore($"Score +{points} = {currentScore}");
        
        // Vérifier si c'est un nouveau record
        if (currentScore > highScore)
        {
            SetNewHighScore(currentScore);
        }
        
        // Émettre l'événement de changement de score
        FireflyDanceEvents.OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Remet le score à zéro
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        firefliesCaptured = 0;
        gameStartTime = 0f;
        
        FireflyDanceLogger.LogScore("Score réinitialisé");
        FireflyDanceEvents.OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Définit un nouveau record
    /// </summary>
    private void SetNewHighScore(int newHighScore)
    {
        int oldHighScore = highScore;
        highScore = newHighScore;
        
        if (saveHighScore)
        {
            SaveHighScore();
        }
        
        FireflyDanceLogger.LogScore($"Nouveau record ! {oldHighScore} -> {newHighScore}");
        FireflyDanceEvents.OnNewHighScore?.Invoke(newHighScore);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Gère le début du jeu
    /// </summary>
    private void OnGameStarted()
    {
        isGameActive = true;
        gameStartTime = Time.time;
        ResetScore();
        
        FireflyDanceLogger.LogScore("Jeu démarré - Score actif");
    }

    /// <summary>
    /// Gère la fin du jeu
    /// </summary>
    private void OnGameEnded()
    {
        isGameActive = false;
        
        FireflyDanceLogger.LogScore($"Jeu terminé - Score final: {currentScore}, Lucioles: {firefliesCaptured}, Durée: {GameDuration:F1}s");
    }

    /// <summary>
    /// Gère la capture d'une luciole
    /// </summary>
    private void OnFireflyCaptured(FireflyController firefly)
    {
        if (!isGameActive) return;
        
        firefliesCaptured++;
        
        // Ajouter les points configurés
        int points = config != null ? config.ScorePerFirefly : 10;
        AddScore(points);
        
        FireflyDanceLogger.LogScore($"Luciole capturée #{firefliesCaptured} - +{points} points");
    }

    #endregion

    #region High Score Persistence

    /// <summary>
    /// Charge le record depuis les PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        if (saveHighScore)
        {
            highScore = PlayerPrefs.GetInt(highScoreKey, 0);
            FireflyDanceLogger.LogScore($"Record chargé: {highScore}");
        }
    }

    /// <summary>
    /// Sauvegarde le record dans les PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        if (saveHighScore)
        {
            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();
            FireflyDanceLogger.LogScore($"Record sauvegardé: {highScore}");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Calcule le score par seconde
    /// </summary>
    public float GetScorePerSecond()
    {
        float duration = GameDuration;
        return duration > 0 ? currentScore / duration : 0f;
    }

    /// <summary>
    /// Calcule le score par luciole
    /// </summary>
    public float GetScorePerFirefly()
    {
        return firefliesCaptured > 0 ? (float)currentScore / firefliesCaptured : 0f;
    }

    /// <summary>
    /// Obtient les statistiques de jeu
    /// </summary>
    public GameStats GetGameStats()
    {
        return new GameStats
        {
            score = currentScore,
            firefliesCaptured = firefliesCaptured,
            gameDuration = GameDuration,
            scorePerSecond = GetScorePerSecond(),
            scorePerFirefly = GetScorePerFirefly()
        };
    }

    /// <summary>
    /// Remet le record à zéro
    /// </summary>
    [ContextMenu("Reset High Score")]
    public void ResetHighScore()
    {
        highScore = 0;
        if (saveHighScore)
        {
            PlayerPrefs.DeleteKey(highScoreKey);
            PlayerPrefs.Save();
        }
        
        FireflyDanceLogger.LogScore("Record réinitialisé");
    }

    /// <summary>
    /// Force la sauvegarde du record actuel
    /// </summary>
    [ContextMenu("Save Current Score as High Score")]
    public void SaveCurrentScoreAsHighScore()
    {
        if (currentScore > highScore)
        {
            SetNewHighScore(currentScore);
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Ajoute des points de debug
    /// </summary>
    [ContextMenu("Add Debug Points")]
    public void AddDebugPoints()
    {
        AddScore(config != null ? config.ScorePerFirefly : 10);
    }

    /// <summary>
    /// Simule la capture d'une luciole
    /// </summary>
    [ContextMenu("Simulate Firefly Capture")]
    public void SimulateFireflyCapture()
    {
        firefliesCaptured++;
        AddScore(config != null ? config.ScorePerFirefly : 10);
    }

    /// <summary>
    /// Affiche les statistiques actuelles
    /// </summary>
    [ContextMenu("Log Current Stats")]
    public void LogCurrentStats()
    {
        GameStats stats = GetGameStats();
        FireflyDanceLogger.LogScore($"Stats - Score: {stats.score}, Lucioles: {stats.firefliesCaptured}, Durée: {stats.gameDuration:F1}s, Score/s: {stats.scorePerSecond:F1}, Score/luciole: {stats.scorePerFirefly:F1}");
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// Structure des statistiques de jeu
    /// </summary>
    [System.Serializable]
    public struct GameStats
    {
        public int score;
        public int firefliesCaptured;
        public float gameDuration;
        public float scorePerSecond;
        public float scorePerFirefly;
    }

    #endregion
}
}
