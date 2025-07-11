using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Script de test pour diagnostiquer les problèmes avec le système Team
    /// Attachez ce script à un GameObject et cliquez sur les boutons dans l'Inspector
    /// </summary>
    public class TeamSystemDebugger : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private Button testCreatePlayerButton;
        [SerializeField] private Button testListPlayersButton;
        [SerializeField] private Button testResetTeamButton;
        [SerializeField] private TMP_InputField testPlayerNameInput;
        [SerializeField] private TextMeshProUGUI debugOutputText;

        [Header("Configuration")]
        [SerializeField] private bool autoInitialize = true;

        private void Start()
        {
            if (autoInitialize)
            {
                InitializeDebugger();
            }
        }

        private void InitializeDebugger()
        {
            if (testCreatePlayerButton != null)
                testCreatePlayerButton.onClick.AddListener(TestCreatePlayer);

            if (testListPlayersButton != null)
                testListPlayersButton.onClick.AddListener(TestListPlayers);

            if (testResetTeamButton != null)
                testResetTeamButton.onClick.AddListener(TestResetTeam);

            LogDebug("Debugger initialisé");
        }

        [ContextMenu("Test - Create Player")]
        public void TestCreatePlayer()
        {
            string playerName = testPlayerNameInput != null ? testPlayerNameInput.text : "TestPlayer";
            
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "TestPlayer" + Random.Range(1, 100);
            }

            LogDebug($"=== TEST CREATE PLAYER: {playerName} ===");

            // Vérifier les instances
            if (Systems.TeamSetupManager.Instance == null)
            {
                LogDebug("❌ TeamSetupManager.Instance est NULL");
                return;
            }
            LogDebug("✅ TeamSetupManager.Instance OK");

            if (Systems.PlayerProfileManager.Instance == null)
            {
                LogDebug("❌ PlayerProfileManager.Instance est NULL");
                return;
            }
            LogDebug("✅ PlayerProfileManager.Instance OK");

            // Vérifier le nom d'équipe
            string teamName = Systems.TeamSetupManager.Instance.GetTeamName();
            LogDebug($"Nom d'équipe: '{teamName}'");

            // Créer le joueur
            var player = Systems.TeamSetupManager.Instance.CreatePlayer(playerName);
            if (player != null)
            {
                LogDebug($"✅ Joueur créé: {player.Nickname} (ID: {player.Id})");
                LogDebug($"   Équipe: {player.TeamName}");
            }
            else
            {
                LogDebug($"❌ Échec de création du joueur: {playerName}");
            }
        }

        [ContextMenu("Test - List Players")]
        public void TestListPlayers()
        {
            LogDebug("=== TEST LIST PLAYERS ===");

            if (Systems.TeamSetupManager.Instance == null)
            {
                LogDebug("❌ TeamSetupManager.Instance est NULL");
                return;
            }

            string teamName = Systems.TeamSetupManager.Instance.GetTeamName();
            LogDebug($"Équipe: '{teamName}'");

            var players = Systems.TeamSetupManager.Instance.GetTeamPlayers();
            LogDebug($"Nombre de joueurs: {players.Count}");

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                LogDebug($"  {i + 1}. {player.Nickname} (ID: {player.Id}, Équipe: {player.TeamName})");
            }

            // Vérifier aussi via PlayerProfileManager
            var allPlayers = Systems.PlayerProfileManager.Instance.GetPlayersByTeam(teamName);
            LogDebug($"Via PlayerProfileManager: {allPlayers.Count} joueurs");
        }

        [ContextMenu("Test - Reset Team")]
        public void TestResetTeam()
        {
            LogDebug("=== TEST RESET TEAM ===");

            if (Systems.TeamSetupManager.Instance != null)
            {
                Systems.TeamSetupManager.Instance.ResetTeamSetup();
                LogDebug("✅ Équipe réinitialisée");
            }
            else
            {
                LogDebug("❌ TeamSetupManager.Instance est NULL");
            }
        }

        [ContextMenu("Test - Check UI References")]
        public void TestCheckUIReferences()
        {
            LogDebug("=== TEST UI REFERENCES ===");

            var teamUI = FindFirstObjectByType<TeamManagementUI>();
            if (teamUI != null)
            {
                LogDebug("✅ TeamManagementUI trouvé");
            }
            else
            {
                LogDebug("❌ TeamManagementUI NON trouvé");
            }

            var mainMenuController = FindFirstObjectByType<UI.Menu.MainMenuController>();
            if (mainMenuController != null)
            {
                LogDebug("✅ MainMenuController trouvé");
            }
            else
            {
                LogDebug("❌ MainMenuController NON trouvé");
            }
        }

        [ContextMenu("Count Player Items")]
        public void CountPlayerItems()
        {
            var teamUI = FindFirstObjectByType<TeamManagementUI>();
            if (teamUI != null)
            {
                // Accéder au playerListContent via réflexion ou méthode publique
                var contentField = typeof(TeamManagementUI).GetField("playerListContent", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (contentField != null)
                {
                    var content = contentField.GetValue(teamUI) as Transform;
                    if (content != null)
                    {
                        LogDebug($"Nombre d'enfants dans le Content: {content.childCount}");
                        for (int i = 0; i < content.childCount; i++)
                        {
                            var child = content.GetChild(i);
                            var simpleButton = child.GetComponent<SimplePlayerButton>();
                            string playerName = simpleButton != null ? simpleButton.PlayerName : "Unknown";
                            LogDebug($"Enfant {i}: {child.name} - Joueur: {playerName}");
                        }
                    }
                    else
                    {
                        LogDebug("Content est null");
                    }
                }
                else
                {
                    LogDebug("Impossible d'accéder au playerListContent");
                }
            }
            else
            {
                LogDebug("TeamManagementUI introuvable");
            }
        }

        [ContextMenu("Force Refresh Player List")]
        public void ForceRefreshPlayerList()
        {
            var teamUI = FindFirstObjectByType<TeamManagementUI>();
            if (teamUI != null)
            {
                teamUI.ForceRefreshPlayerList();
                LogDebug("Rafraîchissement forcé de la liste des joueurs");
            }
            else
            {
                LogDebug("TeamManagementUI introuvable");
            }
        }

        [ContextMenu("Check ScrollView Configuration")]
        public void CheckScrollViewConfiguration()
        {
            var teamUI = FindFirstObjectByType<TeamManagementUI>();
            if (teamUI != null)
            {
                var contentField = typeof(TeamManagementUI).GetField("playerListContent", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (contentField != null)
                {
                    var content = contentField.GetValue(teamUI) as Transform;
                    if (content != null)
                    {
                        var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
                        var contentSizeFitter = content.GetComponent<ContentSizeFitter>();
                        
                        LogDebug($"Content: {content.name}");
                        LogDebug($"Vertical Layout Group: {verticalLayout != null}");
                        LogDebug($"Content Size Fitter: {contentSizeFitter != null}");
                        LogDebug($"Nombre d'enfants: {content.childCount}");
                        
                        for (int i = 0; i < content.childCount; i++)
                        {
                            var child = content.GetChild(i);
                            var rect = child.GetComponent<RectTransform>();
                            LogDebug($"Enfant {i}: {child.name} - Position: {rect.anchoredPosition}, Taille: {rect.sizeDelta}");
                        }
                    }
                }
            }
        }

        [ContextMenu("Check Popup Visibility")]
        public void CheckPopupVisibility()
        {
            var popup = FindFirstObjectByType<PlayerDetailsPopup>();
            if (popup != null)
            {
                LogDebug($"Popup trouvé: {popup.name}");
                LogDebug($"Popup actif: {popup.gameObject.activeInHierarchy}");
                
                var canvasGroup = popup.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    LogDebug($"CanvasGroup Alpha: {canvasGroup.alpha}");
                    LogDebug($"CanvasGroup Interactable: {canvasGroup.interactable}");
                    LogDebug($"CanvasGroup Blocks Raycasts: {canvasGroup.blocksRaycasts}");
                }
                
                var rectTransform = popup.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    LogDebug($"RectTransform Position: {rectTransform.anchoredPosition}");
                    LogDebug($"RectTransform Size: {rectTransform.sizeDelta}");
                }
            }
            else
            {
                LogDebug("Aucun popup PlayerDetailsPopup trouvé");
            }
        }

        private void LogDebug(string message)
        {
            Debug.Log($"[TeamSystemDebugger] {message}");
            
            if (debugOutputText != null)
            {
                debugOutputText.text += $"{message}\n";
            }
        }

        [ContextMenu("Clear Debug Output")]
        public void ClearDebugOutput()
        {
            if (debugOutputText != null)
            {
                debugOutputText.text = "";
            }
        }
    }
}
