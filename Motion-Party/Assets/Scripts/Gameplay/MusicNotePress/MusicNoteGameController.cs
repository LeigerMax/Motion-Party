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
    public int maxLevel = 3;
    public float validationTime = 3f;
    public float delayBetweenLevels = 3f;

    [Header("Game State")]
    private bool gameStarted = false;
    private bool gameEnded = false;
    private bool isPlayingSequence = true;
    private int currentLevel = 1;
    private int noteCountThisLevel = 2;
        private int currentScore = 0;
        private int openFingers = 0;

    private Coroutine playingSequence;

        // Événements
        public event Action<int> OnGameFinished;
        public event Action OnGameStarted;

    void Start()
    {
            ValidateComponents();
            
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
        /// Démarre une nouvelle partie
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[MusicNoteGameController] StartGame appelé");
            
            // Réinitialiser l'état
            gameStarted = false;
            gameEnded = false;
            currentLevel = 1;
            noteCountThisLevel = 2;
            currentScore = 0;

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
        LaunchLevel();

            // Notifier que le jeu a démarré
            OnGameStarted?.Invoke();
    }

    private void LaunchLevel()
    {
        Debug.Log($"Niveau {currentLevel} lancé avec {noteCountThisLevel} notes.");
        gameStarted = true;
        noteSequenceManager.LoadSequence(noteCountThisLevel);

        if (playingSequence != null)
            StopCoroutine(playingSequence);

        playingSequence = StartCoroutine(StartGameAfterDelay());
    }

    private IEnumerator StartGameAfterDelay()
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
            Debug.Log("Séquence réussie, passage au niveau suivant.");
            currentLevel++;
                currentScore += 100 * currentLevel; // Score basé sur le niveau

            if (currentLevel > maxLevel)
            {
                Debug.Log("Jeu terminé ! Tous les niveaux ont été complétés.");
                uiManager.DisplayEndGameScreen(true);
                gameEnded = true;
                    OnGameFinished?.Invoke(currentScore);
                return;
            }

            noteCountThisLevel = currentLevel + 1;
            StartCoroutine(DelayAndLaunchLevel());
        }
        else
        {
            if (currentLevel < maxLevel)
            {
                Debug.Log("Séquence incorrecte. Relance du niveau.");
                StartCoroutine(DelayAndLaunchLevel());
            }
            else
            {
                Debug.Log("Séquence incorrecte au dernier niveau. Fin du jeu.");
                uiManager.DisplayEndGameScreen(false);
                gameEnded = true;
                    OnGameFinished?.Invoke(currentScore);
            }
        }
    }

    private IEnumerator DelayAndLaunchLevel()
    {
        yield return new WaitForSeconds(delayBetweenLevels);
        LaunchLevel();
    }
    
        /// <summary>
        /// Réinitialise le jeu à son état initial
        /// </summary>
        public void ResetGame()
        {
            gameStarted = false;
            gameEnded = false;
            currentLevel = 1;
            noteCountThisLevel = 2;
            currentScore = 0;
            
            if (noteInputManager != null)
            {
                noteInputManager.ResetInput();
            }
        }
    }
}