using UnityEngine;

/// <summary>
/// Script de debug pour tester le système de sessions de mini-jeux
/// </summary>
public class GameSessionDebugger : MonoBehaviour
{
    [Header("Debug Tools")]
    [SerializeField] private bool autoValidateOnStart = true;
    [SerializeField] private bool showDetailedLogs = true;

    void Start()
    {
        if (autoValidateOnStart)
        {
            ValidateGameSessionSystem();
        }
    }

    [ContextMenu("Validate Game Session System")]
    public void ValidateGameSessionSystem()
    {
        Debug.Log("=== GAME SESSION SYSTEM VALIDATION ===");

        // Vérifier GameSessionManager
        var gsm = GameSessionManager.Instance;
        if (gsm != null)
        {
            Debug.Log("✅ GameSessionManager trouvé");
            if (showDetailedLogs)
            {
                gsm.DebugCurrentState();
            }
        }
        else
        {
            Debug.LogError("❌ GameSessionManager introuvable");
        }

        // Vérifier LoadingScreenManager
        var lsm = LoadingScreenManager.Instance;
        if (lsm != null)
        {
            Debug.Log("✅ LoadingScreenManager trouvé");
        }
        else
        {
            Debug.LogWarning("⚠️ LoadingScreenManager introuvable");
        }

        // Vérifier GameSessionRedirector
        var gsr = FindFirstObjectByType<GameSessionRedirector>();
        if (gsr != null)
        {
            Debug.Log("✅ GameSessionRedirector trouvé");
        }
        else
        {
            Debug.LogWarning("⚠️ GameSessionRedirector introuvable");
        }

        // Vérifier MiniGameBase dans la scène actuelle
        var miniGameBases = FindObjectsByType<MiniGameBase>(FindObjectsSortMode.None);
        Debug.Log($"📋 {miniGameBases.Length} MiniGameBase trouvé(s) dans la scène");
        
        if (showDetailedLogs)
        {
            foreach (var mgb in miniGameBases)
            {
                Debug.Log($"  - {mgb.GetType().Name} sur {mgb.gameObject.name}");
            }
        }

        Debug.Log("=== VALIDATION TERMINÉE ===");
    }

    [ContextMenu("Test Mini Game Transition")]
    public void TestMiniGameTransition()
    {
        var gsm = GameSessionManager.Instance;
        if (gsm != null)
        {
            Debug.Log("🧪 Test de transition de mini-jeu...");
            gsm.TriggerNextMiniGameTransition(2f);
        }
        else
        {
            Debug.LogError("Impossible de tester - GameSessionManager introuvable");
        }
    }

    [ContextMenu("Force Debug Mode")]
    public void ForceDebugMode()
    {
        var gsm = GameSessionManager.Instance;
        if (gsm != null)
        {
            // Utiliser la réflexion pour activer forceDebugMode
            var field = gsm.GetType().GetField("forceDebugMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(gsm, true);
                Debug.Log("🔧 Mode debug forcé activé sur GameSessionManager");
            }
        }
    }
}
