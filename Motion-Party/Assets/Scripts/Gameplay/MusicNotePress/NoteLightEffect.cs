using UnityEngine;

public class NoteLightEffect : MonoBehaviour
{
    public Light noteLight;         // Lumière attachée à la note
    public float duration = 0.5f;   // Durée pendant laquelle la lumière reste allumée
    private float timer = 0f;
    private bool isLit = false;

    void Start()
    {
        if (noteLight != null)
            noteLight.enabled = false; // Éteindre la lumière au début
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
            }
        }
    }

    public void PlayNoteEffect()
    {
        if (noteLight != null)
            noteLight.enabled = true;

        isLit = true;
        timer = duration;
    }
}
