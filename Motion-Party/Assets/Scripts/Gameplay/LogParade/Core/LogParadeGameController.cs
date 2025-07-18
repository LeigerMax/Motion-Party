using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Gameplay.LogParade.Score;
using Gameplay.LogParade.Player;
using Gameplay.LogParade.Logs;
using Gameplay.LogParade.UI;
using Gameplay.LogParade.Systems;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Core;

namespace Gameplay.LogParade.Core
{
    /// <summary>
    /// Contrôleur principal du mini-jeu LogParade
    /// </summary>
    public class LogParadeGameController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private LogParadeGameManager gameManager;
        [SerializeField] private LogParadeScoreManager scoreManager;
        [SerializeField] private LogParadeGameTimer timer;
        [SerializeField] private LogParadeLogGenerator logGenerator;
        [SerializeField] private LogParadePlayerAvatar player;

        [Header("Configuration")]
        [SerializeField] private bool enableDebugMode = false;

        private void Start()
        {
            // Vérifier les références
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<LogParadeGameManager>();
                if (gameManager == null)
                {
                    Debug.LogError("[LogParadeGameController] LogParadeGameManager non trouvé!");
                }
            }

            // Initialiser les composants (mais ne pas lancer le jeu)
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }

            if (timer != null)
            {
                timer.OnTimeUp.AddListener(OnTimeUp);
            }

            if (logGenerator != null)
            {
                logGenerator.enabled = false;
            }

            if (player != null)
            {
                player.enabled = true; // Avatar actif dès le début pour permettre le mouvement
                player.ResetPosition();
                // Si le script de mouvement est sur un composant enfant, on l'active aussi
                var moveScript = player.GetComponent<MonoBehaviour>();
                if (moveScript != null)
                {
                    moveScript.enabled = true;
                }
                Debug.Log("[LogParadeGameController] Avatar activé et prêt à bouger (player.enabled = true)");
            }
        }

        /// <summary>
        /// À appeler après la calibration pour lancer le jeu (timer, score, logs, etc)
        /// </summary>
        public void LaunchGameAfterCalibration()
        {
            StartGame();
        }

        // Privée : ne pas appeler directement, passer par LaunchGameAfterCalibration
        private void StartGame()
        {
            // Réinitialiser les composants
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }

            if (timer != null)
            {
                timer.StartTimer();
            }

            if (logGenerator != null)
            {
                logGenerator.enabled = true;
                logGenerator.StartGeneration();
            }

            if (player != null)
            {
                player.enabled = true;
                player.ResetPosition();
            }

            if (enableDebugMode)
            {
                Debug.Log("[LogParadeGameController] Partie démarrée");
            }

            OnGameStarted?.Invoke();
        }

        public void EndGame()
        {
            // Arrêter les composants
            if (timer != null)
            {
                timer.StopTimer();
            }

            if (logGenerator != null)
            {
                logGenerator.enabled = false;
                logGenerator.StopGeneration();
            }

            if (player != null)
            {
                player.enabled = false;
            }

            // Notifier le GameManager
            if (gameManager != null)
            {
                float finalScore = scoreManager != null ? scoreManager.GetCurrentScore() : 0f;
                float duration = timer != null ? timer.GetElapsedTime() : 0f;
                gameManager.OnGameOver(finalScore, duration);
            }

            if (enableDebugMode)
            {
                Debug.Log($"[LogParadeGameController] Partie terminée. Score: {(scoreManager != null ? scoreManager.GetCurrentScore() : 0)}");
            }

            OnGameOver?.Invoke();
        }

        private void OnTimeUp()
        {
            EndGame();
        }

        public void OnPlayerFell()
        {
            EndGame();
        }

        private void OnDestroy()
        {
            if (timer != null)
            {
                timer.OnTimeUp.RemoveListener(OnTimeUp);
            }
        }

        // Events
        public UnityEvent OnGameStarted;
        public UnityEvent OnGameOver;
    }
}
