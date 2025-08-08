using UnityEngine;

[System.Serializable]
public class MiniGameInfo 
{
    [Header("Configuration de base")]
    public string sceneName;
    
    [Header("Écran de chargement")]
    public string loadingTipId;
    
    [Header("Informations optionnelles")]
    public string displayName;
    [TextArea(2, 3)]
    public string description;
}
