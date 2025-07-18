using UnityEngine;
using UnityEngine.UI;

namespace UI.RoundEndScreen
{
    /// <summary>
    /// Bouton pour passer au joueur suivant ou au jeu suivant
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RoundEndNextButton : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private bool isVisible = true;
        [SerializeField] private bool debugMode = false;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
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

        private void OnClick()
        {
            // Appelle le manager pour gérer la navigation
            var manager = GetComponentInParent<RoundEndScreenManager>();
            if (manager != null)
                manager.OnNextButtonClicked();
            else if (debugMode)
                Debug.LogWarning("RoundEndScreenManager non trouvé dans le parent");
        }
    }
}
