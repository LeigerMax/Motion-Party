using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using Newtonsoft.Json;

namespace Systems
{
    /// <summary>
    /// Gestionnaire des profils de joueurs - Gère la persistance et les opérations sur les données joueurs
    /// </summary>
    public class PlayerProfileManager : MonoBehaviour
    {
        private const string PLAYERS_FILE = "players.json";
        private const string TEAMS_FILE = "teams.json";

        [Header("Configuration")]
        [SerializeField] private bool usePlayerPrefs = false;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private int maxPlayers = 50;

        // Données en mémoire
        private List<PlayerData> players = new List<PlayerData>();
        private List<Team> teams = new List<Team>();

        // Singleton
        private static PlayerProfileManager _instance;
        public static PlayerProfileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PlayerProfileManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PlayerProfileManager");
                        _instance = go.AddComponent<PlayerProfileManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // Événements
        public System.Action<PlayerData> OnPlayerAdded;
        public System.Action<PlayerData> OnPlayerRemoved;
        public System.Action<PlayerData> OnPlayerUpdated;
        public System.Action<Team> OnTeamAdded;
        public System.Action<Team> OnTeamRemoved;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Player Management

        /// <summary>
        /// Crée un nouveau joueur
        /// </summary>
        public PlayerData CreatePlayer(string nickname, string teamName = "")
        {
            if (string.IsNullOrEmpty(nickname))
            {
                Debug.LogError("Le nom du joueur ne peut pas être vide");
                return null;
            }

            if (players.Count >= maxPlayers)
            {
                Debug.LogError($"Nombre maximum de joueurs atteint ({maxPlayers})");
                return null;
            }

            // Vérifier si le pseudo existe déjà
            if (players.Any(p => p.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase)))
            {
                Debug.LogError($"Un joueur avec le pseudo '{nickname}' existe déjà");
                return null;
            }

            PlayerData newPlayer = new PlayerData(nickname, teamName);
            players.Add(newPlayer);

            // Ajouter à l'équipe si spécifiée
            if (!string.IsNullOrEmpty(teamName))
            {
                AddPlayerToTeam(newPlayer.Id, teamName);
            }

            SaveData();
            OnPlayerAdded?.Invoke(newPlayer);

            if (debugMode)
                Debug.Log($"Joueur créé: {newPlayer}");

            return newPlayer;
        }

        /// <summary>
        /// Supprime un joueur
        /// </summary>
        public bool RemovePlayer(string playerId)
        {
            PlayerData player = GetPlayer(playerId);
            if (player == null) return false;

            // Retirer de toutes les équipes
            foreach (var team in teams)
            {
                team.RemovePlayer(playerId);
            }

            players.Remove(player);
            SaveData();
            OnPlayerRemoved?.Invoke(player);

            if (debugMode)
                Debug.Log($"Joueur supprimé: {player}");

            return true;
        }

