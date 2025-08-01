using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System.Collections;
using System;

namespace Gameplay.MusicNotePress
{
    /// <summary>
    /// Contrôleur principal du mini-jeu MusicNote
    /// Gère la logique du jeu et les états
    /// </summary>
    public class MusicNoteGameController : MonoBehaviour
{
    [Header("Managers")]
    public UDPReceive udpReceive;
    public UIManager uiManager;
    public NoteSequenceManager noteSequenceManager;
    public NoteInputManager noteInputManager;

    [Header("Game Settings")]
    public float startDelay = 1f;
    public float validationTime = 3f;
    public float delayBetweenWaves = 3f;
    
    [Header("Infinite Wave System")]
    public int startingNotesCount = 2; // Nombre de notes pour la première vague
    public int maxNotesPerWave = 10; // Limite maximale de notes par vague
    public bool enableInfiniteWaves = true; // Active le système de vagues infinies
    public InfiniteWavesConfig wavesConfig; // Configuration optionnelle

    [Header("Game State")]
    private bool gameStarted = false;
    private bool gameEnded = false;
    private bool isPlayingSequence = true;
    private int currentWave = 1; // Remplace currentLevel
    private int noteCountThisWave = 2; // Remplace noteCountThisLevel
    private int currentScore = 0;
    private int openFingers = 0;

    private Coroutine playingSequence;

        // Événements
        public event Action<int> OnGameFinished;
        public event Action OnGameStarted;

    void Start()
    {
            ValidateComponents();
            
            // Appliquer la configuration si elle existe
            if (wavesConfig != null)
            {
                wavesConfig.ApplyTo(this);
            }
            
        if (noteInputManager != null && noteInputManager.spinner != null)
        {
            noteInputManager.spinner.ValidationTime = validationTime;
        }

        noteInputManager.OnSequenceCompleted += HandleSequenceResult;
    }

    private void OnDestroy()
        {
            if (noteInputManager != null)
    {
        noteInputManager.OnSequenceCompleted -= HandleSequenceResult;
    }
        }

        private void ValidateComponents()
        {
            if (noteInputManager == null)
            {
                noteInputManager = FindFirstObjectByType<NoteInputManager>();
                if (noteInputManager == null)
                {
                    Debug.LogError("[MusicNoteGameController] NoteInputManager non trouvé !");
                }
            }

            if (noteSequenceManager == null)
            {
                noteSequenceManager = FindFirstObjectByType<NoteSequenceManager>();
                if (noteSequenceManager == null)
                {
                    Debug.LogError("[MusicNoteGameController] NoteSequenceManager non trouvé !");
                }
            }

            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
                if (uiManager == null)
                {
                    Debug.LogError("[MusicNoteGameController] UIManager non trouvé !");
                }
            }

            if (udpReceive == null)
            {
                udpReceive = FindFirstObjectByType<UDPReceive>();
                if (udpReceive == null)
                {
                    Debug.LogError("[MusicNoteGameController] UDPReceive non trouvé !");
                }
            }
        }

    void Update()
    {
        if (!gameStarted || gameEnded) return;

        if (!isPlayingSequence)
        {
            string data = udpReceive.data;
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            int parsedFingers = 0;
            bool validData = false;

            try
            {
                JObject jsonData = JObject.Parse(data);
                JToken fingersToken = jsonData["open_fingers"];
                if (fingersToken != null && fingersToken.Type == JTokenType.Integer)
                {
                    parsedFingers = (int)fingersToken;
                    validData = true;
                }
                else
                {
                    Debug.LogWarning("Champ 'open_fingers' manquant ou invalide dans les données UDP.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Erreur lors du parsing JSON : " + ex.Message);
            }

            if (validData)
            {
                openFingers = parsedFingers;
                uiManager.UpdateFingerCountDisplay(openFingers);
                noteInputManager.UserInput(openFingers);
            }
        }
    }

        /// <summary>
        /// Démarre une nouvelle partie avec le système de vagues infinies
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[MusicNoteGameController] StartGame appelé - Système de vagues infinies activé");
            
            // Réinitialiser l'état pour les vagues infinies
            gameStarted = false;
            gameEnded = false;
            currentWave = 1;
            noteCountThisWave = startingNotesCount;
            currentScore = 0;

            Debug.Log($"[MusicNoteGameController] Début des vagues infinies - Vague 1 avec {noteCountThisWave} notes");

            // Initialiser le jeu
            InitGame();
        }

    private void InitGame()
    {
        Debug.Log($"[MusicNoteGameController] InitGame() appelé sur {gameObject.name}, actif: {gameObject.activeInHierarchy}");
        SpeechSystemSingleton.Vosk.StartVoskStt();
        
        Debug.Log("Initialisation du mini-jeu...");

        if (noteInputManager == null)
        {
            Debug.LogError("noteInputManager est NULL !");
            return;
        }

        noteInputManager.ResetInput();
        LaunchWave(); // Utiliser LaunchWave au lieu de LaunchLevel

            // Notifier que le jeu a démarré
            OnGameStarted?.Invoke();
    }

