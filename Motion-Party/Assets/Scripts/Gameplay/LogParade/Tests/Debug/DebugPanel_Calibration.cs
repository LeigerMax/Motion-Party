using UnityEngine;

/// <summary>
/// Panneau de debug pour afficher l'état de calibration LogParade.
/// Affiche le statut de calibration, les étapes, et permet des actions de test.
/// </summary>
public class DebugPanel_Calibration : BaseDebugPanel
{
    [Header("Calibration References")]
    [Tooltip("Référence au LogParadeCalibrationManager")]
    [SerializeField] private LogParadeCalibrationManager calibrationManager;
    
    [Tooltip("Référence au LogParadeCalibrationInteractive")]
    [SerializeField] private LogParadeCalibrationInteractive calibrationInteractive;
    
    [Tooltip("Recherche automatiquement les composants de calibration")]
    [SerializeField] private bool autoFindCalibrationComponents = true;
    
    // Données en cache
    private bool isCalibrated = false;
    private bool isCalibrating = false;
    private int calibrationProgress = 0;
    private bool calibrationRequired = true;
    private float calibrationTime = 0f;

    protected override void Start()
    {
        visibleAtStart = true;
        SetVisible(true);
        
        panelTitle = "Calibration";
        
        // Auto-découverte des composants
        if (autoFindCalibrationComponents)
        {
            AutoFindCalibrationComponents();
        }
        
        base.Start();
    }

    protected override void RefreshData()
    {
        RefreshCalibrationStatus();
    }

    protected override void DrawPanelContent()
    {
        GUILayout.BeginVertical();
        // Section état de calibration (statut uniquement)
        DrawCalibrationStatusSection();
        GUILayout.Space(10);
        // Section contrôles
        DrawControlsSection();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// Dessine la section état de calibration (statut uniquement)
    /// </summary>
    private void DrawCalibrationStatusSection()
    {
        GUILayout.Label("<b>📏 ÉTAT CALIBRATION</b>");
        // Statut principal
        string statusText = GetStatusText();
        string statusColor = GetStatusColor();
        GUILayout.Label($"Statut: <color={statusColor}>{statusText}</color>");
        // Temps de calibration (optionnel)
        if (calibrationTime > 0)
        {
            GUILayout.Label($"Durée: {FormatNumber(calibrationTime, 1)}s");
        }
        // Requis ou non
        string requiredText = calibrationRequired ? "<color=orange>REQUIS</color>" : "<color=green>OPTIONNEL</color>";
        GUILayout.Label($"Mode: {requiredText}");
    }

    /// <summary>
    /// Dessine la section contrôles
    /// </summary>
    private void DrawControlsSection()
    {
        GUILayout.Label("<b>🎮 CONTRÔLES</b>");
        
        // Bouton démarrer calibration
        if (!isCalibrating && !isCalibrated)
        {
            if (GUILayout.Button("Démarrer Calibration"))
            {
                StartCalibration();
            }
        }
        
        // Bouton arrêter calibration
        if (isCalibrating)
        {
            if (GUILayout.Button("Arrêter Calibration"))
            {
                StopCalibration();
            }
        }
        
        // Bouton recalibrer
        if (isCalibrated)
        {
            if (GUILayout.Button("Recalibrer"))
            {
                RestartCalibration();
            }
        }
        
        // Bouton reset
        if (GUILayout.Button("Reset Calibration"))
        {
            ResetCalibration();
        }
        
        // Bouton bypass (pour tests)
        if (!isCalibrated && GUILayout.Button("Bypass (Test)"))
        {
            BypassCalibration();
        }
    }

    protected override float GetEstimatedHeight()
    {
        return 200f;
    }

    /// <summary>
    /// Auto-découverte des composants de calibration
    /// </summary>
    private void AutoFindCalibrationComponents()
    {        if (calibrationManager == null)
        {
            calibrationManager = FindFirstObjectByType<LogParadeCalibrationManager>();
        }
        
        if (calibrationInteractive == null)
        {
            calibrationInteractive = FindFirstObjectByType<LogParadeCalibrationInteractive>();
        }
        
        Debug.Log($"[DebugPanel_Calibration] Composants trouvés - " +
                  $"Manager: {calibrationManager != null}, " +
                  $"Interactive: {calibrationInteractive != null}");
    }

    /// <summary>
    /// Rafraîchit l'état de calibration
    /// </summary>
    private void RefreshCalibrationStatus()
    {
        // Données depuis CalibrationManager
        if (calibrationManager != null)
        {
            isCalibrated = GetIsCalibrated();
            calibrationRequired = GetCalibrationRequired();
        }
        
        // Données depuis CalibrationInteractive
        if (calibrationInteractive != null)
        {
            isCalibrating = GetIsCalibrating();
            calibrationTime = GetCalibrationTime();
        }
    }    /// <summary>
    /// Méthodes d'accès aux données privées via réflexion
    /// </summary>
    private bool GetIsCalibrated()
    {
        if (calibrationManager == null) return false;
        
        // Première priorité : méthode publique IsCalibrationCompleted()
        var method = calibrationManager.GetType().GetMethod("IsCalibrationCompleted");
        if (method != null && method.ReturnType == typeof(bool))
        {
            return (bool)method.Invoke(calibrationManager, null);
        }
        
        // Deuxième priorité : champ privé calibrationCompleted (identifié dans le code source)
        var calibrationCompletedField = calibrationManager.GetType().GetField("calibrationCompleted", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (calibrationCompletedField != null && calibrationCompletedField.FieldType == typeof(bool))
        {
            return (bool)calibrationCompletedField.GetValue(calibrationManager);
        }
        
        // Essayer d'autres noms de champs possibles
        var possibleFields = new string[] { "isCalibrated", "_isCalibrated", "calibrated", "isCalibrationComplete", "m_IsCalibrated" };
        
        foreach (var fieldName in possibleFields)
        {
            var field = calibrationManager.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(calibrationManager);
            }
        }
        
        // Essayer via propriétés publiques
        var possibleProperties = new string[] { "IsCalibrated", "Calibrated", "IsCalibrationComplete", "CalibrationCompleted" };
        
        foreach (var propName in possibleProperties)
        {
            var property = calibrationManager.GetType().GetProperty(propName);
            if (property != null && property.PropertyType == typeof(bool))
            {
                return (bool)property.GetValue(calibrationManager);
            }
        }
        
        // Méthode alternative : vérifier via CalibrationInteractive
        if (calibrationInteractive != null)
        {
            var interactiveField = calibrationInteractive.GetType().GetField("isCompleted", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (interactiveField != null)
            {
                return (bool)interactiveField.GetValue(calibrationInteractive);
            }
        }
        
        Debug.LogWarning("[DebugPanel_Calibration] Impossible de trouver l'état de calibration (isCalibrated)");
        return false;
    }

    private bool GetCalibrationRequired()
    {
        if (calibrationManager == null) return true;
        
        var field = calibrationManager.GetType().GetField("preventGameplayUntilCalibrated");
        if (field != null)
        {
            return (bool)field.GetValue(calibrationManager);
        }
        // Ajout fallback
        var prop = calibrationManager.GetType().GetProperty("CalibrationRequired");
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            return (bool)prop.GetValue(calibrationManager);
        }
        return true;
    }

    private bool GetIsCalibrating()
    {
        if (calibrationInteractive == null) return false;
        
        var field = calibrationInteractive.GetType().GetField("isCalibrating", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(calibrationInteractive);
        }
        // Ajout fallback
        var prop = calibrationInteractive.GetType().GetProperty("IsCalibrating");
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            return (bool)prop.GetValue(calibrationInteractive);
        }
        Debug.LogWarning("[DebugPanel_Calibration] Impossible de trouver l'état de calibration (isCalibrating)");
        return false;
    }

