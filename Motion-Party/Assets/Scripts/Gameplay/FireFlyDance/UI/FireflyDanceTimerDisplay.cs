using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;
using System.Collections;

namespace Gameplay.FireFlyDance.UI
{
    /// <summary>
    /// Composant UI spécialisé pour l'affichage du timer dans FireflyDance
    /// Peut être utilisé indépendamment ou avec FireflyDanceUIManager
    /// </summary>
    public class FireflyDanceTimerDisplay : MonoBehaviour
    {
        #region Fields

        [Header("Timer Text Components")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text timerLabelText;
        
        [Header("Timer Display Options")]
        [SerializeField] private bool showTimerText = true;
        [SerializeField] private bool showTimerLabel = true;
        
        [Header("Formatting")]
        [SerializeField] private string timerLabelPrefix = "Temps: ";
        [SerializeField] private bool useMinutesSeconds = true;
        [SerializeField] private float warningThreshold = 0.5f; // 50% du temps
        
        // Configuration
        private FireflyDanceConfig config;
        private float totalGameTime = 60f;
        private float currentTime = 0f;
        private bool isInitialized = false;
        private bool isGameActive = false;
        private Coroutine pulseCoroutine;

        #endregion

        #region Properties

        public float CurrentTime => currentTime;
        public float TotalGameTime => totalGameTime;
        public float TimeRemaining => Mathf.Max(0, currentTime);
        public float TimeProgress => totalGameTime > 0 ? (totalGameTime - currentTime) / totalGameTime : 0f;

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
        /// Initialise l'affichage du timer
        /// </summary>
        private void InitializeDisplay()
        {
            // Auto-détection de la configuration
            if (config == null)
            {
                config = FindObjectOfType<FireflyDanceConfig>();
            }
            
            if (config != null)
            {
                totalGameTime = config.GameDuration;
            }
            
            // Configuration des composants
            SetupComponents();
            
            // Configuration des écouteurs
            SetupEventListeners();
            
            // Affichage initial
            currentTime = totalGameTime;
            UpdateTimerDisplay();
            
            isInitialized = true;
            FireflyDanceLogger.LogUI("TimerDisplay initialisé");
        }

        /// <summary>
        /// Configure les composants UI
        /// </summary>
        private void SetupComponents()
        {
            if (timerText != null) timerText.gameObject.SetActive(showTimerText);
            if (timerLabelText != null) timerLabelText.gameObject.SetActive(showTimerLabel);
            
            // Configuration du label
            if (timerLabelText != null)
            {
                timerLabelText.text = timerLabelPrefix;
            }
        }

        /// <summary>
        /// Configure les écouteurs d'événements
        /// </summary>
        private void SetupEventListeners()
        {
            FireflyDanceEvents.OnGameStarted += OnGameStarted;
            FireflyDanceEvents.OnGameEnded += OnGameEnded;
            FireflyDanceEvents.OnGamePaused += OnGamePaused;
            FireflyDanceEvents.OnTimerCompleted += OnTimerCompleted;
            FireflyDanceEvents.OnTimerTick += OnTimerTick;
        }

        /// <summary>
        /// Nettoie les écouteurs d'événements
        /// </summary>
        private void CleanupEventListeners()
        {
            FireflyDanceEvents.OnGameStarted -= OnGameStarted;
            FireflyDanceEvents.OnGameEnded -= OnGameEnded;
            FireflyDanceEvents.OnGamePaused -= OnGamePaused;
            FireflyDanceEvents.OnTimerCompleted -= OnTimerCompleted;
            FireflyDanceEvents.OnTimerTick -= OnTimerTick;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Appelé quand le timer se met à jour
        /// </summary>
        private void OnTimerTick(float timeRemaining)
        {
            currentTime = timeRemaining;
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Appelé quand le jeu démarre
        /// </summary>
        private void OnGameStarted()
        {
            isGameActive = true;
            currentTime = totalGameTime;
            UpdateTimerDisplay();
            StopPulseAnimation();
            
            FireflyDanceLogger.LogUI("Timer démarré");
        }

        /// <summary>
        /// Appelé quand le jeu se termine
        /// </summary>
        private void OnGameEnded()
        {
            isGameActive = false;
            StopPulseAnimation();
            
            FireflyDanceLogger.LogUI("Timer arrêté");
        }

        /// <summary>
        /// Appelé quand le jeu est mis en pause
        /// </summary>
        private void OnGamePaused(bool isPaused)
        {
            isGameActive = !isPaused;
            
            FireflyDanceLogger.LogUI($"Timer pause: {isPaused}");
        }

        /// <summary>
        /// Appelé quand le timer se termine
        /// </summary>
        private void OnTimerCompleted()
        {
            currentTime = 0f;
            isGameActive = false;
            UpdateTimerDisplay();
            
            // Animation de fin de temps
            AnimateTimeUp();
        }

        #endregion

        #region Display Updates

        /// <summary>
        /// Met à jour l'affichage du timer
        /// </summary>
        private void UpdateTimerDisplay()
        {
            // Mise à jour du texte
            UpdateTimerText();
            
            // Vérifier si on doit déclencher l'animation de pulsation
            if (isGameActive && currentTime > 0)
            {
                float criticalTime = totalGameTime * warningThreshold;
                if (currentTime <= criticalTime && pulseCoroutine == null)
                {
                    StartPulseAnimation();
                }
                else if (currentTime > criticalTime && pulseCoroutine != null)
                {
                    StopPulseAnimation();
                }
            }
        }

        /// <summary>
        /// Met à jour le texte du timer
        /// </summary>
        private void UpdateTimerText()
        {
            if (timerText != null && showTimerText)
            {
                if (useMinutesSeconds)
                {
                    int minutes = Mathf.FloorToInt(currentTime / 60f);
                    int seconds = Mathf.FloorToInt(currentTime % 60f);
                    timerText.text = $"{minutes:00}:{seconds:00}";
                }
                else
                {
                    timerText.text = $"{currentTime:F1}s";
                }
            }
        }

        #endregion

        #region Animation Methods

        /// <summary>
        /// Démarre l'animation de pulsation
        /// </summary>
        private void StartPulseAnimation()
        {
            if (pulseCoroutine != null) return;
            
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }

        /// <summary>
        /// Arrête l'animation de pulsation
        /// </summary>
        private void StopPulseAnimation()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            
            // Réinitialisation de l'échelle
            if (timerText != null)
            {
                timerText.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Coroutine d'animation de pulsation
        /// </summary>
        private IEnumerator PulseAnimation()
        {
            while (pulseCoroutine != null)
            {
                if (timerText != null)
                {
                    // Animation d'agrandissement
                    yield return StartCoroutine(AnimateScale(timerText.transform, Vector3.one * 1.2f, 0.3f));
                    
                    // Animation de retour
                    yield return StartCoroutine(AnimateScale(timerText.transform, Vector3.one, 0.3f));
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Animation de fin de temps
        /// </summary>
        private void AnimateTimeUp()
        {
            if (timerText != null)
            {
                timerText.text = "00:00";
                
                // Arrêter l'animation de pulsation et faire une animation de fin
                StopPulseAnimation();
                
                // Animation de fin plus visible
                StartCoroutine(TimeUpAnimation());
            }
        }
        
        /// <summary>
        /// Animation spécifique de fin de temps
        /// </summary>
        private IEnumerator TimeUpAnimation()
        {
            if (timerText != null)
            {
                // Changer la couleur en rouge si possible
                var originalColor = timerText.color;
                timerText.color = Color.red;
                
                // Animation de clignotement
                for (int i = 0; i < 3; i++)
                {
                    yield return StartCoroutine(AnimateScale(timerText.transform, Vector3.one * 1.5f, 0.2f));
                    yield return StartCoroutine(AnimateScale(timerText.transform, Vector3.one, 0.2f));
                }
                
                // Restaurer la couleur originale
                timerText.color = originalColor;
            }
        }

        /// <summary>
        /// Anime l'échelle d'un transform
        /// </summary>
        private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration)
        {
            if (target == null) yield break;
            
            Vector3 startScale = target.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            target.localScale = targetScale;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Force la mise à jour du temps
        /// </summary>
        public void ForceUpdateTime(float newTime)
        {
            currentTime = Mathf.Max(0, newTime);
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Réinitialise le timer
        /// </summary>
        public void ResetTimer()
        {
            currentTime = totalGameTime;
            isGameActive = false;
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Configure la durée totale du jeu
        /// </summary>
        public void SetTotalGameTime(float newTotalTime)
        {
            totalGameTime = newTotalTime;
            currentTime = totalGameTime;
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Configure la visibilité des éléments
        /// </summary>
        public void SetDisplayVisibility(bool showText, bool showLabel)
        {
            showTimerText = showText;
            showTimerLabel = showLabel;
            
            SetupComponents();
        }

        #endregion
    }
}
