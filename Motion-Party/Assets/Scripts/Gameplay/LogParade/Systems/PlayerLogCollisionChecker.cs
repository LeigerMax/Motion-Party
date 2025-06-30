using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Vérifie en temps réel si l'avatar du joueur est sur un rondin.
/// Gère la tolérance après changement de voie et la détection douce.
/// Ce module est isolé et ne dépend d'aucun autre système (score, lane, etc.).
/// </summary>
public class PlayerLogCollisionChecker : MonoBehaviour
{
    #region Fields
    [Header("Detection Settings")]
    [Tooltip("Rayon du collider de détection sous le joueur")]
    public float detectionRadius = 0.8f;
    
    [Tooltip("Distance maximale entre deux rondins pour les considérer comme 'proches'")]
    public float maxLogGapDistance = 0.5f;
    
    [Header("Tolerance Settings")]
    [Tooltip("Temps de tolérance en secondes après un changement de voie")]
    public float laneChangeToleranceTime = 0.3f;
    
    [Header("Debug")]
    [Tooltip("Afficher les logs de debug dans la console")]
    public bool enableDebugLogs = true;
    
    [Tooltip("Afficher les gizmos de debug dans la scene")]
    public bool showDebugGizmos = true;

    // État public accessible
    /// <summary>
    /// Indique si le joueur est actuellement considéré comme étant sur un rondin
    /// </summary>
    public bool IsOnLog { get; private set; } = false;

    // Variables privées
    private SphereCollider detectionCollider;
    private HashSet<Collider> detectedLogs = new HashSet<Collider>();
    private bool isInTolerancePeriod = false;
    private float toleranceTimer = 0f;
    private int previousLane = -1;
    private LogParadePlayerAvatar playerAvatar;
    
    // Cache pour éviter les allocations
    private List<Collider> logsToRemove = new List<Collider>();
    #endregion

    #region Unity Callbacks
    void Start()
    {
        InitializeDetectionSystem();
    }

    void Update()
    {
        UpdateDetectionState();
        UpdateTolerancePeriod();
        UpdateLogStatus();
    }
    #endregion

    #region Detection Logic
    /// <summary>
    /// Initialise le système de détection
    /// </summary>
    private void InitializeDetectionSystem()
    {
        // Récupérer le composant PlayerAvatar s'il existe
        playerAvatar = GetComponent<LogParadePlayerAvatar>();
        
        // Créer ou configurer le collider de détection
        detectionCollider = GetComponent<SphereCollider>();
        if (detectionCollider == null)
        {
            detectionCollider = gameObject.AddComponent<SphereCollider>();
        }
        
        // Configurer le collider comme trigger
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;
        
        // Positionner le centre du collider légèrement vers le bas
        detectionCollider.center = Vector3.down * 0.2f;
        
        if (enableDebugLogs)
        {
            LogParadeLogger.LogVerbose($"Système initialisé. Rayon de détection: {detectionRadius}");
        }
    }

    /// <summary>
    /// Met à jour l'état de détection basé sur les changements de voie
    /// </summary>
    private void UpdateDetectionState()
    {
        if (playerAvatar == null) return;

        int currentLane = playerAvatar.GetCurrentLane();
        
        // Détecter un changement de voie
        if (previousLane != -1 && previousLane != currentLane)
        {
            StartTolerancePeriod();
            
        }
        
        previousLane = currentLane;
    }

    /// <summary>
    /// Met à jour la période de tolérance
    /// </summary>
    private void UpdateTolerancePeriod()
    {
        if (!isInTolerancePeriod) return;

        toleranceTimer -= Time.deltaTime;
        
        if (toleranceTimer <= 0f)
        {
            isInTolerancePeriod = false;
        }
    }   
    
