using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text fingersCountText;

    public void UpdateFingerCountDisplay(int openFingers)
    {
        if (fingersCountText != null)
        {
            // Vérifier si openFingers est dans une plage valide
            if (openFingers < 0 || openFingers > 5)
            {
                Debug.LogError("Valeur invalide pour openFingers : " + openFingers);
                return;
            }

            fingersCountText.text = "" + openFingers;
        }
        else
        {
            Debug.LogError("fingersCountText  n'est pas assigné dans UIManager !");
        }
    }
    
    public void DisplayEndGameScreen(bool success)
    {
        // Affiche un écran de fin selon le résultat
        if (success)
            Debug.Log("Victoire ! Affichage de l'écran de fin de jeu.");
        else
            Debug.Log("Défaite. Affichage de l'écran de fin de jeu.");
        // Ajoute ici l'affichage réel de l'UI si besoin
    }
}
