using UnityEngine;

/// <summary>
/// Structure pour un message d'encouragement avec son audio associé
/// </summary>
[System.Serializable]
public class EncouragementMessage
{
    [Header("Message")]
    public string text;
    
    [Header("Audio")]
    public AudioClip audioClip;
    
    [Header("Paramètres d'affichage")]
    [Range(1f, 10f)]
    public float displayDuration = 3f;
    
    [Header("Identifiant unique")]
    public string messageId;

    public EncouragementMessage(string text, AudioClip audioClip = null, float displayDuration = 3f)
    {
        this.text = text;
        this.audioClip = audioClip;
        this.displayDuration = displayDuration;
        this.messageId = System.Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Vérifie si le message a un audio associé
    /// </summary>
    public bool HasAudio => audioClip != null;
}
