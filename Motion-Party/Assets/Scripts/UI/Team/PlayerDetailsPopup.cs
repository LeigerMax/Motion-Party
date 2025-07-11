using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Gameplay.Common.Badges; // Ajout pour les badges

namespace UI.Team
{
    /// <summary>
    /// Popup détaillé pour afficher/éditer les informations d'un joueur
    /// ÉTENDU : Maintenant inclut le profil joueur avec badges et statistiques
    /// </summary>
    public class PlayerDetailsPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI playerNameTitle;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI gamesPlayedText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI createdDateText;

        [Header("NOUVEAU : Profil Joueur")]
        [SerializeField] private PlayerBadgeGrid badgeGrid;
        [SerializeField] private PlayerProfileStats profileStats;
        [SerializeField] private GameObject profileSection;
        [SerializeField] private Button profileToggleButton;
        [SerializeField] private TextMeshProUGUI profileToggleText;

        [Header("Action Buttons")]
        [SerializeField] private Button editButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button closeButton;

        [Header("Animation")]
        [SerializeField] private CanvasGroup popupCanvasGroup;
        [SerializeField] private float animationDuration = 0.3f;

        // Variables existantes
        private Systems.PlayerData currentPlayer;
        private System.Action<Systems.PlayerData> onEditPlayer;
        private System.Action<Systems.PlayerData> onDeletePlayer;
        private bool debugMode = true;

        // NOUVEAU : Variables pour le profil
        private bool profileSectionVisible = false;

        private void Awake()
        {
            SetupButtons();
            
            if (popupPanel != null)
                popupPanel.SetActive(false);

            // NOUVEAU : Setup pour le profil
            SetupProfileSection();
        }

        /// <summary>
        /// NOUVEAU : Configuration de la section profil
        /// </summary>
        private void SetupProfileSection()
        {
            // Configurer le bouton toggle du profil
            if (profileToggleButton != null)
                profileToggleButton.onClick.AddListener(OnProfileToggleClicked);

            // Cacher la section profil par défaut
            if (profileSection != null)
                profileSection.SetActive(false);

            // Texte initial du bouton
            UpdateProfileToggleText();
        }

        private void SetupButtons()
        {
            if (editButton != null)
                editButton.onClick.AddListener(OnEditClicked);

            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void ShowPlayerDetails(Systems.PlayerData player, System.Action<Systems.PlayerData> onEdit, System.Action<Systems.PlayerData> onDelete)
        {
            // Vérifier la configuration du Canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (debugMode)
                    Debug.Log($"PlayerDetailsPopup: Canvas trouvé - Render Mode: {canvas.renderMode}, Sort Order: {canvas.sortingOrder}");
            }
            else
            {
                Debug.LogWarning("PlayerDetailsPopup: Aucun Canvas parent trouvé");
            }

            // Vérifier la visibilité
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null && debugMode)
            {
                Debug.Log($"PlayerDetailsPopup: Position: {rectTransform.anchoredPosition}, Taille: {rectTransform.sizeDelta}");
            }

            currentPlayer = player;
            onEditPlayer = onEdit;
            onDeletePlayer = onDelete;

            UpdateDisplay();
            ShowPopup();

            // NOUVEAU : Mise à jour du profil joueur
            UpdatePlayerProfile();

