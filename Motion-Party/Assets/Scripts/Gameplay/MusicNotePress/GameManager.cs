using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;

public class GameManager : MonoBehaviour
{
    public UDPReceive udpReceive;
    public UIManager uiManager;  // Assure-toi que l'UI est bien mis à jour
    public NoteSequenceManager noteSequenceManager; // Référence à NoteSequenceManager
    private int openFingers = 0;

    void Update()
    {
        string data = udpReceive.data;
        if (string.IsNullOrEmpty(data)) return;

        try
        {
            JObject jsonData = JObject.Parse(data);
            openFingers = (int)jsonData["open_fingers"];

            // Met à jour l'affichage du nombre de doigts dans l'UI
            uiManager.UpdateFingerCountDisplay(openFingers);

            // Envoie le nombre de doigts à NoteSequenceManager pour vérifier la saisie de l'utilisateur
            noteSequenceManager.UserInput(openFingers);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Erreur lors du parsing JSON : " + ex.Message);
        }
    }

    public int GetOpenFingersCount()
    {
        return openFingers;
    }
}
