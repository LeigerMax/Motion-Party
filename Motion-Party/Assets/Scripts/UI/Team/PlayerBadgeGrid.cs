using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Gameplay.Common.Badges;

namespace UI.Team
{
    /// <summary>
    /// Gestionnaire de la grille des badges dans le profil joueur
    /// Interface optimisée pour les seniors avec grande lisibilité
    /// </summary>
    public class PlayerBadgeGrid : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform badgeContainer;
        [SerializeField] private GameObject badgeItemPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TextMeshProUGUI noBadgesText;

        [Header("Grid Settings")]
        [SerializeField] private int maxBadgesToShow = 12;
        [SerializeField] private bool showAllBadges = true;

        [Header("Optional Elements")]
        [SerializeField] private GameObject badgeSection;
        [SerializeField] private TextMeshProUGUI badgeCountText;

        private List<GameObject> badgeItems = new List<GameObject>();

        /// <summary>
        /// Affiche les badges d'un joueur
        /// </summary>
        public void DisplayPlayerBadges(string playerName)
        {
            // Nettoyer l'affichage précédent
            ClearBadgeDisplay();

            // Obtenir les badges du joueur
            var badges = BadgeUIHelper.GetPlayerBadgesForUI(playerName);

            // Mettre à jour le compteur
            if (badgeCountText != null)
                badgeCountText.text = $"Badges obtenus : {badges.Count}";

            // Gérer le cas sans badges
            if (badges.Count == 0)
            {
                ShowNoBadgesMessage();
                return;
            }

            // Activer la section badges
            if (badgeSection != null)
                badgeSection.SetActive(true);

            if (noBadgesText != null)
                noBadgesText.gameObject.SetActive(false);

            // Limiter le nombre de badges si nécessaire
            var badgesToShow = showAllBadges ? badges : badges.GetRange(0, Mathf.Min(maxBadgesToShow, badges.Count));

            // Créer les éléments de badges
            foreach (var badge in badgesToShow)
            {
                CreateBadgeItem(badge);
            }

            // Debug pour développement
            Debug.Log($"🏆 Affichage de {badgesToShow.Count} badges pour {playerName}");
        }

        /// <summary>
        /// Crée un élément visuel pour un badge
        /// </summary>
        private void CreateBadgeItem(BadgeDisplayInfo badge)
        {
            if (badgeItemPrefab == null || badgeContainer == null)
            {
                Debug.LogWarning("PlayerBadgeGrid: Prefab ou container non configuré");
                return;
            }

            // Instancier l'élément badge
            var badgeObj = Instantiate(badgeItemPrefab, badgeContainer);
            badgeItems.Add(badgeObj);

            // Configurer l'affichage
            var badgeDisplay = badgeObj.GetComponent<PlayerBadgeDisplayItem>();
            if (badgeDisplay != null)
            {
                badgeDisplay.SetupBadge(badge);
            }
            else
            {
                Debug.LogWarning("PlayerBadgeDisplayItem non trouvé sur le prefab");
            }
        }

        /// <summary>
        /// Affiche le message "pas de badges"
        /// </summary>
        private void ShowNoBadgesMessage()
        {
            if (noBadgesText != null)
            {
                noBadgesText.text = "Aucun badge obtenu pour le moment.\nCommencez à jouer pour en débloquer !";
                noBadgesText.gameObject.SetActive(true);
            }

            if (badgeSection != null)
                badgeSection.SetActive(false);
        }

        /// <summary>
        /// Nettoie l'affichage des badges
        /// </summary>
        private void ClearBadgeDisplay()
        {
            // Détruire tous les éléments existants
            foreach (var item in badgeItems)
            {
                if (item != null)
                    DestroyImmediate(item);
            }
            badgeItems.Clear();
        }

        /// <summary>
        /// Configure les paramètres de la grille
        /// </summary>
        public void SetGridSettings(int maxBadges, bool showAll)
        {
            maxBadgesToShow = maxBadges;
            showAllBadges = showAll;
        }

        /// <summary>
        /// Cache complètement la section badges
        /// </summary>
        public void HideBadgeSection()
        {
            if (badgeSection != null)
                badgeSection.SetActive(false);
        }

        /// <summary>
        /// Nettoie à la destruction
        /// </summary>
        private void OnDestroy()
        {
            ClearBadgeDisplay();
        }
    }
}
