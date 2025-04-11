using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifeTime = 1.5f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    public Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position + offset;
        transform.position = initialPosition;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f){
            gameObject.SetActive(false);
            transform.position = initialPosition;
        }      
    }
}
