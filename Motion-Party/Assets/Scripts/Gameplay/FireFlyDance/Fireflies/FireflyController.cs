using System.Collections;
using UnityEngine;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Fireflies
{
    /// <summary>
    /// Contrôleur de comportement individuel des lucioles
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
        Oscillation, // Mouvement d'oscillation douce
        Static       // Luciole statique
    }

    /// <summary>
    /// États possible d'une luciole
    /// </summary>
    public enum FireflyState
    {
        Spawning,    // En cours d'apparition
        Active,      // Active et interactive
        Warning,     // Proche de l'expiration (clignotement)
        Captured,    // Capturée par le joueur
        Expired      // Expirée (avant destruction)
    }

    #endregion

    #region Fields

    [Header("Configuration")]
    [SerializeField] private FireflyDanceConfig config;
    
    [Header("Comportement")]
    [SerializeField] private MovementBehavior movementType = MovementBehavior.Random;
    [SerializeField] private bool enableMovement = true;
    
    [Header("Paramètres de mouvement")]
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float oscillationAmplitude = 0.5f;
    [SerializeField] private float oscillationFrequency = 1f;
    
    [Header("Paramètres visuels")]
    [SerializeField] private bool enableBlinkEffect = true;
    [SerializeField] private float blinkSpeed = 2f;
    
    // État actuel
    private FireflyState currentState = FireflyState.Spawning;
    private float lifetime = 0f;
    private float maxLifetime = 10f;
    private bool isInteractable = false;
    
    // Propriété publique pour vérifier si la luciole est active
    public bool IsActive => currentState == FireflyState.Active;
    
    // Mouvement
    private Vector3 targetDirection;
    private float directionTimer;
    private Vector3 oscillationCenter;
    private float oscillationTime;
    private Vector3 velocity;
    
    // Composants
    private Renderer fireflyRenderer;
    private Collider fireflyCollider;
    private Light fireflyLight;
    
    // Couleurs d'état
    private Color defaultColor = Color.yellow;
    private Color warningColor = Color.red;
    private Color capturedColor = Color.green;

    #endregion

    #region Properties

    /// <summary>
    /// État actuel de la luciole
    /// </summary>
    public FireflyState CurrentState => currentState;

    /// <summary>
    /// Indique si la luciole est interactive
    /// </summary>
    public bool IsInteractable => isInteractable;

    /// <summary>
    /// Progression de la durée de vie (0 = nouveau, 1 = expiré)
    /// </summary>
    public float LifetimeProgress => maxLifetime > 0 ? lifetime / maxLifetime : 0f;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        ValidateComponents();
        if (config != null)
        {
            Initialize();
        }
    }

    void Update()
    {
        if (!IsConfigValid()) return;

        UpdateMovement();
        UpdateVisualEffects();
        
        // Vérification de sécurité : forcer la luciole à rester dans les limites
        if (!IsWithinBounds())
        {
            ClampPositionToBounds();
        }
    }

    void OnDestroy()
    {
        // Émettre événement de destruction
        FireflyDanceEvents.OnFireflyDestroyed?.Invoke(this);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialise la luciole avec la configuration fournie
    /// </summary>
    public void Initialize(FireflyDanceConfig newConfig = null)
    {
        if (newConfig != null)
            config = newConfig;
            
        if (!IsConfigValid())
        {
            FireflyDanceLogger.LogError("FireflyController - Configuration manquante", this);
            return;
        }

        SetupFirefly();
        SetState(FireflyState.Spawning);
        
        // Démarrer la transition vers l'état actif
        StartCoroutine(ActivateAfterDelay(0.5f));
        
    }

    /// <summary>
    /// Configure les paramètres de base de la luciole
    /// </summary>
    private void SetupFirefly()
    {
        // Durée de vie aléatoire
        maxLifetime = Random.Range(config.MinFireflyLifetime, config.MaxFireflyLifetime);
        
        // Direction initiale
        SetRandomDirection();
        
        // Centre d'oscillation
        oscillationCenter = transform.position;
        
        StartLifetimeTimer();
    }

    /// <summary>
    /// Démarre le timer de durée de vie
    /// </summary>
    private void StartLifetimeTimer()
    {
        lifetime = 0f;
        StartCoroutine(LifetimeCoroutine());
    }

    /// <summary>
    /// Coroutine qui gère la durée de vie de la luciole
    /// </summary>
    private IEnumerator LifetimeCoroutine()
    {
        while (lifetime < maxLifetime && currentState != FireflyState.Captured)
        {
            lifetime += Time.deltaTime;
            
            // Passage en état warning à 80% de la durée de vie
            if (lifetime >= maxLifetime * 0.8f && currentState == FireflyState.Active)
            {
                SetState(FireflyState.Warning);
            }
            
            yield return null;
        }
        
        if (currentState != FireflyState.Captured)
        {
            ExpireFirefly();
        }
    }

    /// <summary>
    /// Coroutine d'activation après délai
    /// </summary>
    private IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetState(FireflyState.Active);
        EnableInteraction(true);
    }

    #endregion

    #region State Management

    /// <summary>
    /// Change l'état de la luciole
    /// </summary>
    private void SetState(FireflyState newState)
    {
        if (currentState == newState) return;

        FireflyState oldState = currentState;
        currentState = newState;
        
        OnStateChanged(oldState, newState);
        
        // Émettre événement
        FireflyDanceEvents.OnFireflyStateChanged?.Invoke(this, oldState, newState);
    }

    /// <summary>
    /// Gère les changements d'état
    /// </summary>
    private void OnStateChanged(FireflyState oldState, FireflyState newState)
    {
        switch (newState)
        {
            case FireflyState.Spawning:
                isInteractable = false;
                break;
                
            case FireflyState.Active:
                isInteractable = true;
                break;
                
            case FireflyState.Warning:
                isInteractable = true;
                break;
                
            case FireflyState.Captured:
                isInteractable = false;
                enableMovement = false;
                break;
                
            case FireflyState.Expired:
                isInteractable = false;
                enableMovement = false;
                break;
        }
    }

    /// <summary>
    /// Active ou désactive l'interaction
    /// </summary>
    public void EnableInteraction(bool enable)
    {
        isInteractable = enable;
        
        if (fireflyCollider != null)
        {
            fireflyCollider.enabled = enable;
        }
    }

    #endregion

    #region Lifetime Management

    /// <summary>
    /// Fait expirer la luciole
    /// </summary>
    private void ExpireFirefly()
    {
        SetState(FireflyState.Expired);
        FireflyDanceEvents.OnFireflyExpired?.Invoke(this);
        
        // Destruction avec délai pour l'animation
        StartCoroutine(DestroyAfterDelay(1f));
    }

    #endregion

    #region Movement

    /// <summary>
    /// Met à jour le mouvement
    /// </summary>
    private void UpdateMovement()
    {
        if (!enableMovement) return;

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
                
            case MovementBehavior.Static:
                // Pas de mouvement
                break;
        }
        
        ApplyMovement();
    }

    /// <summary>
    /// Applique le mouvement à la position
    /// </summary>
    private void ApplyMovement()
    {
        transform.position += velocity * Time.deltaTime;
        CheckBoundsAndMove();
    }

    /// <summary>
    /// Mouvement linéaire simple
    /// </summary>
    private void UpdateLinearMovement()
    {
        velocity = targetDirection * config.FireflyMoveSpeed;
    }

    /// <summary>
    /// Mouvement aléatoire avec changement de direction
    /// </summary>
    private void UpdateRandomMovement()
    {
        directionTimer += Time.deltaTime;
        
        if (directionTimer >= directionChangeInterval)
        {
            SetRandomDirection();
            directionTimer = 0f;
        }
        
        velocity = targetDirection * config.FireflyMoveSpeed;
    }

    /// <summary>
    /// Mouvement d'oscillation douce
    /// </summary>
    private void UpdateOscillationMovement()
    {
        oscillationTime += Time.deltaTime;
        
        Vector3 oscillation = new Vector3(
            Mathf.Sin(oscillationTime * oscillationFrequency) * oscillationAmplitude,
            Mathf.Cos(oscillationTime * oscillationFrequency * 0.7f) * oscillationAmplitude * 0.5f,
            Mathf.Sin(oscillationTime * oscillationFrequency * 0.3f) * oscillationAmplitude * 0.3f
        );
        
        Vector3 targetPos = oscillationCenter + oscillation;
        velocity = (targetPos - transform.position) * config.FireflyMoveSpeed;
    }

    /// <summary>
    /// Définit une direction aléatoire
    /// </summary>
    private void SetRandomDirection()
    {
        targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }

    /// <summary>
    /// Vérifie les limites et fait rebondir si nécessaire
    /// </summary>
    private void CheckBoundsAndMove()
    {
        if (!IsConfigValid()) return;
        
        Vector3 pos = transform.position;
        bool bounced = false;
        
        // Utiliser les limites de la configuration
        if (pos.x < config.TopLeft.x || pos.x > config.BottomRight.x)
        {
            targetDirection.x = -targetDirection.x;
            pos.x = Mathf.Clamp(pos.x, config.TopLeft.x, config.BottomRight.x);
            bounced = true;
        }
        
        if (pos.y < config.BottomRight.y || pos.y > config.TopLeft.y)
        {
            targetDirection.y = -targetDirection.y;
            pos.y = Mathf.Clamp(pos.y, config.BottomRight.y, config.TopLeft.y);
            bounced = true;
        }
        
        // Vérifier aussi les limites en profondeur Z
        float minZ = -config.Depth/2f;
        float maxZ = config.Depth/2f;
        if (pos.z < minZ || pos.z > maxZ)
        {
            targetDirection.z = -targetDirection.z;
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            bounced = true;
        }
        
        transform.position = pos;
        
        if (bounced)
        {
            directionTimer = 0f; // Reset timer on bounce
        }
    }

    /// <summary>
    /// Force la position de la luciole à rester dans les limites
    /// </summary>
    public void ClampPositionToBounds()
    {
        if (!IsConfigValid()) return;
        
        Vector2 clampedPos = config.ClampToBounds(transform.position);
        float clampedZ = Mathf.Clamp(transform.position.z, -config.Depth/2f, config.Depth/2f);
        transform.position = new Vector3(clampedPos.x, clampedPos.y, clampedZ);
    }

    /// <summary>
    /// Vérifie si la luciole est dans les limites
    /// </summary>
    public bool IsWithinBounds()
    {
        if (!IsConfigValid()) return true;
        
        Vector3 pos = transform.position;
        
        // Vérifier les limites X et Y avec la méthode de la config
        bool inXYBounds = config.IsValidPosition(pos);
        
        // Vérifier les limites Z
        bool inZBounds = pos.z >= -config.Depth/2f && pos.z <= config.Depth/2f;
        
        return inXYBounds && inZBounds;
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Met à jour les effets visuels
    /// </summary>
    private void UpdateVisualEffects()
    {
        if (enableBlinkEffect)
        {
            UpdateBlinkEffect();
        }
        
        UpdateStateVisuals();
    }

    /// <summary>
    /// Met à jour l'effet de clignotement
    /// </summary>
    private void UpdateBlinkEffect()
    {
        if (fireflyLight != null)
        {
            float blink = Mathf.Sin(Time.time * blinkSpeed) * 0.5f + 0.5f;
            fireflyLight.intensity = blink;
        }
    }

    /// <summary>
    /// Met à jour les visuels selon l'état
    /// </summary>
    private void UpdateStateVisuals()
    {
        Color targetColor = GetStateColor();
        
        if (fireflyRenderer != null)
        {
            fireflyRenderer.material.color = targetColor;
        }
        
        if (fireflyLight != null)
        {
            fireflyLight.color = targetColor;
        }
    }

    /// <summary>
    /// Retourne la couleur selon l'état
    /// </summary>
    private Color GetStateColor()
    {
        switch (currentState)
        {
            case FireflyState.Spawning:
                return Color.white;
            case FireflyState.Active:
                return defaultColor;
            case FireflyState.Warning:
                return warningColor;
            case FireflyState.Captured:
                return capturedColor;
            case FireflyState.Expired:
                return Color.gray;
            default:
                return defaultColor;
        }
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Appelée quand la luciole est capturée
    /// </summary>
    public void OnCaptured()
    {
        if (currentState == FireflyState.Captured) return;
        
        SetState(FireflyState.Captured);
        
        // Destruction avec délai pour l'animation
        StartCoroutine(DestroyAfterDelay(1.5f));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Détruit la luciole immédiatement
    /// </summary>
    public void DestroyFirefly()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Vérifie si la configuration est valide
    /// </summary>
    private bool IsConfigValid()
    {
        return config != null;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Valide les composants requis
    /// </summary>
    private void ValidateComponents()
    {
        fireflyRenderer = GetComponent<Renderer>();
        fireflyCollider = GetComponent<Collider>();
        fireflyLight = GetComponent<Light>();
        
        if (fireflyRenderer == null)
        {
            FireflyDanceLogger.LogWarning("FireflyController - Renderer manquant", this);
        }
        
        if (fireflyCollider == null)
        {
            FireflyDanceLogger.LogWarning("FireflyController - Collider manquant", this);
        }
    }

    /// <summary>
    /// Valide les paramètres dans l'éditeur
    /// </summary>
    void OnValidate()
    {
        directionChangeInterval = Mathf.Max(0.1f, directionChangeInterval);
        oscillationAmplitude = Mathf.Max(0f, oscillationAmplitude);
        oscillationFrequency = Mathf.Max(0.1f, oscillationFrequency);
        blinkSpeed = Mathf.Max(0.1f, blinkSpeed);
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Coroutine de destruction avec délai
    /// </summary>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyFirefly();
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        // Dessiner les limites de mouvement depuis la configuration
        if (IsConfigValid())
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(config.Center.x, config.Center.y, 0);
            Vector3 size = new Vector3(config.Width, config.Height, config.Depth);
            Gizmos.DrawWireCube(center, size);
        }
        
        // Dessiner la direction actuelle
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + targetDirection);
        }
        
        // Dessiner le centre d'oscillation
        if (movementType == MovementBehavior.Oscillation)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(oscillationCenter, 0.1f);
        }
    }

    #endregion
}
}
