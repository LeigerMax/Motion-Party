using UnityEngine;

/// <summary>
/// Configuration spécifique des astuces pour Motion-Party
/// </summary>
[CreateAssetMenu(fileName = "MotionPartyLoadingTips", menuName = "Motion-Party/Loading Tips Configuration")]
public class MotionPartyLoadingTips : ScriptableObject
{
    [Header("Astuces par mini-jeu")]
    public LoadingScreenData.LoadingTip[] gameSpecificTips = new LoadingScreenData.LoadingTip[]
    {
        new LoadingScreenData.LoadingTip
        {
            tipId = "firefly_dance",
            tipText = "Fermez doucement vos mains pour attraper les lucioles ! Il en existe 3 types.",
            backgroundColor = new Color(0.1f, 0.1f, 0.3f)
        },
        new LoadingScreenData.LoadingTip
        {
            tipId = "log_parade",
            tipText = "Bougez votre corps de gauche à droite pour rester sur les buches !",
            backgroundColor = new Color(0.3f, 0.2f, 0.1f)
        },
        new LoadingScreenData.LoadingTip
        {
            tipId = "movement_game",
            tipText = "Faites des mouvements fluides pour de meilleurs résultats !",
            backgroundColor = new Color(0.1f, 0.3f, 0.1f)
        },
        new LoadingScreenData.LoadingTip
        {
            tipId = "precision_game",
            tipText = "La précision est plus importante que la vitesse !",
            backgroundColor = new Color(0.3f, 0.1f, 0.1f)
        },
        new LoadingScreenData.LoadingTip
        {
            tipId = "session_complete",
            tipText = "Bravo ! Retour au menu principal...",
            backgroundColor = new Color(0.2f, 0.2f, 0.2f)
        },
        new LoadingScreenData.LoadingTip
        {
            tipId = "music_note",
            tipText = "Retiens les notes pour réussir la séquence, ensuite affiche avec ta main chaque nombre !",
            backgroundColor = new Color(0.2f, 0.2f, 0.2f)
        }
    };
    
    [Header("Configuration globale")]
    public string defaultTipText = "Préparez-vous pour le prochain défi !";
    public Color defaultBackgroundColor = Color.black;
    
    /// <summary>
    /// Applique ces astuces à un LoadingScreenData
    /// </summary>
    public void ApplyToLoadingScreenData(LoadingScreenData data)
    {
        if (data == null) return;
        
        data.defaultTipText = defaultTipText;
        data.tips = gameSpecificTips;
        
        Debug.Log($"MotionPartyLoadingTips: {gameSpecificTips.Length} astuces appliquées");
    }
    
    /// <summary>
    /// Crée un LoadingScreenData avec ces astuces
    /// </summary>
    public LoadingScreenData CreateLoadingScreenData()
    {
        var data = ScriptableObject.CreateInstance<LoadingScreenData>();
        ApplyToLoadingScreenData(data);
        return data;
    }
}
