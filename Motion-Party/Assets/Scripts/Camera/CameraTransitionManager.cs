using System.Collections;
using UnityEngine;

namespace CameraTransitions
{
    /// <summary>
    /// Gestionnaire de transitions fluides entre caméras.
    /// Permet un mouvement fluide au lieu d'un switch instantané entre différents points de vue.
    /// </summary>
    public class CameraTransitionManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableCameraTransition = true;
        [SerializeField] private float defaultTransitionDuration = 2.0f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Cameras")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private UnityEngine.Camera mainMenuCamera;
        [SerializeField] private UnityEngine.Camera gameSelectionCamera;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // État interne
        private bool isTransitioning = false;
        private Coroutine currentTransition;
        
        // Instance singleton pour accès facile
        public static CameraTransitionManager Instance { get; private set; }
        
        private void Awake()
        {
            // Pattern Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeCamera();
        }
        
        private void InitializeCamera()
        {
            // Utiliser Camera.main si aucune caméra n'est assignée
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("CameraTransitionManager: Aucune caméra principale trouvée !");
                    return;
                }
            }
            
            // S'assurer que les caméras de destination sont désactivées pour éviter les conflits
            if (mainMenuCamera != null && mainMenuCamera != mainCamera)
            {
                mainMenuCamera.enabled = false;
                
                // Désactiver l'Audio Listener sur la caméra de destination pour éviter les conflits
                AudioListener listener = mainMenuCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                    if (showDebugLogs)
                        Debug.Log($"CameraTransitionManager: Audio Listener désactivé sur {mainMenuCamera.name}");
                }
                
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManager: Caméra {mainMenuCamera.name} désactivée (utilisée comme cible)");
            }
            
            if (gameSelectionCamera != null && gameSelectionCamera != mainCamera)
            {
                gameSelectionCamera.enabled = false;
                
                // Désactiver l'Audio Listener sur la caméra de destination pour éviter les conflits
                AudioListener listener = gameSelectionCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                    if (showDebugLogs)
                        Debug.Log($"CameraTransitionManager: Audio Listener désactivé sur {gameSelectionCamera.name}");
                }
                
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManager: Caméra {gameSelectionCamera.name} désactivée (utilisée comme cible)");
            }
            
            // S'assurer que la caméra principale a un Audio Listener actif
            AudioListener mainListener = mainCamera.GetComponent<AudioListener>();
            if (mainListener == null)
            {
                mainListener = mainCamera.gameObject.AddComponent<AudioListener>();
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManager: Audio Listener ajouté sur {mainCamera.name}");
            }
            else
            {
                mainListener.enabled = true;
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManager: Audio Listener activé sur {mainCamera.name}");
            }
            
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManager initialisé avec la caméra: {mainCamera.name}");
        }
        
        /// <summary>
        /// Démarre une transition fluide vers une caméra cible
        /// </summary>
        /// <param name="targetCamera">Caméra cible (position + rotation)</param>
        /// <param name="duration">Durée de la transition (optionnel, utilise defaultTransitionDuration si non spécifié)</param>
        public void StartTransition(UnityEngine.Camera targetCamera, float duration = -1f)
        {
            if (!enableCameraTransition)
            {
                // Transition désactivée : switch instantané
                if (mainCamera != null && targetCamera != null)
                {
                    mainCamera.transform.position = targetCamera.transform.position;
                    mainCamera.transform.rotation = targetCamera.transform.rotation;
                }
                return;
            }
            
            if (targetCamera == null)
            {
                Debug.LogError("CameraTransitionManager: targetCamera est null !");
                return;
            }
            
            if (isTransitioning)
            {
                if (showDebugLogs)
                    Debug.LogWarning("CameraTransitionManager: Transition déjà en cours, arrêt de la précédente");
                StopCurrentTransition();
            }
            
            float transitionDuration = duration > 0 ? duration : defaultTransitionDuration;
            currentTransition = StartCoroutine(MoveToTargetCamera(targetCamera.transform, transitionDuration));
        }
        
        /// <summary>
        /// Transition vers la caméra du menu principal
        /// </summary>
        public void TransitionToMainMenu(float duration = -1f)
        {
            if (mainMenuCamera != null)
            {
                StartTransition(mainMenuCamera, duration);
            }
            else
            {
                Debug.LogWarning("CameraTransitionManager: mainMenuCamera n'est pas assignée !");
            }
        }
        
        /// <summary>
        /// Transition vers la caméra de sélection de jeu
        /// </summary>
        public void TransitionToGameSelection(float duration = -1f)
        {
            if (gameSelectionCamera != null)
            {
                StartTransition(gameSelectionCamera, duration);
            }
            else
            {
                Debug.LogWarning("CameraTransitionManager: gameSelectionCamera n'est pas assignée !");
            }
        }
        
        /// <summary>
        /// Coroutine principale pour effectuer la transition fluide
        /// </summary>
        private IEnumerator MoveToTargetCamera(Transform targetTransform, float duration)
        {
            isTransitioning = true;
            
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManager: Début de transition vers {targetTransform.name} (durée: {duration}s)");
            
            // Positions et rotations de départ et d'arrivée
            Vector3 startPosition = mainCamera.transform.position;
            Quaternion startRotation = mainCamera.transform.rotation;
            Vector3 targetPosition = targetTransform.position;
            Quaternion targetRotation = targetTransform.rotation;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                // Calcul du pourcentage de progression avec courbe d'animation
                float progress = elapsedTime / duration;
                float curveValue = transitionCurve.Evaluate(progress);
                
                // Interpolation de la position et rotation
                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
                mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // S'assurer que la caméra est exactement à la position finale
            mainCamera.transform.position = targetPosition;
            mainCamera.transform.rotation = targetRotation;
            
            isTransitioning = false;
            currentTransition = null;
            
            if (showDebugLogs)
                Debug.Log("CameraTransitionManager: Transition terminée");
        }
        
        /// <summary>
        /// Arrête la transition en cours si elle existe
        /// </summary>
        public void StopCurrentTransition()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
                isTransitioning = false;
                
                if (showDebugLogs)
                    Debug.Log("CameraTransitionManager: Transition arrêtée");
            }
        }
        
        /// <summary>
        /// Vérifie si une transition est actuellement en cours
        /// </summary>
        public bool IsTransitioning => isTransitioning;
        
        /// <summary>
        /// Active ou désactive les transitions (utile pour debug/performance)
        /// </summary>
        public void SetTransitionsEnabled(bool enabled)
        {
            enableCameraTransition = enabled;
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManager: Transitions {(enabled ? "activées" : "désactivées")}");
        }
        
        private void OnDestroy()
        {
            StopCurrentTransition();
            
            if (Instance == this)
                Instance = null;
        }
        
        #region Debug et Gizmos
        
        private void OnDrawGizmosSelected()
        {
            // Dessiner les positions des caméras dans la scène
            if (mainMenuCamera != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(mainMenuCamera.transform.position, Vector3.one * 0.5f);
                Gizmos.DrawRay(mainMenuCamera.transform.position, mainMenuCamera.transform.forward * 2f);
            }
            
            if (gameSelectionCamera != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(gameSelectionCamera.transform.position, Vector3.one * 0.5f);
                Gizmos.DrawRay(gameSelectionCamera.transform.position, gameSelectionCamera.transform.forward * 2f);
            }
        }
        
        #endregion
    }
}
