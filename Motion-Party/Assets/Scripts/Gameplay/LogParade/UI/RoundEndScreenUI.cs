using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.UI
{
    /// <summary>
    /// Interface d'affichage des résultats de fin de round pour LogParade
    /// </summary>
    public class RoundEndScreenUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Button nextPlayerButton;
        [SerializeField] private Button finishButton;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (enableDebugLogs)
            {
                LogParadeLogger.LogVerbose("Initialisation de l'UI de fin de round");
            }

            if (nextPlayerButton != null)
                nextPlayerButton.gameObject.SetActive(false);
            if (finishButton != null)
                finishButton.gameObject.SetActive(false);
        }

        public void DisplayResults(string playerName, float score, float duration, bool hasNextPlayer)
        {
            if (playerNameText != null)
                playerNameText.text = playerName;

            if (scoreText != null)
                scoreText.text = $"Score: {score:F0}";

            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(duration / 60);
                int seconds = Mathf.FloorToInt(duration % 60);
                timeText.text = $"Temps: {minutes:00}:{seconds:00}";
            }

            // Afficher le bon bouton selon s'il y a un joueur suivant
            if (nextPlayerButton != null)
                nextPlayerButton.gameObject.SetActive(hasNextPlayer);
            if (finishButton != null)
                finishButton.gameObject.SetActive(!hasNextPlayer);

            if (enableDebugLogs)
            {
                LogParadeLogger.LogVerbose($"Résultats affichés pour {playerName} - Score: {score:F0}, Durée: {duration:F1}s");
            }
        }
    }
} 