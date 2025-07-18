using UnityEngine;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Core;
using Gameplay.LogParade.Systems;
using Gameplay.LogParade.Score;

namespace Gameplay.LogParade.Tests.Debug
{
    /// <summary>
    /// Exemple d'intégration des différents systèmes LogParade
    /// Montre l'utilisation des événements centralisés et du validateur système
    /// </summary>
    public class LogParadeIntegrationExample : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDetailedLogs = true;

        private LogParadeSystemValidator systemValidator;
        private LogParadeEventCoordinator eventCoordinator;

        void Start()
        {
            // S'abonner aux événements centralisés
            SubscribeToEvents();
            
            // Exemple d'utilisation du SystemValidator
            ValidateSystemExample();
        }

        void OnDestroy()
        {
            // Se désabonner des événements
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventCoordinator = LogParadeEventCoordinator.Instance;
            if (eventCoordinator != null)
            {
                eventCoordinator.OnCalibrationStarted += () => LogMessage("🎯 Calibration démarrée");
                eventCoordinator.OnCalibrationCompleted += () => LogMessage("✅ Calibration terminée");
                eventCoordinator.OnCalibrationFailed += () => LogMessage("❌ Calibration échouée");
                eventCoordinator.OnGameStarted += () => LogMessage("🎮 Jeu démarré");
                eventCoordinator.OnGameEnded += () => LogMessage("🏁 Jeu terminé");
                eventCoordinator.OnScoreChanged += (score) => LogMessage($"📊 Score: {score}");
                LogMessage("✅ Abonné aux événements");
            }
            else
            {
                LogMessage("⚠️ EventCoordinator non disponible");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (eventCoordinator != null)
            {
                eventCoordinator.OnCalibrationStarted -= () => LogMessage("🎯 Calibration démarrée");
                eventCoordinator.OnCalibrationCompleted -= () => LogMessage("✅ Calibration terminée");
                eventCoordinator.OnCalibrationFailed -= () => LogMessage("❌ Calibration échouée");
                eventCoordinator.OnGameStarted -= () => LogMessage("🎮 Jeu démarré");
                eventCoordinator.OnGameEnded -= () => LogMessage("🏁 Jeu terminé");
                eventCoordinator.OnScoreChanged -= (score) => LogMessage($"📊 Score: {score}");
            }
        }

        private void ValidateSystemExample()
        {
            systemValidator = LogParadeSystemValidator.Instance;
            if (systemValidator != null)
            {
                // Forcer une validation
                systemValidator.ForceValidation();
                string report = systemValidator.GenerateSystemReport();
                LogMessage($"🔍 Validation système: {report}");
                
                // Obtenir des composants spécifiques
                var gameController = systemValidator.GetValidatedComponent<LogParadeGameController>();
                var scoreManager = systemValidator.GetValidatedComponent<LogParadeScoreManager>();
                
                LogMessage($"📋 Composants trouvés: GameController={gameController != null}, ScoreManager={scoreManager != null}");
            }
            else
            {
                LogMessage("⚠️ SystemValidator non disponible");
            }
        }

        [ContextMenu("Test System Validation")]
        public void TestSystemValidation()
        {
            systemValidator = LogParadeSystemValidator.Instance;
            if (systemValidator != null)
            {
                LogMessage("🔍 Test de validation système");
                
                // Afficher le rapport complet
                string report = systemValidator.GenerateSystemReport();
                LogParadeLogger.Log(report);
                
                // Vider le cache et re-valider
                systemValidator.ClearComponentCache();
                systemValidator.ForceValidation();
                
                LogMessage($"🔍 Validation après vidage cache terminée");
            }
            else
            {
                LogMessage("❌ SystemValidator non disponible pour le test");
            }
        }

        private void LogMessage(string message)
        {
            if (enableDetailedLogs)
                LogParadeLogger.Log($"[IntegrationExample] {message}");
        }
    }
}