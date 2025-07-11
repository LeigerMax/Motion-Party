using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Bouton simple pour afficher un joueur dans la liste
    /// Quand on clique dessus, ça ouvre les détails du joueur
    /// </summary>
    public class SimplePlayerButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerStatsText;
        [SerializeField] private Button mainButton;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Configuration")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.8f, 0.8f, 1f, 1f);

        private Systems.PlayerData playerData;
        private System.Action<Systems.PlayerData> onPlayerSelected;
        private bool debugMode = true;
        
        // Propriété publique pour le débogage
        public string PlayerName => playerData?.Nickname ?? "Unknown";

        private void Awake()
        {
            if (mainButton == null)
                mainButton = GetComponent<Button>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            SetupButton();
        }

        private void SetupButton()
        {
            if (mainButton != null)
            {
                mainButton.onClick.RemoveAllListeners();
                mainButton.onClick.AddListener(OnButtonClicked);
            }

            // Configurer les couleurs hover
            if (mainButton != null && backgroundImage != null)
            {
                var colors = mainButton.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = hoverColor;
                colors.pressedColor = selectedColor;
                colors.selectedColor = selectedColor;
                mainButton.colors = colors;
            }
        }

        public void SetupPlayer(Systems.PlayerData player, System.Action<Systems.PlayerData> onSelected)
        {
            playerData = player;
            onPlayerSelected = onSelected;

            UpdateDisplay();

            if (debugMode)
                Debug.Log($"SimplePlayerButton: Configuré pour {player.Nickname}");
        }

        private void UpdateDisplay()
        {
            if (playerData == null) return;

            // Nom du joueur
            if (playerNameText != null)
            {
                playerNameText.text = playerData.Nickname;
            }

            // Statistiques simples
            if (playerStatsText != null)
            {
                string stats = $"{playerData.GamesPlayed} parties";
                if (playerData.TotalScore > 0)
                    stats += $" • {playerData.TotalScore} pts";
                
                playerStatsText.text = stats;
            }

            // Assurer que le bouton est visible
            if (mainButton != null)
                mainButton.interactable = true;

            if (debugMode)
                Debug.Log($"SimplePlayerButton: Affichage mis à jour pour {playerData.Nickname}");
        }

        private void OnButtonClicked()
        {
            if (debugMode)
                Debug.Log($"SimplePlayerButton: Clic sur {playerData?.Nickname}");

            onPlayerSelected?.Invoke(playerData);
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedColor : normalColor;
            }
        }

        [ContextMenu("Debug - Show Player Info")]
        public void DebugShowPlayerInfo()
        {
            Debug.Log("=== SIMPLE PLAYER BUTTON DEBUG ===");
            Debug.Log($"Player Data: {(playerData != null ? playerData.Nickname : "NULL")}");
            Debug.Log($"Player Name Text: {(playerNameText != null ? playerNameText.text : "NULL")}");
            Debug.Log($"Player Stats Text: {(playerStatsText != null ? playerStatsText.text : "NULL")}");
            Debug.Log($"Main Button: {(mainButton != null ? "OK" : "NULL")}");
            Debug.Log($"Background Image: {(backgroundImage != null ? "OK" : "NULL")}");
        }
    }
}
