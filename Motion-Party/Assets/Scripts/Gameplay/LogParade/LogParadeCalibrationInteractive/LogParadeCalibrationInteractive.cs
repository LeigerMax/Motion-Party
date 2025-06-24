using UnityEngine;
using TMPro;
using System.Collections;
using System;

/// <summary>
/// Système de calibration interactive "Déplacez-vous" pour LogParade
/// Le joueur doit se déplacer successivement vers la lane 1 puis la lane 4
/// pour valider la calibration avant de commencer le jeu.
/// </summary>
public class LogParadeCalibrationInteractive : MonoBehaviour
{
    [Header("Configuration de Calibration")]
    [SerializeField] private float timeoutDuration = 15f;
    [SerializeField] private float laneDetectionTolerance = 0.5f;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Références Player")]
    [SerializeField] private LogParadePlayerAvatar playerAvatar;
    [SerializeField] private LogParadeLateralTracker lateralTracker;
    
    [Header("Références Lanes")]
    [SerializeField] private Transform[] laneTransforms = new Transform[4];
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private Transform[] calibrationLogs = new Transform[4];
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject calibrationUI;
    [SerializeField] private CanvasGroup instructionCanvasGroup;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Material highlightMaterial;
    
    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip timeoutSound;
    [SerializeField] private AudioSource audioSource;

    // États de calibration
    private enum CalibrationState
    {
        NotStarted,
        WaitingForLane1,
        Lane1Completed,
        WaitingForLane4,
        Completed,
        Failed
    }

    private CalibrationState currentState = CalibrationState.NotStarted;
    private float stateTimer = 0f;
    private bool isCalibrationActive = false;
    
    // Événements
    public static event Action OnCalibrationCompleted;
    public static event Action OnCalibrationFailed;
    public static event Action<int> OnLaneReached;

    // Cache des renderers originaux pour restaurer les matériaux
    private Renderer[] originalLogRenderers = new Renderer[4];
    private Material[] originalMaterials = new Material[4];

    void Start()
    {
        InitializeCalibration();
    }

    void Update()
    {
        if (isCalibrationActive)
        {
            UpdateCalibrationState();
            UpdateTimer();
        }
    }

    /// <summary>
    /// Initialise le système de calibration
    /// </summary>
    private void InitializeCalibration()
    {
        // Vérifier les références nécessaires
        if (!ValidateReferences())
        {
            Debug.LogError("LogParadeCalibrationInteractive: Références manquantes!");
            return;
        }

        // Configurer l'AudioSource si nécessaire
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Créer les rondins de calibration
        SetupCalibrationLogs();
        
        // Initialiser l'UI
        SetupUI();
        
        Debug.Log("Système de calibration interactive initialisé.");
    }

