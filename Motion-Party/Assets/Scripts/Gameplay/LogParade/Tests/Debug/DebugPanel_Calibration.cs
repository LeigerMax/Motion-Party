using UnityEngine;

/// <summary>
/// Panneau de debug pour afficher l'état de calibration LogParade.
/// Affiche le statut de calibration, les étapes, et permet des actions de test.
/// </summary>
public class DebugPanel_Calibration : BaseDebugPanel
{
#region Champs & Références
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
    #pragma warning disable CS0414
    private int calibrationProgress = 0;
    #pragma warning restore CS0414
    private bool calibrationRequired = true;
    private float calibrationTime = 0f;
#endregion

#region Initialisation & Cycle de Vie
    protected override void Start()
    {
        visibleAtStart = true;
        SetVisible(true);
        panelTitle = "Calibration";
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
#endregion

#region Affichage
    protected override void DrawPanelContent()
    {
        GUILayout.BeginVertical();
        DrawCalibrationStatusSection();
        GUILayout.Space(10);
        DrawControlsSection();
        GUILayout.EndVertical();
    }
    private void DrawCalibrationStatusSection()
    {
        GUILayout.Label("<b>📏 ÉTAT CALIBRATION</b>");
        string statusText = GetStatusText();
        string statusColor = GetStatusColor();
        GUILayout.Label($"Statut: <color={statusColor}>{statusText}</color>");
        if (calibrationTime > 0)
        {
            GUILayout.Label($"Durée: {FormatNumber(calibrationTime, 1)}s");
        }
        string requiredText = calibrationRequired ? "<color=orange>REQUIS</color>" : "<color=green>OPTIONNEL</color>";
        GUILayout.Label($"Mode: {requiredText}");
    }
    private void DrawControlsSection()
    {
        GUILayout.Label("<b>🎮 CONTRÔLES</b>");
        if (!isCalibrating && !isCalibrated)
        {
            if (GUILayout.Button("Démarrer Calibration"))
            {
                StartCalibration();
            }
        }
        if (isCalibrating)
        {
            if (GUILayout.Button("Arrêter Calibration"))
            {
                StopCalibration();
            }
        }
        if (isCalibrated)
        {
            if (GUILayout.Button("Recalibrer"))
            {
                RestartCalibration();
            }
        }
        if (GUILayout.Button("Reset Calibration"))
        {
            ResetCalibration();
        }
        if (!isCalibrated && GUILayout.Button("Bypass (Test)"))
        {
            BypassCalibration();
        }
    }
    protected override float GetEstimatedHeight()
    {
        return 200f;
    }
#endregion

#region Découverte & Rafraîchissement
    private void AutoFindCalibrationComponents()
    {
        if (calibrationManager == null)
        {
            calibrationManager = FindFirstObjectByType<LogParadeCalibrationManager>();
        }
        if (calibrationInteractive == null)
        {
            calibrationInteractive = FindFirstObjectByType<LogParadeCalibrationInteractive>();
        }
        // Log supprimé (inutile en UI)
    }
    private void RefreshCalibrationStatus()
    {
        if (calibrationManager != null)
        {
            isCalibrated = GetIsCalibrated();
            calibrationRequired = GetCalibrationRequired();
        }
        if (calibrationInteractive != null)
        {
            isCalibrating = GetIsCalibrating();
            calibrationTime = GetCalibrationTime();
        }
    }
#endregion

#region Accès Données (Réflexion)
    private bool GetIsCalibrated()
    {
        if (calibrationManager == null) return false;
        var method = calibrationManager.GetType().GetMethod("IsCalibrationCompleted");
        if (method != null && method.ReturnType == typeof(bool))
        {
            return (bool)method.Invoke(calibrationManager, null);
        }
        var calibrationCompletedField = calibrationManager.GetType().GetField("calibrationCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (calibrationCompletedField != null && calibrationCompletedField.FieldType == typeof(bool))
        {
            return (bool)calibrationCompletedField.GetValue(calibrationManager);
        }
        var possibleFields = new string[] { "isCalibrated", "_isCalibrated", "calibrated", "isCalibrationComplete", "m_IsCalibrated" };
        foreach (var fieldName in possibleFields)
        {
            var field = calibrationManager.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(calibrationManager);
            }
        }
        var possibleProperties = new string[] { "IsCalibrated", "Calibrated", "IsCalibrationComplete", "CalibrationCompleted" };
        foreach (var propName in possibleProperties)
        {
            var property = calibrationManager.GetType().GetProperty(propName);
            if (property != null && property.PropertyType == typeof(bool))
            {
                return (bool)property.GetValue(calibrationManager);
            }
        }
        if (calibrationInteractive != null)
        {
            var interactiveField = calibrationInteractive.GetType().GetField("isCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (interactiveField != null)
            {
                return (bool)interactiveField.GetValue(calibrationInteractive);
            }
        }
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
        var field = calibrationInteractive.GetType().GetField("isCalibrating", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(calibrationInteractive);
        }
        var prop = calibrationInteractive.GetType().GetProperty("IsCalibrating");
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            return (bool)prop.GetValue(calibrationInteractive);
        }
        return false;
    }
    private float GetCalibrationTime()
    {
        if (calibrationInteractive == null) return 0f;
        var field = calibrationInteractive.GetType().GetField("calibrationTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (float)field.GetValue(calibrationInteractive);
        }
        var prop = calibrationInteractive.GetType().GetProperty("CalibrationTime");
        if (prop != null && prop.PropertyType == typeof(float))
        {
            return (float)prop.GetValue(calibrationInteractive);
        }
        return 0f;
    }
#endregion

#region Actions Calibration
    private void StartCalibration()
    {
        if (calibrationInteractive != null)
        {
            var method = calibrationInteractive.GetType().GetMethod("StartCalibration");
            if (method != null)
            {
                method.Invoke(calibrationInteractive, null);
                LogParadeLogger.Log("[DebugPanel_Calibration] Calibration démarrée");
            }
        }
        else if (calibrationManager != null)
        {
            var method = calibrationManager.GetType().GetMethod("StartCalibration");
            if (method != null)
            {
                method.Invoke(calibrationManager, null);
                LogParadeLogger.Log("[DebugPanel_Calibration] Calibration démarrée via Manager");
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
                LogParadeLogger.Log("[DebugPanel_Calibration] Calibration arrêtée");
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
                LogParadeLogger.Log("[DebugPanel_Calibration] Calibration reset");
                return;
            }
        }
        if (calibrationInteractive != null)
        {
            var method = calibrationInteractive.GetType().GetMethod("ResetCalibration");
            if (method != null)
            {
                method.Invoke(calibrationInteractive, null);
                LogParadeLogger.Log("[DebugPanel_Calibration] Calibration reset via Interactive");
            }
        }
    }
    private void BypassCalibration()
    {
        if (calibrationManager != null)
        {
            var field = calibrationManager.GetType().GetField("isCalibrated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(calibrationManager, true);
                LogParadeLogger.Log("[DebugPanel_Calibration] Calibration bypassée");
            }
            else
            {
                LogParadeLogger.LogError("[DebugPanel_Calibration] Impossible de bypass : champ 'isCalibrated' introuvable");
            }
        }
        else
        {
            LogParadeLogger.LogError("[DebugPanel_Calibration] Impossible de bypass : calibrationManager null");
        }
    }
#endregion

#region Utilitaires Affichage
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
#endregion
}