        /// <summary>
        /// Met à jour un joueur
        /// </summary>
        public bool UpdatePlayer(string playerId, string newNickname = null, string newTeamName = null)
        {
            PlayerData player = GetPlayer(playerId);
            if (player == null) return false;

            if (!string.IsNullOrEmpty(newNickname))
            {
                // Vérifier si le nouveau pseudo existe déjà
                if (players.Any(p => p.Id != playerId && p.Nickname.Equals(newNickname, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.LogError($"Un joueur avec le pseudo '{newNickname}' existe déjà");
                    return false;
                }
                player.Nickname = newNickname;
            }

            if (newTeamName != null)
            {
                // Retirer de l'ancienne équipe
                RemovePlayerFromAllTeams(playerId);
                
                // Ajouter à la nouvelle équipe si spécifiée
                if (!string.IsNullOrEmpty(newTeamName))
                {
                    AddPlayerToTeam(playerId, newTeamName);
                }
                
                player.TeamName = newTeamName;
            }

            SaveData();
            OnPlayerUpdated?.Invoke(player);

            if (debugMode)
                Debug.Log($"Joueur mis à jour: {player}");

            return true;
        }

        /// <summary>
        /// Récupère un joueur par son ID
        /// </summary>
        public PlayerData GetPlayer(string playerId)
        {
            return players.FirstOrDefault(p => p.Id == playerId);
        }

        /// <summary>
        /// Récupère un joueur par son pseudo
        /// </summary>
        public PlayerData GetPlayerByNickname(string nickname)
        {
            return players.FirstOrDefault(p => p.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Récupère tous les joueurs
        /// </summary>
        public List<PlayerData> GetAllPlayers()
        {
            return new List<PlayerData>(players);
        }

        /// <summary>
        /// Récupère tous les joueurs actifs
        /// </summary>
        public List<PlayerData> GetActivePlayers()
        {
            return players.Where(p => p.IsActive).ToList();
        }

        /// <summary>
        /// Récupère les joueurs d'une équipe
        /// </summary>
        public List<PlayerData> GetPlayersByTeam(string teamName)
        {
            return players.Where(p => p.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        #endregion

        #region Team Management

        /// <summary>
        /// Crée une nouvelle équipe
        /// </summary>
        public Team CreateTeam(string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
            {
                Debug.LogError("Le nom de l'équipe ne peut pas être vide");
                return null;
            }

            // Vérifier si l'équipe existe déjà
            if (teams.Any(t => t.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase)))
            {
                Debug.LogError($"Une équipe avec le nom '{teamName}' existe déjà");
                return null;
            }

            Team newTeam = new Team(teamName);
            teams.Add(newTeam);

            SaveData();
            OnTeamAdded?.Invoke(newTeam);

            if (debugMode)
                Debug.Log($"Équipe créée: {newTeam}");

            return newTeam;
        }

        /// <summary>
        /// Supprime une équipe
        /// </summary>
        public bool RemoveTeam(string teamId)
        {
            Team team = GetTeam(teamId);
            if (team == null) return false;

            // Mettre à jour les joueurs de cette équipe
            foreach (string playerId in team.PlayerIds.ToList())
            {
                PlayerData player = GetPlayer(playerId);
                if (player != null)
                {
                    player.TeamName = "";
                }
            }

            teams.Remove(team);
            SaveData();
            OnTeamRemoved?.Invoke(team);

            if (debugMode)
                Debug.Log($"Équipe supprimée: {team}");

            return true;
        }

        /// <summary>
        /// Ajoute un joueur à une équipe
        /// </summary>
        public bool AddPlayerToTeam(string playerId, string teamName)
        {
            PlayerData player = GetPlayer(playerId);
            if (player == null) return false;

            // Créer l'équipe si elle n'existe pas
            Team team = GetTeamByName(teamName);
            if (team == null)
            {
                team = CreateTeam(teamName);
            }

            // Retirer le joueur de toutes les autres équipes
            RemovePlayerFromAllTeams(playerId);

            // Ajouter à la nouvelle équipe
            team.AddPlayer(playerId);
            player.TeamName = teamName;

            SaveData();
            return true;
        }

        /// <summary>
        /// Retire un joueur de toutes les équipes
        /// </summary>
        public void RemovePlayerFromAllTeams(string playerId)
        {
            foreach (var team in teams)
            {
                team.RemovePlayer(playerId);
            }
        }

        /// <summary>
        /// Récupère une équipe par son ID
        /// </summary>
        public Team GetTeam(string teamId)
        {
            return teams.FirstOrDefault(t => t.Id == teamId);
        }

        /// <summary>
        /// Récupère une équipe par son nom
        /// </summary>
        public Team GetTeamByName(string teamName)
        {
            return teams.FirstOrDefault(t => t.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Récupère toutes les équipes
        /// </summary>
        public List<Team> GetAllTeams()
        {
            return new List<Team>(teams);
        }

        /// <summary>
        /// Récupère les noms de toutes les équipes
        /// </summary>
        public List<string> GetTeamNames()
        {
            return teams.Select(t => t.Name).ToList();
        }

        #endregion

        #region Data Persistence

        /// <summary>
        /// Sauvegarde les données
        /// </summary>
        private void SaveData()
        {
            try
            {
                if (usePlayerPrefs)
                {
                    SaveToPlayerPrefs();
                }
                else
                {
                    SaveToFiles();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors de la sauvegarde: {e.Message}");
            }
        }

        /// <summary>
        /// Charge les données
        /// </summary>
        private void LoadData()
        {
            try
            {
                if (usePlayerPrefs)
                {
                    LoadFromPlayerPrefs();
                }
                else
                {
                    LoadFromFiles();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors du chargement: {e.Message}");
                InitializeDefaultData();
            }
        }

        /// <summary>
        /// Sauvegarde dans PlayerPrefs
        /// </summary>
        private void SaveToPlayerPrefs()
        {
            string playersJson = JsonConvert.SerializeObject(players, Formatting.Indented);
            string teamsJson = JsonConvert.SerializeObject(teams, Formatting.Indented);

            PlayerPrefs.SetString("MotionParty_Players", playersJson);
            PlayerPrefs.SetString("MotionParty_Teams", teamsJson);
            PlayerPrefs.Save();

            if (debugMode)
                Debug.Log("Données sauvegardées dans PlayerPrefs");
        }

        /// <summary>
        /// Charge depuis PlayerPrefs
        /// </summary>
        private void LoadFromPlayerPrefs()
        {
            string playersJson = PlayerPrefs.GetString("MotionParty_Players", "");
            string teamsJson = PlayerPrefs.GetString("MotionParty_Teams", "");

            if (!string.IsNullOrEmpty(playersJson))
            {
                players = JsonConvert.DeserializeObject<List<PlayerData>>(playersJson) ?? new List<PlayerData>();
            }

            if (!string.IsNullOrEmpty(teamsJson))
            {
                teams = JsonConvert.DeserializeObject<List<Team>>(teamsJson) ?? new List<Team>();
            }

            if (debugMode)
                Debug.Log($"Données chargées depuis PlayerPrefs: {players.Count} joueurs, {teams.Count} équipes");
        }

        /// <summary>
        /// Sauvegarde dans des fichiers JSON
        /// </summary>
        private void SaveToFiles()
        {
            string playersPath = Path.Combine(Application.persistentDataPath, PLAYERS_FILE);
            string teamsPath = Path.Combine(Application.persistentDataPath, TEAMS_FILE);

            string playersJson = JsonConvert.SerializeObject(players, Formatting.Indented);
            string teamsJson = JsonConvert.SerializeObject(teams, Formatting.Indented);

            File.WriteAllText(playersPath, playersJson);
            File.WriteAllText(teamsPath, teamsJson);

            if (debugMode)
                Debug.Log($"Données sauvegardées dans: {playersPath} et {teamsPath}");
        }

        /// <summary>
        /// Charge depuis des fichiers JSON
        /// </summary>
        private void LoadFromFiles()
        {
            string playersPath = Path.Combine(Application.persistentDataPath, PLAYERS_FILE);
            string teamsPath = Path.Combine(Application.persistentDataPath, TEAMS_FILE);

            if (File.Exists(playersPath))
            {
                string playersJson = File.ReadAllText(playersPath);
                players = JsonConvert.DeserializeObject<List<PlayerData>>(playersJson) ?? new List<PlayerData>();
            }

            if (File.Exists(teamsPath))
            {
                string teamsJson = File.ReadAllText(teamsPath);
                teams = JsonConvert.DeserializeObject<List<Team>>(teamsJson) ?? new List<Team>();
            }

            if (debugMode)
                Debug.Log($"Données chargées depuis fichiers: {players.Count} joueurs, {teams.Count} équipes");
        }

        /// <summary>
        /// Initialise des données par défaut
        /// </summary>
        private void InitializeDefaultData()
        {
            players = new List<PlayerData>();
            teams = new List<Team>();

            if (debugMode)
                Debug.Log("Données par défaut initialisées");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Efface toutes les données
        /// </summary>
        public void ClearAllData()
        {
            players.Clear();
            teams.Clear();
            SaveData();

            if (debugMode)
                Debug.Log("Toutes les données ont été effacées");
        }

        /// <summary>
        /// Retourne le nombre total de joueurs
        /// </summary>
        public int GetPlayerCount()
        {
            return players.Count;
        }

        /// <summary>
        /// Retourne le nombre total d'équipes
        /// </summary>
        public int GetTeamCount()
        {
            return teams.Count;
        }

        /// <summary>
        /// Vérifie si un pseudo est disponible
        /// </summary>
        public bool IsNicknameAvailable(string nickname)
        {
            return !players.Any(p => p.Nickname.Equals(nickname, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vérifie si un nom d'équipe est disponible
        /// </summary>
        public bool IsTeamNameAvailable(string teamName)
        {
            return !teams.Any(t => t.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
