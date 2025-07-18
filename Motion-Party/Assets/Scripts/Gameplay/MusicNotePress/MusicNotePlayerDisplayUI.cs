using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;

namespace Gameplay.MusicNotePress
{
    /// <summary>
    /// Composant UI pour afficher les informations du joueur actuel dans le mini-jeu MusicNote
    /// </summary>
    public class MusicNotePlayerDisplayUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerIndexText;
        [SerializeField] private Image playerBackground;

        [Header("Configuration")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool showPlayerIndex = true;

        [Header("Visual Configuration")]
        [SerializeField] private Color[] playerColors = new Color[]
        {
            new Color(0.2f, 0.6f, 1f),    // Bleu clair
            new Color(1f, 0.4f, 0.4f),    // Rouge clair
            new Color(0.4f, 0.9f, 0.4f),  // Vert clair
            new Color(1f, 0.8f, 0.2f),    // Jaune
            new Color(0.8f, 0.4f, 1f),    // Violet
            new Color(0.4f, 0.8f, 0.8f)   // Turquoise
        };

        private void Start()
        {
            if (autoFindComponents)
            {
                AutoFindComponents();
            }
        }

        private void AutoFindComponents()
        {
            if (playerNameText == null)
            {
                var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in allTexts)
                {
                    if (text.gameObject.name.ToLower().Contains("name"))
                    {
                        playerNameText = text;
                        break;
                    }
                }
            }

            if (playerIndexText == null)
            {
                var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in allTexts)
                {
                    if (text.gameObject.name.ToLower().Contains("index") || 
                        text.gameObject.name.ToLower().Contains("turn"))
                    {
                        playerIndexText = text;
                        break;
                    }
                }
            }

            if (playerBackground == null)
            {
                playerBackground = GetComponent<Image>();
            }
        }

        /// <summary>
        /// Met à jour l'affichage avec les informations du joueur actuel
        /// </summary>
        public void UpdateDisplay(PlayerData currentPlayer, GamePlayerSelector playerSelector)
        {
            if (currentPlayer == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Nom du joueur
            if (playerNameText != null)
            {
                playerNameText.text = currentPlayer.Nickname;
            }

            // Index du joueur
            if (playerIndexText != null && showPlayerIndex && playerSelector != null)
            {
                var currentIndex = playerSelector.CurrentPlayerIndex + 1;
                var totalPlayers = playerSelector.SelectedPlayers.Count;
                playerIndexText.text = $"Joueur {currentIndex}/{totalPlayers}";
            }

            // Couleur de fond
            if (playerBackground != null && playerSelector != null)
            {
                var colorIndex = playerSelector.CurrentPlayerIndex % playerColors.Length;
                playerBackground.color = playerColors[colorIndex];
            }
        }

        /// <summary>
        /// Affiche un message de transition entre joueurs
        /// </summary>
        public void ShowTransitionMessage(string message, float duration = 3f)
        {
            if (playerNameText != null)
            {
                StartCoroutine(ShowTemporaryMessage(message, duration));
            }
        }

        private System.Collections.IEnumerator ShowTemporaryMessage(string message, float duration)
        {
            if (playerNameText == null) yield break;

            string originalText = playerNameText.text;
            playerNameText.text = message;
            
            yield return new WaitForSeconds(duration);
            
            if (playerNameText != null)
            {
                playerNameText.text = originalText;
            }
        }
    }
} 