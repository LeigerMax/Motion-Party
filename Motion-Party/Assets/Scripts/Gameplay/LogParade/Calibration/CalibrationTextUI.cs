using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gestionnaire d'interface utilisateur spécifique à la calibration interactive
/// Peut être utilisé séparément ou intégré dans LogParadeCalibrationInteractive
/// </summary>
public class CalibrationTextUI : MonoBehaviour
{
#region Fields
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI mainInstructionText;

    [Header("Animation Settings")]
    [SerializeField] private float textFadeDuration = 0.5f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color highlightTextColor = Color.yellow;
    [SerializeField] private Color successTextColor = Color.green;
    [SerializeField] private Color timeoutTextColor = Color.red;

    private CanvasGroup calibrationCanvasGroup;
    private bool isPulsing = false;
    private Coroutine currentTextAnimation;
#endregion

#region Unity Lifecycle
    void Awake()
    {
        // Obtenir le CanvasGroup pour les animations
        calibrationCanvasGroup = GetComponent<CanvasGroup>();
        if (calibrationCanvasGroup == null)        {
            calibrationCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        calibrationCanvasGroup.alpha = 0f;
    }  
#endregion

#region Public API
    /// <summary>
    /// Affiche le texte d'instruction principal
    /// </summary>
    public void ShowInstruction(string instruction, bool highlight = false)
    {
        if (mainInstructionText != null)
        {
            mainInstructionText.text = instruction;
            mainInstructionText.color = highlight ? highlightTextColor : normalTextColor;
        }
        
        // Animer le texte si c'est important
        if (highlight)
        {
            StartTextPulse();
        }
        else
        {
            StopTextPulse();
        }
    }

    /// <summary>
    /// Affiche un message de succès
    /// </summary>
    public void ShowSuccessMessage(string message)
    {
        if (mainInstructionText != null)
        {
            mainInstructionText.text = message;
            mainInstructionText.color = successTextColor;
        }
        
        StopTextPulse();
    }

    /// <summary>
    /// Affiche un message de timeout
    /// </summary>
    public void ShowTimeoutMessage(string message)
    {
        if (mainInstructionText != null)
        {
            mainInstructionText.text = message;
            mainInstructionText.color = timeoutTextColor;
        }
        
        StopTextPulse();
    }


    /// <summary>
    /// Affiche l'UI de calibration avec animation de fondu
    /// </summary>
    public void ShowCalibrationUI()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Masque l'UI de calibration avec animation de fondu
    /// </summary>
    public void HideCalibrationUI()
    {
        StartCoroutine(FadeOutAndHide());
    }

    /// <summary>
    /// Réinitialise l'UI à son état initial
    /// </summary>
    public void ResetUI()
    {
        StopTextPulse();
        
        if (mainInstructionText != null)
        {
            mainInstructionText.text = "";
            mainInstructionText.color = normalTextColor;
        }
        
    }

    // Interface publique pour les événements externes
    
    /// <summary>
    /// Appelé quand la calibration démarre
    /// </summary>
    public void OnCalibrationStarted()
    {
        ResetUI();
        ShowCalibrationUI();
    }

    /// <summary>
    /// Appelé quand la calibration se termine avec succès
    /// </summary>
    public void OnCalibrationCompleted()
    {
        ShowSuccessMessage("Calibration terminée !");
        StartCoroutine(DelayedHide(3f));
    }

    /// <summary>
    /// Appelé quand la calibration échoue
    /// </summary>
    public void OnCalibrationFailed()
    {
        ShowTimeoutMessage("Calibration échouée...");
    }
#endregion

#region Animation
    /// <summary>
    /// Démarre l'animation de pulsation du texte
    /// </summary>
    private void StartTextPulse()
    {
        if (!isPulsing && mainInstructionText != null)
        {
            isPulsing = true;
            if (currentTextAnimation != null)
            {
                StopCoroutine(currentTextAnimation);
            }
            currentTextAnimation = StartCoroutine(PulseText());
        }
    }

    /// <summary>
    /// Arrête l'animation de pulsation du texte
    /// </summary>
    private void StopTextPulse()
    {
        isPulsing = false;
        if (currentTextAnimation != null)
        {
            StopCoroutine(currentTextAnimation);
            currentTextAnimation = null;
        }
        
        // Restaurer l'alpha du texte
        if (mainInstructionText != null)
        {
            mainInstructionText.alpha = 1f;
        }
    }

    /// <summary>
    /// Coroutine pour l'animation de pulsation du texte
    /// </summary>
    private IEnumerator PulseText()
    {
        while (isPulsing && mainInstructionText != null)
        {
            float time = Time.time * pulseSpeed;
            float alpha = 1f - (Mathf.Sin(time) * pulseIntensity);
            mainInstructionText.alpha = alpha;
            yield return null;
        }
    }  

    /// <summary>
    /// Coroutine pour le fondu d'entrée
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / textFadeDuration;
            calibrationCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }
        
        calibrationCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Coroutine pour le fondu de sortie et masquage
    /// </summary>
    private IEnumerator FadeOutAndHide()
    {
        float elapsed = 0f;
        
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / textFadeDuration;
            calibrationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            yield return null;
        }
        
        calibrationCanvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Masque un panel après un délai
    /// </summary>
    private IEnumerator HidePanelAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    /// <summary>
    /// Masque l'UI après un délai
    /// </summary>
    private IEnumerator DelayedHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCalibrationUI();
    }
#endregion
}
