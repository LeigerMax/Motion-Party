using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.FireFlyDance.UI
{
    /// <summary>
    /// Composant UI pour afficher les informations du joueur actuel
    /// </summary>
    public class FireflyPlayerDisplayUI : MonoBehaviour
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
            Color.blue,
            Color.red,
            Color.green,
            Color.yellow,
            Color.magenta,
            Color.cyan
        };

        private Systems.GamePlayerSelector gamePlayerSelector;
        private bool isInitialized = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;

            // Auto-trouver les composants si activé
            if (autoFindComponents)
            {
                AutoFindComponents();
            }

            // Trouver le GamePlayerSelector
            gamePlayerSelector = Systems.GamePlayerSelector.Instance;
            if (gamePlayerSelector == null)
            {
                Debug.LogWarning("GamePlayerSelector non trouvé - PlayerDisplayUI désactivé");
                gameObject.SetActive(false);
                return;
            }

            // S'abonner aux événements
            gamePlayerSelector.OnCurrentPlayerChanged += OnCurrentPlayerChanged;

            // Afficher le joueur initial
            UpdateDisplay();

            isInitialized = true;
        }

        private void AutoFindComponents()
        {
            if (playerNameText == null)
            {
                // Chercher un TextMeshPro avec "name" dans le nom
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

        private void OnCurrentPlayerChanged(Systems.PlayerData newPlayer)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Met à jour l'affichage avec les informations du joueur actuel
        /// </summary>
        public void UpdateDisplay()
        {
            if (gamePlayerSelector == null) return;

            var currentPlayer = gamePlayerSelector.CurrentPlayer;
            if (currentPlayer == null)
            {
                HideDisplay();
                return;
            }

            ShowDisplay();

            // Nom du joueur
            if (playerNameText != null)
            {
                playerNameText.text = currentPlayer.Nickname;
            }

            // Index du joueur
            if (playerIndexText != null && showPlayerIndex)
            {
                var currentIndex = gamePlayerSelector.CurrentPlayerIndex + 1;
                var totalPlayers = gamePlayerSelector.SelectedPlayers.Count;
                playerIndexText.text = $"Joueur {currentIndex}/{totalPlayers}";
            }

            // Couleur de fond
            if (playerBackground != null)
            {
                var colorIndex = gamePlayerSelector.CurrentPlayerIndex % playerColors.Length;
                playerBackground.color = playerColors[colorIndex];
            }
        }

        /// <summary>
        /// Affiche le panneau du joueur
        /// </summary>
        private void ShowDisplay()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Cache le panneau du joueur
        /// </summary>
        private void HideDisplay()
        {
            gameObject.SetActive(false);
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
            string originalText = playerNameText.text;
            playerNameText.text = message;
            
            yield return new WaitForSeconds(duration);
            
            playerNameText.text = originalText;
        }

        /// <summary>
        /// Configure manuellement les références UI
        /// </summary>
        public void SetUIReferences(TextMeshProUGUI nameText, TextMeshProUGUI indexText = null, 
                                   Image background = null)
        {
            playerNameText = nameText;
            playerIndexText = indexText;
            playerBackground = background;
        }

        private void OnDestroy()
        {
            if (gamePlayerSelector != null)
            {
                gamePlayerSelector.OnCurrentPlayerChanged -= OnCurrentPlayerChanged;
            }
        }


    }
}
