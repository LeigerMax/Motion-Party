using UnityEngine;
using System;
using Newtonsoft.Json.Linq; // Ajoute Newtonsoft.Json pour parser le JSON
using Core;

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;
    private string handGesture = "Main Ouverte"; 
    private int openFingers = 0;

    private int offset = 70;

    void Update()
    {
        string data = udpReceive.data;
        if (string.IsNullOrEmpty(data)) return; // Évite les erreurs si aucune donnée n'est reçue

        try
        {
            // Parsing du JSON
            JObject jsonData = JObject.Parse(data);
            JArray positions = (JArray)jsonData["hand_positions"];
            handGesture = jsonData["gesture"].ToString(); 
            openFingers = (int)jsonData["open_fingers"];

            // Affichage du statut de la main
            //Debug.Log("Statut de la main : " + handGesture);
            // Affichage du nombre de doigts ouverts
            // Debug.Log("Nombre de doigts ouverts : " + openFingers);

            // Mise à jour des positions des points de la main
            for (int i = 0; i < 21; i++)
            {
                float x = 7 - (float)positions[i][0] / this.offset;
                float y = (float)positions[i][1] / this.offset;
                float z = (float)positions[i][2] / this.offset;

                handPoints[i].transform.localPosition = new Vector3(x, y, z);
            }
        }
        catch (Exception e)
        {
            //Debug.LogWarning("Erreur de parsing des données UDP : " + e.Message);
        }
    }

    public bool IsHandClosed()
    {
        //Debug.Log("IsHandClosed");
        return handGesture == "hand_close";
    }

    public Vector3 GetHandPosition()
    {
        return handPoints[0].transform.position; // Utilise le poignet comme référence
    }

    public int GetOpenFingersCount()
    {
        return openFingers; 
    }

    public bool IsNoteSelected(GameObject note)
    {
        // Vérifie si l'utilisateur sélectionne la note en la pointant avec ses doigts
        for (int i = 0; i < 21; i++)
        {
            if (handPoints[i].transform.position == note.transform.position) // Comparer la position des doigts avec la position de la note
            {
                return true;
            }
        }
        return false;
    }


}
