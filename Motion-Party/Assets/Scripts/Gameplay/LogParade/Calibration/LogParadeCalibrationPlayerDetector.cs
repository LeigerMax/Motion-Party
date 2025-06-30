using UnityEngine;

/// <summary>
/// Détecteur de position du joueur pour la calibration LogParade.
/// Responsable de calculer la lane du joueur et de vérifier sa position.
/// </summary>
public class LogParadeCalibrationPlayerDetector
{
    #region Dependencies
    private LogParadePlayerAvatar playerAvatar;
    private LogParadeLateralTracker lateralTracker;
    private Transform[] laneTransforms;
    #endregion

    #region Configuration
    private float laneDetectionTolerance;
    #endregion

    #region Constructor
    public LogParadeCalibrationPlayerDetector(
        LogParadePlayerAvatar playerAvatar,
        LogParadeLateralTracker lateralTracker,
        Transform[] laneTransforms,
        float detectionTolerance = 0.5f)
    {
        this.playerAvatar = playerAvatar;
        this.lateralTracker = lateralTracker;
        this.laneTransforms = laneTransforms;
        this.laneDetectionTolerance = detectionTolerance;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Obtient la lane actuelle du joueur.
    /// </summary>
    /// <returns>Numéro de lane (1-4)</returns>
    public int GetPlayerCurrentLane()
    {
        if (playerAvatar != null)
        {
            return playerAvatar.GetCurrentLane();
        }

        // Fallback: calculer en fonction de la position
        if (lateralTracker != null && playerAvatar != null)
        {
            return CalculateLaneFromPosition(playerAvatar.transform.position);
        }

        return 2; 
    }

    /// <summary>
    /// Vérifie si le joueur est sur une lane spécifique.
    /// </summary>
    /// <param name="targetLane">Lane cible (1-4)</param>
    /// <returns>True si le joueur est sur la lane</returns>
    public bool IsPlayerOnLane(int targetLane)
    {
        if (playerAvatar == null || targetLane < 1 || targetLane > 4) 
            return false;

        Vector3 playerPos = playerAvatar.transform.position;
        Vector3 lanePos = laneTransforms[targetLane - 1].position;
        
        float distance = Vector3.Distance(playerPos, lanePos);
        return distance <= laneDetectionTolerance;
    }

    /// <summary>
    /// Obtient la position actuelle du joueur.
    /// </summary>
    /// <returns>Position du joueur</returns>
    public Vector3 GetPlayerPosition()
    {
        if (playerAvatar != null)
        {
            return playerAvatar.transform.position;
        }
        
        return Vector3.zero;
    }

    /// <summary>
    /// Valide que tous les composants nécessaires sont présents.
    /// </summary>
    /// <returns>True si tous les composants sont valides</returns>
    public bool ValidateComponents()
    {        if (playerAvatar == null)
        {
            LogParadeLogger.LogError("PlayerAvatar non assigné!");
            return false;
        }        if (laneTransforms == null || laneTransforms.Length != 4)
        {
            LogParadeLogger.LogError("Lane Transforms invalides!");
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (laneTransforms[i] == null)
            {
                LogParadeLogger.LogError($"Lane Transform {i + 1} non assigné!");
                return false;
            }
        }

        return true;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Calcule la lane en fonction de la position world.
    /// </summary>
    /// <param name="position">Position à analyser</param>
    /// <returns>Numéro de lane (1-4)</returns>
    private int CalculateLaneFromPosition(Vector3 position)
    {
        float closestDistance = float.MaxValue;
        int closestLane = 1;

        for (int i = 0; i < 4; i++)
        {
            if (laneTransforms[i] == null) continue;
            
            float distance = Vector3.Distance(position, laneTransforms[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = i + 1;
            }
        }

        return closestLane;
    }
    #endregion

    #region Configuration
    /// <summary>
    /// Met à jour la tolérance de détection.
    /// </summary>
    /// <param name="tolerance">Nouvelle tolérance</param>
    public void SetDetectionTolerance(float tolerance)
    {
        laneDetectionTolerance = Mathf.Max(0f, tolerance);
    }

    /// <summary>
    /// Obtient la tolérance de détection actuelle.
    /// </summary>
    /// <returns>Tolérance de détection</returns>
    public float GetDetectionTolerance()
    {
        return laneDetectionTolerance;
    }
    #endregion

}
