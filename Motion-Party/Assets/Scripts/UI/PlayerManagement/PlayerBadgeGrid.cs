using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Gameplay.Common.Badges;

namespace UI.Team
{
    /// <summary>
    /// Affiche les badges obtenus par le joueur dans une grille (GridLayoutGroup)
    /// </summary>
    public class PlayerBadgeGrid : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform badgeContent; // Content du ScrollView
        [SerializeField] private GameObject badgeItemPrefab; // Prefab carré avec emoji + nom
        [SerializeField] private TextMeshProUGUI badgeCountText;
        [SerializeField] private TextMeshProUGUI noBadgesText; // Ajout pour le message

        private List<GameObject> badgeItems = new List<GameObject>();

        /// <summary>
        /// Affiche les badges d'un joueur
        /// </summary>
        public void DisplayPlayerBadges(string playerName)
        {
            ClearBadgeDisplay();
            var badges = BadgeUIHelper.GetPlayerBadgesForUI(playerName);
            if (badgeCountText != null)
                badgeCountText.text = $"Badges obtenus : {badges.Count}";
            if (badges.Count == 0)
            {
                if (noBadgesText != null)
                {
                    noBadgesText.text = "Aucun badge obtenu pour le moment.";
                    noBadgesText.gameObject.SetActive(true);
                }
                return;
            }
            if (noBadgesText != null)
                noBadgesText.gameObject.SetActive(false);
            foreach (var badge in badges)
            {
                var badgeObj = Instantiate(badgeItemPrefab, badgeContent);
                var display = badgeObj.GetComponent<PlayerBadgeDisplayItem>();
                if (display != null)
                    display.SetupBadge(badge);
                badgeItems.Add(badgeObj);
            }
        }

        /// <summary>
        /// Nettoie l'affichage des badges
        /// </summary>
        public void ClearBadgeDisplay()
        {
            foreach (var item in badgeItems)
            {
                if (item != null)
                    Destroy(item);
            }
            badgeItems.Clear();
            if (noBadgesText != null)
                noBadgesText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ClearBadgeDisplay();
        }
    }
}
