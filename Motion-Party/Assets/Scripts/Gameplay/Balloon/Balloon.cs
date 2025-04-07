using UnityEngine;

public class Balloon : MonoBehaviour
{
    public float floatSpeed = 2f;  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)  
    {
        if (other.CompareTag("Hand"))  
        {
            Destroy(gameObject); 
        }
    }
}
