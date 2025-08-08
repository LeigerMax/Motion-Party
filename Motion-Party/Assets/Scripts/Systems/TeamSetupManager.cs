using UnityEngine;
using System.Collections.Generic;

namespace Systems
{
    /// <summary>
    /// Gestionnaire pour la configuration initiale de l'équipe (nom de la maison de repos)
    /// et la gestion des joueurs de base.
    /// </summary>
    public class TeamSetupManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private string defaultTeamName = "Ma Maison de Repos";
        
        // Singleton
        private static TeamSetupManager _instance;
        public static TeamSetupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TeamSetupManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TeamSetupManager");
                        _instance = go.AddComponent<TeamSetupManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // Clés pour PlayerPrefs
        private const string TEAM_NAME_KEY = "MotionParty_TeamName";
        private const string TEAM_SETUP_DONE_KEY = "MotionParty_TeamSetupDone";

        // Événements
        public System.Action<string> OnTeamNameChanged;
        public System.Action<PlayerData> OnPlayerAdded;
        public System.Action<PlayerData> OnPlayerRemoved;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Vérifie si la configuration initiale de l'équipe a été faite
        /// </summary>
        public bool IsTeamSetupDone()
        {
            return PlayerPrefs.GetInt(TEAM_SETUP_DONE_KEY, 0) == 1;
        }

        /// <summary>
        /// Récupère le nom de l'équipe actuelle
        /// </summary>
        public string GetTeamName()
        {
            return PlayerPrefs.GetString(TEAM_NAME_KEY, defaultTeamName);
        }

        /// <summary>
        /// Définit le nom de l'équipe et marque la configuration comme terminée
        /// </summary>
        public void SetTeamName(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                teamName = defaultTeamName;
            }

            PlayerPrefs.SetString(TEAM_NAME_KEY, teamName);
            PlayerPrefs.SetInt(TEAM_SETUP_DONE_KEY, 1);
            PlayerPrefs.Save();

            if (debugMode)
                Debug.Log($"TeamSetupManager: Nom d'équipe défini : {teamName}");

            OnTeamNameChanged?.Invoke(teamName);
        }

        /// <summary>
        /// Crée un nouveau joueur avec le nom donné
        /// </summary>
        public PlayerData CreatePlayer(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("TeamSetupManager: Nom de joueur vide, création annulée");
                return null;
            }

            // Créer le joueur via PlayerProfileManager
            PlayerData newPlayer = PlayerProfileManager.Instance.CreatePlayer(playerName.Trim(), GetTeamName());
            
            if (newPlayer != null)
            {
                if (debugMode)
                    Debug.Log($"TeamSetupManager: Joueur créé : {newPlayer.Nickname}");

                OnPlayerAdded?.Invoke(newPlayer);
                return newPlayer;
            }

            return null;
        }

        /// <summary>
        /// Supprime un joueur
        /// </summary>
        public bool RemovePlayer(string playerId)
        {
            var player = PlayerProfileManager.Instance.GetPlayer(playerId);
            if (player != null)
            {
                if (PlayerProfileManager.Instance.RemovePlayer(playerId))
                {
                    if (debugMode)
                        Debug.Log($"TeamSetupManager: Joueur supprimé : {player.Nickname}");

                    OnPlayerRemoved?.Invoke(player);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Récupère tous les joueurs de l'équipe actuelle
        /// </summary>
        public List<PlayerData> GetTeamPlayers()
        {
            return PlayerProfileManager.Instance.GetPlayersByTeam(GetTeamName());
        }

        /// <summary>
        /// Récupère le nombre de joueurs dans l'équipe
        /// </summary>
        public int GetTeamPlayerCount()
        {
            return GetTeamPlayers().Count;
        }

        /// <summary>
        /// Réinitialise la configuration de l'équipe (pour debug)
        /// </summary>
        [ContextMenu("Reset Team Setup")]
        public void ResetTeamSetup()
        {
            PlayerPrefs.DeleteKey(TEAM_NAME_KEY);
            PlayerPrefs.DeleteKey(TEAM_SETUP_DONE_KEY);
            PlayerPrefs.Save();
            
            if (debugMode)
                Debug.Log("TeamSetupManager: Configuration réinitialisée");
        }
    }
}