    private void LaunchWave()
    {
        Debug.Log($"[MusicNoteGameController] Vague {currentWave} lancée avec {noteCountThisWave} notes");
        gameStarted = true;
        
        // Mettre à jour l'UI avec les informations de la vague
        if (uiManager != null)
        {
            uiManager.UpdateWaveDisplay(currentWave, noteCountThisWave, currentScore);
            uiManager.ShowWaveProgress($"Préparez-vous pour la vague {currentWave} !");
        }
        
        noteSequenceManager.LoadSequence(noteCountThisWave);

        if (playingSequence != null)
            StopCoroutine(playingSequence);

        playingSequence = StartCoroutine(StartWaveAfterDelay());
    }

    private IEnumerator StartWaveAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        isPlayingSequence = true;
        noteInputManager.isPlayingSequence = isPlayingSequence;

        Debug.Log("Lancement de la séquence !");
        yield return noteSequenceManager.PlaySequenceCoroutine();

        isPlayingSequence = false;
        noteInputManager.isPlayingSequence = isPlayingSequence;

        noteInputManager.PrepareExpectedSequence(noteSequenceManager.GetGeneratedNotes());
        Debug.Log("Tour du joueur, prêt à recevoir les inputs.");
    }

    private void HandleSequenceResult(bool success)
    {
        if (success)
        {
            Debug.Log($"[MusicNoteGameController] Vague {currentWave} réussie !");
            currentScore += 100 * currentWave; // Score basé sur la vague
            
            // Mettre à jour l'UI avec le nouveau score
            if (uiManager != null)
            {
                uiManager.UpdateWaveDisplay(currentWave, noteCountThisWave, currentScore);
                uiManager.ShowWaveProgress($"Vague {currentWave} réussie ! +{100 * currentWave} points");
            }
            
            if (enableInfiniteWaves)
            {
                // Système de vagues infinies : progression continue
                currentWave++;
                
                // Utiliser la configuration si disponible, sinon la logique par défaut
                if (wavesConfig != null)
                {
                    noteCountThisWave = wavesConfig.GetNotesCountForWave(currentWave);
                    currentScore += wavesConfig.GetScoreForWave(currentWave - 1); // Score de la vague précédente
                }
                else
                {
                    noteCountThisWave = Mathf.Min(startingNotesCount + (currentWave - 1), maxNotesPerWave);
                }
                
                Debug.Log($"[MusicNoteGameController] Progression vers la vague {currentWave} avec {noteCountThisWave} notes");
                StartCoroutine(DelayAndLaunchNextWave());
            }
            else
            {
                // Ancien système : fin après quelques niveaux
                Debug.Log("Jeu terminé ! Mode classique complété.");
                uiManager.DisplayEndGameScreen(true);
                gameEnded = true;
                OnGameFinished?.Invoke(currentScore);
            }
        }
        else
        {
            // En cas d'échec : FIN DU JEU (système de vagues infinies)
            Debug.Log($"[MusicNoteGameController] Échec à la vague {currentWave} - Fin du jeu");
            Debug.Log($"[MusicNoteGameController] Score final : {currentScore} points - Vagues complétées : {currentWave - 1}");
            
            // Mettre à jour l'UI avec les résultats finaux
            if (uiManager != null)
            {
                uiManager.UpdateWaveDisplay(currentWave, noteCountThisWave, currentScore);
                uiManager.ShowWaveProgress($"Échec ! Score final: {currentScore} points, {currentWave - 1} vagues complétées");
            }
            
            uiManager.DisplayEndGameScreen(false);
            gameEnded = true;
            OnGameFinished?.Invoke(currentScore);
        }
    }

    private IEnumerator DelayAndLaunchNextWave()
    {
        Debug.Log($"[MusicNoteGameController] Préparation de la vague {currentWave} dans {delayBetweenWaves} secondes...");
        
        // Afficher un message de préparation
        if (uiManager != null)
        {
            uiManager.ShowWaveProgress($"Vague {currentWave} dans {delayBetweenWaves} secondes...");
        }
        
        yield return new WaitForSeconds(delayBetweenWaves);
        LaunchWave();
    }
    
        /// <summary>
        /// Réinitialise le jeu à son état initial pour les vagues infinies
        /// </summary>
        public void ResetGame()
        {
            gameStarted = false;
            gameEnded = false;
            currentWave = 1;
            noteCountThisWave = startingNotesCount;
            currentScore = 0;
            
            Debug.Log("[MusicNoteGameController] Jeu réinitialisé pour les vagues infinies");
            
            if (noteInputManager != null)
            {
                noteInputManager.ResetInput();
            }
        }
        
        /// <summary>
        /// Obtient le numéro de la vague actuelle
        /// </summary>
        public int GetCurrentWave()
        {
            return currentWave;
        }
        
        /// <summary>
        /// Obtient le nombre de notes dans la vague actuelle
        /// </summary>
        public int GetCurrentWaveNotesCount()
        {
            return noteCountThisWave;
        }
        
        /// <summary>
        /// Obtient le score actuel
        /// </summary>
        public int GetCurrentScore()
        {
            return currentScore;
        }
        
        /// <summary>
        /// Vérifie si le système de vagues infinies est activé
        /// </summary>
        public bool IsInfiniteWavesEnabled()
        {
            return enableInfiniteWaves;
        }
    }
}