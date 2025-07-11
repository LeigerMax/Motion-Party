using UnityEngine;
using System.Collections.Generic;

namespace UI.PlayerSelection
{
    /// <summary>
    /// Script de test et de debug pour le système de sélection des joueurs
    /// </summary>
    public class PlayerSelectionTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private PlayerSelectionUI playerSelectionUI;

        [Header("Test Players")]
        [SerializeField] private List<string> testPlayerNames = new List<string> 
        { 
            "Alice", 
            "Bob", 
            "Charlie", 
            "Diana" 
        };

        private void Start()
        {
            if (playerSelectionUI == null)
                playerSelectionUI = FindFirstObjectByType<PlayerSelectionUI>();

            if (debugMode)
            {
                Debug.Log("PlayerSelectionTester: Prêt pour les tests");
            }
        }

        [ContextMenu("Test - Create Test Players")]
        public void CreateTestPlayers()
        {
            if (Systems.PlayerProfileManager.Instance == null)
            {
                Debug.LogError("PlayerProfileManager.Instance est null");
                return;
            }

            Debug.Log("Création de joueurs de test...");

            foreach (var name in testPlayerNames)
            {
                try
                {
                    var existingPlayer = Systems.PlayerProfileManager.Instance.GetPlayerByNickname(name);
                    if (existingPlayer == null)
                    {
                        var player = Systems.PlayerProfileManager.Instance.CreatePlayer(name, "Équipe Test");
                        Debug.Log($"Joueur créé: {player.Nickname}");
                    }
                    else
                    {
                        Debug.Log($"Joueur déjà existant: {name}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Erreur lors de la création du joueur {name}: {ex.Message}");
                }
            }

            if (playerSelectionUI != null)
            {
                playerSelectionUI.RefreshPlayers();
                Debug.Log("Liste des joueurs rafraîchie");
            }
        }

        [ContextMenu("Test - Fix ScrollView")]
        public void TestFixScrollView()
        {
            if (playerSelectionUI != null)
            {
                var helper = playerSelectionUI.GetComponent<PlayerSelectionScrollViewHelper>();
                if (helper == null)
                {
                    helper = playerSelectionUI.gameObject.AddComponent<PlayerSelectionScrollViewHelper>();
                }
                helper.FixScrollViewConfiguration();
                Debug.Log("Configuration du ScrollView corrigée");
            }
            else
            {
                Debug.LogError("PlayerSelectionUI non trouvé");
            }
        }

        [ContextMenu("Test - Debug ScrollView")]
        public void TestDebugScrollView()
        {
            if (playerSelectionUI != null)
            {
                var helper = playerSelectionUI.GetComponent<PlayerSelectionScrollViewHelper>();
                if (helper == null)
                {
                    helper = playerSelectionUI.gameObject.AddComponent<PlayerSelectionScrollViewHelper>();
                }
                helper.DebugScrollViewInfo();
            }
            else
            {
                Debug.LogError("PlayerSelectionUI non trouvé");
            }
        }

        [ContextMenu("Test - Debug Selection State")]
        public void TestDebugSelectionState()
        {
            if (playerSelectionUI != null)
            {
                playerSelectionUI.DebugSelectionState();
            }
            else
            {
                Debug.LogError("PlayerSelectionUI non trouvé");
            }
        }

        [ContextMenu("Test - Simulate Cancel")]
        public void TestSimulateCancel()
        {
            if (playerSelectionUI != null)
            {
                playerSelectionUI.ForceReturnToGameSelection();
                Debug.Log("Test annulation effectué");
            }
            else
            {
                // Fallback avec l'helper
                if (PlayerSelectionAPIHelper.SafeReturnToGameSelection())
                {
                    Debug.Log("Test annulation effectué via APIHelper");
                }
                else
                {
                    Debug.LogError("PlayerSelectionUI non trouvé et aucune méthode de retour disponible");
                }
            }
        }

        [ContextMenu("Test - Simulate Confirm")]
        public void TestSimulateConfirm()
        {
            if (playerSelectionUI != null)
            {
                // Pré-sélectionner le premier joueur pour le test
                var allPlayers = Systems.PlayerProfileManager.Instance.GetActivePlayers();
                if (allPlayers.Count > 0)
                {
                    playerSelectionUI.PreSelectPlayers(new List<string> { allPlayers[0].Id });
                    playerSelectionUI.ForceLaunchGame();
                    Debug.Log("Test confirmation effectué avec premier joueur sélectionné");
                }
                else
                {
                    Debug.LogError("Aucun joueur disponible pour le test");
                }
            }
            else
            {
                Debug.LogError("PlayerSelectionUI non trouvé");
            }
        }

        [ContextMenu("Test - Configure All Items")]
        public void TestConfigureAllItems()
        {
            var allItems = FindObjectsByType<PlayerSelectionItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            Debug.Log($"Configuration de {allItems.Length} PlayerSelectionItem(s)...");
            
            foreach (var item in allItems)
            {
                var configurator = item.GetComponent<PlayerSelectionItemConfigurator>();
                if (configurator == null)
                {
                    configurator = item.gameObject.AddComponent<PlayerSelectionItemConfigurator>();
                }
                configurator.ConfigureItem();
            }
            
            Debug.Log("Configuration de tous les items terminée");
        }

        [ContextMenu("Test - Clear All Players")]
        public void TestClearAllPlayers()
        {
            if (Systems.PlayerProfileManager.Instance != null)
            {
                // Attention: ceci supprime TOUS les joueurs
                var allPlayers = Systems.PlayerProfileManager.Instance.GetAllPlayers();
                int removedCount = 0;
                
                foreach (var player in allPlayers)
                {
                    if (PlayerSelectionAPIHelper.SafeRemovePlayer(player.Id))
                    {
                        removedCount++;
                    }
                }
                
                if (playerSelectionUI != null)
                {
                    playerSelectionUI.RefreshPlayers();
                }
                
                Debug.Log($"{removedCount}/{allPlayers.Count} joueurs ont été supprimés");
            }
        }

        [ContextMenu("Test - Check Navigation Systems")]
        public void TestCheckNavigationSystems()
        {
            PlayerSelectionAPIHelper.DebugAvailableSystems();
        }
    }
}
