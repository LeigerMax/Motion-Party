using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core
{
    /// <summary>
    /// Écran de chargement simple qui s'affiche au démarrage et se ferme quand des données UDP arrivent
    /// </summary>
    public class SimpleLoadingScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image loadingIcon;
        
        private bool isActive = false;

        private void Start()
        {
            ShowLoadingScreen();
        }

        private void Update()
        {
            // Animation de l'icône
            if (isActive && loadingIcon != null)
            {
                loadingIcon.transform.Rotate(0, 0, -90 * Time.deltaTime);
            }

            // Vérifier si des données UDP sont reçues
            if (isActive)
            {
                UDPReceive udp = FindObjectOfType<UDPReceive>();
                if (udp != null && udp.HasReceivedData)
                {
                    HideLoadingScreen();
                }
            }
        }

        public void ShowLoadingScreen()
        {
            if (isActive) return;
            
            isActive = true;
            
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }
            
            if (statusText != null)
            {
                statusText.text = "Initialisation du jeu...\nVeuillez patienter";
            }
        }

        public void HideLoadingScreen()
        {
            if (!isActive) return;
            
            isActive = false;
            
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
            
            // Auto-destruction après fermeture
            Destroy(gameObject, 1f);
        }
    }
}
