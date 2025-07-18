using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace Gameplay.Common.Badges
{
    /// <summary>
    /// Singleton pour gérer les badges gagnés par chaque joueur pendant la session (tous les mini-jeux)
    /// Persiste entre les scènes, mais pas sauvegardé sur disque.
    /// </summary>
    public class SessionBadgeTracker : MonoBehaviour
    {
        public static SessionBadgeTracker Instance { get; private set; }
    // Permet d'auto-instancier le singleton si besoin
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("SessionBadgeTracker");
            Instance = go.AddComponent<SessionBadgeTracker>();
            DontDestroyOnLoad(go);
        }
    }

        // Stockage par joueur : playerId => liste de tous les badges gagnés (doublons possibles)
        private Dictionary<string, List<BadgeInstance>> playerSessionBadges = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Ajoute un badge pour un joueur (doublons autorisés)
        /// </summary>
        public void AddBadgeForPlayer(string playerId, BadgeInstance badge)
        {
            if (!playerSessionBadges.ContainsKey(playerId))
                playerSessionBadges[playerId] = new List<BadgeInstance>();
            playerSessionBadges[playerId].Add(badge);
        }

        /// <summary>
        /// Récupère tous les badges de session pour un joueur (doublons inclus)
        /// </summary>
        public List<BadgeInstance> GetSessionBadgesForPlayer(string playerId)
        {
            if (playerSessionBadges.TryGetValue(playerId, out var badges))
                return badges;
            return new List<BadgeInstance>();
        }

        /// <summary>
        /// Nettoie tous les badges de session (à la fin de la partie)
        /// </summary>
        public void ClearAllSessionBadges()
        {
            playerSessionBadges.Clear();
        }
    }
}