using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script pour configurer automatiquement l'UI du LogParade.
/// Utilise ce script pour créer rapidement une UI basique et fonctionnelle.
/// </summary>
[System.Serializable]
public class LogParadeUIAutoSetup : MonoBehaviour
{
    [Header("Configuration Automatique")]
    [Tooltip("Créer automatiquement les éléments UI manquants")]
    public bool autoCreateMissingElements = true;
    
    [Tooltip("Canvas parent pour créer les éléments UI")]
    public Canvas parentCanvas;
    
    [Header("Références")]
    [Tooltip("LogParadeUIManager à configurer")]
    public LogParadeUIManager uiManager;

    void Start()
    {
        if (autoCreateMissingElements && uiManager != null)
        {
            SetupBasicUI();
        }
    }

    /// <summary>
    /// Configure une UI basique pour LogParade
    /// </summary>
    [ContextMenu("Setup Basic UI")]
    public void SetupBasicUI()
    {
        if (uiManager == null)
        {
            LogParadeLogger.LogError("LogParadeUIManager non assigné !");
            return;
        }

        if (parentCanvas == null)
        {
            parentCanvas = FindFirstObjectByType<Canvas>();
            if (parentCanvas == null)
            {
                LogParadeLogger.LogError("Aucun Canvas trouvé ! Créez un Canvas d'abord.");
                return;
            }
        }

        LogParadeLogger.Log("🔧 Configuration automatique de l'UI LogParade...");

        // Créer le panel principal
        GameObject mainPanel = CreateUIPanel("LogParade_MainPanel", parentCanvas.transform);
        
        // Créer les textes de statut
        CreateStatusTexts(mainPanel.transform);
          // REMOVED: Lane indicators creation (system removed)
        
        // Créer le panel de calibration
        CreateCalibrationPanel(mainPanel.transform);

        LogParadeLogger.Log("✅ UI basique créée ! Configurez les références dans LogParadeUIManager.");
    }

    /// <summary>
    /// Crée spécifiquement CalibrationTextUI pour la calibration interactive
    /// </summary>
    [ContextMenu("Setup CalibrationTextUI")]
    public void SetupCalibrationTextUI()
    {
        if (parentCanvas == null)
        {
            parentCanvas = FindFirstObjectByType<Canvas>();
            if (parentCanvas == null)
            {
                LogParadeLogger.LogError("Aucun Canvas trouvé ! Créez un Canvas d'abord.");
                return;
            }
        }

        LogParadeLogger.Log("🎯 Création de CalibrationTextUI...");

        // Créer le GameObject principal pour CalibrationTextUI
        GameObject calibrationUI = new GameObject("CalibrationTextUI");
        calibrationUI.transform.SetParent(parentCanvas.transform, false);

        // Ajouter le composant CalibrationTextUI
        CalibrationTextUI calibrationTextUI = calibrationUI.AddComponent<CalibrationTextUI>();

        // Configurer comme panel fullscreen
        RectTransform calibRect = calibrationUI.GetComponent<RectTransform>();
        if (calibRect == null) calibRect = calibrationUI.AddComponent<RectTransform>();
        calibRect.anchorMin = Vector2.zero;
        calibRect.anchorMax = Vector2.one;
        calibRect.sizeDelta = Vector2.zero;
        calibRect.anchoredPosition = Vector2.zero;

        // Ajouter un fond semi-transparent
        Image background = calibrationUI.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);

        // Créer les éléments UI requis
        CreateCalibrationUIElements(calibrationUI.transform);

