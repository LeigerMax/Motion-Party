using UnityEngine;
using System.Reflection;

namespace UI.PlayerSelection
{
    /// <summary>
    /// Helper pour gérer les API et éviter les erreurs de compilation
    /// </summary>
    public static class PlayerSelectionAPIHelper
    {
        /// <summary>
        /// Supprime un joueur de manière sécurisée en utilisant l'API disponible
        /// </summary>
        public static bool SafeRemovePlayer(string playerId)
        {
            try
            {
                var playerManager = Systems.PlayerProfileManager.Instance;
                if (playerManager == null) return false;

                // Utiliser RemovePlayer au lieu de DeletePlayer
                return playerManager.RemovePlayer(playerId);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de la suppression du joueur {playerId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Vide la sélection de joueurs de manière sécurisée
        /// </summary>
        public static bool SafeClearPlayerSelection()
        {
            try
            {
                var gamePlayerSelector = Systems.GamePlayerSelector.Instance;
                if (gamePlayerSelector == null) return false;

                // Utiliser ClearSelection au lieu de ClearPlayers
                gamePlayerSelector.ClearSelection();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors du vidage de la sélection: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lance un mini-jeu de manière sécurisée
        /// </summary>
        public static bool SafeLaunchMiniGame(System.Action onGameFinished = null)
        {
            try
            {
                // Rechercher un MiniGameBase (sans namespace Core)
                var miniGame = Object.FindFirstObjectByType<MiniGameBase>(FindObjectsInactive.Include);
                if (miniGame != null)
                {
                    if (!miniGame.gameObject.activeInHierarchy)
                    {
                        miniGame.gameObject.SetActive(true);
                    }
                    
                    miniGame.StartMiniGame(onGameFinished ?? (() => { }));
                    return true;
                }

                Debug.LogWarning("Aucun MiniGameBase trouvé dans la scène");
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors du lancement du mini-jeu: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Vérifie les systèmes disponibles pour le debug
        /// </summary>
        public static void DebugAvailableSystems()
        {
            Debug.Log("=== SYSTEMES DISPONIBLES ===");
            
            try
            {
                Debug.Log($"PlayerProfileManager: {Systems.PlayerProfileManager.Instance != null}");
                Debug.Log($"GamePlayerSelector: {Systems.GamePlayerSelector.Instance != null}");
                
                var mainMenuController = Object.FindFirstObjectByType<UI.Menu.MainMenuController>();
                Debug.Log($"MainMenuController: {mainMenuController != null}");
                
                var gameSelectionController = Object.FindFirstObjectByType<UI.Menu.GameSelectionController>();
                Debug.Log($"GameSelectionController: {gameSelectionController != null}");
                
                Debug.Log($"CameraTransitionManager: {CameraTransitions.CameraTransitionManager.Instance != null}");
                
                var gameSessionManager = Object.FindFirstObjectByType<GameSessionManager>();
                Debug.Log($"GameSessionManager: {gameSessionManager != null}");
                
                var miniGames = Object.FindObjectsByType<MiniGameBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Debug.Log($"MiniGameBase instances: {miniGames.Length}");
                
                foreach (var miniGame in miniGames)
                {
                    Debug.Log($"  - {miniGame.GetType().Name} (Active: {miniGame.gameObject.activeInHierarchy})");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors du debug des systèmes: {ex.Message}");
            }
            
            Debug.Log("=== FIN DEBUG ===");
        }

        /// <summary>
        /// Appelle une méthode par réflexion de manière sécurisée
        /// </summary>
        public static bool SafeInvokeMethod(object target, string methodName, object[] parameters = null)
        {
            try
            {
                if (target == null) return false;

                var type = target.GetType();
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (method == null)
                {
                    Debug.LogWarning($"Méthode {methodName} non trouvée dans {type.Name}");
                    return false;
                }

                method.Invoke(target, parameters);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de l'appel de {methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retourne au menu de sélection de jeu avec plusieurs fallbacks
        /// </summary>
        public static bool SafeReturnToGameSelection()
        {
            try
            {
                // Méthode 1: MainMenuController
                var mainMenuController = Object.FindFirstObjectByType<UI.Menu.MainMenuController>();
                if (mainMenuController != null)
                {
                    if (SafeInvokeMethod(mainMenuController, "ShowGameSelection"))
                    {
                        Debug.Log("Retour au menu via MainMenuController.ShowGameSelection()");
                        return true;
                    }
                }

                // Méthode 2: CameraTransitionManager
                if (CameraTransitions.CameraTransitionManager.Instance != null)
                {
                    var transitionManager = CameraTransitions.CameraTransitionManager.Instance;
                    if (SafeInvokeMethod(transitionManager, "TransitionToGameSelection"))
                    {
                        Debug.Log("Retour au menu via CameraTransitionManager.TransitionToGameSelection()");
                        return true;
                    }
                    else
                    {
                        transitionManager.TransitionToMainMenu();
                        Debug.Log("Retour au menu principal via CameraTransitionManager");
                        return true;
                    }
                }

                // Méthode 3: Activation directe du GameSelectionController
                var gameSelectionController = Object.FindFirstObjectByType<UI.Menu.GameSelectionController>();
                if (gameSelectionController != null)
                {
                    gameSelectionController.gameObject.SetActive(true);
                    Debug.Log("GameSelectionController activé directement");
                    return true;
                }

                // Méthode 4: Recherche par nom
                var gameSelectionPanel = GameObject.Find("GameSelectionPanel");
                if (gameSelectionPanel != null)
                {
                    gameSelectionPanel.SetActive(true);
                    Debug.Log("GameSelectionPanel activé par nom");
                    return true;
                }

                Debug.LogWarning("Impossible de retourner au menu de sélection - Aucune méthode trouvée");
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors du retour au menu: {ex.Message}");
                return false;
            }
        }
    }
}
