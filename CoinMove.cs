using UnityEngine;

public class CoinMove : MonoBehaviour
{
    public float rotationSpeed = 1f; 
    public float moveDistance = 0.5f;
    public float moveSpeed = 1f;
    private Vector3 initialPosition; 

    private void Start()
    {
        initialPosition = transform.position; 
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        Vector3 newPosition = initialPosition + new Vector3(0f, Mathf.Sin(Time.time * moveSpeed) * moveDistance, 0f);
        transform.position = newPosition;
    }
}