using UnityEngine;
using UnityEngine.UI;

public class SpinnerValidator : MonoBehaviour
{
    [Header("Validation Progress")]
    public bool IsValidating = false; // Indique si l'on est en mode validation
    public float ValidationTime = 5f; // Durée pour valider (en secondes)
    private float currentValidationTime = 0f; // Temps écoulé de la validation

    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();
    }

    void Update()
    {
        // Si nous sommes en validation, on augmente le temps écoulé
        if (IsValidating)
        {
            currentValidationTime += Time.deltaTime;

            // Calculer le pourcentage de validation
            float validationProgress = Mathf.Clamp01(currentValidationTime / ValidationTime);

            // Remplir l'image du spinner en fonction du temps de validation
            _image.fillAmount = validationProgress;

            // Si la validation est terminée, on arrête le remplissage
            if (currentValidationTime >= ValidationTime)
            {
                IsValidating = false;
                currentValidationTime = 0f; // Reset du temps pour la prochaine validation
            }
        }
    }

    // Méthode pour démarrer la validation
    public void StartValidation()
    {
        IsValidating = true;
        currentValidationTime = 0f;
    }

    // Méthode pour arrêter la validation et réinitialiser
    public void StopValidation()
    {
        IsValidating = false;
        _image.fillAmount = 0f; // Réinitialiser le remplissage
        currentValidationTime = 0f; // Réinitialiser le temps
    }
}