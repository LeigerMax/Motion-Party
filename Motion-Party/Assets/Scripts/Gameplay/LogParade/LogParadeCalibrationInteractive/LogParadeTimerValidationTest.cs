using UnityEngine;
using System.Collections;

/// <summary>
/// Script de test spécifique pour valider le timer et la fin de partie
/// </summary>
public class LogParadeTimerValidationTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestOnStart = false;
    [SerializeField] private bool showDetailedLogs = true;
    
    private LogParadeGameTimer gameTimer;
    private LogParadeCalibrationManager calibrationManager;
    
    void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(ValidateTimerFunctionality());
        }
    }
    
    private IEnumerator ValidateTimerFunctionality()
    {
        Log("=== VALIDATION DU TIMER ET FIN DE PARTIE ===");
        
        // 1. Trouver les composants
        gameTimer = FindObjectOfType<LogParadeGameTimer>();
        calibrationManager = FindObjectOfType<LogParadeCalibrationManager>();
        
        Log($"GameTimer trouvé: {(gameTimer != null ? "✅" : "❌")}");
        Log($"CalibrationManager trouvé: {(calibrationManager != null ? "✅" : "❌")}");
        
        if (gameTimer == null)
        {
            Log("❌ ERREUR: GameTimer non trouvé!");
            yield break;
        }
        
        // 2. Vérifier l'état initial du timer
        yield return StartCoroutine(ValidateTimerInitialState());
        
        // 3. Attendre que la calibration soit terminée
        yield return StartCoroutine(WaitForCalibrationToComplete());
        
        // 4. Vérifier que le timer démarre après la calibration
        yield return StartCoroutine(ValidateTimerStartsAfterCalibration());
        
        // 5. Monitorer le timer pendant le jeu
        yield return StartCoroutine(MonitorTimerDuringGame());
        
        Log("=== VALIDATION TIMER TERMINÉE ===");
    }
    
    private IEnumerator ValidateTimerInitialState()
    {
        Log("🔍 Validation de l'état initial du timer...");
        
        bool timerActive = gameTimer.gameObject.activeInHierarchy;
        Log($"Timer actif dans la hiérarchie: {(timerActive ? "✅" : "❌")}");
        
        bool gameActive = gameTimer.IsGameActive;
        Log($"Jeu actif dans le timer: {(gameActive ? "⚠️ OUI (peut être un problème)" : "✅ NON")}");
        
        // Vérifier les composants UI du timer
        var timerUITexts = gameTimer.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        Log($"Composants UI Text trouvés: {timerUITexts.Length}");
        
        var timerUIImages = gameTimer.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        Log($"Composants UI Image trouvés: {timerUIImages.Length}");
        
        foreach (var text in timerUITexts)
        {
            Log($"  - Text '{text.name}': actif={text.gameObject.activeInHierarchy}, texte='{text.text}'");
        }
        
        yield return null;
    }
    
    private IEnumerator WaitForCalibrationToComplete()
    {
        Log("⏳ Attente de la fin de calibration...");
        
        float timeout = 120f; // 2 minutes max
        float elapsed = 0f;
        
        while (LogParadeCalibrationManager.IsCalibrationInProgress && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            
            if (elapsed % 10f < Time.deltaTime) // Log toutes les 10 secondes
            {
                Log($"Calibration en cours... ({elapsed:F1}s)");
            }
            
            yield return null;
        }
        
        if (elapsed >= timeout)
        {
            Log("⏰ TIMEOUT: Calibration trop longue");
        }
        else
        {
            Log("✅ Calibration terminée!");
        }
    }
    
    private IEnumerator ValidateTimerStartsAfterCalibration()
    {
        Log("🚀 Validation du démarrage du timer après calibration...");
        
        // Attendre un peu pour que tous les systèmes se mettent en place
        yield return new WaitForSeconds(3f);
        
        bool gameActive = gameTimer.IsGameActive;
        Log($"Timer actif après calibration: {(gameActive ? "✅ OUI" : "❌ NON")}");
        
        // Vérifier l'UI du timer
        var timerUITexts = gameTimer.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        foreach (var text in timerUITexts)
        {
            if (text.name.ToLower().Contains("timer") || text.name.ToLower().Contains("time"))
            {
                Log($"UI Timer '{text.name}': actif={text.gameObject.activeInHierarchy}, texte='{text.text}'");
            }
        }
        
        if (!gameActive)
        {
            Log("⚠️ PROBLÈME: Le timer ne semble pas actif après la calibration");
            Log("Tentative de démarrage manuel...");
            
            try
            {
                gameTimer.LaunchLevel();
                Log("✅ Démarrage manuel du timer réussi");
            }
            catch (System.Exception ex)
            {
                Log($"❌ Erreur lors du démarrage manuel: {ex.Message}");
            }
        }
    }
    
    private IEnumerator MonitorTimerDuringGame()
    {
        Log("📊 Monitoring du timer pendant le jeu...");
        
        float monitoringTime = 30f; // Monitorer pendant 30 secondes
        float elapsed = 0f;
        float lastTimeValue = -1f;
        
        while (elapsed < monitoringTime && gameTimer.IsGameActive)
        {
            // Récupérer la valeur actuelle du timer
            var currentTimeField = gameTimer.GetType().GetField("currentTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (currentTimeField != null)
            {
                float currentTime = (float)currentTimeField.GetValue(gameTimer);
                
                if (lastTimeValue != currentTime && elapsed % 5f < Time.deltaTime) // Log toutes les 5 secondes
                {
                    Log($"Temps restant: {currentTime:F1}s");
                    lastTimeValue = currentTime;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!gameTimer.IsGameActive)
        {
            Log("🏁 Timer terminé - Fin de partie détectée!");
        }
        else
        {
            Log("⏰ Monitoring terminé - Jeu toujours en cours");
        }
    }
    
    /// <summary>
    /// Méthode publique pour lancer les tests manuellement
    /// </summary>
    [ContextMenu("Validate Timer")]
    public void ValidateTimerManually()
    {
        StartCoroutine(ValidateTimerFunctionality());
    }
    
    /// <summary>
    /// Force le démarrage du timer (pour debug)
    /// </summary>
    [ContextMenu("Force Start Timer")]
    public void ForceStartTimer()
    {
        var gameTimer = FindObjectOfType<LogParadeGameTimer>();
        if (gameTimer != null)
        {
            try
            {
                gameTimer.LaunchLevel();
                Log("🚀 Timer forcé à démarrer");
            }
            catch (System.Exception ex)
            {
                Log($"❌ Erreur lors du démarrage forcé: {ex.Message}");
            }
        }
        else
        {
            Log("❌ GameTimer non trouvé");
        }
    }
    
    private void Log(string message)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[TimerValidationTest] {message}");
        }
    }
}
