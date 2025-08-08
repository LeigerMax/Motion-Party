using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Adaptateur pour utiliser PlayerSelectionItem avec le système Team
    /// Permet de réutiliser le prefab existant sans modification
    /// </summary>
    public class PlayerSelectionItemAdapter : MonoBehaviour
    {
        [Header("Player Selection Item References")]
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Toggle selectionToggle;
        [SerializeField] private Image backgroundImage;

        private Systems.PlayerData playerData;
        private System.Action<Systems.PlayerData> onEditCallback;
        private System.Action<Systems.PlayerData> onRemoveCallback;

        public void SetupPlayer(Systems.PlayerData player, System.Action<Systems.PlayerData> onEdit, System.Action<Systems.PlayerData> onRemove)
        {
            playerData = player;
            onEditCallback = onEdit;
            onRemoveCallback = onRemove;

            UpdateDisplay();
            SetupInteraction();
        }

        private void UpdateDisplay()
        {
            if (playerData == null) return;

            // Mettre à jour les textes
            if (nicknameText != null)
                nicknameText.text = playerData.Nickname;

            if (teamNameText != null)
                teamNameText.text = playerData.TeamName;

            if (statsText != null)
                statsText.text = $"{playerData.GamesPlayed} parties • {playerData.TotalScore} pts";
        }

        private void SetupInteraction()
        {
            if (selectionToggle != null)
            {
                // Transformer le toggle en bouton d'édition
                selectionToggle.onValueChanged.RemoveAllListeners();
                selectionToggle.onValueChanged.AddListener(OnToggleChanged);
            }

            // Ajouter un clic droit pour supprimer (optionnel)
            var button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnItemClicked);
        }

        private void OnToggleChanged(bool value)
        {
            if (value)
            {
                // Quand le toggle est activé, déclencher l'édition
                onEditCallback?.Invoke(playerData);
                
                // Remettre le toggle à false après un court délai
                Invoke(nameof(ResetToggle), 0.1f);
            }
        }

        private void ResetToggle()
        {
            if (selectionToggle != null)
                selectionToggle.isOn = false;
        }

        private void OnItemClicked()
        {
            // Clic simple pour éditer
            onEditCallback?.Invoke(playerData);
        }

        public void RemovePlayer()
        {
            onRemoveCallback?.Invoke(playerData);
        }

        // Context Menu pour tester
        [ContextMenu("Test Remove Player")]
        public void TestRemovePlayer()
        {
            if (playerData != null)
            {
                Debug.Log($"Test suppression de {playerData.Nickname}");
                RemovePlayer();
            }
        }
    }
}
