using System.Collections;
using UnityEngine;

#if DOTWEEN_ENABLED
using DG.Tweening;
#endif

namespace CameraTransitions
{
    /// <summary>
    /// Version avancée du CameraTransitionManager avec support DOTween optionnel.
    /// Offre des transitions plus fluides et des options d'easing avancées si DOTween est installé.
    /// </summary>
    public class CameraTransitionManagerAdvanced : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableCameraTransition = true;
        [SerializeField] private float defaultTransitionDuration = 2.0f;
        [SerializeField] private bool useDOTween = true;
        
        #if DOTWEEN_ENABLED
        [SerializeField] private Ease doTweenEase = Ease.InOutQuad;
        #endif
        
        [SerializeField] private AnimationCurve fallbackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Cameras")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private UnityEngine.Camera mainMenuCamera;
        [SerializeField] private UnityEngine.Camera gameSelectionCamera;
        
        [Header("Advanced Options")]
        [SerializeField] private bool smoothRotation = true;
        [SerializeField] private bool useShortestRotationPath = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        // État interne
        private bool isTransitioning = false;
        
        #if DOTWEEN_ENABLED
        private Tween currentPositionTween;
        private Tween currentRotationTween;
        #else
        private Coroutine currentTransition;
        #endif
        
        // Instance singleton
        public static CameraTransitionManagerAdvanced Instance { get; private set; }
        
        private void Awake()
        {
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
            
            #if DOTWEEN_ENABLED
            if (showDebugLogs && useDOTween)
                Debug.Log("CameraTransitionManagerAdvanced: DOTween détecté et activé");
            #else
            if (useDOTween && showDebugLogs)
                Debug.LogWarning("CameraTransitionManagerAdvanced: DOTween non disponible, utilisation du fallback Lerp");
            #endif
        }
        
