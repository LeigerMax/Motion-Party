using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Systems
{
    /// <summary>
    /// Représente une équipe maison de repos avec ses joueurs
    /// </summary>
    [Serializable]
    public class Team
    {
        [SerializeField] private string id;
        [SerializeField] private string name;
        [SerializeField] private List<string> playerIds;
        [SerializeField] private DateTime createdAt;
        [SerializeField] private bool isActive;

        /// <summary>
        /// Identifiant unique de l'équipe
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Nom de l'équipe
        /// </summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// Liste des IDs des joueurs de l'équipe
        /// </summary>
        public List<string> PlayerIds => playerIds;

        /// <summary>
        /// Date de création de l'équipe
        /// </summary>
        public DateTime CreatedAt => createdAt;

        /// <summary>
        /// Si l'équipe est active
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Team()
        {
            id = System.Guid.NewGuid().ToString();
            name = "";
            playerIds = new List<string>();
            createdAt = DateTime.Now;
            isActive = true;
        }

        /// <summary>
        /// Constructeur avec nom d'équipe
        /// </summary>
        public Team(string teamName)
        {
            id = System.Guid.NewGuid().ToString();
            name = teamName;
            playerIds = new List<string>();
            createdAt = DateTime.Now;
            isActive = true;
        }

        /// <summary>
        /// Ajoute un joueur à l'équipe
        /// </summary>
        public void AddPlayer(string playerId)
        {
            if (!playerIds.Contains(playerId))
            {
                playerIds.Add(playerId);
            }
        }

        /// <summary>
        /// Retire un joueur de l'équipe
        /// </summary>
        public void RemovePlayer(string playerId)
        {
            playerIds.Remove(playerId);
        }

        /// <summary>
        /// Vérifie si un joueur fait partie de l'équipe
        /// </summary>
        public bool ContainsPlayer(string playerId)
        {
            return playerIds.Contains(playerId);
        }

        /// <summary>
        /// Retourne le nombre de joueurs dans l'équipe
        /// </summary>
        public int GetPlayerCount()
        {
            return playerIds.Count;
        }

        /// <summary>
        /// Retourne une représentation string de l'équipe
        /// </summary>
        public override string ToString()
        {
            return $"{name} ({playerIds.Count} joueurs)";
        }
    }
}
