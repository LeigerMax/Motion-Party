using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSequenceManager : MonoBehaviour
{
    public GameObject[] noteObjects;
    public AudioClip[] noteSounds;
    public GameObject[] noteNumberObjects; 

    public int sessionLength = 2;
    public float delayBetweenNotesDisplay = 1f; 

    private List<int> generatedListNotes = new List<int>();

    void Start()
    {
        foreach (GameObject numberObj in noteNumberObjects)
        {
            if (numberObj != null)
            {
                numberObj.SetActive(false);
            }
        }

        PlayNote(); 
    }

    public void PlayNote()
    {
        GenerateNotes();
        PrintGenerateNotes();
        StartCoroutine(PlayNoteSequence());
    }

    // Fonction pour générer une séquence de notes aléatoires
    public void GenerateNotes()
    {
        generatedListNotes.Clear(); 
        for (int i = 0; i < sessionLength; i++)
        {
            generatedListNotes.Add(Random.Range(1, 6));
        }
    }

    // Coroutine pour jouer la séquence
    public IEnumerator PlayNoteSequence()
    {
        SetPlayerTurnVisual(false);

        foreach (int noteIndex in generatedListNotes)
        {
            int realIndex = noteIndex - 1;
            PlayNoteEffect(realIndex);
            yield return new WaitForSeconds(delayBetweenNotesDisplay);
        }

        Debug.Log("Séquence jouée !");
        SetPlayerTurnVisual(true);
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

    private void PlayNoteSound(int realIndex, GameObject note)
    {
        AudioSource audioSource = note.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = note.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (realIndex < noteSounds.Length)
        {
            audioSource.clip = noteSounds[realIndex];
            audioSource.Play();
        }

    }

    private void ShowNoteNumber(int realIndex)
    {
        if (realIndex < noteNumberObjects.Length && noteNumberObjects[realIndex] != null)
        {
            GameObject numberObj = noteNumberObjects[realIndex];
            FloatingText floatingText = numberObj.GetComponent<FloatingText>();
    
            if (floatingText != null)
            {
                // Réinitialiser l'animation et activer l'objet
                numberObj.SetActive(true);
                floatingText.lifeTime = 1.5f; // Réinitialiser la durée de vie
                floatingText.transform.position = floatingText.initialPosition; // Réinitialiser la position
            }
            else
            {
                Debug.LogWarning("FloatingText script is missing on the object: " + numberObj.name);
            }
        }
    }

    private IEnumerator HideNumberAfterDelay(GameObject numberObject)
    {
        yield return new WaitForSeconds(delayBetweenNotesDisplay);
        numberObject.SetActive(false);
    }


    
   /* public IEnumerator PlayNoteSequence()
    {
        SetPlayerTurnVisual(false);
        foreach (int noteIndex in generatedListNotes)
        {
            int realIndex = noteIndex - 1; 

            if (realIndex >= 0 && realIndex < noteObjects.Length)
            {
                GameObject note = noteObjects[realIndex];

                NoteLightEffect effect = note.GetComponent<NoteLightEffect>();
                if (effect != null)
                {
                    effect.PlayNoteEffect(); 
                }

                AudioSource audioSource = note.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = note.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }

                if (realIndex < noteSounds.Length)
                {
                    audioSource.clip = noteSounds[realIndex];
                    audioSource.Play();
                }

                if (realIndex < noteNumberObjects.Length && noteNumberObjects[realIndex] != null)
                {
                    GameObject numberObj = noteNumberObjects[realIndex];
                    numberObj.SetActive(true);
                    StartCoroutine(HideNumberAfterDelay(numberObj, delayBetweenNotesDisplay));
                }


            }
            
            yield return new WaitForSeconds(delayBetweenNotesDisplay);
        }

        Debug.Log("Séquence jouée !");
        SetPlayerTurnVisual(true);

    }*/


    public void PrintGenerateNotes()
    {
        Debug.Log("Generated Notes: " + string.Join(", ", generatedListNotes));
    }


    public List<int> GetGeneratedNotes()
    {
        return generatedListNotes;
    }

   public void SetPlayerTurnVisual(bool isPlayerTurn)
    {
        foreach (GameObject note in noteObjects)
        {
            NoteLightEffect effect = note.GetComponent<NoteLightEffect>();
            if (effect != null)
            {
                effect.SetAuraActive(isPlayerTurn); // Centralisé dans la classe NoteLightEffect
            }
        }
    }

   


}