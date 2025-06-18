using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// Responsable du tracking latéral du joueur et du mapping sur les 4 voies
/// Récupère les données MediaPipe pour détecter la position X de la tête
/// Version 1.2 - Calibration avancée et système anti-sensibilité
/// </summary>
public class LogParadeLateralTracker : MonoBehaviour
{
    [Header("UDP Settings")]
    public UDPReceive udpReceive;

    [Header("Camera Calibration")]
    [Tooltip("Largeur de l'image de la caméra MediaPipe (ex: 640)")]
    public int cameraInputWidth = 640;
    
    [Tooltip("Hauteur de l'image de la caméra MediaPipe (ex: 480)")]
    public int cameraInputHeight = 480;
    
    [Tooltip("Facteur d'échelle pour adapter le tracking à la taille réelle")]
    [Range(0.5f, 3.0f)]
    public float trackingScale = 1.0f;
    
    [Tooltip("Zone morte centrale pour éviter les micro-mouvements (en pixels)")]
    [Range(10f, 100f)]
    public float centralDeadZone = 30f;

    [Header("Tracking Settings")]
    [Range(0.1f, 1.0f)]
    public float smoothingFactor = 0.8f;
    [Range(-2.0f, 2.0f)]
    public float leftBoundary = -1.5f;
    [Range(-2.0f, 2.0f)]
    public float rightBoundary = 1.5f;
    
    [Header("Lane Change Sensitivity")]
    [Tooltip("Temps minimum à maintenir dans une position avant changement de voie")]
    [Range(0.2f, 3.0f)]
    public float laneChangeValidationTime = 1.0f;
    
    [Tooltip("Seuil minimum de mouvement pour déclencher un changement de voie")]
    [Range(0.1f, 2.0f)]
    public float laneChangeThreshold = 0.3f;
    
    [Tooltip("Si activé, les changements nécessitent une validation temporelle")]
    public bool requireLaneChangeValidation = true;
    
    [Header("Calibration")]
    public bool enableAutoCalibration = true;
    public float calibrationTime = 3.0f;
    public bool continuousCalibration = true;
    
    [Tooltip("Afficher le guide de position centrale pendant la calibration")]
    public bool showCenterGuide = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Events
    public event Action<int> OnLaneChanged;
    public event Action<Vector3> OnPositionUpdated;
    public event Action<int> OnLaneChangePreview;
    public event Action<bool> OnCalibrationStateChanged; // Nouveau pour l'UI

    // Private fields
    private Vector3 currentPosition;
    private Vector3 smoothedPosition;
    private int currentLane = 2; // Démarre au centre (lane 1-4)
    private bool isCalibrated = false;
    private float calibrationTimer = 0f;
    private Vector3 calibrationCenter;
    private int calibrationSamples = 0;
    private string trackingSource = "None";
    
    // Validation de changement de voie
    private int pendingLane = -1;
    private float laneValidationTimer = 0f;
    private Vector3 lastStablePosition;
    
    // Calibration avancée
    private float baseXPosition = 0f; // Position X de référence (centre)
    private float minObservedX = float.MaxValue;
    private float maxObservedX = float.MinValue;
    private float effectiveTrackingWidth = 0f;
    
    // Offset pour conversion pixels -> coordonnées Unity
    private int offset = 70;

    void Start()
    {
        if (udpReceive == null)
        {
            Debug.LogError("UDPReceive n'est pas assigné dans LogParadeLateralTracker !");
            return;
        }

        // Initialiser la position au centre
        smoothedPosition = Vector3.zero;
        currentPosition = Vector3.zero;
        lastStablePosition = Vector3.zero;
        
        if (enableAutoCalibration)
        {
            StartCalibration();
        }
        else
        {
            isCalibrated = true;
        }
    }

    void Update()
    {
        if (!ProcessUDPData()) return;

        // Toujours mettre à jour la position, même pendant la calibration
        UpdatePosition();
        UpdateLane();

        // Gérer la calibration en parallèle
        if (enableAutoCalibration && !isCalibrated)
        {
            UpdateCalibration();
        }
    }

