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

    void Start()
    {
        if (ShouldResumeSession)
        {
            ShouldResumeSession = false;
            // Relancer la session
            var gsm = GameSessionManager.Instance;
            if (gsm != null)
            {
                int nextIndex = -1;
                if (ResumeMiniGameIndex >= 0)
                {
                    Debug.Log($"[GameSessionRedirector] ResumeMiniGameIndex reçu: {ResumeMiniGameIndex}");
                    nextIndex = ResumeMiniGameIndex + 1;
                }
                else if (!string.IsNullOrEmpty(ResumeMiniGameSceneName))
                {
                    // On tente de retrouver l'index à partir du nom de la scène
                    var miniGames = gsm.GetMiniGameSceneNames(); // Ajoute une méthode utilitaire dans GameSessionManager si besoin
                    int idx = miniGames.IndexOf(ResumeMiniGameSceneName);
                    Debug.Log($"[GameSessionRedirector] ResumeMiniGameSceneName reçu: {ResumeMiniGameSceneName}, index trouvé: {idx}");
                    if (idx >= 0)
                        nextIndex = idx + 1;
                }
                if (nextIndex >= 0)
                {
                    Debug.Log($"[GameSessionRedirector] On tente de reprendre au mini-jeu index: {nextIndex}");
                    gsm.ResumeSessionAt(nextIndex);
                }
                else
                {
                    Debug.Log("[GameSessionRedirector] Aucun index valide, on relance la session depuis le début.");
                    gsm.StartGameSession();
                }
                ResumeMiniGameIndex = -1;
                ResumeMiniGameSceneName = null;
            }
            else
            {
                Debug.LogError("[GameSessionRedirector] GameSessionManager introuvable lors de la reprise de session.");
            }
        }
    }
}
