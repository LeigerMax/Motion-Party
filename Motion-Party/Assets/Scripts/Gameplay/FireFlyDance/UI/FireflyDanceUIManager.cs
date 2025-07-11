using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.UI
{
    /// <summary>
    /// Gestionnaire d'interface utilisateur pour le mini-jeu "Danse des Lucioles".
    /// Orchestre les différents modules d'affichage UI et fournit une interface unifiée.
    /// Suit l'architecture modulaire établie dans LogParade.
    /// </summary>
    public class FireflyDanceUIManager : MonoBehaviour
    {
        #region Dependencies & References
        
        [Header("Score UI")]
        [SerializeField] private TMP_Text currentScoreText;
        
        [Header("Timer UI")]
        [SerializeField] private TMP_Text timerText;
        
        [Header("Game Status UI")]
        [SerializeField] private TMP_Text gameStatusText;
        
        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        [SerializeField] private bool hideUIOnStart = false;
        
        [Header("UI Panels Control")]
        [SerializeField] private bool showScorePanel = true;
        [SerializeField] private bool showTimerPanel = true;
        [SerializeField] private bool showStatusPanel = true;
        
        #endregion

        #region Properties
        
        public bool IsInitialized { get; private set; }
        
        #endregion

        #region Unity Lifecycle
        
        void Start()
        {
            InitializeUI();
        }

        void OnDestroy()
        {
            CleanupEventListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise l'interface utilisateur
        /// </summary>
        private void InitializeUI()
        {
            // Auto-détection de la config si pas assignée
            if (config == null)
            {
                config = FindFirstObjectByType<FireflyDanceConfig>();
                if (config == null)
                {
                    FireflyDanceLogger.LogError("FireflyDanceConfig introuvable ! Certaines fonctionnalités UI pourraient ne pas fonctionner.");
                }
            }

            // Configuration des panneaux
            SetupPanels();
            
            // Initialisation des valeurs par défaut
            InitializeDefaultValues();
            
            // Configuration des écouteurs d'événements
            SetupEventListeners();
            
            IsInitialized = true;
            FireflyDanceLogger.LogUI("UIManager initialisé avec succès");
        }

        /// <summary>
        /// Configure la visibilité des panneaux selon les préférences
        /// </summary>
        private void SetupPanels()
        {
            // Panneaux principaux
            if (currentScoreText != null) currentScoreText.gameObject.SetActive(showScorePanel);
            if (timerText != null) timerText.gameObject.SetActive(showTimerPanel);
            if (gameStatusText != null) gameStatusText.gameObject.SetActive(showStatusPanel);
        }

        /// <summary>
        /// Initialise les valeurs par défaut des éléments UI
        /// </summary>
        private void InitializeDefaultValues()
        {
            // Score
            UpdateScoreDisplay(0);
            
            // Timer
            UpdateTimerDisplay(config != null ? config.GameDuration : 60f);
            
            // Status
            UpdateGameStatusDisplay("En attente...");
        }

        /// <summary>
        /// Configure les écouteurs d'événements
        /// </summary>
        private void SetupEventListeners()
        {
            // Événements de score
            FireflyDanceEvents.OnScoreChanged += UpdateScoreDisplay;
            
            // Événements de jeu
            FireflyDanceEvents.OnGameStarted += OnGameStarted;
            FireflyDanceEvents.OnGameEnded += OnGameEnded;
            FireflyDanceEvents.OnGamePaused += OnGamePaused;
            FireflyDanceEvents.OnGameStateChanged += OnGameStateChanged;
            
            // Événements de timer
            FireflyDanceEvents.OnTimerCompleted += OnTimerCompleted;
            FireflyDanceEvents.OnTimerTick += UpdateTimerDisplay;
        }

        /// <summary>
        /// Nettoie les écouteurs d'événements
        /// </summary>
        private void CleanupEventListeners()
        {
            // Événements de score
            FireflyDanceEvents.OnScoreChanged -= UpdateScoreDisplay;
            
            // Événements de jeu
            FireflyDanceEvents.OnGameStarted -= OnGameStarted;
            FireflyDanceEvents.OnGameEnded -= OnGameEnded;
            FireflyDanceEvents.OnGamePaused -= OnGamePaused;
            FireflyDanceEvents.OnGameStateChanged -= OnGameStateChanged;
            
            // Événements de timer
            FireflyDanceEvents.OnTimerCompleted -= OnTimerCompleted;
            FireflyDanceEvents.OnTimerTick -= UpdateTimerDisplay;
        }

        #endregion

        #region UI Update Methods

        /// <summary>
        /// Met à jour l'affichage du score actuel
        /// </summary>
        public void UpdateScoreDisplay(int newScore)
        {
            if (currentScoreText != null)
            {
                currentScoreText.text = $"Score: {newScore}";
            }
        }

        /// <summary>
        /// Met à jour l'affichage du timer
        /// </summary>
        public void UpdateTimerDisplay(float timeRemaining)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        /// <summary>
        /// Met à jour l'affichage du statut du jeu
        /// </summary>
        public void UpdateGameStatusDisplay(string status)
        {
            if (gameStatusText != null)
            {
                gameStatusText.text = status;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Appelé quand le jeu démarre
        /// </summary>
        private void OnGameStarted()
        {
            UpdateGameStatusDisplay("Jeu en cours");
            FireflyDanceLogger.LogUI("Jeu démarré - UI mise à jour");
        }

        /// <summary>
        /// Appelé quand le jeu se termine
        /// </summary>
        private void OnGameEnded()
        {
            UpdateGameStatusDisplay("Jeu terminé");
            FireflyDanceLogger.LogUI("Jeu terminé - UI mise à jour");
        }

        /// <summary>
        /// Appelé quand le jeu est mis en pause
        /// </summary>
        private void OnGamePaused(bool isPaused)
        {
            UpdateGameStatusDisplay(isPaused ? "Jeu en pause" : "Jeu en cours");
            FireflyDanceLogger.LogUI($"Jeu {(isPaused ? "mis en pause" : "repris")} - UI mise à jour");
        }

        /// <summary>
        /// Appelé quand l'état du jeu change
        /// </summary>
        private void OnGameStateChanged(FireflyDanceGameController.GameState newState)
        {
            string statusText = newState switch
            {
                FireflyDanceGameController.GameState.Idle => "En attente",
                FireflyDanceGameController.GameState.Calibrating => "Calibration",
                FireflyDanceGameController.GameState.Playing => "Jeu en cours",
                FireflyDanceGameController.GameState.Paused => "En pause",
                FireflyDanceGameController.GameState.Finished => "Terminé",
                _ => "État inconnu"
            };
            
            UpdateGameStatusDisplay(statusText);
        }

        /// <summary>
        /// Appelé quand le timer se termine
        /// </summary>
        private void OnTimerCompleted()
        {
            UpdateTimerDisplay(0f);
            UpdateGameStatusDisplay("Temps écoulé !");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Affiche ou masque un panneau spécifique
        /// </summary>
        public void SetPanelVisibility(string panelName, bool visible)
        {
            switch (panelName.ToLower())
            {
                case "score":
                    showScorePanel = visible;
                    if (currentScoreText != null) currentScoreText.gameObject.SetActive(visible);
                    break;
                    
                case "timer":
                    showTimerPanel = visible;
                    if (timerText != null) timerText.gameObject.SetActive(visible);
                    break;
                    
                case "status":
                    showStatusPanel = visible;
                    if (gameStatusText != null) gameStatusText.gameObject.SetActive(visible);
                    break;
            }
        }

        /// <summary>
        /// Réinitialise l'interface utilisateur
        /// </summary>
        public void ResetUI()
        {
            InitializeDefaultValues();
            FireflyDanceLogger.LogUI("UI réinitialisée");
        }

        #endregion
    }
}
