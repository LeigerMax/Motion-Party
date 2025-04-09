using UnityEngine;

public class HandDataProcessor : MonoBehaviour
{
    public int OpenFingers { get; private set; }

    public void ProcessHandData(string data)
    {
        try
        {
            HandData handData = JsonUtility.FromJson<HandData>(data);
            if (handData != null)
            {
                OpenFingers = Mathf.Clamp(handData.open_fingers, 0, 5); // Assurer que la valeur est dans une plage valide
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors du traitement des données de la main: " + e.Message);
        }
    }

    [System.Serializable]
    public class HandData
    {
        public string gesture;
        public int open_fingers;
    }
}
