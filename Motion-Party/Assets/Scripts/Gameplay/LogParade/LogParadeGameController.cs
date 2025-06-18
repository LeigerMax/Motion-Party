using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System.Collections;

/// <summary>
/// Contrôleur principal du mini-jeu "Le Défilé des Rondins"
/// Gère le tracking latéral du joueur et son mapping sur 4 voies verticales
/// Version améliorée avec validation temporelle des changements de voie
/// </summary>
public class LogParadeGameController : MiniGameBase
{
    [Header("Managers")]
    public UDPReceive udpReceive;
    public LogParadeUIManager uiManager;
    public LogParadeLateralTracker lateralTracker;
    public LogParadePlayerAvatar playerAvatar;

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
    }

    void Start()
    {
        // S'assurer que les composants sont bien assignés
        ValidateComponents();
          // Configurer les événements du tracker
        if (lateralTracker != null)
        {
            lateralTracker.OnLaneChanged += HandleLaneChanged;
            lateralTracker.OnPositionUpdated += HandlePositionUpdated;
            lateralTracker.OnLaneChangePreview += HandleLaneChangePreview;
            lateralTracker.OnCalibrationStateChanged += HandleCalibrationStateChanged; // Nouveau
        }
    }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        ProcessMediaPipeData();
        UpdateGameLogic();
    }

    private void OnDestroy()
    {        // Nettoyer les événements
        if (lateralTracker != null)
        {
            lateralTracker.OnLaneChanged -= HandleLaneChanged;
            lateralTracker.OnPositionUpdated -= HandlePositionUpdated;
            lateralTracker.OnLaneChangePreview -= HandleLaneChangePreview;
            lateralTracker.OnCalibrationStateChanged -= HandleCalibrationStateChanged; // Nouveau
        }
    }

    /// <summary>
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

        // Démarrer le jeu après le délai
        StartCoroutine(StartGameAfterDelay());
    }

    /// <summary>
    /// Démarre le jeu après un délai
    /// </summary>
    private IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        
        gameStarted = true;
        
        if (uiManager != null)
        {
            uiManager.UpdateGameStatus("Jeu démarré ! Bougez pour changer de voie.");
        }

        Debug.Log("Le Défilé des Rondins démarré !");
    }

    /// <summary>
    /// Traite les données MediaPipe reçues
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
        // - Système de score
        // - Gestion de la difficulté progressive
    }

    /// <summary>
    /// Gère les changements de voie confirmés
    /// </summary>
    private void HandleLaneChanged(int newLane)
    {
        currentLane = newLane;
        
        // Cacher l'interface de validation (changement confirmé)
        if (uiManager != null)
        {
            uiManager.HideLaneValidation();
        }
        
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
            Debug.Log($"Changement de voie confirmé : {newLane}");
        }
    }

    /// <summary>
    /// Gère les mises à jour de position du joueur
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
    /// Gère la prévisualisation de changement de voie (validation en cours)
    /// </summary>
    private void HandleLaneChangePreview(int targetLane)
    {
        if (lateralTracker != null && uiManager != null)
        {
            float progress = lateralTracker.GetValidationProgress();
            uiManager.ShowLaneValidation(targetLane, progress);
            
            if (enableDebugMode)
                Debug.Log($"Prévisualisation changement vers voie {targetLane} : {progress:P0}");
        }
    }

    /// <summary>
    /// Gère les changements d'état de calibration
    /// </summary>
    private void HandleCalibrationStateChanged(bool isCalibrating)
    {
        if (uiManager != null)
        {
            if (isCalibrating)
            {
                uiManager.UpdateGameStatus("🎯 Calibration en cours - Placez-vous au centre !");
            }
            else
            {
                uiManager.UpdateGameStatus("✅ Calibration terminée - Prêt à jouer !");
                
                // Démarrer le message de jeu après calibration
                StartCoroutine(ShowGameStartMessageAfterDelay());
            }
        }
        
        if (enableDebugMode)
        {
            Debug.Log($"État de calibration changé : {(isCalibrating ? "EN COURS" : "TERMINÉE")}");
        }
    }
    
    /// <summary>
    /// Affiche le message de début de jeu après un court délai
    /// </summary>
    private System.Collections.IEnumerator ShowGameStartMessageAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        if (uiManager != null)
        {
            uiManager.ShowGameStartMessage();
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
            uiManager = FindObjectOfType<LogParadeUIManager>();
            if (uiManager == null)
                Debug.LogError("LogParadeUIManager non trouvé !");
        }
        
        // Valider la configuration des lanes manuelles
        var laneVisualizer = FindObjectOfType<LogParadeLaneVisualizer>();
        if (laneVisualizer != null)
        {
            var lanePositions = laneVisualizer.GetAllLanePositions();
            bool allLanesValid = true;
            
            for (int i = 0; i < 4; i++)
            {
                if (lanePositions[i] == Vector3.zero)
                {
                    Debug.LogError($"🚨 Lane manuelle {i + 1} n'est pas configurée correctement !");
                    allLanesValid = false;
                }
            }
            
            if (allLanesValid)
            {
                Debug.Log("✅ Configuration des lanes manuelles validée !");
                float spacing = laneVisualizer.GetLaneSpacing();
                Debug.Log($"📏 Espacement des lanes : {spacing:F2} unités");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ LogParadeLaneVisualizer non trouvé - pas de validation des lanes !");
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
    }

    /// <summary>
    /// Indique si le jeu est actif
    /// </summary>
    public bool IsGameActive()
    {
        return gameStarted && !gameEnded;
    }
}
