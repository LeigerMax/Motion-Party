using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace UI.Team
{
    /// <summary>
    /// Interface de gestion de l'équipe - Section "Équipe" du menu principal
    /// Gère la création d'équipe, l'ajout de joueurs et l'affichage de la liste
    /// </summary>
    public class TeamManagementUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button addPlayerButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Transform playerListContent;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI playerCountText;

        [Header("Player Item")]
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private GameObject simplePlayerButtonPrefab; // Nouveau prefab simple

        [Header("Player Details")]
        [SerializeField] private GameObject playerDetailsPopupPrefab;

        [Header("Team Setup")]
        [SerializeField] private GameObject teamSetupPanel;
        [SerializeField] private TMP_InputField teamNameInput;
        [SerializeField] private Button confirmTeamButton;
        [SerializeField] private Button cancelTeamButton;

        [Header("Player Creation")]
        [SerializeField] private GameObject playerCreationPanel;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button confirmPlayerButton;
        [SerializeField] private Button cancelPlayerButton;

        [Header("Configuration")]
        [SerializeField] private bool debugMode = true; // Activé par défaut pour débugger

        [Header("Popup Animation")]
        [SerializeField] private CanvasGroup teamSetupCanvasGroup;
        [SerializeField] private CanvasGroup playerCreationCanvasGroup;
        [SerializeField] private float animationDuration = 0.3f;


        private List<GameObject> playerItems = new List<GameObject>();
        
        // Variables privées pour gestion du popup
        private PlayerDetailsPopup currentPopupInstance;

        private void Start()
        {
            // Forcer l'initialisation du PlayerProfileManager
            if (Systems.PlayerProfileManager.Instance == null)
            {
                Debug.LogError("TeamManagementUI: PlayerProfileManager non initialisé");
            }
            else if (debugMode)
            {
                Debug.Log("TeamManagementUI: PlayerProfileManager initialisé");
            }

            InitializeUI();
            CheckTeamSetup();
        }

        private void OnEnable()
        {
            // S'abonner aux événements
            if (Systems.TeamSetupManager.Instance != null)
            {
                Systems.TeamSetupManager.Instance.OnTeamNameChanged += OnTeamNameChanged;
                Systems.TeamSetupManager.Instance.OnPlayerAdded += OnPlayerAdded;
                Systems.TeamSetupManager.Instance.OnPlayerRemoved += OnPlayerRemoved;
            }
        }

        private void OnDisable()
        {
            // Se désabonner des événements
            if (Systems.TeamSetupManager.Instance != null)
            {
                Systems.TeamSetupManager.Instance.OnTeamNameChanged -= OnTeamNameChanged;
                Systems.TeamSetupManager.Instance.OnPlayerAdded -= OnPlayerAdded;
                Systems.TeamSetupManager.Instance.OnPlayerRemoved -= OnPlayerRemoved;
            }
            
            // Nettoyer le popup s'il existe
            if (currentPopupInstance != null)
            {
                Destroy(currentPopupInstance.gameObject);
                currentPopupInstance = null;
            }
        }

        private void InitializeUI()
        {
            // Configuration des boutons
            if (addPlayerButton != null)
                addPlayerButton.onClick.AddListener(OnAddPlayerClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            // Configuration team setup
            if (confirmTeamButton != null)
                confirmTeamButton.onClick.AddListener(OnConfirmTeamClicked);

            if (cancelTeamButton != null)
                cancelTeamButton.onClick.AddListener(OnCancelTeamClicked);

            // Configuration player creation
            if (confirmPlayerButton != null)
                confirmPlayerButton.onClick.AddListener(OnConfirmPlayerClicked);

            if (cancelPlayerButton != null)
                cancelPlayerButton.onClick.AddListener(OnCancelPlayerClicked);

            // Configurer les panels
            if (teamSetupPanel != null)
                teamSetupPanel.SetActive(false);

            if (playerCreationPanel != null)
                playerCreationPanel.SetActive(false);

            if (debugMode)
                Debug.Log("TeamManagementUI: Interface initialisée");
        }

        private void CheckTeamSetup()
        {
            if (!Systems.TeamSetupManager.Instance.IsTeamSetupDone())
            {
                ShowTeamSetup();
            }
            else
            {
                RefreshTeamDisplay();
            }
        }

        private void ShowTeamSetup()
        {
            if (teamSetupPanel != null)
            {
                teamSetupPanel.SetActive(true);
                
                // Préremplir avec le nom par défaut
                if (teamNameInput != null)
                {
                    teamNameInput.text = Systems.TeamSetupManager.Instance.GetTeamName();
                    teamNameInput.Select();
                }

                // Animation d'apparition
                if (teamSetupCanvasGroup != null)
                {
                    StartCoroutine(FadeInPopup(teamSetupCanvasGroup));
                }
            }
        }

        private void RefreshTeamDisplay()
        {
            // Mettre à jour le nom d'équipe
            if (teamNameText != null)
            {
                teamNameText.text = Systems.TeamSetupManager.Instance.GetTeamName();
            }

            // Mettre à jour le compteur
            UpdatePlayerCount();

            // Rafraîchir la liste des joueurs
            RefreshPlayerList();
        }

        private void UpdatePlayerCount()
        {
            if (playerCountText != null)
            {
                int count = Systems.TeamSetupManager.Instance.GetTeamPlayerCount();
                playerCountText.text = $"{count} joueur{(count > 1 ? "s" : "")}";
            }
        }

        private void RefreshPlayerList()
        {
            // Nettoyer la liste existante
            ClearPlayerList();

            // Vérifier que le PlayerProfileManager est initialisé
            if (Systems.PlayerProfileManager.Instance == null)
            {
                Debug.LogError("TeamManagementUI: PlayerProfileManager.Instance est null");
                return;
            }

            // Récupérer les joueurs
            var players = Systems.TeamSetupManager.Instance.GetTeamPlayers();

            if (debugMode)
                Debug.Log($"TeamManagementUI: Récupération de {players.Count} joueurs pour l'équipe '{Systems.TeamSetupManager.Instance.GetTeamName()}'");

            // Créer les items
            foreach (var player in players)
            {
                if (debugMode)
                    Debug.Log($"TeamManagementUI: Création item pour joueur {player.Nickname}");
                CreatePlayerItem(player);
            }

            if (debugMode)
                Debug.Log($"TeamManagementUI: Liste rafraîchie avec {players.Count} joueurs");
        }

        private void ClearPlayerList()
        {
            foreach (var item in playerItems)
            {
                if (item != null)
                    Destroy(item);
            }
            playerItems.Clear();
        }

        private void CreatePlayerItem(Systems.PlayerData player)
        {
            GameObject prefabToUse = simplePlayerButtonPrefab != null ? simplePlayerButtonPrefab : playerItemPrefab;
            
            if (prefabToUse == null)
            {
                Debug.LogError("TeamManagementUI: Aucun prefab assigné - vérifiez simplePlayerButtonPrefab ou playerItemPrefab dans l'Inspector");
                return;
            }

            if (playerListContent == null)
            {
                Debug.LogError("TeamManagementUI: playerListContent est null - vérifiez l'assignation dans l'Inspector");
                return;
            }

            if (debugMode)
                Debug.Log($"TeamManagementUI: Création d'un item pour {player.Nickname}");

            GameObject item = Instantiate(prefabToUse, playerListContent);
            playerItems.Add(item);

            // Utiliser le système de boutons simples en priorité
            if (simplePlayerButtonPrefab != null)
            {
                var simpleButton = item.GetComponent<SimplePlayerButton>();
                if (simpleButton == null)
                {
                    simpleButton = item.AddComponent<SimplePlayerButton>();
                }
                
                simpleButton.SetupPlayer(player, OnPlayerSelected);
                if (debugMode)
                    Debug.Log($"TeamManagementUI: Item configuré avec SimplePlayerButton pour {player.Nickname}");
            }
            else
            {
                // Fallback vers les anciens systèmes
                var teamPlayerItem = item.GetComponent<TeamPlayerItem>();
                if (teamPlayerItem != null)
                {
                    teamPlayerItem.SetupPlayer(player, OnEditPlayer, OnRemovePlayer);
                    if (debugMode)
                        Debug.Log($"TeamManagementUI: Item configuré avec TeamPlayerItem pour {player.Nickname}");
                }
                else
                {
                    var managementAdapter = item.GetComponent<PlayerManagementItemAdapter>();
                    if (managementAdapter == null)
                    {
                        managementAdapter = item.AddComponent<PlayerManagementItemAdapter>();
                    }
                    
                    managementAdapter.SetupPlayer(player, OnEditPlayer, OnRemovePlayer);
                    if (debugMode)
                        Debug.Log($"TeamManagementUI: Item configuré avec PlayerManagementItemAdapter pour {player.Nickname}");
                }
            }
        }

        #region Event Handlers

        private void OnAddPlayerClicked()
        {
            if (playerCreationPanel != null)
            {
                playerCreationPanel.SetActive(true);
                
                if (playerNameInput != null)
                {
                    playerNameInput.text = "";
                    playerNameInput.Select();
                }

                // Animation d'apparition
                if (playerCreationCanvasGroup != null)
                {
                    StartCoroutine(FadeInPopup(playerCreationCanvasGroup));
                }
            }
        }

        private void OnBackClicked()
        {
            // Retourner au menu principal
            var mainMenuController = FindFirstObjectByType<UI.Menu.MainMenuController>();
            if (mainMenuController != null)
            {
                mainMenuController.ShowMainMenu();
            }
            else
            {
                // Alternative : désactiver ce panel et chercher le parent
                if (debugMode)
                    Debug.LogWarning("TeamManagementUI: MainMenuController non trouvé, tentative alternative");
                
                // Désactiver ce panel
                gameObject.SetActive(false);
                
                // Chercher et activer le menu principal
                var mainMenuPanel = GameObject.Find("MainMenuPanel");
                if (mainMenuPanel != null)
                {
                    mainMenuPanel.SetActive(true);
                }
            }
        }

        private void OnConfirmTeamClicked()
        {
            if (teamNameInput != null)
            {
                string teamName = teamNameInput.text.Trim();
                if (!string.IsNullOrEmpty(teamName))
                {
                    Systems.TeamSetupManager.Instance.SetTeamName(teamName);
                    
                    // Animation de fermeture
                    if (teamSetupCanvasGroup != null)
                    {
                        StartCoroutine(FadeOutPopup(teamSetupCanvasGroup, () => {
                            if (teamSetupPanel != null)
                                teamSetupPanel.SetActive(false);
                        }));
                    }
                    else
                    {
                        if (teamSetupPanel != null)
                            teamSetupPanel.SetActive(false);
                    }

                    RefreshTeamDisplay();
                }
            }
        }

        private void OnCancelTeamClicked()
        {
            // Animation de fermeture
            if (teamSetupCanvasGroup != null)
            {
                StartCoroutine(FadeOutPopup(teamSetupCanvasGroup, () => {
                    if (teamSetupPanel != null)
                        teamSetupPanel.SetActive(false);

                    // Si pas de setup, retourner au menu principal
                    if (!Systems.TeamSetupManager.Instance.IsTeamSetupDone())
                    {
                        OnBackClicked();
                    }
                }));
            }
            else
            {
                if (teamSetupPanel != null)
                    teamSetupPanel.SetActive(false);

                // Si pas de setup, retourner au menu principal
                if (!Systems.TeamSetupManager.Instance.IsTeamSetupDone())
                {
                    OnBackClicked();
                }
            }
        }

        private void OnConfirmPlayerClicked()
        {
            if (playerNameInput != null)
            {
                string playerName = playerNameInput.text.Trim();
                if (!string.IsNullOrEmpty(playerName))
                {
                    if (debugMode)
                        Debug.Log($"TeamManagementUI: Tentative de création du joueur '{playerName}'");

                    var createdPlayer = Systems.TeamSetupManager.Instance.CreatePlayer(playerName);
                    
                    if (createdPlayer != null)
                    {
                        if (debugMode)
                            Debug.Log($"TeamManagementUI: Joueur créé avec succès : {createdPlayer.Nickname}");
                    }
                    else
                    {
                        Debug.LogError($"TeamManagementUI: Échec de la création du joueur '{playerName}'");
                    }
                    
                    // Animation de fermeture
                    if (playerCreationCanvasGroup != null)
                    {
                        StartCoroutine(FadeOutPopup(playerCreationCanvasGroup, () => {
                            if (playerCreationPanel != null)
                                playerCreationPanel.SetActive(false);
                        }));
                    }
                    else
                    {
                        if (playerCreationPanel != null)
                            playerCreationPanel.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogWarning("TeamManagementUI: Nom de joueur vide");
                }
            }
        }

        private void OnCancelPlayerClicked()
        {
            // Animation de fermeture
            if (playerCreationCanvasGroup != null)
            {
                StartCoroutine(FadeOutPopup(playerCreationCanvasGroup, () => {
                    if (playerCreationPanel != null)
                        playerCreationPanel.SetActive(false);
                }));
            }
            else
            {
                if (playerCreationPanel != null)
                    playerCreationPanel.SetActive(false);
            }
        }

        private void OnEditPlayer(Systems.PlayerData player)
        {
            // TODO: Implémenter l'édition (pour plus tard)
            if (debugMode)
                Debug.Log($"TeamManagementUI: Édition de {player.Nickname}");
        }

        private void OnRemovePlayer(Systems.PlayerData player)
        {
            // Confirmation simple
            if (Systems.TeamSetupManager.Instance.RemovePlayer(player.Id))
            {
                if (debugMode)
                    Debug.Log($"TeamManagementUI: Joueur supprimé : {player.Nickname}");
            }
        }

        private void OnTeamNameChanged(string newName)
        {
            RefreshTeamDisplay();
        }

        private void OnPlayerAdded(Systems.PlayerData player)
        {
            RefreshPlayerList();
            UpdatePlayerCount();
        }

        private void OnPlayerRemoved(Systems.PlayerData player)
        {
            RefreshPlayerList();
            UpdatePlayerCount();
        }

        #endregion

        public void ShowTeamSetupPopup()
        {
            ShowTeamSetup();
        }

        public void HideTeamSetupPopup()
        {
            if (teamSetupCanvasGroup != null)
            {
                StartCoroutine(FadeOutPopup(teamSetupCanvasGroup, () => {
                    if (teamSetupPanel != null)
                        teamSetupPanel.SetActive(false);
                }));
            }
            else
            {
                if (teamSetupPanel != null)
                    teamSetupPanel.SetActive(false);
            }
        }

        public void HidePlayerDetailsPopup()
        {
            if (currentPopupInstance != null)
            {
                Destroy(currentPopupInstance.gameObject);
                currentPopupInstance = null;
                
                if (debugMode)
                    Debug.Log("TeamManagementUI: Popup de détails fermé");
            }
        }

        private IEnumerator FadeInPopup(CanvasGroup canvasGroup)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float elapsed = 0f;
                
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
            }
        }

        private IEnumerator FadeOutPopup(CanvasGroup canvasGroup, System.Action onComplete)
        {
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 0f;
            }
            
            onComplete?.Invoke();
        }

        private void OnPlayerSelected(Systems.PlayerData player)
        {
            if (debugMode)
                Debug.Log($"TeamManagementUI: Joueur sélectionné : {player.Nickname}");

            // Instancier le popup dynamiquement
            if (playerDetailsPopupPrefab != null)
            {
                // Détruire l'instance précédente si elle existe
                if (currentPopupInstance != null)
                {
                    Destroy(currentPopupInstance.gameObject);
                }

                // Trouver le Canvas principal pour assurer le bon rendu
                Canvas mainCanvas = FindFirstObjectByType<Canvas>();
                Transform parentTransform = mainCanvas != null ? mainCanvas.transform : transform;

                // Créer une nouvelle instance dans le bon parent
                GameObject popupObject = Instantiate(playerDetailsPopupPrefab, parentTransform);
                currentPopupInstance = popupObject.GetComponent<PlayerDetailsPopup>();
                
                if (currentPopupInstance != null)
                {
                    // Assurer que le popup est au premier plan
                    popupObject.transform.SetAsLastSibling();
                    
                    // Vérifier la configuration RectTransform
                    var rectTransform = popupObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchorMin = Vector2.zero;
                        rectTransform.anchorMax = Vector2.one;
                        rectTransform.offsetMin = Vector2.zero;
                        rectTransform.offsetMax = Vector2.zero;
                    }
                    
                    currentPopupInstance.ShowPlayerDetails(player, OnEditPlayer, OnRemovePlayer);
                    
                    if (debugMode)
                        Debug.Log($"TeamManagementUI: Popup créé pour {player.Nickname} - Parent: {parentTransform.name} - Ordre: {popupObject.transform.GetSiblingIndex()}");
                }
                else
                {
                    Debug.LogError("TeamManagementUI: Le prefab PlayerDetailsPopup n'a pas de composant PlayerDetailsPopup");
                }
            }
            else
            {
                Debug.LogError("TeamManagementUI: playerDetailsPopupPrefab non assigné");
            }
        }

        // Méthode pour forcer le rafraîchissement avec délai
        private IEnumerator DelayedRefresh()
        {
            ClearPlayerList();
            yield return null; // Attendre une frame pour que la destruction soit effective
            
            var players = Systems.TeamSetupManager.Instance.GetTeamPlayers();
            foreach (var player in players)
            {
                CreatePlayerItem(player);
                yield return null; // Attendre entre chaque création
            }
            
            if (debugMode)
                Debug.Log($"TeamManagementUI: Rafraîchissement forcé terminé avec {players.Count} joueurs");
        }
        
        // Méthode publique pour forcer le rafraîchissement
        public void ForceRefreshPlayerList()
        {
            if (debugMode)
                Debug.Log("TeamManagementUI: Forçage du rafraîchissement de la liste");
            StartCoroutine(DelayedRefresh());
        }
    }
    
}
