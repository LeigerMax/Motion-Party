using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSequenceManager : MonoBehaviour 
{
    public GameObject[] noteObjects;
    public AudioClip[] noteSounds;
    public GameObject[] noteNumberObjects;

    public float delayBetweenNotesDisplay = 1f;

    private List<int> generatedListNotes = new List<int>();

    public void LoadSequence(int noteCount)
    {
        GenerateNotes(noteCount);
        Debug.Log("Generated Notes: " + string.Join(", ", generatedListNotes));
    }

    public IEnumerator PlaySequenceCoroutine()
    {
        SetPlayerTurnVisual(false);

        foreach (int noteIndex in generatedListNotes)
        {
            int realIndex = noteIndex - 1;
            PlayNoteEffect(realIndex);
            yield return new WaitForSeconds(delayBetweenNotesDisplay);
        }

        SetPlayerTurnVisual(true);
    }

    // Fonction pour générer une séquence de notes aléatoires
    public void GenerateNotes(int noteCount)
    {
        generatedListNotes.Clear();
        for (int i = 0; i < noteCount; i++)
        {
            generatedListNotes.Add(Random.Range(1, 6));
        }
    }

    public List<int> GetGeneratedNotes()
    {
        return generatedListNotes;
    }
    

    public void SetPlayerTurnVisual(bool isPlayerTurn)
    {
        foreach (GameObject note in noteObjects)
        {
            var effect = note.GetComponent<NoteLightEffect>();
            if (effect != null)
                effect.SetAuraActive(isPlayerTurn);
        }
    }

    public void PlayNoteEffect(int realIndex)
    {
        if (realIndex >= 0 && realIndex < noteObjects.Length)
        {
            GameObject note = noteObjects[realIndex];
            note.GetComponent<NoteLightEffect>().PlayNoteEffect();
            PlayNoteSound(realIndex, note);
            ShowNoteNumber(realIndex);
        }
    }

    private void PlayNoteSound(int index, GameObject note)
    {
        AudioSource audioSource = note.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = note.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            //Debug.LogWarning($"AudioSource ajouté sur {note.name}");
        }
        else
        {
            audioSource.playOnAwake = false;
        }
    
        if (index < noteSounds.Length && noteSounds[index] != null)
        {
            audioSource.clip = noteSounds[index];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Pas de son assigné pour l'index {index}.");
        }
    }

    private void ShowNoteNumber(int index)
    {
        if (index < noteNumberObjects.Length && noteNumberObjects[index] != null)
        {
            GameObject numberObj = noteNumberObjects[index];
            FloatingText floatingText = numberObj.GetComponent<FloatingText>();
    
            if (floatingText != null)
            {
                // Réinitialiser l'animation et activer l'objet
                floatingText.ResetAndPlay();
            }
            else
            {
                Debug.LogWarning("FloatingText script is missing on the object: " + numberObj.name);
            }
        }
    }
    
}