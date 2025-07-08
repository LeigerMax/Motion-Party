using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CameraTransitions
{
    /// <summary>
    /// Script utilitaire pour configurer automatiquement DOTween si disponible.
    /// Ajoute le symbole de compilation DOTWEEN_ENABLED automatiquement.
    /// </summary>
    public class DOTweenSetup : MonoBehaviour
    {
        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void CheckDOTween()
        {
            // Vérifier si DOTween est présent dans le projet
            var doTweenType = System.Type.GetType("DG.Tweening.DOTween, DOTween");
            
            if (doTweenType != null)
            {
                // DOTween est présent, ajouter le symbole de compilation
                string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                
                if (!currentDefines.Contains("DOTWEEN_ENABLED"))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                        EditorUserBuildSettings.selectedBuildTargetGroup,
                        currentDefines + ";DOTWEEN_ENABLED"
                    );
                    
                    Debug.Log("DOTweenSetup: Symbole DOTWEEN_ENABLED ajouté automatiquement");
                }
            }
            else
            {
                // DOTween n'est pas présent, retirer le symbole si il existe
                string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                
                if (currentDefines.Contains("DOTWEEN_ENABLED"))
                {
                    currentDefines = currentDefines.Replace("DOTWEEN_ENABLED", "").Replace(";;", ";");
                    
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                        EditorUserBuildSettings.selectedBuildTargetGroup,
                        currentDefines
                    );
                    
                    Debug.Log("DOTweenSetup: Symbole DOTWEEN_ENABLED retiré (DOTween non trouvé)");
                }
            }
        }
        #endif
        
        /// <summary>
        /// Vérifie si DOTween est disponible dans le projet
        /// </summary>
        public static bool IsDOTweenAvailable()
        {
            #if DOTWEEN_ENABLED
            return true;
            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// Affiche des informations sur la disponibilité de DOTween
        /// </summary>
        [ContextMenu("Check DOTween Status")]
        public void CheckDOTweenStatus()
        {
            bool isAvailable = IsDOTweenAvailable();
            string message = isAvailable 
                ? "DOTween est disponible et configuré correctement"
                : "DOTween n'est pas disponible. Utilisation du système Lerp de base.";
                
            Debug.Log($"DOTweenSetup: {message}");
            
            #if UNITY_EDITOR
            EditorUtility.DisplayDialog("Statut DOTween", message, "OK");
            #endif
        }
    }
}
