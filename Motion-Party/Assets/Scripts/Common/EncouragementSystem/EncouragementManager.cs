using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

/// <summary>
/// Gestionnaire de messages d'encouragement pour les mini-jeux
/// Affiche des messages aléatoires avec audio pendant la partie
/// </summary>
public class EncouragementManager : MonoBehaviour
{
    #region Configuration
    [Header("Configuration")]
    [SerializeField] private EncouragementConfiguration configuration;
    
    [Header("Configuration manuelle (si pas de ScriptableObject)")]
    [SerializeField] private List<EncouragementMessage> encouragementMessages = new List<EncouragementMessage>();
    
    [Header("Timing")]
    [SerializeField] private float minInterval = 15f;
    [SerializeField] private float maxInterval = 20f;
    [SerializeField] private float defaultDisplayDuration = 3f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool playAudioWithMessage = true;
    [SerializeField] [Range(0f, 2f)] private float audioVolume = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    #endregion

    #region Private Fields
    private bool isSystemActive = false;
    private Coroutine encouragementCoroutine;
    private string lastMessageId = "";
    private List<EncouragementMessage> availableMessages = new List<EncouragementMessage>();
    private EncouragementDisplay displayComponent;
    #endregion

    #region Events
    public System.Action<string> OnMessageDisplayed;
    public System.Action OnMessageHidden;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        // Configuration de l'AudioSource
        SetupAudioSource();
        
        // Recherche du composant d'affichage
        displayComponent = GetComponent<EncouragementDisplay>();
        if (displayComponent == null)
        {
            displayComponent = gameObject.AddComponent<EncouragementDisplay>();
        }
        
        // Charger la configuration si disponible
        LoadConfiguration();
        
