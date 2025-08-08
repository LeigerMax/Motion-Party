using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Gameplay.Common.Badges;

namespace UI.Team
{
    /// <summary>
    /// Composant pour afficher les statistiques du profil joueur
    /// Interface simple pour les seniors
    /// </summary>
    public class PlayerProfileStats : MonoBehaviour
    {
        [Header("Stats UI References")]
        [SerializeField] private TextMeshProUGUI totalBadgesText;
        [SerializeField] private TextMeshProUGUI scorePerGameText;
        [SerializeField] private TextMeshProUGUI favGameText;
        [SerializeField] private TextMeshProUGUI reactionTimeText;

        [Header("Badge Breakdown")]
        [SerializeField] private TextMeshProUGUI fireflyBadgesText;
        [SerializeField] private TextMeshProUGUI balloonBadgesText;
        [SerializeField] private TextMeshProUGUI musicBadgesText;

        [Header("Optional Elements")]
        [SerializeField] private GameObject statsContainer;

        /// <summary>
        /// Met à jour l'affichage des statistiques du joueur
        /// </summary>
        public void UpdatePlayerStats(Systems.PlayerData playerData)
        {
            if (playerData == null) return;

            // Récupérer les stats de badges
            var badgeStats = BadgeUIHelper.GetPlayerBadgeStats(playerData.Nickname);

            // Stats générales
            if (totalBadgesText != null)
                totalBadgesText.text = $"Badges : {badgeStats.TotalBadges}";

            if (scorePerGameText != null)
            {
                float avgScore = playerData.GetAverageScore();
                scorePerGameText.text = $"Score moyen : {avgScore:F0} pts";
            }

            // Jeu favori (basé sur le nombre de badges)
            if (favGameText != null)
            {
                string favoriteGame = GetFavoriteGame(badgeStats);
                favGameText.text = $"Jeu favori : {favoriteGame}";
            }

            // Temps de réaction simulé (peut être implémenté plus tard)
            if (reactionTimeText != null)
            {
                float estimatedReactionTime = EstimateReactionTime(playerData);
                reactionTimeText.text = $"Réactivité : {estimatedReactionTime:F1}s";
            }

            // Breakdown par jeu
            if (fireflyBadgesText != null)
                fireflyBadgesText.text = $"Lucioles : {badgeStats.FireflyBadges}";

            if (balloonBadgesText != null)
                balloonBadgesText.text = $"Ballons : {badgeStats.BalloonBadges}";

            if (musicBadgesText != null)
                musicBadgesText.text = $"Musique : {badgeStats.MusicBadges}";

            // Activer le conteneur si nécessaire
            if (statsContainer != null)
                statsContainer.SetActive(true);
        }

        /// <summary>
        /// Détermine le jeu favori du joueur
        /// </summary>
        private string GetFavoriteGame(PlayerBadgeStats stats)
        {
            int maxBadges = Mathf.Max(stats.FireflyBadges, stats.BalloonBadges, stats.MusicBadges);
            
            if (maxBadges == 0) return "Aucun";

            if (stats.FireflyBadges == maxBadges) return "Lucioles";
            if (stats.BalloonBadges == maxBadges) return "Ballons";
            if (stats.MusicBadges == maxBadges) return "Musique";

            return "Aucun";
        }

        /// <summary>
        /// Estime le temps de réaction basé sur les performances
        /// </summary>
        private float EstimateReactionTime(Systems.PlayerData playerData)
        {
            // Estimation basique basée sur le score moyen
            // Plus le score est élevé, plus on suppose que la réaction est rapide
            float avgScore = playerData.GetAverageScore();
            
            if (avgScore == 0) return 2.0f; // Défaut pour nouveaux joueurs
            
            // Fonction simple : meilleur score = temps de réaction plus rapide
            // Plage réaliste : 0.8s à 2.5s
            float reactionTime = Mathf.Lerp(2.5f, 0.8f, Mathf.Clamp01(avgScore / 1000f));
            return reactionTime;
        }

        /// <summary>
        /// Cache l'affichage des stats
        /// </summary>
        public void HideStats()
        {
            if (statsContainer != null)
                statsContainer.SetActive(false);
        }
    }
}
