using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validateur centralisé pour tous les systèmes LogParade.
/// Gère la validation, la récupération automatique des composants, 
/// et les diagnostics de santé du système.
/// </summary>
public class LogParadeSystemValidator : MonoBehaviour
{
    #region Fields
    
    [Header("Validation Settings")]
    [SerializeField] private bool enableAutoValidation = true;
    [SerializeField] private float validationInterval = 5f;
    [SerializeField] private bool enableDetailedLogs = true;

    [Header("Required Systems")]
    [SerializeField] private bool requireGameController = true;
    [SerializeField] private bool requireGameTimer = true;
    [SerializeField] private bool requireLogGenerator = true;
    [SerializeField] private bool requireScoreManager = true;
    [SerializeField] private bool requireCalibrationManager = true;
    // Cache des composants validés
    private Dictionary<System.Type, Component> componentCache = new Dictionary<System.Type, Component>();
    
    // État de validation
    private bool isSystemHealthy = false;
    private float lastValidationTime = 0f;
    
    // Événements
    public System.Action<bool> OnSystemHealthChanged;
    public System.Action<string> OnValidationFailed;
    public System.Action OnValidationCompleted;
    #endregion

    #region Singleton
    // Instance singleton pour accès facile
    private static LogParadeSystemValidator instance;
    public static LogParadeSystemValidator Instance => instance;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LogStatus("SystemValidator initialisé");
        
