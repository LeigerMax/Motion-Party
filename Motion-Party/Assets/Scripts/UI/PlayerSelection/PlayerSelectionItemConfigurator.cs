using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.PlayerSelection
{
    /// <summary>
    /// Configurateur automatique pour les prefabs PlayerSelectionItem
    /// Corrige les problèmes de centrage et de configuration
    /// </summary>
    public class PlayerSelectionItemConfigurator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoConfigureOnAwake = true;
        [SerializeField] private bool debugMode = false;

        private void Awake()
        {
            if (autoConfigureOnAwake)
            {
                ConfigureItem();
            }
        }

        [ContextMenu("Configure Item")]
        public void ConfigureItem()
        {
            if (debugMode)
                Debug.Log($"Configuration de PlayerSelectionItem: {gameObject.name}");

            // Configuration du RectTransform principal
            ConfigureMainRectTransform();

            // Configuration du Layout Element si nécessaire
            ConfigureLayoutElement();

            // Configuration des composants UI internes
            ConfigureInternalComponents();

            if (debugMode)
                Debug.Log($"Configuration terminée pour: {gameObject.name}");
        }

        private void ConfigureMainRectTransform()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            // Configuration pour être compatible avec un VerticalLayoutGroup parent
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Hauteur fixe pour une présentation cohérente
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 80f);

            if (debugMode)
                Debug.Log($"RectTransform configuré - Anchors: {rectTransform.anchorMin} to {rectTransform.anchorMax}, Size: {rectTransform.sizeDelta}");
        }

        private void ConfigureLayoutElement()
        {
            var layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
                if (debugMode)
                    Debug.Log("LayoutElement ajouté");
            }

            // Configuration pour une hauteur préférée fixe
            layoutElement.minHeight = 80f;
            layoutElement.preferredHeight = 80f;
            layoutElement.flexibleHeight = 0f;

            // Largeur flexible pour s'adapter au parent
            layoutElement.flexibleWidth = 1f;

            if (debugMode)
                Debug.Log($"LayoutElement configuré - MinHeight: {layoutElement.minHeight}, PreferredHeight: {layoutElement.preferredHeight}");
        }

        private void ConfigureInternalComponents()
        {
            // Chercher et configurer un Horizontal Layout Group pour organiser le contenu interne
            var horizontalLayout = GetComponentInChildren<HorizontalLayoutGroup>();
            if (horizontalLayout == null)
            {
                // Créer un conteneur pour le contenu si nécessaire
                var contentContainer = transform.Find("Content");
                if (contentContainer == null)
                {
                    var contentObject = new GameObject("Content");
                    contentObject.transform.SetParent(transform, false);
                    
                    var contentRect = contentObject.AddComponent<RectTransform>();
                    contentRect.anchorMin = Vector2.zero;
                    contentRect.anchorMax = Vector2.one;
                    contentRect.offsetMin = Vector2.zero;
                    contentRect.offsetMax = Vector2.zero;

                    horizontalLayout = contentObject.AddComponent<HorizontalLayoutGroup>();
                    
                    if (debugMode)
                        Debug.Log("Conteneur Content créé avec HorizontalLayoutGroup");
                }
            }

            if (horizontalLayout != null)
            {
                horizontalLayout.spacing = 10f;
                horizontalLayout.padding = new RectOffset(10, 10, 5, 5);
                horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
                horizontalLayout.childControlWidth = false;
                horizontalLayout.childControlHeight = false;
                horizontalLayout.childForceExpandWidth = false;
                horizontalLayout.childForceExpandHeight = false;

                if (debugMode)
                    Debug.Log("HorizontalLayoutGroup configuré");
            }

            // Configuration spécifique des composants de texte
            ConfigureTextComponents();

            // Configuration spécifique du Toggle
            ConfigureToggleComponent();
        }

        private void ConfigureTextComponents()
        {
            var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
            
            foreach (var text in textComponents)
            {
                // Configuration pour une meilleure lisibilité
                text.enableAutoSizing = true;
                text.fontSizeMin = 12f;
                text.fontSizeMax = 18f;
                text.alignment = TextAlignmentOptions.Left;

                // Configuration du RectTransform pour les textes
                var textRect = text.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // Les textes doivent s'adapter au contenu
                    var layoutElement = text.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = text.gameObject.AddComponent<LayoutElement>();
                    }
                    
                    layoutElement.flexibleWidth = 1f;
                    layoutElement.flexibleHeight = 1f;
                }

                if (debugMode)
                    Debug.Log($"TextMeshProUGUI configuré: {text.gameObject.name}");
            }
        }

        private void ConfigureToggleComponent()
        {
            var toggle = GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                // Configuration du Toggle pour qu'il reste à droite
                var toggleRect = toggle.GetComponent<RectTransform>();
                if (toggleRect != null)
                {
                    var layoutElement = toggle.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = toggle.gameObject.AddComponent<LayoutElement>();
                    }

                    // Taille fixe pour le toggle
                    layoutElement.preferredWidth = 50f;
                    layoutElement.preferredHeight = 50f;
                    layoutElement.flexibleWidth = 0f;
                    layoutElement.flexibleHeight = 0f;
                }

                if (debugMode)
                    Debug.Log($"Toggle configuré: {toggle.gameObject.name}");
            }
        }

        [ContextMenu("Debug Item Configuration")]
        public void DebugItemConfiguration()
        {
            Debug.Log($"=== DEBUG ITEM CONFIGURATION: {gameObject.name} ===");
            
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"RectTransform - Anchors: {rectTransform.anchorMin} to {rectTransform.anchorMax}");
                Debug.Log($"RectTransform - Position: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}");
                Debug.Log($"RectTransform - Pivot: {rectTransform.pivot}");
            }

            var layoutElement = GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                Debug.Log($"LayoutElement - MinHeight: {layoutElement.minHeight}, PreferredHeight: {layoutElement.preferredHeight}");
                Debug.Log($"LayoutElement - FlexibleWidth: {layoutElement.flexibleWidth}, FlexibleHeight: {layoutElement.flexibleHeight}");
            }

            var horizontalLayout = GetComponentInChildren<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                Debug.Log($"HorizontalLayoutGroup - Spacing: {horizontalLayout.spacing}");
                Debug.Log($"HorizontalLayoutGroup - Alignment: {horizontalLayout.childAlignment}");
                Debug.Log($"HorizontalLayoutGroup - Control Size: W={horizontalLayout.childControlWidth}, H={horizontalLayout.childControlHeight}");
            }

            var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
            Debug.Log($"Nombre de TextMeshProUGUI: {textComponents.Length}");

            var toggle = GetComponentInChildren<Toggle>();
            Debug.Log($"Toggle trouvé: {toggle != null}");

            Debug.Log("=== FIN DEBUG ===");
        }
    }
}
