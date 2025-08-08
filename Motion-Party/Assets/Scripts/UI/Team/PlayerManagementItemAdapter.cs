using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Adaptateur pour le prefab PlayerManagementItem existant
    /// Permet d'utiliser le prefab existant avec le système Team
    /// </summary>
    public class PlayerManagementItemAdapter : MonoBehaviour
    {
        [Header("Auto-Detection")]
        [SerializeField] private bool autoDetectComponents = true;
        [SerializeField] private bool debugMode = true;

        [Header("UI References (assignées automatiquement)")]
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button editButton;
        [SerializeField] private Button statusButton;
        [SerializeField] private Image backgroundImage;

        private Systems.PlayerData playerData;
        private System.Action<Systems.PlayerData> onEditCallback;
        private System.Action<Systems.PlayerData> onRemoveCallback;

        private void Awake()
        {
            if (autoDetectComponents)
            {
                AutoDetectComponents();
            }
        }

        private void AutoDetectComponents()
        {
            // Récupérer tous les TextMeshPro du prefab
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
            
            if (debugMode)
                Debug.Log($"PlayerManagementItemAdapter: Détection de {allTexts.Length} composants TextMeshPro");

            // Assigner automatiquement selon les noms ou positions
            for (int i = 0; i < allTexts.Length; i++)
            {
                var text = allTexts[i];
                string name = text.name.ToLower();
                
                if (debugMode)
                    Debug.Log($"  - TextMeshPro [{i}]: {text.name} = '{text.text}'");

                // Détecter le nickname (généralement le premier ou celui avec "nickname/name")
                if (nicknameText == null && (name.Contains("nickname") || name.Contains("name") || i == 0))
                {
                    nicknameText = text;
                    if (debugMode)
                        Debug.Log($"    → Assigné comme nicknameText");
                }
                // Détecter le team name
                else if (teamNameText == null && name.Contains("team"))
                {
                    teamNameText = text;
                    if (debugMode)
                        Debug.Log($"    → Assigné comme teamNameText");
                }
                // Détecter les stats
                else if (statsText == null && (name.Contains("stats") || name.Contains("score") || i == 2))
                {
                    statsText = text;
                    if (debugMode)
                        Debug.Log($"    → Assigné comme statsText");
                }
            }

            // Récupérer les boutons
            var allButtons = GetComponentsInChildren<Button>();
            if (debugMode)
                Debug.Log($"PlayerManagementItemAdapter: Détection de {allButtons.Length} boutons");

            for (int i = 0; i < allButtons.Length; i++)
            {
                var button = allButtons[i];
                if (debugMode)
                    Debug.Log($"  - Button [{i}]: {button.name}");

                if (editButton == null && i == 0)
                {
                    editButton = button;
                    if (debugMode)
                        Debug.Log($"    → Assigné comme editButton");
                }
                else if (statusButton == null && i == 1)
                {
                    statusButton = button;
                    if (debugMode)
                        Debug.Log($"    → Assigné comme statusButton");
                }
            }

            // Récupérer l'image de fond
            backgroundImage = GetComponent<Image>();
            if (backgroundImage != null && debugMode)
                Debug.Log($"PlayerManagementItemAdapter: backgroundImage assignée");
        }

        public void SetupPlayer(Systems.PlayerData player, System.Action<Systems.PlayerData> onEdit, System.Action<Systems.PlayerData> onRemove)
        {
            playerData = player;
            onEditCallback = onEdit;
            onRemoveCallback = onRemove;

            UpdateDisplay();
            SetupInteractions();
        }

        private void UpdateDisplay()
        {
            if (playerData == null) return;

            // Mettre à jour les textes
            if (nicknameText != null)
            {
                nicknameText.text = playerData.Nickname;
                if (debugMode)
                    Debug.Log($"PlayerManagementItemAdapter: Nickname mis à jour → {playerData.Nickname}");
            }

            if (teamNameText != null)
            {
                teamNameText.text = playerData.TeamName;
                if (debugMode)
                    Debug.Log($"PlayerManagementItemAdapter: TeamName mis à jour → {playerData.TeamName}");
            }

            if (statsText != null)
            {
                string statsDisplay = $"{playerData.GamesPlayed} parties";
                if (playerData.TotalScore > 0)
                    statsDisplay += $" • {playerData.TotalScore} pts";
                
                statsText.text = statsDisplay;
                if (debugMode)
                    Debug.Log($"PlayerManagementItemAdapter: Stats mis à jour → {statsDisplay}");
            }
        }

        private void SetupInteractions()
        {
            // Configurer le bouton d'édition
            if (editButton != null)
            {
                editButton.onClick.RemoveAllListeners();
                editButton.onClick.AddListener(OnEditClicked);
                if (debugMode)
                    Debug.Log($"PlayerManagementItemAdapter: Bouton d'édition configuré");
            }

            // Configurer le bouton de statut (réutilisé pour supprimer)
            if (statusButton != null)
            {
                statusButton.onClick.RemoveAllListeners();
                statusButton.onClick.AddListener(OnRemoveClicked);
                if (debugMode)
                    Debug.Log($"PlayerManagementItemAdapter: Bouton de suppression configuré");
            }
        }

        private void OnEditClicked()
        {
            if (debugMode)
                Debug.Log($"PlayerManagementItemAdapter: Édition demandée pour {playerData?.Nickname}");
            onEditCallback?.Invoke(playerData);
        }

        private void OnRemoveClicked()
        {
            if (debugMode)
                Debug.Log($"PlayerManagementItemAdapter: Suppression demandée pour {playerData?.Nickname}");
            onRemoveCallback?.Invoke(playerData);
        }

        [ContextMenu("Debug - Show Component Info")]
        public void DebugShowComponentInfo()
        {
            Debug.Log("=== PLAYER MANAGEMENT ITEM ADAPTER DEBUG ===");
            Debug.Log($"Player Data: {(playerData != null ? playerData.Nickname : "NULL")}");
            Debug.Log($"NicknameText: {(nicknameText != null ? nicknameText.name : "NULL")}");
            Debug.Log($"TeamNameText: {(teamNameText != null ? teamNameText.name : "NULL")}");
            Debug.Log($"StatsText: {(statsText != null ? statsText.name : "NULL")}");
            Debug.Log($"EditButton: {(editButton != null ? editButton.name : "NULL")}");
            Debug.Log($"StatusButton: {(statusButton != null ? statusButton.name : "NULL")}");
            Debug.Log($"BackgroundImage: {(backgroundImage != null ? backgroundImage.name : "NULL")}");
        }
    }
}
