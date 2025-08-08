using UnityEngine;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Configuration pour le jeu de danse des lucioles
    /// Contient tous les paramètres modifiables depuis l'inspecteur Unity
    /// </summary>
    [CreateAssetMenu(fileName = "FireflyDanceConfig", menuName = "Motion Party/Firefly Dance/Config")]
    public class FireflyDanceConfig : ScriptableObject
{
    [Header("Limites de la zone de jeu")]
    [SerializeField] private Vector2 topLeft = new Vector2(-2.413f, 3.386f);      // Coin supérieur gauche
    [SerializeField] private Vector2 bottomRight = new Vector2(1.371f, 1.832f);  // Coin inférieur droit
    [SerializeField] private float depth = 2f;                                    // Profondeur de la zone de jeu (axe Z)
    
    [Header("Paramètres des lucioles")]
    [SerializeField] private int maxFireflies = 10;    // Nombre maximum de lucioles simultanées
    [SerializeField] private float maxSpeed = 1f;      // Vitesse maximale de déplacement
    [SerializeField] private float minSpeed = 0.1f;      // Vitesse minimale de déplacement
    
    [Header("Paramètres d'apparition")]
    [SerializeField] private float spawnInterval = 5f;     // Intervalle entre chaque apparition (en secondes)
    [SerializeField] private float fireflyLifetime = 50f;  // Durée de vie d'une luciole (en secondes)
    
    [Header("Paramètres de gameplay")]
    [SerializeField] private float captureRadius = 1f;   // Rayon de capture d'une luciole
    [SerializeField] private int scorePerFirefly = 10;   // Points gagnés par luciole capturée (legacy)
    [SerializeField] private float gameDuration = 60f;   // Durée de la partie en secondes
    
    [Header("Types de libellules")]
    [SerializeField] private float staticFireflyProbability = 0.3f;     // Probabilité d'apparition des libellules statiques (30%)
    [SerializeField] private float slowMovingProbability = 0.4f;        // Probabilité des libellules lentes (40%)
    [SerializeField] private float fastMovingProbability = 0.3f;        // Probabilité des libellules rapides (30%)
    
    [Header("Scores par type")]
    [SerializeField] private int staticFireflyScore = 5;      // Score pour libellules statiques
    [SerializeField] private int slowMovingScore = 10;        // Score pour libellules lentes  
    [SerializeField] private int fastMovingScore = 15;        // Score pour libellules rapides
    
    // Propriétés publiques pour l'accès en lecture seule
    public Vector2 TopLeft => topLeft;
    public Vector2 BottomRight => bottomRight;
    public float Depth => depth;
    public int MaxFireflies => maxFireflies;
    public float MaxSpeed => maxSpeed;
    public float MinSpeed => minSpeed;
    public float SpawnInterval => spawnInterval;
    public float FireflyLifetime => fireflyLifetime;
    public float CaptureRadius => captureRadius;
    public int ScorePerFirefly => scorePerFirefly;
    public float GameDuration => gameDuration;
    
    // Propriétés pour les types de libellules
    public float StaticFireflyProbability => staticFireflyProbability;
    public float SlowMovingProbability => slowMovingProbability;
    public float FastMovingProbability => fastMovingProbability;
    public int StaticFireflyScore => staticFireflyScore;
    public int SlowMovingScore => slowMovingScore;
    public int FastMovingScore => fastMovingScore;
    
    // Propriétés additionnelles pour FireflyController
    public float MinFireflyLifetime => fireflyLifetime * 0.8f;  // 80% de la durée de vie
    public float MaxFireflyLifetime => fireflyLifetime * 1.2f;  // 120% de la durée de vie
    public float FireflyMoveSpeed => (minSpeed + maxSpeed) * 0.5f;  // Vitesse moyenne
    
    // Propriétés calculées pour faciliter l'utilisation
    public float Width => bottomRight.x - topLeft.x;           // Largeur de la zone de jeu
    public float Height => topLeft.y - bottomRight.y;          // Hauteur de la zone de jeu
    public Vector2 Center => (topLeft + bottomRight) * 0.5f;   // Centre de la zone de jeu
    
    /// <summary>
    /// Vérifie si une position est dans les limites de la zone de jeu
    /// </summary>
    /// <param name="position">Position à vérifier</param>
    /// <returns>True si la position est valide, false sinon</returns>
    public bool IsValidPosition(Vector2 position)
    {
        return position.x >= topLeft.x && position.x <= bottomRight.x &&
               position.y <= topLeft.y && position.y >= bottomRight.y;
    }
    
