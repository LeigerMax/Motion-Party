using UnityEngine;
using TMPro; 

public class UIManager : MonoBehaviour
{
    public TMP_Text fingersText;

    public void UpdateFingerCountDisplay(int openFingers)
    {
        if (fingersText != null)
        {
            // Vérifier si openFingers est dans une plage valide
            if (openFingers < 0 || openFingers > 5)
            {
                Debug.LogError("Valeur invalide pour openFingers : " + openFingers);
                return;
            }

            fingersText.text = ""+openFingers;
        }
        else
        {
            Debug.LogError("fingersText n'est pas assigné dans UIManager !");
        }
    }
}
