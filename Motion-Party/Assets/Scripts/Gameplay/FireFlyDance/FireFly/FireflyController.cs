using UnityEngine;

/// <summary>
/// Contrôleur de comportement individuel des lucioles
/// Gère le mouvement autonome dans la zone définie par FireflyDanceConfig
/// </summary>
public class FireflyController : MonoBehaviour
{
    #region Enums

    /// <summary>
    /// Types de comportement de mouvement disponibles pour les lucioles
    /// </summary>
    public enum MovementBehavior
    {
        Linear,      // Mouvement linéaire avec rebond sur les bords
        Random,      // Mouvement aléatoire avec changement de direction périodique
        Oscillation  // Mouvement d'oscillation douce
    }

    #endregion

    #region Fields

    [Header("Configuration")]
    [SerializeField] private FireflyDanceConfig config;
    
    [Header("Comportement")]
    [SerializeField] private MovementBehavior movementType = MovementBehavior.Random;
    [SerializeField] private bool enableMovement = true;
    
    [Header("Paramètres de mouvement")]
    [SerializeField] private float directionChangeInterval = 2f; // Intervalle pour changer de direction (Random mode)
    [SerializeField] private float oscillationAmplitude = 0.5f;  // Amplitude d'oscillation (Oscillation mode)
    [SerializeField] private float oscillationFrequency = 1f;    // Fréquence d'oscillation (Oscillation mode)
    
    [Header("Runtime Info (Read Only)")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector2 currentDirection;
    [SerializeField] private bool isMoving = false;

    // Variables privées pour la logique de mouvement
    private Vector2 velocity;
    private float directionTimer;
    private Vector2 initialPosition;
    private float oscillationTime;
    private Renderer fireflyRenderer;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        InitializeFirefly();
    }

