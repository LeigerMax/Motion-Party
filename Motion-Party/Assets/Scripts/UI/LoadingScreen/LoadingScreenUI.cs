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
    
    private Coroutine loadingAnimationCoroutine;
    private bool isShowing = false;
    
    /// <summary>
    /// Indique si l'écran de chargement est actuellement affiché
    /// </summary>
    public bool IsShowing => isShowing;
    
    private void Awake()
    {
        // S'assurer que le canvas est initialement caché
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
            
        if (loadingCanvas != null)
            loadingCanvas.enabled = false;
    }
    
    /// <summary>
    /// Affiche l'écran de chargement avec les données spécifiées
    /// </summary>
    public void Show(LoadingScreenData.LoadingTip tipData, string title = null, string description = null)
    {
        Debug.Log($"[LoadingScreenUI] Show() appelé. isShowing avant: {isShowing}");
    
        // Permettre les interactions uniquement pendant l'affichage
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        if (isShowing) return;

        isShowing = true;
        Debug.Log($"[LoadingScreenUI] isShowing défini à true");

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
        Debug.Log($"[LoadingScreenUI] Hide() appelé. isShowing: {isShowing}");
        
        // Désactiver les interactions UI quand le loading screen est caché
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
        if (!isShowing) return;

        Debug.Log("[LoadingScreenUI] Démarrage de l'animation FadeOut");
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
        Debug.Log("[LoadingScreenUI] FadeOut terminé, isShowing défini à false");
        
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
}
