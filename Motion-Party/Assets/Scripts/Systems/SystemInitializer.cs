using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Initialise les managers principaux du système au démarrage
    /// Assure que tous les singletons sont prêts avant l'utilisation
    /// </summary>
    public class SystemInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool initializeOnAwake = true;

        private void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeAllSystems();
            }
        }

        [ContextMenu("Initialize All Systems")]
        public void InitializeAllSystems()
        {
            if (debugMode)
                Debug.Log("SystemInitializer: Initialisation des systèmes...");

            // Initialiser PlayerProfileManager
            InitializePlayerProfileManager();

            // Initialiser TeamSetupManager
            InitializeTeamSetupManager();

            if (debugMode)
                Debug.Log("SystemInitializer: Tous les systèmes initialisés");
        }

        private void InitializePlayerProfileManager()
        {
            var playerManager = PlayerProfileManager.Instance;
            if (playerManager != null)
            {
                if (debugMode)
                    Debug.Log("SystemInitializer: ✅ PlayerProfileManager initialisé");
            }
            else
            {
                Debug.LogError("SystemInitializer: ❌ Échec initialisation PlayerProfileManager");
            }
        }

        private void InitializeTeamSetupManager()
        {
            var teamManager = TeamSetupManager.Instance;
            if (teamManager != null)
            {
                if (debugMode)
                    Debug.Log("SystemInitializer: ✅ TeamSetupManager initialisé");
            }
            else
            {
                Debug.LogError("SystemInitializer: ❌ Échec initialisation TeamSetupManager");
            }
        }

        [ContextMenu("Check System Status")]
        public void CheckSystemStatus()
        {
            Debug.Log("=== STATUS DES SYSTÈMES ===");
            Debug.Log($"PlayerProfileManager: {(PlayerProfileManager.Instance != null ? "✅ OK" : "❌ NULL")}");
            Debug.Log($"TeamSetupManager: {(TeamSetupManager.Instance != null ? "✅ OK" : "❌ NULL")}");
            
            if (TeamSetupManager.Instance != null)
            {
                Debug.Log($"Nom d'équipe: '{TeamSetupManager.Instance.GetTeamName()}'");
                Debug.Log($"Nombre de joueurs: {TeamSetupManager.Instance.GetTeamPlayerCount()}");
            }
        }
    }
}
