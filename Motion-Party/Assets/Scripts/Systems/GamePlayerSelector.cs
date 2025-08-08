using UnityEngine;
using System.Collections.Generic;
using Systems;

namespace Systems
{
    /// <summary>
    /// Gestionnaire de sélection des joueurs pour les parties
    /// </summary>
    public class GamePlayerSelector : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool debugMode = false;

        // Données de session
        private List<PlayerData> selectedPlayers = new List<PlayerData>();
        private int currentPlayerIndex = 0;
        private bool isGameActive = false;

        // Singleton
        private static GamePlayerSelector _instance;
        public static GamePlayerSelector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GamePlayerSelector>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GamePlayerSelector");
                        _instance = go.AddComponent<GamePlayerSelector>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // Événements
        public System.Action<PlayerData> OnCurrentPlayerChanged;
        public System.Action<List<PlayerData>> OnPlayersSelected;
        public System.Action OnGameStarted;
        public System.Action OnGameEnded;

        // Propriétés
        public List<PlayerData> SelectedPlayers => new List<PlayerData>(selectedPlayers);
        public PlayerData CurrentPlayer => currentPlayerIndex >= 0 && currentPlayerIndex < selectedPlayers.Count ? 
            selectedPlayers[currentPlayerIndex] : null;
        public int CurrentPlayerIndex => currentPlayerIndex;
        public bool IsGameActive => isGameActive;
        public int PlayerCount => selectedPlayers.Count;

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

        #region Player Selection

        /// <summary>
        /// Sélectionne les joueurs pour la partie
        /// </summary>
        public void SelectPlayers(List<PlayerData> players)
        {
            if (players == null || players.Count == 0)
            {
                Debug.LogWarning("Aucun joueur sélectionné pour la partie");
                return;
            }

            selectedPlayers = new List<PlayerData>(players);
            currentPlayerIndex = 0;
            isGameActive = false;

            if (debugMode)
                Debug.Log($"Joueurs sélectionnés: {string.Join(", ", selectedPlayers.ConvertAll(p => p.Nickname))}");

            OnPlayersSelected?.Invoke(selectedPlayers);
        }

        /// <summary>
        /// Ajoute un joueur à la sélection
        /// </summary>
        public bool AddPlayer(PlayerData player)
        {
            if (player == null) return false;
            if (selectedPlayers.Contains(player)) return false;

            selectedPlayers.Add(player);

            if (debugMode)
                Debug.Log($"Joueur ajouté: {player.Nickname}");

            OnPlayersSelected?.Invoke(selectedPlayers);
            return true;
        }

        /// <summary>
        /// Retire un joueur de la sélection
        /// </summary>
        public bool RemovePlayer(PlayerData player)
        {
            if (player == null) return false;
            if (!selectedPlayers.Contains(player)) return false;

            int removedIndex = selectedPlayers.IndexOf(player);
            selectedPlayers.Remove(player);

            // Ajuster l'index du joueur actuel si nécessaire
            if (currentPlayerIndex >= selectedPlayers.Count)
            {
                currentPlayerIndex = selectedPlayers.Count - 1;
            }
            else if (currentPlayerIndex > removedIndex)
            {
                currentPlayerIndex--;
            }

            if (debugMode)
                Debug.Log($"Joueur retiré: {player.Nickname}");

            OnPlayersSelected?.Invoke(selectedPlayers);
            return true;
        }

        /// <summary>
        /// Efface la sélection
        /// </summary>
        public void ClearSelection()
        {
            selectedPlayers.Clear();
            currentPlayerIndex = 0;
            isGameActive = false;

            if (debugMode)
                Debug.Log("Sélection effacée");

            OnPlayersSelected?.Invoke(selectedPlayers);
        }

        #endregion

        #region Game Flow

        /// <summary>
        /// Démarre une partie avec les joueurs sélectionnés
        /// </summary>
        public bool StartGame()
        {
            if (selectedPlayers.Count == 0)
            {
                Debug.LogError("Aucun joueur sélectionné pour démarrer la partie");
                return false;
            }

            isGameActive = true;
            currentPlayerIndex = 0;

            if (debugMode)
                Debug.Log($"Partie démarrée avec {selectedPlayers.Count} joueurs");

            OnGameStarted?.Invoke();
            OnCurrentPlayerChanged?.Invoke(CurrentPlayer);

            return true;
        }

        /// <summary>
        /// Termine la partie
        /// </summary>
        public void EndGame()
        {
            isGameActive = false;

            // Incrémenter le nombre de parties jouées pour chaque joueur
            foreach (var player in selectedPlayers)
            {
                player.IncrementGamesPlayed();
            }

            if (debugMode)
                Debug.Log("Partie terminée");

            OnGameEnded?.Invoke();
        }

        /// <summary>
        /// Passe au joueur suivant
        /// </summary>
        public PlayerData NextPlayer()
        {
            if (selectedPlayers.Count == 0) return null;

            currentPlayerIndex = (currentPlayerIndex + 1) % selectedPlayers.Count;
            
            if (debugMode)
                Debug.Log($"Joueur suivant: {CurrentPlayer?.Nickname}");

            OnCurrentPlayerChanged?.Invoke(CurrentPlayer);
            return CurrentPlayer;
        }