    /// <summary>
    /// Traite les données UDP reçues de MediaPipe avec calibration caméra
    /// </summary>
    private bool ProcessUDPData()
    {
        string data = udpReceive.data;
        if (string.IsNullOrEmpty(data)) return false;

        try
        {
            // Debug du JSON reçu
            if (showDebugInfo && Time.frameCount % 60 == 0)
            {
                Debug.Log($"JSON reçu: '{data.Substring(0, Mathf.Min(100, data.Length))}...'");
            }
            
            data = data.Trim();
            JObject jsonData = JObject.Parse(data);
            
            // Priorité 1: Utiliser les landmarks de pose (tête = landmark 0)
            JArray poseLandmarks = (JArray)jsonData["pose_landmarks"];
            if (poseLandmarks != null && poseLandmarks.Count > 0)
            {
                // Récupérer la position brute en pixels
                float rawX = (float)poseLandmarks[0][0];
                float rawY = (float)poseLandmarks[0][1];
                float rawZ = (float)poseLandmarks[0][2];
                
                // Normaliser selon la taille de la caméra
                float normalizedX = (rawX / cameraInputWidth) - 0.5f; // -0.5 à +0.5
                float normalizedY = (rawY / cameraInputHeight) - 0.5f;
                
                // Appliquer l'échelle de tracking
                float scaledX = normalizedX * trackingScale;
                float scaledY = normalizedY * trackingScale;
                
                currentPosition = new Vector3(scaledX, scaledY, rawZ * 0.01f);
                trackingSource = "Head (Pose)";
                
                // Mettre à jour les bornes observées pendant la calibration
                if (!isCalibrated && enableAutoCalibration)
                {
                    minObservedX = Mathf.Min(minObservedX, scaledX);
                    maxObservedX = Mathf.Max(maxObservedX, scaledX);
                }
                
                return true;
            }
            
            // Fallback: Utiliser les données de main
            JArray handPositions = (JArray)jsonData["hand_positions"];
            if (handPositions != null && handPositions.Count > 0)
            {
                float rawX = (float)handPositions[0][0];
                float rawY = (float)handPositions[0][1];
                float rawZ = (float)handPositions[0][2];
                
                // Appliquer la même normalisation
                float normalizedX = (rawX / cameraInputWidth) - 0.5f;
                float normalizedY = (rawY / cameraInputHeight) - 0.5f;
                float scaledX = normalizedX * trackingScale;
                float scaledY = normalizedY * trackingScale;
                
                currentPosition = new Vector3(scaledX, scaledY, rawZ * 0.01f);
                trackingSource = "Hand (Fallback)";
                return true;
            }
            
            trackingSource = "No Data";
        }
        catch (Exception e)
        {
            if (showDebugInfo)
                Debug.LogWarning($"Erreur lors du parsing des données UDP : {e.Message}");
        }

        return false;
    }    /// <summary>
    /// Démarre la calibration automatique avec guide central
    /// </summary>
    private void StartCalibration()
    {
        isCalibrated = false;
        calibrationTimer = 0f;
        calibrationCenter = Vector3.zero;
        calibrationSamples = 0;
        minObservedX = float.MaxValue;
        maxObservedX = float.MinValue;
        
        // Notifier l'UI de démarrer la calibration
        OnCalibrationStateChanged?.Invoke(true);
        
        // Afficher le guide de calibration centrale via l'UI
        var uiManager = FindObjectOfType<LogParadeUIManager>();
        if (uiManager != null && showCenterGuide)
        {
            uiManager.ShowCenterGuide();
        }
        
        if (showDebugInfo)
            Debug.Log("🎯 Calibration démarrée - Placez-vous au CENTRE et restez immobile !");
    }    /// <summary>
    /// Met à jour la calibration avec détection automatique des bornes
    /// </summary>
    private void UpdateCalibration()
    {
        calibrationTimer += Time.deltaTime;
        
        // Accumuler les échantillons pour calculer la position centrale
        calibrationCenter += currentPosition;
        calibrationSamples++;
        
        // Notifier l'UI du progrès avec le guide central
        float progress = calibrationTimer / calibrationTime;
        var uiManager = FindObjectOfType<LogParadeUIManager>();
        if (uiManager != null)
        {
            uiManager.ShowCalibrationUI(progress);
            if (showCenterGuide)
            {
                uiManager.UpdateCenterGuide(progress);
            }
        }
        
        if (calibrationTimer >= calibrationTime)
        {
            // Finaliser la calibration
            if (calibrationSamples > 0)
            {
                calibrationCenter /= calibrationSamples;
                baseXPosition = calibrationCenter.x;
                
                // Calculer la largeur effective de tracking
                if (maxObservedX > minObservedX)
                {
                    effectiveTrackingWidth = maxObservedX - minObservedX;
                    if (showDebugInfo)
                        Debug.Log($"📏 Largeur de tracking détectée : {effectiveTrackingWidth:F2}");
                }
                
                isCalibrated = true;
                OnCalibrationStateChanged?.Invoke(false);
                
                if (uiManager != null)
                {
                    uiManager.HideCalibrationUI();
                    if (showCenterGuide)
                    {
                        uiManager.HideCenterGuide();
                    }
                }
                
                if (showDebugInfo)
                    Debug.Log($"✅ Calibration terminée. Centre : {calibrationCenter}, Base X : {baseXPosition:F2}");
            }
        }
    }

