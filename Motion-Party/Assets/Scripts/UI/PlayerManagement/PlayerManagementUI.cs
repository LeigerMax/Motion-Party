using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Systems;

namespace UI.PlayerManagement
{
    /// <summary>
    /// Interface utilisateur pour la gestion des joueurs (création, modification, suppression)
    /// </summary>
    public class PlayerManagementUI : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject createPlayerPanel;
        [SerializeField] private GameObject editPlayerPanel;
        [SerializeField] private GameObject playerListPanel;

        [Header("Create Player Panel")]
        [SerializeField] private TMP_InputField createNicknameInput;
        [SerializeField] private TMP_Dropdown createTeamDropdown;
        [SerializeField] private TMP_InputField createNewTeamInput;
        [SerializeField] private Button createConfirmButton;
        [SerializeField] private Button createCancelButton;

        [Header("Edit Player Panel")]
        [SerializeField] private TMP_InputField editNicknameInput;
        [SerializeField] private TMP_Dropdown editTeamDropdown;
        [SerializeField] private TMP_InputField editNewTeamInput;
        [SerializeField] private Button editConfirmButton;
        [SerializeField] private Button editCancelButton;
        [SerializeField] private Button deletePlayerButton;

        [Header("Player List")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private ScrollRect playerScrollRect;
        [SerializeField] private TextMeshProUGUI playerCountText;

        [Header("Navigation")]
        [SerializeField] private Button addPlayerButton;
        [SerializeField] private Button backButton;

        [Header("Search & Filter")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Dropdown teamFilterDropdown;
        [SerializeField] private Button clearFiltersButton;

        [Header("Configuration")]
        [SerializeField] private bool debugMode = false;

        // Données
        private List<PlayerData> displayedPlayers = new List<PlayerData>();
        private List<PlayerManagementItem> playerItems = new List<PlayerManagementItem>();
        private PlayerData currentEditingPlayer = null;
        private string searchTerm = "";
        private string selectedTeamFilter = "";

        private void Start()
        {
            InitializeUI();
            RefreshPlayerList();
            RefreshTeamDropdowns();
        }

        private void InitializeUI()
        {
            // Configuration des boutons principaux
            if (addPlayerButton != null)
                addPlayerButton.onClick.AddListener(ShowCreatePlayerPanel);
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBack);

            // Création de joueur
            if (createConfirmButton != null)
                createConfirmButton.onClick.AddListener(OnCreatePlayer);
            
            if (createCancelButton != null)
                createCancelButton.onClick.AddListener(ShowMainPanel);

            // Édition de joueur
            if (editConfirmButton != null)
                editConfirmButton.onClick.AddListener(OnEditPlayer);
            
            if (editCancelButton != null)
                editCancelButton.onClick.AddListener(ShowMainPanel);
            
            if (deletePlayerButton != null)
                deletePlayerButton.onClick.AddListener(OnDeletePlayer);

            // Recherche et filtres
            if (searchInput != null)
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            
            if (teamFilterDropdown != null)
                teamFilterDropdown.onValueChanged.AddListener(OnTeamFilterChanged);
            
            if (clearFiltersButton != null)
                clearFiltersButton.onClick.AddListener(OnClearFilters);

            // Validation en temps réel
            if (createNicknameInput != null)
                createNicknameInput.onValueChanged.AddListener(_ => ValidateCreateForm());
            
            if (editNicknameInput != null)
                editNicknameInput.onValueChanged.AddListener(_ => ValidateEditForm());

            ShowMainPanel();
        }

        #region Panel Management

        /// <summary>
        /// Affiche le panneau principal
        /// </summary>
        private void ShowMainPanel()
        {
            SetActivePanel(mainPanel);
            RefreshPlayerList();
        }

        /// <summary>
        /// Affiche le panneau de création de joueur
        /// </summary>
        private void ShowCreatePlayerPanel()
        {
            SetActivePanel(createPlayerPanel);
            ClearCreateForm();
            RefreshTeamDropdowns();
        }

        /// <summary>
        /// Affiche le panneau d'édition de joueur
        /// </summary>
        private void ShowEditPlayerPanel(PlayerData player)
        {
            SetActivePanel(editPlayerPanel);
            currentEditingPlayer = player;
            PopulateEditForm(player);
            RefreshTeamDropdowns();
        }

        /// <summary>
        /// Active un panneau spécifique
        /// </summary>
        private void SetActivePanel(GameObject activePanel)
        {
            if (mainPanel != null) mainPanel.SetActive(mainPanel == activePanel);
            if (createPlayerPanel != null) createPlayerPanel.SetActive(createPlayerPanel == activePanel);
            if (editPlayerPanel != null) editPlayerPanel.SetActive(editPlayerPanel == activePanel);
        }

        #endregion

        #region Player Creation

        /// <summary>
        /// Efface le formulaire de création
        /// </summary>
        private void ClearCreateForm()
        {
            if (createNicknameInput != null)
                createNicknameInput.text = "";
            
            if (createTeamDropdown != null)
                createTeamDropdown.value = 0;
            
            if (createNewTeamInput != null)
                createNewTeamInput.text = "";

            ValidateCreateForm();
        }

        /// <summary>
        /// Valide le formulaire de création
        /// </summary>
        private void ValidateCreateForm()
        {
            bool isValid = true;

            // Vérifier le pseudo
            if (createNicknameInput != null && createConfirmButton != null)
            {
                string nickname = createNicknameInput.text.Trim();
                isValid = !string.IsNullOrEmpty(nickname) && 
                         PlayerProfileManager.Instance.IsNicknameAvailable(nickname);
            }

            if (createConfirmButton != null)
                createConfirmButton.interactable = isValid;
        }

        /// <summary>
        /// Crée un nouveau joueur
        /// </summary>
        private void OnCreatePlayer()
        {
            if (createNicknameInput == null) return;

            string nickname = createNicknameInput.text.Trim();
            if (string.IsNullOrEmpty(nickname)) return;

            // Déterminer l'équipe
            string teamName = "";
            if (createTeamDropdown != null && createTeamDropdown.value > 0)
            {
                // Équipe existante sélectionnée
                teamName = createTeamDropdown.options[createTeamDropdown.value].text;
            }
            else if (createNewTeamInput != null && !string.IsNullOrEmpty(createNewTeamInput.text.Trim()))
            {
                // Nouvelle équipe
                teamName = createNewTeamInput.text.Trim();
            }

            // Créer le joueur
            PlayerData newPlayer = PlayerProfileManager.Instance.CreatePlayer(nickname, teamName);
            
            if (newPlayer != null)
            {
                if (debugMode)
                    Debug.Log($"Joueur créé avec succès: {newPlayer}");
                
                ShowMainPanel();
            }
            else
            {
                if (debugMode)
                    Debug.LogError("Échec de la création du joueur");
            }
        }

        #endregion

        #region Player Editing

        /// <summary>
        /// Remplit le formulaire d'édition
        /// </summary>
        private void PopulateEditForm(PlayerData player)
        {
            if (editNicknameInput != null)
                editNicknameInput.text = player.Nickname;

            // Sélectionner l'équipe actuelle dans le dropdown
            if (editTeamDropdown != null)
            {
                int teamIndex = 0;
                if (!string.IsNullOrEmpty(player.TeamName))
                {
                    for (int i = 0; i < editTeamDropdown.options.Count; i++)
                    {
                        if (editTeamDropdown.options[i].text == player.TeamName)
                        {
                            teamIndex = i;
                            break;
                        }
                    }
                }
                editTeamDropdown.value = teamIndex;
            }

            if (editNewTeamInput != null)
                editNewTeamInput.text = "";

            ValidateEditForm();
        }

        /// <summary>
        /// Valide le formulaire d'édition
        /// </summary>
        private void ValidateEditForm()
        {
            bool isValid = true;

            if (editNicknameInput != null && editConfirmButton != null && currentEditingPlayer != null)
            {
                string nickname = editNicknameInput.text.Trim();
                isValid = !string.IsNullOrEmpty(nickname) && 
                         (nickname == currentEditingPlayer.Nickname || 
                          PlayerProfileManager.Instance.IsNicknameAvailable(nickname));
            }

            if (editConfirmButton != null)
                editConfirmButton.interactable = isValid;
        }

        /// <summary>
        /// Édite le joueur
        /// </summary>
        private void OnEditPlayer()
        {
            if (currentEditingPlayer == null || editNicknameInput == null) return;

            string nickname = editNicknameInput.text.Trim();
            if (string.IsNullOrEmpty(nickname)) return;

            // Déterminer l'équipe
            string teamName = "";
            if (editTeamDropdown != null && editTeamDropdown.value > 0)
            {
                teamName = editTeamDropdown.options[editTeamDropdown.value].text;
            }
            else if (editNewTeamInput != null && !string.IsNullOrEmpty(editNewTeamInput.text.Trim()))
            {
                teamName = editNewTeamInput.text.Trim();
            }

            // Mettre à jour le joueur
            bool success = PlayerProfileManager.Instance.UpdatePlayer(
                currentEditingPlayer.Id, 
                nickname, 
                teamName
            );

            if (success)
            {
                if (debugMode)
                    Debug.Log($"Joueur mis à jour avec succès: {currentEditingPlayer}");
                
                ShowMainPanel();
            }
            else
            {
                if (debugMode)
                    Debug.LogError("Échec de la mise à jour du joueur");
            }
        }

        /// <summary>
        /// Supprime le joueur
        /// </summary>
        private void OnDeletePlayer()
        {
            if (currentEditingPlayer == null) return;

            // Confirmation simple (peut être améliorée avec un dialogue)
            bool confirmed = true; // Pour l'instant, pas de dialogue de confirmation

            if (confirmed)
            {
                bool success = PlayerProfileManager.Instance.RemovePlayer(currentEditingPlayer.Id);
                
                if (success)
                {
                    if (debugMode)
                        Debug.Log($"Joueur supprimé avec succès: {currentEditingPlayer}");
                    
                    ShowMainPanel();
                }
                else
                {
                    if (debugMode)
                        Debug.LogError("Échec de la suppression du joueur");
                }
            }
        }

        #endregion

        #region Player List Management

        /// <summary>
        /// Rafraîchit la liste des joueurs
        /// </summary>
        private void RefreshPlayerList()
        {
            // Récupérer tous les joueurs
            var allPlayers = PlayerProfileManager.Instance.GetAllPlayers();

            // Appliquer les filtres
            displayedPlayers = FilterPlayers(allPlayers);

            // Créer les éléments UI
            CreatePlayerItems();

            // Mettre à jour le compteur
            UpdatePlayerCount();
        }

        /// <summary>
        /// Filtre les joueurs selon les critères
        /// </summary>
        private List<PlayerData> FilterPlayers(List<PlayerData> players)
        {
            var filtered = new List<PlayerData>(players);

            // Filtre par recherche
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filtered = filtered.FindAll(p => 
                    p.Nickname.ToLower().Contains(searchTerm.ToLower()) ||
                    p.TeamName.ToLower().Contains(searchTerm.ToLower())
                );
            }

            // Filtre par équipe
            if (!string.IsNullOrEmpty(selectedTeamFilter))
            {
                filtered = filtered.FindAll(p => p.TeamName == selectedTeamFilter);
            }

            return filtered;
        }

