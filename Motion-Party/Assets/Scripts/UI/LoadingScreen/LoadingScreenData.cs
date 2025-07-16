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
        if (string.IsNullOrEmpty(tipId))
            return CreateDefaultTip();
            
        foreach (var tip in tips)
        {
            if (tip.tipId == tipId)
                return tip;
        }
        
        Debug.LogWarning($"Astuce '{tipId}' introuvable, utilisation de l'astuce par défaut");
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