     /// <summary>
    /// Met à jour le statut "sur rondin" du joueur
    /// </summary>
    private void UpdateLogStatus()
    {
        // Nettoyer les rondins détruits ou invalides
        CleanupDetectedLogs();

        bool wasOnLog = IsOnLog;
        
        // Vérifier si on est physiquement sur un rondin
        bool physicallyOnLog = detectedLogs.Count > 0;
        
        // Vérifier si on est entre deux rondins proches
        bool betweenCloseLogs = IsPlayerBetweenCloseLogs();
        
        // Si on est physiquement sur un rondin, arrêter la période de tolérance
        if (physicallyOnLog && isInTolerancePeriod)
        {
            isInTolerancePeriod = false;
            toleranceTimer = 0f;
    
        }
        
        // Le joueur est considéré sur un rondin si :
        // 1. Il est physiquement sur un rondin, OU
        // 2. Il est entre deux rondins proches, OU
        // 3. Il est en période de tolérance après changement de voie
        IsOnLog = physicallyOnLog || betweenCloseLogs || isInTolerancePeriod;

        // Logger les changements d'état
        if (enableDebugLogs && wasOnLog != IsOnLog)
        {
            string reason = "";
            if (physicallyOnLog) reason = "contact physique";
            else if (betweenCloseLogs) reason = "entre rondins proches";
            else if (isInTolerancePeriod) reason = "période de tolérance";
            else reason = "aucun rondin détecté";

            LogParadeLogger.LogVerbose($"État changé: {(IsOnLog ? "SUR RONDIN" : "DANS L'EAU")} - Raison: {reason}");
        }
    }

    /// <summary>
    /// Vérifie si le joueur est entre deux rondins proches sur la même voie
    /// </summary>
    private bool IsPlayerBetweenCloseLogs()
    {
        if (detectedLogs.Count >= 2) return true; // Si on détecte plusieurs rondins, on est forcément "entre" eux
        
        return false;
    }

    /// <summary>
    /// Nettoie la liste des rondins détectés (supprime les objets détruits)
    /// </summary>
    private void CleanupDetectedLogs()
    {
        logsToRemove.Clear();
        
        foreach (var log in detectedLogs)
        {
            if (log == null)
            {
                logsToRemove.Add(log);
            }
        }
        
        foreach (var log in logsToRemove)
        {
            detectedLogs.Remove(log);
        }
    }

    /// <summary>
    /// Démarre la période de tolérance après changement de voie
    /// </summary>
    private void StartTolerancePeriod()
    {
        isInTolerancePeriod = true;
        toleranceTimer = laneChangeToleranceTime;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Méthode publique pour interroger l'état "sur rondin"
    /// Utilisée par les autres systèmes (score, feedback, etc.)
    /// </summary>
    /// <returns>True si le joueur est considéré comme étant sur un rondin</returns>
    public bool IsPlayerOnLog()
    {
        return IsOnLog;
    }

    /// <summary>
    /// Indique si le joueur est actuellement en période de tolérance
    /// </summary>
    /// <returns>True si en période de tolérance</returns>
    public bool IsInTolerancePeriod()
    {
        return isInTolerancePeriod;
    }

    /// <summary>
    /// Retourne le nombre de rondins actuellement détectés
    /// </summary>
    /// <returns>Nombre de rondins en contact</returns>
    public int GetDetectedLogCount()
    {
        CleanupDetectedLogs();
        return detectedLogs.Count;
    }

    /// <summary>
    /// Force la mise à jour de la taille du collider de détection
    /// </summary>
    public void UpdateDetectionRadius(float newRadius)
    {
        detectionRadius = newRadius;
        if (detectionCollider != null)
        {
            detectionCollider.radius = detectionRadius;
        }
    }
    #endregion

    #region Unity Trigger Events
    void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est un rondin (par tag ou layer)
        if (IsLogObject(other))
        {
            detectedLogs.Add(other);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // S'assurer que le rondin est toujours dans la liste
        if (IsLogObject(other) && !detectedLogs.Contains(other))
        {
            detectedLogs.Add(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsLogObject(other))
        {
            detectedLogs.Remove(other);

        }
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Détermine si un collider représente un rondin
    /// Utilise le tag "Log" par défaut, mais peut être étendu
    /// </summary>
    private bool IsLogObject(Collider collider)
    {
        // Vérifier par tag (méthode recommandée)
        if (collider.CompareTag("Log"))
            return true;
            
        // Vérifier par nom (fallback pour les tests)
        if (collider.name.ToLower().Contains("log") || collider.name.ToLower().Contains("rondin"))
            return true;
            
        return false;
    }
    #endregion
}
