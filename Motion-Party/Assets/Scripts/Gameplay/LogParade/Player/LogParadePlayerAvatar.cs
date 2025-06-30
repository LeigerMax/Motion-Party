using UnityEngine;
using System;

/// <summary>
/// Gère l'avatar du joueur qui se déplace sur les 4 voies
/// Réagit aux changements de voie détectés par LogParadeLateralTracker
/// S'inspire des patterns de mouvement des scripts existants
/// </summary>
public class LogParadePlayerAvatar : MonoBehaviour
{
    #region Fields

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
    private int targetLane = 2;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float movementProgress = 0f;
    private Vector3 startMovePosition;
    private Vector3 lastPosition;
    private AudioSource audioSource;


    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeAvatar();
    }

    void Update()
    {
        UpdateMovement();
        UpdateRotation();
    }
    #endregion

    #region Initialization
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
    }
    #endregion

    #region Lane Management
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
    #endregion

    #region Movement & Rotation
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
    #endregion

    #region Effects
    /// <summary>
    /// Joue les effets de changement de voie
    /// </summary>
    private void PlayLaneChangeEffects()
    {
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
    #endregion

    #region Public API
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
    #endregion
}
