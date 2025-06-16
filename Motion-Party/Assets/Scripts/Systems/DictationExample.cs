using UnityEngine;
using UnityEngine.Windows.Speech;

// Reconnaissance vocale Windows pour transcrire la parole en texte
public class DictationExample : MonoBehaviour
{
    private DictationRecognizer recognizer;

    void Start()
    {
        recognizer = new DictationRecognizer();
        recognizer.DictationResult += (text, confidence) =>
        {
            Debug.Log("Texte reconnu : " + text);
        };
        recognizer.Start();
    }

    void OnApplicationQuit()
    {
        recognizer.Stop();
        recognizer.Dispose();
    }
}
