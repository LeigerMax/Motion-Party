using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    /// <summary>
    /// Contrôleur du menu principal - gère la navigation entre les panneaux UI et les actions des boutons principaux
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameSelectionPanel;
        [SerializeField] private GameObject playerSelectionPanel;
        [SerializeField] private GameObject playerManagementPanel;

        [Header("Cameras")]
        [SerializeField] private UnityEngine.Camera mainMenuCamera;
        [SerializeField] private UnityEngine.Camera gameSelectionCamera;

        [Header("UI Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button teamButton;
        [SerializeField] private Button quitButton;

        [Header("Game Selection Controller")]
        [SerializeField] private GameSelectionController gameSelectionController;

        [Header("Player Management")]
        [SerializeField] private UI.PlayerManagement.PlayerManagementUI playerManagementUI;

        [Header("Team Management")]
        [SerializeField] private UI.Team.TeamManagementUI teamManagementUI;

        [Header("Player Selection")]
        [SerializeField] private UI.PlayerSelection.PlayerSelectionUI playerSelectionUI;

        private void Start()
        {
            InitializeButtons();
            InitializePanels();
            ShowMainMenu();
        }

        private void InitializePanels()
        {
            // S'assurer que les panneaux existent
            if (mainMenuPanel == null)
                Debug.LogWarning("MainMenuPanel n'est pas assigné dans MainMenuController");
            
            if (gameSelectionPanel == null)
                Debug.LogWarning("GameSelectionPanel n'est pas assigné dans MainMenuController");
                
            if (playerSelectionPanel == null)
                Debug.LogWarning("PlayerSelectionPanel n'est pas assigné dans MainMenuController");
                
            if (playerManagementPanel == null)
                Debug.LogWarning("PlayerManagementPanel n'est pas assigné dans MainMenuController");
                
            // S'assurer que les caméras existent (optionnel)
            if (mainMenuCamera == null)
                Debug.LogWarning("MainMenuCamera n'est pas assignée dans MainMenuController (optionnel)");
            
            if (gameSelectionCamera == null)
                Debug.LogWarning("GameSelectionCamera n'est pas assignée dans MainMenuController (optionnel)");
        }

        private void InitializeButtons()
        {
            // Configuration des boutons principaux
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
            
            if (optionsButton != null)
                optionsButton.onClick.AddListener(OnOptionsButtonClicked);
            
            if (teamButton != null)
                teamButton.onClick.AddListener(OnTeamButtonClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        /// <summary>
        /// Affiche le menu principal (MainMenuPanel actif + MainMenuCamera si configurée)
        /// </summary>
        public void ShowMainMenu()
        {

            // Gestion des panneaux UI
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(false);

            if (playerSelectionPanel != null)
                playerSelectionPanel.SetActive(false);

            if (playerManagementPanel != null)
                playerManagementPanel.SetActive(false);

            // Transition de caméra fluide (nouveau système)
            if (CameraTransitions.CameraTransitionManager.Instance != null)
            {
                Debug.Log("MainMenuController: Lancement de la transition vers le menu principal");
                CameraTransitions.CameraTransitionManager.Instance.TransitionToMainMenu();
            }
            else
            {
                Debug.LogWarning("MainMenuController: CameraTransitionManager non trouvé, utilisation du fallback");
                // Fallback : gestion des caméras classique (optionnel)
                if (mainMenuCamera != null)
                {
                    mainMenuCamera.gameObject.SetActive(true);
                    mainMenuCamera.enabled = true;
                }

                if (gameSelectionCamera != null)
                {
                    gameSelectionCamera.gameObject.SetActive(false);
                    gameSelectionCamera.enabled = false;
                }
            }

            // Désactiver le contrôleur de sélection de jeu
            if (gameSelectionController != null)
                gameSelectionController.gameObject.SetActive(false);

        }

        /// <summary>
        /// Affiche la vue de sélection de jeu (GameSelectionPanel actif + GameSelectionCamera si configurée)
        /// </summary>
        public void ShowGameSelection()
        {
            // Gestion des panneaux UI
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(true);

            if (playerSelectionPanel != null)
                playerSelectionPanel.SetActive(false);

            if (playerManagementPanel != null)
                playerManagementPanel.SetActive(false);

            // Transition de caméra fluide (nouveau système)
            if (CameraTransitions.CameraTransitionManager.Instance != null)
            {
                CameraTransitions.CameraTransitionManager.Instance.TransitionToGameSelection();
            }
            else
            {
                // Fallback : gestion des caméras classique (optionnel)
                if (mainMenuCamera != null)
                {
                    mainMenuCamera.gameObject.SetActive(false);
                    mainMenuCamera.enabled = false;
                }

                if (gameSelectionCamera != null)
                {
                    gameSelectionCamera.gameObject.SetActive(true);
                    gameSelectionCamera.enabled = true;
                }
            }

            // Activer le contrôleur de sélection de jeu
            if (gameSelectionController != null)
                gameSelectionController.gameObject.SetActive(true);
        }

        /// <summary>
        /// Affiche la vue de sélection des joueurs (PlayerSelectionPanel actif)
        /// </summary>
        public void ShowPlayerSelection()
        {
            // Gestion des panneaux UI
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(false);

            if (playerSelectionPanel != null)
                playerSelectionPanel.SetActive(true);

            if (playerManagementPanel != null)
                playerManagementPanel.SetActive(false);

            // Activer l'interface de sélection des joueurs
            if (playerSelectionUI != null)
                playerSelectionUI.gameObject.SetActive(true);
        }

        /// <summary>
        /// Affiche la vue de gestion des joueurs (PlayerManagementPanel actif)
        /// </summary>
        public void ShowPlayerManagement()
        {
            // Gestion des panneaux UI
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (gameSelectionPanel != null)
                gameSelectionPanel.SetActive(false);

            if (playerSelectionPanel != null)
                playerSelectionPanel.SetActive(false);

            if (playerManagementPanel != null)
                playerManagementPanel.SetActive(true);

            // Activer l'interface de gestion de l'équipe
            if (teamManagementUI != null)
                teamManagementUI.gameObject.SetActive(true);
                
            // Activer l'interface de gestion des joueurs (pour compatibilité)
            if (playerManagementUI != null)
                playerManagementUI.gameObject.SetActive(true);
        }

        #region Button Events

        private void OnPlayButtonClicked()
        {
            // Navigation vers le panneau de sélection de jeu
            ShowGameSelection();
        }

        private void OnOptionsButtonClicked()
        {
            // TODO: Implémenter l'ouverture des options
            // Garde la place pour des paramètres de jeu futurs (son, langue, accessibilité...)
            Debug.Log("Options - À implémenter plus tard");
        }

        private void OnTeamButtonClicked()
        {
            // Navigation vers l'interface de gestion de l'équipe
            ShowPlayerManagement();
        }

        private void OnQuitButtonClicked()
        {
            QuitApplication();
        }

        #endregion

        #region Simulation Methods (pour contrôle vocal/gestuel futur)

        /// <summary>
        /// Simule un clic sur le bouton Jouer (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulatePlayClick()
        {
            OnPlayButtonClicked();
        }

        /// <summary>
        /// Simule un clic sur le bouton Options (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateOptionsClick()
        {
            OnOptionsButtonClicked();
        }

        /// <summary>
        /// Simule un clic sur le bouton Équipe (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateTeamClick()
        {
            OnTeamButtonClicked();
        }

        /// <summary>
        /// Simule un clic sur le bouton Quitter (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateQuitClick()
        {
            OnQuitButtonClicked();
        }

        #endregion

        private void QuitApplication()
        {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
        }

        private void OnDestroy()
        {
            // Nettoyage des listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            
            if (optionsButton != null)
                optionsButton.onClick.RemoveListener(OnOptionsButtonClicked);
            
            if (teamButton != null)
                teamButton.onClick.RemoveListener(OnTeamButtonClicked);
            
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }
}
