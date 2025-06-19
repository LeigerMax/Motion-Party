using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

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
    public float rightBoundary = 1.5f;    [Header("Lane Change Sensitivity")]
    [Tooltip("Temps minimum à maintenir dans une position avant changement de voie")]
    [Range(0.2f, 3.0f)]
    public float laneChangeValidationTime = 0.5f;
    
    [Tooltip("Seuil minimum de mouvement pour déclencher un changement de voie")]
    [Range(0.1f, 2.0f)]
    public float laneChangeThreshold = 0.1f;
    
    [Tooltip("Si activé, les changements nécessitent une validation temporelle")]
    public bool requireLaneChangeValidation = false; // Désactivé pour les tests
      [Tooltip("Facteur d'amplification pour atteindre les lanes extrêmes (1 et 4)")]
    [Range(0.5f, 2.0f)]
    public float extremeLanesSensitivity = 1.2f;
      [Tooltip("Utiliser un mapping simplifié basé sur les pourcentages")]
    public bool useSimpleMapping = true; // Activé par défaut pour les tests
    
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
            {            // Récupérer la position brute en pixels
                float rawX = (float)poseLandmarks[0][0];
                float rawY = (float)poseLandmarks[0][1];
                float rawZ = (float)poseLandmarks[0][2];
                
                // Log des positions brutes pour debug
                if (showDebugInfo && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Position brute: X={rawX:F1}, Y={rawY:F1}, Z={rawZ:F3}");
                }
                
                // Normaliser selon la taille de la caméra (0 à 1)
                float normalizedX = rawX / cameraInputWidth; // 0 à 1
                float normalizedY = rawY / cameraInputHeight; // 0 à 1
                
                // Centrer et appliquer l'échelle (-0.5 à +0.5 puis scaling)
                float scaledX = (normalizedX - 0.5f) * trackingScale;
                float scaledY = (normalizedY - 0.5f) * trackingScale;
                
                currentPosition = new Vector3(scaledX, scaledY, rawZ * 0.01f);
                trackingSource = "Head (Pose)";
                
                // Mettre à jour les bornes observées pendant la calibration
                if (!isCalibrated && enableAutoCalibration)
                {
                    minObservedX = Mathf.Min(minObservedX, rawX); // Utiliser rawX pour les bornes
                    maxObservedX = Mathf.Max(maxObservedX, rawX);
                }
                
                // Log des positions normalisées pour debug
                if (showDebugInfo && Time.frameCount % 120 == 0)
                {
                    Debug.Log($"Position normalisée: X={scaledX:F3}, Bornes: [{minObservedX:F1}, {maxObservedX:F1}]");
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
    }    /// <summary>
    /// Calcule et met à jour la voie actuelle avec validation temporelle
    /// </summary>
    private void UpdateLane()
    {
        // Utiliser directement la position X actuelle (données brutes converties)
        float positionX = currentPosition.x;
        
        // Appliquer le seuil minimum pour éviter les micro-mouvements
        if (Mathf.Abs(positionX - lastStablePosition.x) < laneChangeThreshold && 
            pendingLane == -1)
        {
            return;
        }
        
        // Mapper la position X sur les 4 voies
        int targetLane = CalculateLaneFromPosition(positionX);
        
        // Log pour debug
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎯 UpdateLane: posX={positionX:F3}, targetLane={targetLane}, currentLane={currentLane}");
        }
        
        // Gestion de la validation temporelle
        if (requireLaneChangeValidation)
        {
            ProcessLaneChangeValidation(targetLane);
        }
        else
        {
            ApplyLaneChange(targetLane);
        }
    }    /// <summary>
    /// Calcule la voie selon la position X avec mapping adaptatif
    /// Utilise les données brutes pour un mapping plus précis
    /// </summary>
    private int CalculateLaneFromPosition(float positionX)
    {
        if (useSimpleMapping)
        {
            return CalculateLaneSimple(positionX);
        }
        
        // Convertir la position normalisée (-0.5 à +0.5) en plage utilisable
        // positionX va de -0.5 à +0.5 environ
        
        // Utiliser un mapping direct basé sur les seuils
        // Ajuster les seuils selon vos données (88 à 517 pixels normalisés)
        
        // Mapping plus agressif pour assurer l'accès aux 4 lanes
        float threshold1 = -0.3f;  // Seuil pour lane 1
        float threshold2 = -0.1f;  // Seuil pour lane 2
        float threshold3 = 0.1f;   // Seuil pour lane 3
        // Au-dessus de threshold3 = lane 4
        
        int lane;
        if (positionX <= threshold1)
            lane = 1; // Lane gauche
        else if (positionX <= threshold2)
            lane = 2; // Lane centre-gauche
        else if (positionX <= threshold3)
            lane = 3; // Lane centre-droite
        else
            lane = 4; // Lane droite
        
        // Log détaillé pour debug
        if (showDebugInfo && Time.frameCount % 120 == 0)
        {
            Debug.Log($"🎯 Mapping: posX={positionX:F3} → Lane {lane} (seuils: {threshold1:F1}, {threshold2:F1}, {threshold3:F1})");
        }
        
        return lane;
    }
      /// <summary>
    /// Mapping simplifié basé sur les bornes min/max observées
    /// Utilise les données brutes en pixels pour plus de précision
    /// </summary>
    private int CalculateLaneSimple(float positionX)
    {
        // Reconvertir la position normalisée en pixels pour le calcul
        float currentPixelX = (positionX + 0.5f) * cameraInputWidth;
        
        // Utiliser les bornes observées en pixels (minObservedX, maxObservedX)
        float minPixelX = minObservedX;
        float maxPixelX = maxObservedX;
        
        // Si pas de calibration, utiliser vos données observées
        if (minPixelX >= maxPixelX || minPixelX == float.MaxValue)
        {
            minPixelX = 88f;  // Votre position extrême gauche
            maxPixelX = 517f; // Votre position extrême droite
            
            if (showDebugInfo)
            {
                Debug.Log($"⚠️ Utilisation des valeurs par défaut: [{minPixelX}, {maxPixelX}] pixels");
            }
        }
        
        // Étendre légèrement les bornes pour faciliter l'accès aux extrêmes
        float range = maxPixelX - minPixelX;
        float extendedMin = minPixelX - (range * 0.1f);
        float extendedMax = maxPixelX + (range * 0.1f);
        
        // Normaliser entre 0 et 1
        float normalizedPosition = (currentPixelX - extendedMin) / (extendedMax - extendedMin);
        normalizedPosition = Mathf.Clamp01(normalizedPosition);
        
        // Diviser en 4 zones égales
        int lane;
        if (normalizedPosition < 0.25f)
            lane = 1; // Lane gauche (0-25%)
        else if (normalizedPosition < 0.5f)
            lane = 2; // Lane centre-gauche (25-50%)
        else if (normalizedPosition < 0.75f)
            lane = 3; // Lane centre-droite (50-75%)
        else
            lane = 4; // Lane droite (75-100%)
        
        // Log détaillé pour debug
        if (showDebugInfo && Time.frameCount % 120 == 0)
        {
            Debug.Log($"🎯 Simple Mapping: {currentPixelX:F0}px ({normalizedPosition:P0}) → Lane {lane}, Bornes: [{minPixelX:F0}, {maxPixelX:F0}]");
        }
        
        return lane;
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
                extremeLanesSensitivity = 1.3f; // Plus de sensibilité pour les petites résolutions
                break;
                
            case "1280x720":
            case "hd":
                cameraInputWidth = 1280;
                cameraInputHeight = 720;
                trackingScale = 0.8f; // Moins sensible pour les grandes résolutions
                centralDeadZone = 50f;
                extremeLanesSensitivity = 1.2f;
                break;
                
            case "1920x1080":
            case "fullhd":
                cameraInputWidth = 1920;
                cameraInputHeight = 1080;
                trackingScale = 0.6f;
                centralDeadZone = 70f;
                extremeLanesSensitivity = 1.1f;
                break;
                
            default:
                Debug.LogWarning($"⚠️ Preset de caméra '{preset}' non reconnu. Presets disponibles : 640x480, 1280x720, 1920x1080");
                break;
        }
        
        if (showDebugInfo)
            Debug.Log($"📷 Preset caméra appliqué : {preset} -> {cameraInputWidth}x{cameraInputHeight}, Scale: {trackingScale}, DeadZone: {centralDeadZone}, ExtremeSensitivity: {extremeLanesSensitivity}");
    }
    
    /// <summary>
    /// Recalibration étendue pour améliorer l'accès aux lanes extrêmes
    /// </summary>
    public void RecalibrateForExtremeLanes()
    {
        if (showDebugInfo)
            Debug.Log("🎯 Calibration étendue - Bougez de GAUCHE à DROITE pendant la calibration !");
            
        // Augmenter temporairement le temps de calibration pour permettre plus de mouvement
        float originalCalibrationTime = calibrationTime;
        calibrationTime = 5.0f; // 5 secondes au lieu de 3
        
        StartCalibration();
        
        // Programmer la restauration du temps original
        StartCoroutine(RestoreCalibrationTime(originalCalibrationTime));
    }
    
    /// <summary>
    /// Restaure le temps de calibration original après la calibration étendue
    /// </summary>
    private System.Collections.IEnumerator RestoreCalibrationTime(float originalTime)
    {
        yield return new WaitForSeconds(calibrationTime + 1f);
        calibrationTime = originalTime;
        
        if (showDebugInfo)
            Debug.Log($"⏰ Temps de calibration restauré : {originalTime}s");
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
    }    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 450, 400));
        GUILayout.Label("=== LogParade Tracker - Données Réelles ===");
        GUILayout.Label($"Source: {trackingSource}");
        GUILayout.Label($"Caméra: {cameraInputWidth}x{cameraInputHeight}");
        
        // Données brutes importantes
        if (currentPosition != Vector3.zero)
        {
            float rawPixelX = (currentPosition.x + 0.5f) * cameraInputWidth;
            GUILayout.Label($"🎯 Position brute: {rawPixelX:F0} pixels");
            GUILayout.Label($"Position normalisée: {currentPosition.x:F3}");
            
            // Afficher les bornes observées
            if (minObservedX != float.MaxValue && maxObservedX != float.MinValue)
            {
                GUILayout.Label($"Bornes observées: [{minObservedX:F0}, {maxObservedX:F0}] pixels");
            }
            else
            {
                GUILayout.Label($"Bornes par défaut: [88, 517] pixels");
            }
        }
        
        GUILayout.Label($"🎮 Voie actuelle: {currentLane}/4");
        
        // Status de calibration
        GUILayout.Label($"Calibré: {(isCalibrated ? "✅ OUI" : "❌ NON")}");
        if (!isCalibrated && enableAutoCalibration)
        {
            float progress = calibrationTimer / calibrationTime;
            GUILayout.Label($"⏳ Calibration: {progress:P0}");
            GUILayout.Label("🎯 Placez-vous au CENTRE et restez immobile !");
        }
        
        // Mapping actuel
        GUILayout.Label($"Mapping: {(useSimpleMapping ? "🔵 SIMPLE" : "🟡 AVANCÉ")}");
        GUILayout.Label($"Validation: {(requireLaneChangeValidation ? "🔒 ACTIVÉE" : "🔓 DÉSACTIVÉE")}");
        
        // Affichage de la validation en cours
        if (pendingLane != -1)
        {
            float progress = GetValidationProgress();
            GUILayout.Label($"⏳ Validation vers voie {pendingLane}: {progress:P0}");
        }
        
        // Mapping détaillé si on utilise le mode simple
        if (useSimpleMapping && currentPosition != Vector3.zero)
        {
            float currentPixelX = (currentPosition.x + 0.5f) * cameraInputWidth;
            float minPx = minObservedX != float.MaxValue ? minObservedX : 88f;
            float maxPx = maxObservedX != float.MinValue ? maxObservedX : 517f;
            float range = maxPx - minPx;
            float progress = (currentPixelX - minPx) / range;
            
            GUILayout.Label($"📊 Progression: {progress:P0} ({currentPixelX:F0}/{maxPx:F0}px)");
            
            // Afficher les zones de lane
            string zone = "";
            if (progress < 0.25f) zone = "🔴 LANE 1 (0-25%)";
            else if (progress < 0.5f) zone = "🟡 LANE 2 (25-50%)";
            else if (progress < 0.75f) zone = "🟢 LANE 3 (50-75%)";
            else zone = "🔵 LANE 4 (75-100%)";
            
            GUILayout.Label($"Zone: {zone}");
        }
        
        GUILayout.Space(10);
        
        // Boutons de contrôle
        if (GUILayout.Button("🔄 Recalibrer"))
        {
            Recalibrate();
        }
        
        if (GUILayout.Button(useSimpleMapping ? "→ Mapping Avancé" : "→ Mapping Simple"))
        {
            useSimpleMapping = !useSimpleMapping;
            Debug.Log($"🔄 Mapping changé vers : {(useSimpleMapping ? "SIMPLE" : "AVANCÉ")}");
        }
        
        if (GUILayout.Button(requireLaneChangeValidation ? "→ Validation OFF" : "→ Validation ON"))
        {
            requireLaneChangeValidation = !requireLaneChangeValidation;
            Debug.Log($"🔄 Validation temporelle : {(requireLaneChangeValidation ? "ACTIVÉE" : "DÉSACTIVÉE")}");
        }
        
        GUILayout.EndArea();
    }
}
