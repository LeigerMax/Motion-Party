using UnityEngine;

public class LoadingScreenCreator : MonoBehaviour
{
    [Header("Création automatique des données")]
    [SerializeField] private bool createDefaultData = true;
    
    [ContextMenu("Créer les données par défaut")]
    public void CreateDefaultLoadingData()
    {
        // Créer le ScriptableObject
        var loadingData = ScriptableObject.CreateInstance<LoadingScreenData>();
        
        // Configurer les astuces par défaut
        loadingData.defaultTipText = "Préparez-vous pour le prochain défi !";
        
        // Créer quelques astuces d'exemple
        loadingData.tips = new LoadingScreenData.LoadingTip[]
        {
            new LoadingScreenData.LoadingTip
            {
                tipId = "movement_game",
                tipText = "Bouge doucement les bras pour de meilleurs résultats !",
                backgroundColor = new Color(0.1f, 0.3f, 0.7f)
            },
            new LoadingScreenData.LoadingTip
            {
                tipId = "precision_game",
                tipText = "Essaye d'être précis dans tes mouvements !",
                backgroundColor = new Color(0.7f, 0.2f, 0.1f)
            },
            new LoadingScreenData.LoadingTip
            {
                tipId = "speed_game",
                tipText = "Plus tu vas vite, plus tu gagnes de points !",
                backgroundColor = new Color(0.2f, 0.7f, 0.3f)
            },
            new LoadingScreenData.LoadingTip
            {
                tipId = "coordination_game",
                tipText = "Coordonne tes deux mains pour réussir !",
                backgroundColor = new Color(0.7f, 0.5f, 0.2f)
            },
            new LoadingScreenData.LoadingTip
            {
                tipId = "menu_return",
                tipText = "Retour au menu principal... Bien joué !",
                backgroundColor = new Color(0.3f, 0.3f, 0.3f)
            }
        };
        
        // Sauvegarder dans le dossier Resources
        string resourcePath = "Assets/Resources";
        if (!System.IO.Directory.Exists(resourcePath))
        {
            System.IO.Directory.CreateDirectory(resourcePath);
        }
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(loadingData, $"{resourcePath}/LoadingScreenData.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log("LoadingScreenData créé avec succès dans Resources !");
        #endif
    }
    
    private void Start()
    {
        if (createDefaultData)
        {
            CreateDefaultLoadingData();
        }
    }
}