        /// <summary>
        /// Crée les éléments UI pour chaque joueur
        /// </summary>
        private void CreatePlayerItems()
        {
            ClearPlayerItems();

            foreach (var player in displayedPlayers)
            {
                CreatePlayerItem(player);
            }
        }

        /// <summary>
        /// Crée un élément UI pour un joueur
        /// </summary>
        private void CreatePlayerItem(PlayerData player)
        {
            if (playerItemPrefab == null || playerListContainer == null) return;

            GameObject itemObject = Instantiate(playerItemPrefab, playerListContainer);
            PlayerManagementItem item = itemObject.GetComponent<PlayerManagementItem>();

            if (item == null)
            {
                item = itemObject.AddComponent<PlayerManagementItem>();
            }

            item.Initialize(player, OnEditPlayerClicked);
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
        /// Met à jour le compteur de joueurs
        /// </summary>
        private void UpdatePlayerCount()
        {
            if (playerCountText != null)
            {
                int total = PlayerProfileManager.Instance.GetPlayerCount();
                playerCountText.text = $"Joueurs: {displayedPlayers.Count}/{total}";
            }
        }

        #endregion

        #region Search & Filter

        /// <summary>
        /// Callback de changement de recherche
        /// </summary>
        private void OnSearchChanged(string newSearchTerm)
        {
            searchTerm = newSearchTerm;
            RefreshPlayerList();
        }

        /// <summary>
        /// Callback de changement de filtre d'équipe
        /// </summary>
        private void OnTeamFilterChanged(int index)
        {
            if (teamFilterDropdown != null && index >= 0 && index < teamFilterDropdown.options.Count)
            {
                selectedTeamFilter = index == 0 ? "" : teamFilterDropdown.options[index].text;
                RefreshPlayerList();
            }
        }

        /// <summary>
        /// Efface tous les filtres
        /// </summary>
        private void OnClearFilters()
        {
            searchTerm = "";
            selectedTeamFilter = "";

            if (searchInput != null)
                searchInput.text = "";
            
            if (teamFilterDropdown != null)
                teamFilterDropdown.value = 0;

            RefreshPlayerList();
        }

        #endregion

        #region Team Dropdown Management

        /// <summary>
        /// Rafraîchit les dropdowns d'équipes
        /// </summary>
        private void RefreshTeamDropdowns()
        {
            var teamNames = PlayerProfileManager.Instance.GetTeamNames();
            
            RefreshTeamDropdown(createTeamDropdown, teamNames);
            RefreshTeamDropdown(editTeamDropdown, teamNames);
            RefreshTeamFilterDropdown(teamNames);
        }

        /// <summary>
        /// Rafraîchit un dropdown d'équipe
        /// </summary>
        private void RefreshTeamDropdown(TMP_Dropdown dropdown, List<string> teamNames)
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            
            // Option "Aucune équipe"
            options.Add(new TMP_Dropdown.OptionData("Aucune équipe"));
            
            // Ajouter les équipes existantes
            foreach (string teamName in teamNames)
            {
                options.Add(new TMP_Dropdown.OptionData(teamName));
            }

            dropdown.AddOptions(options);
        }