    /// <summary>
    /// Génère une position aléatoire dans les limites de la zone de jeu
    /// </summary>
    /// <returns>Position aléatoire valide</returns>
    public Vector2 GetRandomPosition()
    {
        return new Vector2(
            Random.Range(topLeft.x, bottomRight.x),
            Random.Range(bottomRight.y, topLeft.y)
        );
    }
    
    /// <summary>
    /// Contraint une position dans les limites de la zone de jeu
    /// </summary>
    /// <param name="position">Position à contraindre</param>
    /// <returns>Position contrainte dans les limites</returns>
    public Vector2 ClampToBounds(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, topLeft.x, bottomRight.x),
            Mathf.Clamp(position.y, bottomRight.y, topLeft.y)
        );
    }
    
    /// <summary>
    /// Sélectionne aléatoirement un type de libellule selon les probabilités configurées
    /// </summary>
    /// <returns>Type de libellule sélectionné</returns>
    public Fireflies.FireflyController.FireflyType GetRandomFireflyType()
    {
        float totalProbability = staticFireflyProbability + slowMovingProbability + fastMovingProbability;
        float randomValue = Random.Range(0f, totalProbability);
        
        if (randomValue < staticFireflyProbability)
        {
            return Fireflies.FireflyController.FireflyType.Static;
        }
        else if (randomValue < staticFireflyProbability + slowMovingProbability)
        {
            return Fireflies.FireflyController.FireflyType.SlowMoving;
        }
        else
        {
            return Fireflies.FireflyController.FireflyType.FastMoving;
        }
    }
    
    /// <summary>
    /// Validation automatique des paramètres dans l'éditeur Unity
    /// </summary>
    private void OnValidate()
    {
        // S'assurer que les limites sont cohérentes
        if (topLeft.x >= bottomRight.x)
            topLeft.x = bottomRight.x - 0.1f;
        if (topLeft.y <= bottomRight.y)
            topLeft.y = bottomRight.y + 0.1f;
            
        // S'assurer que les valeurs sont positives quand nécessaire
        maxFireflies = Mathf.Max(1, maxFireflies);
        maxSpeed = Mathf.Max(0.1f, maxSpeed);
        minSpeed = Mathf.Max(0.1f, minSpeed);
        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        fireflyLifetime = Mathf.Max(1f, fireflyLifetime);
        captureRadius = Mathf.Max(0.1f, captureRadius);
        gameDuration = Mathf.Max(1f, gameDuration);
        depth = Mathf.Max(0.1f, depth);
        
        // S'assurer que la vitesse min n'est pas supérieure à la vitesse max
        if (minSpeed > maxSpeed)
            minSpeed = maxSpeed;
            
        // Valider les probabilités des types de libellules
        staticFireflyProbability = Mathf.Clamp01(staticFireflyProbability);
        slowMovingProbability = Mathf.Clamp01(slowMovingProbability);
        fastMovingProbability = Mathf.Clamp01(fastMovingProbability);
        
        // Valider les scores
        staticFireflyScore = Mathf.Max(1, staticFireflyScore);
        slowMovingScore = Mathf.Max(1, slowMovingScore);
        fastMovingScore = Mathf.Max(1, fastMovingScore);
    }

    /// <summary>
    /// Crée une instance de configuration avec des valeurs par défaut
    /// </summary>
    public static FireflyDanceConfig CreateDefault()
    {
        var config = CreateInstance<FireflyDanceConfig>();
        
        // Les valeurs par défaut sont déjà définies dans les champs [SerializeField]
        // Cette méthode peut être étendue pour personnaliser davantage les valeurs par défaut
        
        return config;
    }

    #region Configuration Helper Methods

    private static FireflyDanceConfig _defaultConfig;
    
    /// <summary>
    /// Obtient une configuration, en créant une par défaut si nécessaire
    /// </summary>
    public static FireflyDanceConfig GetOrCreateConfig()
    {
        // Chercher une config existante dans la scène
        var existingConfig = FindFirstObjectByType<FireflyDanceGameManager>()?.config;
        if (existingConfig != null)
        {
            return existingConfig;
        }
        
        // Utiliser ou créer une config par défaut
        if (_defaultConfig == null)
        {
            _defaultConfig = CreateDefault();
            FireflyDanceLogger.Log("Configuration par défaut créée automatiquement");
        }
        
        return _defaultConfig;
    }
    
    /// <summary>
    /// Assigne automatiquement une configuration à un composant si elle est manquante
    /// </summary>
    public static void EnsureConfig(ref FireflyDanceConfig config, string componentName = "Component")
    {
        if (config == null)
        {
            config = GetOrCreateConfig();
            FireflyDanceLogger.LogVerbose($"{componentName} - Configuration assignée automatiquement");
        }
    }

    #endregion
}
}
