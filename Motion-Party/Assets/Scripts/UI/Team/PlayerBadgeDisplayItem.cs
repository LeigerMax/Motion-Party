using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Gameplay.Common.Badges;

namespace UI.Team
{
    /// <summary>
    /// Composant pour afficher un badge individuel dans la grille
    /// Simple et optimisé pour les seniors
    /// </summary>
    public class PlayerBadgeDisplayItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image badgeIcon;
        [SerializeField] private TextMeshProUGUI badgeName;
        [SerializeField] private TextMeshProUGUI badgeGame;
        [SerializeField] private Image rarityBackground;
        [SerializeField] private GameObject tooltipTrigger;

        [Header("Visual Settings")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = Color.magenta;
        [SerializeField] private Color legendaryColor = Color.yellow;

        private BadgeDisplayInfo currentBadge;

        /// <summary>
        /// Configure l'affichage d'un badge
        /// </summary>
        public void SetupBadge(BadgeDisplayInfo badge)
        {
            if (badge == null) return;

            currentBadge = badge;

            // Icône du badge
            if (badgeIcon != null)
            {
                badgeIcon.sprite = badge.Icon;
                badgeIcon.enabled = badge.Icon != null;
            }

            // Nom du badge
            if (badgeName != null)
                badgeName.text = badge.BadgeName;

            // Nom du jeu
            if (badgeGame != null)
                badgeGame.text = badge.GameName;

            // Couleur de rareté
            if (rarityBackground != null)
            {
                rarityBackground.color = GetRarityColor(badge.Rarity);
            }
        }

        /// <summary>
        /// Retourne la couleur selon la rareté
        /// </summary>
        private Color GetRarityColor(string rarity)
        {
            return rarity switch
            {
                "Common" => commonColor,
                "Uncommon" => uncommonColor,
                "Rare" => rareColor,
                "Epic" => epicColor,
                "Legendary" => legendaryColor,
                _ => commonColor
            };
        }

        /// <summary>
        /// Appelé quand on clique sur le badge (pour tooltip)
        /// </summary>
        public void OnBadgeClicked()
        {
            if (currentBadge != null)
            {
                ShowBadgeTooltip();
            }
        }

        /// <summary>
        /// Affiche les détails du badge
        /// </summary>
        private void ShowBadgeTooltip()
        {
            // Simple debug pour l'instant - peut être étendu avec un vrai tooltip
            Debug.Log($"🏆 {currentBadge.BadgeName}\n" +
                     $"📱 {currentBadge.GameName}\n" +
                     $"💎 {currentBadge.Rarity}\n" +
                     $"📅 {currentBadge.EarnedDate}\n" +
                     $"📖 {currentBadge.Description}");
        }
    }
}