        /// <summary>
        /// Rafraîchit le dropdown de filtre d'équipe
        /// </summary>
        private void RefreshTeamFilterDropdown(List<string> teamNames)
        {
            if (teamFilterDropdown == null) return;

            teamFilterDropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            
            // Option "Toutes les équipes"
            options.Add(new TMP_Dropdown.OptionData("Toutes les équipes"));
            
            // Ajouter les équipes existantes
            foreach (string teamName in teamNames)
            {
                options.Add(new TMP_Dropdown.OptionData(teamName));
            }

            teamFilterDropdown.AddOptions(options);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Callback quand un joueur est cliqué pour édition
        /// </summary>
        private void OnEditPlayerClicked(PlayerData player)
        {
            ShowEditPlayerPanel(player);
        }

        /// <summary>
        /// Callback du bouton retour
        /// </summary>
        private void OnBack()
        {
            // Retour à l'écran précédent (peut être configuré)
            if (debugMode)
                Debug.Log("Retour depuis la gestion des joueurs");
        }

        #endregion

        private void OnDestroy()
        {
            // Nettoyage des événements
            if (addPlayerButton != null)
                addPlayerButton.onClick.RemoveAllListeners();
            if (createConfirmButton != null)
                createConfirmButton.onClick.RemoveAllListeners();
            if (editConfirmButton != null)
                editConfirmButton.onClick.RemoveAllListeners();
            // ... autres nettoyages
        }
    }
}