    void Update()
    {
        if (enableMovement && isMoving && config != null)
        {
            UpdateMovement();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            DrawMovementGizmos();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initialise la luciole avec une configuration spécifique
    /// </summary>
    /// <param name="danceConfig">Configuration à utiliser</param>
    public void Initialize(FireflyDanceConfig danceConfig)
    {
        config = danceConfig;
        InitializeFirefly();
    }

    /// <summary>
    /// Active ou désactive le mouvement de la luciole
    /// </summary>
    /// <param name="enable">True pour activer, false pour désactiver</param>
    public void SetMovementEnabled(bool enable)
    {
        enableMovement = enable;
        isMoving = enable;
    }

    /// <summary>
    /// Change le type de comportement de mouvement
    /// </summary>
    /// <param name="newBehavior">Nouveau comportement à appliquer</param>
    public void SetMovementBehavior(MovementBehavior newBehavior)
    {
        movementType = newBehavior;
        ResetMovementState();
    }

    /// <summary>
    /// Force la repositionnement de la luciole dans les limites de la zone
    /// </summary>
    public void ForceRepositionInBounds()
    {
        if (config == null) return;
        
        Vector2 clampedPosition = config.ClampToBounds(transform.position);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, transform.position.z);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialise les paramètres de base de la luciole
    /// </summary>
    private void InitializeFirefly()
    {
        if (config == null)
        {
            Debug.LogWarning("FireflyController: Aucune configuration assignée!", this);
            return;
        }

        // Récupération du renderer pour les validations futures
        fireflyRenderer = GetComponent<Renderer>();
        if (fireflyRenderer == null)
        {
            Debug.LogWarning("FireflyController: Aucun Renderer trouvé sur le GameObject!", this);
        }

        // Positionnement initial dans les limites si nécessaire
        ForceRepositionInBounds();
        
        // Initialisation de la vitesse et direction
        currentSpeed = Random.Range(config.MinSpeed, config.MaxSpeed);
        
        // Sauvegarde de la position initiale pour l'oscillation
        initialPosition = transform.position;
        
        // Démarrage du mouvement selon le type choisi
        ResetMovementState();
        isMoving = enableMovement;
        
        Debug.Log($"FireflyController: Luciole initialisée avec comportement {movementType}, vitesse {currentSpeed:F2}");
    }

    /// <summary>
    /// Remet à zéro l'état du mouvement selon le type de comportement
    /// </summary>
    private void ResetMovementState()
    {
        switch (movementType)
        {
            case MovementBehavior.Linear:
                SetRandomDirection();
                break;
                
            case MovementBehavior.Random:
                SetRandomDirection();
                directionTimer = 0f;
                break;
                
            case MovementBehavior.Oscillation:
                oscillationTime = 0f;
                initialPosition = transform.position;
                break;
        }
    }

    /// <summary>
    /// Met à jour le mouvement de la luciole selon son comportement
    /// </summary>
    private void UpdateMovement()
    {
        switch (movementType)
        {
            case MovementBehavior.Linear:
                UpdateLinearMovement();
                break;
                
            case MovementBehavior.Random:
                UpdateRandomMovement();
                break;
                
            case MovementBehavior.Oscillation:
                UpdateOscillationMovement();
                break;
        }

        // Vérification des limites et application du mouvement
        CheckBoundsAndMove();
    }

    /// <summary>
    /// Gère le mouvement linéaire avec rebond sur les bords
    /// </summary>
    private void UpdateLinearMovement()
    {
        velocity = currentDirection * currentSpeed;
    }

    /// <summary>
    /// Gère le mouvement aléatoire avec changement de direction périodique
    /// </summary>
    private void UpdateRandomMovement()
    {
        directionTimer += Time.deltaTime;
        
        // Changement de direction à intervalle régulier
        if (directionTimer >= directionChangeInterval)
        {
            SetRandomDirection();
            directionTimer = 0f;
        }
        
        velocity = currentDirection * currentSpeed;
    }

    /// <summary>
    /// Gère le mouvement d'oscillation douce
    /// </summary>
    private void UpdateOscillationMovement()
    {
        oscillationTime += Time.deltaTime;
        
        // Calcul de l'oscillation en forme de huit ou circulaire
        float x = Mathf.Sin(oscillationTime * oscillationFrequency) * oscillationAmplitude;
        float y = Mathf.Cos(oscillationTime * oscillationFrequency * 0.5f) * oscillationAmplitude * 0.5f;
        
        Vector2 oscillationOffset = new Vector2(x, y);
        Vector2 targetPosition = initialPosition + oscillationOffset;
        
        // Calcul de la vitesse pour atteindre la position cible
        Vector2 currentPos = transform.position;
        velocity = (targetPosition - currentPos) * currentSpeed;
    }

    /// <summary>
    /// Définit une direction aléatoire pour le mouvement
    /// </summary>
    private void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        currentDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    /// <summary>
    /// Vérifie les limites de la zone et applique le mouvement
    /// </summary>
    private void CheckBoundsAndMove()
    {
        Vector2 currentPos = transform.position;
        Vector2 nextPosition = currentPos + velocity * Time.deltaTime;
        
        // Vérification des collisions avec les bords et rebond
        if (nextPosition.x <= config.TopLeft.x || nextPosition.x >= config.BottomRight.x)
        {
            currentDirection.x = -currentDirection.x; // Rebond horizontal
            velocity.x = -velocity.x;
        }
        
        if (nextPosition.y >= config.TopLeft.y || nextPosition.y <= config.BottomRight.y)
        {
            currentDirection.y = -currentDirection.y; // Rebond vertical
            velocity.y = -velocity.y;
        }
        
        // Application du mouvement avec contrainte dans les limites
        Vector2 finalPosition = config.ClampToBounds(currentPos + velocity * Time.deltaTime);
        transform.position = new Vector3(finalPosition.x, finalPosition.y, transform.position.z);
    }

    /// <summary>
    /// Dessine les gizmos de débogage pour visualiser le mouvement
    /// </summary>
    private void DrawMovementGizmos()
    {
        // Zone de jeu
        Gizmos.color = Color.green;
        Vector3 topLeft = new Vector3(config.TopLeft.x, config.TopLeft.y, transform.position.z);
        Vector3 bottomRight = new Vector3(config.BottomRight.x, config.BottomRight.y, transform.position.z);
        Vector3 topRight = new Vector3(config.BottomRight.x, config.TopLeft.y, transform.position.z);
        Vector3 bottomLeft = new Vector3(config.TopLeft.x, config.BottomRight.y, transform.position.z);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Direction actuelle
        if (Application.isPlaying && enableMovement)
        {
            Gizmos.color = Color.red;
            Vector3 directionVector = new Vector3(currentDirection.x, currentDirection.y, 0) * 0.5f;
            Gizmos.DrawLine(transform.position, transform.position + directionVector);
        }
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validation automatique des paramètres dans l'éditeur Unity
    /// </summary>
    private void OnValidate()
    {
        // Validation des intervalles
        directionChangeInterval = Mathf.Max(0.1f, directionChangeInterval);
        oscillationAmplitude = Mathf.Max(0.1f, oscillationAmplitude);
        oscillationFrequency = Mathf.Max(0.1f, oscillationFrequency);
        
        // Mise à jour de la vitesse si config est disponible
        if (config != null && Application.isPlaying)
        {
            currentSpeed = Mathf.Clamp(currentSpeed, config.MinSpeed, config.MaxSpeed);
        }
    }

    #endregion
}
