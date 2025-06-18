using UnityEngine;

/// <summary>
/// Composant helper pour référencer les 4 voies manuellement placées dans la scène Unity
/// Version 1.2 - NE GÉNÈRE PLUS automatiquement, utilise les lanes existantes
/// </summary>
public class LogParadeLaneVisualizer : MonoBehaviour
{
    [Header("Manual Lane References")]
    [Tooltip("Assignez manuellement les 4 lanes existantes dans la scène")]
    public Transform[] manualLanes = new Transform[4];
    
    [Header("Lane Highlighting")]
    public Material activeLaneMaterial;
    public Material inactiveLaneMaterial;
    public Color[] laneColors = new Color[4] 
    { 
        Color.red,      // Voie 1 - Gauche
        Color.yellow,   // Voie 2 - Centre-gauche  
        Color.green,    // Voie 3 - Centre-droite
        Color.blue      // Voie 4 - Droite
    };
    
    [Header("Debug")]
    public bool showLaneDebug = true;
    
    private Renderer[] laneRenderers = new Renderer[4];
    private int currentActiveLane = -1;

    void Start()
    {
        // Ne plus générer automatiquement - utiliser les références manuelles
        if (Application.isPlaying)
        {
            ValidateManualLanes();
            InitializeLaneRenderers();
        }
    }

    /// <summary>
    /// Valide que toutes les lanes manuelles sont assignées
    /// </summary>
    private void ValidateManualLanes()
    {
        bool allAssigned = true;
        for (int i = 0; i < 4; i++)
        {
            if (manualLanes[i] == null)
            {
                Debug.LogError($"🚨 Lane manuelle {i + 1} n'est pas assignée dans LogParadeLaneVisualizer !");
                allAssigned = false;
            }
        }
        
        if (allAssigned && showLaneDebug)
        {
            Debug.Log("✅ Toutes les lanes manuelles sont correctement assignées !");
        }
    }

    /// <summary>
    /// Initialise les renderers des lanes pour le highlighting
    /// </summary>
    private void InitializeLaneRenderers()
    {
        for (int i = 0; i < 4; i++)
        {
            if (manualLanes[i] != null)
            {
                laneRenderers[i] = manualLanes[i].GetComponent<Renderer>();
                if (laneRenderers[i] == null)
                {
                    Debug.LogWarning($"⚠️ Aucun Renderer trouvé sur la lane {i + 1}. Le highlighting ne fonctionnera pas.");
                }
            }
        }
    }

    /// <summary>
    /// Met en surbrillance une voie spécifique
    /// </summary>
    public void HighlightLane(int laneIndex)
    {
        if (laneIndex < 1 || laneIndex > 4) return;
        
        // Désactiver la surbrillance précédente
        if (currentActiveLane != -1 && currentActiveLane <= laneRenderers.Length)
        {
            SetLaneHighlight(currentActiveLane - 1, false);
        }
        
        // Activer la nouvelle surbrillance
        SetLaneHighlight(laneIndex - 1, true);
        currentActiveLane = laneIndex;
        
        if (showLaneDebug)
            Debug.Log($"🎯 Lane {laneIndex} mise en surbrillance");
    }

    /// <summary>
    /// Active/désactive la surbrillance d'une lane
    /// </summary>
    private void SetLaneHighlight(int index, bool highlight)
    {
        if (index < 0 || index >= laneRenderers.Length || laneRenderers[index] == null) return;
        
        if (highlight)
        {
            // Appliquer la couleur de la voie
            if (activeLaneMaterial != null)
            {
                laneRenderers[index].material = activeLaneMaterial;
                laneRenderers[index].material.color = laneColors[index];
            }
        }
        else
        {
            // Appliquer le matériau inactif
            if (inactiveLaneMaterial != null)
            {
                laneRenderers[index].material = inactiveLaneMaterial;
            }
        }
    }

    /// <summary>
    /// Obtient la position d'une lane spécifique
    /// </summary>
    public Vector3 GetLanePosition(int laneIndex)
    {
        if (laneIndex < 1 || laneIndex > 4 || manualLanes[laneIndex - 1] == null)
        {
            Debug.LogWarning($"🚨 Lane {laneIndex} non assignée ou invalide !");
            return Vector3.zero;
        }
        
        return manualLanes[laneIndex - 1].position;
    }

    /// <summary>
    /// Obtient toutes les positions des lanes
    /// </summary>
    public Vector3[] GetAllLanePositions()
    {
        Vector3[] positions = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            positions[i] = GetLanePosition(i + 1);
        }
        return positions;
    }

    /// <summary>
    /// Obtient la largeur entre les lanes pour calibration
    /// </summary>
    public float GetLaneSpacing()
    {
        if (manualLanes[0] != null && manualLanes[3] != null)
        {
            float totalWidth = Vector3.Distance(manualLanes[0].position, manualLanes[3].position);
            return totalWidth / 3f; // Espacement entre 4 lanes = 3 intervalles
        }
        
        return 2f; // Valeur par défaut
    }

    /// <summary>
    /// Réinitialise toutes les surbrillances
    /// </summary>
    public void ClearAllHighlights()
    {
        for (int i = 0; i < 4; i++)
        {
            SetLaneHighlight(i, false);
        }
        currentActiveLane = -1;
    }

    void OnDrawGizmos()
    {
        if (!showLaneDebug) return;
        
        // Dessiner les positions des lanes dans l'éditeur
        for (int i = 0; i < 4; i++)
        {
            if (manualLanes[i] != null)
            {
                Gizmos.color = laneColors[i];
                Gizmos.DrawWireCube(manualLanes[i].position, Vector3.one * 0.5f);
                
                // Afficher le numéro de la lane
                Gizmos.color = Color.white;
                Vector3 labelPos = manualLanes[i].position + Vector3.up * 1f;
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPos, $"Lane {i + 1}");
                #endif
            }
        }
    }
}