    /// <summary>
    /// Met à jour la position avec la nouvelle logique de calibration
    /// </summary>
    private void UpdatePosition()
    {
        // Appliquer la zone morte centrale
        float deltaX = currentPosition.x - baseXPosition;
        if (Mathf.Abs(deltaX) < (centralDeadZone / cameraInputWidth) * trackingScale)
        {
            deltaX = 0f; // Annuler les micro-mouvements
        }
        
        // Calibration continue : ajuster automatiquement le centre
        if (continuousCalibration && !enableAutoCalibration)
        {
            baseXPosition = Mathf.Lerp(baseXPosition, currentPosition.x, 0.001f);
        }
        
        // Position ajustée avec la base centrale
        Vector3 adjustedPosition = new Vector3(deltaX, currentPosition.y, currentPosition.z);
        
        // Lisser la position pour éviter les tremblements
        smoothedPosition = Vector3.Lerp(smoothedPosition, adjustedPosition, smoothingFactor);
        
        OnPositionUpdated?.Invoke(smoothedPosition);
    }

    /// <summary>
    /// Calcule et met à jour la voie actuelle avec validation temporelle
    /// </summary>
    private void UpdateLane()
    {
        float normalizedX = smoothedPosition.x;
        
        // Appliquer le seuil minimum pour éviter les micro-mouvements
        if (Mathf.Abs(normalizedX - lastStablePosition.x) < laneChangeThreshold && 
            pendingLane == -1)
        {
            return;
        }
        
        // Mapper la position X sur les 4 voies
        int targetLane;
        
        if (normalizedX < leftBoundary * 0.5f)
            targetLane = 1;
        else if (normalizedX < 0f)
            targetLane = 2;
        else if (normalizedX < rightBoundary * 0.5f)
            targetLane = 3;
        else
            targetLane = 4;
        
        // Gestion de la validation temporelle
        if (requireLaneChangeValidation)
        {
            ProcessLaneChangeValidation(targetLane);
        }
        else
        {
            ApplyLaneChange(targetLane);
        }
    }
    
    /// <summary>
    /// Traite la validation temporelle du changement de voie
    /// </summary>
    private void ProcessLaneChangeValidation(int targetLane)
    {
        if (targetLane == currentLane)
        {
            if (pendingLane != -1)
            {
                pendingLane = -1;
                laneValidationTimer = 0f;
                
                if (showDebugInfo)
                    Debug.Log("❌ Changement de voie annulé - retour à la voie actuelle");
            }
            return;
        }
        
        if (targetLane == pendingLane)
        {
            laneValidationTimer += Time.deltaTime;
            OnLaneChangePreview?.Invoke(targetLane);
            
            if (laneValidationTimer >= laneChangeValidationTime)
            {
                ApplyLaneChange(targetLane);
                pendingLane = -1;
                laneValidationTimer = 0f;
                lastStablePosition = smoothedPosition;
                
                if (showDebugInfo)
                    Debug.Log($"✅ Changement de voie validé après {laneValidationTimer:F1}s : voie {targetLane}");
            }
        }
        else
        {
            pendingLane = targetLane;
            laneValidationTimer = 0f;
            
            if (showDebugInfo)
                Debug.Log($"⏳ Début validation changement vers voie {targetLane}");
        }
    }
    
    /// <summary>
    /// Applique le changement de voie définitivement
    /// </summary>
    private void ApplyLaneChange(int newLane)
    {
        if (newLane != currentLane)
        {
            currentLane = newLane;
            OnLaneChanged?.Invoke(currentLane);
            
            if (showDebugInfo)
                Debug.Log($"🎯 Changement de voie appliqué : {currentLane}");
        }
    }    /// <summary>
    /// Force la recalibration
    /// </summary>
    public void Recalibrate()
    {
        if (enableAutoCalibration)
        {
            StartCalibration();
        }
    }
    
