using UnityEngine;
using UnityEngine.UI;
using Systems;
using System;
using System.Collections;
using TMPro;

namespace UI.RoundEndScreen
{
    /// <summary>
    /// Gère l'affichage de l'écran de fin de manche et la navigation entre joueurs/jeux
    /// </summary>
    public class RoundEndScreenManager : MonoBehaviour
    {
        [Header("Références UI")]
        [SerializeField] private RoundEndPlayerInfo playerInfo;
        [SerializeField] private TextMeshProUGUI nextPlayerText; 
        [SerializeField] private RoundEndNextButton nextButton;

        [Header("UI Transition Mini-Jeu")]
        [SerializeField] private GameObject nextGamePanel; // Panel spécial pour transition mini-jeu
        [SerializeField] private TextMeshProUGUI nextGameMessageText; // "Plus aucun joueur, passage au mini-jeu suivant"
        [SerializeField] private TextMeshProUGUI countdownText; // Compte à rebours optionnel

        [Header("Options")]
        [SerializeField] private bool showOnEnable = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float nextGameDelay = 4f; // Délai avant passage au mini-jeu suivant

        // Callbacks
        public Action OnNextPlayerCallback;
        public Action OnNextMiniGameCallback; // Nouveau callback pour transition mini-jeu

        private Coroutine countdownCoroutine;

        private void OnEnable()
        {
            if (showOnEnable)
                ShowCurrentPlayerInfo();
        }

        private void OnDisable()
        {
            // Arrêter le countdown si l'objet est désactivé
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }

        /// <summary>
        /// Affiche les infos du joueur courant (legacy, affiche le joueur à jouer)
        /// </summary>
        public void ShowCurrentPlayerInfo()
        {
            var currentPlayer = GamePlayerSelector.Instance.CurrentPlayer;
            if (currentPlayer != null)
            {
                SetNormalMode();
                playerInfo.SetPlayer(currentPlayer);
                nextButton.SetActive(true);
            }
            else
            {
                if (debugMode)
                    Debug.Log("Aucun joueur courant à afficher");
                nextButton.SetActive(false);
            }
        }

        /// <summary>
        /// Affiche les infos de fin de round : joueur qui vient de jouer + prochain joueur
        /// </summary>
        public void ShowEndOfRoundInfo(Systems.PlayerData previousPlayer, Systems.PlayerData nextPlayer, int roundScore = 0)
        {
            SetNormalMode();

            if (previousPlayer != null)
            {
                playerInfo.SetPlayer(previousPlayer, roundScore);
                // Afficher les badges de session du joueur précédent
                playerInfo.ShowSessionBadges(previousPlayer.Nickname);
            }
            else
            {
                playerInfo.SetPlayer(null, 0);
            }

            // Afficher le nom du prochain joueur (si existe)
            string nextPlayerName = nextPlayer != null ? nextPlayer.Nickname : "Aucun";
            string message = $"Prochain joueur : {nextPlayerName}";
            
            if (debugMode)
                Debug.Log($"Fin de round - Joueur précédent: {previousPlayer?.Nickname}, Score du tour: {roundScore}, {message}");

            nextPlayerText.text = message;
            nextButton.SetActive(true);
        }

        /// <summary>
        /// Nouvelle méthode : Affiche l'écran de transition vers le mini-jeu suivant
        /// </summary>
        public void ShowNextMiniGameTransition(Systems.PlayerData lastPlayer, int finalScore = 0)
        {
            if (debugMode)
                Debug.Log($"[RoundEndScreenManager] Transition vers mini-jeu suivant - Dernier joueur: {lastPlayer?.Nickname}");

            SetNextMiniGameMode();

            // Afficher les infos du dernier joueur
            if (lastPlayer != null)
            {
                playerInfo.SetPlayer(lastPlayer, finalScore);
                playerInfo.ShowSessionBadges(lastPlayer.Nickname);
            }

            // Message de transition
            if (nextGameMessageText != null)
            {
                nextGameMessageText.text = "Plus aucun joueur, passage au mini-jeu suivant";
            }

            // Démarrer le compte à rebours
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            countdownCoroutine = StartCoroutine(NextMiniGameCountdown());
        }

        /// <summary>
        /// Coroutine de compte à rebours pour le passage au mini-jeu suivant
        /// </summary>
        private IEnumerator NextMiniGameCountdown()
        {
            float timeRemaining = nextGameDelay;

            while (timeRemaining > 0)
            {
                if (countdownText != null)
                {
                    countdownText.text = $"Prochaine étape dans {Mathf.Ceil(timeRemaining)}s";
                }

                yield return new WaitForSeconds(0.1f);
                timeRemaining -= 0.1f;
            }

            if (countdownText != null)
            {
                countdownText.text = "Chargement...";
            }

            // Déclencher la transition vers le mini-jeu suivant
            OnNextMiniGameCallback?.Invoke();
        }

        /// <summary>
        /// Méthode pour forcer immédiatement le passage au mini-jeu suivant
        /// </summary>
        public void ForceNextMiniGame()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            OnNextMiniGameCallback?.Invoke();
        }

        /// <summary>
        /// Configure l'UI en mode normal (joueur suivant)
        /// </summary>
        private void SetNormalMode()
        {
            if (nextGamePanel != null)
                nextGamePanel.SetActive(false);
            
            if (playerInfo != null)
                playerInfo.gameObject.SetActive(true);
            
            if (nextPlayerText != null)
                nextPlayerText.gameObject.SetActive(true);
        }

        /// <summary>
        /// Configure l'UI en mode transition mini-jeu
        /// </summary>
        private void SetNextMiniGameMode()
        {
            if (nextGamePanel != null)
                nextGamePanel.SetActive(true);
            
            if (nextPlayerText != null)
                nextPlayerText.gameObject.SetActive(false);

            nextButton.SetActive(false); // Pas de bouton manuel en mode transition
        }

        /// <summary>
        /// Appelé par le bouton suivant (mode normal seulement)
        /// </summary>
        public void OnNextButtonClicked()
        {
            // Passe au joueur suivant ou termine la partie
            var selector = GamePlayerSelector.Instance;
            if (selector.CurrentPlayerIndex < selector.PlayerCount - 1)
            {
                selector.NextPlayer();
                ShowCurrentPlayerInfo();
            }
            else
            {
                selector.EndGame();
                nextButton.SetActive(false);
                if (debugMode)
                    Debug.Log("Fin de partie, plus de joueur à passer.");
            }

            // Notifier le GameManager si callback défini
            if (OnNextPlayerCallback != null)
            {
                OnNextPlayerCallback.Invoke();
            }
        }
    }
}