using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core
{
    /// <summary>
    /// Créateur automatique d'écran de chargement simple
    /// </summary>
    public class SimpleLoadingCreator : MonoBehaviour
    {
        [Header("Style")]
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] private Color textColor = Color.white;

        private void Awake()
        {
            CreateSimpleLoadingScreen();
        }

        private void CreateSimpleLoadingScreen()
        {
            // Créer le Canvas
            GameObject canvasGO = new GameObject("LoadingCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Panel de fond
            GameObject panelGO = new GameObject("LoadingPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = backgroundColor;
            
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Texte de statut
            GameObject textGO = new GameObject("StatusText");
            textGO.transform.SetParent(panelGO.transform, false);
            
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Initialisation du jeu...\nVeuillez patienter";
            text.color = textColor;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(400, 100);

            // Icône de chargement (carré qui tourne)
            GameObject iconGO = new GameObject("LoadingIcon");
            iconGO.transform.SetParent(panelGO.transform, false);
            
            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.color = Color.white;
            
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.4f);
            iconRect.anchorMax = new Vector2(0.5f, 0.4f);
            iconRect.sizeDelta = new Vector2(50, 50);

            // Ajouter le script SimpleLoadingScreen
            SimpleLoadingScreen loadingScreen = canvasGO.AddComponent<SimpleLoadingScreen>();
            
            // Assigner les références via réflexion
            var loadingPanelField = typeof(SimpleLoadingScreen).GetField("loadingPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var statusTextField = typeof(SimpleLoadingScreen).GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loadingIconField = typeof(SimpleLoadingScreen).GetField("loadingIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            loadingPanelField?.SetValue(loadingScreen, panelGO);
            statusTextField?.SetValue(loadingScreen, text);
            loadingIconField?.SetValue(loadingScreen, iconImage);

            // Détruire ce créateur
            Destroy(gameObject);
        }
    }
}
