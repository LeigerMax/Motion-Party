using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    /// <summary>
    /// Contrôleur de la vue de sélection de jeu - gère les options de lancement de partie
    /// </summary>
    public class GameSelectionController : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button newLocalGameButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button backButton;

        [Header("References")]
        [SerializeField] private MainMenuController mainMenuController;

        private void Start()
        {
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            // Configuration des boutons de sélection de jeu
            if (newLocalGameButton != null)
                newLocalGameButton.onClick.AddListener(OnNewLocalGameClicked);
            
            if (multiplayerButton != null)
            {
                multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
                // Désactiver le bouton multijoueur pour le moment
                multiplayerButton.interactable = false;
            }
            
            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
                // Désactiver la sélection de niveau pour le moment
                levelSelectButton.interactable = false;
            }
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        #region Button Events

        private void OnNewLocalGameClicked()
        {
            // Lancer le mini-jeu FireflyDance via MiniGameBase
            LaunchFireflyDance();
        }

        private void OnMultiplayerClicked()
        {
            // TODO: Implémenter le multijoueur
            Debug.Log("Multijoueur - À venir");
        }

        private void OnLevelSelectClicked()
        {
            // TODO: Implémenter la sélection de niveau
            Debug.Log("Sélection de niveau - À venir");
        }

        private void OnBackClicked()
        {
            Debug.Log("GameSelectionController: Bouton Retour cliqué");
            
            // Retour au menu principal avec transition fluide
            if (mainMenuController != null)
            {
                Debug.Log("GameSelectionController: Appel de ShowMainMenu() sur MainMenuController");
                mainMenuController.ShowMainMenu();
            }
            else
            {
                Debug.LogWarning("GameSelectionController: MainMenuController n'est pas assigné, utilisation du fallback");
                // Fallback : utiliser directement le CameraTransitionManager si disponible
                if (CameraTransitions.CameraTransitionManager.Instance != null)
                {
                    Debug.Log("GameSelectionController: Utilisation du CameraTransitionManager en fallback");
                    CameraTransitions.CameraTransitionManager.Instance.TransitionToMainMenu();
                }
                else
                {
                    Debug.LogError("GameSelectionController: Aucun système de transition disponible !");
                }
            }
        }

        #endregion

        #region Simulation Methods (pour contrôle vocal/gestuel futur)

        /// <summary>
        /// Simule un clic sur Nouvelle partie locale (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateNewLocalGameClick()
        {
            OnNewLocalGameClicked();
        }

        /// <summary>
        /// Simule un clic sur Multijoueur (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateMultiplayerClick()
        {
            OnMultiplayerClicked();
        }

        /// <summary>
        /// Simule un clic sur Sélection de niveau (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateLevelSelectClick()
        {
            OnLevelSelectClicked();
        }

        /// <summary>
        /// Simule un clic sur Retour (pour contrôle vocal/gestuel)
        /// </summary>
        public void SimulateBackClick()
        {
            OnBackClicked();
        }

        #endregion

        private void LaunchFireflyDance()
        {
            // Rechercher le GameSessionManager dans la scène
            var gameSessionManager = FindObjectOfType<GameSessionManager>();
            
            if (gameSessionManager != null)
            {
                // Lancer la session de mini-jeux
                Debug.Log("Lancement de la session de mini-jeux via GameSessionManager");
                gameSessionManager.StartGameSession();
            }
            else
            {
                // Fallback : rechercher un mini-jeu individuel dans la scène (pour les tests)
                var fireflyGame = FindObjectOfType<MiniGameBase>();
                
                if (fireflyGame != null)
                {
                    Debug.Log("Lancement d'un mini-jeu individuel");
                    // Lancer le mini-jeu avec un callback de fin
                    fireflyGame.StartMiniGame(() => {
                        // Callback appelé quand le mini-jeu se termine
                        Debug.Log("Mini-jeu FireflyDance terminé");
                        
                        // Retourner au menu principal
                        if (mainMenuController != null)
                            mainMenuController.ShowMainMenu();
                    });
                }
                else
                {
                    Debug.LogWarning("Aucun GameSessionManager ou MiniGameBase trouvé dans la scène. Assurez-vous qu'un GameObject avec un de ces composants est présent.");
                }
            }
        }

        private void OnDestroy()
        {
            // Nettoyage des listeners
            if (newLocalGameButton != null)
                newLocalGameButton.onClick.RemoveListener(OnNewLocalGameClicked);
            
            if (multiplayerButton != null)
                multiplayerButton.onClick.RemoveListener(OnMultiplayerClicked);
            
            if (levelSelectButton != null)
                levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
            
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
        }
    }
}
