using UnityEngine;
using Gameplay.LogParade.UI;
using Gameplay.LogParade.Utils;
using Gameplay.LogParade.Core;

namespace Gameplay.LogParade.Tests.Debug
{
    /// <summary>
    /// Panneau de debug pour le système de lancement coordonné LogParade.
    /// Affiche l'état des systèmes et permet de contrôler le lancement du jeu.
    /// </summary>
    public class DebugPanel_GameLauncher : BaseDebugPanel
    {
    #region Champs & Références
        [SerializeField] private LogParadeGameLauncher gameLauncher;
    #endregion
        // Données en cache
        private bool isLaunching = false;
        private bool gameStarted = false;
        private bool calibrationCompleted = false;
        private int systemsFound = 0;
        private string lastLaunchStatus = "Non démarré";

    #region Initialisation
        protected override void Start()
        {
            if (gameLauncher == null)
                gameLauncher = FindFirstObjectByType<LogParadeGameLauncher>();
            visibleAtStart = true;
            SetVisible(true);
            panelTitle = "🚀 Game Launcher";
            base.Start();
        }
    #endregion

    #region Rafraîchissement
        protected override void RefreshData()
        {
            // plus de gameLauncher à rafraîchir
        }
    #endregion

    #region Affichage
        protected override void DrawPanelContent()
        {
            // plus de gameLauncher à afficher
            GUILayout.BeginVertical();
            DrawGeneralStatusSection();
            GUILayout.Space(5);
            DrawSystemsSection();
            GUILayout.Space(5);
            DrawControlsSection();
            GUILayout.EndVertical();
        }
        private void DrawGeneralStatusSection()
        {
            GUILayout.Label("<b>🎮 ÉTAT GÉNÉRAL</b>");
            string launchingStatus = isLaunching ? "<color=yellow>EN COURS</color>" : "<color=gray>ARRÊTÉ</color>";
            GUILayout.Label($"Lancement: {launchingStatus}");
            string gameStatus = gameStarted ? "<color=green>DÉMARRÉ</color>" : "<color=orange>EN ATTENTE</color>";
            GUILayout.Label($"Jeu: {gameStatus}");
            string calibrationStatus = calibrationCompleted ? "<color=green>TERMINÉE</color>" : "<color=red>REQUISE</color>";
            GUILayout.Label($"Calibration: {calibrationStatus}");
            GUILayout.Label($"Dernière action: {lastLaunchStatus}");
        }
        private void DrawSystemsSection()
        {
            GUILayout.Label("<b>🔧 SYSTÈMES</b>");
            string systemsColor = systemsFound >= 4 ? "green" : (systemsFound >= 2 ? "yellow" : "red");
            GUILayout.Label($"Détectés: <color={systemsColor}>{systemsFound}/4</color>");
            if (gameLauncher != null)
            {
                var gameController = GetSystemComponent("gameController");
                var gameTimer = GetSystemComponent("gameTimer");
                var logGenerator = GetSystemComponent("logGenerator");
                var scoreManager = GetSystemComponent("scoreManager");
                GUILayout.Label($"• Controller: {GetSystemStatusIcon(gameController)}");
                GUILayout.Label($"• Timer: {GetSystemStatusIcon(gameTimer)}");
                GUILayout.Label($"• Generator: {GetSystemStatusIcon(logGenerator)}");
                GUILayout.Label($"• Score: {GetSystemStatusIcon(scoreManager)}");
            }
        }
        private void DrawControlsSection()
        {
            GUILayout.Label("<b>🎯 CONTRÔLES</b>");
            if (!isLaunching && !gameStarted)
            {
                if (GUILayout.Button("Lancer le Jeu"))
                {
                    LaunchGame();
                }
            }
            if (gameStarted && GUILayout.Button("Relancer"))
            {
                RestartGame();
            }
            if (gameStarted && GUILayout.Button("Arrêter Tout"))
            {
                StopAllSystems();
            }
            if (GUILayout.Button("Diagnostic"))
            {
                RunDiagnostic();
            }
        }
        protected override float GetEstimatedHeight()
        {
            return gameLauncher != null ? 220f : 80f;
        }
    #endregion

