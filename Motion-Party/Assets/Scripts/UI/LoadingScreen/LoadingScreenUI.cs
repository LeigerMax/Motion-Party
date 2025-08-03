using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{

    [Header("Composants UI")]
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Image gamePreviewImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject loadingIcon;
    
    [Header("Configuration d'animation")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float loadingIconRotationSpeed = 360f;
    
    [Header("Éléments optionnels")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Contrôle utilisateur")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;
    [SerializeField] private float minimumReadTime = 30f; // Temps minimum avant que le bouton apparaisse (30 secondes)
    
    // État du contrôle utilisateur
    private bool userCanContinue = false;
    private bool userRequestedContinue = false;
    private System.Action onUserContinue;
    
    private Coroutine loadingAnimationCoroutine;
    private bool isShowing = false;
    
    private void Awake()
    {
        // S'assurer que le canvas est initialement caché
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
            
        if (loadingCanvas != null)
            loadingCanvas.enabled = false;
            
        // Configurer le bouton Continue
        SetupContinueButton();
    }
    
    /// <summary>
    /// Configure le bouton Continue
    /// </summary>
    private void SetupContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(OnContinueButtonPressed);
            Debug.Log("[LoadingScreenUI] Bouton Continue configuré depuis l'Inspector");
        }
        else
        {
            Debug.LogWarning("[LoadingScreenUI] Aucun bouton Continue configuré. Tentative de création automatique...");
            CreateContinueButtonAutomatically();
        }
        
        // Texte par défaut du bouton
        if (continueButtonText != null)
        {
            continueButtonText.text = "Continuer";
        }
    }
    
    /// <summary>
    /// Crée automatiquement un bouton Continue si il n'est pas configuré
    /// </summary>
    private void CreateContinueButtonAutomatically()
    {
        // Chercher le Canvas root de la scène
        Canvas rootCanvas = null;
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        // Préférer le loadingCanvas si disponible
        if (loadingCanvas != null && loadingCanvas.GetComponent<Canvas>() != null)
        {
            rootCanvas = loadingCanvas.GetComponent<Canvas>();
        }
        else
        {
            // Sinon chercher un Canvas ScreenSpace
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    rootCanvas = canvas;
                    break;
                }
            }
            
            // En dernier recours, prendre le premier Canvas trouvé
            if (rootCanvas == null && allCanvases.Length > 0)
            {
                rootCanvas = allCanvases[0];
            }
        }
        
        if (rootCanvas == null)
        {
            Debug.LogError("[LoadingScreenUI] Aucun Canvas trouvé pour créer le bouton!");
            return;
        }
        
        Debug.Log($"[LoadingScreenUI] Utilisation du Canvas: {rootCanvas.name} pour créer le bouton");
        
        // Créer un GameObject pour le bouton
        GameObject buttonGO = new GameObject("ContinueButton_Auto");
        buttonGO.transform.SetParent(rootCanvas.transform, false);
        
        // Ajouter le composant Button
        continueButton = buttonGO.AddComponent<Button>();
        
        // Ajouter l'Image pour le Button
        Image buttonImage = buttonGO.AddComponent<Image>();
        
        // Convertir la couleur hex EED51E en Color Unity
        if (ColorUtility.TryParseHtmlString("#EED51E", out Color buttonColor))
        {
            buttonImage.color = buttonColor;
        }
        else
        {
            // Couleur de fallback si la conversion échoue
            buttonImage.color = new Color(0.93f, 0.83f, 0.12f, 1f); // EED51E en RGB
        }
        
        // Configurer le RectTransform - Position spécifiée par l'utilisateur
        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Centre de l'écran
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -224f); // Position x=0, y=-224
        rectTransform.sizeDelta = new Vector2(300f, 100f); // Width=300, Height=100
        
        // Créer le texte du bouton
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        continueButtonText = textGO.AddComponent<TextMeshProUGUI>();
        continueButtonText.text = "🎮 Continuer";
        continueButtonText.fontSize = 28f; // Plus grand pour la nouvelle taille
        continueButtonText.color = Color.black; // Noir pour contraster avec le jaune EED51E
        continueButtonText.alignment = TextAlignmentOptions.Center;
        continueButtonText.fontStyle = FontStyles.Bold; // En gras
        
        // Configurer le RectTransform du texte
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Configurer le bouton
        continueButton.targetGraphic = buttonImage;
        continueButton.onClick.AddListener(OnContinueButtonPressed);
        
        // S'assurer que le bouton est au premier plan
        buttonGO.transform.SetAsLastSibling();
        buttonGO.SetActive(false);
        
        // Vérifications supplémentaires pour le Canvas
        Debug.Log($"[LoadingScreenUI] Canvas configuration - Mode: {rootCanvas.renderMode}, Sort Order: {rootCanvas.sortingOrder}, Enabled: {rootCanvas.enabled}");
        
        // Forcer le bon mode de rendu
        if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("[LoadingScreenUI] Conversion du Canvas en ScreenSpaceOverlay");
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        // S'assurer qu'il est au premier plan
        rootCanvas.sortingOrder = 999;
        
        Debug.Log($"[LoadingScreenUI] Bouton Continue créé automatiquement - Position: {rectTransform.anchoredPosition}, Taille: {rectTransform.sizeDelta}, Couleur: {buttonImage.color}");
    }
    
    /// <summary>
    /// Affiche l'écran de chargement avec les données spécifiées
    /// </summary>
    public void Show(LoadingScreenData.LoadingTip tipData, string title = null, string description = null)
    {
    
        // Permettre les interactions uniquement pendant l'affichage
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        if (isShowing) return;

        isShowing = true;

        // Configurer l'interface
        SetupUI(tipData, title, description);

        // Activer le canvas
        if (loadingCanvas != null)
            loadingCanvas.enabled = true;

        // Démarrer l'animation d'apparition
        StartCoroutine(FadeIn());

        // Démarrer l'animation de chargement
        StartLoadingAnimation();
    }
    
    /// <summary>
    /// Cache l'écran de chargement
    /// </summary>
    public void Hide()
    {
        // Désactiver les interactions UI quand le loading screen est caché
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
        if (!isShowing) return;

        StartCoroutine(FadeOut());
    }
    
    
    /// <summary>
    /// Met à jour la progression du chargement
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;
            
        if (progressText != null)
            progressText.text = $"Chargement... {Mathf.RoundToInt(progress * 100)}%";
    }
    
    /// <summary>
    /// Configure l'interface utilisateur avec les données d'astuce
    /// </summary>
    private void SetupUI(LoadingScreenData.LoadingTip tipData, string title = null, string description = null)
    {
        // Configurer le texte d'astuce
        if (tipText != null)
        {
            tipText.text = tipData.tipText;
            // Ajout du titre et de la description si fournis
            if (!string.IsNullOrEmpty(title))
                tipText.text = $"<b>{title}</b>\n" + tipText.text;
            if (!string.IsNullOrEmpty(description))
                tipText.text += $"\n<size=80%>{description}</size>";
        }
        // Configurer l'image de prévisualisation
        if (gamePreviewImage != null)
        {
            if (tipData.gamePreviewImage != null)
            {
                gamePreviewImage.sprite = tipData.gamePreviewImage;
                gamePreviewImage.gameObject.SetActive(true);
            }
            else
            {
                gamePreviewImage.gameObject.SetActive(false);
            }
        }
        // Configurer l'arrière-plan
        if (backgroundImage != null)
            backgroundImage.color = tipData.backgroundColor;
        // Réinitialiser la barre de progression
        if (progressBar != null)
            progressBar.value = 0f;
            
        if (progressText != null)
            progressText.text = "Chargement... 0%";
    }
    
    /// <summary>
    /// Animation d'apparition en fondu
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Animation de disparition en fondu
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Désactiver le canvas
        if (loadingCanvas != null)
            loadingCanvas.enabled = false;
            
        isShowing = false;
        
        // Arrêter l'animation de chargement
        StopLoadingAnimation();
    }
    
    /// <summary>
    /// Démarre l'animation de l'icône de chargement
    /// </summary>
    private void StartLoadingAnimation()
    {
        if (loadingIcon != null)
        {
            loadingIcon.SetActive(true);
            loadingAnimationCoroutine = StartCoroutine(RotateLoadingIcon());
        }
    }
    
    /// <summary>
    /// Arrête l'animation de chargement
    /// </summary>
    private void StopLoadingAnimation()
    {
        if (loadingAnimationCoroutine != null)
        {
            StopCoroutine(loadingAnimationCoroutine);
            loadingAnimationCoroutine = null;
        }
        
        if (loadingIcon != null)
            loadingIcon.SetActive(false);
    }
    
    /// <summary>
    /// Animation de rotation de l'icône de chargement
    /// </summary>
    private IEnumerator RotateLoadingIcon()
    {
        Transform iconTransform = loadingIcon.transform;
        
        while (true)
        {
            iconTransform.Rotate(0f, 0f, -loadingIconRotationSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Active l'objet LoadingScreenUI
    /// </summary>
    public void EnableLoadingScreen()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Désactive l'objet LoadingScreenUI
    /// </summary>
    public void DisableLoadingScreen()
    {
        gameObject.SetActive(false);
    }
    
    #region Contrôle Utilisateur
    
    /// <summary>
    /// Active le contrôle utilisateur avec un bouton Continue
    /// </summary>
    /// <param name="onContinue">Callback appelé quand l'utilisateur clique sur Continuer</param>
    public void EnableUserControl(System.Action onContinue)
    {
        Debug.Log("[LoadingScreenUI] EnableUserControl appelé");
        
        onUserContinue = onContinue;
        userRequestedContinue = false;
        
        // Vérifier si le bouton est configuré
        if (continueButton == null)
        {
            Debug.LogWarning("[LoadingScreenUI] continueButton est null! Assurez-vous de l'assigner dans l'Inspector.");
            return;
        }
        
        Debug.Log($"[LoadingScreenUI] Bouton trouvé: {continueButton.name}, minimumReadTime: {minimumReadTime}s");
        
        // Démarrer la coroutine pour afficher le bouton après le temps minimum
        StartCoroutine(ShowContinueButtonAfterDelay());
    }
    
    /// <summary>
    /// Vérifie si l'utilisateur a demandé à continuer
    /// </summary>
    public bool HasUserRequestedContinue()
    {
        return userRequestedContinue;
    }
    
    /// <summary>
    /// Affiche le bouton Continue après le délai minimum
    /// </summary>
    private IEnumerator ShowContinueButtonAfterDelay()
    {
        Debug.Log($"[LoadingScreenUI] Attente de {minimumReadTime}s avant d'afficher le bouton...");
        yield return new WaitForSecondsRealtime(minimumReadTime);
        
        userCanContinue = true;
        Debug.Log("[LoadingScreenUI] Temps d'attente écoulé, affichage du bouton Continue");
        
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            Debug.Log($"[LoadingScreenUI] Bouton activé: {continueButton.gameObject.activeInHierarchy}");
            Debug.Log($"[LoadingScreenUI] Canvas actif: {continueButton.GetComponentInParent<Canvas>()?.enabled}");
            Debug.Log($"[LoadingScreenUI] Position bouton: {continueButton.transform.position}");
            
            // Animation d'apparition du bouton
            CanvasGroup buttonCanvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null)
                buttonCanvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
                
            buttonCanvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                buttonCanvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            buttonCanvasGroup.alpha = 1f;
            Debug.Log("[LoadingScreenUI] Animation du bouton terminée, alpha = 1");
            
            // Vérifier que le bouton est interactable
            Debug.Log($"[LoadingScreenUI] Bouton interactable: {continueButton.interactable}");
        }
        else
        {
            Debug.LogError("[LoadingScreenUI] continueButton est null dans ShowContinueButtonAfterDelay!");
        }
    }
    
    /// <summary>
    /// Méthode appelée quand le bouton Continue est pressé
    /// </summary>
    private void OnContinueButtonPressed()
    {
        if (!userCanContinue) return;
        
        userRequestedContinue = true;
        
        // Masquer le bouton
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
            
        // Appeler le callback
        onUserContinue?.Invoke();
        
        Debug.Log("[LoadingScreenUI] L'utilisateur a demandé à continuer");
    }
    
    /// <summary>
    /// Réinitialise l'état du contrôle utilisateur
    /// </summary>
    public void ResetUserControl()
    {
        userCanContinue = false;
        userRequestedContinue = false;
        onUserContinue = null;
        
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Méthode de test pour forcer la visibilité du bouton
    /// </summary>
    [ContextMenu("Test Button Visibility")]
    public void TestButtonVisibility()
    {
        if (continueButton == null)
        {
            Debug.LogError("[LoadingScreenUI] Pas de bouton à tester!");
            return;
        }
        
        Debug.Log($"[LoadingScreenUI] === TEST BOUTON VISIBILITY ===");
        Debug.Log($"Button Active: {continueButton.gameObject.activeInHierarchy}");
        Debug.Log($"Button Position: {continueButton.transform.position}");
        Debug.Log($"Button LocalPosition: {continueButton.transform.localPosition}");
        
        RectTransform rect = continueButton.GetComponent<RectTransform>();
        if (rect != null)
        {
            Debug.Log($"RectTransform: AnchoredPosition={rect.anchoredPosition}, SizeDelta={rect.sizeDelta}");
            Debug.Log($"Anchors: Min={rect.anchorMin}, Max={rect.anchorMax}");
        }
        
        Canvas canvas = continueButton.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Parent Canvas: {canvas.name}, RenderMode={canvas.renderMode}, SortOrder={canvas.sortingOrder}");
            Debug.Log($"Canvas Enabled: {canvas.enabled}, GameObject Active: {canvas.gameObject.activeInHierarchy}");
        }
        
        CanvasGroup cg = continueButton.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            Debug.Log($"CanvasGroup: Alpha={cg.alpha}, Interactable={cg.interactable}, BlocksRaycasts={cg.blocksRaycasts}");
        }
        
        Image img = continueButton.GetComponent<Image>();
        if (img != null)
        {
            Debug.Log($"Image: Color={img.color}, Enabled={img.enabled}");
        }
        
        // Forcer l'activation pour test
        continueButton.gameObject.SetActive(true);
        continueButton.transform.SetAsLastSibling();
        
        Debug.Log($"[LoadingScreenUI] === FIN TEST ===");
    }
    
    #endregion
}
