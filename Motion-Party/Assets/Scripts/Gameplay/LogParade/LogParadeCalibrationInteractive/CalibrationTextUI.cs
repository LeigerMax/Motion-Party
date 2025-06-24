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
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI mainInstructionText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("Visual Elements")]
    [SerializeField] private Image progressBar;
    [SerializeField] private Image[] laneIndicators = new Image[4];
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject timeoutPanel;
    
    [Header("Animation Settings")]
    [SerializeField] private float textFadeDuration = 0.5f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    [Header("Colors")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color highlightTextColor = Color.yellow;
    [SerializeField] private Color successTextColor = Color.green;
    [SerializeField] private Color timeoutTextColor = Color.red;
    [SerializeField] private Color activeLaneColor = Color.yellow;
    [SerializeField] private Color completedLaneColor = Color.green;
    [SerializeField] private Color inactiveLaneColor = Color.gray;

    private CanvasGroup mainCanvasGroup;
    private bool isPulsing = false;
    private Coroutine currentTextAnimation;

    void Awake()
    {
        // Obtenir le CanvasGroup pour les animations
        mainCanvasGroup = GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        InitializeLaneIndicators();
    }

    void Start()
    {
        // Masquer les panels au démarrage
        if (successPanel != null) successPanel.SetActive(false);
        if (timeoutPanel != null) timeoutPanel.SetActive(false);
        
        // Initialiser l'alpha
        mainCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Initialise les indicateurs de voies
    /// </summary>
    private void InitializeLaneIndicators()
    {
        for (int i = 0; i < laneIndicators.Length; i++)
        {
            if (laneIndicators[i] != null)
            {
                laneIndicators[i].color = inactiveLaneColor;
            }
        }
    }

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
        
        // Afficher le panel de succès si disponible
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            StartCoroutine(HidePanelAfterDelay(successPanel, 2f));
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
        
        // Afficher le panel de timeout si disponible
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(true);
            StartCoroutine(HidePanelAfterDelay(timeoutPanel, 2f));
        }
        
        StopTextPulse();
    }

    /// <summary>
    /// Met à jour le texte de statut
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
    }

    /// <summary>
    /// Met à jour l'affichage du timer
    /// </summary>
    public void UpdateTimer(float currentTime, float maxTime)
    {
        if (timerText != null)
        {
            int remainingSeconds = Mathf.CeilToInt(maxTime - currentTime);
            timerText.text = $"Temps restant: {remainingSeconds}s";
            
            // Changer la couleur si le temps est critique
            if (remainingSeconds <= 5)
            {
                timerText.color = timeoutTextColor;
            }
            else
            {
                timerText.color = normalTextColor;
            }
        }
        
        // Mettre à jour la barre de progression si disponible
        if (progressBar != null)
        {
            progressBar.fillAmount = 1f - (currentTime / maxTime);
        }
    }

    /// <summary>
    /// Met en évidence une lane spécifique
    /// </summary>
    public void HighlightLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneIndicators.Length) return;
        
        // Réinitialiser toutes les lanes
        for (int i = 0; i < laneIndicators.Length; i++)
        {
            if (laneIndicators[i] != null)
            {
                laneIndicators[i].color = inactiveLaneColor;
            }
        }
        
        // Mettre en évidence la lane ciblée
        if (laneIndicators[laneIndex] != null)
        {
            laneIndicators[laneIndex].color = activeLaneColor;
            
            // Animer la lane active
            StartCoroutine(PulseLaneIndicator(laneIndicators[laneIndex]));
        }
    }

    /// <summary>
    /// Marque une lane comme complétée
    /// </summary>
    public void SetLaneCompleted(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneIndicators.Length) return;
        
        if (laneIndicators[laneIndex] != null)
        {
            laneIndicators[laneIndex].color = completedLaneColor;
        }
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
    /// Coroutine pour l'animation de pulsation d'un indicateur de lane
    /// </summary>
    private IEnumerator PulseLaneIndicator(Image indicator)
    {
        Color originalColor = indicator.color;
        float duration = 2f; // Durée de l'animation
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = elapsed * pulseSpeed;
            float intensity = 1f - (Mathf.Sin(time) * 0.2f);
            
            Color pulsedColor = originalColor * intensity;
            pulsedColor.a = originalColor.a;
            indicator.color = pulsedColor;
            
            yield return null;
        }
        
        // Restaurer la couleur originale
        indicator.color = originalColor;
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
            mainCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }
        
        mainCanvasGroup.alpha = 1f;
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
            mainCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            yield return null;
        }
        
        mainCanvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Masque un panel après un délai
    /// </summary>
    private IEnumerator HidePanelAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null)
        {
            panel.SetActive(false);
        }
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
        
        if (statusText != null)
        {
            statusText.text = "";
        }
        
        if (timerText != null)
        {
            timerText.text = "";
        }
        
        InitializeLaneIndicators();
        
        if (successPanel != null) successPanel.SetActive(false);
        if (timeoutPanel != null) timeoutPanel.SetActive(false);
        
        if (progressBar != null)
        {
            progressBar.fillAmount = 1f;
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

    /// <summary>
    /// Masque l'UI après un délai
    /// </summary>
    private IEnumerator DelayedHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCalibrationUI();
    }
}