    #region Abonnement aux Événements
        private void SubscribeToEvents()
        {
            if (gameLauncher == null) return;
            // Note: L'abonnement via réflexion ne fonctionne pas (+= sur une copie locale n'a pas d'effet réel).
            // Pour un vrai abonnement, il faudrait exposer une méthode publique ou utiliser Delegate.Combine/SetValue.
            // Ici, on laisse la structure pour compatibilité, mais il n'y aura pas de callback effectif.
            try
            {
                var onLaunchStartedEvent = gameLauncher.GetType().GetField("OnGameLaunchStarted");
                if (onLaunchStartedEvent != null)
                {
                    var eventValue = onLaunchStartedEvent.GetValue(gameLauncher) as System.Action;
                    if (eventValue != null)
                    {
                        // eventValue += OnLaunchStarted; // Ne modifie pas la cible
                    }
                }
                var onLaunchCompletedEvent = gameLauncher.GetType().GetField("OnGameLaunchCompleted");
                if (onLaunchCompletedEvent != null)
                {
                    var eventValue = onLaunchCompletedEvent.GetValue(gameLauncher) as System.Action;
                    if (eventValue != null)
                    {
                        // eventValue += OnLaunchCompleted;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogParadeLogger.LogWarning($"[DebugPanel_GameLauncher] Impossible de s'abonner aux événements: {ex.Message}");
            }
        }
    #endregion

    #region Accès Données (Réflexion)
        private bool GetIsLaunching()
        {
            if (gameLauncher == null) return false;
            var field = gameLauncher.GetType().GetField("isLaunching", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (bool)field.GetValue(gameLauncher);
            }
            return false;
        }
        private bool GetGameStarted()
        {
            if (gameLauncher == null) return false;
            var field = gameLauncher.GetType().GetField("gameFullyStarted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (bool)field.GetValue(gameLauncher);
            }
            return false;
        }
        private bool GetCalibrationCompleted()
        {
            if (gameLauncher == null) return false;
            var method = gameLauncher.GetType().GetMethod("IsCalibrationCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                return (bool)method.Invoke(gameLauncher, null);
            }
            return false;
        }
        private int CountFoundSystems()
        {
            if (gameLauncher == null) return 0;
            int count = 0;
            var systemFields = new string[] { "gameController", "gameTimer", "logGenerator", "scoreManager" };
            foreach (var fieldName in systemFields)
            {
                var field = gameLauncher.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var value = field.GetValue(gameLauncher);
                    if (value != null) count++;
                }
            }
            return count;
        }
        private object GetSystemComponent(string fieldName)
        {
            if (gameLauncher == null) return null;
            var field = gameLauncher.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(gameLauncher);
        }
        private string GetSystemStatusIcon(object component)
        {
            if (component == null) return "<color=red>❌</color>";
            var unityObj = component as UnityEngine.Object;
            if (unityObj == null) return "<color=red>❌</color>";
            var gameObject = unityObj as GameObject;
            if (gameObject != null)
            {
                return gameObject.activeInHierarchy ? "<color=green>✅</color>" : "<color=yellow>⚠️</color>";
            }
            var monoBehaviour = unityObj as MonoBehaviour;
            if (monoBehaviour != null)
            {
                return (monoBehaviour.enabled && monoBehaviour.gameObject.activeInHierarchy) ? "<color=green>✅</color>" : "<color=yellow>⚠️</color>";
            }
            return "<color=green>✅</color>";
        }
    #endregion

    #region Actions & Contrôles
        private void LaunchGame()
        {
            if (gameLauncher == null) return;
            lastLaunchStatus = "Lancement demandé";
            gameLauncher.LaunchFullGame();
            LogParadeLogger.Log("[DebugPanel_GameLauncher] Lancement du jeu demandé");
        }
        private void RestartGame()
        {
            if (gameLauncher == null) return;
            lastLaunchStatus = "Redémarrage demandé";
            gameLauncher.RestartGame();
            LogParadeLogger.Log("[DebugPanel_GameLauncher] Redémarrage du jeu demandé");
        }
        private void StopAllSystems()
        {
            if (gameLauncher == null) return;
            lastLaunchStatus = "Redémarrage demandé";
            gameLauncher.RestartGame();
            LogParadeLogger.Log("[DebugPanel_GameLauncher] Redémarrage du jeu demandé");
        }
        private void RunDiagnostic()
        {
            if (gameLauncher == null) return;
            lastLaunchStatus = "Diagnostic effectué";
            LogParadeLogger.Log("=== DIAGNOSTIC LOGPARADE GAME LAUNCHER ===");
            LogParadeLogger.Log($"GameLauncher trouvé: {gameLauncher != null}");
            LogParadeLogger.Log($"Systèmes détectés: {systemsFound}/4");
            LogParadeLogger.Log($"En cours de lancement: {isLaunching}");
            LogParadeLogger.Log($"Jeu démarré: {gameStarted}");
            LogParadeLogger.Log($"Calibration terminée: {calibrationCompleted}");
            LogParadeLogger.Log("=== FIN DIAGNOSTIC ===");
        }
    #endregion

    #region Gestionnaires d'Événements
        private void OnLaunchStarted()
        {
            lastLaunchStatus = "Lancement démarré";
        }
        private void OnLaunchCompleted()
        {
            lastLaunchStatus = "Lancement terminé";
        }
    #endregion
    }
}