    private float GetCalibrationTime()
    {
        if (calibrationInteractive == null) return 0f;
        
        var field = calibrationInteractive.GetType().GetField("calibrationTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (float)field.GetValue(calibrationInteractive);
        }
        // Ajout fallback
        var prop = calibrationInteractive.GetType().GetProperty("CalibrationTime");
        if (prop != null && prop.PropertyType == typeof(float))
        {
            return (float)prop.GetValue(calibrationInteractive);
        }
        Debug.LogWarning("[DebugPanel_Calibration] Impossible de trouver le temps de calibration");
        return 0f;
    }

    /// <summary>
    /// Actions de contrôle
    /// </summary>
    private void StartCalibration()
    {
        if (calibrationInteractive != null)
        {
            var method = calibrationInteractive.GetType().GetMethod("StartCalibration");
            if (method != null)
            {
                method.Invoke(calibrationInteractive, null);
                Debug.Log("[DebugPanel_Calibration] Calibration démarrée");
            }
        }
        else if (calibrationManager != null)
        {
            var method = calibrationManager.GetType().GetMethod("StartCalibration");
            if (method != null)
            {
                method.Invoke(calibrationManager, null);
                Debug.Log("[DebugPanel_Calibration] Calibration démarrée via Manager");
            }
        }
    }

    private void StopCalibration()
    {
        if (calibrationInteractive != null)
        {
            var method = calibrationInteractive.GetType().GetMethod("StopCalibration");
            if (method != null)
            {
                method.Invoke(calibrationInteractive, null);
                Debug.Log("[DebugPanel_Calibration] Calibration arrêtée");
            }
        }
    }

    private void RestartCalibration()
    {
        ResetCalibration();
        StartCalibration();
    }

    private void ResetCalibration()
    {
        if (calibrationManager != null)
        {
            var method = calibrationManager.GetType().GetMethod("ResetCalibration");
            if (method != null)
            {
                method.Invoke(calibrationManager, null);
                Debug.Log("[DebugPanel_Calibration] Calibration reset");
                return;
            }
        }
        
        if (calibrationInteractive != null)
        {
            var method = calibrationInteractive.GetType().GetMethod("ResetCalibration");
            if (method != null)
            {
                method.Invoke(calibrationInteractive, null);
                Debug.Log("[DebugPanel_Calibration] Calibration reset via Interactive");
            }
        }
    }

    private void BypassCalibration()
    {
        if (calibrationManager != null)
        {
            // Forcer l'état calibré
            var field = calibrationManager.GetType().GetField("isCalibrated", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(calibrationManager, true);
                Debug.Log("[DebugPanel_Calibration] Calibration bypassée");
            }
        }
    }

    /// <summary>
    /// Méthodes utilitaires
    /// </summary>
    private string GetStatusText()
    {
        if (isCalibrating) return "EN COURS";
        if (isCalibrated) return "TERMINÉE";
        return "NON CALIBRÉ";
    }

    private string GetStatusColor()
    {
        if (isCalibrating) return "yellow";
        if (isCalibrated) return "green";
        return "red";
    }
}
