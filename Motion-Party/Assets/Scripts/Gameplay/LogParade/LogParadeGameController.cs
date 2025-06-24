using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System.Collections;

/// <summary>
/// Contrôleur principal du mini-jeu "Le Défilé des Rondins"
/// Gère le tracking latéral du joueur et son mapping sur 4 voies verticales
/// </summary>
public class LogParadeGameController : MiniGameBase
{    [Header("Managers")]
    public UDPReceive udpReceive;
    public LogParadeUIManager uiManager;
    public LogParadeLateralTracker lateralTracker;
    public LogParadePlayerAvatar playerAvatar;
    public LogParadeScoreManager scoreManager;

    [Header("Game Settings")]
    public float startDelay = 1f;
    public bool enableDebugMode = true;

    [Header("Lane Settings")]
    public Transform[] laneMarkers = new Transform[4]; // Positions des 4 voies
    public Material[] laneMaterials = new Material[4]; // Matériaux pour visualiser les voies

    // Game State
    private bool gameStarted = false;
    private bool gameEnded = false;
    private Vector3 currentPlayerPosition;
    private int currentLane = 2; // Commence au centre (1-4)

    protected override void Launch()
    {
        InitGame();
    }    void Start()
    {
        // S'assurer que les composants sont bien assignés
        ValidateComponents();
        
        // Configurer les événements du tracker
        if (lateralTracker != null)
        {
            lateralTracker.OnLaneChanged += HandleLaneChanged;
            lateralTracker.OnPositionUpdated += HandlePositionUpdated;
        }

        // Lancer automatiquement le jeu
        Launch();
    }    void Update()
    {
        // Toujours traiter les données MediaPipe pour permettre le mouvement pendant la calibration
        ProcessMediaPipeData();
        
        // Seule la logique de jeu est bloquée si le jeu n'a pas commencé
        if (gameStarted && !gameEnded)
        {
            UpdateGameLogic();
        }
    }

    private void OnDestroy()
    {
        // Nettoyer les événements
        if (lateralTracker != null)
        {
            lateralTracker.OnLaneChanged -= HandleLaneChanged;
            lateralTracker.OnPositionUpdated -= HandlePositionUpdated;
        }
    }    /// <summary>
    /// Initialise le jeu
    /// </summary>
    private void InitGame()
    {
        Debug.Log("Initialisation du mini-jeu 'Le Défilé des Rondins'...");
        
        gameStarted = false;
        gameEnded = false;
        currentLane = 2;

        // Initialiser l'UI
        if (uiManager != null)
        {
            uiManager.InitializeUI();
        }

        // Initialiser l'avatar au centre
        if (playerAvatar != null)
        {
            playerAvatar.SetLaneInstant(2);
        }

        // Initialiser le système de score
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            Debug.Log("Système de score initialisé et remis à zéro.");
        }

