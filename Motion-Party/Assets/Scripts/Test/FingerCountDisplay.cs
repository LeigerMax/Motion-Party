using UnityEngine;
using TMPro; 

public class FingerCountDisplay : MonoBehaviour
{
    public HandTracking handTracking;  
    public TMP_Text fingersText; 

    void Update()
    {
        fingersText.text = "Doigts ouverts: " + handTracking.GetOpenFingersCount();
    }
}