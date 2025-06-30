using UnityEngine;

/// <summary>
/// Gestionnaire principal pour les lanes dans LogParade
/// Permet de gérer l'utilisation des lanes préfabriquées uniquement
/// </summary>
public class LogParadeLaneManager : MonoBehaviour
{
    #region Fields
    [Header("Lanes Préfabriquées")]
    [SerializeField] private GameObject[] prebuiltLanes = new GameObject[4];
    [Header("Références")]
    [SerializeField] private LogParadeLaneVisualizer laneVisualizer;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        // Plus de génération automatique : on suppose toujours l'utilisation des lanes préfabriquées
    }
    void Start()
    {
        SetupPrebuiltLanes();
    }
    #endregion

    #region Lane Management
    /// <summary>
    /// Configure les lanes 
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
            laneVisualizer.SetExternalLanes(prebuiltLanes);
        }
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Vérifie si toutes les lanes sont assignées
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
                prebuiltLanes[laneCount] = obj;
                laneCount++;
            }
        }
        if (laneCount != 4)
        {
           LogParadeLogger.LogWarning($"Seulement {laneCount} lanes trouvées. Veuillez créer ou assigner les lanes manuellement.");
        }
    }
    #endregion
}
