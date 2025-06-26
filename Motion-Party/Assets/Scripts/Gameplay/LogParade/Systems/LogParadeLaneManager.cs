using UnityEngine;

/// <summary>
/// Gestionnaire principal pour les lanes dans LogParade
/// Permet de contrôler facilement l'utilisation des lanes préexistantes vs génération automatique
/// </summary>
public class LogParadeLaneManager : MonoBehaviour
{
    [Header("Configuration des Lanes")]
    [SerializeField] private bool usePrebuiltLanes = true; // Utiliser les lanes préfabriquées
    [SerializeField] private bool disableAutoGeneration = true; // Désactiver la génération automatique
    
    [Header("Lanes Préfabriquées")]
    [SerializeField] private GameObject[] prebuiltLanes = new GameObject[4];
    
    [Header("Références")]
    [SerializeField] private LogParadeLaneVisualizer laneVisualizer;
    
    void Awake()
    {
        // S'assurer que la génération automatique est désactivée si on veut utiliser des lanes préfabriquées
        if (usePrebuiltLanes && disableAutoGeneration)
        {
            DisableAutoGeneration();
        }
    }
    
    void Start()
    {
        if (usePrebuiltLanes)
        {
            SetupPrebuiltLanes();
        }
    }
      /// <summary>
    /// Désactive la génération automatique de lanes
    /// </summary>
    public void DisableAutoGeneration()
    {
        // Maintenant que la génération automatique est désactivée par défaut,
        // cette méthode sert principalement à confirmer le statut
        LogParadeLogger.LogVerbose("Génération automatique de lanes désactivée - utilisation des lanes préfabriquées");
    }
    
    /// <summary>
    /// Configure les lanes préfabriquées
    /// </summary>
    public void SetupPrebuiltLanes()
    {
        if (!HasAllPrebuiltLanes())
        {
            LogParadeLogger.LogWarning("Toutes les 4 lanes préfabriquées ne sont pas assignées !");
            return;
        }
          if (laneVisualizer == null)
        {
            laneVisualizer = FindFirstObjectByType<LogParadeLaneVisualizer>();
        }
          if (laneVisualizer != null)
        {
            // Assigner les lanes préfabriquées au visualizer
            laneVisualizer.SetExternalLanes(prebuiltLanes);
            LogParadeLogger.LogVerbose("Lanes préfabriquées configurées avec succès !");
        }
    }
    
    /// <summary>
    /// Vérifie si toutes les lanes préfabriquées sont assignées
    /// </summary>
    private bool HasAllPrebuiltLanes()
    {
        for (int i = 0; i < 4; i++)
        {
            if (prebuiltLanes[i] == null)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Assigne automatiquement les lanes trouvées dans la scène
    /// </summary>
    [ContextMenu("Auto-Assign Prebuilt Lanes")]
    public void AutoAssignPrebuiltLanes()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int laneCount = 0;
        
        // Chercher des objets qui ressemblent à des lanes
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            if ((objName.Contains("lane") || objName.Contains("voie") || objName.Contains("track")) 
                && laneCount < 4)
            {
                // Éviter les lanes générées automatiquement
                if (!objName.StartsWith("lane_"))
                {
                    prebuiltLanes[laneCount] = obj;
                    laneCount++;
                }
            }
        }
        
        if (laneCount == 4)
        {
            LogParadeLogger.LogVerbose($"4 lanes préfabriquées trouvées et assignées automatiquement !");
            usePrebuiltLanes = true;
        }
        else
        {
            LogParadeLogger.LogWarning($"Seulement {laneCount} lanes trouvées. Veuillez créer ou assigner les lanes manuellement.");
        }
    }
    
    /// <summary>
    /// Réactive la génération automatique (si nécessaire)
    /// </summary>
    [ContextMenu("Enable Auto Generation")]
    public void EnableAutoGeneration()
    {
        usePrebuiltLanes = false;
        disableAutoGeneration = false;
        
        if (laneVisualizer != null)
        {
            laneVisualizer.GenerateLanes();
        }
        
        LogParadeLogger.LogVerbose("Génération automatique de lanes réactivée");
    }
}
