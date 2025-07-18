using UnityEngine;
using System.Collections.Generic;
using Gameplay.LogParade.Core;
using Gameplay.LogParade.Score;
using Gameplay.LogParade.Player;
using Gameplay.LogParade.Logs;
using Gameplay.LogParade.UI;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Calibration;

namespace Gameplay.LogParade.Systems
{
    /// <summary>
    /// Valide et gère les dépendances entre les composants du système LogParade
    /// </summary>
public class LogParadeSystemValidator : MonoBehaviour
{
    [SerializeField] private LogParadeGameLauncher gameLauncher;
        private static LogParadeSystemValidator _instance;
        public static LogParadeSystemValidator Instance => _instance;

        [Header("Required Components")]
        [SerializeField] private LogParadeGameController gameController;
        [SerializeField] private LogParadeGameTimer gameTimer;
        [SerializeField] private LogParadeLogGenerator logGenerator;
        [SerializeField] private LogParadeScoreManager scoreManager;
        [SerializeField] private LogParadeCalibrationManager calibrationManager;

        [Header("Optional Components")]
        [SerializeField] private LogParadeUIManager uiManager;
        [SerializeField] private LogParadePlayerAvatar playerAvatar;
        [SerializeField] private LogParadeGameStateController gameStateController;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                ValidateAllComponents();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void ValidateAllComponents()
        {
            // Valider les composants requis
            ValidateRequiredComponent(ref gameController, "GameController");
            ValidateRequiredComponent(ref gameTimer, "GameTimer");
            ValidateRequiredComponent(ref logGenerator, "LogGenerator");
            ValidateRequiredComponent(ref scoreManager, "ScoreManager");
            ValidateRequiredComponent(ref calibrationManager, "CalibrationManager");

            // Valider les composants optionnels
            ValidateOptionalComponent(ref uiManager, "UIManager");
            ValidateOptionalComponent(ref playerAvatar, "PlayerAvatar");
            ValidateOptionalComponent(ref gameStateController, "GameStateController");
        }

        private void ValidateRequiredComponent<T>(ref T component, string componentName) where T : Component
        {
            if (component == null)
            {
                component = FindFirstObjectByType<T>();
                if (component == null)
                {
                    LogParadeLogger.LogError($"{componentName} manquant !");
                }
            }
        }

        private void ValidateOptionalComponent<T>(ref T component, string componentName) where T : Component
        {
            if (component == null)
            {
                component = FindFirstObjectByType<T>();
                if (component == null)
                {
                    LogParadeLogger.LogVerbose($"{componentName} non trouvé (optionnel)");
                }
            }
        }

        public T GetValidatedComponent<T>() where T : Component
        {
            if (typeof(T) == typeof(LogParadeGameController))
                return gameController as T;
            if (typeof(T) == typeof(LogParadeGameTimer))
                return gameTimer as T;
            if (typeof(T) == typeof(LogParadeLogGenerator))
                return logGenerator as T;
            if (typeof(T) == typeof(LogParadeScoreManager))
                return scoreManager as T;
            if (typeof(T) == typeof(LogParadeCalibrationManager))
                return calibrationManager as T;
            if (typeof(T) == typeof(LogParadeUIManager))
                return uiManager as T;
            if (typeof(T) == typeof(LogParadePlayerAvatar))
                return playerAvatar as T;
            if (typeof(T) == typeof(LogParadeGameStateController))
                return gameStateController as T;

            return null;
        }

        public void ForceValidation()
        {
            ValidateAllComponents();
            LogParadeLogger.Log("Validation forcée des composants effectuée");
        }

        public string GenerateSystemReport()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("=== RAPPORT SYSTÈME LOGPARADE ===");
            report.AppendLine($"GameController: {(gameController != null ? "✅" : "❌")}");
            report.AppendLine($"GameTimer: {(gameTimer != null ? "✅" : "❌")}");
            report.AppendLine($"LogGenerator: {(logGenerator != null ? "✅" : "❌")}");
            report.AppendLine($"ScoreManager: {(scoreManager != null ? "✅" : "❌")}");
            report.AppendLine($"CalibrationManager: {(calibrationManager != null ? "✅" : "❌")}");
            report.AppendLine($"UIManager: {(uiManager != null ? "✅" : "❌")} (optionnel)");
            report.AppendLine($"PlayerAvatar: {(playerAvatar != null ? "✅" : "❌")} (optionnel)");
            report.AppendLine($"GameLauncher: {(gameLauncher != null ? "✅" : "❌")} (optionnel)");
            report.AppendLine($"GameStateController: {(gameStateController != null ? "✅" : "❌")} (optionnel)");
            return report.ToString();
        }

        public void ClearComponentCache()
        {
            gameController = null;
            gameTimer = null;
            logGenerator = null;
            scoreManager = null;
            calibrationManager = null;
            uiManager = null;
            playerAvatar = null;
            gameLauncher = null;
            gameStateController = null;
            LogParadeLogger.Log("Cache des composants effacé");
        }
    }
}