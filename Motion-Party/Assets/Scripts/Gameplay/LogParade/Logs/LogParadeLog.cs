using UnityEngine;
using System;

/// <summary>
/// Composant attaché à chaque rondin du mini-jeu LogParade.
/// Gère le mouvement de descente et la destruction automatique.
/// </summary>
public class LogParadeLog : MonoBehaviour
{
    #region Fields

    [Header("Informations du Rondin")]
    [SerializeField] private int laneIndex = -1;
    [SerializeField] private float moveSpeed = 1.2f;

    [Header("État")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isInitialized = false;
    public event Action OnDestroyed;
    private Rigidbody rb;
    private Collider col;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Récupération des composants
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        // Configuration du Rigidbody si présent
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        // Assure qu'il y a un collider pour l'interaction joueur
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
    }

    private void Update()
    {
        if (!isInitialized || !isMoving) return;
        MoveDown();
        CheckDestroyCondition();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialise le rondin avec les paramètres de mouvement
    /// </summary>
    public void Initialize(float speed, int lane)
    {
        moveSpeed = speed;
        laneIndex = lane;
        isMoving = true;
        isInitialized = true;
        gameObject.name = $"LogParadeLog_Lane{lane + 1}_{GetInstanceID()}";
    }
    #endregion

    #region Movement & Destruction
    /// <summary>
    /// Gère le mouvement du rondin sur l'axe Z (vue du dessus)
    /// </summary>
    private void MoveDown()
    {
        Vector3 currentPosition = transform.position;
        currentPosition.z -= moveSpeed * Time.deltaTime;
        transform.position = currentPosition;
    }
    /// <summary>
    /// Vérifie si le rondin doit être détruit (sur l'axe Z)
    /// </summary>
    private void CheckDestroyCondition()
    {
        if (transform.position.z <= -20f)
        {
            DestroyLog();
        }
    }
    /// <summary>
    /// Détruit le rondin proprement
    /// </summary>
    public void DestroyLog()
    {
        if (!isInitialized) return;
        isMoving = false;
        isInitialized = false;
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
    #endregion

    #region Player Interaction
    /// <summary>
    /// Appelé quand le joueur entre en contact avec le rondin
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Vérification flexible pour détecter le joueur
        if (other.CompareTag("Player") || other.name.Contains("Player") || other.name.Contains("Avatar"))
        {
            OnPlayerContact(other.gameObject);
        }
    }
    /// <summary>
    /// Gère le contact avec le joueur
    /// </summary>
    private void OnPlayerContact(GameObject player)
    {
        // Logique d'interaction avec le joueur
        // À personnaliser selon les mécaniques de jeu souhaitées
        // Exemple : effet visuel, son, points, etc.
    }
    #endregion

    #region Public API
    /// <summary>
    /// Arrête le mouvement du rondin (utile pour pause)
    /// </summary>
    public void StopMovement()
    {
        isMoving = false;
    }
    /// <summary>
    /// Reprend le mouvement du rondin
    /// </summary>
    public void ResumeMovement()
    {
        if (isInitialized)
        {
            isMoving = true;
        }
    }
    /// <summary>
    /// Modifie la vitesse de mouvement
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
    /// <summary>
    /// Obtient l'index de la voie
    /// </summary>
    public int GetLaneIndex()
    {
        return laneIndex;
    }
    /// <summary>
    /// Vérifie si le rondin est en mouvement
    /// </summary>
    public bool IsMoving()
    {
        return isMoving && isInitialized;
    }
    /// <summary>
    /// Obtient la position Y actuelle
    /// </summary>
    public float GetCurrentY()
    {
        return transform.position.y;
    }
    #endregion

}
