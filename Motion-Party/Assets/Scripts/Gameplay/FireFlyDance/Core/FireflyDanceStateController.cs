using UnityEngine;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Core
{
    /// <summary>
    /// Contrôleur centralisé de l'état du mini-jeu Danse des Lucioles
    /// Gère les autorisations de gameplay et les verrous système
    /// </summary>
    public class FireflyDanceStateController : MonoBehaviour
{
    #region Fields & Properties

    [Header("State Settings")]
    [SerializeField] private bool showDetailedLogs = true;

    /// <summary>
    /// Indique si le gameplay est actif
    /// </summary>
    public static bool IsGameplayActive { get; private set; } = false;

    /// <summary>
    /// Indique si le jeu est en pause
    /// </summary>
    public static bool IsGamePaused { get; private set; } = false;

    /// <summary>
    /// Indique si le spawn est autorisé
    /// </summary>
    public static bool IsSpawnAllowed { get; private set; } = false;

    /// <summary>
    /// Indique si les interactions sont autorisées
    /// </summary>
    public static bool IsInteractionAllowed { get; private set; } = false;

    // Instance singleton pour accès facilité
    private static FireflyDanceStateController instance;
    public static FireflyDanceStateController Instance => instance;

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeState();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialise l'état par défaut du jeu
    /// </summary>
    private void InitializeState()
    {
        IsGameplayActive = false;
        IsGamePaused = false;
        IsSpawnAllowed = false;
        IsInteractionAllowed = false;

        if (showDetailedLogs)
        {
            FireflyDanceLogger.LogVerbose("État initialisé - Gameplay et interactions désactivés");
        }
    }

    #endregion

    #region Gameplay State Management

    /// <summary>
    /// Démarre le gameplay
    /// </summary>
    public static void StartGameplay()
    {
        IsGameplayActive = true;
        IsGamePaused = false;
        IsSpawnAllowed = true;
        IsInteractionAllowed = true;

        if (instance?.showDetailedLogs == true)
        {
            FireflyDanceLogger.Log("Gameplay démarré - Toutes les interactions activées");
        }

        // Émettre l'événement
        FireflyDanceEvents.OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Termine le gameplay
    /// </summary>
    public static void EndGameplay()
    {
        IsGameplayActive = false;
        IsGamePaused = false;
        IsSpawnAllowed = false;
        IsInteractionAllowed = false;

        if (instance?.showDetailedLogs == true)
        {
            FireflyDanceLogger.Log("Gameplay terminé - Toutes les interactions désactivées");
        }

        // Émettre l'événement
        FireflyDanceEvents.OnGameEnded?.Invoke();
    }

    /// <summary>
    /// Met en pause ou reprend le gameplay
    /// </summary>
    public static void SetGamePaused(bool paused)
    {
        if (IsGamePaused == paused) return;

        IsGamePaused = paused;
        IsSpawnAllowed = !paused && IsGameplayActive;
        IsInteractionAllowed = !paused && IsGameplayActive;

        if (instance?.showDetailedLogs == true)
        {
            FireflyDanceLogger.Log($"Gameplay {(paused ? "en pause" : "repris")}");
        }

        // Émettre l'événement
        FireflyDanceEvents.OnGamePaused?.Invoke(paused);
    }

    #endregion

    #region Permission Checks

    /// <summary>
    /// Vérifie si le spawn est autorisé
    /// </summary>
    public static bool CanSpawn()
    {
        return IsSpawnAllowed && !IsGamePaused;
    }

    /// <summary>
    /// Vérifie si les interactions sont autorisées
    /// </summary>
    public static bool CanInteract()
    {
        return IsInteractionAllowed && !IsGamePaused;
    }

    /// <summary>
    /// Vérifie si le gameplay est actif
    /// </summary>
    public static bool IsGameActive()
    {
        return IsGameplayActive && !IsGamePaused;
    }

    #endregion

    #region Public Control Methods

    /// <summary>
    /// Force l'arrêt de tous les systèmes
    /// </summary>
    public static void ForceStop()
    {
        IsGameplayActive = false;
        IsGamePaused = false;
        IsSpawnAllowed = false;
        IsInteractionAllowed = false;

        if (instance?.showDetailedLogs == true)
        {
            FireflyDanceLogger.LogWarning("Arrêt forcé de tous les systèmes");
        }
    }

    /// <summary>
    /// Réinitialise complètement l'état
    /// </summary>
    public static void ResetState()
    {
        if (instance != null)
        {
            instance.InitializeState();
            FireflyDanceLogger.Log("État réinitialisé");
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Affiche l'état actuel dans les logs
    /// </summary>
    [ContextMenu("Log Current State")]
    public void LogCurrentState()
    {
        FireflyDanceLogger.Log($"État actuel - Gameplay: {IsGameplayActive}, Pause: {IsGamePaused}, Spawn: {IsSpawnAllowed}, Interaction: {IsInteractionAllowed}");
    }

    #endregion
}
}
