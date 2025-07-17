using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gameplay.Common.Badges;

namespace UI.Team
{
    /// <summary>
    /// Composant pour afficher un badge individuel dans la grille
    /// Affichage simplifié : emoji + nom du badge dans un carré
    /// </summary>
    public class PlayerBadgeDisplayItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI badgeEmojiText;
        [SerializeField] private TextMeshProUGUI badgeNameText;
        [SerializeField] private Image backgroundImage;

        private BadgeDisplayInfo currentBadge;

        /// <summary>
        /// Configure l'affichage d'un badge
        /// </summary>
        public void SetupBadge(BadgeDisplayInfo badge)
        {
            if (badge == null) return;
            currentBadge = badge;

            // Emoji du badge (unicode 🏅 U+1F3C5)
          // if (badgeEmojiText != null)
          // {
          //     badgeEmojiText.text = "\U0001F3C5";
          // }

            // Nom du badge
            if (badgeNameText != null)
                badgeNameText.text = badge.BadgeName;

            // Fond carré (optionnel, couleur personnalisable dans Unity)
            if (backgroundImage != null)
            {
                backgroundImage.enabled = true;
                // La couleur peut être configurée dans l'inspecteur Unity
            }
        }
    }
}
