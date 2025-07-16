using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Utilitaire pour créer automatiquement le prefab LoadingScreenUI
/// </summary>
public class LoadingScreenPrefabCreator : MonoBehaviour
{
    [Header("Configuration du prefab")]
    [SerializeField] private bool createPrefabOnStart = false;
    
    [ContextMenu("Créer Prefab LoadingScreen")]
    public void CreateLoadingScreenPrefab()
    {
        // Créer le GameObject principal
        GameObject loadingScreenObj = new GameObject("LoadingScreenCanvas");
        
        // Ajouter Canvas
        Canvas canvas = loadingScreenObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Au-dessus de tout
        
        // Ajouter CanvasScaler
        CanvasScaler scaler = loadingScreenObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Ajouter GraphicRaycaster
        loadingScreenObj.AddComponent<GraphicRaycaster>();
        
        // Ajouter CanvasGroup pour les animations
        CanvasGroup canvasGroup = loadingScreenObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        // Créer l'arrière-plan
        GameObject background = CreateBackground(loadingScreenObj.transform);
        
        // Créer le panel de contenu
        GameObject contentPanel = CreateContentPanel(loadingScreenObj.transform);
        
        // Créer les éléments UI
        GameObject gamePreview = CreateGamePreview(contentPanel.transform);
        GameObject tipText = CreateTipText(contentPanel.transform);
        GameObject loadingIcon = CreateLoadingIcon(contentPanel.transform);
        GameObject progressPanel = CreateProgressPanel(contentPanel.transform);
        
        // Ajouter le composant LoadingScreenUI
        LoadingScreenUI loadingUI = loadingScreenObj.AddComponent<LoadingScreenUI>();
        
        // Configurer les références
        var canvasField = typeof(LoadingScreenUI).GetField("loadingCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var canvasGroupField = typeof(LoadingScreenUI).GetField("canvasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var tipTextField = typeof(LoadingScreenUI).GetField("tipText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gamePreviewField = typeof(LoadingScreenUI).GetField("gamePreviewImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var backgroundField = typeof(LoadingScreenUI).GetField("backgroundImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var loadingIconField = typeof(LoadingScreenUI).GetField("loadingIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var progressBarField = typeof(LoadingScreenUI).GetField("progressBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var progressTextField = typeof(LoadingScreenUI).GetField("progressText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        canvasField?.SetValue(loadingUI, canvas);
        canvasGroupField?.SetValue(loadingUI, canvasGroup);
        tipTextField?.SetValue(loadingUI, tipText.GetComponent<TextMeshProUGUI>());
        gamePreviewField?.SetValue(loadingUI, gamePreview.GetComponent<Image>());
        backgroundField?.SetValue(loadingUI, background.GetComponent<Image>());
        loadingIconField?.SetValue(loadingUI, loadingIcon);
        progressBarField?.SetValue(loadingUI, progressPanel.transform.Find("ProgressBar").GetComponent<Slider>());
        progressTextField?.SetValue(loadingUI, progressPanel.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>());
        
        // Créer le GameObject Manager
        GameObject managerObj = new GameObject("LoadingScreenManager");
        LoadingScreenManager manager = managerObj.AddComponent<LoadingScreenManager>();
        
        // Configurer les références du manager
        var loadingDataField = typeof(LoadingScreenManager).GetField("loadingData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var loadingUIField = typeof(LoadingScreenManager).GetField("loadingUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        loadingUIField?.SetValue(manager, loadingUI);
        
        // Charger les données par défaut
        manager.LoadDataFromResources();
        
        // Faire du Manager un enfant du Canvas pour la hiérarchie
        managerObj.transform.SetParent(loadingScreenObj.transform);
        
        Debug.Log("Prefab LoadingScreen créé avec succès !");
        
        #if UNITY_EDITOR
        // Créer le dossier Prefabs s'il n'existe pas
        string prefabPath = "Assets/Prefabs";
        if (!System.IO.Directory.Exists(prefabPath))
        {
            System.IO.Directory.CreateDirectory(prefabPath);
        }
        
        // Sauvegarder comme prefab
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(loadingScreenObj, $"{prefabPath}/LoadingScreenCanvas.prefab");
        Debug.Log("Prefab sauvegardé dans Assets/Prefabs/LoadingScreenCanvas.prefab");
        #endif
    }
    
    private GameObject CreateBackground(Transform parent)
    {
        GameObject background = new GameObject("Background");
        background.transform.SetParent(parent);
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.black;
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        return background;
    }
    
    private GameObject CreateContentPanel(Transform parent)
    {
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(parent);
        
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        return contentPanel;
    }
    
    private GameObject CreateGamePreview(Transform parent)
    {
        GameObject gamePreview = new GameObject("GamePreview");
        gamePreview.transform.SetParent(parent);
        
        Image previewImage = gamePreview.AddComponent<Image>();
        previewImage.preserveAspect = true;
        
        RectTransform previewRect = gamePreview.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.5f, 0.7f);
        previewRect.anchorMax = new Vector2(0.5f, 0.7f);
        previewRect.sizeDelta = new Vector2(300, 200);
        
        return gamePreview;
    }
    
    private GameObject CreateTipText(Transform parent)
    {
        GameObject tipText = new GameObject("TipText");
        tipText.transform.SetParent(parent);
        
        TextMeshProUGUI text = tipText.AddComponent<TextMeshProUGUI>();
        text.text = "Préparez-vous pour le prochain défi !";
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        RectTransform textRect = tipText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return tipText;
    }
    
    private GameObject CreateLoadingIcon(Transform parent)
    {
        GameObject loadingIcon = new GameObject("LoadingIcon");
        loadingIcon.transform.SetParent(parent);
        
        Image iconImage = loadingIcon.AddComponent<Image>();
        iconImage.color = Color.white;
        
        RectTransform iconRect = loadingIcon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.2f);
        iconRect.anchorMax = new Vector2(0.5f, 0.2f);
        iconRect.sizeDelta = new Vector2(50, 50);
        
        return loadingIcon;
    }
    
    private GameObject CreateProgressPanel(Transform parent)
    {
        GameObject progressPanel = new GameObject("ProgressPanel");
        progressPanel.transform.SetParent(parent);
        
        RectTransform panelRect = progressPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.1f);
        panelRect.anchorMax = new Vector2(0.8f, 0.2f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Créer la barre de progression
        GameObject progressBar = new GameObject("ProgressBar");
        progressBar.transform.SetParent(progressPanel.transform);
        
        Slider slider = progressBar.AddComponent<Slider>();
        slider.value = 0f;
        
        RectTransform sliderRect = progressBar.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 1f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        // Créer le texte de progression
        GameObject progressText = new GameObject("ProgressText");
        progressText.transform.SetParent(progressPanel.transform);
        
        TextMeshProUGUI progText = progressText.AddComponent<TextMeshProUGUI>();
        progText.text = "Chargement... 0%";
        progText.fontSize = 24;
        progText.alignment = TextAlignmentOptions.Center;
        progText.color = Color.white;
        
        RectTransform progTextRect = progressText.GetComponent<RectTransform>();
        progTextRect.anchorMin = new Vector2(0f, 0f);
        progTextRect.anchorMax = new Vector2(1f, 0.5f);
        progTextRect.offsetMin = Vector2.zero;
        progTextRect.offsetMax = Vector2.zero;
        
        return progressPanel;
    }
    
    private void Start()
    {
        if (createPrefabOnStart)
        {
            CreateLoadingScreenPrefab();
        }
    }
}
