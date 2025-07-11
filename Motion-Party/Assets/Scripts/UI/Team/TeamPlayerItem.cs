using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Composant pour un item de joueur dans la liste d'équipe
    /// Affiche les informations du joueur et les boutons d'action
    /// </summary>
    public class TeamPlayerItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private Button editButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Configuration")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private Systems.PlayerData playerData;
        private System.Action<Systems.PlayerData> onEditCallback;
        private System.Action<Systems.PlayerData> onRemoveCallback;

        private void Start()
        {
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            if (editButton != null)
                editButton.onClick.AddListener(OnEditClicked);

            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemoveClicked);
        }

        /// <summary>
        /// Configure l'item avec les données du joueur
        /// </summary>
        public void SetupPlayer(Systems.PlayerData player, 
            System.Action<Systems.PlayerData> onEdit, 
            System.Action<Systems.PlayerData> onRemove)
        {
            playerData = player;
            onEditCallback = onEdit;
            onRemoveCallback = onRemove;

            UpdateDisplay();
        }

        /// <summary>
        /// Met à jour l'affichage des informations
        /// </summary>
        private void UpdateDisplay()
        {
            if (playerData == null) return;

            // Nom du joueur
            if (playerNameText != null)
            {
                playerNameText.text = playerData.Nickname;
            }

            // Statistiques
            if (playerStatsText != null)
            {
                string stats = $"{playerData.GamesPlayed} partie{(playerData.GamesPlayed > 1 ? "s" : "")}";
                if (playerData.TotalScore > 0)
                {
                    stats += $" • {playerData.TotalScore} pts";
                }
                playerStatsText.text = stats;
            }

            // Couleur de fond selon l'état
            if (backgroundImage != null)
            {
                backgroundImage.color = playerData.IsActive ? normalColor : new Color(0.8f, 0.8f, 0.8f, 0.5f);
            }
        }

        /// <summary>
        /// Anime l'item au survol
        /// </summary>
        public void OnPointerEnter()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = hoverColor;
            }
        }

        /// <summary>
        /// Restaure l'apparence normale
        /// </summary>
        public void OnPointerExit()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = playerData?.IsActive == true ? normalColor : new Color(0.8f, 0.8f, 0.8f, 0.5f);
            }
        }

        private void OnEditClicked()
        {
            onEditCallback?.Invoke(playerData);
        }

        private void OnRemoveClicked()
        {
            onRemoveCallback?.Invoke(playerData);
        }

        /// <summary>
        /// Met à jour les données du joueur (si modifié ailleurs)
        /// </summary>
        public void RefreshData(Systems.PlayerData updatedPlayer)
        {
            playerData = updatedPlayer;
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            // Nettoyage des callbacks
            if (editButton != null)
                editButton.onClick.RemoveListener(OnEditClicked);

            if (removeButton != null)
                removeButton.onClick.RemoveListener(OnRemoveClicked);
        }
    }
}
