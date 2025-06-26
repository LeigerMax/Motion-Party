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
    // REMOVED: Lane indicator colors (system removed)

    private CanvasGroup mainCanvasGroup;
    private bool isPulsing = false;
    private Coroutine currentTextAnimation;

    void Awake()
    {
        // Obtenir le CanvasGroup pour les animations
        mainCanvasGroup = GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null)        {
            mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // REMOVED: Lane indicators initialization (system removed)
    }

    void Start()
    {
        // Masquer les panels au démarrage
        if (successPanel != null) successPanel.SetActive(false);
        if (timeoutPanel != null) timeoutPanel.SetActive(false);
        
        // Initialiser l'alpha
        mainCanvasGroup.alpha = 0f;
    }    // REMOVED: InitializeLaneIndicators method (system removed)

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
    }    /// <summary>
    /// REMOVED: Lane highlighting functionality (system removed)
    /// </summary>
    public void HighlightLane(int laneIndex)
    {
        LogParadeLogger.Log($"HighlightLane({laneIndex}) called but lane indicators system has been removed");
    }

    /// <summary>
    /// REMOVED: Lane completion functionality (system removed)
    /// </summary>
    public void SetLaneCompleted(int laneIndex)
    {
        LogParadeLogger.Log($"SetLaneCompleted({laneIndex}) called but lane indicators system has been removed");
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
    }    // REMOVED: PulseLaneIndicator method (lane indicators system removed)

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
        // REMOVED: Lane indicators initialization (system removed)
        
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
