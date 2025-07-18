using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Core;

namespace Gameplay.LogParade.Tests.Debug
{
    /// <summary>
    /// Panel de debug pour afficher et contrôler l'état du jeu LogParade
    /// </summary>
    public class DebugPanel_Status : BaseDebugPanel
    {
        private LogParadeGameStateController gameStateController;
        private bool isInitialized = false;

        protected override void Start()
        {
            base.Start();
            InitializePanel();
        }

        private void InitializePanel()
        {
            gameStateController = LogParadeGameStateController.Instance;
            if (gameStateController == null)
            {
                LogParadeLogger.LogError("GameStateController non trouvé!");
                return;
            }

            isInitialized = true;
            LogParadeLogger.LogVerbose("Panel de statut initialisé");
        }

        protected override void DrawPanelContent()
        {
            if (!isInitialized) return;

            // État actuel
            GUILayout.Label($"État actuel: {LogParadeGameStateController.GetCurrentStatusText()}");
            GUILayout.Label($"Calibration en cours: {LogParadeGameStateController.IsCalibrationInProgress}");
            GUILayout.Label($"Gameplay autorisé: {LogParadeGameStateController.IsGameplayAllowed}");
            GUILayout.Label($"Jeu démarré: {LogParadeGameStateController.IsGameStarted}");

            // Actions
            if (GUILayout.Button("Démarrer Calibration"))
            {
                LogParadeGameStateController.StartCalibration();
            }

            if (GUILayout.Button("Terminer Calibration"))
            {
                LogParadeGameStateController.CompleteCalibration();
            }

            if (GUILayout.Button("Redémarrer Calibration"))
            {
                LogParadeGameStateController.RestartCalibration();
            }

            if (GUILayout.Button("Démarrer Jeu"))
            {
                LogParadeGameStateController.StartGame();
            }

            if (GUILayout.Button("Arrêter Jeu"))
            {
                LogParadeGameStateController.StopGame();
            }

            if (GUILayout.Button("Reset État"))
            {
                LogParadeGameStateController.ResetGameState();
            }

            #if UNITY_EDITOR
            if (GUILayout.Button("Forcer Gameplay"))
            {
                LogParadeGameStateController.ForceEnableGameplay();
            }
            #endif
        }
    }
}
