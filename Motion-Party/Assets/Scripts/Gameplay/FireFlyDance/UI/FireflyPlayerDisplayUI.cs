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
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private Image playerBackground;

        [Header("Configuration")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool showTeamName = true;
        [SerializeField] private bool showPlayerIndex = true;
        [SerializeField] private bool showCurrentScore = false; // Score du tour actuel

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

            if (playerScoreText == null)
            {
                var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in allTexts)
                {
                    if (text.gameObject.name.ToLower().Contains("score"))
                    {
                        playerScoreText = text;
                        break;
                    }
                }
            }

            if (teamNameText == null)
            {
                var allTexts = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in allTexts)
                {
                    if (text.gameObject.name.ToLower().Contains("team"))
                    {
                        teamNameText = text;
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

            // Équipe
            if (teamNameText != null && showTeamName && !string.IsNullOrEmpty(currentPlayer.TeamName))
            {
                teamNameText.text = currentPlayer.TeamName;
            }

            // Score (si activé)
            if (playerScoreText != null && showCurrentScore)
            {
                playerScoreText.text = $"Score: {currentPlayer.TotalScore}";
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
        /// Met à jour uniquement le score affiché (si activé)
        /// </summary>
        public void UpdateScore(int newScore)
        {
            if (playerScoreText != null && showCurrentScore)
            {
                playerScoreText.text = $"Score: {newScore}";
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
            string originalText = playerNameText.text;
            playerNameText.text = message;
            
            yield return new WaitForSeconds(duration);
            
            playerNameText.text = originalText;
        }

        /// <summary>
        /// Configure manuellement les références UI
        /// </summary>
        public void SetUIReferences(TextMeshProUGUI nameText, TextMeshProUGUI indexText = null, 
                                   TextMeshProUGUI scoreText = null, TextMeshProUGUI teamText = null, 
                                   Image background = null)
        {
            playerNameText = nameText;
            playerIndexText = indexText;
            playerScoreText = scoreText;
            teamNameText = teamText;
            playerBackground = background;
        }

        private void OnDestroy()
        {
            if (gamePlayerSelector != null)
            {
                gamePlayerSelector.OnCurrentPlayerChanged -= OnCurrentPlayerChanged;
            }
        }

        #region Debug Methods

        [ContextMenu("Test Update Display")]
        public void TestUpdateDisplay()
        {
            if (!isInitialized) Initialize();
            UpdateDisplay();
        }

        [ContextMenu("Test Transition Message")]
        public void TestTransitionMessage()
        {
            ShowTransitionMessage("Au tour de Alice !", 2f);
        }

        [ContextMenu("Debug UI References")]
        public void DebugUIReferences()
        {
            Debug.Log($"=== DEBUG UI REFERENCES ===");
            Debug.Log($"PlayerNameText: {(playerNameText != null ? playerNameText.gameObject.name : "NULL")}");
            Debug.Log($"PlayerIndexText: {(playerIndexText != null ? playerIndexText.gameObject.name : "NULL")}");
            Debug.Log($"PlayerScoreText: {(playerScoreText != null ? playerScoreText.gameObject.name : "NULL")}");
            Debug.Log($"TeamNameText: {(teamNameText != null ? teamNameText.gameObject.name : "NULL")}");
            Debug.Log($"PlayerBackground: {(playerBackground != null ? playerBackground.gameObject.name : "NULL")}");
            Debug.Log($"GamePlayerSelector: {(gamePlayerSelector != null ? "FOUND" : "NULL")}");
        }

        #endregion
    }
}
