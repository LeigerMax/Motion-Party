using UnityEngine;

/// <summary>
/// Script de test pour créer rapidement des prefabs de rondins temporaires
/// À utiliser uniquement pour tester le LogParadeLogGenerator
/// </summary>
public class LogParadeTestPrefabCreator : MonoBehaviour
{
    [Header("Test - Création de Prefabs Temporaires")]
    [SerializeField] private bool createTestPrefabs = false;
    [SerializeField] private LogParadeLogGenerator logGenerator;
    
    [ContextMenu("Créer Prefabs de Test")]
    public void CreateTestPrefabs()
    {
        if (logGenerator == null)
        {
            Debug.LogError("[TestPrefabCreator] LogParadeLogGenerator non assigné !");
            return;
        }
        
        // Création de 3 prefabs temporaires
        GameObject[] testPrefabs = new GameObject[3];
        
        for (int i = 0; i < 3; i++)
        {
            // Créer un GameObject de base
            GameObject prefab = CreateLogPrefab($"LogPrefab_Test_{i + 1}", i);
            testPrefabs[i] = prefab;
            
            Debug.Log($"[TestPrefabCreator] Prefab créé : {prefab.name}");
        }
        
        Debug.Log("[TestPrefabCreator] 3 prefabs de test créés avec succès !");
        Debug.Log("Vous pouvez maintenant les assigner manuellement au LogParadeLogGenerator dans l'inspecteur.");
    }
    
    private GameObject CreateLogPrefab(string name, int index)
    {
        // Créer un GameObject
        GameObject logObj = new GameObject(name);
        
        // Ajouter un cube primitif pour la visualisation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(logObj.transform);
        visual.transform.localPosition = Vector3.zero;
        
        // Tailles différentes selon le type
        switch (index)
        {
            case 0: // Short
                visual.transform.localScale = new Vector3(1f, 1f, 1f);
                break;
            case 1: // Medium
                visual.transform.localScale = new Vector3(1f, 1f, 1.5f);
                break;
            case 2: // Long
                visual.transform.localScale = new Vector3(1f, 1f, 2f);
                break;
        }
        
        // Couleurs différentes pour distinguer
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            switch (index)
            {
                case 0:
                    mat.color = Color.green;
                    break;
                case 1:
                    mat.color = Color.yellow;
                    break;
                case 2:
                    mat.color = Color.red;
                    break;
            }
            renderer.material = mat;
        }
        
        // Ajouter un BoxCollider au parent
        BoxCollider collider = logObj.AddComponent<BoxCollider>();
        collider.size = visual.transform.localScale;
        
        // Optionnel : Rigidbody
        Rigidbody rb = logObj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        
        return logObj;
    }
    
    private void Update()
    {
        // Raccourci clavier pour créer les prefabs rapidement
        if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftControl))
        {
            CreateTestPrefabs();
        }
    }
}
