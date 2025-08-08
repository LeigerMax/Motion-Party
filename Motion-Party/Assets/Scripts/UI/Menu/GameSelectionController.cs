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
            // Afficher l'interface de sélection des joueurs
            ShowPlayerSelection();
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
            
            // Retour au menu principal avec transition fluide
            if (mainMenuController != null)
            {
                mainMenuController.ShowMainMenu();
            }
            else
            {
                // Fallback : utiliser directement le CameraTransitionManager si disponible
                if (CameraTransitions.CameraTransitionManager.Instance != null)
                {
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

        /// <summary>
        /// Affiche l'interface de sélection des joueurs
        /// </summary>
        private void ShowPlayerSelection()
        {
            // Vérifier s'il y a des joueurs disponibles
            if (Systems.PlayerProfileManager.Instance.GetPlayerCount() == 0)
            {
                Debug.LogWarning("Aucun joueur disponible. Créez d'abord des joueurs dans la section Équipe.");
                return;
            }

            // Utiliser le MainMenuController pour afficher le panel de sélection
            if (mainMenuController != null)
            {
                mainMenuController.ShowPlayerSelection();
            }
            else
            {
                Debug.LogError("MainMenuController n'est pas assigné dans GameSelectionController");
            }
        }

        /// <summary>
        /// Crée l'interface de sélection des joueurs
        /// </summary>
        private void CreatePlayerSelectionInterface()
        {
            // Cette méthode peut être implémentée pour créer dynamiquement l'interface
            // ou simplement activer un panneau existant
            Debug.Log("Interface de sélection des joueurs à implémenter");
        }

        private void LaunchMiniGame()
        {
            Debug.Log("🔍 LaunchMiniGame() appelé");
            
            // Vérifier si des joueurs sont sélectionnés
            var gamePlayerSelector = Systems.GamePlayerSelector.Instance;
            if (gamePlayerSelector.PlayerCount == 0)
            {
                Debug.LogWarning("Aucun joueur sélectionné pour la partie");
                return;
            }

            // Démarrer la partie avec les joueurs sélectionnés
            gamePlayerSelector.StartGame();
            
            // Rechercher le GameSessionManager dans la scène
            var gameSessionManager = FindFirstObjectByType<GameSessionManager>();
            
            if (gameSessionManager != null)
            {
                // Lancer la session de mini-jeux
                gameSessionManager.StartGameSession();
            }
            else
            {
                
                // Fallback : rechercher un mini-jeu individuel dans la scène (incluant les objets inactifs)
                var miniGame = FindFirstObjectByType<MiniGameBase>(FindObjectsInactive.Include);
                
                if (miniGame != null)
                {
                    // S'assurer que le GameObject est actif avant de lancer
                    if (!miniGame.gameObject.activeInHierarchy)
                    {
                        miniGame.gameObject.SetActive(true);
                    }
                    
                    // Lancer le mini-jeu avec un callback de fin
                    miniGame.StartMiniGame(() => {
                        // Callback appelé quand le mini-jeu se termine
                        Debug.Log($"🏁 Mini-jeu {miniGame.GetType().Name} terminé");
                        
                        // Terminer la session de jeu
                        gamePlayerSelector.EndGame();
                        
                        // Retourner au menu principal
                        if (mainMenuController != null)
                            mainMenuController.ShowMainMenu();
                    });
                }
                else
                {
                    Debug.LogError(" Aucun GameSessionManager ou MiniGameBase trouvé dans la scène. Assurez-vous qu'un GameObject avec un de ces composants est présent.");
                    
                    // Debug : lister tous les GameObjects dans la scène
                    var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                    foreach (var obj in allObjects)
                    {
                        if (obj.GetType().Name.Contains("Firefly") || obj.GetType().Name.Contains("Game"))
                        {
                            Debug.Log($"  - {obj.GetType().Name} sur '{obj.gameObject.name}'");
                        }
                    }
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