            if (debugMode)
                Debug.Log($"PlayerDetailsPopup: Affichage des détails pour {player.Nickname}");
        }

        /// <summary>
        /// NOUVEAU : Met à jour l'affichage du profil joueur
        /// </summary>
        private void UpdatePlayerProfile()
        {
            if (currentPlayer == null) return;

            // Mettre à jour les statistiques du profil
            if (profileStats != null)
            {
                profileStats.UpdatePlayerStats(currentPlayer);
            }

            // Préparer les badges (mais ne pas les afficher tout de suite)
            if (badgeGrid != null)
            {
                // Les badges seront affichés quand la section sera visible
                if (profileSectionVisible)
                {
                    badgeGrid.DisplayPlayerBadges(currentPlayer.Nickname);
                }
            }

            if (debugMode)
                Debug.Log($"🏆 Profil mis à jour pour {currentPlayer.Nickname}");
        }

        /// <summary>
        /// NOUVEAU : Gère le clic sur le bouton toggle du profil
        /// </summary>
        private void OnProfileToggleClicked()
        {
            profileSectionVisible = !profileSectionVisible;

            if (profileSection != null)
                profileSection.SetActive(profileSectionVisible);

            // Afficher les badges si on ouvre la section
            if (profileSectionVisible && badgeGrid != null && currentPlayer != null)
            {
                badgeGrid.DisplayPlayerBadges(currentPlayer.Nickname);
            }

            UpdateProfileToggleText();

            if (debugMode)
                Debug.Log($"Profil {(profileSectionVisible ? "affiché" : "masqué")} pour {currentPlayer?.Nickname}");
        }

        /// <summary>
        /// NOUVEAU : Met à jour le texte du bouton toggle
        /// </summary>
        private void UpdateProfileToggleText()
        {
            if (profileToggleText != null)
            {
                profileToggleText.text = profileSectionVisible ? "🔽 Masquer le profil" : "🔼 Voir le profil";
            }
        }

        private void UpdateDisplay()
        {
            if (currentPlayer == null) return;

            // Titre avec nom du joueur
            if (playerNameTitle != null)
                playerNameTitle.text = currentPlayer.Nickname;

            // Nom de l'équipe
            if (teamNameText != null)
                teamNameText.text = $"Équipe : {currentPlayer.TeamName}";

            // Parties jouées
            if (gamesPlayedText != null)
                gamesPlayedText.text = $"Parties jouées : {currentPlayer.GamesPlayed}";

            // Score total
            if (totalScoreText != null)
                totalScoreText.text = $"Score total : {currentPlayer.TotalScore} points";

            // Date de création
            if (createdDateText != null)
            {
                createdDateText.text = $"Créé le : {currentPlayer.CreatedAt:dd/MM/yyyy}";
            }
        }

        private void ShowPopup()
        {
            if (popupPanel != null)
            {
                popupPanel.SetActive(true);
                
                // Forcer la mise au premier plan
                transform.SetAsLastSibling();
                
                // Forcer la visibilité
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                
                StartCoroutine(FadeIn());
                
                if (debugMode)
                    Debug.Log($"PlayerDetailsPopup: Popup affiché - Actif: {gameObject.activeInHierarchy}");
            }
        }

        private void HidePopup()
        {
            StartCoroutine(FadeOut(() => {
                if (popupPanel != null)
                    popupPanel.SetActive(false);
                
                // Détruire le popup après l'animation
                Destroy(gameObject);
            }));
        }

        private IEnumerator FadeIn()
        {
            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = 0f;
                float elapsed = 0f;
                
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
                    yield return null;
                }
                
                popupCanvasGroup.alpha = 1f;
            }
        }

        private IEnumerator FadeOut(System.Action onComplete)
        {
            if (popupCanvasGroup != null)
            {
                float elapsed = 0f;
                
                while (elapsed < animationDuration)
                {
                    elapsed += Time.deltaTime;
                    popupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
                    yield return null;
                }
                
                popupCanvasGroup.alpha = 0f;
            }
            
            onComplete?.Invoke();
        }

        private void OnEditClicked()
        {
            if (debugMode)
                Debug.Log($"PlayerDetailsPopup: Édition demandée pour {currentPlayer?.Nickname}");

            onEditPlayer?.Invoke(currentPlayer);
            HidePopup();
        }

        private void OnDeleteClicked()
        {
            if (debugMode)
                Debug.Log($"PlayerDetailsPopup: Suppression demandée pour {currentPlayer?.Nickname}");

            // Confirmation simple
            if (currentPlayer != null)
            {
                bool confirmDelete = true; // Pour l'instant, toujours confirmer
                if (confirmDelete)
                {
                    onDeletePlayer?.Invoke(currentPlayer);
                    HidePopup();
                }
            }
        }

        private void OnCloseClicked()
        {
            if (debugMode)
                Debug.Log("PlayerDetailsPopup: Fermeture du popup");

            HidePopup();
        }

        public void ForceClose()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);
        }
    }
}
