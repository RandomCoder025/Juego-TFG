using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public GameObject targetObject;
    public float cameraHeight = 0f;
    public float cameraDistance = 0f;
    public float rotationSpeed = 3.0f;

    public float playerHorizontalRotation;
    public float playerVerticalRotation; // Nueva variable para la rotación vertical del jugador

    public float mouseHorizontalSpeed;
    public float mouseVerticalSpeed; // Nueva variable para la velocidad de rotación vertical del ratón

    private void Update()
    {
        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    private void UpdateCameraPosition()
    {
        // Obtener la posición actual del personaje
        Vector3 targetPosition = targetObject.transform.position;

        // Calcular la posición de la cámara
        float targetHeight = targetPosition.y + cameraHeight;
        float targetDistance = targetPosition.z - cameraDistance;
        Vector3 cameraPosition = new Vector3(targetPosition.x, targetHeight, targetDistance);

        // Actualizar la posición de la cámara
        transform.position = cameraPosition;
    }

    private void UpdateCameraRotation()
    {
        // Obtener la rotación horizontal y vertical del personaje del script de movimiento del jugador
        float playerRotation = targetObject.GetComponent<movement>().playerHorizontalRotation;
        float playerVerticalRotation = targetObject.GetComponent<movement>().playerVerticalRotation;

        // Obtener la entrada del ratón para la rotación horizontal y vertical
        float mouseX = Input.GetAxis("Mouse X") * mouseHorizontalSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseVerticalSpeed * Time.deltaTime;

        // Aplicar la rotación horizontal del personaje a la cámara
        transform.rotation = Quaternion.Euler(playerVerticalRotation - mouseY, playerRotation + mouseX, 0f);
    }
}   