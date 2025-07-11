using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Exemple de setup automatique pour un prefab de badge
    /// Peut être attaché à un prefab pour configuration automatique
    /// </summary>
    public class BadgeItemPrefabSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnAwake = true;

        private void Awake()
        {
            if (setupOnAwake)
            {
                SetupBadgeItemPrefab();
            }
        }

        /// <summary>
        /// Configure automatiquement un prefab d'élément de badge
        /// </summary>
        public void SetupBadgeItemPrefab()
        {
            // Ce script peut aider à créer automatiquement la structure
            // d'un prefab de badge pour faciliter la configuration
            
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // Taille recommandée pour un badge
            rectTransform.sizeDelta = new Vector2(120, 140);

            // Ajouter le component principal
            var badgeDisplay = GetComponent<PlayerBadgeDisplayItem>();
            if (badgeDisplay == null)
            {
                badgeDisplay = gameObject.AddComponent<PlayerBadgeDisplayItem>();
            }

            Debug.Log("✅ Badge item prefab configuré automatiquement");
        }

        /// <summary>
        /// Fonction helper pour créer un prefab badge basique dans l'éditeur
        /// </summary>
        [ContextMenu("Créer structure badge basique")]
        public void CreateBasicBadgeStructure()
        {
            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(transform);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Icon
            var icon = new GameObject("Icon");
            icon.transform.SetParent(transform);
            var iconImg = icon.AddComponent<Image>();
            
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.4f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // Title
            var title = new GameObject("Title");
            title.transform.SetParent(transform);
            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Badge";
            titleText.fontSize = 12;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.1f);
            titleRect.anchorMax = new Vector2(1f, 0.4f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            Debug.Log("✅ Structure basique de badge créée");
        }
    }
}
