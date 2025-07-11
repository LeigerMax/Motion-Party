using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerSelection
{
    /// <summary>
    /// Helper pour corriger automatiquement la configuration du ScrollView de sélection des joueurs
    /// </summary>
    public class PlayerSelectionScrollViewHelper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoFixOnStart = true;
        
        private void Start()
        {
            if (autoFixOnStart)
            {
                FixScrollViewConfiguration();
            }
        }

        [ContextMenu("Fix ScrollView Configuration")]
        public void FixScrollViewConfiguration()
        {
            // Trouver le ScrollRect dans ce GameObject ou ses enfants
            var scrollRect = GetComponentInChildren<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogWarning("PlayerSelectionScrollViewHelper: Aucun ScrollRect trouvé");
                return;
            }

            var content = scrollRect.content;
            if (content == null)
            {
                Debug.LogWarning("PlayerSelectionScrollViewHelper: Content du ScrollRect non assigné");
                return;
            }

            Debug.Log("PlayerSelectionScrollViewHelper: Correction de la configuration du ScrollView...");

            // Configuration du Content
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.anchoredPosition = new Vector2(0, 0);
            content.offsetMin = new Vector2(0, content.offsetMin.y);
            content.offsetMax = new Vector2(0, content.offsetMax.y);

            // Vérifier et configurer le Vertical Layout Group
            var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                Debug.Log("PlayerSelectionScrollViewHelper: VerticalLayoutGroup ajouté");
            }

            verticalLayout.spacing = 10f;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.padding = new RectOffset(10, 10, 10, 10);

            // Vérifier et configurer le Content Size Fitter
            var contentSizeFitter = content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = content.gameObject.AddComponent<ContentSizeFitter>();
                Debug.Log("PlayerSelectionScrollViewHelper: ContentSizeFitter ajouté");
            }

            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Configuration du ScrollRect
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;

            // Configuration du Viewport
            if (scrollRect.viewport != null)
            {
                var mask = scrollRect.viewport.GetComponent<Mask>();
                if (mask == null)
                {
                    mask = scrollRect.viewport.gameObject.AddComponent<Mask>();
                    Debug.Log("PlayerSelectionScrollViewHelper: Mask ajouté au Viewport");
                }

                var viewportImage = scrollRect.viewport.GetComponent<Image>();
                if (viewportImage == null)
                {
                    viewportImage = scrollRect.viewport.gameObject.AddComponent<Image>();
                    viewportImage.color = new Color(1, 1, 1, 0.01f); // Transparent mais nécessaire pour le masking
                    Debug.Log("PlayerSelectionScrollViewHelper: Image ajoutée au Viewport");
                }
            }

            Debug.Log("PlayerSelectionScrollViewHelper: Configuration du ScrollView terminée avec succès");
        }

        [ContextMenu("Debug ScrollView Info")]
        public void DebugScrollViewInfo()
        {
            var scrollRect = GetComponentInChildren<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.Log("Aucun ScrollRect trouvé");
                return;
            }

            var content = scrollRect.content;
            if (content == null)
            {
                Debug.Log("Content non assigné");
                return;
            }

            Debug.Log($"=== DEBUG SCROLLVIEW INFO ===");
            Debug.Log($"Content: {content.name}");
            Debug.Log($"Anchor Min: {content.anchorMin}, Anchor Max: {content.anchorMax}");
            Debug.Log($"Anchored Position: {content.anchoredPosition}");
            Debug.Log($"Size Delta: {content.sizeDelta}");
            Debug.Log($"Pivot: {content.pivot}");
            Debug.Log($"Nombre d'enfants: {content.childCount}");

            var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
            Debug.Log($"Vertical Layout Group: {verticalLayout != null}");
            if (verticalLayout != null)
            {
                Debug.Log($"  - Spacing: {verticalLayout.spacing}");
                Debug.Log($"  - Child Alignment: {verticalLayout.childAlignment}");
                Debug.Log($"  - Control Width: {verticalLayout.childControlWidth}");
                Debug.Log($"  - Control Height: {verticalLayout.childControlHeight}");
                Debug.Log($"  - Force Expand Width: {verticalLayout.childForceExpandWidth}");
                Debug.Log($"  - Force Expand Height: {verticalLayout.childForceExpandHeight}");
            }

            var contentSizeFitter = content.GetComponent<ContentSizeFitter>();
            Debug.Log($"Content Size Fitter: {contentSizeFitter != null}");
            if (contentSizeFitter != null)
            {
                Debug.Log($"  - Vertical Fit: {contentSizeFitter.verticalFit}");
                Debug.Log($"  - Horizontal Fit: {contentSizeFitter.horizontalFit}");
            }

            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                var rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Debug.Log($"Enfant {i} ({child.name}): Position: {rect.anchoredPosition}, Taille: {rect.sizeDelta}");
                }
            }
        }
    }
}
