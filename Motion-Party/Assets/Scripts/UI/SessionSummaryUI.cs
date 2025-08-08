using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Core.Analytics.Core;
using Core.Analytics.Data;

namespace UI
{
    /// <summary>
    /// Interface de résumé de fin de session de jeu
    /// Déplacé de Core.Analytics vers Scripts/UI pour une meilleure cohérence architecturale
    /// </summary>
    public class SessionSummaryUI : MonoBehaviour
    {
        [Header("UI Elements")]
        private GameObject backgroundPanel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI summaryText;
        private Button continueButton;
        private ScrollRect insightsScrollRect;
        private Transform insightsContent;

        [Header("State")]
        private SessionAnalyzer sessionAnalyzer;
        private bool isDisplaying = true;

        public bool IsDisplaying => isDisplaying;

        /// <summary>
        /// Initialise l'UI de résumé avec les données de session
        /// </summary>
        public void Initialize(SessionAnalyzer analyzer)
        {
            sessionAnalyzer = analyzer;
            CreateUI();
            PopulateWithData();
        }

        #region UI Creation

        /// <summary>
        /// Crée l'interface utilisateur du résumé
        /// </summary>
        private void CreateUI()
        {
            // Panel de fond
            backgroundPanel = new GameObject("BackgroundPanel");
            backgroundPanel.transform.SetParent(transform);
            
            var bgImage = backgroundPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
            
            var bgRect = backgroundPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Panel principal
            var mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(backgroundPanel.transform);
            
            var mainImage = mainPanel.AddComponent<Image>();
            mainImage.color = Color.white;
            
            var mainRect = mainPanel.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.1f, 0.1f);
            mainRect.anchorMax = new Vector2(0.9f, 0.9f);
            mainRect.sizeDelta = Vector2.zero;
            mainRect.anchoredPosition = Vector2.zero;

            // Titre
            CreateTitle(mainPanel.transform);

            // Contenu de résumé
            CreateSummaryContent(mainPanel.transform);

            // Bouton continuer
            CreateContinueButton(mainPanel.transform);
        }

        private void CreateTitle(Transform parent)
        {
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "🎉 Session Terminée !";
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.black;
            titleText.alignment = TextAlignmentOptions.Center;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
        }

        private void CreateSummaryContent(Transform parent)
        {
            var contentObj = new GameObject("SummaryContent");
            contentObj.transform.SetParent(parent);
            
            summaryText = contentObj.AddComponent<TextMeshProUGUI>();
            summaryText.fontSize = 18;
            summaryText.color = Color.black;
            summaryText.alignment = TextAlignmentOptions.TopLeft;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.2f);
            contentRect.anchorMax = new Vector2(0.95f, 0.8f);
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;