        // Démarrer le jeu après un délai
        StartCoroutine(StartGameAfterDelay());
    }    /// <summary>
    /// Démarre le jeu après un délai
    /// </summary>
    private IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        
        gameStarted = true;
        
        if (uiManager != null)
        {
            uiManager.ShowGameStartMessage();
        }        // Démarrer le système de score automatiquement au lancement du jeu
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
            Debug.Log("🎯 Système de score démarré automatiquement !");
        }
        else
        {
            Debug.LogError("❌ ScoreManager non assigné ! Le score ne sera pas comptabilisé.");
            Debug.LogError("💡 Vérifiez qu'un GameObject avec LogParadeScoreManager existe dans la scène.");
        }

        Debug.Log("Jeu démarré ! Bougez latéralement pour contrôler l'avatar.");
    }

    /// <summary>
    /// Traite les données MediaPipe pour le tracking latéral
    /// </summary>
    private void ProcessMediaPipeData()
    {
        if (udpReceive == null) return;

        string data = udpReceive.data;
        if (string.IsNullOrEmpty(data)) return;

        try
        {
            // Parser les données JSON
            JObject jsonData = JObject.Parse(data);
            
            // Le tracker latéral s'occupe du traitement détaillé
            // Ici on peut ajouter des logs ou des traitements spécifiques au jeu
            
            if (enableDebugMode)
            {
                // Afficher les données brutes en debug si nécessaire
                // Debug.Log($"Données reçues: {data}");
            }
        }
        catch (System.Exception ex)
        {
            if (enableDebugMode)
                Debug.LogWarning($"Erreur lors du parsing des données MediaPipe : {ex.Message}");
        }
    }

    /// <summary>
    /// Met à jour la logique du jeu
    /// </summary>
    private void UpdateGameLogic()
    {
        // Ici, dans les futures versions, on ajoutera :
        // - Génération des rondins
        // - Détection des collisions
        // - Gestion des scores
        // - etc.
        
        // Pour l'instant, on se contente du tracking et de l'avatar
    }

    /// <summary>
    /// Gère les changements de voie détectés par le tracker
    /// </summary>
    private void HandleLaneChanged(int newLane)
    {
        currentLane = newLane;
        
        // Déplacer l'avatar vers la nouvelle voie
        if (playerAvatar != null)
        {
            playerAvatar.SetTargetLane(newLane);
        }

        // Mettre à jour l'UI
        if (uiManager != null)
        {
            uiManager.UpdateCurrentLane(newLane);
        }

        if (enableDebugMode)
        {
            Debug.Log($"Changement de voie : {newLane}");
        }
    }

    /// <summary>
    /// Gère les mises à jour de position du tracker
    /// </summary>
    private void HandlePositionUpdated(Vector3 position)
    {
        currentPlayerPosition = position;
        
        // Mettre à jour l'UI avec la position
        if (uiManager != null)
        {
            uiManager.UpdatePlayerPosition(position);
        }
    }    /// <summary>
    /// Valide que tous les composants nécessaires sont assignés
    /// </summary>
    private void ValidateComponents()
    {
        if (udpReceive == null)
        {
            Debug.LogError("UDPReceive n'est pas assigné dans LogParadeGameController !");
        }

        if (lateralTracker == null)
        {
            lateralTracker = FindObjectOfType<LogParadeLateralTracker>();
            if (lateralTracker == null)
                Debug.LogError("LogParadeLateralTracker non trouvé !");
        }

        if (playerAvatar == null)
        {
            playerAvatar = FindObjectOfType<LogParadePlayerAvatar>();
            if (playerAvatar == null)
                Debug.LogError("LogParadePlayerAvatar non trouvé !");
        }

        if (uiManager == null)
        {
            Debug.LogWarning("LogParadeUIManager n'est pas assigné !");
        }        if (scoreManager == null)
        {
            Debug.Log("Tentative de détection automatique du LogParadeScoreManager...");
            scoreManager = FindObjectOfType<LogParadeScoreManager>();
            
            if (scoreManager == null)
            {
                Debug.LogError("❌ LogParadeScoreManager non trouvé dans la scène ! Le score ne sera pas comptabilisé.");
                Debug.LogError("💡 Solution : Ajoutez un GameObject avec le composant LogParadeScoreManager à votre scène.");
            }
            else
            {
                Debug.Log($"✅ LogParadeScoreManager trouvé automatiquement sur : {scoreManager.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"✅ LogParadeScoreManager déjà assigné sur : {scoreManager.gameObject.name}");
        }
    }

    /// <summary>
    /// Obtient la voie actuelle du joueur
    /// </summary>
    public int GetCurrentLane()
    {
        return currentLane;
    }

    /// <summary>
    /// Obtient la position actuelle du joueur
    /// </summary>
    public Vector3 GetCurrentPlayerPosition()
    {
        return currentPlayerPosition;
    }    /// <summary>
    /// Vérifie si le jeu est en cours
    /// </summary>
    public bool IsGameActive()
    {
        return gameStarted && !gameEnded;
    }

    /// <summary>
    /// Arrête le jeu et le système de score
    /// </summary>
    public void StopGame()
    {
        gameEnded = true;
        gameStarted = false;

        // Arrêter le système de score
        if (scoreManager != null)
        {
            scoreManager.StopScoring();
            Debug.Log($"Jeu terminé ! Score final : {scoreManager.GetCurrentScore()}");
        }

        Debug.Log("Le jeu LogParade a été arrêté.");
    }

    /// <summary>
    /// Force le démarrage immédiat du jeu (pour la calibration)
    /// </summary>
    public void ForceStartGame()
    {
        if (gameStarted)
        {
            Debug.Log("LogParadeGameController: Jeu déjà démarré");
            return;
        }
        
        Debug.Log("LogParadeGameController: Démarrage forcé du jeu après calibration");
        
        // Arrêter la coroutine de délai si elle est en cours
        StopAllCoroutines();
        
        gameStarted = true;
        gameEnded = false;
        
        // Démarrer le score immédiatement
        if (scoreManager != null)
        {
            scoreManager.StartScoring();
            Debug.Log("Score démarré après calibration");
        }        // Initialiser l'UI
        if (uiManager != null)
        {
            uiManager.ShowGameUI();
            Debug.Log("UI de jeu affichée");
        }
        
        Debug.Log("Jeu LogParade forcé avec succès!");
    }
}
