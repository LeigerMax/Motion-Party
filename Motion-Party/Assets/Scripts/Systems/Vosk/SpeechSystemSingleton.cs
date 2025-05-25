using UnityEngine;

public class SpeechSystemSingleton : MonoBehaviour
{
    public static SpeechSystemSingleton Instance { get; private set; }

    [Header("Composants Vosk")] // Permet d'assigner dans l'inspecteur
    public VoskSpeechToText voskSpeechToText;
    public VoiceProcessor voiceProcessor;
    public VoskCommandProcessor voskCommandProcessor;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre les scènes
            // Optionnel : auto-référence si les composants sont sur le même GameObject
            if (voskSpeechToText == null) voskSpeechToText = GetComponent<VoskSpeechToText>();
            if (voiceProcessor == null) voiceProcessor = GetComponent<VoiceProcessor>();
            if (voskCommandProcessor == null) voskCommandProcessor = GetComponent<VoskCommandProcessor>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Évite les doublons
        }
    }

    // Accès statique pratique
    public static VoskSpeechToText Vosk => Instance?.voskSpeechToText;
    public static VoiceProcessor Voice => Instance?.voiceProcessor;
    public static VoskCommandProcessor CommandProcessor => Instance?.voskCommandProcessor;
}

