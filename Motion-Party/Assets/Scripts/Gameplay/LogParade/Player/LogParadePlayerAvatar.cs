using UnityEngine;
using System;

/// <summary>
/// Gère l'avatar du joueur qui se déplace sur les 4 voies
/// Réagit aux changements de voie détectés par LogParadeLateralTracker
/// S'inspire des patterns de mouvement des scripts existants
/// </summary>
public class LogParadePlayerAvatar : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float laneWidth = 2f;
    public Vector3 basePosition = Vector3.zero;
    
    [Header("Animation")]
    public bool enableSmoothMovement = true;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    public GameObject avatarModel;
    public bool rotateTowardsMovement = true;
    public float rotationSpeed = 10f;
    
    [Header("Effects")]
    public ParticleSystem[] laneChangeEffects;
    public AudioClip laneChangeSound;
    
    // Private fields
    private int targetLane = 2;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float movementProgress = 0f;
    private Vector3 startMovePosition;
    private Vector3 lastPosition;
    private AudioSource audioSource;

    [Header("Debug")]
    public bool showDebugInfo = true;

    void Start()
    {
        InitializeAvatar();
    }

    void Update()
    {
        UpdateMovement();
        UpdateRotation();
    }

    /// <summary>
    /// Initialise l'avatar
    /// </summary>
    private void InitializeAvatar()
    {
        // S'assurer qu'on a un modèle d'avatar
        if (avatarModel == null)
        {
            avatarModel = gameObject;
        }
        
        // Configurer l'AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Positionner l'avatar au centre initialement
        SetLaneInstant(2);
        
        LogParadeLogger.LogVerbose("Avatar du LogParade initialisé.");
    }

    /// <summary>
    /// Change la voie cible de l'avatar
    /// </summary>
    public void SetTargetLane(int lane)
    {
        lane = Mathf.Clamp(lane, 1, 4);
        
        if (lane != targetLane)
        {
            targetLane = lane;
            StartMovementToLane();
            PlayLaneChangeEffects();
        }
    }

    /// <summary>
    /// Positionne l'avatar instantanément sur une voie
    /// </summary>
    public void SetLaneInstant(int lane)
    {
        lane = Mathf.Clamp(lane, 1, 4);
        targetLane = lane;
        
        Vector3 lanePosition = CalculateLanePosition(lane);
        transform.position = lanePosition;
        targetPosition = lanePosition;
        
        isMoving = false;
        movementProgress = 1f;
          if (showDebugInfo)
            LogParadeLogger.LogVerbose($"Avatar positionné instantanément sur la voie {lane}");
    }

    /// <summary>
    /// Démarre le mouvement vers une nouvelle voie
    /// </summary>
    private void StartMovementToLane()
    {
        if (enableSmoothMovement)
        {
            startMovePosition = transform.position;
            targetPosition = CalculateLanePosition(targetLane);
            isMoving = true;
            movementProgress = 0f;
              if (showDebugInfo)
                LogParadeLogger.LogVerbose($"Démarrage du mouvement vers la voie {targetLane}");
        }
        else
        {
            SetLaneInstant(targetLane);
        }
    }

    /// <summary>
    /// Calcule la position world d'une voie donnée
    /// </summary>
    private Vector3 CalculateLanePosition(int lane)
    {
        // Convertir le numéro de voie (1-4) en offset X
        // Voie 1 = le plus à gauche, Voie 4 = le plus à droite
        float xOffset = (lane - 2.5f) * laneWidth;
        return basePosition + Vector3.right * xOffset;
    }

    /// <summary>
    /// Met à jour le mouvement de l'avatar
    /// </summary>
    private void UpdateMovement()
    {
        if (!isMoving) return;

        // Avancer le progrès du mouvement
        movementProgress += Time.deltaTime * moveSpeed;
        
        if (movementProgress >= 1f)
        {
            // Mouvement terminé
            movementProgress = 1f;
            isMoving = false;
            transform.position = targetPosition;
              if (showDebugInfo)
                LogParadeLogger.LogVerbose($"Mouvement terminé. Avatar sur la voie {targetLane}");
        }
        else
        {
            // Interpoler la position avec la courbe d'animation
            float curveValue = movementCurve.Evaluate(movementProgress);
            transform.position = Vector3.Lerp(startMovePosition, targetPosition, curveValue);
        }
    }

    /// <summary>
    /// Met à jour la rotation de l'avatar pour qu'il regarde dans la direction du mouvement
    /// </summary>
    private void UpdateRotation()
    {
        if (!rotateTowardsMovement) return;

        Vector3 currentPosition = transform.position;
        Vector3 movementDirection = currentPosition - lastPosition;
        
        if (movementDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            
            // Incliner légèrement dans la direction du mouvement
            if (movementDirection.x != 0)
            {
                float tiltAngle = Mathf.Sign(movementDirection.x) * 15f;
                targetRotation *= Quaternion.Euler(0, 0, -tiltAngle);
            }
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        
        lastPosition = currentPosition;
    }

    /// <summary>
    /// Joue les effets de changement de voie
    /// </summary>
    private void PlayLaneChangeEffects()
    {
        // Jouer le son
        if (laneChangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(laneChangeSound);
        }
        
        // Jouer les effets de particules
        if (laneChangeEffects != null && laneChangeEffects.Length > 0)
        {
            foreach (var effect in laneChangeEffects)
            {
                if (effect != null)
                {
                    effect.Play();
                }
            }
        }
    }

    /// <summary>
    /// Obtient la voie actuelle de l'avatar
    /// </summary>
    public int GetCurrentLane()
    {
        return targetLane;
    }

    /// <summary>
    /// Vérifie si l'avatar est en mouvement
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }

    /// <summary>
    /// Obtient le progrès du mouvement actuel (0-1)
    /// </summary>
    public float GetMovementProgress()
    {
        return movementProgress;
    }

    void OnDrawGizmos()
    {
        // Dessiner les voies dans l'éditeur
        Gizmos.color = Color.yellow;
        
        for (int i = 1; i <= 4; i++)
        {
            Vector3 lanePos = CalculateLanePosition(i);
            Gizmos.DrawWireCube(lanePos, Vector3.one * 0.5f);
            
            // Dessiner une ligne pour chaque voie
            Vector3 lineStart = lanePos + Vector3.back * 10f;
            Vector3 lineEnd = lanePos + Vector3.forward * 10f;
            Gizmos.DrawLine(lineStart, lineEnd);
        }
        
        // Mettre en évidence la voie cible
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;            Vector3 targetPos = CalculateLanePosition(targetLane);
            Gizmos.DrawSphere(targetPos, 0.3f);
        }
    }
}
