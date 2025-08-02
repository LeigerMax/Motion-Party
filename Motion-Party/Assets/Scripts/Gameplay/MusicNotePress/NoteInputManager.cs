using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Analytics;

public class NoteInputManager : MonoBehaviour
{
    public NoteSequenceManager sequenceManager;
    public SpinnerValidator spinner;

    private List<int> generatedListNotes = new List<int>();
    private List<int> userInputs = new List<int>();

    private int currentFingerCount = -1; // Dernier nombre de doigts détecté
    private Systems.PlayerData currentPlayer; // Référence au joueur actuel
    private Coroutine validationCoroutine = null; // Coroutine pour valider l'entrée utilisateur
    private int currentNoteIndex = 0;
    public bool isPlayingSequence = false;

    void Start()
    {
        generatedListNotes = sequenceManager.GetGeneratedNotes();
    }

    /// <summary>
    /// Définit le joueur actuel pour les analytics
    /// </summary>
    public void SetCurrentPlayer(Systems.PlayerData player)
    {
        currentPlayer = player;
    }

    public void UserInput(int openFingers)
    {

        if (isPlayingSequence) return; 
        
        // Enregistrer toutes les captures de main (même les 0)
        string playerID = currentPlayer?.Id ?? AnalyticsHelper.GetCurrentPlayerFromSession();
        if (!string.IsNullOrEmpty(playerID))
        {
            AnalyticsHelper.RecordMusicNoteHandClosure(playerID, openFingers);
        }

        if (openFingers == 0)
        {
            spinner.StopValidation();
            return;
        }

        // Ne démarre une validation que si le nombre de doigts a changé et que ce n'est pas 0
        if (openFingers != currentFingerCount && openFingers != 0)
        {
            currentFingerCount = openFingers;

            // Si une validation est déjà en cours, l'arrêter et la relancer avec le nouveau nombre de doigts
            if (validationCoroutine != null)
            {
                StopCoroutine(validationCoroutine);
                spinner.StopValidation();
                Debug.Log("Validation réinitialisée pour : " + openFingers);
            }

            // Démarrer la validation avec le nouveau nombre de doigts
            validationCoroutine = StartCoroutine(ValidateUserInput(openFingers));
        }
    }

    private IEnumerator ValidateUserInput(int openFingers)
    {
        if (openFingers > 0)
        {
            spinner.StartValidation();
        }

        float validationTime = spinner.ValidationTime;
        float elapsedTime = 0f;

        // Validation active pendant un certain temps
        while (elapsedTime < validationTime)
        {
            // Si le nombre de doigts change, recommencer la validation avec le nouveau nombre de doigts
            if (currentFingerCount != openFingers)
            {
                Debug.Log("Le nombre de doigts a changé. Redémarrage de la validation.");
                spinner.StopValidation();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spinner.StopValidation();

        // Si le nombre de doigts est resté constant, valider l'entrée
        userInputs.Add(openFingers);
        Debug.Log("Input utilisateur validé : " + openFingers);

        CheckUserInput(openFingers);
    }

    private void CheckUserInput(int openFingers)
    {
        if (currentNoteIndex >= generatedListNotes.Count)
        {
            Debug.LogWarning("Tentative de lire au-delà de la séquence.");
            return;
        }
    
        // Comparer la note entrée avec la séquence générée
        if (openFingers == generatedListNotes[currentNoteIndex])
        {
            Debug.Log("Note correcte !");
            sequenceManager.PlayNoteEffect(openFingers - 1);
    
            currentNoteIndex++;
    
            // Vérifier si la séquence est terminée
            if (userInputs.Count == generatedListNotes.Count)
            {
                Debug.Log("Séquence réussie");
                // Notifier le contrôleur principal que la séquence est réussie
                OnSequenceCompleted?.Invoke(true);
            }
        }
        else
        {
            Debug.Log("Note incorrecte.");
            // Notifier le contrôleur principal que la séquence est échouée
            OnSequenceCompleted?.Invoke(false);
        }
    }

    private IEnumerator WaitAndPlayNewSequence(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentFingerCount = -1;
        validationCoroutine = null;
        currentNoteIndex = 0;
        ResetInput();
    }

    public void ResetInput()
    {
        userInputs.Clear();
        currentNoteIndex = 0;
        currentFingerCount = -1;
        Debug.Log("Entrées utilisateur réinitialisées.");
    }


    public void PrepareExpectedSequence(List<int> sequence)
    {
        userInputs.Clear();
        currentNoteIndex = 0;
        generatedListNotes = new List<int>(sequence);
    }

    /// <summary>
    /// Retourne le score du joueur pour la manche en cours (nombre de notes validées)
    /// </summary>
    public int GetScore()
    {
        // Ici, on considère le score comme le nombre de notes correctement validées
        // (peut être adapté selon la logique de scoring souhaitée)
        return userInputs.Count;
    }

    
    // Event pour notifier la fin de la séquence et le résultat
    public delegate void SequenceCompletedHandler(bool success);
    public event SequenceCompletedHandler OnSequenceCompleted;
}
