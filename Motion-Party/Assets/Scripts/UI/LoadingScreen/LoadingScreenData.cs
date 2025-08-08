using UnityEngine;

[CreateAssetMenu(fileName = "LoadingScreenData", menuName = "Motion-Party/Loading Screen Data")]
public class LoadingScreenData : ScriptableObject
{
    [System.Serializable]
    public class LoadingTip
    {
        [Header("Astuce")]
        public string tipId;
        [TextArea(2, 4)]
        public string tipText;
        
        [Header("Image du mini-jeu (optionnel)")]
        public Sprite gamePreviewImage;
        
        [Header("Couleur de fond (optionnel)")]
        public Color backgroundColor = Color.black;
    }
    
    [Header("Configuration des astuces")]
    public LoadingTip[] tips;
    
    [Header("Astuces par défaut")]
    [TextArea(2, 4)]
    public string defaultTipText = "Préparez-vous pour le prochain défi !";
    public Sprite defaultGameImage;
    
    /// <summary>
    /// Récupère une astuce par son ID
    /// </summary>
    public LoadingTip GetTip(string tipId)
    {
        Debug.Log($"[LoadingScreenData] GetTip appelé avec tipId: '{tipId}'");
        
        if (string.IsNullOrEmpty(tipId))
        {
            Debug.Log("[LoadingScreenData] tipId vide, utilisation de l'astuce par défaut");
            return CreateDefaultTip();
        }
            
        if (tips == null)
        {
            Debug.LogWarning("[LoadingScreenData] Aucune astuce configurée, utilisation de l'astuce par défaut");
            return CreateDefaultTip();
        }
        
        Debug.Log($"[LoadingScreenData] Recherche dans {tips.Length} astuces disponibles:");
        foreach (var tip in tips)
        {
            Debug.Log($"  - {tip.tipId}");
            if (tip.tipId == tipId)
            {
                Debug.Log($"[LoadingScreenData] Astuce trouvée: '{tip.tipText}'");
                return tip;
            }
        }
        
        Debug.LogWarning($"[LoadingScreenData] Astuce '{tipId}' introuvable, utilisation de l'astuce par défaut");
        return CreateDefaultTip();
    }
    
    /// <summary>
    /// Crée une astuce par défaut
    /// </summary>
    private LoadingTip CreateDefaultTip()
    {
        return new LoadingTip
        {
            tipId = "default",
            tipText = defaultTipText,
            gamePreviewImage = defaultGameImage,
            backgroundColor = Color.black
        };
    }
}