            // Scroll pour les insights
            CreateInsightsScroll(parent);
        }

        private void CreateInsightsScroll(Transform parent)
        {
            var scrollObj = new GameObject("InsightsScroll");
            scrollObj.transform.SetParent(parent);
            
            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            insightsScrollRect = scrollRect;
            
            var scrollRectTransform = scrollObj.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0.05f, 0.3f);
            scrollRectTransform.anchorMax = new Vector2(0.95f, 0.7f);
            scrollRectTransform.sizeDelta = Vector2.zero;
            scrollRectTransform.anchoredPosition = Vector2.zero;

            // Viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform);
            
            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            
            var viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            
            var viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;

            // Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform);
            
            var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 10;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            
            var contentSizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            insightsContent = contentObj.transform;

            // Configuration du ScrollRect
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
        }

        private void CreateContinueButton(Transform parent)
        {
            var buttonObj = new GameObject("ContinueButton");
            buttonObj.transform.SetParent(parent);
            
            continueButton = buttonObj.AddComponent<Button>();
            
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f);
            
            var buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.35f, 0.05f);
            buttonRect.anchorMax = new Vector2(0.65f, 0.15f);
            buttonRect.sizeDelta = Vector2.zero;
            buttonRect.anchoredPosition = Vector2.zero;

            // Texte du bouton
            var buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform);
            
            var buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Continuer";
            buttonText.fontSize = 20;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            buttonTextRect.anchoredPosition = Vector2.zero;

            // Event du bouton
            continueButton.onClick.AddListener(CloseSummary);
        }

        #endregion

        #region Data Population

        /// <summary>
        /// Remplit l'interface avec les données de session
        /// </summary>
        private void PopulateWithData()
        {
            // Affichage simple
            titleText.text = "🎉 FIN de partie, merci d'avoir joué ! 🎉";
            summaryText.text = "Merci pour cette session de jeu !\n\nVous allez être redirigé vers le menu principal...";
            
            // Pas d'insights pour éviter les erreurs
            // PopulateInsights ne sera pas appelé
        }

        /// <summary>
        /// Crée le texte de résumé principal
        /// </summary>
        private string CreateSummaryText(SessionAnalysisResult analysis)
        {
            var content = new System.Text.StringBuilder();

            content.AppendLine($"📅 <b>Durée:</b> {analysis.sessionDuration:F1} minutes");
            content.AppendLine($"👥 <b>Joueurs:</b> {analysis.playerCount}");
            content.AppendLine($"✅ <b>Completion:</b> {analysis.completionRate * 100:F1}%");
            content.AppendLine();

            content.AppendLine("<b>🏆 Moyennes de Session:</b>");
            
            if (analysis.sessionAverages.TryGetValue("score", out float avgScore))
            {
                content.AppendLine($"• Score moyen: {avgScore:F1} points");
            }
            
            if (analysis.sessionAverages.TryGetValue("accuracy", out float avgAccuracy))
            {
                content.AppendLine($"• Précision moyenne: {avgAccuracy * 100:F1}%");
            }
            
            if (analysis.sessionAverages.TryGetValue("avg_reaction_time", out float avgReaction))
            {
                content.AppendLine($"• Temps de réaction: {avgReaction:F2}s");
            }

            content.AppendLine();
            content.AppendLine("<b>👤 Meilleurs Performances:</b>");

            // Trouver le meilleur joueur
            string bestPlayer = "";
            float bestScore = 0f;
            
            foreach (var playerAnalysis in analysis.playerAnalyses)
            {
                float playerScore = playerAnalysis.Value.playerMetrics.TryGetValue("score", out float score) ? score : 0f;
                if (playerScore > bestScore)
                {
                    bestScore = playerScore;
                    bestPlayer = playerAnalysis.Key;
                }
            }

            if (!string.IsNullOrEmpty(bestPlayer))
            {
                content.AppendLine($"🥇 Meilleur score: {bestPlayer} ({bestScore:F1} points)");
            }

            return content.ToString();
        }

        /// <summary>
        /// Ajoute les insights dans la zone de scroll
        /// </summary>
        private void PopulateInsights(List<string> insights)
        {
            foreach (string insight in insights)
            {
                CreateInsightItem(insight);
            }
        }

        /// <summary>
        /// Crée un élément d'insight
        /// </summary>
        private void CreateInsightItem(string insightText)
        {
            var itemObj = new GameObject("InsightItem");
            itemObj.transform.SetParent(insightsContent);

            var itemImage = itemObj.AddComponent<Image>();
            itemImage.color = new Color(0.95f, 0.95f, 1f, 1f);

            var itemLayout = itemObj.AddComponent<LayoutElement>();
            itemLayout.minHeight = 40;
            itemLayout.flexibleHeight = 1;

            var itemText = itemObj.AddComponent<TextMeshProUGUI>();
            itemText.text = insightText;
            itemText.fontSize = 16;
            itemText.color = Color.black;
            itemText.alignment = TextAlignmentOptions.MidlineLeft;
            itemText.margin = new Vector4(10, 5, 10, 5);

            var itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 1);
            itemRect.anchorMax = new Vector2(1, 1);
            itemRect.sizeDelta = new Vector2(0, 0);
        }

        #endregion

        #region UI Control

        /// <summary>
        /// Ferme le résumé et retourne au menu principal
        /// </summary>
        private void CloseSummary()
        {
            isDisplaying = false;
            
            // Retourner au menu principal automatiquement
            ReturnToMainMenu();
            
            // Animation de fermeture (optionnelle)
            StartCoroutine(FadeOutAndDestroy());
        }

        /// <summary>
        /// Retourne au menu principal après la session
        /// </summary>
        private void ReturnToMainMenu()
        {
            try
            {
                // Utiliser le GameSessionManager pour retourner au menu
                var gameSessionManager = GameSessionManager.Instance;
                if (gameSessionManager != null)
                {
                    Debug.Log("[SessionSummaryUI] Retour au menu via GameSessionManager");
                    gameSessionManager.ReturnToMainMenu();
                    return;
                }

                // Fallback : Charger directement la scène du menu principal
                Debug.Log("[SessionSummaryUI] Retour au menu direct");
                StartCoroutine(LoadMainMenuDelayed());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SessionSummaryUI] Erreur lors du retour au menu: {e.Message}");
                // Fallback: charger directement le menu
                StartCoroutine(LoadMainMenuDelayed());
            }
        }

        /// <summary>
        /// Charge le menu principal avec un délai pour l'animation
        /// </summary>
        private IEnumerator LoadMainMenuDelayed()
        {
            // Attendre un peu pour l'animation de fermeture
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                // Charger la scène du menu principal (MiniGameManager)
                UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameManager");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SessionSummaryUI] Impossible de charger MiniGameManager: {e.Message}");
                
                // Essayer des noms alternatifs
                try
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                }
                catch
                {
                    try
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Première scène du build
                    }
                    catch (System.Exception fallbackError)
                    {
                        Debug.LogError($"[SessionSummaryUI] Impossible de retourner au menu: {fallbackError.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Animation de fermeture en fondu
        /// </summary>
        private IEnumerator FadeOutAndDestroy()
        {
            var canvasGroup = backgroundPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = backgroundPanel.AddComponent<CanvasGroup>();
            }

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            // Détruire l'objet
            Destroy(gameObject);
        }

        /// <summary>
        /// Fermeture automatique après timeout
        /// </summary>
        private void Start()
        {
            StartCoroutine(AutoCloseAfterTimeout());
        }

        private IEnumerator AutoCloseAfterTimeout()
        {
            yield return new WaitForSeconds(5f); // 5 secondes seulement pour un message simple
            
            if (isDisplaying)
            {
                CloseSummary();
            }
        }

        #endregion
    }
}
