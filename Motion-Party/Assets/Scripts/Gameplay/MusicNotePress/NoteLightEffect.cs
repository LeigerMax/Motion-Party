using UnityEngine;
using TMPro;

public class NoteLightEffect : MonoBehaviour
{
    public Light noteLight;         // Lumière attachée à la note
    public float duration = 1f;     // Durée pendant laquelle la lumière reste allumée
    private float timer = 0f;
    private bool isLit = false;
    public Light auraHighlight;   
    public GameObject noteNumberObject; 
    public float numberDisplayTime = 1.5f;
    private float numberTimer = 0f;

    void Start()
    {
        if (noteLight != null)
            noteLight.enabled = false;

        if (auraHighlight != null)
        {
            auraHighlight.enabled = false;
        }
    }

    void Update()
    {
        if (isLit)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (noteLight != null)
                    noteLight.enabled = false;

                isLit = false;

                if (auraHighlight != null)
                {
                    auraHighlight.enabled = false;
                }
            }
        }

        if (noteNumberObject != null && noteNumberObject.activeSelf)
        {
            numberTimer -= Time.deltaTime;
            if (numberTimer <= 0f)
            {
                noteNumberObject.SetActive(false);
            }
        }
    }

    public void PlayNoteEffect()
    {
        if (noteLight != null)
            noteLight.enabled = true;

        isLit = true;
        timer = duration;

        ShowNoteNumber();
    }

    public void SetAuraActive(bool active)
    {

        auraHighlight.enabled = active;
    }

    private void ShowNoteNumber()
    {
        if (noteNumberObject != null)
        {
            noteNumberObject.SetActive(true);
            numberTimer = numberDisplayTime;
        }
    }



}