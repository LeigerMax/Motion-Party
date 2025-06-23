using UnityEngine;

/// <summary>
/// Script de diagnostic temporaire pour tester la détection des rondins.
/// À attacher sur vos objets rondins pour vérifier qu'ils détectent bien le joueur.
/// Supprimez ce script une fois que PlayerOnLogChecker fonctionne correctement.
/// </summary>
public class LogDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Info")]
    public bool showDetailedLogs = true;
    
    void Start()
    {
        // Vérifier la configuration au démarrage
        ValidateConfiguration();
    }
    
    void ValidateConfiguration()
    {
        Debug.Log($"=== DIAGNOSTIC RONDIN: {gameObject.name} ===");
        
        // Vérifier le tag
        if (gameObject.CompareTag("Log"))
        {
            Debug.Log($"✅ Tag 'Log' correctement assigné sur {gameObject.name}");
        }
        else
        {
            Debug.LogError($"❌ PROBLÈME: Tag 'Log' manquant sur {gameObject.name}. Tag actuel: '{gameObject.tag}'");
        }
        
        // Vérifier les colliders
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;
        
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                Debug.Log($"✅ Collider trigger trouvé: {col.GetType().Name} sur {gameObject.name}");
            }
            else
            {
                Debug.Log($"ℹ️ Collider non-trigger: {col.GetType().Name} sur {gameObject.name}");
            }
        }
        
        if (!hasTrigger)
        {
            Debug.LogError($"❌ PROBLÈME: Aucun collider configuré en 'Is Trigger' sur {gameObject.name}");
        }
        
        // Info sur le layer
        Debug.Log($"ℹ️ Layer: {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer})");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[LOG DIAGNOSTIC - {gameObject.name}] 🟢 TRIGGER ENTER: {other.name} (Tag: '{other.tag}', Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        }
        
        // Vérifier si c'est le joueur
        PlayerOnLogChecker playerChecker = other.GetComponent<PlayerOnLogChecker>();
        if (playerChecker != null)
        {
            Debug.Log($"[LOG DIAGNOSTIC - {gameObject.name}] ✅ JOUEUR DÉTECTÉ! PlayerOnLogChecker trouvé sur {other.name}");
        }
        else
        {
            Debug.Log($"[LOG DIAGNOSTIC - {gameObject.name}] ⚠️ Objet détecté mais pas de PlayerOnLogChecker: {other.name}");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        // Log moins fréquent pour éviter le spam
        if (showDetailedLogs && Time.frameCount % 60 == 0) // Une fois par seconde environ
        {
            Debug.Log($"[LOG DIAGNOSTIC - {gameObject.name}] 🔄 TRIGGER STAY: {other.name}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[LOG DIAGNOSTIC - {gameObject.name}] 🔴 TRIGGER EXIT: {other.name} (Tag: '{other.tag}')");
        }
        
        PlayerOnLogChecker playerChecker = other.GetComponent<PlayerOnLogChecker>();
        if (playerChecker != null)
        {
            Debug.Log($"[LOG DIAGNOSTIC - {gameObject.name}] ❌ JOUEUR QUITTÉ! {other.name}");
        }
    }
    
    void OnDrawGizmos()
    {
        // Dessiner la zone de détection du rondin
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Vert transparent
            
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            }
        }
    }
}
