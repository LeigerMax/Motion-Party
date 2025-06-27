using UnityEngine;
using System.Collections;

/// <summary>
/// Gestionnaire de feedback visuel et audio pour la calibration LogParade.
/// Responsable de la mise en surbrillance des lanes et des effets audio.
/// </summary>
public class LogParadeCalibrationVisualFeedback
{
    #region Dependencies
    private Transform[] calibrationLogs;
    private GameObject logPrefab;
    private Transform[] laneTransforms;
    private AudioSource audioSource;
    #endregion

    #region Configuration
    private Color highlightColor;
    private Color completedColor;
    private Material highlightMaterial;
    private AudioClip successSound;
    private AudioClip timeoutSound;
    #endregion

    #region State
    private Renderer[] originalLogRenderers = new Renderer[4];
    private Material[] originalMaterials = new Material[4];
    private MonoBehaviour coroutineRunner; // Pour les coroutines
    #endregion

    #region Constructor
    public LogParadeCalibrationVisualFeedback(
        Transform[] calibrationLogs,
        GameObject logPrefab,
        Transform[] laneTransforms,
        AudioSource audioSource,
        Color highlightColor,
        Color completedColor,
        Material highlightMaterial,
        AudioClip successSound,
        AudioClip timeoutSound,
        MonoBehaviour coroutineRunner)
    {
        // Validation des paramètres critiques
        if (laneTransforms == null)
        {
            LogParadeLogger.LogError("laneTransforms ne peut pas être null dans LogParadeCalibrationVisualFeedback");
            this.laneTransforms = new Transform[4]; // Array vide pour éviter les crashes
        }
        else
        {
            this.laneTransforms = laneTransforms;
        }

        // Assurer que calibrationLogs a la bonne taille
        if (calibrationLogs == null || calibrationLogs.Length != 4)
        {
            LogParadeLogger.LogWarning($"calibrationLogs doit avoir 4 éléments, trouvés: {calibrationLogs?.Length ?? 0}. Création d'un array par défaut.");
            this.calibrationLogs = new Transform[4];
        }
        else
        {
            this.calibrationLogs = calibrationLogs;
        }

        this.logPrefab = logPrefab;
        this.audioSource = audioSource;
        this.highlightColor = highlightColor;
        this.completedColor = completedColor;
        this.highlightMaterial = highlightMaterial;
        this.successSound = successSound; this.timeoutSound = timeoutSound;
        this.coroutineRunner = coroutineRunner;
    }
    #endregion

    #region Validation
    /// <summary>
    /// Valide que tous les prérequis pour la configuration sont présents
    /// </summary>
    private bool ValidateSetupRequirements()
    {
        if (laneTransforms == null)
        {
            LogParadeLogger.LogError("laneTransforms est null - impossible de configurer les rondins de calibration");
            return false;
        }

        if (laneTransforms.Length < 2)
        {
            LogParadeLogger.LogError($"Au moins 2 lanes sont requises pour la calibration, trouvées: {laneTransforms.Length}");
            return false;
        }

        // Vérifier que nous avons au moins les lanes 1 et 4 (indices 0 et 3)
        if (laneTransforms.Length >= 4)
        {
            if (laneTransforms[0] == null || laneTransforms[3] == null)
            {
                LogParadeLogger.LogWarning("Les lanes 1 et 4 doivent être assignées pour la calibration interactive");
            }
        }

        return true;
    }
    #endregion

    #region Public API - Setup

