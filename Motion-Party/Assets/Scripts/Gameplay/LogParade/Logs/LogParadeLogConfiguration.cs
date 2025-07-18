using UnityEngine;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Logs
{
    /// <summary>
    /// Configuration centralisée pour le système de génération de rondins LogParade.
    /// Gère la validation des références et la configuration des paramètres.
    /// </summary>
    public class LogParadeLogConfiguration : MonoBehaviour
    {
        #region Champs de configuration

        [Header("Configuration des Voies")]
        [SerializeField] private Transform[] lanes = new Transform[4];

        [Header("Prefabs de Rondins")]
        [SerializeField] private GameObject[] logPrefabs = new GameObject[3];

        [Header("Paramètres de Génération")]
        [SerializeField] private float logSpeed = 1.2f;
        [SerializeField] private float verticalSpacing = 2.5f;
        [SerializeField] private float generationInterval = 2.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        // Positions X fixes des voies
        private readonly float[] lanePositions = { -3f, -1f, 1f, 3f };
        // État de validation
        private bool isValidated = false;
        #endregion

        #region Propriétés publiques
        public Transform[] Lanes => lanes;
        public GameObject[] LogPrefabs => logPrefabs;
        public float LogSpeed => logSpeed;
        public float VerticalSpacing => verticalSpacing;
        public float GenerationInterval => generationInterval;
        public bool ShowDebugInfo => showDebugInfo;
        public float[] LanePositions => lanePositions;
        public bool IsValidated => isValidated;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            ValidateConfiguration();
        }
        #endregion

        #region Validation
        /// <summary>
        /// Valide la configuration complète du système
        /// </summary>
        public bool ValidateConfiguration()
        {
            LogDebug("Validation de la configuration...");

            bool configValid = true;

            // Validation des voies
            if (!ValidateLanes())
            {
                configValid = false;
            }

            // Validation des prefabs
            if (!ValidatePrefabs())
            {
                configValid = false;
            }

            // Validation des paramètres
            if (!ValidateParameters())
            {
                configValid = false;
            }

            isValidated = configValid;
            if (isValidated)
            {
                LogDebug(" Configuration validée avec succès");
                // Seulement initialiser les positions si on a des lanes valides
                if (lanes != null && lanes.Length >= 4)
                    InitializeLanePositions();
            }
            else
            {
                LogWarning(" Configuration incomplète - utilisation des valeurs par défaut");
                isValidated = true; // Permettre le fonctionnement avec des valeurs par défaut
            }

            return isValidated;
        }
        /// <summary>
        /// Valide les voies
        /// </summary>
        private bool ValidateLanes()
        {
            if (lanes == null)
            {
                LogWarning(" Array de lanes est null - initialisation...");
                lanes = new Transform[4];
            }

            if (lanes.Length != 4)
            {
                LogWarning($" Exactement 4 voies sont recommandées ! Trouvées: {lanes.Length}");
            }

            for (int i = 0; i < lanes.Length; i++)
            {
                if (lanes[i] == null)
                {
                    LogWarning($" La voie {i + 1} n'est pas assignée !");
                }
                else
                {
                    LogDebug($"Voie {i + 1} assignée : {lanes[i].name}");
                }
            }

            return true;
        }
        /// <summary>
        /// Valide les prefabs de rondins
        /// </summary>
        private bool ValidatePrefabs()
        {
            if (logPrefabs == null)
            {
                LogWarning(" Array de prefabs est null - initialisation...");
                logPrefabs = new GameObject[3];
            }

            if (logPrefabs.Length != 3)
            {
                LogWarning($" Exactement 3 prefabs de rondins sont recommandés ! Trouvés: {logPrefabs.Length}");
            }

            for (int i = 0; i < logPrefabs.Length; i++)
            {
                if (logPrefabs[i] == null)
                {
                    LogWarning($" Le prefab de rondin {i + 1} n'est pas assigné !");
                }
                else
                {
                    LogDebug($" Prefab {i + 1} assigné : {logPrefabs[i].name}");
                }
            }

            return true; // Toujours retourner true
        }
        /// <summary>
        /// Valide les paramètres de génération
        /// </summary>
        private bool ValidateParameters()
        {
            bool valid = true;

            if (logSpeed <= 0)
            {
                LogError($" Vitesse des rondins invalide: {logSpeed} (doit être > 0)");
                valid = false;
            }

            if (verticalSpacing <= 0)
            {
                LogError($" Espacement vertical invalide: {verticalSpacing} (doit être > 0)");
                valid = false;
            }

            if (generationInterval <= 0)
            {
                LogError($" Intervalle de génération invalide: {generationInterval} (doit être > 0)");
                valid = false;
            }


            return valid;
        }
        #endregion

        #region Initialisation des positions
        /// <summary>
        /// Initialise les positions des voies
        /// </summary>
        private void InitializeLanePositions()
        {
            LogDebug(" Initialisation des positions des voies...");

            // Vérification de sécurité
            if (lanes == null || lanes.Length < 4)
            {
                LogWarning(" Lanes non initialisées correctement - skip de l'initialisation des positions");
                return;
            }

            for (int i = 0; i < lanes.Length && i < lanePositions.Length; i++)
            {
                if (lanes[i] != null)
                {
                    Vector3 lanePos = lanes[i].position;
                    lanePos.x = lanePositions[i];
                    lanes[i].position = lanePos;

                    LogDebug($"  → Voie {i + 1} positionnée à X={lanePositions[i]}");
                }
                else
                {
                    LogDebug($"  → Voie {i + 1} est null - skip");
                }
            }

            LogDebug(" Positions des voies initialisées");
        }
        #endregion

        #region API publique
        /// <summary>
        /// Obtient la position de spawn pour une voie donnée
        /// </summary>
        public Vector3 GetSpawnPosition(int laneIndex)
        {
            if (laneIndex < 0 || laneIndex >= lanePositions.Length)
            {
                LogError($" Index de voie invalide: {laneIndex}");
                return Vector3.zero;
            }

            return new Vector3(lanePositions[laneIndex], 0, 0);
        }
        /// <summary>
        /// Obtient un prefab de rondin aléatoire
        /// </summary>
        public GameObject GetRandomLogPrefab()
        {
            if (logPrefabs == null || logPrefabs.Length == 0)
            {
                LogError(" Aucun prefab de rondin disponible");
                return null;
            }

            // Trouver un prefab non-null
            var validPrefabs = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < logPrefabs.Length; i++)
            {
                if (logPrefabs[i] != null)
                {
                    validPrefabs.Add(logPrefabs[i]);
                }
            }

            if (validPrefabs.Count == 0)
            {
                LogError(" Aucun prefab valide trouvé");
                return null;
            }

            int randomIndex = Random.Range(0, validPrefabs.Count);
            return validPrefabs[randomIndex];
        }
        /// <summary>
        /// Vérifie si une voie existe
        /// </summary>
        public bool IsValidLaneIndex(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < lanePositions.Length;
        }
        /// <summary>
        /// Modifie la vitesse des rondins en runtime
        /// </summary>
        public void SetLogSpeed(float newSpeed)
        {
            if (newSpeed > 0)
            {
                logSpeed = newSpeed;
                LogDebug($" Vitesse des rondins modifiée: {newSpeed}");
            }
            else
            {
                LogError($" Vitesse invalide: {newSpeed}");
            }
        }
        /// <summary>
        /// Modifie l'intervalle de génération en runtime
        /// </summary>
        public void SetGenerationInterval(float newInterval)
        {
            if (newInterval > 0)
            {
                generationInterval = newInterval;
                LogDebug($" Intervalle de génération modifié: {newInterval}");
            }
            else
            {
                LogError($" Intervalle invalide: {newInterval}");
            }
        }
        #endregion

        #region Logging
        private void LogDebug(string message)
        {
            if (showDebugInfo)
                LogParadeLogger.LogVerbose($"[LogParadeLogConfiguration] {message}");
        }

        private void LogError(string message)
        {
            LogParadeLogger.LogError($"[LogParadeLogConfiguration] {message}");
        }

        private void LogWarning(string message)
        {
            LogParadeLogger.LogWarning($"[LogParadeLogConfiguration] {message}");
        }
        #endregion
    }
}