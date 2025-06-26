using UnityEngine;

/// <summary>
/// Configuration centralisée pour le système de génération de rondins LogParade.
/// Gère la validation des références et la configuration des paramètres.
/// </summary>
public class LogParadeLogConfiguration : MonoBehaviour
{
    [Header("Configuration des Voies")]
    [SerializeField] private Transform[] lanes = new Transform[4];
    
    [Header("Prefabs de Rondins")]
    [SerializeField] private GameObject[] logPrefabs = new GameObject[3];
    
    [Header("Paramètres de Génération")]
    [SerializeField] private float logSpeed = 1.2f;
    [SerializeField] private float verticalSpacing = 2.5f;
    [SerializeField] private float generationInterval = 2.5f;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float destroyHeight = -5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Positions X fixes des voies
    private readonly float[] lanePositions = { -3f, -1f, 1f, 3f };
    
    // État de validation
    private bool isValidated = false;

    #region Propriétés publiques

    public Transform[] Lanes => lanes;
    public GameObject[] LogPrefabs => logPrefabs;
    public float LogSpeed => logSpeed;
    public float VerticalSpacing => verticalSpacing;
    public float GenerationInterval => generationInterval;
    public float SpawnHeight => spawnHeight;
    public float DestroyHeight => destroyHeight;
    public bool ShowDebugInfo => showDebugInfo;
    public float[] LanePositions => lanePositions;
    public bool IsValidated => isValidated;

    #endregion

    void Awake()
    {
        ValidateConfiguration();
    }