        if (enableAutoValidation)
        {
            // Validation initiale
            ValidateAllSystems();
        }
    }

    void Update()
    {
        if (enableAutoValidation && Time.time - lastValidationTime >= validationInterval)
        {
            ValidateAllSystems();
        }
    }
    #endregion

    #region Validation

    /// <summary>
    /// Valide tous les systèmes requis
    /// </summary>
    public bool ValidateAllSystems()
    {
        lastValidationTime = Time.time;
        bool allValid = true;
        List<string> missingComponents = new List<string>();

        LogStatus("🔍 Validation des systèmes en cours...");

        // Valider chaque système requis
        if (requireGameController && !ValidateComponent<LogParadeGameController>("GameController"))
        {
            allValid = false;
            missingComponents.Add("GameController");
        }

        if (requireGameTimer && !ValidateComponent<LogParadeGameTimer>("GameTimer"))
        {
            allValid = false;
            missingComponents.Add("GameTimer");
        }

        if (requireLogGenerator && !ValidateComponent<LogParadeLogGenerator>("LogGenerator"))
        {
            allValid = false;
            missingComponents.Add("LogGenerator");
        }

        if (requireScoreManager && !ValidateComponent<LogParadeScoreManager>("ScoreManager"))
        {
            allValid = false;
            missingComponents.Add("ScoreManager");
        }

        if (requireCalibrationManager && !ValidateComponent<LogParadeCalibrationManager>("CalibrationManager"))
        {
            allValid = false;
            missingComponents.Add("CalibrationManager");
        }

        // Optionnels (utiles mais pas bloquants)
        ValidateComponent<LogParadeUIManager>("UIManager", false);
        ValidateComponent<LogParadePlayerAvatar>("PlayerAvatar", false);
        ValidateComponent<LogParadeGameLauncher>("GameLauncher", false);
        ValidateComponent<LogParadeGameStateController>("GameStateController", false);

        // Mettre à jour l'état de santé
        bool previousHealth = isSystemHealthy;
        isSystemHealthy = allValid;

        if (isSystemHealthy)
        {
            LogStatus($" Validation terminée - Tous les systèmes sont opérationnels");
            OnValidationCompleted?.Invoke();
        }
        else
        {
            string missingList = string.Join(", ", missingComponents);
            LogError($" Validation échouée - Composants manquants: {missingList}");
            OnValidationFailed?.Invoke(missingList);
        }

        // Notifier si l'état de santé a changé
        if (previousHealth != isSystemHealthy)
        {
            OnSystemHealthChanged?.Invoke(isSystemHealthy);
        }

        return isSystemHealthy;
    }

    /// <summary>
    /// Valide un composant spécifique
    /// </summary>
    private bool ValidateComponent<T>(string componentName, bool isRequired = true) where T : Component
    {
        System.Type componentType = typeof(T);
        
        // Vérifier le cache d'abord
        if (componentCache.ContainsKey(componentType) && componentCache[componentType] != null)
        {
            LogStatus($"  ✅ {componentName} (depuis cache)");
            return true;
        }        // Rechercher le composant
        T component = FindFirstObjectByType<T>();
        
        if (component != null)
        {
            // Mettre en cache
            componentCache[componentType] = component;
            LogStatus($"  ✅ {componentName} trouvé et mis en cache");
            return true;
        }
        else
        {
            if (isRequired)
            {
                LogError($"  ❌ {componentName} MANQUANT (requis)");
            }
            else
            {
                LogWarning($"  ⚠️ {componentName} non trouvé (optionnel)");
            }
            return false;
        }
    }

    #endregion

    #region Component Retrieval

    /// <summary>
    /// Récupère un composant validé depuis le cache
    /// </summary>
    public T GetValidatedComponent<T>() where T : Component
    {
        System.Type componentType = typeof(T);
        
        if (componentCache.ContainsKey(componentType) && componentCache[componentType] != null)
        {
            return componentCache[componentType] as T;
        }
          // Si pas en cache, essayer de le trouver
        T component = FindFirstObjectByType<T>();
        if (component != null)
        {
            componentCache[componentType] = component;
            return component;
        }
        
        return null;
    }

    /// <summary>
    /// Force la recherche et mise en cache d'un composant
    /// </summary>
    public T RefreshComponent<T>() where T : Component
    {
        System.Type componentType = typeof(T);
        
        // Supprimer du cache
        if (componentCache.ContainsKey(componentType))
        {
            componentCache.Remove(componentType);
        }
          // Rechercher à nouveau
        T component = FindFirstObjectByType<T>();
        if (component != null)
        {
            componentCache[componentType] = component;
            LogStatus($"Composant {typeof(T).Name} rafraîchi et mis en cache");
        }
        
        return component;
    }

    #endregion

    #region Diagnostics

    /// <summary>
    /// Génère un rapport complet de l'état du système
    /// </summary>
    public string GenerateSystemReport()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("📋 RAPPORT D'ÉTAT DU SYSTÈME LOGPARADE");
        report.AppendLine($"Santé générale: {(isSystemHealthy ? " SAIN" : " PROBLÈMES DÉTECTÉS")}");
        report.AppendLine($"Dernière validation: {System.DateTime.Now:HH:mm:ss}");
        report.AppendLine($"Composants en cache: {componentCache.Count}");
        report.AppendLine();
        
        report.AppendLine("🔧 Composants requis:");
        CheckAndReportComponent<LogParadeGameController>("GameController", requireGameController, report);
        CheckAndReportComponent<LogParadeGameTimer>("GameTimer", requireGameTimer, report);
        CheckAndReportComponent<LogParadeLogGenerator>("LogGenerator", requireLogGenerator, report);
        CheckAndReportComponent<LogParadeScoreManager>("ScoreManager", requireScoreManager, report);
        CheckAndReportComponent<LogParadeCalibrationManager>("CalibrationManager", requireCalibrationManager, report);
        
        report.AppendLine();
        report.AppendLine("🔧 Composants optionnels:");
        CheckAndReportComponent<LogParadeUIManager>("UIManager", false, report);
        CheckAndReportComponent<LogParadePlayerAvatar>("PlayerAvatar", false, report);
        CheckAndReportComponent<LogParadeGameLauncher>("GameLauncher", false, report);
        CheckAndReportComponent<LogParadeGameStateController>("GameStateController", false, report);
        
        return report.ToString();
    }

    private void CheckAndReportComponent<T>(string name, bool isRequired, System.Text.StringBuilder report) where T : Component
    {
        T component = GetValidatedComponent<T>();
        string status = component != null ? "✅" : (isRequired ? "❌" : "⚠️");
        string requiredText = isRequired ? "(requis)" : "(optionnel)";
        report.AppendLine($"  {status} {name} {requiredText}");
    }

    /// <summary>
    /// Vide le cache des composants (force une re-validation)
    /// </summary>
    public void ClearComponentCache()
    {
        componentCache.Clear();
        LogStatus("Cache des composants vidé - prochaine validation sera complète");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Vérifie si le système est en bonne santé
    /// </summary>
    public bool IsSystemHealthy()
    {
        return isSystemHealthy;
    }

    /// <summary>
    /// Force une validation immédiate
    /// </summary>
    public bool ForceValidation()
    {
        return ValidateAllSystems();
    }

    /// <summary>
    /// Active/désactive la validation automatique
    /// </summary>
    public void SetAutoValidation(bool enable)
    {
        enableAutoValidation = enable;
        LogStatus($"Validation automatique {(enable ? "activée" : "désactivée")}");
    }

    #endregion

    #region Logging

    private void LogStatus(string message)
    {
        if (enableDetailedLogs)
            LogParadeLogger.LogVerbose($"{message}");
    }

    private void LogWarning(string message)
    {
        LogParadeLogger.LogWarning($"{message}");
    }

    private void LogError(string message)
    {
        LogParadeLogger.LogError($"{message}");
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Debug: affiche le rapport complet dans la console
    /// </summary>
    [ContextMenu("Debug System Report")]
    public void DebugSystemReport()
    {
        LogParadeLogger.Log(GenerateSystemReport());
    }

    /// <summary>
    /// Debug: force une validation et affiche le résultat
    /// </summary>
    [ContextMenu("Force Validation")]
    public void DebugForceValidation()
    {
        bool result = ForceValidation();
        LogParadeLogger.LogVerbose($"Validation forcée - Résultat: {(result ? "SUCCÈS" : "ÉCHEC")}");
    }

    #endregion
}