        /// <summary>
        /// Revient au joueur précédent
        /// </summary>
        public PlayerData PreviousPlayer()
        {
            if (selectedPlayers.Count == 0) return null;

            currentPlayerIndex = (currentPlayerIndex - 1 + selectedPlayers.Count) % selectedPlayers.Count;
            
            if (debugMode)
                Debug.Log($"Joueur précédent: {CurrentPlayer?.Nickname}");

            OnCurrentPlayerChanged?.Invoke(CurrentPlayer);
            return CurrentPlayer;
        }

        /// <summary>
        /// Définit le joueur actuel par index
        /// </summary>
        public bool SetCurrentPlayer(int index)
        {
            if (index < 0 || index >= selectedPlayers.Count) return false;

            currentPlayerIndex = index;
            
            if (debugMode)
                Debug.Log($"Joueur actuel défini: {CurrentPlayer?.Nickname}");

            OnCurrentPlayerChanged?.Invoke(CurrentPlayer);
            return true;
        }

        /// <summary>
        /// Définit le joueur actuel par ID
        /// </summary>
        public bool SetCurrentPlayer(string playerId)
        {
            for (int i = 0; i < selectedPlayers.Count; i++)
            {
                if (selectedPlayers[i].Id == playerId)
                {
                    return SetCurrentPlayer(i);
                }
            }
            return false;
        }

        #endregion

        #region Scoring

        /// <summary>
        /// Ajoute des points au joueur actuel
        /// </summary>
        public void AddScoreToCurrentPlayer(int points)
        {
            if (CurrentPlayer != null)
            {
                CurrentPlayer.AddScore(points);
                
                if (debugMode)
                    Debug.Log($"Score ajouté: {points} points pour {CurrentPlayer.Nickname}");
            }
        }

        /// <summary>
        /// Ajoute des points à un joueur spécifique
        /// </summary>
        public void AddScoreToPlayer(string playerId, int points)
        {
            var player = selectedPlayers.Find(p => p.Id == playerId);
            if (player != null)
            {
                player.AddScore(points);
                
                if (debugMode)
                    Debug.Log($"Score ajouté: {points} points pour {player.Nickname}");
            }
        }

        /// <summary>
        /// Retourne le classement des joueurs pour cette partie
        /// </summary>
        public List<PlayerData> GetPlayerRanking()
        {
            var ranking = new List<PlayerData>(selectedPlayers);
            ranking.Sort((a, b) => b.TotalScore.CompareTo(a.TotalScore));
            return ranking;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Vérifie si un joueur est sélectionné
        /// </summary>
        public bool IsPlayerSelected(PlayerData player)
        {
            return selectedPlayers.Contains(player);
        }

        /// <summary>
        /// Vérifie si un joueur est sélectionné par ID
        /// </summary>
        public bool IsPlayerSelected(string playerId)
        {
            return selectedPlayers.Exists(p => p.Id == playerId);
        }

        /// <summary>
        /// Retourne la position d'un joueur dans la sélection
        /// </summary>
        public int GetPlayerIndex(PlayerData player)
        {
            return selectedPlayers.IndexOf(player);
        }

        /// <summary>
        /// Retourne la position d'un joueur dans la sélection par ID
        /// </summary>
        public int GetPlayerIndex(string playerId)
        {
            return selectedPlayers.FindIndex(p => p.Id == playerId);
        }

        /// <summary>
        /// Mélange l'ordre des joueurs
        /// </summary>
        public void ShufflePlayers()
        {
            for (int i = 0; i < selectedPlayers.Count; i++)
            {
                int randomIndex = Random.Range(i, selectedPlayers.Count);
                var temp = selectedPlayers[i];
                selectedPlayers[i] = selectedPlayers[randomIndex];
                selectedPlayers[randomIndex] = temp;
            }

            currentPlayerIndex = 0;

            if (debugMode)
                Debug.Log("Ordre des joueurs mélangé");

            OnCurrentPlayerChanged?.Invoke(CurrentPlayer);
        }

        /// <summary>
        /// Retourne des informations sur la session de jeu
        /// </summary>
        public GameSessionInfo GetSessionInfo()
        {
            return new GameSessionInfo
            {
                PlayerCount = selectedPlayers.Count,
                CurrentPlayerIndex = currentPlayerIndex,
                IsGameActive = isGameActive,
                Players = new List<PlayerData>(selectedPlayers)
            };
        }

        #endregion
    }

    /// <summary>
    /// Informations sur la session de jeu
    /// </summary>
    [System.Serializable]
    public class GameSessionInfo
    {
        public int PlayerCount;
        public int CurrentPlayerIndex;
        public bool IsGameActive;
        public List<PlayerData> Players;

        public override string ToString()
        {
            return $"Session: {PlayerCount} joueurs, Actuel: {CurrentPlayerIndex}, Actif: {IsGameActive}";
        }
    }
}