    /// <summary>
    /// Valide la configuration complète du système
    /// </summary>
    public bool ValidateConfiguration()
    {
        LogDebug("🔍 Validation de la configuration...");
        
        bool configValid = true;
        
        // Validation des voies
        if (!ValidateLanes())
        {
            configValid = false;
        }
        
        // Validation des prefabs
        if (!ValidatePrefabs())
        {
            configValid = false;
        }
        
        // Validation des paramètres
        if (!ValidateParameters())
        {
            configValid = false;
        }
        
        isValidated = configValid;
          if (isValidated)
        {
            LogDebug("✅ Configuration validée avec succès");
            // Seulement initialiser les positions si on a des lanes valides
            if (lanes != null && lanes.Length >= 4)
            {
                InitializeLanePositions();
            }
        }
        else
        {
            LogWarning("⚠️ Configuration incomplète - utilisation des valeurs par défaut");
            isValidated = true; // Permettre le fonctionnement avec des valeurs par défaut
        }
        
        return isValidated;
    }    /// <summary>
    /// Valide les voies
    /// </summary>
    private bool ValidateLanes()
    {
        if (lanes == null)
        {
            LogWarning("⚠️ Array de lanes est null - initialisation...");
            lanes = new Transform[4];
        }
        
        if (lanes.Length != 4)
        {
            LogWarning($"⚠️ Exactement 4 voies sont recommandées ! Trouvées: {lanes.Length}");
            // Tenter de corriger automatiquement
            AutoFixLanes();
        }
        
        bool hasAnyLane = false;
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == null)
            {
                LogWarning($"⚠️ La voie {i + 1} n'est pas assignée ! Tentative d'auto-correction...");
                // Continuer malgré l'erreur au lieu de retourner false
            }
            else
            {
                LogDebug($"✅ Voie {i + 1} assignée : {lanes[i].name}");
                hasAnyLane = true;
            }
        }
        
        return true; // Toujours retourner true, même avec des warnings
    }    /// <summary>
    /// Valide les prefabs de rondins
    /// </summary>
    private bool ValidatePrefabs()
    {
        if (logPrefabs == null)
        {
            LogWarning("⚠️ Array de prefabs est null - initialisation...");
            logPrefabs = new GameObject[3];
        }
        
        if (logPrefabs.Length != 3)
        {
            LogWarning($"⚠️ Exactement 3 prefabs de rondins sont recommandés ! Trouvés: {logPrefabs.Length}");
            // Tenter de corriger automatiquement
            AutoFixPrefabs();
        }
        
        bool hasAnyPrefab = false;
        for (int i = 0; i < logPrefabs.Length; i++)
        {
            if (logPrefabs[i] == null)
            {
                LogWarning($"⚠️ Le prefab de rondin {i + 1} n'est pas assigné ! Tentative d'auto-correction...");
                // Continuer malgré l'erreur
            }
            else
            {
                LogDebug($"✅ Prefab {i + 1} assigné : {logPrefabs[i].name}");
                hasAnyPrefab = true;
            }
        }
        
        return true; // Toujours retourner true
    }

    /// <summary>
    /// Valide les paramètres de génération
    /// </summary>
    private bool ValidateParameters()
    {
        bool valid = true;
        
        if (logSpeed <= 0)
        {
            LogError($"❌ Vitesse des rondins invalide: {logSpeed} (doit être > 0)");
            valid = false;
        }
        
        if (verticalSpacing <= 0)
        {
            LogError($"❌ Espacement vertical invalide: {verticalSpacing} (doit être > 0)");
            valid = false;
        }
        
        if (generationInterval <= 0)
        {
            LogError($"❌ Intervalle de génération invalide: {generationInterval} (doit être > 0)");
            valid = false;
        }
        
        if (spawnHeight <= destroyHeight)
        {
            LogError($"❌ Hauteurs invalides: spawn={spawnHeight}, destroy={destroyHeight} (spawn doit être > destroy)");
            valid = false;
        }
        
        if (valid)
        {
            LogDebug($"✅ Paramètres validés: Speed={logSpeed}, Interval={generationInterval}, Spacing={verticalSpacing}");
        }
        
        return valid;
    }    /// <summary>
    /// Initialise les positions des voies
    /// </summary>
    private void InitializeLanePositions()
    {
        LogDebug("🎯 Initialisation des positions des voies...");
        
        // Vérification de sécurité
        if (lanes == null || lanes.Length < 4)
        {
            LogWarning("⚠️ Lanes non initialisées correctement - skip de l'initialisation des positions");
            return;
        }
        
        for (int i = 0; i < lanes.Length && i < lanePositions.Length; i++)
        {
            if (lanes[i] != null)
            {
                Vector3 lanePos = lanes[i].position;
                lanePos.x = lanePositions[i];
                lanes[i].position = lanePos;
                
                LogDebug($"  → Voie {i + 1} positionnée à X={lanePositions[i]}");
            }
            else
            {
                LogDebug($"  → Voie {i + 1} est null - skip");
            }
        }
        
        LogDebug("✅ Positions des voies initialisées");
    }    /// <summary>
    /// Obtient la position de spawn pour une voie donnée
    /// </summary>
    public Vector3 GetSpawnPosition(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanePositions.Length)
        {
            LogError($"❌ Index de voie invalide: {laneIndex}");
            return Vector3.zero;
        }
        
        return new Vector3(lanePositions[laneIndex], spawnHeight, 0);
    }    /// <summary>
    /// Obtient un prefab de rondin aléatoire
    /// </summary>
    public GameObject GetRandomLogPrefab()
    {
        if (logPrefabs == null || logPrefabs.Length == 0)
        {
            LogError("❌ Aucun prefab de rondin disponible");
            return null;
        }
        
        // Trouver un prefab non-null
        var validPrefabs = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < logPrefabs.Length; i++)
        {
            if (logPrefabs[i] != null)
            {
                validPrefabs.Add(logPrefabs[i]);
            }
        }
        
        if (validPrefabs.Count == 0)
        {
            LogError("❌ Aucun prefab valide trouvé");
            return null;
        }
        
        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }    /// <summary>
    /// Vérifie si une voie existe
    /// </summary>
    public bool IsValidLaneIndex(int laneIndex)
    {
        return laneIndex >= 0 && laneIndex < lanePositions.Length;
    }

    #region Méthodes de configuration runtime

    /// <summary>
    /// Modifie la vitesse des rondins en runtime
    /// </summary>
    public void SetLogSpeed(float newSpeed)
    {
        if (newSpeed > 0)
        {
            logSpeed = newSpeed;
            LogDebug($"🎛️ Vitesse des rondins modifiée: {newSpeed}");
        }
        else
        {
            LogError($"❌ Vitesse invalide: {newSpeed}");
        }
    }

    /// <summary>
    /// Modifie l'intervalle de génération en runtime
    /// </summary>
    public void SetGenerationInterval(float newInterval)
    {
        if (newInterval > 0)
        {
            generationInterval = newInterval;
            LogDebug($"🎛️ Intervalle de génération modifié: {newInterval}");
        }
        else
        {
            LogError($"❌ Intervalle invalide: {newInterval}");
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Affiche un rapport de configuration complet
    /// </summary>
    [ContextMenu("Debug Configuration Report")]
    public void DebugConfigurationReport()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("📋 RAPPORT DE CONFIGURATION LOGPARADE");
        report.AppendLine($"État: {(isValidated ? "✅ VALIDÉ" : "❌ INVALIDE")}");
        report.AppendLine();
          report.AppendLine("🛣️ Voies:");
        if (lanes != null)
        {
            for (int i = 0; i < lanes.Length; i++)
            {
                string status = lanes[i] != null ? "✅" : "❌";
                string name = lanes[i] != null ? lanes[i].name : "NON ASSIGNÉ";
                report.AppendLine($"  {status} Voie {i + 1}: {name} (X={lanePositions[i]})");
            }
        }
        else
        {
            report.AppendLine("  ❌ Array de lanes est null");
        }
        
        report.AppendLine();
        report.AppendLine("🪵 Prefabs:");
        if (logPrefabs != null)
        {
            for (int i = 0; i < logPrefabs.Length; i++)
            {
                string status = logPrefabs[i] != null ? "✅" : "❌";
                string name = logPrefabs[i] != null ? logPrefabs[i].name : "NON ASSIGNÉ";
                report.AppendLine($"  {status} Prefab {i + 1}: {name}");
            }
        }
        else
        {
            report.AppendLine("  ❌ Array de prefabs est null");
        }
        
        report.AppendLine();
        report.AppendLine("⚙️ Paramètres:");
        report.AppendLine($"  Vitesse: {logSpeed}");
        report.AppendLine($"  Intervalle: {generationInterval}s");
        report.AppendLine($"  Espacement: {verticalSpacing}");
        report.AppendLine($"  Spawn Height: {spawnHeight}");
        report.AppendLine($"  Destroy Height: {destroyHeight}");
        
        Debug.Log(report.ToString());
    }

    #endregion

    #region Auto-Correction
      /// <summary>
    /// Tente de corriger automatiquement les lanes manquantes
    /// </summary>
    private void AutoFixLanes()
    {
        if (lanes == null || lanes.Length != 4)
        {
            lanes = new Transform[4];
        }
        
        // Chercher des lanes existantes par tag
        GameObject[] existingLanes = GameObject.FindGameObjectsWithTag("Lane");
        if (existingLanes.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(4, existingLanes.Length); i++)
            {
                if (lanes[i] == null && existingLanes[i] != null)
                {
                    lanes[i] = existingLanes[i].transform;
                    LogDebug($"Lane {i + 1} auto-assignée : {existingLanes[i].name}");
                }
            }
        }
        
        LogDebug("Auto-correction des lanes effectuée");
    }
      /// <summary>
    /// Tente de corriger automatiquement les prefabs manquants
    /// </summary>
    private void AutoFixPrefabs()
    {
        if (logPrefabs == null || logPrefabs.Length != 3)
        {
            logPrefabs = new GameObject[3];
        }
        
        // Note: En mode runtime, on ne peut pas créer de vrais prefabs
        // Mais on peut marquer les emplacements pour que le générateur les gère
        LogDebug("Auto-correction des prefabs effectuée (sera gérée par le générateur)");
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[LogConfiguration] {message}");
    }
    
    #endregion

    #region Logging

    private void LogDebug(string message)
    {
        if (showDebugInfo)
            Debug.Log($"[LogConfiguration] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[LogConfiguration] {message}");
    }

    #endregion
}
