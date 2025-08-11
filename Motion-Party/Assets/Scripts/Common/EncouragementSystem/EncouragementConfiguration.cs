using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Configuration ScriptableObject pour les messages d'encouragement
/// Permet de créer différents packs de messages pour différents mini-jeux
/// </summary>
[CreateAssetMenu(fileName = "EncouragementConfig", menuName = "Motion Party/Encouragement/Configuration", order = 1)]
public class EncouragementConfiguration : ScriptableObject
{
    [Header("Informations générales")]
    public string configurationName = "Configuration par défaut";
    [TextArea(2, 4)]
    public string description = "Description de cette configuration de messages";
    
    [Header("Messages d'encouragement")]
    public List<EncouragementMessage> messages = new List<EncouragementMessage>();
    
    [Header("Paramètres de timing")]
    [Range(5f, 30f)]
    public float minInterval = 15f;
    [Range(5f, 30f)]
    public float maxInterval = 20f;
    [Range(1f, 10f)]
    public float defaultDisplayDuration = 3f;
    
    [Header("Paramètres audio")]
    [Range(0f, 2f)]
    public float audioVolume = 3f;
    public bool playAudioWithMessage = true;

    #region Validation
    void OnValidate()
    {
        // S'assurer que maxInterval >= minInterval
        if (maxInterval < minInterval)
        {
            maxInterval = minInterval;
        }
        
        // Générer des IDs uniques pour les messages qui n'en ont pas
        for (int i = 0; i < messages.Count; i++)
        {
            if (string.IsNullOrEmpty(messages[i].messageId))
            {
                messages[i].messageId = System.Guid.NewGuid().ToString();
            }
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Applique cette configuration à un EncouragementManager
    /// </summary>
    public void ApplyToManager(EncouragementManager manager)
    {
        if (manager == null) return;

        // Nettoyer les messages existants
        manager.ClearMessages();
        
        // Ajouter tous les messages de cette configuration
        foreach (var message in messages)
        {
            manager.AddMessage(message);
        }
        
        // Appliquer les paramètres de timing (via réflexion si nécessaire)
        // Note: Il faudrait ajouter des méthodes publiques dans EncouragementManager pour cela
    }

    /// <summary>
    /// Crée une configuration par défaut avec tous les messages standard
    /// </summary>
    [ContextMenu("Créer configuration par défaut")]
    public void CreateDefaultConfiguration()
    {
        string[] defaultTexts = {
            "Bien joué !",
            "Continue comme ça !",
            "Super, tu y es presque !",
            "Bravo, tu progresses !",
            "C'est parti, tu gères !",
            "Excellent !",
            "Tu es dans le rythme !",
            "Parfait !",
            "Tu assures !",
            "Encore un peu !",
            "Doucement mais sûrement !",
            "Tu fais ça très bien !",
            "C'est fluide, continue !",
            "Tu tiens le bon rythme !",
            "Belle précision !",
            "Tout en maîtrise !",
            "Les copains vont être fiers !",
            "Le camp entier t'applaudit !",
            "Le chef de colo te félicite !",
            "T'es la star de la veillée !",
            "Les animateurs sont bluffés !",
            "On t'offre la première part de marshmallow !",
            "Le feu de camp est à toi !"
        };

        messages.Clear();
        foreach (string text in defaultTexts)
        {
            messages.Add(new EncouragementMessage(text, null, defaultDisplayDuration));
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    /// <summary>
    /// Ajoute un nouveau message vide
    /// </summary>
    [ContextMenu("Ajouter message vide")]
    public void AddEmptyMessage()
    {
        messages.Add(new EncouragementMessage("Nouveau message", null, defaultDisplayDuration));
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    #endregion
}