        LogParadeLogger.Log("✅ CalibrationTextUI créé ! Assignez-le dans LogParadeCalibrationInteractive.");
        LogParadeLogger.Log("💡 Configurez les références dans l'inspecteur CalibrationTextUI.");
    }

    private GameObject CreateUIPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.1f); // Légèrement transparent

        return panel;
    }

    private void CreateStatusTexts(Transform parent)
    {
        // Panel pour les textes de statut
        GameObject statusPanel = CreateUIPanel("StatusPanel", parent);
        RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.8f);
        statusRect.anchorMax = new Vector2(1, 1);

        // Texte de la voie actuelle
        CreateText("CurrentLaneText", "Voie: 2", statusPanel.transform, new Vector2(0, 0.5f), new Vector2(0.3f, 1));
        
        // Texte de position
        CreateText("PositionText", "Position: (0, 0, 0)", statusPanel.transform, new Vector2(0.3f, 0.5f), new Vector2(0.7f, 1));
        
        // Texte de statut du jeu
        CreateText("GameStatusText", "Prêt", statusPanel.transform, new Vector2(0.7f, 0.5f), new Vector2(1, 1));
        
        LogParadeLogger.Log("📝 Textes de statut créés");
    }    // REMOVED: CreateLaneIndicators and CreateLaneIndicator methods (system removed)

    private void CreateCalibrationPanel(Transform parent)
    {
        // Panel de calibration (masqué par défaut)
        GameObject calibPanel = CreateUIPanel("CalibrationPanel", parent);
        RectTransform calibRect = calibPanel.GetComponent<RectTransform>();
        calibRect.anchorMin = new Vector2(0.2f, 0.4f);
        calibRect.anchorMax = new Vector2(0.8f, 0.6f);

        calibPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        calibPanel.SetActive(false);        // Texte de calibration
        CreateText("CalibrationText", "Calibration en cours...", calibPanel.transform, new Vector2(0, 0.2f), new Vector2(1, 1));

        // Slider de progression (optionnel - commenté pour éviter les erreurs)
        // CreateSlider("CalibrationSlider", calibPanel.transform, new Vector2(0, 0), new Vector2(1, 0.4f));

        LogParadeLogger.Log("⚙️ Panel de calibration créé (sans slider)");
    }

    private GameObject CreateText(string name, string content, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = 16;
        text.color = Color.white;

        return textObj;
    }

    private GameObject CreateSlider(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);

        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.value = 0f;

        // Créer les éléments du slider
        CreateSliderElements(sliderObj);

        return sliderObj;
    }

    private void CreateSliderElements(GameObject sliderObj)
    {
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.gray;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        RectTransform fillImageRect = fill.AddComponent<RectTransform>();
        fillImageRect.anchorMin = Vector2.zero;
        fillImageRect.anchorMax = Vector2.one;
        fillImageRect.sizeDelta = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;

        // Assigner au slider
        Slider slider = sliderObj.GetComponent<Slider>();
        slider.fillRect = fillImageRect;
    }

    private void CreateCalibrationUIElements(Transform parent)
    {
        // Panel central pour les textes
        GameObject textPanel = CreateUIPanel("TextPanel", parent);
        RectTransform textRect = textPanel.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.3f);
        textRect.anchorMax = new Vector2(0.9f, 0.7f);
        textPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // Texte d'instruction principal
        GameObject instructionText = CreateText("InstructionText", "Instructions de calibration", textPanel.transform, 
                                              new Vector2(0, 0.6f), new Vector2(1, 1));
        instructionText.GetComponent<TextMeshProUGUI>().fontSize = 24;
        instructionText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Texte de statut
        GameObject statusText = CreateText("StatusText", "Statut: En attente", textPanel.transform, 
                                         new Vector2(0, 0.3f), new Vector2(1, 0.6f));
        statusText.GetComponent<TextMeshProUGUI>().fontSize = 18;
        statusText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Texte de timer
        GameObject timerText = CreateText("TimerText", "Temps: 15s", textPanel.transform, 
                                        new Vector2(0, 0), new Vector2(1, 0.3f));
        timerText.GetComponent<TextMeshProUGUI>().fontSize = 16;
        timerText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;        // REMOVED: Lane indicators creation (system removed)

        LogParadeLogger.Log("📝 Éléments CalibrationTextUI créés");
    }    // REMOVED: CreateCalibrationLaneIndicators method (system removed)

    /// <summary>
    /// Auto-assigne les références créées au UIManager
    /// </summary>
    [ContextMenu("Auto Assign References")]
    public void AutoAssignReferences()
    {
        if (uiManager == null)
        {
            LogParadeLogger.LogError("LogParadeUIManager non assigné !");
            return;
        }

        // Rechercher et assigner automatiquement les références
        LogParadeLogger.Log("🔗 Attribution automatique des références...");

        // TODO: Implémenter l'auto-assignation via reflection
        // Pour l'instant, l'utilisateur doit assigner manuellement

        LogParadeLogger.Log("⚠️ Assignez manuellement les références dans l'inspecteur LogParadeUIManager");
    }
}
