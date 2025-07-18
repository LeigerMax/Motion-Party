using UnityEngine;
using System.Collections;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Calibration
{
    /// <summary>
    /// Gestionnaire de feedback visuel et audio pour la calibration LogParade.
    /// Responsable de la mise en surbrillance des lanes et des effets audio.
    /// </summary>
    public class LogParadeCalibrationVisualFeedback
    {
        #region Dependencies
        private Transform[] calibrationLog;
        private GameObject logPrefab;
        private Transform[] laneTransforms;
        #endregion

        #region State
        private Renderer[] originalLogRenderers = new Renderer[4];
        private Material[] originalMaterials = new Material[4];
        private MonoBehaviour coroutineRunner; 
        #endregion

        #region Constructor
        public LogParadeCalibrationVisualFeedback(
            Transform[] calibrationLog,
            GameObject logPrefab,
            Transform[] laneTransforms,
            MonoBehaviour coroutineRunner)
        {
            // Validation des paramètres critiques
            if (laneTransforms == null)
            {
                LogParadeLogger.LogError("laneTransforms ne peut pas être null dans LogParadeCalibrationVisualFeedback");
                this.laneTransforms = new Transform[4]; 
            }
            else
            {
                this.laneTransforms = laneTransforms;
            }

            // Assurer que calibrationLog a la bonne taille
            if (calibrationLog == null || calibrationLog.Length != 4)
            {
                LogParadeLogger.LogWarning($"calibrationLog doit avoir 4 éléments, trouvés: {calibrationLog?.Length ?? 0}. Création d'un array par défaut.");
                this.calibrationLog = new Transform[4];
            }
            else
            {
                this.calibrationLog = calibrationLog;
            }

            this.logPrefab = logPrefab;
            this.coroutineRunner = coroutineRunner;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Valide que tous les prérequis pour la configuration sont présents
        /// </summary>
        private bool ValidateSetupRequirements()
        {
            if (laneTransforms == null)
            {
                LogParadeLogger.LogError("laneTransforms est null - impossible de configurer les rondins de calibration");
                return false;
            }

            if (laneTransforms.Length < 2)
            {
                LogParadeLogger.LogError($"Au moins 2 lanes sont requises pour la calibration, trouvées: {laneTransforms.Length}");
                return false;
            }

            // Vérifier que nous avons au moins les lanes 1 et 4 (indices 0 et 3)
            if (laneTransforms.Length >= 4)
            {
                if (laneTransforms[0] == null || laneTransforms[3] == null)
                {
                    LogParadeLogger.LogWarning("Les lanes 1 et 4 doivent être assignées pour la calibration interactive");
                }
            }

            return true;
        }
        #endregion

        #region Public API - Setup

        /// <summary>
        /// Configure le rodin fixe pour la calibration.
        /// </summary>
        public void SetupCalibrationLog()
        {
            // Validation des prérequis
            if (!ValidateSetupRequirements())
            {
                LogParadeLogger.LogError("Impossible de configurer le rodin de calibration - prérequis manquants");
                return;
            }


            // Générer un seul rondin de calibration au centre 
            Vector3 calibrationPos = new Vector3(0f, laneTransforms[0].position.y, 0f); // X=0, Y=sol, Z=0
            Quaternion calibrationRotation = Quaternion.Euler(0f, 180f, 0f);
            GameObject calibrationLogGO = Object.Instantiate(logPrefab, calibrationPos, calibrationRotation);
            calibrationLogGO.name = "CalibrationLog";
        
            var moveScript = calibrationLogGO.GetComponent<Rigidbody>();
            if (moveScript != null)
            {
                moveScript.isKinematic = true;
            }
            // Stocker la référence pour destruction future
            calibrationLog = new Transform[1] { calibrationLogGO.transform };

            // Ajouter le script de destruction automatique si un log généré touche le rondin de calibration
            if (calibrationLogGO.GetComponent<Collider>() == null)
                calibrationLogGO.AddComponent<BoxCollider>().isTrigger = true;
            calibrationLogGO.AddComponent<CalibrationLogDestroyer>();

        }

        /// <summary>
        /// Nettoie le rodin de calibration.
        /// </summary>
        public void CleanupCalibrationLog()
        {
            if (calibrationLog != null)
            {
                for (int i = 0; i < calibrationLog.Length; i++)
                {
                    if (calibrationLog[i] != null)
                    {
                        Object.Destroy(calibrationLog[i].gameObject);
                        calibrationLog[i] = null;
                    }
                }
            }
        }
        #endregion

    }
}