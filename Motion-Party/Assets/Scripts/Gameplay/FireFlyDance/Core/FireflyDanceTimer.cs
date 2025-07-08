using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Gère le timer du mini-jeu Danse des Lucioles
    /// Contrôle la durée de la partie et déclenche les événements associés
    /// Système inspiré de LogParadeGameTimer avec intégration FireflyDance
    /// </summary>
    public class FireflyDanceTimer : MonoBehaviour
    {
        #region Champs de configuration
        
        [Header("Configuration du Timer")]
        [Tooltip("Configuration scriptable contenant la durée de la partie")]
        [SerializeField] private FireflyDanceConfig config;
        
        [Tooltip("Durée de la partie en secondes (si pas de config)")]
        [SerializeField] private float fallbackDurationInSeconds = 60f;
        
        [Tooltip("Délai avant le démarrage du timer (en secondes)")]
        [SerializeField] private float startDelay = 2f;
        
        [Header("Composants Optionnels")]
        [Tooltip("Gestionnaire principal du jeu (optionnel)")]
        [SerializeField] private FireflyDanceGameManager gameManager;
        
        [Header("Événements Unity")]
        [Tooltip("Événement déclenché au début du timer")]
        public UnityEvent OnTimerStarted = new UnityEvent();
        
        [Tooltip("Événement déclenché à chaque seconde (avec temps restant)")]
        public UnityEvent<float> OnTimerTick = new UnityEvent<float>();
        
        [Tooltip("Événement déclenché quand le timer se termine")]
        public UnityEvent OnTimerCompleted = new UnityEvent();
        
        [Tooltip("Événement déclenché quand le timer est mis en pause")]
        public UnityEvent<bool> OnTimerPaused = new UnityEvent<bool>();
        
        [Header("Debug")]
        [Tooltip("Afficher les logs de debug dans la console")]
        [SerializeField] private bool enableDebugLogs = true;
        
        #endregion
        
        #region État privé
        
        // État du timer
        private bool timerStarted = false;
        private bool timerPaused = false;
        private bool timerCompleted = false;
        private float timeRemaining = 0f;
        
        // Coroutine du timer principal
        private Coroutine timerCoroutine;
        
        // Durée configurée (depuis config ou fallback)
        private float configuredDuration = 0f;
        
        #endregion
        
        #region Propriétés publiques
        
        /// <summary>
        /// Indique si le timer est actuellement actif (démarré et non terminé)
        /// </summary>
        public bool IsTimerActive => timerStarted && !timerCompleted;
        
        /// <summary>
        /// Indique si le timer est en pause
        /// </summary>
        public bool IsTimerPaused => timerPaused;
        
        /// <summary>
        /// Temps restant en secondes
        /// </summary>
        public float TimeRemaining => timeRemaining;
        
        /// <summary>
        /// Temps écoulé en secondes
        /// </summary>
        public float TimeElapsed => configuredDuration - timeRemaining;
        
        /// <summary>
        /// Durée totale configurée du timer
        /// </summary>
        public float TotalDuration => configuredDuration;
        
        /// <summary>
        /// Progression du timer (0.0 à 1.0)
        /// </summary>
        public float Progress => configuredDuration > 0 ? Mathf.Clamp01(TimeElapsed / configuredDuration) : 0f;
        
        #endregion
        
        #region Unity Lifecycle
        
        void Awake()
        {
            InitializeTimer();
        }
        
        void Start()
        {
            // Souscrire aux événements FireflyDance si disponibles
            SubscribeToEvents();
            
            // Validation des composants
            ValidateComponents();
        }
        
        void OnDestroy()
        {
            // Se désabonner des événements
            UnsubscribeFromEvents();
            
            // Nettoyer les coroutines
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
        }
        
        #endregion
        
        #region Initialisation
        
        /// <summary>
        /// Initialise le système de timer
        /// </summary>
        private void InitializeTimer()
        {
            // Déterminer la durée à utiliser
            if (config != null && config.GameDuration > 0)
            {
                configuredDuration = config.GameDuration;
                if (enableDebugLogs)
                    FireflyDanceLogger.Log($"Durée configurée depuis config: {configuredDuration}s");
            }
            else
            {
                configuredDuration = fallbackDurationInSeconds;
                if (enableDebugLogs)
                    FireflyDanceLogger.Log($"Durée fallback utilisée: {configuredDuration}s");
            }
            
            // Reset de l'état
            ResetTimer();
        }
        
        /// <summary>
        /// Validation des composants optionnels
        /// </summary>
        private void ValidateComponents()
        {
            // Recherche automatique du GameManager si non assigné
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<FireflyDanceGameManager>();
            }
        }
        
        /// <summary>
        /// S'abonne aux événements du système FireflyDance
        /// </summary>
        private void SubscribeToEvents()
        {
            // Écouter les événements de jeu pour gérer le timer automatiquement
            FireflyDanceEvents.OnGameStarted += OnGameStartedEvent;
            FireflyDanceEvents.OnGameEnded += OnGameEndedEvent;
            FireflyDanceEvents.OnGamePaused += OnGamePausedEvent;
        }
        
        /// <summary>
        /// Se désabonne des événements du système FireflyDance
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            FireflyDanceEvents.OnGameStarted -= OnGameStartedEvent;
            FireflyDanceEvents.OnGameEnded -= OnGameEndedEvent;
            FireflyDanceEvents.OnGamePaused -= OnGamePausedEvent;
        }
        
        #endregion
        
        #region Méthodes publiques de contrôle
        
        /// <summary>
        /// Démarre le timer
        /// </summary>
        public void StartTimer()
        {
            if (timerStarted && !timerCompleted)
            {
                if (enableDebugLogs)
                    FireflyDanceLogger.LogWarning("Timer déjà démarré !");
                return;
            }
            
            if (configuredDuration <= 0)
            {
                FireflyDanceLogger.LogError("Durée invalide pour démarrer le timer !");
                return;
            }
            
            StartCoroutine(StartTimerAfterDelay());
        }
        
        /// <summary>
        /// Met en pause ou reprend le timer
        /// </summary>
        /// <param name="paused">True pour mettre en pause, false pour reprendre</param>
        public void PauseTimer(bool paused)
        {
            if (!timerStarted || timerCompleted)
            {
                if (enableDebugLogs)
                    FireflyDanceLogger.LogWarning("Impossible de mettre en pause un timer non actif");
                return;
            }
            
            if (timerPaused == paused)
                return; // Pas de changement d'état
            
            timerPaused = paused;
            
            if (enableDebugLogs)
                FireflyDanceLogger.Log($"Timer {(paused ? "mis en pause" : "repris")}");
            
            // Déclencher uniquement l'événement Unity (éviter la double émission)
            OnTimerPaused?.Invoke(paused);
        }
        
        /// <summary>
        /// Reprend le timer s'il était en pause
        /// </summary>
        public void ResumeTimer()
        {
            PauseTimer(false);
        }
        
        /// <summary>
        /// Remet le timer à zéro
        /// </summary>
        public void ResetTimer()
        {
            // Arrêter la coroutine en cours
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            
            // Reset de l'état
            timerStarted = false;
            timerPaused = false;
            timerCompleted = false;
            timeRemaining = configuredDuration;
            
            if (enableDebugLogs)
                FireflyDanceLogger.Log($"Timer réinitialisé à {configuredDuration}s");
        }
        
        /// <summary>
        /// Force l'arrêt du timer
        /// </summary>
        public void StopTimer()
        {
            if (!timerStarted || timerCompleted)
                return;
            
            // Compléter le timer manuellement
            CompleteTimer();
        }
        
        #endregion
        
        #region Coroutines et logique du timer
        
        /// <summary>
        /// Démarre le timer après le délai configuré
        /// </summary>
        private IEnumerator StartTimerAfterDelay()
        {
            if (startDelay > 0)
            {
                if (enableDebugLogs)
                    FireflyDanceLogger.Log($"Démarrage dans {startDelay}s...");
                
                yield return new WaitForSeconds(startDelay);
            }
            
            InternalStartTimer();
        }
        
        /// <summary>
        /// Démarre effectivement le timer
        /// </summary>
        private void InternalStartTimer()
        {
            if (timerStarted)
                return;
            
            timerStarted = true;
            timerPaused = false;
            timerCompleted = false;
            timeRemaining = configuredDuration;
            
            if (enableDebugLogs)
                FireflyDanceLogger.Log($"Timer démarré pour {configuredDuration}s");
            
            // Déclencher l'événement de démarrage
            OnTimerStarted?.Invoke();
            
            // Déclencher aussi l'événement statique pour les composants qui l'écoutent
            FireflyDanceEvents.OnTimerTick?.Invoke(timeRemaining);
            
            // Démarrer la boucle de timer
            timerCoroutine = StartCoroutine(TimerLoop());
        }
        
        /// <summary>
        /// Boucle principale du timer
        /// </summary>
        private IEnumerator TimerLoop()
        {
            while (timeRemaining > 0f && !timerCompleted)
            {
                // Attendre une seconde (ou pause si nécessaire)
                yield return new WaitForSeconds(1f);
                
                // Skip si en pause
                if (timerPaused)
                    continue;
                
                // Décrémenter le temps
                timeRemaining = Mathf.Max(0f, timeRemaining - 1f);
                
                // Déclencher l'événement de tick local
                OnTimerTick?.Invoke(timeRemaining);
                
                // Déclencher aussi l'événement statique pour les composants qui l'écoutent
                FireflyDanceEvents.OnTimerTick?.Invoke(timeRemaining);
            }
            
            // Temps écoulé
            CompleteTimer();
        }
        
        /// <summary>
        /// Termine le timer
        /// </summary>
        private void CompleteTimer()
        {
            if (timerCompleted)
                return;
            
            timerCompleted = true;
            timeRemaining = 0f;
            
            // Nettoyer la coroutine
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            
            if (enableDebugLogs)
                FireflyDanceLogger.Log("Timer terminé !");
            
            // Déclencher l'événement de fin
            OnTimerCompleted?.Invoke();
            
            // Notifier le système d'événements
            FireflyDanceEvents.OnTimerCompleted?.Invoke();
        }
        
        #endregion
        
        #region Gestionnaires d'événements
        
        /// <summary>
        /// Gestionnaire pour l'événement OnGameStarted
        /// </summary>
        private void OnGameStartedEvent()
        {
            if (enableDebugLogs)
                FireflyDanceLogger.Log("Jeu démarré - démarrage automatique du timer");
            
            StartTimer();
        }
        
        /// <summary>
        /// Gestionnaire pour l'événement OnGameEnded
        /// </summary>
        private void OnGameEndedEvent()
        {
            if (enableDebugLogs)
                FireflyDanceLogger.Log("Jeu terminé - arrêt du timer");
            
            StopTimer();
        }
        
        /// <summary>
        /// Gestionnaire pour l'événement OnGamePaused
        /// </summary>
        private void OnGamePausedEvent(bool paused)
        {
            if (enableDebugLogs)
                FireflyDanceLogger.Log($"Jeu {(paused ? "mis en pause" : "repris")} - synchronisation du timer");
            
            PauseTimer(paused);
        }
        
        #endregion

    }
}
