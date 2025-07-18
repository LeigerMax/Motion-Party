using UnityEngine;
using UnityEngine.UI;
using Systems;
using System;
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

        [Header("Options")]
        [SerializeField] private bool showOnEnable = true;
        [SerializeField] private bool debugMode = false;

        // Ajout : callback pour signaler au GameManager de passer au joueur suivant
        public Action OnNextPlayerCallback;

        private void OnEnable()
        {
            if (showOnEnable)
                ShowCurrentPlayerInfo();
        }

        /// <summary>
        /// Affiche les infos du joueur courant (legacy, affiche le joueur à jouer)
        /// </summary>
        public void ShowCurrentPlayerInfo()
        {
            var currentPlayer = GamePlayerSelector.Instance.CurrentPlayer;
            if (currentPlayer != null)
            {
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
        /// Affiche les infos de fin de round : joueur qui vient de jouer + prochain joueur
        /// </summary>
        public void ShowEndOfRoundInfo(Systems.PlayerData previousPlayer, Systems.PlayerData nextPlayer, int roundScore = 0)
        {
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
            // Ajoutez ici un affichage dans l'UI si vous avez un champ dédié, sinon log
            if (debugMode)
                Debug.Log($"Fin de round - Joueur précédent: {previousPlayer?.Nickname}, Score du tour: {roundScore}, {message}");

             nextPlayerText.text = message;

            nextButton.SetActive(true);
        }

        /// <summary>
        /// Appelé par le bouton suivant
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

            // Ajout : notifier le GameManager si callback défini
            if (OnNextPlayerCallback != null)
            {
                OnNextPlayerCallback.Invoke();
            }
        }
    }
}
