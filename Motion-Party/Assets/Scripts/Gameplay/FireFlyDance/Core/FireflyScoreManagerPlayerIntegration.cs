using UnityEngine;
using Gameplay.FireFlyDance.Scoring;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Extension du FireflyScoreManager pour intégrer le système de joueurs
    /// </summary>
    public class FireflyScoreManagerPlayerIntegration : MonoBehaviour
    {
        [Header("Score Manager Integration")]
        [SerializeField] private FireflyScoreManager scoreManager;
        [SerializeField] private bool debugMode = false;

        private void Start()
        {
            // Trouver automatiquement le ScoreManager si pas assigné
            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<FireflyScoreManager>();
            }

            if (scoreManager == null)
            {
                Debug.LogError("FireflyScoreManager non trouvé pour l'intégration joueurs");
            }
        }

        /// <summary>
        /// Récupère le score actuel depuis le ScoreManager
        /// </summary>
        public int GetCurrentScore()
        {
            if (scoreManager == null) return 0;

            // Utiliser la réflexion pour accéder au score si la propriété n'est pas publique
            try
            {
                var scoreField = typeof(FireflyScoreManager).GetField("currentScore", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (scoreField != null)
                {
                    return (int)scoreField.GetValue(scoreManager);
                }

                // Fallback: essayer une propriété publique
                var scoreProperty = typeof(FireflyScoreManager).GetProperty("CurrentScore");
                if (scoreProperty != null)
                {
                    return (int)scoreProperty.GetValue(scoreManager);
                }

                if (debugMode)
                    Debug.LogWarning("Impossible d'accéder au score depuis FireflyScoreManager");
                
                return 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de la récupération du score: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Remet le score à zéro
        /// </summary>
        public void ResetScore()
        {
            if (scoreManager == null) return;

            try
            {
                // Essayer d'appeler une méthode Reset si elle existe
                var resetMethod = typeof(FireflyScoreManager).GetMethod("ResetScore");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(scoreManager, null);
                    if (debugMode)
                        Debug.Log("Score réinitialisé via ResetScore()");
                    return;
                }

                // Fallback: remettre le champ à zéro par réflexion
                var scoreField = typeof(FireflyScoreManager).GetField("currentScore", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (scoreField != null)
                {
                    scoreField.SetValue(scoreManager, 0);
                    if (debugMode)
                        Debug.Log("Score réinitialisé via champ direct");
                    return;
                }

                if (debugMode)
                    Debug.LogWarning("Impossible de réinitialiser le score de FireflyScoreManager");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Erreur lors de la réinitialisation du score: {ex.Message}");
            }
        }

        /// <summary>
        /// Vérifie si le ScoreManager a une méthode pour obtenir le score
        /// </summary>
        [ContextMenu("Debug Score Access Methods")]
        public void DebugScoreAccessMethods()
        {
            if (scoreManager == null)
            {
                Debug.Log("ScoreManager non assigné");
                return;
            }

            var type = typeof(FireflyScoreManager);
            Debug.Log($"=== ANALYSE DE {type.Name} ===");

            // Lister les propriétés publiques
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Debug.Log($"Propriétés publiques ({properties.Length}):");
            foreach (var prop in properties)
            {
                Debug.Log($"  - {prop.PropertyType.Name} {prop.Name}");
            }

            // Lister les méthodes publiques
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            Debug.Log($"Méthodes publiques ({methods.Length}):");
            foreach (var method in methods)
            {
                Debug.Log($"  - {method.ReturnType.Name} {method.Name}()");
            }

            // Lister les champs privés (pour debug seulement)
            var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Debug.Log($"Champs privés ({fields.Length}):");
            foreach (var field in fields)
            {
                if (field.Name.ToLower().Contains("score"))
                {
                    Debug.Log($"  - {field.FieldType.Name} {field.Name}");
                }
            }

            // Test de récupération de score
            int currentScore = GetCurrentScore();
            Debug.Log($"Score actuel récupéré: {currentScore}");
        }
    }
}
