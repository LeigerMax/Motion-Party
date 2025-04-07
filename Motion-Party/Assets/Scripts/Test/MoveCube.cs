using UnityEngine;

public class MoveCube : MonoBehaviour
{
    private HandTracking handTracking;
    private bool isHolding = false;

    void Start()
    {
        handTracking = FindObjectOfType<HandTracking>();
    }

    void Update()
    {
        if (isHolding && handTracking != null)
        {
            transform.position = handTracking.GetHandPosition();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (handTracking != null && handTracking.IsHandClosed())
        {
            isHolding = true;
        }

        if (other.CompareTag("TargetCube")) 
        {
            Debug.Log("Success !");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isHolding = false;
    }


}
