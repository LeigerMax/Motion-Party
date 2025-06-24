using UnityEngine;
using UnityEditor;

/// <summary>
/// Script de setup automatique pour le système de calibration interactive LogParade
/// Aide à configurer rapidement tous les composants nécessaires dans Unity
/// </summary>
public class LogParadeCalibrationSetup : MonoBehaviour
{
    [Header("Auto-Setup Configuration")]
    [SerializeField] private bool setupOnAwake = false;
    [SerializeField] private bool createUICanvas = true;
    [SerializeField] private bool assignLanesAutomatically = true;
    [SerializeField] private bool createCalibrationLogs = true;

    [Header("Prefab References")]
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private GameObject uiCanvasPrefab;

    [Header("Generated References (Auto-filled)")]
    [SerializeField] private LogParadeCalibrationInteractive calibrationSystem;
    [SerializeField] private LogParadeCalibrationManager calibrationManager;
    [SerializeField] private CalibrationTextUI calibrationUI;
    [SerializeField] private Transform[] generatedLanes;

    void Awake()
    {
        if (setupOnAwake)
        {
            PerformAutoSetup();
        }
    }

    /// <summary>
    /// Effectue le setup automatique complet
    /// </summary>
    [ContextMenu("Perform Auto Setup")]
    public void PerformAutoSetup()
    {
        Debug.Log("=== Début du setup automatique de la calibration LogParade ===");

        // 1. Setup du système de calibration principal
        SetupCalibrationSystem();

        // 2. Setup du manager de calibration
        SetupCalibrationManager();

        // 3. Setup des lanes si nécessaire
        if (assignLanesAutomatically)
        {
            SetupLanes();
        }

        // 4. Setup de l'UI
        if (createUICanvas)
        {
            SetupCalibrationUI();
        }

        // 5. Setup des rondins de calibration
        if (createCalibrationLogs)
        {
            SetupCalibrationLogs();
        }

        // 6. Connecter tous les composants
        ConnectComponents();

        Debug.Log("=== Setup automatique terminé ===");
    }

    /// <summary>
    /// Configure le système de calibration principal
    /// </summary>
    private void SetupCalibrationSystem()
    {
        // Chercher ou créer le système de calibration
        calibrationSystem = FindObjectOfType<LogParadeCalibrationInteractive>();
        
        if (calibrationSystem == null)
        {
            GameObject calibrationGO = new GameObject("LogParadeCalibrationSystem");
            calibrationGO.transform.SetParent(transform);
            calibrationSystem = calibrationGO.AddComponent<LogParadeCalibrationInteractive>();
            
            // Ajouter AudioSource
            calibrationGO.AddComponent<AudioSource>();
            
            Debug.Log("Système de calibration créé");
        }
        else
        {
            Debug.Log("Système de calibration existant trouvé");
        }
    }

    /// <summary>
    /// Configure le manager de calibration
    /// </summary>
    private void SetupCalibrationManager()
    {
        calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        
        if (calibrationManager == null)
        {
            GameObject managerGO = new GameObject("LogParadeCalibrationManager");
            managerGO.transform.SetParent(transform);
            calibrationManager = managerGO.AddComponent<LogParadeCalibrationManager>();
            
            Debug.Log("Manager de calibration créé");
        }
        else
        {
            Debug.Log("Manager de calibration existant trouvé");
        }
    }

    /// <summary>
    /// Configure les lanes automatiquement
    /// </summary>
    private void SetupLanes()
    {
        generatedLanes = new Transform[4];
        
        // Chercher des lanes existantes d'abord
        GameObject[] existingLanes = FindExistingLanes();
        
        if (existingLanes.Length >= 4)
        {
            Debug.Log("Lanes existantes trouvées, utilisation de celles-ci");
            for (int i = 0; i < 4; i++)
            {
                generatedLanes[i] = existingLanes[i].transform;
            }
        }
        else
        {
            Debug.Log("Création de nouvelles lanes");
            CreateNewLanes();
        }
    }

