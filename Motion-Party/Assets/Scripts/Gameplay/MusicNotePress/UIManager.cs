using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Finger Display")]
    public TMP_Text fingersCountText;
    
    [Header("Wave Information")]
    public TMP_Text waveNumberText;
    public TMP_Text notesCountText;
    public TMP_Text scoreText;
    public TMP_Text waveProgressText;

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
    
    /// <summary>
    /// Met à jour l'affichage des informations de vague
    /// </summary>
    public void UpdateWaveDisplay(int waveNumber, int notesCount, int score)
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"Vague {waveNumber}";
        }
        
        if (notesCountText != null)
        {
            notesCountText.text = $"{notesCount} notes";
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        
        Debug.Log($"[UIManager] Affichage mis à jour - Vague {waveNumber}, {notesCount} notes, Score: {score}");
    }
    
    /// <summary>
    /// Affiche un message de progression de vague
    /// </summary>
    public void ShowWaveProgress(string message)
    {
        if (waveProgressText != null)
        {
            waveProgressText.text = message;
            Debug.Log($"[UIManager] Message de progression: {message}");
        }
    }
    
    public void DisplayEndGameScreen(bool success)
    {
        // Affiche un écran de fin selon le résultat
        if (success)
        {
            Debug.Log("Victoire ! Affichage de l'écran de fin de jeu.");
            ShowWaveProgress("Félicitations ! Toutes les vagues ont été complétées !");
        }
        else
        {
            Debug.Log("Défaite. Affichage de l'écran de fin de jeu.");
            ShowWaveProgress("Vague échouée ! Fin du jeu.");
        }
        // Ajoute ici l'affichage réel de l'UI si besoin
    }
}
