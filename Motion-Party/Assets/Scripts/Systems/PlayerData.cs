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
        [SerializeField] private string birthDate; // Format: "YYYY-MM-DD"
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
        /// Date de naissance au format "YYYY-MM-DD"
        /// </summary>
        public string BirthDate
        {
            get => birthDate;
            set => birthDate = value;
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
            birthDate = "";
            createdAt = DateTime.Now;
            gamesPlayed = 0;
            totalScore = 0;
            isActive = true;
        }

        /// <summary>
        /// Constructeur avec nickname
        /// </summary>
        public PlayerData(string nickname, string teamName = "", string birthDate = "")
        {
            id = System.Guid.NewGuid().ToString();
            this.nickname = nickname;
            this.teamName = teamName;
            this.birthDate = birthDate;
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
        /// Calcule l'âge du joueur basé sur sa date de naissance
        /// </summary>
        public int CalculateAge()
        {
            if (string.IsNullOrEmpty(birthDate))
                return 0;

            try
            {
                DateTime birth = DateTime.ParseExact(birthDate, "yyyy-MM-dd", null);
                DateTime today = DateTime.Today;
                int age = today.Year - birth.Year;
                
                // Si l'anniversaire n'est pas encore passé cette année, soustraire 1
                if (birth.Date > today.AddYears(-age))
                    age--;
                    
                return age;
            }
            catch
            {
                return 0;
            }
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
