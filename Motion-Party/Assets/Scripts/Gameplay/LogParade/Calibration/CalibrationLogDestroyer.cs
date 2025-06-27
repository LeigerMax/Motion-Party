using UnityEngine;


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