    /// <summary>
    /// Configure automatiquement les paramètres selon une résolution de caméra commune
    /// </summary>
    public void SetCameraPreset(string preset)
    {
        switch (preset.ToLower())
        {
            case "640x480":
            case "vga":
                cameraInputWidth = 640;
                cameraInputHeight = 480;
                trackingScale = 1.0f;
                centralDeadZone = 30f;
                break;
                
            case "1280x720":
            case "hd":
                cameraInputWidth = 1280;
                cameraInputHeight = 720;
                trackingScale = 0.8f; // Moins sensible pour les grandes résolutions
                centralDeadZone = 50f;
                break;
                
            case "1920x1080":
            case "fullhd":
                cameraInputWidth = 1920;
                cameraInputHeight = 1080;
                trackingScale = 0.6f;
                centralDeadZone = 70f;
                break;
                
            default:
                Debug.LogWarning($"⚠️ Preset de caméra '{preset}' non reconnu. Presets disponibles : 640x480, 1280x720, 1920x1080");
                break;
        }
        
        if (showDebugInfo)
            Debug.Log($"📷 Preset caméra appliqué : {preset} -> {cameraInputWidth}x{cameraInputHeight}, Scale: {trackingScale}, DeadZone: {centralDeadZone}");
    }

    /// <summary>
    /// Obtient la voie actuelle (1-4)
    /// </summary>
    public int GetCurrentLane()
    {
        return currentLane;
    }

    /// <summary>
    /// Obtient la voie en attente de validation (-1 si aucune)
    /// </summary>
    public int GetPendingLane()
    {
        return pendingLane;
    }

    /// <summary>
    /// Obtient le progrès de validation (0-1)
    /// </summary>
    public float GetValidationProgress()
    {
        if (pendingLane == -1) return 0f;
        return Mathf.Clamp01(laneValidationTimer / laneChangeValidationTime);
    }

    /// <summary>
    /// Obtient la position lissée actuelle
    /// </summary>
    public Vector3 GetSmoothedPosition()
    {
        return smoothedPosition;
    }

    /// <summary>
    /// Vérifie si le tracker est calibré
    /// </summary>
    public bool IsCalibrated()
    {
        return isCalibrated;
    }

    /// <summary>
    /// Obtient les informations de calibration pour debug
    /// </summary>
    public string GetCalibrationInfo()
    {
        return $"Base X: {baseXPosition:F2}, Largeur: {effectiveTrackingWidth:F2}, Centre: {calibrationCenter}";
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== LogParade Lateral Tracker v1.2 ===");
        GUILayout.Label($"Source: {trackingSource}");
        GUILayout.Label($"Caméra: {cameraInputWidth}x{cameraInputHeight}");
        GUILayout.Label($"Calibré: {(isCalibrated ? "OUI" : "NON")}");
        
        if (!isCalibrated && enableAutoCalibration)
        {
            float progress = calibrationTimer / calibrationTime;
            GUILayout.Label($"⏳ Calibration: {progress:P0}");
            GUILayout.Label("🎯 Placez-vous au CENTRE et restez immobile !");
        }
        
        GUILayout.Label($"Position brute: {currentPosition}");
        GUILayout.Label($"Position lissée: {smoothedPosition}");
        GUILayout.Label($"Base X: {baseXPosition:F2}");
        GUILayout.Label($"Voie actuelle: {currentLane}/4");
        
        // Affichage de la validation en cours
        if (pendingLane != -1)
        {
            float progress = GetValidationProgress();
            GUILayout.Label($"⏳ Validation vers voie {pendingLane}: {progress:P0}");
        }
        
        GUILayout.Label($"Validation: {(requireLaneChangeValidation ? "ACTIVÉE" : "DÉSACTIVÉE")}");
        GUILayout.Label($"Seuil mouvement: {laneChangeThreshold:F2}");
        GUILayout.Label($"Zone morte: {centralDeadZone}px");
        
        if (GUILayout.Button("🔄 Recalibrer"))
        {
            Recalibrate();
        }
        
        GUILayout.EndArea();
    }
}
