using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Composant d'affichage des messages d'encouragement
/// Gère l'animation et l'apparence visuelle des messages
/// </summary>
public class EncouragementDisplay : MonoBehaviour
{
    #region Configuration UI
    [Header("Références UI")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Positionnement")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private Vector2 messagePosition = new Vector2(0, 100);
    [SerializeField] private Vector2 messageSize = new Vector2(400, 100);
    
    [Header("Style")]
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private FontStyles fontStyle = FontStyles.Bold;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
    #endregion

    #region Private Fields
    private Coroutine currentDisplayCoroutine;
    private bool isDisplaying = false;
    #endregion

    #region Events
    public System.Action<string> OnMessageShown;
    public System.Action OnMessageHidden;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (autoCreateUI && (messagePanel == null || messageText == null))
        {
            CreateUI();
        }
        
        SetupUI();
    }

    void Start()
    {
        // S'assurer que l'UI est masquée au démarrage
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Affiche un message pendant une durée spécifiée
    /// </summary>
    public void ShowMessage(string message, float duration = 3f)
    {
        if (string.IsNullOrEmpty(message)) return;

        // Arrêter l'affichage précédent si nécessaire
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }

        currentDisplayCoroutine = StartCoroutine(DisplayMessageCoroutine(message, duration));
    }

    /// <summary>
    /// Masque le message immédiatement
    /// </summary>
    public void HideMessage()
    {
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
            currentDisplayCoroutine = null;
        }

        StartCoroutine(FadeOutCoroutine());
    }

    /// <summary>
    /// Vérifie si un message est en cours d'affichage
    /// </summary>
    public bool IsDisplaying => isDisplaying;
    #endregion

    #region Private Methods
    /// <summary>
    /// Crée automatiquement l'UI si elle n'existe pas
    /// </summary>
    private void CreateUI()
    {
        // Rechercher un Canvas parent
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            // Créer un Canvas si nécessaire
            GameObject canvasGO = new GameObject("EncouragementCanvas");
            canvasGO.transform.SetParent(transform);
            parentCanvas = canvasGO.AddComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            parentCanvas.sortingOrder = 100; // Priorité élevée pour être au-dessus
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Créer le panel principal
        GameObject panelGO = new GameObject("EncouragementPanel");
        panelGO.transform.SetParent(parentCanvas.transform, false);
        
        messagePanel = panelGO;
        
        // Ajouter Image pour le background
        Image backgroundImage = panelGO.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        // Configuration RectTransform du panel
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = messagePosition;
        panelRect.sizeDelta = messageSize;
        
        // Créer le texte
        GameObject textGO = new GameObject("MessageText");
        textGO.transform.SetParent(panelGO.transform, false);
        
        messageText = textGO.AddComponent<TextMeshProUGUI>();
        
        // Configuration RectTransform du texte
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        // Ajouter CanvasGroup pour les animations
        canvasGroup = panelGO.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// Configure l'UI avec les paramètres définis
    /// </summary>
    private void SetupUI()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.color = textColor;
            messageText.fontSize = fontSize;
            messageText.fontStyle = fontStyle;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.verticalAlignment = VerticalAlignmentOptions.Middle;
        }

        if (canvasGroup == null && messagePanel != null)
        {
            canvasGroup = messagePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = messagePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// Coroutine principale d'affichage d'un message
    /// </summary>
    private IEnumerator DisplayMessageCoroutine(string message, float duration)
    {
        isDisplaying = true;
        
        // Configurer le texte
        if (messageText != null)
        {
            messageText.text = message;
        }

        // Activer le panel
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }

        // Animation d'entrée
        yield return StartCoroutine(FadeInCoroutine());
        
        // Déclencher l'événement
        OnMessageShown?.Invoke(message);

        // Attendre la durée d'affichage
        yield return new WaitForSeconds(duration);

        // Animation de sortie
        yield return StartCoroutine(FadeOutCoroutine());
        
        isDisplaying = false;
        currentDisplayCoroutine = null;
    }

    /// <summary>
    /// Animation de fondu d'entrée
    /// </summary>
    private IEnumerator FadeInCoroutine()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            float curveValue = fadeInCurve.Evaluate(progress);
            canvasGroup.alpha = curveValue;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Animation de fondu de sortie
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            float curveValue = fadeOutCurve.Evaluate(progress);
            canvasGroup.alpha = startAlpha * curveValue;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Désactiver le panel
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        
        // Déclencher l'événement
        OnMessageHidden?.Invoke();
    }
    #endregion

    #region Public Configuration
    /// <summary>
    /// Met à jour la couleur du texte
    /// </summary>
    public void SetTextColor(Color color)
    {
        textColor = color;
        if (messageText != null)
        {
            messageText.color = color;
        }
    }

    /// <summary>
    /// Met à jour la taille de la police
    /// </summary>
    public void SetFontSize(int size)
    {
        fontSize = size;
        if (messageText != null)
        {
            messageText.fontSize = size;
        }
    }

    /// <summary>
    /// Met à jour la position du message
    /// </summary>
    public void SetMessagePosition(Vector2 position)
    {
        messagePosition = position;
        if (messagePanel != null)
        {
            RectTransform rect = messagePanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = position;
            }
        }
    }
    #endregion
}
