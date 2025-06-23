using UnityEngine;

/// <summary>
/// Configuration globale pour le mini-jeu LogParade
/// ScriptableObject pour stocker les paramètres réutilisables
/// </summary>
[CreateAssetMenu(fileName = "LogParadeConfig", menuName = "LogParade/Game Configuration")]
public class LogParadeConfig : ScriptableObject
{
    [Header("Tracking Settings")]
    [Range(0.1f, 1.0f)]
    public float defaultSmoothingFactor = 0.8f;
    
    [Range(-5.0f, 5.0f)]
    public float defaultLeftBoundary = -1.5f;
    
    [Range(-5.0f, 5.0f)]
    public float defaultRightBoundary = 1.5f;
    
    [Header("Calibration")]
    public bool enableAutoCalibration = true;
    public float calibrationTime = 3.0f;
    
    [Header("Avatar Movement")]
    public float avatarMoveSpeed = 5f;
    public float laneWidth = 2f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    public Color[] laneColors = new Color[4] 
    { 
        Color.red,      // Voie 1 - Gauche
        Color.yellow,   // Voie 2 - Centre-gauche  
        Color.green,    // Voie 3 - Centre-droite
        Color.blue      // Voie 4 - Droite
    };
    
    public Color activeLaneColor = Color.green;
    public Color inactiveLaneColor = Color.gray;    [Header("Audio")]
    public AudioClip laneChangeSound;
    public AudioClip backgroundMusic;
    
    [Header("Debug")]
    public bool showDebugInfoByDefault = true;
    public bool enableInputSimulator = false;
    
    /// <summary>
    /// Applique cette configuration aux composants LogParade
    /// </summary>
    public void ApplyToComponents()
    {
        // Trouver et configurer le tracker
        LogParadeLateralTracker tracker = FindObjectOfType<LogParadeLateralTracker>();
        if (tracker != null)
        {
            tracker.smoothingFactor = defaultSmoothingFactor;
            tracker.leftBoundary = defaultLeftBoundary;
            tracker.rightBoundary = defaultRightBoundary;
            tracker.enableAutoCalibration = enableAutoCalibration;
            tracker.calibrationTime = calibrationTime;
            tracker.showDebugInfo = showDebugInfoByDefault;
        }
        
        // Trouver et configurer l'avatar
        LogParadePlayerAvatar avatar = FindObjectOfType<LogParadePlayerAvatar>();
        if (avatar != null)
        {
            avatar.moveSpeed = avatarMoveSpeed;
            avatar.laneWidth = laneWidth;
            avatar.movementCurve = movementCurve;
            avatar.laneChangeSound = laneChangeSound;
            avatar.showDebugInfo = showDebugInfoByDefault;
        }        // Trouver et configurer le visualisateur
        LogParadeLaneVisualizer visualizer = FindObjectOfType<LogParadeLaneVisualizer>();
        if (visualizer != null)
        {
            visualizer.laneWidth = laneWidth;
            visualizer.laneColors = laneColors;
        }
        
        // Trouver et configurer l'UI
        LogParadeUIManager uiManager = FindObjectOfType<LogParadeUIManager>();
        if (uiManager != null)
        {
            uiManager.activeLaneColor = activeLaneColor;
            uiManager.inactiveLaneColor = inactiveLaneColor;
        }
        
        // Configurer le simulateur si nécessaire
        LogParadeInputSimulator simulator = FindObjectOfType<LogParadeInputSimulator>();
        if (simulator != null)
        {
            simulator.enableSimulation = enableInputSimulator;
        }
        
        Debug.Log("Configuration LogParade appliquée aux composants de la scène.");
    }
    
    /// <summary>
    /// Réinitialise les valeurs par défaut
    /// </summary>
    [ContextMenu("Reset to Defaults")]
    public void ResetToDefaults()
    {
        defaultSmoothingFactor = 0.8f;
        defaultLeftBoundary = -1.5f;
        defaultRightBoundary = 1.5f;
        enableAutoCalibration = true;
        calibrationTime = 3.0f;
        avatarMoveSpeed = 5f;
        laneWidth = 2f;
        movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        laneColors = new Color[4] 
        { 
            Color.red, Color.yellow, Color.green, Color.blue
        };
        
        activeLaneColor = Color.green;
        inactiveLaneColor = Color.gray;
        showDebugInfoByDefault = true;
        enableInputSimulator = false;
    }
    
    void OnValidate()
    {
        // S'assurer que les boundaries sont logiques
        if (defaultLeftBoundary >= defaultRightBoundary)
        {
            defaultRightBoundary = defaultLeftBoundary + 1f;
        }
        
        // S'assurer qu'on a bien 4 couleurs pour les voies
        if (laneColors == null || laneColors.Length != 4)
        {
            laneColors = new Color[4] { Color.red, Color.yellow, Color.green, Color.blue };
        }
    }
}
