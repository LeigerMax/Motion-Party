using UnityEngine;

public class DestroyCube : MonoBehaviour
{
private HandTracking handTracking;

void Start()
{
    handTracking = FindFirstObjectByType<HandTracking>();
}

private void OnTriggerEnter(Collider other)
{
    if (handTracking != null && handTracking.IsHandClosed())
    {
        Debug.Log("Cube détruit !");
        Destroy(gameObject);
    }
}
}
