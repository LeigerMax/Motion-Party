using UnityEngine;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Fireflies;
using Gameplay.FireFlyDance.Hand;
using Gameplay.FireFlyDance.Scoring;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Testing
{
    /// <summary>
    /// Outils de debug pour tester l'intégration main/luciole dans FireflyDance
    /// </summary>
    public class FireflyDanceDebugger : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        
        [Header("Managers")]
        [SerializeField] private FireflyScoreManager scoreManager;
        [SerializeField] private FireflySpawner spawner;
        [SerializeField] private HandTracker handTracker;
        [SerializeField] private HandInteractor handInteractor;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugKeys = true;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("Test Settings")]
        [SerializeField] private Vector2 testCapturePosition = Vector2.zero;
        [SerializeField] private bool simulateClosedHand = false;

        #endregion

        #region Unity Lifecycle

        void Update()
        {
            if (enableDebugKeys)
            {
                HandleDebugKeys();
            }
        }

        void OnGUI()
        {
            if (showDebugInfo)
            {
                DrawDebugInfo();
            }
        }

        #endregion

        #region Debug Input

        /// <summary>
        /// Gère les touches de debug
        /// </summary>
        private void HandleDebugKeys()
        {
            // F1 - Simuler capture d'une luciole
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SimulateFireflyCapture();
            }
            
            // F2 - Spawn manuel d'une luciole
            if (Input.GetKeyDown(KeyCode.F2))
            {
                SpawnTestFirefly();
            }
            
            // F3 - Tester détection de main
            if (Input.GetKeyDown(KeyCode.F3))
            {
                TestHandDetection();
            }
            
            // F4 - Reset score
            if (Input.GetKeyDown(KeyCode.F4))
            {
                ResetScore();
            }
            
            // F5 - Test interaction zone
            if (Input.GetKeyDown(KeyCode.F5))
            {
                TestInteractionZone();
            }
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Simule la capture d'une luciole
        /// </summary>
        [ContextMenu("Simulate Firefly Capture")]
        public void SimulateFireflyCapture()
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("Config manquante pour la simulation");
                return;
            }

            // Créer une luciole temporaire pour la simulation
            GameObject tempFirefly = new GameObject("DebugFirefly");
            FireflyController fireflyController = tempFirefly.AddComponent<FireflyController>();
            
            // L'initialiser
            fireflyController.Initialize(config);
            
            FireflyDanceLogger.Log("Simulation capture de luciole...");
            
            // Déclencher l'événement de capture
            FireflyDanceEvents.OnFireflyCaptured?.Invoke(fireflyController);
            
            // Nettoyer
            Destroy(tempFirefly, 0.1f);
        }

        /// <summary>
        /// Spawn une luciole de test
        /// </summary>
        [ContextMenu("Spawn Test Firefly")]
        public void SpawnTestFirefly()
        {
            if (spawner != null)
            {
                spawner.ForceSpawnFirefly();
                FireflyDanceLogger.Log("Luciole de test spawnée");
            }
            else
            {
                FireflyDanceLogger.LogError("FireflySpawner non trouvé");
            }
        }

        /// <summary>
        /// Teste la détection de main
        /// </summary>
        [ContextMenu("Test Hand Detection")]
        public void TestHandDetection()
        {
            if (handTracker != null)
            {
                bool detected = handTracker.IsHandDetected;
                bool closed = handTracker.IsHandClosed;
                Vector2 position = handTracker.CurrentPosition;
                
                FireflyDanceLogger.Log($"Main - Détectée: {detected}, Fermée: {closed}, Position: {position}");
            }
            else
            {
                FireflyDanceLogger.LogError("HandTracker non trouvé");
            }
        }

        /// <summary>
        /// Reset le score
        /// </summary>
        [ContextMenu("Reset Score")]
        public void ResetScore()
        {
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
                FireflyDanceLogger.Log("Score réinitialisé");
            }
            else
            {
                FireflyDanceLogger.LogError("ScoreManager non trouvé");
            }
        }

        /// <summary>
        /// Teste la zone d'interaction
        /// </summary>
        [ContextMenu("Test Interaction Zone")]
        public void TestInteractionZone()
        {
            if (handInteractor != null && config != null)
            {
                Vector2 testPos = testCapturePosition;
                bool canCapture = handInteractor.CheckCaptureAtPosition(testPos);
                FireflyController[] fireflies = handInteractor.GetFirefliesInRange(testPos);
                
                FireflyDanceLogger.Log($"Zone d'interaction - Position: {testPos}, Capture possible: {canCapture}, Lucioles dans la zone: {fireflies.Length}");
            }
            else
            {
                FireflyDanceLogger.LogError("HandInteractor ou Config non trouvé");
            }
        }

        #endregion

        #region Debug Display

        /// <summary>
        /// Affiche les informations de debug
        /// </summary>
        private void DrawDebugInfo()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label("=== FIREFLY DANCE DEBUG ===", new GUIStyle(GUI.skin.label) { fontSize = 16 });
            
            // Informations du score
            if (scoreManager != null)
            {
                GUILayout.Label($"Score actuel: {scoreManager.CurrentScore}");
                GUILayout.Label($"Record: {scoreManager.HighScore}");
                GUILayout.Label($"Lucioles capturées: {scoreManager.FirefliesCaptured}");
            }
            
            // Informations de la main
            if (handTracker != null)
            {
                GUILayout.Label($"Main détectée: {handTracker.IsHandDetected}");
                GUILayout.Label($"Main fermée: {handTracker.IsHandClosed}");
                GUILayout.Label($"Position: {handTracker.CurrentPosition}");
            }
            
            // Informations du spawner
            if (spawner != null)
            {
                GUILayout.Label($"Lucioles actives: {spawner.ActiveFireflyCount}");
                GUILayout.Label($"Spawn actif: {spawner.IsSpawning}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Touches de debug:");
            GUILayout.Label("F1 - Simuler capture");
            GUILayout.Label("F2 - Spawn luciole");
            GUILayout.Label("F3 - Test main");
            GUILayout.Label("F4 - Reset score");
            GUILayout.Label("F5 - Test zone");
            
            GUILayout.EndArea();
        }

        #endregion

        #region Validation

        void Start()
        {
            ValidateComponents();
        }

        /// <summary>
        /// Valide les composants requis
        /// </summary>
        private void ValidateComponents()
        {
            if (config == null)
                config = FindFirstObjectByType<FireflyDanceConfig>();
                
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<FireflyScoreManager>();
                
            if (spawner == null)
                spawner = FindFirstObjectByType<FireflySpawner>();
                
            if (handTracker == null)
                handTracker = FindFirstObjectByType<HandTracker>();
                
            if (handInteractor == null)
                handInteractor = FindFirstObjectByType<HandInteractor>();
        }

        #endregion
    }
}
