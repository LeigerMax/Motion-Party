using UnityEngine;

namespace Core
{
    /// <summary>
    /// Setup automatique pour l'intégration Python avec écran de chargement
    /// Ajoutez ce script à un GameObject dans votre scène principale
    /// </summary>
    public class PythonIntegrationSetup : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool setupOnAwake = true;
        [SerializeField] private bool createLoadingScreen = true;
        [SerializeField] private bool addUDPReceiver = true;
        [SerializeField] private bool addPythonLauncher = true;

        private void Awake()
        {
            if (setupOnAwake)
            {
                SetupPythonIntegration();
            }
        }

        [ContextMenu("Setup Python Integration")]
        public void SetupPythonIntegration()
        {
            GameObject setupRoot = this.gameObject;
            
            Debug.Log("[PythonIntegrationSetup] Configuration de l'intégration Python...");

            // 1. Ajouter UDPReceive si nécessaire
            if (addUDPReceiver)
            {
                UDPReceive existingUDP = FindObjectOfType<UDPReceive>();
                if (existingUDP == null)
                {
                    setupRoot.AddComponent<UDPReceive>();
                    Debug.Log("[PythonIntegrationSetup] UDPReceive ajouté");
                }
                else
                {
                    Debug.Log("[PythonIntegrationSetup] UDPReceive déjà présent");
                }
            }

            // 2. Ajouter PythonLauncher si nécessaire
            if (addPythonLauncher)
            {
                PythonLauncher existingLauncher = FindObjectOfType<PythonLauncher>();
                if (existingLauncher == null)
                {
                    setupRoot.AddComponent<PythonLauncher>();
                    Debug.Log("[PythonIntegrationSetup] PythonLauncher ajouté");
                }
                else
                {
                    Debug.Log("[PythonIntegrationSetup] PythonLauncher déjà présent");
                }
            }

            // 3. Créer l'écran de chargement si nécessaire
            if (createLoadingScreen)
            {
                SimpleLoadingScreen existingLoadingScreen = FindObjectOfType<SimpleLoadingScreen>();
                if (existingLoadingScreen == null)
                {
                    // Créer un GameObject pour l'écran de chargement
                    GameObject loadingGO = new GameObject("SimpleLoadingCreator");
                    loadingGO.transform.SetParent(setupRoot.transform);
                    loadingGO.AddComponent<SimpleLoadingCreator>();
                    
                    Debug.Log("[PythonIntegrationSetup] Écran de chargement simple créé");
                }
                else
                {
                    Debug.Log("[PythonIntegrationSetup] Écran de chargement déjà présent");
                }
            }

            Debug.Log("[PythonIntegrationSetup] ✅ Configuration terminée avec succès!");
            Debug.Log("[PythonIntegrationSetup] 📝 L'écran de chargement s'affichera au démarrage et se fermera dès réception des données UDP.");
        }

        [ContextMenu("Test Python Connection")]
        public void TestPythonConnection()
        {
            PythonLauncher launcher = FindObjectOfType<PythonLauncher>();
            UDPReceive udp = FindObjectOfType<UDPReceive>();
            
            Debug.Log("=== Test de Connexion Python ===");
            Debug.Log($"PythonLauncher trouvé: {launcher != null}");
            if (launcher != null)
            {
                Debug.Log($"Python en cours d'exécution: {launcher.IsPythonRunning()}");
            }
            
            Debug.Log($"UDPReceive trouvé: {udp != null}");
            if (udp != null)
            {
                Debug.Log($"Données UDP reçues: {!string.IsNullOrEmpty(udp.data)}");
                if (!string.IsNullOrEmpty(udp.data))
                {
                    Debug.Log($"Dernières données: {udp.data}");
                }
            }
            
            Debug.Log("=== Fin du Test ===");
        }
    }
}
