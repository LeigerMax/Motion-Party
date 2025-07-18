using UnityEngine;

namespace Gameplay.LogParade.Calibration
{
    /// <summary>
    /// Composant responsable de la destruction automatique des logs de calibration
    /// lorsqu'ils entrent en collision avec un autre log.
    /// À attacher sur les logs de calibration uniquement !
    /// </summary>

    public class CalibrationLogDestroyer : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Log"))
            {
                Destroy(gameObject);
            }
        }
    }
}