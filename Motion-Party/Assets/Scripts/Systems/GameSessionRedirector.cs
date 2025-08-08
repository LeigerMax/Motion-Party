using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utilitaire pour relancer automatiquement la session de mini-jeux si on revient sur la scène principale sans GameSessionManager.
/// Place ce script sur un GameObject vide dans la scène principale (menu).
/// </summary>
public class GameSessionRedirector : MonoBehaviour
{
    // Flag statique pour demander une relance automatique
    public static bool ShouldResumeSession = false;

    // Optionnel : index du mini-jeu à relancer (si tu veux reprendre à un point précis)
    public static int ResumeMiniGameIndex = -1;
    // Optionnel : nom de la scène du mini-jeu à relancer (fallback si index non trouvé)
    public static string ResumeMiniGameSceneName = null;
    // Délai à appliquer avant la transition
    public static float TransitionDelay = 1f;
    
    // Variable pour éviter les appels multiples
    private static bool isProcessingResume = false;

    void Start()
    {
        if (ShouldResumeSession && !isProcessingResume)
        {
            ShouldResumeSession = false;
            isProcessingResume = true;
            StartCoroutine(HandleResumeSession());
        }
    }

    private System.Collections.IEnumerator HandleResumeSession()
    {
        Debug.Log($"[GameSessionRedirector][Frame:{Time.frameCount}] Début de HandleResumeSession");
        
        // Attendre un court délai pour que la scène soit complètement chargée
        yield return new WaitForSeconds(0.5f);
        
        // Attendre le délai de transition si spécifié
        if (TransitionDelay > 0)
        {
            Debug.Log($"[GameSessionRedirector] Attente de {TransitionDelay}s avant reprise de session");
            yield return new WaitForSeconds(TransitionDelay);
        }
        
        Debug.Log($"[GameSessionRedirector] Recherche du GameSessionManager...");
        
        // Relancer la session
        var gsm = GameSessionManager.Instance;
        if (gsm != null)
        {
            Debug.Log($"[GameSessionRedirector] GameSessionManager trouvé");
            
            int nextIndex = -1;
            if (ResumeMiniGameIndex >= 0)
            {
                Debug.Log($"[GameSessionRedirector] ResumeMiniGameIndex reçu: {ResumeMiniGameIndex}");
                nextIndex = ResumeMiniGameIndex + 1;
            }
            else if (!string.IsNullOrEmpty(ResumeMiniGameSceneName))
            {
                // On tente de retrouver l'index à partir du nom de la scène
                var miniGames = gsm.GetMiniGameSceneNames();
                int idx = miniGames.IndexOf(ResumeMiniGameSceneName);
                Debug.Log($"[GameSessionRedirector] ResumeMiniGameSceneName reçu: {ResumeMiniGameSceneName}, index trouvé: {idx}");
                if (idx >= 0)
                    nextIndex = idx + 1;
            }
            
            if (nextIndex >= 0)
            {
                Debug.Log($"[GameSessionRedirector] Reprise de session au mini-jeu index: {nextIndex}");
                gsm.ResumeSessionAt(nextIndex);
            }
            else
            {
                Debug.Log("[GameSessionRedirector] Aucun index valide, relance de session depuis le début.");
                gsm.StartGameSession();
            }
            
            // Reset des variables statiques
            ResumeMiniGameIndex = -1;
            ResumeMiniGameSceneName = null;
            TransitionDelay = 1f;
            isProcessingResume = false;
        }
        else
        {
            Debug.LogError("[GameSessionRedirector] GameSessionManager introuvable lors de la reprise de session.");
            isProcessingResume = false;
        }
    }
}
