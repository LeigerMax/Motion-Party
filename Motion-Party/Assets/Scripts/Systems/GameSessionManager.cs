using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Linq;
using Core.Analytics.Core;
using Core.Analytics.Data;
using Core.Audio;
using Systems;

public enum GameState { Intro, Playing, Results }

public class GameSessionManager : MonoBehaviour
{
    [Header("Mini-Games Configuration")]
    [SerializeField] private List<MiniGameInfo> miniGames;
    [SerializeField] private string mainMenuSceneName = "MiniGameManager"; // Nom de la scène du menu principal
    
    [Header("Transition Settings")]
    [SerializeField] private float defaultTransitionDelay = 1f; // Délai par défaut entre les jeux
    [SerializeField] private bool enableDetailedLogs = true; // Logs détaillés pour debug
    [SerializeField] private bool forceDebugMode = false; // Force les logs même si enableDetailedLogs est false
    
    [Header("Analytics Configuration")]
    [SerializeField] private bool enableAnalytics = true; // Par défaut activé
    [SerializeField] private bool showCompletionSummary = true;
    
    private int currentGameIndex = 0;
    
    // Analytics variables
    private string currentSessionId;
    public SessionAnalyzer currentSession;
    private bool sessionActive = false;
    private string currentPlayerID; // ID du joueur actuel

    // Données persistantes pour survivre aux changements de scène
    [System.Serializable]
    private struct PersistentSessionData
    {
        public string sessionId;
        public bool sessionActive;
        public List<string> playerIds;
        public int currentGameIndex;
        public string currentPlayerID;
    }
    
    private static PersistentSessionData savedSessionData;
    private static bool hasSavedData = false;

    // Singleton
    public static GameSessionManager Instance { get; private set; }
    