    /// <summary>
    /// Valide que toutes les références nécessaires sont présentes
    /// </summary>
    private bool ValidateReferences()
    {
        if (playerAvatar == null)
        {
            Debug.LogError("PlayerAvatar non assigné!");
            return false;
        }

        if (instructionText == null)
        {
            Debug.LogError("InstructionText non assigné!");
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (laneTransforms[i] == null)
            {
                Debug.LogError($"Lane Transform {i + 1} non assigné!");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Configure les rondins fixes pour la calibration
    /// </summary>
    private void SetupCalibrationLogs()
    {
        // Si pas de prefab assigné, essayer de trouver un rondin existant dans la scène
        if (logPrefab == null)
        {
            GameObject existingLog = GameObject.FindWithTag("Log");
            if (existingLog != null)
            {
                logPrefab = existingLog;
                Debug.Log("Prefab de rondin trouvé automatiquement dans la scène.");
            }
        }

        // Créer ou assigner les rondins de calibration pour chaque lane
        for (int i = 0; i < 4; i++)
        {
            if (calibrationLogs[i] == null && logPrefab != null)
            {
                // Créer un rondin fixe pour cette lane
                GameObject calibrationLog = Instantiate(logPrefab, laneTransforms[i].position, laneTransforms[i].rotation);
                calibrationLog.name = $"CalibrationLog_Lane{i + 1}";
                calibrationLog.transform.SetParent(laneTransforms[i]);
                
                // Désactiver les scripts de mouvement s'il y en a
                var moveScript = calibrationLog.GetComponent<Rigidbody>();
                if (moveScript != null)
                {
                    moveScript.isKinematic = true;
                }
                
                calibrationLogs[i] = calibrationLog.transform;
            }

            // Sauvegarder les matériaux originaux
            if (calibrationLogs[i] != null)
            {
                originalLogRenderers[i] = calibrationLogs[i].GetComponent<Renderer>();
                if (originalLogRenderers[i] != null)
                {
                    originalMaterials[i] = originalLogRenderers[i].material;
                }
            }
        }

        Debug.Log("Rondins de calibration configurés.");
    }

    /// <summary>
    /// Configure l'interface utilisateur
    /// </summary>
    private void SetupUI()
    {
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
        }

        if (instructionCanvasGroup != null)
        {
            instructionCanvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Démarre la séquence de calibration
    /// </summary>
    public void StartCalibration()
    {
        if (isCalibrationActive)
        {
            Debug.LogWarning("Calibration déjà en cours!");
            return;
        }

        isCalibrationActive = true;
        currentState = CalibrationState.WaitingForLane1;
        stateTimer = 0f;

        // Activer l'UI de calibration
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(true);
        }

        // Afficher l'instruction initiale
        UpdateInstructionText("Placez-vous sur la lane 1...");
        FadeInInstructions();

        // Mettre en évidence la lane 1
        HighlightLane(0);

        // S'assurer que le joueur est visible et peut se déplacer
        if (playerAvatar != null)
        {
            playerAvatar.gameObject.SetActive(true);
        }

        Debug.Log("Calibration interactive démarrée - en attente de la lane 1");
    }

    /// <summary>
    /// Met à jour l'état de la calibration
    /// </summary>
    private void UpdateCalibrationState()
    {
        int currentLane = GetPlayerCurrentLane();

        switch (currentState)
        {
            case CalibrationState.WaitingForLane1:
                if (IsPlayerOnLane(1))
                {
                    OnPlayerReachedLane1();
                }
                break;

            case CalibrationState.Lane1Completed:
                // Transition automatique vers lane 4
                if (stateTimer > 1f) // Petit délai pour montrer le succès
                {
                    StartWaitingForLane4();
                }
                break;

            case CalibrationState.WaitingForLane4:
                if (IsPlayerOnLane(4))
                {
                    OnPlayerReachedLane4();
                }
                break;

            case CalibrationState.Completed:
                if (stateTimer > 2f) // Afficher le succès pendant 2 secondes
                {
                    CompleteCalibration();
                }
                break;
        }
    }

    /// <summary>
    /// Met à jour le timer et gère les timeouts
    /// </summary>
    private void UpdateTimer()
    {
        stateTimer += Time.deltaTime;

        // Vérifier le timeout pour les états d'attente
        if ((currentState == CalibrationState.WaitingForLane1 || currentState == CalibrationState.WaitingForLane4) 
            && stateTimer >= timeoutDuration)
        {
            OnCalibrationTimeout();
        }
    }

    /// <summary>
    /// Obtient la lane actuelle du joueur
    /// </summary>
    private int GetPlayerCurrentLane()
    {
        if (playerAvatar != null)
        {
            return playerAvatar.GetCurrentLane();
        }

        // Fallback: calculer en fonction de la position
        if (lateralTracker != null)
        {
            // Utiliser le tracker latéral si disponible
            return CalculateLaneFromPosition(playerAvatar.transform.position);
        }

        return 2; // Défaut au centre
    }

    /// <summary>
    /// Calcule la lane en fonction de la position world
    /// </summary>
    private int CalculateLaneFromPosition(Vector3 position)
    {
        float closestDistance = float.MaxValue;
        int closestLane = 1;

        for (int i = 0; i < 4; i++)
        {
            float distance = Vector3.Distance(position, laneTransforms[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = i + 1;
            }
        }

        return closestLane;
    }

    /// <summary>
    /// Vérifie si le joueur est sur une lane spécifique
    /// </summary>
    private bool IsPlayerOnLane(int targetLane)
    {
        if (playerAvatar == null) return false;

        Vector3 playerPos = playerAvatar.transform.position;
        Vector3 lanePos = laneTransforms[targetLane - 1].position;
        
        float distance = Vector3.Distance(playerPos, lanePos);
        return distance <= laneDetectionTolerance;
    }

    /// <summary>
    /// Appelé quand le joueur atteint la lane 1
    /// </summary>
    private void OnPlayerReachedLane1()
    {
        currentState = CalibrationState.Lane1Completed;
        stateTimer = 0f;

        // Feedback visuel et audio
        UpdateInstructionText("Très bien ! Maintenant, allez sur la lane 4");
        PlaySuccessSound();
        SetLaneCompleted(0);
        
        OnLaneReached?.Invoke(1);
        
        Debug.Log("Lane 1 atteinte avec succès!");
    }

    /// <summary>
    /// Démarre l'attente de la lane 4
    /// </summary>
    private void StartWaitingForLane4()
    {
        currentState = CalibrationState.WaitingForLane4;
        stateTimer = 0f;

        // Mettre en évidence la lane 4
        HighlightLane(3);
        
        UpdateInstructionText("Allez sur la lane 4...");
    }

    /// <summary>
    /// Appelé quand le joueur atteint la lane 4
    /// </summary>
    private void OnPlayerReachedLane4()
    {
        currentState = CalibrationState.Completed;
        stateTimer = 0f;

        // Feedback final
        UpdateInstructionText("Parfait ! Calibration terminée 🎉");
        PlaySuccessSound();
        SetLaneCompleted(3);
        
        OnLaneReached?.Invoke(4);
        
        Debug.Log("Lane 4 atteinte - Calibration terminée!");
    }

    /// <summary>
    /// Finalise la calibration
    /// </summary>
    private void CompleteCalibration()
    {
        isCalibrationActive = false;
        
        // Nettoyer l'UI
        FadeOutInstructions();
        
        // Restaurer les matériaux des rondins
        RestoreOriginalMaterials();
        
        // Notifier la completion
        OnCalibrationCompleted?.Invoke();
        
        // Désactiver l'UI de calibration
        if (calibrationUI != null)
        {
            StartCoroutine(HideCalibrationUIAfterDelay(1f));
        }
        
        Debug.Log("Calibration interactive terminée avec succès!");
    }

    /// <summary>
    /// Gère le timeout de calibration
    /// </summary>
    private void OnCalibrationTimeout()
    {
        // Jouer le son de timeout
        if (timeoutSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(timeoutSound);
        }

        // Afficher message de relance
        UpdateInstructionText("Timeout... Recommençons!");
        
        // Attendre un moment puis relancer
        StartCoroutine(RestartCalibrationAfterDelay(2f));
        
        Debug.Log("Timeout de calibration - relancement...");
    }

    /// <summary>
    /// Relance la calibration après un délai
    /// </summary>
    private IEnumerator RestartCalibrationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Restaurer les matériaux
        RestoreOriginalMaterials();
        
        // Redémarrer la calibration
        currentState = CalibrationState.WaitingForLane1;
        stateTimer = 0f;
        
        UpdateInstructionText("Placez-vous sur la lane 1...");
        HighlightLane(0);
    }

    /// <summary>
    /// Met en évidence une lane spécifique
    /// </summary>
    private void HighlightLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= 4) return;
        
        // Restaurer tous les matériaux d'abord
        RestoreOriginalMaterials();
        
        // Appliquer le matériau de surbrillance
        if (calibrationLogs[laneIndex] != null && highlightMaterial != null)
        {
            Renderer renderer = originalLogRenderers[laneIndex];
            if (renderer != null)
            {
                renderer.material = highlightMaterial;
            }
        }
    }

    /// <summary>
    /// Marque une lane comme completée
    /// </summary>
    private void SetLaneCompleted(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= 4) return;
        
        if (calibrationLogs[laneIndex] != null)
        {
            Renderer renderer = originalLogRenderers[laneIndex];
            if (renderer != null)
            {
                // Créer un matériau vert pour le succès
                Material completedMaterial = new Material(renderer.material);
                completedMaterial.color = completedColor;
                renderer.material = completedMaterial;
            }
        }
    }