    /// <summary>
    /// Configure les rondins fixes pour la calibration.
    /// </summary>
    public void SetupCalibrationLogs()
    {
        // Validation des prérequis
        if (!ValidateSetupRequirements())
        {
            LogParadeLogger.LogError("Impossible de configurer les rondins de calibration - prérequis manquants");
            return;
        }

        // Si pas de prefab assigné, essayer de trouver un rondin existant dans la scène
        if (logPrefab == null)
        {
            GameObject existingLog = GameObject.FindWithTag("Log");
            if (existingLog != null)
            {
                logPrefab = existingLog;
                LogParadeLogger.LogVerbose("Prefab de rondin trouvé automatiquement dans la scène.");
            }
        }

        // Supprimer tous les anciens logs de calibration s'ils existent
        if (calibrationLogs != null)
        {
            foreach (var t in calibrationLogs)
            {
                // Ne plus détruire automatiquement au démarrage du jeu
                // if (t != null) GameObject.Destroy(t.gameObject);
            }
        }
        // Générer un seul rondin de calibration au centre (ex: lane 2 ou 3, ou au centre de la zone)
        Vector3 calibrationPos = new Vector3(0f, laneTransforms[0].position.y, 0f); // X=0, Y=sol, Z=0
        Quaternion calibrationRotation = Quaternion.Euler(0f, 180f, 0f);
        GameObject calibrationLog = Object.Instantiate(logPrefab, calibrationPos, calibrationRotation);
        calibrationLog.name = "CalibrationLog";
        // Désactiver les scripts de mouvement s'il y en a
        var moveScript = calibrationLog.GetComponent<Rigidbody>();
        if (moveScript != null)
        {
            moveScript.isKinematic = true;
        }
        // Stocker la référence pour destruction future
        calibrationLogs = new Transform[1] { calibrationLog.transform };

        // Ajouter le script de destruction automatique si un log généré touche le rondin de calibration
        if (calibrationLog.GetComponent<Collider>() == null)
            calibrationLog.AddComponent<BoxCollider>().isTrigger = true;
        calibrationLog.AddComponent<CalibrationLogDestroyer>();

        LogParadeLogger.LogVerbose("Rondins de calibration configurés.");
    }

    /// <summary>
    /// Nettoie les rondins de calibration.
    /// </summary>
    public void CleanupCalibrationLogs()
    {
        if (calibrationLogs != null)
        {
            for (int i = 0; i < calibrationLogs.Length; i++)
            {
                if (calibrationLogs[i] != null)
                {
                    LogParadeLogger.LogVerbose($"Suppression du rondin de calibration {i + 1}");
                    Object.Destroy(calibrationLogs[i].gameObject);
                    calibrationLogs[i] = null;
                }
            }
        }
        
        LogParadeLogger.LogVerbose("Rondins de calibration nettoyés");
    }
    #endregion

    #region Public API - Visual Feedback
    /// <summary>
    /// Met en évidence une lane spécifique.
    /// </summary>
    /// <param name="laneIndex">Index de lane (0-3)</param>
    public void HighlightLane(int laneIndex)
    {
        // Correction : éviter l'accès hors tableau si calibrationLogs n'a qu'un seul élément
        if (calibrationLogs == null || calibrationLogs.Length == 0) return;
        if (calibrationLogs.Length == 1) laneIndex = 0;
        if (laneIndex < 0 || laneIndex >= calibrationLogs.Length) return;
        
        // Restaurer tous les matériaux d'abord
        RestoreOriginalMaterials();
        
        // Appliquer le matériau de surbrillance
        if (calibrationLogs[laneIndex] != null && highlightMaterial != null)
        {
            Renderer renderer = calibrationLogs[laneIndex].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = highlightMaterial;
            }
        }
        
