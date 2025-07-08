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

    // État interne
    private bool isInitialized = false;
    private bool isGameActive = false;

    // Propriétés publiques
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int FirefliesCaptured => firefliesCaptured;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        // Auto-initialisation si pas déjà fait
        if (!isInitialized)
        {
            
            if (config == null)
                config = FindFirstObjectByType<FireflyDanceConfig>();
                
            if (config != null)
            {
                Initialize(config);
            }
            else
            {
                // Initialisation d'urgence sans config
                LoadHighScore();
                SetupEventListeners();
                ResetScore();
                isInitialized = true;
            }
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
        if (!isInitialized)
        {
            FireflyDanceLogger.LogWarning($"Impossible d'ajouter des points - ScoreManager pas initialisé", this);
            return;
        }

        // ➖ Empêcher l'ajout de points après la fin du jeu
        if (!isGameActive)
        {
            FireflyDanceLogger.LogWarning($"Tentative d'ajout de points après la fin du jeu - Ignoré");
            return;
        }

        currentScore += points;
        
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
        /// Gère la capture d'une luciole 
        /// </summary>
        private void OnFireflyCaptured(FireflyController firefly, int points)
        {

            if (!isGameActive)
            {
                return;
            }

            firefliesCaptured++;    
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

    /// <summary>
    /// Gère le début du jeu
    /// </summary>
    public void OnGameStarted()
    {
        isGameActive = true;
        ResetScore();
    }

    /// <summary>
    /// Gère la fin du jeu
    /// </summary>
    public void OnGameEnded()
    {
        isGameActive = false;
        
        FireflyDanceLogger.LogScore($"Jeu terminé - Score final: {currentScore}, Lucioles capturées: {firefliesCaptured}");
    }

    #endregion
}
}
