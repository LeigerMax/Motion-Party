using UnityEngine;
using System;
using Newtonsoft.Json.Linq; // Ajoute Newtonsoft.Json pour parser le JSON

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;
    private string handGesture = "Main Ouverte"; // Valeur par défaut

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
            handGesture = jsonData["gesture"].ToString(); // Met à jour le statut de la main

            // Affichage du statut de la main
            //Debug.Log("Statut de la main : " + handGesture);

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
        return handGesture == "hand_close"; // Retourne vrai si la main est fermée
    }

    public Vector3 GetHandPosition()
    {
        return handPoints[0].transform.position; // Utilise le poignet comme référence
    }

}
