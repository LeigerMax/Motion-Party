using UnityEngine;
using System;

namespace Systems
{
    /// <summary>
    /// Représente les données d'un joueur individuel
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [SerializeField] private string id;
        [SerializeField] private string nickname;
        [SerializeField] private string teamName;
        [SerializeField] private DateTime createdAt;
        [SerializeField] private int gamesPlayed;
        [SerializeField] private int totalScore;
        [SerializeField] private bool isActive;

        /// <summary>
        /// Identifiant unique du joueur
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Prénom ou pseudo du joueur
        /// </summary>
        public string Nickname
        {
            get => nickname;
            set => nickname = value;
        }

        /// <summary>
        /// Nom de l'équipe maison de repos
        /// </summary>
        public string TeamName
        {
            get => teamName;
            set => teamName = value;
        }

        /// <summary>
        /// Date de création du profil
        /// </summary>
        public DateTime CreatedAt => createdAt;

        /// <summary>
        /// Nombre de parties jouées
        /// </summary>
        public int GamesPlayed
        {
            get => gamesPlayed;
            set => gamesPlayed = value;
        }

        /// <summary>
        /// Score total accumulé
        /// </summary>
        public int TotalScore
        {
            get => totalScore;
            set => totalScore = value;
        }

        /// <summary>
        /// Si le joueur est actif
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public PlayerData()
        {
            id = System.Guid.NewGuid().ToString();
            nickname = "";
            teamName = "";
            createdAt = DateTime.Now;
            gamesPlayed = 0;
            totalScore = 0;
            isActive = true;
        }

        /// <summary>
        /// Constructeur avec nickname
        /// </summary>
        public PlayerData(string nickname, string teamName = "")
        {
            id = System.Guid.NewGuid().ToString();
            this.nickname = nickname;
            this.teamName = teamName;
            createdAt = DateTime.Now;
            gamesPlayed = 0;
            totalScore = 0;
            isActive = true;
        }

        /// <summary>
        /// Incrémente le nombre de parties jouées
        /// </summary>
        public void IncrementGamesPlayed()
        {
            gamesPlayed++;
        }

        /// <summary>
        /// Ajoute des points au score total
        /// </summary>
        public void AddScore(int points)
        {
            totalScore += points;
        }

        /// <summary>
        /// Calcule le score moyen par partie
        /// </summary>
        public float GetAverageScore()
        {
            if (gamesPlayed == 0) return 0f;
            return (float)totalScore / gamesPlayed;
        }

        /// <summary>
        /// Retourne une représentation string des données du joueur
        /// </summary>
        public override string ToString()
        {
            return $"{nickname} ({teamName}) - Parties: {gamesPlayed}, Score: {totalScore}";
        }
    }
}
