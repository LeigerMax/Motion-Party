using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace UI.PlayerSelection
{
    /// <summary>
    /// Interface utilisateur pour la sélection des joueurs avant une partie
    /// </summary>
    public class PlayerSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI selectedCountText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button selectAllButton;
        [SerializeField] private Button deselectAllButton;

        [Header("Configuration")]
        [SerializeField] private int maxSelectedPlayers = 4;
        [SerializeField] private int minSelectedPlayers = 1;
        [SerializeField] private bool allowMultipleSelection = true;
        [SerializeField] private bool debugMode = false;

        // Données
        private List<Systems.PlayerData> allPlayers = new List<Systems.PlayerData>();
        private List<Systems.PlayerData> selectedPlayers = new List<Systems.PlayerData>();
        private List<PlayerSelectionItem> playerItems = new List<PlayerSelectionItem>();

        // Événements
        public System.Action<List<Systems.PlayerData>> OnPlayersSelected;
        public System.Action OnSelectionCancelled;

        private void Start()
        {
            InitializeUI();
            LoadPlayers();
        }

        private void InitializeUI()
        {
            // Configuration des boutons
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmSelection);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelSelection);
            
            if (selectAllButton != null)
                selectAllButton.onClick.AddListener(OnSelectAll);
            
            if (deselectAllButton != null)
                deselectAllButton.onClick.AddListener(OnDeselectAll);

            // Désactiver les boutons si pas de sélection multiple
            if (!allowMultipleSelection)
            {
                if (selectAllButton != null)
                    selectAllButton.gameObject.SetActive(false);
                if (deselectAllButton != null)
                    deselectAllButton.gameObject.SetActive(false);
            }

            UpdateUI();
        }

        /// <summary>
        /// Charge tous les joueurs disponibles
        /// </summary>
        private void LoadPlayers()
        {
            allPlayers = Systems.PlayerProfileManager.Instance.GetActivePlayers();
            CreatePlayerItems();
            UpdateUI();

            if (debugMode)
                Debug.Log($"Chargé {allPlayers.Count} joueurs actifs");
        }

        /// <summary>
        /// Recharge les joueurs (utile après ajout/suppression)
        /// </summary>
        public void RefreshPlayers()
        {
            ClearPlayerItems();
            LoadPlayers();
        }

        /// <summary>
        /// Crée les éléments UI pour chaque joueur
        /// </summary>
        private void CreatePlayerItems()
        {
            ClearPlayerItems();

            foreach (var player in allPlayers)
            {
                CreatePlayerItem(player);
            }
        }

        /// <summary>
        /// Crée un élément UI pour un joueur
        /// </summary>
        private void CreatePlayerItem(Systems.PlayerData player)
        {
            if (playerItemPrefab == null || playerListContainer == null) return;

            GameObject itemObject = Instantiate(playerItemPrefab, playerListContainer);
            PlayerSelectionItem item = itemObject.GetComponent<PlayerSelectionItem>();

            if (item == null)
            {
                item = itemObject.AddComponent<PlayerSelectionItem>();
            }

            item.Initialize(player, OnPlayerToggled);
            playerItems.Add(item);
        }

        /// <summary>
        /// Supprime tous les éléments UI
        /// </summary>
        private void ClearPlayerItems()
        {
            foreach (var item in playerItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            playerItems.Clear();
        }

        /// <summary>
        /// Callback quand un joueur est sélectionné/désélectionné
        /// </summary>
        private void OnPlayerToggled(Systems.PlayerData player, bool isSelected)
        {
            if (isSelected)
            {
                // Ajouter à la sélection
                if (!allowMultipleSelection)
                {
                    // Mode sélection unique: désélectionner tous les autres
                    selectedPlayers.Clear();
                    foreach (var item in playerItems)
                    {
                        if (item.PlayerData.Id != player.Id)
                        {
                            item.SetSelected(false);
                        }
                    }
                }

                if (selectedPlayers.Count < maxSelectedPlayers)
                {
                    if (!selectedPlayers.Contains(player))
                    {
                        selectedPlayers.Add(player);
                    }
                }
                else
                {
                    // Limite atteinte, désélectionner
                    var item = playerItems.FirstOrDefault(i => i.PlayerData.Id == player.Id);
                    if (item != null)
                    {
                        item.SetSelected(false);
                    }
                    
                    if (debugMode)
                        Debug.Log($"Limite de sélection atteinte ({maxSelectedPlayers})");
                }
            }
            else
            {
                // Retirer de la sélection
                selectedPlayers.Remove(player);
            }

            UpdateUI();
        }

        /// <summary>
        /// Met à jour l'interface utilisateur
        /// </summary>
        private void UpdateUI()
        {
            // Titre
            if (titleText != null)
            {
                string title = allowMultipleSelection ? 
                    $"Sélectionnez les joueurs ({minSelectedPlayers}-{maxSelectedPlayers})" :
                    "Sélectionnez un joueur";
                titleText.text = title;
            }

            // Compteur de sélection
            if (selectedCountText != null)
            {
                selectedCountText.text = $"Sélectionnés: {selectedPlayers.Count}/{maxSelectedPlayers}";
            }

            // Bouton de confirmation
            if (confirmButton != null)
            {
                bool canConfirm = selectedPlayers.Count >= minSelectedPlayers && 
                                 selectedPlayers.Count <= maxSelectedPlayers;
                confirmButton.interactable = canConfirm;
            }

            // Boutons de sélection multiple
            if (allowMultipleSelection)
            {
                if (selectAllButton != null)
                {
                    selectAllButton.interactable = selectedPlayers.Count < allPlayers.Count && 
                                                  allPlayers.Count <= maxSelectedPlayers;
                }

                if (deselectAllButton != null)
                {
                    deselectAllButton.interactable = selectedPlayers.Count > 0;
                }
            }
        }

        /// <summary>
        /// Confirme la sélection
        /// </summary>
        private void OnConfirmSelection()
        {
            if (selectedPlayers.Count < minSelectedPlayers || selectedPlayers.Count > maxSelectedPlayers)
            {
                if (debugMode)
                    Debug.LogWarning($"Sélection invalide: {selectedPlayers.Count} joueurs sélectionnés");
                return;
            }

            if (debugMode)
                Debug.Log($"Sélection confirmée: {selectedPlayers.Count} joueurs - Lancement du jeu");

            // Mettre à jour le GamePlayerSelector avec les joueurs sélectionnés
            if (PlayerSelectionAPIHelper.SafeClearPlayerSelection())
            {
                var gamePlayerSelector = Systems.GamePlayerSelector.Instance;
                if (gamePlayerSelector != null)
                {
                    foreach (var player in selectedPlayers)
                    {
                        gamePlayerSelector.AddPlayer(player);
                    }
                }
            }

            // Déclencher l'événement personnalisé si défini
            OnPlayersSelected?.Invoke(new List<Systems.PlayerData>(selectedPlayers));

            // Lancer le jeu directement
            LaunchGameWithSelectedPlayers();
        }

        /// <summary>
        /// Annule la sélection
        /// </summary>
        private void OnCancelSelection()
        {
            if (debugMode)
                Debug.Log("Sélection annulée - Retour au menu de sélection de jeu");

            // Déclencher l'événement personnalisé si défini
            OnSelectionCancelled?.Invoke();

            // Retourner au menu de sélection de jeu
            ReturnToGameSelection();
        }

        /// <summary>
        /// Lance le jeu avec les joueurs sélectionnés
        /// </summary>
        private void LaunchGameWithSelectedPlayers()
        {
            try
            {
                // Méthode 1: Utiliser GameSelectionController si disponible
                var gameSelectionController = FindFirstObjectByType<UI.Menu.GameSelectionController>();
                if (gameSelectionController != null)
                {
                    // Utiliser la réflexion pour appeler LaunchMiniGame (méthode privée)
                    var launchMethod = typeof(UI.Menu.GameSelectionController).GetMethod("LaunchMiniGame", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (launchMethod != null)
                    {
                        launchMethod.Invoke(gameSelectionController, null);
                        if (debugMode)
                            Debug.Log("Jeu lancé via GameSelectionController.LaunchMiniGame()");
                        return;
                    }
                }

                // Méthode 2: Utiliser GamePlayerSelector directement
                var gamePlayerSelector = Systems.GamePlayerSelector.Instance;
                if (gamePlayerSelector != null && gamePlayerSelector.StartGame())
                {
                    if (debugMode)
                        Debug.Log("Jeu lancé via GamePlayerSelector.StartGame()");
                    return;
                }

                // Méthode 3: Rechercher un GameSessionManager
                var gameSessionManager = FindFirstObjectByType<GameSessionManager>();
                if (gameSessionManager != null)
                {
                    gameSessionManager.StartGameSession();
                    if (debugMode)
                        Debug.Log("Jeu lancé via GameSessionManager.StartGameSession()");
                    return;
                }

                // Méthode 4: Rechercher un mini-jeu directement
                if (PlayerSelectionAPIHelper.SafeLaunchMiniGame(() => {
                    if (debugMode)
                        Debug.Log("Mini-jeu terminé");
                }))
                {
                    if (debugMode)
                        Debug.Log("Mini-jeu lancé directement via APIHelper");
                    return;
                }

                Debug.LogError("PlayerSelectionUI: Impossible de lancer le jeu - Aucun gestionnaire de jeu trouvé");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PlayerSelectionUI: Erreur lors du lancement du jeu: {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne au menu de sélection de jeu
        /// </summary>
        private void ReturnToGameSelection()
        {
            if (PlayerSelectionAPIHelper.SafeReturnToGameSelection())
            {
                if (debugMode)
                    Debug.Log("Retour au menu de sélection effectué via APIHelper");
                return;
            }

            // Fallback manuel si l'helper échoue
            try
            {
                Debug.LogWarning("PlayerSelectionUI: Fallback manual - tentative de désactivation du panel actuel");
                gameObject.SetActive(false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PlayerSelectionUI: Erreur lors du fallback: {ex.Message}");
            }
        }

        /// <summary>
        /// Sélectionne tous les joueurs
        /// </summary>
        private void OnSelectAll()
        {
            if (!allowMultipleSelection) return;

            selectedPlayers.Clear();
            int toSelect = Mathf.Min(allPlayers.Count, maxSelectedPlayers);

            for (int i = 0; i < toSelect; i++)
            {
                selectedPlayers.Add(allPlayers[i]);
                if (i < playerItems.Count)
                {
                    playerItems[i].SetSelected(true);
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// Désélectionne tous les joueurs
        /// </summary>
        private void OnDeselectAll()
        {
            selectedPlayers.Clear();
            
            foreach (var item in playerItems)
            {
                item.SetSelected(false);
            }

            UpdateUI();
        }

        /// <summary>
        /// Filtre les joueurs par équipe
        /// </summary>
        public void FilterByTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                // Afficher tous les joueurs
                allPlayers = Systems.PlayerProfileManager.Instance.GetActivePlayers();
            }
            else
            {
                // Filtrer par équipe
                allPlayers = Systems.PlayerProfileManager.Instance.GetPlayersByTeam(teamName);
            }

            CreatePlayerItems();
            UpdateUI();
        }

        /// <summary>
        /// Recherche de joueurs par nom
        /// </summary>
        public void SearchPlayers(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                allPlayers = Systems.PlayerProfileManager.Instance.GetActivePlayers();
            }
            else
            {
                allPlayers = Systems.PlayerProfileManager.Instance.GetActivePlayers()
                    .Where(p => p.Nickname.ToLower().Contains(searchTerm.ToLower()))
                    .ToList();
            }

            CreatePlayerItems();
            UpdateUI();
        }

        /// <summary>
        /// Configure les paramètres de sélection
        /// </summary>
        public void ConfigureSelection(int minPlayers, int maxPlayers, bool multipleSelection = true)
        {
            minSelectedPlayers = minPlayers;
            maxSelectedPlayers = maxPlayers;
            allowMultipleSelection = multipleSelection;

            InitializeUI();
            UpdateUI();
        }

        /// <summary>
        /// Retourne les joueurs actuellement sélectionnés
        /// </summary>
        public List<Systems.PlayerData> GetSelectedPlayers()
        {
            return new List<Systems.PlayerData>(selectedPlayers);
        }

        /// <summary>
        /// Pré-sélectionne des joueurs
        /// </summary>
        public void PreSelectPlayers(List<string> playerIds)
        {
            selectedPlayers.Clear();

            foreach (string playerId in playerIds)
            {
                var player = allPlayers.FirstOrDefault(p => p.Id == playerId);
                if (player != null && selectedPlayers.Count < maxSelectedPlayers)
                {
                    selectedPlayers.Add(player);
                    
                    var item = playerItems.FirstOrDefault(i => i.PlayerData.Id == playerId);
                    if (item != null)
                    {
                        item.SetSelected(true);
                    }
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// Configure les callbacks de navigation personnalisés
        /// </summary>
        public void SetNavigationCallbacks(System.Action onCancel = null, System.Action<List<Systems.PlayerData>> onConfirm = null)
        {
            if (onCancel != null)
                OnSelectionCancelled = onCancel;
            
            if (onConfirm != null)
                OnPlayersSelected = onConfirm;
        }

        /// <summary>
        /// Force le retour au menu de sélection de jeu (méthode publique)
        /// </summary>
        public void ForceReturnToGameSelection()
        {
            ReturnToGameSelection();
        }

        /// <summary>
        /// Force le lancement du jeu avec les joueurs actuellement sélectionnés (méthode publique)
        /// </summary>
        public void ForceLaunchGame()
        {
            if (selectedPlayers.Count >= minSelectedPlayers && selectedPlayers.Count <= maxSelectedPlayers)
            {
                LaunchGameWithSelectedPlayers();
            }
            else
            {
                Debug.LogWarning($"PlayerSelectionUI: Impossible de lancer le jeu - Sélection invalide ({selectedPlayers.Count} joueurs)");
            }
        }

        /// <summary>
        /// Affiche des informations de debug sur l'état actuel
        /// </summary>
        [ContextMenu("Debug Player Selection State")]
        public void DebugSelectionState()
        {
            Debug.Log($"=== PLAYER SELECTION DEBUG ===");
            Debug.Log($"Joueurs totaux: {allPlayers.Count}");
            Debug.Log($"Joueurs sélectionnés: {selectedPlayers.Count}/{maxSelectedPlayers}");
            Debug.Log($"Min requis: {minSelectedPlayers}");
            Debug.Log($"Sélection multiple: {allowMultipleSelection}");
            Debug.Log($"Items UI créés: {playerItems.Count}");
            
            Debug.Log("Joueurs sélectionnés:");
            foreach (var player in selectedPlayers)
            {
                Debug.Log($"  - {player.Nickname} (ID: {player.Id})");
            }

            Debug.Log("Systèmes disponibles:");
            PlayerSelectionAPIHelper.DebugAvailableSystems();
        }

        private void OnDestroy()
        {
            // Nettoyage des événements
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmSelection);
            
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelSelection);
            
            if (selectAllButton != null)
                selectAllButton.onClick.RemoveListener(OnSelectAll);
            
            if (deselectAllButton != null)
                deselectAllButton.onClick.RemoveListener(OnDeselectAll);
        }
    }
}
