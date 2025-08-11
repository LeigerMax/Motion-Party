using UnityEngine;

/// <summary>
/// Script d'exemple montrant comment utiliser le système d'encouragement
/// À ajouter sur un GameObject dans la scène LogParade pour tester
/// </summary>
public class EncouragementSystemExample : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private EncouragementManager encouragementManager;
    
    [Header("Test")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private KeyCode testKey = KeyCode.Space;

    void Start()
    {
        // Auto-recherche du manager
        if (encouragementManager == null)
        {
            encouragementManager = FindFirstObjectByType<EncouragementManager>();
        }

        // Créer un manager si aucun n'existe
        if (encouragementManager == null && autoStart)
        {
            CreateEncouragementManager();
        }

        // Démarrer automatiquement si configuré
        if (autoStart && encouragementManager != null)
        {
            encouragementManager.StartEncouragement();
            Debug.Log("[EncouragementSystemExample] Système d'encouragement démarré automatiquement");
        }
    }

    void Update()
    {
        // Test manuel avec une touche
        if (Input.GetKeyDown(testKey) && encouragementManager != null)
        {
            encouragementManager.ShowRandomMessage();
            Debug.Log("[EncouragementSystemExample] Message d'encouragement déclenché manuellement");
        }
    }

    /// <summary>
    /// Crée un EncouragementManager avec configuration par défaut
    /// </summary>
    private void CreateEncouragementManager()
    {
        GameObject managerGO = new GameObject("EncouragementManager");
        managerGO.transform.SetParent(transform);
        
        encouragementManager = managerGO.AddComponent<EncouragementManager>();
        
        // Le manager se configurera automatiquement avec les messages par défaut
        Debug.Log("[EncouragementSystemExample] EncouragementManager créé automatiquement");
    }

    /// <summary>
    /// Méthode publique pour démarrer le système (peut être appelée depuis l'Inspector)
    /// </summary>
    [ContextMenu("Démarrer encouragements")]
    public void StartEncouragement()
    {
        if (encouragementManager != null)
        {
            encouragementManager.StartEncouragement();
        }
    }

    /// <summary>
    /// Méthode publique pour arrêter le système (peut être appelée depuis l'Inspector)
    /// </summary>
    [ContextMenu("Arrêter encouragements")]
    public void StopEncouragement()
    {
        if (encouragementManager != null)
        {
            encouragementManager.StopEncouragement();
        }
    }

    /// <summary>
    /// Méthode publique pour afficher un message immédiatement (peut être appelée depuis l'Inspector)
    /// </summary>
    [ContextMenu("Message immédiat")]
    public void ShowImmediateMessage()
    {
        if (encouragementManager != null)
        {
            encouragementManager.ShowRandomMessage();
        }
    }
}
