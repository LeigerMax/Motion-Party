using UnityEngine;

/// <summary>
/// Script pour déplacer les rondins de calibration après la calibration.
/// </summary>
public class CalibrationLogMover : MonoBehaviour
{
    private float moveSpeed = 5f;
    private bool isMoving = true;

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    void Update()
    {
        if (!isMoving) return;
        // Mouvement vers -z (et non en y)
        transform.position += Vector3.back * moveSpeed * Time.deltaTime;
        if (transform.position.z <= -20f)
        {
            isMoving = false;
            Destroy(gameObject);
        }
    }
}
