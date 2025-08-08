using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Menu
{
    /// <summary>
    /// Contrôleur du menu Options simplifié - optimisé pour fonctionnement automatique
    /// </summary>
    public class OptionsMenuController : MonoBehaviour
    {
        #region Variables Publiques
        
        [Header("UI Elements")]
        public GameObject optionsPanel;
        public Button volumeUpButton;
        public Button volumeDownButton;
        public Button resolutionUpButton;
        public Button resolutionDownButton;
        public Button backButton;
        public Button applyButton;
        public TextMeshProUGUI volumeText;
        public TextMeshProUGUI resolutionText;
        
        #endregion

        #region Variables Privées

        private float currentVolume = 0.8f;
        private int currentResolutionIndex = 2; // 1366x768 par défaut
        private Resolution[] recommendedResolutions = new Resolution[]
        {
            new Resolution { width = 1024, height = 768 },   // 4:3 Petite
            new Resolution { width = 1280, height = 720 },   // HD
            new Resolution { width = 1366, height = 768 },   // Par défaut
            new Resolution { width = 1440, height = 900 },   // 16:10
            new Resolution { width = 1920, height = 1080 },  // Full HD
            new Resolution { width = 2560, height = 1440 }   // 2K
        };

        #endregion

        #region Lifecycle Unity

        private void Awake()
        {
            // Pré-initialisation rapide
            LoadSettings();
        }

        private void Start()
        {
            // Setup automatique si nécessaire
            if (!HasValidUI())
            {
                Debug.Log("[OptionsMenuController] Setup automatique du menu...");
                SimpleSetupWithoutLayouts();
            }

            // Configuration des événements
            ConfigureButtonEvents();
            
            // Mise à jour UI
            UpdateUI();
            
            Debug.Log("[OptionsMenuController] Menu Options prêt ✅");
        }

        #endregion

        #region Méthodes Publiques

        /// <summary>
        /// Affiche le menu Options (appelé par MainMenuController)
        /// </summary>
        public void ShowOptionsMenu()
        {
            OpenOptionsMenu();
        }

        /// <summary>
        /// Ouvre le menu Options
        /// </summary>
        public void OpenOptionsMenu()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
                UpdateUI();
            }
        }

        /// <summary>
        /// Ferme le menu Options
        /// </summary>
        public void CloseOptionsMenu()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Cache le menu Options (appelé par MainMenuController)
        /// </summary>
        public void HideOptionsMenu()
        {
            CloseOptionsMenu();
        }

        #endregion

        #region Contrôles Audio

        /// <summary>
        /// Augmente le volume
        /// </summary>
        public void OnVolumeUpClicked()
        {
            currentVolume = Mathf.Clamp01(currentVolume + 0.1f);
            UpdateUI();
            ApplyVolumeChange();
        }

        /// <summary>
        /// Diminue le volume
        /// </summary>
        public void OnVolumeDownClicked()
        {
            currentVolume = Mathf.Clamp01(currentVolume - 0.1f);
            UpdateUI();
            ApplyVolumeChange();
        }

        #endregion

        #region Contrôles Résolution

        /// <summary>
        /// Résolution suivante
        /// </summary>
        public void OnResolutionUpClicked()
        {
            currentResolutionIndex = (currentResolutionIndex + 1) % recommendedResolutions.Length;
            UpdateUI();
        }

        /// <summary>
        /// Résolution précédente
        /// </summary>
        public void OnResolutionDownClicked()
        {
            currentResolutionIndex = (currentResolutionIndex - 1 + recommendedResolutions.Length) % recommendedResolutions.Length;
            UpdateUI();
        }

        /// <summary>
        /// Applique la résolution sélectionnée
        /// </summary>
        public void OnApplyClicked()
        {
            ApplyResolutionChange();
            SaveAllSettings();
            Debug.Log("Paramètres appliqués et sauvegardés");
        }

        #endregion

        #region Méthodes Privées

        /// <summary>
        /// Vérifie si l'UI est configurée correctement
        /// </summary>
        private bool HasValidUI()
        {
            return optionsPanel != null && 
                   volumeUpButton != null && 
                   volumeDownButton != null &&
                   volumeText != null;
        }

        /// <summary>
        /// Configure les événements des boutons
        /// </summary>
        private void ConfigureButtonEvents()
        {
            if (volumeUpButton != null)
                volumeUpButton.onClick.AddListener(OnVolumeUpClicked);
            
            if (volumeDownButton != null)
                volumeDownButton.onClick.AddListener(OnVolumeDownClicked);
            
            if (resolutionUpButton != null)
                resolutionUpButton.onClick.AddListener(OnResolutionUpClicked);
            
            if (resolutionDownButton != null)
                resolutionDownButton.onClick.AddListener(OnResolutionDownClicked);
            
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);
            
            if (backButton != null)
                backButton.onClick.AddListener(CloseOptionsMenu);
        }

        /// <summary>
        /// Met à jour l'affichage UI
        /// </summary>
        private void UpdateUI()
        {
            if (volumeText != null)
            {
                volumeText.text = $"Volume: {Mathf.RoundToInt(currentVolume * 100)}%";
            }

            if (resolutionText != null)
            {
                var res = recommendedResolutions[currentResolutionIndex];
                resolutionText.text = $"Résolution: {res.width}x{res.height}";
            }
        }

        /// <summary>
        /// Applique le changement de volume
        /// </summary>
        private void ApplyVolumeChange()
        {
            AudioListener.volume = currentVolume;
        }

        /// <summary>
        /// Applique le changement de résolution
        /// </summary>
        private void ApplyResolutionChange()
        {
            var res = recommendedResolutions[currentResolutionIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }

        /// <summary>
        /// Charge les paramètres sauvegardés
        /// </summary>
        private void LoadSettings()
        {
            currentVolume = PlayerPrefs.GetFloat("GameVolume", 0.8f);
            currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 2);
            
            ApplyVolumeChange();
        }

        /// <summary>
        /// Sauvegarde tous les paramètres
        /// </summary>
        private void SaveAllSettings()
        {
            PlayerPrefs.SetFloat("GameVolume", currentVolume);
            PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
            PlayerPrefs.Save();
        }

        #endregion

        #region Setup Automatique

        /// <summary>
        /// Setup simple sans LayoutGroups pour éviter les problèmes d'initialisation
        /// </summary>
        [ContextMenu("🔧 Setup Simple")]
        public void SimpleSetupWithoutLayouts()
        {
            Debug.Log("[Setup] Création du menu Options simple...");

            // Créer le panel principal
            CreateMainPanel();
            
            // Créer les contrôles de volume
            CreateVolumeControls();
            
            // Créer les contrôles de résolution
            CreateResolutionControls();
            
            // Créer les boutons de navigation
            CreateNavigationButtons();

            Debug.Log("[Setup] Menu Options créé avec succès ✅");
        }

        /// <summary>
        /// Crée le panel principal
        /// </summary>
        private void CreateMainPanel()
        {
            if (optionsPanel == null)
            {
                GameObject panelGO = new GameObject("OptionsPanel");
                panelGO.transform.SetParent(transform);
                
                optionsPanel = panelGO;
                
                // Ajouter Image pour le fond
                var image = panelGO.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0.8f);
                
                // RectTransform pour couvrir l'écran
                var rect = panelGO.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// Crée les contrôles de volume
        /// </summary>
        private void CreateVolumeControls()
        {
            // Label Volume
            GameObject volumeLabel = CreateLabel("VolumeLabel", "VOLUME", new Vector2(0, 100));
            
            // Bouton Volume -
            volumeDownButton = CreateButton("VolumeDownButton", "−", new Vector2(-100, 50)).GetComponent<Button>();
            
            // Texte Volume
            volumeText = CreateLabel("VolumeText", "Volume: 80%", new Vector2(0, 50)).GetComponent<TextMeshProUGUI>();
            
            // Bouton Volume +
            volumeUpButton = CreateButton("VolumeUpButton", "+", new Vector2(100, 50)).GetComponent<Button>();
        }

        /// <summary>
        /// Crée les contrôles de résolution
        /// </summary>
        private void CreateResolutionControls()
        {
            // Label Résolution
            GameObject resolutionLabel = CreateLabel("ResolutionLabel", "RÉSOLUTION", new Vector2(0, -50));
            
            // Bouton Résolution -
            resolutionDownButton = CreateButton("ResolutionDownButton", "−", new Vector2(-100, -100)).GetComponent<Button>();
            
            // Texte Résolution
            resolutionText = CreateLabel("ResolutionText", "1366x768", new Vector2(0, -100)).GetComponent<TextMeshProUGUI>();
            
            // Bouton Résolution +
            resolutionUpButton = CreateButton("ResolutionUpButton", "+", new Vector2(100, -100)).GetComponent<Button>();
        }

        /// <summary>
        /// Crée les boutons de navigation
        /// </summary>
        private void CreateNavigationButtons()
        {
            // Bouton Appliquer
            applyButton = CreateButton("ApplyButton", "APPLIQUER", new Vector2(-100, -200)).GetComponent<Button>();
            
            // Bouton Retour
            backButton = CreateButton("BackButton", "RETOUR", new Vector2(100, -200)).GetComponent<Button>();
        }

        /// <summary>
        /// Crée un label simple
        /// </summary>
        private GameObject CreateLabel(string name, string text, Vector2 position)
        {
            GameObject labelGO = new GameObject(name);
            labelGO.transform.SetParent(optionsPanel.transform);
            
            var textComponent = labelGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 32;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            var rect = labelGO.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300, 60);
            
            return labelGO;
        }

        /// <summary>
        /// Crée un bouton simple
        /// </summary>
        private GameObject CreateButton(string name, string text, Vector2 position)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(optionsPanel.transform);
            
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.5f, 0.8f);
            
            var button = buttonGO.AddComponent<Button>();
            
            // Texte du bouton
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            
            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 28;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var rect = buttonGO.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(150, 60);
            
            return buttonGO;
        }

        /// <summary>
        /// Nettoie le menu existant
        /// </summary>
        [ContextMenu("🗑️ Nettoyer Menu")]
        public void CleanupOptionsMenuUI()
        {
            if (optionsPanel != null)
            {
                DestroyImmediate(optionsPanel);
                optionsPanel = null;
            }
            
            // Reset des références
            volumeUpButton = null;
            volumeDownButton = null;
            resolutionUpButton = null;
            resolutionDownButton = null;
            backButton = null;
            applyButton = null;
            volumeText = null;
            resolutionText = null;
            
            Debug.Log("Menu Options nettoyé");
        }

        #endregion

        #region Tests et Debug

        /// <summary>
        /// Teste la connexion avec MainMenuController
        /// </summary>
        [ContextMenu("🔗 Tester Connexion MainMenu")]
        public void TestMainMenuConnection()
        {
            var mainMenuController = FindObjectOfType<MainMenuController>();
            if (mainMenuController != null)
            {
                Debug.Log("✅ MainMenuController trouvé dans la scène");
                
                // Vérifier si ce script est assigné
                var field = mainMenuController.GetType().GetField("optionsMenuController", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    var value = field.GetValue(mainMenuController);
                    if (value == this)
                    {
                        Debug.Log("✅ OptionsMenuController correctement assigné dans MainMenuController");
                    }
                    else if (value == null)
                    {
                        Debug.LogWarning("⚠️ OptionsMenuController n'est PAS assigné dans MainMenuController");
                        Debug.Log("👉 Sélectionnez MainMenuController dans l'Inspector et assignez ce script dans le champ 'Options Menu Controller'");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Un autre OptionsMenuController est assigné dans MainMenuController");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ MainMenuController non trouvé dans la scène");
            }
        }

        /// <summary>
        /// Auto-assigne ce controller au MainMenuController
        /// </summary>
        [ContextMenu("🔧 Auto-Assigner au MainMenu")]
        public void AutoAssignToMainMenu()
        {
            var mainMenuController = FindObjectOfType<MainMenuController>();
            if (mainMenuController != null)
            {
                var field = mainMenuController.GetType().GetField("optionsMenuController", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(mainMenuController, this);
                    Debug.Log("✅ OptionsMenuController auto-assigné au MainMenuController");
                    
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(mainMenuController);
                    #endif
                }
            }
            else
            {
                Debug.LogError("❌ MainMenuController non trouvé dans la scène");
            }
        }

        #endregion
    }
}