    /// <summary>
    /// Trouve les lanes existantes dans la scène
    /// </summary>
    private GameObject[] FindExistingLanes()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var lanes = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            if (objName.Contains("lane") || objName.Contains("voie") || objName.Contains("track"))
            {
                lanes.Add(obj);
            }
        }
        
        return lanes.ToArray();
    }

    /// <summary>
    /// Crée de nouvelles lanes si nécessaire
    /// </summary>
    private void CreateNewLanes()
    {
        GameObject lanesParent = new GameObject("CalibrationLanes");
        lanesParent.transform.SetParent(transform);
        
        float laneWidth = 2f; // Même valeur que dans LogParadePlayerAvatar
        Vector3 basePosition = Vector3.zero;
        
        for (int i = 0; i < 4; i++)
        {
            GameObject lane = new GameObject($"Lane_{i + 1}");
            lane.transform.SetParent(lanesParent.transform);
            
            // Calculer la position (même logique que LogParadePlayerAvatar)
            float xOffset = (i + 1 - 2.5f) * laneWidth;
            lane.transform.position = basePosition + Vector3.right * xOffset;
            
            generatedLanes[i] = lane.transform;
        }
        
        Debug.Log("4 nouvelles lanes créées");
    }

    /// <summary>
    /// Configure l'UI de calibration
    /// </summary>
    private void SetupCalibrationUI()
    {
        // Chercher un Canvas existant
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        
        if (existingCanvas == null)
        {
            CreateNewUICanvas();
        }
        else
        {
            CreateCalibrationUIInCanvas(existingCanvas);
        }
    }

    /// <summary>
    /// Crée un nouveau Canvas pour l'UI de calibration
    /// </summary>
    private void CreateNewUICanvas()
    {
        GameObject canvasGO = new GameObject("CalibrationUICanvas");
        canvasGO.transform.SetParent(transform);
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Au-dessus des autres UI
        
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        CreateCalibrationUIInCanvas(canvas);
        
        Debug.Log("Nouveau Canvas UI créé pour la calibration");
    }

    /// <summary>
    /// Crée les éléments UI de calibration dans un Canvas
    /// </summary>
    private void CreateCalibrationUIInCanvas(Canvas canvas)
    {
        // Créer le panel principal
        GameObject uiPanel = new GameObject("CalibrationUIPanel");
        uiPanel.transform.SetParent(canvas.transform, false);
        
        var rectTransform = uiPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        uiPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.5f);
        
        // Ajouter le script CalibrationTextUI
        calibrationUI = uiPanel.AddComponent<CalibrationTextUI>();
        
        // Créer le texte d'instruction
        CreateInstructionText(uiPanel);
        
        Debug.Log("UI de calibration créée");
    }

    /// <summary>
    /// Crée le texte d'instruction principal
    /// </summary>
    private void CreateInstructionText(GameObject parent)
    {
        GameObject textGO = new GameObject("InstructionText");
        textGO.transform.SetParent(parent.transform, false);
        
        var rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(800, 200);
        rectTransform.anchoredPosition = Vector2.zero;
        
        var textMesh = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        textMesh.text = "Instructions de calibration";
        textMesh.fontSize = 36;
        textMesh.color = Color.white;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        
        // Assigner à CalibrationTextUI si disponible
        if (calibrationUI != null)
        {
            // Utiliser la réflexion pour assigner le champ privé
            var field = typeof(CalibrationTextUI).GetField("mainInstructionText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(calibrationUI, textMesh);
        }
    }

    /// <summary>
    /// Configure les rondins de calibration
    /// </summary>
    private void SetupCalibrationLogs()
    {
        if (logPrefab == null)
        {
            // Essayer de trouver un prefab de rondin existant
            logPrefab = FindExistingLogPrefab();
        }
        
        if (logPrefab == null)
        {
            CreateSimpleLogPrefab();
        }
        
        Debug.Log($"Prefab de rondin configuré: {logPrefab.name}");
    }

    /// <summary>
    /// Trouve un prefab de rondin existant
    /// </summary>
    private GameObject FindExistingLogPrefab()
    {
        // Chercher dans la scène
        GameObject[] logs = GameObject.FindGameObjectsWithTag("Log");
        if (logs.Length > 0)
        {
            return logs[0];
        }
        
        return null;
    }

    /// <summary>
    /// Crée un prefab de rondin simple
    /// </summary>
    private void CreateSimpleLogPrefab()
    {
        GameObject logGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        logGO.name = "CalibrationLog";
        logGO.transform.rotation = Quaternion.Euler(0, 0, 90); // Coucher le cylindre
        logGO.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        
        // Ajouter un matériau marron
        var renderer = logGO.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.6f, 0.3f, 0.1f); // Marron
        }
        
        logPrefab = logGO;
        
        Debug.Log("Prefab de rondin simple créé");
    }

    /// <summary>
    /// Connecte tous les composants entre eux
    /// </summary>
    private void ConnectComponents()
    {
        if (calibrationSystem != null)
        {
            // Assigner les références au système de calibration
            AssignFieldValue(calibrationSystem, "playerAvatar", FindObjectOfType<LogParadePlayerAvatar>());
            AssignFieldValue(calibrationSystem, "lateralTracker", FindObjectOfType<LogParadeLateralTracker>());
            AssignFieldValue(calibrationSystem, "laneTransforms", generatedLanes);
            AssignFieldValue(calibrationSystem, "logPrefab", logPrefab);
            
            if (calibrationUI != null)
            {
                var instructionText = calibrationUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                AssignFieldValue(calibrationSystem, "instructionText", instructionText);
                AssignFieldValue(calibrationSystem, "calibrationUI", calibrationUI.gameObject);
            }
        }

        if (calibrationManager != null)
        {
            // Assigner les références au manager
            AssignFieldValue(calibrationManager, "calibrationSystem", calibrationSystem);
            AssignFieldValue(calibrationManager, "playerAvatar", FindObjectOfType<LogParadePlayerAvatar>());
            AssignFieldValue(calibrationManager, "uiManager", FindObjectOfType<LogParadeUIManager>());
            AssignFieldValue(calibrationManager, "gameController", FindObjectOfType<LogParadeGameController>());
        }
        
        Debug.Log("Composants connectés");
    }

    /// <summary>
    /// Assigne une valeur à un champ privé en utilisant la réflexion
    /// </summary>
    private void AssignFieldValue(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.Public);
        
        if (field != null && value != null)
        {
            field.SetValue(target, value);
            Debug.Log($"Champ {fieldName} assigné sur {target.GetType().Name}");
        }
        else if (field == null)
        {
            Debug.LogWarning($"Champ {fieldName} non trouvé sur {target.GetType().Name}");
        }
    }

    /// <summary>
    /// Valide que le setup est correct
    /// </summary>
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== Validation du setup ===");
        
        bool isValid = true;
        
        if (calibrationSystem == null)
        {
            Debug.LogError("Système de calibration manquant!");
            isValid = false;
        }
        
        if (calibrationManager == null)
        {
            Debug.LogError("Manager de calibration manquant!");
            isValid = false;
        }
        
        if (generatedLanes == null || generatedLanes.Length != 4)
        {
            Debug.LogError("4 lanes requises!");
            isValid = false;
        }
        
        if (logPrefab == null)
        {
            Debug.LogWarning("Prefab de rondin manquant - sera créé automatiquement");
        }
        
        var playerAvatar = FindObjectOfType<LogParadePlayerAvatar>();
        if (playerAvatar == null)
        {
            Debug.LogError("LogParadePlayerAvatar manquant dans la scène!");
            isValid = false;
        }
        
        if (isValid)
        {
            Debug.Log("✅ Setup valide!");
        }
        else
        {
            Debug.LogError("❌ Setup invalide - vérifiez les erreurs ci-dessus");
        }
    }

    /// <summary>
    /// Nettoie le setup (supprime les objets générés)
    /// </summary>
    [ContextMenu("Cleanup Setup")]
    public void CleanupSetup()
    {
        if (calibrationSystem != null)
        {
            DestroyImmediate(calibrationSystem.gameObject);
        }
        
        if (calibrationManager != null)
        {
            DestroyImmediate(calibrationManager.gameObject);
        }
        
        if (calibrationUI != null)
        {
            DestroyImmediate(calibrationUI.transform.root.gameObject);
        }
        
        // Nettoyer les lanes générées
        GameObject lanesParent = GameObject.Find("CalibrationLanes");
        if (lanesParent != null)
        {
            DestroyImmediate(lanesParent);
        }
        
        Debug.Log("Setup nettoyé");
    }
}
