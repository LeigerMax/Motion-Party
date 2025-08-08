using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Systems
{
    /// <summary>
    /// Gestionnaire des équipes - Fonctionnalités avancées de gestion d'équipes
    /// </summary>
    public class TeamManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxTeams = 20;
        [SerializeField] private int maxPlayersPerTeam = 10;
        [SerializeField] private bool debugMode = false;

        // Singleton
        private static TeamManager _instance;
        public static TeamManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TeamManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TeamManager");
                        _instance = go.AddComponent<TeamManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

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

        #region Team Operations

        /// <summary>
        /// Crée une équipe avec plusieurs joueurs
        /// </summary>
        public Team CreateTeamWithPlayers(string teamName, List<string> playerIds)
        {
            if (playerIds.Count > maxPlayersPerTeam)
            {
                Debug.LogError($"Trop de joueurs pour l'équipe (max: {maxPlayersPerTeam})");
                return null;
            }

            Team team = PlayerProfileManager.Instance.CreateTeam(teamName);
            if (team == null) return null;

            // Ajouter chaque joueur à l'équipe
            foreach (string playerId in playerIds)
            {
                PlayerProfileManager.Instance.AddPlayerToTeam(playerId, teamName);
            }

            if (debugMode)
                Debug.Log($"Équipe créée avec {playerIds.Count} joueurs: {team}");

            return team;
        }

        /// <summary>
        /// Transfère un joueur d'une équipe à une autre
        /// </summary>
        public bool TransferPlayer(string playerId, string newTeamName)
        {
            PlayerData player = PlayerProfileManager.Instance.GetPlayer(playerId);
            if (player == null) return false;

            string oldTeamName = player.TeamName;
            bool success = PlayerProfileManager.Instance.AddPlayerToTeam(playerId, newTeamName);

            if (success && debugMode)
                Debug.Log($"Joueur {player.Nickname} transféré de '{oldTeamName}' vers '{newTeamName}'");

            return success;
        }

        /// <summary>
        /// Équilibre les équipes (redistribue les joueurs de manière équitable)
        /// </summary>
        public void BalanceTeams()
        {
            var allPlayers = PlayerProfileManager.Instance.GetActivePlayers();
            var allTeams = PlayerProfileManager.Instance.GetAllTeams();

            if (allTeams.Count == 0 || allPlayers.Count == 0) return;

            // Retirer tous les joueurs de leurs équipes
            foreach (var player in allPlayers)
            {
                PlayerProfileManager.Instance.RemovePlayerFromAllTeams(player.Id);
            }

            // Redistribuer de manière équitable
            int playersPerTeam = allPlayers.Count / allTeams.Count;
            int remainder = allPlayers.Count % allTeams.Count;

            var shuffledPlayers = allPlayers.OrderBy(x => System.Guid.NewGuid()).ToList();
            int playerIndex = 0;

            for (int i = 0; i < allTeams.Count; i++)
            {
                var team = allTeams[i];
                int teamSize = playersPerTeam + (i < remainder ? 1 : 0);

                for (int j = 0; j < teamSize && playerIndex < shuffledPlayers.Count; j++)
                {
                    PlayerProfileManager.Instance.AddPlayerToTeam(shuffledPlayers[playerIndex].Id, team.Name);
                    playerIndex++;
                }
            }

            if (debugMode)
                Debug.Log($"Équipes équilibrées: {allTeams.Count} équipes, {allPlayers.Count} joueurs");
        }

        /// <summary>
        /// Fusionne deux équipes
        /// </summary>
        public bool MergeTeams(string team1Id, string team2Id, string newTeamName)
        {
            Team team1 = PlayerProfileManager.Instance.GetTeam(team1Id);
            Team team2 = PlayerProfileManager.Instance.GetTeam(team2Id);

            if (team1 == null || team2 == null) return false;

            // Créer la nouvelle équipe
            Team newTeam = PlayerProfileManager.Instance.CreateTeam(newTeamName);
            if (newTeam == null) return false;

            // Transférer tous les joueurs vers la nouvelle équipe
            var allPlayerIds = team1.PlayerIds.Concat(team2.PlayerIds).ToList();
            foreach (string playerId in allPlayerIds)
            {
                PlayerProfileManager.Instance.AddPlayerToTeam(playerId, newTeamName);
            }

            // Supprimer les anciennes équipes
            PlayerProfileManager.Instance.RemoveTeam(team1Id);
            PlayerProfileManager.Instance.RemoveTeam(team2Id);

            if (debugMode)
                Debug.Log($"Équipes fusionnées: {team1.Name} + {team2.Name} = {newTeamName}");

            return true;
        }

        /// <summary>
        /// Divise une équipe en deux
        /// </summary>
        public bool SplitTeam(string teamId, string newTeam1Name, string newTeam2Name)
        {
            Team originalTeam = PlayerProfileManager.Instance.GetTeam(teamId);
            if (originalTeam == null || originalTeam.PlayerIds.Count < 2) return false;

            // Créer les deux nouvelles équipes
            Team newTeam1 = PlayerProfileManager.Instance.CreateTeam(newTeam1Name);
            Team newTeam2 = PlayerProfileManager.Instance.CreateTeam(newTeam2Name);

            if (newTeam1 == null || newTeam2 == null) return false;

            // Diviser les joueurs
            var playerIds = originalTeam.PlayerIds.ToList();
            int midpoint = playerIds.Count / 2;

            for (int i = 0; i < playerIds.Count; i++)
            {
                string targetTeam = i < midpoint ? newTeam1Name : newTeam2Name;
                PlayerProfileManager.Instance.AddPlayerToTeam(playerIds[i], targetTeam);
            }

            // Supprimer l'équipe originale
            PlayerProfileManager.Instance.RemoveTeam(teamId);

            if (debugMode)
                Debug.Log($"Équipe divisée: {originalTeam.Name} → {newTeam1Name} + {newTeam2Name}");

            return true;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Retourne les statistiques d'une équipe
        /// </summary>
        public TeamStats GetTeamStats(string teamId)
        {
            Team team = PlayerProfileManager.Instance.GetTeam(teamId);
            if (team == null) return null;

            var teamPlayers = team.PlayerIds.Select(id => PlayerProfileManager.Instance.GetPlayer(id))
                                           .Where(p => p != null)
                                           .ToList();

            return new TeamStats
            {
                TeamId = teamId,
                TeamName = team.Name,
                PlayerCount = teamPlayers.Count,
                TotalGamesPlayed = teamPlayers.Sum(p => p.GamesPlayed),
                TotalScore = teamPlayers.Sum(p => p.TotalScore),
                AverageScore = teamPlayers.Count > 0 ? teamPlayers.Average(p => p.GetAverageScore()) : 0f,
                ActivePlayers = teamPlayers.Count(p => p.IsActive)
            };
        }

        /// <summary>
        /// Retourne les statistiques de toutes les équipes
        /// </summary>
        public List<TeamStats> GetAllTeamStats()
        {
            return PlayerProfileManager.Instance.GetAllTeams()
                                               .Select(team => GetTeamStats(team.Id))
                                               .Where(stats => stats != null)
                                               .ToList();
        }

        /// <summary>
        /// Retourne l'équipe avec le meilleur score moyen
        /// </summary>
        public TeamStats GetBestTeam()
        {
            var allStats = GetAllTeamStats();
            return allStats.OrderByDescending(s => s.AverageScore).FirstOrDefault();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Vérifie si on peut créer une nouvelle équipe
        /// </summary>
        public bool CanCreateNewTeam()
        {
            return PlayerProfileManager.Instance.GetTeamCount() < maxTeams;
        }

        /// <summary>
        /// Vérifie si on peut ajouter un joueur à une équipe
        /// </summary>
        public bool CanAddPlayerToTeam(string teamId)
        {
            Team team = PlayerProfileManager.Instance.GetTeam(teamId);
            if (team == null) return false;

            return team.GetPlayerCount() < maxPlayersPerTeam;
        }

        /// <summary>
        /// Retourne les équipes avec de la place
        /// </summary>
        public List<Team> GetAvailableTeams()
        {
            return PlayerProfileManager.Instance.GetAllTeams()
                                               .Where(team => team.GetPlayerCount() < maxPlayersPerTeam)
                                               .ToList();
        }

        /// <summary>
        /// Génère un nom d'équipe automatique
        /// </summary>
        public string GenerateTeamName()
        {
            string[] prefixes = { "Équipe", "Les", "Groupe", "Maison" };
            string[] suffixes = { "des Champions", "des Héros", "du Soleil", "de la Joie", "des Roses", "des Lilas", "des Chênes", "des Pins" };

            string prefix = prefixes[Random.Range(0, prefixes.Length)];
            string suffix = suffixes[Random.Range(0, suffixes.Length)];

            string baseName = $"{prefix} {suffix}";
            int counter = 1;
            string finalName = baseName;

            while (!PlayerProfileManager.Instance.IsTeamNameAvailable(finalName))
            {
                finalName = $"{baseName} {counter}";
                counter++;
            }

            return finalName;
        }

        #endregion
    }

    /// <summary>
    /// Statistiques d'une équipe
    /// </summary>
    [System.Serializable]
    public class TeamStats
    {
        public string TeamId;
        public string TeamName;
        public int PlayerCount;
        public int TotalGamesPlayed;
        public int TotalScore;
        public float AverageScore;
        public int ActivePlayers;

        public override string ToString()
        {
            return $"{TeamName}: {PlayerCount} joueurs, Score moyen: {AverageScore:F1}";
        }
    }
}