    /// <summary>
    /// Nettoie les rondins de calibration
    /// </summary>
    public void CleanupCalibrationLogs()
    {
        if (calibrationLogs != null)
        {
            for (int i = 0; i < calibrationLogs.Length; i++)
            {
                if (calibrationLogs[i] != null)
                {
                    Debug.Log($"Suppression du rondin de calibration {i + 1}");
                    Destroy(calibrationLogs[i].gameObject);
                    calibrationLogs[i] = null;
                }
            }
        }
        
        Debug.Log("Rondins de calibration nettoyés");
    }
    
    /// <summary>
    /// Remet les matériaux des rondins à leur état d'origine
    /// </summary>
    private void RestoreOriginalMaterials()
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
    /// Met à jour le texte d'instruction
    /// </summary>
    private void UpdateInstructionText(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }

    /// <summary>
    /// Fait apparaître les instructions en fondu
    /// </summary>
    private void FadeInInstructions()
    {
        if (instructionCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(instructionCanvasGroup, 1f, 0.5f));
        }
    }

    /// <summary>
    /// Fait disparaître les instructions en fondu
    /// </summary>
    private void FadeOutInstructions()
    {
        if (instructionCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(instructionCanvasGroup, 0f, 0.5f));
        }
    }

    /// <summary>
    /// Coroutine pour fade d'un CanvasGroup
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Masque l'UI de calibration après un délai
    /// </summary>
    private IEnumerator HideCalibrationUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
        }
    }

    /// <summary>
    /// Joue le son de succès
    /// </summary>
    private void PlaySuccessSound()
    {
        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }
    }

    /// <summary>
    /// Arrête la calibration (pour debug ou reset)
    /// </summary>
    public void StopCalibration()
    {
        isCalibrationActive = false;
        currentState = CalibrationState.NotStarted;
        
        RestoreOriginalMaterials();
        
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
        }
        
        Debug.Log("Calibration arrêtée.");
    }

    /// <summary>
    /// Redémarre la calibration
    /// </summary>
    public void RestartCalibration()
    {
        StopCalibration();
        StartCalibration();
    }

    // Interface publique pour intégration
    
    /// <summary>
    /// Vérifie si la calibration est terminée
    /// </summary>
    public bool IsCalibrationCompleted()
    {
        return currentState == CalibrationState.Completed && !isCalibrationActive;
    }

    /// <summary>
    /// Vérifie si la calibration est en cours
    /// </summary>
    public bool IsCalibrationActive()
    {
        return isCalibrationActive;
    }

    /// <summary>
    /// Obtient l'état actuel de la calibration    /// </summary>
    public string GetCalibrationStatus()
    {
        return currentState.ToString();
    }

    // Debug et visualisation
    
    void OnDrawGizmos()
    {
        // Dessiner les zones de détection des lanes
        if (laneTransforms != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < 4; i++)
            {
                if (laneTransforms[i] != null)
                {
                    Gizmos.DrawWireSphere(laneTransforms[i].position, laneDetectionTolerance);
                }
            }
        }

        // Mettre en évidence la lane cible si calibration active
        if (Application.isPlaying && isCalibrationActive)
        {
            int targetLane = -1;
            if (currentState == CalibrationState.WaitingForLane1) targetLane = 0;
            else if (currentState == CalibrationState.WaitingForLane4) targetLane = 3;

            if (targetLane >= 0 && laneTransforms[targetLane] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(laneTransforms[targetLane].position, 0.3f);
            }
        }
    }
}
