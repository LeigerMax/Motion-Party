using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Core;

namespace Gameplay.LogParade.UI
{
    /// <summary>
    /// Gestionnaire centralisé de l'interface utilisateur pour LogParade
    /// </summary>
    public class LogParadeUIManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject calibrationPanel;
        [SerializeField] private Slider calibrationProgressSlider;
        [SerializeField] private TextMeshProUGUI calibrationText;

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
                LogParadeLogger.LogVerbose("Initialisation de l'UI");
            }

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (calibrationPanel != null)
                calibrationPanel.SetActive(false);
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        public void UpdateStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        public void ShowPauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void HidePauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        public void ShowCalibrationUI(float progress = 0f)
        {
            if (calibrationPanel != null)
            {
                calibrationPanel.SetActive(true);
                if (calibrationProgressSlider != null)
                {
                    calibrationProgressSlider.value = progress;
                }
                if (calibrationText != null)
                {
                    calibrationText.text = "Calibration en cours...";
                }
            }
        }

        public void HideCalibrationUI()
        {
            if (calibrationPanel != null)
            {
                calibrationPanel.SetActive(false);
            }
        }

        public void UpdateCalibrationProgress(float progress)
        {
            if (calibrationProgressSlider != null)
            {
                calibrationProgressSlider.value = progress;
            }
        }

        public void UpdateCalibrationText(string text)
        {
            if (calibrationText != null)
            {
                calibrationText.text = text;
            }
        }
    }
}