        // Initialisation des messages par défaut si la liste est vide
        if (encouragementMessages.Count == 0)
        {
            InitializeDefaultMessages();
        }
    }

    void Start()
    {
        // Copie de la liste des messages disponibles
        RefreshAvailableMessages();
    }
    #endregion

    #region Public API
    /// <summary>
    /// Démarre le système d'encouragement
    /// </summary>
    public void StartEncouragement()
    {
        if (isSystemActive)
        {
            DebugLog("Système d'encouragement déjà actif");
            return;
        }

        isSystemActive = true;
        encouragementCoroutine = StartCoroutine(EncouragementLoop());
        DebugLog("Système d'encouragement démarré");
    }

    /// <summary>
    /// Arrête le système d'encouragement
    /// </summary>
    public void StopEncouragement()
    {
        if (!isSystemActive) return;

        isSystemActive = false;
        
        if (encouragementCoroutine != null)
        {
            StopCoroutine(encouragementCoroutine);
            encouragementCoroutine = null;
        }
        
        // Masquer le message actuel s'il y en a un
        displayComponent?.HideMessage();
        
        DebugLog("Système d'encouragement arrêté");
    }

    /// <summary>
    /// Affiche immédiatement un message d'encouragement aléatoire
    /// </summary>
    public void ShowRandomMessage()
    {
        var message = GetRandomMessage();
        if (message != null)
        {
            ShowMessage(message);
        }
    }

    /// <summary>
    /// Ajoute un nouveau message d'encouragement
    /// </summary>
    public void AddMessage(EncouragementMessage message)
    {
        if (message != null && !encouragementMessages.Contains(message))
        {
            encouragementMessages.Add(message);
            RefreshAvailableMessages();
            DebugLog($"Message ajouté: {message.text}");
        }
    }

    /// <summary>
    /// Supprime un message d'encouragement
    /// </summary>
    public void RemoveMessage(EncouragementMessage message)
    {
        if (encouragementMessages.Remove(message))
        {
            RefreshAvailableMessages();
            DebugLog($"Message supprimé: {message.text}");
        }
    }

    /// <summary>
    /// Vide tous les messages d'encouragement
    /// </summary>
    public void ClearMessages()
    {
        encouragementMessages.Clear();
        RefreshAvailableMessages();
        DebugLog("Tous les messages ont été supprimés");
    }

    /// <summary>
    /// Charge une configuration depuis un ScriptableObject
    /// </summary>
    public void LoadConfigurationAsset(EncouragementConfiguration config)
    {
        if (config == null) return;
        
        configuration = config;
        LoadConfiguration();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Configuration de l'AudioSource
    /// </summary>
    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        audioSource.playOnAwake = false;
        audioSource.volume = audioVolume;
    }

    /// <summary>
    /// Initialise les messages par défaut
    /// </summary>
    private void InitializeDefaultMessages()
    {
        string[] defaultTexts = {
            "Bien joué !",
            "Continue comme ça !",
            "Super, tu y es presque !",
            "Bravo, tu progresses !",
            "C'est parti, tu gères !",
            "Excellent !",
            "Tu es dans le rythme !",
            "Parfait !",
            "Tu assures !",
            "Encore un peu !",
            "Doucement mais sûrement !",
            "Tu fais ça très bien !",
            "C'est fluide, continue !",
            "Tu tiens le bon rythme !",
            "Belle précision !",
            "Tout en maîtrise !",
            "Les copains vont être fiers !",
            "Le camp entier t'applaudit !",
            "Le chef de colo te félicite !",
            "T'es la star de la veillée !",
            "Les animateurs sont bluffés !",
            "On t'offre la première part de marshmallow !",
            "Le feu de camp est à toi !"
        };

        encouragementMessages.Clear();
        foreach (string text in defaultTexts)
        {
            encouragementMessages.Add(new EncouragementMessage(text, null, defaultDisplayDuration));
        }
        
        DebugLog($"Initialisé avec {encouragementMessages.Count} messages par défaut");
    }

    /// <summary>
    /// Charge la configuration depuis le ScriptableObject
    /// </summary>
    private void LoadConfiguration()
    {
        if (configuration == null) return;

        // Charger les messages
        encouragementMessages.Clear();
        encouragementMessages.AddRange(configuration.messages);
        
        // Charger les paramètres de timing
        minInterval = configuration.minInterval;
        maxInterval = configuration.maxInterval;
        defaultDisplayDuration = configuration.defaultDisplayDuration;
        
        // Charger les paramètres audio
        audioVolume = configuration.audioVolume;
        playAudioWithMessage = configuration.playAudioWithMessage;
        
        DebugLog($"Configuration chargée: {configuration.configurationName} avec {configuration.messages.Count} messages");
    }

    /// <summary>
    /// Rafraîchit la liste des messages disponibles
    /// </summary>
    private void RefreshAvailableMessages()
    {
        availableMessages = new List<EncouragementMessage>(encouragementMessages);
    }

    /// <summary>
    /// Obtient un message aléatoire en évitant les répétitions
    /// </summary>
    private EncouragementMessage GetRandomMessage()
    {
        if (availableMessages.Count == 0)
        {
            RefreshAvailableMessages();
        }

        if (availableMessages.Count == 0)
        {
            DebugLog("Aucun message disponible");
            return null;
        }

        // Retirer le dernier message utilisé de la liste si possible
        if (availableMessages.Count > 1 && !string.IsNullOrEmpty(lastMessageId))
        {
            availableMessages.RemoveAll(m => m.messageId == lastMessageId);
        }

        // Sélectionner un message aléatoire
        int randomIndex = Random.Range(0, availableMessages.Count);
        var selectedMessage = availableMessages[randomIndex];
        
        // Retirer le message sélectionné de la liste disponible
        availableMessages.RemoveAt(randomIndex);
        
        // Mémoriser le dernier message
        lastMessageId = selectedMessage.messageId;
        
        return selectedMessage;
    }

    /// <summary>
    /// Affiche un message spécifique
    /// </summary>
    private void ShowMessage(EncouragementMessage message)
    {
        if (message == null || displayComponent == null) return;

        // Afficher le texte
        displayComponent.ShowMessage(message.text, message.displayDuration);
        
        // Jouer l'audio si disponible et activé
        if (playAudioWithMessage && message.HasAudio && audioSource != null)
        {
            audioSource.clip = message.audioClip;
            audioSource.volume = audioVolume;
            audioSource.Play();
            DebugLog($"Audio joué pour: {message.text}");
        }
        
        // Déclencher l'événement
        OnMessageDisplayed?.Invoke(message.text);
        
        DebugLog($"Message affiché: {message.text}");
    }

    /// <summary>
    /// Boucle principale d'affichage des encouragements
    /// </summary>
    private IEnumerator EncouragementLoop()
    {
        while (isSystemActive)
        {
            // Attendre un intervalle aléatoire
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
            
            if (!isSystemActive) break;
            
            // Afficher un message aléatoire
            ShowRandomMessage();
        }
    }

    /// <summary>
    /// Log de debug conditionnel
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[EncouragementManager] {message}");
        }
    }
    #endregion

    #region Editor Helpers
    #if UNITY_EDITOR
    [Header("Outils d'édition")]
    [SerializeField] private bool testInEditor = false;
    
    void Update()
    {
        if (testInEditor && Application.isEditor && Input.GetKeyDown(KeyCode.E))
        {
            ShowRandomMessage();
        }
    }
    #endif
    #endregion
}
