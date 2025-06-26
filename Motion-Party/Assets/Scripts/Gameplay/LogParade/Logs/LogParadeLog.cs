using UnityEngine;
using System;

/// <summary>
/// Composant attaché à chaque rondin du mini-jeu LogParade.
/// Gère le mouvement de descente et la destruction automatique.
/// </summary>
public class LogParadeLog : MonoBehaviour
{
    [Header("Informations du Rondin")]
    [SerializeField] private int laneIndex = -1;
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float destroyHeight = -5f;
    
    [Header("État")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isInitialized = false;
    
    // Events
    public event Action OnDestroyed;
    
    // Composants
    private Rigidbody rb;
    private Collider col;
    
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
        
        // Pas de tag personnalisé requis - on garde le tag par défaut
        // Le tag sera géré par l'identification via le composant LogParadeLog
    }
    
    /// <summary>
    /// Initialise le rondin avec les paramètres de mouvement
    /// </summary>
    /// <param name="speed">Vitesse de descente</param>
    /// <param name="destroyY">Hauteur Y de destruction</param>
    /// <param name="lane">Index de la voie (0-3)</param>
    public void Initialize(float speed, float destroyY, int lane)
    {
        moveSpeed = speed;
        destroyHeight = destroyY;
        laneIndex = lane;
        isMoving = true;
        isInitialized = true;
        
        // Nomme l'objet pour le debug
        gameObject.name = $"LogParadeLog_Lane{lane + 1}_{GetInstanceID()}";
    }
    
    private void Update()
    {
        if (!isInitialized || !isMoving) return;
        
        // Mouvement de descente
        MoveDown();
        
        // Vérification de destruction
        CheckDestroyCondition();
    }
    
    /// <summary>
    /// Gère le mouvement de descente du rondin
    /// </summary>
    private void MoveDown()
    {
        Vector3 currentPosition = transform.position;
        currentPosition.y -= moveSpeed * Time.deltaTime;
        transform.position = currentPosition;
    }
    
    /// <summary>
    /// Vérifie si le rondin doit être détruit
    /// </summary>
    private void CheckDestroyCondition()
    {
        if (transform.position.y <= destroyHeight)
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
        
        // Notification de destruction
        OnDestroyed?.Invoke();
        
        // Destruction de l'objet
        Destroy(gameObject);
    }
    
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
    /// <param name="newSpeed">Nouvelle vitesse</param>
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }
    
    /// <summary>
    /// Obtient l'index de la voie
    /// </summary>
    /// <returns>Index de la voie (0-3)</returns>
    public int GetLaneIndex()
    {
        return laneIndex;
    }
    
    /// <summary>
    /// Vérifie si le rondin est en mouvement
    /// </summary>
    /// <returns>True si en mouvement</returns>
    public bool IsMoving()
    {
        return isMoving && isInitialized;
    }
    
    /// <summary>
    /// Obtient la position Y actuelle
    /// </summary>
    /// <returns>Position Y</returns>
    public float GetCurrentY()
    {
        return transform.position.y;
    }
    
    // Méthodes pour l'interaction avec le joueur (à étendre selon les besoins)
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
    /// <param name="player">GameObject du joueur</param>
    private void OnPlayerContact(GameObject player)
    {
        // Logique d'interaction avec le joueur
        // À personnaliser selon les mécaniques de jeu souhaitées
        
        // Exemple : effet visuel, son, points, etc.
        Debug.Log($"[LogParadeLog] Joueur en contact avec rondin voie {laneIndex + 1}");
        
        // Optionnel : arrêt temporaire du mouvement
        // StopMovement();
    }
    
    // Debug et visualisation
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!isInitialized) return;
        
        // Dessine la trajectoire de descente
        Gizmos.color = Color.cyan;
        Vector3 currentPos = transform.position;
        Vector3 endPos = new Vector3(currentPos.x, destroyHeight, currentPos.z);
        Gizmos.DrawLine(currentPos, endPos);
        
        // Dessine l'indicateur de voie
        Gizmos.color = Color.white;
        Vector3 labelPos = currentPos + Vector3.up * 0.5f;
        Gizmos.DrawWireCube(labelPos, Vector3.one * 0.3f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Informations détaillées quand sélectionné
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
    }
    #endif
}
