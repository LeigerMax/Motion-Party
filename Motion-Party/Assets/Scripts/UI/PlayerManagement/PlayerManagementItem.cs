using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;

namespace UI.PlayerManagement
{
    /// <summary>
    /// Élément UI représentant un joueur dans la liste de gestion
    /// </summary>
    public class PlayerManagementItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI teamText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI createdDateText;
        [SerializeField] private Button editButton;
        [SerializeField] private Image statusIcon;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Configuration")]
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.red;
        [SerializeField] private Color normalBackgroundColor = Color.white;
        [SerializeField] private Color hoverBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        // Données
        private PlayerData playerData;
        private System.Action<PlayerData> onEditCallback;

        public PlayerData PlayerData => playerData;

        /// <summary>
        /// Initialise l'élément avec les données du joueur
        /// </summary>
        public void Initialize(PlayerData player, System.Action<PlayerData> editCallback)
        {
            playerData = player;
            onEditCallback = editCallback;

            SetupUI();
            UpdateVisuals();
        }

        /// <summary>
        /// Configuration initiale de l'interface
        /// </summary>
        private void SetupUI()
        {
            if (editButton != null)
            {
                editButton.onClick.AddListener(OnEditClicked);
            }

            // Si les références UI ne sont pas assignées, les trouver automatiquement
            if (nicknameText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) nicknameText = texts[0];
            }

            if (editButton == null)
                editButton = GetComponentInChildren<Button>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            // Ajouter des événements de survol
            if (backgroundImage != null)
            {
                var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }

                // Événement d'entrée de la souris
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((data) => OnPointerEnter());
                eventTrigger.triggers.Add(pointerEnter);

                // Événement de sortie de la souris
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => OnPointerExit());
                eventTrigger.triggers.Add(pointerExit);
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
                teamText.text = string.IsNullOrEmpty(playerData.TeamName) ? 
                    "<color=gray>Aucune équipe</color>" : 
                    $"<color=blue>{playerData.TeamName}</color>";
            }

            // Statistiques
            if (statsText != null)
            {
                float avgScore = playerData.GetAverageScore();
                statsText.text = $"Parties: {playerData.GamesPlayed} | Score: {playerData.TotalScore} | Moy: {avgScore:F1}";
            }

            // Date de création
            if (createdDateText != null)
            {
                createdDateText.text = $"Créé le: {playerData.CreatedAt:dd/MM/yyyy}";
            }

            // Statut actif/inactif
            if (statusIcon != null)
            {
                statusIcon.color = playerData.IsActive ? activeColor : inactiveColor;
            }

            // Couleur de fond initiale
            if (backgroundImage != null)
            {
                backgroundImage.color = normalBackgroundColor;
            }
        }

        /// <summary>
        /// Callback quand le bouton d'édition est cliqué
        /// </summary>
        private void OnEditClicked()
        {
            onEditCallback?.Invoke(playerData);
        }

        /// <summary>
        /// Callback quand la souris entre dans la zone
        /// </summary>
        private void OnPointerEnter()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = hoverBackgroundColor;
            }
        }

        /// <summary>
        /// Callback quand la souris sort de la zone
        /// </summary>
        private void OnPointerExit()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = normalBackgroundColor;
            }
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
        /// Active/désactive l'élément
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// Active/désactive l'interactivité
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (editButton != null)
            {
                editButton.interactable = interactable;
            }
        }

        /// <summary>
        /// Anime l'élément (par exemple lors d'une mise à jour)
        /// </summary>
        public void AnimateUpdate()
        {
            if (backgroundImage != null)
            {
                // Animation de flash pour indiquer une mise à jour
                Color originalColor = backgroundImage.color;
                Color flashColor = Color.yellow;

                // Animation simple sans LeanTween
                StartCoroutine(FlashAnimation(originalColor, flashColor));
            }
        }

        /// <summary>
        /// Coroutine pour l'animation de flash
        /// </summary>
        private System.Collections.IEnumerator FlashAnimation(Color originalColor, Color flashColor)
        {
            backgroundImage.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            backgroundImage.color = originalColor;
        }

        /// <summary>
        /// Anime l'élément lors de la suppression
        /// </summary>
        public void AnimateRemoval(System.Action onComplete = null)
        {
            // Animation simple sans LeanTween
            StartCoroutine(RemovalAnimation(onComplete));
        }

        /// <summary>
        /// Coroutine pour l'animation de suppression
        /// </summary>
        private System.Collections.IEnumerator RemovalAnimation(System.Action onComplete = null)
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }

            onComplete?.Invoke();
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Anime l'apparition de l'élément
        /// </summary>
        public void AnimateAppearance()
        {
            StartCoroutine(AppearanceAnimation());
        }

        /// <summary>
        /// Coroutine pour l'animation d'apparition
        /// </summary>
        private System.Collections.IEnumerator AppearanceAnimation()
        {
            Vector3 originalScale = transform.localScale;
            transform.localScale = Vector3.zero;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Effet de rebond simple
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easeT);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private void OnDestroy()
        {
            if (editButton != null)
            {
                editButton.onClick.RemoveListener(OnEditClicked);
            }
        }
    }
}
