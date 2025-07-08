using UnityEngine;
using TMPro;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.UI
{
    /// <summary>
    /// Composant UI spécialisé pour l'affichage du score dans FireflyDance
    /// Peut être utilisé indépendamment ou avec FireflyDanceUIManager
    /// </summary>
    public class FireflyDanceScoreDisplay : MonoBehaviour
    {
        #region Fields

        [Header("Score Text Components")]
        [SerializeField] private TMP_Text currentScoreText;
        
        [Header("Score Display Options")]
        [SerializeField] private bool showCurrentScore = true;
        [SerializeField] private bool animateScoreChanges = false;
        
        [Header("Formatting")]
        [SerializeField] private string scorePrefix = "Score: ";
        
        // État interne
        private int currentScore = 0;
        private bool isInitialized = false;

        #endregion

        #region Properties

        public int CurrentScore => currentScore;

        #endregion

        #region Unity Lifecycle

        void Start()
        {
            InitializeDisplay();
        }

        void OnDestroy()
        {
            CleanupEventListeners();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise l'affichage du score
        /// </summary>
        private void InitializeDisplay()
        {
            // Configuration des textes
            SetupTextComponents();
            
            // Configuration des écouteurs
            SetupEventListeners();
            
            // Affichage initial
            UpdateAllDisplays();
            
            isInitialized = true;
            FireflyDanceLogger.LogUI("ScoreDisplay initialisé");
        }

        /// <summary>
        /// Configure les composants de texte
        /// </summary>
        private void SetupTextComponents()
        {
            if (currentScoreText != null) currentScoreText.gameObject.SetActive(showCurrentScore);
        }

        /// <summary>
        /// Configure les écouteurs d'événements
        /// </summary>
        private void SetupEventListeners()
        {
            FireflyDanceEvents.OnScoreChanged += OnScoreChanged;
            FireflyDanceEvents.OnGameStarted += OnGameStarted;
            FireflyDanceEvents.OnGameEnded += OnGameEnded;
        }

        /// <summary>
        /// Nettoie les écouteurs d'événements
        /// </summary>
        private void CleanupEventListeners()
        {
            FireflyDanceEvents.OnScoreChanged -= OnScoreChanged;
            FireflyDanceEvents.OnGameStarted -= OnGameStarted;
            FireflyDanceEvents.OnGameEnded -= OnGameEnded;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Appelé quand le score change
        /// </summary>
        private void OnScoreChanged(int newScore)
        {
            currentScore = newScore;
            UpdateCurrentScoreDisplay();
            
            // Animation du score si activée
            if (animateScoreChanges)
            {
                AnimateScoreIncrease();
            }
        }

        /// <summary>
        /// Appelé quand le jeu démarre
        /// </summary>
        private void OnGameStarted()
        {
            // Réinitialisation des compteurs de session
            currentScore = 0;
            UpdateAllDisplays();
        }

        /// <summary>
        /// Appelé quand le jeu se termine
        /// </summary>
        private void OnGameEnded()
        {
            // Pas d'action spécifique pour le moment
        }

        #endregion

        #region Display Updates

        /// <summary>
        /// Met à jour l'affichage du score actuel
        /// </summary>
        private void UpdateCurrentScoreDisplay()
        {
            if (currentScoreText != null && showCurrentScore)
            {
                currentScoreText.text = $"{scorePrefix}{currentScore}";
            }
        }

        /// <summary>
        /// Met à jour tous les affichages
        /// </summary>
        private void UpdateAllDisplays()
        {
            UpdateCurrentScoreDisplay();
        }

        #endregion

        #region Animation Methods

        /// <summary>
        /// Anime l'augmentation du score
        /// </summary>
        private void AnimateScoreIncrease()
        {
            if (currentScoreText != null)
            {
                // Animation simple sans LeanTween
                StartCoroutine(AnimateScaleCoroutine(currentScoreText.transform));
            }
        }

        /// <summary>
        /// Coroutine d'animation d'échelle
        /// </summary>
        private System.Collections.IEnumerator AnimateScaleCoroutine(Transform target)
        {
            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * 1.1f;
            
            float duration = 0.2f;
            float elapsed = 0f;
            
            // Animation d'agrandissement
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                target.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // Animation de retour
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                target.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            target.localScale = originalScale;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Force la mise à jour de l'affichage avec des valeurs spécifiques
        /// </summary>
        public void ForceUpdateDisplay(int score)
        {
            currentScore = score;
            UpdateAllDisplays();
        }

        /// <summary>
        /// Réinitialise l'affichage
        /// </summary>
        public void ResetDisplay()
        {
            currentScore = 0;
            UpdateAllDisplays();
        }

        /// <summary>
        /// Configure la visibilité des éléments
        /// </summary>
        public void SetDisplayVisibility(bool showScore)
        {
            showCurrentScore = showScore;
            SetupTextComponents();
            UpdateAllDisplays();
        }

        /// <summary>
        /// Active ou désactive les animations
        /// </summary>
        public void SetAnimationEnabled(bool enabled)
        {
            animateScoreChanges = enabled;
        }

        #endregion
    }
}
