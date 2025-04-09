using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSequenceManager : MonoBehaviour
{
    public GameObject[] noteObjects;         // Tableau de GameObjects représentant les notes (avec NoteLightEffect)
    public int sequenceLength = 5;           // Nombre de notes dans la séquence
    public float delayBetweenNotes = 1.0f;   // Délai entre l'affichage de chaque note

    private List<int> currentSequence = new List<int>(); // Séquence générée
    private List<int> userSequence = new List<int>();   // Séquence de l'utilisateur
    private bool isUserTurn = false;           // Indicateur si c'est au tour de l'utilisateur de jouer
    private int currentNoteIndex = 0;         // Indice de la note actuelle dans la séquence

    void Start()
    {
        GenerateSequence();
        StartCoroutine(PlaySequence());
    }

    // Génère une séquence aléatoire de notes
    public void GenerateSequence()
    {
        currentSequence.Clear();

        if (noteObjects == null || noteObjects.Length == 0)
        {
            Debug.LogError("Aucune note n'est assignée dans 'noteObjects' !");
            return;
        }

        for (int i = 0; i < sequenceLength; i++)
        {
            int randomIndex = Random.Range(1,6);
            currentSequence.Add(randomIndex);
            Debug.Log("Note ajoutée à la séquence : " + randomIndex);
        }
    }

    // Joue visuellement la séquence avec effets de lumière
    public IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(1f); // Petite pause avant de commencer

        foreach (int index in currentSequence)
        {
            GameObject note = noteObjects[index];

            // Allume la lumière sur la note
            NoteLightEffect effect = note.GetComponent<NoteLightEffect>();
            if (effect != null)
                effect.PlayNoteEffect();

            // Attendre avant la note suivante
            yield return new WaitForSeconds(delayBetweenNotes);
        }

        // Après la séquence, commence le tour de l'utilisateur
        Debug.Log("Séquence jouée, à toi de jouer !");
        isUserTurn = true;

        // Démarre le processus où l'utilisateur doit reproduire la séquence
        StartCoroutine(UserTurn());
    }

    // Reproduit la séquence par l'utilisateur
    private IEnumerator UserTurn()
    {
        userSequence.Clear();
        currentNoteIndex = 0;  // Commence à la première note

        while (currentNoteIndex < currentSequence.Count)
        {
            // Attend que l'utilisateur entre le bon nombre de doigts pour cette note
            bool noteValidated = false;

            while (!noteValidated)
            {
                // Attend que l'utilisateur entre une donnée
                if (userSequence.Count > currentNoteIndex)
                {
                    int userFingerCount = userSequence[currentNoteIndex];
                    if (userFingerCount == currentSequence[currentNoteIndex])
                    {
                        noteValidated = true;
                        Debug.Log("Note " + (currentNoteIndex + 1) + " validée !");
                    }
                }
                yield return null;  // Attendre jusqu'à la prochaine frame
            }

            // Si la note est validée, passe à la suivante
            currentNoteIndex++;
        }

        // Après que l'utilisateur ait fini de jouer toutes les notes
        if (userSequence.Count == currentSequence.Count && IsSequenceCorrect())
        {
            Debug.Log("Séquence validée !");
            GenerateSequence();  // Nouvelle séquence générée
            StartCoroutine(PlaySequence());  // Rejouer la séquence
        }
        else
        {
            Debug.Log("Mauvaise séquence, essaie encore !");
        }
    }

    // Vérifie si la séquence de l'utilisateur est correcte
    private bool IsSequenceCorrect()
    {
        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (userSequence[i] != currentSequence[i])
            {
                return false;
            }
        }
        return true;
    }

    // Cette fonction doit être appelée lorsqu'un utilisateur lève ses doigts
    public void UserInput(int fingerCount)
    {
        if (isUserTurn && currentNoteIndex < currentSequence.Count)
        {
            userSequence.Add(fingerCount);
        }
    }

    // Pour accéder à la séquence actuelle (ex: pour vérifier la réponse du joueur)
    public List<int> GetCurrentSequence()
    {
        return currentSequence;
    }
}