    /// <summary>
    /// S'assure qu'une instance de GameSessionManager existe
    /// </summary>
    public static GameSessionManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }
        
        // Chercher d'abord l'instance persistante (DontDestroyOnLoad)
        // FindFirstObjectByType ne trouve que dans la scène active, 
        // donc on utilise FindObjectsOfType pour chercher partout
        GameSessionManager[] allInstances = FindObjectsOfType<GameSessionManager>();
        
        if (allInstances.Length > 0)
        {
            // Prendre la première instance trouvée (devrait être la persistante)
            Instance = allInstances[0];
            Debug.Log("[GameSessionManager] Instance persistante trouvée");
            return Instance;
        }
        
        // Aucune instance trouvée, en créer une nouvelle
        GameObject go = new GameObject("GameSessionManager");
        Instance = go.AddComponent<GameSessionManager>();
        
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(go);
        }
        
        Debug.Log("[GameSessionManager] Instance créée automatiquement");
        
        // Restaurer les données sauvegardées si disponibles
        if (hasSavedData)
        {
            Instance.RestoreSavedSessionData();
        }
        
        return Instance;
    }
    
    // Analytics Events
    public static event Action<string> OnMiniGameCompleted;
    public static event Action<SessionAnalysisResult> OnSessionCompleted;
    public static event Action<string, SessionAnalyzer> OnSessionStarted;

    private void Awake()
    {
        // Si une instance existe déjà et que c'est pas nous
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[GameSessionManager] Instance déjà existante détectée. Destruction de {gameObject.name}");
            Destroy(this.gameObject);
            return;
        }
        
        // Nous sommes la nouvelle instance
        Instance = this;
        
        // S'assurer que l'objet survit aux changements de scène
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(this.gameObject);
            Debug.Log($"[GameSessionManager] Instance {gameObject.name} configurée comme persistante");
        }
    }

    private void OnDestroy()
    {
        // Si nous sommes l'instance principale qui est détruite
        if (Instance == this)
        {
            Debug.Log("[GameSessionManager] Instance principale détruite - Sauvegarde des données de session");
            
            // Sauvegarder les données de session avant destruction
            SaveSessionDataBeforeDestruction();
            
            // Sauvegarder une dernière fois la configuration avant destruction
            if (miniGames != null && miniGames.Count > 0)
            {
                SaveMiniGamesConfiguration();
            }
            
            // Déconnexion des événements GamePlayerSelector
            if (Systems.GamePlayerSelector.Instance != null)
            {
                Systems.GamePlayerSelector.Instance.OnPlayersSelected -= OnPlayersSelected;
            }
            
            Instance = null;
            OnMiniGameCompleted = null;
            OnSessionCompleted = null;
        }
    }

    /// <summary>
    /// Sauvegarde les données de session avant destruction pour les restaurer plus tard
    /// </summary>
    private void SaveSessionDataBeforeDestruction()
    {
        if (sessionActive && !string.IsNullOrEmpty(currentSessionId))
        {
            savedSessionData = new PersistentSessionData
            {
                sessionId = currentSessionId,
                sessionActive = sessionActive,
                playerIds = currentSession?.GetAllPlayers() ?? new List<string>(),
                currentGameIndex = currentGameIndex,
                currentPlayerID = currentPlayerID
            };
            hasSavedData = true;
            
            Debug.Log($"[GameSessionManager] Données de session sauvegardées - Session: {currentSessionId}, Joueurs: {savedSessionData.playerIds.Count}");
        }
        else
        {
            hasSavedData = false;
            Debug.Log("[GameSessionManager] Aucune session active à sauvegarder");
        }
    }

    /// <summary>
    /// Restaure les données de session sauvegardées après recréation de l'instance
    /// </summary>
    private void RestoreSavedSessionData()
    {
        if (!hasSavedData)
        {
            Debug.Log("[GameSessionManager] Aucune donnée sauvegardée à restaurer");
            return;
        }

        Debug.Log($"[GameSessionManager] Restauration des données de session - Session: {savedSessionData.sessionId}");
        
        // Restaurer les variables de base
        currentSessionId = savedSessionData.sessionId;
        sessionActive = savedSessionData.sessionActive;
        currentGameIndex = savedSessionData.currentGameIndex;
        currentPlayerID = savedSessionData.currentPlayerID;
        
        // Recréer la session analytics SEULEMENT si elle n'existe pas déjà
        if (sessionActive && !string.IsNullOrEmpty(currentSessionId))
        {
            // Si currentSession est null, la recréer
            if (currentSession == null)
            {
                currentSession = new SessionAnalyzer(currentSessionId);
                Debug.Log($"[GameSessionManager] Session analytics recréée : {currentSessionId}");
            }
            else
            {
                Debug.Log($"[GameSessionManager] Session analytics existante conservée : {currentSessionId}");
            }
            
            // Vérifier si les joueurs sont déjà présents dans la session
            var existingPlayers = currentSession.GetAllPlayers();
            
            // Ré-ajouter seulement les joueurs manquants
            foreach (string playerId in savedSessionData.playerIds)
            {
                if (!existingPlayers.Contains(playerId))
                {
                    currentSession.AddPlayer(playerId);
                    Debug.Log($"[GameSessionManager] Joueur {playerId} restauré dans la session");
                }
                else
                {
                    Debug.Log($"[GameSessionManager] Joueur {playerId} déjà présent dans la session");
                }
            }
            
            Debug.Log($"[GameSessionManager] Session {currentSessionId} restaurée avec {savedSessionData.playerIds.Count} joueur(s)");
            
            // Déclencher l'événement de session restaurée
            OnSessionStarted?.Invoke(currentSessionId, currentSession);
        }
        
        // Marquer les données comme utilisées
        hasSavedData = false;
    }

    private void Start()
    {
        // Debug: Vérifier l'état des analytics
        LogWithContext($"Analytics activés: {enableAnalytics}", true);
        LogWithContext($"Instance GameSessionManager: {Instance != null}", true);
        LogWithContext($"GameObject name: {gameObject.name}", true);
        LogWithContext($"Mini-jeux configurés: {miniGames?.Count ?? 0}", true);
        
        // Note: La musique est maintenant gérée manuellement sur chaque scène
        if (IsInMainMenuScene())
        {
            LogWithContext("Scene détectée comme menu principal", true);
        }
        else
        {
            LogWithContext("Scene détectée comme mini-jeu", true);
        }
        
        // Essayer de restaurer la configuration si elle est vide
        if (miniGames == null || miniGames.Count == 0)
        {
            LogWithContext("Tentative de restauration depuis la sauvegarde...", true);
            LoadMiniGamesConfiguration();
        }
        
        // Vérifier et initialiser les mini-jeux si nécessaire
        EnsureMiniGamesConfiguration();
        
        // Sauvegarder la configuration actuelle
        SaveMiniGamesConfiguration();
        
        // S'abonner aux événements de sélection de joueurs
        SubscribeToPlayerSelectionEvents();
        
        // NE PAS démarrer automatiquement une session analytics ici
        // La session ne démarre que quand une partie commence réellement
        LogWithContext("💡 Session analytics sera créée au début d'une partie, pas au lancement du jeu", true);
        
        // Vérifier le résultat
        LogWithContext($"GameSessionManager prêt - Session analytics: {IsAnalyticsSessionActive()}", true);
        
        // Démarrer la surveillance continue
        StartCoroutine(WatchSessionHealth());
    }

    /// <summary>
    /// S'abonne aux événements du GamePlayerSelector
    /// </summary>
    private void SubscribeToPlayerSelectionEvents()
    {
        if (Systems.GamePlayerSelector.Instance != null)
        {
            // S'abonner à l'événement de sélection de joueurs
            Systems.GamePlayerSelector.Instance.OnPlayersSelected -= OnPlayersSelected;
            Systems.GamePlayerSelector.Instance.OnPlayersSelected += OnPlayersSelected;
            
            LogWithContext("Abonnement aux événements de sélection de joueurs effectué", true);
        }
        else
        {
            LogWithContext("GamePlayerSelector non trouvé - Pas d'intégration automatique des joueurs", true);
        }
    }

    /// <summary>
    /// Gestionnaire d'événement quand des joueurs sont sélectionnés
    /// </summary>
    private void OnPlayersSelected(List<PlayerData> players)
    {
        LogWithContext($"Joueurs sélectionnés détectés: {players.Count}", true);
        
        if (enableAnalytics && IsAnalyticsSessionActive())
        {
            foreach (var player in players)
            {
                AddPlayerToAnalyticsSession(player.Id);
                LogWithContext($"Joueur {player.Id} ({player.Nickname}) ajouté à la session analytics", true);
            }
        }
    }

    /// <summary>
    /// Ajoute les joueurs déjà sélectionnés dans GamePlayerSelector
    /// </summary>
    private void AddExistingSelectedPlayers()
    {
        if (Systems.GamePlayerSelector.Instance != null && enableAnalytics && IsAnalyticsSessionActive())
        {
            var existingPlayers = Systems.GamePlayerSelector.Instance.SelectedPlayers;
            if (existingPlayers.Count > 0)
            {
                LogWithContext($"Ajout des joueurs déjà sélectionnés: {existingPlayers.Count}", true);
                
                foreach (var player in existingPlayers)
                {
                    AddPlayerToAnalyticsSession(player.Id);
                    LogWithContext($"Joueur existant {player.Id} ({player.Nickname}) ajouté à la session analytics", true);
                }
            }
        }
    }

    /// <summary>
    /// S'assure que la configuration des mini-jeux est présente
    /// </summary>
    private void EnsureMiniGamesConfiguration()
    {
        if (miniGames == null || miniGames.Count == 0)
        {
            LogWithContext("⚠️ Liste de mini-jeux vide - Restauration de la configuration par défaut", true);
            
            miniGames = new List<MiniGameInfo>();
            
            // Configuration par défaut basée sur les vraies scènes Motion Party
            var musicGame = new MiniGameInfo();
            musicGame.sceneName = "MiniGame_MusicNote";
            musicGame.displayName = "Music Note Press";
            musicGame.description = "Jeu de rythme musical";
            musicGame.loadingTipId = "music_tips";
            
            var logGame = new MiniGameInfo();
            logGame.sceneName = "MiniGame_LogParade";
            logGame.displayName = "Log Parade";
            logGame.description = "Navigation sur rondins";
            logGame.loadingTipId = "log_tips";
            
            var fireflyGame = new MiniGameInfo();
            fireflyGame.sceneName = "MiniGame_FireFlyDance";
            fireflyGame.displayName = "Firefly Dance";
            fireflyGame.description = "Capture des lucioles";
            fireflyGame.loadingTipId = "firefly_tips";
            
            miniGames.Add(musicGame);
            miniGames.Add(logGame);
            miniGames.Add(fireflyGame);
            
            LogWithContext($"Configuration par défaut restaurée avec {miniGames.Count} mini-jeu(s)", true);
            
            // Sauvegarder cette configuration pour éviter de la perdre
            SaveMiniGamesConfiguration();
        }
        else
        {
            LogWithContext($"Configuration existante trouvée avec {miniGames.Count} mini-jeu(s)", true);
            
            // Vérifier que les scènes sont bien configurées
            for (int i = 0; i < miniGames.Count; i++)
            {
                var game = miniGames[i];
                if (string.IsNullOrEmpty(game.sceneName))
                {
                    LogWithContext($"⚠️ Mini-jeu {i} n'a pas de nom de scène défini", true);
                }
                else
                {
                    LogWithContext($"Mini-jeu {i}: {game.sceneName} ({game.displayName})", enableDetailedLogs);
                }
            }
        }
    }

    /// <summary>
    /// Sauvegarde la configuration des mini-jeux pour éviter de la perdre
    /// </summary>
    private void SaveMiniGamesConfiguration()
    {
        if (miniGames != null && miniGames.Count > 0)
        {
            // Sauvegarder dans PlayerPrefs comme backup
            PlayerPrefs.SetInt("MiniGames_Count", miniGames.Count);
            
            for (int i = 0; i < miniGames.Count; i++)
            {
                var game = miniGames[i];
                PlayerPrefs.SetString($"MiniGame_{i}_SceneName", game.sceneName ?? "");
                PlayerPrefs.SetString($"MiniGame_{i}_DisplayName", game.displayName ?? "");
                PlayerPrefs.SetString($"MiniGame_{i}_Description", game.description ?? "");
                PlayerPrefs.SetString($"MiniGame_{i}_LoadingTipId", game.loadingTipId ?? "");
            }
            
            PlayerPrefs.Save();
            LogWithContext("Configuration des mini-jeux sauvegardée", enableDetailedLogs);
        }
    }

    /// <summary>
    /// Restaure la configuration des mini-jeux depuis la sauvegarde
    /// </summary>
    private void LoadMiniGamesConfiguration()
    {
        int count = PlayerPrefs.GetInt("MiniGames_Count", 0);
        
        if (count > 0)
        {
            LogWithContext($"Restauration de {count} mini-jeux depuis la sauvegarde", true);
            
            miniGames = new List<MiniGameInfo>();
            
            for (int i = 0; i < count; i++)
            {
                var game = new MiniGameInfo();
                game.sceneName = PlayerPrefs.GetString($"MiniGame_{i}_SceneName", "");
                game.displayName = PlayerPrefs.GetString($"MiniGame_{i}_DisplayName", "");
                game.description = PlayerPrefs.GetString($"MiniGame_{i}_Description", "");
                game.loadingTipId = PlayerPrefs.GetString($"MiniGame_{i}_LoadingTipId", "");
                
                if (!string.IsNullOrEmpty(game.sceneName))
                {
                    miniGames.Add(game);
                    LogWithContext($"Mini-jeu restauré: {game.sceneName} ({game.displayName})", enableDetailedLogs);
                }
            }
        }
    }

    /// <summary>
    /// Surveille la santé de la session analytics (sans recréation automatique)
    /// </summary>
    private IEnumerator WatchSessionHealth()
    {
        while (Application.isPlaying)
        {
            yield return new WaitForSeconds(5f); // Vérifier toutes les 5 secondes
            
            // Juste reporter le statut, ne pas recréer automatiquement
            if (enableAnalytics)
            {
                bool isActive = IsAnalyticsSessionActive();
                if (!isActive)
                {
                    LogWithContext("⚠️ Session analytics inactive détectée (pas de recréation automatique)", true);
                }
                else
                {
                    LogWithContext($"✅ Session analytics active: {currentSessionId}", enableDetailedLogs);
                }
            }
        }
    }
    
    /// <summary>
    /// Finalise les fichiers de session avec les données complètes
    /// </summary>
    private void FinalizeSessionFiles(SessionAnalysisResult finalAnalysis)
    {
        if (AnalyticsFileGenerator.Instance != null)
        {
            try
            {
                AnalyticsFileGenerator.Instance.FinalizeSessionFiles(currentSessionId, finalAnalysis);
                // Finaliser le JSON incrémental et générer le rapport HTML final
                AnalyticsFileGenerator.Instance.FinalizeIncrementalSession(currentSessionId, currentSession);
                LogWithContext("📁 Fichiers de session et JSON incrémental finalisés avec rapport HTML", enableDetailedLogs);
            }
            catch (System.Exception e)
            {
                LogWithContext($"❌ Erreur lors de la finalisation des fichiers: {e.Message}", true);
            }
        }
    }

    /// <summary>
    /// Méthode publique pour déclencher la transition vers le mini-jeu suivant avec un délai optionnel
    /// Appelée par les GameManager via OnTransitionToNextMiniGame
    /// Cette méthode force le retour à la scène principale pour utiliser le LoadingScreenManager
    /// </summary>
    public void TriggerNextMiniGameTransition(float delay = -1f)
    {
        if (delay < 0) delay = defaultTransitionDelay;
        
        LogWithContext($"Transition déclenchée vers le mini-jeu suivant avec délai de {delay}s", true);
        
        // Utiliser la méthode commune pour la transition
        TriggerTransitionToNextGame(delay);
    }

    /// <summary>
    /// Charge directement le mini-jeu suivant avec l'écran de chargement (Option A)
    /// Méthode utilisée par tous les mini-jeux pour une transition directe
    /// </summary>
    public void LoadNextMiniGameWithLoadingScreen()
    {
        LogWithContext("*** LoadNextMiniGameWithLoadingScreen appelée - OPTION A ***", true);
        LogWithContext($"Index actuel AVANT incrément: {currentGameIndex}, Total mini-jeux: {miniGames?.Count ?? 0}", true);
        
        // Vérifier que la liste de mini-jeux est configurée
        if (miniGames == null || miniGames.Count == 0)
        {
            LogWithContext("❌ Aucun mini-jeu configuré - Retour au menu principal", true);
            LogWithContext("Veuillez configurer les mini-jeux dans l'Inspector du GameSessionManager", true);
            
            // Retour direct au menu
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "session_complete", null);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            return;
        }
        
        // S'assurer que le LoadingScreenManager existe
        EnsureLoadingScreenManager();
        
        // Incrémenter pour passer au mini-jeu suivant
        currentGameIndex++;
        LogWithContext($"Index actuel APRÈS incrément: {currentGameIndex}", true);
        
        // Vérifier s'il reste des mini-jeux
        if (currentGameIndex >= miniGames.Count)
        {
            LogWithContext("Tous les mini-jeux ont été joués - Fin de session", true);
            
            // Finaliser la session analytics
            CompleteAnalyticsSession();
            
            // Utiliser la même logique que StartNextMiniGameCoroutine()
            if (LoadingScreenManager.Instance != null)
            {
                LogWithContext($"Retour au menu principal: {mainMenuSceneName}", true);
                LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "session_complete", null);
            }
            else
            {
                LogWithContext("LoadingScreenManager introuvable - Chargement direct vers menu principal", true);
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            return;
        }
        
        var nextMiniGame = miniGames[currentGameIndex];
        LogWithContext($"Chargement du mini-jeu {currentGameIndex + 1}/{miniGames.Count}: {nextMiniGame.sceneName}", true);
        
        // Analytics - marquer le mini-jeu précédent comme complété
        if (currentGameIndex > 0)
        {
            var previousMiniGame = miniGames[currentGameIndex - 1];
            CompleteMiniGameAnalytics(previousMiniGame.sceneName);
        }
        LogWithContext($"*** CHARGEMENT DIRECT du mini-jeu {currentGameIndex + 1}/{miniGames.Count} : {nextMiniGame.sceneName} ***", true);
        
        // Utiliser LoadingScreenManager directement
        if (LoadingScreenManager.Instance != null)
        {
            LogWithContext("*** UTILISATION DIRECTE DE LOADINGSCREENMANAGER ***", true);
            
            string tipId = !string.IsNullOrEmpty(nextMiniGame.loadingTipId) 
                ? nextMiniGame.loadingTipId 
                : "default";
            
            LoadingScreenManager.Instance.ShowAndLoadScene(
                nextMiniGame.sceneName,
                tipId,
                () => { 
                    LogWithContext($"Mini-jeu suivant {nextMiniGame.sceneName} chargé avec succès", true);
                },
                nextMiniGame.displayName,
                nextMiniGame.description
            );
        }
        else
        {
            LogWithContext("*** ERREUR: LoadingScreenManager non disponible après création - Utilisation du fallback ***", true);
            // Fallback : utiliser l'ancienne méthode
            TriggerTransitionToNextGame(2f);
        }
    }

    /// <summary>
    /// S'assure que le LoadingScreenManager existe et est configuré
    /// </summary>
    private void EnsureLoadingScreenManager()
    {
        if (LoadingScreenManager.Instance == null)
        {
            LogWithContext("*** CRÉATION AUTOMATIQUE DU LOADINGSCREENMANAGER ***", true);
            
            // Rechercher d'abord si un LoadingScreenManager existe quelque part
            LoadingScreenManager existingManager = FindFirstObjectByType<LoadingScreenManager>();
            if (existingManager != null)
            {
                LogWithContext("*** LoadingScreenManager trouvé dans la scène ***", true);
                return;
            }
            
            // Essayer de charger le prefab LoadingScreenCanvas depuis Resources
            GameObject loadingScreenPrefab = Resources.Load<GameObject>("LoadingScreenCanvas");
            if (loadingScreenPrefab == null)
            {
                // Essayer un autre chemin
                loadingScreenPrefab = Resources.Load<GameObject>("Prefabs/LoadingScreenCanvas");
            }
            
            if (loadingScreenPrefab != null)
            {
                LogWithContext("*** Instanciation du prefab LoadingScreenCanvas ***", true);
                GameObject loadingScreenInstance = Instantiate(loadingScreenPrefab);
                DontDestroyOnLoad(loadingScreenInstance);
                LogWithContext("*** LoadingScreenCanvas instancié et configuré comme persistant ***", true);
            }
            else
            {
                LogWithContext("*** ERREUR: Impossible de trouver le prefab LoadingScreenCanvas ***", true);
                // Créer un LoadingScreenManager basique
                CreateBasicLoadingScreenManager();
            }
        }
        else
        {
            LogWithContext("*** LoadingScreenManager déjà disponible ***", true);
        }
    }

    /// <summary>
    /// Crée un LoadingScreenManager basique en cas d'urgence
    /// </summary>
    private void CreateBasicLoadingScreenManager()
    {
        LogWithContext("*** Création d'un LoadingScreenManager basique ***", true);
        
        GameObject loadingManagerObj = new GameObject("LoadingScreenManager");
        LoadingScreenManager loadingManager = loadingManagerObj.AddComponent<LoadingScreenManager>();
        
        // Le marquer comme persistant
        DontDestroyOnLoad(loadingManagerObj);
        
        LogWithContext("*** LoadingScreenManager basique créé (sans UI) ***", true);
    }

    /// <summary>
    /// Méthode commune pour gérer les transitions vers le mini-jeu suivant
    /// </summary>
    private void TriggerTransitionToNextGame(float delay)
    {
        // Incrémenter l'index pour le prochain jeu
        currentGameIndex++;
        
        LogWithContext($"Transition vers mini-jeu suivant (index: {currentGameIndex}) avec délai de {delay}s", true);
            
        // Configurer le redirector pour reprendre automatiquement au bon endroit
        GameSessionRedirector.ShouldResumeSession = true;
        GameSessionRedirector.ResumeMiniGameIndex = currentGameIndex - 1; // -1 car ResumeSessionAt va l'incrémenter
        GameSessionRedirector.TransitionDelay = delay;
        
        LogWithContext($"Retour à la scène principale: {mainMenuSceneName} pour transition", true);
        
        // Charger la scène principale
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Reprend la session de mini-jeux à l'index donné (utilisé pour le fallback automatique)
    /// </summary>
    public void ResumeSessionAt(int index)
    {
        if (index < 0 || index >= miniGames.Count)
        {
            Debug.LogWarning($"[GameSessionManager] Index de mini-jeu invalide pour la reprise ({index}), la session recommence au début.");
            StartGameSession();
            return;
        }
        currentGameIndex = index;
        Debug.Log($"[GameSessionManager] Reprise de la session au mini-jeu index {index} : {miniGames[index].sceneName}");
        StartCoroutine(StartNextMiniGameCoroutine());
    }


    /// <summary>
    /// Lance une session de mini-jeux (appelé depuis le menu principal)
    /// C'est ici que la session analytics doit VRAIMENT commencer
    /// </summary>
    public void StartGameSession()
    {
        LogWithContext("🎮 DÉBUT DE PARTIE - Démarrage de la session analytics", true);
        
        // Démarrer la session analytics maintenant qu'une partie commence
        StartNewAnalyticsSession();
        
        // Ajouter les joueurs sélectionnés à la session analytics
        AddExistingSelectedPlayers();
        
        currentGameIndex = 0; // Reset l'index
        StartCoroutine(StartNextMiniGameCoroutine());
    }

    private IEnumerator StartNextMiniGameCoroutine()
    {
        if (currentGameIndex >= miniGames.Count)
        {
            Debug.Log("[GameSessionManager] Tous les mini-jeux sont terminés !");
            // Utiliser l'écran de chargement pour le retour au menu final
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ShowAndLoadScene(mainMenuSceneName, "session_complete", null);
            }
            else
            {
                Debug.LogWarning("[GameSessionManager] LoadingScreenManager introuvable - Chargement direct");
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            yield break;
        }

        var currentMiniGame = miniGames[currentGameIndex];
        string sceneName = currentMiniGame.sceneName;
        string tipId = !string.IsNullOrEmpty(currentMiniGame.loadingTipId) 
            ? currentMiniGame.loadingTipId 
            : "default";

        if (enableDetailedLogs)
            LogWithContext($"Chargement du mini-jeu {currentGameIndex + 1}/{miniGames.Count} : {sceneName}", true);
        
        // Utiliser l'écran de chargement (doit être disponible dans la scène principale)
        if (LoadingScreenManager.Instance != null)
        {
            LogWithContext("Utilisation de LoadingScreenManager pour charger la scène", true);

            bool sceneLoaded = false;

            // Afficher l'écran de chargement et charger la scène avec titre et description
            LoadingScreenManager.Instance.ShowAndLoadScene(
                sceneName,
                tipId,
                () => { 
                    sceneLoaded = true;
                    LogWithContext($"Scène {sceneName} chargée avec succès", true);
                },
                currentMiniGame.displayName,
                currentMiniGame.description
            );

            // Attendre que la scène soit chargée
            yield return new WaitUntil(() => sceneLoaded);
        }
        else
        {
            // Fallback : chargement classique sans écran de chargement
            Debug.LogWarning("[GameSessionManager] LoadingScreenManager non disponible, chargement classique");
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            yield return loadOp;
        }

        yield return null; // attendre une frame que tout soit bien initialisé

        MiniGameBase loadedMiniGame = FindFirstObjectByType<MiniGameBase>();
        if (loadedMiniGame != null)
        {
            LogWithContext($"MiniGameBase trouvé: {loadedMiniGame.GetType().Name} sur {loadedMiniGame.gameObject.name}, actif: {loadedMiniGame.gameObject.activeInHierarchy}", true);
            loadedMiniGame.StartMiniGame(OnMiniGameFinished);
        }
        else
        {
            Debug.LogError("[GameSessionManager] MiniGameBase introuvable dans la scène chargée !");
        }
    }

    /// <summary>
    /// Retourne au menu principal (MiniGameManager)
    /// </summary>
    public void ReturnToMainMenu()
    {
        LogWithContext("Retour au menu principal demandé", true);
        
        try
        {
            // Finaliser la session analytics si active
            if (IsAnalyticsSessionActive())
            {
                CompleteAnalyticsSession();
            }
            // Charger la scène du menu principal
            if (LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.ShowAndLoadScene(
                    "MiniGameManager",
                    "returning_to_menu",
                    () => LogWithContext("Retour au menu principal réussi", true),
                    "Retour au menu",
                    "Merci d'avoir joué !"
                );
            }
            else
            {
                // Fallback : chargement direct
                UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameManager");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSessionManager] Erreur lors du retour au menu: {e.Message}");
            // Essayer le chargement direct en cas d'erreur
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGameManager");
            }
            catch (System.Exception fallbackError)
            {
                Debug.LogError($"[GameSessionManager] Impossible de retourner au menu: {fallbackError.Message}");
                // Essayer avec la première scène du build
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    private void OnMiniGameFinished()
    {
        LogWithContext("Mini-jeu terminé via MiniGameBase.FinishMiniGame()", true);
            
        // Utiliser la méthode commune pour la transition
        TriggerTransitionToNextGame(defaultTransitionDelay);
    }

    /// <summary>
    /// Retourne la liste des noms de scènes de tous les mini-jeux de la session.
    /// </summary>
    public List<string> GetMiniGameSceneNames()
    {
        List<string> names = new List<string>();
        foreach (var mg in miniGames)
        {
            if (mg != null && !string.IsNullOrEmpty(mg.sceneName))
                names.Add(mg.sceneName);
        }
        return names;
    }

    /// <summary>
    /// Méthode pour logger avec contexte
    /// </summary>
    private void LogWithContext(string message, bool forceLog = false)
    {
        if (enableDetailedLogs || forceDebugMode || forceLog)
        {
            //Debug.Log($"[GameSessionManager][Frame:{Time.frameCount}] {message}");
        }
    }

    /// <summary>
    /// Méthode de debug pour afficher l'état actuel
    /// </summary>
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log("=== GAME SESSION MANAGER STATE ===");
        Debug.Log($"Current Game Index: {currentGameIndex}");
        Debug.Log($"Total Mini Games: {miniGames?.Count ?? 0}");
        Debug.Log($"LoadingScreenManager Available: {LoadingScreenManager.Instance != null}");
        Debug.Log($"Active Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Tests de création automatique du LoadingScreenManager
        if (LoadingScreenManager.Instance == null)
        {
            Debug.Log("*** TEST: Tentative de création automatique du LoadingScreenManager ***");
            EnsureLoadingScreenManager();
            Debug.Log($"LoadingScreenManager Available après création: {LoadingScreenManager.Instance != null}");
        }
        
        if (miniGames != null && currentGameIndex < miniGames.Count)
        {
            var current = miniGames[currentGameIndex];
            Debug.Log($"Next Mini Game: {current.sceneName} ({current.displayName})");
        }
        Debug.Log("==================================");
    }

    #region Analytics Integration

    /// <summary>
    /// Démarre une nouvelle session analytics seulement quand une partie commence réellement
    /// UNIQUEMENT si aucune session n'est déjà active
    /// </summary>
    public void StartNewAnalyticsSession()
    {
        LogWithContext($"StartNewAnalyticsSession appelée - enableAnalytics: {enableAnalytics}", true);
        
        if (!enableAnalytics) 
        {
            LogWithContext("Analytics désactivés - pas de session créée", true);
            return;
        }
        
        // Ne pas redémarrer si une session est déjà active et valide
        if (sessionActive && currentSession != null)
        {
            LogWithContext($"⚠️ Session analytics déjà active: {currentSessionId} - Conservation de la session existante", true);
            return;
        }
        
        // Terminer proprement l'ancienne session si elle existe mais est corrompue
        if (sessionActive && currentSession == null)
        {
            LogWithContext("⚠️ Session corrompue détectée - Nettoyage", true);
            sessionActive = false;
        }

        currentSessionId = GenerateSessionId();
        currentSession = new SessionAnalyzer(currentSessionId);
        sessionActive = true;

        // Initialiser les fichiers de session dès le début pour écriture dynamique
        InitializeSessionFiles();

        LogWithContext($"✅ Session analytics démarrée: {currentSessionId}", true);
        LogWithContext($"Session créée avec succès - sessionActive: {sessionActive}, currentSession != null: {currentSession != null}", true);
        
        // Notifier les observateurs qu'une nouvelle session a démarré
        OnSessionStarted?.Invoke(currentSessionId, currentSession);
    }
    
    /// <summary>
    /// Génère un ID de session unique et persistant pour toute la partie
    /// </summary>
    private string GenerateSessionId()
    {
        // Utiliser un format avec timestamp pour garantir l'unicité et la lisibilité
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string randomSuffix = UnityEngine.Random.Range(1000, 9999).ToString();
        return $"SESSION_{timestamp}_{randomSuffix}";
    }
    
    /// <summary>
    /// Initialise les fichiers de session dès le début pour écriture dynamique
    /// </summary>
    private void InitializeSessionFiles()
    {
        if (AnalyticsFileGenerator.Instance != null && currentSession != null)
        {
            AnalyticsFileGenerator.Instance.InitializeSessionFiles(currentSessionId, currentSession);
            // Créer le fichier JSON incrémental
            AnalyticsFileGenerator.Instance.CreateIncrementalSessionJson(currentSessionId, currentSession);
            LogWithContext("📁 Fichiers de session initialisés pour écriture dynamique", enableDetailedLogs);
        }
    }

    /// <summary>
    /// Ajoute un joueur à la session analytics
    /// </summary>
    public void AddPlayerToAnalyticsSession(string playerId)
    {
        if (!enableAnalytics || !sessionActive || currentSession == null)
        {
            LogWithContext($"Impossible d'ajouter le joueur {playerId} - Analytics: {enableAnalytics}, Session active: {sessionActive}, Session null: {currentSession == null}", true);
            return;
        }

        LogWithContext($"Ajout du joueur {playerId} à la session {currentSessionId}. Joueurs actuels avant ajout: {currentSession.GetAllPlayers().Count}", true);
        currentSession.AddPlayer(playerId);
        LogWithContext($"Joueur {playerId} ajouté à la session analytics {currentSessionId}. Total joueurs après ajout: {currentSession.GetAllPlayers().Count}", true);
    }

    /// <summary>
    /// Récupère la liste des joueurs de la session analytics actuelle
    /// </summary>
    public List<string> GetCurrentSessionPlayers()
    {
        if (!enableAnalytics || !sessionActive || currentSession == null)
        {
            return new List<string>();
        }

        return currentSession.GetAllPlayers();
    }

    /// <summary>
    /// Marque un mini-jeu comme complété dans les analytics
    /// </summary>
    public void CompleteMiniGameAnalytics(string miniGameName)
    {
        if (!enableAnalytics || !sessionActive || currentSession == null)
        {
            LogWithContext($"Impossible de marquer {miniGameName} comme complété", enableDetailedLogs);
            return;
        }

        currentSession.MarkMiniGameCompleted(miniGameName);
        OnMiniGameCompleted?.Invoke(miniGameName);
        
        // Écriture dynamique des données après chaque mini-jeu
        UpdateSessionFilesAfterMiniGame(miniGameName);
        
        LogWithContext($"Mini-jeu {miniGameName} marqué comme complété dans les analytics", enableDetailedLogs);
    }
    
    /// <summary>
    /// Met à jour les fichiers de session après chaque mini-jeu
    /// </summary>
    private void UpdateSessionFilesAfterMiniGame(string miniGameName)
    {
        if (AnalyticsFileGenerator.Instance != null && currentSession != null)
        {
            try
            {
                // Mettre à jour les fichiers avec les nouvelles données
                AnalyticsFileGenerator.Instance.UpdateSessionFilesAfterMiniGame(currentSessionId, currentSession, miniGameName);
                
                // Collecter les métriques du mini-jeu pour le JSON incrémental
                var miniGameData = CollectMiniGameMetrics(miniGameName);
                AnalyticsFileGenerator.Instance.UpdateIncrementalSessionJson(currentSessionId, miniGameName, miniGameData);
                
                LogWithContext($"📝 Fichiers et JSON mis à jour après {miniGameName}", enableDetailedLogs);
            }
            catch (System.Exception e)
            {
                LogWithContext($"❌ Erreur lors de la mise à jour des fichiers après {miniGameName}: {e.Message}", true);
            }
        }
    }

    /// <summary>
    /// Collecte les métriques du mini-jeu pour le JSON incrémental
    /// </summary>
    private Dictionary<string, object> CollectMiniGameMetrics(string miniGameName)
    {
        var metrics = new Dictionary<string, object>();
        
        try
        {
            // Ajouter des informations générales
            metrics["completedAt"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            metrics["players"] = currentSession.GetAllPlayers();
            
            // Ajouter des métriques spécifiques au mini-jeu selon le type
            switch (miniGameName.ToLower())
            {
                case "minigame_musicnote":
                case "musicnote":
                    metrics["gameType"] = "MusicNote";
                    metrics["description"] = "Jeu de séquences musicales";
                    break;
                    
                case "minigame_fireflyDance":
                case "fireflydance":
                    metrics["gameType"] = "FireflyDance";
                    metrics["description"] = "Capture de lucioles";
                    break;
                    
                case "minigame_logparade":
                case "logparade":
                    metrics["gameType"] = "LogParade";
                    metrics["description"] = "Équilibre sur rondins";
                    break;
                    
                default:
                    metrics["gameType"] = miniGameName;
                    metrics["description"] = "Mini-jeu Motion Party";
                    break;
            }
        }
        catch (System.Exception e)
        {
            LogWithContext($"❌ Erreur collecte métriques pour {miniGameName}: {e.Message}", enableDetailedLogs);
        }
        
        return metrics;
    }

    /// <summary>
    /// Enregistre un score pour un joueur dans la session analytics
    /// </summary>
    public void RecordPlayerScore(string playerId, string gameId, int score)
    {
        if (!enableAnalytics || !sessionActive || currentSession == null)
        {
            LogWithContext($"Impossible d'enregistrer le score - Analytics: {enableAnalytics}, Session active: {sessionActive}, Session nulle: {currentSession == null}", enableDetailedLogs);
            return;
        }

        try
        {
            // Enregistrer le score dans le SessionAnalyzer
            currentSession.RecordPlayerScore(playerId, gameId, score);
            LogWithContext($"Score enregistré pour {playerId} dans {gameId}: {score}", true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSessionManager] Erreur lors de l'enregistrement du score: {e.Message}");
        }
    }

    /// <summary>
    /// Enregistre une métrique de performance pour un joueur
    /// </summary>
    public void RecordPlayerMetric(string playerId, string gameId, string metricName, float value)
    {
        Debug.Log($"[GameSessionManager] RecordPlayerMetric - enableAnalytics: {enableAnalytics}, sessionActive: {sessionActive}, currentSession != null: {currentSession != null}");
        
        if (!enableAnalytics || !sessionActive || currentSession == null)
        {
            Debug.LogWarning($"[GameSessionManager] Métrique ignorée - enableAnalytics: {enableAnalytics}, sessionActive: {sessionActive}, currentSession: {currentSession != null}");
            return;
        }

        try
        {
            currentSession.RecordPerformanceMetric(playerId, gameId, metricName, value);
            LogWithContext($"Métrique {metricName} enregistrée pour {playerId} dans {gameId}: {value}", enableDetailedLogs);
            
            // Mettre à jour aussi le fichier JSON incrémental du joueur
            if (AnalyticsFileGenerator.Instance != null)
            {
                AnalyticsFileGenerator.Instance.UpdatePlayerJsonIncrementally(currentSessionId, playerId, gameId, metricName, value);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSessionManager] Erreur lors de l'enregistrement de la métrique: {e.Message}");
        }
    }

    /// <summary>
    /// Finalise la session analytics et génère les rapports
    /// </summary>
    public void CompleteAnalyticsSession()
    {
        if (!enableAnalytics || !sessionActive || currentSession == null)
        {
            return;
        }

        LogWithContext("Finalisation de la session analytics...", true);

        // VALIDATION: S'assurer que toutes les métriques obligatoires sont présentes
        ValidateAndCompleteSessionMetrics();

        // Générer l'analyse finale
        var finalAnalysis = currentSession.AnalyzeSession();
        LogWithContext($"Analyse finale générée - Joueurs: {finalAnalysis.playerCount}, Durée: {finalAnalysis.sessionDuration:F1}min", true);
        OnSessionCompleted?.Invoke(finalAnalysis);

        // Générer les fichiers de rapport
        if (AnalyticsFileGenerator.Instance != null)
        {
            LogWithContext("AnalyticsFileGenerator trouvé - Génération du rapport de session...", true);
            AnalyticsFileGenerator.Instance.GenerateSessionReport(finalAnalysis);
            
            // Finaliser les fichiers avec les données complètes
            FinalizeSessionFiles(finalAnalysis);
            
            // Générer les rapports individuels pour chaque joueur
            foreach (string playerId in currentSession.GetAllPlayers())
            {
                var playerAnalysis = currentSession.AnalyzePlayer(playerId);
                if (playerAnalysis != null && !string.IsNullOrEmpty(playerAnalysis.playerId))
                {
                    AnalyticsFileGenerator.Instance.GeneratePlayerReport(playerAnalysis);
                }
            }
            
            LogWithContext("Rapport de session et rapports individuels générés", true);
        }
        else
        {
            Debug.LogError("[GameSessionManager] AnalyticsFileGenerator.Instance est null - Tentative de création...");
            // Forcer la création de l'instance
            try
            {
                var generator = AnalyticsFileGenerator.Instance; // Cela devrait créer l'instance
                if (generator != null)
                {
                    LogWithContext("AnalyticsFileGenerator créé - Génération du rapport...", true);
                    generator.GenerateSessionReport(finalAnalysis);
                }
                else
                {
                    Debug.LogError("[GameSessionManager] Impossible de créer AnalyticsFileGenerator !");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameSessionManager] Erreur lors de la création d'AnalyticsFileGenerator: {e.Message}");
            }
        }

        // Afficher le résumé si configuré
        if (showCompletionSummary)
        {
            ShowCompletionSummary(finalAnalysis);
        }

        // Nettoyer la session
        sessionActive = false;
        currentSession = null;
        currentSessionId = null;

        LogWithContext("Session analytics terminée", true);
    }

    /// <summary>
    /// Valide et complète les métriques manquantes de la session
    /// TODO: Implémenter la validation des métriques quand MetricsValidator sera créé
    /// </summary>
    private void ValidateAndCompleteSessionMetrics()
    {
        LogWithContext("🔍 Validation des métriques de la session...", true);
        
        try
        {
            // TODO: Implémenter la validation quand MetricsValidator sera disponible
            // var validationResults = Core.Analytics.MetricsValidator.ValidateAllActiveSessions();
            
            LogWithContext("✅ Validation des métriques désactivée temporairement", true);
            
            /*
            if (validationResults.Count == 0)
            {
                LogWithContext("✅ Toutes les métriques obligatoires sont présentes", true);
                return;
            }

            LogWithContext($"⚠️ {validationResults.Count} problème(s) de métriques détecté(s)", true);
            Core.Analytics.MetricsValidator.LogValidationReport(validationResults);

            // Compléter automatiquement les métriques manquantes avec des valeurs par défaut
            foreach (var result in validationResults)
            {
                if (!result.IsValid)
                {
                    LogWithContext($"🔧 Correction des métriques manquantes pour {result.PlayerId} dans {result.GameId}", true);
                    Core.Analytics.MetricsValidator.AddMissingMetricsWithDefaults(result.PlayerId, result.GameId);
                }
            }

            LogWithContext("✅ Métriques manquantes complétées avec des valeurs par défaut", true);
            */
        }
        catch (System.Exception e)
        {
            LogWithContext($"❌ Erreur lors de la validation des métriques: {e.Message}", true);
        }
    }

    /// <summary>
    /// Obtient la session analytics actuelle
    /// </summary>
    public SessionAnalyzer GetCurrentAnalyticsSession()
    {
        return currentSession;
    }

    /// <summary>
    /// Vérifie si une session analytics est active
    /// </summary>
    public bool IsAnalyticsSessionActive()
    {
        bool isActive = sessionActive && currentSession != null;
        LogWithContext($"IsAnalyticsSessionActive: sessionActive={sessionActive}, currentSession!=null={currentSession != null}, result={isActive}", enableDetailedLogs);
        return isActive;
    }

    /// <summary>
    /// Obtient les métriques de debug de la session actuelle
    /// </summary>
    public void DebugSessionMetrics()
    {
        if (!sessionActive || currentSession == null)
        {
            Debug.Log("[GameSessionManager] Aucune session active pour le debug");
            return;
        }

        var metrics = currentSession.GetStoredMetrics();
        var players = currentSession.GetAllPlayers();
        
        Debug.Log($"[GameSessionManager] Session Debug:");
        Debug.Log($"  - Session ID: {currentSession.SessionId}");
        Debug.Log($"  - Joueurs: {players.Count} ({string.Join(", ", players)})");
        Debug.Log($"  - Métriques stockées: {metrics.Count}");
        
        foreach (var metric in metrics)
        {
            Debug.Log($"    {metric.Key}: {metric.Value}");
        }
        
        Debug.Log($"  - Mini-jeux terminés: {currentSession.SessionProgress * 100:F1}%");
    }

    /// <summary>
    /// Méthode de test pour générer des données de session avec des données simulées
    /// </summary>
    [ContextMenu("Test Generate Analytics Report")]
    public void TestGenerateAnalyticsReport()
    {
        if (!IsAnalyticsSessionActive())
        {
            Debug.LogWarning("[GameSessionManager] Aucune session active - Création d'une session de test...");
            StartNewAnalyticsSession();
            
            // Ajouter des joueurs de test
            AddPlayerToAnalyticsSession("TestPlayer01");
            
            // Ajouter des données de test
            RecordPlayerScore("TestPlayer01", "TestGame", 100);
            RecordPlayerMetric("TestPlayer01", "TestGame", "accuracy", 0.85f);
            RecordPlayerMetric("TestPlayer01", "TestGame", "reaction_time", 0.5f);
            
            // Marquer un mini-jeu comme terminé
            CompleteMiniGameAnalytics("TestGame");
        }
        
        // Générer le rapport
        LogWithContext("Test de génération de rapport analytics...", true);
        CompleteAnalyticsSession();
    }

    /// <summary>
    /// Affiche le résumé de completion
    /// </summary>
    private void ShowCompletionSummary(SessionAnalysisResult analysis)
    {
        // Créer un UI temporaire pour afficher le résumé
        var summaryGO = new GameObject("SessionSummary");
        var summaryCanvas = summaryGO.AddComponent<Canvas>();
        summaryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        summaryCanvas.sortingOrder = 1000;

        // Ajouter le composant de résumé (maintenant dans Scripts/UI)
        var summaryComponent = summaryGO.AddComponent<UI.SessionSummaryUI>();
        summaryComponent.Initialize(currentSession);

        LogWithContext("Affichage du résumé analytics...", true);
    }

    /// <summary>
    /// Obtient le statut de la session analytics
    /// </summary>
    public SessionStatus GetAnalyticsSessionStatus()
    {
        if (!sessionActive || currentSession == null)
            return SessionStatus.Inactive;

        return new SessionStatus
        {
            isActive = true,
            sessionId = currentSessionId,
            progress = currentSession.SessionProgress,
            isComplete = currentSession.IsSessionComplete,
            playerCount = currentSession.GetAllPlayers().Count,
            remainingGames = currentSession.GetRemainingMiniGames()
        };
    }

    /// <summary>
    /// Force la fin d'une session analytics (en cas d'erreur)
    /// </summary>
    public void ForceEndSession()
    {
        if (sessionActive && currentSession != null)
        {
            LogWithContext("Fin forcée de la session analytics", true);
            
            try
            {
                var partialAnalysis = currentSession.AnalyzeSession();
                OnSessionCompleted?.Invoke(partialAnalysis);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameSessionManager] Erreur lors de l'analyse partielle: {e.Message}");
            }

            sessionActive = false;
            currentSession = null;
            currentSessionId = null;
        }
    }

    #region Configuration Management

    /// <summary>
    /// Nettoie la configuration sauvegardée des mini-jeux
    /// </summary>
    public void ClearSavedConfiguration()
    {
        if (!PlayerPrefs.HasKey("MiniGames_Count"))
        {
            LogWithContext("Aucune configuration sauvegardée à nettoyer", enableDetailedLogs);
            return;
        }

        try
        {
            int count = PlayerPrefs.GetInt("MiniGames_Count", 0);
            PlayerPrefs.DeleteKey("MiniGames_Count");
            
            for (int i = 0; i < count; i++)
            {
                PlayerPrefs.DeleteKey($"MiniGame_{i}_SceneName");
                PlayerPrefs.DeleteKey($"MiniGame_{i}_DisplayName");
                PlayerPrefs.DeleteKey($"MiniGame_{i}_Description");
                PlayerPrefs.DeleteKey($"MiniGame_{i}_LoadingTipId");
            }
            
            PlayerPrefs.Save();
            LogWithContext("Configuration sauvegardée nettoyée", true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameSessionManager] Erreur lors du nettoyage: {e.Message}");
        }
    }

    /// <summary>
    /// Reset complet du GameSessionManager - à utiliser avec précaution
    /// </summary>
    public void ResetGameSession()
    {
        LogWithContext("Reset complet de la session", true);
        
        // Nettoyer la configuration sauvegardée
        ClearSavedConfiguration();
        
        // Reset des variables internes
        currentGameIndex = 0;
        
        // Si nous avons une session analytics active, la terminer proprement
        if (IsAnalyticsSessionActive())
        {
            CompleteAnalyticsSession();
        }
        
        // Réinitialiser les mini-jeux avec la configuration par défaut
        EnsureMiniGamesConfiguration();
        
        LogWithContext("Reset terminé", true);
    }

    #endregion

    /// <summary>
    /// Vérifie si on est actuellement dans la scène du menu principal
    /// </summary>
    private bool IsInMainMenuScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Scènes de menu
        bool isMenuScene = currentSceneName.Contains("Menu") || 
                          currentSceneName.Contains("Main") || 
                          currentSceneName == "MiniGameManager" ||
                          currentSceneName.Contains("Manager");
        
        // Exclure explicitement les scènes de mini-jeux
        bool isMiniGameScene = currentSceneName.Contains("LogParade") ||
                              currentSceneName.Contains("FireflyDance") ||
                              currentSceneName.Contains("MusicNote") ||
                              currentSceneName.Contains("Firefly") ||
                              currentSceneName.Contains("Log") && !currentSceneName.Contains("Manager");
        
        bool result = isMenuScene && !isMiniGameScene;
        
        if (enableDetailedLogs)
        {
            Debug.Log($"[GameSessionManager] Scene: '{currentSceneName}' - IsMenu: {isMenuScene} - IsMiniGame: {isMiniGameScene} - Result: {result}");
        }
        
        return result;
    }

    #endregion
}
