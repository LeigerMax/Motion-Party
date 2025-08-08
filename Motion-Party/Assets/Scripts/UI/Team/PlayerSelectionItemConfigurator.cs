using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Team
{
    /// <summary>
    /// Utilitaire pour configurer automatiquement le prefab PlayerSelectionItem
    /// pour fonctionner avec le système Team
    /// </summary>
    public class PlayerSelectionItemConfigurator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoConfigureOnStart = true;
        [SerializeField] private bool debugMode = true;

        [Header("References à assigner")]
        [SerializeField] private GameObject playerSelectionItemPrefab;

        private void Start()
        {
            if (autoConfigureOnStart)
            {
                ConfigurePrefab();
            }
        }

        [ContextMenu("Configure PlayerSelectionItem Prefab")]
        public void ConfigurePrefab()
        {
            if (playerSelectionItemPrefab == null)
            {
                Debug.LogError("PlayerSelectionItemConfigurator: Prefab non assigné");
                return;
            }

            if (debugMode)
                Debug.Log("Configuration du prefab PlayerSelectionItem...");

            // Vérifier si TeamPlayerItem existe déjà
            var teamPlayerItem = playerSelectionItemPrefab.GetComponent<TeamPlayerItem>();
            if (teamPlayerItem != null)
            {
                if (debugMode)
                    Debug.Log("✅ TeamPlayerItem déjà présent sur le prefab");
                return;
            }

            // Ajouter TeamPlayerItem
            teamPlayerItem = playerSelectionItemPrefab.AddComponent<TeamPlayerItem>();
            if (debugMode)
                Debug.Log("✅ TeamPlayerItem ajouté au prefab");

            // Auto-configuration des références
            AutoConfigureReferences(teamPlayerItem);

            if (debugMode)
                Debug.Log("✅ Configuration terminée");
        }

        private void AutoConfigureReferences(TeamPlayerItem teamPlayerItem)
        {
            // Rechercher automatiquement les composants
            var texts = playerSelectionItemPrefab.GetComponentsInChildren<TextMeshProUGUI>();
            var buttons = playerSelectionItemPrefab.GetComponentsInChildren<Button>();
            var toggles = playerSelectionItemPrefab.GetComponentsInChildren<Toggle>();
            var images = playerSelectionItemPrefab.GetComponentsInChildren<Image>();

            // Configurer les textes
            foreach (var text in texts)
            {
                if (text.name.ToLower().Contains("nickname") || text.name.ToLower().Contains("name"))
                {
                    // Utiliser reflection pour assigner playerNameText
                    var field = typeof(TeamPlayerItem).GetField("playerNameText", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(teamPlayerItem, text);
                        if (debugMode)
                            Debug.Log($"✅ playerNameText assigné à {text.name}");
                    }
                }
                else if (text.name.ToLower().Contains("stats") || text.name.ToLower().Contains("score"))
                {
                    var field = typeof(TeamPlayerItem).GetField("playerStatsText", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(teamPlayerItem, text);
                        if (debugMode)
                            Debug.Log($"✅ playerStatsText assigné à {text.name}");
                    }
                }
            }

            // Configurer les boutons
            if (toggles.Length > 0)
            {
                // Utiliser le premier toggle comme bouton d'édition
                var toggle = toggles[0];
                var button = toggle.GetComponent<Button>();
                if (button == null)
                {
                    button = toggle.gameObject.AddComponent<Button>();
                }

                var field = typeof(TeamPlayerItem).GetField("editButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(teamPlayerItem, button);
                    if (debugMode)
                        Debug.Log($"✅ editButton assigné à {button.name}");
                }
            }

            // Configurer l'image de fond
            if (images.Length > 0)
            {
                var backgroundImage = images[0]; // Prendre la première image comme fond
                var field = typeof(TeamPlayerItem).GetField("backgroundImage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(teamPlayerItem, backgroundImage);
                    if (debugMode)
                        Debug.Log($"✅ backgroundImage assigné à {backgroundImage.name}");
                }
            }
        }

        [ContextMenu("Check Prefab Configuration")]
        public void CheckPrefabConfiguration()
        {
            if (playerSelectionItemPrefab == null)
            {
                Debug.LogError("❌ Prefab non assigné");
                return;
            }

            Debug.Log("=== CONFIGURATION DU PREFAB ===");
            Debug.Log($"Prefab: {playerSelectionItemPrefab.name}");

            var teamPlayerItem = playerSelectionItemPrefab.GetComponent<TeamPlayerItem>();
            Debug.Log($"TeamPlayerItem: {(teamPlayerItem != null ? "✅ Présent" : "❌ Manquant")}");

            var adapter = playerSelectionItemPrefab.GetComponent<PlayerSelectionItemAdapter>();
            Debug.Log($"PlayerSelectionItemAdapter: {(adapter != null ? "✅ Présent" : "❌ Manquant")}");

            var texts = playerSelectionItemPrefab.GetComponentsInChildren<TextMeshProUGUI>();
            Debug.Log($"TextMeshPro components: {texts.Length}");
            foreach (var text in texts)
            {
                Debug.Log($"  - {text.name}: '{text.text}'");
            }

            var buttons = playerSelectionItemPrefab.GetComponentsInChildren<Button>();
            Debug.Log($"Button components: {buttons.Length}");

            var toggles = playerSelectionItemPrefab.GetComponentsInChildren<Toggle>();
            Debug.Log($"Toggle components: {toggles.Length}");
        }
    }
}
