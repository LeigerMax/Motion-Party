using UI.Team; // Pour PlayerBadgeDisplayItem
using UnityEngine;
using TMPro;
using Systems;
using System.Collections.Generic;
using Gameplay.Common.Badges;

namespace UI.RoundEndScreen
{
    /// <summary>
    /// Affiche les informations du joueur (nom, score)
    /// </summary>
    public class RoundEndPlayerInfo : MonoBehaviour
    {
        [Header("Références UI")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private bool debugMode = false;

        [Header("Badges UI")]
        [SerializeField] private Transform badgesContainer;
        [SerializeField] private GameObject badgeDisplayPrefab; // prefab avec icône, nom, nombre

        /// <summary>
        /// Met à jour l'affichage avec les infos du joueur
        /// </summary>
        public void SetPlayer(PlayerData player)
        {
            if (player == null)
            {
                playerNameText.text = "-";
                playerScoreText.text = "-";
                if (debugMode)
                    Debug.Log("Aucun joueur à afficher");
                return;
            }
            playerNameText.text = player.Nickname;
            playerScoreText.text = $"Score : {player.TotalScore}";
        }

        /// <summary>
        /// Met à jour l'affichage avec les infos du joueur et le score du tour
        /// </summary>
        public void SetPlayer(PlayerData player, int roundScore)
        {
            if (player == null)
            {
                playerNameText.text = "-";
                playerScoreText.text = "-";
                if (debugMode)
                    Debug.Log("Aucun joueur à afficher");
                return;
            }
            playerNameText.text = player.Nickname;
            playerScoreText.text = $"Score du tour : {roundScore}";
        }

        /// <summary>
        /// Affiche les badges gagnés pendant la session pour ce joueur
        /// </summary>
        public void ShowSessionBadges(string playerId)
        {
            // Nettoyer l'affichage précédent
            if (debugMode)
                Debug.Log($"[ShowSessionBadges] Nettoyage des anciens badges UI pour {playerId}");
            foreach (Transform child in badgesContainer)
                Destroy(child.gameObject);

            if (SessionBadgeTracker.Instance == null)
            {
                Debug.LogError($"[ShowSessionBadges] SessionBadgeTracker.Instance est null ! Impossible d'afficher les badges pour {playerId}");
                return;
            }

            var sessionBadges = SessionBadgeTracker.Instance.GetSessionBadgesForPlayer(playerId);
            if (debugMode)
                Debug.Log($"[ShowSessionBadges] Badges trouvés pour {playerId} : {(sessionBadges != null ? sessionBadges.Count : 0)}");
            if (sessionBadges == null || sessionBadges.Count == 0)
            {
                if (debugMode)
                    Debug.Log($"Aucun badge de session pour {playerId}");
                // Optionnel : afficher un message "Aucun badge"
                return;
            }

            // Grouper par badgeId pour compter les occurrences
            var badgeCounts = new Dictionary<string, (BadgeInstance info, int count)>();
            foreach (var badge in sessionBadges)
            {
                var badgeId = badge.BadgeDefinition?.BadgeId ?? "";
                if (!badgeCounts.ContainsKey(badgeId))
                    badgeCounts[badgeId] = (badge, 0);
                badgeCounts[badgeId] = (badge, badgeCounts[badgeId].count + 1);
                if (debugMode)
                    Debug.Log($"[ShowSessionBadges] Ajout badgeId={badgeId} badgeName={badge.BadgeDefinition?.BadgeName ?? ""}");
            }

            // Instancier les éléments UI pour chaque badge
            foreach (var kvp in badgeCounts)
            {
                var info = kvp.Value.info;
                var count = kvp.Value.count;

                if (debugMode)
                    Debug.Log($"[ShowSessionBadges] Instanciation UI badge {info.BadgeDefinition?.BadgeName ?? ""} x{count} (id: {info.BadgeDefinition?.BadgeId ?? ""})");

                if (badgeDisplayPrefab == null || badgesContainer == null)
                {
                    Debug.LogError("[ShowSessionBadges] badgeDisplayPrefab ou badgesContainer n'est pas assigné dans l'inspecteur !");
                    continue;
                }

                var go = Instantiate(badgeDisplayPrefab, badgesContainer);
                var display = go.GetComponent<PlayerBadgeDisplayItem>();
                if (display != null)
                {
                    // Conversion vers BadgeDisplayInfo pour compatibilité
                    var def = info.BadgeDefinition;
                    var badgeDisplayInfo = new BadgeDisplayInfo
                    {
                        BadgeId = def?.BadgeId ?? "",
                        BadgeName = (def?.BadgeName ?? "") + (count > 1 ? $" x{count}" : ""),
                        Description = def?.Description ?? "",
                        Icon = def?.Icon,
                        GameId = def?.MiniGameId ?? "",
                        EarnedDate = info.EarnedDate
                        // Ajoute d'autres champs si nécessaire
                    };
                    display.SetupBadge(badgeDisplayInfo);
                }
                else
                {
                    Debug.LogError("[ShowSessionBadges] Le prefab n'a pas de PlayerBadgeDisplayItem !");
                }
            }
        }
    }
}

