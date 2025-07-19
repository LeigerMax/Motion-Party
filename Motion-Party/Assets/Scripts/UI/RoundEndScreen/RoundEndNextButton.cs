using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.RoundEndScreen
{
    /// <summary>
    /// Bouton pour passer au joueur suivant ou au jeu suivant
    /// Gère aussi le mode "Passer au mini-jeu suivant"
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RoundEndNextButton : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private bool isVisible = true;
        [SerializeField] private bool debugMode = false;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI buttonText; // Texte du bouton (optionnel)

        private Button button;
        private RoundEndScreenManager manager;

        // États possibles du bouton
        public enum ButtonMode
        {
            NextPlayer,     // "Joueur suivant"
            NextMiniGame,   // "Passer au mini-jeu suivant"
            Hidden          // Caché
        }

        private ButtonMode currentMode = ButtonMode.NextPlayer;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            
            // Trouver le manager parent
            manager = GetComponentInParent<RoundEndScreenManager>();
            if (manager == null && debugMode)
                Debug.LogWarning("RoundEndScreenManager non trouvé dans le parent");

            // Trouver le texte du bouton si pas assigné
            if (buttonText == null)
                buttonText = GetComponentInChildren<TextMeshProUGUI>();

            SetActive(isVisible);
        }

        /// <summary>
        /// Active ou désactive le bouton depuis l'inspecteur
        /// </summary>
        public void SetActive(bool active)
        {
            isVisible = active;
            gameObject.SetActive(active);
            if (debugMode)
                Debug.Log($"Bouton suivant {(active ? "activé" : "désactivé")}");
        }

        /// <summary>
        /// Configure le mode du bouton
        /// </summary>
        public void SetMode(ButtonMode mode)
        {
            currentMode = mode;
            
            switch (mode)
            {
                case ButtonMode.NextPlayer:
                    SetActive(true);
                    if (buttonText != null)
                        buttonText.text = "Joueur suivant";
                    break;
                    
                case ButtonMode.NextMiniGame:
                    SetActive(true);
                    if (buttonText != null)
                        buttonText.text = "Passer au mini-jeu suivant";
                    break;
                    
                case ButtonMode.Hidden:
                    SetActive(false);
                    break;
            }

            if (debugMode)
                Debug.Log($"Mode du bouton changé : {mode}");
        }

        /// <summary>
        /// Configure le texte personnalisé du bouton
        /// </summary>
        public void SetCustomText(string text)
        {
            if (buttonText != null)
                buttonText.text = text;
        }

        private void OnClick()
        {
            if (manager == null)
            {
                if (debugMode)
                    Debug.LogWarning("RoundEndScreenManager non trouvé, impossible de traiter le clic");
                return;
            }

            switch (currentMode)
            {
                case ButtonMode.NextPlayer:
                    manager.OnNextButtonClicked();
                    break;
                    
                case ButtonMode.NextMiniGame:
                    manager.ForceNextMiniGame();
                    break;
                    
                default:
                    if (debugMode)
                        Debug.LogWarning($"Mode de bouton non géré : {currentMode}");
                    break;
            }
        }

        /// <summary>
        /// Méthodes de convenance pour changer de mode rapidement
        /// </summary>
        public void SetNextPlayerMode() => SetMode(ButtonMode.NextPlayer);
        public void SetNextMiniGameMode() => SetMode(ButtonMode.NextMiniGame);
        public void Hide() => SetMode(ButtonMode.Hidden);
    }
}