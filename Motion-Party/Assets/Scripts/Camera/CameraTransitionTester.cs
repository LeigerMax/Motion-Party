using UnityEngine;
using UnityEngine.UI;

namespace CameraTransitions
{
    /// <summary>
    /// Script de test et debug pour les transitions de caméra.
    /// Utile pour tester les transitions sans avoir besoin de l'interface complète.
    /// </summary>
    public class CameraTransitionTester : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private KeyCode testMainMenuKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode testGameSelectionKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode stopTransitionKey = KeyCode.Escape;
        
        [Header("Manual Targets (Optionnel)")]
        [SerializeField] private UnityEngine.Camera customCamera1;
        [SerializeField] private UnityEngine.Camera customCamera2;
        [SerializeField] private KeyCode customCamera1Key = KeyCode.Alpha3;
        [SerializeField] private KeyCode customCamera2Key = KeyCode.Alpha4;
        
        [Header("Test Settings")]
        [SerializeField] private float testDuration = 2.0f;
        [SerializeField] private bool showInstructions = true;
        
        [Header("UI Test Buttons (Optionnel)")]
        [SerializeField] private Button testMainMenuButton;
        [SerializeField] private Button testGameSelectionButton;
        [SerializeField] private Button stopButton;
        
        private void Start()
        {
            SetupUIButtons();
            
            if (showInstructions)
                ShowInstructions();
        }
        
        private void SetupUIButtons()
        {
            if (testMainMenuButton != null)
                testMainMenuButton.onClick.AddListener(() => TestTransitionToMainMenu());
                
            if (testGameSelectionButton != null)
                testGameSelectionButton.onClick.AddListener(() => TestTransitionToGameSelection());
                
            if (stopButton != null)
                stopButton.onClick.AddListener(() => StopCurrentTransition());
        }
        
        private void Update()
        {
            HandleKeyboardInput();
        }
        
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(testMainMenuKey))
                TestTransitionToMainMenu();
                
            if (Input.GetKeyDown(testGameSelectionKey))
                TestTransitionToGameSelection();
                
            if (Input.GetKeyDown(stopTransitionKey))
                StopCurrentTransition();
                
            if (customCamera1 != null && Input.GetKeyDown(customCamera1Key))
                TestCustomTransition(customCamera1);
                
            if (customCamera2 != null && Input.GetKeyDown(customCamera2Key))
                TestCustomTransition(customCamera2);
        }
        
        private void TestTransitionToMainMenu()
        {
            Debug.Log("CameraTransitionTester: Test transition vers menu principal");
            
            // Tester la version de base d'abord
            if (CameraTransitionManager.Instance != null)
            {
                CameraTransitionManager.Instance.TransitionToMainMenu(testDuration);
                return;
            }
            
            // Fallback sur la version avancée
            if (CameraTransitionManagerAdvanced.Instance != null)
            {
                CameraTransitionManagerAdvanced.Instance.TransitionToMainMenu(testDuration);
                return;
            }
            
            Debug.LogWarning("CameraTransitionTester: Aucun CameraTransitionManager trouvé !");
        }
        
        private void TestTransitionToGameSelection()
        {
            Debug.Log("CameraTransitionTester: Test transition vers sélection de jeu");
            
            // Tester la version de base d'abord
            if (CameraTransitionManager.Instance != null)
            {
                CameraTransitionManager.Instance.TransitionToGameSelection(testDuration);
                return;
            }
            
            // Fallback sur la version avancée
            if (CameraTransitionManagerAdvanced.Instance != null)
            {
                CameraTransitionManagerAdvanced.Instance.TransitionToGameSelection(testDuration);
                return;
            }
            
            Debug.LogWarning("CameraTransitionTester: Aucun CameraTransitionManager trouvé !");
        }
        
        private void TestCustomTransition(UnityEngine.Camera targetCamera)
        {
            if (targetCamera == null)
            {
                Debug.LogWarning("CameraTransitionTester: Caméra cible est null");
                return;
            }
            
            Debug.Log($"CameraTransitionTester: Test transition vers {targetCamera.name}");
            
            // Tester la version de base d'abord
            if (CameraTransitionManager.Instance != null)
            {
                CameraTransitionManager.Instance.StartTransition(targetCamera, testDuration);
                return;
            }
            
            // Fallback sur la version avancée
            if (CameraTransitionManagerAdvanced.Instance != null)
            {
                CameraTransitionManagerAdvanced.Instance.StartTransition(targetCamera, testDuration);
                return;
            }
            
            Debug.LogWarning("CameraTransitionTester: Aucun CameraTransitionManager trouvé !");
        }
        
        private void StopCurrentTransition()
        {
            Debug.Log("CameraTransitionTester: Arrêt de la transition en cours");
            
            if (CameraTransitionManager.Instance != null)
                CameraTransitionManager.Instance.StopCurrentTransition();
                
            if (CameraTransitionManagerAdvanced.Instance != null)
                CameraTransitionManagerAdvanced.Instance.StopCurrentTransition();
        }
        
        private void ShowInstructions()
        {
            string instructions = $@"
=== CAMERA TRANSITION TESTER ===

Contrôles clavier :
• {testMainMenuKey} : Transition vers menu principal
• {testGameSelectionKey} : Transition vers sélection de jeu
• {stopTransitionKey} : Arrêter la transition en cours";

            if (customCamera1 != null)
                instructions += $"\n• {customCamera1Key} : Transition vers {customCamera1.name}";
                
            if (customCamera2 != null)
                instructions += $"\n• {customCamera2Key} : Transition vers {customCamera2.name}";

            instructions += $"\n\nDurée des tests : {testDuration}s";
            
            Debug.Log(instructions);
        }
        
        private void OnGUI()
        {
            if (!showInstructions) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Camera Transition Tester", GUI.skin.label);
            GUILayout.Space(5);
            
            if (GUILayout.Button($"Main Menu ({testMainMenuKey})"))
                TestTransitionToMainMenu();
                
            if (GUILayout.Button($"Game Selection ({testGameSelectionKey})"))
                TestTransitionToGameSelection();
                
            if (GUILayout.Button($"Stop ({stopTransitionKey})"))
                StopCurrentTransition();
            
            GUILayout.Space(10);
            
            // Afficher l'état des managers
            bool hasBasic = CameraTransitionManager.Instance != null;
            bool hasAdvanced = CameraTransitionManagerAdvanced.Instance != null;
            
            GUILayout.Label($"Manager de base : {(hasBasic ? "✓" : "✗")}");
            GUILayout.Label($"Manager avancé : {(hasAdvanced ? "✓" : "✗")}");
            
            if (hasBasic)
                GUILayout.Label($"En transition : {CameraTransitionManager.Instance.IsTransitioning}");
            else if (hasAdvanced)
                GUILayout.Label($"En transition : {CameraTransitionManagerAdvanced.Instance.IsTransitioning}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void OnDestroy()
        {
            // Nettoyer les listeners UI
            if (testMainMenuButton != null)
                testMainMenuButton.onClick.RemoveAllListeners();
                
            if (testGameSelectionButton != null)
                testGameSelectionButton.onClick.RemoveAllListeners();
                
            if (stopButton != null)
                stopButton.onClick.RemoveAllListeners();
        }
    }
}