        private void InitializeCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("CameraTransitionManagerAdvanced: Aucune caméra principale trouvée !");
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
                        Debug.Log($"CameraTransitionManagerAdvanced: Audio Listener désactivé sur {mainMenuCamera.name}");
                }
                
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManagerAdvanced: Caméra {mainMenuCamera.name} désactivée (utilisée comme cible)");
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
                        Debug.Log($"CameraTransitionManagerAdvanced: Audio Listener désactivé sur {gameSelectionCamera.name}");
                }
                
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManagerAdvanced: Caméra {gameSelectionCamera.name} désactivée (utilisée comme cible)");
            }
            
            // S'assurer que la caméra principale a un Audio Listener actif
            AudioListener mainListener = mainCamera.GetComponent<AudioListener>();
            if (mainListener == null)
            {
                mainListener = mainCamera.gameObject.AddComponent<AudioListener>();
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManagerAdvanced: Audio Listener ajouté sur {mainCamera.name}");
            }
            else
            {
                mainListener.enabled = true;
                if (showDebugLogs)
                    Debug.Log($"CameraTransitionManagerAdvanced: Audio Listener activé sur {mainCamera.name}");
            }
            
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManagerAdvanced initialisé avec la caméra: {mainCamera.name}");
        }
        
        /// <summary>
        /// Démarre une transition fluide vers une caméra cible
        /// </summary>
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
                Debug.LogError("CameraTransitionManagerAdvanced: targetCamera est null !");
                return;
            }
            
            if (isTransitioning)
                StopCurrentTransition();
            
            float transitionDuration = duration > 0 ? duration : defaultTransitionDuration;
            
            #if DOTWEEN_ENABLED
            if (useDOTween)
            {
                StartDOTweenTransition(targetCamera.transform, transitionDuration);
            }
            else
            {
                currentTransition = StartCoroutine(MoveToTargetCamera(targetCamera.transform, transitionDuration));
            }
            #else
            currentTransition = StartCoroutine(MoveToTargetCamera(targetCamera.transform, transitionDuration));
            #endif
        }
        
        #if DOTWEEN_ENABLED
        private void StartDOTweenTransition(Transform targetTransform, float duration)
        {
            isTransitioning = true;
            
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManagerAdvanced: Début de transition DOTween vers {targetTransform.name} (durée: {duration}s)");
            
            // Animation de position
            currentPositionTween = mainCamera.transform.DOMove(targetTransform.position, duration)
                .SetEase(doTweenEase)
                .OnComplete(() => {
                    if (currentRotationTween == null || !currentRotationTween.IsActive())
                        OnTransitionComplete();
                });
            
            // Animation de rotation
            if (smoothRotation)
            {
                if (useShortestRotationPath)
                {
                    currentRotationTween = mainCamera.transform.DORotateQuaternion(targetTransform.rotation, duration)
                        .SetEase(doTweenEase)
                        .OnComplete(() => {
                            if (currentPositionTween == null || !currentPositionTween.IsActive())
                                OnTransitionComplete();
                        });
                }
                else
                {
                    currentRotationTween = mainCamera.transform.DORotate(targetTransform.eulerAngles, duration)
                        .SetEase(doTweenEase)
                        .OnComplete(() => {
                            if (currentPositionTween == null || !currentPositionTween.IsActive())
                                OnTransitionComplete();
                        });
                }
            }
            else
            {
                mainCamera.transform.rotation = targetTransform.rotation;
            }
        }
        
        private void OnTransitionComplete()
        {
            isTransitioning = false;
            currentPositionTween = null;
            currentRotationTween = null;
            
            if (showDebugLogs)
                Debug.Log("CameraTransitionManagerAdvanced: Transition DOTween terminée");
        }
        #endif
        
        /// <summary>
        /// Coroutine fallback utilisant Lerp (utilisée si DOTween n'est pas disponible)
        /// </summary>
        private IEnumerator MoveToTargetCamera(Transform targetTransform, float duration)
        {
            isTransitioning = true;
            
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManagerAdvanced: Début de transition Lerp vers {targetTransform.name} (durée: {duration}s)");
            
            Vector3 startPosition = mainCamera.transform.position;
            Quaternion startRotation = mainCamera.transform.rotation;
            Vector3 targetPosition = targetTransform.position;
            Quaternion targetRotation = targetTransform.rotation;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                float progress = elapsedTime / duration;
                float curveValue = fallbackCurve.Evaluate(progress);
                
                // Interpolation de position
                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
                
                // Interpolation de rotation
                if (smoothRotation)
                {
                    if (useShortestRotationPath)
                        mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);
                    else
                        mainCamera.transform.rotation = Quaternion.LerpUnclamped(startRotation, targetRotation, curveValue);
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Position finale exacte
            mainCamera.transform.position = targetPosition;
            if (smoothRotation)
                mainCamera.transform.rotation = targetRotation;
            
            isTransitioning = false;
            #if !DOTWEEN_ENABLED
            currentTransition = null;
            #endif
            
            if (showDebugLogs)
                Debug.Log("CameraTransitionManagerAdvanced: Transition Lerp terminée");
        }
        
        /// <summary>
        /// Transition vers la caméra du menu principal
        /// </summary>
        public void TransitionToMainMenu(float duration = -1f)
        {
            if (mainMenuCamera != null)
                StartTransition(mainMenuCamera, duration);
            else
                Debug.LogWarning("CameraTransitionManagerAdvanced: mainMenuCamera n'est pas assignée !");
        }
        
        /// <summary>
        /// Transition vers la caméra de sélection de jeu
        /// </summary>
        public void TransitionToGameSelection(float duration = -1f)
        {
            if (gameSelectionCamera != null)
                StartTransition(gameSelectionCamera, duration);
            else
                Debug.LogWarning("CameraTransitionManagerAdvanced: gameSelectionCamera n'est pas assignée !");
        }
        
        /// <summary>
        /// Arrête la transition en cours
        /// </summary>
        public void StopCurrentTransition()
        {
            #if DOTWEEN_ENABLED
            if (useDOTween)
            {
                if (currentPositionTween != null && currentPositionTween.IsActive())
                    currentPositionTween.Kill();
                if (currentRotationTween != null && currentRotationTween.IsActive())
                    currentRotationTween.Kill();
                    
                currentPositionTween = null;
                currentRotationTween = null;
            }
            else if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
            #else
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
            #endif
            
            isTransitioning = false;
            
            if (showDebugLogs)
                Debug.Log("CameraTransitionManagerAdvanced: Transition arrêtée");
        }
        
        /// <summary>
        /// Vérifie si une transition est en cours
        /// </summary>
        public bool IsTransitioning => isTransitioning;
        
        /// <summary>
        /// Active ou désactive les transitions
        /// </summary>
        public void SetTransitionsEnabled(bool enabled)
        {
            enableCameraTransition = enabled;
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManagerAdvanced: Transitions {(enabled ? "activées" : "désactivées")}");
        }
        
        /// <summary>
        /// Bascule entre DOTween et Lerp (si DOTween est disponible)
        /// </summary>
        public void SetUseDOTween(bool use)
        {
            #if DOTWEEN_ENABLED
            useDOTween = use;
            if (showDebugLogs)
                Debug.Log($"CameraTransitionManagerAdvanced: DOTween {(use ? "activé" : "désactivé")}");
            #else
            if (showDebugLogs)
                Debug.LogWarning("CameraTransitionManagerAdvanced: DOTween non disponible, paramètre ignoré");
            #endif
        }
        
        private void OnDestroy()
        {
            StopCurrentTransition();
            
            if (Instance == this)
                Instance = null;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Dessiner les positions des caméras
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
            
            // Ligne entre les deux positions si toutes les deux sont définies
            if (mainMenuCamera != null && gameSelectionCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(mainMenuCamera.transform.position, gameSelectionCamera.transform.position);
            }
        }
    }
}