        LogParadeLogger.LogVerbose($"Lane {laneIndex + 1} mise en surbrillance");
    }

    /// <summary>
    /// Marque une lane comme completée.
    /// </summary>
    /// <param name="laneIndex">Index de lane (0-3)</param>
    public void SetLaneCompleted(int laneIndex)
    {
        // Correction : éviter l'accès hors tableau si calibrationLogs n'a qu'un seul élément
        if (calibrationLogs == null || calibrationLogs.Length == 0) return;
        if (calibrationLogs.Length == 1) laneIndex = 0;
        if (laneIndex < 0 || laneIndex >= calibrationLogs.Length) return;
        // Ne rien faire sur le log de calibration (plus de changement de couleur)
        LogParadeLogger.LogVerbose($"Lane {laneIndex + 1} marquée comme complétée");
    }

    /// <summary>
    /// Remet les matériaux des rondins à leur état d'origine.
    /// </summary>
    public void RestoreOriginalMaterials()
    {
        for (int i = 0; i < originalLogRenderers.Length; i++)
        {
            if (originalLogRenderers[i] != null && originalMaterials[i] != null)
            {
                originalLogRenderers[i].material = originalMaterials[i];
            }
        }
    }

    /// <summary>
    /// Fait clignoter une lane pour attirer l'attention.
    /// </summary>
    /// <param name="laneIndex">Index de lane (0-3)</param>
    /// <param name="duration">Durée du clignotement</param>
    /// <param name="frequency">Fréquence du clignotement</param>
    public void BlinkLane(int laneIndex, float duration = 2f, float frequency = 2f)
    {
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(BlinkLaneCoroutine(laneIndex, duration, frequency));
        }
    }
    #endregion

    #region Public API - Audio Feedback
    /// <summary>
    /// Joue le son de succès.
    /// </summary>
    public void PlaySuccessSound()
    {
        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
            LogParadeLogger.LogVerbose("Son de succès joué");
        }
    }

    /// <summary>
    /// Joue le son de timeout.
    /// </summary>
    public void PlayTimeoutSound()
    {
        if (timeoutSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(timeoutSound);
            LogParadeLogger.LogVerbose("Son de timeout joué");
        }
    }

    /// <summary>
    /// Configure l'AudioSource si nécessaire.
    /// </summary>
    /// <param name="gameObject">GameObject pour créer l'AudioSource</param>
    public void EnsureAudioSource(GameObject gameObject)
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            LogParadeLogger.LogVerbose("AudioSource créé automatiquement");
        }
    }
    #endregion

    #region Debug
    /// <summary>
    /// Retourne des informations de debug sur l'état du système de feedback visuel.
    /// </summary>
    public string GetDebugInfo()
    {
        string info = "=== LogParadeCalibrationVisualFeedback Debug ===\n";
        
        info += $"Lane Transforms: {(laneTransforms != null ? $"{laneTransforms.Length} éléments" : "null")}\n";
        info += $"Calibration Logs: {(calibrationLogs != null ? $"{calibrationLogs.Length} éléments" : "null")}\n";
        info += $"Log Prefab: {(logPrefab != null ? logPrefab.name : "null")}\n";
        info += $"Audio Source: {(audioSource != null ? "OK" : "null")}\n";
        info += $"Highlight Material: {(highlightMaterial != null ? highlightMaterial.name : "null")}\n";
        info += $"Success Sound: {(successSound != null ? successSound.name : "null")}\n";
        info += $"Timeout Sound: {(timeoutSound != null ? timeoutSound.name : "null")}\n";
        info += $"Coroutine Runner: {(coroutineRunner != null ? coroutineRunner.name : "null")}\n";
        
        // État des rondins de calibration
        info += "\n--- État des Rondins ---\n";
        if (calibrationLogs != null)
        {
            for (int i = 0; i < calibrationLogs.Length; i++)
            {
                if (calibrationLogs[i] != null)
                {
                    info += $"Lane {i + 1}: {calibrationLogs[i].name} (Actif)\n";
                }
                else
                {
                    info += $"Lane {i + 1}: Non assigné\n";
                }
            }
        }
        
        // État des renderers
        info += "\n--- État des Renderers ---\n";
        for (int i = 0; i < originalLogRenderers.Length; i++)
        {
            if (originalLogRenderers[i] != null)
            {
                info += $"Renderer {i + 1}: OK\n";
            }
            else
            {
                info += $"Renderer {i + 1}: null\n";
            }
        }
        
        return info;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Coroutine pour faire clignoter une lane.
    /// </summary>
    private IEnumerator BlinkLaneCoroutine(int laneIndex, float duration, float frequency)
    {
        if (laneIndex < 0 || laneIndex >= 4 || calibrationLogs[laneIndex] == null) 
            yield break;

        Renderer renderer = originalLogRenderers[laneIndex];
        if (renderer == null) yield break;

        Material originalMat = originalMaterials[laneIndex];
        Material blinkMaterial = highlightMaterial != null ? highlightMaterial : 
                                new Material(originalMat) { color = highlightColor };

        float elapsed = 0f;
        float interval = 1f / frequency;
        bool isHighlighted = false;

        while (elapsed < duration)
        {
            // Alterner entre matériau original et surbrillance
            renderer.material = isHighlighted ? originalMat : blinkMaterial;
            isHighlighted = !isHighlighted;

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }        // Restaurer le matériau original
        renderer.material = originalMat;
    }
    #endregion

    // --- Ajout : rendre les rondins de calibration mobiles après la calibration ---
    public void MakeCalibrationLogsMobile(float moveSpeed = 5f)
    {
        // Trouver tous les logs de calibration générés (nommés CalibrationLog_LaneX_ZY)
        foreach (Transform lane in laneTransforms)
        {
            foreach (Transform child in lane)
            {
                if (child != null && child.name.StartsWith("CalibrationLog_Lane"))
                {
                    var rb = child.GetComponent<Rigidbody>();
                    if (rb == null) rb = child.gameObject.AddComponent<Rigidbody>();
                    rb.isKinematic = false;
                    // Ajoute un script de déplacement temporaire si besoin
                    if (child.GetComponent<CalibrationLogMover>() == null)
                    {
                        child.gameObject.AddComponent<CalibrationLogMover>().SetSpeed(moveSpeed);
                    }
                }
            }
        }
    }
}
