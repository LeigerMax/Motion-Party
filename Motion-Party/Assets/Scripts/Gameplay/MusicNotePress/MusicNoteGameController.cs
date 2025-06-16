using UnityEngine;
using Core;
using Newtonsoft.Json.Linq;
using System.Collections;

public class MusicNoteGameController : MiniGameBase
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
    private int openFingers = 0;
    public float delayBetweenLevels = 3f;

    [Header("Game State")]
    private bool gameStarted = false;
    private bool gameEnded = false;
    private bool isPlayingSequence = true;
    private int currentLevel = 1;
    private int noteCountThisLevel = 2;

    private Coroutine playingSequence;

    protected override void Launch()
    {
        InitGame();
    }

    void Start()
    {
        if (noteInputManager != null && noteInputManager.spinner != null)
        {
            noteInputManager.spinner.ValidationTime = validationTime;
        }

        noteInputManager.OnSequenceCompleted += HandleSequenceResult;
        //InitGame();
    }

    private void OnDestroy()
    {
        noteInputManager.OnSequenceCompleted -= HandleSequenceResult;
    }


    // Update is called once per frame
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



    private void InitGame()
    {
        SpeechSystemSingleton.Vosk.StartVoskStt();
        
        Debug.Log("Initialisation du mini-jeu...");

        if (noteInputManager == null)
        {
            Debug.LogError("noteInputManager est NULL !");
            return;
        }

        gameStarted = false;
        gameEnded = false;
        currentLevel = 1;
        noteCountThisLevel = currentLevel + 1;

        noteInputManager.ResetInput();
        LaunchLevel();
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

            if (currentLevel > maxLevel)
            {
                Debug.Log("Jeu terminé ! Tous les niveaux ont été complétés.");
                uiManager.DisplayEndGameScreen(true);
                gameEnded = true;
                FinishMiniGame();
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
            }
        }
    }

    private IEnumerator DelayAndLaunchLevel()
    {
        yield return new WaitForSeconds(delayBetweenLevels);
        LaunchLevel();
    }
    
}
