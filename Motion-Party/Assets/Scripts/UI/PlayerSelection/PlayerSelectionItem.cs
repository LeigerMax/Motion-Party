using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;

namespace UI.PlayerSelection
{
    /// <summary>
    /// Élément UI représentant un joueur dans la liste de sélection
    /// </summary>
    public class PlayerSelectionItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI teamText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Toggle selectionToggle;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Configuration")]
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color deselectedColor = Color.white;
        [SerializeField] private Color disabledColor = Color.gray;

        // Données
        private PlayerData playerData;
        private System.Action<PlayerData, bool> onToggleCallback;
        private bool isInteractable = true;

        public PlayerData PlayerData => playerData;
        public bool IsSelected => selectionToggle != null && selectionToggle.isOn;

        /// <summary>
        /// Initialise l'élément avec les données du joueur
        /// </summary>
        public void Initialize(PlayerData player, System.Action<PlayerData, bool> toggleCallback)
        {
            playerData = player;
            onToggleCallback = toggleCallback;

            SetupUI();
            UpdateVisuals();
        }

        /// <summary>
        /// Configuration initiale de l'interface
        /// </summary>
        private void SetupUI()
        {
            if (selectionToggle != null)
            {
                selectionToggle.onValueChanged.AddListener(OnToggleChanged);
                selectionToggle.isOn = false;
            }

            // Si les références UI ne sont pas assignées, les trouver automatiquement
            if (nicknameText == null)
                nicknameText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (selectionToggle == null)
                selectionToggle = GetComponentInChildren<Toggle>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            // Auto-configuration pour corriger les problèmes de layout
            AutoConfigure();
        }

        /// <summary>
        /// Configuration automatique du layout et des composants
        /// </summary>
        private void AutoConfigure()
        {
            // Ajouter le configurateur si pas déjà présent
            var configurator = GetComponent<PlayerSelectionItemConfigurator>();
            if (configurator == null)
            {
                configurator = gameObject.AddComponent<PlayerSelectionItemConfigurator>();
                configurator.ConfigureItem();
            }
        }

        /// <summary>
        /// Met à jour l'affichage visuel
        /// </summary>
        private void UpdateVisuals()
        {
            if (playerData == null) return;

            // Nom du joueur
            if (nicknameText != null)
            {
                nicknameText.text = playerData.Nickname;
            }

            // Équipe
            if (teamText != null)
            {
                teamText.text = string.IsNullOrEmpty(playerData.TeamName) ? "Aucune équipe" : playerData.TeamName;
            }

            // Statistiques
            if (statsText != null)
            {
                statsText.text = $"Parties: {playerData.GamesPlayed} | Score: {playerData.TotalScore}";
            }

            // Couleur de fond
            UpdateBackgroundColor();
        }

        /// <summary>
        /// Met à jour la couleur de fond selon l'état
        /// </summary>
        private void UpdateBackgroundColor()
        {
            if (backgroundImage == null) return;

            Color targetColor;
            if (!isInteractable)
                targetColor = disabledColor;
            else if (IsSelected)
                targetColor = selectedColor;
            else
                targetColor = deselectedColor;

            backgroundImage.color = targetColor;
        }

        /// <summary>
        /// Callback quand le toggle change
        /// </summary>
        private void OnToggleChanged(bool isOn)
        {
            UpdateBackgroundColor();
            onToggleCallback?.Invoke(playerData, isOn);
        }

        /// <summary>
        /// Force la sélection/déselection
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionToggle != null)
            {
                selectionToggle.isOn = selected;
            }
            UpdateBackgroundColor();
        }

        /// <summary>
        /// Active/désactive l'interactivité
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (selectionToggle != null)
            {
                selectionToggle.interactable = interactable;
            }

            UpdateBackgroundColor();
        }

        /// <summary>
        /// Met à jour les données du joueur
        /// </summary>
        public void UpdatePlayerData(PlayerData updatedPlayer)
        {
            playerData = updatedPlayer;
            UpdateVisuals();
        }

        /// <summary>
        /// Anime l'élément lors de la sélection
        /// </summary>
        public void AnimateSelection()
        {
            if (backgroundImage != null)
            {
                // Animation simple de pulsation
                StartCoroutine(SelectionAnimation());
            }
        }

        /// <summary>
        /// Coroutine pour l'animation de sélection
        /// </summary>
        private System.Collections.IEnumerator SelectionAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.05f;
            
            // Animation d'agrandissement
            float duration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // Animation de retour
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }

        /// <summary>
        /// Anime l'élément lors de la désélection
        /// </summary>
        public void AnimateDeselection()
        {
            if (backgroundImage != null)
            {
                StartCoroutine(DeselectionAnimation());
            }
        }

        /// <summary>
        /// Coroutine pour l'animation de désélection
        /// </summary>
        private System.Collections.IEnumerator DeselectionAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 0.95f;
            
            // Animation de rétrécissement
            float duration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // Animation de retour
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }

        private void OnDestroy()
        {
            if (selectionToggle != null)
            {
                selectionToggle.onValueChanged.RemoveListener(OnToggleChanged);
            }
        }
    }
}
