using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteInputManager : MonoBehaviour
{
    public NoteSequenceManager sequenceManager;
    public SpinnerValidator spinner;


    private List<int> generatedListNotes = new List<int>();
    private List<int> userInputs = new List<int>();

    private int currentFingerCount = -1; // Dernier nombre de doigts détecté
    private Coroutine validationCoroutine; // Coroutine pour valider l'entrée utilisateur
    private int currentNoteIndex = 0;

    void Start()
    {
        generatedListNotes = sequenceManager.GetGeneratedNotes();
    }

    void Update()
    {
        // Logique de mise à jour si nécessaire
    }

    public void UserInput(int openFingers) 
    {
        if (openFingers == 0)
        {
            spinner.StopValidation(); 
        }

        // Ne démarre une validation que si le nombre de doigts a changé et que ce n'est pas 0
        if (openFingers != currentFingerCount && openFingers != 0 )
        {
            currentFingerCount = openFingers;

            // Si une validation est déjà en cours, l'arrêter et la relancer avec le nouveau nombre de doigts
            if (validationCoroutine != null)
            {
                StopCoroutine(validationCoroutine);
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
        

        float validationTime = 5f;
        float elapsedTime = 0f;

        // Validation active pendant un certain temps
        while (elapsedTime < validationTime)
        {
            // Si le nombre de doigts change, recommencer la validation avec le nouveau nombre de doigts
            if (currentFingerCount != openFingers)
            {
                Debug.Log("Le nombre de doigts a changé. Redémarrage de la validation.");
                spinner.StopValidation(); // Arrêter le spinner
                yield break; // Sortir de la coroutine, mais elle sera relancée
            }

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        // Si le nombre de doigts est resté constant, valider l'entrée
        userInputs.Add(openFingers);
        Debug.Log("Input utilisateur validé : " + openFingers);

        CheckUserInput(openFingers);
        spinner.StopValidation(); // Arrêter le spinner après validation
    }

    private void CheckUserInput(int openFingers)
    {
        // Comparer la note entrée avec la séquence générée
        if (openFingers == generatedListNotes[currentNoteIndex])
        {
            Debug.Log("Note correcte !");
            currentNoteIndex++;

            sequenceManager.PlayNoteEffect(openFingers - 1); 

            // Vérifier si la séquence est terminée
            if (userInputs.Count == generatedListNotes.Count)
            {
                Debug.Log("Séquence complète ! Passer à la suivante...");
                currentNoteIndex = 0; // Réinitialiser l'index pour la prochaine séquence
                sequenceManager.PlayNote(); // Lancer une nouvelle séquence
            }
        }
        else
        {
            Debug.Log("Note incorrecte. Nouvelle séquence !");
            // Si la note est incorrecte, redémarrer la séquence
            sequenceManager.PlayNote();
        }
    }

    void ResetInput()
    {
        userInputs.Clear();
        Debug.Log("Entrées utilisateur réinitialisées.");
    }
}
